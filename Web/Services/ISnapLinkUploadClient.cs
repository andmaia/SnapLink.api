namespace Web.Services
{
    public interface ISnapLinkUploadClient
    {
        Task<HttpResponseMessage> PostMultipartAsync(string url, MultipartFormDataContent content, string? token = null);

    }
}
