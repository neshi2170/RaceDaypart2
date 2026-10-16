# Part 2 Implementation Notes

## Architecture
- ASP.NET Core 8 Web API controllers
- EF Core Code First with SQL Server provider
- JWT bearer tokens and role claims
- PBKDF2-SHA256 password hashing with per-password random salts
- Swagger/OpenAPI for browser-based endpoint testing
- xUnit tests and GitHub Actions workflow

## Authorization rules
- Public: register, login, event browse/details, health, route read.
- Any authenticated user: read/update their own profile.
- Organiser: create events/categories, update/delete only events they own, view enrolments for their own events, record results for their own event, update route metadata.
- Participant: enrol in a category and view their own enrolments/results.

## Test scope
The test project checks password hash verification, seeded roles and model relationships, registration response safety, participant denial for organiser-only actions, and organiser event creation/public visibility.

## Before final submission
Run the solution on your own computer and fix any environment-specific SQL Server connection-string issue. Save your actual passing GitHub Actions screenshot and add your unlisted YouTube demonstration URL to the README. Do not claim a green build until your own workflow has completed successfully.
