using System.Diagnostics;
using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Tổng số sản phẩm đang hoạt động
        ViewBag.TotalProducts = await _context.Products
            .CountAsync(p => p.IsActive);

        // Tổng số lượng hàng đang tồn trong kho
        ViewBag.TotalStock = await _context.Products
            .Where(p => p.IsActive)
            .SumAsync(p => (int?)p.Quantity) ?? 0;

        // Tổng nhà cung cấp đang hoạt động
        ViewBag.TotalSuppliers = await _context.Suppliers
            .CountAsync(s => s.IsActive);

        // Sản phẩm có số lượng <= mức tồn kho tối thiểu
        ViewBag.LowStockCount = await _context.Products
            .CountAsync(p =>
                p.IsActive &&
                p.Quantity <= p.MinimumStock);

        // Danh sách tối đa 5 sản phẩm sắp hết hàng
        ViewBag.LowStockProducts = await _context.Products
            .Include(p => p.Category)
            .Where(p =>
                p.IsActive &&
                p.Quantity <= p.MinimumStock)
            .OrderBy(p => p.Quantity)
            .Take(5)
            .ToListAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
        });
    }
}
