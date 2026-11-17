using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http.Json;
using System.Security;
using System.Text;

namespace KeyStone_Identity.IntegrationTests.Auth
{
    public class LoginTests : IClassFixture<KeyStoneApiFactory>
    {
        private readonly KeyStoneApiFactory _factory;
        public LoginTests(KeyStoneApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccess()
        {
            // ARRANGE
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider
                .GetRequiredService<KeyStone_Identity_DbContext>();

            var user = new User
            {
                UserName = "janet",
                FirstName = "Jane",
                MiddleName = "Doe",
                LastName = "Test",
                EmailAddress = "jane@example2.com",
                Password = BCrypt.Net.BCrypt.HashPassword(
                    "CorrectPassword1!", workFactor: 12),
                DateOfBirth = new DateOnly(2007, 9, 5),
                IsEmailVerified = true
            };

            var role = new Role
            {
                Name = "User"
            };

            var permission = new Permissions
            {
                Name = "profile.read",
                CreatedAt = DateTime.UtcNow,
                Description = "Test"
            };

            db.Users.Add(user);
            db.Roles.Add(role);
            db.Permissions.Add(permission);

            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole
            {
                UserId = user.ID,
                RoleId = role.ID
            });

            db.RolePermissions.Add(new RolePermissions
            {
                RoleID = role.ID,
                PermissionID = permission.ID
            });

            await db.SaveChangesAsync();

            var client = _factory.CreateClient();

            // ACT
            var response = await client.PostAsJsonAsync(
                "/api/Auth/Login",
                new LoginDTO
                {
                    Username = "jane@example2.com",
                    Password = "CorrectPassword1!"
                });

            // ASSERT
            Assert.Equal(
                System.Net.HttpStatusCode.OK,
                response.StatusCode);

        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsFailure()
        {
            //ARRANGE
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KeyStone_Identity_DbContext>();

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1!", workFactor: 12);
            db.Users.Add(new User
            {
                UserName = "janet",
                FirstName = "Jane",
                MiddleName = "Doe",
                LastName = "Test",
                EmailAddress = "jane@example.com",
                Password = hashedPassword,
                DateOfBirth = new DateOnly(2007, 9, 5)
            });
            await db.SaveChangesAsync();
            var client = _factory.CreateClient();

            //ACT
            var response = await client.PostAsJsonAsync("/api/Auth/Login", new LoginDTO()
            {
                Username = "kane@example.com",
                Password = "WrongPassword!"
            });
            //ASSERT
            Assert.NotEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

    }
}
