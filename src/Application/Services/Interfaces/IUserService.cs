using Tienda.src.Application.DTO.AuthDTO;

namespace Tienda.src.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<(string token, int userId)> LoginAsync(LoginDTO loginDTO, HttpContext httpContext);
        Task<string> RegisterAsync(RegisterDTO registerDTO, HttpContext httpContext);
        Task<string> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO);
        Task<string> ResendEmailVerificationCodeAsync(
            ResendEmailVerificationCodeDTO resendEmailVerificationCodeDTO
        );
        Task<int> DeleteUnconfirmedAsync();
        Task<string> RecoverPasswordAsync(RecoverPasswordDTO recoverPasswordDTO);
        Task<string> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);
    }
}