// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Organization.Application;

using SharedKernel.Application;

public class CompanyModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public AddressModel Address { get; set; }

    public string RegistrationNumber { get; set; }

    public string ContactEmail { get; set; }

    public string ContactPhone { get; set; }

    public string Website { get; set; }

    public string VatNumber { get; set; }
}