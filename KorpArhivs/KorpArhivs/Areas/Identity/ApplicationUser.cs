using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KorpArhivs.Areas.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(30)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        public bool IsReviewed { get; set; }
    }
}
