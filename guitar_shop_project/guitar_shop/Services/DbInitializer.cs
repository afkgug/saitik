using Microsoft.EntityFrameworkCore;
using guitar_shop.Data;

namespace guitar_shop.Services;

public class DbInitializer
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Создаем базу данных, если она не существует
        context.Database.EnsureCreated();
    }
}
