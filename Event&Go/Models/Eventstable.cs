using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Event_Go.Models;

namespace WebApp.Models
{
    public partial class Eventstable
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Event Name")]
        public string EventName { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [DisplayName("Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [CustomValidation(typeof(Eventstable), nameof(ValidateEndDate))]
        [DisplayName("End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(200)]
        [DisplayName("Venue Address")]
        public string Venue { get; set; }

        [Required]
        [StringLength(200)]
        [DisplayName("Location")]
        public string Location { get; set; }
        
        [Required]
        [DisplayName("Visibility")]
        public string Visibility { get; set; } = "Public";
        
        [DisplayName("Reminders")] 
        public string Reminder { get; set; } = "";

        [Required]
        [CustomValidation(typeof(Eventstable), nameof(ValidateStatus))]
        [DisplayName("Status")]
        public string Status { get; set; }


        [Required]
        [StringLength(100)]
        [DisplayName("Username")]
        public string BookingUserName { get; set; }

        [ValidateNever]
        [DisplayName("Image Name")]
        public string ImageName { get; set; }

        [DisplayName("Organizer Email")]
        public string CreatedBy { get; set; }

        [Required]
        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile ImageFile { get; set; }

        public string CreatedByUserId { get; set; }
        public virtual Event_category Category { get; set; }

        public ICollection<TicketRequest> TicketRequests { get; set; } = new List<TicketRequest>();


        public static ValidationResult ValidateEndDate(DateTime endDate, ValidationContext context)
        {
            var instance = context.ObjectInstance as Eventstable;
            if (instance != null && instance.StartDate > endDate)
            {
                return new ValidationResult("End Date must be later than Start Date.");
            }

            return ValidationResult.Success;
        }


        public static ValidationResult ValidateStatus(string status, ValidationContext context)
        {
            var instance = context.ObjectInstance as Eventstable;
            if (instance != null)
            {
                DateTime today = DateTime.Today;
                if (instance.StartDate == today && status != "Ongoing")
                {
                    return new ValidationResult("For today's start date, the status must be 'Ongoing'.");
                }
                else if (instance.StartDate > today && instance.StartDate <= today.AddDays(3) && status != "Upcoming")
                {
                    return new ValidationResult("For events starting within 3 days, the status must be 'Upcoming'.");
                }
                else if (instance.StartDate > today.AddDays(3) && status != "Completed")
                {
                    return new ValidationResult("For events starting in more than 3 days, the status must be 'Completed'.");
                }
                else if (instance.StartDate < today && status != "Cancelled")
                {
                    return new ValidationResult("For past events, the status must be 'Cancelled'.");
                }
            }
            return ValidationResult.Success;
        }

    }
}
