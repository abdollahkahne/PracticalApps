using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebAPI.ModelBinders
{
    // Mapping between incoming request data and application models is handled by model binders.
    // They bind text-based input from the request directly to model types. for example:
    // An image is base-64 encoded text which then bind to byte[] model
    // When it is possible to do our work with simple TypeConvertor from string to our type it may handled directly at action
    // A simple type is converted from a single string in the input. A complex type is converted from multiple input values. The framework determines the difference (Simple vs Complex Types) based on the existence of a TypeConverter. 
    // To use a model binder we have two choice:
    // 1- use it directly in model using Binding Attribute for example [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids or even add it to Model Definition. In this case we do not need ModelBinderProvider
    // 2- Use IModelBinderProvider and implement its GetBinder method to get the binder related to model type and meta data. We should add the model binder provider to IMVCOptions in startup. The get binder should return an instance of Model Binder using new ArrayModelBinder() or new BinderTypeModelBinder(typeof(ArrayModelBinder)).
    // in BinderTypeModelBinder uses factory pattern and do dependency injection by itself. But in case of new ArrayModelBinder if it have a dependency, we should provide in constructor our self.
    // since order matters in ModelBinderProviders we should add it to the first of list as options.ModelBinderProviders.Insert(0, new AuthorEntityBinderProvider()); to be effective always
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // 1- check that it is used for binding enumerable model like IEnumerable<string> or IEnumerable<Guid>
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();// create a failure binding result. we can similary create success result
                return Task.CompletedTask;
            }
            var modelKey = bindingContext.ModelName;
            var providedValue = bindingContext.ValueProvider.GetValue(modelKey).FirstValue;// using this method we can get all provided value to this model. It can be from route, header,query string, body, form data and every value provider registered
            if (string.IsNullOrEmpty(providedValue))
            {
                bindingContext.Result = ModelBindingResult.Success(null); // if nothing provided as value or it is empty set it null
                return Task.CompletedTask;
            }

            // Get generic type provided to IEnumerable<T>
            var genericType = bindingContext.ModelType.GetGenericArguments().FirstOrDefault(); // It is always none-null
            var convertor = TypeDescriptor.GetConverter(genericType);// Create a convertor to convert string to generic type
            // split the string to an array and convert its string element to generic type
            var providedArray = providedValue.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(str => convertor.ConvertFromString(str.Trim())).ToArray();
            var genericArray = Array.CreateInstance(genericType, providedArray.Length);
            providedArray.CopyTo(genericArray, 0);

            bindingContext.Result = ModelBindingResult.Success(genericArray);
            // Console.WriteLine(bindingContext.Model.ToString());
            return Task.CompletedTask;

        }
    }
}