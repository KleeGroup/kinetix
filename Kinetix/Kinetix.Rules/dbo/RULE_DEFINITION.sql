
-- ===========================================================================================
--   Description		:	Création de la table RULE_DEFINITION.
-- ===========================================================================================

create table [dbo].[RULE_DEFINITION] (
	[ID] int identity,
	[LABEL] nvarchar(100),
	[CREATION_DATE] datetime2,
	[ITEM_ID] int,
	constraint [PK_SELECTOR_DEFINITION] primary key clustered ([ID] ASC))
go

/* Index on foreign key column for RULE_DEFINITION.ITEM_ID */
create nonclustered index [IDX_RULE_DEFINITION_ITEM_ID]
	on [dbo].[RULE_DEFINITION] ([ITEM_ID] ASC)
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'RuleDefinition', 'SCHEMA', 'dbo', 'TABLE', 'RULE_DEFINITION';
