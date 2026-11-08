# RaceDay Event Management System

### PROG6212 POE - Programming 2B

RaceDay is a full-stack, cloud-aware event management platform built for the South African road running, walking, and cycling community. Event Organisers create and manage events, categories, and participant results. Participants browse upcoming events, enrol by selecting a category, and track their personal race history.

The project was built in three parts. All three parts live in this repository and build on each other:

| Part | Focus |
|---|---|
| Part 1 | System planning: ERD, API endpoint plan, SQL database script |
| Part 2 | RESTful API (ASP.NET Core Web API, EF Core, unit tests, CI) |
| Part 3 | MVC web application, Azure Blob Storage, Docker |

---

## Table of Contents
1. [User Roles](#user-roles)
2. [Part 1 - System Planning and Database](#part-1---system-planning-and-database)
3. [Part 2 - RESTful API Development](#part-2---restful-api-development)
4. [Part 3 - MVC Web Application, Azure Blob Storage and Docker](#part-3---mvc-web-application-azure-blob-storage-and-docker)
5. [Solution Structure](#solution-structure)
6. [Setup and Run Instructions](#setup-and-run-instructions)
7. [Run the Web Application in Docker](#run-the-web-application-in-docker)
8. [Unit Tests](#unit-tests)
9. [CI/CD](#cicd)
10. [Video Walkthroughs](#video-walkthroughs)

---

## User Roles

The system supports two distinct roles. A user chooses their role when registering, and the role is stored in the server session at login.

| Role | Capabilities |
|---|---|
| **Organiser** | Create, edit, and delete their own events (with a banner image); manage event categories; view all enrolments for their events; capture finish times and finishing positions. |
| **Participant** | Register an account; browse and filter upcoming events; enrol in an event by selecting a category; view their own enrolments and their status; view their personal results (time and position); update their profile and profile picture. |

Both roles can register, log in, log out, and view and update their own profile. Each role sees its own navigation menu, and pages belonging to the other role are blocked (a logged-out visitor is redirected to Login, and the wrong role sees an "Access denied" page).

---

## Part 1 - System Planning and Database

All Part 1 planning documents are in the `/docs` folder:

| File | Description |
|---|---|
| `docs/ERD.pdf` | Entity Relationship Diagram for the full RaceDay data model (6 entities). |
| `docs/RaceDay_API_Endpoint_Plan.pdf` | Endpoint plan covering Authentication, User Profile, Events, Categories, Event Enrolments, and Results. |
| `docs/RaceDay_Database.sql` | SQL Server script that creates and seeds the RaceDay database, tested in SSMS. |

---

## Part 2 - RESTful API Development

Part 2 builds the RaceDay API as specified in the Part 1 endpoint plan, using ASP.NET Core Web API (.NET 9) and Entity Framework Core (Code-First) on SQL Server.

### Tech Stack
- ASP.NET Core Web API (.NET 9)
- Entity Framework Core 9.0.0, SQL Server, Code-First with Migrations
- Swagger for live endpoint documentation and testing
- xUnit for unit testing
- GitHub Actions for CI

### Features

**Database (Code-First)**
- Six entities: `Role`, `User`, `Event`, `Category`, `Enrolment`, `Result`.
- Data annotations, foreign keys, and navigation properties match the Part 1 ERD and SQL script.
- Roles use a separate lookup table rather than an enum, as designed in Part 1.
- The schema is created through EF Core migrations.

**Authentication and Sessions**
- Users register with their personal details and select a role (Organiser or Participant).
- Passwords are hashed before storage and are never stored in plain text.
- Login stores the user's ID and role in the server session, which maintains their authenticated state across requests.
- Logging out ends the session.

**Role-Based Access**
- A custom `SessionAuthorize` filter protects all non-public endpoints.
- Unauthenticated requests are rejected with `401 Unauthorized`.
- Requests from the wrong role are rejected with `403 Forbidden`.

**Swagger**
- Every endpoint is listed and can be tested in the browser at `/swagger`.

---

## Part 3 - MVC Web Application, Azure Blob Storage and Docker

Part 3 adds the `RaceDay.Web` ASP.NET Core MVC application. It performs **all** data operations through the Part 2 API using the `ApiClient` service. It has no database access and no duplicated business logic.

**Branch:** `part3-mvc`

### Authentication and role-based navigation
- Register and login pages that call the API. The register page has a live password strength meter and rule checklist.
- The user's ID, name, and role are kept in the session, so every page knows who is logged in.
- A `SessionAuthorize` filter in the MVC project redirects logged-out users to Login and shows "Access denied" to the wrong role.
- Participants and Organisers see different menus, and the link for the current page is highlighted.
- Logout clears the session and returns to the home page.

### Organiser interface
| Page | What it does |
|---|---|
| **Dashboard** | Totals for events, upcoming events, and enrolments, plus a table of the organiser's events with enrolment counts and an Upcoming/Completed tag. |
| **Manage Events** | Create, edit, and delete events (name, description, date, location, distance, type) and upload an event banner image. |
| **Categories** | Add and remove age or distance categories for each event. |
| **Enrolments** | See every participant enrolled in an event, with their category, a colour-coded status, and their result. |
| **Results entry** | Capture the finish time and finishing position for each enrolled participant. |

### Participant interface
| Page | What it does |
|---|---|
| **Home** | Hero section, event types, and all **upcoming events** with search and type filtering. |
| **Events** | Search by name or location and filter by type. Upcoming events are listed first, and finished events are tagged. |
| **Event details** | Full event information, the banner image, and the available categories. A logged-in participant selects a category and enrols, and the enrolment is recorded immediately. |
| **My Enrolments** | All of the participant's enrolments with a colour-coded status. |
| **My Results** | Personal race history: event, date, category, finish time, and position (for example "47th of 312 finishers"). |
| **Profile** | View and update personal information and upload a profile picture. |

### Azure Blob Storage
- Images are stored in an Azure Storage account (container `raceday-images`), with `events/` for banners and `profiles/` for profile pictures.
- The flow is **browser -> MVC -> API -> Azure**. The MVC application never talks to Azure directly. The API (`BlobStorageService`) uploads the file and saves the returned URL in the database (`Events.BannerImageUrl` and `Users.ProfileImageUrl`). MVC then displays the image from that URL.
- Uploads are limited to JPG, PNG, WEBP, or GIF files of up to 5 MB. The API checks the content type and the file's first bytes, so a renamed non-image file is rejected with a friendly message.
- The Azure connection string is a secret. It is **never committed**. It is supplied through .NET user secrets (see [Setup](#setup-and-run-instructions)) or an environment variable.
- If Azure is not configured, the rest of the application still works. Only image uploads show a friendly "image storage is unavailable" message.

### Docker
- The MVC application is containerised with a multi-stage `Dockerfile` in the repository root: the .NET 9 SDK image builds and publishes the app, and the smaller ASP.NET runtime image runs it as a non-root user on port 8080.
- A `.dockerignore` keeps build output and unrelated projects out of the image.
- The container reaches the API running on the host through `host.docker.internal`, and behaves the same as the locally running application. See [Run the Web Application in Docker](#run-the-web-application-in-docker).

### UI design
- Bootstrap 5 for layout and responsiveness, with a custom RaceDay theme (navy and amber, Inter font) in `wwwroot/css/site.css`.
- Consistent page headers, cards, buttons, and spacing on every page.
- Colour-coded enrolment statuses: green for Confirmed, amber for Pending, red for Cancelled.
- Friendly validation messages, a "Something went wrong" page, and a "Page not found" page. Users never see raw errors or stack traces.

### API changes made in Part 3
No tables or columns were added in Part 3, so the database still matches the Part 1 ERD and SQL script. The API gained the endpoints below to support the new pages. They are documented here as deliberate additions to the Part 1 plan.

| Method | Route | Role | Purpose |
|---|---|---|---|
| GET | `/api/events/mine` | Organiser | The organiser's own events with enrolment counts (Dashboard and Manage Events). |
| GET | `/api/users/me` | Any logged in | View own profile, including the profile picture URL. |
| PUT | `/api/users/me` | Any logged in | Update own full name and phone number. |
| POST | `/api/events/{id}/banner` | Organiser (owner) | Upload an event banner to Azure Blob Storage. |
| POST | `/api/users/me/picture` | Any logged in | Upload a profile picture to Azure Blob Storage. |

Two existing endpoints return extra fields: `GET /api/results/me` now includes `totalFinishers` (for "47th of 312"), and `GET /api/events/{eventId}/enrolments` now includes the result (time and position) when one has been captured.

---

## Solution Structure

```
RaceDay-Event-Management-System/
  .github/workflows/     # GitHub Actions CI workflow
  docs/                  # Part 1 planning documents and CI screenshot
  Dockerfile             # Container image for the MVC application
  .dockerignore
  README.md
  RaceDay.Api/
    RaceDay.Api/         # Web API project (https://localhost:5001), solution file lives here
      Controllers/       # API endpoints
      Data/              # RaceDayDbContext
      DTOs/              # Request/response models
      Filters/           # SessionAuthorize role-based access filter
      Migrations/        # EF Core migrations
      Models/            # Role, User, Event, Category, Enrolment, Result
      Services/          # BlobStorageService (Azure Blob Storage)
    RaceDay.Web/         # MVC web application (https://localhost:7035)
      Controllers/       # Page controllers
      Filters/           # Session-based role authorization
      Models/            # View models
      Services/          # ApiClient - all communication with the API
      Views/             # Razor views
      wwwroot/           # CSS, JS, images, video
    RaceDay.Api.Tests/   # Unit tests
```

---

## Setup and Run Instructions

**Prerequisites:** .NET 9 SDK, SQL Server (or SQL Server Express), Visual Studio 2022 or the `dotnet` CLI. Optional: Docker Desktop (for the container) and an Azure Storage account (for image uploads).

**1. Clone the repository**
```bash
git clone https://github.com/nhlavutelorequest/RaceDay-Event-Management-System.git
cd RaceDay-Event-Management-System
```

**2. Configure the database connection** in `RaceDay.Api/RaceDay.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**3. Create the database** from the EF Core migrations:
```bash
cd RaceDay.Api/RaceDay.Api
dotnet ef database update
cd ../..
```
Alternatively, run `docs/RaceDay_Database.sql` in SQL Server Management Studio to create and seed the database.

**4. (Optional) Configure Azure Blob Storage for image uploads**

1. In the Azure portal, create a Storage account, then add a container named `raceday-images` with the anonymous access level **Blob**.
2. Copy the account's connection string (Security + networking -> Access keys) and store it as a user secret. It is not stored in the repository:
```bash
dotnet user-secrets set "AzureBlob:ConnectionString" "<your connection string>" --project RaceDay.Api/RaceDay.Api
```
The container name defaults to `raceday-images`, and can be changed with the `AzureBlob:ContainerName` setting.

**5. Run the API and the web application together**

*Visual Studio:* right-click the solution -> **Configure Startup Projects** -> **Multiple startup projects** -> set both `RaceDay.Api` and `RaceDay.Web` to **Start**, then press **F5**.

*Command line (two terminals):*
```bash
dotnet run --project RaceDay.Api/RaceDay.Api
dotnet run --project RaceDay.Api/RaceDay.Web
```

**6. Open the applications**

| Application | URL |
|---|---|
| Swagger (API) | https://localhost:5001/swagger |
| Web application | https://localhost:7035 |

**7. Try it out:** register one **Organiser** and one **Participant** on the Register page. As the Organiser, create an event and add categories. As the Participant, enrol in it. Then, as the Organiser, capture the result and view it as the Participant under **My Results**.

---

## Run the Web Application in Docker

The container runs the MVC application. The API and the database still run on your machine, so start the API first (step 5 above) so that it is listening on `https://localhost:5001`.

**Build the image** (from the repository root, with Docker Desktop running):
```bash
docker build -t raceday-web .
```

**Run the container (single command):**
```bash
docker run --rm -p 8080:8080 -e ApiBaseUrl=https://host.docker.internal:5001/ -e ApiAllowInvalidCert=true --name raceday-web raceday-web
```

Then open **http://localhost:8080**.

| Setting | Why |
|---|---|
| `ApiBaseUrl=https://host.docker.internal:5001/` | Inside a container, `localhost` is the container itself, so the host machine is reached as `host.docker.internal`. |
| `ApiAllowInvalidCert=true` | The API's development HTTPS certificate is issued for `localhost`. This switch is off by default and is only used for this local Docker run. |

Add `-d` to run in the background, and stop it with `docker stop raceday-web`. The container needs no secrets, because Azure is only used by the API.

---

## Unit Tests

Unit tests cover both success and failure scenarios for:
- User registration and login
- Event management by Organisers
- Rejection of unauthenticated requests and of requests from the wrong role
- Participant enrolment and correct recording of the enrolment

Run all tests:
```bash
dotnet test RaceDay.Api/RaceDay.Api/RaceDay.Api.sln
```
The tests do not need Azure or any secrets.

---

## CI/CD

A GitHub Actions workflow in `.github/workflows/ci.yml` runs on every push and pull request. It:
1. Checks that the required planning documents exist in `/docs`.
2. Restores and builds the whole solution, which includes the API, the MVC project, and the tests.
3. Runs all unit tests.

**Part 3 green build:**

<img width="1341" height="726" alt="image" src="https://github.com/user-attachments/assets/82ced868-1cef-46a2-9118-97c55ff3f322" />

## Video Walkthroughs

| Part | Link |
|---|---|
| Part 1 - Planning and Database | https://youtu.be/fnTeI4-AHi4?si=Loz_87pzdS3DlO0j |
| Part 2 - RESTful API | https://youtu.be/E3xnsPQr90I?si=lu1AK9K7cO_u4KCL |
| Part 3 - MVC, Azure Blob Storage and Docker |https://youtu.be/uBD-ZSGXLBY?si=tZP9m_8SmjUZCI3D |

**Part 3 video covers:**
- The full running application for both roles, including the Docker container
- MVC structure: controllers, views, view models, the `ApiClient` service, and the session authorization filter
- Authentication, session management, and role-based navigation
- Azure Blob Storage: banner and profile picture upload through the API, and the files in the Azure portal
- The Dockerfile and the Docker run command
