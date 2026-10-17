using System.ComponentModel.DataAnnotations;
using InventoryManagement.Models;

namespace InventoryManagement.ViewModels;

public class StockImportCreateViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn nhà cung cấp.")]
    public int SupplierId { get; set; }

    public string? Note { get; set; }

    public List<StockImportItemViewModel> Items { get; set; } = new();

    public List<Supplier> Suppliers { get; set; } = new();

    public List<Product> Products { get; set; } = new();
}

public class StockImportItemViewModel
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
