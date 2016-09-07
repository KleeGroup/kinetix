
-- ===========================================================================================
--   Description		:	Création de la table WF_WORKFLOW.
-- ===========================================================================================

create table [dbo].[WF_TRANSITION_DEFINITION] (
	[WFTD_ID] int identity,
	[NAME] nvarchar(100),
	[WFWD_ID] int,
	[WFAD_ID_FROM] int not null,
	[WFAD_ID_TO] int not null,
	constraint [PK_WF_TRANSITION_DEFINITION] primary key clustered ([WFTD_ID] ASC),
	constraint [FK_WFT_WFA_FROM] foreign key ([WFAD_ID_FROM]) references [dbo].[WF_ACTIVITY_DEFINITION] ([WFAD_ID]),
	constraint [FK_WFT_WFA_TO] foreign key ([WFAD_ID_TO]) references [dbo].[WF_ACTIVITY_DEFINITION] ([WFAD_ID]),
	constraint [FK_WFWD_WFTD] foreign key ([WFWD_ID]) references [dbo].[WF_WORKFLOW_DEFINITION] ([WFWD_ID]))
go

/* Index on foreign key column for WF_TRANSITION_DEFINITION.WFAD_ID_FROM */
create nonclustered index [IDX_WF_TRANSITION_DEFINITION_WFAD_ID_FROM_FK]
	on [dbo].[WF_TRANSITION_DEFINITION] ([WFAD_ID_FROM] ASC)
go

/* Index on foreign key column for WF_TRANSITION_DEFINITION.WFAD_ID_TO */
create nonclustered index [IDX_WF_TRANSITION_DEFINITION_WFAD_ID_TO_FK]
	on [dbo].[WF_TRANSITION_DEFINITION] ([WFAD_ID_TO] ASC)
go

/* Index on foreign key column for WF_TRANSITION_DEFINITION.WFWD_ID */
create nonclustered index [IDX_WF_TRANSITION_DEFINITION_WFWD_ID_FK]
	on [dbo].[WF_TRANSITION_DEFINITION] ([WFWD_ID] ASC)
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'TransitionDefinition', 'SCHEMA', 'dbo', 'TABLE', 'WF_TRANSITION_DEFINITION';
