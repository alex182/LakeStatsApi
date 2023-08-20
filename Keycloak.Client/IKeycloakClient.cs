using Keycloak.Client.Models;

namespace Keycloak.Client
{
    public interface IKeycloakClient
    {
        Task<Token> GetToken(string clientId, string clientSecret);
    }
}