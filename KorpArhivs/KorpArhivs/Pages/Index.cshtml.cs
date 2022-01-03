using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KorpArhivs.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty]
        public List<Event> Events { get; set; }

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly ILogger<IndexModel> _logger;

        //public IndexModel(ILogger<IndexModel> logger)
        //{
        //    _logger = logger;
        //}

        public void OnGet()
        {

            var tableName = _configuration["StorageTables:Events"];
            var connectionString = _configuration.GetConnectionString("TableStorage");

            var tableClient = new TableClient(connectionString, tableName);

            var shownEvents = tableClient.Query<TableEntity>();

            Events = new List<Event>();

            foreach (var shownEvent in shownEvents)
            {
                Events.Add(new Event
                {
                    EventName = shownEvent.GetString("Name"),
                    Id = shownEvent.RowKey,
                    EventDate = shownEvent.GetDateTimeOffset("Date").Value,
                    EventDescription = shownEvent.GetString("Description"),
                    Keyword = shownEvent.GetString("Keyword"),
                    EventGroup = shownEvent.GetString("Group"),
                });
            }

        }
        public class Event
        {
            public string EventGroup { get; set; }
            public string Id { get; set; }
            public string EventName { get; set; }
            public System.DateTimeOffset EventDate { get; set; }
            public string EventDescription { get; set; }
            public string Keyword { get; set; }


        }

    }
}
