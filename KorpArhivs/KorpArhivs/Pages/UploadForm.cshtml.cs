using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

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

        //public string Id { get; set; }

        public void OnGet()
        {

        }

        public async Task OnPost()
        {
            //Id= id;

            var tableName = _configuration["StorageTables:Events"];
            var containerName = _configuration["StorageBlobs:Events"];
            var connectionString = _configuration.GetConnectionString("TableStorage");

            var tableClient = new TableClient(connectionString, tableName);

            var guid = Guid.NewGuid().ToString();

            // Make a dictionary entity by defining a <see cref="TableEntity">.
            var entity = new TableEntity(Input.Category, guid)
            {
                { "Name", Input.EventName },
                { "Date", Input.EventDate.ToUniversalTime()},
                { "Description", Input.EventDescription },
                { "Keyword", Input.Keyword },
                { "Group", Input.Category },
                { "EventSubcategory", Input.EventSubcategory }

            };

            tableClient.AddEntity(entity);

            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            foreach (var uploadedFile in Input.Upload)
            {
                using var memoryStream = new MemoryStream();
                uploadedFile.CopyTo(memoryStream);
                memoryStream.Position = 0;

                var fileName = $"{Input.Category}/{guid}/{uploadedFile.FileName}";

                // Get a reference to a blob
                BlobClient blobClient = containerClient.GetBlobClient(fileName);

                await blobClient.UploadAsync(memoryStream, true);
            }

            //return RedirectToPage("/UploadedEvent", new { Input.Category, guid });
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
