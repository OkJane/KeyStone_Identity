using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Enums
{
    public enum AuditEventType
    {
        UserRegistrationFailed,
        UserRegistered,
        LoginSuccessful,
        LoginFailed,
        AccountLocked,
        PasswordChanged,
        RefreshTokenCreated,
        RefreshTokenRevoked,
        RefreshTokenRefreshFailed,
        EmailVerified
    }

    public enum FailureReason
    {
        UserNotFound,
        IncorrectPassword,
        AccountLocked,
        EmailUnverified
    }
}
