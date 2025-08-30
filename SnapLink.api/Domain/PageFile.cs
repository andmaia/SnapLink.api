using SnapLink.api.Crosscutting.Enum;
using SnapLink.Api.Domain;

namespace SnapLink.api.Domain
{
    public class PageFile
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public Byte[] Data { get; private set; }
        public int Size { get;private set; }
        public string ContentType { get; set; }
        public string PageId { get; set; }
        public Page Page { get; set; }
        public DateTime CreatedAt { get; set; }
        public TimeToExpire TimeToExpire { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime FinishedAT { get; set; }

        public PageFile(string filename, string contentType, string pageId, TimeToExpire timeToExpire)
        {
            Id = Guid.NewGuid().ToString();
            FileName = filename;
            ContentType = contentType;
            PageId = pageId;
            CreatedAt = DateTime.UtcNow;
            TimeToExpire = timeToExpire;
            IsActive = true;
            CalculateExpiresAt();
        }

        public void AddFile(byte[] data)
        {
          
            Data = data;
            Size = data.Length;
        }


        public void CalculateExpiresAt()
        {
            if (TimeToExpire == TimeToExpire.UNDEFINED)
            {
                ExpiresAt = null;
                return;
            }
            ExpiresAt = CreatedAt.AddMinutes((int)TimeToExpire);
        }


        public bool VerifyIfExpire()
        {
            return DateTime.UtcNow > ExpiresAt;
        }

        public void Disable()
        {
            
            IsActive = false;
            Data = null;
            FinishedAT = DateTime.UtcNow;
        }


    }
}
