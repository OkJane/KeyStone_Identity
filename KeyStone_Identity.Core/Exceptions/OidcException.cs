using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Exceptions
{
    public abstract class OidcException : DomainException
    {
        public abstract string ErrorCode { get; }
        protected OidcException(string message) : base(message)
        {
            
        }
    }

    public class InvalidClientException : OidcException
    {
        public override int StatusCode => 401;
        public override string ErrorCode => "invalid_client";
        public InvalidClientException(string clientId)
            : base($"Client '{clientId}' is not registered or credentials are invalid.") { }
    }

    public class TokenExpiredException : OidcException
    {
        public override int StatusCode => 401;
        public override string ErrorCode => "invalid_token";
        public TokenExpiredException()
            : base("The token has expired.") { }
    }
}
