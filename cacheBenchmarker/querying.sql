/****** Script for SelectTopNRows command from SSMS  ******/
SELECT count(*)
  FROM [CacheTest].[dbo].[cache]

  SELECT count(*)
  FROM [CacheTest].[dbo].[cacheDisk]

  select top 100 * from cache

--  delete from cache

--  truncate table cacheDisk
ALTER DATABASE CacheTest
	 SET DELAYED_DURABILITY = FORCED    

ALTER DATABASE CacheTest
	 SET DELAYED_DURABILITY = DISABLED     
