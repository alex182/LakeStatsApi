using Microsoft.AspNetCore.Authorization;

namespace LakeStatsApi.Attributes
{
    public class HasScopeRequirement : IAuthorizationRequirement
    {
        public HasScopeRequirement(string scope) => Scope = scope;
        public string Scope { get; set; }
    }
}
