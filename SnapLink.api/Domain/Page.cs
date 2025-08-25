using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace SnapLink.Api.Domain
{
    public class Page
    {
        public string Id { get; set; }

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

            var hasher = new PasswordHasher<Page>();
            AccessCode = hasher.HashPassword(this, AccessCode);
        }

        public bool VerifyPassword(string inputPassword)
        {
            var hasher = new PasswordHasher<Page>();
            var result = hasher.VerifyHashedPassword(this, AccessCode, inputPassword);
            return result == PasswordVerificationResult.Success;
        }

    }
}
