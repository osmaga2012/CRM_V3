using CRM.Dtos;
using CRM.Dtos.Login;

namespace CRM.V3.Shared.Interfaces
{
    public interface IAuthService
    {
        event Action OnLoginSuccess;
        void LoginOk();
        string? LastErrorMessage { get; }
        Task<LoginResultDto> LoginAsync(string email, string password);
        Task<string?> GetAccessTokenAsync();
        Task LogoutAsync();
    }
}
