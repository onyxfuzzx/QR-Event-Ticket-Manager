CREATE DATABASE QREventDb;
GO

USE QREventDb;
GO

/* ===============================
   AUDIT LOGS
================================ */
CREATE TABLE dbo.AuditLogs (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    Entity NVARCHAR(50) NOT NULL,
    EntityId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2(7) NOT NULL
        CONSTRAINT DF_AuditLogs_CreatedAt
        DEFAULT (DATEADD(MINUTE, 330, GETUTCDATE()))
);
GO

/* ===============================
   USERS
================================ */
CREATE TABLE dbo.Users (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role INT NOT NULL,
    IsActive BIT NOT NULL
        CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt DATETIME2(7) NOT NULL
        CONSTRAINT DF_Users_CreatedAt DEFAULT (GETUTCDATE()),
    CreatedByAdminId UNIQUEIDENTIFIER NULL,
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

/* ===============================
   EVENTS
================================ */
CREATE TABLE dbo.Events (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    EventDate DATETIME2(7) NOT NULL,
    PublicId UNIQUEIDENTIFIER NOT NULL,
    SecretKey UNIQUEIDENTIFIER NOT NULL,
    CreatedByAdminId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL
        CONSTRAINT DF_Events_CreatedAt DEFAULT (GETUTCDATE()),
    Location NVARCHAR(200) NULL,
    Tickets INT NOT NULL,
    UsedTickets INT NOT NULL,
    IsActive BIT NOT NULL
        CONSTRAINT DF_Events_IsActive DEFAULT (1),
    CONSTRAINT UQ_Events_PublicId UNIQUE (PublicId),
    CONSTRAINT UQ_Events_SecretKey UNIQUE (SecretKey)
);
GO

/* ===============================
   EVENT WORKERS
================================ */
CREATE TABLE dbo.EventWorkers (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL,
    WorkerId UNIQUEIDENTIFIER NOT NULL,
    AssignedAt DATETIME2(7) NOT NULL
        CONSTRAINT DF_EventWorkers_AssignedAt
        DEFAULT (DATEADD(MINUTE, 330, GETUTCDATE())),
    IsActive BIT NOT NULL
        CONSTRAINT DF_EventWorkers_IsActive DEFAULT (1),
    CONSTRAINT UQ_EventWorkers UNIQUE (EventId, WorkerId)
);
GO

/* ===============================
   TICKETS
================================ */
CREATE TABLE dbo.Tickets (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL,
    Code NVARCHAR(100) NOT NULL,
    IsUsed BIT NOT NULL
        CONSTRAINT DF_Tickets_IsUsed DEFAULT (0),
    UsedAt DATETIME2(7) NULL,
    UsedByWorkerId UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2(7) NOT NULL
        CONSTRAINT DF_Tickets_CreatedAt
        DEFAULT (DATEADD(MINUTE, 330, GETUTCDATE())),
    IsActive BIT NOT NULL
        CONSTRAINT DF_Tickets_IsActive DEFAULT (1),
    CONSTRAINT UQ_Tickets_Code UNIQUE (Code)
);
GO

/* ===============================
   TICKET REVALIDATIONS
================================ */
CREATE TABLE dbo.TicketRevalidations (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    TicketId UNIQUEIDENTIFIER NOT NULL,
    WorkerId UNIQUEIDENTIFIER NOT NULL,
    RevalidatedAt DATETIME2(7) NOT NULL
        CONSTRAINT DF_TicketRevalidations_RevalidatedAt
        DEFAULT (GETUTCDATE())
);
GO

/* ===============================
   TICKET SCAN LOGS
================================ */
CREATE TABLE dbo.TicketScanLogs (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    TicketId UNIQUEIDENTIFIER NULL,
    TicketCode NVARCHAR(100) NOT NULL,
    EventId UNIQUEIDENTIFIER NOT NULL,
    EventName NVARCHAR(200) NOT NULL,
    WorkerId UNIQUEIDENTIFIER NOT NULL,
    WorkerName NVARCHAR(200) NOT NULL,
    AdminId UNIQUEIDENTIFIER NOT NULL,
    ScanResult NVARCHAR(20) NOT NULL,
    ScanSource NVARCHAR(50) NULL,
    ScannedAt DATETIME2(7) NOT NULL
        CONSTRAINT DF_TicketScanLogs_ScannedAt
        DEFAULT (GETUTCDATE())
);
GO

CREATE INDEX IX_TicketScanLogs_ScannedAt
ON dbo.TicketScanLogs (ScannedAt DESC);
GO

/* ===============================
   NOTIFICATIONS
================================ */
CREATE TABLE dbo.Notifications (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    IsRead BIT NOT NULL
        CONSTRAINT DF_Notifications_IsRead DEFAULT (0),
    CreatedAt DATETIME2(7) NOT NULL
        CONSTRAINT DF_Notifications_CreatedAt
        DEFAULT (DATEADD(MINUTE, 330, GETUTCDATE()))
);
GO
