using System.Collections.Generic;

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
