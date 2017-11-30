using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prosper.Contracts;

namespace ProsperLibrary
{
    public class Client : IClient
    {
        public async Task<T> Post<T>(string uri, Dictionary<string, string> headers, string body, JsonSerializerSettings jsonSerializerSettings)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Content = new StringContent(body);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (HttpContent content = response.Content)
                        {
                            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), jsonSerializerSettings);
                        }
                    }
                }

            }
            return JsonConvert.DeserializeObject<T>(string.Empty);
        }

        public async Task<T> Get<T>(string uri, Dictionary<string, string> headers, JsonSerializerSettings jsonSerializerSettings)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (HttpContent content = response.Content)
                        {
                            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), jsonSerializerSettings);
                        }
                    }
                }

            }
            return JsonConvert.DeserializeObject<T>(string.Empty);
        }
    }
}
