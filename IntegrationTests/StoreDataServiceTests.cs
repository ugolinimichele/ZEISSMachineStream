using MachineStreamCore.Configuration;
using MachineStreamCore.Interfaces;
using MachineStreamCore.Services;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class StoreDataServiceTests : IDisposable
    {
        private readonly IStoreData storeData;
        //define the moq mongo here because I need to dispose at the end of the tests
        private readonly MongoDbRunner runner;

        public StoreDataServiceTests()
        {
            runner = MongoDbRunner.Start();
            var mongoService = new MongoService(
                new MongoClient(runner.ConnectionString),
                Options.Create(new MongoDBConfiguration()
                {
                    DatabaseName = "TestMachine",
                    EventCollection = "TestEvents"
                }),
                LoggerUtilsMoq.Logger<MongoService>()
            );
            storeData = new StoreDataService(mongoService, LoggerUtilsMoq.Logger<StoreDataService>());
        }

        [Fact]
        public async Task ValidStore_TestAsync()
        {
            await storeData.StoreEventAsync(FakeInstanceUtils.GenerateRandomStreamEvent(), "tests");
            Assert.True(true);
        }

        public void Dispose()
        {
            runner?.Dispose();
        }
    }
}
