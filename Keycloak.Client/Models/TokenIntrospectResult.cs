using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Keycloak.Models
{
    public class TokenIntrospectResult
    {
        [JsonPropertyName("exp")]
        public int Exp { get; set; }

        [JsonPropertyName("iat")]
        public int Iat { get; set; }

        [JsonPropertyName("jti")]
        public string Jti { get; set; }

        [JsonPropertyName("iss")]
        public string Iss { get; set; }

        [JsonPropertyName("sub")]
        public string Sub { get; set; }

        [JsonPropertyName("typ")]
        public string Typ { get; set; }

        [JsonPropertyName("azp")]
        public string Azp { get; set; }

        [JsonPropertyName("acr")]
        public string Acr { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }

        [JsonPropertyName("clientHost")]
        public string ClientHost { get; set; }

        [JsonPropertyName("clientAddress")]
        public string ClientAddress { get; set; }

        [JsonPropertyName("client_id")]
        public string Client_id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }
    }
}
