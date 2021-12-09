using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KorpArhivs.Pages
{
    public class UploadedEventModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public UploadedEventModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Category { get; set; }
        public string Id { get; set; }

        [BindProperty]
        public string EventName { get; set; }

        [BindProperty]
        public System.DateTimeOffset EventDate { get; set; }

        [BindProperty]
        public string EventDescription { get; set; }

        [BindProperty]
        public List<string> Keywords { get; set; }

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

            EventName = uploadedEvent.Value.GetString("Name");
            EventDate = uploadedEvent.Value.GetDateTimeOffset("Date").Value;
            EventDescription = uploadedEvent.Value.GetString("Description");
            Keywords = uploadedEvent.Value.GetString("Keyword").Split(';').ToList();

            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

           // Call the listing operation and return pages of the specified size.
            var resultSegment = containerClient.GetBlobsAsync(prefix:$"{category}/{id}").AsPages();

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


    }

}
