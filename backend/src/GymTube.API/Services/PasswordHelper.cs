namespace GymTube.API.Services
{
    public static class PasswordHelper
    {
        // Hashiranje lozinke s bcrypt-om
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Provjera lozinke s bcrypt-om
        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
} 