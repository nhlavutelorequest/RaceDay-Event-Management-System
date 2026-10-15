# RaceDay-Event-Management-System

### PROG6212 POE - Programming 2B

RaceDay is a full-stack, cloud-aware event management platform built for the South African road running, walking, and cycling community. Event Organisers can create and manage events, categories, and participant results, while Participants can browse events, enter them, and track their personal performance history.

---

## The Roles

| Role | Capabilities |
|---|---|
| **Organiser** | Create, edit, and delete events; manage event categories; capture participant results; view all enrolments for their events. |
| **Participant** | Register an account; browse events; enrol in an event by selecting a category; view their own enrolments; track their personal results. |

---

## Part 1 - System Planning and Database

All Part 1 planning documents are in the `/docs` folder:

- `docs/ERD.pdf` - Entity Relationship Diagram for the full RaceDay data model (6 entities).
- `docs/API_Endpoint_Plan.md` - Full endpoint plan covering Authentication, User Profile, Events, Categories, Event Enrolments, and Results.
- `docs/RaceDay_Schema.sql` - SQL Server script that creates and seeds the RaceDay database. Tested in SQL Server Management Studio (SSMS).

### Part 1 Video walkthrough
**YouTube link:** https://youtu.be/fnTeI4-AHi4?si=Loz_87pzdS3DlO0j

The video covers:
- A walkthrough of the ERD and the reasoning behind the entity/relationship design
- A walkthrough of the API endpoint plan
- The SQL script executed live in SSMS

---

## Part 2 - RESTful API Development

Part 2 implements the API specified in the Part 1 endpoint plan, using ASP.NET Core Web API (.NET 9) and Entity Framework Core (Code-First) on SQL Server.

**Branch:** `[feature/part2-api-development]`

### Tech stack
- ASP.NET Core Web API (.NET 9)
- Entity Framework Core 9.0.0 (SQL Server, Code-First with Migrations)
- Swagger / Swashbuckle (6.9.0)
- xUnit unit tests
- GitHub Actions CI

### Solution structure
```
RaceDay.Api/             # Web API project
  Controllers/           # API endpoints
  Data/                  # RaceDayDbContext
  DTOs/                  # Request/response models
  Filters/               # SessionAuthorize role-based access filter
  Migrations/            # EF Core migrations
  Models/                # Role, User, Event, Category, Enrolment, Result
  Program.cs
RaceDay.Api.Tests/       # Unit tests
  Helpers/               # FakeSession, TestDbContextFactory
docs/                    # Part 1 planning documents
```

### What was implemented
- **Database (Code-First):** Six entity models (`Role`, `User`, `Event`, `Category`, `Enrolment`, `Result`) with data annotations, foreign keys, and navigation properties. Roles use a separate lookup table rather than an enum, matching the Part 1 ERD. Schema is created through EF Core migrations.
- **Authentication:** Users register with their personal details and choose a role (Organiser or Participant), then log in with their credentials. Passwords are hashed with `[hashing method, e.g. BCrypt / PBKDF2]` and never stored in plain text.
- **Session management:** On login the user's ID and role are stored in the server session and used for all later requests.
- **Role-based access:** A custom `SessionAuthorize` attribute blocks unauthenticated requests (401) and wrong-role requests (403). Organiser endpoints are not accessible to Participants and vice versa.
- **Endpoints:** Authentication, User Profile, Events, Categories, Event Enrolments, and Results, as defined in `docs/API_Endpoint_Plan.md`. 
- **Swagger:** All endpoints are listed and testable at `/swagger`, each with a description, expected input, and responses.
- **Unit tests:** Success and failure cases for registration and login, organiser event management, role rejection, and enrolment recording.

### Setup and run instructions

**Prerequisites:** .NET 9 SDK, SQL Server (or SQL Server Express), Visual Studio 2022 or the `dotnet` CLI.

1. Set your SQL Server connection string in `RaceDay.Api/appsettings.json`:
```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
```
2. Create the database from the migrations:
```bash
   cd RaceDay.Api
   dotnet ef database update
```
3. Run the API:
```bash
   dotnet run
```
4. Open Swagger at `https://localhost:5001/swagger`.

### Running the unit tests
```bash
dotnet test
```

### CI/CD
A GitHub Actions workflow in `.github/workflows/` builds the solution and runs all unit tests on every push.

**CI/CD green build screenshot (Part 2):**

`[paste the new Part 2 green build screenshot here]`

### Part 2 Video walkthrough
**YouTube link:** `[unlisted YouTube link]`

The video covers:
- The running API demonstrated through Swagger
- Code structure (models, DbContext, controllers, DTOs, filters)
- Endpoints and how they match the Part 1 plan
- Authentication, session management, and role-based access
- Unit tests, run locally
