using System;

namespace GymTube.API.Domain
{
    public class Video
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? FileName { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Mux-specific fields
        public string? MuxUploadId { get; set; }
        public string? MuxAssetId { get; set; }
        public string? MuxPlaybackId { get; set; }
        public int? Duration { get; set; }
        public long? FileSize { get; set; }
        public string? UploadStatus { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Voting and view fields
        public int Views { get; set; } = 0;
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        public bool IsPremium { get; set; }
        public bool IsPrivate { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}