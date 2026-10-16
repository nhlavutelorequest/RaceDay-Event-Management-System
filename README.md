# RaceDay Event Management System

### PROG6212 POE - Programming 2B

RaceDay is a full-stack, cloud-aware event management platform built for the South African road running, walking, and cycling community. Event Organisers can create and manage events, categories, and participant results, while Participants can browse events, enter them, and track their personal performance history.

---

## Table of Contents
1. [User Roles](#user-roles)
2. [Part 1 - System Planning and Database](#part-1---system-planning-and-database)
3. [Part 2 - RESTful API Development](#part-2---restful-api-development)
4. [Setup and Run Instructions](#setup-and-run-instructions)
5. [Unit Tests](#unit-tests)
6. [CI/CD](#cicd)
7. [Video Walkthroughs](#video-walkthroughs)

---

## User Roles

| Role | Capabilities |
|---|---|
| **Organiser** | Create, edit, and delete events; manage event categories; capture participant results; view all enrolments for their events. |
| **Participant** | Register an account; browse events; enrol in an event by selecting a category; view their own enrolments; track their personal results. |

Both roles can register, log in, and view and update their own profile.

---

## Part 1 - System Planning and Database

All Part 1 planning documents are in the `/docs` folder:

| File | Description |
|---|---|
| `docs/ERD.pdf` | Entity Relationship Diagram for the full RaceDay data model (6 entities). |
| `docs/API_Endpoint_Plan.md` | Endpoint plan covering Authentication, User Profile, Events, Categories, Event Enrolments, and Results. |
| `docs/RaceDay_Schema.sql` | SQL Server script that creates and seeds the RaceDay database, tested in SSMS. |

---

## Part 2 - RESTful API Development

Part 2 builds the RaceDay API exactly as specified in the Part 1 endpoint plan, using ASP.NET Core Web API (.NET 9) and Entity Framework Core (Code-First) on SQL Server. An ASP.NET Core MVC web application (`RaceDay.Web`) consumes the API.

**Branch:** `feature/part2-web-ui`

### Tech Stack
- ASP.NET Core Web API (.NET 9)
- ASP.NET Core MVC (`RaceDay.Web`)
- Entity Framework Core 9.0.0, SQL Server, Code-First with Migrations
- Swagger 
- xUnit for unit testing
- GitHub Actions for CI

### Solution Structure
```
RaceDay.Api/             # Web API project (https://localhost:5001)
  Controllers/           # API endpoints
  Data/                  # RaceDayDbContext
  DTOs/                  # Request/response models
  Filters/               # SessionAuthorize role-based access filter
  Migrations/            # EF Core migrations
  Models/                # Role, User, Event, Category, Enrolment, Result
  Program.cs
RaceDay.Web/             # MVC web application (https://localhost:7035)
  Controllers/           # Page controllers
  Models/                # View models
  Services/              # ApiClient - all communication with the API
  Views/                 # Razor views
  wwwroot/               # CSS, JS, static files
  Program.cs
RaceDay.Api.Tests/       # Unit tests
  Helpers/               # FakeSession, TestDbContextFactory
docs/                    # Part 1 planning documents
```

### Features

**Database (Code-First)**
- Six entities: `Role`, `User`, `Event`, `Category`, `Enrolment`, `Result`.
- Data annotations, foreign keys, and navigation properties match the Part 1 ERD and SQL script.
- Roles use a separate lookup table rather than an enum, as designed in Part 1.
- The schema is created through EF Core migrations.

**Authentication and Sessions**
- Users register with their personal details and select a role (Organiser or Participant).
- Passwords are securely hashed before storage and are never stored in plain text.
- Login stores the user's ID and role in the server session, which maintains their authenticated state across requests.
- Logging out ends the session.

**Role-Based Access**
- A custom `SessionAuthorize` filter protects all non-public endpoints.
- Unauthenticated requests are rejected with `401 Unauthorized`.
- Requests from the wrong role are rejected with `403 Forbidden`, so Organiser functionality is not accessible to Participants and vice versa.

**API Endpoints** (as defined in `docs/API_Endpoint_Plan.md`)

| Area | Functionality |
|---|---|
| Authentication | Register and log in. |
| User Profile | Both roles can view and update their own profile. |
| Events | Organisers create, update, and delete events (name, description, date, location, distance, event type: run, walk, or cycle). Both roles can view events. |
| Categories | Organisers define age or distance categories for each event. Both roles can view them. |
| Event Enrolments | Participants enrol by selecting a category. Organisers view all enrolments for their events. |
| Results | Organisers capture finish times and finishing positions. Participants view their own results. |

**Swagger**
- Every endpoint is listed and can be tested in the browser at `/swagger`, with its purpose, expected input, and responses documented.

**Web Application (`RaceDay.Web`)**
- Communicates with the API only through the `ApiClient` service, with no direct database access or duplicated business logic.
- Register and login pages, with session-based authentication.
- Event browsing and an event details page showing full event information and available categories.
- Participants can enrol in an event by selecting a category.
- Plain, friendly validation and error messages are shown to users instead of raw API codes or JSON.

---

## Setup and Run Instructions

**Prerequisites:** .NET 9 SDK, SQL Server (or SQL Server Express), and Visual Studio 2022 or the `dotnet` CLI.

1. **Clone the repository**
```bash
git clone https:(https://github.com/nhlavutelorequest/RaceDay-Event-Management-System.git)
cd RaceDay-Event-Management-System
git checkout feature/part2-web-ui
```

2. **Configure the database connection** in `RaceDay.Api/appsettings.json`:
```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
```

3. **Create the database** from the EF Core migrations:
```bash
   cd RaceDay.Api
   dotnet ef database update
```

4. **Run the API and the web application together.**

   *Visual Studio:* right-click the solution → **Configure Startup Projects** → **Multiple startup projects** → set both `RaceDay.Api` and `RaceDay.Web` to **Start**, then press **F5**.

   *Command line (two terminals):*
```bash
   dotnet run --project RaceDay.Api
   dotnet run --project RaceDay.Web
```

5. **Open the applications**

   | Application | URL |
   |---|---|
   | Swagger (API) | https://localhost:5001/swagger |
   | Web application | https://localhost:7035 |

---

## Unit Tests

Unit tests cover both success and failure scenarios for:
- User registration and login
- Event management by Organisers
- Rejection of unauthenticated requests and of requests from the wrong role
- Participant enrolment and correct recording of the enrolment

Run all tests:
```bash
dotnet test
```

---

## CI/CD

A GitHub Actions workflow in `.github/workflows/` builds the solution and runs all unit tests on every push.

**Green build screenshot:**

 <img width="1365" height="574" alt="image" src="https://github.com/user-attachments/assets/54b6e4c7-5212-4dda-9000-9a2f828d833c" />


---

## Video Walkthroughs

| Part | Link |
|---|---|
| Part 1 - Planning and Database | https://youtu.be/fnTeI4-AHi4?si=Loz_87pzdS3DlO0j |
| Part 2 - RESTful API |  (https://youtu.be/E3xnsPQr90I?si=lu1AK9K7cO_u4KCL) |

**Part 2 video covers:**
- The running API demonstrated through Swagger
- The web application consuming the API
- Code structure: models, DbContext, controllers, DTOs, filters, and ApiClient
- Endpoints and how they match the Part 1 plan
- Authentication, session management, and role-based access
- Unit tests, run locally
