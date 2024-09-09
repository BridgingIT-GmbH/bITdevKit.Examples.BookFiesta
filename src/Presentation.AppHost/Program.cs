// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Presentation_Web_Server>("presentation-web-server");

builder.Build().Run();
