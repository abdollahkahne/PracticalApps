using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NorthwindIntl.ModelBinders;

namespace NorthwindIntl.Models
{
    public class Book
    {
        [BindNever]
        [HiddenInput]
        [ValidateNever]
        public int Id {get;set;}

        [Required]
        [Display(Name ="Book Title",Description ="Book Title",Prompt ="Please Insert Book Title")]
        public string Title {get;set;}

        [Required]
        [Range(minimum:1,maximum:100,ErrorMessage ="Please Enter a value between 1 and 100")]
        [Display(Name ="Book Price", Description ="Book Price in Euro",Prompt ="Please Insert Price of Book in Euro")]
        public int Price {get;set;}

        [EnumDataType(typeof(Subject))]
        [DisplayFormat(NullDisplayText ="Book Subject is not provided")]
        [Display(Name ="Book Subject",Description ="Book Subject and Content",Prompt ="Please Insert Book Subject")]
        public string Subject {get;set;}

        [ModelBinder(BinderType =typeof(SizeModelBinder),Name ="Size")]
        [Display(Name ="Paper Size",Description ="Print Paper size",Prompt ="Please Insert Book Paper Size")]
        public string Size {get;set;}
        public string Description {get;set;}
    }
    public enum Subject {
        ASP=0,
        Dotnet=1,
        Development=2

    }
    public enum PaperSize {
        A4,
        A5,
    }
}