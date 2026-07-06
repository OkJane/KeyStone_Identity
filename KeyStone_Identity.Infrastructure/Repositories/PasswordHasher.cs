using KeyStone_Identity.Core.Interfaces;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Repositories
{
    public class PasswordHasher : IPasswordHasher
    {
        //ASP Identity
        //private readonly IPasswordHasher<object> _passwordHasher;

        //public PasswordHasher(IPasswordHasher<object> passwordHasher)
        //{
        //    this._passwordHasher = passwordHasher;
        //}
        //public string Hash(string password)
        //{
        //    return _passwordHasher.HashPassword(null, password);
        //}

        //public bool VerifyHashedPassword(string passwordHash, string providedPassword)
        //{
        //    var result = _passwordHasher.VerifyHashedPassword(null, passwordHash, providedPassword);
        //    return result == PasswordVerificationResult.Success;
        //}

        //KeyStone
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool VerifyHashedPassword(string passwordHash, string providedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, passwordHash);
        }
    }
}
