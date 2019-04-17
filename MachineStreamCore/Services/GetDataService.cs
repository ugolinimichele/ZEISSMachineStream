using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MachineStreamCore.Interfaces;
using MachineStreamCore.Models;
using MachineStreamCore.Models.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace MachineStreamCore.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class GetDataService : IGetData
    {
        private readonly ILogger logger;
        private readonly IMongo mongo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mongo"></param>
        /// <param name="logger"></param>
        public GetDataService(IMongo mongo, ILogger<GetDataService> logger)
        {
            this.logger = logger;
            this.mongo = mongo;
        }

        private FilterDefinition<WebSocketStream> GetIsDeletedFilter(bool value = false)
        {
            return Builders<WebSocketStream>.Filter.Eq(x => x.IsDeleted, value);
        }

        /// <summary>
        /// It returns the set of events fetched using the defined filters
        /// </summary>
        /// <param name="filters">The filters to apply, for more info on filters check GetStreamEventsFilters()</param>
        /// <returns>A task that contains the enumerable list of IWebSocketStream that matches the filters</returns>
        public async Task<IEnumerable<IWebSocketStream>> GetEventsByFiltersAsync(IDictionary<string, string> filters)
        {
            var (mongoDBFilters, limit) = ParseFilters(filters);
            return await mongo.FindEventsByFiltersAsync(mongoDBFilters, limit: limit);
        }

        private (FilterDefinition<WebSocketStream> mongoDBfilters, int limit) ParseFilters(IDictionary<string, string> filters)
        {
            FilterDefinition<WebSocketStream> mongoDBFilters = GetIsDeletedFilter();
            int limit = 100;

            foreach (var filter in filters)
                switch (filter.Key)
                {
                    case "machine_id":
                        mongoDBFilters = mongoDBFilters & Builders<WebSocketStream>.Filter.Eq(x => x.StreamEvent.Payload.MachineId, filter.Value);
                        break;
                    case "status":
                        mongoDBFilters = mongoDBFilters & Builders<WebSocketStream>.Filter.In(x => x.StreamEvent.Payload.Status, filter.Value.Split(","));
                        break;
                    case "from":
                        mongoDBFilters = mongoDBFilters & Builders<WebSocketStream>.Filter.Gte(x => x.StreamEvent.Payload.Timestamp, filter.Value);
                        break;
                    case "to":
                        mongoDBFilters = mongoDBFilters & Builders<WebSocketStream>.Filter.Lte(x => x.StreamEvent.Payload.Timestamp, filter.Value);
                        break;
                    case "limit":
                        limit = int.Parse(filter.Value);
                        break;
                    default:
                        break;
                }

            return (mongoDBFilters, limit);
        }

        /// <summary>
        /// It return the single event that match the defined id
        /// </summary>
        /// <param name="id">The id of the event we want to retrieve</param>
        /// <returns>A task that contains the WebSocketStream correspond to the id passed as param</returns>
        public async Task<IWebSocketStream> GetEventByIdAsync(string Id)
        {
            FilterDefinition<WebSocketStream> idFilter = Builders<WebSocketStream>.Filter.Eq(x => x.StreamEvent.Payload.Id, Id);
            var findResults = await mongo.FindEventsByFiltersAsync(idFilter & GetIsDeletedFilter(), limit: 1);
            return findResults?.FirstOrDefault();
        }

        /// <summary>
        /// Get the last (ordered by payload.timestamp descending) events that match the machineId passed as param
        /// </summary>
        /// <param name="machineId">The machine id to apply as filter</param>
        /// <param name="limit">The optional limit on the number of records to retrieve (default: 100)</param>
        /// <returns>A task that contains the enumerable list of IWebSocketStream that matches the machine id passed as param</returns>
        public async Task<IEnumerable<IWebSocketStream>> GetLastEventsByMachineIdAsync(string machineId, int limit = 100)
        {
            FilterDefinition<WebSocketStream> machineIdFilter = Builders<WebSocketStream>.Filter.Eq(x => x.StreamEvent.Payload.MachineId, machineId);
            return await mongo.FindEventsByFiltersAsync(
                machineIdFilter & GetIsDeletedFilter(),
                Builders<WebSocketStream>.Sort.Descending(x => x.StreamEvent.Payload.Timestamp),
                limit
            );
        }

        /// <summary>
        /// Get the last (ordered by payload.timestamp descending) events that match the status passed as param
        /// </summary>
        /// <param name="status">The status of the event to use as filter</param>
        /// <param name="limit">The optional limit on the number of records to retrieve (default: 100)</param>
        /// <returns>A task that contains the enumerable list of IWebSocketStream that matches the status passed as param</returns>
        public async Task<IEnumerable<IWebSocketStream>> GetLastEventsByStatusAsync(string status, int limit = 100)
        {
            FilterDefinition<WebSocketStream> statusFilter = Builders<WebSocketStream>.Filter.Eq(x => x.StreamEvent.Payload.Status, status);
            return await mongo.FindEventsByFiltersAsync(
                statusFilter & GetIsDeletedFilter(),
                Builders<WebSocketStream>.Sort.Descending(x => x.StreamEvent.Payload.Timestamp),
                limit
            );
        }

        /// <summary>
        /// Get the last event ordered by payload.timestamp descending
        /// </summary>
        /// <param name="limit">The optional limit on the number of records to retrieve (default: 100)</param>
        /// <returns>A task that contains the enumerable list of last received IWebSocketStream</returns>
        public async Task<IEnumerable<IWebSocketStream>> GetLastEventsAsync(int limit = 100)
        {
            return await mongo.FindEventsByFiltersAsync(
                GetIsDeletedFilter(),
                Builders<WebSocketStream>.Sort.Descending(x => x.StreamEvent.Payload.Timestamp),
                limit: limit
            );
        }
    }
}
