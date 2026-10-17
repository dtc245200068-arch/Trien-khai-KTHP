using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string Unit { get; set; } = "Cái";

    [Column(TypeName = "decimal(18,2)")]
    public decimal ImportPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ExportPrice { get; set; }

    public int Quantity { get; set; } = 0;

    public int MinimumStock { get; set; } = 0;

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
