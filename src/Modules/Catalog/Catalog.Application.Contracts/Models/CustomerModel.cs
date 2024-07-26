// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Application;

public class CustomerModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public AddressModel Address { get; set; }

    public string Email { get; set; }

    public string Version { get; set; }
}

public class AddressModel
{
    public string Line1 { get; set; }

    public string Line2 { get; set; }

    public string PostalCode { get; set; }

    public string City { get; set; }

    public string Country { get; set; }
}