using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnapLink.api.Crosscutting.Enum;
using SnapLink.api.Domain;

namespace SnapLink.tests.Services
{
    public class PageFileTest
    {

        [Theory(DisplayName = "Should set ExpiresAt correctly based on TimeToExpire")]
        [InlineData(TimeToExpire.MIN1, 1)]
        [InlineData(TimeToExpire.MIN5, 5)]
        [InlineData(TimeToExpire.MIN10, 10)]
        [InlineData(TimeToExpire.MIN15, 15)]
        [InlineData(TimeToExpire.MIN20, 20)]
        [InlineData(TimeToExpire.MIN30, 30)]
        [InlineData(TimeToExpire.MIN60, 60)]
        public void CreatePageFile_ShouldCalculateExpiresAt_WhenTimeToExpireIsSet(TimeToExpire timeToExpire, int expectedMinutes)
        {
            var createdAt = DateTime.UtcNow;
            var pageFile = new PageFile(string.Empty, string.Empty, string.Empty, timeToExpire)
            {
                Id = Guid.NewGuid().ToString(),
                FileName = "testfile.txt",
                ContentType = "text/plain",
                CreatedAt = createdAt,
                IsActive = true
            };


            pageFile.CalculateExpiresAt();


            Assert.Equal(createdAt.AddMinutes(expectedMinutes), pageFile.ExpiresAt);
        }
        [Fact(DisplayName = "Should set ExpiresAt correctly when time to expire be undefined")]
        public void CreatePageFile_ShoulSetExpireNUll_WhenTimeToExpireIsUndefinnd()
        {
            var createdAt = DateTime.UtcNow;
            var pageFile = new PageFile(string.Empty, string.Empty, string.Empty, TimeToExpire.UNDEFINED)
            {
                Id = Guid.NewGuid().ToString(),
                FileName = "testfile.txt",
                ContentType = "text/plain",
                CreatedAt = createdAt,
                IsActive = true
            };

            pageFile.CalculateExpiresAt();


            Assert.Null(pageFile.ExpiresAt);
        }

        [Theory(DisplayName = "Should disable and clear data when ExpiresAt has passed")]
        [InlineData(TimeToExpire.MIN1, 1)]
        [InlineData(TimeToExpire.MIN5, 5)]
        [InlineData(TimeToExpire.MIN10, 10)]
        [InlineData(TimeToExpire.MIN15, 15)]
        [InlineData(TimeToExpire.MIN20, 20)]
        [InlineData(TimeToExpire.MIN30, 30)]
        [InlineData(TimeToExpire.MIN60, 60)]
        public void DisablePageFile_ShouldRemoveData_WhenTimeToExpireHasPassed(TimeToExpire timeToExpire, int expectedMinutes)
        {
            var createdAt = DateTime.UtcNow.AddMinutes(-(expectedMinutes + 1));
            var pageFile = new PageFile(string.Empty, string.Empty, string.Empty, timeToExpire)
            {
                Id = Guid.NewGuid().ToString(),
                FileName = "testfile.txt",
                ContentType = "text/plain",
                CreatedAt = createdAt,
                IsActive = true
            };

            pageFile.Disable();

            Assert.False(pageFile.IsActive);     
            Assert.Null(pageFile.Data);          
            Assert.NotNull(pageFile.FinishedAT); 
        }

        [Theory(DisplayName = "VerifyIfExpire deve retornar o valor correto baseado em ExpiresAt")]
        [InlineData(-1, true)]  
        [InlineData(0, true)]   // Expira exatamente agora -> considerado expirado
        [InlineData(1, false)]  // Ainda não expirou
        public void VerifyIfExpire_ShouldReturnExpectedResult(int minutesOffset, bool expectedResult)
        {
            var pageFile = new PageFile("id", "testfile.txt", "text/plain", TimeToExpire.MIN1)
            {
                Id = Guid.NewGuid().ToString(),
                FileName = "testfile.txt",
                ContentType = "text/plain",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(minutesOffset)
            };

            var result = pageFile.VerifyIfExpire();

            Assert.Equal(expectedResult, result);
        }
        [Fact(DisplayName = "AddFile should set Data and Size correctly")]
        public void AddFile_ShouldSetDataAndSize()
        {
            var pageFile = new PageFile("testfile.txt", "text/plain", "page1", TimeToExpire.UNDEFINED);
            var data = Encoding.UTF8.GetBytes("Hello World"); 

            pageFile.AddFile(data);


            Assert.Equal(data, pageFile.Data);
            Assert.Equal(data.Length, pageFile.Size);
        }
    }
}
