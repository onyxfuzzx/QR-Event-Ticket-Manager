USE QREventDb;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmailTemplates')
BEGIN
    CREATE TABLE dbo.EmailTemplates (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        EventId UNIQUEIDENTIFIER NOT NULL,
        LayoutJson NVARCHAR(MAX) NOT NULL,
        HtmlContent NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
        UpdatedAt DATETIME2(7) NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT FK_EmailTemplates_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id)
    );
END
GO
