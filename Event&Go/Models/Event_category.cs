using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Event_category
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string Category { get; set; }

        public string Description { get; set; }

        [Precision(10, 2)]
        public decimal PricePerHour { get; set; }

        [Precision(10, 2)]
        public decimal PricePerDay { get; set; }

        public bool IsActive { get; set; }

        public int MaxCapacity { get; set; }
    }
}


