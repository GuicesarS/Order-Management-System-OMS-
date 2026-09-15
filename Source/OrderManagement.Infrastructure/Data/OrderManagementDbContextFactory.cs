using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderManagement.Infrastructure.Data;

public class OrderManagementDbContextFactory : IDesignTimeDbContextFactory<OrderManagementDbContext>
{
    public OrderManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderManagementDbContext>();

        // Usada só pelas ferramentas de design-time do EF Core (dotnet ef migrations add/update)
        // quando rodadas fora do host da aplicação (que normalmente pega a connection string
        // de appsettings/variáveis de ambiente). Não é a connection string usada em runtime.
        var connectionString = "Server=localhost,1433;Database=oms_dev;User ID=sa;Password=SuaSenha@123;TrustServerCertificate=true";
        optionsBuilder.UseSqlServer(connectionString);

        return new OrderManagementDbContext(optionsBuilder.Options);
    }
}