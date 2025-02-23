using AutoMapper;
using CMAPTask.Domain.Entities;
using CMAPTask.Infrastructure.Services;
using CMAPTask.web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CMAPTask.Test
{
    public class HomeControllerTests
    {
        private readonly Mock<TimesheetService> _mockTimesheetService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            // Create a mock of the ITimesheetService
            _mockTimesheetService = new Mock<TimesheetService>();
            _mockMapper = new Mock<IMapper>();

            // Setup the mock to return a list of TimesheetEntry when called
            _mockTimesheetService
                .Setup(service => service.GetEntriesAsync())
                .ReturnsAsync(new List<Timesheet>
                {
                    new Timesheet { Id = 1, Description = "Task 1", HoursWorked = 8 },
                    new Timesheet { Id = 2, Description = "Task 2", HoursWorked = 7 }
                });

            // Initialize the controller, passing the mocked service
            _controller = new HomeController(_mockTimesheetService.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithTimesheetEntries()
        {
            //Act
            var result = await _controller.Index();  // Await the asynchronous result

            //Assert
            Assert.IsType<ViewResult>(result);  // Check if the result is a ViewResult

            var viewResult = result as ViewResult;  // Explicitly cast to ViewResult

            Assert.NotNull(viewResult);  // Ensure that the result is not null
            Assert.IsType<List<Timesheet>>(viewResult.Model);  // Check the model type
            var entries = viewResult.Model as List<Timesheet>;
            Assert.Equal(2, entries.Count);  // Assert the correct number of entries
            Assert.Equal("Task 1", entries[0].Description);  // Validate the first entry
            Assert.Equal("Task 2", entries[1].Description);  // Validate the second entry
        }

       
    }
}
