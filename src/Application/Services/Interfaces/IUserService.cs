using Tienda.src.Application.DTO.AuthDTO;
using Tienda.src.Application.DTO.UserDTO;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;


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

        Task<UserProfileDTO> GetProfileAsync(int userId);
        
        Task<UserProfileDTO> UpdateProfileAsync(int userId, UpdateProfileDTO dto);

        Task ChangePasswordAsync(int userId, ChangePasswordDTO dto, HttpContext httpContext);

    }
}