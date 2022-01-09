using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KorpArhivs.Pages
{
    public class EditEventModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty]
        public EditingModel Update { get; set; }

        public EditEventModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public string Category { get; set; }
        [BindProperty]
        public string Id { get; set; }

        [BindProperty]
        public List<UploadedFile> UploadedFiles { get; set; }

        public async Task OnGet(string category, string id)
        {
            Category = category;
            Id = id;

            var tableName = _configuration["StorageTables:Events"];
            var containerName = _configuration["StorageBlobs:Events"];
            var connectionString = _configuration.GetConnectionString("TableStorage");

            var tableClient = new TableClient(connectionString, tableName);

            var uploadedEvent = tableClient.GetEntity<TableEntity>(Category, Id);

            Update = new EditingModel();

            Update.EventName = uploadedEvent.Value.GetString("Name");
            Update.EventDate = uploadedEvent.Value.GetDateTimeOffset("Date").Value;
            Update.EventDescription = uploadedEvent.Value.GetString("Description");
            Update.Keyword = uploadedEvent.Value.GetString("Keyword");
            Update.EventSubcategory = uploadedEvent.Value.GetString("EventSubcategory");

            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Call the listing operation and return pages of the specified size.
            var resultSegment = containerClient.GetBlobsAsync(prefix: $"{category}/{id}").AsPages();

            UploadedFiles = new List<UploadedFile>();

            // Enumerate the blobs returned for each page.
            await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);
                    var file = new UploadedFile();
                    file.Uri = blobClient.Uri.ToString();
                    file.BlobName = blobItem.Name;
                    UploadedFiles.Add(file);
                }

            }
        }

        public async Task<IActionResult> OnPost()
        {
            var tableName = _configuration["StorageTables:Events"];
            var containerName = _configuration["StorageBlobs:Events"];
            var connectionString = _configuration.GetConnectionString("TableStorage");

            var tableClient = new TableClient(connectionString, tableName);

            var entity = new TableEntity(Category, Id)
            {
                { "Name", Update.EventName },
                { "Date", Update.EventDate.ToUniversalTime()},
                { "Description", Update.EventDescription },
                { "Keyword", Update.Keyword },
                { "EventSubcategory", Update.EventSubcategory }

            };

            tableClient.UpdateEntity(entity, ETag.All);

            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            if (Update.Upload != null)
            {
                foreach (var uploadedFile in Update.Upload)
                {
                    using var memoryStream = new MemoryStream();
                    uploadedFile.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    var fileName = $"{Category}/{Id}/{uploadedFile.FileName}";

                    // Get a reference to a blob
                    BlobClient blobClient = containerClient.GetBlobClient(fileName);

                    await blobClient.UploadAsync(memoryStream, true);
                }
            }


            var filesToDelete = UploadedFiles.Where(x => x.IsMarkedForDeletion);

            if (filesToDelete.Any())
            {
                foreach (var file in filesToDelete)
                {
                    BlobClient blobClient = containerClient.GetBlobClient(file.BlobName);
                    await blobClient.DeleteAsync();
                }
            }

            return RedirectToPage("/UploadedEvent", new { category = Category, id = Id });
        }

        public class EditingModel
        {

            [Required(ErrorMessage = "Nosaukuma lauks ir obligāti jāaizpilda")]
            [Display(Name = "Notikuma nosaukums")]
            [StringLength(255, ErrorMessage = "Notikums nedrīkst būt garāks par 255 simboliem")]
            public string EventName { get; set; }

            [Required]
            [Display(Name = "Notikuma datums:")]
            public DateTimeOffset EventDate { get; set; }

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

            [Required]
            [Display(Name = "Pievienojiet failu:")]
            public IFormFile[] Upload { get; set; }
        }

        public class UploadedFile
        {
            public string Uri { get; set; }
            public bool IsMarkedForDeletion { get; set; }
            public string BlobName { get; set; }
        }
    }
}