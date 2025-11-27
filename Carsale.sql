
USE master;
GO

-- ==============================================================
-- PART 1: CREATE DATABASE
-- ==============================================================

IF DB_ID('CARSALE_DB') IS NOT NULL
BEGIN
    ALTER DATABASE CARSALE_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CARSALE_DB;
END
GO

CREATE DATABASE CARSALE_DB
ON PRIMARY 
(
    NAME = N'CARSALE_Data',
    FILENAME = N'C:\SQLData\CARSALE_Data.mdf',
    SIZE = 500MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 100MB
)
LOG ON 
(
    NAME = N'CARSALE_Log',
    FILENAME = N'C:\SQLData\CARSALE_Log. ldf',
    SIZE = 100MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 50MB
);
GO

USE CARSALE_DB;
GO

-- ==============================================================
-- PART 2: CREATE TABLES
-- ==============================================================

-- Table: CUSTOMER
CREATE TABLE CUSTOMER (
    MAKH INT IDENTITY(1,1) PRIMARY KEY,
    HOTEN NVARCHAR(100),
    EMAIL NVARCHAR(100) UNIQUE NOT NULL,
    SDT NVARCHAR(20),
    MATKHAU NVARCHAR(256),
    DIACHI NVARCHAR(200),
    NGAYDANGKY DATETIME DEFAULT GETDATE()
);

-- Table: CAR
CREATE TABLE CAR (
    MAXE INT IDENTITY(1,1) PRIMARY KEY,
    TENXE NVARCHAR(100),
    HANGXE NVARCHAR(50),
    GIA DECIMAL(12,2),
    NAMSX INT,
    MOTA NVARCHAR(MAX),
    HINHANH NVARCHAR(200),
    TRANGTHAI NVARCHAR(30) DEFAULT N'Còn hàng'
);

-- Table: ORDERS
CREATE TABLE ORDERS (
    MADON INT IDENTITY(1,1) PRIMARY KEY,
    MAKH INT NOT NULL,
    NGAYDAT DATETIME DEFAULT GETDATE(),
    TONGTIEN DECIMAL(12,2),
    TRANGTHAI NVARCHAR(30) DEFAULT N'Chờ xử lý',
    CONSTRAINT FK_ORDER_CUSTOMER FOREIGN KEY (MAKH) REFERENCES CUSTOMER(MAKH)
);

-- Table: ORDER_DETAIL
CREATE TABLE ORDER_DETAIL (
    MADON INT NOT NULL,
    MAXE INT NOT NULL,
    SOLUONG INT,
    DONGIA DECIMAL(12,2),
    CONSTRAINT PK_ORDER_DETAIL PRIMARY KEY (MADON, MAXE),
    CONSTRAINT FK_ORDERDETAIL_ORDER FOREIGN KEY (MADON) REFERENCES ORDERS(MADON),
    CONSTRAINT FK_ORDERDETAIL_CAR FOREIGN KEY (MAXE) REFERENCES CAR(MAXE)
);

-- Table: FEEDBACK
CREATE TABLE FEEDBACK (
    MAFB INT IDENTITY(1,1) PRIMARY KEY,
    MAKH INT NOT NULL,
    MAXE INT NOT NULL,
    NOIDUNG NVARCHAR(1000),
    DIEMDANHGIA INT,
    NGAYDANHGIA DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_FEEDBACK_CUSTOMER FOREIGN KEY (MAKH) REFERENCES CUSTOMER(MAKH),
    CONSTRAINT FK_FEEDBACK_CAR FOREIGN KEY (MAXE) REFERENCES CAR(MAXE)
);

-- Table: AUDIT_LOG
CREATE TABLE AUDIT_LOG (
    MALOG INT IDENTITY(1,1) PRIMARY KEY,
    MATK INT,
    HANHDONG NVARCHAR(100),
    BANGTACDONG NVARCHAR(50),
    NGAYGIO DATETIME DEFAULT GETDATE(),
    IP NVARCHAR(30)
);

-- Table: ACCOUNT_ROLE
CREATE TABLE ACCOUNT_ROLE (
    MATK INT IDENTITY(1,1) PRIMARY KEY,
    MAKH INT NOT NULL,
    ROLENAME NVARCHAR(30),
    CONSTRAINT FK_ROLE_CUSTOMER FOREIGN KEY (MAKH) REFERENCES CUSTOMER(MAKH)
);

GO

-- ==============================================================
-- PART 3: CREATE STORED PROCEDURES
-- ==============================================================

-- Procedure: Register Customer
CREATE OR ALTER PROCEDURE SP_REGISTER_CUSTOMER
    @HOTEN NVARCHAR(100),
    @EMAIL NVARCHAR(100),
    @SDT NVARCHAR(20),
    @MATKHAU NVARCHAR(100),
    @DIACHI NVARCHAR(200),
    @RESULT INT OUTPUT,
    @MESSAGE NVARCHAR(200) OUTPUT,
    @MAKH INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if email exists
    IF EXISTS (SELECT 1 FROM CUSTOMER WHERE EMAIL = @EMAIL)
    BEGIN
        SET @RESULT = 0;
        SET @MESSAGE = N'Email đã được sử dụng';
        SET @MAKH = NULL;
        RETURN;
    END
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Hash password with SHA2_256
        DECLARE @EncryptedPassword NVARCHAR(256);
        SET @EncryptedPassword = CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', @MATKHAU), 2);
        
        -- Insert customer
        INSERT INTO CUSTOMER (HOTEN, EMAIL, SDT, MATKHAU, DIACHI)
        VALUES (@HOTEN, @EMAIL, @SDT, @EncryptedPassword, @DIACHI);
        
        SET @MAKH = SCOPE_IDENTITY();
        
        -- Assign default role
        INSERT INTO ACCOUNT_ROLE (MAKH, ROLENAME)
        VALUES (@MAKH, 'CUSTOMER');
        
        -- Audit log
        INSERT INTO AUDIT_LOG (MATK, HANHDONG, BANGTACDONG)
        VALUES (@MAKH, 'REGISTER', 'CUSTOMER');
        
        COMMIT TRANSACTION;
        
        SET @RESULT = 1;
        SET @MESSAGE = N'Đăng ký thành công';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @RESULT = 0;
        SET @MESSAGE = ERROR_MESSAGE();
        SET @MAKH = NULL;
    END CATCH
END
GO

-- Procedure: Login Customer
CREATE OR ALTER PROCEDURE SP_LOGIN_CUSTOMER
    @EMAIL NVARCHAR(100),
    @MATKHAU NVARCHAR(100),
    @IP NVARCHAR(30),
    @RESULT INT OUTPUT,
    @MESSAGE NVARCHAR(200) OUTPUT,
    @MAKH INT OUTPUT,
    @HOTEN NVARCHAR(100) OUTPUT,
    @ROLENAME NVARCHAR(30) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Hash password
    DECLARE @EncryptedPassword NVARCHAR(256);
    SET @EncryptedPassword = CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', @MATKHAU), 2);
    
    -- Check credentials
    SELECT TOP 1 
        @MAKH = c.MAKH,
        @HOTEN = c.HOTEN,
        @ROLENAME = ISNULL(ar.ROLENAME, 'CUSTOMER')
    FROM CUSTOMER c
    LEFT JOIN ACCOUNT_ROLE ar ON c.MAKH = ar. MAKH
    WHERE c. EMAIL = @EMAIL 
    AND c.MATKHAU = @EncryptedPassword;
    
    IF @MAKH IS NULL
    BEGIN
        SET @RESULT = 0;
        SET @MESSAGE = N'Email hoặc mật khẩu không đúng';
        RETURN;
    END
    
    -- Success
    SET @RESULT = 1;
    SET @MESSAGE = N'Đăng nhập thành công';
    
    -- Audit log
    INSERT INTO AUDIT_LOG (MATK, HANHDONG, BANGTACDONG, IP)
    VALUES (@MAKH, 'LOGIN', 'CUSTOMER', @IP);
END
GO

-- Procedure: Logout Customer
CREATE OR ALTER PROCEDURE SP_LOGOUT_CUSTOMER
    @MAKH INT,
    @IP NVARCHAR(30),
    @RESULT INT OUTPUT,
    @MESSAGE NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO AUDIT_LOG (MATK, HANHDONG, BANGTACDONG, IP)
    VALUES (@MAKH, 'LOGOUT', 'CUSTOMER', @IP);
    
    SET @RESULT = 1;
    SET @MESSAGE = N'Đăng xuất thành công';
END
GO

-- Procedure: Get All Cars (with filter)
CREATE OR ALTER PROCEDURE SP_GET_ALL_CARS
    @SEARCH NVARCHAR(100) = NULL,
    @HANGXE NVARCHAR(50) = NULL,
    @MIN_PRICE DECIMAL(12,2) = NULL,
    @MAX_PRICE DECIMAL(12,2) = NULL,
    @NAMSX INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        MAXE, TENXE, HANGXE, GIA, NAMSX, MOTA, HINHANH, TRANGTHAI
    FROM CAR
    WHERE 
        (@SEARCH IS NULL OR TENXE LIKE '%' + @SEARCH + '%')
        AND (@HANGXE IS NULL OR HANGXE = @HANGXE)
        AND (@MIN_PRICE IS NULL OR GIA >= @MIN_PRICE)
        AND (@MAX_PRICE IS NULL OR GIA <= @MAX_PRICE)
        AND (@NAMSX IS NULL OR NAMSX = @NAMSX)
    ORDER BY MAXE DESC;
END
GO

-- Procedure: Get Car by ID
CREATE OR ALTER PROCEDURE SP_GET_CAR_BY_ID
    @MAXE INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        MAXE, TENXE, HANGXE, GIA, NAMSX, MOTA, HINHANH, TRANGTHAI
    FROM CAR
    WHERE MAXE = @MAXE;
END
GO

-- Procedure: Get Brands
CREATE OR ALTER PROCEDURE SP_GET_BRANDS
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT HANGXE 
    FROM CAR 
    WHERE HANGXE IS NOT NULL
    ORDER BY HANGXE;
END
GO

-- Procedure: Create Order
CREATE OR ALTER PROCEDURE SP_CREATE_ORDER
    @MAKH INT,
    @MAXE INT,
    @SOLUONG INT,
    @RESULT INT OUTPUT,
    @MESSAGE NVARCHAR(200) OUTPUT,
    @MADON INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Get car price
        DECLARE @DONGIA DECIMAL(12,2);
        SELECT @DONGIA = GIA FROM CAR WHERE MAXE = @MAXE;
        
        IF @DONGIA IS NULL
        BEGIN
            SET @RESULT = 0;
            SET @MESSAGE = N'Xe không tồn tại';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        DECLARE @TONGTIEN DECIMAL(12,2) = @DONGIA * @SOLUONG;
        
        -- Create order
        INSERT INTO ORDERS (MAKH, TONGTIEN, TRANGTHAI)
        VALUES (@MAKH, @TONGTIEN, N'Chờ xử lý');
        
        SET @MADON = SCOPE_IDENTITY();
        
        -- Create order detail
        INSERT INTO ORDER_DETAIL (MADON, MAXE, SOLUONG, DONGIA)
        VALUES (@MADON, @MAXE, @SOLUONG, @DONGIA);
        
        -- Audit log
        INSERT INTO AUDIT_LOG (MATK, HANHDONG, BANGTACDONG)
        VALUES (@MAKH, 'CREATE_ORDER', 'ORDERS');
        
        COMMIT TRANSACTION;
        
        SET @RESULT = 1;
        SET @MESSAGE = N'Đặt hàng thành công';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @RESULT = 0;
        SET @MESSAGE = ERROR_MESSAGE();
        SET @MADON = NULL;
    END CATCH
END
GO

-- Procedure: Get My Orders
CREATE OR ALTER PROCEDURE SP_GET_MY_ORDERS
    @MAKH INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.MADON,
        o.NGAYDAT,
        o. TONGTIEN,
        o. TRANGTHAI,
        od.MAXE,
        c.TENXE,
        c. HANGXE,
        od.SOLUONG,
        od.DONGIA
    FROM ORDERS o
    INNER JOIN ORDER_DETAIL od ON o.MADON = od. MADON
    INNER JOIN CAR c ON od.MAXE = c.MAXE
    WHERE o.MAKH = @MAKH
    ORDER BY o.NGAYDAT DESC;
END
GO

-- ==============================================================
-- PART 4: CREATE TRIGGERS
-- ==============================================================

-- Trigger: Audit CUSTOMER INSERT
CREATE OR ALTER TRIGGER TRG_AUDIT_CUSTOMER_INSERT
ON CUSTOMER
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO AUDIT_LOG (MATK, HANHDONG, BANGTACDONG)
    SELECT MAKH, 'INSERT', 'CUSTOMER'
    FROM inserted;
END
GO

-- Trigger: Audit CUSTOMER UPDATE
CREATE OR ALTER TRIGGER TRG_AUDIT_CUSTOMER_UPDATE
ON CUSTOMER
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO AUDIT_LOG (MATK, HANHDONG, BANGTACDONG)
    SELECT MAKH, 'UPDATE', 'CUSTOMER'
    FROM inserted;
END
GO

-- Trigger: Audit CUSTOMER DELETE
CREATE OR ALTER TRIGGER TRG_AUDIT_CUSTOMER_DELETE
ON CUSTOMER
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO AUDIT_LOG (MATK, HANHDONG, BANGTACDONG)
    SELECT MAKH, 'DELETE', 'CUSTOMER'
    FROM deleted;
END
GO

-- ==============================================================
-- PART 5: CREATE VIEWS
-- ==============================================================

-- View: User Roles
CREATE OR ALTER VIEW VW_USER_ROLES AS
SELECT 
    c.MAKH,
    c.HOTEN,
    c.EMAIL,
    c.SDT,
    ar.ROLENAME,
    c.NGAYDANGKY,
    (SELECT COUNT(*) FROM AUDIT_LOG WHERE MATK = c.MAKH) AS TOTAL_ACTIVITIES
FROM CUSTOMER c
LEFT JOIN ACCOUNT_ROLE ar ON c.MAKH = ar. MAKH;
GO

-- View: Audit Log Detail
CREATE OR ALTER VIEW VW_AUDIT_LOG_DETAIL AS
SELECT 
    al.MALOG,
    al.MATK,
    c.HOTEN,
    c.EMAIL,
    ar.ROLENAME,
    al.HANHDONG,
    al.BANGTACDONG,
    al.NGAYGIO,
    al.IP
FROM AUDIT_LOG al
LEFT JOIN CUSTOMER c ON al. MATK = c.MAKH
LEFT JOIN ACCOUNT_ROLE ar ON al.MATK = ar. MATK;
GO

-- View: Order Summary
CREATE OR ALTER VIEW VW_ORDER_SUMMARY AS
SELECT 
    o.MADON,
    o.MAKH,
    c.HOTEN,
    c.EMAIL,
    o.NGAYDAT,
    o. TONGTIEN,
    o. TRANGTHAI,
    COUNT(od.MAXE) AS SO_XE
FROM ORDERS o
INNER JOIN CUSTOMER c ON o.MAKH = c.MAKH
LEFT JOIN ORDER_DETAIL od ON o.MADON = od. MADON
GROUP BY o. MADON, o.MAKH, c.HOTEN, c. EMAIL, o.NGAYDAT, o.TONGTIEN, o.TRANGTHAI;
GO

-- ==============================================================
-- PART 6: INSERT SAMPLE DATA (VinFast Cars)
-- ==============================================================

SET IDENTITY_INSERT CAR ON;

INSERT INTO CAR (MAXE, TENXE, HANGXE, GIA, NAMSX, MOTA, HINHANH, TRANGTHAI)
VALUES 
(1, N'VinFast Fadil', N'VinFast', 425000000, 2021, 
N'Xe hatchback 5 chỗ cỡ nhỏ, phù hợp đô thị.  Tiết kiệm nhiên liệu. ', 
'images/vinfast_fadil.jpg', N'Hết hàng'),

(2, N'VinFast Lux A2. 0', N'VinFast', 1100000000, 2020, 
N'Sedan hạng D cao cấp, động cơ BMW 2.0L turbo 228 mã lực. ', 
'images/vinfast_lux_a. jpg', N'Hết hàng'),

(3, N'VinFast Lux SA2.0', N'VinFast', 1350000000, 2021, 
N'SUV 7 chỗ hạng sang, khung gầm BMW, động cơ 2.0L turbo. ', 
'images/vinfast_lux_sa.jpg', N'Hết hàng'),

(4, N'VinFast President', N'VinFast', 4600000000, 2021, 
N'SUV hạng sang giới hạn, động cơ V8 6.2L công suất 420 mã lực.', 
'images/vinfast_president.jpg', N'Hết hàng'),

(5, N'VinFast VF e34', N'VinFast', 690000000, 2022, 
N'SUV điện đầu tiên, pin 42 kWh, quãng đường 300km. Sạc nhanh.', 
'images/vinfast_vfe34.jpg', N'Còn hàng'),

(6, N'VinFast VF 5 Plus', N'VinFast', 468000000, 2023, 
N'SUV điện mini 5 chỗ, pin 37. 23 kWh, quãng đường 326km. ', 
'images/vinfast_vf5. jpg', N'Còn hàng'),

(7, N'VinFast VF 6', N'VinFast', 675000000, 2024, 
N'SUV điện compact 5 chỗ, pin 59.6 kWh, quãng đường 410km.', 
'images/vinfast_vf6.jpg', N'Còn hàng'),

(8, N'VinFast VF 7 Base', N'VinFast', 850000000, 2024, 
N'SUV điện 5 chỗ tầm trung, pin 59.6 kWh, quãng đường 375km.', 
'images/vinfast_vf7_base.jpg', N'Còn hàng'),

(9, N'VinFast VF 7 Plus', N'VinFast', 999000000, 2024, 
N'Bản cao cấp VF 7, pin 75.3 kWh, quãng đường 450km.  ADAS Level 2+.', 
'images/vinfast_vf7_plus. jpg', N'Còn hàng'),

(10, N'VinFast VF 8 Eco', N'VinFast', 1090000000, 2023, 
N'SUV điện cao cấp, pin 82 kWh, quãng đường 447km.  AWD 260 kW.', 
'images/vinfast_vf8_eco.jpg', N'Còn hàng'),

(11, N'VinFast VF 8 Plus', N'VinFast', 1250000000, 2023, 
N'Bản cao cấp VF 8, pin 87.7 kWh, quãng đường 471km. ADAS Level 2+.', 
'images/vinfast_vf8_plus.jpg', N'Còn hàng'),

(12, N'VinFast VF 9 Eco', N'VinFast', 1590000000, 2023, 
N'SUV điện 7 chỗ flagship, pin 92 kWh, quãng đường 438km. AWD 300 kW.', 
'images/vinfast_vf9_eco.jpg', N'Còn hàng'),

(13, N'VinFast VF 9 Plus', N'VinFast', 1890000000, 2023, 
N'Bản cao cấp VF 9, pin 123 kWh, quãng đường 594km.  Nội thất siêu sang.', 
'images/vinfast_vf9_plus.jpg', N'Còn hàng'),

(14, N'VinFast VF 3', N'VinFast', 235000000, 2024, 
N'Xe điện mini 2 cửa 4 chỗ, pin 18.64 kWh, quãng đường 215km.', 
'images/vinfast_vf3.jpg', N'Còn hàng'),

(15, N'VinFast VF Wild', N'VinFast', 2500000000, 2025, 
N'Bán tải điện, pin 150 kWh, quãng đường 600km. AWD 330 kW.', 
'images/vinfast_vf_wild.jpg', N'Còn hàng');

SET IDENTITY_INSERT CAR OFF;
GO

-- ==============================================================
-- PART 7: CREATE INDEXES (for performance)
-- ==============================================================

CREATE NONCLUSTERED INDEX IX_CUSTOMER_EMAIL ON CUSTOMER(EMAIL);
CREATE NONCLUSTERED INDEX IX_CAR_HANGXE ON CAR(HANGXE);
CREATE NONCLUSTERED INDEX IX_CAR_GIA ON CAR(GIA);
CREATE NONCLUSTERED INDEX IX_ORDERS_MAKH ON ORDERS(MAKH);
CREATE NONCLUSTERED INDEX IX_AUDIT_LOG_MATK ON AUDIT_LOG(MATK);
CREATE NONCLUSTERED INDEX IX_AUDIT_LOG_NGAYGIO ON AUDIT_LOG(NGAYGIO);
GO

-- ==============================================================
-- PART 8: VERIFICATION
-- ==============================================================

PRINT '========================================';
PRINT 'CARSALE DATABASE SETUP COMPLETED! ';
PRINT '========================================';
PRINT '';
PRINT 'Tables created:';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;

PRINT '';
PRINT 'Stored Procedures created:';
SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' ORDER BY ROUTINE_NAME;

PRINT '';
PRINT 'Views created:';
SELECT TABLE_NAME FROM INFORMATION_SCHEMA. VIEWS ORDER BY TABLE_NAME;

PRINT '';
PRINT 'Cars inserted:';
SELECT COUNT(*) AS TOTAL_CARS FROM CAR;

GO
-- ==============================================================
-- PART 1. 5: CREATE LOGINS, USERS & GRANT PERMISSIONS (FIXED)
-- ==============================================================

USE master;
GO

-- ==============================================================
-- 1. CREATE SQL SERVER LOGINS (Server-level)
-- ==============================================================

-- Login for Admin
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'carsale_admin')
BEGIN
    CREATE LOGIN carsale_admin WITH PASSWORD = 'Admin@123456', 
    DEFAULT_DATABASE = CARSALE_DB,
    CHECK_POLICY = OFF,
    CHECK_EXPIRATION = OFF;
    PRINT 'Created login: carsale_admin';
END
GO

-- Login for Manager
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'carsale_manager')
BEGIN
    CREATE LOGIN carsale_manager WITH PASSWORD = 'Manager@123456', 
    DEFAULT_DATABASE = CARSALE_DB,
    CHECK_POLICY = OFF,
    CHECK_EXPIRATION = OFF;
    PRINT 'Created login: carsale_manager';
END
GO

-- Login for Sales
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'carsale_sales')
BEGIN
    CREATE LOGIN carsale_sales WITH PASSWORD = 'Sales@123456', 
    DEFAULT_DATABASE = CARSALE_DB,
    CHECK_POLICY = OFF,
    CHECK_EXPIRATION = OFF;
    PRINT 'Created login: carsale_sales';
END
GO

-- Login for Customer (app connection)
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'carsale_app')
BEGIN
    CREATE LOGIN carsale_app WITH PASSWORD = 'App@123456', 
    DEFAULT_DATABASE = CARSALE_DB,
    CHECK_POLICY = OFF,
    CHECK_EXPIRATION = OFF;
    PRINT 'Created login: carsale_app';
END
GO

-- ✅ Grant server-level permissions (if needed) - MUST BE IN MASTER
-- Grant VIEW ANY DATABASE to admin (optional)
GRANT VIEW ANY DATABASE TO carsale_admin;
GO

-- ==============================================================
-- 2.  SWITCH TO DATABASE & CREATE USERS
-- ==============================================================

USE CARSALE_DB;
GO

-- Create users from logins
IF NOT EXISTS (SELECT * FROM sys. database_principals WHERE name = 'carsale_admin')
BEGIN
    CREATE USER carsale_admin FOR LOGIN carsale_admin;
    PRINT 'Created user: carsale_admin';
END
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'carsale_manager')
BEGIN
    CREATE USER carsale_manager FOR LOGIN carsale_manager;
    PRINT 'Created user: carsale_manager';
END
GO

IF NOT EXISTS (SELECT * FROM sys. database_principals WHERE name = 'carsale_sales')
BEGIN
    CREATE USER carsale_sales FOR LOGIN carsale_sales;
    PRINT 'Created user: carsale_sales';
END
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'carsale_app')
BEGIN
    CREATE USER carsale_app FOR LOGIN carsale_app;
    PRINT 'Created user: carsale_app';
END
GO

-- ==============================================================
-- 3. CREATE DATABASE ROLES
-- ==============================================================

-- Role: ADMIN
IF NOT EXISTS (SELECT * FROM sys. database_principals WHERE name = 'CARSALE_ADMIN_ROLE' AND type = 'R')
BEGIN
    CREATE ROLE CARSALE_ADMIN_ROLE;
    PRINT 'Created role: CARSALE_ADMIN_ROLE';
END
GO

-- Role: MANAGER
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'CARSALE_MANAGER_ROLE' AND type = 'R')
BEGIN
    CREATE ROLE CARSALE_MANAGER_ROLE;
    PRINT 'Created role: CARSALE_MANAGER_ROLE';
END
GO

-- Role: SALES
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'CARSALE_SALES_ROLE' AND type = 'R')
BEGIN
    CREATE ROLE CARSALE_SALES_ROLE;
    PRINT 'Created role: CARSALE_SALES_ROLE';
END
GO

-- Role: VIEWER
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'CARSALE_VIEWER_ROLE' AND type = 'R')
BEGIN
    CREATE ROLE CARSALE_VIEWER_ROLE;
    PRINT 'Created role: CARSALE_VIEWER_ROLE';
END
GO

-- ==============================================================
-- 4.  GRANT PERMISSIONS TO ROLES (Database-level only)
-- ==============================================================

-- ========== ADMIN ROLE (Full Database Control) ==========
-- Grant db_owner role (easiest way for full control)
ALTER ROLE db_owner ADD MEMBER CARSALE_ADMIN_ROLE;

-- OR grant individual permissions:
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO CARSALE_ADMIN_ROLE;
GRANT EXECUTE ON SCHEMA::dbo TO CARSALE_ADMIN_ROLE;
GRANT VIEW DEFINITION ON SCHEMA::dbo TO CARSALE_ADMIN_ROLE;
GRANT ALTER ON SCHEMA::dbo TO CARSALE_ADMIN_ROLE;
GO

-- ========== MANAGER ROLE ==========
-- Tables
GRANT SELECT, INSERT, UPDATE ON CUSTOMER TO CARSALE_MANAGER_ROLE;
GRANT SELECT, INSERT, UPDATE ON CAR TO CARSALE_MANAGER_ROLE;
GRANT SELECT, INSERT, UPDATE ON ORDERS TO CARSALE_MANAGER_ROLE;
GRANT SELECT, INSERT, UPDATE ON ORDER_DETAIL TO CARSALE_MANAGER_ROLE;
GRANT SELECT ON FEEDBACK TO CARSALE_MANAGER_ROLE;
GRANT SELECT ON AUDIT_LOG TO CARSALE_MANAGER_ROLE;
GRANT SELECT ON ACCOUNT_ROLE TO CARSALE_MANAGER_ROLE;

-- Stored Procedures
GRANT EXECUTE ON SP_GET_ALL_CARS TO CARSALE_MANAGER_ROLE;
GRANT EXECUTE ON SP_GET_CAR_BY_ID TO CARSALE_MANAGER_ROLE;
GRANT EXECUTE ON SP_GET_BRANDS TO CARSALE_MANAGER_ROLE;
GRANT EXECUTE ON SP_CREATE_ORDER TO CARSALE_MANAGER_ROLE;
GRANT EXECUTE ON SP_GET_MY_ORDERS TO CARSALE_MANAGER_ROLE;

-- Views
GRANT SELECT ON VW_USER_ROLES TO CARSALE_MANAGER_ROLE;
GRANT SELECT ON VW_AUDIT_LOG_DETAIL TO CARSALE_MANAGER_ROLE;
GRANT SELECT ON VW_ORDER_SUMMARY TO CARSALE_MANAGER_ROLE;
GO

-- ========== SALES ROLE ==========
-- Tables
GRANT SELECT ON CUSTOMER TO CARSALE_SALES_ROLE;
GRANT SELECT ON CAR TO CARSALE_SALES_ROLE;
GRANT SELECT, INSERT, UPDATE ON ORDERS TO CARSALE_SALES_ROLE;
GRANT SELECT, INSERT, UPDATE ON ORDER_DETAIL TO CARSALE_SALES_ROLE;
GRANT SELECT ON FEEDBACK TO CARSALE_SALES_ROLE;

-- Stored Procedures
GRANT EXECUTE ON SP_GET_ALL_CARS TO CARSALE_SALES_ROLE;
GRANT EXECUTE ON SP_GET_CAR_BY_ID TO CARSALE_SALES_ROLE;
GRANT EXECUTE ON SP_GET_BRANDS TO CARSALE_SALES_ROLE;
GRANT EXECUTE ON SP_CREATE_ORDER TO CARSALE_SALES_ROLE;
GRANT EXECUTE ON SP_GET_MY_ORDERS TO CARSALE_SALES_ROLE;

-- Views
GRANT SELECT ON VW_ORDER_SUMMARY TO CARSALE_SALES_ROLE;
GO

-- ========== VIEWER ROLE (Read-only) ==========
-- Tables
GRANT SELECT ON CAR TO CARSALE_VIEWER_ROLE;
GRANT SELECT ON FEEDBACK TO CARSALE_VIEWER_ROLE;

-- Stored Procedures
GRANT EXECUTE ON SP_GET_ALL_CARS TO CARSALE_VIEWER_ROLE;
GRANT EXECUTE ON SP_GET_CAR_BY_ID TO CARSALE_VIEWER_ROLE;
GRANT EXECUTE ON SP_GET_BRANDS TO CARSALE_VIEWER_ROLE;
GO

-- ==============================================================
-- 5.  ASSIGN USERS TO ROLES
-- ==============================================================

-- Admin → db_owner (full control)
ALTER ROLE db_owner ADD MEMBER carsale_admin;
PRINT 'Assigned carsale_admin to db_owner';
GO

-- Manager → MANAGER role
ALTER ROLE CARSALE_MANAGER_ROLE ADD MEMBER carsale_manager;
PRINT 'Assigned carsale_manager to CARSALE_MANAGER_ROLE';
GO

-- Sales → SALES role
ALTER ROLE CARSALE_SALES_ROLE ADD MEMBER carsale_sales;
PRINT 'Assigned carsale_sales to CARSALE_SALES_ROLE';
GO

-- App → Custom permissions
ALTER ROLE CARSALE_VIEWER_ROLE ADD MEMBER carsale_app;

-- Grant specific SPs for app user
GRANT EXECUTE ON SP_REGISTER_CUSTOMER TO carsale_app;
GRANT EXECUTE ON SP_LOGIN_CUSTOMER TO carsale_app;
GRANT EXECUTE ON SP_LOGOUT_CUSTOMER TO carsale_app;
GRANT EXECUTE ON SP_CREATE_ORDER TO carsale_app;
GRANT EXECUTE ON SP_GET_MY_ORDERS TO carsale_app;

PRINT 'Assigned carsale_app to CARSALE_VIEWER_ROLE with extra permissions';
GO

-- ==============================================================
-- 6. CREATE ADMIN HELPER PROCEDURES
-- ==============================================================

-- Procedure: Grant Role to User
CREATE OR ALTER PROCEDURE SP_GRANT_ROLE_TO_USER
    @USERNAME NVARCHAR(100),
    @ROLENAME NVARCHAR(100),
    @RESULT INT OUTPUT,
    @MESSAGE NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @USERNAME AND type IN ('S', 'U'))
        BEGIN
            SET @RESULT = 0;
            SET @MESSAGE = N'User không tồn tại: ' + @USERNAME;
            RETURN;
        END
        
        -- Check if role exists
        IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @ROLENAME AND type = 'R')
        BEGIN
            SET @RESULT = 0;
            SET @MESSAGE = N'Role không tồn tại: ' + @ROLENAME;
            RETURN;
        END
        
        -- Grant role
        DECLARE @SQL NVARCHAR(500);
        SET @SQL = N'ALTER ROLE ' + QUOTENAME(@ROLENAME) + N' ADD MEMBER ' + QUOTENAME(@USERNAME);
        EXEC sp_executesql @SQL;
        
        SET @RESULT = 1;
        SET @MESSAGE = N'Đã gán role ' + @ROLENAME + N' cho user ' + @USERNAME;
        
        -- Audit log
        INSERT INTO AUDIT_LOG (MATK, HANHDONG, BANGTACDONG)
        VALUES (0, 'GRANT_ROLE: ' + @ROLENAME, 'USER: ' + @USERNAME);
    END TRY
    BEGIN CATCH
        SET @RESULT = 0;
        SET @MESSAGE = ERROR_MESSAGE();
    END CATCH
END
GO

-- Procedure: Revoke Role from User
CREATE OR ALTER PROCEDURE SP_REVOKE_ROLE_FROM_USER
    @USERNAME NVARCHAR(100),
    @ROLENAME NVARCHAR(100),
    @RESULT INT OUTPUT,
    @MESSAGE NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DECLARE @SQL NVARCHAR(500);
        SET @SQL = N'ALTER ROLE ' + QUOTENAME(@ROLENAME) + N' DROP MEMBER ' + QUOTENAME(@USERNAME);
        EXEC sp_executesql @SQL;
        
        SET @RESULT = 1;
        SET @MESSAGE = N'Đã thu hồi role ' + @ROLENAME + N' từ user ' + @USERNAME;
        
        -- Audit log
        INSERT INTO AUDIT_LOG (MATK, HANHDONG, BANGTACDONG)
        VALUES (0, 'REVOKE_ROLE: ' + @ROLENAME, 'USER: ' + @USERNAME);
    END TRY
    BEGIN CATCH
        SET @RESULT = 0;
        SET @MESSAGE = ERROR_MESSAGE();
    END CATCH
END
GO

-- Procedure: List User Roles
CREATE OR ALTER PROCEDURE SP_LIST_USER_ROLES
    @USERNAME NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.name AS UserName,
        r.name AS RoleName,
        r. type_desc AS RoleType
    FROM sys.database_principals u
    INNER JOIN sys.database_role_members rm ON u.principal_id = rm.member_principal_id
    INNER JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
    WHERE (@USERNAME IS NULL OR u.name = @USERNAME)
    AND u.type IN ('S', 'U')
    ORDER BY u.name, r.name;
END
GO

-- Procedure: List User Permissions
CREATE OR ALTER PROCEDURE SP_LIST_USER_PERMISSIONS
    @USERNAME NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.name AS UserName,
        p.class_desc AS PermissionClass,
        p.permission_name AS Permission,
        OBJECT_NAME(p.major_id) AS ObjectName,
        p.state_desc AS GrantState
    FROM sys.database_principals u
    INNER JOIN sys.database_permissions p ON u. principal_id = p.grantee_principal_id
    WHERE u.name = @USERNAME
    ORDER BY p.class_desc, ObjectName, p.permission_name;
END
GO

-- ==============================================================
-- 7.  VERIFICATION
-- ==============================================================

PRINT '';
PRINT '========================================';
PRINT 'USERS & PERMISSIONS SETUP COMPLETED';
PRINT '========================================';
PRINT '';

PRINT 'Database Users:';
SELECT 
    name AS UserName,
    type_desc AS UserType,
    create_date AS CreatedDate
FROM sys.database_principals
WHERE name LIKE 'carsale_%'
AND type IN ('S', 'U')
ORDER BY name;

PRINT '';
PRINT 'Database Roles:';
SELECT 
    name AS RoleName,
    type_desc AS RoleType,
    create_date AS CreatedDate
FROM sys.database_principals
WHERE name LIKE 'CARSALE_%'
AND type = 'R'
ORDER BY name;

PRINT '';
PRINT 'User-Role Memberships:';
EXEC SP_LIST_USER_ROLES;

PRINT '';
PRINT '========================================';
PRINT 'LOGIN CREDENTIALS:';
PRINT '========================================';
PRINT 'Admin:   carsale_admin / Admin@123456 (db_owner)';
PRINT 'Manager: carsale_manager / Manager@123456';
PRINT 'Sales:   carsale_sales / Sales@123456';
PRINT 'App:     carsale_app / App@123456';
PRINT '========================================';
PRINT '';

GO
USE CARSALE_DB;
GO

-- ==============================================================
-- CREATE ADMIN & MANAGER USERS
-- ==============================================================

DECLARE @AdminEmail NVARCHAR(100) = 'admin@carsale.com';
DECLARE @AdminPassword NVARCHAR(100) = 'Admin@123';
DECLARE @AdminId INT;

DECLARE @ManagerEmail NVARCHAR(100) = 'manager@carsale.com';
DECLARE @ManagerPassword NVARCHAR(100) = 'Manager@123';
DECLARE @ManagerId INT;

-- ==============================================================
-- 1. CREATE ADMIN USER
-- ==============================================================

-- Check if admin exists
IF NOT EXISTS (SELECT 1 FROM CUSTOMER WHERE EMAIL = @AdminEmail)
BEGIN
    PRINT '🔄 Creating ADMIN user...';
    
    -- ✅ Insert without specifying MAKH (let IDENTITY auto-generate)
    INSERT INTO CUSTOMER (HOTEN, EMAIL, SDT, MATKHAU, DIACHI, NGAYDANGKY)
    VALUES (
        N'Administrator',
        @AdminEmail,
        '0900000000',
        CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', @AdminPassword), 2),
        N'Trụ sở VinFast, Hà Nội',
        GETDATE()
    );
    
    -- Get the auto-generated ID
    SET @AdminId = SCOPE_IDENTITY();
    
    -- Assign ADMIN role
    INSERT INTO ACCOUNT_ROLE (MAKH, ROLENAME)
    VALUES (@AdminId, 'ADMIN');
    
    PRINT '✅ Admin account created successfully!';
    PRINT '   Email: admin@carsale.com';
    PRINT '   Password: Admin@123';
    PRINT '   MAKH: ' + CAST(@AdminId AS NVARCHAR(10));
END
ELSE
BEGIN
    PRINT '⚠️  Admin email already exists.  Updating role...';
    
    -- Get existing admin ID
    SELECT @AdminId = MAKH FROM CUSTOMER WHERE EMAIL = @AdminEmail;
    
    -- Update or insert role
    IF EXISTS (SELECT 1 FROM ACCOUNT_ROLE WHERE MAKH = @AdminId)
    BEGIN
        UPDATE ACCOUNT_ROLE SET ROLENAME = 'ADMIN' WHERE MAKH = @AdminId;
        PRINT '✅ Role updated to ADMIN for existing user';
    END
    ELSE
    BEGIN
        INSERT INTO ACCOUNT_ROLE (MAKH, ROLENAME)
        VALUES (@AdminId, 'ADMIN');
        PRINT '✅ ADMIN role assigned to existing user';
    END
    
    PRINT '   MAKH: ' + CAST(@AdminId AS NVARCHAR(10));
END

-- ==============================================================
-- 2. CREATE MANAGER USER (OPTIONAL)
-- ==============================================================

IF NOT EXISTS (SELECT 1 FROM CUSTOMER WHERE EMAIL = @ManagerEmail)
BEGIN
    PRINT '';
    PRINT '🔄 Creating MANAGER user...';
    
    INSERT INTO CUSTOMER (HOTEN, EMAIL, SDT, MATKHAU, DIACHI, NGAYDANGKY)
    VALUES (
        N'Quản lý hệ thống',
        @ManagerEmail,
        '0900000001',
        CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', @ManagerPassword), 2),
        N'Văn phòng VinFast, TP.HCM',
        GETDATE()
    );
    
    SET @ManagerId = SCOPE_IDENTITY();
    
    INSERT INTO ACCOUNT_ROLE (MAKH, ROLENAME)
    VALUES (@ManagerId, 'MANAGER');
    
    PRINT '✅ Manager account created successfully!';
    PRINT '   Email: manager@carsale.com';
    PRINT '   Password: Manager@123';
    PRINT '   MAKH: ' + CAST(@ManagerId AS NVARCHAR(10));
END
ELSE
BEGIN
    PRINT '';
    PRINT '⚠️  Manager email already exists. Skipping...';
END

-- ==============================================================
-- 3.  VERIFY RESULTS
-- ==============================================================

PRINT '';
PRINT '========================================';
PRINT 'ALL ADMIN USERS:';
PRINT '========================================';

SELECT 
    c.MAKH,
    c.HOTEN,
    c.EMAIL,
    c.SDT,
    ar. ROLENAME,
    c.NGAYDANGKY
FROM CUSTOMER c
INNER JOIN ACCOUNT_ROLE ar ON c.MAKH = ar.MAKH
WHERE ar. ROLENAME IN ('ADMIN', 'MANAGER')
ORDER BY ar.ROLENAME, c.MAKH;

PRINT '';
PRINT '========================================';
PRINT 'LOGIN CREDENTIALS:';
PRINT '========================================';
PRINT 'ADMIN:';
PRINT '  Email: admin@carsale.com';
PRINT '  Password: Admin@123';
PRINT '';
PRINT 'MANAGER:';
PRINT '  Email: manager@carsale.com';
PRINT '  Password: Manager@123';
PRINT '========================================';

GO