using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

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

        public void OnGet()
        {
            //Nesapratu kaa pareizi pievienot...

            var tableName = _configuration["StorageTables:Galleries"];
            var connectionString = _configuration.GetConnectionString("TableStorage");

            var tableClient = new TableClient(connectionString, tableName);

            var galleries = tableClient.Query<TableEntity>();

            Galleries = new List<Gallery>();

            foreach (var gallery in galleries)
            {
                Galleries.Add(new Gallery
                {
                    EventName = gallery.GetString("Name"),
                    EventDate = gallery.GetDateTimeOffset("Date").Value,
                    EventDescription = gallery.GetString("Description"),
                    Keyword = gallery.GetString("Keyword"),
                    EventGroup = gallery.GetString("Group"),
                });
            }

            //Events = new List<Event>
            //{
            //new Event
            //{
            //    EventName = "Pirmais notikums",
            //    EventDate = new DateTime(2020, 08, 13),
            //    EventDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            //    Keyword = "",
            //    EventGroup = "Bildes"
            //},
            //new Event
            //{
            //    EventName = "Otrais notikums",
            //    EventDate = new DateTime(2000, 07, 03),
            //    EventDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            //    Keyword = "",
            //    EventGroup = "Bildes"
            //},
            //new Event
            //{
            //    EventName = "Tresais notikums",
            //    EventDate = new DateTime(2018, 10, 20),
            //    EventDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            //    Keyword = "",
            //    EventGroup = "Bildes"
            //},
            //new Event
            //{
            //    EventName = "Ceturtais notikums",
            //    EventDate = new DateTime(2001, 02, 28),
            //    EventDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            //    Keyword = "",
            //    EventGroup = "Bildes"
            //}

            //};

        }
    }

    public class Gallery
    {
        public string EventName { get; set; }
        public System.DateTimeOffset EventDate { get; set; }
        public string EventDescription { get; set; }
        public string Keyword { get; set; }
        public string EventGroup { get; set; }

    }
}