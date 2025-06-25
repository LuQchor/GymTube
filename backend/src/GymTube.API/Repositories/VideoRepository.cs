using Dapper;
using GymTube.API.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace GymTube.API.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly string _connectionString;

        public VideoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' not found.");
        }

        public async Task<IEnumerable<Video>> GetByUserIdAsync(Guid userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Videos WHERE UserId = @UserId ORDER BY CreatedAt DESC";
            return await connection.QueryAsync<Video>(sql, new { UserId = userId });
        }

        public async Task<Video?> GetByIdAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Videos WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Video>(sql, new { Id = id });
        }

        public async Task<Video?> GetByMuxAssetIdAsync(string muxAssetId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "SELECT * FROM Videos WHERE MuxAssetId = @MuxAssetId";
            return await connection.QueryFirstOrDefaultAsync<Video>(sql, new { MuxAssetId = muxAssetId });
        }

        public async Task<Video> CreateAsync(Video video)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                INSERT INTO Videos (Id, Title, Description, UserId, IsPremium, IsPrivate, UploadStatus, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.*
                VALUES (@Id, @Title, @Description, @UserId, @IsPremium, @IsPrivate, @UploadStatus, @CreatedAt, @UpdatedAt)";
            
            video.Id = Guid.NewGuid();
            video.CreatedAt = DateTime.UtcNow;
            video.UpdatedAt = DateTime.UtcNow;

            var createdVideo = await connection.QuerySingleAsync<Video>(sql, video);
            return createdVideo;
        }

        public async Task UpdateAsync(Video video)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                UPDATE Videos 
                SET Title = @Title, Description = @Description, FileSize = @FileSize, 
                    UploadStatus = @UploadStatus, MuxAssetId = @MuxAssetId, 
                    MuxPlaybackId = @MuxPlaybackId, Duration = @Duration, UpdatedAt = @UpdatedAt, 
                    IsPremium = @IsPremium, IsPrivate = @IsPrivate
                WHERE Id = @Id";
            
            // Ensure null values are handled properly
            var parameters = new
            {
                video.Id,
                video.Title,
                video.Description,
                FileSize = video.FileSize ?? 0,
                video.UploadStatus,
                video.MuxAssetId,
                video.MuxPlaybackId,
                Duration = video.Duration ?? 0.0,
                video.UpdatedAt,
                video.IsPremium,
                video.IsPrivate
            };
            
            await connection.ExecuteAsync(sql, parameters);
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "DELETE FROM Videos WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task UpdateVideoStatusAsync(string muxAssetId, string status)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "UPDATE Videos SET UploadStatus = @Status, UpdatedAt = @UpdatedAt WHERE MuxAssetId = @MuxAssetId";
            await connection.ExecuteAsync(sql, new { Status = status, MuxAssetId = muxAssetId, UpdatedAt = DateTime.UtcNow });
        }

        public async Task UpdatePlaybackIdAsync(string muxAssetId, string playbackId)
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = "UPDATE Videos SET MuxPlaybackId = @PlaybackId, UpdatedAt = @UpdatedAt WHERE MuxAssetId = @MuxAssetId";
            await connection.ExecuteAsync(sql, new { PlaybackId = playbackId, MuxAssetId = muxAssetId, UpdatedAt = DateTime.UtcNow });
        }

        public async Task<IEnumerable<Video>> GetAllWithUsersAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT 
                    v.*, 
                    u.Id, u.Name, u.Email, u.IsPremium, u.ProfileImageUrl, u.IsProfilePrivate
                FROM Videos v
                LEFT JOIN Users u ON v.UserId = u.Id
                ORDER BY v.CreatedAt DESC";

            var videoDictionary = new Dictionary<Guid, Video>();

            var videos = await connection.QueryAsync<Video, User, Video>(sql, (video, user) =>
            {
                if (!videoDictionary.TryGetValue(video.Id, out var existingVideo))
                {
                    existingVideo = video;
                    videoDictionary.Add(existingVideo.Id, existingVideo);
                }
                existingVideo.User = user;
                return existingVideo;
            }, splitOn: "Id");

            return videoDictionary.Values;
        }

        public async Task<VoteResult> AddOrUpdateVoteAsync(Guid videoId, Guid userId, string voteType)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Check for existing vote
                var existingVoteSql = "SELECT * FROM Votes WHERE VideoId = @VideoId AND UserId = @UserId";
                var existingVote = await connection.QuerySingleOrDefaultAsync<Vote>(existingVoteSql, new { VideoId = videoId, UserId = userId }, transaction);

                if (existingVote != null)
                {
                    if (existingVote.VoteType == voteType)
                    {
                        // User clicked the same button again, so remove the vote (toggle off)
                        var deleteVoteSql = "DELETE FROM Votes WHERE Id = @Id";
                        await connection.ExecuteAsync(deleteVoteSql, new { existingVote.Id }, transaction);
                    }
                    else
                    {
                        // User changed their vote (e.g., from like to dislike)
                        var updateVoteSql = "UPDATE Votes SET VoteType = @VoteType, CreatedAt = GETDATE() WHERE Id = @Id";
                        await connection.ExecuteAsync(updateVoteSql, new { VoteType = voteType, existingVote.Id }, transaction);
                    }
                }
                else
                {
                    // No existing vote, so add a new one
                    var insertVoteSql = "INSERT INTO Votes (VideoId, UserId, VoteType) VALUES (@VideoId, @UserId, @VoteType)";
                    await connection.ExecuteAsync(insertVoteSql, new { VideoId = videoId, UserId = userId, VoteType = voteType }, transaction);
                }

                // Update Likes and Dislikes counts on the Videos table
                var countSql = @"
                    UPDATE Videos
                    SET 
                        Likes = (SELECT COUNT(*) FROM Votes WHERE VideoId = @VideoId AND VoteType = 'like'),
                        Dislikes = (SELECT COUNT(*) FROM Votes WHERE VideoId = @VideoId AND VoteType = 'dislike')
                    WHERE Id = @VideoId";
                await connection.ExecuteAsync(countSql, new { VideoId = videoId }, transaction);

                // Get the final counts and the user's current vote
                var resultSql = @"
                    SELECT 
                        v.Likes, 
                        v.Dislikes,
                        (SELECT VoteType FROM Votes WHERE VideoId = @VideoId AND UserId = @UserId) as UserVote
                    FROM Videos v
                    WHERE v.Id = @VideoId";
                
                var result = await connection.QuerySingleAsync<VoteResult>(resultSql, new { VideoId = videoId, UserId = userId }, transaction);

                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}