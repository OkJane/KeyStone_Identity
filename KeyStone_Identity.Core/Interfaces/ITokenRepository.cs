using KeyStone_Identity.Core.Enums;
using KeyStone_Identity.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface ITokenRepository
    {
        Task<Token> Upsert(Token token);
        Task<Token> GetToken(string hashedToken);
        Task InvalidateTokens(long userId, TokenTypes type);
        Task<bool> ConsumeToken(string hashedToken, TokenTypes type);
    }
}
