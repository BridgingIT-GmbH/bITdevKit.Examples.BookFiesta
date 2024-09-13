// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[TypedEntityId<Guid>]
public class Author : AuditableAggregateRoot<AuthorId>, IConcurrent
{
    private readonly List<AuthorBook> books = [];
    private readonly List<Tag> tags = [];

    private Author() { } // Private constructor required by EF Core

    private Author(TenantId tenantId, PersonFormalName name, string biography = null)
    {
        this.TenantId = tenantId;
        this.SetName(name);
        this.SetBiography(biography);
    }

    public TenantId TenantId { get; private set; }

    public PersonFormalName PersonName { get; private set; }

    public string Biography { get; private set; }

    public IEnumerable<AuthorBook> Books => this.books;

    public IEnumerable<Tag> Tags => this.tags.OrderBy(e => e.Name);

    public Guid Version { get; set; }

    public static Author Create(TenantId tenantId, PersonFormalName name, string biography = null)
    {
        _ = tenantId ?? throw new DomainRuleException("TenantId cannot be empty.");

        var author = new Author(tenantId, name, biography);

        author.DomainEvents.Register(
            new AuthorCreatedDomainEvent(tenantId, author));

        return author;
    }

    public Author SetName(PersonFormalName name)
    {
        _ = name ?? throw new DomainRuleException("Author Name cannot be empty.");

        if (this.PersonName == name)
        {
            return this;
        }

        this.PersonName = name;

        if (this.Id?.IsEmpty == false)
        {
            this.DomainEvents.Register(
                new AuthorUpdatedDomainEvent(this.TenantId, this), true);
        }

        return this;
    }

    public Author SetBiography(string biography)
    {
        if (this.Biography == biography)
        {
            return this;
        }

        this.Biography = biography;

        this.DomainEvents.Register(
            new AuthorUpdatedDomainEvent(this.TenantId, this), true);

        return this;
    }

    public Author AssignBook(Book book)
    {
        if (this.books.Any(e => e.BookId == book.Id))
        {
            return this;
        }

        this.books.Add(AuthorBook.Create(book));

        this.DomainEvents.Register(
            new AuthorBookAssignedDomainEvent(this, book));

        return this;
    }

    //public Author RemoveBook(BookId bookId)
    //{
    //    var bookAuthor = this.bookIds.FirstOrDefault(ba => ba.BookId == bookId);
    //    if (bookAuthor != null)
    //    {
    //        this.bookIds.Remove(bookAuthor);
    //        // Reorder remaining books
    //        for (var i = 0; i < this.bookIds.Count; i++)
    //        {
    //            this.bookIds[i] = new BookAuthor(this.bookIds[i].BookId, this.Id.Value, i);
    //        }
    //    }

    //    return this;
    //}

    public Author AddTag(Tag tag)
    {
        if (this.tags.Contains(tag))
        {
            return this;
        }

        DomainRules.Apply([new TagMustBelongToTenantRule(tag, this.TenantId)]);

        this.tags.Add(tag);

        return this;
    }

    public Author RemoveTag(TagId tagId)
    {
        this.tags.RemoveAll(t => t.Id == tagId);

        return this;
    }
}