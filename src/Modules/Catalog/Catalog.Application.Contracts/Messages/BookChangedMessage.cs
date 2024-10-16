// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookUpdatedMessage
{
    public string TenantId { get; set; }

    public string BookId { get; set; }

    public string Sku { get; set; }

    public string Title { get; set; }

    public string Isbn { get; set; }

    public decimal Price { get; set; }
}