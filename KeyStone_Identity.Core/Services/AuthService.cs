using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Enums;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace KeyStone_Identity.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        public AuthService(IPasswordHasher passwordHasher, IUserRepository userRepository, ITokenService tokenService)
        {
            this._passwordHasher = passwordHasher;
            this._userRepository = userRepository;
            this._tokenService = tokenService;
        }

        public async Task<UserRegistrationResponseDTO> RegisterUser(UserRegistrationDTO userDTO)
        {
            try
            {
                if (await _userRepository.UserExists(userDTO.UserName, userDTO.EmailAddress))
                {
                    throw new Exception("Username or email is already in use");
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
            catch(Exception ex)
            {
                throw new Exception($"{ex.Message} \n Exception: {ex.ToString()} \n Inner Exception: {ex?.InnerException}");
            }
        }

        public async Task<JWTAuthResult> Login(LoginDTO loginDTO)
        {
            if (loginDTO == null)
            {
                throw new ArgumentNullException(nameof(loginDTO));
            }
            loginDTO.Username = loginDTO.Username.Trim().ToLower();

            var email = new MailAddress(loginDTO.Username);
            var user = await _userRepository.GetUser(loginDTO.Username);
            if (user == null)
            {
                return new JWTAuthResult
                {
                    Code = ResponseCodes.Failed,
                    Message = "Username or Password is incorrect",
                };
            }
            if(!_passwordHasher.VerifyHashedPassword(user.Password, loginDTO.Password))
            {
                return new JWTAuthResult
                {
                    Code = ResponseCodes.Failed,
                    Message = "Username or Password is incorrect",
                };
            }
            return _tokenService.GenerateToken(user);
        }
    }
}
