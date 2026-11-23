using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.DTOs.Request;
using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Enums;
using KeyStone_Identity.Core.Exceptions;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text;

namespace KeyStone_Identity.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserRepository _userRepository;
        private readonly IJWTokenService _jwttokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEmailService _emailVerificationService;
        private readonly IActivationTokenRepository _activationTokenRepository;
        private readonly IConfiguration _configurationManager;
        private readonly ILogger<AuthService> _logger;
        private readonly IAuditLogService _auditLogService;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly ITokenService _tokenService;
        private readonly ITokenRepository _tokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IPasswordHasher passwordHasher, IUserRepository userRepository, IJWTokenService jwttokenService, IRefreshTokenRepository refreshTokenRepository,
            IEmailService emailVerificationService, IActivationTokenRepository activationTokenRepository, IConfiguration configurationManager, ILogger<AuthService> logger,
            IAuditLogService auditLogService, IRoleRepository roleRepository, IUserRoleRepository userRoleRepository, ITokenService tokenService,
            ITokenRepository tokenRepository, IUnitOfWork unitOfWork)
        {
            this._passwordHasher = passwordHasher;
            this._userRepository = userRepository;
            this._jwttokenService = jwttokenService;
            this._refreshTokenRepository = refreshTokenRepository;
            _emailVerificationService = emailVerificationService;
            _activationTokenRepository = activationTokenRepository;
            _configurationManager = configurationManager;
            _logger = logger;
            _auditLogService = auditLogService;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _tokenService = tokenService;
            _tokenRepository = tokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserRegistrationResponseDTO> RegisterUser(UserRegistrationDTO userDTO)
        {

            if (await _userRepository.UserExists(userDTO.UserName, userDTO.EmailAddress))
            {
                _logger.LogWarning("User registration failed because the user already exists. Username : {UserName} ", userDTO.UserName);
                await _auditLogService.LogAsync(AuditEventType.UserRegistrationFailed, null, false, userDTO.UserName, $"User already exists");
                throw new UserAlreadyExistsException(userDTO.UserName ?? userDTO.EmailAddress);
            }
            var passwordHash = _passwordHasher.Hash(userDTO.Password);
            User user = new User()
            {
                FirstName = userDTO.FirstName,
                MiddleName = userDTO.MiddleName,
                LastName = userDTO.LastName,
                DateOfBirth = userDTO.DateOfBirth,
                UserName = userDTO.UserName,
                EmailAddress = userDTO.EmailAddress,
                Password = passwordHash,
                DateCreated = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                IsEmailVerified = false,
            };
            var response = await _userRepository.Upsert(user);

            var role = await _roleRepository.GetRoleByRoleName(UserRoles.User.ToString());


            _logger.LogInformation("Assigning Role {role} to {UserId}", role.Name, response.ID);
            var userRole = new UserRole
            {
                RoleId = role.ID,
                UserId = response.ID
            };
            await _userRoleRepository.Insert(userRole);

            var token = _emailVerificationService.GenerateActivationToken();
            var activationToken = new ActivationToken()
            {
                UserId = response.ID,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToInt32(_configurationManager["EmailVerification:ExpirationHours"])),
            };
            await _activationTokenRepository.Upsert(activationToken);
            await _emailVerificationService.SendActivationMail(userDTO.FirstName, userDTO.EmailAddress, token);

            _logger.LogInformation("New user registration {UserId}", response.ID);
            await _auditLogService.LogAsync(AuditEventType.UserRegistered, response.ID, true, user.UserName, $"User registered successfully");

            return new UserRegistrationResponseDTO
            {
                ID = response.ID,
                UserName = response.UserName,
                FirstName = response.FirstName,
                LastName = response.LastName,
                EmailAddress = response.EmailAddress
            };
        }


        public async Task<JWTAuthResult> Login(LoginDTO loginDTO)
        {
            int maxLoginAttempt = Convert.ToInt32(_configurationManager["MaxLoginAttempt"]);
            if (loginDTO == null)
            {
                throw new ArgumentNullException(nameof(loginDTO));
            }
            loginDTO.Username = loginDTO.Username.Trim().ToLower();
            User? user;

            if (new EmailAddressAttribute().IsValid(loginDTO.Username))
            {
                user = await _userRepository.RetrieveUserByEmailAddress(loginDTO.Username);
            }
            else
            {
                user = await _userRepository.RetrieveUserByUserName(loginDTO.Username);
            }

            if (user == null)
            {
                _logger.LogWarning("Failed user login due to invalid credentials. UserName - {UserName}", loginDTO.Username);
                await _auditLogService.LogAsync(AuditEventType.LoginFailed, null, false, loginDTO.Username, FailureReason.UserNotFound.ToString());
                throw new InvalidCredentialsException();
            }

            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                int timeLeftTillUnlock = (int)(user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes;
                _logger.LogWarning("Failed user login due to locked account. UserId : {UserId}", user.ID);
                await _auditLogService.LogAsync(AuditEventType.LoginFailed, user.ID, false, loginDTO.Username, FailureReason.AccountLocked.ToString());
                throw new UserAccountLockedException(timeLeftTillUnlock);
            }

            if (user.LockedUntil.HasValue && user.LockedUntil <= DateTime.UtcNow && user.FailedLoginAttempt >= maxLoginAttempt)
            {
                user.FailedLoginAttempt = 0;
            }

            if (!_passwordHasher.VerifyHashedPassword(user.Password, loginDTO.Password))
            {
                var lockedUntil = _configurationManager["AccountLockoutDurationInMinutes"];
                user.FailedLoginAttempt += 1;
                if (user.FailedLoginAttempt >= maxLoginAttempt)
                {
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(Convert.ToInt32(lockedUntil));
                    user.FailedLoginAttempt = 0;
                    _logger.LogWarning("User account has been locked for {LockOutDuration} minutes. UserName - {UserName}", lockedUntil, loginDTO.Username);
                    await _auditLogService.LogAsync(AuditEventType.AccountLocked, user.ID, false, loginDTO.Username);
                }
                await _userRepository.Upsert(user);
                _logger.LogWarning("Failed user login due to invalid credentials. UserName - {UserName}", loginDTO.Username);
                await _auditLogService.LogAsync(AuditEventType.LoginFailed, user.ID, false, loginDTO.Username, FailureReason.IncorrectPassword.ToString());
                throw new InvalidCredentialsException();
            }
            if (user.IsEmailVerified == false)
            {
                _logger.LogWarning("Failed user login. Email is unverified. UserId - {UserId}", user.ID);
                await _auditLogService.LogAsync(AuditEventType.LoginFailed, user.ID, false, loginDTO.Username, FailureReason.EmailUnverified.ToString());
                throw new UserNotActiveException();
            }
            await _refreshTokenRepository.RevokeTokens(user.ID);

            var role = await _roleRepository.GetRoleByUserId(user.ID);
            var jwtAuthResult = await _jwttokenService.GenerateToken(user, role);
            var refreshToken = new RefreshToken()
            {
                UserId = user.ID,
                Token = jwtAuthResult.RefreshToken,
                ExpiresAt = jwtAuthResult.RefreshTokenExpirationDate,
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenRepository.Save(refreshToken);

            user.FailedLoginAttempt = 0;
            await _userRepository.Upsert(user);

            _logger.LogInformation("Successful user login. User ID : {UserId}", user.ID);
            await _auditLogService.LogAsync(AuditEventType.LoginSuccessful, user.ID, true, loginDTO.Username);
            return jwtAuthResult;
        }

        public async Task<JWTAuthResult> Refresh(string refreshTokenString)
        {
            var refreshToken = await _refreshTokenRepository.GetToken(refreshTokenString);
            if (refreshToken == null || refreshToken.ExpiresAt <= DateTime.UtcNow || refreshToken.RevokedAt.HasValue)
            {
                _logger.LogWarning("Refresh token expired.");
                await _auditLogService.LogAsync(AuditEventType.RefreshTokenRefreshFailed, null, false);
                throw new TokenExpiredException();
            }
            var user = await _userRepository.GetUserById(refreshToken.UserId);
            if (user == null)
            {
                _logger.LogWarning("Refresh token failed because user was not found. UserId: {UserId}", refreshToken.UserId);
                await _auditLogService.LogAsync(AuditEventType.RefreshTokenRevoked, null, false, null, FailureReason.UserNotFound.ToString());
                throw new UserNotFoundException(refreshToken.UserId.ToString());
            }

            await _refreshTokenRepository.RevokeTokens(user.ID);

            var role = await _roleRepository.GetRoleByUserId(user.ID);

            var jwtAuthResult = await _jwttokenService.GenerateToken(user, role);
            var newRefreshToken = new RefreshToken()
            {
                UserId = user.ID,
                Token = jwtAuthResult.RefreshToken,
                ExpiresAt = jwtAuthResult.RefreshTokenExpirationDate,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.Save(newRefreshToken);
            _logger.LogInformation("Refresh token successfully generated. UserId : {UserId}", user.ID);
            await _auditLogService.LogAsync(AuditEventType.RefreshTokenCreated, user.ID, true, user.UserName);
            return jwtAuthResult;

        }

        public async Task<string> ActivateAccount(string token)
        {
            var activationToken = await _activationTokenRepository.Get(token);
            if (activationToken == null || activationToken.ExpiresAt <= DateTime.UtcNow || activationToken.RevokedAt != null)
            {
                throw new TokenExpiredException();
            }
            if (activationToken.ActivatedAt != null)
            {
                return "Email has been verified";
            }

            var userID = activationToken.UserId;
            var user = await _userRepository.GetUserById(userID);

            if (user == null) throw new UserNotFoundException(userID.ToString());

            user.IsEmailVerified = true;
            activationToken.ActivatedAt = DateTime.UtcNow;
            await _userRepository.Upsert(user);
            await _activationTokenRepository.Upsert(activationToken);
            _logger.LogInformation("Email successfully verified. Email : {Email}", user.EmailAddress);
            await _auditLogService.LogAsync(AuditEventType.EmailVerified, user.ID, true, user.EmailAddress);
            return "Email verification is successful";

        }

        public async Task<string> ResendEmailVerification(string username)
        {
            User? user;
            if (new EmailAddressAttribute().IsValid(username))
            {
                user = await _userRepository.RetrieveUserByEmailAddress(username);
            }
            else
            {
                user = await _userRepository.RetrieveUserByUserName(username);
            }

            if (user == null) { throw new UserNotFoundException(username); }

            if (user.IsEmailVerified) { return "Email address has already been verified"; }

            var existingActivationToken = await _activationTokenRepository.GetLatestTokenByUser(user.ID);

            existingActivationToken.RevokedAt = DateTime.UtcNow;
            await _activationTokenRepository.Upsert(existingActivationToken);

            var newToken = _emailVerificationService.GenerateActivationToken();
            var activationToken = new ActivationToken()
            {
                UserId = user.ID,
                Token = newToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToInt32(_configurationManager["EmailVerification:ExpirationHours"])),
            };
            await _activationTokenRepository.Upsert(activationToken);
            await _emailVerificationService.SendActivationMail(user.FirstName, user.EmailAddress, newToken);
            _logger.LogInformation("Verification email successfully resent. Email : {Email}", user.EmailAddress);

            return "Verification email has been sent successfully.";


        }

        public async Task<List<AuditHistoryResponse>> GetAuditHistory(AuditHistoryRequest request)
        {
            return await _auditLogService.GetAuditHistory(request);
        }


        public async Task<string> ResetPassword(string emailAddress)
        {
            if (!new EmailAddressAttribute().IsValid(emailAddress))
                return "An email has been sent to your email address";

            var user = await _userRepository
                .RetrieveUserByEmailAddress(emailAddress.Trim());

            if (user == null)
                return "An email has been sent to your email address";

            var token = _tokenService.GenerateToken();
            var hashedToken = _tokenService.HashToken(token);

            var now = DateTime.UtcNow;

            var tokenLifetime = Convert.ToInt32(
                _configurationManager["TokenLifetimeInMinutes"]);

            // Invalidate previously issued reset tokens.
            await _tokenRepository.InvalidateTokens(
                user.ID,
                TokenTypes.Reset);

            var tokenData = new Token
            {
                UserId = user.ID,
                TokenHash = hashedToken,
                Type = TokenTypes.Reset,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(tokenLifetime)
            };

            await _tokenRepository.Upsert(tokenData);

            var body = $"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
        </head>
        <body style="margin:0;padding:0;background-color:#f4f6f8;
                     font-family:Arial,sans-serif;">

            <div style="max-width:560px;margin:40px auto;padding:35px;
                        background-color:#ffffff;border-radius:8px;">

                <h2 style="color:#202938;">
                    Reset Your KeyStone Password
                </h2>

                <p style="color:#4b5563;font-size:15px;line-height:1.6;">
                    Hello,
                </p>

                <p style="color:#4b5563;font-size:15px;line-height:1.6;">
                    We received a request to reset the password associated
                    with your KeyStone account.
                </p>

                <p style="color:#4b5563;font-size:15px;">
                    Use the verification code below to proceed:
                </p>

                <div style="margin:30px 0;padding:20px;
                            background-color:#f4f6f8;border-radius:8px;
                            text-align:center;">

                    <span style="font-size:32px;font-weight:bold;
                                 letter-spacing:8px;color:#2563eb;">
                        {token}
                    </span>

                </div>

                <p style="color:#4b5563;font-size:14px;line-height:1.6;">
                    This code expires in <strong>{tokenLifetime} minutes</strong>
                    and can only be used once.
                </p>

                <p style="color:#4b5563;font-size:14px;line-height:1.6;">
                    If you did not request a password reset, you can safely
                    ignore this email. Your password will remain unchanged.
                </p>

                <hr style="border:none;border-top:1px solid #e5e7eb;
                           margin:30px 0;">

                <p style="color:#9ca3af;font-size:12px;line-height:1.5;">
                    Never share this code with anyone.
                    KeyStone will never ask you to disclose your password
                    or verification code.
                </p>

                <p style="color:#6b7280;font-size:13px;">
                    Regards,<br>
                    <strong>KeyStone Security Team</strong>
                </p>

            </div>
        </body>
        </html>
        """;

            try
            {
                await _emailVerificationService.SendEmailAsync(
                    user.EmailAddress,
                    "Reset Your KeyStone Password",
                    body);
            }
            catch (Exception)
            {
                // Invalidate the token if email delivery fails.
                await _tokenRepository.InvalidateTokens(
                    user.ID,
                    TokenTypes.Reset);

                throw;
            }

            return "An email has been sent to your email address";
        }





        public async Task<string> SetNewPassword(
    ResetPasswordRequest resetPasswordRequest)
        {
            if (string.IsNullOrWhiteSpace(resetPasswordRequest.Token) ||
                string.IsNullOrWhiteSpace(resetPasswordRequest.NewPassword))
            {
                return "Invalid password reset request";
            }

            var hashedToken = _tokenService.HashToken(
                resetPasswordRequest.Token);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var token = await _tokenRepository.GetToken(hashedToken);

                if (token == null ||
                    token.Type != TokenTypes.Reset ||
                    token.ExpiresAt <= DateTime.UtcNow ||
                    token.UsedAt.HasValue)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return "Kindly enter a valid token";
                }

                var user = await _userRepository.GetUserById(token.UserId);

                if (user == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return "Unable to reset password";
                }

                // Atomically consume the token.
                var consumed = await _tokenRepository.ConsumeToken(
                    hashedToken,
                    TokenTypes.Reset);

                if (!consumed)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return "Kindly enter a valid token";
                }

                var hashedPassword = _passwordHasher.Hash(
                    resetPasswordRequest.NewPassword);

                user.Password = hashedPassword;

                await _userRepository.Upsert(user);

                await _refreshTokenRepository.RevokeTokens(user.ID);

                await _unitOfWork.CommitTransactionAsync();

                return "Your password has been reset successfully";
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

    }
}
