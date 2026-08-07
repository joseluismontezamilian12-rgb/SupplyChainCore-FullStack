using SupplyChainCore.Infrastructure.Security;

namespace SupplyChainCore.UnitTests;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Verify_DeberiaAceptarLaContraseñaCorrecta()
    {
        string hash = _hasher.Hash("Admin123!");

        Assert.True(_hasher.Verify("Admin123!", hash));
    }

    [Theory]
    [InlineData("admin123!")]   // difiere solo en mayúsculas
    [InlineData("Admin123")]    // le falta un carácter
    [InlineData("Admin123!!")]  // tiene uno de más
    [InlineData("otra-clave")]
    public void Verify_DeberiaRechazarCualquierContraseñaDistinta(string incorrecta)
    {
        string hash = _hasher.Hash("Admin123!");

        Assert.False(_hasher.Verify(incorrecta, hash));
    }

    [Fact]
    public void Hash_DeberiaProducirResultadosDistintosParaLaMismaContraseña()
    {
        // Sal aleatoria por usuario: dos cuentas con la misma contraseña no deben
        // compartir hash, o filtrar una revelaría a la otra.
        string primero = _hasher.Hash("MismaClave123!");
        string segundo = _hasher.Hash("MismaClave123!");

        Assert.NotEqual(primero, segundo);
        Assert.True(_hasher.Verify("MismaClave123!", primero));
        Assert.True(_hasher.Verify("MismaClave123!", segundo));
    }

    [Fact]
    public void Hash_DeberiaEmitirElFormatoIteracionesSalHash()
    {
        string hash = _hasher.Hash("Cualquiera123!");

        string[] partes = hash.Split('.');
        Assert.Equal(3, partes.Length);
        Assert.Equal(100_000, int.Parse(partes[0]));
        Assert.Equal(16, Convert.FromBase64String(partes[1]).Length); // sal
        Assert.Equal(32, Convert.FromBase64String(partes[2]).Length); // hash
    }

    [Theory]
    [InlineData("hashed_password")]                 // el placeholder de la semilla original
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sin.puntos")]
    [InlineData("abc.def.ghi")]                     // base64 inválido
    [InlineData("0.YWJj.ZGVm")]                     // cero iteraciones
    [InlineData("-5.YWJj.ZGVm")]                    // iteraciones negativas
    [InlineData("noEsNumero.YWJj.ZGVm")]
    public void Verify_DeberiaDevolverFalseAnteUnHashMalformado_SinLanzarExcepcion(string hashCorrupto)
    {
        // Un hash inválido en base de datos es un fallo de autenticación, no un
        // error 500: si lanzara, cualquier registro corrupto tumbaría el login.
        bool resultado = _hasher.Verify("cualquier-clave", hashCorrupto);

        Assert.False(resultado);
    }

    [Fact]
    public void Verify_DeberiaDevolverFalse_CuandoLaContraseñaEstaVacia()
    {
        string hash = _hasher.Hash("Admin123!");

        Assert.False(_hasher.Verify("", hash));
        Assert.False(_hasher.Verify("   ", hash));
    }

    [Fact]
    public void Hash_DeberiaRechazarUnaContraseñaVacia()
    {
        Assert.Throws<ArgumentException>(() => _hasher.Hash(""));
        Assert.Throws<ArgumentException>(() => _hasher.Hash("   "));
    }

    [Theory]
    [InlineData("100000.P3ocnlstSKbA4fS3jSNZrg==.tFxScx1pDVYzd6/qJok/Te3DERUxXxr5c4EGiHOby0U=", "Admin123!")]
    [InlineData("100000.obLD1OX2BxgpOktcbX6PkA==.GL3wW8VDG1K7qiEzAaCmF0xTdJ60eJshIbcUEvygKG0=", "Operador123!")]
    public void Verify_DeberiaAceptarLosHashesSembradosEnLaMigracion(string hashSembrado, string contraseñaDocumentada)
    {
        // Blinda la semilla: si alguien edita el hash del seed sin recalcularlo,
        // el usuario de demostración no podría entrar y esto lo detecta antes
        // de que el fallo aparezca en tiempo de ejecución.
        Assert.True(_hasher.Verify(contraseñaDocumentada, hashSembrado));
    }
}
