# Entity Framework Core Migration Guide

## Prerequisites
- PostgreSQL installed and running
- .NET 10 SDK installed

## Configuration

### Connection Strings
- **Production**: `appsettings.json` - Connection string for production database (TechADb)
- **Development**: `appsettings.Development.json` - Connection string for development database (TechADb_Dev)

Update the connection strings with your PostgreSQL credentials:
```
Host=localhost;Port=5432;Database=YourDatabaseName;Username=your_username;Password=your_password
```

## EF Core Migration Commands

### 1. Create Initial Migration
Run this command from the solution directory:
```bash
dotnet ef migrations add InitialCreate --project TechA.Repository --startup-project TechA.Api
```

### 2. Update Database
Apply migrations to create/update the database:
```bash
dotnet ef database update --project TechA.Repository --startup-project TechA.Api
```

### 3. Add New Migration (after model changes)
```bash
dotnet ef migrations add YourMigrationName --project TechA.Repository --startup-project TechA.Api
```

### 4. Remove Last Migration (if not applied to database)
```bash
dotnet ef migrations remove --project TechA.Repository --startup-project TechA.Api
```

### 5. List All Migrations
```bash
dotnet ef migrations list --project TechA.Repository --startup-project TechA.Api
```

### 6. Generate SQL Script
```bash
dotnet ef migrations script --project TechA.Repository --startup-project TechA.Api
```

## Database Schema

### Tables Created
- **WeatherForecasts**
  - Date (Primary Key)
  - TemperatureC (int)
  - Summary (string, max 100 chars)

## Notes
- Migrations are stored in the `TechA.Repository` project
- The `ApplicationDbContext` is configured to use PostgreSQL
- The connection string is read from `appsettings.json`
