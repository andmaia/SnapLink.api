namespace Web.Services
{
    public interface ISnapLinkApiClient
    {
        Task<HttpResponseMessage> GetAsync(string url, string? token = null);
        Task<HttpResponseMessage> PostAsync(string url, HttpContent content, string? token = null);
        Task<HttpResponseMessage> DeleteAsync(string url, string? token = null);
    }
}
