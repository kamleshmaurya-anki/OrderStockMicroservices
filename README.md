# Order & Stock Microservices

Two independent ASP.NET Core (.NET 8) Web APIs, each with its own SQL Server
database, following a Controller → Service → Repository → DTO/Entity layout.

```
OrderStockMicroservices/
├── OrderStockMicroservices.sln
├── ProductService/            <- owns product_db
│   ├── Controllers/ProductsController.cs
│   ├── Services/               IProductService, ProductManagementService
│   ├── Repositories/           IProductRepository, ProductRepository
│   ├── DTOs/
│   ├── Entities/Product.cs
│   ├── Data/ProductDbContext.cs
│   ├── Middleware/GlobalExceptionMiddleware.cs
│   ├── Exceptions/
│   ├── Scripts/product_db.sql
│   └── Program.cs
└── OrderService/              <- owns order_db, talks to ProductService over HTTP only
    ├── Controllers/OrdersController.cs
    ├── Services/               IOrderService, OrderManagementService
    ├── Repositories/           IOrderRepository, OrderRepository
    ├── Clients/                IProductServiceClient, ProductServiceClient (HttpClient)
    ├── DTOs/
    ├── Entities/Order.cs
    ├── Data/OrderDbContext.cs
    ├── Middleware/GlobalExceptionMiddleware.cs
    ├── Exceptions/
    ├── Scripts/order_db.sql
    └── Program.cs
```

## How the inter-service rule is enforced

Order Service has **no reference at all** to `product_db` — no connection
string, no shared DbContext, nothing. The only way it learns about products
or affects stock is through `IProductServiceClient`, a typed `HttpClient`
that calls Product Service's own API:

- `GET  /api/products/{id}` — read a product (used for 404 checks / display)
- `POST /api/products/{id}/reserve-stock` — **atomically** checks and deducts
  stock in a single SQL `UPDATE ... WHERE stock_qty >= @qty`, so two
  concurrent orders can never both succeed against the same last units.
- `POST /api/products/{id}/release-stock` — compensating call: if Order
  Service reserves stock successfully but then fails to save the order row,
  it calls this to put the stock back, keeping the two databases consistent.

`OrderManagementService.CreateOrderAsync` flow:
1. Call Product Service to reserve stock for `(ProductId, Quantity)`.
2. Product not found → 404. Insufficient stock → 409 (order is never
   created). Reservation succeeds → continue.
3. Save the order (`CREATED` status). If that save throws, release the
   reserved stock back on Product Service and rethrow.

## Business rules covered

- Order cannot be placed if stock is insufficient (`409 Conflict`, order row
  is never written).
- Stock is updated atomically via a single conditional `UPDATE` statement
  (no read-then-write race).
- Global exception handling middleware in both services converts exceptions
  into consistent JSON error responses and logs everything.
- Serilog writes structured logs to `Logs/*.log` (rolling daily) plus the
  console, in both services.
- Pagination (`page`, `pageSize`, capped at 100) on both `GET /api/products`
  and `GET /api/orders`.

## Setting up SQL Server

Run each script against your SQL Server instance (SSMS, Azure Data Studio,
or `sqlcmd`):

```
sqlcmd -S localhost -i ProductService/Scripts/product_db.sql
sqlcmd -S localhost -i OrderService/Scripts/order_db.sql
```

Update the connection strings in `ProductService/appsettings.json` and
`OrderService/appsettings.json` if your SQL Server instance name, auth mode,
or credentials differ from the defaults (`Trusted_Connection=True`).

## Running in Visual Studio

1. Open `OrderStockMicroservices.sln`.
2. Restore NuGet packages (Visual Studio will prompt automatically, or
   right-click the solution → *Restore NuGet Packages*).
3. Right-click the solution → *Set Startup Projects* → *Multiple startup
   projects* → set both `ProductService` and `OrderService` to **Start**.
4. Run. Product Service comes up on `https://localhost:5001`, Order Service
   on `https://localhost:5101`. Order Service is pre-configured
   (`appsettings.json` → `ProductService:BaseUrl`) to call Product Service
   at `https://localhost:5001`.
5. Swagger UI opens for each service at `/swagger` — use Product Service's
   Swagger to create a product first, then use Order Service's Swagger to
   place an order against that `ProductId`.

## Running from the CLI

```
# from OrderStockMicroservices/
dotnet restore
dotnet run --project ProductService
dotnet run --project OrderService   # in a second terminal
```

## API quick reference

**Product Service** (`https://localhost:5001`)
| Method | Route | Purpose |
|---|---|---|
| POST | `/api/products` | Create product |
| GET | `/api/products/{id}` | Get by id |
| GET | `/api/products?page=&pageSize=` | Paged list |
| PUT | `/api/products/{id}` | Update |
| DELETE | `/api/products/{id}` | Soft delete (`is_active = 0`) |
| POST | `/api/products/{id}/reserve-stock` | Internal — atomic stock check + deduct |
| POST | `/api/products/{id}/release-stock` | Internal — compensating stock add-back |

**Order Service** (`https://localhost:5101`)
| Method | Route | Purpose |
|---|---|---|
| POST | `/api/orders` | Create order (validates stock via Product Service) |
| GET | `/api/orders/{id}` | Get by id |
| GET | `/api/orders?page=&pageSize=` | Paged list |

## Notes / known trade-offs

- No distributed transaction coordinator (e.g. saga orchestrator) is used —
  the reserve/release pattern above is a pragmatic compensating-action
  approach appropriate for this scope. In a production system with more
  services you'd typically formalize this with an outbox pattern or a saga.
- `dotnet restore` requires network access to nuget.org, which this sandbox
  doesn't have, so the projects were written by hand rather than built here.
  They follow standard .NET 8 / EF Core 8 conventions and should restore and
  run as-is once opened in Visual Studio with normal internet access.
