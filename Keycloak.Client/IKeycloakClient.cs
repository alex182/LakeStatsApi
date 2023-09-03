using Keycloak.Client.Models;
using Keycloak.Models;

namespace Keycloak.Client
{
    public interface IKeycloakClient
    {
        Task<Token> GetToken(string clientId, string clientSecret);
        Task<TokenIntrospectResult> ValidateJwt(string jwt);
    }
}