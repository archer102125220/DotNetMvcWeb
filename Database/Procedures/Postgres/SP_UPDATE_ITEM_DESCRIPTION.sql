CREATE OR REPLACE PROCEDURE SP_UPDATE_ITEM_DESCRIPTION (
    p_Id integer,
    p_NewDescription character varying
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE "PostgresDemoItems"
    SET "Description" = p_NewDescription
    WHERE "Id" = p_Id;
END;
$$;
