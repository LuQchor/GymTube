using System;

namespace GymTube.API.Application.Commands
{
    public class UpdateUserNameCommand
    {
        public Guid UserId { get; set; }
        public string NewName { get; set; } = string.Empty;
    }
}