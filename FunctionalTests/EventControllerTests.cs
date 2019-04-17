using IntegrationTests;
using MachineStreamCore.Interfaces;
using MachineStreamCore.Models;
using MachineStreamCore.Models.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using ZEISSMachineStream;
using ZEISSMachineStream.Models;

namespace FunctionalTests
{
    public class EventControllerTests : IDisposable, IClassFixture<MachineStreamWebApplicationFactory<Startup>>
    {
        //define the moq mongo here because I need to dispose at the end of the tests
        private readonly MongoDbRunner runner;
        private readonly HttpClient client;
        private readonly MachineStreamWebApplicationFactory<Startup> factory;

        private const string baseEventControllerPath = "v1/Events/";

        public EventControllerTests(MachineStreamWebApplicationFactory<Startup> factory)
        {
            runner = MongoDbRunner.Start();
            
            this.factory = factory;
            client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(typeof(IMongoClient), new MongoClient(runner.ConnectionString));
                });
            }).CreateClient();
        }

        [Fact]
        public async Task GetFilterSpecs_OkResultAsync()
        {
            // The endpoint or route of the controller action.
            var httpResponse = await client.GetAsync($"{baseEventControllerPath}GetFilterSpecs");

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var filterSpecifications = JsonConvert.DeserializeObject<IEnumerable<FilterSpecifications>>(stringResponse);

            Assert.IsAssignableFrom<IEnumerable<FilterSpecifications>>(filterSpecifications);
            Assert.IsAssignableFrom<IEnumerable<IFilterSpecifications>>(filterSpecifications);

            foreach (var filterSpecification in filterSpecifications)
            {
                Assert.IsType<FilterSpecifications>(filterSpecification);
                Assert.IsAssignableFrom<IFilterSpecifications>(filterSpecification);

                Assert.IsType<string>(filterSpecification.Name);
                Assert.IsType<string>(filterSpecification.Type);
                Assert.IsType<string>(filterSpecification.Description);
            }
        }

        private (HttpClient testClient, string Id, string machineId, DateTime timestamp, string status) CreateClientAndSeedData()
        {
            string eventId = "";
            string machineId = "";
            DateTime timestamp = DateTime.UtcNow;
            string status = "";

            var testClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(typeof(IMongoClient), new MongoClient(runner.ConnectionString));

                    var serviceProvider = services.BuildServiceProvider();

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var mongo = scope.ServiceProvider.GetRequiredService<IMongo>();

                        var seedData = FakeInstanceUtils.SeedDataAsync(mongo).GetAwaiter().GetResult();
                        eventId = seedData.id;
                        machineId = seedData.machineId;
                        timestamp = seedData.timestamp;
                        status = seedData.status;
                    }
                });
            }).CreateClient();

            return (testClient, eventId, machineId, timestamp, status);
        }

        [Fact]
        public async Task GetById_OkResultAsync()
        {
            var clientAndSeed = CreateClientAndSeedData();

            // The endpoint or route of the controller action.
            var httpResponse = await clientAndSeed.testClient.GetAsync(
                $"{baseEventControllerPath}GetById/{clientAndSeed.Id}"
            );

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var streamEvent = JsonConvert.DeserializeObject<StreamEvent>(stringResponse);

            Assert.IsType<StreamEvent>(streamEvent);
            Assert.IsAssignableFrom<IStreamEvent>(streamEvent);

            Assert.Equal(clientAndSeed.Id, streamEvent.Payload.Id);
        }

        [Fact]
        public async Task GetById_NotFoundResultAsync()
        {
            var wrongEventId = Guid.NewGuid().ToString();
            var clientAndSeed = CreateClientAndSeedData();

            // The endpoint or route of the controller action.
            var httpResponse = await clientAndSeed.testClient.GetAsync(
                $"{baseEventControllerPath}GetById/{wrongEventId}"
            );

            Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var apiNotFoundModel = JsonConvert.DeserializeObject<ApiNotFoundModel>(stringResponse);

            Assert.IsType<ApiNotFoundModel>(apiNotFoundModel);

            Assert.Equal(wrongEventId, apiNotFoundModel.CallParams.ToString());
        }

        [Fact]
        public async Task GetByMachineId_OkResultAsync()
        {
            var clientAndSeed = CreateClientAndSeedData();

            // The endpoint or route of the controller action.            
            var httpResponse = await clientAndSeed.testClient.GetAsync(
                $"{baseEventControllerPath}GetByMachineId/{clientAndSeed.machineId}?limit=5"
            );
            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var streamEvents = JsonConvert.DeserializeObject<IEnumerable<StreamEvent>>(stringResponse);

            Assert.IsAssignableFrom<IEnumerable<StreamEvent>>(streamEvents);
            Assert.IsAssignableFrom<IEnumerable<IStreamEvent>>(streamEvents);

            Assert.Equal(5, streamEvents.Count());
            Assert.True(streamEvents.All(x => x.Payload.MachineId == clientAndSeed.machineId));
        }

        [Fact]
        public async Task GetByMachineId_NotFoundResultAsync()
        {
            var clientAndSeed = CreateClientAndSeedData();
            var wrongMachineId = Guid.NewGuid().ToString();

            // The endpoint or route of the controller action.
            var httpResponse = await clientAndSeed.testClient.GetAsync(
                $"{baseEventControllerPath}GetByMachineId/{wrongMachineId}"
            );

            Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var apiNotFoundModel = JsonConvert.DeserializeObject<ApiNotFoundModel>(stringResponse);

            Assert.IsType<ApiNotFoundModel>(apiNotFoundModel);

            Assert.Equal(wrongMachineId, apiNotFoundModel.CallParams.ToString());
        }

        [Fact]
        public async Task GetLastEvents_OkResultAsync()
        {
            var clientAndSeed = CreateClientAndSeedData();
            
            // The endpoint or route of the controller action.
            var httpResponse = await clientAndSeed.testClient.GetAsync(
                $"{baseEventControllerPath}GetLastEvents"
            );

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var streamEvents = JsonConvert.DeserializeObject<IEnumerable<StreamEvent>>(stringResponse);
            
            Assert.IsAssignableFrom<IEnumerable<StreamEvent>>(streamEvents);
            Assert.IsAssignableFrom<IEnumerable<IStreamEvent>>(streamEvents);

            Assert.Equal(31, streamEvents.Count());
        }

        [Fact]
        public async Task GetLastEvents_NotFoundResultAsync()
        {
            // The endpoint or route of the controller action.
            var httpResponse = await client.GetAsync(
                $"{baseEventControllerPath}GetLastEvents"
            );

            Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var apiNotFoundModel = JsonConvert.DeserializeObject<ApiNotFoundModel>(stringResponse);

            Assert.IsType<ApiNotFoundModel>(apiNotFoundModel);

            Assert.Null(apiNotFoundModel.CallParams);
        }

        [Fact]
        public async Task GetByFiltersMachineId_OkResultAsync()
        {
            var clientAndSeed = CreateClientAndSeedData();

            // The endpoint or route of the controller action.
            var httpResponse = await clientAndSeed.testClient.GetAsync(
                $"{baseEventControllerPath}GetByFilters?machine_id={clientAndSeed.machineId}&limit=5"
            );

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var streamEvents = JsonConvert.DeserializeObject<IEnumerable<StreamEvent>>(stringResponse);

            Assert.IsAssignableFrom<IEnumerable<StreamEvent>>(streamEvents);
            Assert.IsAssignableFrom<IEnumerable<IStreamEvent>>(streamEvents);

            Assert.Equal(5, streamEvents.Count());
            Assert.True(streamEvents.All(x => x.Payload.MachineId == clientAndSeed.machineId));
        }

        [Fact]
        public async Task GetByFiltersStatus_OkResultAsync()
        {
            var clientAndSeed = CreateClientAndSeedData();

            // The endpoint or route of the controller action.
            var httpResponse = await clientAndSeed.testClient.GetAsync(
                $"{baseEventControllerPath}GetByFilters?status={clientAndSeed.status},running&limit=20"
            );

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var streamEvents = JsonConvert.DeserializeObject<IEnumerable<StreamEvent>>(stringResponse);

            Assert.IsAssignableFrom<IEnumerable<StreamEvent>>(streamEvents);
            Assert.IsAssignableFrom<IEnumerable<IStreamEvent>>(streamEvents);

            Assert.InRange(streamEvents.Count(), 10, 20);
            Assert.Contains(clientAndSeed.status, streamEvents.Select(x => x.Payload.Status));
        }

        [Fact]
        public async Task GetByFilters_NotFoundResultAsync()
        {
            // The endpoint or route of the controller action.
            var httpResponse = await client.GetAsync(
                $"{baseEventControllerPath}GetByFilters"
            );

            Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var apiNotFoundModel = JsonConvert.DeserializeObject<ApiNotFoundModel>(stringResponse);

            Assert.IsType<ApiNotFoundModel>(apiNotFoundModel);
            Assert.IsType<JObject>(apiNotFoundModel.CallParams);
        }

        [Fact]
        public async Task GetByFiltersTimestampFrom_OkResultAsync()
        {
            var clientAndSeed = CreateClientAndSeedData();
            var from = DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");

            // The endpoint or route of the controller action.
            var httpResponse = await clientAndSeed.testClient.GetAsync(
                $"{baseEventControllerPath}GetByFilters?from={from},running&limit=20"
            );

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var streamEvents = JsonConvert.DeserializeObject<IEnumerable<StreamEvent>>(stringResponse);

            Assert.IsAssignableFrom<IEnumerable<StreamEvent>>(streamEvents);
            Assert.IsAssignableFrom<IEnumerable<IStreamEvent>>(streamEvents);

            Assert.Equal(20, streamEvents.Count());
            foreach (var streamEvent in streamEvents)
            {
                var dateTo = DateTime.Now;
                var dateFrom = DateTime.Parse(from);
                Assert.InRange(DateTime.Parse(streamEvent.Payload.Timestamp), dateFrom, dateTo);
            }
        }

        [Fact]
        public async Task GetByFiltersTimestampTo_OkResultAsync()
        {
            var clientAndSeed = CreateClientAndSeedData();
            var to = DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");

            // The endpoint or route of the controller action.
            var httpResponse = await clientAndSeed.testClient.GetAsync(
                $"{baseEventControllerPath}GetByFilters?to={to},running&limit=20"
            );

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var streamEvents = JsonConvert.DeserializeObject<IEnumerable<StreamEvent>>(stringResponse);

            Assert.IsAssignableFrom<IEnumerable<StreamEvent>>(streamEvents);
            Assert.IsAssignableFrom<IEnumerable<IStreamEvent>>(streamEvents);

            Assert.Equal(10, streamEvents.Count());
            foreach (var streamEvent in streamEvents)
            {
                var dateTo = DateTime.Parse(to);
                var dateFrom = DateTime.Now.AddDays(-6);
                Assert.InRange(DateTime.Parse(streamEvent.Payload.Timestamp), dateFrom, dateTo);
            }
        }

        public void Dispose()
        {
            runner?.Dispose();
        }
    }
}
