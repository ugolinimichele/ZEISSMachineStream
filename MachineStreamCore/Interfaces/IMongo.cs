using MachineStreamCore.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MachineStreamCore.Interfaces
{
    /// <summary>
    /// Interface used to manage all the operation to MongoDB (CosmosDB in this case)
    /// </summary>
    public interface IMongo
    {
        /// <summary>
        /// Method contains the logic to insert a WebSocket event into MongoDB
        /// </summary>
        /// <param name="WebSocketStream"></param>
        /// <returns></returns>
        Task InsertEventAsync(WebSocketStream WebSocketStream);

        /// <summary>
        /// Method that query mongo to retrieve the data to provide to the FrontEnd
        /// </summary>
        /// <param name="filters">The filters to apply to the Event Collection</param>
        /// <param name="sort">The optional sort definition to apply to the Event Collection</param>
        /// <param name="limit">The optional limit to set the max number of records to retrieve</param>
        /// <returns>The list of the retrieved records</returns>
        Task<List<WebSocketStream>> FindEventsByFiltersAsync(FilterDefinition<WebSocketStream> filters, SortDefinition<WebSocketStream> sort = null, int limit = 100);
    }
}