using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Sample.SecondAPI.Controllers
{
    [Authorize]
    [Route("api/claims")]
    public class ClaimsController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var claimsList = new List<string>();

            // return the user claims
            foreach (var claim in User.Claims)
            {
                claimsList.Add($"{claim.Type} : {claim.Value}");
            }
            return claimsList;
        }
    }
}
