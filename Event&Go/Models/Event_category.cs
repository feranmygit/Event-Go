using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class Event_category
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        [DisplayName("Category Name")]
        public string Category { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [Precision(10, 2)]
        [DisplayName("Price Per Hour")]
        public decimal PricePerHour { get; set; }

        [Precision(10, 2)]
        [DisplayName("Price Per Day")]
        public decimal PricePerDay { get; set; }

        [DisplayName("Category Status")]
        public bool IsActive { get; set; }

        [DisplayName("Maximum Capacity")]
        public int MaxCapacity { get; set; }
    }
}


