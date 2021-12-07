using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace KorpArhivs.Pages
{
    public class DocumentsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty]
        public List<Document> Documents { get; set; }

        public DocumentsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {

            var tableName = _configuration["StorageTables:Events"];
            var connectionString = _configuration.GetConnectionString("TableStorage");

            var tableClient = new TableClient(connectionString, tableName);

            var documents = tableClient.Query<TableEntity>(filter: $"PartitionKey eq 'Dokumenti'");

            Documents = new List<Document>();

            foreach (var document in documents)
            {
                Documents.Add(new Document
                {
                    EventName = document.GetString("Name"),
                    EventDate = document.GetDateTimeOffset("Date").Value,
                    EventDescription = document.GetString("Description"),
                    Keyword = document.GetString("Keyword"),
                    EventGroup = document.GetString("Group"),
                });
            }

        }
    }

    public class Document
    {
        public string EventName { get; set; }
        public System.DateTimeOffset EventDate { get; set; }
        public string EventDescription { get; set; }
        public string Keyword { get; set; }
        public string EventGroup { get; set; }

    }
}