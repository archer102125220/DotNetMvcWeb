CREATE PROCEDURE SP_UPDATE_ITEM_DESCRIPTION (
    IN p_Id INT,
    IN p_NewDescription TEXT
)
BEGIN
    UPDATE `MysqlDemoItems`
    SET `Description` = p_NewDescription
    WHERE `Id` = p_Id;
END
