using TestCoreApi.Models;

namespace TestCoreApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
