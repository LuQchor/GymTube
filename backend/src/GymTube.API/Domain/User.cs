namespace GymTube.API.Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? GoogleId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsPremium { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public DateTime? PremiumExpiresAt { get; set; }
        public string? SubscriptionStatus { get; set; }
        public string? PlanType { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Nova polja za prošireni profil
        public bool IsProfilePrivate { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Description { get; set; }
        public bool HasPassword { get; set; }
    }
}
