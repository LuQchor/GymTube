using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GymTube.API.Domain;
using GymTube.API.Repositories;
using GymTube.API.Services;
using System.Threading;
using MediatR;
using BCrypt.Net;

namespace GymTube.API.Application.Commands
{
    public class RegisterUserCommand : IRequest<Guid>
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;

        public RegisterUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Provjeri postoji li korisnik s istim emailom
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            // Provjeri je li ime zauzeto
            if (await _userRepository.IsNameTakenAsync(request.Name))
            {
                throw new InvalidOperationException("Username is already taken.");
            }

            // Provjeri minimalnu dužinu lozinke
            if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 6)
            {
                throw new InvalidOperationException("Lozinka mora imati barem 6 znakova.");
            }

            // Hashiraj lozinku
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Stvori novog korisnika
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                IsAdmin = false,
                IsPremium = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsProfilePrivate = false // Default: javni profil
            };

            // Spremi korisnika u bazu
            await _userRepository.AddAsync(user);

            return user.Id;
        }
    }
}
