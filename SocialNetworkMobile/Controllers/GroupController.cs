using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        #region Group Management

        /// <summary>
        /// Tạo nhóm mới
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<GroupResponse>> CreateGroup([FromBody] CreateGroupRequest request)
        {
            try
            {
                var group = await _groupService.CreateGroupAsync(request);
                return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lấy thông tin nhóm theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupResponse>> GetGroup(int id)
        {
            try
            {
                var group = await _groupService.GetGroupByIdAsync(id);
                return Ok(group);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhóm
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<GroupResponse>> UpdateGroup(int id, [FromBody] UpdateGroupRequest request)
        {
            try
            {
                var userIdStr = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                var group = await _groupService.UpdateGroupAsync(id, request, userId);
                return Ok(group);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Xóa nhóm (chỉ creator)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteGroup(int id, [FromQuery] int userId)
        {
            try
            {
                var result = await _groupService.DeleteGroupAsync(id, userId);
                if (result)
                    return Ok(new { message = "Group deleted successfully" });
                return NotFound("Group not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Search & Browse

        /// <summary>
        /// Tìm kiếm nhóm theo tên
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<GroupResponse>>> SearchGroups([FromQuery] string name)
        {
            try
            {
                var groups = await _groupService.SearchGroupsByNameAsync(name);
                return Ok(groups);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lấy tất cả nhóm
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<GroupResponse>>> GetAllGroups()
        {
            var groups = await _groupService.GetAllGroupsAsync();
            return Ok(groups);
        }

        /// <summary>
        /// Lấy danh sách nhóm public
        /// </summary>
        [HttpGet("public")]
        public async Task<ActionResult<List<GroupResponse>>> GetPublicGroups()
        {
            var groups = await _groupService.GetPublicGroupsAsync();
            return Ok(groups);
        }

        /// <summary>
        /// Lấy danh sách nhóm của user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<GroupResponse>>> GetUserGroups(int userId)
        {
            try
            {
                var groups = await _groupService.GetUserGroupsAsync(userId);
                return Ok(groups);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Member Management

        /// <summary>
        /// Mời thành viên vào nhóm
        /// </summary>
        [HttpPost("invite")]
        [Authorize]
        public async Task<ActionResult<GroupMemberResponse>> InviteMember([FromBody] InviteMemberRequest request)
        {
            try
            {
                var member = await _groupService.InviteMemberAsync(request);
                return Ok(member);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Chấp nhận lời mời vào nhóm
        /// </summary>
        [HttpPost("{groupId}/accept-invitation")]
        [Authorize]
        public async Task<ActionResult> AcceptInvitation(int groupId, [FromQuery] int userId)
        {
            try
            {
                var result = await _groupService.AcceptInvitationAsync(groupId, userId);
                if (result)
                    return Ok(new { message = "Invitation accepted" });
                return NotFound("Invitation not found");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Từ chối lời mời vào nhóm
        /// </summary>
        [HttpPost("{groupId}/reject-invitation")]
        [Authorize]
        public async Task<ActionResult> RejectInvitation(int groupId, [FromQuery] int userId)
        {
            try
            {
                var result = await _groupService.RejectInvitationAsync(groupId, userId);
                if (result)
                    return Ok(new { message = "Invitation rejected" });
                return NotFound("Invitation not found");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Xóa thành viên khỏi nhóm (admin only)
        /// </summary>
        [HttpDelete("{groupId}/members/{userId}")]
        [Authorize]
        public async Task<ActionResult> RemoveMember(int groupId, int userId, [FromQuery] int removedBy)
        {
            try
            {
                var result = await _groupService.RemoveMemberAsync(groupId, userId, removedBy);
                if (result)
                    return Ok(new { message = "Member removed successfully" });
                return NotFound("Member not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Rời khỏi nhóm
        /// </summary>
        [HttpPost("{groupId}/leave")]
        [Authorize]
        public async Task<ActionResult> LeaveGroup(int groupId, [FromQuery] int userId)
        {
            try
            {
                var result = await _groupService.LeaveGroupAsync(groupId, userId);
                if (result)
                    return Ok(new { message = "Left group successfully" });
                return NotFound("Membership not found");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Member Queries

        /// <summary>
        /// Xem tất cả thành viên nhóm
        /// </summary>
        [HttpGet("{groupId}/members")]
        public async Task<ActionResult<List<GroupMemberResponse>>> GetGroupMembers(int groupId)
        {
            try
            {
                var members = await _groupService.GetGroupMembersAsync(groupId);
                return Ok(members);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Xem thành viên active
        /// </summary>
        [HttpGet("{groupId}/members/active")]
        public async Task<ActionResult<List<GroupMemberResponse>>> GetActiveMembers(int groupId)
        {
            try
            {
                var members = await _groupService.GetActiveGroupMembersAsync(groupId);
                return Ok(members);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Xem lời mời pending
        /// </summary>
        [HttpGet("{groupId}/invitations/pending")]
        [Authorize]
        public async Task<ActionResult<List<GroupMemberResponse>>> GetPendingInvitations(int groupId)
        {
            try
            {
                var invitations = await _groupService.GetPendingInvitationsAsync(groupId);
                return Ok(invitations);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Kiểm tra user có phải thành viên không
        /// </summary>
        [HttpGet("{groupId}/is-member/{userId}")]
        public async Task<ActionResult<bool>> IsMember(int groupId, int userId)
        {
            var isMember = await _groupService.IsMemberAsync(groupId, userId);
            return Ok(new { isMember });
        }

        /// <summary>
        /// Lấy role của member
        /// </summary>
        [HttpGet("{groupId}/member/{userId}/role")]
        public async Task<ActionResult> GetMemberRole(int groupId, int userId)
        {
            var role = await _groupService.GetMemberRoleAsync(groupId, userId);
            if (role == null)
                return NotFound("Member not found");
            return Ok(new { role });
        }

        #endregion

        #region Admin Functions

        /// <summary>
        /// Promote member to admin
        /// </summary>
        [HttpPost("{groupId}/members/{userId}/promote-admin")]
        [Authorize]
        public async Task<ActionResult> PromoteToAdmin(int groupId, int userId, [FromQuery] int promotedBy)
        {
            try
            {
                var result = await _groupService.PromoteToAdminAsync(groupId, userId, promotedBy);
                if (result)
                    return Ok(new { message = "Member promoted to admin" });
                return NotFound("Member not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Promote member to moderator
        /// </summary>
        [HttpPost("{groupId}/members/{userId}/promote-moderator")]
        [Authorize]
        public async Task<ActionResult> PromoteToModerator(int groupId, int userId, [FromQuery] int promotedBy)
        {
            try
            {
                var result = await _groupService.PromoteToModeratorAsync(groupId, userId, promotedBy);
                if (result)
                    return Ok(new { message = "Member promoted to moderator" });
                return NotFound("Member not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Demote to member
        /// </summary>
        [HttpPost("{groupId}/members/{userId}/demote")]
        [Authorize]
        public async Task<ActionResult> DemoteToMember(int groupId, int userId, [FromQuery] int demotedBy)
        {
            try
            {
                var result = await _groupService.DemoteToMemberAsync(groupId, userId, demotedBy);
                if (result)
                    return Ok(new { message = "Member demoted" });
                return NotFound("Member not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Ban member
        /// </summary>
        [HttpPost("{groupId}/members/{userId}/ban")]
        [Authorize]
        public async Task<ActionResult> BanMember(int groupId, int userId, [FromQuery] int bannedBy)
        {
            try
            {
                var result = await _groupService.BanMemberAsync(groupId, userId, bannedBy);
                if (result)
                    return Ok(new { message = "Member banned" });
                return NotFound("Member not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Unban member
        /// </summary>
        [HttpPost("{groupId}/members/{userId}/unban")]
        [Authorize]
        public async Task<ActionResult> UnbanMember(int groupId, int userId, [FromQuery] int unbannedBy)
        {
            try
            {
                var result = await _groupService.UnbanMemberAsync(groupId, userId, unbannedBy);
                if (result)
                    return Ok(new { message = "Member unbanned" });
                return NotFound("Member not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Lấy số lượng thành viên
        /// </summary>
        [HttpGet("{groupId}/member-count")]
        public async Task<ActionResult<int>> GetMemberCount(int groupId)
        {
            var count = await _groupService.GetGroupMemberCountAsync(groupId);
            return Ok(new { memberCount = count });
        }

        #endregion
    }
}

