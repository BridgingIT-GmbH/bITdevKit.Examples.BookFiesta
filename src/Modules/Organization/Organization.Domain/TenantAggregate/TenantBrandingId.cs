//// MIT-License
//// Copyright BridgingIT GmbH - All Rights Reserved
//// Use of this source code is governed by an MIT-style license that can be
//// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

//namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;

//public class TenantBrandingId : AggregateRootId<Guid>
//{
//    private TenantBrandingId()
//    {
//    }

//    private TenantBrandingId(Guid guid)
//    {
//        this.Value = guid;
//    }

//    public override Guid Value { get; protected set; }

//    public bool IsEmpty => this.Value == Guid.Empty;

//    public static implicit operator Guid(TenantBrandingId id) => id?.Value ?? default; // allows a TenantBrandingId value to be implicitly converted to a Guid.
//    public static implicit operator string(TenantBrandingId id) => id?.Value.ToString(); // allows a TenantBrandingId value to be implicitly converted to a string.
//    public static implicit operator TenantBrandingId(Guid id) => id; // allows a Guid value to be implicitly converted to a TenantBrandingId object.

//    public static TenantBrandingId Create()
//    {
//        return new TenantBrandingId(Guid.NewGuid());
//    }

//    public static TenantBrandingId Create(Guid id)
//    {
//        return new TenantBrandingId(id);
//    }

//    public static TenantBrandingId Create(string id)
//    {
//        if (string.IsNullOrWhiteSpace(id))
//        {
//            throw new ArgumentException("Id cannot be null or whitespace.");
//        }

//        return new TenantBrandingId(Guid.Parse(id));
//    }

//    protected override IEnumerable<object> GetAtomicValues()
//    {
//        yield return this.Value;
//    }
//}
