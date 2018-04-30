using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.WebClient.Services
{
    public class Tenant
    {
        public string DisplayName { get; set; }
        public string Identifier { get; set; }
        public string MatchingHost { get; set; }
    }
}
