using GymTube.API.Application.Commands;
using GymTube.API.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using GymTube.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using GymTube.API.Repositories;
using GymTube.API.Domain;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using System.Linq;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace GymTube.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly RegisterUserCommandHandler _registerUserHandler;
        private readonly LoginUserQueryHandler _loginUserHandler;
        private readonly UpdateUserNameCommandHandler _updateUserNameHandler;
        private readonly UpdateProfileImageCommandHandler _updateProfileImageHandler;
        private readonly JwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly GoogleAuthService _googleAuthService;
        private readonly ImageService _imageService;
        private readonly ILogger<AuthController> _logger;
        private readonly IVideoRepository _videoRepository;
        private readonly MuxService _muxService;
        private readonly IConfiguration _configuration;

        public AuthController(
            RegisterUserCommandHandler registerUserHandler,
            LoginUserQueryHandler loginUserHandler,
            UpdateUserNameCommandHandler updateUserNameHandler,
            UpdateProfileImageCommandHandler updateProfileImageHandler,
            JwtService jwtService,
            IUserRepository userRepository,
            GoogleAuthService googleAuthService,
            ImageService imageService,
            ILogger<AuthController> logger,
            IVideoRepository videoRepository,
            MuxService muxService,
            IConfiguration configuration)
        {
            _registerUserHandler = registerUserHandler;
            _loginUserHandler = loginUserHandler;
            _updateUserNameHandler = updateUserNameHandler;
            _updateProfileImageHandler = updateProfileImageHandler;
            _jwtService = jwtService;
            _userRepository = userRepository;
            _googleAuthService = googleAuthService;
            _imageService = imageService;
            _logger = logger;
            _videoRepository = videoRepository;
            _muxService = muxService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            try
            {
                var userId = await _registerUserHandler.Handle(command, default);
                return Ok(new { UserId = userId });
            }
            catch (System.Exception ex)
            {
                // U produkciji koristi bolje hendlanje grešaka!
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserQuery query)
        {
            var user = await _loginUserHandler.Handle(query);
            if (user == null)
                return BadRequest(new { error = "Pogrešan email ili lozinka." });

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token,
                userId = user.Id,
                email = user.Email,
                name = user.Name
            });
        }

        [HttpPost("google-signin")]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.AuthorizationCode))
                {
                    return BadRequest(new { error = "Authorization code is required." });
                }

                // Get user info from Google
                var googleUserInfo = await _googleAuthService.GetUserInfoAsync(request.AuthorizationCode);

                if (googleUserInfo?.Email == null)
                {
                    return BadRequest(new { error = "Failed to get user information from Google." });
                }

                // Check if user exists in database
                var user = await _userRepository.GetByEmailAsync(googleUserInfo.Email);

                if (user == null)
                {
                    // Generiraj unikatno korisničko ime
                    string baseName = googleUserInfo.Name ?? googleUserInfo.GivenName ?? "Unknown";
                    string uniqueName = baseName;
                    var rand = new Random();
                    int attempts = 0;
                    while (await _userRepository.IsNameTakenAsync(uniqueName) && attempts < 10)
                    {
                        uniqueName = $"{baseName} {rand.Next(0, 10000)}";
                        attempts++;
                    }
                    // Create new user
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = googleUserInfo.Email,
                        Name = uniqueName,
                        GoogleId = googleUserInfo.Id,
                        PasswordHash = null, // No password for Google users
                        IsAdmin = false,
                        IsPremium = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsProfilePrivate = false
                    };
                    await _userRepository.AddAsync(user);
                }
                else
                {
                    // Update existing user's Google ID if not set
                    if (string.IsNullOrEmpty(user.GoogleId))
                    {
                        user.GoogleId = googleUserInfo.Id;
                        await _userRepository.UpdateAsync(user);
                    }
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);

                return Ok(new
                {
                    token,
                    userId = user.Id,
                    email = user.Email,
                    name = user.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google sign-in");
                return BadRequest(new { error = "Google sign-in failed.", details = ex.Message });
            }
        }

        [HttpPost("upload-profile-image")]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            try
            {
                // Get user ID from JWT token
                var userId = GetCurrentUserId();
                if (userId == null) return Unauthorized("Invalid user ID.");
                var user = await _userRepository.GetByIdAsync(userId.Value);

                if (user == null)
                {
                    return BadRequest(new { error = "Korisnik nije pronađen u bazi." });
                }

                // Ako korisnik već ima profilnu sliku, obriši staru datoteku
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    await _imageService.DeleteProfileImage(user.ProfileImageUrl);
                }

                // Spremi novu sliku
                var imageUrl = await _imageService.SaveProfileImageAsync(file);
                if (imageUrl == null)
                {
                    return BadRequest(new { error = "Neispravna datoteka ili format." });
                }

                // Ažuriraj korisnika u bazi
                var command = new UpdateProfileImageCommand
                {
                    UserId = userId.Value,
                    ProfileImageUrl = imageUrl
                };

                var success = await _updateProfileImageHandler.Handle(command);
                
                if (success)
                {
                    // Get updated user info and generate a new token
                    var updatedUser = await _userRepository.GetByIdAsync(userId.Value);
                    var newToken = _jwtService.GenerateToken(updatedUser!);

                    return Ok(new { 
                        success = true, 
                        profileImageUrl = updatedUser?.ProfileImageUrl,
                        token = newToken,
                        message = "Profilna slika je uspješno ažurirana."
                    });
                }
                else
                {
                    return BadRequest(new { error = "Korisnik nije pronađen." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile image");
                return BadRequest(new { error = "Došlo je do greške prilikom uploada slike." });
            }
        }

        [HttpDelete("remove-profile-image")]
        [Authorize]
        public async Task<IActionResult> RemoveProfileImage()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Obriši staru sliku ako postoji
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                await _imageService.DeleteProfileImage(user.ProfileImageUrl);
                user.ProfileImageUrl = null;
                await _userRepository.UpdateAsync(user); // Spremi promjene u bazu
            }

            // Generiraj novi token s ažuriranim podacima
            var updatedToken = _jwtService.GenerateToken(user);
            
            return Ok(new
            {
                message = "Profilna slika je uspješno uklonjena.",
                token = updatedToken
            });
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> Refresh()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token,
                user = new 
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.Name,
                    is_premium = user.IsPremium,
                    premium_expires_at = user.PremiumExpiresAt,
                    profile_image_url = user.ProfileImageUrl,
                    role = user.IsAdmin ? "admin" : "user",
                    is_profile_private = user.IsProfilePrivate,
                    description = user.Description,
                    gender = user.Gender,
                    birth_date = user.BirthDate,
                    hasPassword = !string.IsNullOrEmpty(user.PasswordHash)
                }
            });
        }

        [HttpGet("current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Automatski ispravi premium status na temelju datuma isteka
            if (user.PremiumExpiresAt.HasValue && user.PremiumExpiresAt.Value > DateTime.UtcNow)
            {
                if (!user.IsPremium)
                {
                    user.IsPremium = true;
                    user.SubscriptionStatus = "active";
                    await _userRepository.UpdateAsync(user);
                }
            }
            else if (user.IsPremium && user.PremiumExpiresAt.HasValue && user.PremiumExpiresAt.Value < DateTime.UtcNow)
            {
                user.IsPremium = false;
                user.SubscriptionStatus = "expired";
                await _userRepository.UpdateAsync(user);
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token,
                user = new 
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.Name,
                    is_premium = user.IsPremium,
                    premium_expires_at = user.PremiumExpiresAt,
                    profile_image_url = user.ProfileImageUrl,
                    role = user.IsAdmin ? "admin" : "user",
                    is_profile_private = user.IsProfilePrivate,
                    description = user.Description,
                    gender = user.Gender,
                    birth_date = user.BirthDate,
                    hasPassword = !string.IsNullOrEmpty(user.PasswordHash)
                }
            });
        }

        // NOVI ENDPOINTI
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return BadRequest("Query must be at least 2 characters long.");
            }

            try
            {
                var users = await _userRepository.SearchUsersAsync(query);
                var result = users.Select(u => new
                {
                    id = u.Id,
                    name = u.Name,
                    email = u.Email,
                    is_premium = u.IsPremium,
                    profile_image_url = u.ProfileImageUrl
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("profile/{identifier}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserProfile(string identifier)
        {
            try
            {
                User? user = null;
                
                // Pokušaj pronaći po emailu
                if (identifier.Contains("@"))
                {
                    user = await _userRepository.GetByEmailAsync(identifier);
                }
                else
                {
                    // Pokušaj pronaći po imenu
                    user = await _userRepository.GetByNameAsync(identifier);
                }

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Ako je profil privatan, prikaži samo osnovne podatke
                if (user.IsProfilePrivate)
                {
                    return Ok(new
                    {
                        id = user.Id,
                        name = user.Name,
                        is_profile_private = true,
                        is_premium = user.IsPremium,
                        profile_image_url = user.ProfileImageUrl
                    });
                }

                // Prikaži sve javne podatke
                return Ok(new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    is_premium = user.IsPremium,
                    profile_image_url = user.ProfileImageUrl,
                    gender = user.Gender,
                    birth_date = user.BirthDate,
                    description = user.Description,
                    is_profile_private = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);
            
            if (user == null)
            {
                return NotFound("User not found.");
            }

            try
            {
                // Ažuriraj polja ako su dostavljena
                if (request.Gender != null) user.Gender = request.Gender;
                if (request.BirthDate.HasValue) user.BirthDate = request.BirthDate.Value;
                if (request.Description != null) user.Description = request.Description;
                user.IsProfilePrivate = request.IsProfilePrivate;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                var token = _jwtService.GenerateToken(user);

                return Ok(new
                {
                    message = "Profile updated successfully.",
                    token,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        name = user.Name,
                        is_premium = user.IsPremium,
                        premium_expires_at = user.PremiumExpiresAt,
                        profile_image_url = user.ProfileImageUrl,
                        role = user.IsAdmin ? "admin" : "user",
                        is_profile_private = user.IsProfilePrivate,
                        description = user.Description,
                        gender = user.Gender,
                        birth_date = user.BirthDate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("update-name")]
        [Authorize]
        public async Task<IActionResult> UpdateName([FromBody] UpdateUserNameRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);
            
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (string.IsNullOrWhiteSpace(request.NewName) || request.NewName.Length < 2)
            {
                return BadRequest("Name must be at least 2 characters long.");
            }

            // Provjeri je li ime već zauzeto
            if (await _userRepository.IsNameTakenAsync(request.NewName, userId.Value))
            {
                return BadRequest("Name is already taken.");
            }

            try
            {
                user.Name = request.NewName.Trim();
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                var token = _jwtService.GenerateToken(user);

                return Ok(new
                {
                    message = "Name updated successfully.",
                    name = user.Name,
                    token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating name");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("update-password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            if (user.PasswordHash == null)
            {
                return BadRequest(new { error = "Korisnici prijavljeni putem Googlea nemaju postavljenu lozinku. Postavite lozinku putem opcije 'Zaboravljena lozinka' ili kontaktirajte podršku." });
            }
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { error = "Sva polja su obavezna." });
            }
            if (!Services.PasswordHelper.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { error = "Trenutna lozinka nije ispravna." });
            }
            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new { error = "Nova lozinka mora imati barem 6 znakova." });
            }
            user.PasswordHash = Services.PasswordHelper.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            var token = _jwtService.GenerateToken(user);
            return Ok(new { message = "Lozinka je uspješno promijenjena.", token });
        }

        [HttpPut("set-password")]
        [Authorize]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                return BadRequest(new { error = "Već imate postavljenu lozinku. Koristite promjenu lozinke." });
            }
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return BadRequest(new { error = "Nova lozinka mora imati barem 6 znakova." });
            }
            user.PasswordHash = Services.PasswordHelper.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            var token = _jwtService.GenerateToken(user);
            return Ok(new { message = "Lozinka je uspješno postavljena.", token });
        }

        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            try
            {
                // Get current user
                var user = await GetCurrentUserAsync(_userRepository);
                if (user == null)
                {
                    return NotFound(new { error = "Korisnik nije pronađen." });
                }

                // Validate password first (except for Google users without password)
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    if (string.IsNullOrEmpty(request.Password))
                    {
                        return BadRequest(new { error = "Lozinka je obavezna za potvrdu brisanja računa." });
                    }

                    if (!Services.PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
                    {
                        return BadRequest(new { error = "Pogrešna lozinka." });
                    }
                }

                // Check if user is premium - don't allow deletion if they have active subscription
                if (user.IsPremium)
                {
                    return BadRequest(new { error = "Ne možete obrisati račun dok ste premium korisnik. Sačekajte da vam završi pretplata" });
                }

                // Get all user's videos
                var userVideos = await _videoRepository.GetByUserIdAsync(user.Id);

                // Delete videos from Mux and database
                foreach (var video in userVideos)
                {
                    if (!string.IsNullOrEmpty(video.MuxAssetId))
                    {
                        try
                        {
                            await _muxService.DeleteAssetAsync(video.MuxAssetId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete Mux asset {AssetId} for user {UserId}", video.MuxAssetId, user.Id);
                        }
                    }

                    try
                    {
                        await _videoRepository.DeleteAsync(video.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete video {VideoId} from database for user {UserId}", video.Id, user.Id);
                    }
                }

                // Delete user's profile image if exists
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    await _imageService.DeleteProfileImage(user.ProfileImageUrl);
                }

                // Delete user from database
                // Note: We need to add DeleteAsync method to IUserRepository and UserRepository
                // For now, we'll use a direct SQL approach
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();

                try
                {
                    // Log the user ID being deleted for safety
                    _logger.LogInformation("Attempting to delete user {UserId} ({Email})", user.Id, user.Email);
                    
                    // Verify user exists before deletion
                    var verifyUserSql = "SELECT COUNT(*) FROM Users WHERE Id = @UserId";
                    var userCount = await connection.ExecuteScalarAsync<int>(verifyUserSql, new { UserId = user.Id }, transaction);
                    
                    if (userCount == 0)
                    {
                        _logger.LogWarning("User {UserId} not found in database during deletion", user.Id);
                        transaction.Rollback();
                        return NotFound(new { error = "Korisnik nije pronađen u bazi podataka." });
                    }
                    
                    if (userCount > 1)
                    {
                        _logger.LogError("Multiple users found with ID {UserId} - this should not happen!", user.Id);
                        transaction.Rollback();
                        return BadRequest(new { error = "Greška u bazi podataka - pronađeno više korisnika s istim ID-om." });
                    }

                    // Delete votes for user's videos (only for this specific user)
                    var deleteVotesSql = "DELETE FROM Votes WHERE VideoId IN (SELECT Id FROM Videos WHERE UserId = @UserId)";
                    var votesDeleted = await connection.ExecuteAsync(deleteVotesSql, new { UserId = user.Id }, transaction);
                    _logger.LogInformation("Deleted {VotesCount} votes for user {UserId}", votesDeleted, user.Id);

                    // Delete user (with explicit parameter check)
                    var deleteUserSql = "DELETE FROM Users WHERE Id = @UserId";
                    var usersDeleted = await connection.ExecuteAsync(deleteUserSql, new { UserId = user.Id }, transaction);
                    
                    if (usersDeleted != 1)
                    {
                        _logger.LogError("Expected to delete 1 user, but deleted {Count} users for ID {UserId}", usersDeleted, user.Id);
                        transaction.Rollback();
                        return BadRequest(new { error = "Greška prilikom brisanja korisnika - očekivano 1, obrisano " + usersDeleted });
                    }

                    _logger.LogInformation("Successfully deleted user {UserId} ({Email})", user.Id, user.Email);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during user deletion transaction for user {UserId}", user.Id);
                    transaction.Rollback();
                    throw;
                }

                return Ok(new { message = "Račun je uspješno obrisan." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user account");
                return BadRequest(new { error = "Greška prilikom brisanja računa." });
            }
        }
    }

    public class GoogleSignInRequest
    {
        public string AuthorizationCode { get; set; } = string.Empty;
    }

    public class UpdateUserNameRequest
    {
        public string NewName { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Description { get; set; }
        public bool IsProfilePrivate { get; set; }
    }

    public class UpdatePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class SetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    public class DeleteAccountRequest
    {
        public string? Password { get; set; }
    }
}
