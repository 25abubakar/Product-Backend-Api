using Microsoft.EntityFrameworkCore;
using TestCoreApi.Data;
using TestCoreApi.Models;
using TestCoreApi.Models.DTOs;

namespace TestCoreApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext    _db;
        private readonly IJwtService     _jwt;
        private readonly IConfiguration  _config;

        public AuthService(AppDbContext db, IJwtService jwt, IConfiguration config)
        {
            _db     = db;
            _jwt    = jwt;
            _config = config;
        }

        public async Task<(bool Success, string? Error, AuthResponseDto? Result)> RegisterAsync(RegisterDto dto)
        {
            var emailNorm = dto.Email.ToLower().Trim();

            if (await _db.Users.AnyAsync(u => u.Email == emailNorm))
                return (false, "Email is already registered.", null);

            var user = new User
            {
                FullName     = dto.FullName.Trim(),
                Email        = emailNorm,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role         = "User",
                CreatedAt    = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return (true, null, BuildResponse(user));
        }

        public async Task<(bool Success, string? Error, AuthResponseDto? Result)> LoginAsync(LoginDto dto)
        {
            var emailNorm = dto.Email.ToLower().Trim();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == emailNorm);

            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return (false, "Invalid email or password.", null);

            return (true, null, BuildResponse(user));
        }

        private AuthResponseDto BuildResponse(User user)
        {
            var expiryMins = int.Parse(_config["JwtSettings:ExpiryMinutes"]!);
            return new AuthResponseDto
            {
                TokenType  = "Bearer",
                AccessToken = _jwt.GenerateToken(user),
                Email     = user.Email,
                FullName  = user.FullName,
                Role      = user.Role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMins)
            };
        }
    }
}
