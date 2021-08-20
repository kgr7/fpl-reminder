using FPL.Reminder.src;
using FPL.Reminder.src.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Globalization;
using Xunit;

namespace FPL.Reminder.Tests
{
    public class WorkerTests
    {
        private readonly Worker _sut;
        private readonly Mock<IWebService> _webServiceMock = new Mock<IWebService>();
        private readonly Mock<IDateTimeProvider> _mockDateTimeProvider = new Mock<IDateTimeProvider>();
        private readonly Mock<IConfiguration> _mockConfig = new Mock<IConfiguration>();
        public WorkerTests()
        {
            _sut = new Worker(_webServiceMock.Object, _mockDateTimeProvider.Object, _mockConfig.Object);
        }
        
        [Theory]
        [InlineData("2021-08-13T17:30:00Z", "2021-08-13T17:30:00Z")]
        [InlineData("2021-08-15T10:01:00Z", "2021-08-15T10:00:00Z")]
        [InlineData("2021-08-13T11:54:00Z", "2021-08-13T11:45:00Z")]
        [InlineData("2021-07-08T09:00:01Z", "2021-07-08T09:00:00Z")]
        public void ShouldRoundDateTimeToNearest15Mins(string dt, string ex)
        {
            // Arrange
            var datetime = DateTime.ParseExact(
                dt,
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.CurrentCulture).ToUniversalTime();

            var expected = DateTime.ParseExact(
                ex,
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.CurrentCulture).ToUniversalTime();

            // Act
            var result = _sut.ToNearest15Mins(datetime);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("2021-08-13T17:32:00Z", "2021-08-14T17:30:00Z", 24)]
        [InlineData("2021-08-15T10:16:00Z", "2021-08-15T22:15:00Z", 12)]
        [InlineData("2021-08-13T11:30:03Z", "2021-08-13T13:30:00Z", 2)]
        [InlineData("2021-07-08T09:01:51Z", "2021-07-08T11:00:00Z", 2)]
        [InlineData("2021-08-20T22:00:00Z", "2021-08-21T10:00:00Z", 12)]
        [InlineData("2021-08-20T10:00:00Z", "2021-08-21T10:00:00Z", 24)]
        [InlineData("2021-08-21T08:00:00Z", "2021-08-21T10:00:00Z", 2)]
        public void ShouldSendAppropriateMessage(string currentTime, string deadlineTime, int hrsBefore)
        {
            // Arrange
            var currentDateTime = DateTime.ParseExact(
                currentTime,
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.CurrentCulture).ToUniversalTime();

            _mockDateTimeProvider.Setup(x => x.UtcNow).Returns(currentDateTime);

            var deadlineDateTime = DateTime.ParseExact(
                deadlineTime,
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.CurrentCulture).ToUniversalTime();

            var e = new Event { Id = 7, DeadlineTime = deadlineDateTime, IsNext = true };

            // Act
            var result = _sut.DoWork(e);
            // Assert
            _webServiceMock.Verify(x => x.SendReminder(hrsBefore, 7), Times.Once());
        }
    }
}
