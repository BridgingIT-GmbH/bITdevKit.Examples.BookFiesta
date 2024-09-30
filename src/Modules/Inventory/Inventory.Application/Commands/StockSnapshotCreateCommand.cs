// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;

public class StockSnapshotCreateCommand(string tenantId, string stockId)
    : CommandRequestBase<Result<StockSnapshot>>, ITenantAware
{
    public string TenantId { get; } = tenantId;

    public string StockId { get; } = stockId;

    public override ValidationResult Validate()
    {
        return new Validator().Validate(this);
    }

    public class Validator : AbstractValidator<StockSnapshotCreateCommand>
    {
        public Validator()
        {
            this.RuleFor(c => c.TenantId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
            this.RuleFor(c => c.StockId).MustNotBeDefaultOrEmptyGuid().WithMessage("Must not be empty or invalid.");
        }
    }
}