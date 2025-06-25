namespace GymTube.API.Domain
{
    public class Vote
    {
        public Guid Id { get; set; }
        public Guid VideoId { get; set; }
        public Guid UserId { get; set; }
        public string VoteType { get; set; } = string.Empty; // "like" ili "dislike"
        public DateTime CreatedAt { get; set; }
    }
} 