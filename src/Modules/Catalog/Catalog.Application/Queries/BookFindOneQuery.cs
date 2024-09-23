﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookFindOneQuery(
    string tenantId,
    string bookId) : QueryRequestBase<Result<Book>>,
    ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string BookId { get; } = bookId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<BookFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.BookId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
        }
    }
}