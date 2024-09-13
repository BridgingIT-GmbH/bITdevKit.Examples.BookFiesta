// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Number={Number}, Title={Title}")]
[TypedEntityId<Guid>]
public class BookChapter : Entity<BookChapterId>
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

    public static BookChapter Create(int number, string title, string content)
    {
        return new BookChapter(number, title, content);
    }

    public BookChapter SetTitle(string title)
    {
        _ = title ?? throw new DomainRuleException("BookChapter Title cannot be empty.");

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