﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Isbn={Isbn}, Sku={Sku}, Title={Title}")]
[TypedEntityId<Guid>]
public class Book : AuditableAggregateRoot<BookId>, IConcurrent
{
    private readonly List<BookAuthor> authors = [];
    private readonly List<Category> categories = [];
    private readonly List<Tag> tags = [];
    private readonly List<BookKeyword> keywords = [];
    private List<BookChapter> chapters = [];

    private Book() { } // Private constructor required by EF Core

    private Book(
        TenantId tenantId,
        string title,
        string edition,
        string description,
        Language language,
        ProductSku sku,
        BookIsbn isbn,
        Money price,
        Publisher publisher,
        DateOnly? publishedDate = null)
    {
        this.TenantId = tenantId;
        this.SetTitle(title);
        this.SetEdition(edition);
        this.SetDescription(description);
        this.SetSku(sku);
        this.SetIsbn(isbn);
        this.SetPrice(price);
        this.SetPublisher(publisher);
        this.SetPublishedDate(publishedDate);
        this.SetLanguage(language);
        this.AverageRating = AverageRating.Create();
    }

    public TenantId TenantId { get; }

    public string Title { get; private set; }

    public string Edition { get; private set; }

    public string Description { get; private set; }

    public Language Language { get; private set; }

    public ProductSku Sku { get; private set; }

    public BookIsbn Isbn { get; private set; }

    public Money Price { get; private set; }

    public BookPublisher Publisher { get; private set; }

    public DateOnly? PublishedDate { get; private set; }

    public AverageRating AverageRating { get; }

    public IEnumerable<BookKeyword> Keywords
        => this.keywords;

    public IEnumerable<BookAuthor> Authors
        => this.authors;

    public IEnumerable<Category> Categories
        => this.categories.OrderBy(e => e.Order);

    public IEnumerable<BookChapter> Chapters
        => this.chapters.OrderBy(e => e.Number);

    public IEnumerable<Tag> Tags
        => this.tags.OrderBy(e => e.Name);

    public int StockQuantityOnHand { get; private set; }

    public int StockQuantityReserved { get; private set; }

    /// <summary>
    ///     Gets or sets the concurrency token to handle optimistic concurrency.
    /// </summary>
    public Guid Version { get; set; }

    public static Book Create(
        TenantId tenantId,
        string title,
        string edition,
        string description,
        Language language,
        ProductSku sku,
        BookIsbn isbn,
        Money price,
        Publisher publisher,
        DateOnly? publishedDate = null)
    {
        _ = tenantId ?? throw new ArgumentException("TenantId cannot be empty.");

        var book = new Book(tenantId, title, edition, description, language, sku, isbn, price, publisher, publishedDate);

        book.DomainEvents.Register(new BookChangedDomainEvent(tenantId, book), true);

        return book;
    }

    public Book SetTitle(string title)
    {
        _ = title ?? throw new ArgumentException("Book Title cannot be empty.");

        if (this.Title == title)
        {
            return this;
        }

        this.Title = title;
        this.ReindexKeywords();

        this.DomainEvents.Register(new BookChangedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetEdition(string edition)
    {
        if (this.Edition == edition)
        {
            return this;
        }

        this.Edition = edition;
        // this.ReindexKeywords();

        this.DomainEvents.Register(new BookChangedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetDescription(string description)
    {
        if (this.Description == description)
        {
            return this;
        }

        this.ReindexKeywords();

        this.DomainEvents.Register(new BookChangedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetSku(ProductSku sku)
    {
        _ = sku ?? throw new ArgumentException("Book Sku cannot be empty.");

        if (this.Sku == sku)
        {
            return this;
        }

        this.Sku = sku;

        this.DomainEvents.Register(new BookChangedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetIsbn(BookIsbn isbn)
    {
        _ = isbn ?? throw new ArgumentException("Book Isbn cannot be empty.");

        if (this.Isbn == isbn)
        {
            return this;
        }

        this.Isbn = isbn;

        this.DomainEvents.Register(new BookChangedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetPrice(Money price)
    {
        _ = price ?? throw new ArgumentException("Book Price cannot be empty.");

        if (this.Price == price)
        {
            return this;
        }

        // TODO: Validate price is > 0
        this.Price = price;

        this.DomainEvents.Register(new BookChangedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetPublisher(Publisher publisher)
    {
        _ = publisher ?? throw new ArgumentException("Book Publisher cannot be empty.");

        var bookPublisher = BookPublisher.Create(publisher);
        if (this.Publisher == bookPublisher)
        {
            return this;
        }

        this.Publisher = bookPublisher;

        this.DomainEvents.Register(new BookChangedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetPublishedDate(DateOnly? publishedDate)
    {
        if (this.PublishedDate == publishedDate)
        {
            return this;
        }

        this.PublishedDate = publishedDate;

        this.DomainEvents.Register(new BookChangedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetLanguage(Language language)
    {
        _ = language ?? throw new ArgumentException("Book Language cannot be empty.");

        if (this.Language == language)
        {
            return this;
        }

        this.Language = language;

        this.DomainEvents.Register(new BookChangedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book SetStock(int quantityOnHand, int quantityReserved)
    {
        if (this.StockQuantityOnHand == quantityOnHand && this.StockQuantityReserved == quantityReserved)
        {
            return this;
        }

        this.StockQuantityOnHand = quantityOnHand;
        this.StockQuantityReserved = quantityReserved;

        this.DomainEvents.Register(new BookChangedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Book AddRating(Rating rating)
    {
        _ = rating ?? throw new ArgumentException("Book Rating cannot be empty.");

        this.AverageRating.Add(rating);

        return this;
    }

    public Book AssignAuthor(Author author, int position = 0)
    {
        _ = author ?? throw new ArgumentException("Book Author cannot be empty.");

        if (this.authors.Any(e => e.AuthorId == author.Id))
        {
            return this;
        }

        this.authors.Add(BookAuthor.Create(author, position == 0 ? this.authors.Count + 1 : 0));
        this.ReindexKeywords();

        this.DomainEvents.Register(new BookAuthorAssignedDomainEvent(this, author));

        return this;
    }

    //public Book RemoveAuthor(AuthorId authorId)
    //{
    //    var bookAuthor = this.bookAuthors.FirstOrDefault(ba => ba.AuthorId == authorId);
    //    if (bookAuthor != null)
    //    {
    //        this.bookAuthors.Remove(bookAuthor);
    //        // Reorder remaining authors
    //        for (var i = 0; i < this.bookAuthors.Count; i++)
    //        {
    //            this.bookAuthors[i] = new BookAuthor(this.Id.Value, this.bookAuthors[i].AuthorId, i);
    //        }
    //    }

    //    return this;
    //}

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
            this.chapters.Add(BookChapter.Create(number, title, content));
        }
        else
        {
            this.chapters.Insert(index, BookChapter.Create(number, title, content));
        }

        this.chapters = this.ReindexChapters(this.chapters);
        this.ReindexKeywords();

        return this;
    }

    public Book UpdateChapter(BookChapter chapter)
    {
        return this.UpdateChapter(chapter.Id, chapter.Title, chapter.Number, chapter.Content);
    }

    public Book UpdateChapter(BookChapterId id, string title, int number, string content = null)
    {
        var chapter = this.chapters.SingleOrDefault(c => c.Id == id);
        if (chapter == null)
        {
            return this;
        }

        chapter.SetTitle(title);
        chapter.SetNumber(number);
        chapter.SetContent(content);

        this.chapters = this.ReindexChapters(this.chapters);
        this.ReindexKeywords();

        return this;
    }

    public Book RemoveChapter(BookChapter chapter)
    {
        return this.RemoveChapter(chapter.Id);
    }

    public Book RemoveChapter(int number)
    {
        var chapter = this.chapters.SingleOrDefault(c => c.Number == number);
        if (chapter == null)
        {
            return this;
        }

        this.RemoveChapter(chapter.Id);
        this.ReindexKeywords();

        return this;
    }

    public Book RemoveChapter(BookChapterId id)
    {
        this.chapters.RemoveAll(c => c.Id == id);
        this.chapters = this.ReindexChapters(this.chapters);
        this.ReindexKeywords();

        return this;
    }

    public Book AddCategory(Category category)
    {
        if (this.categories.Contains(category))
        {
            return this;
        }

        this.categories.Add(category);
        this.ReindexKeywords();

        return this;
    }

    public Book RemoveCategory(Category category)
    {
        this.categories.Remove(category);
        this.ReindexKeywords();

        return this;
    }

    public Book AddTag(Tag tag)
    {
        if (this.tags.Contains(tag))
        {
            return this;
        }

        DomainRules.Apply([new TagMustBelongToTenantRule(tag, this.TenantId)]);

        this.tags.Add(tag);
        this.ReindexKeywords();

        return this;
    }

    public Book RemoveTag(Tag tag)
    {
        if (this.tags.Contains(tag))
        {
            return this;
        }

        this.tags.Remove(tag);
        this.ReindexKeywords();

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
            if ((usedNumbers.Contains(currentChapter.Number) || currentChapter.Number < expectedNumber) &&
                !usedNumbers.Contains(expectedNumber))
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

    private List<string> ReindexKeywords()
    {
        var keywords = new HashSet<string>();
        keywords.UnionWith(this.Title.SafeNull().ToLower().Split(' ').Where(word => word.Length > 3));
        //keywords.UnionWith(this.Edition.SafeNull().ToLower().Split(' ').Where(word => word.Length > 3));
        keywords.UnionWith(this.Description.SafeNull().ToLower().Split(' ').Where(word => word.Length > 3));
        keywords.UnionWith(
            this.authors.SafeNull().SelectMany(a => a.Name.ToLower().Split(' ').Where(word => word.Length > 3)));
        //newKeywords.UnionWith(this.categories.SafeNull().SelectMany(c => c.Name.ToLower().Split(' ').Where(word => word.Length > 3)));
        keywords.UnionWith(this.tags.SafeNull().Select(t => t.Name.ToLower()));
        keywords.UnionWith(
            this.chapters.SafeNull().SelectMany(c => c.Title.ToLower().Split(' ').Where(word => word.Length > 3)));

        UpdateKeywords(keywords);

        return [.. keywords]; // TODO: order by weight?

        void UpdateKeywords(HashSet<string> newKeywords)
        {
            var existingKeywords = this.keywords.ToDictionary(ki => ki.Text);

            // Remove keywords that are no longer present
            foreach (var keyword in existingKeywords.Keys.Except(newKeywords).ToList())
            {
                this.keywords.Remove(existingKeywords[keyword]);
            }

            // Add new keywords
            foreach (var keyword in newKeywords.Except(existingKeywords.Keys))
            {
                this.keywords.Add(new BookKeyword { BookId = this.Id, Text = keyword });
            }
        }
    }
}