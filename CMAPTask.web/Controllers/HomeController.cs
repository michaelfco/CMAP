using AutoMapper;
using CMAPTask.Infrastructure.Services;
using CMAPTask.web.ViewModel;
using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using CMAPTask.Domain.Entities;
using CMAPTask.Application.DTOs;
using CMAPTask.Application.Interfaces;

namespace CMAPTask.web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITimesheetService _service;
        private readonly IMapper _mapper;

        //DP - Dependency Injection
        public HomeController(ITimesheetService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
     
        //SOLID - Single Responsability
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", "Customer");
            ViewData["Title"] = "Timesheet";
            var data = await _service.GetEntriesAsync();  
            //use automapper to map to viewmodel
            var viewModel = _mapper.Map<List<TimesheetViewModel>>(data);         
            return View(viewModel);
        }

        //SOLID - Open/Closed Principle
        public async Task<IActionResult> DownloadCsv()
        {            
            //data to be exported
            var dtExport = await _service.GetEntriesAsync();

            //Group and calculate grouped hours
            var group = dtExport
            .GroupBy(e => new { e.UserName, e.Date })
            .SelectMany(g => g.Select(e => new
            {
                e.UserName,
                Date = e.Date.ToString("dd/MM/yyyy"),
                e.Project,
                e.Description,
                e.HoursWorked,
                TotalHours = g.Sum(x => x.HoursWorked)
            }))
            .ToList();

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));
            csv.WriteRecords(group);
            writer.Flush();
            return File(memoryStream.ToArray(), "text/csv", $"timesheets_{DateTime.Now.ToString("yyyy_MM_dd")}.csv");
        }


        //SOLID - Single Responsibility,Dependency Inversion Principle(depends on ITimesheetService)
        [HttpPost]
        [Route("Home/Create")]
        public async Task<IActionResult> Create(TimesheetDto entry)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors });
            }
            await _service.AddEntryAsync(entry);
            return Json(new { success = true });
        }
    }
}
