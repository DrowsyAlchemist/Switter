using UserService.DTOs;

namespace UserService.Interfaces.Commands
{
    public interface IProfileCommands
    {
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    }
}
