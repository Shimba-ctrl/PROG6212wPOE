/* ============================================================
   RaceDay Database Script
   Run this in SQL Server Management Studio (SSMS) on a clean
   SQL Server instance. Creates the PROG6212wPOE database, all
   tables matching the ERD, and seeds sample data.

   NOTE: This script can be re-run safely - it drops and
   recreates the database each time. Make sure your query
   window is NOT currently using PROG6212wPOE as its active
   database when you run this (the USE master below handles
   that automatically).
   ============================================================ */

-- Switch to master first so we're not "inside" the database we're about to drop
USE master;
GO

-- Create a fresh database
IF DB_ID('PROG6212wPOE') IS NOT NULL
BEGIN
    ALTER DATABASE PROG6212wPOE SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PROG6212wPOE;
END
GO

CREATE DATABASE PROG6212wPOE;
GO

USE PROG6212wPOE;
GO

/* ============================================================
   TABLE: Users
   Stores both Organisers and Participants, distinguished by Role.
   ============================================================ */
CREATE TABLE Users (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    FullName        VARCHAR(100)    NOT NULL,
    Email           VARCHAR(150)    NOT NULL UNIQUE,
    PasswordHash    VARCHAR(255)    NOT NULL,
    Role            VARCHAR(20)     NOT NULL
                        CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser', 'Participant')),
    CreatedAt       DATETIME        NOT NULL DEFAULT GETDATE()
);
GO

/* ============================================================
   TABLE: Events
   Each Event is created by one Organiser (a User with Role = Organiser).
   ============================================================ */
CREATE TABLE Events (
    EventId         INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId     INT             NOT NULL,
    Name            VARCHAR(150)    NOT NULL,
    Description     VARCHAR(1000)   NULL,
    EventDate       DATE            NOT NULL,
    Location        VARCHAR(150)    NOT NULL,
    CreatedAt       DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserId)
        REFERENCES Users(UserId)
);
GO

/* ============================================================
   TABLE: Categories
   Each Category belongs to exactly one Event (e.g. "21km", "Seniors 50+").
   ============================================================ */
CREATE TABLE Categories (
    CategoryId      INT IDENTITY(1,1) PRIMARY KEY,
    EventId         INT             NOT NULL,
    Name            VARCHAR(100)    NOT NULL,
    Distance        DECIMAL(6,2)    NOT NULL,
    MaxParticipants INT             NULL,
    CONSTRAINT FK_Categories_Event FOREIGN KEY (EventId)
        REFERENCES Events(EventId)
);
GO

/* ============================================================
   TABLE: Routes
   Route and elevation details for an Event (supports the
   live weather/route info feature).
   ============================================================ */
CREATE TABLE Routes (
    RouteId          INT IDENTITY(1,1) PRIMARY KEY,
    EventId          INT             NOT NULL,
    StartLocation    VARCHAR(150)    NOT NULL,
    EndLocation      VARCHAR(150)    NOT NULL,
    DistanceKm       DECIMAL(6,2)    NOT NULL,
    ElevationGain    DECIMAL(6,2)    NULL,
    RouteDescription VARCHAR(1000)  NULL,
    CONSTRAINT FK_Routes_Event FOREIGN KEY (EventId)
        REFERENCES Events(EventId)
);
GO

/* ============================================================
   TABLE: Enrolments
   Resolves the many-to-many relationship between Participants
   (Users) and Categories - a Participant enters an Event by
   selecting a Category.
   ============================================================ */
CREATE TABLE Enrolments (
    EnrolmentId     INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId   INT             NOT NULL,
    CategoryId      INT             NOT NULL,
    EnrolmentDate   DATETIME        NOT NULL DEFAULT GETDATE(),
    Status          VARCHAR(20)     NOT NULL DEFAULT 'Confirmed'
                        CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Confirmed', 'Cancelled')),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantId)
        REFERENCES Users(UserId),
    CONSTRAINT FK_Enrolments_Category FOREIGN KEY (CategoryId)
        REFERENCES Categories(CategoryId),
    -- Prevent the same participant enrolling in the same category twice
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (ParticipantId, CategoryId)
);
GO

/* ============================================================
   TABLE: Results
   One Result per Enrolment (enforced via UNIQUE on EnrolmentId),
   captured by an Organiser.
   ============================================================ */
CREATE TABLE Results (
    ResultId                INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId             INT             NOT NULL UNIQUE,
    FinishTime              TIME            NOT NULL,
    Position                INT             NULL,
    CapturedByOrganiserId   INT             NOT NULL,
    CONSTRAINT FK_Results_Enrolment FOREIGN KEY (EnrolmentId)
        REFERENCES Enrolments(EnrolmentId),
    CONSTRAINT FK_Results_Organiser FOREIGN KEY (CapturedByOrganiserId)
        REFERENCES Users(UserId)
);
GO

/* ============================================================
   SEED DATA
   ============================================================ */

-- 2 Organisers, 2 Participants
INSERT INTO Users (FullName, Email, PasswordHash, Role) VALUES
('Thabo Nkosi',    'thabo.nkosi01' + '@raceday-test.co.za',    'hashed_password_1', 'Organiser'),
('Lindiwe Dlamini', 'lindiwe.dlamini02' + '@raceday-test.co.za', 'hashed_password_2', 'Organiser'),
('Sarah van Wyk',   'sarah.vanwyk03' + '@raceday-test.co.za',  'hashed_password_3', 'Participant'),
('Michael Botha',   'michael.botha04' + '@raceday-test.co.za',  'hashed_password_4', 'Participant');
GO

-- 3 Events, each created by an Organiser
INSERT INTO Events (OrganiserId, Name, Description, EventDate, Location) VALUES
(1, 'Comrades Marathon 2027',   'Iconic ultramarathon between Durban and Pietermaritzburg.', '2027-06-13', 'Durban, KwaZulu-Natal'),
(1, 'Cape Town Cycle Tour 2027', 'One of the largest timed cycle races in the world.',       '2027-03-08', 'Cape Town, Western Cape'),
(2, 'Soweto Marathon 2027',      'Community road race through the streets of Soweto.',       '2027-11-07', 'Soweto, Gauteng');
GO

-- Categories for each event
INSERT INTO Categories (EventId, Name, Distance, MaxParticipants) VALUES
(1, 'Up Run 87km',      87.00,  20000),
(1, 'Novice 87km',      87.00,  5000),
(2, 'Full Cycle 109km', 109.00, 35000),
(2, 'Half Cycle 55km',  55.00,  10000),
(3, '42km Marathon',    42.20,  8000),
(3, '10km Fun Run',     10.00,  4000);
GO

-- Route details for each event
INSERT INTO Routes (EventId, StartLocation, EndLocation, DistanceKm, ElevationGain, RouteDescription) VALUES
(1, 'Pietermaritzburg City Hall', 'Kingsmead Stadium, Durban', 87.00, 1200.00, 'Point-to-point route through the KZN Midlands with several major climbs.'),
(2, 'Grand Parade, Cape Town',    'Grand Parade, Cape Town',   109.00, 950.00, 'Loop route around the Cape Peninsula via Chapmans Peak.'),
(3, 'FNB Stadium, Soweto',        'FNB Stadium, Soweto',       42.20,  180.00, 'Loop route through the historic streets of Soweto.');
GO

-- Sample enrolments (Participants entering Categories)
INSERT INTO Enrolments (ParticipantId, CategoryId, Status) VALUES
(3, 1, 'Confirmed'),  -- Sarah entering Comrades Up Run
(3, 5, 'Confirmed'),  -- Sarah entering Soweto Marathon
(4, 3, 'Confirmed'),  -- Michael entering Cape Town Cycle Tour Full
(4, 6, 'Confirmed');  -- Michael entering Soweto 10km Fun Run
GO

-- Sample captured result (one enrolment already has a result)
INSERT INTO Results (EnrolmentId, FinishTime, Position, CapturedByOrganiserId) VALUES
(1, '08:45:12', 1523, 1);
GO

/* ============================================================
   Quick verification queries (optional - run to sanity check)
   ============================================================ */
-- SELECT * FROM Users;
-- SELECT * FROM Events;
-- SELECT * FROM Categories;
-- SELECT * FROM Routes;
-- SELECT * FROM Enrolments;
-- SELECT * FROM Results;
