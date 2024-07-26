// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Application;

public class CategoryModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public int Order { get; set; }

    public string Title { get; set; }

    public string ParentId { get; set; }

    public CategoryModel[] Children { get; set; }

    public string Version { get; set; }
}