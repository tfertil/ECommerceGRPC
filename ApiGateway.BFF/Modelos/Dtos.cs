namespace ApiGateway.BFF.Modelos;

// Lo que ve el cliente REST. No es el mensaje gRPC: el gateway transforma uno en otro (Mapeos.cs).

public record LoginDto(string Email, string Clave);

public record TokenDto(string Token, DateTime ExpiraEn, string Rol);

public class ProductoDto
{
    public int Id { get; init; }
    public string Nombre { get; init; } = "";
    public string? Descripcion { get; init; }
    public decimal? Precio { get; init; }
    public string? Categoria { get; init; }
    public int? Stock { get; init; }
    public bool? Disponible { get; init; }
}

public record ActualizarStockDto(int Cantidad);

public record UsuarioDto(int Id, string Email, string NombreCompleto, bool Activo, DateTime CreadoEl);

public record LineaPedidoDto(int ProductoId, int Cantidad);

public record PedidoDto(List<LineaPedidoDto> Lineas);

public record ValidacionPedidoDto(bool Valido, List<string> Problemas, decimal Total);

// Codigo escrito manualmente
public record CrearProductoDto(decimal Precio);

public record RegistroDto(string Email, string Clave, string Nombre, string Apellido);
