namespace GymTube.API.Application.Queries
{
    public class LoginUserQuery
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
