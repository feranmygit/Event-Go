using System.ComponentModel.DataAnnotations;
using WebApp.Models;

namespace Event_Go.Models
{
    public class TicketRequest
    {
        [Key]
        public int TicketId { get; set; }
        public int EventId { get; set; }
        public string UserName { get; set; } // User requesting the ticket
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // "Pending", "Accepted", "Rejected"
        public string UniqueCode { get; set; } // Unique code for the ticket

        public TicketRequest()
        {
            UniqueCode = ""; // Explicitly set the default value in the constructor
        }

        public Eventstable Event { get; set; } // Navigation property
    }

}
