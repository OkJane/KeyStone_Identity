using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Core.DTOs.Request;
using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace KeyStone_Identity.IntegrationTests.Auth
{
    public class RoleAccessTests : IClassFixture<KeyStoneApiFactory>
    {
        private KeyStoneApiFactory _keyStoneApiFactory;
        public RoleAccessTests(KeyStoneApiFactory keyStoneApiFactory)
        {
            _keyStoneApiFactory = keyStoneApiFactory;
        }


        [Fact]
        public async Task AdminRole_CanViewAuditLogs()
        {
            //ARRANGE
            using var scope = _keyStoneApiFactory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KeyStone_Identity_DbContext>();

            var client = _keyStoneApiFactory.CreateClient();

            var token = await GenerateTokenWithPermission("Admin","audit.read_all");
            var auditHistoryRequest = new AuditHistoryRequest() { };
            
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/Auth/AuditHistory")
            {
                Content = JsonContent.Create(auditHistoryRequest)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            //ACT
            var response = await client.SendAsync(request);

            //ASSERT
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UserRole_CannotViewAuditLogs()
        {
            //ARRANGE
            using var scope = _keyStoneApiFactory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KeyStone_Identity_DbContext>();

            var client = _keyStoneApiFactory.CreateClient();

            var token = await GenerateTokenWithPermission("User","profie.read");
            var auditHistoryRequest = new AuditHistoryRequest() { };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/Auth/AuditHistory")
            {
                Content = JsonContent.Create(auditHistoryRequest)
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            //ACT
            var response = await client.SendAsync(request);

            //ASSERT
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        public async Task<string> GenerateTokenWithPermission(
    string role,
    string permission)
        {
            using var scope = _keyStoneApiFactory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KeyStone_Identity_DbContext>();

            var user = await db.Users
                .FirstOrDefaultAsync(u => u.EmailAddress == "jane3@example.com");

            if (user is null)
            {
                user = new User
                {
                    UserName = "janet",
                    FirstName = "Jane",
                    MiddleName = "Doe",
                    LastName = "Test",
                    EmailAddress = "jane3@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword(
                        "CorrectPassword1!", workFactor: 12),
                    DateOfBirth = new DateOnly(2007, 9, 5),
                    IsEmailVerified = true
                };

                db.Users.Add(user);
            }

            var roleEntity = await db.Roles
                .FirstOrDefaultAsync(r => r.Name == role);

            if (roleEntity is null)
            {
                roleEntity = new Role { Name = role };
                db.Roles.Add(roleEntity);
            }

            var permissionEntity = await db.Permissions
                .FirstOrDefaultAsync(p => p.Name == permission);

            if (permissionEntity is null)
            {
                permissionEntity = new Permissions
                {
                    Name = permission,
                    CreatedAt = DateTime.UtcNow,
                    Description = "Test"
                };

                db.Permissions.Add(permissionEntity);
            }

            await db.SaveChangesAsync();

            var userRoleExists = await db.UserRoles.AnyAsync(ur =>
                ur.UserId == user.ID && ur.RoleId == roleEntity.ID);

            if (!userRoleExists)
            {
                db.UserRoles.Add(new UserRole
                {
                    UserId = user.ID,
                    RoleId = roleEntity.ID
                });
            }

            var rolePermissionExists = await db.RolePermissions.AnyAsync(rp =>
                rp.RoleID == roleEntity.ID &&
                rp.PermissionID == permissionEntity.ID);

            if (!rolePermissionExists)
            {
                db.RolePermissions.Add(new RolePermissions
                {
                    RoleID = roleEntity.ID,
                    PermissionID = permissionEntity.ID
                });
            }

            await db.SaveChangesAsync();

            var client = _keyStoneApiFactory.CreateClient();

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/Login",
                new LoginDTO
                {
                    Username = "jane3@example.com",
                    Password = "CorrectPassword1!"
                });

            loginResponse.EnsureSuccessStatusCode();

            var loginResult =
                await loginResponse.Content.ReadFromJsonAsync<JWTAuthResult>();

            return loginResult!.AccessToken;
        }
    }
}
