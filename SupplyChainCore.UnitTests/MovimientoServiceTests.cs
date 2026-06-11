using Moq;
using SupplyChainCore.Application.Interfaces;
using SupplyChainCore.Application.Services;

namespace SupplyChainCore.UnitTests;

public class MovimientoServiceTests
{
    [Fact]
    public async Task RegistrarTransaccion_DeberiaLanzarExcepcion_CuandoStockEsInsuficiente()
    {
        // 1. ARRANGE (Preparar el escenario)
        var repositoryMock = new Mock<IMovimientoInventarioRepository>();

        // Simulamos que el repositorio dice que solo hay 10 unidades de stock real en la base de datos
        repositoryMock
            .Setup(repo => repo.ObtenerStockActualAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(10);

        // Inyectamos el clon simulado dentro de nuestro servicio real
        var servicio = new MovimientoService(repositoryMock.Object);

        // Datos de prueba: Intentaremos retirar 50 unidades (teniendo solo 10)
        int productoId = 1;
        int almacenId = 1;
        int usuarioId = 1;
        int cantidadSolicitada = 50;
        string tipoMovimiento = "SALIDA";
        string motivo = "Despacho a tienda";

        // 2. ACT & 3. ASSERT (Ejecutar la acción y verificar el bloqueo)
        // Verificamos que el sistema efectivamente arroje un 'InvalidOperationException'
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servicio.RegistrarTransaccionAsync(productoId, almacenId, usuarioId, cantidadSolicitada, tipoMovimiento, motivo)
        );

        // Verificamos que el mensaje de error de la excepción contenga la advertencia de stock
        Assert.Contains("Stock insuficiente", excepcion.Message);

        // Verificamos por seguridad que el repositorio NUNCA haya guardado nada corrupto en la base de datos
        repositoryMock.Verify(repo => repo.RegistrarMovimientoAsync(It.IsAny<Domain.Entities.MovimientoInventario>()), Times.Never);
    }
}