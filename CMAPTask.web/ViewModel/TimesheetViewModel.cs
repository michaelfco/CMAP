using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.web.ViewModel
{
    public class TimesheetViewModel
    {       
        public int Id { get; set; }      
        public string UserName { get; set; }       
        public DateTime Date { get; set; }       
        public string Project { get; set; }       
        public string Description { get; set; }       
        public int HoursWorked { get; set; }
        public object TotalHoursForDay { get; internal set; }
    }
}
