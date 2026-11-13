using ApiGateway.BFF.Modelos;
using ECommerceGRPC.Users;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiGateway.BFF.Controllers;

// Segundo servicio gRPC detrás del gateway: UserService.gRPC (7002).
[ApiController]
[Route("api/v1/usuarios")]
[Authorize]
public class UsuariosController(UserService.UserServiceClient usuarios) : ControllerBase
{

    // Creados por CODEX: Registrar, Listar
    /// <summary>Registra un usuario sin requerir token.</summary>
    /// <remarks>Llama a CreateUser. Datos inválidos producen 400 y un correo repetido, 409. Limita intentos por IP.</remarks>
    [HttpPost("registro")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UsuarioDto>> Registrar(RegistroDto dto, CancellationToken ct)
    {
        var reply = await usuarios.CreateUserAsync(Mapeos.ACreateUser(dto), cancellationToken: ct);

        return CreatedAtAction(nameof(Obtener), new { id = reply.Id }, Mapeos.ADto(reply));
    }

    /// <summary>Lista los usuarios activos. Solo el rol Admin.</summary>
    /// <remarks>Consume ListUsers por server streaming y reúne sus mensajes en una lista JSON.</remarks>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("lecturas")]
    [ProducesResponseType<List<UsuarioDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<List<UsuarioDto>>> Listar(CancellationToken ct)
    {
        using var llamada = usuarios.ListUsers(new ListUsersRequest(), cancellationToken: ct);
        var lista = new List<UsuarioDto>();
        await foreach (var usuario in llamada.ResponseStream.ReadAllAsync(ct))
            lista.Add(Mapeos.ADto(usuario));
        return Ok(lista);
    }

    /// <summary>Un usuario por id. Admin ve a cualquiera; Cliente, solo a sí mismo.</summary>
    [HttpGet("{id:int}")]
    [EnableRateLimiting("lecturas")]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> Obtener(int id, CancellationToken ct)
    {
        // "sub" es el id del usuario dentro del token.
        if (!User.IsInRole("Admin") && User.FindFirst("sub")?.Value != id.ToString())
            return Forbid();

        var reply = await usuarios.GetUserAsync(new GetUserRequest { Id = id }, cancellationToken: ct);
        return Ok(Mapeos.ADto(reply));
    }
}
