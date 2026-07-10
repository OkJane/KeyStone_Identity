using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface ITokenService
    {
        Task<JWTAuthResult> GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
