/****** Script for SelectTopNRows command from SSMS  ******/
SELECT count(*)
  FROM [CacheTest].[dbo].[cache]

  select top 1000 '''' + cacheKey + ''','
  FROM [CacheTest].[dbo].[cache]

  SELECT count(*)
  FROM [CacheTest].[dbo].[cacheDisk]

  select top 100 * from cache


  --insert into cacheDisk(cacheKey,cacheValue)
  --select * from cache
 --DBCC SHRINKFILE('CacheTest_log', 0, TRUNCATEONLY)
 
    insert into cache(cacheKey,cacheValue)
  select * from cacheDisk
--  delete from cache

--  truncate table cacheDisk
 

ALTER DATABASE CacheTest
	 SET DELAYED_DURABILITY = FORCED     
