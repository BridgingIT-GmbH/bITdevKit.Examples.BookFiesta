// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.GettingStarted.Domain;

using System;
using System.Diagnostics;
using System.Linq;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Model;
using static System.Net.Mime.MediaTypeNames;

[DebuggerDisplay("Id={Id}, Title={Title}")]
public class Book : AuditableAggregateRoot<BookId, Guid>, IConcurrent
{
    private readonly List<AuthorId> authorIds = [];
    private readonly List<Category> categories = [];
    private readonly List<Tag> tags = [];
    private List<BookChapter> chapters = [];

    private Book() { } // Private constructor required by EF Core

    private Book(string title, string description, BookIsbn isbn, Money price)
    {
        this.SetTitle(title);
        this.SetDescription(description);
        this.SetIsbn(isbn);
        this.SetPrice(price);
    }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public BookIsbn Isbn { get; private set; }

    public Money Price { get; private set; }

    //public IEnumerable<AuthorId> AuthorIds => this.authorIds;

    public IEnumerable<Category> Categories => this.categories.OrderBy(e => e.Order);

    public IEnumerable<BookChapter> Chapters => this.chapters.OrderBy(e => e.Number);

    public IEnumerable<Tag> Tags => this.tags.OrderBy(e => e.Name);

    public Guid Version { get; set; }

    public static Book Create(string title, string description, BookIsbn isbn, Money price)
    {
        return new Book(title, description, isbn, price);
    }

    public Book SetTitle(string title)
    {
        // Validate title
        this.Title = title;
        return this;
    }

    public Book SetDescription(string description)
    {
        // Validate description
        this.Description = description;
        return this;
    }

    public Book SetIsbn(BookIsbn isbn)
    {
        this.Isbn = isbn;
        return this;
    }

    public Book SetPrice(Money price)
    {
        // Validate price is > 0
        this.Price = price;
        return this;
    }

    public Book AddAuthor(AuthorId authorId)
    {
        if (!this.authorIds.Contains(authorId))
        {
            this.authorIds.Add(authorId);
        }

        return this;
    }

    public Book RemoveAuthor(AuthorId authorId)
    {
        this.authorIds.Remove(authorId);

        return this;
    }

    public Book AddChapter(string title, string content = null)
    {
        return this.AddChapter(title, this.chapters.LastOrDefault()?.Number + 1 ?? 1, content);
    }

    public Book AddChapter(string title, int number, string content = null)
    {
        // Validate title
        var index = this.chapters.FindIndex(c => c.Number == number);
        if (index < 0)
        {
            this.chapters.Add(BookChapter.For(number, title, content));
        }
        else
        {
            this.chapters.Insert(index, BookChapter.For(number, title, content));
        }

        this.chapters = this.ReindexChapters(this.chapters);

        return this;
    }

    public Book UpdateChapter(BookChapter chapter)
    {
        return this.UpdateChapter(chapter.Id.Value, chapter.Title, chapter.Number, chapter.Content);
    }

    public Book UpdateChapter(BookChapterId id, string title, int number, string content = null)
    {
        var chapter = this.chapters.SingleOrDefault(c => c.Id == id);
        if (chapter is not null)
        {
            chapter.SetTitle(title);
            chapter.SetNumber(number);
            chapter.SetContent(content);

            this.chapters = this.ReindexChapters(this.chapters);
        }

        return this;
    }

    public Book RemoveChapter(BookChapter chapter)
    {
        return this.RemoveChapter(chapter.Id.Value);
    }

    public Book RemoveChapter(int number)
    {
        var chapter = this.chapters.SingleOrDefault(c => c.Number == number);
        if (chapter is not null)
        {
            this.RemoveChapter(chapter.Id.Value);
        }

        return this;
    }

    public Book RemoveChapter(BookChapterId id)
    {
        this.chapters.RemoveAll(c => c.Id == id);
        this.chapters = this.ReindexChapters(this.chapters);

        return this;
    }

    public Book AddCategory(Category category)
    {
        if (!this.categories.Contains(category))
        {
            this.categories.Add(category);
        }

        return this;
    }

    public Book RemoveCategory(Category category)
    {
        this.categories.Remove(category);

        return this;
    }

    public Book AddTag(Tag tag)
    {
        if (!this.tags.Contains(tag))
        {
            this.tags.Add(tag);
        }

        return this;
    }

    public Book RemoveTag(TagId tagId)
    {
        this.tags.RemoveAll(t => t.Id == tagId);

        return this;
    }

    private List<BookChapter> ReindexChapters(IEnumerable<BookChapter> chapters)
    {
        // First, sort the chapters by their number to ensure they are in order.
        var sortedChapters = chapters.OrderBy(c => c.Number).ToList();

        // Use a HashSet to keep track of used numbers to easily identify gaps.
        var usedNumbers = new HashSet<int>();

        for (var i = 0; i < sortedChapters.Count; i++)
        {
            var currentChapter = sortedChapters[i];
            var expectedNumber = i + 1;

            // If the current chapter's number is already used or it's less than the expected number,
            // and the expected number hasn't been used, then set the chapter number to the expected number.
            if ((usedNumbers.Contains(currentChapter.Number) || currentChapter.Number < expectedNumber) && !usedNumbers.Contains(expectedNumber))
            {
                currentChapter.SetNumber(expectedNumber);
                usedNumbers.Add(expectedNumber);
            }
            else if (!usedNumbers.Contains(currentChapter.Number))
            {
                // If the current chapter's number is not used, just add it to the set of used numbers.
                usedNumbers.Add(currentChapter.Number);
            }
            else
            {
                // Find the next available number that isn't used.
                while (usedNumbers.Contains(expectedNumber))
                {
                    expectedNumber++;
                }

                currentChapter.SetNumber(expectedNumber);
                usedNumbers.Add(expectedNumber);
            }
        }

        // After reindexing, the chapters list might be out of order, so sort it again.
        return [.. sortedChapters.OrderBy(c => c.Number)];
    }
}