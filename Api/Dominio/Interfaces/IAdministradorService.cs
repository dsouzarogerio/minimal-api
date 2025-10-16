using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Dominio.Interfaces;

public interface IAdministradorService
{
    void Atualiza(Administrador administrador);
    Administrador? BuscaPorId(int id);
    List<Administrador> BuscaTodos(int? pagina);
    void DeletaAsync(int id);
    Administrador Incluir(AdministradorDTO administradorDTO);
    Administrador? ValidaLogin(LoginDTO loginDTO);
}