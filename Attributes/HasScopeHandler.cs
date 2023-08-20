using Keycloak.Client;
using Keycloak.Client.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.WebSockets;

namespace LakeStatsApi.Attributes
{
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        private readonly ILogger _logger;
        private readonly IKeycloakClient _keycloakClient;
        private IHttpContextAccessor _httpContextAccessor = null;

        public HasScopeHandler(ILogger<HasScopeHandler> logger, IKeycloakClient keycloakClient, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _keycloakClient= keycloakClient;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            var token = await GetToken();

            if (token?.AccessToken == null)
            {
                context.Fail();
                return;
            }

            if (token?.AccessToken != null)
            {
                var tokenScopes = token.Scope.Split(" ");

                if (requirement.Scopes.Any(x => tokenScopes.Contains(x)))
                {
                    context.Succeed(requirement);
                    return;
                }
                else
                {
                    context.Fail();
                    return;
                }
            }
            context.Fail();
            return;
        }

        internal async Task<Token> GetToken()
        {
            var token = new Token(); 
            var httpContext = _httpContextAccessor.HttpContext;
            var query = httpContext.Request.Query;

            string clientSecret= query["PASSWORD"];
            string clientId =  query["ID"];

            token = await _keycloakClient.GetToken(clientId, clientSecret);

            return token; 
        }

    }
}
