PROG6212w Part 1
Shimba Kalenga 
ST10489773 

Description

RaceDay is a full-stack web-based event management system built for the South African road running, walking, and cycling community. It allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter events, track their personal performance history, and view live weather and route information ahead of race day.


RaceDay supports two distinct user roles:

Organiser - Can create, edit, and delete events, manage event categories, capture participant results, and view all enrolments for their events.
Participant - Can create an account, browse events, enter an event by selecting a category, view their own enrolments, and track their personal results.

Role-based access is planned at the API level (enforced in Part 2) and will be reflected consistently in the MVC interface (Part 3).

Part 1 Contents (/docs)

File	Description

raceday_erd.pdf	Entity Relationship Diagram covering all 6 core entities, their attributes, and relationships.
api-endpoint-plan.md	Full API endpoint plan covering Authentication, User Profile, Events, Categories, Routes, Enrolments, and Results.
part1PROG-database.sql	SQL Server script that creates the PROG6212wPOE database schema and seeds it with sample data.
Setup Instructions - Running the SQL Script
Open SQL Server Management Studio (SSMS) and connect to your SQL Server instance.
Open a New Query window and make sure the active database is set to master (important - do not run this while connected inside a database of the same name).
Open docs/part1PROG-database.sql, select all the text, and paste it into the query window.
Press F5 (or click Execute) to run the full script.
Check the Messages tab - you should see the database and all six tables created successfully, followed by row-count confirmations for the seed data (Users, Events, Categories, Routes, Enrolments, Results).
In Object Explorer, refresh Databases, expand PROG6212wPOE -> Tables, and right-click any table -> "Select Top 1000 Rows" to confirm the seed data loaded correctly.
CI/CD

A GitHub Actions workflow validates the repository structure on every push, checking that /docs exists and contains the ERD, API endpoint plan, and SQL script, and that README.md exists at the repo root.

Show Image

Video Presentation
<!-- TODO: Replace with your unlisted YouTube link once recorded. -->

Video walkthrough: [Add YouTube link here]

The video covers:

An explanation of the ERD and the design decisions behind it.
A walkthrough of the API endpoint plan and how it maps to the two user roles.

AI Tool Disclosure

I used Claude throughout the planning process for Part 1: to help reason through the ERD structure and relationships, understand what is needed for the API endpoint plan table, write and debug the SQL database script (including troubleshooting SSMS errors), and set up my Git/GitHub workflow and the GitHub Actions CI/CD file. All final design decisions, understanding, and explanations in the accompanying video are my own.

A live run of the SQL script in SSMS, showing the database and seed data being created successfully.


