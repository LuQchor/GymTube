using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GymTube.API.Domain;
using GymTube.API.Repositories;
using GymTube.API.Services;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace GymTube.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VideosController : BaseController
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VideosController> _logger;
        private readonly MuxService _muxService;
        private readonly IUserRepository _userRepository;

        public VideosController(
            IVideoRepository videoRepository,
            IConfiguration configuration,
            ILogger<VideosController> logger,
            MuxService muxService,
            IUserRepository userRepository)
        {
            _videoRepository = videoRepository;
            _configuration = configuration;
            _logger = logger;
            _muxService = muxService;
            _userRepository = userRepository;
        }

        [HttpPost("initiate-upload")]
        [Authorize]
        public async Task<IActionResult> InitiateUpload([FromBody] InitiateUploadRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);
            var userIsPremium = user?.IsPremium ?? false;

            if (request.IsPremium && !userIsPremium)
            {
                return Forbid("Only premium users can upload premium videos.");
            }

            try
            {
                var video = new Video
                {
                    Title = request.Title,
                    Description = request.Description,
                    UserId = userId.Value,
                    IsPremium = request.IsPremium,
                    UploadStatus = "pending",
                    IsPrivate = request.IsPrivate
                };

                var createdVideo = await _videoRepository.CreateAsync(video);

                var uploadUrl = await _muxService.GetSignedUploadUrlAsync(createdVideo.Id);

                return Ok(new { uploadUrl, videoId = createdVideo.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating upload");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("my-videos")]
        [Authorize]
        public async Task<IActionResult> GetMyVideos()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized(new { error = "Invalid user token." });

                // Get all videos with user information and filter by current user
                var allVideos = await _videoRepository.GetAllWithUsersAsync();
                var videos = allVideos.Where(v => v.UserId == userId.Value).ToList();

                // Dohvati userVote za svaki video
                var response = new List<object>();
                foreach (var v in videos)
                {
                    var userVote = await GetUserVoteAsync(v.Id, userId.Value);
                    response.Add(new
                    {
                        v.Id,
                        v.Title,
                        v.Description,
                        v.MuxPlaybackId,
                        v.ThumbnailUrl,
                        v.FileSize,
                        v.Duration,
                        v.CreatedAt,
                        v.UpdatedAt,
                        v.Likes,
                        v.Dislikes,
                        v.IsPremium,
                        v.IsPrivate,
                        v.UploadStatus,
                        User = v.User != null ? new
                        {
                            v.User.Id,
                            v.User.Name,
                            v.User.IsPremium,
                            v.User.ProfileImageUrl
                        } : null,
                        userVote
                    });
                }
                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Internal server error." });
            }
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllVideos(
            [FromQuery] string? sortBy = "recent",
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                User? currentUser = null;
                var userId = GetCurrentUserId();
                if (userId != null)
                {
                    currentUser = await _userRepository.GetByIdAsync(userId.Value);
                }
                var allVideos = await _videoRepository.GetAllWithUsersAsync();
                var videos = allVideos.Where(v => !v.IsPrivate).ToList();
                // Filter by upload status (only show ready videos)
                videos = videos.Where(v => v.UploadStatus == "ready" && !string.IsNullOrEmpty(v.MuxPlaybackId)).ToList();
                // Filtriraj videe korisnika s privatnim profilima: prikazuj samo videe korisnika s javnim profilima
                videos = videos.Where(v => v.User != null && !v.User.IsProfilePrivate).ToList();
                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    videos = videos.Where(v =>
                        v.Title.ToLower().Contains(searchLower) ||
                        (v.Description != null && v.Description.ToLower().Contains(searchLower)) ||
                        (v.User?.Name != null && v.User.Name.ToLower().Contains(searchLower))
                    ).ToList();
                }
                // Apply sorting
                videos = sortBy switch
                {
                    "recent" => videos.OrderByDescending(v => v.CreatedAt).ToList(),
                    "oldest" => videos.OrderBy(v => v.CreatedAt).ToList(),
                    "popular" => videos.OrderByDescending(v => v.Likes).ThenByDescending(v => v.CreatedAt).ToList(),
                    "title" => videos.OrderBy(v => v.Title).ToList(),
                    _ => videos.OrderByDescending(v => v.CreatedAt).ToList()
                };
                // Apply pagination
                var totalCount = videos.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var skip = (page - 1) * pageSize;
                var pagedVideos = videos.Skip(skip).Take(pageSize).ToList();
                var response = new List<object>();
                foreach (var v in pagedVideos)
                {
                    string? userVote = null;
                    if (currentUser != null)
                    {
                        userVote = await GetUserVoteAsync(v.Id, currentUser.Id);
                    }
                    var userIsPremium = currentUser?.IsPremium ?? false;
                    response.Add(new
                    {
                        v.Id,
                        v.Title,
                        v.Description,
                        MuxPlaybackId = (v.IsPremium && !userIsPremium) ? null : v.MuxPlaybackId,
                        v.ThumbnailUrl,
                        v.FileSize,
                        v.Duration,
                        v.CreatedAt,
                        v.UpdatedAt,
                        v.Likes,
                        v.Dislikes,
                        v.IsPremium,
                        v.IsPrivate,
                        v.UploadStatus,
                        User = v.User != null ? new
                        {
                            v.User.Id,
                            v.User.Name,
                            v.User.IsPremium,
                            v.User.ProfileImageUrl
                        } : null,
                        userVote
                    });
                }
                return Ok(new
                {
                    videos = response,
                    pagination = new
                    {
                        currentPage = page,
                        totalPages,
                        totalCount,
                        pageSize,
                        hasNextPage = page < totalPages,
                        hasPreviousPage = page > 1
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all videos");
                return StatusCode(500, new { error = "Internal server error." });
            }
        }

        [HttpPost("{id}/vote")]
        [Authorize]
        public async Task<IActionResult> VoteOnVideo(Guid id, [FromBody] VoteRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized(new { error = "Invalid user token." });

                // Validate vote type
                if (request.VoteType != "like" && request.VoteType != "dislike")
                {
                    return BadRequest(new { error = "Invalid vote type. Must be 'like' or 'dislike'." });
                }

                // Check if video exists
                var video = await _videoRepository.GetByIdAsync(id);
                if (video == null)
                {
                    return NotFound(new { error = "Video not found." });
                }

                // Add or update vote
                var result = await _videoRepository.AddOrUpdateVoteAsync(id, userId.Value, request.VoteType);

                return Ok(new
                {
                    message = "Vote recorded successfully.",
                    likes = result.Likes,
                    dislikes = result.Dislikes,
                    userVote = result.UserVote
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on video");
                return StatusCode(500, new { error = "Internal server error." });
            }
        }

        [HttpGet("{id}/votes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVideoVotes(Guid id)
        {
            try
            {
                var video = await _videoRepository.GetByIdAsync(id);
                if (video == null)
                {
                    return NotFound(new { error = "Video not found." });
                }
                User? currentUser = null;
                var userId = GetCurrentUserId();
                if (userId != null)
                {
                    currentUser = await _userRepository.GetByIdAsync(userId.Value);
                }
                var userVote = currentUser != null ? await GetUserVote(id, currentUser.Id) : null;
                return Ok(new
                {
                    likes = video.Likes,
                    dislikes = video.Dislikes,
                    userVote
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video votes");
                return StatusCode(500, new { error = "Internal server error." });
            }
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromBody] MuxWebhookPayload payload)
        {
            try
            {
                // Poveži asset s videom u bazi na eventima asset.created i asset.ready
                if (payload.Type == "video.asset.created" || payload.Type == "video.asset.ready")
                {
                    var muxAssetId = payload.Data?.Id;
                    var passthrough = payload.Data?.Passthrough;
                    if (!string.IsNullOrEmpty(muxAssetId) && !string.IsNullOrEmpty(passthrough))
                    {
                        var videoId = Guid.Parse(passthrough);
                        var video = await _videoRepository.GetByIdAsync(videoId);
                        if (video != null)
                        {
                            video.MuxAssetId = muxAssetId;
                            video.UpdatedAt = DateTime.UtcNow;
                            await _videoRepository.UpdateAsync(video);
                        }
                    }
                    // Ako je ready, odmah ažuriraj status i playbackId
                    if (payload.Type == "video.asset.ready" && !string.IsNullOrEmpty(muxAssetId))
                    {
                        await _videoRepository.UpdateVideoStatusAsync(muxAssetId, "ready");
                        var playbackId = await GetPlaybackIdFromMux(muxAssetId);
                        if (!string.IsNullOrEmpty(playbackId))
                        {
                            await _videoRepository.UpdatePlaybackIdAsync(muxAssetId, playbackId);
                        }
                    }
                }
                else if (payload.Type == "video.asset.errored")
                {
                    var assetId = payload.Data?.Id;
                    if (!string.IsNullOrEmpty(assetId))
                    {
                        await _videoRepository.UpdateVideoStatusAsync(assetId, "failed");
                    }
                }
                else
                {
                    // Nepoznati tip webhook-a
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Webhook error: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("check-status/{assetId}")]
        [Authorize]
        public async Task<IActionResult> CheckVideoStatus(string assetId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("Token is required");
                }
                // Check if video belongs to user
                var video = await _videoRepository.GetByMuxAssetIdAsync(assetId);
                if (video == null || video.UserId != userId)
                {
                    return NotFound("Video not found or access denied");
                }

                // Get status from Mux API
                AssetStatusData statusData;
                try
                {
                    statusData = await _muxService.GetAssetStatusAsync(assetId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting asset status from Mux for asset {AssetId}", assetId);
                    return StatusCode(502, "Error getting asset status from Mux");
                }

                if (statusData == null)
                {
                    return NotFound("Asset not found in Mux");
                }

                // Update video status in database if it changed
                if (statusData.Status != video.UploadStatus)
                {
                    video.UploadStatus = statusData.Status;
                    video.MuxPlaybackId = statusData.PlaybackId;
                    video.UpdatedAt = DateTime.UtcNow;
                    await _videoRepository.UpdateAsync(video);
                }

                return Ok(new
                {
                    status = statusData.Status,
                    playbackId = statusData.PlaybackId,
                    duration = statusData.Duration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking video status for asset {AssetId}", assetId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteVideo(Guid id, [FromBody] DeleteVideoRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("Token is required");
                }
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }
                var video = await _videoRepository.GetByIdAsync(id);
                if (video == null)
                {
                    return NotFound("Video not found");
                }
                var isAdmin = user.IsAdmin;
                if (!isAdmin)
                {
                    if (video.UserId != user.Id)
                    {
                        return Forbid("Nije dozvoljeno brisanje tuđih videa.");
                    }
                    if (string.IsNullOrEmpty(request.Password))
                    {
                        return BadRequest("Lozinka je obavezna.");
                    }
                    if (string.IsNullOrEmpty(user.PasswordHash))
                    {
                        return BadRequest("Korisnik nema postavljenu lozinku. Postavite lozinku u profilu.");
                    }
                    var isPasswordValid = PasswordHelper.VerifyPassword(request.Password, user.PasswordHash);
                    if (!isPasswordValid)
                    {
                        return BadRequest("Pogrešna lozinka.");
                    }
                }
                if (!string.IsNullOrEmpty(video.MuxAssetId))
                {
                    await _muxService.DeleteAssetAsync(video.MuxAssetId);
                }
                await _videoRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DELETE] Error deleting video {VideoId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}/toggle-premium")]
        [Authorize]
        public async Task<IActionResult> TogglePremiumStatus(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);
            var userIsPremium = user?.IsPremium ?? false;

            if (!userIsPremium)
            {
                return Forbid("Only premium users can manage premium status.");
            }

            var video = await _videoRepository.GetByIdAsync(id);

            if (video == null)
            {
                return NotFound();
            }

            if (video.UserId != userId)
            {
                return Forbid("You can only change the status of your own videos.");
            }

            video.IsPremium = !video.IsPremium;
            video.UpdatedAt = DateTime.UtcNow;

            await _videoRepository.UpdateAsync(video);

            return Ok(new { message = "Video premium status updated.", isPremium = video.IsPremium });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateVideo(Guid id, [FromBody] UpdateVideoRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized(new { error = "Invalid user token." });
            var user = await _userRepository.GetByIdAsync(userId.Value);
            var userIsPremium = user?.IsPremium ?? false;

            var video = await _videoRepository.GetByIdAsync(id);
            if (video == null)
            {
                return NotFound(new { error = "Video not found." });
            }
            if (video.UserId != userId)
            {
                return Forbid("You can only edit your own videos.");
            }
            // Samo premium korisnici mogu označiti video kao premium
            if (request.IsPremium && !userIsPremium)
            {
                return Forbid("Only premium users can set videos as premium.");
            }
            video.Title = request.Title?.Trim() ?? video.Title;
            video.Description = request.Description?.Trim() ?? video.Description;
            video.IsPremium = request.IsPremium;
            video.IsPrivate = request.IsPrivate;
            video.UpdatedAt = DateTime.UtcNow;
            await _videoRepository.UpdateAsync(video);
            return Ok(new { message = "Video updated successfully.", video });
        }

        [HttpGet("user/{identifier}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserVideos(string identifier)
        {
            try
            {
                // Pronađi korisnika po emailu ili imenu
                User? user = null;
                if (identifier.Contains("@"))
                {
                    user = await _userRepository.GetByEmailAsync(identifier);
                }
                else
                {
                    user = await _userRepository.GetByNameAsync(identifier);
                }

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Ako je profil privatan, vrati praznu listu
                if (user.IsProfilePrivate)
                {
                    return Ok(new { videos = new List<object>(), user = new { name = user.Name, is_profile_private = true } });
                }

                // Dohvati sve videe korisnika
                var allVideos = await _videoRepository.GetAllWithUsersAsync();
                var userVideos = allVideos.Where(v => v.UserId == user.Id && !v.IsPrivate).ToList();

                // Provjeri je li trenutni korisnik premium
                bool viewerIsPremium = false;
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Guid? viewerId = null;
                if (!string.IsNullOrEmpty(currentUserId) && Guid.TryParse(currentUserId, out var parsedId))
                {
                    viewerId = parsedId;
                    var viewer = await _userRepository.GetByIdAsync(viewerId.Value);
                    viewerIsPremium = viewer?.IsPremium == true;
                }

                var response = new List<object>();
                foreach (var v in userVideos)
                {
                    string? userVote = null;
                    if (viewerId != null)
                    {
                        userVote = await GetUserVoteAsync(v.Id, viewerId.Value);
                    }
                    response.Add(new
                    {
                        v.Id,
                        v.Title,
                        v.Description,
                        muxPlaybackId = (v.IsPremium && !viewerIsPremium) ? null : v.MuxPlaybackId,
                        v.ThumbnailUrl,
                        v.FileSize,
                        v.Duration,
                        v.CreatedAt,
                        v.UpdatedAt,
                        v.Likes,
                        v.Dislikes,
                        v.IsPremium,
                        v.IsPrivate,
                        v.UploadStatus,
                        user = v.User != null ? new
                        {
                            v.User.Id,
                            v.User.Name,
                            v.User.Email,
                            v.User.IsPremium,
                            v.User.ProfileImageUrl
                        } : null,
                        userVote
                    });
                }

                return Ok(new
                {
                    videos = response,
                    user = new
                    {
                        id = user.Id,
                        name = user.Name,
                        email = user.Email,
                        is_premium = user.IsPremium,
                        profile_image_url = user.ProfileImageUrl,
                        gender = user.Gender,
                        birth_date = user.BirthDate,
                        description = user.Description,
                        is_profile_private = false
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user videos");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<string> GetPlaybackIdFromMux(string assetId)
        {
            try
            {
                var tokenId = _configuration["Mux:TokenId"];
                var tokenSecret = _configuration["Mux:TokenSecret"];

                if (string.IsNullOrEmpty(tokenId) || string.IsNullOrEmpty(tokenSecret))
                {
                    return string.Empty;
                }

                var credentials = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{tokenId}:{tokenSecret}")
                );

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                var response = await client.GetAsync($"https://api.mux.com/video/v1/assets/{assetId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var assetData = JsonSerializer.Deserialize<JsonElement>(content);

                    if (assetData.TryGetProperty("data", out var data) &&
                        data.TryGetProperty("playback_ids", out var playbackIds) &&
                        playbackIds.GetArrayLength() > 0)
                    {
                        var firstPlaybackId = playbackIds[0];
                        if (firstPlaybackId.TryGetProperty("id", out var id))
                        {
                            return id.GetString() ?? string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting playback ID: {ex.Message}");
            }

            return string.Empty;
        }

        private async Task<string?> GetUserVote(Guid videoId, Guid userId)
        {
            await Task.CompletedTask;
            return null;
        }

        private async Task<string?> GetUserVoteAsync(Guid videoId, Guid userId)
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var sql = "SELECT VoteType FROM Votes WHERE VideoId = @VideoId AND UserId = @UserId";
            return await connection.QueryFirstOrDefaultAsync<string>(sql, new { VideoId = videoId, UserId = userId });
        }
    }

    public record InitiateUploadRequest(string Title, string? Description, bool IsPremium, bool IsPrivate);

    public class MuxWebhookData
    {
        public string Type { get; set; } = string.Empty;
        public MuxAssetData Data { get; set; } = new();
    }

    public class MuxAssetData
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double? Duration { get; set; }
        public List<MuxPlaybackId>? PlaybackIds { get; set; }
        public string? Passthrough { get; set; }
    }

    public class MuxPlaybackId
    {
        public string Id { get; set; } = string.Empty;
        public string Policy { get; set; } = string.Empty;
    }

    public class MuxWebhookPayload
    {
        public string Type { get; set; } = string.Empty;
        public MuxAssetData Data { get; set; } = new();
    }

    public class DeleteVideoRequest
    {
        public string Password { get; set; } = string.Empty;
    }

    public class VoteRequest
    {
        public string VoteType { get; set; } = string.Empty; // "like" ili "dislike"
    }

    public class UpdateVideoRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPremium { get; set; }
        public bool IsPrivate { get; set; }
    }
}