using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models;

public class StockExportDetail
{
    public int Id { get; set; }

    public int StockExportId { get; set; }

    public StockExport? StockExport { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal Amount => Quantity * UnitPrice;
}
