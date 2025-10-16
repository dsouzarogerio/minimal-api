using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos;

public class AdministradorService : IAdministradorService
{
    private readonly DbContexto _contexto;

    public AdministradorService(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public void Atualiza(Administrador administrador)
    {
        _contexto.Administradores.Update(administrador);
        _contexto.SaveChanges();
    }

    public Administrador? BuscaPorId(int id)
    {
        return _contexto.Administradores.Where(a => a.Id == id).FirstOrDefault();
    }

    public List<Administrador> BuscaTodos(int? pagina)
    {
        var query = _contexto.Administradores.AsQueryable();
        
        int pageSize = 10;
        if (pagina != null && pagina >= 1)
        {
            int skip = (int)((pagina - 1) * pageSize);
            query = query
                        .Skip(skip)
                        .Take(pageSize);
        }
        else
        {
            query = query.Take(pageSize);
        }

        return query.ToList();
    }

    public async void DeletaAsync(int id)
    {
        var administrador = await _contexto.Administradores.FindAsync(id);
        if (administrador != null)
        {
            _contexto.Administradores.Remove(administrador);
            await _contexto.SaveChangesAsync();    
        }
        
    }

    public Administrador Incluir(AdministradorDTO administradorDTO)
    {
        var administrador = new Administrador
        {
            Email = administradorDTO.Email,
            Senha = administradorDTO.Senha,
            Perfil = administradorDTO.Perfil.ToString()
        };

        _contexto.Administradores.Add(administrador);
        _contexto.SaveChanges();

        return administrador;
    }

    public Administrador? ValidaLogin(LoginDTO loginDTO)
    {
        return _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha)
            .FirstOrDefault();
    }
}