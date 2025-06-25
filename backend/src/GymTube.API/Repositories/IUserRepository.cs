using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using GymTube.API.Domain;

namespace GymTube.API.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<Guid> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<User?> GetByStripeSubscriptionIdAsync(string subscriptionId);
        Task<User?> GetByNameAsync(string name);
        Task<IEnumerable<User>> SearchUsersAsync(string query);
        Task<bool> IsNameTakenAsync(string name, Guid? excludeUserId = null);
        // Task<List<User>> GetExpiredPremiumUsersAsync(DateTime currentTime);
    }
}
