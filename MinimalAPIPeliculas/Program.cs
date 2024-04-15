using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using MinimalAPIPeliculas;
using MinimalAPIPeliculas.Endpoints;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Migrations;
using MinimalAPIPeliculas.Repositorios;
using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);
var ambiente = builder.Configuration.GetValue<string>("ambiente");
var origenesPermitidos = builder.Configuration.GetValue<string>("origenesPermitidos")!;
//Aquí van los servicios (antes del app builder)

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
opciones.UseSqlServer("name=DefaultConnection"));

builder.Services.AddCors(options =>
        options.AddDefaultPolicy(configuration=>
    {
        configuration.WithOrigins(origenesPermitidos).AllowAnyHeader().AllowAnyMethod();  
    }));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOutputCache();
builder.Services.AddScoped<IRepositorioGeneros, RepositorioGeneros>();
builder.Services.AddAutoMapper(typeof(Program));
//***************************************************

//Aquí los middlewares después del app builder
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
//Servicio Personalizado para acceso a la interfaz de repositorios (inversión de dependencias)


app.UseCors();
app.UseOutputCache();
//app.MapGet("/", () =>"En "+ ambiente+" Hello World!");
//app.MapGet("/other-page", () =>  "Hello other page!");

app.MapGroup("/generos").MapGeneros();

//Endpoint para leer la tabla de géneros (con función flecha)
//endpointGeneros.MapGet("/", async (IRepositorioGeneros repositorio) =>
//{
//    return await repositorio.ObtenerTodos();
//}).CacheOutput(c=>c.Expire(TimeSpan.FromSeconds(20)).Tag("generos-tag-cache"));

//Endpoint para crear 1 Genero (con flecha)
//endpointGeneros.MapPost("/", async(Genero genero, IRepositorioGeneros repositorio, IOutputCacheStore outputCacheStore) =>
//{
//    var id = await repositorio.Crear(genero);
//    await outputCacheStore.EvictByTagAsync("generos-tag-cache",default);
//    return Results.Created($"/generos/{id}", genero);
//});

//*************************************************

app.Run();

