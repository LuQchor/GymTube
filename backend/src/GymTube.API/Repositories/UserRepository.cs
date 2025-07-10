using Dapper;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using GymTube.API.Domain;
using GymTube.API.Repositories;
using Microsoft.Data.SqlClient;

namespace GymTube.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly string _connectionString;

        public UserRepository(IDbConnection dbConnection, string connectionString)
        {
            _dbConnection = dbConnection;
            _connectionString = connectionString;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var sql = "SELECT * FROM Users WHERE Email = @Email";
            return await _dbConnection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var sql = "SELECT * FROM Users WHERE Id = @Id";
            return await _dbConnection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<Guid> AddAsync(User user)
        {
            var sql = @"INSERT INTO Users (Id, Name, Email, PasswordHash, GoogleId, IsAdmin, IsPremium, StripeCustomerId, SubscriptionStatus, PlanType, ProfileImageUrl, IsProfilePrivate, Gender, BirthDate, Description)
                        VALUES (@Id, @Name, @Email, @PasswordHash, @GoogleId, @IsAdmin, @IsPremium, @StripeCustomerId, @SubscriptionStatus, @PlanType, @ProfileImageUrl, @IsProfilePrivate, @Gender, @BirthDate, @Description);
                        SELECT @Id;";
            return await _dbConnection.ExecuteScalarAsync<Guid>(sql, user);
        }

        public async Task UpdateAsync(User user)
        {
            var sql = @"UPDATE Users 
                        SET 
                            Name = @Name, 
                            Email = @Email, 
                            PasswordHash = @PasswordHash, 
                            GoogleId = @GoogleId,
                            IsAdmin = @IsAdmin,
                            IsPremium = @IsPremium,
                            StripeCustomerId = @StripeCustomerId,
                            StripeSubscriptionId = @StripeSubscriptionId,
                            PremiumExpiresAt = @PremiumExpiresAt,
                            SubscriptionStatus = @SubscriptionStatus,
                            PlanType = @PlanType,
                            ProfileImageUrl = @ProfileImageUrl,
                            IsProfilePrivate = @IsProfilePrivate,
                            Gender = @Gender,
                            BirthDate = @BirthDate,
                            Description = @Description,
                            UpdatedAt = GETDATE()
                        WHERE Id = @Id";

            await _dbConnection.ExecuteAsync(sql, user);
        }

        public async Task<User?> GetByStripeSubscriptionIdAsync(string subscriptionId)
        {
            var sql = "SELECT * FROM Users WHERE StripeSubscriptionId = @SubscriptionId";
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { SubscriptionId = subscriptionId });
        }

        // NOVE METODE
        public async Task<User?> GetByNameAsync(string name)
        {
            var sql = "SELECT * FROM Users WHERE Name = @Name";
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(sql, new { Name = name });
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string query)
        {
            var sql = @"SELECT * FROM Users 
                        WHERE (Name LIKE @Query OR Email LIKE @Query) 
                        AND IsProfilePrivate = 0
                        ORDER BY Name";
            var searchQuery = $"%{query}%";
            return await _dbConnection.QueryAsync<User>(sql, new { Query = searchQuery });
        }

        public async Task<bool> IsNameTakenAsync(string name, Guid? excludeUserId = null)
        {
            var sql = excludeUserId.HasValue
                ? "SELECT COUNT(*) FROM Users WHERE Name = @Name AND Id != @ExcludeUserId"
                : "SELECT COUNT(*) FROM Users WHERE Name = @Name";

            object parameters = excludeUserId.HasValue
                ? new { Name = name, ExcludeUserId = excludeUserId.Value }
                : new { Name = name };

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, parameters);
            return count > 0;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                UPDATE Users 
                SET Name = @Name, 
                    Email = @Email, 
                    IsPremium = @IsPremium, 
                    StripeSubscriptionId = @StripeSubscriptionId, 
                    PremiumExpiresAt = @PremiumExpiresAt,
                    ProfileImageUrl = @ProfileImageUrl,
                    Description = @Description,
                    Gender = @Gender,
                    BirthDate = @BirthDate,
                    IsProfilePrivate = @IsProfilePrivate,
                    HasPassword = @HasPassword
                WHERE Id = @Id;
                
                SELECT * FROM Users WHERE Id = @Id;";

            var parameters = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.IsPremium,
                user.StripeSubscriptionId,
                user.PremiumExpiresAt,
                user.ProfileImageUrl,
                user.Description,
                user.Gender,
                user.BirthDate,
                user.IsProfilePrivate,
                user.HasPassword
            };

            var updatedUser = await connection.QueryFirstOrDefaultAsync<User>(sql, parameters);
            return updatedUser;
        }

        // Dodaj ostale metode po potrebi
    }
}
