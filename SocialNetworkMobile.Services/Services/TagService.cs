using SocialNetworkMobile.Repository.Basic;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Mapster;

namespace SocialNetworkMobile.Services.Services
{
    public class TagService : ITagService
    {
        private readonly GenericRepository<Tag> _tagRepository;
        private readonly GenericRepository<PostTag> _postTagRepository;

        public TagService(GenericRepository<Tag> tagRepository, GenericRepository<PostTag> postTagRepository)
        {
            _tagRepository = tagRepository;
            _postTagRepository = postTagRepository;
        }

        public async Task<List<TagResponse>> GetAllTagsAsync()
        {
            var tags = await _tagRepository.GetAllAsync();
            return tags.Adapt<List<TagResponse>>();
        }

        public async Task<TagResponse> GetTagByIdAsync(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
                throw new ArgumentException("Tag not found");

            return tag.Adapt<TagResponse>();
        }

        public async Task<TagResponse> CreateTagAsync(CreateTagRequest request)
        {
            // Check if tag with same name already exists
            var existingTag = await _tagRepository.GetFirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower());
            if (existingTag != null)
            {
                throw new ArgumentException($"Tag with name '{request.Name}' already exists");
            }

            var tag = new Tag
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                UsageCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tagRepository.AddAsync(tag);
            return tag.Adapt<TagResponse>();
        }

        public async Task<TagResponse> UpdateTagAsync(int id, UpdateTagRequest request)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                throw new ArgumentException("Tag not found");
            }

            // Check if new name conflicts with existing tag
            if (!string.IsNullOrEmpty(request.Name) && request.Name.ToLower() != tag.Name.ToLower())
            {
                var existingTag = await _tagRepository.GetFirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower() && t.Id != id);
                if (existingTag != null)
                {
                    throw new ArgumentException($"Tag with name '{request.Name}' already exists");
                }
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                tag.Name = request.Name.Trim();
            }

            if (request.Description != null)
            {
                tag.Description = request.Description.Trim();
            }

            tag.UpdatedAt = DateTime.UtcNow;

            await _tagRepository.UpdateAsync(tag);
            return tag.Adapt<TagResponse>();
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                return false;
            }

            // Check if tag is being used by any posts
            var postTags = await _postTagRepository.GetAllAsync(pt => pt.TagId == id);
            if (postTags.Any())
            {
                throw new ArgumentException("Cannot delete tag that is being used by posts");
            }

            await _tagRepository.DeleteAsync(tag);
            return true;
        }

        public async Task<List<TagResponse>> SearchTagsAsync(string searchTerm)
        {
            var tags = await _tagRepository.GetAllAsync(t => t.Name.ToLower().Contains(searchTerm.ToLower()));
            return tags.Adapt<List<TagResponse>>();
        }

        public async Task<List<TagResponse>> GetPopularTagsAsync(int limit = 10)
        {
            var tags = await _tagRepository.GetAllAsync();
            var popularTags = tags
                .OrderByDescending(t => t.UsageCount)
                .ThenByDescending(t => t.CreatedAt)
                .Take(limit)
                .ToList();

            return popularTags.Adapt<List<TagResponse>>();
        }
    }
}
