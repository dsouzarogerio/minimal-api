using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Infraestrutura.Db;

public class DbContexto : DbContext
{
    private readonly IConfiguration? _configuration;

    // Construtor para uso normal (injeção de IConfiguration)
    public DbContexto(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Construtor para testes (injeção de opções)
    public DbContexto(DbContextOptions<DbContexto> options) : base(options)
    {
    }

    public DbSet<Administrador> Administradores { get; set; } = default!;
    public DbSet<Veiculo> Veiculos { get; set; } = default!;

    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     modelBuilder.Entity<Administrador>().HasData(
    //         new Administrador
    //         {
    //             Id = 1,
    //             Email = "admin@teste.com",
    //             Senha = "123456",
    //             Perfil = "Admin"
    //         }
    //     );
    // }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _configuration != null)
        {
            var configurationString = _configuration.GetConnectionString("MySql")?.ToString();
            if (!string.IsNullOrEmpty(configurationString))
            {
                optionsBuilder.UseMySql(
                    configurationString,
                    ServerVersion.AutoDetect(configurationString));
            }
        }
    }
}