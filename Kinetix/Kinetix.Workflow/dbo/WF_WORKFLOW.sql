
-- ===========================================================================================
--   Description		:	Création de la table WF_WORKFLOW.
-- ===========================================================================================

create table [dbo].[WF_WORKFLOW] (
	[WFW_ID] int identity,
	[CREATION_DATE] datetime2,
	[ITEM_ID] int,
	[USERNAME] nvarchar(50),
	[USER_LOGIC] bit not null,
	[WFWD_ID] int not null,
	[WFS_CODE] nvarchar(3) not null,
	[WFA_ID_2] int,
	constraint [PK_WORKFLOW] primary key clustered ([WFW_ID] ASC),
	constraint [FK_WFWD_ID] foreign key ([WFWD_ID]) references [dbo].[WF_WORKFLOW_DEFINITION] ([WFWD_ID]),
	constraint [FK_WFS_CODE] foreign key ([WFS_CODE]) references [dbo].[WF_STATUS] ([WFS_CODE]))
go

/* Index on foreign key column for WF_WORKFLOW.WFW_ID */
create nonclustered index [IDX_WF_WORKFLOW_WFWD_ID_FK]
	on [dbo].[WF_WORKFLOW] ([WFWD_ID] ASC)
go

/* Index on foreign key column for WF_WORKFLOW.WFS_CODE */
create nonclustered index [IDX_WF_WORKFLOW_WFS_CODE_FK]
	on [dbo].[WF_WORKFLOW] ([WFS_CODE] ASC)
go

/* Index on foreign key column for WF_WORKFLOW.WFA_ID_2 */
create nonclustered index [IDX_WF_WORKFLOW_WFA_ID_2_FK]
	on [dbo].[WF_WORKFLOW] ([WFA_ID_2] ASC)
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'Workflow', 'SCHEMA', 'dbo', 'TABLE', 'WF_WORKFLOW';
