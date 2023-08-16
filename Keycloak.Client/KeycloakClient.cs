﻿using Keycloak.Client.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace Keycloak.Client
{
    public class KeycloakClient : IKeycloakClient
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly KeycloakOptions _keycloakOptions;

        public KeycloakClient(ILogger<KeycloakClient> logger, HttpClient httpClient, KeycloakOptions options)
        {
            _logger = logger;
            _httpClient = httpClient;
            _keycloakOptions = options;
        }

        public async Task<Token> GetToken(string clientId, string clientSecret)
        {
            var requestUri = $"{_keycloakOptions.BaseUrl}/auth/realms/{_keycloakOptions.Realm}/protocol/openid-connect/token";
            var token = new Token();

            _logger.LogInformation($"{nameof(KeycloakClient)} {nameof(GetToken)} {clientId} {requestUri}");

            var tokenRequest = new Dictionary<string, string>()
            {
                { "client_id", clientId },
                {"client_secret", clientSecret },
                { "grant_type","client_credentials" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = new FormUrlEncodedContent(tokenRequest) };

            var tokenResponse = await _httpClient.SendAsync(request);

            if (tokenResponse.IsSuccessStatusCode)
            {
                var tokenString = await tokenResponse.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<Token>(tokenString);
            }

            return token;
        }

    }
}