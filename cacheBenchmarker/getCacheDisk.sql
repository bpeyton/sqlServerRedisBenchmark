--======================================================
-- Create Natively Compiled Stored Procedure Template
--======================================================

USE CacheTest
GO

-- Drop stored procedure if it already exists
IF OBJECT_ID('dbo.getCacheDisk','P') IS NOT NULL
   DROP PROCEDURE dbo.getCacheDisk
GO

CREATE PROCEDURE getCacheDisk
	@cacheKey varchar(200)
AS
	select cacheValue from dbo.cacheDisk where cacheKey=@cacheKey;
GO

-- =============================================
-- Example to execute the stored procedure
-- =============================================
EXECUTE <Schema_Name, sysname, dbo>.<Procedure_Name, sysname, Procedure_Name> <value_for_param1, , 1>, <value_for_param2, , 2>
GO