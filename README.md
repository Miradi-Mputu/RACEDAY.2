# PROG6212 - RaceDay

## Project Description

RaceDay is a race management system that allows organisers to create and manage race events, and participants to enter those events. This repository contains Part 2  the RESTful API built with ASP.NET Core Web API.

## Project Location

The API project is inside the RACEDAY.2 folder.

## Roles

### Organiser
Organisers create and manage race events, define categories for each event, view all enrolments for their events, and capture race results including finish times and positions.

### Participant
Participants register and log in, manage their own profile, view available events and categories, enter events by selecting a category, and view their own results.

## Features Implemented

- Session-based authentication with role selection (Organiser or Participant)
- Password hashing using BCrypt
- Role-based access control on all protected endpoints
- Event management (Organisers create/update/delete, both roles view)
- Category management (Organisers create/delete, both roles view)
- Event enrolments (Participants enrol, Organisers view enrolments for their events)
- Results (Organisers capture, Participants view their own)
- User profile management (both roles view and update their own)

## Setup and Run Instructions

1. Clone the repository:
   git clone https://github.com/Miradi-Mputu/RACEDAY.2.git

2. Open the solution in Visual Studio:
   RACEDAY.2/RACEDAY.2.sln

3. Restore the NuGet packages (right-click solution, then Restore NuGet Packages).

4. Ensure the connection string in appsettings.json points to your SQL Server instance.

5. Open Package Manager Console and run:
   Update-Database

6. Set RACEDAY.2 as the Startup Project.

7. Press F5 to run. Swagger will open at https://localhost:7234/swagger.

## Testing

The API includes 12 unit tests covering:

- User registration with password hashing
- Login with valid and invalid credentials
- Role enforcement (Organiser vs Participant)
- Event creation and retrieval
- Enrolment linking participant, event and category

To run the tests:
- In Visual Studio, open Test Explorer (Test menu, then Test Explorer) and click Run All.
- Or run: dotnet test

## CI/CD

The GitHub Actions workflow builds the solution and runs all unit tests on every push.

<img width="1324" height="670" alt="Screenshot 2026-10-13 175631" src="https://github.com/user-attachments/assets/f581b561-91e4-4707-bc9a-a2b5639467fc" />


## YouTube Demonstration

[INSERT YOUR VIDEO LINK HERE]

## Technology Stack

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core (Code First)
- SQL Server
- BCrypt.Net for password hashing
- xUnit for unit testing
- GitHub Actions for CI/CD
