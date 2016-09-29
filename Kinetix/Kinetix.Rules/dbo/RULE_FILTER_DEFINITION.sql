
-- ===========================================================================================
--   Description		:	Création de la table RULE_FILTER_DEFINITION.
-- ===========================================================================================

create table [dbo].[RULE_FILTER_DEFINITION] (
	[ID] int identity,
	[LABEL] nvarchar(100),
	[FIELD] nvarchar(50),
	[OPERATOR] nvarchar(3),
	[EXPRESSION] nvarchar(100),
	[SEL_ID] int not null,
	constraint [PK_RULE_FILTER_DEFINITION] primary key clustered ([ID] ASC),
	constraint [FK_RFI_SEL] foreign key ([SEL_ID]) references [dbo].[SELECTOR_DEFINITION] ([ID]))
go

/* Index on foreign key column for RULE_CONDITION_DEFINITION.SEL_ID */
create nonclustered index [IDX_RULE_FILTER_DEFINITION_SEL_ID]
	on [dbo].[RULE_FILTER_DEFINITION] ([SEL_ID] ASC)
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'RuleFilterDefinition', 'SCHEMA', 'dbo', 'TABLE', 'RULE_FILTER_DEFINITION';
