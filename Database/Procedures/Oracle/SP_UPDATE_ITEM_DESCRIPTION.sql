CREATE OR REPLACE PROCEDURE SP_UPDATE_ITEM_DESCRIPTION (
    p_Id IN NUMBER,
    p_NewDescription IN VARCHAR2
) AS
BEGIN
    UPDATE "OracleDemoItems"
    SET "Description" = p_NewDescription
    WHERE "Id" = p_Id;
END;
