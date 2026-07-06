using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool VerifyHashedPassword(string passwordHash, string providedPassword);
    }
}
