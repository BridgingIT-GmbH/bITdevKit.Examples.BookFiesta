// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Presentation;

using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using Mapster;

public class CatalogMapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //config.ForType<Customer, CustomerModel>()
        //    .Map(d => d.Email, s => s.Email.Value);

        //config.ForType<Category, CategoryModel>()
        //    .Map(d => d.ParentId, s => s.Parent.Id.Value.ToString(), e => e.Parent != null);

        //config.ForType<CustomerModel, CustomerCreateCommand>()
        //    .Map(d => d.AddressName, s => s.Address.Name)
        //    .Map(d => d.AddressLine1, s => s.Address.Line1)
        //    .Map(d => d.AddressLine2, s => s.Address.Line2)
        //    .Map(d => d.AddressPostalCode, s => s.Address.PostalCode)
        //    .Map(d => d.AddressCity, s => s.Address.City)
        //    .Map(d => d.AddressCountry, s => s.Address.Country);

        //config.ForType<CustomerModel, CustomerUpdateCommand>()
        //    .Map(d => d.AddressName, s => s.Address.Name)
        //    .Map(d => d.AddressLine1, s => s.Address.Line1)
        //    .Map(d => d.AddressLine2, s => s.Address.Line2)
        //    .Map(d => d.AddressPostalCode, s => s.Address.PostalCode)
        //    .Map(d => d.AddressCity, s => s.Address.City)
        //    .Map(d => d.AddressCountry, s => s.Address.Country);
    }
}