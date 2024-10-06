﻿// MIT-License
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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Common;

public class SwaggerGeneratorStartupTask(
    ILogger<SwaggerGeneratorStartupTask> logger,
    IServiceProvider serviceProvider,
    IWebHostEnvironment environment,
    IOptions<SwaggerGeneratorOptions> options,
    IOptions<AspNetCoreOpenApiDocumentGeneratorSettings> openApiSettings)
    : IStartupTask
{
    private readonly SwaggerGeneratorOptions options = options?.Value ?? new SwaggerGeneratorOptions();
    private readonly AspNetCoreOpenApiDocumentGeneratorSettings openApiSettings = openApiSettings?.Value ?? new AspNetCoreOpenApiDocumentGeneratorSettings();

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Swagger documentation generation");

        var generator = new AspNetCoreOpenApiDocumentGenerator(this.openApiSettings);
        var fullSwaggerPath = Path.Combine(environment.ContentRootPath, this.options.SwaggerDirectory);
        var document = await generator.GenerateAsync(serviceProvider);

        var tags = this.DiscoverTags(document);

        // Generate full Swagger document
        await this.GenerateSwaggerFile(document, "all", fullSwaggerPath, cancellationToken);

        // Generate Swagger documents for each tag
        foreach (var tag in tags)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var filteredDocument = new OpenApiDocument
            {
                Info = document.Info,
            };

            filteredDocument.Info.Title = $"{document.Info.Title} - {tag}";

            foreach (var path in document.Paths)
            {
                if (path.Value.Values.Any(operation => operation.Tags.Contains(tag)))
                {
                    filteredDocument.Paths[path.Key] = path.Value;
                }
            }

            foreach (var definition in document.Definitions)
            {
                filteredDocument.Definitions[definition.Key] = definition.Value;
            }

            await this.GenerateSwaggerFile(filteredDocument, tag, fullSwaggerPath, cancellationToken);
        }
    }

    private HashSet<string> DiscoverTags(OpenApiDocument document)
    {
        var tags = new HashSet<string>();

        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Values)
            {
                foreach (var tag in operation.Tags)
                {
                    tags.Add(tag);
                }
            }
        }

        return tags;
    }

    private async Task GenerateSwaggerFile(OpenApiDocument document, string tag, string fullSwaggerPath, CancellationToken cancellationToken)
    {
        var json = document.ToJson();
        var fileName = tag == "all" ? "swagger.json" : $"swagger_{this.SanitizeForFileName(tag)}.json";
        var filePath = Path.Combine(fullSwaggerPath, fileName);

        if (await this.HasChangesAsync(json, filePath, cancellationToken))
        {
            Directory.CreateDirectory(fullSwaggerPath);
            var tempFilePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFilePath, json, cancellationToken);
            File.Move(tempFilePath, filePath, true);

            logger.LogInformation("New Swagger documentation generated and saved: {FilePath}", filePath);
        }
        else
        {
            logger.LogInformation("No changes detected in Swagger documentation for tag: {FilePath}", filePath);
        }
    }

    private async Task<bool> HasChangesAsync(string newContent, string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return true;
        }

        var existingContent = await File.ReadAllTextAsync(filePath, cancellationToken);

        return !string.Equals(HashHelper.Compute(existingContent), HashHelper.Compute(newContent));
    }

    private string SanitizeForFileName(string tag)
    {
        return string.Join("_", tag.Split(Path.GetInvalidFileNameChars())).Replace("__", "_").ToLower();
    }
}

public class SwaggerGeneratorOptions
{
    public string SwaggerDirectory { get; set; } = "wwwroot/swagger";
}