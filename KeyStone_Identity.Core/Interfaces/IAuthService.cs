using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface IAuthService
    {
        Task<UserRegistrationResponseDTO> RegisterUser(UserRegistrationDTO user);
        Task<JWTAuthResult> Login(LoginDTO loginDTO);
        Task<JWTAuthResult> Refresh(string refreshTokenString);
        Task<string> ActivateAccount(string token);
        Task<string> ResendEmailVerification(string username);
    }
}
