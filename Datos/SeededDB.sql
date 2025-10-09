/*
================================================================================
PARQUE TEMÁTICO VIRTUAL - SEED DATA (SIMPLIFIED)
================================================================================

HOW TO EXECUTE IN DBEAVER:
1. Open this file in DBeaver
2. Connect to your ParqueTematicoDB database
3. Select all (Ctrl+A / Cmd+A)
4. Execute (Ctrl+Enter / Cmd+Enter or press F5)

NOTE: This will DELETE all existing data except Roles and StrategyConfigurations

================================================================================
*/

-- ============================================================================
-- DELETE EXISTING DATA (in FK dependency order)
-- ============================================================================

DELETE FROM Reports;
DELETE FROM VisitorReports;
DELETE FROM EventAttraction;
DELETE FROM Tickets;
DELETE FROM UserRoles;
DELETE FROM Users;
DELETE FROM Events;
DELETE FROM Attractions;
DELETE FROM DateTimeConfigurations;


-- ============================================================================
-- INSERT USERS
-- ============================================================================

-- Admin and Operator
INSERT INTO Users (Id, Name, LastName, Email, Password, BirthDate, MembershipLevel, Score)
VALUES
    ('11111111-1111-1111-1111-111111111111', 'Admin', 'User', 'admin@test.com',
     '$2a$11$TgMvdaYlj5ZLE7ybPvoFi.jqSqp6S39yr3JXz34wo/ReZThKeHuYq',
     '1980-01-01', 2, 0),

    ('22222222-2222-2222-2222-222222222222', 'Operator', 'User', 'operator@test.com',
     '$2a$11$QHVhQ21m/dB3cntgTO2aqu3SNiQn6d7nUnRE3lPE4LPEoFJGRSEJu',
     '1985-01-01', 0, 0);

-- Visitors
INSERT INTO Users (Id, Name, LastName, Email, Password, BirthDate, MembershipLevel, Score)
VALUES
    ('33333333-3333-3333-3333-333333333333', 'Maria', 'Garcia', 'maria.garcia@email.com',
     '$2a$11$TgMvdaYlj5ZLE7ybPvoFi.jqSqp6S39yr3JXz34wo/ReZThKeHuYq',
     '1995-05-15', 0, 50),

    ('44444444-4444-4444-4444-444444444444', 'Carlos', 'Rodriguez', 'carlos.rodriguez@email.com',
     '$2a$11$TgMvdaYlj5ZLE7ybPvoFi.jqSqp6S39yr3JXz34wo/ReZThKeHuYq',
     '1988-08-22', 1, 150),

    ('55555555-5555-5555-5555-555555555555', 'Ana', 'Martinez', 'ana.martinez@email.com',
     '$2a$11$TgMvdaYlj5ZLE7ybPvoFi.jqSqp6S39yr3JXz34wo/ReZThKeHuYq',
     '1992-03-10', 2, 300),

    ('66666666-6666-6666-6666-666666666666', 'Pedro', 'Lopez', 'pedro.lopez@email.com',
     '$2a$11$TgMvdaYlj5ZLE7ybPvoFi.jqSqp6S39yr3JXz34wo/ReZThKeHuYq',
     '2005-11-30', 0, 25),

    ('77777777-7777-7777-7777-777777777777', 'Sofia', 'Hernandez', 'sofia.hernandez@email.com',
     '$2a$11$TgMvdaYlj5ZLE7ybPvoFi.jqSqp6S39yr3JXz34wo/ReZThKeHuYq',
     '1975-07-18', 1, 200);


-- ============================================================================
-- INSERT USER ROLES
-- ============================================================================

INSERT INTO UserRoles (UserId, RoleId)
VALUES
    ('11111111-1111-1111-1111-111111111111', 1),
    ('22222222-2222-2222-2222-222222222222', 2),
    ('33333333-3333-3333-3333-333333333333', 3),
    ('44444444-4444-4444-4444-444444444444', 3),
    ('55555555-5555-5555-5555-555555555555', 3),
    ('66666666-6666-6666-6666-666666666666', 3),
    ('77777777-7777-7777-7777-777777777777', 3);


-- ============================================================================
-- INSERT ATTRACTIONS
-- ============================================================================

INSERT INTO Attractions (Id, Name, Description, Type, MinAge, MaxCapacity, CurrentCapacity, Incidents)
VALUES
    ('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
     'Dragon Fury',
     'An intense roller coaster with 5 loops and speeds up to 120 km/h',
     1, 12, 50, 15, '[]'),

    ('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB',
     'Thunder Mountain',
     'Family-friendly coaster through a mountain landscape',
     1, 8, 80, 0, '[]'),

    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB',
     'Space Explorer VR',
     'Virtual reality space exploration experience',
     2, 10, 40, 20, '[]'),

    ('BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBC',
     'Racing Simulator',
     'High-speed racing simulator with realistic physics',
     2, 12, 30, 5, '["Minor technical glitch reported on simulator 3"]'),

    ('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC',
     'Dolphin Show',
     'Amazing dolphin and sea lion performance',
     3, 0, 200, 80, '[]'),

    ('CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCD',
     'Magic Castle Show',
     'Live magic and illusion performance',
     3, 0, 150, 0, '["Stage lighting issue - under maintenance"]'),

    ('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD',
     'Adventure Playground',
     'Interactive play area for children with climbing walls and slides',
     4, 0, 100, 35, '[]'),

    ('DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDE',
     'Virtual Gaming Zone',
     'State-of-the-art gaming consoles and VR experiences',
     4, 8, 60, 25, '[]');


-- ============================================================================
-- INSERT EVENTS
-- ============================================================================

INSERT INTO Events (Id, Name, Date, Hour, MaxCapacity, CurrentCapacity, Cost)
VALUES
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE',
     'Summer Night Spectacular',
     '2025-07-15', 20, 500, 150, 45.00),

    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEF',
     'Halloween Horror Nights',
     '2025-10-31', 19, 600, 0, 55.00),

    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEE0',
     'New Year Fireworks',
     '2025-12-31', 23, 1000, 320, 75.00),

    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEE1',
     'Spring Family Festival',
     '2025-04-20', 14, 400, 85, 35.00);


-- ============================================================================
-- INSERT EVENT-ATTRACTION ASSOCIATIONS
-- ============================================================================

INSERT INTO EventAttraction (EventId, AttractionId)
VALUES
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA'),
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB'),
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE', 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC'),
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEF', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA'),
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEF', 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDE'),
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEE0', 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC'),
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEE0', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB'),
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEE1', 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD'),
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEE1', 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB'),
    ('EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEE1', 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC');


-- ============================================================================
-- INSERT TICKETS
-- ============================================================================

INSERT INTO Tickets (Id, VisitorId, PurchaseDate, VisitDate, Type, QRCode, EventId)
VALUES
    ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF',
     '33333333-3333-3333-3333-333333333333',
     '2025-01-15', '2025-02-01', 0, 'QRQRQRQR-QRQR-QRQR-QRQR-QRQRQRQRQRQ1', NULL),

    ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFE',
     '44444444-4444-4444-4444-444444444444',
     '2025-02-20', '2025-03-15', 0, 'QRQRQRQR-QRQR-QRQR-QRQR-QRQRQRQRQRQ2', NULL),

    ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFD',
     '55555555-5555-5555-5555-555555555555',
     '2025-03-01', '2025-03-20', 0, 'QRQRQRQR-QRQR-QRQR-QRQR-QRQRQRQRQRQ3', NULL),

    ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFC',
     '66666666-6666-6666-6666-666666666666',
     '2025-03-10', '2025-04-05', 0, 'QRQRQRQR-QRQR-QRQR-QRQR-QRQRQRQRQRQ4', NULL),

    ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFB',
     '77777777-7777-7777-7777-777777777777',
     '2025-04-01', '2025-05-01', 0, 'QRQRQRQR-QRQR-QRQR-QRQR-QRQRQRQRQRQ5', NULL),

    ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFA',
     '33333333-3333-3333-3333-333333333333',
     '2025-06-01', '2025-07-15', 1, 'QRQRQRQR-QRQR-QRQR-QRQR-QRQRQRQRQRQ6',
     'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE'),

    ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFF9',
     '44444444-4444-4444-4444-444444444444',
     '2025-06-05', '2025-07-15', 1, 'QRQRQRQR-QRQR-QRQR-QRQR-QRQRQRQRQRQ7',
     'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE'),

    ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFF8',
     '55555555-5555-5555-5555-555555555555',
     '2025-11-01', '2025-12-31', 1, 'QRQRQRQR-QRQR-QRQR-QRQR-QRQRQRQRQRQ8',
     'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEE0'),

    ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFF7',
     '66666666-6666-6666-6666-666666666666',
     '2025-03-15', '2025-04-20', 1, 'QRQRQRQR-QRQR-QRQR-QRQR-QRQRQRQRQRQ9',
     'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEE1'),

    ('FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFF6',
     '77777777-7777-7777-7777-777777777777',
     '2025-03-18', '2025-04-20', 1, 'QRQRQRQR-QRQR-QRQR-QRQR-QRQRQRQRQR10',
     'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEE1');


-- ============================================================================
-- INSERT VISITOR REPORTS
-- ============================================================================

INSERT INTO VisitorReports (Id, Date, VisitorId)
VALUES
    ('VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV1', '2025-02-01', '33333333-3333-3333-3333-333333333333'),
    ('VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV2', '2025-03-15', '44444444-4444-4444-4444-444444444444'),
    ('VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV3', '2025-03-20', '55555555-5555-5555-5555-555555555555'),
    ('VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV4', '2025-03-21', '55555555-5555-5555-5555-555555555555'),
    ('VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV5', '2025-04-05', '66666666-6666-6666-6666-666666666666');


-- ============================================================================
-- INSERT ATTRACTION VISIT REPORTS
-- ============================================================================

INSERT INTO Reports (Id, EnterDate, ExitDate, AttractionId, VisitorReportId)
VALUES
    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRPR1',
     '2025-02-01 10:00:00', '2025-02-01 10:30:00',
     'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV1'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRPR2',
     '2025-02-01 11:00:00', '2025-02-01 11:45:00',
     'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV1'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRPR3',
     '2025-02-01 14:00:00', '2025-02-01 14:30:00',
     'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV1'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRPR4',
     '2025-03-15 09:30:00', '2025-03-15 10:00:00',
     'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV2'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRPR5',
     '2025-03-15 11:00:00', '2025-03-15 11:30:00',
     'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV2'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRPR6',
     '2025-03-15 15:00:00', NULL,
     'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDE',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV2'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRPR7',
     '2025-03-20 10:00:00', '2025-03-20 10:30:00',
     'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV3'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRPR8',
     '2025-03-20 12:00:00', '2025-03-20 12:40:00',
     'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV3'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRPR9',
     '2025-03-21 09:00:00', '2025-03-21 09:30:00',
     'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV4'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRP10',
     '2025-03-21 10:30:00', '2025-03-21 11:30:00',
     'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV4'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRP11',
     '2025-04-05 13:00:00', '2025-04-05 14:00:00',
     'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV5'),

    ('RPRPRPRP-RPRP-RPRP-RPRP-RPRPRPRPRP12',
     '2025-04-05 15:00:00', NULL,
     'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAB',
     'VRVRVRVR-VRVR-VRVR-VRVR-VRVRVRVRVRV5');


-- ============================================================================
-- INSERT DATETIME CONFIGURATION
-- ============================================================================

INSERT INTO DateTimeConfigurations (CurrentDateTime)
VALUES ('2025-03-01 10:00:00');


/*
================================================================================
SUCCESS! SEED DATA INSERTED
================================================================================

DATA SUMMARY:
- 7 Users (Admin, Operator, 5 Visitors)
- 8 Attractions (all types)
- 4 Events
- 10 Event-Attraction links
- 10 Tickets
- 5 Visitor Reports
- 12 Visit Reports
- 1 DateTime Config

TEST CREDENTIALS (password: admin123):
- admin@test.com
- operator@test.com
- maria.garcia@email.com
- carlos.rodriguez@email.com
- ana.martinez@email.com
- pedro.lopez@email.com
- sofia.hernandez@email.com

================================================================================
*/
