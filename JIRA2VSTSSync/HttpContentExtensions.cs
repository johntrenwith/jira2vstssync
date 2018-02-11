using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
 
namespace JIRA2VSTSSync
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            string json = await content.ReadAsStringAsync();
            T value = JsonConvert.DeserializeObject<T>(json);
            return value;
        }
    }
}