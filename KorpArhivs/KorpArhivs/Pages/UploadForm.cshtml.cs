using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Editor,Administrator")]
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

        public async Task<IActionResult> OnPost()
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

            return RedirectToPage("/UploadedEvent", new { category = Input.Category, id = guid });
        }

        public class UploadModel
        {

            [Required(ErrorMessage = "Nosaukuma lauks ir obligāti jāaizpilda")]
            [Display(Name = "Notikuma nosaukums")]
            [StringLength(255,ErrorMessage = "Notikums nedrīkst būt garāks par 255 simboliem")]
            public string EventName { get; set; }

            [Required(ErrorMessage = "Datuma lauks ir obligāti jāaizpilda")]
            [Display(Name = "Notikuma datums:")]
            public DateTime EventDate { get; set; }

            [Required(ErrorMessage = "Apraksta lauks ir obligāti jāaizpilda")]
            [Display(Name = "Notikuma apraksts:")]
            [StringLength(4000, ErrorMessage = "Notikuma apraksts nedrīkst būt garāks par 4000 simboliem")]
            public string EventDescription { get; set; }

            [Required(ErrorMessage = "Notikuma atslēgas vārds ir obligāti jāaizpilda")]
            [Display(Name = "Notikuma atslēgas vārds:")]
            [StringLength(255, ErrorMessage = "Notikuma atslēgas vārds nedrīkst būt garāks par 255 simboliem")]
            public string Keyword { get; set; }

            [Required(ErrorMessage = "Notikuma Kategorija")]
            [Display(Name = "Kategorija:")]
            public string Category { get; set; }

            [Required(ErrorMessage = "Notikuma apakškategorija ir obligāti jāaizpilda")]
            [Display(Name = "Notikuma apakškategorija:")]
            [StringLength(255, ErrorMessage = "Notikuma apakškategorija nedrīkst būt garāka par 255 simboliem")]
            public string EventSubcategory { get; set; }

            [Required(ErrorMessage = "Jāpievieno vismaz viens fails")]
            [Display(Name = "Pievienojiet failu:")]
            public IFormFile[] Upload { get; set; }
        }
    }
}
