using AutoMapper;
using CMAPTask.Domain.Entities;
using CMAPTask.web.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMAPTask.web
{
    public class MappingProfile : Profile
    {
        //map
        public MappingProfile()
        {
            CreateMap<Timesheet, TimesheetViewModel>()
               .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToString("dd/MM/yyyy"))) 
               .ForMember(dest => dest.TotalHoursForDay, opt => opt.Ignore()); 

        }
    }
}
