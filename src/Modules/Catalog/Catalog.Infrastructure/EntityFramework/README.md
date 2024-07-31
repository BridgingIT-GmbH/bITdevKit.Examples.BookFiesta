# BookFiesta - Catalog Module

## Create and apply a new Database Migration

These database commands should be executed from the solution root folder.

### new migration: 
- `dotnet ef migrations add Initial --context CatalogDbContext --output-dir .\EntityFramework\Migrations --project .\src\Modules\Catalog\Catalog.Infrastructure\Catalog.Infrastructure.csproj --startup-project .\src\Presentation.Web.Server\Presentation.Web.Server.csproj`

### update database: 
- `dotnet ef database update --context CatalogDbContext --project .\src\Modules\Catalog\Catalog.Infrastructure\Catalog.Infrastructure.csproj --startup-project .\src\Presentation.Web.Server\Presentation.Web.Server.csproj`