// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("Id={Id}, Title={Title}")]
[TypedEntityId<Guid>]
public class Book : AuditableAggregateRoot<BookId>, IConcurrent
{
    private readonly List<BookAuthor> authors = [];
    private readonly List<Category> categories = [];
    private readonly List<Tag> tags = [];
    private readonly List<BookKeyword> keywords = [];
    private List<BookChapter> chapters = [];

    private Book() { } // Private constructor required by EF Core

    private Book(TenantId tenantId, string title, string description, BookIsbn isbn, Money price, Publisher publisher, DateOnly publishedDate)
    {
        this.TenantId = tenantId;
        this.SetTitle(title);
        this.SetDescription(description);
        this.SetIsbn(isbn);
        this.SetPrice(price);
        this.SetPublisher(publisher);
        this.SetPublishedDate(publishedDate);
        this.AverageRating = AverageRating.Create();
    }

    public TenantId TenantId { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public BookIsbn Isbn { get; private set; }

    public Money Price { get; private set; }

    public BookPublisher Publisher { get; private set; }

    public DateOnly PublishedDate { get; private set; }

    public AverageRating AverageRating { get; private set; }

    public IEnumerable<BookKeyword> Keywords => this.keywords;

    public IEnumerable<BookAuthor> Authors => this.authors;

    public IEnumerable<Category> Categories => this.categories.OrderBy(e => e.Order);

    public IEnumerable<BookChapter> Chapters => this.chapters.OrderBy(e => e.Number);

    public IEnumerable<Tag> Tags => this.tags.OrderBy(e => e.Name);

    public Guid Version { get; set; }

    public static Book Create(TenantId tenantId, string title, string description, BookIsbn isbn, Money price, Publisher publisher, DateOnly publishedDate)
    {
        _ = tenantId ?? throw new DomainRuleException("TenantId cannot be empty.");

        var book = new Book(tenantId, title, description, isbn, price, publisher, publishedDate);

        book.DomainEvents.Register(
                new BookCreatedDomainEvent(tenantId, book), true);

        return book;
    }

    public Book SetTitle(string title)
    {
        // Validate title
        if (title != this.Title)
        {
            this.Title = title;
            this.ReindexKeywords();

            this.DomainEvents.Register(
                new BookUpdatedDomainEvent(this.TenantId, this), true);
        }

        return this;
    }

    public Book SetDescription(string description)
    {
        // Validate description
        if (description != this.Description)
        {
            this.Description = description;
            this.ReindexKeywords();

            this.DomainEvents.Register(
                new BookUpdatedDomainEvent(this.TenantId, this), true);
        }

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

    public Book SetPublisher(Publisher publisher)
    {
        // Validate publisher
        this.Publisher = BookPublisher.Create(publisher);
        return this;
    }

    public Book SetPublishedDate(DateOnly publishedDate)
    {
        // Validate published date
        this.PublishedDate = publishedDate;
        return this;
    }

    public Book AddRating(Rating rating)
    {
        if (rating == null)
        {
            return this;
        }

        this.AverageRating.Add(rating);

        return this;
    }

    public Book AssignAuthor(Author author, int position = 0)
    {
        if (!this.authors.Any(e => e.AuthorId == author.Id))
        {
            this.authors.Add(
                BookAuthor.Create(author, position == 0 ? this.authors.Count + 1 : 0));
            this.ReindexKeywords();

            this.DomainEvents.Register(
                new BookAuthorAssignedDomainEvent(this, author));
        }

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
        if (chapter != null)
        {
            chapter.SetTitle(title);
            chapter.SetNumber(number);
            chapter.SetContent(content);

            this.chapters = this.ReindexChapters(this.chapters);
            this.ReindexKeywords();
        }

        return this;
    }

    public Book RemoveChapter(BookChapter chapter)
    {
        return this.RemoveChapter(chapter.Id);
    }

    public Book RemoveChapter(int number)
    {
        var chapter = this.chapters.SingleOrDefault(c => c.Number == number);
        if (chapter != null)
        {
            this.RemoveChapter(chapter.Id);
            this.ReindexKeywords();
        }

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
        if (!this.categories.Contains(category))
        {
            this.categories.Add(category);
            this.ReindexKeywords();
        }

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
        if (!this.tags.Contains(tag))
        {
            DomainRules.Apply([new TagMustBelongToTenantRule(tag, this.TenantId)]);

            this.tags.Add(tag);
            this.ReindexKeywords();
        }

        return this;
    }

    public Book RemoveTag(Tag tag)
    {
        if (!this.tags.Contains(tag))
        {
            this.tags.Remove(tag);
            this.ReindexKeywords();
        }

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

    private List<string> ReindexKeywords()
    {
        var keywords = new HashSet<string>();
        keywords.UnionWith(this.Title.SafeNull().ToLower().Split(' ').Where(word => word.Length > 3));
        keywords.UnionWith(this.Description.SafeNull().ToLower().Split(' ').Where(word => word.Length > 3));
        keywords.UnionWith(this.authors.SafeNull().SelectMany(a => a.Name.ToLower().Split(' ').Where(word => word.Length > 3)));
        //newKeywords.UnionWith(this.categories.SafeNull().SelectMany(c => c.Name.ToLower().Split(' ').Where(word => word.Length > 3)));
        keywords.UnionWith(this.tags.SafeNull().Select(t => t.Name.ToLower()));
        keywords.UnionWith(this.chapters.SafeNull().SelectMany(c => c.Title.ToLower().Split(' ').Where(word => word.Length > 3)));

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
                this.keywords.Add(
                    new BookKeyword { BookId = this.Id, Text = keyword });
            }
        }
    }
}