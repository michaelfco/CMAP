using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.Domain.Entities
{
    public class Timesheet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Project { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int HoursWorked { get; set; }
    }
}
