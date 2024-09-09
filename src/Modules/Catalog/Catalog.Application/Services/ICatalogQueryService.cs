// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

using System.Collections.Generic;
using System.Threading.Tasks;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Domain;

public interface ICatalogQueryService
{
    /// <summary>
    /// Retrieves a collection of related books based on the provided book and limit.
    /// </summary>
    /// <param name="book">The book to find related books for.</param>
    /// <param name="limit">The maximum number of related books to retrieve (default is 5).</param>
    Task<Result<IEnumerable<Book>>> BookFindAllRelatedAsync(Book book, int limit = 5);
}