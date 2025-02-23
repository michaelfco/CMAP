using AutoMapper;
using CMAPTask.Application.DTOs;
using CMAPTask.Application.Interfaces;
using CMAPTask.Domain.Entities;
using CMAPTask.Infrastructure.Services;
using CMAPTask.web.Controllers;
using CMAPTask.web.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CMAPTask.Test
{
    public class HomeControllerTests
    {
        private readonly Mock<ITimesheetService> _mockTimesheetService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockTimesheetService = new Mock<ITimesheetService>();
            _mockMapper = new Mock<IMapper>();

            //mock to return a list of Timesheet entities
            var timesheetEntries = new List<Timesheet>
            {
                new Timesheet { Id = 1, UserName = "Michael S", Date = DateTime.Now, Project = "Project 1", Description = "Task 1", HoursWorked = 8 },
                new Timesheet { Id = 2, UserName = "Michael A", Date = DateTime.Now, Project = "Project 2", Description = "Task 2", HoursWorked = 7 }
            };

            _mockTimesheetService
                .Setup(service => service.GetEntriesAsync())
                .ReturnsAsync(timesheetEntries);

            //mock the mapping of Timesheet
            var timesheetDtos = new List<TimesheetViewModel>
            {
                new TimesheetViewModel { Id = 1, Description = "Task 1", HoursWorked = 8 },
                new TimesheetViewModel { Id = 2, Description = "Task 2", HoursWorked = 7 }
            };

            _mockMapper
                .Setup(mapper => mapper.Map<List<TimesheetViewModel>>(timesheetEntries))
                .Returns(timesheetDtos);

            //initialize the controller with services
            _controller = new HomeController(_mockTimesheetService.Object, _mockMapper.Object);
        }


        /// <summary>
        /// check if the controller correctly interacts with the mocked ITimesheetService to retrieve data
        /// check if the data is correctly mapped using AutoMapper
        /// check if he correct view model is passed to the view
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Index_ReturnViewWithTimesheetEntries()
        {
            //act
            var result = await _controller.Index();

            //assert
            var viewResult = Assert.IsType<ViewResult>(result);
            //ensure the model is of the expected type
            var model = Assert.IsType<List<TimesheetViewModel>>(viewResult.Model);

            Assert.NotNull(model);
            //number of entries
            Assert.Equal(2, model.Count);
            Assert.Equal("Task 1", model[0].Description);
            Assert.Equal("Task 2", model[1].Description);
        }



        /// <summary>
        ///check if the controller correctly interacts with service to retrieve data
        ///check if the controller generates a CSV file with the correct data
        ///ensure that the returned file has the correct data and file name
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DownloadCsv_ReturnCsvFile()
        {
            //mock data and service
            var timesheetEntries = new List<Timesheet>
            {
                new Timesheet { Id = 1, UserName = "Michael S", Date = DateTime.Now, Project = "Project 1", Description = "Task 1", HoursWorked = 8 },
                new Timesheet { Id = 2, UserName = "Michael A", Date = DateTime.Now, Project = "Project 2", Description = "Task 2", HoursWorked = 7 }
            };

            _mockTimesheetService
                .Setup(service => service.GetEntriesAsync())
                .ReturnsAsync(timesheetEntries);

            //call method to download CSV
            var result = await _controller.DownloadCsv();

            //check that the result is a FileResult
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.NotNull(fileResult);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.StartsWith("timesheets_", fileResult.FileDownloadName);
        }

        /// <summary>
        ///check if controller handles the creation of a new entry when valid data is provided
        ///check if the add method is called and the controller returns a success response
        ///ensure that the result is a JsonResult with success = true
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Create_ReturnSuccessForValidEntry()
        {
            //create a valid Timesheet obj
            var timesheetDto = new TimesheetDto
            {
                UserName = "Michael S",
                Date = DateTime.Now,
                Project = "Project 1",
                Description = "Task 1",
                HoursWorked = 8
            };

            //call create action
            var result = await _controller.Create(timesheetDto);

            //check if the result is a JsonResult with success = true
            var jsonResult = Assert.IsType<JsonResult>(result);

            Assert.NotNull(jsonResult.Value);

            var jsonString = JsonConvert.SerializeObject(jsonResult.Value);
            Assert.Contains("\"success\":true", jsonString);
        }
     

    }
}
