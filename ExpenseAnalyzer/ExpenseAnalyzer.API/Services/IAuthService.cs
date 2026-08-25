using ExpenseAnalyzer.API.DTOs;

namespace ExpenseAnalyzer.API.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterUserDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}
