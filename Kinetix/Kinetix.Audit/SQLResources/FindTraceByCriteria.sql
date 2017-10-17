Select
	*
From 
	AUDIT_TRACE aud
Where
	1 = 1
	[if notnull="AUD_BUSINESS_DATE_START"]
		and aud.AUD_BUSINESS_DATE >= @AUD_BUSINESS_DATE_START
	[/if]
	[if notnull="AUD_BUSINESS_DATE_END"]
		and aud.AUD_BUSINESS_DATE < @AUD_BUSINESS_DATE_END
	[/if]
	[if notnull="AUD_EXECUTION_DATE_START"]
		and aud.AUD_EXECUTION_DATE >= @AUD_EXECUTION_DATE_START
	[/if]
	[if notnull="AUD_EXECUTION_DATE_END"]
		and aud.AUD_EXECUTION_DATE < @AUD_EXECUTION_DATE_END
	[/if]
	[if notnull="AUD_CATEGORY"]
		and aud.AUD_CATEGORY = @AUD_CATEGORY
	[/if]
	[if notnull="AUD_ITEM"]
		and aud.AUD_ITEM = @AUD_ITEM
	[/if]
	[if notnull="AUD_USERNAME"]
		and aud.AUD_USERNAME = @AUD_USERNAME
	[/if]

	
	
	