// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;
using FluentValidation;
using FluentValidation.Results;

public class PublisherFindOneQuery(string tenantId, string bookId) : QueryRequestBase<Result<Publisher>>
{
    public string TenantId { get; } = tenantId;

    public string PublisherId { get; } = bookId;

    public override ValidationResult Validate() =>
        new Validator().Validate(this);

    public class Validator : AbstractValidator<PublisherFindOneQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
            this.RuleFor(c => c.PublisherId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}
