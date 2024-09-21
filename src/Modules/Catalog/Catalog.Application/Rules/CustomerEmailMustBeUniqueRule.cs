// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using Common;
using DevKit.Domain;
using DevKit.Domain.Repositories;
using Domain;

public class CustomerEmailMustBeUniqueRule(IGenericRepository<Customer> repository, Customer customer) : DomainRuleBase
{
    public override string Message
        => "Customer email should be unique";

    public override async Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(CustomerSpecifications.ForEmail(customer.TenantId, customer.Email), cancellationToken: cancellationToken)).SafeAny();
    }
}

public static class CustomerRules
{
    public static IDomainRule EmailMustBeUnique(IGenericRepository<Customer> repository, Customer customer)
    {
        return new CustomerEmailMustBeUniqueRule(repository, customer);
    }
}