// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Catalog.Application;

using BridgingIT.DevKit.Examples.BookFiesta.SharedKernel.Application;

public class CustomerModel
{
    public string Id { get; set; }

    public string TenantId { get; set; }

    public PersonFormalNameModel PersonName { get; set; }

    public AddressModel Address { get; set; }

    public string Email { get; set; }

    public string Version { get; set; }
}