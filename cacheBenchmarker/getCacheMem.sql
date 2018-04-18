--======================================================
-- Create Natively Compiled Stored Procedure Template
--======================================================

USE CacheTest
GO

-- Drop stored procedure if it already exists
IF OBJECT_ID('dbo.getCacheMem','P') IS NOT NULL
   DROP PROCEDURE dbo.getCacheMem
GO

CREATE PROCEDURE getCacheMem
	@cacheKey varchar(200)
WITH NATIVE_COMPILATION, SCHEMABINDING
AS
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT, LANGUAGE = N'us_english')  

	select cacheValue from dbo.cacheMem where cacheKey=@cacheKey;
END;
GO

-- =============================================
-- Example to execute the stored procedure
-- =============================================
EXECUTE <Schema_Name, sysname, dbo>.<Procedure_Name, sysname, Procedure_Name> <value_for_param1, , 1>, <value_for_param2, , 2>
GO