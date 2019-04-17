using MachineStreamCore.Interfaces;
using MachineStreamCore.Models;
using MachineStreamCore.Models.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace MachineStreamCore.Services
{
    /// <summary>
    /// Class that encapsulate the business logic to store the data for the Machine Stream solution
    /// </summary>
    public class StoreDataService: IStoreData
    {
        private readonly ILogger logger;
        private readonly IMongo mongo;

        /// <summary>
        /// Constructor used by the DI
        /// </summary>
        /// <param name="mongo">The injected interface used to access to the mongo exposed methods</param>
        /// <param name="logger">The injected logger</param>
        public StoreDataService(IMongo mongo, ILogger<StoreDataService> logger)
        {
            this.logger = logger;
            this.mongo = mongo;
        }

        /// <summary>
        /// Method that encapsulate the business logic to store the event
        /// </summary>
        /// <param name="streamEvent">The stream event that need to be stored</param>
        /// <param name="source">A string description of the event source</param>
        /// <returns></returns>
        public async Task StoreEventAsync(IStreamEvent streamEvent, string source)
        {
            await mongo.InsertEventAsync(new WebSocketStream(streamEvent, source));
        }
    }
}
