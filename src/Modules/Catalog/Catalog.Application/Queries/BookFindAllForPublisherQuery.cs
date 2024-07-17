// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Application;

using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;
using FluentValidation;
using FluentValidation.Results;

public class BookFindAllForPublisherQuery(string publisherId) : QueryRequestBase<Result<IEnumerable<Book>>>
{
    public string PublisherId { get; } = publisherId;

    public override ValidationResult Validate() =>
        new Validator().Validate(this);

    public class Validator : AbstractValidator<BookFindAllForPublisherQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.PublisherId).NotNull().NotEmpty().WithMessage("Must not be empty.");
        }
    }
}