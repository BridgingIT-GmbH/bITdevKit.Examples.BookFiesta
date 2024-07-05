// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using System;
using BridgingIT.DevKit.Domain.Model;
using static BridgingIT.DevKit.Examples.BookStore.Catalog.Domain.CoreSeedModels;

public class Author : AuditableAggregateRoot<AuthorId/*, Guid*/>, IConcurrent
{
    private readonly List<AuthorBook> books = [];
    private readonly List<Tag> tags = [];

    private Author() { } // Private constructor required by EF Core

    private Author(PersonFormalName name, string biography)
    {
        this.SetName(name);
        this.SetBiography(biography);
    }

    public PersonFormalName PersonName { get; private set; }

    public string Biography { get; private set; }

    public IEnumerable<AuthorBook> Books => this.books;

    public IEnumerable<Tag> Tags => this.tags.OrderBy(e => e.Name);

    public Guid Version { get; set; }

    public static Author Create(PersonFormalName name, string biography = null)
    {
        return new Author(name, biography);
    }

    public Author SetName(PersonFormalName name)
    {
        this.PersonName = name;

        return this;
    }

    public Author SetBiography(string biography)
    {
        // Validate biography
        this.Biography = biography;

        return this;
    }

    public Author AssignBook(Book book)
    {
        if (!this.books.Any(e => e.BookId == book.Id))
        {
            this.books.Add(AuthorBook.Create(book));

            this.DomainEvents.Register(
                new AuthorBookAssignedDomainEvent(this, book));
        }

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
        if (!this.tags.Contains(tag))
        {
            this.tags.Add(tag);
        }

        return this;
    }

    public Author RemoveTag(TagId tagId)
    {
        this.tags.RemoveAll(t => t.Id == tagId);

        return this;
    }
}
