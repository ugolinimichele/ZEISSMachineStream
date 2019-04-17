using MachineStreamCore.Configuration;
using MachineStreamCore.Interfaces;
using MachineStreamCore.Models;
using MachineStreamCore.Models.Interfaces;
using MachineStreamCore.Services;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class GetDataServiceTests : IDisposable
    {
        private readonly IGetData getData;
        private readonly IMongo mongo;
        //define the moq mongo here because I need to dispose at the end of the tests
        private readonly MongoDbRunner runner;

        public GetDataServiceTests()
        {
            runner = MongoDbRunner.Start();
            mongo = new MongoService(
                new MongoClient(runner.ConnectionString),
                Options.Create(new MongoDBConfiguration()
                {
                    DatabaseName = "TestMachine",
                    EventCollection = "TestEvents"
                }),
                LoggerUtilsMoq.Logger<MongoService>()
            );
            getData = new GetDataService(mongo, LoggerUtilsMoq.Logger<GetDataService>());
        }

        [Fact]
        public async Task GetEventById_TestAsync()
        {
            string id = Guid.NewGuid().ToString();
            var streamEvent = FakeInstanceUtils.GenerateRandomStreamEvent(id: id);

            await mongo.InsertEventAsync(new WebSocketStream(streamEvent, "test"));

            var singleEvent = await getData.GetEventByIdAsync(id);

            Assert.IsAssignableFrom<IWebSocketStream>(singleEvent);
            Assert.IsAssignableFrom<IStreamEvent>(singleEvent.StreamEvent);
            Assert.IsAssignableFrom<IPayload>(singleEvent.StreamEvent.Payload);

            Assert.Equal(streamEvent.Event, singleEvent.StreamEvent.Event);
            Assert.Null(singleEvent.StreamEvent.JoinRef);
            Assert.Null(singleEvent.StreamEvent.Ref);
            Assert.Equal(streamEvent.Topic, singleEvent.StreamEvent.Topic);

            Assert.Equal(streamEvent.Payload.Timestamp, singleEvent.StreamEvent.Payload.Timestamp);
            Assert.Equal(streamEvent.Payload.MachineId, singleEvent.StreamEvent.Payload.MachineId);
            Assert.Equal(streamEvent.Payload.Status, singleEvent.StreamEvent.Payload.Status);
        }

        [Fact]
        public async Task GetLastEvents_TestAsync()
        {
            var (id, machine_id, timestamp, status) = await FakeInstanceUtils.SeedDataAsync(mongo);

            var lastEvents = await getData.GetLastEventsAsync(20);

            Assert.Equal(20, lastEvents.Count());

            foreach (var singleEvent in lastEvents)
            {
                Assert.IsAssignableFrom<IWebSocketStream>(singleEvent);
                Assert.IsAssignableFrom<IStreamEvent>(singleEvent.StreamEvent);
                Assert.IsAssignableFrom<IPayload>(singleEvent.StreamEvent.Payload);

                Assert.Null(singleEvent.StreamEvent.JoinRef);
                Assert.Null(singleEvent.StreamEvent.Ref);
            }
        }

        [Fact]
        public async Task GetLastEventsByMachineId_TestAsync()
        {
            var (id, machine_id, timestamp, status) = await FakeInstanceUtils.SeedDataAsync(mongo);

            var lastEvents = await getData.GetLastEventsByMachineIdAsync(machine_id);

            Assert.Equal(10, lastEvents.Count());

            foreach (var singleEvent in lastEvents)
            {
                Assert.IsAssignableFrom<IWebSocketStream>(singleEvent);
                Assert.IsAssignableFrom<IStreamEvent>(singleEvent.StreamEvent);
                Assert.IsAssignableFrom<IPayload>(singleEvent.StreamEvent.Payload);

                Assert.Null(singleEvent.StreamEvent.JoinRef);
                Assert.Null(singleEvent.StreamEvent.Ref);

                Assert.Equal(singleEvent.StreamEvent.Payload.MachineId, machine_id);
            }
        }

        [Fact]
        public async Task GetLastEventsByStatus_TestAsync()
        {
            var (id, machine_id, timestamp, status) = await FakeInstanceUtils.SeedDataAsync(mongo);

            var lastEvents = await getData.GetLastEventsByStatusAsync(status);

            foreach (var singleEvent in lastEvents)
            {
                Assert.IsAssignableFrom<IWebSocketStream>(singleEvent);
                Assert.IsAssignableFrom<IStreamEvent>(singleEvent.StreamEvent);
                Assert.IsAssignableFrom<IPayload>(singleEvent.StreamEvent.Payload);

                Assert.Null(singleEvent.StreamEvent.JoinRef);
                Assert.Null(singleEvent.StreamEvent.Ref);

                Assert.Equal(singleEvent.StreamEvent.Payload.Status, status);
            }
        }

        [Fact]
        public async Task GetEventsByFiltersWithEmpty_TestAsync()
        {
            var (id, machine_id, timestamp, status) = await FakeInstanceUtils.SeedDataAsync(mongo);

            IDictionary<string, string> filters = new Dictionary<string, string>();

            var events = await getData.GetEventsByFiltersAsync(filters);
            Assert.Equal(31, events.Count());
            foreach (var singleEvent in events)
            {
                Assert.IsAssignableFrom<IWebSocketStream>(singleEvent);
                Assert.IsAssignableFrom<IStreamEvent>(singleEvent.StreamEvent);
                Assert.IsAssignableFrom<IPayload>(singleEvent.StreamEvent.Payload);

                Assert.Null(singleEvent.StreamEvent.JoinRef);
                Assert.Null(singleEvent.StreamEvent.Ref);
            }
        }

        [Fact]
        public async Task GetEventsByFiltersWithMachineId_TestAsync()
        {
            var (id, machine_id, timestamp, status) = await FakeInstanceUtils.SeedDataAsync(mongo);

            IDictionary<string, string> filters = new Dictionary<string, string>
            {
                { "machine_id", machine_id },
                { "limit", "50" }
            };

            var events = await getData.GetEventsByFiltersAsync(filters);
            Assert.InRange(events.Count(), 0, 50);
            foreach (var singleEvent in events)
            {
                Assert.IsAssignableFrom<IWebSocketStream>(singleEvent);
                Assert.IsAssignableFrom<IStreamEvent>(singleEvent.StreamEvent);
                Assert.IsAssignableFrom<IPayload>(singleEvent.StreamEvent.Payload);

                Assert.Null(singleEvent.StreamEvent.JoinRef);
                Assert.Null(singleEvent.StreamEvent.Ref);

                Assert.Equal(singleEvent.StreamEvent.Payload.MachineId, machine_id);
            }
        }

        [Fact]
        public async Task GetEventsByFiltersWithStatus_TestAsync()
        {
            var (id, machine_id, timestamp, status) = await FakeInstanceUtils.SeedDataAsync(mongo);

            IDictionary<string, string> filters = new Dictionary<string, string>
            {
                { "status", $"{status},finished" },
                { "limit", "50" }
            };

            var events = await getData.GetEventsByFiltersAsync(filters);

            foreach (var singleEvent in events)
            {
                Assert.IsAssignableFrom<IWebSocketStream>(singleEvent);
                Assert.IsAssignableFrom<IStreamEvent>(singleEvent.StreamEvent);
                Assert.IsAssignableFrom<IPayload>(singleEvent.StreamEvent.Payload);

                Assert.Null(singleEvent.StreamEvent.JoinRef);
                Assert.Null(singleEvent.StreamEvent.Ref);

                Assert.Contains(singleEvent.StreamEvent.Payload.Status, new string[] { status, "finished" });
            }
        }

        [Fact]
        public async Task GetEventsByFiltersWithFromTo_TestAsync()
        {
            var (id, machine_id, timestamp, status) = await FakeInstanceUtils.SeedDataAsync(mongo);

            IDictionary<string, string> filters = new Dictionary<string, string>
            {
                { "from", timestamp.AddMinutes(-1).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK") },
                { "to", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK") },
                { "limit", "20" }
            };

            var events = await getData.GetEventsByFiltersAsync(filters);
            Assert.InRange(events.Count(), 10, 20);
            foreach (var singleEvent in events)
            {
                Assert.IsAssignableFrom<IWebSocketStream>(singleEvent);
                Assert.IsAssignableFrom<IStreamEvent>(singleEvent.StreamEvent);
                Assert.IsAssignableFrom<IPayload>(singleEvent.StreamEvent.Payload);

                Assert.Null(singleEvent.StreamEvent.JoinRef);
                Assert.Null(singleEvent.StreamEvent.Ref);
            }
        }

        public void Dispose()
        {
            runner?.Dispose();
        }
    }
}
