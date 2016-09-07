
-- ===========================================================================================
--   Description		:	Insertion des valeurs de la table WF_STATUS.
-- ===========================================================================================

DECLARE @SCRIPT_NAME varchar(100)

SET @SCRIPT_NAME = 'WF_STATUS.insert'
IF not exists(Select 1 From SCRIPT_HISTORIQUE WHERE SVH_NOM_SCRIPT = @SCRIPT_NAME)
BEGIN
	PRINT 'Appling script ' + @SCRIPT_NAME;
	SET XACT_ABORT ON
	BEGIN TRANSACTION

	INSERT INTO WF_STATUS(WFS_CODE, LABEL) VALUES (N'CRE', N'Créer');
	INSERT INTO WF_STATUS(WFS_CODE, LABEL) VALUES (N'STA', N'Démaré');
	INSERT INTO WF_STATUS(WFS_CODE, LABEL) VALUES (N'PAU', N'Suspendu');
	INSERT INTO WF_STATUS(WFS_CODE, LABEL) VALUES (N'END', N'Terminé');

	COMMIT TRANSACTION
END
GO
