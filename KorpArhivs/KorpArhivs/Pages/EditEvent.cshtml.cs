﻿using Azure;
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
        public List<string> UploadedFiles { get; set; }

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

            UploadedFiles = new List<string>();

            // Enumerate the blobs returned for each page.
            await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);
                    UploadedFiles.Add(blobClient.Uri.ToString());
                }

            }
        }

        public async Task <IActionResult> OnPost()
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

            //foreach (var uploadedFile in Update.Upload)
            //{
            //    using var memoryStream = new MemoryStream();
            //    uploadedFile.CopyTo(memoryStream);
            //    memoryStream.Position = 0;

            //    var fileName = $"{Category}/{Id}/{uploadedFile.FileName}";

            //    // Get a reference to a blob
            //    BlobClient blobClient = containerClient.GetBlobClient(fileName);

            //    await blobClient.UploadAsync(memoryStream, true);
            //}

            return RedirectToPage("/UploadedEvent", new {category = Category, id = Id});
        }

        public class EditingModel
        {

            [Required]
            [Display(Name = "Notikuma nosaukums")]
            public string EventName { get; set; }

            [Required]
            [Display(Name = "Notikuma datums:")]
            public DateTimeOffset EventDate { get; set; }

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
