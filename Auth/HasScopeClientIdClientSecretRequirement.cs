using Microsoft.AspNetCore.Authorization;

namespace LakeStatsApi.Auth
{
    public class HasScopeClientIdClientSecretRequirement : IAuthorizationRequirement
    {
        public HasScopeClientIdClientSecretRequirement(List<string> scope) => Scopes = scope;
        public List<string> Scopes { get; set; }
    }
}
