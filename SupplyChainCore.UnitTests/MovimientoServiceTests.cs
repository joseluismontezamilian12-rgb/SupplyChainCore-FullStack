using Moq;
using SupplyChainCore.Application.Interfaces;
using SupplyChainCore.Application.Services;
using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.UnitTests;

/// <summary>
/// Reglas de negocio del ledger de inventario. El invariante que se defiende en
/// todo el archivo: ninguna operación puede dejar el stock en negativo, y nada
/// llega al repositorio si una regla falla.
/// </summary>
public class MovimientoServiceTests
{
    private readonly Mock<IMovimientoInventarioRepository> _repositoryMock = new();
    private readonly MovimientoService _servicio;

    public MovimientoServiceTests()
    {
        _servicio = new MovimientoService(_repositoryMock.Object);
    }

    private void ConStockDisponible(int cantidad) =>
        _repositoryMock
            .Setup(r => r.ObtenerStockActualAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(cantidad);

    private void VerificarQueNoSePersistioNada() =>
        _repositoryMock.Verify(
            r => r.RegistrarMovimientoAsync(It.IsAny<MovimientoInventario>()),
            Times.Never);

    [Fact]
    public async Task RegistrarTransaccion_DeberiaLanzarExcepcion_CuandoStockEsInsuficiente()
    {
        ConStockDisponible(10);

        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicio.RegistrarTransaccionAsync(1, 1, 1, 50, "SALIDA", "Despacho a tienda"));

        Assert.Contains("Stock insuficiente", excepcion.Message);
        VerificarQueNoSePersistioNada();
    }

    [Fact]
    public async Task RegistrarTransaccion_DeberiaPermitirLaSalida_CuandoElStockEsExactamenteElSolicitado()
    {
        // El límite exacto es el caso que más se rompe al refactorizar: 10 unidades
        // disponibles y 10 solicitadas debe dejar el saldo en cero, no fallar.
        ConStockDisponible(10);

        await _servicio.RegistrarTransaccionAsync(1, 1, 1, 10, "SALIDA", "Despacho exacto");

        _repositoryMock.Verify(
            r => r.RegistrarMovimientoAsync(It.Is<MovimientoInventario>(m => m.Cantidad == 10)),
            Times.Once);
    }

    [Theory]
    [InlineData("SALIDA")]
    [InlineData("MERMA")]
    public async Task RegistrarTransaccion_DeberiaValidarStock_EnTodoMovimientoQueRestaInventario(string tipo)
    {
        ConStockDisponible(5);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicio.RegistrarTransaccionAsync(1, 1, 1, 6, tipo, "Prueba"));

        VerificarQueNoSePersistioNada();
    }

    [Fact]
    public async Task RegistrarTransaccion_NoDeberiaConsultarStock_CuandoEsUnIngreso()
    {
        // Un ingreso suma inventario: pedir el stock actual sería trabajo inútil
        // contra la base y además bloquearía la primera carga de un producto nuevo.
        await _servicio.RegistrarTransaccionAsync(1, 1, 1, 100, "INGRESO", "Compra inicial");

        _repositoryMock.Verify(
            r => r.ObtenerStockActualAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
        _repositoryMock.Verify(
            r => r.RegistrarMovimientoAsync(It.IsAny<MovimientoInventario>()),
            Times.Once);
    }

    [Theory]
    [InlineData("TRANSFERENCIA")]
    [InlineData("")]
    [InlineData("ingreso_masivo")]
    public async Task RegistrarTransaccion_DeberiaRechazarTiposDeMovimientoNoReconocidos(string tipoInvalido)
    {
        var excepcion = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarTransaccionAsync(1, 1, 1, 5, tipoInvalido, "Prueba"));

        Assert.Contains("INGRESO", excepcion.Message);
        VerificarQueNoSePersistioNada();
    }

    [Theory]
    [InlineData("ingreso")]
    [InlineData("Salida")]
    [InlineData("mErMa")]
    public async Task RegistrarTransaccion_DeberiaNormalizarElTipoAMayusculas(string tipoConMayusculasMixtas)
    {
        ConStockDisponible(1000);

        await _servicio.RegistrarTransaccionAsync(1, 1, 1, 5, tipoConMayusculasMixtas, "Prueba");

        _repositoryMock.Verify(
            r => r.RegistrarMovimientoAsync(It.Is<MovimientoInventario>(
                m => m.TipoMovimiento == tipoConMayusculasMixtas.ToUpper())),
            Times.Once);
    }

    [Fact]
    public async Task RegistrarTransaccion_DeberiaMapearTodosLosDatosAlMovimientoPersistido()
    {
        ConStockDisponible(500);
        MovimientoInventario? capturado = null;

        _repositoryMock
            .Setup(r => r.RegistrarMovimientoAsync(It.IsAny<MovimientoInventario>()))
            .Callback<MovimientoInventario>(m => capturado = m)
            .Returns(Task.CompletedTask);

        await _servicio.RegistrarTransaccionAsync(
            productoId: 7, almacenId: 3, usuarioId: 42,
            cantidad: 25, tipoMovimiento: "SALIDA", motivo: "Despacho a tienda Miraflores");

        Assert.NotNull(capturado);
        Assert.Equal(7, capturado!.ProductoId);
        Assert.Equal(3, capturado.AlmacenId);
        Assert.Equal(42, capturado.UsuarioId);
        Assert.Equal(25, capturado.Cantidad);
        Assert.Equal("SALIDA", capturado.TipoMovimiento);
        Assert.Equal("Despacho a tienda Miraflores", capturado.Motivo);
    }

    [Fact]
    public async Task RegistrarTransaccion_DeberiaSellarLaFechaEnUtc()
    {
        // El ledger es una bitácora auditable: si la fecha se guardara en hora
        // local, el orden de los movimientos dejaría de ser comparable entre zonas.
        ConStockDisponible(100);
        MovimientoInventario? capturado = null;

        _repositoryMock
            .Setup(r => r.RegistrarMovimientoAsync(It.IsAny<MovimientoInventario>()))
            .Callback<MovimientoInventario>(m => capturado = m)
            .Returns(Task.CompletedTask);

        var antes = DateTime.UtcNow.AddSeconds(-1);
        await _servicio.RegistrarTransaccionAsync(1, 1, 1, 1, "SALIDA", "Prueba");
        var despues = DateTime.UtcNow.AddSeconds(1);

        Assert.NotNull(capturado);
        Assert.Equal(DateTimeKind.Utc, capturado!.FechaTransaccion.Kind);
        Assert.InRange(capturado.FechaTransaccion, antes, despues);
    }

    [Fact]
    public async Task ObtenerStockDisponible_DeberiaDelegarEnElRepositorio()
    {
        _repositoryMock
            .Setup(r => r.ObtenerStockActualAsync(9, 4))
            .ReturnsAsync(37);

        int stock = await _servicio.ObtenerStockDisponibleAsync(9, 4);

        Assert.Equal(37, stock);
    }

    [Fact]
    public async Task ObtenerHistorialProducto_DeberiaDevolverLosMovimientosDelRepositorio()
    {
        var esperados = new List<MovimientoInventario>
        {
            new() { Id = 1, ProductoId = 9, Cantidad = 10, TipoMovimiento = "INGRESO" },
            new() { Id = 2, ProductoId = 9, Cantidad = 4,  TipoMovimiento = "SALIDA" }
        };

        _repositoryMock
            .Setup(r => r.ObtenerHistorialAsync(9, 4))
            .ReturnsAsync(esperados);

        var historial = await _servicio.ObtenerHistorialProductoAsync(9, 4);

        Assert.Equal(2, historial.Count());
        Assert.Equal(esperados, historial);
    }
}
