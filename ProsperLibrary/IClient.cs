using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProsperLibrary
{
    public interface IClient
    {
        Task<T> Get<T>(string uri, Dictionary<string, string> headers, JsonSerializerSettings jsonSerializerSettings);
        Task<T> Post<T>(string uri, Dictionary<string, string> headers, string body, JsonSerializerSettings jsonSerializerSettings);
    }
}