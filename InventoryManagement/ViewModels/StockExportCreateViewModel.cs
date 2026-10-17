using InventoryManagement.Models;

namespace InventoryManagement.ViewModels;

public class StockExportCreateViewModel
{
    public string? Receiver { get; set; }

    public string? Note { get; set; }

    public List<StockExportItemViewModel> Items { get; set; } = new();

    public List<Product> Products { get; set; } = new();
}

public class StockExportItemViewModel
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
