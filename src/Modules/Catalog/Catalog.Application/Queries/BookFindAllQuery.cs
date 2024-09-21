// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Common;
using DevKit.Application.Queries;
using Domain;
using FluentValidation;

public class BookFindAllQuery(string tenantId) : QueryRequestBase<Result<IEnumerable<Book>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public class Validator : AbstractValidator<BookFindAllQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
        }
    }
}