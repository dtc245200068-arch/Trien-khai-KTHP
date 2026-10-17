using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(150)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
