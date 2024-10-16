// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string Title { get; set; }

    public string Edition { get; set; }

    public string Description { get; set; }

    public string Language { get; set; }

    public string Sku { get; set; }

    public string Isbn { get; set; }

    public decimal Price { get; set; }

    public BookPublisherModel Publisher { get; set; }

    public DateOnly PublishedDate { get; set; }

    public IEnumerable<string> Keywords { get; set; }

    public IEnumerable<BookAuthorModel> Authors { get; set; }

    public BookCategoryModel[] Categories { get; set; }

    public BookChapterModel[] Chapters { get; set; }

    public TagModel[] Tags { get; set; }

    public string Version { get; set; }
}

public class BookChapterModel
{
    public string Id { get; set; }

    public string Title { get; set; }

    public int Number { get; set; }

    public string Content { get; set; }
}

public class BookPublisherModel
{
    public string Id { get; set; }

    public string Name { get; set; }
}

public class BookAuthorModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public int Position { get; set; }
}

public class BookCategoryModel
{
    public string Id { get; set; }

    public string Title { get; set; }
}