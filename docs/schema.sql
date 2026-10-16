-- =======================================================
-- RaceDay System - Part 1 Database Schema
-- Target: Microsoft SQL Server / SSMS
-- =======================================================

USE master;
GO

IF DB_ID(N'RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

-- Drop tables in dependency order so the script can be rerun safely.
IF OBJECT_ID(N'dbo.Results', N'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID(N'dbo.Enrolments', N'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID(N'dbo.Routes', N'U') IS NOT NULL DROP TABLE dbo.Routes;
IF OBJECT_ID(N'dbo.Categories', N'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID(N'dbo.Events', N'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL DROP TABLE dbo.Roles;
GO

CREATE TABLE dbo.Roles (
    RoleId INT IDENTITY(1,1) CONSTRAINT PK_Roles PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_RoleName UNIQUE
);
GO

CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) CONSTRAINT PK_Users PRIMARY KEY,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    RoleId INT NOT NULL,
    Phone VARCHAR(30) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
);
GO

CREATE TABLE dbo.Events (
    EventId INT IDENTITY(1,1) CONSTRAINT PK_Events PRIMARY KEY,
    OrganiserId INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description VARCHAR(MAX) NULL,
    Location VARCHAR(200) NOT NULL,
    EventDate DATETIME2(0) NOT NULL,
    EventType VARCHAR(20) NOT NULL CONSTRAINT DF_Events_EventType DEFAULT 'Run',
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE TABLE dbo.Categories (
    CategoryId INT IDENTITY(1,1) CONSTRAINT PK_Categories PRIMARY KEY,
    EventId INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,
    AgeGroup VARCHAR(100) NULL,
    CONSTRAINT CK_Categories_DistanceKm CHECK (DistanceKm > 0),
    CONSTRAINT CK_Categories_EntryFee CHECK (EntryFee >= 0),
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Routes (
    RouteId INT IDENTITY(1,1) CONSTRAINT PK_Routes PRIMARY KEY,
    CategoryId INT NOT NULL CONSTRAINT UQ_Routes_CategoryId UNIQUE,
    GpxDataUrl VARCHAR(500) NULL,
    ElevationGainMeters INT NULL,
    StartCoordinates VARCHAR(100) NULL,
    WeatherLookupReference VARCHAR(255) NULL,
    CONSTRAINT CK_Routes_ElevationGain CHECK (ElevationGainMeters IS NULL OR ElevationGainMeters >= 0),
    CONSTRAINT FK_Routes_Categories FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories(CategoryId) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.Enrolments (
    EnrolmentId INT IDENTITY(1,1) CONSTRAINT PK_Enrolments PRIMARY KEY,
    ParticipantId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolmentDate DATETIME2(0) NOT NULL CONSTRAINT DF_Enrolments_Date DEFAULT SYSDATETIME(),
    EmergencyContact VARCHAR(100) NOT NULL,
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (ParticipantId, CategoryId),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId)
);
GO

CREATE TABLE dbo.Results (
    ResultId INT IDENTITY(1,1) CONSTRAINT PK_Results PRIMARY KEY,
    EnrolmentId INT NOT NULL CONSTRAINT UQ_Results_EnrolmentId UNIQUE,
    FinishTimeMs BIGINT NOT NULL,
    OverallPlacement INT NULL,
    RecordedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Results_RecordedAt DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Results_FinishTimeMs CHECK (FinishTimeMs >= 0),
    CONSTRAINT CK_Results_OverallPlacement CHECK (OverallPlacement IS NULL OR OverallPlacement > 0),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId)
        REFERENCES dbo.Enrolments(EnrolmentId) ON DELETE CASCADE
);
GO

-- =======================================================
-- SEED DATA
-- 2 Organisers, 2 Participants, 3 Events
-- =======================================================

INSERT INTO dbo.Roles (RoleName)
VALUES ('Organiser'), ('Participant');
GO

INSERT INTO dbo.Users (FirstName, LastName, Email, PasswordHash, RoleId)
VALUES
('Sipho', 'Nkosi', 'sipho.organiser@raceday.co.za', 'MOCK_HASH_CHANGE_IN_APPLICATION', 1),
('Anika', 'Van Zyl', 'anika.organiser@raceday.co.za', 'MOCK_HASH_CHANGE_IN_APPLICATION', 1),
('Thabo', 'Mokoena', 'thabo.runner@gmail.com', 'MOCK_HASH_CHANGE_IN_APPLICATION', 2),
('Liesl', 'Meyer', 'liesl.cyclist@gmail.com', 'MOCK_HASH_CHANGE_IN_APPLICATION', 2);
GO

INSERT INTO dbo.Events (OrganiserId, Title, Description, Location, EventDate, EventType)
VALUES
(1, 'Soweto City Run', 'Annual township marathon celebrating culture and endurance.', 'Soweto, Johannesburg', '2026-11-01T06:00:00', 'Run'),
(1, 'Durban Ocean Challenge', 'Coastal running and walking event along the beachfront.', 'Durban North', '2026-12-05T05:30:00', 'Walk'),
(2, 'Cape Winelands Cycle Tour', 'Scenic cycling route through mountain passes and vineyards.', 'Stellenbosch', '2027-02-14T06:30:00', 'Cycle');
GO

INSERT INTO dbo.Categories (EventId, CategoryName, DistanceKm, EntryFee)
VALUES
(1, '42km Open Marathon', 42.20, 350.00),
(1, '10km Community Walk', 10.00, 150.00),
(2, '21km Half Marathon', 21.10, 250.00),
(3, '90km Road Cycle', 90.00, 500.00);
GO

INSERT INTO dbo.Routes (CategoryId, ElevationGainMeters, StartCoordinates)
VALUES
(1, 450, '-26.2485,27.9016'),
(3, 120, '-29.8587,31.0218');
GO

INSERT INTO dbo.Enrolments (ParticipantId, CategoryId, EmergencyContact)
VALUES
(3, 1, '0821234567'),
(3, 3, '0821234567'),
(4, 4, '0839876543');
GO

INSERT INTO dbo.Results (EnrolmentId, FinishTimeMs, OverallPlacement)
VALUES
(1, 12600000, 14);
GO

-- Verification queries
SELECT * FROM dbo.Roles;
SELECT * FROM dbo.Users;
SELECT * FROM dbo.Events;
SELECT * FROM dbo.Categories;
SELECT * FROM dbo.Routes;
SELECT * FROM dbo.Enrolments;
SELECT * FROM dbo.Results;
GO
