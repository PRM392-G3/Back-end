using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface ITagService
    {
        Task<List<TagResponse>> GetAllTagsAsync();
        Task<TagResponse> GetTagByIdAsync(int id);
        Task<TagResponse> CreateTagAsync(CreateTagRequest request);
        Task<TagResponse> UpdateTagAsync(int id, UpdateTagRequest request);
        Task<bool> DeleteTagAsync(int id);
        Task<List<TagResponse>> SearchTagsAsync(string searchTerm);
        Task<List<TagResponse>> GetPopularTagsAsync(int limit = 10);
    }
}
