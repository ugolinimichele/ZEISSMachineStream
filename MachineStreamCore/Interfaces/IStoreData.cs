using MachineStreamCore.Models.Interfaces;
using System.Threading.Tasks;

namespace MachineStreamCore.Interfaces
{
    /// <summary>
    /// Interface that expose the business logic to store the data from ZEISS MachineStream
    /// </summary>
    public interface IStoreData
    {
        /// <summary>
        /// Method used to store the new received event (JSON like)
        /// </summary>
        /// <param name="streamEvent">The stream event that need to be stored</param>
        /// <param name="source">A string description of the event source</param>
        /// <returns></returns>
        Task StoreEventAsync(IStreamEvent streamEvent, string source);
    }
}
