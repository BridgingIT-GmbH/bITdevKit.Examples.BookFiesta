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
        // Author aggregate mappings
        config.ForType<Author, AuthorModel>();

        config.ForType<AuthorBook, AuthorBookModel>()
            .Map(d => d.Id, s => s.BookId.Value);

        // Book aggregate mappings
        config.ForType<Book, BookModel>()
            .Map(d => d.Sku, s => s.Sku.Value)
            .Map(d => d.Keywords, s => s.Keywords.Select(e => e.Text).ToList())
            .Map(d => d.Isbn, s => s.Isbn.Value);

        config.ForType<BookAuthor, BookAuthorModel>()
            .Map(d => d.Id, s => s.AuthorId.Value);

        config.ForType<BookPublisher, BookPublisherModel>()
            .Map(d => d.Id, s => s.PublisherId.Value);

        // Customer aggregate mappings
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