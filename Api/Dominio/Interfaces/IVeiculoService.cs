using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Dominio.Interfaces;

public interface IVeiculoService
{
    List<Veiculo> BuscaTodos(int? pagina = 1, string? nome = null, string? marca = null);
    Veiculo? BuscaPorId(int id);
    Veiculo Adiciona(VeiculoDTO veiculoDTO);
    void DeletaAsync(int id);
    void Atualiza(Veiculo veiculo);
}