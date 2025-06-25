using System.Security.Cryptography;
using System.Text;
using GymTube.API.Domain;
using GymTube.API.Repositories;
using GymTube.API.Services;
using BCrypt.Net;

namespace GymTube.API.Application.Queries
{
    public class LoginUserQueryHandler
    {
        private readonly IUserRepository _userRepository;

        public LoginUserQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> Handle(LoginUserQuery query)
        {
            var user = await _userRepository.GetByEmailAsync(query.Email);
            if (user == null)
                return null;

            // Verify the password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(query.Password, user.PasswordHash))
                return null;

            return user;
        }
    }
}
