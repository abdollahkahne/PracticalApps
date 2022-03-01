using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static BlazorWebAssemblySignalRApp.Shared.Models.ComponentEnums;

namespace BlazorWebAssemblySignalRApp.Shared.Models
{
    public class Student
    {
        [Required]
        [MaxLength(10)]
        [Display(Name = "Student Full Name")]
        public string? Title { get; set; }
        [Display(Name = "Student Age")]
        [Required]
        [Range(15, 35, ErrorMessage = "Student Age should be between 15 and 35")]
        [IsOdd(ErrorMessage = "Age should not be odd value!")]
        public int Age { get; set; }
        [Display(Name = "Student Birthdate"), MinimumAgeByDate(20, ErrorMessage = "The birthdaye year must be {0} years ago!")]
        public DateTime Birthdate { get; set; }
        [Display(Name = "Is Married")]
        public bool Married { get; set; }
        [Required]
        [Display(Name = "Conteinent")]
        [EnumDataType(typeof(Continent))]
        public Continent Continent { get; set; }
        [Required]
        [Display(Name = "Student Origin")]
        [EnumDataType(typeof(Country))]
        public Country Country { get; set; }
        [Display(Name = "Comments")]
        public string? Description { get; set; }
        [Required, Display(Name = "Favourite Sport"), EnumDataType(typeof(Sports))]
        public Sports FavouriteSport { get; set; }
        [Required, Display(Name = "Previous Degree"), EnumDataType(typeof(Degree))]
        [Range(type: typeof(Degree), minimum: nameof(Degree.Bachelor), maximum: nameof(Degree.Phd))]
        public Degree PreviousDegree { get; set; }
        [Required, Display(Name = "Interested In"), EnumDataType(typeof(Major))]
        public Major Major { get; set; }

        public ParentInfo ParentInfo { get; set; } = new();
    }
    public enum Country
    {
        [Display(Name = "United States")]
        US,
        [Display(Name = "United Kingdom")]
        UK,
        [Display(Name = "United Arab Emirates")]
        UAE,
        [Display(Name = "Germany")]
        Germany,
        [Display(Name = "France")]
        France,
        [Display(Name = "China")]
        China,
        [Display(Name = "South Korea")]
        Korea
    }
    public enum Continent
    {
        [Display(Name = "Asia")]
        Asia,
        [Display(Name = "Europe")]
        Europe,
        [Display(Name = "Africa")]
        Africa,
        [Display(Name = "North America")]
        NorthAmerica,
        [Display(Name = "South America")]
        SouthAmerica,
        [Display(Name = "Oceanious and Australia")]
        Oceanious
    }

    public class ParentInfo
    {
        [StringLength(10)]
        [Required]
        public string? MotherName { get; set; }
        [StringLength(10)]
        [Required]
        public string? FatherName { get; set; }
        [Range(10, 100)]
        [Required]
        public int MariagedLength { get; set; }
        public bool IsOKay { get; set; } = true;
    }

    // We use this validation attribute like others (Required,Range,MinLength)
    // It has an IsValid method that should be override
    // It return ValidationResult.Success in case of success and a Validation result with error in case of error
    // It has some property which can be used in it for example ErrorMessage
    public class IsOddAttribute : ValidationAttribute
    {

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not null)
            {
                try
                {
                    var residual = System.Convert.ToInt16(value) % 2;
                    if (residual != 1)
                    {
                        return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
                    }
                }
                catch (System.Exception)
                {

                    throw;
                }
            }
            return ValidationResult.Success;
        }
    }
    public class MinimumAgeByDateAttribute : ValidationAttribute
    {
        public int Year { get; }
        public int Age { get; set; }
        public MinimumAgeByDateAttribute(int age)
        {
            Age = age;
            Year = DateTime.Now.Year - age;
            // Year from age
        }

        public string GetErrorMessage() => !string.IsNullOrEmpty(ErrorMessage) ? string.Format(ErrorMessage, Age) : $"Birthday year should not be after {Year}";
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not null)
            {
                try
                {
                    // Use service provider for getting external validator (Just for sample!)
                    var name = ((UserService?)validationContext.GetService(typeof(IUserService)))?.Name;
                    Console.WriteLine(name);
                    // ValidationContext.GetService is null. Injecting services for validation in the IsValid method isn't supported.
                    var year = ((DateTime)value).Year;
                    if (Year < year)
                    {
                        return new ValidationResult(GetErrorMessage(), new[] { validationContext.MemberName! });
                    };
                }
                catch (System.Exception)
                {

                }
            }
            return ValidationResult.Success;
        }
    }
}