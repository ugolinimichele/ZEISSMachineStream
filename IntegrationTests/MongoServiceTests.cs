using MachineStreamCore.Configuration;
using MachineStreamCore.Interfaces;
using MachineStreamCore.Models;
using MachineStreamCore.Models.Interfaces;
using MachineStreamCore.Services;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class MongoServiceTests : IDisposable
    {
        private readonly IMongo mongo;
        //define the moq mongo here because I need to dispose at the end of the tests
        private readonly MongoDbRunner runner;

        public MongoServiceTests()
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
        }

        [Fact]
        public async Task SingleInsertAndFind_TestAsync()
        {
            var streamEvent = FakeInstanceUtils.GenerateRandomStreamEvent();

            await mongo.InsertEventAsync(new WebSocketStream(streamEvent, "tests"));

            var events = await mongo.FindEventsByFiltersAsync(Builders<WebSocketStream>.Filter.Eq(x => x.StreamEvent.Payload.Id, streamEvent.Payload.Id));

            Assert.Single(events);
            var singleEvent = events.First();
            Assert.IsType<WebSocketStream>(singleEvent);
            Assert.IsType<StreamEvent>(singleEvent.StreamEvent);
            Assert.IsType<Payload>(singleEvent.StreamEvent.Payload);

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
        public async Task MultipleInsertAndFindByMachineId_TestAsync()
        {
            string machineId = Guid.NewGuid().ToString();
            for (int i = 0; i < 10; i++)
                await mongo.InsertEventAsync(new WebSocketStream(
                    FakeInstanceUtils.GenerateRandomStreamEvent(machineId: machineId), "tests"
                ));
            

            var events = await mongo.FindEventsByFiltersAsync(
                        Builders<WebSocketStream>.Filter.Eq(x => x.StreamEvent.Payload.MachineId, machineId),
                        Builders<WebSocketStream>.Sort.Descending(x => x.StreamEvent.Payload.Timestamp),
                        50
                    );

            Assert.Equal(10, events.Count);
            foreach (var singleEvent in events)
            {
                Assert.IsType<WebSocketStream>(singleEvent);
                Assert.IsType<StreamEvent>(singleEvent.StreamEvent);
                Assert.IsType<Payload>(singleEvent.StreamEvent.Payload);

                Assert.IsAssignableFrom<IStreamEvent>(singleEvent.StreamEvent);
                Assert.IsAssignableFrom<IPayload>(singleEvent.StreamEvent.Payload);

                Assert.Equal(machineId, singleEvent.StreamEvent.Payload.MachineId);
            }
        }

        public void Dispose()
        {
            runner?.Dispose();
        }
    }
}
