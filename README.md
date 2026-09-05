PROG6212w Part 1
Shimba Kalenga 
ST10489773 
Description

RaceDay is a full-stack web-based event management system built for the South African road running, walking, and cycling community. It allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter events, track their personal performance history, and view live weather and route information ahead of race day.

This repository is submitted as a Portfolio of Evidence (POE) across three parts:

Part 1 - System planning: Entity Relationship Diagram, API endpoint plan, and SQL database script.
Part 2 - A RESTful API built in C#, connected to the database, with unit tests and CI/CD.
Part 3 - An MVC web application consuming the API, with Azure Blob Storage integration and Docker containerisation.
User Roles

RaceDay supports two distinct user roles:

Organiser - Can create, edit, and delete events, manage event categories, capture participant results, and view all enrolments for their events.

Participant - Can create an account, browse events, enter an event by selecting a category, view their own enrolments, and track their personal results.

Role-based access is planned at the API level (enforced in Part 2) and will be reflected consistently in the MVC interface (Part 3).

Part 1 Contents (docs)

File	Description

raceday_erd.pdf	

Entity Relationship Diagram covering all 6 core entities, their attributes, and relationships.

api-endpoint-plan.md	

Full API endpoint plan covering Authentication, User Profile, Events, Categories, Routes, Enrolments, and Results.

part1PROG-database.sql

SQL Server script that creates the PROG6212wPOE database schema and seeds it with sample data.


Setup Instructions - Running the SQL Script


Check the Messages tab - you should see the database and all six tables created successfully, followed by row-count confirmations for the seed data (Users, Events, Categories, Routes, Enrolments, Results).


In Object Explorer, refresh Databases, expand PROG6212wPOE -> Tables, and right-click any table -> "Select Top 1000 Rows" to confirm the seed data loaded correctly.
CI/CD


Video Presentation


Video LINK : 

The video covers:

An explanation of the ERD and the design decisions behind it.
A walkthrough of the API endpoint plan and how it maps to the two user roles.
A live run of the SQL script in SSMS, showing the database and seed data being created successfully.

