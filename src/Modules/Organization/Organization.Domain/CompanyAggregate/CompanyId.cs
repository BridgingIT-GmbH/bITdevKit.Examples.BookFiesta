//// MIT-License
//// Copyright BridgingIT GmbH - All Rights Reserved
//// Use of this source code is governed by an MIT-style license that can be
//// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

//namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;

//public class CompanyId : AggregateRootId<Guid>
//{
//    private CompanyId()
//    {
//    }

//    private CompanyId(Guid guid)
//    {
//        this.Value = guid;
//    }

//    public override Guid Value { get; protected set; }

//    public bool IsEmpty => this.Value == Guid.Empty;

//    public static implicit operator Guid(CompanyId id) => id?.Value ?? default; // allows a CompanyId value to be implicitly converted to a Guid.
//    public static implicit operator string(CompanyId id) => id?.Value.ToString(); // allows a CompanyId value to be implicitly converted to a string.
//    public static implicit operator CompanyId(Guid id) => id; // allows a Guid value to be implicitly converted to a CompanyId object.

//    public static CompanyId Create()
//    {
//        return new CompanyId(Guid.NewGuid());
//    }

//    public static CompanyId Create(Guid id)
//    {
//        return new CompanyId(id);
//    }

//    public static CompanyId Create(string id)
//    {
//        if (string.IsNullOrWhiteSpace(id))
//        {
//            throw new ArgumentException("Id cannot be null or whitespace.");
//        }

//        return new CompanyId(Guid.Parse(id));
//    }

//    protected override IEnumerable<object> GetAtomicValues()
//    {
//        yield return this.Value;
//    }
//}
