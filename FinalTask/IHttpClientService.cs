namespace MySpace
{
    public interface IHttpClientService<T>
    {

        Task<T> GetAsync(string url,
            HttpMethod httpMethod,
            string contentType,
            ProcessResponse<T> ProcessResponse,
            IDictionary<string, string> headers = null,
            string requestBody = null,
            IDictionary<string, string> queryParams = null);
    }
}
