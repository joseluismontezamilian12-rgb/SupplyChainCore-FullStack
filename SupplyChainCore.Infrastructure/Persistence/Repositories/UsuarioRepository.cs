using Microsoft.EntityFrameworkCore;
using SupplyChainCore.Application.Interfaces;
using SupplyChainCore.Domain.Entities;

namespace SupplyChainCore.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        // AsNoTracking: el login solo lee. Include(Rol) es necesario porque el
        // nombre del rol termina como claim dentro del token.
        return await _context.Usuarios
            .AsNoTracking()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}
