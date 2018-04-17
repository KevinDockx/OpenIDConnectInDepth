using System.Net.Http;
using System.Threading.Tasks;

namespace Sample.WebClient.Services
{
    public interface ISampleHttpClient
    {
        Task<HttpClient> GetClient();
    }
}
 