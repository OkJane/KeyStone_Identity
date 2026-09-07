using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Exceptions
{
    public abstract class DomainException : Exception
    {
        public abstract int StatusCode { get; }
        protected DomainException(string message) : base(message) { }
    }

    public class InvalidCredentialsException : DomainException
    {
        public override int StatusCode => 401;
        public InvalidCredentialsException() : base("Invalid username or password.") { }
    }

    public class UserNotActiveException : DomainException
    {
        public override int StatusCode => 500;
        public UserNotActiveException() : base("User is not active") {}
    }

    public class UserNotFoundException : DomainException
    {
        public override int StatusCode => 404;
        public UserNotFoundException(string identifier)
            : base($"User '{identifier}' was not found.") { }
    }

    public class UserAlreadyExistsException : DomainException
    {
        public override int StatusCode => 409;
        public UserAlreadyExistsException(string usernameOrEmail) : base($"An account with username or email {usernameOrEmail} already exists."){ }
    }

    public class UserAccountLockedException : DomainException
    {
        public override int StatusCode => 401;
        public UserAccountLockedException(int timeLeftTillUnlock) : base($"Your account has been temporarily locked due to multiple failed login attempts. Please try again in {timeLeftTillUnlock} minutes.")
        {
            
        }
    }
}
