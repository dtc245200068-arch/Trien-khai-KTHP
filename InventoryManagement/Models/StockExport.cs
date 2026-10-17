using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models;

public class StockExport
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string Code { get; set; } = string.Empty;

    public DateTime ExportDate { get; set; } = DateTime.Now;

    [StringLength(200)]
    public string? Receiver { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<StockExportDetail> Details { get; set; }
        = new List<StockExportDetail>();
}
