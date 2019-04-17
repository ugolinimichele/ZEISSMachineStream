using System.Collections.Generic;

namespace MachineStreamCore.Models.Interfaces
{
    /// <summary>
    /// The interface that incapsulate the definition of the filters
    /// </summary>
    public interface IFilterSpecifications: IEnumFilterSpecifications
    {
        /// <summary>
        /// The name of the filter to apply, is also the key of the filters passed in the request
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// The type of the value of the filters
        /// </summary>
        string Type { get; set; }
        /// <summary>
        /// A brief description of the filter
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// The optional format of the value
        /// </summary>
        string Format { get; set; }
    }

    public interface IEnumFilterSpecifications
    {
        /// <summary>
        /// If the filter is based on a set of values, the value have to be in this list
        /// </summary>
        IEnumerable<string> Values { get; set; }
    }
}
