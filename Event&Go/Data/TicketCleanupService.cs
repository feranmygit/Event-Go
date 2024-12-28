using Event_Go.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;

namespace Event_Go.Data
    {
        public class TicketCleanupService : IHostedService, IDisposable
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly EventReminderService _eventReminderService; 
            private Timer _timer;

            public TicketCleanupService(IServiceProvider serviceProvider, EventReminderService eventReminderService)
            {
                _serviceProvider = serviceProvider;
                _eventReminderService = eventReminderService;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                _timer = new Timer(HandleExpiredTickets, null, TimeSpan.Zero, TimeSpan.FromDays(1));

                return Task.CompletedTask;
            }

        private void HandleExpiredTickets(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var now = DateTime.Now.Date; // Use only the date part for comparison

                // Fetch tickets for events that have fully ended (end date is before today)
                var expiredTickets = dbContext.TicketRequests
                    .Include(tr => tr.Event)
                    .Where(tr => tr.Event.EndDate.Date < now && tr.Status == "Approved") // Check if the end date is before today
                    .ToList();

                foreach (var ticket in expiredTickets)
                {
                    // Mark the ticket as expired
                    ticket.Status = "Expired";
                }

                dbContext.SaveChanges();
            }

            // Optionally call the reminder service or other email logic
            _eventReminderService.SendRemindersAsync().Wait();
        }




        public Task StopAsync(CancellationToken cancellationToken)
            {
                _timer?.Change(Timeout.Infinite, 0);
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                _timer?.Dispose();
            }
        }
    }


public class EventReminderService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventReminderService> _logger;

    public EventReminderService(IServiceScopeFactory scopeFactory, ILogger<EventReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task SendRemindersAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tomorrow = DateTime.Now.Date.AddDays(1);

        // Fetch events happening tomorrow with approved ticket requests
        var upcomingEvents = await dbContext.Eventstables
            .Include(e => e.TicketRequests)
            .Where(e => e.StartDate == tomorrow && e.TicketRequests.Any(tr => tr.Status == "Approved"))
            .ToListAsync();

        foreach (var eventDetails in upcomingEvents)
        {
            foreach (var ticket in eventDetails.TicketRequests.Where(tr => tr.Status == "Approved"))
            {
                try
                {
                    // Fetch the email of the user based on UserName
                    var userEmail = await dbContext.Users
                        .Where(u => u.UserName == ticket.UserName)
                        .Select(u => u.Email)
                        .FirstOrDefaultAsync();

                    if (userEmail == null)
                    {
                        _logger.LogWarning($"No email found for user {ticket.UserName}.");
                        continue;
                    }

                    // Prepare email content
                    string subject = $"Reminder: {eventDetails.EventName} is happening tomorrow!";
                   string body = $"Dear {ticket.UserName},<br/><br/>" +
              $"Get ready for an unforgettable experience! The highly anticipated event <b>{eventDetails.EventName}</b> " +
              $"is just around the corner and will be taking place tomorrow, {eventDetails.StartDate:dddd, MMM dd, yyyy}.<br/><br/>" +
              $"We can’t wait to welcome you to an event packed with exciting moments, inspiring discussions, and wonderful people. " +
              $"This is your chance to be part of something special, and we’re thrilled that you'll be there with us!<br/><br/>" +
              "Don’t miss out on the fun and the chance to make unforgettable memories! We’ve got some great surprises in store for you.<br/><br/>" +
              "Looking forward to seeing you tomorrow – it’s going to be amazing!<br/><br/>" +
              "Best regards,<br/>" +
              "<b>The Event Management Team</b><br/>" +
              "P.S. Make sure to bring your energy – we’re ready to make this event one to remember!";


                    // Send the email
                    SendEmail(userEmail, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send reminder to {ticket.UserName}: {ex.Message}");
                }
            }
        }
    }

    public static void SendEmail(string toEmail, string subject, string body)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var smtpHost = configuration.GetValue<string>("EmailConfig:Host");
        var smtpPort = configuration.GetValue<int>("EmailConfig:Port");
        var smtpUser = configuration.GetValue<string>("EmailConfig:Username");
        var smtpPass = configuration.GetValue<string>("EmailConfig:Password");
        var fromEmail = configuration.GetValue<string>("EmailConfig:FromEmail");

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        try
        {
            client.Send(mailMessage);
        }
        catch (Exception ex)
        {
            //_logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
        }
    }
}

