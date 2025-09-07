using SnapLink.api.Crosscutting.Enum;

namespace SnapLink.api.Crosscutting.DTO.Request
{
    public class CreatePageFileRequest
    {
        public string? FileName { get; set; }
        public IFormFile Data { get;  set; }
        public string ContentType { get; set; }
        public string PageId { get; set; }
        public bool IsPagePrivate { get; set; }

        public string PageName { get; set; }

        public int TimeToExpire { get; set; }
    }
}
