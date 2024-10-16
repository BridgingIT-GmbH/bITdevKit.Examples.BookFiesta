// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class BookCreateOrUpdateCommand(string tenantId, BookModel model, UpsertOperationType operationType)
    : CommandRequestBase<Result<Book>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public BookModel Model { get; } = model;

    public UpsertOperationType OperationType { get; } = operationType;
    //public UpsertOperationType OperationType { get; } = model.Id.IsNullOrEmpty() ? UpsertOperationType.Create : UpsertOperationType.Update;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<BookCreateOrUpdateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.TenantId).Must((command, tenantId) => tenantId == command.Model.TenantId).WithMessage("Must be equal to Model.TenantId.");
            this.RuleFor(c => c.Model).SetValidator((command, model) => new ModelValidator(command.OperationType));
        }

        private class ModelValidator : AbstractValidator<BookModel>
        {
            public ModelValidator(UpsertOperationType operationType)
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Title).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Language).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Sku).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Isbn).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Publisher).NotNull().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Publisher.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty.");
                this.RuleFor(m => m.PublishedDate).NotNull().NotEmpty().WithMessage("Must not be empty.");
                this.RuleFor(m => m.Authors).NotNull().NotEmpty().WithMessage("Must not be empty.").ForEach(c => c.SetValidator(new BookAuthorValidator()));
                this.RuleFor(m => m.Categories).NotNull().NotEmpty().WithMessage("Must not be empty.").ForEach(c => c.SetValidator(new BookCategoryValidator()));

                this.RuleFor(m => m.Id)
                    .Must((model, id) => operationType == UpsertOperationType.Create ? id.IsNullOrEmpty() : !id.IsNullOrEmpty())
                    .WithMessage(m => m.Id.IsNullOrEmpty()
                        ? "Id must not be empty for update operation."
                        : "Id must be empty for create operation.");
            }
        }

        private class BookAuthorValidator : AbstractValidator<BookAuthorModel>
        {
            public BookAuthorValidator()
            {
                this.RuleFor(c => c.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            }
        }

        private class BookCategoryValidator : AbstractValidator<BookCategoryModel>
        {
            public BookCategoryValidator()
            {
                this.RuleFor(c => c.Id).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            }
        }
    }
}

public enum UpsertOperationType
{
    Create,
    Update
}