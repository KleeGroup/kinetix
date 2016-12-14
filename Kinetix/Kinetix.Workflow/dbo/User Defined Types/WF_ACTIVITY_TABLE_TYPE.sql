
CREATE TYPE [dbo].[WF_ACTIVITY_TABLE_TYPE] AS TABLE (
    [CREATION_DATE] DATETIME2 (7) NULL,
    [WFW_ID]        INT           NULL,
    [WFAD_ID]       INT           NULL,
    [IS_AUTO]       BIT           NOT NULL,
	[IS_VALID]      BIT           NOT NULL,
    [INSERT_KEY]    INT           NULL);
GO


GRANT EXECUTE ON TYPE::[dbo].[WF_ACTIVITY_TABLE_TYPE] TO PUBLIC;
go
