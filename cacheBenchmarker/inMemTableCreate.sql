USE [master]
GO

ALTER DATABASE [CacheTest] ADD FILEGROUP [memory_optimized_filegroup_0] CONTAINS MEMORY_OPTIMIZED_DATA 
GO

ALTER DATABASE [CacheTest] ADD FILE ( NAME = N'memory_optimized_file_1095774209', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.SQL2017\MSSQL\DATA\memory_optimized_file_1095774209' ) TO FILEGROUP [memory_optimized_filegroup_0]
GO

USE [CacheTest]
GO

EXEC dbo.sp_rename @objname = N'[dbo].[cacheDisk]', @newname = N'cache', @objtype = N'OBJECT'
GO

USE [CacheTest]
GO

SET ANSI_NULLS ON
GO

CREATE TABLE [dbo].[cacheDisk]
(
	[cacheKey] [varchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[cacheValue] [varchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

 CONSTRAINT [cacheDisk_primaryKey_1]  PRIMARY KEY NONCLUSTERED HASH 
(
	[cacheKey]
)WITH ( BUCKET_COUNT = 1048576)
)WITH ( MEMORY_OPTIMIZED = ON , DURABILITY = SCHEMA_ONLY )

GO

INSERT INTO [CacheTest].[dbo].[cacheDisk] ([cacheKey], [cacheValue]) SELECT [cacheKey], [cacheValue] FROM [CacheTest].[dbo].[cache] 

GO


