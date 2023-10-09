using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MySpace
{
    public delegate T ProcessResponse<T>(string response);

    public class HttpClientService<T> : IHttpClientService<T> where T : class
    {
        public async Task<T> GetAsync(
            string url,
            HttpMethod httpMethod,
            string contentType,
            ProcessResponse<T> ProcessResponse,
            IDictionary<string, string> headers = null,
            string requestBody = null,
            IDictionary<string, string> queryParams = null)
        {
            try
            {
                if (queryParams != null && queryParams.Count > 0)
                {
                    var query = string.Join("&", queryParams.Select(kv => $"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value)}"));
                    url = $"{url}?{query}";
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(url);

                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    headers?.ToList().ForEach(header => httpClient.DefaultRequestHeaders.Add(header.Key, header.Value));

                    var requestMessage = new HttpRequestMessage(httpMethod, url);

                    if (httpMethod != HttpMethod.Get)
                    {
                        requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, contentType);
                    }

                    var response = await httpClient.SendAsync(requestMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        return ProcessResponse(await response.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        throw new HttpRequestException($"HTTP request failed with status code: {response.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
    }
}
