using CMAPTask.Application.DTOs;
using CMAPTask.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Application.Interfaces
{
    public interface ITimesheetService
    {
        Task AddEntryAsync(TimesheetDto dto);
        Task<List<Timesheet>> GetEntriesAsync();
    }
}
