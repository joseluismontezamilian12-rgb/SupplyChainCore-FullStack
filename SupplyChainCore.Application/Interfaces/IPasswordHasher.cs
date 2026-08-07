namespace SupplyChainCore.Application.Interfaces;

public interface IPasswordHasher
{
    /// <summary>Deriva un hash con sal aleatoria. Dos llamadas con la misma contraseña devuelven hashes distintos.</summary>
    string Hash(string password);

    /// <summary>Verifica una contraseña en claro contra un hash almacenado. Nunca lanza ante un hash malformado: devuelve false.</summary>
    bool Verify(string password, string hashAlmacenado);
}
