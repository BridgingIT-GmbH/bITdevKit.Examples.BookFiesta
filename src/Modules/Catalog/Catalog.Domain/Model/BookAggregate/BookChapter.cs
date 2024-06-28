// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using System;
using System.Diagnostics;
using BridgingIT.DevKit.Domain.Model;

[DebuggerDisplay("Id={Id}, Number={Number}, Title={Title}")]
public class BookChapter : Entity<BookChapterId, Guid>
{
    private BookChapter() { } // Private constructor required by EF Core

    private BookChapter(int number, string title, string content)
    {
        this.Number = number;
        this.SetTitle(title);
        this.SetContent(content);
    }

    public string Title { get; private set; }

    public int Number { get; private set; }

    public string Content { get; private set; }

    public static BookChapter For(int number, string title, string content)
    {
        return new BookChapter(number, title, content);
    }

    public BookChapter SetTitle(string title)
    {
        // Validate title
        this.Title = title;
        return this;
    }

    public BookChapter SetNumber(int number)
    {
        // Validate number
        this.Number = number;
        return this;
    }

    public BookChapter SetContent(string content)
    {
        // Validate content
        this.Content = content;
        return this;
    }
}