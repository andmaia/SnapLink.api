namespace SnapLink.api.Application.Services
{
    public interface ITokenService
    {
        string GeneratePageToken(string pageId);
    }
}
