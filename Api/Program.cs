using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.Dominio.DTOs;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.Infraestrutura.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

//Adicionando Serviços
builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

//JWT
var key = builder.Configuration.GetSection("Jwt").ToString();

if(string.IsNullOrEmpty(key))
{
    throw new InvalidOperationException("JWT key is not configured.");
}

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

//DbContext
builder.Services.AddDbContext<DbContexto>(
    options => options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    )
);

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header, 
        Description = "Insira o token JWT desta forma: Bearer {Seu Token}."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
var app = builder.Build();

#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home()))
.AllowAnonymous()
.WithTags("Home")
.WithName("Home")
.Produces<Home>(StatusCodes.Status200OK);

#endregion

#region Administrador

string GerarTokenJwt(Administrador administrador)
{
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil)
        
    };
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: credentials
    );
    return new JwtSecurityTokenHandler().WriteToken(token);
}

ErrosValidacao ValidacaoAdministradorDTO(AdministradorDTO administradorDTO)
{
    var erros = new List<string>();

    if (string.IsNullOrEmpty(administradorDTO.Email))
    {
        erros.Add("O campo E-MAIL é obrigatório.");
    }

    if (string.IsNullOrEmpty(administradorDTO.Senha))
    {
        erros.Add("O campo SENHA é obrigatório.");
    }
    if (string.IsNullOrEmpty(administradorDTO.Perfil.ToString()))
    {
        erros.Add("O campo PERFIL é obrigatório.");
    }

    return new ErrosValidacao { Mensagem = erros };
}

//valida login
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) =>
{
    var adm = administradorService.ValidaLogin(loginDTO);

    if (adm != null)
    {
        var token = GerarTokenJwt(adm);
        return Results.Ok(new AdministradorLogadoModelView
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
            
        });
    }
    else
    {
        return Results.Unauthorized();
    }
}).AllowAnonymous()
  .WithTags("Administradores")
  .WithName("Login do Administrador")
  .Produces<string>(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status401Unauthorized);

//criar administradores
app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorService administradorService) =>
{
    var validacao = ValidacaoAdministradorDTO(administradorDTO);

    if (validacao.Mensagem.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = administradorService.Incluir(administradorDTO);
    return Results.Created($"/administradores/{administrador.Id}", administrador);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
  .WithTags("Administradores")
  .WithName("Cadastro de Administrador")
  .Produces<Administrador>(StatusCodes.Status201Created);

//buscar todos
app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService administradorService) =>
{
    var adms = new List<AdministradorModelView>();
    var administradores = administradorService.BuscaTodos(pagina);

    foreach (var adm in administradores)
    {
        adms.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }

    return Results.Ok(adms);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
  .WithTags("Administradores")
  .WithName("Lista de Administradores")
  .Produces<List<Administrador>>(StatusCodes.Status200OK);

//buscar por id
app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorService administradorService) =>
{
    var administrador = administradorService.BuscaPorId(id);
     if (administrador == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(administrador);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
  .WithTags("Administradores")
  .WithName("Busca de Administrador por Id")
  .Produces<Administrador>(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status404NotFound);

//atualiza usuário
app.MapPut("/administradores/{id}", ([FromRoute] int id, [FromBody] AdministradorDTO administradorDTO, IAdministradorService administradorService) =>
{
    var validacao = ValidacaoAdministradorDTO(administradorDTO);

    if (validacao.Mensagem.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = administradorService.BuscaPorId(id);
    if (administrador == null)
    {
        return Results.NotFound();
    }

    administrador.Email = administradorDTO.Email;
    administrador.Senha = administradorDTO.Senha;
    administrador.Perfil = administradorDTO.Perfil.ToString();

    administradorService.Atualiza(administrador);
    return Results.Ok(administrador);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
  .WithTags("Administradores")
  .WithName("Atualiza de Administrador por Id")
  .Produces<Administrador>(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status404NotFound);

//deletar usuário
app.MapDelete("/administradores/{id}", ([FromRoute] int id, IAdministradorService administradorService) =>
{
    var administrador = administradorService.BuscaPorId(id);
    if (administrador == null)
    {
        return Results.NotFound();
    }
    administradorService.DeletaAsync(id);
    return Results.NoContent();
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
  .WithTags("Administradores")
  .WithName("Exclui Administrador por Id")
  .Produces(StatusCodes.Status204NoContent)
  .Produces(StatusCodes.Status404NotFound);

#endregion

#region Veiculos

ErrosValidacao ValidacaoErro(VeiculoDTO veiculoDTO)
{
    var erros = new List<string>();

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
    {
        erros.Add("O campo Nome é obrigatório.");
    }

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
    {
        erros.Add("O campo Marca é obrigatório.");
    }
    
    if (veiculoDTO.Ano <= 0 && veiculoDTO.Ano < 1950)
    {
        erros.Add("O campo Ano deve ser um número positivo e acima de 1950.");
    }

    return new ErrosValidacao { Mensagem = erros };
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
{
    var validacao = ValidacaoErro(veiculoDTO);

    if (validacao.Mensagem.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = veiculoService.Adiciona(veiculoDTO);
    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" })
  .WithTags("Veiculos")
  .WithName("Cadastro de Veiculo")
  .Produces<Veiculo>(StatusCodes.Status201Created);


app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoService veiculoService) =>
{
    var veiculos = veiculoService.BuscaTodos(pagina);
    return Results.Ok(veiculos);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" })
  .WithTags("Veiculos")
  .WithName("Lista de Veiculos")
  .Produces<List<Veiculo>>(StatusCodes.Status200OK);

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(veiculo);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" })
  .WithTags("Veiculos")
  .WithName("Busca de Veiculo por Id")
  .Produces<Veiculo>(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status404NotFound);


app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
{
    var validacao = ValidacaoErro(veiculoDTO);

    if (validacao.Mensagem.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = veiculoService.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoService.Atualiza(veiculo);
    return Results.Ok(veiculo);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
  .WithTags("Veiculos")
  .WithName("Atualiza de Veiculo por Id")
  .Produces<Veiculo>(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status404NotFound);

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }

    veiculoService.DeletaAsync(id);
    return Results.NoContent();
}).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
  .WithTags("Veiculos")
  .WithName("Deleta de Veiculo por Id")
  .Produces(StatusCodes.Status204NoContent)
  .Produces(StatusCodes.Status404NotFound);

#endregion

#region Apps
//Swagger
app.UseSwagger();
app.UseSwaggerUI();

//Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

//Run
app.Run();

#endregion
