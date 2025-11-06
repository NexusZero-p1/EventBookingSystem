CREATE DATABASE EventBookingSystem;
GO

USE EventBookingSystem;
GO

CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(100) NOT NULL,
    EventDate DATETIME NOT NULL,
    AvailableTickets INT NOT NULL,
    Price DECIMAL(10,2) NOT NULL
);

CREATE TABLE Bookings (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT FOREIGN KEY REFERENCES Events(EventID),
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerEmail NVARCHAR(100) NOT NULL,
    TicketsCount INT NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    BookingDate DATETIME DEFAULT GETDATE()
);

INSERT INTO Events (EventName, EventDate, AvailableTickets, Price) VALUES
('Рок концерт', '2024-12-20 19:00:00', 100, 1500.00),
('Джаз вечер', '2024-12-15 20:00:00', 50, 2000.00),
('Классика', '2024-12-25 18:00:00', 80, 1200.00);