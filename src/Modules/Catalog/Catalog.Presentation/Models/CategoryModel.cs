// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Presentation;

public class CategoryModel
{
    public string Id { get; set; }

    public int Order { get; set; }

    public string Title { get; set; }

    public string ParentId { get; set; }

    public CategoryModel[] Children { get; set; }

    public string Version { get; set; }
}

public class BookModel
{
    public string Id { get; set; }

    public string Title { get; set; }

    public CategoryModel[] Categories { get; set; }

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

public class TagModel
{
    public string Id { get; set; }

    public string Name { get; set; }
}