using Microsoft.AspNetCore.Identity;

namespace Event_Go.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
