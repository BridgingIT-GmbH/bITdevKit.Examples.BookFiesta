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

public class BookFindAllRelatedQuery(string bookId) : QueryRequestBase<Result<IEnumerable<Book>>>
{
    public string BookId { get; } = bookId;

    public override ValidationResult Validate() =>
        new Validator().Validate(this);

    public class Validator : AbstractValidator<BookFindAllRelatedQuery>
    {
        public Validator()
        {
            this.RuleFor(c => c.BookId).NotNull().NotEmpty().WithMessage("Must not be empty.");
        }
    }
}