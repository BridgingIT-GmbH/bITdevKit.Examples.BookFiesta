// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

[DebuggerDisplay("AuthorId={AuthorId}, Name={Name}")]
public class BookAuthor : ValueObject
{
    private BookAuthor() { }

#pragma warning disable SA1202 // Elements should be ordered by access
    public BookAuthor(AuthorId authorId, PersonFormalName name, int position)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        this.AuthorId = authorId;
        this.Name = name;
        this.Position = position;
    }

    public AuthorId AuthorId { get; private set; }

    public string Name { get; private set; }

    public int Position { get; private set; }

    public static BookAuthor Create(Author author, int position)
    {
        _ = author ?? throw new DomainRuleException("BookAuthor Author cannot be empty.");

        return new BookAuthor(author.Id, author.PersonName, position);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return this.AuthorId;
    }
}