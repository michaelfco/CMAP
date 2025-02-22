using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Application.DTOs
{
    public class TimesheetDto
    {
        public string UserName { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Project { get; set; }
        public string Description { get; set; }
        public int HoursWorked { get; set; }
    }
}
