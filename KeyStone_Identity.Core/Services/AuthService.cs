using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Enums;
using KeyStone_Identity.Core.Exceptions;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Core.Utilities;
using Microsoft.Extensions.Configuration;
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
        public AuthService(IPasswordHasher passwordHasher, IUserRepository userRepository, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository, IEmailService emailVerificationService, IActivationTokenRepository activationTokenRepository, IConfiguration configurationManager)
        {
            this._passwordHasher = passwordHasher;
            this._userRepository = userRepository;
            this._tokenService = tokenService;
            this._refreshTokenRepository = refreshTokenRepository;
            _emailVerificationService = emailVerificationService;
            _activationTokenRepository = activationTokenRepository;
            _configurationManager = configurationManager;
        }

        public async Task<UserRegistrationResponseDTO> RegisterUser(UserRegistrationDTO userDTO)
        {

            if (await _userRepository.UserExists(userDTO.UserName, userDTO.EmailAddress))
            {
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
                throw new InvalidCredentialsException();
            }

            if(user.LockedUntil > DateTime.UtcNow)
            {
                int timeLeftTillUnlock = (int)(user.LockedUntil - DateTime.UtcNow).TotalMinutes;
                throw new UserAccountLockedException(timeLeftTillUnlock);
            }

            if(user.LockedUntil != null && user.LockedUntil <= DateTime.UtcNow && user.FailedLoginAttempt >= maxLoginAttempt)
            {
                user.FailedLoginAttempt = 0;
            }

            if (!_passwordHasher.VerifyHashedPassword(user.Password, loginDTO.Password))
            {
                user.FailedLoginAttempt += 1;
                if(user.FailedLoginAttempt >= maxLoginAttempt)
                {
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configurationManager["AccountLockoutDurationInMinutes"]));
                    user.FailedLoginAttempt = 0;
                }
                await _userRepository.Upsert(user);
                throw new InvalidCredentialsException();
            }
            if (user.IsEmailVerified == false)
            {
                throw new UserNotActiveException();
            }
            await _refreshTokenRepository.RevokeTokens(user.ID);
            var jwtAuthResult = await _tokenService.GenerateToken(user);
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

            return jwtAuthResult;
        }

        public async Task<JWTAuthResult> Refresh(string refreshTokenString)
        {
            var refreshToken = await _refreshTokenRepository.GetToken(refreshTokenString);
            if (refreshToken == null || refreshToken.ExpiresAt <= DateTime.UtcNow || refreshToken.RevokedAt != null)
            {
                throw new TokenExpiredException();
            }
            var user = await _userRepository.GetUserById(refreshToken.UserId);
            if (user == null)
            {
                throw new UserNotFoundException(refreshToken.UserId.ToString());
            }

            await _refreshTokenRepository.RevokeTokens(user.ID);

            var jwtAuthResult = await _tokenService.GenerateToken(user);
            var newRefreshToken = new RefreshToken()
            {
                UserId = user.ID,
                Token = jwtAuthResult.RefreshToken,
                ExpiresAt = jwtAuthResult.RefreshTokenExpirationDate,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.Save(newRefreshToken);
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

            return "Verification email has been sent successfully.";


        }
    }
}
