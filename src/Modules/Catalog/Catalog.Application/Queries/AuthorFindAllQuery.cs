﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class AuthorFindAllQuery(string tenantId) : QueryRequestBase<Result<IEnumerable<Author>>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public class Validator : AbstractValidator<AuthorFindAllQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must be valid and not be empty.");
        }
    }
}