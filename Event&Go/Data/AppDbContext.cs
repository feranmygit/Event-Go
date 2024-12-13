using Event_Go.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace Event_Go.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public virtual DbSet<Event_category> Event_categories { get; set; }
        public virtual DbSet<Eventstable> Eventstables { get; set; }

        public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

    }
}
