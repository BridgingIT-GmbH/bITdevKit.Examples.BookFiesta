// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.GettingStarted.Application;

using System.Collections.Generic;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Queries;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Domain.Specifications;
using BridgingIT.DevKit.Examples.GettingStarted.Domain;
using Microsoft.Extensions.Logging;

public class CategoryFindAllQueryHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Category> repository)
        : QueryHandlerBase<CategoryFindAllQuery, Result<IEnumerable<Category>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Category>>>> Process(
        CategoryFindAllQuery query, CancellationToken cancellationToken)
    {
        var categories = await repository.FindAllResultAsync(cancellationToken: cancellationToken).AnyContext();
        this.PrintCategories(categories.Value.SafeNull().Where(c => c.Parent == null).OrderBy(e => e.Order));

        return QueryResponse.Success(categories.Value.SafeNull()
            .Where(c => c.Parent == null).OrderBy(e => e.Order).AsEnumerable());
    }

    private void PrintCategories(IEnumerable<Category> categories, int level = 0)
    {
        foreach (var category in categories)
        {
            Console.WriteLine($"{new string(' ', level * 4)}[{category.Order}] {category.Title}");

            if (category.Children.SafeAny())
            {
                this.PrintCategories(category.Children.OrderBy(e => e.Title), level + 1);
            }
        }
    }
}