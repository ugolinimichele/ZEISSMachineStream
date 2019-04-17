using MachineStreamCore.Models.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineStreamCore.Interfaces
{
    /// <summary>
    /// Interface that expose the business logic to get the data for the frontend consumers
    /// </summary>
    public interface IGetData
    {
        /// <summary>
        /// It returns the set of events fetched using the defined filters
        /// </summary>
        /// <param name="filters">The filters to apply, for more info on filters check GetStreamEventsFilters()</param>
        /// <returns>A task that contains the enumerable list of IWebSocketStream that matches the filters</returns>
        Task<IEnumerable<IWebSocketStream>> GetEventsByFiltersAsync(IDictionary<string, string> filters);

        /// <summary>
        /// It return the single event that match the defined id
        /// </summary>
        /// <param name="id">The id of the event we want to retrieve</param>
        /// <returns>A task that contains the IWebSocketStream correspond to the id passed as param</returns>
        Task<IWebSocketStream> GetEventByIdAsync(string id);

        /// <summary>
        /// Get the last (ordered by payload.timestamp descending) events that match the machineId passed as param
        /// </summary>
        /// <param name="machineId">The machineId to apply as filter</param>
        /// <param name="limit">The optional limit on the number of records to retrieve (default: 100)</param>
        /// <returns>A task that contains the enumerable list of IWebSocketStream that matches the machine id passed as param</returns>
        Task<IEnumerable<IWebSocketStream>> GetLastEventsByMachineIdAsync(string machineId, int limit = 100);

        /// <summary>
        /// Get the last (ordered by payload.timestamp descending) events that match the status passed as param
        /// </summary>
        /// <param name="status">The status of the event to use as filter</param>
        /// <param name="limit">The optional limit on the number of records to retrieve (default: 100)</param>
        /// <returns>A task that contains the enumerable list of IWebSocketStream that matches the status passed as param</returns>
        Task<IEnumerable<IWebSocketStream>> GetLastEventsByStatusAsync(string status, int limit = 100);

        /// <summary>
        /// Get the last event ordered by payload.timestamp descending
        /// </summary>
        /// <param name="limit">The optional limit on the number of records to retrieve (default: 100)</param>
        /// <returns>A task that contains the enumerable list of last received IWebSocketStream</returns>
        Task<IEnumerable<IWebSocketStream>> GetLastEventsAsync(int limit = 100);
    }
}
