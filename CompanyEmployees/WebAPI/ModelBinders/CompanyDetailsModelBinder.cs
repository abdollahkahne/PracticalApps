using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebAPI.ModelBinders
{
    // Consider we have a complex type defined that can not directly convert from string
    // The binder should Converts incoming request data into strongly typed key arguments (only a key provided by value providers and we should bind this to a complex type).
    // we can add Model Binding attribute to its definition as below as we can add it in controller action if we want too. But adding it here act globally and do not need modelBindeingProvider either
    // except the followings, You can apply the ModelBinder attribute to individual model properties (such as on a viewmodel) or to action method parameters to specify a certain model binder or model name for just that type or action.
    // Another use case of ModelBinder attribute is where we want to use different name for parametter and model name. for example we have a parametter name Id in route but we need companyId we can add befor the action argument the following:
    // [ModelBinder(Name ="id")] CompanyDetails companyId ** apparantly this does not work for some binder use like this for similar requirement 
    // [FromQuery(Name = "id")] Guid companyId
    [ModelBinder(BinderType = typeof(CompanyDetailsModelBinder))]
    public class CompanyDetails
    {
        public Guid CompanyId { get; set; }
        public int EmployeesCount { get; set; }
        public float AverageAge { get; set; }
    }
    public class CompanyDetailsModelBinder : IModelBinder
    {
        private readonly IRepositoryManager _repository;
        public CompanyDetailsModelBinder(IRepositoryManager repository)
        {
            _repository = repository;
        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // if model type is different than Company Detail it should fail
            if (bindingContext.ModelType != typeof(CompanyDetails))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
            var providedValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, providedValue);
            var value = providedValue.FirstValue;
            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }
            var companyId = Guid.Parse(value);
            var employees = _repository.Employee.GetEmployees(companyId, false).AsEnumerable();
            var averageAge = 0;
            var model = new CompanyDetails
            {
                CompanyId = companyId,
                AverageAge = averageAge,
                EmployeesCount = employees.Count()
            };
            bindingContext.Result = ModelBindingResult.Success(model); // This do not add any thing to model state (added here is model). we can do it separately here

            return Task.CompletedTask;
        }
    }
}