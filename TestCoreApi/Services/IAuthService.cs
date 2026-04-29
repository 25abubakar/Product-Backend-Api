using TestCoreApi.Models.DTOs;

namespace TestCoreApi.Services
{
    public interface IAuthService
    {
        /// <summary>Returns (success, errorMessage, result)</summary>
        Task<(bool Success, string? Error, AuthResponseDto? Result)> RegisterAsync(RegisterDto dto);

        /// <summary>Returns (success, errorMessage, result)</summary>
        Task<(bool Success, string? Error, AuthResponseDto? Result)> LoginAsync(LoginDto dto);
    }
}
