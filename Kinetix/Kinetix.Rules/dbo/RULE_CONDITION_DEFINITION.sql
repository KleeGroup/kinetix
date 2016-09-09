
-- ===========================================================================================
--   Description		:	Création de la table RULE_CONDITION_DEFINITION.
-- ===========================================================================================

create table [dbo].[RULE_CONDITION_DEFINITION] (
	[ID] int identity,
	[LABEL] nvarchar(100),
	[FIELD] nvarchar(50),
	[OPERATOR] nvarchar(3),
	[EXPRESSION] nvarchar(100),
	[RUD_ID] int not null,
	constraint [PK_SELECTOR_DEFINITION] primary key clustered ([ID] ASC),
	constraint [FK_RUD_RCO] foreign key ([RUD_ID]) references [dbo].[RULE_DEFINITION] ([ID]))
go

/* Index on foreign key column for RULE_CONDITION_DEFINITION.RUD_ID */
create nonclustered index [IDX_RULE_CONDITION_DEFINITION_RUD_ID]
	on [dbo].[RULE_CONDITION_DEFINITION] ([RUD_ID] ASC)
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'RuleDefinition', 'SCHEMA', 'dbo', 'TABLE', 'RULE_DEFINITION';
