//// MIT-License
//// Copyright BridgingIT GmbH - All Rights Reserved
//// Use of this source code is governed by an MIT-style license that can be
//// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

//namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Domain;

//public class CustomerId : AggregateRootId<Guid>
//{
//    private CustomerId()
//    {
//    }

//    private CustomerId(Guid guid)
//    {
//        this.Value = guid;
//    }

//    public override Guid Value { get; protected set; }

//    public bool IsEmpty => this.Value == Guid.Empty;

//    //public static implicit operator Guid(CustomerId id) => id?.Value ?? default; // allows a CustomerId value to be implicitly converted to a Guid.
//    //public static implicit operator CustomerId(Guid id) => id; // allows a Guid value to be implicitly converted to a CustomerId object.

//    public static CustomerId Create()
//    {
//        return new CustomerId(Guid.NewGuid());
//    }

//    public static CustomerId Create(Guid id)
//    {
//        return new CustomerId(id);
//    }

//    public static CustomerId Create(string id)
//    {
//        if (string.IsNullOrWhiteSpace(id))
//        {
//            throw new ArgumentException("Id cannot be null or whitespace.");
//        }

//        return new CustomerId(Guid.Parse(id));
//    }

//    protected override IEnumerable<object> GetAtomicValues()
//    {
//        yield return this.Value;
//    }
//}