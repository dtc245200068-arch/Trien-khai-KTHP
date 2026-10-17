using InventoryManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? status)
    {
        var query = _context.Products
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Code.Contains(search) ||
                x.Name.Contains(search));
        }

        if (status == "out")
        {
            query = query.Where(x => x.Quantity <= 0);
        }
        else if (status == "low")
        {
            query = query.Where(x =>
                x.Quantity > 0 &&
                x.Quantity <= x.MinimumStock);
        }
        else if (status == "available")
        {
            query = query.Where(x =>
                x.Quantity > x.MinimumStock);
        }

        var products = await query
            .OrderBy(x => x.Quantity)
            .ThenBy(x => x.Name)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;

        ViewBag.TotalProducts =
            await _context.Products.CountAsync();

        ViewBag.TotalQuantity =
            await _context.Products.SumAsync(x => x.Quantity);

        ViewBag.LowStock =
            await _context.Products.CountAsync(x =>
                x.Quantity > 0 &&
                x.Quantity <= x.MinimumStock);

        ViewBag.OutOfStock =
            await _context.Products.CountAsync(x =>
                x.Quantity <= 0);

        return View(products);
    }
}
