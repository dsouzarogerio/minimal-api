

using MinimalApi.Dominio.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Olá World!");
app.MapPost("/login", (LoginDTO loginDTO)  => {
    if(loginDTO.Email == "admin@teste.com" && loginDTO.Senha == "123456")
    {
        return Results.Ok("Login efetuado com sucesso!");
    }
    else
    {
       return Results.Unauthorized();
    }
});

app.Run();
