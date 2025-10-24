using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IGroupService
    {
        // Group Management
        Task<GroupResponse> CreateGroupAsync(CreateGroupRequest request);
        Task<GroupResponse> GetGroupByIdAsync(int groupId);
        Task<GroupResponse> UpdateGroupAsync(int groupId, UpdateGroupRequest request, int updatedBy);
        Task<bool> DeleteGroupAsync(int groupId, int userId);
        
        // Search & Browse
        Task<List<GroupResponse>> SearchGroupsByNameAsync(string name);
        Task<List<GroupResponse>> GetAllGroupsAsync();
        Task<List<GroupResponse>> GetPublicGroupsAsync();
        Task<List<GroupResponse>> GetUserGroupsAsync(int userId);
        
        // Member Management
        Task<GroupMemberResponse> InviteMemberAsync(InviteMemberRequest request);
        Task<bool> AcceptInvitationAsync(int groupId, int userId);
        Task<bool> RejectInvitationAsync(int groupId, int userId);
        Task<bool> RemoveMemberAsync(int groupId, int userId, int removedBy);
        Task<bool> LeaveGroupAsync(int groupId, int userId);
        
        // Member Queries
        Task<List<GroupMemberResponse>> GetGroupMembersAsync(int groupId);
        Task<List<GroupMemberResponse>> GetActiveGroupMembersAsync(int groupId);
        Task<List<GroupMemberResponse>> GetPendingInvitationsAsync(int groupId);
        Task<bool> IsMemberAsync(int groupId, int userId);
        Task<string?> GetMemberRoleAsync(int groupId, int userId);
        
        // Admin Functions
        Task<bool> PromoteToAdminAsync(int groupId, int userId, int promotedBy);
        Task<bool> PromoteToModeratorAsync(int groupId, int userId, int promotedBy);
        Task<bool> DemoteToMemberAsync(int groupId, int userId, int demotedBy);
        Task<bool> BanMemberAsync(int groupId, int userId, int bannedBy);
        Task<bool> UnbanMemberAsync(int groupId, int userId, int unbannedBy);
        
        // Statistics
        Task<int> GetGroupMemberCountAsync(int groupId);
        Task<bool> UpdateMemberCountAsync(int groupId);
    }
}

