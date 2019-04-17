using Newtonsoft.Json;
using System.Collections.Generic;

namespace ZEISSMachineStream.Models
{
    // in this case we could do in a easier way and avoid to define these
    // models, but in more complex case can be very useful define model accondingly
    // with the FrontEnd team, introducing also particular code (as enum) to identity
    // in a more specific way the correct error (e.g. instead than using always 500)

    /// <summary>
    /// The base api response model, it contains only the CallParams object that
    /// represent the params used in the call
    /// </summary>
    public class BaseApiModel
    {
        /// <summary>
        /// Optional object to identity the params used in the call
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object CallParams { get; set; }
    }

    /// <summary>
    /// Model used to identify the 5xx errors
    /// </summary>
    public class ApiServerErrorModel : BaseApiModel
    {
        /// <summary>
        /// List of string describe the errors during the call
        /// </summary>
        public IEnumerable<string> Errors { get; set; }
    }

    /// <summary>
    /// Model used to identify the 400 errors
    /// </summary>
    public class ApiBadRequestModel : BaseApiModel
    {
        /// <summary>
        /// List of strings describe the reasons to mark the request as BadRequest
        /// </summary>
        public IEnumerable<string> Reasons { get; set; }
    }

    /// <summary>
    /// Model used to identify the 404 errors
    /// </summary>
    public class ApiNotFoundModel : BaseApiModel
    {
        /// <summary>
        /// List of strings describe the NotFound reasons of the call
        /// </summary>
        public IEnumerable<string> Messages { get; set; }
    }
}
