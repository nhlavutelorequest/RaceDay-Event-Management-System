/* ============================================================
   RaceDay Database Schema
   PROG6212 POE - Part 1, Section C
   ============================================================ */

IF DB_ID('RaceDayDB') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

/* ============================================================
   TABLE: Roles
   ============================================================ */
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(20) NOT NULL UNIQUE
);
GO

/* ============================================================
   TABLE: Users
   ============================================================ */
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    RoleId INT NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    ProfileImageUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
GO

/* ============================================================
   TABLE: Events
   ============================================================ */
CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Description NVARCHAR(1000) NULL,
    EventDate DATETIME2 NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EventType NVARCHAR(10) NOT NULL,
    BannerImageUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) REFERENCES Users(UserId),
    CONSTRAINT CK_Events_EventType CHECK (EventType IN ('Run', 'Walk', 'Cycle'))
);
GO

/* ============================================================
   TABLE: Categories
   ============================================================ */
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    CategoryName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200) NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE
);
GO

/* ============================================================
   TABLE: Enrolments
   ============================================================ */
CREATE TABLE Enrolments (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    EnrolledAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantId) REFERENCES Users(UserId),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventId) REFERENCES Events(EventId),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),
    CONSTRAINT UQ_Enrolments_Participant_Event UNIQUE (ParticipantId, EventId)
);
GO

/* ============================================================
   TABLE: Results
   ============================================================ */
CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId INT NOT NULL UNIQUE,
    FinishTime TIME NOT NULL,
    FinishPosition INT NOT NULL,
    CapturedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId) REFERENCES Enrolments(EnrolmentId) ON DELETE CASCADE
);
GO

/* ============================================================
   SEED DATA
   ============================================================ */

-- Roles
INSERT INTO Roles (RoleName) VALUES ('Organiser'), ('Participant');
GO

-- Users: 2 Organisers, 2 Participants
-- NOTE: PasswordHash values below are placeholders representing a hashed password.
INSERT INTO Users (FullName, Email, PasswordHash, RoleId, PhoneNumber, CreatedAt)
VALUES
('Thabo Mokoena', 'thabo.mokoena@raceday.co.za', 'AQAAAAIAAYagAAAAEHash1==', 1, '0821234567', SYSUTCDATETIME()),
('Sarah van der Merwe', 'sarah.vdm@raceday.co.za', 'AQAAAAIAAYagAAAAEHash2==', 1, '0827654321', SYSUTCDATETIME()),
('Lindiwe Nkosi', 'lindiwe.nkosi@gmail.com', 'AQAAAAIAAYagAAAAEHash3==', 2, '0731112222', SYSUTCDATETIME()),
('James Botha', 'james.botha@gmail.com', 'AQAAAAIAAYagAAAAEHash4==', 2, '0733334444', SYSUTCDATETIME());
GO

-- Events: 3 events (run, walk, cycle) - owned by the two Organisers
INSERT INTO Events (OrganiserId, Name, Description, EventDate, Location, DistanceKm, EventType, BannerImageUrl, CreatedAt)
VALUES
(1, 'Pretoria Sunrise 10km Run', 'A scenic early-morning road run through the Union Buildings precinct.', '2026-09-12 06:00:00', 'Pretoria, Gauteng', 10.00, 'Run', NULL, SYSUTCDATETIME()),
(1, 'Tshwane Community Park Walk', 'A family-friendly walk supporting local charities.', '2026-09-20 07:30:00', 'Centurion, Gauteng', 5.00, 'Walk', NULL, SYSUTCDATETIME()),
(2, 'Cape Winelands Cycle Challenge', 'A scenic cycling route through the Cape Winelands.', '2026-10-03 06:30:00', 'Stellenbosch, Western Cape', 42.00, 'Cycle', NULL, SYSUTCDATETIME());
GO

-- Categories: at least one per event
INSERT INTO Categories (EventId, CategoryName, Description)
VALUES
(1, 'Senior', 'Open category, 20 years and older'),
(1, 'Under 20', 'Junior category, under 20 years old'),
(2, '5km Walk', 'Standard 5km walking category'),
(3, '42km Road', 'Full distance road cycling category'),
(3, '21km Road', 'Half distance road cycling category');
GO

-- Enrolments: sample enrolments linking Participants to Events and Categories
INSERT INTO Enrolments (ParticipantId, EventId, CategoryId, Status, EnrolledAt)
VALUES
(3, 1, 1, 'Confirmed', SYSUTCDATETIME()),
(4, 1, 2, 'Pending', SYSUTCDATETIME()),
(3, 3, 4, 'Confirmed', SYSUTCDATETIME()),
(4, 2, 3, 'Confirmed', SYSUTCDATETIME());
GO

-- Results: sample result for a completed enrolment
INSERT INTO Results (EnrolmentId, FinishTime, FinishPosition, CapturedAt)
VALUES
(1, '00:48:32', 47, SYSUTCDATETIME());
GO
SELECT * FROM Users;

/* ============================================================
   VERIFICATION QUERIES ( for manual testing)
   ============================================================ */
-- SELECT * FROM Roles;
-- SELECT * FROM Users;
-- SELECT * FROM Events;
-- SELECT * FROM Categories;
-- SELECT * FROM Enrolments;
-- SELECT * FROM Results;