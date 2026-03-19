/*
   To create a new DB, uncomment lines below:
   CREATE DATABASE QREventDb;
   GO
   USE QREventDb;
   GO
*/

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
    QrUrl NVARCHAR(500) NULL,
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
   EVENT FORMS
================================ */
CREATE TABLE dbo.EventForms (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL,
    [Schema] NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
    UpdatedAt DATETIME2(7) NULL,
    CONSTRAINT FK_EventForms_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id)
);
GO

/* ===============================
   EVENT FORM SUBMISSIONS
================================ */
CREATE TABLE dbo.EventFormSubmissions (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL,
    TicketId UNIQUEIDENTIFIER NOT NULL,
    Data NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT FK_Submissions_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id),
    CONSTRAINT FK_Submissions_Tickets FOREIGN KEY (TicketId) REFERENCES dbo.Tickets(Id)
);
GO

/* ===============================
   EMAIL TEMPLATES
================================ */
CREATE TABLE dbo.EmailTemplates (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EventId UNIQUEIDENTIFIER NOT NULL,
    LayoutJson NVARCHAR(MAX) NOT NULL,
    HtmlContent NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
    UpdatedAt DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT FK_EmailTemplates_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id)
);
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

/* ===============================
   PASSWORD RESET TOKENS
================================ */
CREATE TABLE dbo.PasswordResetTokens (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(255) NOT NULL,
    ExpiresAt DATETIME2(7) NOT NULL,
    IsUsed BIT NOT NULL DEFAULT (0),
    CreatedAt DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT FK_ResetTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
CREATE INDEX IX_PasswordResetTokens_Token ON dbo.PasswordResetTokens (Token);
GO

/* ===============================
   INITIAL DATA (SEED)
================================ */
INSERT INTO dbo.Users (Id, Name, Email, PasswordHash, Role, IsActive, CreatedAt, CreatedByAdminId)
VALUES (
    '542CEA27-B3CE-4B47-A2E6-CED3CBCB39ED', 
    'SuperAdmin', 
    'superadmin@qrevent.com', 
    '$2a$12$kxDRqPwsC33dt0CWKULuGeXveV3hdghGulqUYMO7zYZsxuctrf2l.', 
    0, 
    1, 
    '2026-01-06 11:23:03.0000000', 
    NULL
);
GO
