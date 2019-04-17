using MachineStreamCore.Models;
using MachineStreamCore.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests
{
    public class FilterSpecificationsTests
    {
        private readonly IEnumerable<IFilterSpecifications> filterSpecifications;
        private readonly IEnumerable<string> streamEventsStatuses;

        public FilterSpecificationsTests()
        {
            filterSpecifications = ModelHelpers.GetStreamEventsFilters();
            streamEventsStatuses = ModelHelpers.GetStreamEventStatuses();
        }

        [Fact]
        public void ValidFilterSpecificationNumber_Test()
        {
            Assert.IsAssignableFrom<IEnumerable<IFilterSpecifications>>(filterSpecifications);

            Assert.Equal(5, filterSpecifications.Count());

            foreach (var filterSpecification in filterSpecifications)
            {
                Assert.IsType<FilterSpecifications>(filterSpecification);
                Assert.IsAssignableFrom<IFilterSpecifications>(filterSpecification);

                Assert.IsType<string>(filterSpecification.Name);
                Assert.IsType<string>(filterSpecification.Description);
                Assert.IsType<string>(filterSpecification.Type);
            }
        }

        [Fact]
        public void ValidStreamEventsStatuesNumber_Test()
        {
            Assert.IsAssignableFrom<IEnumerable<string>>(streamEventsStatuses);
            Assert.Equal(5, streamEventsStatuses.Count());

            foreach (var streamEventsStatus in streamEventsStatuses)
                Assert.IsType<string>(streamEventsStatus);
        }

        [Fact]
        public void ValidFilterSpecificationContent_Test()
        {
            foreach (var filterSpecification in filterSpecifications)
            {
                switch (filterSpecification.Name)
                {
                    case "machine_id":
                        Assert.Equal("guid", filterSpecification.Type);
                        Assert.Null(filterSpecification.Format);
                        Assert.Null(filterSpecification.Values);
                        break;
                    case "status":
                        Assert.Equal("string", filterSpecification.Type);
                        Assert.Null(filterSpecification.Format);
                        Assert.Equal(streamEventsStatuses, filterSpecification.Values);
                        break;
                    case "from":
                    case "to":
                        Assert.Equal("date", filterSpecification.Type);
                        Assert.Equal("YYYY-MM-DDTHH:mm:ss.sssZ", filterSpecification.Format);
                        Assert.Null(filterSpecification.Values);
                        break;
                    case "limit":
                        Assert.Equal("int", filterSpecification.Type);
                        Assert.Null(filterSpecification.Format);
                        Assert.Null(filterSpecification.Values);
                        break;
                    default:
                        Assert.False(true);
                        break;
                }
            }
        }
    }
}
