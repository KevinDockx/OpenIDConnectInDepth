using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.WebClient.ViewModels
{
    public class CallApiViewModel
    {
        public IEnumerable<string> ClaimsFromApi { get; set; } = new List<string>();

        public CallApiViewModel()
        {

        }

        public CallApiViewModel(IEnumerable<string> claimsFromApi)
        {
            ClaimsFromApi = claimsFromApi;
        }
    }
}
