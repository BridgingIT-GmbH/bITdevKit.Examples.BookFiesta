//// MIT-License
//// Copyright BridgingIT GmbH - All Rights Reserved
//// Use of this source code is governed by an MIT-style license that can be
//// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

//namespace BridgingIT.DevKit.Examples.BookStore.Infrastructure;
//using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//public class BookAuthorEntityTypeConfiguration : IEntityTypeConfiguration<BookAuthor>
//{
//    public void Configure(EntityTypeBuilder<BookAuthor> builder)
//    {
//        builder.ToTable("BookAuthors");

//        builder.HasKey(ba => new { ba.BookId, ba.AuthorId });

//        builder.Property(e => e.BookId)
//            .ValueGeneratedOnAdd()
//            .HasConversion(
//                id => id.Value,
//                value => BookId.Create(value));

//        builder.Property(e => e.AuthorId)
//            .ValueGeneratedOnAdd()
//            .HasConversion(
//                id => id.Value,
//                value => AuthorId.Create(value));
//    }
//}