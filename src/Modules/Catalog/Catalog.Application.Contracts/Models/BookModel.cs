// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Application;

public class BookModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string Title { get; set; }

    public string Description { get; private set; }

    public string Isbn { get; private set; }

    public decimal Price { get; private set; }

    public BookPublisherModel Publisher { get; private set; }

    public DateOnly PublishedDate { get; private set; }

    public IEnumerable<string> Keywords { get; private set; }

    public IEnumerable<BookAuthorModel> Authors { get; private set; }

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