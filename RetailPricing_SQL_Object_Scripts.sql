--- Create Database
CREATE DATABASE RetailPricingDB;
GO

USE RetailPricingDB;
GO


--- Stores Table
CREATE TABLE Stores (
    StoreId INT NOT NULL PRIMARY KEY,
    StoreName NVARCHAR(200) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    Region NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

--- Products Table
CREATE TABLE Products (
    SKU NVARCHAR(50) NOT NULL PRIMARY KEY,
    ProductName NVARCHAR(300) NOT NULL,
    Category NVARCHAR(150) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

 
--- Pricing Records (Main Table)
CREATE TABLE PricingRecords (
    PricingRecordId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,

    StoreId INT NOT NULL,
    SKU NVARCHAR(50) NOT NULL,

    Price DECIMAL(18,2) NOT NULL,
    PriceDate DATE NOT NULL,

    UploadBatchId UNIQUEIDENTIFIER NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Pricing_Store
        FOREIGN KEY (StoreId) REFERENCES Stores(StoreId),

    CONSTRAINT FK_Pricing_Product
        FOREIGN KEY (SKU) REFERENCES Products(SKU)
);
GO

--- Prevent Duplicate Pricing Per Day Per Store Per Product
CREATE UNIQUE INDEX UX_Pricing_Unique_Record
ON PricingRecords (StoreId, SKU, PriceDate);
GO

--- Upload History Table (Audit Trail)
CREATE TABLE UploadHistory (
    UploadId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    FileName NVARCHAR(300) NOT NULL,
    UploadedBy NVARCHAR(150) NULL,
    UploadedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Status NVARCHAR(50) NOT NULL,
    TotalRecords INT NULL,
    FailedRecords INT NULL,
    Remarks NVARCHAR(MAX) NULL
);
GO

---Search by Store + Date
CREATE INDEX IX_Pricing_Store_Date
ON PricingRecords(StoreId, PriceDate);
GO

--- Search by SKU + Date
CREATE INDEX IX_Pricing_SKU_Date
ON PricingRecords(SKU, PriceDate);
GO

--- Upload Batch Query

CREATE INDEX IX_Pricing_UploadBatch
ON PricingRecords(UploadBatchId);
GO


CREATE TABLE UploadErrors (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UploadId UNIQUEIDENTIFIER NOT NULL,
    RowNumber INT NOT NULL,
    Error NVARCHAR(MAX) NOT NULL,
    RawData NVARCHAR(MAX) NULL,
    CONSTRAINT FK_UploadErrors_UploadHistory FOREIGN KEY (UploadId) REFERENCES UploadHistory(UploadId) ON DELETE CASCADE
);
CREATE INDEX IX_UploadErrors_UploadId ON UploadErrors (UploadId);

---- Once off scrits
--INSERT INTO Stores (StoreId, StoreName, Country, Region) VALUES (1, 'Reliance', 'India', 'South')
--INSERT INTO Stores (StoreId, StoreName, Country, Region) VALUES (2, 'Insta Mart', 'India', 'North')
--INSERT INTO Stores (StoreId, StoreName, Country, Region) VALUES (3, 'Walmart', 'USA', 'North')
--INSERT INTO Stores (StoreId, StoreName, Country, Region) VALUES (4, 'LuLu', 'UAE', 'East')

--INSERT INTO Products (SKU, ProductName, Category) VALUES ('RXSOU001','Horlicks','Food')
--INSERT INTO Products (SKU, ProductName, Category) VALUES ('RXSOU002','Boost','Food')
--INSERT INTO Products (SKU, ProductName, Category) VALUES ('RXSOU003','Sunflower Oil','Food')
--INSERT INTO Products (SKU, ProductName, Category) VALUES ('RXSOU004','Apple','Fruits')
--INSERT INTO Products (SKU, ProductName, Category) VALUES ('RXSOU005','Almond','Nuts')

--INSERT INTO Products (SKU, ProductName, Category) VALUES ('RXSOU011','Pen','Stationary')
--INSERT INTO Products (SKU, ProductName, Category) VALUES ('RXSOU012','Note Book','Stationary')
--INSERT INTO Products (SKU, ProductName, Category) VALUES ('RXSOU013','Chair','Furniture')
--INSERT INTO Products (SKU, ProductName, Category) VALUES ('RXSOU014','Laptop','Electronics')
--INSERT INTO Products (SKU, ProductName, Category) VALUES ('RXSOU015','Mouse','Electronics')