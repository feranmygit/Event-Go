 # Event&Go Project

## Table of Contents
1. [Project Overview](#project-overview)
2. [Features](#features)
3. [Technologies Used](#technologies-used)
4. [Setup and Installation](#setup-and-installation)
5. [Usage](#usage)
6. [Troubleshooting](#Troubleshooting)

## Project Overview

The **Event&Go Project** is a user-friendly and feature-rich solution designed for managing events and conferences effectively. It provides functionalities such as participant registration, ticket booking, scheduling, email reminders, and administrative tools for organizers and admins.

The project includes functionality for:
- Separate page for admin registration.
- Authentication and Authorization for both organizers or users.
- Creating and managing event for both admin and organizers.
- Email sending on creating an events via SMTP implementation.
- Tickets approval options for both admin and organizers.
- Tickets token generations on approval using guid.
- User Tickets event booking request options.
- In-app notifications.
- Event reminders via email and in-app.
- Event search functionality.
- MySQL as database etc.

## Features
    
- Role-based access control (Admin, Organizer, User).
- Automated email notifications for events and ticket updates.
- Secure authentication and authorization using ASP.NET Identity.
- Real-time status updates for events (Upcoming, Ongoing, Completed, Cancelled).

## Technologies Used

This project is built using the following technologies:

- **Backend:**  ASP.NET Core (C#) with MVC architecture.
- **Frontend:** HTML, CSS, and JavaScript (without Bootstrap for a clean design).
- **Database:**  MySQL Server.
- **Authentication & Authorization** ASP.NET Identity.
- **Email Notifications** SMTP service (e.g., Gmail).
- **Deployment** Not yet deploy.

## Setup and Installation

### Prerequisites

- Visual Studio (2022) with .NET Core 8.0 SDK installed.
- MySQL Server.
- A Gmail account or other SMTP-compatible email account.

### Steps

1. Clone the repository.
2. Open the project in Visual Studio.
3. Apply database migrations.
4. Run the application. 


## Usage

1. **Access Roles:**
   - **Users** Register for events and book tickets.
   - **Organizers** Manage event creation, ticket approvals, and expired tickets.
   - **Admins** Oversee all events, categories, and roles.

2. **Ticket Booking:**
   - View event details.
   - Click "Book Event" for available events.
   - Receive confirmation via in-app.

3. **Email Notifications:**
    `Automated email notifications for:`

   - Event creations.
   - Events reminders to users after a successful ticketing.


## Troubleshooting

`Email Issues`

- Ensure SMTP credentials are valid in appsettings.json.
- Verify that my email provider allows app-specific passwords or SMTP connections.

`Database Issues`

- Confirm the DefaultConnection string points to a valid MySQL Server instance.
- Ensure migrations are applied using Add-Migration InitialMigrations

`Debugging Tips`

- Check logs in the console for detailed error messages.
- Use breakpoints in Visual Studio to trace issues.

