-- DELETE ALL DATA

DELETE FROM Reports;
DELETE FROM VisitorReports;
DELETE FROM EventAttraction;
DELETE FROM Tickets;
DELETE FROM UserRoles;
DELETE FROM Users;
DELETE FROM Events;
DELETE FROM Attractions;
DELETE FROM DateTimeConfigurations;


-- CREATE ONE ADMIN USER
-- Email: admin@test.com
-- Password: admin123

INSERT INTO Users (Id, Name, LastName, Email, Password, BirthDate, MembershipLevel, Score)
VALUES ('11111111-1111-1111-1111-111111111111', 'Admin', 'User', 'admin@test.com', '$2a$11$TgMvdaYlj5ZLE7ybPvoFi.jqSqp6S39yr3JXz34wo/ReZThKeHuYq', '1980-01-01', 2, 0);

INSERT INTO UserRoles (UserId, RoleId)
VALUES ('11111111-1111-1111-1111-111111111111', 1);
