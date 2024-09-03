//// MIT-License
//// Copyright BridgingIT GmbH - All Rights Reserved
//// Use of this source code is governed by an MIT-style license that can be
//// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

//namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;

//public class BookKeywordId : EntityId<Guid>
//{
//    private BookKeywordId()
//    {
//    }

//    private BookKeywordId(Guid guid)
//    {
//        this.Value = guid;
//    }

//    public override Guid Value { get; protected set; }

//    public bool IsEmpty => this.Value == Guid.Empty;

//    //public static implicit operator Guid(BookKeywordId id) => id?.Value ?? default; // allows a BookKeywordId value to be implicitly converted to a Guid.
//    //public static implicit operator BookKeywordId(Guid value) => value; // allows a Guid value to be implicitly converted to a BookKeywordId object.

//    public static BookKeywordId Create()
//    {
//        return new BookKeywordId(Guid.NewGuid());
//    }

//    public static BookKeywordId Create(Guid value)
//    {
//        return new BookKeywordId(value);
//    }

//    public static BookKeywordId Create(string value)
//    {
//        return new BookKeywordId(Guid.Parse(value));
//    }

//    protected override IEnumerable<object> GetAtomicValues()
//    {
//        yield return this.Value;
//    }
//}
