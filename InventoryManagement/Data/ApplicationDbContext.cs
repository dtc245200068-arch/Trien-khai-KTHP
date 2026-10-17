using InventoryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }

    public DbSet<StockImport> StockImports { get; set; }
    public DbSet<StockImportDetail> StockImportDetails { get; set; }

    public DbSet<StockExport> StockExports { get; set; }
    public DbSet<StockExportDetail> StockExportDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================
        // NHẬP KHO
        // =========================

        modelBuilder.Entity<StockImport>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<StockImport>()
            .HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockImportDetail>()
            .HasOne(x => x.StockImport)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.StockImportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockImportDetail>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================
        // XUẤT KHO
        // =========================

        modelBuilder.Entity<StockExport>()
            .HasIndex(x => x.Code)
            .IsUnique();

        modelBuilder.Entity<StockExportDetail>()
            .HasOne(x => x.StockExport)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.StockExportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockExportDetail>()
            .HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
