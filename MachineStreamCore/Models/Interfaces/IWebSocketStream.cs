using MongoDB.Bson.Serialization.Attributes;

namespace MachineStreamCore.Models.Interfaces
{
    /// <summary>
    /// Interface to handle the stream from the MachineStream
    /// </summary>
    public interface IWebSocketStream: IAuditFields
    {
        /// <summary>
        /// The container of the stream event
        /// </summary>
        IStreamEvent StreamEvent { get; set; }
    }

    /// <summary>
    /// Interface used to encapsulate the Event from the stream/websocket
    /// </summary>
    public interface IStreamEvent
    {
        /// <summary>
        /// The topic of the event
        /// </summary>
        string Topic { get; set; }
        /// <summary>
        /// The reference of the event
        /// </summary>
        string Ref { get; set; }
        /// <summary>
        /// The payload of the event
        /// </summary>
        IPayload Payload { get; set; }
        /// <summary>
        /// The Join reference
        /// </summary>
        string JoinRef { get; set; }
        /// <summary>
        /// The event type
        /// </summary>
        string Event { get; set; }
    }

    /// <summary>
    /// Interface used to encapusate the payload of the event
    /// </summary>
    public interface IPayload
    {
        /// <summary>
        /// The timestamp of the event, when it was registered
        /// </summary>
        string Timestamp { get; set; }
        /// <summary>
        /// The status can be either idle, running, finished or errorred in which case they will be repaired automatically and
        /// a repaired event will be sent before resetting to idle again.
        /// </summary>
        string Status { get; set; }
        /// <summary>
        /// The unique identifier of the machine where the event was registered
        /// </summary>
        string MachineId { get; set; }
        /// <summary>
        /// The unique identifier of the event
        /// </summary>
        string Id { get; set; }
    }
}
