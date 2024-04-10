using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using MinimalAPIPeliculas;
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

builder.Services.AddScoped<IRepositorioGeneros, RepositorioGeneros>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOutputCache();
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

var endpointGeneros = app.MapGroup("/generos");

//Endpoint para leer la tabla de géneros (con función flecha)
//endpointGeneros.MapGet("/", async (IRepositorioGeneros repositorio) =>
//{
//    return await repositorio.ObtenerTodos();
//}).CacheOutput(c=>c.Expire(TimeSpan.FromSeconds(20)).Tag("generos-tag-cache"));

//Lo mismo que el ejemplo anterior pero con funcion nombrada
endpointGeneros.MapGet("/", ObtenerGeneros).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(50)).Tag("generos-tag-cache"));


//Endpoint para crear 1 Genero (con flecha)
//endpointGeneros.MapPost("/", async(Genero genero, IRepositorioGeneros repositorio, IOutputCacheStore outputCacheStore) =>
//{
//    var id = await repositorio.Crear(genero);
//    await outputCacheStore.EvictByTagAsync("generos-tag-cache",default);
//    return Results.Created($"/generos/{id}", genero);
//});

endpointGeneros.MapPost("/", CrearGenero);
//Endpoint para actualizar un género
endpointGeneros.MapPut("/{id:int}", ActualizarGenero);
//Endpoint para borrar un Género
endpointGeneros.MapDelete("/{id:int}", BorrarGenero);
//Obtener genero por Id
endpointGeneros.MapGet("/{id:int}", ObtenerGeneroPorId);

//*************************************************

app.Run();

static async Task<Ok<List<Genero>>> ObtenerGeneros (IRepositorioGeneros repositorio)
{
    var generos = await repositorio.ObtenerTodos();
    //Con typed results se regresa una estructura como respuesta para saber que esperar de retorno de la funcion.
    return TypedResults.Ok(generos);
}

//Función nombrada que regresa un género por Id
static async Task<Results<Ok<Genero>, NotFound>> ObtenerGeneroPorId (IRepositorioGeneros repositorio, int id)
{
    var genero = await repositorio.ObtenerPorId(id);
    if (genero is null)
    {
        return TypedResults.NotFound();
    }
    return TypedResults.Ok(genero);
}

static async Task<Created<Genero>>CrearGenero(Genero genero, IRepositorioGeneros repositorio, IOutputCacheStore outputCacheStore)
{
    var id = await repositorio.Crear(genero);
    await outputCacheStore.EvictByTagAsync("generos-tag-cache", default);
    return TypedResults.Created($"/generos/{id}", genero);
}

static async Task<Results<NoContent,NotFound>> BorrarGenero (int id, IRepositorioGeneros repositorio,
   IOutputCacheStore outputCacheStore) 
{
    var existe = await repositorio.Existe(id);
    if (!existe)
    {
        return TypedResults.NotFound();
    }
    await repositorio.Borrar(id);
    await outputCacheStore.EvictByTagAsync("generos-tag-cache", default);
    return TypedResults.NoContent();
}

static async Task<Results<NoContent, NotFound>> ActualizarGenero (int id, Genero genero, IRepositorioGeneros repositorio,
    IOutputCacheStore outputCacheStore)
{ 
    var existe = await repositorio.Existe(id);
if (!existe)
{
    return TypedResults.NotFound();
}
await repositorio.Actualizar(genero);
await outputCacheStore.EvictByTagAsync("generos-tag-cache", default);
return TypedResults.NoContent();

}