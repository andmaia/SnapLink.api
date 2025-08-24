using System.Security.Cryptography;
using System.Text;

namespace SnapLink.Api.Domain
{
    public class Page
    {
        public Guid Id { get; set; }

        public string Name { get; set; } 

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

        public bool IsActive { get; set; }

        public bool IsPrivate { get; set; }

        public string? AccessCode { get; set; }

        public void Deactivate()
        {
            IsActive = false;
            FinishedAt = DateTime.UtcNow;
        }

        public void HashPassword()
        {
            if (string.IsNullOrEmpty(AccessCode))
                return;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(AccessCode);
            var hash = sha256.ComputeHash(bytes);
            AccessCode = Convert.ToBase64String(hash);
        }
    }
}
