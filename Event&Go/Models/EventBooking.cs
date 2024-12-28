using System.ComponentModel.DataAnnotations;
using WebApp.Models;

namespace Event_Go.Models
{
    public class EventBooking
    {
        [Key]
        public int BookingId { get; set; }
        public int EventId { get; set; }
        public string UserName { get; set; }
        public DateTime BookingDate { get; set; }
        public Eventstable Event { get; set; } // Navigation property
    }

}
