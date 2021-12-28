using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace KorpArhivs.Pages
{
    public class UsersModel : PageModel
    {
        [BindProperty]
        public List<AppUser> Users { get; set; }

        public void OnGet()
        {
            Users = new List<AppUser>();
            Users.Add(new AppUser
            {
                Name = "Kārlis",
                Surname = "Bērziņš",
                Email = "test@test.com",
                PhoneNumber = "26359017",
                IsConfirmed = true,
            });
        }

        public class AppUser
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public bool IsConfirmed { get; set; }

        }
    }
}
