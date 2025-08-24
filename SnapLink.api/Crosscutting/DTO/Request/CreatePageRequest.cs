namespace SnapLink.api.Crosscutting.DTO.Request
{
    public class CreatePageRequest
    {
        public string Name { get; set; }
        public string? AccessCode { get; set; }
        public bool IsPrivate { get; set; } 
    }
}
