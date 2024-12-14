using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApp.Models
{
    public partial class Eventstable
    {
        [ValidateNever]
        [Key]
        public int EventId { get; set; }
        [Required]
        [StringLength(255)]
        public string? EventName { get; set; }
        [Required]
        [StringLength(255)]
        public string Description { get; set; }
        [Required]
        public int CategoryId { get; set; } 
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        [StringLength(200)]
        public string? Venue { get; set; }
        [Required]
        [StringLength(200)]
        public string? Location { get; set; }
        //[Required]
        [StringLength(200)]
        public string? FromEmailAddress { get; set; }
        //[Required]
        [StringLength(200)]
        public string? ToEmailAddress { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        [StringLength(100)]
        public string? BookingUserName { get; set; }
        [ValidateNever]
        [Column(TypeName = "nvarchar(100)")]
        [DisplayName("Image Name")]
        public string ImageName { get; set; }
        [Required]
        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile ImageFile { get; set; }    

        // Navigation Property
        public virtual Event_category? Category { get; set; }
    }
}
