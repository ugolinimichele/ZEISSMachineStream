using MachineStreamCore.Models.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MachineStreamCore.Models
{
    /// <summary>
    /// Class used to define and describe the filters that can be used by the front-end
    /// </summary>
    public class FilterSpecifications : IFilterSpecifications
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Format { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Values { get; set; }

        public FilterSpecifications(string name, string type, string description, string format = null, IEnumerable<string> values = null)
        {
            Name = name;
            Type = type;
            Description = description;
            Format = format;
            Values = values;
        }
    }
}
