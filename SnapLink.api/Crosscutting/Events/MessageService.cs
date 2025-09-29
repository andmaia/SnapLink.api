
using SnapLink.api.Crosscutting.Events;

namespace SnapLink.Api.Crosscutting.Events
{
    public class MessageService
    {
        private static readonly List<Message> _messages = new();

        public static IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

        public static bool HasMessage()
        {
            return _messages.Any();
        }

        public static void AddMessage(string description)
        {
            var message = new Message(Guid.NewGuid(), description);
            _messages.Add(message);
        }

        public static IEnumerable<string> GetAllDescriptions()
        {
            return _messages.Select(m => m.Description);
        }

        public static void ClearMessages()
        {
            _messages.Clear();
        }
    }
}
