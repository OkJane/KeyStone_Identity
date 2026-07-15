using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Enums;
using KeyStone_Identity.Core.Exceptions;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
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
        public AuthService(IPasswordHasher passwordHasher, IUserRepository userRepository, ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository)
        {
            this._passwordHasher = passwordHasher;
            this._userRepository = userRepository;
            this._tokenService = tokenService;
            this._refreshTokenRepository = refreshTokenRepository;
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
            };
            var response = await _userRepository.Upsert(user);
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
            if (!_passwordHasher.VerifyHashedPassword(user.Password, loginDTO.Password))
            {
                throw new InvalidCredentialsException();
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
    }
}
