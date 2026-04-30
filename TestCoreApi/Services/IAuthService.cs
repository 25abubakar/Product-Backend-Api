using TestCoreApi.Models.DTOs;

namespace TestCoreApi.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? Error, AuthResponseDto? Result)> RegisterAsync(RegisterDto dto);

        Task<(bool Success, string? Error, AuthResponseDto? Result)> LoginAsync(LoginDto dto);
    }
}
