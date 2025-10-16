using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class VeiculoService : IVeiculoService
{
    private readonly DbContexto _contexto;

    public VeiculoService(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public Veiculo Adiciona(VeiculoDTO veiculoDTO)
    {
        var veiculo = new Veiculo
        {
            Nome = veiculoDTO.Nome,
            Marca = veiculoDTO.Marca,
            Ano = veiculoDTO.Ano
        };

        _contexto.Veiculos.Add(veiculo);
        _contexto.SaveChanges();

        return veiculo;
    }

    public void Atualiza(Veiculo veiculo)
    {
        _contexto.Veiculos.Update(veiculo);
        _contexto.SaveChanges();
    }

    public Veiculo? BuscaPorId(int id)
    {
        return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
    }

    public List<Veiculo> BuscaTodos(int? pagina = 1, string? nome = null, string? marca = null)
    {
        var query = _contexto.Veiculos.AsQueryable();

        if (!string.IsNullOrEmpty(nome))
        {
            query = query.Where(v => v.Nome.Contains(nome));
        }

        if (!string.IsNullOrEmpty(marca))
        {
            query = query.Where(v => v.Marca.Contains(marca));
        }

        int pageSize = 10;
        if (pagina != null && pagina >= 1)
        {
            int skip = (int)((pagina - 1) * pageSize);
            query = query.Skip(skip).Take(pageSize);
        }
        else
        {
            query = query.Take(pageSize);
        }

        return query.ToList();
    }

    public async void DeletaAsync(int id)
    {
        var veiculo = await _contexto.Veiculos.FindAsync(id);
        if (veiculo != null)
        {
            _contexto.Veiculos.Remove(veiculo);
            await _contexto.SaveChangesAsync();    
        }
        
    }


}