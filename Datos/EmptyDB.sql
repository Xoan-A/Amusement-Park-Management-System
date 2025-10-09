/*
================================================================================
ADD ADMIN USER - MINIMAL SCRIPT
================================================================================

HOW TO EXECUTE IN DBEAVER:
1. Open this file in DBeaver
2. Connect to your ParqueTematicoDB database
3. Select all (Ctrl+A / Cmd+A)
4. Execute (Ctrl+Enter / Cmd+Enter or press F5)

WHAT THIS DOES:
- Creates ONE admin user
- Email: admin@test.com
- Password: admin123
- Role: Administrator

NOTE: This script only ADDS data, it does NOT delete anything.

================================================================================
*/

-- Insert Admin User
INSERT INTO Users (Id, Name, LastName, Email, Password, BirthDate, MembershipLevel, Score)
VALUES
    (NEWID(),
     'Admin',
     'User',
     'admin@test.com',
     '$2a$11$TgMvdaYlj5ZLE7ybPvoFi.jqSqp6S39yr3JXz34wo/ReZThKeHuYq',
     '1980-01-01',
     2,
     0);

-- Get the newly created user's ID and assign Administrator role
DECLARE @AdminUserId UNIQUEIDENTIFIER;
SELECT @AdminUserId = Id FROM Users WHERE Email = 'admin@test.com';

INSERT INTO UserRoles (UserId, RoleId)
VALUES (@AdminUserId, 1);

/*
================================================================================
SUCCESS! Admin user created.

CREDENTIALS:
- Email: admin@test.com
- Password: admin123

================================================================================
*/
