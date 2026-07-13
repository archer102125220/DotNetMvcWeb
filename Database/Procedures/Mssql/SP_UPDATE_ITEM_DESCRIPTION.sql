CREATE OR ALTER PROCEDURE SP_UPDATE_ITEM_DESCRIPTION
    @Id INT,
    @NewDescription NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [MssqlDemoItems]
    SET [Description] = @NewDescription
    WHERE [Id] = @Id;
END
GO
