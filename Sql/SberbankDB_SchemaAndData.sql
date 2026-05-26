IF DB_ID(N'SberbankDB') IS NOT NULL
BEGIN
    ALTER DATABASE SberbankDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SberbankDB;
END
GO

CREATE DATABASE SberbankDB;
GO

USE SberbankDB;
GO

CREATE TABLE Roles
(
    RoleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_Name UNIQUE
);

CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Login NVARCHAR(50) NOT NULL CONSTRAINT UQ_Users_Login UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    RoleId INT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

CREATE TABLE Departments
(
    DepartmentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Departments PRIMARY KEY,
    Name NVARCHAR(120) NOT NULL,
    Address NVARCHAR(250) NULL,
    Phone NVARCHAR(30) NULL
);

CREATE TABLE Employees
(
    EmployeeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employees PRIMARY KEY,
    UserId INT NULL,
    DepartmentId INT NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    Position NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(30) NULL,
    Email NVARCHAR(120) NULL,
    CONSTRAINT FK_Employees_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId)
);

CREATE TABLE Clients
(
    ClientId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clients PRIMARY KEY,
    UserId INT NOT NULL CONSTRAINT UQ_Clients_UserId UNIQUE,
    FullName NVARCHAR(150) NULL,
    PassportNumber NVARCHAR(30) NULL,
    Phone NVARCHAR(30) NULL,
    Email NVARCHAR(120) NULL,
    Address NVARCHAR(250) NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Clients_CreatedAt DEFAULT GETDATE(),
    CONSTRAINT FK_Clients_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

CREATE TABLE BankProducts
(
    ProductId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BankProducts PRIMARY KEY,
    Name NVARCHAR(120) NOT NULL,
    ProductType NVARCHAR(60) NOT NULL,
    Description NVARCHAR(500) NULL,
    Rate DECIMAL(9,2) NOT NULL CONSTRAINT DF_BankProducts_Rate DEFAULT 0,
    ServiceCost DECIMAL(12,2) NOT NULL CONSTRAINT DF_BankProducts_ServiceCost DEFAULT 0,
    IsActive BIT NOT NULL CONSTRAINT DF_BankProducts_IsActive DEFAULT 1
);

CREATE TABLE ApplicationStatuses
(
    StatusId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ApplicationStatuses PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL CONSTRAINT UQ_ApplicationStatuses_Name UNIQUE
);

CREATE TABLE Applications
(
    ApplicationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Applications PRIMARY KEY,
    ClientId INT NOT NULL,
    ProductId INT NOT NULL,
    StatusId INT NOT NULL,
    EmployeeId INT NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Applications_CreatedAt DEFAULT GETDATE(),
    Comment NVARCHAR(500) NULL,
    CONSTRAINT FK_Applications_Clients FOREIGN KEY (ClientId) REFERENCES Clients(ClientId),
    CONSTRAINT FK_Applications_BankProducts FOREIGN KEY (ProductId) REFERENCES BankProducts(ProductId),
    CONSTRAINT FK_Applications_Statuses FOREIGN KEY (StatusId) REFERENCES ApplicationStatuses(StatusId),
    CONSTRAINT FK_Applications_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId)
);

CREATE TABLE Payments
(
    PaymentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
    ApplicationId INT NOT NULL,
    Amount DECIMAL(12,2) NOT NULL,
    PaymentDate DATETIME NOT NULL CONSTRAINT DF_Payments_PaymentDate DEFAULT GETDATE(),
    Purpose NVARCHAR(200) NOT NULL,
    CONSTRAINT FK_Payments_Applications FOREIGN KEY (ApplicationId) REFERENCES Applications(ApplicationId)
);
GO

INSERT INTO Roles(Name)
VALUES (N'Admin'), (N'Client'), (N'Employee');

INSERT INTO Users(Login, Password, RoleId, IsActive)
VALUES
(N'Admin', N'Admin1!', 1, 1),
(N'ivanov', N'User1!', 2, 1),
(N'petrova', N'Client2!', 2, 1),
(N'smirnov_manager', N'Manager1!', 3, 1);

INSERT INTO Departments(Name, Address, Phone)
VALUES
(N'Отдел розничного обслуживания', N'г. Тюмень, ул. Республики, 10', N'+7 (3452) 11-22-33'),
(N'Кредитный отдел', N'г. Тюмень, ул. Ленина, 25', N'+7 (3452) 44-55-66'),
(N'Отдел по работе с юридическими лицами', N'г. Тюмень, ул. Мельникайте, 80', N'+7 (3452) 77-88-99');

INSERT INTO Employees(UserId, DepartmentId, FullName, Position, Phone, Email)
VALUES
(4, 2, N'Смирнов Алексей Павлович', N'Кредитный специалист', N'+7 900 100-20-30', N'smirnov@sberbank.local'),
(NULL, 1, N'Кузнецова Мария Сергеевна', N'Менеджер клиентского зала', N'+7 900 200-30-40', N'kuznetsova@sberbank.local');

INSERT INTO Clients(UserId, FullName, PassportNumber, Phone, Email, Address)
VALUES
(2, N'Иванов Иван Иванович', N'7111 123456', N'+7 912 111-22-33', N'ivanov@example.com', N'г. Тюмень, ул. Первомайская, 1'),
(3, N'Петрова Анна Викторовна', N'7112 654321', N'+7 922 444-55-66', N'petrova@example.com', N'г. Тюмень, ул. Мира, 15');

INSERT INTO BankProducts(Name, ProductType, Description, Rate, ServiceCost, IsActive)
VALUES
(N'Дебетовая карта СберКарта', N'Карта', N'Карта для ежедневных покупок, переводов и снятия наличных.', 3.00, 150.00, 1),
(N'Кредитная карта 120 дней', N'Кредитная карта', N'Кредитная карта с льготным периодом и лимитом по решению банка.', 29.90, 150000.00, 1),
(N'Потребительский кредит', N'Кредит', N'Кредит наличными на личные цели с индивидуальной ставкой.', 18.90, 300000.00, 1),
(N'Вклад СберВклад', N'Вклад', N'Срочный вклад с начислением процентов на выбранный срок.', 14.50, 10000.00, 1),
(N'Ипотека на готовое жилье', N'Ипотека', N'Ипотечная программа для покупки квартиры на вторичном рынке.', 16.20, 12000.00, 1),
(N'Расчетный счет для бизнеса', N'Бизнес', N'Открытие и обслуживание расчетного счета для ООО и ИП.', 1.50, 990.00, 1);

INSERT INTO ApplicationStatuses(Name)
VALUES (N'Новая'), (N'В работе'), (N'Одобрена'), (N'Отклонена'), (N'Закрыта');

INSERT INTO Applications(ClientId, ProductId, StatusId, EmployeeId, CreatedAt, Comment)
VALUES
(1, 1, 3, 2, DATEADD(DAY, -10, GETDATE()), N'Карта выдана клиенту.'),
(1, 3, 2, 1, DATEADD(DAY, -2, GETDATE()), N'Проверка кредитной истории.'),
(2, 4, 1, NULL, DATEADD(DAY, -1, GETDATE()), N'Клиент интересуется вкладом на 12 месяцев.');

INSERT INTO Payments(ApplicationId, Amount, PaymentDate, Purpose)
VALUES
(1, 0.00, DATEADD(DAY, -9, GETDATE()), N'Выпуск дебетовой карты'),
(2, 0.00, DATEADD(DAY, -2, GETDATE()), N'Рассмотрение кредитной заявки');
GO

CREATE INDEX IX_Applications_ClientId ON Applications(ClientId);
CREATE INDEX IX_Applications_ProductId ON Applications(ProductId);
CREATE INDEX IX_Applications_StatusId ON Applications(StatusId);
GO
