using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymTube.API.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GymTube.API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("name", user.Name),
                new Claim("is_premium", user.IsPremium.ToString()),
                new Claim("role", user.IsAdmin ? "admin" : "user"),
                new Claim("profile_image_url", user.ProfileImageUrl ?? ""),
                new Claim("stripe_customer_id", user.StripeCustomerId ?? ""),
                new Claim("stripe_subscription_id", user.StripeSubscriptionId ?? ""),
                new Claim("subscription_status", user.SubscriptionStatus ?? ""),
                new Claim("plan_type", user.PlanType ?? ""),
                new Claim("premium_expires_at", user.PremiumExpiresAt?.ToString("O") ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
