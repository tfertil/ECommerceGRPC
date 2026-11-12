using ApiGateway.BFF.Modelos;
using ECommerceGRPC.Products;
using ECommerceGRPC.Users;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiGateway.BFF.Controllers;

// REST afuera, gRPC adentro: cada acción recibe JSON, llama a ProductService.gRPC (7001)
// y devuelve JSON. Los errores gRPC los traduce RpcExceptionHandler.
[ApiController]
[Route("api/v1/productos")]
public class ProductosController(ProductService.ProductServiceClient productos) : ControllerBase
{
    /// <summary>Lista los productos activos. Opcional: filtrar por categoría.</summary>
    /// <remarks>Por dentro llama a ListProducts, que es server streaming: el gateway junta los mensajes en una lista JSON.</remarks>
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("lecturas")]
    [ProducesResponseType<List<ProductoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<ProductoDto>>> Listar([FromQuery] string? categoria, CancellationToken ct)
    {
        using var llamada = productos.ListProducts(new ListProductsRequest { Category = categoria ?? "" }, cancellationToken: ct);
        var lista = new List<ProductoDto>();
        await foreach (var p in llamada.ResponseStream.ReadAllAsync(ct))
            lista.Add(Mapeos.ADto(p));
        return Ok(lista);
    }

    /// <summary>Un producto por id.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [EnableRateLimiting("lecturas")]
    [ProducesResponseType<ProductoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductoDto>> Obtener(int id, CancellationToken ct)
    {
        // TODO 2 · Llame a productos.GetProductAsync con un GetProductRequest { Id = id }
        //   y devuelva Ok(Mapeos.ADto(reply)). Si no existe, gRPC responde NotFound
        //   y RpcExceptionHandler lo convierte en 404: no hace falta un try/catch.

        // Manual
        var reply = await productos.GetProductAsync(new GetProductRequest { Id = id }, cancellationToken: ct);
        return Ok(Mapeos.ADto(reply));

        // Codigo temporal comentado
        //await Task.CompletedTask;
        //return StatusCode(501, "Falta el TODO 2");
    }

    /// <summary>Cambia el stock de un producto. Solo el rol Admin.</summary>
    /// <remarks>Por dentro llama a UpdateStock, que además avisa a quien esté mirando WatchStock.</remarks>
    [HttpPut("{id:int}/stock")]
    [EnableRateLimiting("escrituras")]
    [ProducesResponseType<ProductoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]

    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoDto>> ActualizarStock(int id, ActualizarStockDto dto, CancellationToken ct)
    {
        // TODO 5 · Proteja esta acción con [Authorize(Roles = "Admin")] (arriba de [HttpPut]).
        //   Después llame a productos.UpdateStockAsync(Mapeos.AUpdateStock(id, dto))
        //   y devuelva Ok(Mapeos.ADto(reply)).


        //await Task.CompletedTask;
        //return StatusCode(501, "Falta el TODO 5");

        var reply = await productos.UpdateStockAsync(Mapeos.AUpdateStock(id, dto), cancellationToken: ct);
        return Ok(Mapeos.ADto(reply));
    }
}
