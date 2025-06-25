using GymTube.API.Domain;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GymTube.API.Repositories
{
    public interface IVideoRepository
    {
        Task<IEnumerable<Video>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Video>> GetAllWithUsersAsync();
        Task<Video?> GetByIdAsync(Guid id);
        Task<Video?> GetByMuxAssetIdAsync(string muxAssetId);
        Task<Video> CreateAsync(Video video);
        Task UpdateAsync(Video video);
        Task DeleteAsync(Guid id);
        Task UpdateVideoStatusAsync(string muxAssetId, string status);
        Task UpdatePlaybackIdAsync(string muxAssetId, string playbackId);
        Task<VoteResult> AddOrUpdateVoteAsync(Guid videoId, Guid userId, string voteType);
    }

    public class VoteResult
    {
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public string? UserVote { get; set; }
    }
}