using InventoryManagement.Data;
using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers;

[Authorize]
public class StockImportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public StockImportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Danh sách phiếu nhập
    public async Task<IActionResult> Index()
    {
        var imports = await _context.StockImports
            .Include(x => x.Supplier)
            .Include(x => x.Details)
            .OrderByDescending(x => x.ImportDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        return View(imports);
    }

    // Chi tiết phiếu nhập
    public async Task<IActionResult> Details(int id)
    {
        var stockImport = await _context.StockImports
            .Include(x => x.Supplier)
            .Include(x => x.Details)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (stockImport == null)
        {
            return NotFound();
        }

        return View(stockImport);
    }

    // Form nhập kho
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new StockImportCreateViewModel();

        await LoadFormData(model);

        return View(model);
    }

    // Lưu phiếu nhập
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        StockImportCreateViewModel model)
    {
        // Loại bỏ các dòng trống
        model.Items = model.Items
            .Where(x => x.ProductId > 0)
            .ToList();

        if (!model.Items.Any())
        {
            ModelState.AddModelError(
                "",
                "Phiếu nhập phải có ít nhất một sản phẩm.");
        }

        if (model.Items.Any(x => x.Quantity <= 0))
        {
            ModelState.AddModelError(
                "",
                "Số lượng nhập phải lớn hơn 0.");
        }

        if (model.Items.Any(x => x.UnitPrice < 0))
        {
            ModelState.AddModelError(
                "",
                "Giá nhập không được nhỏ hơn 0.");
        }

        var duplicateProduct = model.Items
            .GroupBy(x => x.ProductId)
            .Any(g => g.Count() > 1);

        if (duplicateProduct)
        {
            ModelState.AddModelError(
                "",
                "Một sản phẩm không được xuất hiện nhiều lần trong cùng phiếu.");
        }

        var supplierExists = await _context.Suppliers
            .AnyAsync(x =>
                x.Id == model.SupplierId &&
                x.IsActive);

        if (!supplierExists)
        {
            ModelState.AddModelError(
                "SupplierId",
                "Nhà cung cấp không hợp lệ.");
        }

        if (!ModelState.IsValid)
        {
            await LoadFormData(model);
            return View(model);
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Tạo mã phiếu tự động
            var code = await GenerateImportCode();

            var stockImport = new StockImport
            {
                Code = code,
                SupplierId = model.SupplierId,
                ImportDate = DateTime.Now,
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
                        "Có sản phẩm không tồn tại hoặc đã ngừng sử dụng.");
                }

                // Cộng tồn kho
                product.Quantity += item.Quantity;

                // Cập nhật giá nhập gần nhất
                product.ImportPrice = item.UnitPrice;

                var detail = new StockImportDetail
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };

                stockImport.Details.Add(detail);

                totalAmount +=
                    item.Quantity * item.UnitPrice;
            }

            stockImport.TotalAmount = totalAmount;

            _context.StockImports.Add(stockImport);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Success"] =
                $"Nhập kho thành công. Mã phiếu: {stockImport.Code}";

            return RedirectToAction(
                nameof(Details),
                new { id = stockImport.Id });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                "",
                "Không thể nhập kho: " + ex.Message);

            await LoadFormData(model);

            return View(model);
        }
    }

    private async Task LoadFormData(
        StockImportCreateViewModel model)
    {
        model.Suppliers = await _context.Suppliers
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

        model.Products = await _context.Products
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    private async Task<string> GenerateImportCode()
    {
        var today = DateTime.Now.ToString("yyyyMMdd");

        var countToday = await _context.StockImports
            .CountAsync(x =>
                x.ImportDate.Date == DateTime.Today);

        return $"PN-{today}-{countToday + 1:000}";
    }
}
