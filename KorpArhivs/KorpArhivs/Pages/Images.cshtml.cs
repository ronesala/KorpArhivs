using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KorpArhivs.Pages
{
    public class ImagesModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty]
        public List<Gallery> Galleries { get; set; }

        public ImagesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGet()
        {

            var tableName = _configuration["StorageTables:Events"];
            var containerName = _configuration["StorageBlobs:Events"];
            var connectionString = _configuration.GetConnectionString("TableStorage");

            // Create a BlobServiceClient object which will be used to create a container client
            var blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container and return a container client object
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var tableClient = new TableClient(connectionString, tableName);

            var galleries = tableClient.Query<TableEntity>(filter: $"PartitionKey eq 'Bildes'");

            Galleries = new List<Gallery>();

            foreach (var gallery in galleries)
            {
                Galleries.Add(new Gallery
                {
                    EventName = gallery.GetString("Name"),
                    Id = gallery.RowKey,
                    EventDate = gallery.GetDateTimeOffset("Date").Value,
                    EventDescription = gallery.GetString("Description"),
                    Keyword = gallery.GetString("Keyword"),
                    EventGroup = gallery.GetString("Group"),
                    FirstImageUri = await GetFirstImageUri(containerClient, gallery)
                });
            }


        }

        private async Task<string> GetFirstImageUri(BlobContainerClient containerClient, TableEntity gallery)
        {
            var result = string.Empty;

            // Call the listing operation and return pages of the specified size.
            var resultSegment = containerClient.GetBlobsAsync(prefix: $"{gallery.GetString("Group")}/{gallery.RowKey}").AsPages();

            // Enumerate the blobs returned for each page.
            await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);
                    result = blobClient.Uri.ToString();
                    break;
                }
                break;
            }

            return result;
        }

    }

    public class Gallery
    {
        public string EventGroup { get; set; }
        public string Id { get; set; }
        public string EventName { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public string EventDescription { get; set; }
        public string Keyword { get; set; }
        public string FirstImageUri { get; set; }
    }
}