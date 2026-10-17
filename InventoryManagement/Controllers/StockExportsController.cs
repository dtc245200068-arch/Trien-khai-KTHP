using InventoryManagement.Data;
using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers;

[Authorize]
public class StockExportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public StockExportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var exports = await _context.StockExports
            .Include(x => x.Details)
            .OrderByDescending(x => x.ExportDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        return View(exports);
    }

    public async Task<IActionResult> Details(int id)
    {
        var stockExport = await _context.StockExports
            .Include(x => x.Details)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (stockExport == null)
        {
            return NotFound();
        }

        return View(stockExport);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new StockExportCreateViewModel();

        await LoadProducts(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        StockExportCreateViewModel model)
    {
        model.Items = model.Items
            .Where(x => x.ProductId > 0)
            .ToList();

        if (!model.Items.Any())
        {
            ModelState.AddModelError(
                "",
                "Phiếu xuất phải có ít nhất một sản phẩm.");
        }

        if (model.Items.Any(x => x.Quantity <= 0))
        {
            ModelState.AddModelError(
                "",
                "Số lượng xuất phải lớn hơn 0.");
        }

        if (model.Items.Any(x => x.UnitPrice < 0))
        {
            ModelState.AddModelError(
                "",
                "Giá xuất không được nhỏ hơn 0.");
        }

        var duplicateProduct = model.Items
            .GroupBy(x => x.ProductId)
            .Any(x => x.Count() > 1);

        if (duplicateProduct)
        {
            ModelState.AddModelError(
                "",
                "Một sản phẩm không được xuất hiện nhiều lần trong cùng phiếu.");
        }

        if (!ModelState.IsValid)
        {
            await LoadProducts(model);
            return View(model);
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var stockExport = new StockExport
            {
                Code = await GenerateExportCode(),
                ExportDate = DateTime.Now,
                Receiver = model.Receiver,
                Note = model.Note,
                CreatedBy = User.Identity?.Name ?? "Unknown",
                CreatedAt = DateTime.Now
            };

            decimal totalAmount = 0;

            foreach (var item in model.Items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.Id == item.ProductId &&
                        x.IsActive);

                if (product == null)
                {
                    throw new Exception(
                        "Sản phẩm không tồn tại hoặc đã ngừng sử dụng.");
                }

                // QUAN TRỌNG:
                // Không cho xuất quá số lượng tồn.
                if (product.Quantity < item.Quantity)
                {
                    throw new Exception(
                        $"Sản phẩm {product.Name} chỉ còn " +
                        $"{product.Quantity} {product.Unit} trong kho.");
                }

                // Trừ tồn kho
                product.Quantity -= item.Quantity;

                // Cập nhật giá xuất gần nhất
                product.ExportPrice = item.UnitPrice;

                stockExport.Details.Add(
                    new StockExportDetail
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });

                totalAmount +=
                    item.Quantity * item.UnitPrice;
            }

            stockExport.TotalAmount = totalAmount;

            _context.StockExports.Add(stockExport);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Success"] =
                $"Xuất kho thành công. Mã phiếu: {stockExport.Code}";

            return RedirectToAction(
                nameof(Details),
                new { id = stockExport.Id });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                "",
                "Không thể xuất kho: " + ex.Message);

            await LoadProducts(model);

            return View(model);
        }
    }

    private async Task LoadProducts(
        StockExportCreateViewModel model)
    {
        model.Products = await _context.Products
            .Where(x =>
                x.IsActive &&
                x.Quantity > 0)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    private async Task<string> GenerateExportCode()
    {
        var today = DateTime.Now.ToString("yyyyMMdd");

        var countToday = await _context.StockExports
            .CountAsync(x =>
                x.ExportDate.Date == DateTime.Today);

        return $"PX-{today}-{countToday + 1:000}";
    }
}
