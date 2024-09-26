// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

var builder = DistributedApplication.CreateBuilder(args);

//var sqlPassword = builder.AddParameter("sql-password", secret: true);
var sql = builder.AddSqlServer(
        "sql")
        //port: 14329)
        // password: sqlPassword)
    .WithImageTag("latest")
    //.WithDataVolume() // requires persistent password https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/persist-data-volumes
    //.WithHealthCheck()
    .AddDatabase("sqldata");

builder.AddProject<Projects.Presentation_Web_Server>("presentation-web-server")
    .WaitFor(sql)
    .WithReference(sql);

//TODO: add SEQ integration https://learn.microsoft.com/en-us/dotnet/aspire/logging/seq-integration?tabs=dotnet-cli

builder.Build().Run();