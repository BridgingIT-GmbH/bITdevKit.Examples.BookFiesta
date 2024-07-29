// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Application;

public class AuthorModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string PersonName { get; set; }

    public string Biography { get; set; }

    public TagModel[] Tags { get; set; }

    public string Version { get; set; }
}