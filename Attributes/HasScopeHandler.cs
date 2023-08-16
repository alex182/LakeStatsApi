using Keycloak.Client;
using Keycloak.Client.Models;
using Microsoft.AspNetCore.Authorization;

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
            }

            if (token?.AccessToken != null)
            {
                if (token.Scope.ToLower() == requirement.Scope.ToLower())
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
        }

        internal async Task<Token> GetToken()
        {
            var token = new Token(); 
            var httpContext = _httpContextAccessor.HttpContext;
            var routeData = httpContext.GetRouteData();

            var clientSecret = routeData?.Values["PASSWORD"]?.ToString();
            var clientId = routeData?.Values["ID"]?.ToString();

            token = await _keycloakClient.GetToken(clientId, clientSecret);

            return token; 
        }

    }
}
