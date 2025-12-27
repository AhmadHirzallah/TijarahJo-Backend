-- ============================================================
-- TIJARAHJO DATABASE SCHEMA EXTRACTION SCRIPT
-- Run this in SQL Server Management Studio (SSMS)
-- Copy the output and share it for analysis
-- ============================================================

USE TijarahJoDB; -- Change this to your database name if different
GO

PRINT '============================================================'
PRINT 'TIJARAHJO DATABASE SCHEMA REPORT'
PRINT 'Generated: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '============================================================'
PRINT ''

-- ============================================================
-- 1. ALL TABLES
-- ============================================================
PRINT '==================== 1. TABLES ===================='
SELECT 
    t.TABLE_SCHEMA AS [Schema],
    t.TABLE_NAME AS [Table],
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS c WHERE c.TABLE_NAME = t.TABLE_NAME) AS [Columns]
FROM INFORMATION_SCHEMA.TABLES t
WHERE t.TABLE_TYPE = 'BASE TABLE'
ORDER BY t.TABLE_NAME;

-- ============================================================
-- 2. ALL COLUMNS WITH DETAILS
-- ============================================================
PRINT ''
PRINT '==================== 2. COLUMNS ===================='
SELECT 
    c.TABLE_NAME AS [Table],
    c.COLUMN_NAME AS [Column],
    c.DATA_TYPE AS [DataType],
    CASE 
        WHEN c.CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN '(' + CAST(c.CHARACTER_MAXIMUM_LENGTH AS VARCHAR) + ')'
        WHEN c.NUMERIC_PRECISION IS NOT NULL THEN '(' + CAST(c.NUMERIC_PRECISION AS VARCHAR) + ',' + CAST(c.NUMERIC_SCALE AS VARCHAR) + ')'
        ELSE ''
    END AS [Size],
    c.IS_NULLABLE AS [Nullable],
    COALESCE(c.COLUMN_DEFAULT, '') AS [Default]
FROM INFORMATION_SCHEMA.COLUMNS c
JOIN INFORMATION_SCHEMA.TABLES t ON c.TABLE_NAME = t.TABLE_NAME
WHERE t.TABLE_TYPE = 'BASE TABLE'
ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION;

-- ============================================================
-- 3. PRIMARY KEYS
-- ============================================================
PRINT ''
PRINT '==================== 3. PRIMARY KEYS ===================='
SELECT 
    tc.TABLE_NAME AS [Table],
    kcu.COLUMN_NAME AS [PK Column],
    tc.CONSTRAINT_NAME AS [Constraint]
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
ORDER BY tc.TABLE_NAME;

-- ============================================================
-- 4. FOREIGN KEYS (CRITICAL FOR CASCADE DELETE ANALYSIS)
-- ============================================================
PRINT ''
PRINT '==================== 4. FOREIGN KEYS ===================='
SELECT 
    fk.name AS [FK Name],
    OBJECT_NAME(fk.parent_object_id) AS [Child Table],
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS [Child Column],
    OBJECT_NAME(fk.referenced_object_id) AS [Parent Table],
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS [Parent Column],
    fk.delete_referential_action_desc AS [On Delete],
    fk.update_referential_action_desc AS [On Update]
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
ORDER BY OBJECT_NAME(fk.parent_object_id), fk.name;

-- ============================================================
-- 5. ALL STORED PROCEDURES (NAMES)
-- ============================================================
PRINT ''
PRINT '==================== 5. STORED PROCEDURES ===================='
SELECT 
    ROUTINE_NAME AS [Procedure Name],
    CREATED AS [Created],
    LAST_ALTERED AS [Last Modified]
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
ORDER BY ROUTINE_NAME;

-- ============================================================
-- 6. STORED PROCEDURE DEFINITIONS (FULL CODE)
-- ============================================================
PRINT ''
PRINT '==================== 6. STORED PROCEDURE CODE ===================='

-- Delete-related procedures (most important for this issue)
PRINT '-- DELETE PROCEDURES --'
SELECT 
    ROUTINE_NAME,
    ROUTINE_DEFINITION
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
AND (ROUTINE_NAME LIKE '%Delete%' OR ROUTINE_NAME LIKE '%Remove%')
ORDER BY ROUTINE_NAME;

-- Post-related procedures
PRINT ''
PRINT '-- POST-RELATED PROCEDURES --'
SELECT 
    ROUTINE_NAME,
    ROUTINE_DEFINITION
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
AND ROUTINE_NAME LIKE '%Post%'
ORDER BY ROUTINE_NAME;

-- Image-related procedures
PRINT ''
PRINT '-- IMAGE-RELATED PROCEDURES --'
SELECT 
    ROUTINE_NAME,
    ROUTINE_DEFINITION
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
AND (ROUTINE_NAME LIKE '%Image%' OR ROUTINE_NAME LIKE '%Photo%')
ORDER BY ROUTINE_NAME;

-- ============================================================
-- 7. ALL INDEXES
-- ============================================================
PRINT ''
PRINT '==================== 7. INDEXES ===================='
SELECT 
    t.name AS [Table],
    i.name AS [Index],
    i.type_desc AS [Type],
    i.is_unique AS [Unique],
    i.is_primary_key AS [PK]
FROM sys.indexes i
JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.name IS NOT NULL
ORDER BY t.name, i.name;

-- ============================================================
-- 8. CHECK CONSTRAINTS
-- ============================================================
PRINT ''
PRINT '==================== 8. CHECK CONSTRAINTS ===================='
SELECT 
    cc.name AS [Constraint],
    OBJECT_NAME(cc.parent_object_id) AS [Table],
    cc.definition AS [Definition]
FROM sys.check_constraints cc
ORDER BY OBJECT_NAME(cc.parent_object_id);

-- ============================================================
-- 9. DEFAULT CONSTRAINTS
-- ============================================================
PRINT ''
PRINT '==================== 9. DEFAULT CONSTRAINTS ===================='
SELECT 
    dc.name AS [Constraint],
    OBJECT_NAME(dc.parent_object_id) AS [Table],
    COL_NAME(dc.parent_object_id, dc.parent_column_id) AS [Column],
    dc.definition AS [Default Value]
FROM sys.default_constraints dc
ORDER BY OBJECT_NAME(dc.parent_object_id);

-- ============================================================
-- 10. TRIGGERS
-- ============================================================
PRINT ''
PRINT '==================== 10. TRIGGERS ===================='
SELECT 
    tr.name AS [Trigger],
    OBJECT_NAME(tr.parent_id) AS [Table],
    tr.is_instead_of_trigger AS [Instead Of],
    CASE 
        WHEN OBJECTPROPERTY(tr.object_id, 'ExecIsInsertTrigger') = 1 THEN 'INSERT'
        WHEN OBJECTPROPERTY(tr.object_id, 'ExecIsUpdateTrigger') = 1 THEN 'UPDATE'
        WHEN OBJECTPROPERTY(tr.object_id, 'ExecIsDeleteTrigger') = 1 THEN 'DELETE'
        ELSE 'MULTIPLE'
    END AS [Event]
FROM sys.triggers tr
WHERE tr.parent_id != 0
ORDER BY OBJECT_NAME(tr.parent_id);

-- ============================================================
-- 11. VIEWS
-- ============================================================
PRINT ''
PRINT '==================== 11. VIEWS ===================='
SELECT 
    TABLE_NAME AS [View Name],
    VIEW_DEFINITION AS [Definition]
FROM INFORMATION_SCHEMA.VIEWS
ORDER BY TABLE_NAME;

-- ============================================================
-- 12. SPECIFIC: TbPosts and TbPostImages RELATIONSHIP
-- ============================================================
PRINT ''
PRINT '==================== 12. POSTS & IMAGES TABLES DETAIL ===================='

-- Posts table structure
SELECT 
    'TbPosts' AS [Table],
    COLUMN_NAME AS [Column],
    DATA_TYPE + CASE 
        WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN '(' + CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR) + ')'
        ELSE ''
    END AS [Type],
    IS_NULLABLE AS [Nullable]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TbPosts'
ORDER BY ORDINAL_POSITION;

-- PostImages table structure
SELECT 
    'TbPostImages' AS [Table],
    COLUMN_NAME AS [Column],
    DATA_TYPE + CASE 
        WHEN CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN '(' + CAST(CHARACTER_MAXIMUM_LENGTH AS VARCHAR) + ')'
        ELSE ''
    END AS [Type],
    IS_NULLABLE AS [Nullable]
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TbPostImages'
ORDER BY ORDINAL_POSITION;

-- FK between Posts and PostImages
SELECT 
    fk.name AS [FK Name],
    OBJECT_NAME(fk.parent_object_id) AS [Child Table],
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS [Child Column],
    OBJECT_NAME(fk.referenced_object_id) AS [Parent Table],
    fk.delete_referential_action_desc AS [ON DELETE],
    fk.update_referential_action_desc AS [ON UPDATE]
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'TbPostImages'
   OR OBJECT_NAME(fk.referenced_object_id) = 'TbPosts';

-- ============================================================
-- 13. SP_DeletePost FULL DEFINITION
-- ============================================================
PRINT ''
PRINT '==================== 13. SP_DeletePost DEFINITION ===================='
SELECT OBJECT_DEFINITION(OBJECT_ID('SP_DeletePost')) AS [SP_DeletePost Code];

-- ============================================================
-- 14. COUNT OF DATA (for context)
-- ============================================================
PRINT ''
PRINT '==================== 14. DATA COUNTS ===================='
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 
    'SELECT ''' + TABLE_NAME + ''' AS [Table], COUNT(*) AS [Rows] FROM ' + TABLE_NAME + ' UNION ALL '
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';

-- Remove last UNION ALL
SET @sql = LEFT(@sql, LEN(@sql) - 10);
EXEC sp_executesql @sql;

PRINT ''
PRINT '============================================================'
PRINT 'END OF SCHEMA REPORT'
PRINT '============================================================'
