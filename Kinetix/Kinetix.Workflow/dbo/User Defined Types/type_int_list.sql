/* User-Defined Table de type int (Pour une utilisation dans un IN) */


CREATE TYPE type_int_list AS TABLE 
( val INT );
GO

GRANT EXECUTE ON TYPE::[dbo].[type_int_list] TO PUBLIC;
go
