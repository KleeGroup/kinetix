
-- ===========================================================================================
--   Description		:	Création de la table WF_STATUS.
-- ===========================================================================================

create table [dbo].[WF_STATUS] (
	[WFS_CODE] nvarchar(3),
	[LABEL] nvarchar(100),
	constraint [PK_WF_STATUS] primary key clustered ([WFS_CODE] ASC))
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'Status', 'SCHEMA', 'dbo', 'TABLE', 'WF_STATUS';

