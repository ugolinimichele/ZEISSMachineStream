using MachineStreamCore.Models.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MachineStreamCore.Models
{
    /// <summary>
    /// The model contains and implement all the information of a stream event
    /// </summary>
    public class WebSocketStream : IWebSocketStream
    {
        [JsonIgnore]
        public ObjectId Id { get; set; }
        [BsonSerializer(typeof(ImpliedImplementationInterfaceSerializer<IStreamEvent, StreamEvent>))]
        public IStreamEvent StreamEvent { get; set; } = new StreamEvent();
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Base constructor passing the stream event and eventually the CreatedBy field
        /// </summary>
        /// <param name="streamEvent">The stream event of the model</param>
        /// <param name="createdBy">The source of the creation, if not specified if take the standard value "backend"</param>
        public WebSocketStream(IStreamEvent streamEvent, string createdBy = "backend")
        {
            StreamEvent = streamEvent;
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
            UpdatedBy = CreatedBy;
            UpdatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }

        /// <summary>
        /// Override the base ToString method
        /// </summary>
        /// <returns>Formatted string with the MachineId, the Id and the updated audit fields</returns>
        public override string ToString()
        {
            return $"WebSocketStream MachineId: {StreamEvent?.Payload?.MachineId ?? "N.A."} " +
                $"EventId: {StreamEvent?.Payload?.Id ?? "N.A."} UpdatedAt: {UpdatedAt.ToLocalTime()} UpdatedBy {UpdatedBy}";
        }
    }

    /// <summary>
    /// The implementaion of IStreamEvent that contain the stream event received from the websocket
    /// </summary>
    public class StreamEvent : IStreamEvent
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }
        [JsonProperty("ref")]
        public string Ref { get; set; }
        [JsonProperty("payload")]
        [BsonSerializer(typeof(ImpliedImplementationInterfaceSerializer<IPayload, Payload>))]
        public IPayload Payload { get; set; } = new Payload();
        [JsonProperty("join_ref")]
        public string JoinRef { get; set; }
        [JsonProperty("event")]
        public string Event { get; set; }

        //these 2 addition fields are not relevant with the current
        //specification and we can eventually remove them, but since
        //both the JSON on the websocket and the JSON written into mongo
        //can change in this case we can adapt easily to extra fields

        /// <summary>
        /// Used as catch all during deserialization,
        /// but we cannot used JToken to write into mongo
        /// </summary>
        [BsonIgnore]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtraJson { get; set; }
        /// <summary>
        /// Used as catch all if there is additional data into mongo
        /// </summary>
        [JsonIgnore]
        [BsonExtraElements]
        public Dictionary<string, object> ExtraBson { get; set; }
    }

    /// <summary>
    /// The implementaion of IPayload that contain the payload received from the websocket
    /// </summary>
    public class Payload : IPayload
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("machine_id")]
        public string MachineId { get; set;}
        [JsonProperty("id")]
        public string Id { get; set; }
        /// <summary>
        /// Used as catch all during deserialization,
        /// but we cannot used JToken to write into mongo
        /// </summary>
        [BsonIgnore]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtraJson { get; set; }
        /// <summary>
        /// Used as catch all if there is additional data into mongo
        /// </summary>
        [JsonIgnore]
        [BsonExtraElements]
        public Dictionary<string, object> ExtraBson { get; set; }
    }
}
