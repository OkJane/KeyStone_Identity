using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken();
        string HashToken(string token);
    }
}
