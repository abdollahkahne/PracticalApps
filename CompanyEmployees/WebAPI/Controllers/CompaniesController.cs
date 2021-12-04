using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Repository;
using WebAPI.ModelBinders;

namespace WebAPI.Controllers
{
    [ApiController] // this makes to FromBody source applied automaticly
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;

        public CompaniesController(ILoggerManager logger, IRepositoryManager repository, IMapper mapper)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _repository.Company.GetAll(false).ProjectTo<CompanyDto>(_mapper.ConfigurationProvider).ToListAsync();
            // var companiesDTO = companies.Select(c => new CompanyDto
            // {
            //     // Id = c.Id,
            //     // Name = c.Name,
            //     FullAddress = $"{c.Address} {c.Country}"
            // }).AsEnumerable();
            // var companiesDTO = _mapper.Map<IQueryable<CompanyDto>>(companies).AsEnumerable(); // This does not work apparantly so we should first run the query!
            // var companiesDTO = _mapper.ProjectTo<IQueryable<CompanyDto>>(companies).AsEnumerable();// ; // This does not work apparantly so we should first run the query!
            // var companiesDTO = _mapper.Map<IQueryable<Company>, IEnumerable<CompanyDto>>(companies); // Altough this work but I am not sure that works performant. we need to see the query that sends to database
            // var companiesDTO = _mapper.Map<IEnumerable<CompanyDto>>(companies.AsEnumerable());
            return Ok(companies); // The Ok method is equal to new OKResult();

        }

        [HttpGet("{id}", Name = "GetById")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            var company = await _repository.Company.GetByIdAsync(id, false);
            if (company == null)
            {
                _logger.LogInfo($"The company with Id:{id} does not exist in database");
                return NotFound();
            }
            var companyDto = _mapper.Map<CompanyDto>(company);
            return Ok(companyDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany(CompanyForCreationDto company)
        {
            // ****IMPORTANT****
            //By default it Only Accept the company from body and not form since it has apiController atrribute!
            // If this has not api-controller attribute it bind the data by default from form, 
            // We should specify binding source explicitly if we want other cases
            // In case of form we get the unsupported Media Types (415) error code
            if (company == null)
            {
                _logger.LogError("CompanyForCreationDto object sent from client is null.");
                ModelState.AddModelError("", "Company can not be null");
            }
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<Company>(company);
                _repository.Company.CreateCompany(entity);
                await _repository.SaveAsync();// We should call this to apply the change to database!
                var created = _mapper.Map<CompanyDto>(entity);// we should not show the total created entity to users!
                return CreatedAtRoute("GetById", new { id = created.Id }, created);
            }
            return BadRequest(ModelState);
        }

        [HttpGet("collection/{ids}", Name = "GetCompaniesCollection")]
        public async Task<IActionResult> GetCompaniesCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                _logger.LogError("Parameter Ids is null");
                return BadRequest();
            }
            var companies = await _repository.Company.GetCompaniesCollection(ids, false)
                .ProjectTo<CompanyDto>(_mapper.ConfigurationProvider).ToListAsync();
            return Ok(companies);
        }
        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanies(IEnumerable<CompanyForCreationDto> collection)
        {
            if (ModelState.IsValid)
            {
                // var createdCompanies = new List<Company>();
                // foreach (var item in collection)
                // {
                //     var company = _mapper.Map<Company>(item);// since we need the company Ids which determined by EF, It is better to Map collection or make a collection from tracked companies!
                //     createdCompanies.Add(company);
                //     _repository.Company.CreateCompany(company);
                // }

                // This is cleaner but the above is also OKay
                var companiesCollection = _mapper.Map<IEnumerable<Company>>(collection);
                foreach (var company in companiesCollection)
                {
                    _repository.Company.CreateCompany(company);
                }
                await _repository.SaveAsync();

                var companies = _mapper.Map<IEnumerable<CompanyDto>>(companiesCollection);
                var ids = String.Join(",", companiesCollection.Select(c => c.Id.ToString()));
                return CreatedAtRoute("GetCompaniesCollection", new { ids }, companies);
            }
            return BadRequest(ModelState);
        }

        // This action is only created for Model Binder Learning and should be commented
        [HttpGet("details/{id}")]
        public IActionResult GetCompanyDetails(CompanyDetails id)
        {
            if (ModelState.IsValid)
            {

            }
            if (id == null)
            {
                return BadRequest();
            }
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            // check its existance
            var company = await _repository.Company.GetByIdAsync(id, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with Id {id} does not exist in database!");
                return NotFound();
            }
            _repository.Company.DeleteCompany(company);
            await _repository.SaveAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(Guid id, CompanyForUpdateDto input)
        {
            if (ModelState.IsValid)
            {
                var company = await _repository.Company.GetByIdAsync(id, true);
                if (company == null)
                {
                    _logger.LogInfo($"The company with Id {id} does not exist in database");
                    return NotFound();
                }
                _mapper.Map(input, company);
                await _repository.SaveAsync();
                var output = _mapper.Map<CompanyDto>(company);
                return Ok(output);
            }
            return BadRequest(ModelState);

        }
    }
}