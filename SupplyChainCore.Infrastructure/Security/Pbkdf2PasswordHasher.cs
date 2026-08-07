using System.Security.Cryptography;
using SupplyChainCore.Application.Interfaces;

namespace SupplyChainCore.Infrastructure.Security;

/// <summary>
/// Hashing de contraseñas con PBKDF2-HMAC-SHA256, sal aleatoria por usuario y
/// comparación en tiempo constante.
///
/// Formato almacenado: {iteraciones}.{salBase64}.{hashBase64}
/// Guardar las iteraciones dentro del propio hash permite subir el factor de
/// trabajo más adelante sin invalidar las contraseñas ya existentes.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iteraciones = 100_000;
    private const int TamañoSalBytes = 16;
    private const int TamañoHashBytes = 32;
    private static readonly HashAlgorithmName Algoritmo = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        byte[] sal = RandomNumberGenerator.GetBytes(TamañoSalBytes);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, sal, Iteraciones, Algoritmo, TamañoHashBytes);

        return $"{Iteraciones}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hashAlmacenado)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashAlmacenado))
        {
            return false;
        }

        // Un hash con formato inválido (por ejemplo el placeholder "hashed_password"
        // de una semilla vieja) es un fallo de autenticación, no una excepción.
        string[] partes = hashAlmacenado.Split('.', 3);
        if (partes.Length != 3 ||
            !int.TryParse(partes[0], out int iteraciones) ||
            iteraciones <= 0)
        {
            return false;
        }

        byte[] sal, hashEsperado;
        try
        {
            sal = Convert.FromBase64String(partes[1]);
            hashEsperado = Convert.FromBase64String(partes[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (sal.Length == 0 || hashEsperado.Length == 0)
        {
            return false;
        }

        byte[] hashCandidato = Rfc2898DeriveBytes.Pbkdf2(
            password, sal, iteraciones, Algoritmo, hashEsperado.Length);

        // FixedTimeEquals evita filtrar, por diferencia de tiempo, cuántos bytes coincidieron.
        return CryptographicOperations.FixedTimeEquals(hashCandidato, hashEsperado);
    }
}
