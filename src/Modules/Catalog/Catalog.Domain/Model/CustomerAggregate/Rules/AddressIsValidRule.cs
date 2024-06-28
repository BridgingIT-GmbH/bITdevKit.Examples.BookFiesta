// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookStore.Catalog.Domain;

using BridgingIT.DevKit.Domain;

#pragma warning disable CS9113 // Parameter is unread.
public class AddressIsValidRule(Address address) : IBusinessRule
#pragma warning restore CS9113 // Parameter is unread.
{
    public string Message => "Not a valid address";

    public Task<bool> IsSatisfiedAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(true); // TODO: Implement validation
}

public static partial class AddressRules
{
    public static IBusinessRule IsValid(Address address) => new AddressIsValidRule(address);
}