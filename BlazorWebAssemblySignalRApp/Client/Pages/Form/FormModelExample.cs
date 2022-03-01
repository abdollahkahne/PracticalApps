using System.ComponentModel.DataAnnotations;

namespace BlazorWebAssemblySignalRApp.Client.Pages.Form
{
    public class FormModelExample
    {
        [Required(ErrorMessage = "This field: {0} is required")]
        [StringLength(10, ErrorMessage = "Maximum Length allowed is {0}!")]
        public string? Name { get; set; }

        [Range(minimum: 18, maximum: 82, ErrorMessage = "The value you provided should be between 18 and 82")]
        [Display(Name = "User Age")]
        public int Age { get; set; }
        public Double TotalAverage { get; set; }
        public bool Married { get; set; }
        // [EnumDataType(enumType: typeof(Countries))] // make error in case of Array
        public Countries[] Country { get; set; } = new Countries[] { }; // need initial value in case of multiple
        [Display(Name = "Birth Day")]
        public DateTime? BirthDay { get; set; }
        public string? Address { get; set; }
        public Accessories Accessories { get; set; }
        [Range(typeof(bool), "true", "true", ErrorMessage = "You should agree to continue")]
        public bool IAgree { get; set; }
    }
    public enum Countries
    {
        [Display(Name = "United States")]
        US = 1,
        [Display(Name = "United Kingdom")]
        UK = 2,
        [Display(Name = "United Arab Emirates")]
        UAE = 3,
        [Display(Name = "Germany")]
        Germany = 4,
        [Display(Name = "France")]
        France = 5,
        [Display(Name = "Belgium")]
        Belgium = 6,
        [Display(Name = "Australia")]
        Australia = 7,
    }

    [Flags]
    public enum Accessories
    {
        Book = 1,
        Pen = 2,
        Pencil = 4,
        wiper = 8,
        Other = 16,
    }
}