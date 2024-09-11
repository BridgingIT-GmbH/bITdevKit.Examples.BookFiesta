// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql", port: 14329)
    .WithDataVolume()
    .WithHealthCheck()
    .AddDatabase("sqldata");

builder.AddProject<Projects.Presentation_Web_Server>("presentation-web-server")
    .WaitFor(sql)
    .WithReference(sql);

builder.Build().Run();
