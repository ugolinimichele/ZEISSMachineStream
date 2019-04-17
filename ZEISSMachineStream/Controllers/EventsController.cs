using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MachineStreamCore.Interfaces;
using MachineStreamCore.Models;
using MachineStreamCore.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZEISSMachineStream.Models;

namespace ZEISSMachineStream.Controllers
{
    /// <summary>
    /// The API controller used by the FrontEnd to fetch the data of the events
    /// </summary>
    [Route("v1/[controller]/[action]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly IGetData getData;

        public EventsController(IGetData getData, ILogger<EventsController> logger)
        {
            this.logger = logger;
            this.getData = getData;
        }

        /// <summary>
        /// Get the filter specification used to call the GetByFilters API
        /// </summary>
        /// <returns>A list of all the possible filters to apply when GetByFilters is called</returns>
        /// <response code="200">The list is retrieved correctly</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IFilterSpecifications>), 200)]
        public ActionResult<IEnumerable<IFilterSpecifications>> GetFilterSpecs()
        {
            return Ok(ModelHelpers.GetStreamEventsFilters());
        }

        /// <summary>
        /// Get the events applying the filters passed in query string (Optional)
        /// </summary>
        /// <param name="filters">Optional. The available filter to apply. Check GetFilterSpecs for more info.</param>
        /// <returns>The list of the events with the applied filters</returns>
        /// <response code="200">The list is retrieved correctly applying the filters</response>
        /// <response code="400">The filters passed in query string are not formatted correctly</response>
        /// <response code="404">No events available for the provided filters</response>
        /// <response code="500">Server error or the service is currently unavailable</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IStreamEvent>), 200)]
        [ProducesResponseType(typeof(ApiBadRequestModel), 400)]
        [ProducesResponseType(typeof(ApiNotFoundModel), 404)]
        [ProducesResponseType(typeof(ApiServerErrorModel), 500)]
        public async Task<ActionResult<IEnumerable<IStreamEvent>>> GetByFilters([FromQuery] Dictionary<string, string> filters)
        {
            var validation = ModelHelpers.ValidateFilters(filters);
            if (validation.Count() > 0)
                return BadRequest(new ApiBadRequestModel
                {
                    CallParams = filters,
                    Reasons = validation
                });
            else
            {
                try
                {
                    var webSocketStreams = await getData.GetEventsByFiltersAsync(filters);
                    if (webSocketStreams.Count() > 0)
                        return Ok(webSocketStreams.Select(x => x.StreamEvent));

                    return NotFound(new ApiNotFoundModel
                    {
                        CallParams = filters,
                        Messages = new string[] { "There aren't events for the specified filters" }
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"EventController.GetByFilters({filters}) caused an error");
                    return StatusCode(500, new ApiServerErrorModel
                    {
                        CallParams = filters,
                        Errors = new string[] { $"The fetch of data by filters caused and error or the service is currently unavailable" }
                    });
                }
            }
        }

        /// <summary>
        /// Fetch the event corresponding to the id passed as URL parameter
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <returns></returns>
        /// <response code="200">The event corresponding to the id passed as URL parameter</response>
        /// <response code="404">The event for the specified id is not found or is been deleted</response>
        /// <response code="500">Server error or the service is currently unavailable</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IStreamEvent), 200)]
        [ProducesResponseType(typeof(ApiNotFoundModel), 404)]
        [ProducesResponseType(typeof(ApiServerErrorModel), 500)]
        public async Task<ActionResult<IStreamEvent>> GetById(string id)
        {
            try
            {
                var webSocketStream = await getData.GetEventByIdAsync(id);
                if (webSocketStream != null)
                    return Ok(webSocketStream.StreamEvent);
                else
                    return NotFound(new ApiNotFoundModel
                    {
                        CallParams = id,
                        Messages = new string[] { $"The event for the specified id is not found or is been deleted" }
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"EventController.GetById({id}) caused an error");
                return StatusCode(500, new ApiServerErrorModel
                {
                    CallParams = id,
                    Errors = new string[] { $"The fetch of data by id caused and error or the service is currently unavailable" }
                });
            }
        }

        /// <summary>
        /// Fetch the last events (based on the payload.timestamp descending) for the provided machine id
        /// </summary>
        /// <param name="machine_id">The machine id that identify the machine where the events are streamed</param>
        /// <param name="limit">Optional (default 100). to limit the number of events to fetch</param>
        /// <returns>A list of events ordered by timestamp id passed as URL parameter</returns>
        /// <response code="200">The events streamed by the machine_id passed as URL parameter</response>
        /// <response code="404">The events for the specified machine_id are not found or are been deleted</response>
        /// <response code="500">Server error or the service is currently unavailable</response>
        [HttpGet("{machine_id}")]
        [ProducesResponseType(typeof(IEnumerable<IStreamEvent>), 200)]
        [ProducesResponseType(typeof(ApiNotFoundModel), 404)]
        [ProducesResponseType(typeof(ApiServerErrorModel), 500)]
        public async Task<ActionResult<IEnumerable<IStreamEvent>>> GetByMachineId(string machine_id, [FromQuery] int limit = 100)
        {
            try
            {
                var webSocketStreams = await getData.GetLastEventsByMachineIdAsync(machine_id, limit);
                if (webSocketStreams.Count() > 0)
                    return Ok(webSocketStreams.Select(x => x.StreamEvent));
                else
                    return NotFound(new ApiNotFoundModel
                    {
                        CallParams = machine_id,
                        Messages = new string[] { $"The event for the specified machine_id is not found or is been deleted" }
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"EventController.GetByMachineId({machine_id}) caused an error");
                return StatusCode(500, new ApiServerErrorModel
                {
                    CallParams = machine_id,
                    Errors = new string[] { $"The fetch of data by machine_id caused and error or the service is currently unavailable" }
                });
            }
        }

        /// <summary>
        /// Fetch the last events (based on the payload.timestamp descending) that match the status passed in the URL parameter
        /// </summary>
        /// <param name="status">The status of the events to fetch</param>
        /// <param name="limit">Optional (default 100). to limit the number of events to fetch</param>
        /// <returns>A list of events ordered by timestamp that match the status passed as URL parameter</returns>
        /// <response code="200">The events that match the status passed in the URL parameter</response>
        /// <response code="404">The events for the specified status are not found or are been deleted</response>
        /// <response code="500">Server error or the service is currently unavailable</response>
        [HttpGet("{status}")]
        [ProducesResponseType(typeof(IEnumerable<IStreamEvent>), 200)]
        [ProducesResponseType(typeof(ApiNotFoundModel), 404)]
        [ProducesResponseType(typeof(ApiServerErrorModel), 500)]
        public async Task<ActionResult<IEnumerable<IStreamEvent>>> GetByStatus(string status, [FromQuery] int limit = 100)
        {
            try
            {
                var webSocketStreams = await getData.GetLastEventsByStatusAsync(status, limit);
                if (webSocketStreams.Count() > 0)
                    return Ok(webSocketStreams.Select(x => x.StreamEvent));
                else
                    return NotFound(new ApiNotFoundModel
                    {
                        Messages = new string[] { $"There are no events or the events are deleted" }
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"EventController.GetLastEventsAsync() caused an error");
                return StatusCode(500, new ApiServerErrorModel
                {
                    Errors = new string[] { $"The fetch of last events caused and error or the service is currently unavailable" }
                });
            }
        }

        /// <summary>
        /// Fetch the last arrived events, ordered by payload.timestamp descending
        /// </summary>
        /// <param name="limit">Optional (default 100). to limit the number of events to fetch</param>
        /// <returns>A list with the last events received, by payload.timestamp descending</returns>
        /// <response code="200">The last events ordered by payload.timestamp descending</response>
        /// <response code="404">There are no events or the events are deleted</response>
        /// <response code="500">Server error or the service is currently unavailable</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IStreamEvent>), 200)]
        [ProducesResponseType(typeof(ApiNotFoundModel), 404)]
        [ProducesResponseType(typeof(ApiServerErrorModel), 500)]
        public async Task<ActionResult<IEnumerable<IStreamEvent>>> GetLastEvents([FromQuery] int limit = 100)
        {
            try
            {
                var webSocketStreams = await getData.GetLastEventsAsync(limit);
                if (webSocketStreams.Count() > 0)
                    return Ok(webSocketStreams.Select(x => x.StreamEvent));
                else
                    return NotFound(new ApiNotFoundModel
                    {
                        Messages = new string[] { $"There are no events or the events are deleted" }
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"EventController.GetLastEventsAsync() caused an error");
                return StatusCode(500, new ApiServerErrorModel
                {
                    Errors = new string[] { $"The fetch of last events caused and error or the service is currently unavailable" }
                });
            }
        }
    }
}
