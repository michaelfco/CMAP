using CMAPTask.Application.DTOs;
using CMAPTask.Application.Interfaces;
using CMAPTask.Domain.Entities;
using CMAPTask.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Infrastructure.Services;

public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository _repository;

    //DP - Dependency Injection
    public TimesheetService(ITimesheetRepository repository)
    {
        _repository = repository;
    }

    //SOLID - Single Responsability
    public async Task AddEntryAsync(TimesheetDto dto)
    {
        var entry = new Timesheet
        {
            UserName = dto.UserName,
            Date = dto.Date,
            Project = dto.Project,
            Description = dto.Description,
            HoursWorked = dto.HoursWorked
        };
        //SOLID - Dependency Inversion Principle(depends on ITimesheetRepository)
        await _repository.AddEntryAsync(entry);
    }

    //SOLID - Single Responsability
    public async Task<List<Timesheet>> GetEntriesAsync()
    {
        return await _repository.GetAllEntriesAsync();
    }

   
}

