using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KorpArhivs.Pages
{
    public class DeleteEventConfirmationModel : PageModel
    {
        private readonly IConfiguration _configuration;

        private BlobContainerClient _blobContainerClient;

        private TableClient _tableClient;

        private string _connectionString;

        public DeleteEventConfirmationModel(IConfiguration configuration)
        {
            _configuration = configuration;
            var containerName = _configuration["StorageBlobs:Events"];
            _connectionString = _configuration.GetConnectionString("TableStorage");
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var tableName = _configuration["StorageTables:Events"];
            _tableClient = new TableClient(_connectionString, tableName);

        }

        [BindProperty]
        public string EventName { get; set; }

        [BindProperty]
        public string EventDescription { get; set; }

        [BindProperty]
        public int FileCount { get; set; }

        public async Task OnGet()
        {
            var category = (string)RouteData.Values["category"];
            var id = (string)RouteData.Values["id"];

            var deleteEventConfirmationModel = _tableClient.GetEntity<TableEntity>(category, id);

            EventName = deleteEventConfirmationModel.Value.GetString("Name");
            EventDescription = deleteEventConfirmationModel.Value.GetString("Description");

            FileCount = (await GetAllBlobs(category, id)).Count;
        }

        //Delete the entire event 
        public async Task<IActionResult> OnPost()
        {
            var category = (string)RouteData.Values["category"];
            var id = (string)RouteData.Values["id"];

            await _tableClient.DeleteEntityAsync(category, id);

            var uploadedFiles = await GetAllBlobs(category, id);

            foreach (var file in uploadedFiles)
            {
                BlobClient blobClient = _blobContainerClient.GetBlobClient(file);
                await blobClient.DeleteAsync();
            }

            return RedirectToPage("/Index");
        }

        private async Task<List<string>> GetAllBlobs(string category, string id)
        {
            var result = new List<string>();
            // Call the listing operation and return pages of the specified size.
            var resultSegment = _blobContainerClient.GetBlobsAsync(prefix: $"{category}/{id}").AsPages();

            // Enumerate the blobs returned for each page.
            await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    result.Add(blobItem.Name);
                }
            }
            return result;
        }
    }
}
