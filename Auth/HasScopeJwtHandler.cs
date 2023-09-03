using Keycloak.Client;
using Keycloak.Client.Models;
using Keycloak.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.WebSockets;

namespace LakeStatsApi.Auth
{
    public class HasScopeJwtHandler : AuthorizationHandler<HasScopeJwtRequirement>
    {
        private readonly ILogger _logger;
        private readonly IKeycloakClient _keycloakClient;
        private IHttpContextAccessor _httpContextAccessor = null;

        public HasScopeJwtHandler(ILogger<HasScopeJwtHandler> logger, IKeycloakClient keycloakClient, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _keycloakClient= keycloakClient;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeJwtRequirement requirement)
        {
            var validationResult = await GetToken();

            if (!validationResult.Active)
            {
                context.Fail();
                return;
            }

            if (!string.IsNullOrEmpty(validationResult.Scope))
            {
                var tokenScopes = validationResult.Scope.Split(" ");

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

        internal async Task<TokenIntrospectResult> GetToken()
        {
            var result = new TokenIntrospectResult(); 
            var httpContext = _httpContextAccessor.HttpContext;
            var headers = httpContext.Request.Headers;

            string jwt= headers["Authorization"];

            if (!string.IsNullOrEmpty(jwt))
            {
                jwt = jwt.Replace("Bearer ", "");
            }

            result = await _keycloakClient.ValidateJwt(jwt);

            return result; 
        }

    }
}
