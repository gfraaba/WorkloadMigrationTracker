IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'WorkloadMigration')
BEGIN
    CREATE DATABASE WorkloadMigration;
    PRINT 'Database created';
END
GO

USE WorkloadMigration;
GO