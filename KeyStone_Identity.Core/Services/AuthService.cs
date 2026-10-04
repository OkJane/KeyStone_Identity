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
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IEmailService _emailVerificationService;
        private readonly IActivationTokenRepository _activationTokenRepository;
        private readonly IConfiguration _configurationManager;
        private readonly ILogger<AuthService> _logger;
        private readonly IAuditLogService _auditLogService;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        
        public AuthService(IPasswordHasher passwordHasher, IUserRepository userRepository, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository, 
            IEmailService emailVerificationService, IActivationTokenRepository activationTokenRepository, IConfiguration configurationManager, ILogger<AuthService> logger, 
            IAuditLogService auditLogService, IRoleRepository roleRepository, IUserRoleRepository userRoleRepository)
        {
            this._passwordHasher = passwordHasher;
            this._userRepository = userRepository;
            this._tokenService = tokenService;
            this._refreshTokenRepository = refreshTokenRepository;
            _emailVerificationService = emailVerificationService;
            _activationTokenRepository = activationTokenRepository;
            _configurationManager = configurationManager;
            _logger = logger;
            _auditLogService = auditLogService;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
        }

        public async Task<UserRegistrationResponseDTO> RegisterUser(UserRegistrationDTO userDTO)
        {

            if (await _userRepository.UserExists(userDTO.UserName, userDTO.EmailAddress))
            {
                _logger.LogWarning("User registration failed because the user already exists. Username : {UserName} ", userDTO.UserName);
                await _auditLogService.LogAsync(AuditEventType.UserRegistrationFailed,null,false,userDTO.UserName, $"User already exists");
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
                RoleId = role.Id,
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
           await _emailVerificationService.SendActivationMail(userDTO.FirstName,userDTO.EmailAddress, token);

            _logger.LogInformation("New user registration {UserId}", response.ID);
            await _auditLogService.LogAsync(AuditEventType.UserRegistered,response.ID, true,user.UserName, $"User registered successfully");

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

            if(user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                int timeLeftTillUnlock = (int)(user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes;
                _logger.LogWarning("Failed user login due to locked account. UserId : {UserId}", user.ID);
                await _auditLogService.LogAsync(AuditEventType.LoginFailed, user.ID, false, loginDTO.Username, FailureReason.AccountLocked.ToString());
                throw new UserAccountLockedException(timeLeftTillUnlock);
            }

            if(user.LockedUntil.HasValue && user.LockedUntil <= DateTime.UtcNow && user.FailedLoginAttempt >= maxLoginAttempt)
            {
                user.FailedLoginAttempt = 0;
            }

            if (!_passwordHasher.VerifyHashedPassword(user.Password, loginDTO.Password))
            {
                var lockedUntil = _configurationManager["AccountLockoutDurationInMinutes"];
                user.FailedLoginAttempt += 1;
                if(user.FailedLoginAttempt >= maxLoginAttempt)
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
            var jwtAuthResult = await _tokenService.GenerateToken(user, role);
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

            var jwtAuthResult = await _tokenService.GenerateToken(user, role);
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
            if(activationToken.ActivatedAt != null)
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
            if(new EmailAddressAttribute().IsValid(username))
            {
                user = await _userRepository.RetrieveUserByEmailAddress(username);
            }
            else
            {
                user = await _userRepository.RetrieveUserByUserName(username);
            }

            if(user == null) { throw new UserNotFoundException(username); }

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
    }
}
