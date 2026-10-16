# RaceDay — Part 1 + Part 2

RaceDay is a race-event management REST API built with ASP.NET Core Web API, Entity Framework Core, SQL Server, JWT authentication, role-based authorization, Swagger/OpenAPI, and automated tests.

## Part 2 implementation

- Registration and login with PBKDF2 password hashing; plain-text passwords are never stored or returned.
- JWT bearer authentication with `Organiser` and `Participant` roles.
- User profile retrieval and update.
- Public event listing/details; organisers can create, update, and delete their own events.
- Event categories for distance/age groups.
- Participant enrolment, with duplicate-enrolment protection.
- Organiser-only enrolment listing for their events.
- Organiser result capture and participant-only personal results.
- Route metadata including GPX URL, elevation, start coordinates, and a weather lookup reference.
- Swagger UI with JWT bearer authorization.
- xUnit tests and GitHub Actions CI build/test workflow.

## Requirements

- .NET 8 SDK
- Visual Studio 2022 (ASP.NET and web development workload) or VS Code
- SQL Server / SQL Server Express / LocalDB
- Git and a GitHub account

## Run locally in Visual Studio

1. Open `RaceDay.sln`.
2. Confirm the connection string in `src/RaceDay.Api/appsettings.json` matches your SQL Server installation.
3. Set `RaceDay.Api` as the startup project.
4. Run the project. In Development, EF Core creates the database schema if the database is new.
5. Open Swagger at the launch URL, normally `https://localhost:7190/swagger`.

The default connection string uses SQL Server LocalDB. If you use SQL Server Express, change it to an appropriate connection string, for example:
`Server=.\SQLEXPRESS;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;`

> Important: `EnsureCreated()` is intended for a fresh development database and does not update an existing database schema. If you already created the Part 1 database manually, use a fresh database for this EF Code-First demo or create and apply EF Core migrations before using an existing database.

## First-time API use

1. `POST /api/auth/register` to create an account.
2. For the role, use `Organiser` or `Participant`.
3. `POST /api/auth/login` with the same email/password.
4. Copy the returned `token`.
5. In Swagger, click **Authorize** and paste the token (or `Bearer <token>` if prompted).
6. Test the endpoints permitted to that role.

Example registration body:
```json
{
  "firstName": "Ayesha",
  "lastName": "Runner",
  "email": "ayesha@example.test",
  "password": "StrongPass!123",
  "role": "Participant"
}
```

Registration currently allows a caller to choose either supported role to align with the assignment's registration requirement. For a real public deployment, organiser accounts should instead be approved by an administrator.

## API endpoint overview

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| GET / PUT | `/api/users/profile` | Authenticated |
| GET | `/api/events` | Public |
| GET | `/api/events/{id}` | Public |
| POST | `/api/events` | Organiser |
| PUT / DELETE | `/api/events/{id}` | Owning Organiser |
| POST | `/api/events/{id}/categories` | Owning Organiser |
| GET | `/api/events/{id}/enrolments` | Owning Organiser |
| POST | `/api/enrolments` | Participant |
| GET | `/api/enrolments/me` | Participant |
| POST | `/api/results` | Organiser of event |
| GET | `/api/results/me` | Participant |
| GET | `/api/categories/{categoryId}/route` | Public |
| PUT | `/api/categories/{categoryId}/route` | Organiser |
| GET | `/health` | Public |

## Database model

The Code-First model includes the seven Part 1 entities: Roles, Users, Events, Categories, Routes, Enrolments, and Results. Entity relationships, unique constraints, and seeded roles are configured in `RaceDayDbContext`.

The original Part 1 planning deliverables remain in `docs/`:
- `docs/ERD.svg`
- `docs/EndpointPlan.md`
- `docs/schema.sql`

## Build and test

From the repository root:

```bash
dotnet restore RaceDay.sln
dotnet build RaceDay.sln --configuration Release
dotnet test RaceDay.sln --configuration Release
```

The tests cover password hashing/verification, seeded roles and relationships, registration response safety, event creation, and role-based rejection. GitHub Actions runs restore, build, and tests on pushes and pull requests.

## GitHub submission checklist

- [ ] Push the repository to your own GitHub account.
- [ ] Confirm the **RaceDay API CI** workflow passes under the Actions tab.
- [ ] Add a screenshot of the successful green workflow run to this README or `docs/ci-success.png` after your own workflow passes.
- [ ] Record and add your unlisted YouTube demonstration link after recording the running API, Swagger endpoints, authentication, role restrictions, and tests.
- [ ] Make at least 20 meaningful commits from your own GitHub account as required by the assignment. Commits should reflect genuine development steps, not empty/repeated changes.

## Suggested demonstration flow

1. Show the solution and folder structure.
2. Run the API and open Swagger.
3. Register/login as Participant and show that organiser-only event creation is forbidden.
4. Register/login as Organiser and create an event/category.
5. Enrol the participant and record/view results.
6. Run `dotnet test` and show the passing test output.
7. Show the successful GitHub Actions run.

## Security and deployment notes

The key in `appsettings.json` is for local development only. Replace it with a long random secret stored in user-secrets or environment variables before deployment. Use HTTPS in production, configure a production SQL Server connection securely, and do not commit real credentials or personal data.


## First-time setup (Visual Studio 2022)

1. Install the **.NET 8 SDK** and Visual Studio 2022 with the **ASP.NET and web development** workload.
2. Open `RaceDay.sln`. In Solution Explorer, right-click the solution and choose **Restore NuGet Packages**.
3. In SQL Server Management Studio (SSMS), run `docs/schema.sql` against your local SQL Server. This script recreates the tables and includes the columns used by the API. **It deletes/recreates the project tables and seed data in RaceDayDB**, so do not run it against data you need to keep.
4. Check that `src/RaceDay.Api/appsettings.json` uses a SQL Server instance available on your computer. The default is `(localdb)\\MSSQLLocalDB`, which is available with many Visual Studio installations. If you use a different instance, update the `RaceDay` connection string.
5. Set `RaceDay.Api` as the startup project and press **F5**. Swagger should open. If it does not, browse to `https://localhost:7190/swagger` or use the URL shown in the Output window.
6. To run tests, choose **Test → Run All Tests** in Visual Studio, or run `dotnet test` in a terminal from the folder containing `RaceDay.sln`.


