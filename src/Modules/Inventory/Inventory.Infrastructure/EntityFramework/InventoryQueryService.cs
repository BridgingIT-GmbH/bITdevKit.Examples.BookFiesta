// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Infrastructure;

using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Application;
using BridgingIT.DevKit.Examples.BookFiesta.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class InventoryQueryService(IGenericRepository<Stock> stockRepository, InventoryDbContext dbContext)
    : IInventoryQueryService
{
    /// <summary>
    ///     Retrieves the top stocks based on total movement quantity within a specified time period.
    /// </summary>
    /// <param name="start">The start of the time period.</param>
    /// <param name="end">The end of the time period.</param>
    /// <param name="limit">The maximum number of stocks to retrieve (default is 5).</param>
    public async Task<Result<IEnumerable<Stock>>> StockFindTopAsync(DateTimeOffset start, DateTimeOffset end, int limit = 5)
    {
        var topStockIds = await dbContext.Stocks
            .SelectMany(s => s.Movements)
            .Where(m => m.Timestamp >= start && m.Timestamp <= end)
            .GroupBy(m => m.StockId)
            .Select(g => new
            {
                StockId = g.Key,
                TotalMovement = Math.Abs(g.Sum(m => m.Quantity))
            })
            .OrderByDescending(x => x.TotalMovement)
            .Take(limit)
            .Select(x => x.StockId)
            .ToListAsync();

        var topStocks = await dbContext.Stocks
            .Where(s => topStockIds.Contains(s.Id))
            .Include(s => s.Movements.Where(m => m.Timestamp >= start && m.Timestamp <= end))
            .ToListAsync();

        // Order the results to match the order of topStockIds
        var orderedTopStocks = topStockIds
            .Select(id => topStocks.First(s => s.Id == id))
            .ToList();

        var result = new List<Stock>();
        foreach (var stock in orderedTopStocks)
        {
            result.Add(await stockRepository.FindOneAsync(stock.Id));
        }

        return Result<IEnumerable<Stock>>.Success(result);
    }
}