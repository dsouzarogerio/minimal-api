using MinimalApi.Dominio.Entidades;

namespace Test.Dominio.Entidades;

public class VeiculoTest
{
    [TestMethod]
    public void TestarPropriedadesGetSet()
    {
        //Arrange
        var veiculo = new Veiculo();

        //Act - SET
        veiculo.Id = 1;
        veiculo.Nome = "Corolla";
        veiculo.Marca = "Toyota";
        veiculo.Ano = 2020;

        //Assert - GET
        Assert.AreEqual(1, veiculo.Id);
        Assert.AreEqual("Corolla", veiculo.Nome);
        Assert.AreEqual("Toyota", veiculo.Marca);
        Assert.AreEqual(2020, veiculo.Ano);
    }

}