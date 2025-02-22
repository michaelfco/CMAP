using CMAPTask.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Domain.Interfaces;

public interface ITimesheetRepository
{
    Task AddEntryAsync(Timesheet entry);
    Task<List<Timesheet>> GetAllEntriesAsync();
}
