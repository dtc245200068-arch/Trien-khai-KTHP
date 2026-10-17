using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models;

public class StockImportDetail
{
    public int Id { get; set; }

    public int StockImportId { get; set; }

    public StockImport? StockImport { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal Amount => Quantity * UnitPrice;
}
