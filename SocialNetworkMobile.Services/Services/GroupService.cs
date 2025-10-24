using Microsoft.EntityFrameworkCore;
using SocialNetworkMobile.Repository.Context;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Services
{
    public class GroupService : IGroupService
    {
        private readonly SocialNetworkDbContext _context;

        public GroupService(SocialNetworkDbContext context)
        {
            _context = context;
        }

        #region Group Management

        public async Task<GroupResponse> CreateGroupAsync(CreateGroupRequest request)
        {
            // Validate creator exists
            var creator = await _context.Users.FindAsync(request.CreatedById);
            if (creator == null)
                throw new ArgumentException("Creator not found");

            var group = new Group
            {
                Name = request.Name,
                Description = request.Description,
                AvatarUrl = request.AvatarUrl,
                CoverImageUrl = request.CoverImageUrl,
                CreatedById = request.CreatedById,
                Privacy = request.Privacy,
                MemberCount = 1, // Creator is first member
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Auto-add creator as admin
            var creatorMember = new GroupMember
            {
                GroupId = group.Id,
                UserId = request.CreatedById,
                Role = GroupMemberRole.Admin,
                Status = GroupMemberStatus.Active,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.GroupMembers.Add(creatorMember);
            await _context.SaveChangesAsync();

            return await GetGroupByIdAsync(group.Id);
        }

        public async Task<GroupResponse> GetGroupByIdAsync(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.CreatedBy)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                throw new ArgumentException("Group not found");

            return MapToResponse(group);
        }

        public async Task<GroupResponse> UpdateGroupAsync(int groupId, UpdateGroupRequest request, int updatedBy)
        {
            var group = await _context.Groups
                .AsTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                throw new ArgumentException("Group not found");

            // Check if user is an admin of the group
            var memberRole = await GetMemberRoleAsync(groupId, updatedBy);
            if (memberRole != GroupMemberRole.Admin.ToString().ToLower())
                throw new UnauthorizedAccessException("Only group admins can update group information");

            if (!string.IsNullOrEmpty(request.Name))
                group.Name = request.Name;

            if (request.Description != null)
                group.Description = request.Description;

            if (request.AvatarUrl != null)
                group.AvatarUrl = request.AvatarUrl;

            if (request.CoverImageUrl != null)
                group.CoverImageUrl = request.CoverImageUrl;

            if (!string.IsNullOrEmpty(request.Privacy))
                group.Privacy = request.Privacy;

            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetGroupByIdAsync(groupId);
        }

        public async Task<bool> DeleteGroupAsync(int groupId, int userId)
        {
            var group = await _context.Groups
                .AsTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return false;

            // Only creator can delete group
            if (group.CreatedById != userId)
                throw new UnauthorizedAccessException("Only group creator can delete the group");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Search & Browse

        public async Task<List<GroupResponse>> SearchGroupsByNameAsync(string name)
        {
            var groups = await _context.Groups
                .Where(g => g.IsActive && g.Name.Contains(name))
                .Include(g => g.CreatedBy)
                .OrderBy(g => g.Name)
                .ToListAsync();

            return groups.Select(MapToResponse).ToList();
        }

        public async Task<List<GroupResponse>> GetAllGroupsAsync()
        {
            var groups = await _context.Groups
                .Where(g => g.IsActive)
                .Include(g => g.CreatedBy)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return groups.Select(MapToResponse).ToList();
        }

        public async Task<List<GroupResponse>> GetPublicGroupsAsync()
        {
            var groups = await _context.Groups
                .Where(g => g.IsActive && g.Privacy == GroupPrivacy.Public)
                .Include(g => g.CreatedBy)
                .OrderByDescending(g => g.MemberCount)
                .ToListAsync();

            return groups.Select(MapToResponse).ToList();
        }

        public async Task<List<GroupResponse>> GetUserGroupsAsync(int userId)
        {
            var groupIds = await _context.GroupMembers
                .Where(gm => gm.UserId == userId && gm.Status == GroupMemberStatus.Active)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            var groups = await _context.Groups
                .Where(g => groupIds.Contains(g.Id))
                .Include(g => g.CreatedBy)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return groups.Select(MapToResponse).ToList();
        }

        #endregion

        #region Member Management

        public async Task<GroupMemberResponse> InviteMemberAsync(InviteMemberRequest request)
        {
            // Validate group and users exist
            var group = await _context.Groups.FindAsync(request.GroupId);
            var user = await _context.Users.FindAsync(request.UserId);
            var inviter = await _context.Users.FindAsync(request.InvitedById);

            if (group == null || user == null || inviter == null)
                throw new ArgumentException("Group, User, or Inviter not found");

            // Check if inviter has permission (admin or moderator)
            var inviterMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == request.GroupId && 
                    gm.UserId == request.InvitedById &&
                    gm.Status == GroupMemberStatus.Active);

            if (inviterMember == null || 
                (inviterMember.Role != GroupMemberRole.Admin && inviterMember.Role != GroupMemberRole.Moderator))
                throw new UnauthorizedAccessException("Only admins and moderators can invite members");

            // Check if user already member
            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == request.GroupId && gm.UserId == request.UserId);

            if (existingMember != null)
                throw new ArgumentException("User is already a member or has pending invitation");

            var member = new GroupMember
            {
                GroupId = request.GroupId,
                UserId = request.UserId,
                Role = request.Role,
                Status = GroupMemberStatus.Pending,
                InvitedById = request.InvitedById,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.GroupMembers.Add(member);
            await _context.SaveChangesAsync();

            return await GetGroupMemberByIdAsync(member.Id);
        }

        public async Task<bool> AcceptInvitationAsync(int groupId, int userId)
        {
            var member = await _context.GroupMembers
                .AsTracking()
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == userId && 
                    gm.Status == GroupMemberStatus.Pending);

            if (member == null)
                return false;

            member.Status = GroupMemberStatus.Active;
            member.JoinedAt = DateTime.UtcNow;
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await UpdateMemberCountAsync(groupId);

            return true;
        }

        public async Task<bool> RejectInvitationAsync(int groupId, int userId)
        {
            var member = await _context.GroupMembers
                .AsTracking()
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == userId && 
                    gm.Status == GroupMemberStatus.Pending);

            if (member == null)
                return false;

            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveMemberAsync(int groupId, int userId, int removedBy)
        {
            // Check permission
            var remover = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == removedBy && 
                    gm.Status == GroupMemberStatus.Active);

            if (remover == null || remover.Role != GroupMemberRole.Admin)
                throw new UnauthorizedAccessException("Only admins can remove members");

            var member = await _context.GroupMembers
                .AsTracking()
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == userId);

            if (member == null)
                return false;

            // Cannot remove group creator
            var group = await _context.Groups.FindAsync(groupId);
            if (group != null && group.CreatedById == userId)
                throw new InvalidOperationException("Cannot remove group creator");

            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
            await UpdateMemberCountAsync(groupId);

            return true;
        }

        public async Task<bool> LeaveGroupAsync(int groupId, int userId)
        {
            var member = await _context.GroupMembers
                .AsTracking()
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == userId);

            if (member == null)
                return false;

            // Group creator cannot leave
            var group = await _context.Groups.FindAsync(groupId);
            if (group != null && group.CreatedById == userId)
                throw new InvalidOperationException("Group creator cannot leave. Delete the group instead.");

            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();
            await UpdateMemberCountAsync(groupId);

            return true;
        }

        #endregion

        #region Member Queries

        public async Task<List<GroupMemberResponse>> GetGroupMembersAsync(int groupId)
        {
            var members = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .Include(gm => gm.User)
                .Include(gm => gm.Group)
                .OrderByDescending(gm => gm.Role)
                .ThenBy(gm => gm.JoinedAt)
                .ToListAsync();

            return members.Select(MapMemberToResponse).ToList();
        }

        public async Task<List<GroupMemberResponse>> GetActiveGroupMembersAsync(int groupId)
        {
            var members = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId && gm.Status == GroupMemberStatus.Active)
                .Include(gm => gm.User)
                .OrderByDescending(gm => gm.Role)
                .ThenBy(gm => gm.JoinedAt)
                .ToListAsync();

            return members.Select(MapMemberToResponse).ToList();
        }

        public async Task<List<GroupMemberResponse>> GetPendingInvitationsAsync(int groupId)
        {
            var members = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId && gm.Status == GroupMemberStatus.Pending)
                .Include(gm => gm.User)
                .OrderByDescending(gm => gm.CreatedAt)
                .ToListAsync();

            return members.Select(MapMemberToResponse).ToList();
        }

        public async Task<bool> IsMemberAsync(int groupId, int userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == userId && 
                    gm.Status == GroupMemberStatus.Active);
        }

        public async Task<string?> GetMemberRoleAsync(int groupId, int userId)
        {
            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == userId && 
                    gm.Status == GroupMemberStatus.Active);

            return member?.Role;
        }

        #endregion

        #region Admin Functions

        public async Task<bool> PromoteToAdminAsync(int groupId, int userId, int promotedBy)
        {
            return await UpdateMemberRoleAsync(groupId, userId, promotedBy, GroupMemberRole.Admin);
        }

        public async Task<bool> PromoteToModeratorAsync(int groupId, int userId, int promotedBy)
        {
            return await UpdateMemberRoleAsync(groupId, userId, promotedBy, GroupMemberRole.Moderator);
        }

        public async Task<bool> DemoteToMemberAsync(int groupId, int userId, int demotedBy)
        {
            return await UpdateMemberRoleAsync(groupId, userId, demotedBy, GroupMemberRole.Member);
        }

        public async Task<bool> BanMemberAsync(int groupId, int userId, int bannedBy)
        {
            // Check permission
            var banner = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == bannedBy && 
                    gm.Status == GroupMemberStatus.Active);

            if (banner == null || banner.Role != GroupMemberRole.Admin)
                throw new UnauthorizedAccessException("Only admins can ban members");

            var member = await _context.GroupMembers
                .AsTracking()
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == userId);

            if (member == null)
                return false;

            member.Status = GroupMemberStatus.Banned;
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await UpdateMemberCountAsync(groupId);

            return true;
        }

        public async Task<bool> UnbanMemberAsync(int groupId, int userId, int unbannedBy)
        {
            // Check permission
            var unbanner = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == unbannedBy && 
                    gm.Status == GroupMemberStatus.Active);

            if (unbanner == null || unbanner.Role != GroupMemberRole.Admin)
                throw new UnauthorizedAccessException("Only admins can unban members");

            var member = await _context.GroupMembers
                .AsTracking()
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == userId && 
                    gm.Status == GroupMemberStatus.Banned);

            if (member == null)
                return false;

            member.Status = GroupMemberStatus.Active;
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await UpdateMemberCountAsync(groupId);

            return true;
        }

        #endregion

        #region Statistics

        public async Task<int> GetGroupMemberCountAsync(int groupId)
        {
            return await _context.GroupMembers
                .CountAsync(gm => gm.GroupId == groupId && gm.Status == GroupMemberStatus.Active);
        }

        public async Task<bool> UpdateMemberCountAsync(int groupId)
        {
            var group = await _context.Groups
                .AsTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return false;

            group.MemberCount = await GetGroupMemberCountAsync(groupId);
            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Private Helper Methods

        private async Task<bool> UpdateMemberRoleAsync(int groupId, int userId, int updatedBy, string newRole)
        {
            // Check permission - only admin can change roles
            var updater = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == updatedBy && 
                    gm.Status == GroupMemberStatus.Active);

            if (updater == null || updater.Role != GroupMemberRole.Admin)
                throw new UnauthorizedAccessException("Only admins can change member roles");

            var member = await _context.GroupMembers
                .AsTracking()
                .FirstOrDefaultAsync(gm => 
                    gm.GroupId == groupId && 
                    gm.UserId == userId && 
                    gm.Status == GroupMemberStatus.Active);

            if (member == null)
                return false;

            member.Role = newRole;
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<GroupMemberResponse> GetGroupMemberByIdAsync(int memberId)
        {
            var member = await _context.GroupMembers
                .Include(gm => gm.User)
                .Include(gm => gm.Group)
                .FirstOrDefaultAsync(gm => gm.Id == memberId);

            if (member == null)
                throw new ArgumentException("Group member not found");

            return MapMemberToResponse(member);
        }

        private GroupResponse MapToResponse(Group group)
        {
            return new GroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                AvatarUrl = group.AvatarUrl,
                CoverImageUrl = group.CoverImageUrl,
                CreatedById = group.CreatedById,
                Privacy = group.Privacy,
                MemberCount = group.MemberCount,
                IsActive = group.IsActive,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt,
                CreatedBy = group.CreatedBy != null ? MapUserToResponse(group.CreatedBy) : null
            };
        }

        private GroupMemberResponse MapMemberToResponse(GroupMember member)
        {
            return new GroupMemberResponse
            {
                Id = member.Id,
                GroupId = member.GroupId,
                UserId = member.UserId,
                Role = member.Role,
                Status = member.Status,
                JoinedAt = member.JoinedAt,
                InvitedById = member.InvitedById,
                User = member.User != null ? MapUserToResponse(member.User) : null,
                Group = member.Group != null ? MapToResponse(member.Group) : null
            };
        }

        private UserResponse MapUserToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                Location = user.Location,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        #endregion
    }
}

