using ApiGateway.BFF.Modelos;
using ECommerceGRPC.Users;
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
