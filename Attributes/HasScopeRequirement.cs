using Microsoft.AspNetCore.Authorization;

namespace LakeStatsApi.Attributes
{
    public class HasScopeRequirement : IAuthorizationRequirement
    {
        public HasScopeRequirement(List<string> scope) => Scopes = scope;
        public List<string> Scopes { get; set; }
    }
}
