using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keycloak.Client.Models
{
    public class KeycloakOptions
    {
        public string BaseUrl { get; set; } = "http://192.168.1.136:28080";
        public string Realm { get; set; } = "master";
        public string AdminId { get; set; }
        public string AdminSecret { get; set; }
    }
}
