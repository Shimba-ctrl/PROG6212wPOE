# RaceDay – PROG6212 Web Programming POE

Shimba Kalenga  
ST10489773  
PROG6212 POE

**Repository:** [PROG6212wPOE](https://github.com/Shimba-ctrl/PROG6212wPOE)

## 1. Project Overview

RaceDay is a web-based event management system designed for the South African running, walking, and cycling community. The system supports event organisers in managing events, categories, participant enrolments, and race results. Participants can register accounts, browse events, enrol in categories, manage their profiles, and view their own results.

This repository contains the project planning and database documentation for Part 1 and the ASP.NET Core Web API implementation for Part 2.

## 2. Technologies Used

- C# and ASP.NET Core Web API (.NET 8)
- Entity Framework Core (Code First)
- Microsoft SQL Server LocalDB
- Session-based authentication and role-based access control
- Swagger / OpenAPI for API documentation and testing
- xUnit for unit testing
- Git and GitHub for version control
- GitHub Actions for continuous integration (CI)

## 3. Repository Structure

```text
PROG6212wPOE/
├── .github/
│   └── workflows/
│       ├── dotnet.yml
│       └── validate-structure.yml
├── Part2-API/
│   └── PROG6212_PART2/
│       ├── PROG6212_PART2.sln
│       ├── PROG6212_PART2/
│       │   ├── Authorization/
│       │   ├── Controllers/
│       │   ├── Data/
│       │   ├── DTOs/
│       │   ├── Models/
│       │   ├── Migrations/
│       │   ├── Program.cs
│       │   └── appsettings.json
│       └── PROG6212_PART2.Tests/
├── docs/
│   ├── raceday_erd.pdf
│   ├── api-endpoint-plan.md
│   └── part1PROG-database.sql
├── README.md
└── docs/screenshots/
```

The structure above describes the intended organisation of the project. Folder and file names should be checked against the actual repository before submission.

## 4. Part 1 – System Planning and Database Design

Part 1 covers the system design and database planning for RaceDay.

The documentation includes:

- Entity Relationship Diagram (ERD) for the six core entities.
- API endpoint plan for the planned system functionality.
- SQL Server database script for creating the database schema and sample data.

The core entities are Users, Events, Categories, Routes, Enrolments, and Results.

### Running the Part 1 SQL script

1. Open SQL Server Management Studio (SSMS).
2. Connect to your SQL Server instance.
3. Open a new query window and select the `master` database.
4. Open `docs/part1PROG-database.sql`.
5. Execute the script.
6. Refresh the Databases section in Object Explorer and verify that the database, tables, and sample data were created.

## 5. Part 2 – Web API Implementation

Part 2 implements the RaceDay backend as a RESTful ASP.NET Core Web API.

### Main functionality

- **Authentication:** Account registration, login, logout, and session checking.
- **User profiles:** View and update the authenticated user's own profile.
- **Events:** View events and manage events according to organiser permissions.
- **Categories:** View categories and manage categories according to organiser permissions.
- **Enrolments:** Allow participants to enrol in categories, view their own enrolments, and cancel their enrolments.
- **Results:** Allow organisers to capture results and participants to view their own results.
- **API documentation:** Swagger provides an interactive interface for inspecting and testing endpoints.

### User roles and permissions

| Functionality | Organiser | Participant |
|---|---|---|
| Register and log in | Yes | Yes |
| View and update own profile | Yes | Yes |
| View events | Yes | Yes |
| Create, update, and delete events | Yes, for authorised events | No |
| Manage event categories | Yes, subject to authorisation | No |
| Create an enrolment | No | Yes |
| View own enrolments | Not applicable | Yes |
| View enrolments for own events | Yes | No access to other organisers' enrolments |
| Capture race results | Yes, for own events | No |
| View own results | Not applicable | Yes |

Protected operations require an authenticated session and the appropriate role. The API checks ownership where applicable to prevent users from accessing resources belonging to other users.

## 6. Setup and Installation

### Prerequisites

Install the following software before running the application:

- Visual Studio 2022 with the ASP.NET and web development workload.
- .NET 8 SDK.
- SQL Server LocalDB.
- Git, if cloning the repository through the command line.

### Step 1: Clone the repository

```bash
git clone https://github.com/Shimba-ctrl/PROG6212wPOE.git
cd PROG6212wPOE
```

### Step 2: Open the solution

Open the solution file in Visual Studio:

`Part2-API/PROG6212_PART2/PROG6212_PART2.sln`

If the solution file is located elsewhere in your checked-out repository, open its actual location.

### Step 3: Configure the database

The API uses Entity Framework Core with SQL Server LocalDB.

Check the `ConnectionStrings` section in `appsettings.json`. The development connection string used for this project is:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\ProjectModels;Database=PROG6212Part2Db;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Ensure that the LocalDB instance exists on your computer. If your SQL Server instance has a different name, update the connection string accordingly.

### Step 4: Restore dependencies

Open a terminal in the repository root and run:

```bash
dotnet restore Part2-API/PROG6212_PART2/PROG6212_PART2.sln
```

### Step 5: Create or update the database

In Visual Studio, open **Tools → NuGet Package Manager → Package Manager Console**.

Set the default project to `PROG6212_PART2`, then run:

```powershell
Update-Database
```

This applies the existing Entity Framework Core migrations to the configured database. If the database is already up to date, no additional changes may be necessary.

### Step 6: Build the solution

From the repository root, run:

```bash
dotnet build Part2-API/PROG6212_PART2/PROG6212_PART2.sln
```

Alternatively, build the solution in Visual Studio.

### Step 7: Run the API

Set `PROG6212_PART2` as the startup project and run it using the HTTPS launch profile.

The development Swagger URL is:

[https://localhost:7251/swagger/index.html](https://localhost:7251/swagger/index.html)

The application may use a different port if the launch settings have been changed.

## 7. Testing the API

Swagger is used to test the available API endpoints.

Recommended testing sequence:

1. Register a test account.
2. Log in using the registered credentials.
3. Check the current session.
4. Test event and category retrieval.
5. Test organiser-only event and category management using an Organiser account.
6. Test participant enrolment using a Participant account.
7. Test enrolment cancellation and result retrieval.
8. Test profile viewing and updating.
9. Test logout and confirm that protected endpoints reject unauthenticated requests.
10. Verify that requests made with the wrong role are denied.

### Unit tests

The solution includes an xUnit test project named `PROG6212_PART2.Tests`.

The tests use EF Core InMemory to test selected functionality without requiring a live SQL Server database for each test. Tests cover session authentication, role checks, unauthenticated event access, organiser-only event creation, and valid and invalid enrolment scenarios.

Run the unit tests from the repository root:

```bash
dotnet test Part2-API/PROG6212_PART2/PROG6212_PART2.sln --configuration Release
```

## 8. Continuous Integration (GitHub Actions)

GitHub Actions is configured to automate the API build and unit tests when changes are pushed to the repository or pull requests are created.

The `dotnet.yml` workflow performs the following steps:

1. Checks out the repository.
2. Sets up the .NET 8 SDK.
3. Restores project dependencies.
4. Builds the solution in Release configuration.
5. Runs the unit tests.

A separate `validate-structure.yml` workflow validates the expected repository structure and documentation.

### Latest successful workflow

View the workflow runs here:

[GitHub Actions – RaceDay POE](https://github.com/Shimba-ctrl/PROG6212wPOE/actions)

**Build and test evidence:** Add a screenshot of the successful workflow run showing a green check mark.

![GitHub Actions successful build and test screenshot](docs/screenshots/github-actions-green.png)

## 9. Screenshots and Evidence

The following screenshots should be included as evidence of the completed implementation:

<img width="624" height="596" alt="image" src="https://github.com/user-attachments/assets/7cd28f4e-37c4-4deb-a8d5-f2655be2259e" />

<img width="602" height="588" alt="image" src="https://github.com/user-attachments/assets/277db136-41e9-45bf-8f85-b11af1f7b578" />

<img width="629" height="581" alt="image" src="https://github.com/user-attachments/assets/9a083e7f-87ef-4047-8a81-71eb745aeee3" />

<img width="588" height="585" alt="image" src="https://github.com/user-attachments/assets/532f04d6-40af-418a-9d5b-c9d73f87e959" />

<img width="580" height="578" alt="image" src="https://github.com/user-attachments/assets/8c5a306e-2f61-46b6-8373-0c50585b6ac0" />

<img width="454" height="574" alt="image" src="https://github.com/user-attachments/assets/d8cff62b-21f4-41f2-9ad3-a6dc1549b66a" />


## 10. Demonstration Video – Part 2

**Video link:** [Add your unlisted Part 2 demonstration video link here]

The demonstration should show:

1. A brief introduction to the RaceDay system and its purpose.
2. The solution structure in Visual Studio.
3. The database configuration and Entity Framework Core migrations.
4. The API running in Swagger.
5. Registration, login, session handling, and logout.
6. Organiser and Participant role-based access.
7. Event, category, enrolment, profile, and results functionality.
8. Unit tests running successfully.
9. The successful GitHub Actions workflow.


## 11. Part 1 Demonstration Video

The existing Part 1 presentation is available here:

(https://youtu.be/5qC4FpsGJ0k)

This video covers the system design, ERD, endpoint planning, and database script demonstration.

## 12. Version Control

Git and GitHub are used to maintain the project history and track changes. GitHub Actions provides automated validation of the repository and the API build and test process.

Repository: [https://github.com/Shimba-ctrl/PROG6212wPOE](https://github.com/Shimba-ctrl/PROG6212wPOE)

Before final submission, verify that the repository contains the complete source code, test project, workflow files, documentation, screenshots, and the required demonstration video link.

## 13. Conclusion

The RaceDay project applies database design, RESTful API development, Entity Framework Core, session-based authentication, role-based access control, automated testing, and continuous integration to a road-event management scenario.

Part 1 documents the system design and database planning, while Part 2 implements and tests the API functionality.
