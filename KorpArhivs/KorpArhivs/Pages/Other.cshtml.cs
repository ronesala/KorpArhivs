using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace KorpArhivs.Pages
{
    public class OtherModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty]
        public List<Other> Various { get; set; }

        public OtherModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {

            var tableName = _configuration["StorageTables:Events"];
            var connectionString = _configuration.GetConnectionString("TableStorage");

            var tableClient = new TableClient(connectionString, tableName);

            var various = tableClient.Query<TableEntity>(filter: $"PartitionKey eq 'Dažādi'");

            Various = new List<Other>();

            foreach (var other in various)
            {
                Various.Add(new Other
                {
                    EventName = other.GetString("Name"),
                    Id = other.RowKey,
                    EventDate = other.GetDateTimeOffset("Date").Value,
                    EventDescription = other.GetString("Description"),
                    Keyword = other.GetString("Keyword"),
                    EventGroup = other.GetString("Group"),
                });
            }

        }
    }

    public class Other
    {
        public string EventGroup { get; set; }
        public string Id { get; set; }
        public string EventName { get; set; }
        public System.DateTimeOffset EventDate { get; set; }
        public string EventDescription { get; set; }
        public string Keyword { get; set; }

    }
}