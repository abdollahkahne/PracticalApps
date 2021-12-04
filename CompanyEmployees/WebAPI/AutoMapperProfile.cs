using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Entities.DataTransferObjects;
using Entities.Models;

namespace WebAPI
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Company, CompanyDto>()
            .ForMember(company => company.FullAddress, memberOptions => memberOptions.MapFrom(c => $"{c.Address} {c.Country}"));
            CreateMap<Employee, EmployeeDto>();
            CreateMap<CompanyForCreationDto, Company>();
            CreateMap<EmployeeForCreationDto, Employee>();
            CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();
            CreateMap<EmployeeForCreationUpdateDto, Employee>();
            CreateMap<CompanyForUpdateDto, Company>();
        }
    }
}