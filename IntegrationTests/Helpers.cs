using MachineStreamCore.Interfaces;
using MachineStreamCore.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public static class FakeInstanceUtils
    {
        public static StreamEvent GenerateRandomStreamEvent(string id = null,
            string machineId = null,
            DateTime? timestamp = null,
            string status = null)
        {
            Random random = new Random();
            var statuses = ModelHelpers.GetStreamEventStatuses();
            return new StreamEvent()
            {
                Event = "new",
                JoinRef = null,
                Payload = new Payload
                {
                    Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id,
                    MachineId = string.IsNullOrWhiteSpace(machineId) ? Guid.NewGuid().ToString() : machineId,
                    Timestamp = timestamp.HasValue
                        ? timestamp.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")
                        : DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"),
                    Status = string.IsNullOrWhiteSpace(machineId)
                        ? statuses.ToArray()[random.Next(statuses.Count())]
                        : status
                },
                Ref = null,
                Topic = "events"
            };
        }

        public static async Task<(string id, string machineId, DateTime timestamp, string status)> SeedDataAsync(IMongo mongo)
        {
            string id = Guid.NewGuid().ToString();
            await mongo.InsertEventAsync(new WebSocketStream(
                    GenerateRandomStreamEvent(id: id), "tests"
                ));

            string machineId = Guid.NewGuid().ToString();
            for (int i = 0; i < 10; i++)
                await mongo.InsertEventAsync(new WebSocketStream(
                        GenerateRandomStreamEvent(machineId: machineId), "tests"
                    ));

            DateTime timestamp = DateTime.UtcNow.AddDays(-5);
            for (int i = 0; i < 10; i++)
                await mongo.InsertEventAsync(new WebSocketStream(
                    GenerateRandomStreamEvent(timestamp: timestamp), "tests"
                ));

            string status = "idle";
            for (int i = 0; i < 10; i++)
                await mongo.InsertEventAsync(new WebSocketStream(
                    GenerateRandomStreamEvent(status: status), "tests"
                ));

            return (id, machineId, timestamp, status);
        }
    }

    public static class LoggerUtilsMoq
    {
        public static Mock<ILogger<T>> LoggerMock<T>() where T : class
        {
            return new Mock<ILogger<T>>();
        }

        /// <summary>
        /// Returns an <pre>ILogger<T></pre> as used by the Microsoft.Logging framework.
        /// You can use this for constructors that require an ILogger parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogger<T> Logger<T>() where T : class
        {
            return LoggerMock<T>().Object;
        }

        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, string message, string failMessage = null)
        {
            loggerMock.VerifyLog(level, message, Times.Once(), failMessage);
        }

        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, string message, Times times, string failMessage = null)
        {
            loggerMock.Verify(l => l.Log<Object>(level, It.IsAny<EventId>(), It.Is<Object>(o => o.ToString() == message), null, It.IsAny<Func<Object, Exception, String>>()), times, failMessage);
        }
    }
}
