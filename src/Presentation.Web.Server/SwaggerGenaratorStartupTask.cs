// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Presentation.Web.Server;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSwag;
using NSwag.Generation.AspNetCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Common;

public class SwaggerGeneratorStartupTask : IStartupTask
{
    private readonly ILogger<SwaggerGeneratorStartupTask> logger;
    private readonly IServiceProvider serviceProvider;
    private readonly IWebHostEnvironment environment;
    private readonly SwaggerGeneratorOptions options;
    private readonly AspNetCoreOpenApiDocumentGeneratorSettings openApiSettings;

    public SwaggerGeneratorStartupTask(
        ILogger<SwaggerGeneratorStartupTask> logger,
        IServiceProvider serviceProvider,
        IWebHostEnvironment environment,
        IOptions<SwaggerGeneratorOptions> options,
        IOptions<AspNetCoreOpenApiDocumentGeneratorSettings> openApiSettings)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.environment = environment;
        this.options = options?.Value ?? new SwaggerGeneratorOptions();
        this.openApiSettings = openApiSettings?.Value ?? new AspNetCoreOpenApiDocumentGeneratorSettings();
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Starting Swagger documentation generation");

        var generator = new AspNetCoreOpenApiDocumentGenerator(this.openApiSettings);
        var fullSwaggerPath = Path.Combine(this.environment.ContentRootPath, this.options.SwaggerDirectory);
        var document = await generator.GenerateAsync(this.serviceProvider);

        foreach (var baseRoute in this.options.BaseRoutes)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var filteredDocument = new OpenApiDocument
            {
                Info = document.Info,
            };

            // Filter paths based on the base route
            foreach (var path in document.Paths)
            {
                if (path.Key.StartsWith(baseRoute, StringComparison.OrdinalIgnoreCase))
                {
                    filteredDocument.Paths[path.Key] = path.Value;
                }
            }

            // Copy all definitions (unfiltered)
            foreach(var definition in document.Definitions)
            {
                filteredDocument.Definitions[definition.Key] = definition.Value;
            }

            var json = filteredDocument.ToJson();
            var fileName = $"swagger_{SanitizeRouteForFileName(baseRoute)}.json";
            var filePath = Path.Combine(fullSwaggerPath, fileName);

            if (await this.HasChangesAsync(json, filePath, cancellationToken))
            {
                Directory.CreateDirectory(fullSwaggerPath);
                var tempFilePath = Path.GetTempFileName();
                await File.WriteAllTextAsync(tempFilePath, json, cancellationToken);
                File.Move(tempFilePath, filePath, true);

                this.logger.LogInformation("New Swagger documentation generated and saved for route: {BaseApiRoute}", baseRoute);
            }
            else
            {
                this.logger.LogInformation("No changes detected in Swagger documentation for route: {BaseApiRoute}", baseRoute);
            }
        }
    }

    private async Task<bool> HasChangesAsync(string newContent, string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return true;
        }

        var existingContent = await File.ReadAllTextAsync(filePath, cancellationToken);

        return !string.Equals(ComputeHash(existingContent), ComputeHash(newContent));
    }

#pragma warning disable SA1204
    private static string ComputeHash(string content)
#pragma warning restore SA1204
    {
        using var sha256 = SHA256.Create();
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = sha256.ComputeHash(contentBytes);

        return Convert.ToBase64String(hashBytes);
    }

    private static string SanitizeRouteForFileName(string route)
    {
        return string.Join("_", route.Split(Path.GetInvalidFileNameChars()));
    }
}

public class SwaggerGeneratorOptions
{
    public string[] BaseRoutes { get; set; } //= new[] { "/api/organization", "/api/tenants/{tenantId}/catalog", "/api/_system" };

    public string[] Tags { get; set; } = new[] { "Catalog", "Organization", "_system" };

    public string SwaggerDirectory { get; set; } = "wwwroot/swagger";
}