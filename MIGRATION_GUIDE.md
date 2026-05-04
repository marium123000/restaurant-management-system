# 🔄 Database Migration Guide

## ⚠️ IMPORTANT: Read Before Proceeding

Your database schema has been significantly updated. You need to apply these changes to your SQL Server database.

## Option 1: Fresh Start (Recommended for Development)

If you don't need to preserve existing data:

### Step 1: Drop Existing Database
```sql
-- Run this in SQL Server Management Studio or Azure Data Studio
USE master;
GO
DROP DATABASE IF EXISTS OnlineRestDb;
GO
```

### Step 2: Let EF Core Recreate Database
The application is configured to automatically create the database on startup with `dbContext.Database.EnsureCreated()` in Program.cs.

Just run the application and the database will be created with all the new schema and seed data!

```bash
dotnet run
```

## Option 2: Use EF Core Migrations (Production Approach)

If you want to preserve existing data or use proper migrations:

### Step 1: Remove EnsureCreated from Program.cs

Comment out this line in `Program.cs`:
```csharp
// dbContext.Database.EnsureCreated(); // Comment this out
```

### Step 2: Create Initial Migration
```bash
# Navigate to project directory
cd OnlineRest

# Create migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

### Step 3: If You Get Errors

If you have existing database with different schema:

```bash
# Remove all migrations
dotnet ef migrations remove

# Drop database manually (SQL Server)
# Then create fresh migration
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Option 3: Manual SQL Script

If you prefer manual control, here's what changed:

### New Columns to Add:

**Users Table:**
```sql
ALTER TABLE Users ADD LoyaltyPoints INT NOT NULL DEFAULT 0;
ALTER TABLE Users ADD PhoneNumber NVARCHAR(20) NULL;
```

**FoodItems Table:**
```sql
ALTER TABLE FoodItems ADD Category NVARCHAR(50) NOT NULL DEFAULT 'Main Course';
ALTER TABLE FoodItems ADD TotalSold INT NOT NULL DEFAULT 0;
```

**Orders Table:**
```sql
ALTER TABLE Orders ADD PaymentMethod NVARCHAR(50) NOT NULL DEFAULT 'Cash on Delivery';
ALTER TABLE Orders ADD PaymentStatus NVARCHAR(20) NULL DEFAULT 'Pending';
ALTER TABLE Orders ADD PointsEarned INT NOT NULL DEFAULT 0;
ALTER TABLE Orders ADD PointsRedeemed INT NOT NULL DEFAULT 0;
ALTER TABLE Orders ADD SpecialInstructions NVARCHAR(MAX) NULL;
```

**New Table - TableReservations:**
```sql
CREATE TABLE TableReservations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    CustomerName NVARCHAR(MAX) NOT NULL,
    Email NVARCHAR(MAX) NOT NULL,
    PhoneNumber NVARCHAR(MAX) NOT NULL,
    ReservationDate DATETIME NOT NULL,
    ReservationTime NVARCHAR(MAX) NOT NULL,
    NumberOfGuests INT NOT NULL,
    SpecialRequests NVARCHAR(MAX) NULL,
    Status NVARCHAR(MAX) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

## 🎯 Recommended Approach for Your Project:

**Use Option 1 (Fresh Start)** because:
1. ✅ Simplest and fastest
2. ✅ Gets you all the new seed data (15 food items, sample orders)
3. ✅ No migration conflicts
4. ✅ Clean slate for testing

## After Migration:

### Default Login Credentials:

**Admin Account:**
- Email: admin@restaurant.com
- Password: Admin123@

**Customer Accounts:**
- Email: customer@gmail.com
- Password: Customer123@

- Email: hashir@gmail.com
- Password: !1@2#3

### Verify Database:

1. Check that all tables exist:
   - Users
   - FoodItems
   - Orders
   - OrderDetails
   - TableReservations

2. Verify seed data:
   - 3 users (1 admin, 2 customers)
   - 15 food items across 4 categories
   - 3 sample orders

3. Test the application:
   - Login as admin
   - Browse menu
   - Add items to cart
   - Place an order

## Troubleshooting:

### Error: "Database already exists"
```bash
# Drop the database first
USE master;
DROP DATABASE OnlineRestDb;
```

### Error: "Migration already applied"
```bash
# Remove migrations folder
# Delete: OnlineRest/Migrations folder
# Then create fresh migration
```

### Error: "Cannot connect to database"
Check your connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=OnlineRestDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## 📝 Notes:

- The application uses `EnsureCreated()` which is perfect for development
- For production, you should use proper migrations
- All seed data is defined in `AppDbContext.cs` in the `SeedData()` method
- The database will be created automatically when you run the application

## ✅ Quick Start:

```bash
# 1. Drop old database (if exists)
# Run in SQL Server Management Studio:
# DROP DATABASE OnlineRestDb;

# 2. Run the application
cd OnlineRest
dotnet run

# 3. Open browser
# https://localhost:5001

# 4. Login as admin
# Email: admin@restaurant.com
# Password: Admin123@
```

That's it! Your database will be created with all the new features! 🎉
