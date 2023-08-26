using Microsoft.AspNetCore.Authorization;

namespace LakeStatsApi.Auth
{
    public class HasScopeAWRequirement : IAuthorizationRequirement
    {
        public HasScopeAWRequirement(List<string> scope) => Scopes = scope;
        public List<string> Scopes { get; set; }
    }
}
