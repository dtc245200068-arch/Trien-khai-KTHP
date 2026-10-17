using InventoryManagement.Data;
using InventoryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _context;

    public SuppliersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Danh sách + tìm kiếm
    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.Name.Contains(search) ||
                (s.Phone != null && s.Phone.Contains(search)) ||
                (s.Email != null && s.Email.Contains(search)));
        }

        ViewBag.Search = search;

        var suppliers = await query
            .OrderByDescending(s => s.IsActive)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return View(suppliers);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        if (!ModelState.IsValid)
        {
            return View(supplier);
        }

        supplier.CreatedAt = DateTime.Now;
        supplier.IsActive = true;

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thêm nhà cung cấp thành công.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
        {
            return NotFound();
        }

        return View(supplier);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier supplier)
    {
        if (id != supplier.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(supplier);
        }

        var existing = await _context.Suppliers.FindAsync(id);

        if (existing == null)
        {
            return NotFound();
        }

        existing.Name = supplier.Name;
        existing.Phone = supplier.Phone;
        existing.Email = supplier.Email;
        existing.Address = supplier.Address;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật nhà cung cấp thành công.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
        {
            return NotFound();
        }

        supplier.IsActive = !supplier.IsActive;

        await _context.SaveChangesAsync();

        TempData["Success"] = supplier.IsActive
            ? "Đã kích hoạt nhà cung cấp."
            : "Đã ngừng sử dụng nhà cung cấp.";

        return RedirectToAction(nameof(Index));
    }
}
