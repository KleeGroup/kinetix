
-- ===========================================================================================
--   Description		:	Insertion des valeurs de la table WF_MULTIPLICITY.
-- ===========================================================================================

DECLARE @SCRIPT_NAME varchar(100)

SET @SCRIPT_NAME = 'WF_MULTIPLICITY.insert'
IF not exists(Select 1 From SCRIPT_HISTORIQUE WHERE SVH_NOM_SCRIPT = @SCRIPT_NAME)
BEGIN
	PRINT 'Appling script ' + @SCRIPT_NAME;
	SET XACT_ABORT ON
	BEGIN TRANSACTION

	INSERT INTO WF_MULTIPLICITY_DEFINITION(WFMD_CODE, LABEL) VALUES (N'Sin', N'Simple');
	INSERT INTO WF_MULTIPLICITY_DEFINITION(WFMD_CODE, LABEL) VALUES (N'Mul', N'Multiple');

	COMMIT TRANSACTION
END
GO
