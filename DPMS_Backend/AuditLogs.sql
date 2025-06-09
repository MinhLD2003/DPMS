CREATE TABLE AuditLogs (
	Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	COMMAND VARCHAR(6) NOT NULL,
	CHANGED_DATE DATETIME DEFAULT GETDATE() NOT NULL,
	TABLE_NAME NVARCHAR(255) NOT NULL,
	COLUMN_NAMES NVARCHAR(255) NULL,
	COLUMN_OLD_VALUES XML NULL,
	COLUMN_NEW_VALUES XML NULL,
	USERNAME NVARCHAR(100) NULL
);
-- ============================================================= --
-- Create Trigger for Table to log changes
CREATE TRIGGER AUDIT_DPIAs
ON DPIAs
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Grab trx type
    DECLARE @command char(6) 
    SET @command =
    CASE
        WHEN EXISTS(SELECT 1 FROM inserted) AND EXISTS(SELECT 1 FROM deleted) THEN 'UPDATE'
        WHEN EXISTS(SELECT 1 FROM inserted) THEN 'INSERT'
        WHEN EXISTS(SELECT 1 FROM deleted) THEN 'DELETE'
        ELSE '0 ROWS' -- if no rows affected, trigger does NOT record an entry
    END 
 
    IF @command = 'INSERT'

        -- Add audit entry
        INSERT INTO AuditLogs (COMMAND, CHANGED_DATE, TABLE_NAME, /*COLUMN_NAMES,*/ COLUMN_OLD_VALUES, COLUMN_NEW_VALUES, USERNAME)
        SELECT 
            Command     = @command, 
            ChangeDate  = GETDATE(), 
            TableName   = 'DPIAs', 
            --ColNames  = @column_names, 
            Column_OLD_Values   = NULL, 
            Column_NEW_Values   = (SELECT inserted.* for xml path(''), type), 
            Username    = SUSER_SNAME()
        FROM inserted 
    
    ELSE IF @command = 'DELETE'

        -- Add audit entry
        INSERT INTO AuditLogs (COMMAND, CHANGED_DATE, TABLE_NAME, /*COLUMN_NAMES,*/ COLUMN_OLD_VALUES, COLUMN_NEW_VALUES, USERNAME)
        SELECT 
            Command     = @command, 
            ChangeDate  = GETDATE(), 
            TableName   = 'DPIAs', 
            --ColNames  = @column_names, 
            Column_OLD_Values   = (SELECT deleted.* for xml path(''), type), 
            Column_NEW_Values   = NULL,
            Username    = SUSER_SNAME()
        FROM deleted

    ELSE -- is UPDATE 

        -- Add audit entry
        INSERT INTO AuditLogs (COMMAND, CHANGED_DATE, TABLE_NAME, /*COLUMN_NAMES,*/ COLUMN_OLD_VALUES, COLUMN_NEW_VALUES, USERNAME)
        SELECT 
            Command     = @command, 
            ChangeDate  = GETDATE(), 
            TableName   = 'DPIAs', 
            --ColNames  = @column_names, 
            Column_OLD_Values   = (SELECT deleted.* for xml path(''), type), 
            Column_NEW_Values   = (SELECT inserted.* for xml path(''), type), 
            Username    = SUSER_SNAME()
        FROM inserted 
        INNER JOIN deleted ON inserted.Id = deleted.Id -- join on w/e the PK is
END