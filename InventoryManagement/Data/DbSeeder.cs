using InventoryManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var admin = new User
            {
                FullName = "Quản trị viên",
                Username = "admin",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var passwordHasher = new PasswordHasher<User>();

            admin.PasswordHash = passwordHasher.HashPassword(
                admin,
                "Admin@123"
            );

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
