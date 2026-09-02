-- ============================
-- Database: order_db (SQL Server)
-- ============================
IF DB_ID('order_db') IS NULL
BEGIN
    CREATE DATABASE order_db;
END
GO

USE order_db;
GO

-- ============================
-- Table: orders
-- ============================
IF OBJECT_ID('dbo.orders', 'U') IS NOT NULL
    DROP TABLE dbo.orders;
GO

CREATE TABLE dbo.orders (
    order_id       UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_orders_order_id DEFAULT NEWID(),
    product_id     UNIQUEIDENTIFIER NOT NULL,
    quantity       INT NOT NULL CONSTRAINT CHK_orders_quantity CHECK (quantity > 0),
    order_status   VARCHAR(30) NOT NULL CONSTRAINT DF_orders_status DEFAULT 'CREATED',
    created_at     DATETIME2 NOT NULL CONSTRAINT DF_orders_created_at DEFAULT GETUTCDATE(),
    CONSTRAINT PK_orders PRIMARY KEY (order_id),
    CONSTRAINT chk_order_status CHECK (order_status IN ('CREATED', 'PAID', 'CANCELLED'))
);
GO

-- Note: no foreign key to a products table - Order Service intentionally
-- does not share a database with Product Service. product_id is only ever
-- validated by calling Product Service's API.

CREATE INDEX idx_orders_product_id ON dbo.orders(product_id);
GO
