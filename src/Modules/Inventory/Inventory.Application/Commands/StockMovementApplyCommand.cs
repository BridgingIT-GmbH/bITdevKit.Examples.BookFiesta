// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockMovementApplyCommand(string tenantId, string stockId, StockMovementModel model)
    : CommandRequestBase<Result<Stock>>,
    ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string StockId { get; } = stockId;

    public StockMovementModel Model { get; } = model;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<StockMovementApplyCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.Model).SetValidator(new ModelValidator());
        }

        private class ModelValidator : AbstractValidator<StockMovementModel>
        {
            public ModelValidator()
            {
                this.RuleFor(m => m).NotNull().NotEmpty().WithMessage("Must not be empty.");
            }
        }
    }
}