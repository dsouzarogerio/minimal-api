using MinimalApi.Dominio.Entidades;

namespace Test.Dominio.Entidades;

public class AdministradorTest
{
    [TestMethod]
    public void TestarPropriedadesGetSet()
    {
        //Arrange
        var adm = new Administrador();

        //Act - SET
        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "ADM";

        //Assert - GET
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("teste", adm.Senha);
        Assert.AreEqual("ADM", adm.Perfil);
    }

}