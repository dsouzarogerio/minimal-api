using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Enums;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;
using Moq;

namespace Test.Dominio.Servicos;

[TestClass]
public class AdministradorServiceTest
{
    private DbContexto? _context;
    private IConfiguration? _configuration;
    private AdministradorService? _service;

    [TestInitialize]
    public void Setup()
    {
        // Configurar o banco em memória
        var options = new DbContextOptionsBuilder<DbContexto>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

    // Criar contexto com banco em memória
    _context = new DbContexto(options);
    // Inicializar o serviço
    _service = new AdministradorService(_context);
    }

    [TestMethod]
    public async Task DeveIncluirAdministradorComSucesso()
    {
        // Arrange
        var administrador = new AdministradorDTO
        {
            Email = "admin@teste.com",
            Senha = "senha123",
            Perfil = Perfil.ADM
        };

        // Act
        var resultado = _service?.Incluir(administrador);

        // Assert
        Assert.IsNotNull(resultado);
        Assert.AreEqual(1, resultado.Id);
        Assert.AreEqual("admin@teste.com", resultado.Email);

    // Verificar se foi realmente salvo no contexto
    Assert.IsNotNull(_context);
    var admSalvo = await _context.Administradores.FindAsync(resultado.Id);
    Assert.IsNotNull(admSalvo);
    Assert.AreEqual(administrador.Email, admSalvo.Email);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Limpar o contexto após cada teste
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }
}