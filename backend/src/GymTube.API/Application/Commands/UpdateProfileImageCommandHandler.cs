using GymTube.API.Repositories;
using Microsoft.Extensions.Logging;

namespace GymTube.API.Application.Commands
{
    public class UpdateProfileImageCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UpdateProfileImageCommandHandler> _logger;

        public UpdateProfileImageCommandHandler(IUserRepository userRepository, ILogger<UpdateProfileImageCommandHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateProfileImageCommand command)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null)
            {
                _logger.LogWarning($"User {command.UserId} not found in database");
                return false;
            }

            // Provjeri da li je URL validan (ako nije null)
            if (!string.IsNullOrEmpty(command.ProfileImageUrl))
            {
                // Za lokalne datoteke, prihvati relativne URL-ove koji počinju s /
                if (command.ProfileImageUrl.StartsWith("/"))
                {
                    // Ovo je relativni URL za lokalnu datoteku - to je u redu
                }
                else if (!Uri.TryCreate(command.ProfileImageUrl, UriKind.Absolute, out _))
                {
                    _logger.LogWarning($"Invalid URL format: {command.ProfileImageUrl}");
                    return false; // Neispravan URL format
                }
            }

            user.ProfileImageUrl = command.ProfileImageUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}