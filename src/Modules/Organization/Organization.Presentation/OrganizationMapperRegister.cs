// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Organization.Presentation;

using BridgingIT.DevKit.Examples.BookFiesta.Organization.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Organization.Domain;
using Mapster;

public class OrganizationMapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<Company, CompanyModel>()
            .Map(d => d.ContactEmail, s => s.ContactEmail.Value);
    }
}