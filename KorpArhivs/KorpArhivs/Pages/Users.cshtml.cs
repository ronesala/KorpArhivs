using KorpArhivs.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace KorpArhivs.Pages
{
    public class UsersModel : PageModel
    {
        private ApplicationDbContext _dbContext;

        [BindProperty]
        public List<AppUser> Users { get; set; }

        public UsersModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void OnGet()
        {
            Users = new List<AppUser>();
            var users = _dbContext.Users;
            foreach (var user in users)
            {
                Users.Add(new AppUser
                {
                    Name = user.FirstName,
                    Surname = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsConfirmed = user.IsReviewed,
                });
            }
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
