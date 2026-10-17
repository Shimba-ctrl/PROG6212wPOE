# RaceDay API Endpoint Plan

Roles: **None** (public, no login required), **Any** (any logged-in user), **Organiser**, **Participant**.
"(own event)" / "(own enrolment)" means the logged-in user must be the owner of that specific record, not just hold the role.

## Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Creates a new user account as either an Organiser or a Participant. | None | { FullName, Email, Password, Role } | 201 Created – new user + token · 400 Bad Request – invalid data or email already in use |
| POST | /api/auth/login | Authenticates a user and returns an access token. | None | { Email, Password } | 200 OK – token + user info · 401 Unauthorized – invalid credentials |

## User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/users/me | Returns the profile of the currently logged-in user. | Any | None | 200 OK – user profile · 401 Unauthorized |
| PUT | /api/users/me | Updates the profile of the currently logged-in user. | Any | { FullName, Email } | 200 OK – updated profile · 400 Bad Request |

## Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events | Lists all upcoming events. Supports browsing for Participants. | None | None | 200 OK – list of events |
| GET | /api/events/{id} | Returns full details for a single event. | None | None | 200 OK – event details · 404 Not Found |
| POST | /api/events | Creates a new event. | Organiser | { Name, Description, EventDate, Location } | 201 Created – new event |
| PUT | /api/events/{id} | Updates an existing event. | Organiser (own event) | { Name, Description, EventDate, Location } | 200 OK – updated event · 403 Forbidden · 404 Not Found |
| DELETE | /api/events/{id} | Deletes an event. | Organiser (own event) | None | 204 No Content · 403 Forbidden · 404 Not Found |

## Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events/{eventId}/categories | Lists all categories available for a specific event. | None | None | 200 OK – list of categories |
| POST | /api/events/{eventId}/categories | Adds a new category to an event. | Organiser (own event) | { Name, Distance, MaxParticipants } | 201 Created – new category |
| PUT | /api/categories/{id} | Updates a category's details. | Organiser (own event) | { Name, Distance, MaxParticipants } | 200 OK – updated category · 403 Forbidden · 404 Not Found |
| DELETE | /api/categories/{id} | Deletes a category. | Organiser (own event) | None | 204 No Content · 403 Forbidden · 404 Not Found |

## Routes (route & elevation info per event)

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | /api/events/{eventId}/route | Returns route details (start/end, distance, elevation) for an event. | None | None | 200 OK – route details · 404 Not Found |
| POST | /api/events/{eventId}/route | Adds or replaces route details for an event. | Organiser (own event) | { StartLocation, EndLocation, DistanceKm, ElevationGain, RouteDescription } | 201 Created – route details · 403 Forbidden |

## Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/enrolments | Enters the logged-in Participant into an event by selecting a category. | Participant | { CategoryId } | 201 Created – new enrolment · 409 Conflict – already enrolled in this category |
| GET | /api/enrolments/me | Lists the logged-in Participant's own enrolments. | Participant | None | 200 OK – list of enrolments |
| GET | /api/events/{eventId}/enrolments | Lists all enrolments for an event, for the Organiser managing it. | Organiser (own event) | None | 200 OK – list of enrolments · 403 Forbidden |
| DELETE | /api/enrolments/{id} | Cancels the logged-in Participant's own enrolment. | Participant (own enrolment) | None | 204 No Content · 403 Forbidden · 404 Not Found |

## Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/results | Captures a result for a participant's enrolment. | Organiser (own event) | { EnrolmentId, FinishTime, Position } | 201 Created – new result · 409 Conflict – result already captured |
| GET | /api/results/me | Lists the logged-in Participant's personal result history. | Participant | None | 200 OK – list of results |
| GET | /api/events/{eventId}/results | Lists all results for an event. | Organiser (own event) | None | 200 OK – list of results · 403 Forbidden |
| PUT | /api/results/{id} | Corrects a previously captured result. | Organiser (own event) | { FinishTime, Position } | 200 OK – updated result · 403 Forbidden · 404 Not Found |
