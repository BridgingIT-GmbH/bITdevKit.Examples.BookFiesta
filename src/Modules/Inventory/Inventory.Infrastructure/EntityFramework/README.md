# BookFiesta - Inventory Module

## Create and apply a new Database Migration

These database commands should be executed from the solution root folder.

### new migration:

-

`dotnet ef migrations add Initial --context InventoryDbContext --output-dir .\EntityFramework\Migrations --project .\src\Modules\Inventory\Inventory.Infrastructure\Inventory.Infrastructure.csproj --startup-project .\src\Presentation.Web.Server\Presentation.Web.Server.csproj`

### update database:

-

`dotnet ef database update --context InventoryDbContext --project .\src\Modules\Inventory\Inventory.Infrastructure\Inventory.Infrastructure.csproj --startup-project .\src\Presentation.Web.Server\Presentation.Web.Server.csproj`