
-- ===========================================================================================
--   Description		:	Création de la table WF_WORKFLOW_DEFINITION.
-- ===========================================================================================

create table [dbo].[WF_WORKFLOW_DEFINITION] (
	[WFWD_ID] int identity,
	[NAME] nvarchar(100),
	[CREATION_DATE] datetime2,
	[WFAD_ID] int,
	constraint [PK_WF_WORKFLOW_DEFINITION] primary key clustered ([WFWD_ID] ASC),
	constraint [FK_WFWD_WFAD] foreign key ([WFAD_ID]) references [dbo].[WF_ACTIVITY_DEFINITION] ([WFAD_ID]))
go

/* Index on foreign key column for WF_WORKFLOW_DEFINITION.WFAD_ID */
create nonclustered index [IDX_WF_WORKFLOW_DEFINITION_WFAD_ID_FK]
	on [dbo].[WF_WORKFLOW_DEFINITION] ([WFAD_ID] ASC)
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'Workflow', 'SCHEMA', 'dbo', 'TABLE', 'WF_WORKFLOW_DEFINITION';
