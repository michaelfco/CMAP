using AutoMapper;
using CMAPTask.Infrastructure.Services;
using CMAPTask.web.ViewModel;
using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using CMAPTask.Domain.Entities;
using CMAPTask.Application.DTOs;

namespace CMAPTask.web.Controllers
{
    public class HomeController : Controller
    {
        private readonly TimesheetService _service;
        private readonly IMapper _mapper;
        public HomeController(TimesheetService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
     
        public async Task<IActionResult> Index()
        {
            var entries = await _service.GetEntriesAsync();

            var totalHoursByDay = entries
         .GroupBy(e => (e.UserName, e.Date))
         .ToDictionary(g => g.Key, g => g.Sum(e => e.HoursWorked));


            var viewModel = _mapper.Map<List<TimesheetViewModel>>(entries);

            foreach (var vm in viewModel)
            {
                var key = (vm.UserName, vm.Date);
                if (totalHoursByDay.TryGetValue(key, out int totalHours))
                {
                    vm.TotalHoursForDay = totalHours;
                }
            }

            return View(viewModel);
        }
      

        [HttpPost]        
        [Route("Home/Create")]
        public async Task<IActionResult> Create(TimesheetDto entry)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
               x => x.Key,
               x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
           );
                return Json(new { success = false, errors = errors });
            }
            await _service.AddEntryAsync(entry);
            return Json(new { success = true });
        }

        public async Task<IActionResult> DownloadCsv()
        {
            var dtExport = await _service.GetEntriesAsync();

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
    }
}
