using Microsoft.EntityFrameworkCore;
using TestCoreApi.Data;
using TestCoreApi.Models;
using TestCoreApi.Models.DTOs;

namespace TestCoreApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IJwtService  _jwt;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IJwtService jwt, IConfiguration config)
        {
            _db     = db;
            _jwt    = jwt;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email.ToLower()))
                throw new InvalidOperationException("Email is already registered.");

            var user = new User
            {
                FullName     = dto.FullName.Trim(),
                Email        = dto.Email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role         = "User",
                CreatedAt    = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return BuildResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower().Trim());

            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return BuildResponse(user);
        }

        private AuthResponseDto BuildResponse(User user)
        {
            var expiryMins = int.Parse(_config["JwtSettings:ExpiryMinutes"]!);
            var token      = _jwt.GenerateToken(user);

            return new AuthResponseDto
            {
                Token     = token,
                Email     = user.Email,
                FullName  = user.FullName,
                Role      = user.Role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMins)
            };
        }
    }
}
