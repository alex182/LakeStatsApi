using FluentAssertions;
using Keycloak.Client;
using Keycloak.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Keycloak.Test
{
    [TestClass]
    public class KeycloakClientTest
    {
        private string _clientId = "test-client";
        private static IKeycloakClient _client;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var logger = Substitute.For<ILogger<KeycloakClient>>();
            var options = new KeycloakOptions();
            var httpClient = new HttpClient();

            _client = new KeycloakClient(logger, httpClient, options);
        }


        [TestMethod]
        public async Task KeycloakClient_Should_ReturnToken()
        {
            var clientSecret = Environment.GetEnvironmentVariable("KeyCloakTestSecret");
            var token = await _client.GetToken(_clientId, clientSecret);

            token.AccessToken.Should().NotBeNull();
            token.ExpiresIn.Should().Be(60);
            token.RefreshExpiresIn.Should().Be(0);
            token.NotBeforePolicy.Should().Be(0);
            token.TokenType.Should().Be("Bearer");
            token.Scope.Should().Be("test-scope");
        }

        [TestMethod]
        public async Task KeycloakClient_Should_NotReturnToken()
        {
            var clientSecret = Environment.GetEnvironmentVariable("NotKeyCloakTestSecret");
            var token = await _client.GetToken(_clientId, clientSecret);

            token.AccessToken.Should().Be(null);
            token.ExpiresIn.Should().Be(0);
            token.RefreshExpiresIn.Should().Be(0);
            token.NotBeforePolicy.Should().Be(0);
            token.TokenType.Should().Be(null);
            token.Scope.Should().Be(null);
        }
    }
}