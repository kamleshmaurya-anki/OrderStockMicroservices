-- ============================
-- Database: product_db (SQL Server)
-- ============================
IF DB_ID('product_db') IS NULL
BEGIN
    CREATE DATABASE product_db;
END
GO

USE product_db;
GO

-- ============================
-- Table: products
-- ============================
IF OBJECT_ID('dbo.products', 'U') IS NOT NULL
    DROP TABLE dbo.products;
GO

CREATE TABLE dbo.products (
    product_id     UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_products_product_id DEFAULT NEWID(),
    product_name   NVARCHAR(150) NOT NULL,
    price          DECIMAL(10,2) NOT NULL CONSTRAINT CHK_products_price CHECK (price >= 0),
    stock_qty      INT NOT NULL CONSTRAINT CHK_products_stock_qty CHECK (stock_qty >= 0),
    is_active      BIT NOT NULL CONSTRAINT DF_products_is_active DEFAULT 1,
    created_at     DATETIME2 NOT NULL CONSTRAINT DF_products_created_at DEFAULT GETUTCDATE(),
    updated_at     DATETIME2 NULL,
    CONSTRAINT PK_products PRIMARY KEY (product_id)
);
GO

-- ============================
-- Indexes
-- ============================
CREATE INDEX idx_product_name ON dbo.products(product_name);
GO
