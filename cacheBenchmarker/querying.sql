/****** Script for SelectTopNRows command from SSMS  ******/
SELECT count(*)
  FROM [CacheTest].[dbo].[cache]

  select top 1000 '''' + cacheKey + ''','
  FROM [CacheTest].[dbo].[cache]

  SELECT count(*)
  FROM [CacheTest].[dbo].[cacheDisk]

  select top 100 * from cache

  truncate table cacheDisk;
  delete from cacheMem;

  --insert into cacheDisk(cacheKey,cacheValue)
  --select * from cache
 --DBCC SHRINKFILE('CacheTest_log', 0, TRUNCATEONLY)
 
    insert into cache(cacheKey,cacheValue)
  select * from cacheDisk
--  delete from cache

--  truncate table cacheDisk
 select cacheValue from cacheDisk where cacheKey='00015210-c9ef-4467-b5d8-15983c7fcb98';
 exec sp_executesql N'select cacheValue from cacheMem where cacheKey=@cacheKey;',N'@cacheKey nvarchar(36)',@cacheKey=N'ffe8bebf-a1f6-4490-96a1-a39c0b11a54c'
 exec sp_executesql N'select cacheValue from cacheDisk where cacheKey=@cacheKey;',N'@cacheKey nvarchar(36)',@cacheKey=N'00015210-c9ef-4467-b5d8-15983c7fcb98'
ALTER DATABASE CacheTest
	 SET DELAYED_DURABILITY = FORCED     
