using InventoryManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers;

[Authorize]
public class StockHistoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public StockHistoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? type, string? search)
    {
        var history = new List<StockHistoryItem>();

        // =========================
        // LỊCH SỬ NHẬP KHO
        // =========================
        if (string.IsNullOrEmpty(type) || type == "import")
        {
            var imports = await _context.StockImportDetails
                .Include(x => x.StockImport)
                    .ThenInclude(x => x!.Supplier)
                .Include(x => x.Product)
                .ToListAsync();

            history.AddRange(imports.Select(x => new StockHistoryItem
            {
                Date = x.StockImport!.ImportDate,
                Code = x.StockImport.Code,
                Type = "Nhập kho",
                ProductCode = x.Product?.Code ?? "",
                ProductName = x.Product?.Name ?? "",
                Quantity = x.Quantity,
                Unit = x.Product?.Unit ?? "",
                UnitPrice = x.UnitPrice,
                Partner = x.StockImport.Supplier?.Name ?? "-",
                CreatedBy = x.StockImport.CreatedBy ?? "-"
            }));
        }

        // =========================
        // LỊCH SỬ XUẤT KHO
        // =========================
        if (string.IsNullOrEmpty(type) || type == "export")
        {
            var exports = await _context.StockExportDetails
                .Include(x => x.StockExport)
                .Include(x => x.Product)
                .ToListAsync();

            history.AddRange(exports.Select(x => new StockHistoryItem
            {
                Date = x.StockExport!.ExportDate,
                Code = x.StockExport.Code,
                Type = "Xuất kho",
                ProductCode = x.Product?.Code ?? "",
                ProductName = x.Product?.Name ?? "",
                Quantity = x.Quantity,
                Unit = x.Product?.Unit ?? "",
                UnitPrice = x.UnitPrice,
                Partner = x.StockExport.Receiver ?? "-",
                CreatedBy = x.StockExport.CreatedBy ?? "-"
            }));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            history = history
                .Where(x =>
                    x.Code.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.ProductCode.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    x.ProductName.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        history = history
            .OrderByDescending(x => x.Date)
            .ToList();

        ViewBag.Type = type;
        ViewBag.Search = search;

        return View(history);
    }
}

public class StockHistoryItem
{
    public DateTime Date { get; set; }

    public string Code { get; set; } = "";

    public string Type { get; set; } = "";

    public string ProductCode { get; set; } = "";

    public string ProductName { get; set; } = "";

    public int Quantity { get; set; }

    public string Unit { get; set; } = "";

    public decimal UnitPrice { get; set; }

    public string Partner { get; set; } = "";

    public string CreatedBy { get; set; } = "";

    public decimal Amount => Quantity * UnitPrice;
}
