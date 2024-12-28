namespace Event_Go.Data
{
    public class NotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        // Get the count of unread notifications
        //public async Task<int> GetUnreadNotificationCount(string userName)
        //{
        //    return await _context.TicketRequests
        //        .Where(n => n.UserName == userName && !n.IsRead)
        //        .CountAsync();
        //}

        //// Get the count of event-related notifications
        //public async Task<int> GetEventNotificationCount(string userName)
        //{
        //    return await _context.EventNotifications
        //        .Where(en => en.UserName == userName && !en.IsRead)
        //        .CountAsync();
        //}

        //// Get the count of general alerts or other types of notifications
        //public async Task<int> GetAlertNotificationCount(string userName)
        //{
        //    return await _context.AlertNotifications
        //        .Where(an => an.UserName == userName && !an.IsRead)
        //        .CountAsync();
        //}
    }

}
