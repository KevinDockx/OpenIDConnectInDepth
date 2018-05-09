using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace Sample.WebClient.Services
{
    public class TenantSelector : ITenantSelector<Tenant>
    {
        public Tenant Current { get; private set; }

        private ICollection<Tenant> _tenants = new List<Tenant>();
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantSelector(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            // create the tenants
            _tenants.Add(new Tenant()
            {
                DisplayName = "The White House",
                Identifier = "whitehouse",
                MatchingHost = "localhost:44318"
            });

            _tenants.Add(new Tenant()
            {
                DisplayName = "Belgian State Department",
                Identifier = "belgianstatedepartment",
                MatchingHost = "localhost:44319"
            });
        }

        public Tenant Select()
        {
            var tenant = _tenants.FirstOrDefault(t =>
               t.MatchingHost.ToLowerInvariant() == 
               (_httpContextAccessor.HttpContext.Request.Host.Value.ToLower()));

            if (tenant != null)
            {
                // store for faster access if needed
                Current = tenant;
            }

            return tenant;
        }
    }
}
