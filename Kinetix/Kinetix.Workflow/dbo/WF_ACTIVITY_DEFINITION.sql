
-- ===========================================================================================
--   Description		:	Création de la table WF_ACTIVITY_DEFINITION.
-- ===========================================================================================

create table [dbo].[WF_ACTIVITY_DEFINITION] (
	[WFAD_ID] int identity,
	[NAME] nvarchar(100),
	[LEVEL] int,
	[WFMD_CODE] nvarchar(3),
	[WFWD_ID] int not null,
	constraint [PK_WF_ACTIVITY_DEFINITION] primary key clustered ([WFAD_ID] ASC),
	constraint [FK_WFMD_CODE] foreign key ([WFMD_CODE]) references [dbo].[WF_MULTIPLICITY_DEFINITION] ([WFMD_CODE]))
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'ActivityDefinition', 'SCHEMA', 'dbo', 'TABLE', 'WF_ACTIVITY_DEFINITION';

