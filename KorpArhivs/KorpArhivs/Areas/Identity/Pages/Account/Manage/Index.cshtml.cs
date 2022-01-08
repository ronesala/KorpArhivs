using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KorpArhivs.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KorpArhivs.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        [Display(Name = "E-pasts")]
        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vārda lauks ir obligāti jāaizpilda")]
            [Display(Name = "Vārds")]
            [StringLength(30, ErrorMessage = "Vārds nedrīkst būt garāks par 30 simboliem")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Uzvārda lauks ir obligāti jāaizpilda")]
            [Display(Name = "Uzvārds")]
            [StringLength(50, ErrorMessage = "Uzvārds nedrīkst būt garāks par 50 simboliem")]
            public string LastName { get; set; }


            [Phone]
            [Display(Name = "Tālrunis")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Nevar parādīt lietotāju ar ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Nevar parādīt lietotāju ar ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (user.FirstName != Input.FirstName || user.LastName != Input.LastName)
            {
                var dbUser = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);
                dbUser.FirstName = Input.FirstName;
                dbUser.LastName = Input.LastName;
                _dbContext.SaveChanges();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Negaidīta kļūda pievienojot tālruni.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Jūsu profils ir atjaunots";
            return RedirectToPage();
        }
    }
}
