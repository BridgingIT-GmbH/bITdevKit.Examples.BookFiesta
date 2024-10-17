// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Catalog.Application;

public class CategoryFindAllQueryHandler(ILoggerFactory loggerFactory, IGenericRepository<Category> repository)
    : QueryHandlerBase<CategoryFindAllQuery, Result<IEnumerable<Category>>>(loggerFactory)
{
    public override async Task<QueryResponse<Result<IEnumerable<Category>>>> Process(
        CategoryFindAllQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = TenantId.Create(query.TenantId);
        var categories = await repository.FindAllResultAsync(
            [new Specification<Category>(e => e.TenantId == tenantId)],
            cancellationToken: cancellationToken).AnyContext();

        this.PrintCategories(categories.Value.SafeNull().Where(c => c.Parent == null).OrderBy(e => e.Order));

        if (query.Flatten)
        {
            categories.Value = this.FlattenCategories(categories.Value);

            return QueryResponse.Success(categories.Value.SafeNull().AsEnumerable());
        }

        return QueryResponse.Success(
            categories.Value.SafeNull().Where(c => c.Parent == null).OrderBy(e => e.Order).AsEnumerable());
    }

    private IEnumerable<Category> FlattenCategories(IEnumerable<Category> categories)
    {
        return categories.SafeAny()
            ? categories.SelectMany(
                    c => new[]
                    {
                        c
                    }.Concat(c.Children))
                .ToList()
                .DistinctBy(c => c.Id)
            : [];
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