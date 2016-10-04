﻿SELECT 
	RUD.*
FROM 
	RULE_DEFINITION RUD
	JOIN RULE_CONDITION_DEFINITION RCD ON (RCD.RUD_ID = RUD.ID)
WHERE 
	RUD.ITEM_ID IN (SELECT VAL FROM @ITEMS_ID)
AND	RCD.FIELD = @FIELD_1
AND RCD.EXPRESSION = @VALUE_1
[if notnull="FIELD_2"]
AND EXIST (SELECT 1 FROM RULE_CONDITION_DEFINITION RCD2 RCD2.FIELD = @FIELD_2 AND RCD.EXPRESSION = @VALUE_2 AND RCD2.ID = RUD.ID)
[/if]
;