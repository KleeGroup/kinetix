
-- ===========================================================================================
--   Description		:	Création de la table WF_ACTIVITY.
-- ===========================================================================================

create table [dbo].[WF_ACTIVITY] (
	[WFA_ID] int identity,
	[CREATION_DATE] datetime2,
	[WFW_ID] int,
	[WFAD_ID] int,
	[IS_AUTO] bit not null,
	[IS_VALID] bit not null,
	[INSERT_KEY] int,
	constraint [PK_WF_ACTIVITY] primary key clustered ([WFA_ID] ASC),
	constraint [FK_WFAD_WFA] foreign key ([WFAD_ID]) references [dbo].[WF_ACTIVITY_DEFINITION] ([WFAD_ID]))
go

/* Index on foreign key column for WF_WORKFLOW.WFW_ID */
create nonclustered index [IDX_WF_ACTIVITY_WFW_ID_FK]
	on [dbo].[WF_ACTIVITY] ([WFW_ID] ASC)
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'Workflow', 'SCHEMA', 'dbo', 'TABLE', 'WF_ACTIVITY';
