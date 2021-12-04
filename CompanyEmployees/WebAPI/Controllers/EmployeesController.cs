using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers
{
    [ApiController] // This do at least two things 1- Bind from body has the most preference but in MVC it is for form 2- Model State checking done automaticly for this. We can set off these in service configure method
    [Route("api/companies/{companyId}/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IRepositoryManager _repository;

        public EmployeesController(ILoggerManager logger, IMapper mapper, IRepositoryManager repository)
        {
            _logger = logger;
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<IActionResult> GetEmployeesFor(Guid companyId)
        {
            // check company existance
            var company = await _repository.Company.GetByIdAsync(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with Id: {companyId} does'nt exist in database");
                return NotFound();
            }

            var employees = await _repository.Employee
            .GetEmployees(companyId, false)
            .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
            return Ok(employees);
        }

        [HttpGet("{id}", Name = "GetEmployeeById")]
        public async Task<IActionResult> GetEmployeeFor(Guid companyId, Guid id)
        {
            // the company checking step is more user freindly but not add any value since we check it in GetEmployee implicitly
            var company = await _repository.Company.GetByIdAsync(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"The company with CompanyId:{companyId} does not exist ");
                return NotFound();
            }
            var employee = await _repository.Employee.GetEmployeeAsync(companyId, id, false);
            if (employee == null)
            {
                _logger.LogInfo($"The Company with {companyId} does not have an Employee with EmployeeId: {id}");
                return NotFound();
            }
            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return Ok(employeeDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployeeFor(Guid companyId, EmployeeForCreationDto data)
        {
            if (data == null)
            {
                _logger.LogError("The employee could not be empty");
                ModelState.AddModelError("", "The employee could not be null");
            }
            // check existence of company
            var company = await _repository.Company.GetByIdAsync(companyId, false);
            if (company == null)
            {
                _logger.LogError($"There is not a company with Id:{companyId} in database");
                ModelState.AddModelError("", $"There is not a company with Id:{companyId} in database");
            }
            if (ModelState.IsValid)
            {
                var employee = _mapper.Map<Employee>(data);
                _repository.Employee.CreateEmployeeFor(companyId, employee);
                await _repository.SaveAsync();
                var result = _mapper.Map<EmployeeDto>(employee);
                // return Ok(result);// This is not precise. we should return CreatedAt Result
                return CreatedAtRoute("GetEmployeeById", new { companyId, id = employee.Id }, result);
            }
            return BadRequest(ModelState);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeFor(Guid companyId, [FromRoute(Name = "id")] Guid employeeId)
        {
            // Check Company Existence
            var company = await _repository.Company.GetByIdAsync(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"There is not any company with {companyId} in our database");
                // ModelState.AddModelError("", $"There is not any company with {companyId} in our database");
                return NotFound();
            }
            var employee = await _repository.Employee.GetEmployeeAsync(companyId, employeeId, false);
            if (employee == null)
            {
                _logger.LogInfo($"There is not an employee with {employeeId} in company with Id {companyId}");
                return NotFound();
            }
            _repository.Employee.DeleteEmployee(employee);
            await _repository.SaveAsync(); // This should call on all unsafe reuqests
            // return Ok();// This is not convinient
            return NoContent(); // 204

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid companyId, [FromRoute(Name = "id")] Guid employeeId, EmployeeForUpdateDto input)
        {
            if (ModelState.IsValid)
            {
                // Check Compony existance
                var company = await _repository.Company.GetByIdAsync(companyId, false);
                if (company == null)
                {
                    _logger.LogInfo($"The company with Id {companyId} does not exist in database");
                    return NotFound();
                }
                var employee = await _repository.Employee.GetEmployeeAsync(companyId, employeeId, true); // Return Employee while change tracking is enabled
                if (employee == null)
                {
                    _logger.LogInfo($"The employee with Id {employeeId} does not exist in database");
                    return NotFound();
                }
                _mapper.Map(input, employee); // what happened to other fields which is not in input here?  They remain un-changed since we used variable
                await _repository.SaveAsync();
                // we can return 204 or 200 in case of update. Here we return 200 to show the resulted employee dto to user
                var output = _mapper.Map<Employee, EmployeeDto>(employee);
                return Ok(output);
            }
            return BadRequest(ModelState);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateEmployee(Guid companyId, [FromRoute(Name = "id")] Guid employeeId, JsonPatchDocument<EmployeeForUpdateDto> input)
        {
            if (input == null)
            {
                _logger.LogError($"The Patch body is null for request {HttpContext.TraceIdentifier}");
                return BadRequest("Patch reuest body is null");
            }
            if (ModelState.IsValid) // Here since our model is JsonPatchDocument which have not any validation, it is always valid but when can do the validation on our interesting model (EmployeeForUpdateDto) when it is created (for example by ApplyTo From json document) using TryValidateModel Method from controller
            {
                // check company is not null
                var company = await _repository.Company.GetByIdAsync(companyId, false);
                if (company == null)
                {
                    _logger.LogInfo($"{HttpContext.TraceIdentifier}: the company with Id {companyId} does not exist in database");
                    return NotFound();
                }

                var employee = await _repository.Employee.GetEmployeeAsync(companyId, employeeId, true);
                if (employee == null)
                {
                    _logger.LogInfo($"{HttpContext.TraceIdentifier}: the employee with Id {employeeId} for Company with Id {companyId} does not exist in database");
                    return NotFound();
                }

                // Here we want to make a JsonPatchDocument with data from both source (entity which returned from db and input which sent by user)
                var entityPart = _mapper.Map<EmployeeForUpdateDto>(employee);
                // In the following we have two (plus one!) type of validation
                // 0- Validation of JsonPatchDocument according to Rest Standard (for example it should be an array! its element have standard shape). This Validation done after binding model and do not happen now
                // 1- Validation of JsonPatchDocument according to our Entity Type
                // 2- Validation of Resulted model from the requested patch
                input.ApplyTo(entityPart, ModelState); // we enforce patches requested by user to entitypart which has a reverse mapping to employee so we should apply the change to employee by that
                TryValidateModel(entityPart); // Here we have a model of interest and an instance from it (EmployeeForUpdateDto) which we defined some validation on it. so we validate the instance against them and the model state update
                // since Model State changed we should recheck it here
                if (ModelState.IsValid)
                {
                    _mapper.Map(entityPart, employee); // we do the object mapping to keep unmapped field  and importantly have the ef fields like Chnage State untouched!
                    await _repository.SaveAsync();
                    //return changed employee to user
                    var output = _mapper.Map<EmployeeDto>(employee);
                    return Ok(output);
                }

            }
            return BadRequest(ModelState);
        }
    }
}