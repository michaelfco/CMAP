using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Application.DTOs
{
    public class TimesheetDto
    {
        [Required(ErrorMessage = "User Name is required.")]
        public string UserName { get; set; }
        public DateTime Date { get; set; } 
        public string Project { get; set; }
        public string Description { get; set; }
        public int HoursWorked { get; set; }
    }
}
