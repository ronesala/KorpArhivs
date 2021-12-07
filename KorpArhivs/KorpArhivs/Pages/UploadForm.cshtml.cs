using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;

namespace KorpArhivs.Pages
{
    public class UploadFormModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty]
        public UploadModel Input { get; set; }

        public string[] Category = new[] { "Bildes", "Dokumenti", "Dažādi" };

        public UploadFormModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public void OnGet()
        {

        }

        public void OnPost()
        {
            var tableName = _configuration["StorageTables:Events"];
            var connectionString = _configuration.GetConnectionString("TableStorage");

            var tableClient = new TableClient(connectionString, tableName);

            // Make a dictionary entity by defining a <see cref="TableEntity">.
            var entity = new TableEntity(Input.Category, Guid.NewGuid().ToString())
            {
                { "Name", Input.EventName },
                { "Date", Input.EventDate.ToUniversalTime()},
                { "Description", Input.EventDescription },
                { "Keyword", Input.Keyword },
                { "Group", Input.Category },
                { "EventSubcategory", Input.EventSubcategory }

            };

            tableClient.AddEntity(entity);

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
