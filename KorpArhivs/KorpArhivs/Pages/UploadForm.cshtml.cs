using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;

namespace KorpArhivs.Pages
{
    public class UploadFormModel : PageModel
    {
        [BindProperty]
        public UploadModel Input { get; set; }

        public string[] Category = new[] { "Bildes", "Dokumenti", "Dažādi" };
    
    public void OnGet()
        {
        }

        public void OnPost()
        {

        }

        public class UploadModel 
        {

            [Required]
            [Display(Name = "Notikuma nosaukums")]
            public string EventName { get; set; }

            [Required]
            [Display(Name = "Notikuma datums:")]
            public DateTime EventDate { get; set; }

            [Required]
            [Display(Name = "Notikuma apraksts:")]
            public string EventDescription { get; set; }

            [Required]
            [Display(Name = "Notikuma atslēgas vārds:")]
            public string Keyword { get; set; }

            [Required]
            [Display(Name = "Kategorija:")]
            public string Category { get; set; }

            [Required]
            [Display(Name = "Notikuma apakškategorija:")]
            public string EventSubcategory { get; set; }

            [Required]
            [Display(Name = "Pievienojiet failu:")]
            public IFormFile[] Upload { get; set; }
        }
    }
}
