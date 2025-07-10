using GymTube.API.Repositories;

namespace GymTube.API.Application.Commands
{
    public class UpdateUserNameCommandHandler
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserNameCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> Handle(UpdateUserNameCommand command)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null || string.IsNullOrWhiteSpace(command.NewName))
            {
                return false;
            }

            user.Name = command.NewName;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}