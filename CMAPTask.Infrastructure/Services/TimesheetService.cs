using CMAPTask.Application.DTOs;
using CMAPTask.Domain.Entities;
using CMAPTask.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Infrastructure.Services;

public class TimesheetService
{
    private readonly ITimesheetRepository _repository;

    public TimesheetService(ITimesheetRepository repository)
    {
        _repository = repository;
    }

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

        await _repository.AddEntryAsync(entry);
    }

    public async Task<List<Timesheet>> GetEntriesAsync()
    {
        return await _repository.GetAllEntriesAsync();
    }

   
}

