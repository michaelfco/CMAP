using CMAPTask.Domain.Entities;
using CMAPTask.Domain.Interfaces;
using CMAPTask.Infrastructure.Context;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Infrastructure.Repository;

public class TimesheetRepository: ITimesheetRepository
{
    private readonly CMAPDbContext _context;

    public TimesheetRepository(CMAPDbContext context)
    {
        _context = context;
    }

    public async Task AddEntryAsync(Timesheet entry)
    {
        await _context.Timesheet.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Timesheet>> GetAllEntriesAsync()
    {
        return await _context.Timesheet.ToListAsync();
    }
   
}

