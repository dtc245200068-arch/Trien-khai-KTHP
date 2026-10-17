using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Danh sách danh mục
    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.Name.Contains(search));
        }

        ViewBag.Search = search;

        var categories = await query
            .OrderByDescending(c => c.IsActive)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    // Form thêm
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Thêm danh mục
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        var exists = await _context.Categories
            .AnyAsync(c => c.Name == category.Name);

        if (exists)
        {
            ModelState.AddModelError(
                "Name",
                "Tên danh mục đã tồn tại."
            );

            return View(category);
        }

        category.CreatedAt = DateTime.Now;
        category.IsActive = true;

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thêm danh mục thành công.";

        return RedirectToAction(nameof(Index));
    }

    // Form sửa
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories
            .FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // Cập nhật
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        var exists = await _context.Categories
            .AnyAsync(c =>
                c.Name == category.Name &&
                c.Id != id);

        if (exists)
        {
            ModelState.AddModelError(
                "Name",
                "Tên danh mục đã tồn tại."
            );

            return View(category);
        }

        var existing = await _context.Categories
            .FindAsync(id);

        if (existing == null)
        {
            return NotFound();
        }

        existing.Name = category.Name;
        existing.Description = category.Description;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật danh mục thành công.";

        return RedirectToAction(nameof(Index));
    }

    // Bật / ngừng sử dụng
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var category = await _context.Categories
            .FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        category.IsActive = !category.IsActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = category.IsActive
            ? "Đã kích hoạt danh mục."
            : "Đã ngừng sử dụng danh mục.";

        return RedirectToAction(nameof(Index));
    }
}
