using MachineStreamCore.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MachineStreamCore.Models
{
    /// <summary>
    /// Helpers methods for models of the application
    /// </summary>
    public static class ModelHelpers
    {
        /// <summary>
        /// Method to get all the possible applicable filters to the events
        /// at the moment this is a static method but in the future we could use a JSON schema
        /// to share this info between Backend and Frontend
        /// </summary>
        /// <returns>A list with the filter specification that the FrontEnd can use to retrieve data</returns>
        public static IEnumerable<IFilterSpecifications> GetStreamEventsFilters()
        {
            List<IFilterSpecifications> filterSpecifications = new List<IFilterSpecifications>();
            
            filterSpecifications.Add(new FilterSpecifications("machine_id", "guid", "the machine id which the events belongs"));
            filterSpecifications.Add(new FilterSpecifications("status", "string", "A comma separated string of the possible values of the status event flag", values: GetStreamEventStatuses()));
            filterSpecifications.Add(new FilterSpecifications("from", "date", "the from date of the reported timestamp (in string format)", format: "YYYY-MM-DDTHH:mm:ss.sssZ"));
            filterSpecifications.Add(new FilterSpecifications("to", "date", "the to date of the reported timestamp (in string format)", format: "YYYY-MM-DDTHH:mm:ss.sssZ"));
            filterSpecifications.Add(new FilterSpecifications("limit", "int", "the max number of records in the response, default: 100"));

            return filterSpecifications;
        }

        public static IEnumerable<string> GetStreamEventStatuses()
        {
            return new string[] { "idle", "running", "finished", "errorred", "repaired" };
        }

        /// <summary>
        /// It validate the filter used in the API call with the available
        /// </summary>
        /// <param name="filters"></param>
        /// <returns>The list of errors in the validation process, if empty the validation succeeded</returns>
        public static IEnumerable<string> ValidateFilters(IDictionary<string, string> filters)
        {
            List<string> errors = new List<string>();
            var availableFilters = GetStreamEventsFilters();

            if (filters.Keys.All(x => availableFilters.Select(y => y.Name).Contains(x)))
            {
                //check machine_id
                Guid guid = Guid.Empty;
                if (filters.Keys.Contains("machine_id") && !Guid.TryParse(filters["machine_id"], out guid))
                    errors.Add("machine_id is not a valid guid");
                //check status
                if (filters.Keys.Contains("status") && !filters["status"].Split(",").All(x => GetStreamEventStatuses().Contains(x)))
                    errors.Add("one or more statuses are not valid, check the possible values");
                //check from
                //I should check with a regex if the format is exactly the one in the specs
                //instead than using lenght and try parse
                DateTime dateTime = DateTime.UtcNow;
                if (filters.Keys.Contains("from") && filters["from"].Length == 24 && !DateTime.TryParse(filters["from"], out dateTime))
                    errors.Add("from is not a valid ISO date");
                //check to
                if (filters.Keys.Contains("to") && filters["to"].Length == 24 && !DateTime.TryParse(filters["to"], out dateTime))
                    errors.Add("to is not a valid ISO date");
                //check limit
                int limit = 0;
                if (filters.Keys.Contains("limit") && !int.TryParse(filters["limit"], out limit))
                    errors.Add("limit is not a valid integer");
            }
            else
            {
                errors.Add("One or more filters are not in the list of the availables");
            }
            return errors;
        }
    }
}
