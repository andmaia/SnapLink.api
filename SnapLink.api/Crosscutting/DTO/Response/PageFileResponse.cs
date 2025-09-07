using SnapLink.api.Crosscutting.Enum;

public class PageFileResponse
{
    public string Id { get; set; }
    public string? FileName { get; set; }
    public int Size { get; set; }
    public string ContentType { get; set; }
    public string PageId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TimeToExpire { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public string DownloadUrl { get; set; }
}
