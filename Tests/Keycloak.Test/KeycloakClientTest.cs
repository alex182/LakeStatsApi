using FluentAssertions;
using Keycloak.Client;
using Keycloak.Client.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using System.Net.Http;

namespace Keycloak.Test
{
    [TestClass]
    public class KeycloakClientTest
    {
        private static string _testClientId = "test-client";
        private static IKeycloakClient _keycloakClient;
        private static Token _adminToken;
        private static Token _validTestToken;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            var logger = Substitute.For<ILogger<KeycloakClient>>();
            var httpClient = new HttpClient();

            var adminClientId = Environment.GetEnvironmentVariable("Keycloak-Admin-Id");
            var adminClientSecret = Environment.GetEnvironmentVariable("Keycloak-Admin-Secret");

            if (string.IsNullOrEmpty(adminClientId))
                throw new ArgumentNullException(adminClientId);

            if (string.IsNullOrEmpty(adminClientSecret))
                throw new ArgumentNullException(adminClientSecret);

            var testClientSecret = Environment.GetEnvironmentVariable("KeyCloakTestSecret");

            if (string.IsNullOrEmpty(testClientSecret))
                throw new ArgumentNullException(testClientSecret);


            var options = new KeycloakOptions() 
            { 
                AdminId= adminClientId,
                AdminSecret= adminClientSecret
            };

            _adminToken = await GetTestJwt(httpClient, adminClientId, adminClientSecret);
            _validTestToken = await GetTestJwt(httpClient, _testClientId, testClientSecret);

            _keycloakClient = new KeycloakClient(logger, httpClient, options);
        }

        [Ignore]
        private  static async Task<Token> GetTestJwt(HttpClient httpClient, string clientId, string clientSecret)
        {
            var requestUri = $"http://192.168.1.136:28080/auth/realms/master/protocol/openid-connect/token";
            var token = new Token();

            var tokenRequest = new Dictionary<string, string>()
            {
                { "client_id", clientId },
                {"client_secret", clientSecret },
                { "grant_type","client_credentials" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = new FormUrlEncodedContent(tokenRequest) };

            var tokenResponse = await httpClient.SendAsync(request);

            if (tokenResponse.IsSuccessStatusCode)
            {
                var tokenString = await tokenResponse.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<Token>(tokenString);
            }

            return token;
        }

        [TestMethod]
        public async Task KeycloakClient_Should_ReturnToken()
        {
            var clientSecret = Environment.GetEnvironmentVariable("KeyCloakTestSecret");
            var token = await _keycloakClient.GetToken(_testClientId, clientSecret);

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
            var token = await _keycloakClient.GetToken(_testClientId, clientSecret);

            token.AccessToken.Should().Be(null);
            token.ExpiresIn.Should().Be(0);
            token.RefreshExpiresIn.Should().Be(0);
            token.NotBeforePolicy.Should().Be(0);
            token.TokenType.Should().Be(null);
            token.Scope.Should().Be(null);
        }

        [TestMethod]
        public async Task ValidateJwt_ShouldBeActive()
        {
            var result = await _keycloakClient.ValidateJwt(_adminToken.AccessToken);

            result.Active.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateJwt_ShouldBeInactive()
        {
            var invalidToken = _adminToken.AccessToken.Remove(0, 1);

            var result = await _keycloakClient.ValidateJwt(invalidToken);

            result.Active.Should().BeFalse();
        }
    }
}