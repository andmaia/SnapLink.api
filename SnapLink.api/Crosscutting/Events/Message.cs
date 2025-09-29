namespace SnapLink.api.Crosscutting.Events
{
    public class Message
    {
        public Guid Id { get; private set; }
        public string Description { get; private set; }
        public Message(Guid id, string description)
        {
            Id = id;
            Description = description;
        }
    }
}
