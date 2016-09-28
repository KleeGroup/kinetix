
-- ===========================================================================================
--   Description		:	Création de la table WF_DECISION.
-- ===========================================================================================

create table [dbo].[WF_DECISION] (
	[WFE_ID] int identity,
	[USERNAME] nvarchar(100),
	[CHOICE] int,
	[DECISION_DATE] datetime2,
	[COMMENTS] nvarchar(3000),
	[WFA_ID] int not null,
	constraint [PK_WF_DECISION] primary key clustered ([WFE_ID] ASC),
	constraint [FK_WFE_WFA] foreign key ([WFA_ID]) references [dbo].[WF_ACTIVITY] ([WFA_ID]))
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'Decision', 'SCHEMA', 'dbo', 'TABLE', 'WF_DECISION';
