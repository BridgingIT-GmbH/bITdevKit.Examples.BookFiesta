// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using System.Collections.Generic;
using BridgingIT.DevKit.Application.Queries;
using Common;
using Domain;
using FluentValidation;

public class CategoryFindAllQuery(string tenantId) : QueryRequestBase<Result<IEnumerable<Category>>>
{
    public string TenantId { get; } = tenantId;

    public bool Flatten { get; set; } = true;

    public class Validator : AbstractValidator<CategoryFindAllQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId)
                .MustNotBeDefaultOrEmptyGuid()
                .WithMessage("Must be valid and not be empty.");
        }
    }
}