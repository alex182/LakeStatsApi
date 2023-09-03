using Microsoft.AspNetCore.Authorization;

namespace LakeStatsApi.Auth
{
    public class HasScopeJwtRequirement : IAuthorizationRequirement
    {
        public HasScopeJwtRequirement(List<string> scope) => Scopes = scope;
        public List<string> Scopes { get; set; }
    }
}
