using System.Net.Http.Headers;

namespace Web.Services
{
    public class SnapLinkApiClient : ISnapLinkApiClient, ISnapLinkUploadClient
        {
            private readonly HttpClient _httpClient;

            public SnapLinkApiClient(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            private HttpRequestMessage BuildRequest(HttpMethod method, string url, HttpContent? content, string? token)
            {
                var request = new HttpRequestMessage(method, url);

                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                if (content is not null)
                    request.Content = content;

                return request;
            }

            public Task<HttpResponseMessage> GetAsync(string url, string? token = null)
                => _httpClient.SendAsync(BuildRequest(HttpMethod.Get, url, null, token));

            public Task<HttpResponseMessage> PostAsync(string url, HttpContent content, string? token = null)
                => _httpClient.SendAsync(BuildRequest(HttpMethod.Post, url, content, token));

            public Task<HttpResponseMessage> DeleteAsync(string url, string? token = null)
                => _httpClient.SendAsync(BuildRequest(HttpMethod.Delete, url, null, token));

            public Task<HttpResponseMessage> PostMultipartAsync(string url, MultipartFormDataContent content, string? token = null)
                => _httpClient.SendAsync(BuildRequest(HttpMethod.Post, url, content, token));
        
    }
}
