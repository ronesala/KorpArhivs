using KorpArhivs.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KorpArhivs.Pages
{
    public class UsersModel : PageModel
    {
        private ApplicationDbContext _dbContext;

        [BindProperty]
        public List<AppUser> Users { get; set; }
        public List<SelectListItem> Roles { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public UsersModel(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void OnGet()
        {
            Users = new List<AppUser>();
            var users = _dbContext.Users.ToList();
            foreach (var user in users)
            {
                var userRole = _dbContext.UserRoles.FirstOrDefault(x => x.UserId == user.Id);
                Users.Add(new AppUser
                {
                    Id = user.Id,
                    Name = user.FirstName,
                    Surname = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsConfirmed = user.IsReviewed,
                    Role = userRole?.RoleId

                });
            }

            Roles = new List<SelectListItem>();
            Roles.Add(new SelectListItem
            {
                Text = "Loma nav piešķirta"
            });
            var roles = _dbContext.Roles.ToList();
            foreach (var role in roles)
            {
                Roles.Add(new SelectListItem
                {
                    Value = role.Id,
                    Text = role.Name
                });
            }
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                foreach (var user in Users)
                {
                    var userRole = _dbContext.UserRoles.FirstOrDefault(x => x.UserId == user.Id);
                    if (userRole != null)
                    {
                        if (user.Role == "Loma nav piešķirta")
                        {
                            //if the role is assigned and now it is "Role not assigned", then the role is deleted 
                            _dbContext.UserRoles.Remove(userRole);
                            await _dbContext.SaveChangesAsync();
                        }
                        else
                        {
                            if (userRole.RoleId != user.Role)
                            {
                                //if a role is assigned and now it is a different role, we renew it 
                                _dbContext.UserRoles.Remove(userRole);
                                await _dbContext.SaveChangesAsync();
                                _dbContext.UserRoles.Add(new IdentityUserRole<string>
                                {
                                    RoleId = user.Role,
                                    UserId = user.Id
                                });
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        if (user.Role != "Loma nav piešķirta")
                        {
                            _dbContext.UserRoles.Add(new IdentityUserRole<string>
                            {
                                RoleId = user.Role,
                                UserId = user.Id
                            });
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
                StatusMessage = "Izmaiņas veiksmīgi saglabātas";
            }
            catch
            {
                StatusMessage = "Kļūda saglabājot datus";
            }

            return RedirectToPage();
        }

        public class AppUser
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public bool IsConfirmed { get; set; }
            public string Role { get; set; }
        }
    }
}
