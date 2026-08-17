using KeyStone_Identity.Core.DTOs;
using KeyStone_Identity.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace KeyStone_Identity.IntegrationTests.Auth
{
    public class UserRegistrationTests : IClassFixture<KeyStoneApiFactory>
    {
        private readonly KeyStoneApiFactory _factory;
        public UserRegistrationTests(KeyStoneApiFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccess()
        {
            //ARRANGE
            using var scope = _factory.Services.CreateScope();
            var client = _factory.CreateClient();

            //ACT
            var response = await client.PostAsJsonAsync("/api/Auth/Register", new UserRegistrationDTO()
            {
                UserName = "janet",
                FirstName = "Jane",
                MiddleName = "Doe",
                LastName = "Test",
                EmailAddress = "jane@example.com",
                Password = "password123",
                DateOfBirth = new DateOnly(2007, 9, 5)
            });
            //ASSET
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
