using MachineStreamCore.Models;
using System.Collections.Generic;
using Xunit;

namespace UnitTests
{
    public class FilterValidationTests
    {
        private IDictionary<string, string> emptyFilters = new Dictionary<string, string>();

        private IDictionary<string, string> filterDate = new Dictionary<string, string>
        {
            { "from", "2019-04-14T15:33:04.036625Z" },
            { "to", "2019-04-14T18:33:04.036625Z" }
        };

        private IDictionary<string, string> filterMachineId = new Dictionary<string, string>
        {
            { "machine_id", "59d9f4b4-018f-43d8-92d0-c51de7d987e5" }
        };

        private IDictionary<string, string> filterStatuses = new Dictionary<string, string>
        {
            { "status", "idle,running" }
        };

        private IDictionary<string, string> filterLimit = new Dictionary<string, string>
        {
            { "limit", "50" }
        };

        private IDictionary<string, string> filterMulti1 = new Dictionary<string, string>
        {
            { "from", "2019-04-14T15:33:04.036625Z" },
            { "to", "2019-04-14T18:33:04.036625Z" },
            { "machine_id", "59d9f4b4-018f-43d8-92d0-c51de7d987e5" },
            { "status", "idle,running" },
            { "limit", "50" }
        };

        private IDictionary<string, string> filterMulti2 = new Dictionary<string, string>
        {
            { "from", "2019-04-14T15:33:04.036625Z" },
            { "to", "2019-04-14T18:33:04.036625Z" },
            { "limit", "50" }
        };

        [Fact]
        public void ValidEmptyFilters_Test()
        {
            var validationResult = ModelHelpers.ValidateFilters(emptyFilters);
            Assert.Empty(validationResult);
        }

        [Fact]        
        public void ValidDateFilters_Test()
        {
            var validationResult = ModelHelpers.ValidateFilters(filterDate);
            Assert.Empty(validationResult);
        }

        [Fact]
        public void ValidMachineIdFilters_Test()
        {
            var validationResult = ModelHelpers.ValidateFilters(filterMachineId);
            Assert.Empty(validationResult);
        }

        [Fact]
        public void ValidStatusFilters_Test()
        {
            var validationResult = ModelHelpers.ValidateFilters(filterStatuses);
            Assert.Empty(validationResult);
        }

        [Fact]
        public void ValidLimitFilters_Test()
        {
            var validationResult = ModelHelpers.ValidateFilters(filterLimit);
            Assert.Empty(validationResult);
        }

        [Fact]
        public void ValidMulti1Filters_Test()
        {
            var validationResult = ModelHelpers.ValidateFilters(filterMulti1);
            Assert.Empty(validationResult);
        }

        [Fact]
        public void ValidMulti2Filters_Test()
        {
            var validationResult = ModelHelpers.ValidateFilters(filterMulti2);
            Assert.Empty(validationResult);
        }
    }
}
