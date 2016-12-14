CREATE TYPE type_int_int_list as TABLE
(
	val1 INT,
	val2 INT
)
GO

GRANT EXECUTE ON TYPE::[dbo].[type_int_int_list] TO PUBLIC;
go