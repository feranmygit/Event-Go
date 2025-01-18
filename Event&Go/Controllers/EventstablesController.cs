using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Event_Go.Data;
using WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Event_Go.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Event_Go.Controllers
{
   
    public class EventstablesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public EventstablesController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }



        public async Task<IActionResult> Index(string searchBy, string SearchValue, int page = 1, int pageSize = 4)
        {
            DateTime today = DateTime.Today;

            // Update statuses
            var eventsToUpdate = _context.Eventstables.Where(e => e.Status != "Cancelled").ToList();
            foreach (var ev in eventsToUpdate)
            {
          
                    if (ev.StartDate < today && ev.EndDate < today)
                    {
                        ev.Status = "Completed";
                    }
                    else if (ev.StartDate == today && ev.StartDate < today && ev.EndDate == today)
                    {
                        ev.Status = "Ongoing";
                    }
                    else if (ev.StartDate > today && ev.EndDate > today)
                    {
                        ev.Status = "Upcoming";
                    }
                    else
                    {
                        ev.Status = "Ongoing";
                    }
            }

            await _context.SaveChangesAsync();

            // Fetch events and statistics
            ViewBag.UpcomingCount = _context.Eventstables.Count(e => e.Status == "Upcoming");
            ViewBag.OngoingCount = _context.Eventstables.Count(e => e.Status == "Ongoing");
            ViewBag.PlannedCount = _context.Eventstables.Count(e => e.Status == "Completed");
            ViewBag.CancelledCount = _context.Eventstables.Count(e => e.Status == "Cancelled");


            var currentUser = User.Identity.Name; 

            // Updated query to exclude Completed events
            var eventsQuery = _context.Eventstables
                .Include(e => e.Category)
                .Where(e => (e.StartDate >= today || e.Status != "Cancelled") && 
                            (e.Visibility == "Public" || e.Visibility != "Private" && e.CreatedBy == currentUser) &&
                             e.Status != "Completed");


            if (!string.IsNullOrEmpty(SearchValue))
            {
                if (searchBy?.ToLower() == "eventname")
                {
                    eventsQuery = eventsQuery.Where(p =>
                        (p.Visibility == "Public" || p.CreatedBy == currentUser) &&
                        p.EventName.ToLower().Contains(SearchValue.ToLower()));
                }
                else if (searchBy?.ToLower() == "bookingusername")
                {
                    eventsQuery = eventsQuery.Where(p =>
                        (p.Visibility == "Public" || p.CreatedBy == currentUser) &&
                        p.BookingUserName != null && p.BookingUserName.ToLower().Contains(SearchValue.ToLower()));
                }
            }



            int searchCount = 0; // Initialize the search count

            if (!string.IsNullOrEmpty(SearchValue))
            {
                if (searchBy?.ToLower() == "eventname")
                {
                    eventsQuery = eventsQuery.Where(p => p.EventName.ToLower().Contains(SearchValue.ToLower()));
                }
                else if (searchBy?.ToLower() == "bookingusername")
                {
                    eventsQuery = eventsQuery.Where(p => p.BookingUserName != null && p.BookingUserName.ToLower().Contains(SearchValue.ToLower()));
                }

                // Count the search results
                searchCount = await eventsQuery.CountAsync();
            }

            ViewBag.SearchCount = searchCount; // Pass the search count to the view
            ViewBag.SearchValue = SearchValue; // Pass the search value for display

            int totalEvents = await eventsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalEvents / pageSize);

            var events = await eventsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EventListPartial", events); // Return partial view for AJAX
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(events);
        }




        [Authorize(Roles = "Admin,Organizer")]
        // GET: Events/Create
        public IActionResult Create()
        {
            // Populating Category list from the database
            ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category");

            ViewBag.VisibilityList = new SelectList(new List<string>
            {
                "Public",
                "Private"
            });

            // Populating Status list as strings
            ViewBag.StatusList = new SelectList(new List<string>
            {
                "Upcoming",
                "Ongoing",
                "Completed",
                "Cancelled"
            });

            return View();
        }



        [Authorize(Roles = "Admin,Organizer")]
        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Eventstable eventstable, IFormFile file)
        {
            if (eventstable.StartDate > eventstable.EndDate)
            {
                ModelState.AddModelError("EndDate", "End Date must be later than Start Date.");
                ViewBag.message = "Email not send! Validations error.";
                return View(eventstable);
            }

            if (!ModelState.IsValid)
            {
                // Set the creator's user ID
                eventstable.CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(eventstable.Visibility))
                {
                    eventstable.Visibility = "Public"; // Default to Public if not specified
                }


                // Repopulate dropdowns if ModelState is invalid
                ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category", eventstable.CategoryId);
                ViewBag.StatusList = new SelectList(new List<string> { "Upcoming", "Ongoing", "Completed", "Cancelled" }, eventstable.Status);

                // Set TempData success message
                //TempData["error"] = "Form submitted failed!";
                //return View(eventstable);
                // Return the same view if validation fails
              

            }


            DateTime today = DateTime.Today;

            // Automatically set the status if not provided
            if (eventstable.Status == null)
            {
                if (eventstable.StartDate < today && eventstable.EndDate < today)
                {
                    eventstable.Status = "Completed";
                }
                else if (eventstable.StartDate == today && eventstable.StartDate < today && eventstable.EndDate == today)
                {
                    eventstable.Status = "Ongoing";
                }
                else if (eventstable.StartDate > today && eventstable.EndDate > today)
                {
                    eventstable.Status = "Upcoming";
                }
                else
                {
                    eventstable.Status = "Ongoing";
                }
            }



            // File Validation
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Event image is required.");
                ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category", eventstable.CategoryId);
                ViewBag.StatusList = new SelectList(new List<string> { "Upcoming", "Ongoing", "Completed", "Cancelled" }, eventstable.Status);
                return View(eventstable);
            }

            var myAppConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            // Email Configuration
            var Username = myAppConfig.GetValue<string>("EmailConfig:Username");
            var Password = myAppConfig.GetValue<string>("EmailConfig:Password");
            var Host = myAppConfig.GetValue<string>("EmailConfig:Host");
            var Port = myAppConfig.GetValue<int>("EmailConfig:Port");
            var FromEmail = myAppConfig.GetValue<string>("EmailConfig:FromEmail");

            try
            {
                // Prepare Email
                MailMessage message = new MailMessage
                {
                    From = new MailAddress(FromEmail),
                    Subject = eventstable.EventName,
                    Body = eventstable.Description,
                    IsBodyHtml = true
                };
                message.To.Add(eventstable.CreatedBy);
                message.Attachments.Add(new Attachment(file.OpenReadStream(), file.FileName));

                using SmtpClient mailClient = new SmtpClient(Host)
                {
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(Username, Password),
                    Host = Host,
                    Port = Port,
                    EnableSsl = true
                };

                await mailClient.SendMailAsync(message);
                ViewBag.message = "Email sent successfully.";
            }
            catch (SmtpException smtpEx)
            {
                ViewBag.message = "Email failed to send!";
                return View(eventstable); // Return the view if email fails
            }
            catch (Exception ex)
            {
                ViewBag.message = "Unexpected error!";
                return View(eventstable);
            }

            // Save Image
            string path = Path.Combine(_hostEnvironment.WebRootPath, "image");
            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            string fullPath = Path.Combine(path, fileName);

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                using FileStream fs = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(fs);

                eventstable.ImageName = "/image/" + fileName; 
            }
            catch (Exception ex)
            {
                ViewBag.message = "File upload failed!!";
                return View(eventstable);
            }

            // Save to Database
            try
            {
                _context.Eventstables.Add(eventstable);
                await _context.SaveChangesAsync();
                TempData["success"] = "An event was created successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to save the event to the database.";
                return View(eventstable); // Return the view on failure
            }

            return RedirectToAction(nameof(Index));
        }

   


        [Authorize(Roles = "Admin,Organizer")]

        // GET: Events/Edit/5
        public IActionResult Edit(int? id)
        {

            if (id == null)
                return NotFound();


            var eventstable = _context.Eventstables.Find(id);
            if (eventstable == null)
                return NotFound();

            // Populating Category list from the database
            ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category", eventstable.CategoryId);

            ViewBag.VisibilityList = new SelectList(new List<string>
            {
                "Public",
                "Private"
            }, eventstable.Visibility);

            // Populating Status list as strings
            ViewBag.StatusList = new SelectList(new List<string>
            {
                "Upcoming", "Ongoing", "Completed", "Cancelled"
            }, eventstable.Status);

            return View(eventstable);
        }


        [Authorize(Roles = "Admin,Organizer")]

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Eventstable eventstable, IFormFile file)
        {

            // Repopulate dropdowns if ModelState is invalid
            ViewBag.VisibilityList = new SelectList(new List<string>
            {
                "Public",
                "Private"
            }, eventstable.Visibility);

            ViewBag.CategoryList = new SelectList(_context.Event_categories, "Id", "Category", eventstable.CategoryId);
            ViewBag.StatusList = new SelectList(new List<string>
    {
        "Upcoming", "Ongoing", "Completed", "Cancelled"
    }, eventstable.Status);

            if (id != eventstable.EventId)
            {
                return NotFound();
            }




            var existingEvent = await _context.Eventstables.FindAsync(id);

            if (existingEvent == null)
            {
                return NotFound();
            }

            DateTime today = DateTime.Today;

            // Automatically set the status if not provided
            if (eventstable.Status == null)
            {
                if (eventstable.StartDate < today)
                    eventstable.Status = "Cancelled";
                else if (eventstable.StartDate == today)
                    eventstable.Status = "Ongoing";
                else if (eventstable.StartDate > today && eventstable.StartDate <= today.AddDays(3))
                    eventstable.Status = "Upcoming";
                else
                    eventstable.Status = "Completed";
            }

            // If a new file is uploaded, delete the old image and upload the new one
            if (file != null)
            {
                // Delete the old image if it exists
                if (!string.IsNullOrEmpty(existingEvent.ImageName))
                {
                    var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingEvent.ImageName.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Upload the new image
                string path = _hostEnvironment.WebRootPath + "/image/";
                Guid Id = Guid.NewGuid();
                string fileName = Id.ToString() + "_" + file.FileName;
                string newPath = Path.Combine(path, fileName);

                using (FileStream fs = new FileStream(newPath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                // Update the ImageName in the database to the new file's relative path
                existingEvent.ImageName = "/image/" + fileName;
            }

            // Update other properties of the event
            existingEvent.EventName = eventstable.EventName;
            existingEvent.Description = eventstable.Description;
            existingEvent.StartDate = eventstable.StartDate;
            existingEvent.EndDate = eventstable.EndDate;
            existingEvent.Venue = eventstable.Venue;
            existingEvent.Status = eventstable.Status;
            existingEvent.Visibility = eventstable.Visibility;
            existingEvent.BookingUserName = eventstable.BookingUserName;
            existingEvent.CategoryId = eventstable.CategoryId;

            _context.Update(existingEvent);
            await _context.SaveChangesAsync();
            TempData["success"] = "The event was updated successfully";

            return RedirectToAction(nameof(Index));
        }


        // GET: Events/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
                return NotFound();

            var eventstable = _context.Eventstables
                .Include(e => e.Category)
                .FirstOrDefault(m => m.EventId == id);

            if (eventstable == null)
                return NotFound();

            return View(eventstable);
        }


        [Authorize(Roles = "Admin,Organizer")]
        // GET: Events/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var eventstable = _context.Eventstables
                .Include(e => e.Category)
                .FirstOrDefault(m => m.EventId == id);

            if (eventstable == null)
                return NotFound();


            return View(eventstable);
        }



        [Authorize(Roles = "Admin,Organizer")]

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var eventstable = _context.Eventstables.Find(id);


            // delete the record
            if (!string.IsNullOrEmpty(eventstable.ImageName))
            {
                var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, eventstable.ImageName.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _context.Eventstables.Remove(eventstable);
            _context.SaveChanges();
            TempData["success"] = "The event was deleted successfully";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin,Organizer,User")]
        public async Task<IActionResult> MyEvents()
        {
            // Get the currently logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch events created by this user
            var userEvents = await _context.Eventstables
                .Include(e => e.Category) // Include related category data
                .Where(e => e.CreatedByUserId == userId) // Assuming 'CreatedByUserId' stores the creator's user ID
                .ToListAsync();

            return View(userEvents);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BookEvent(int eventId)
        {
            var selectedEvent = await _context.Eventstables.FindAsync(eventId);
            if (selectedEvent == null)
            {
                return NotFound();
            }

            var userName = User.Identity.Name;

            // Check if the event is already booked by the current user
            var existingBooking = await _context.EventBookings
                .FirstOrDefaultAsync(b => b.EventId == eventId && b.UserName == userName);

            if (existingBooking != null)
            {
                TempData["error"] = "You have already booked this event!";
                return RedirectToAction(nameof(Index));
            }

            // Save booking information
            var booking = new EventBooking
            {
                EventId = eventId,
                UserName = userName,
                BookingDate = DateTime.Now
            };
            _context.EventBookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["success"] = "Event booked successfully! Kindly see the details in your WishLists";
            return RedirectToAction(nameof(Index));
        }


        //[Authorize(Roles = "Admin,Organizer,User")]
        [HttpPost]
        public async Task<IActionResult> UnbookEvent(int eventId)
        {
            var userName = User.Identity.Name;

            // Find the booking entry for the current user and the event
            var booking = await _context.EventBookings
                .FirstOrDefaultAsync(b => b.EventId == eventId && b.UserName == userName);

            if (booking == null)
            {
                TempData["error"] = "No booking found for this event.";
                return RedirectToAction(nameof(UserNotification));
            }

            // Remove the booking
            _context.EventBookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["success"] = "You have successfully unbooked the event.";
            return RedirectToAction(nameof(UserNotification));
        }


        [Authorize(Roles = "Admin,Organizer,User")]
        public async Task<IActionResult> Inbox()
        {
            var userName = User.Identity.Name;

            // Fetch events with ticket requests for the logged-in user, excluding expired events
            var bookedEvents = await _context.Eventstables
                .Include(e => e.TicketRequests) // Ensure TicketRequests  are loaded
                .Where(e => e.TicketRequests.Any(tr => tr.UserName == userName)
                            && e.Status != "Expired" // Exclude events marked as expired
                            && e.EndDate.Date >= DateTime.Now.Date) // Exclude events whose end date is in the past
                .ToListAsync();

            // Update the Reminder property dynamically
            foreach (var eventItem in bookedEvents)
            {
                if (DateTime.Now.Date >= eventItem.StartDate.Date && DateTime.Now.Date <= eventItem.EndDate.Date)
                {
                    eventItem.Reminder = "Now";
                }
                else if (eventItem.StartDate.Date == DateTime.Now.Date.AddDays(1))
                {
                    eventItem.Reminder = "Tomorrow";
                }
                else if (eventItem.StartDate.Date > DateTime.Now.Date)
                {
                    eventItem.Reminder = "in the Upcoming days";
                }
                else
                {
                    eventItem.Reminder = ""; // No reminder for past or completed events
                }
            }


            return View(bookedEvents);
        }




        [Authorize(Roles = "Admin,Organizer,User")]
        public async Task<IActionResult> UserNotification()
        {
            var userName = User.Identity.Name;

            // Fetch booked events and prepare reminders
            var notifications = await _context.EventBookings
                .Where(b => b.UserName == userName)
                .Include(b => b.Event)
                .ThenInclude(e => e.Category)
                .Select(b => b.Event) // Directly select the Event entity
                .ToListAsync();

            return View("Users/UserNotification", notifications);
        }


        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> TicketRequests()
        {
            var organizerName = User.Identity.Name;

            // Fetch events organized by the current organizer
            var ticketRequests = await _context.TicketRequests
                .Include(tr => tr.Event)
                .Where(tr => tr.Event.CreatedBy == organizerName && tr.Status == "Pending")
                .ToListAsync();

            //return View(ticketRequests);
            return View("Organizer/TicketRequests", ticketRequests);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> ProcessTicketRequest(int requestId, string action)
        {
            var request = await _context.TicketRequests.FindAsync(requestId);
            if (request == null)
            {
                return NotFound();
            }

            if (action == "accept")
            {
                request.Status = "Approved";
            }
            else if (action == "reject")
            {
                request.Status = "Declined";
            }

            // Ensure UniqueCode is not overwritten accidentally.
            if (request.Status == "Approved" && string.IsNullOrEmpty(request.UniqueCode))
            {
                request.UniqueCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            }


            _context.Update(request);
            await _context.SaveChangesAsync();

            TempData["success"] = "The ticket request has been processed.";
            return RedirectToAction(nameof(TicketRequests));
        }



        [Authorize(Roles = "Admin,Organizer,User")]
        [HttpPost]
        public async Task<IActionResult> RequestTicket(int eventId)
        {
            var userName = User.Identity.Name;

            // Check if the event exists and fetch its details
            var eventDetails = await _context.Eventstables.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (eventDetails == null)
            {
                TempData["error"] = "The event does not exist.";
                return RedirectToAction(nameof(UserNotification));
            }

            // Check if the event is today or has already passed
            if (eventDetails.StartDate <= DateTime.Today)
            {
                TempData["error"] = "Ticket requests are unavailable for events starting today or in the past.";
                return RedirectToAction(nameof(UserNotification));
            }

            // Check if a ticket request already exists
            var existingRequest = await _context.TicketRequests
                .FirstOrDefaultAsync(tr => tr.EventId == eventId && tr.UserName == userName);

            if (existingRequest != null)
            {
                TempData["error"] = "You have already requested a ticket for this event.";
                return RedirectToAction(nameof(UserNotification));
            }

            // Save the ticket request
            var ticketRequest = new TicketRequest
            {
                EventId = eventId,
                UserName = userName,
                RequestDate = DateTime.Now,
                Status = "Pending"
            };
            _context.TicketRequests.Add(ticketRequest);
            await _context.SaveChangesAsync();

            TempData["success"] = "Your ticket request has been sent to the organizer.";
            return RedirectToAction(nameof(UserNotification));
        }


        [Authorize(Roles = "Admin,Organizer,User")]
        [HttpPost]
        public async Task<IActionResult> RevokeTicketRequest(int eventId)
        {
            var userName = User.Identity.Name;

            // Find the ticket request for the event and user
            var ticketRequest = await _context.TicketRequests
                .FirstOrDefaultAsync(tr => tr.EventId == eventId && tr.UserName == userName);

            if (ticketRequest == null)
            {
                TempData["error"] = "Ticket request not found.";
                return RedirectToAction(nameof(Inbox)); 
            }

            if (ticketRequest.Status == "Approved")
            {
                TempData["error"] = "Cannot revoke an already approved ticket.";
                return RedirectToAction(nameof(Inbox));
            }

            // Remove the ticket request
            _context.TicketRequests.Remove(ticketRequest);
            await _context.SaveChangesAsync();

            TempData["success"] = "Your ticket request has been successfully revoked.";
            return RedirectToAction(nameof(Inbox)); 
        }



        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> ApprovedTickets()
        {
            var organizerName = User.Identity.Name;
            var currentDate = DateTime.Now;

            // Fetch approved ticket requests for events organized by the current organizer
            var approvedRequests = await _context.TicketRequests
                .Include(tr => tr.Event)
                .Where(tr => tr.Event.CreatedBy == organizerName && tr.Status == "Approved" && tr.Event.EndDate >= currentDate)
                .ToListAsync();

            return View("Organizer/ApprovedTickets", approvedRequests);
        }



        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public async Task<IActionResult> ApproveTicketRequest(int ticketRequestId)
        {
            var ticketRequest = await _context.TicketRequests.FindAsync(ticketRequestId);
            if (ticketRequest == null)
            {
                return NotFound();
            }

            // Generate and save a unique code
            ticketRequest.UniqueCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            ticketRequest.Status = "Approved";

            _context.Update(ticketRequest);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Unique Code Generated: {ticketRequest.UniqueCode}"); // For debugging
            TempData["success"] = "Ticket request approved successfully!";
            return RedirectToAction(nameof(TicketRequests));
        }

        [Authorize(Roles = "Admin,Organizer,User")]
        [HttpPost]
        public async Task<IActionResult> DisapproveTicketRequest(int ticketRequestId)
        {
            var ticketRequest = await _context.TicketRequests.FindAsync(ticketRequestId);
            if (ticketRequest == null)
            {
                return NotFound();
            }
             
            // Update ticket status to "Disapproved" or revert back to "Pending"
            ticketRequest.Status = "Rejected"; // Or "Disapproved" based on your logic
            ticketRequest.UniqueCode = "";  // Remove the unique code if needed

            _context.Update(ticketRequest);
            await _context.SaveChangesAsync();

            TempData["success"] = "Ticket request disapproved successfully!";
            return RedirectToAction(nameof(ApprovedTickets)); // Redirect to the list of approved tickets
        }


        [Authorize(Roles = "Admin,Organizer,User")]
        public async Task<IActionResult> ExpiredTickets()
        {
            var userName = User.Identity.Name; // Get the logged-in user

            var expiredTickets = await _context.TicketRequests
                .Include(tr => tr.Event) // Include the event details for display
                .Where(tr => tr.UserName == userName && tr.Status == "Expired")
                .ToListAsync();

            return View(expiredTickets);
        }


        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> ExpiredApprovedTickets()
        {
            var organizerName = User.Identity.Name;
            var currentDate = DateTime.Now;

            // Fetch expired approved ticket requests for events organized by the current organizer
            var expiredRequests = await _context.TicketRequests
                .Include(tr => tr.Event)
                .Where(tr => tr.Event.CreatedBy == organizerName
                             && tr.Status == "Expired"
                             && tr.Event.StartDate < currentDate && tr.Event.EndDate < currentDate)
                .ToListAsync();

            return View("Organizer/ExpiredApprovedTickets", expiredRequests);
        }


        //[Authorize(Roles = "Admin,Organizer")]
        //public async Task<IActionResult> HideTicket(int id)
        //{
        //    var ticket = await _context.TicketRequests.FindAsync(id);
        //    if (ticket != null)
        //    {
        //        ticket.IsHidden = true;
        //        _context.TicketRequests.Update(ticket);
        //        await _context.SaveChangesAsync();
        //    }
        //    return RedirectToAction(nameof(ExpiredApprovedTickets));
        //}



        private async Task SendEmailAsync(string CreatedBy, string subject, string body)
        {
            var myAppConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var username = myAppConfig.GetValue<string>("EmailConfig:Username");
            var password = myAppConfig.GetValue<string>("EmailConfig:Password");
            var host = myAppConfig.GetValue<string>("EmailConfig:Host");
            var port = myAppConfig.GetValue<int>("EmailConfig:Port");
            var fromEmail = myAppConfig.GetValue<string>("EmailConfig:FromEmail");

            using var mailClient = new SmtpClient(host)
            {
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(username, password),
                EnableSsl = true,
                Port = port
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(CreatedBy);

            await mailClient.SendMailAsync(message);
        }




        [HttpPost]
        public async Task<IActionResult> ValidateTicket([FromBody] dynamic payload)
        {
            try
            {
                string uniqueCode = payload?.uniqueCode;
                if (string.IsNullOrWhiteSpace(uniqueCode))
                {
                    return Json(new { success = false, message = "Unique code cannot be empty." });
                }

                var ticketRequest = await _context.TicketRequests
                    .Include(tr => tr.Event)
                    .FirstOrDefaultAsync(tr => tr.UniqueCode == uniqueCode);

                if (ticketRequest == null || ticketRequest.Status != "Approved")
                {
                    return Json(new { success = false, message = "Invalid or unapproved ticket." });
                }

                return Json(new { success = true, message = "Ticket is valid.", eventName = ticketRequest.Event.EventName });
            }
            catch (Exception ex)
            {
                // Log the exception to identify the issue
                Console.WriteLine($"Error in ValidateTicket: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }



    }
}
