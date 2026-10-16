# RaceDay API Endpoint Plan

## Overview

This document defines the planned REST API endpoints for the RaceDay platform. Endpoints are grouped by authentication, users, events, enrolments and results.

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Registers a new user account | Public | `{ email, password, firstName, lastName, role }` | `201 Created` |
| POST | `/api/auth/login` | Authenticates a user and returns a JWT | Public | `{ email, password }` | `200 OK` |
| GET | `/api/users/profile` | Retrieves the current user's profile | Authenticated | None | `200 OK` |
| PUT | `/api/users/profile` | Updates personal profile information | Authenticated | `{ firstName, lastName, phone }` | `200 OK` |
| GET | `/api/events` | Browses upcoming events with filtering | Public | None | `200 OK` |
| GET | `/api/events/{id}` | Gets details for one event and its categories | Public | None | `200 OK` |
| POST | `/api/events` | Creates a new race event | Organiser | `{ title, description, date, location }` | `201 Created` |
| PUT | `/api/events/{id}` | Edits an existing race event | Organiser | `{ title, description, date, location }` | `200 OK` |
| DELETE | `/api/events/{id}` | Removes an event | Organiser | None | `204 No Content` / `404 Not Found` |
| POST | `/api/events/{id}/categories` | Adds a race category/distance | Organiser | `{ categoryName, distanceKm, fee }` | `201 Created` |
| GET | `/api/events/{id}/enrolments` | Views participant entries for an event | Organiser | None | `200 OK` |
| POST | `/api/enrolments` | Enrols a participant in a category | Participant | `{ categoryId, emergencyContact }` | `201 Created` |
| GET | `/api/enrolments/me` | Retrieves the participant's entry history | Participant | None | `200 OK` |
| POST | `/api/results` | Records a participant's race result | Organiser | `{ enrolmentId, finishTime, placement }` | `201 Created` |
| GET | `/api/results/me` | Retrieves the participant's historical results | Participant | None | `200 OK` |

## Authentication and Authorisation

- Public endpoints do not require authentication.
- Authenticated endpoints require a valid bearer token.
- Organiser endpoints require the Organiser role.
- Participant endpoints require the Participant role.
- The API should validate ownership before allowing an organiser to modify their events or results.

## Planned Response Conventions

- `200 OK` — successful retrieval/update.
- `201 Created` — successful creation.
- `204 No Content` — successful deletion with no response body.
- `400 Bad Request` — invalid request data.
- `401 Unauthorized` — missing or invalid authentication.
- `403 Forbidden` — authenticated user lacks the required role.
- `404 Not Found` — requested resource does not exist.
- `409 Conflict` — duplicate or conflicting operation.
- `500 Internal Server Error` — unexpected server-side failure.
