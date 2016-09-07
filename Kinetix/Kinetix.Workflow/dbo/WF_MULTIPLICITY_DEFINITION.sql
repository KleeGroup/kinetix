
-- ===========================================================================================
--   Description		:	Création de la table WF_WORKFLOW.
-- ===========================================================================================

create table [dbo].[WF_MULTIPLICITY_DEFINITION] (
	[WFMD_CODE] nvarchar(3),
	[LABEL] nvarchar(100),
	constraint [PK_WF_MULTIPLICITY_DEFINITION] primary key clustered ([WFMD_CODE] ASC))
go

/* Description property. */
EXECUTE sp_addextendedproperty 'Description', 'MultiplicityDefinition', 'SCHEMA', 'dbo', 'TABLE', 'WF_MULTIPLICITY_DEFINITION';
