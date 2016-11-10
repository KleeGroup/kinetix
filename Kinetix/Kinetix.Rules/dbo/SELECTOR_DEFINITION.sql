
-- ===========================================================================================
--   Description		:	Création de la table SELECTOR_DEFINITION.
-- ===========================================================================================

create table [dbo].[SELECTOR_DEFINITION] (
	[ID] int identity,
	[LABEL] nvarchar(100),
	[CREATION_DATE] datetime2,
	[ITEM_ID] int,
	[GROUP_ID] nvarchar(100),
	constraint [PK_SELECTOR_DEFINITION] primary key clustered ([ID] ASC))
go

/* Index on foreign key column for SELECTOR_DEFINITION.ITEM_ID */
create nonclustered index [IDX_SELECTOR_DEFINITION_ITEM_ID]
	on [dbo].[SELECTOR_DEFINITION] ([ITEM_ID] ASC)
go

/* Index on foreign key column for SELECTOR_DEFINITION.GROUP_ID */
create nonclustered index [IDX_SELECTOR_DEFINITION_GROUP_ID]
	on [dbo].[SELECTOR_DEFINITION] ([GROUP_ID] ASC)
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'SelectorDefinition', 'SCHEMA', 'dbo', 'TABLE', 'SELECTOR_DEFINITION';
