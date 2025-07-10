using System;

namespace GymTube.API.Application.Commands
{
    public class UpdateProfileImageCommand
    {
        public Guid UserId { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}