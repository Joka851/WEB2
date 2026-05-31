using UserService.DTOs;

namespace UserService.Services
{
    public interface IAuthService
    {
        Task<UserDto?> Register(RegisterDto dto);
        Task<string?> Login(LoginDto dto);
    }
}