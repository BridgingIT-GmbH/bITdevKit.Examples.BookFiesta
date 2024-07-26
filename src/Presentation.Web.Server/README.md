# BookStore

## Create and apply a new Database Migration

## Update the generated ApiClient

### Prerequisites
- `dotnet new tool-manifest`
- `dotnet tool install NSwag.ConsoleCore`

The tools manifest can be found [here](../../../.config/dotnet-tools.json)

### Install the dotnet tools
- `dotnet tool restore`

### Start the web project
- `dotnet run --project '.\src\Presentation.Web.Server\Presentation.Web.Server.csproj'`

### Update the swagger file
- `dotnet nswag run '.\src\Presentation.Web.Server\nswag.json'`

Rebuild the solution and the ApiClient should be updated. For details see the OpenApiReference target in the Client project.