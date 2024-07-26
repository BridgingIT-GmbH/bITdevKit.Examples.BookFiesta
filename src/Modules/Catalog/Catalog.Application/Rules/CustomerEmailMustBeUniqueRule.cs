﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Application;

using System.Threading.Tasks;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

public class CustomerEmailMustBeUniqueRule(
    IGenericRepository<Customer> repository,
    Customer customer) : IBusinessRule
{
    public string Message => "Customer email should be unique";

    public async Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default)
    {
        return !(await repository.FindAllAsync(
            CustomerSpecifications.ForEmail(customer.Email), cancellationToken: cancellationToken)).SafeAny();
    }
}

public static partial class CustomerRules
{
    public static IBusinessRule EmailMustBeUnique(
        IGenericRepository<Customer> repository,
        Customer customer) => new CustomerEmailMustBeUniqueRule(repository, customer);
}