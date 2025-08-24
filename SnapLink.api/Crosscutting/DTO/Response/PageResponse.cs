namespace SnapLink.api.Crosscutting.DTO.Response
{
    public class PageResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        public bool IsPrivate { get; set; }
    }
}
