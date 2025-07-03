-- Query to verify data in SQL Server
-- Run this in SQL Server Management Studio or Azure Data Studio

USE ContactCardsDB;

-- Check if tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';

-- View all contacts
SELECT Id, Name, Email, Phone, CreatedAt 
FROM Contacts 
ORDER BY CreatedAt DESC;

-- Count total contacts
SELECT COUNT(*) as TotalContacts FROM Contacts;

-- Check table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Contacts'
ORDER BY ORDINAL_POSITION;
