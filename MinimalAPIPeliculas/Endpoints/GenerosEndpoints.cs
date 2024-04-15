using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIPeliculas.DTOs;
using MinimalAPIPeliculas.Entidades;
using MinimalAPIPeliculas.Migrations;
using MinimalAPIPeliculas.Repositorios;

namespace MinimalAPIPeliculas.Endpoints
{
    public static class GenerosEndpoints
    {
        public static RouteGroupBuilder MapGeneros(this RouteGroupBuilder group)
        {
            group.MapPost("/", CrearGenero);
            //Endpoint para actualizar un género
            group.MapPut("/{id:int}", ActualizarGenero);
            //Endpoint para borrar un Género
            group.MapDelete("/{id:int}", BorrarGenero);
            //Obtener genero por Id
            group.MapGet("/{id:int}", ObtenerGeneroPorId);
            //Lo mismo que el ejemplo en la clase program pero con funcion nombrada
            group.MapGet("/", ObtenerGeneros).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(50)).Tag("generos-tag-cache"));

            static async Task<Ok<List<GeneroDTO>>> ObtenerGeneros(IRepositorioGeneros repositorio, IMapper mapper)
            {
                var generos = await repositorio.ObtenerTodos();
                //Con typed results se regresa una estructura como respuesta para saber que esperar de retorno de la funcion.
                
                //Sin el mapper
                //var generosDTO = generos.Select(x=> new GeneroDTO {Id=x.Id, Nombre=x.Nombre }).ToList();

                var generosDTO = mapper.Map<List<GeneroDTO>>(generos);
                return TypedResults.Ok(generosDTO);
            }

            //Función nombrada que regresa un género por Id
            static async Task<Results<Ok<GeneroDTO>, NotFound>> ObtenerGeneroPorId(IRepositorioGeneros repositorio, int id, IMapper mapper)
            {
                var genero = await repositorio.ObtenerPorId(id);
                if (genero is null)
                {
                    return TypedResults.NotFound();
                }

                //Mapeo Manual
                //var generoDTO = new GeneroDTO
                //{
                //    Id = id,
                //    Nombre = genero.Nombre
                //};

                //Mapeo con automapper
                var generoDTO = mapper.Map<GeneroDTO>(genero);

                return TypedResults.Ok(generoDTO);
            }

            static async Task<Created<GeneroDTO>> CrearGenero(CrearGeneroDTO crearGeneroDTO, IRepositorioGeneros repositorio, 
                IOutputCacheStore outputCacheStore, IMapper mapper)
            {
                var genero = mapper.Map<Genero>(crearGeneroDTO);

                var id = await repositorio.Crear(genero);
                await outputCacheStore.EvictByTagAsync("generos-tag-cache", default);
                var generoDTO = mapper.Map<GeneroDTO>(genero);

                return TypedResults.Created($"/generos/{id}", generoDTO);
            }

            static async Task<Results<NoContent, NotFound>> BorrarGenero(int id, IRepositorioGeneros repositorio,
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

            static async Task<Results<NoContent, NotFound>> ActualizarGenero(int id, CrearGeneroDTO crearGeneroDTO, IRepositorioGeneros repositorio,
                IOutputCacheStore outputCacheStore, IMapper mapper)
            {
                var existe = await repositorio.Existe(id);
                if (!existe)
                {
                    return TypedResults.NotFound();
                }
                var genero=mapper.Map<Genero>(crearGeneroDTO);
                genero.Id = id;

                await repositorio.Actualizar(genero);
                await outputCacheStore.EvictByTagAsync("generos-tag-cache", default);
                return TypedResults.NoContent();

            }
            return group;
        }
    }
}
