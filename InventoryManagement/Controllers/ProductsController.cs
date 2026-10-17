using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Danh sách + tìm kiếm + lọc danh mục
    public async Task<IActionResult> Index(
        string? search,
        int? categoryId)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Code.Contains(search) ||
                p.Name.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p =>
                p.CategoryId == categoryId.Value);
        }

        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;

        ViewBag.Categories = new SelectList(
            await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync(),
            "Id",
            "Name",
            categoryId
        );

        var products = await query
            .OrderByDescending(p => p.IsActive)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return View(products);
    }

    // Form thêm sản phẩm
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadCategories();
        return View();
    }

    // Thêm sản phẩm
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        var codeExists = await _context.Products
            .AnyAsync(p => p.Code == product.Code);

        if (codeExists)
        {
            ModelState.AddModelError(
                "Code",
                "Mã sản phẩm đã tồn tại.");
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c =>
                c.Id == product.CategoryId &&
                c.IsActive);

        if (!categoryExists)
        {
            ModelState.AddModelError(
                "CategoryId",
                "Danh mục không hợp lệ.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategories(product.CategoryId);
            return View(product);
        }

        product.CreatedAt = DateTime.Now;
        product.IsActive = true;

        // Khi tạo sản phẩm mới chưa có nghiệp vụ nhập kho
        product.Quantity = 0;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Thêm sản phẩm thành công.";

        return RedirectToAction(nameof(Index));
    }

    // Form sửa
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products
            .FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        await LoadCategories(product.CategoryId);

        return View(product);
    }

    // Cập nhật sản phẩm
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        var codeExists = await _context.Products
            .AnyAsync(p =>
                p.Code == product.Code &&
                p.Id != id);

        if (codeExists)
        {
            ModelState.AddModelError(
                "Code",
                "Mã sản phẩm đã tồn tại.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategories(product.CategoryId);
            return View(product);
        }

        var existing = await _context.Products
            .FindAsync(id);

        if (existing == null)
        {
            return NotFound();
        }

        existing.Code = product.Code;
        existing.Name = product.Name;
        existing.Unit = product.Unit;
        existing.ImportPrice = product.ImportPrice;
        existing.ExportPrice = product.ExportPrice;
        existing.MinimumStock = product.MinimumStock;
        existing.CategoryId = product.CategoryId;

        // Không sửa Quantity ở đây.
        // Tồn kho chỉ thay đổi qua nhập/xuất kho.

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Cập nhật sản phẩm thành công.";

        return RedirectToAction(nameof(Index));
    }

    // Kích hoạt / ngừng sử dụng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var product = await _context.Products
            .FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        product.IsActive = !product.IsActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = product.IsActive
            ? "Đã kích hoạt sản phẩm."
            : "Đã ngừng sử dụng sản phẩm.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategories(
        int? selectedId = null)
    {
        ViewBag.Categories = new SelectList(
            await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync(),
            "Id",
            "Name",
            selectedId
        );
    }
}
