using EventifyJC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventifyJC.Identity;

// Este store le dice a ASP.NET Core Identity cómo leer/escribir usuarios
// usando tu tabla Usuario tal cual está (sin agregar columnas de Identity).
public class UsuarioStore :
    IUserStore<Usuario>,
    IUserPasswordStore<Usuario>,
    IUserEmailStore<Usuario>
{
    private readonly ApplicationDbContext _context;

    public UsuarioStore(ApplicationDbContext context)
    {
        _context = context;
    }

    // ---------- IUserStore ----------

    public Task<string> GetUserIdAsync(Usuario user, CancellationToken ct)
        => Task.FromResult(user.IdUsuario.ToString());

    // No tenemos columna "UserName" separada: usamos el email como nombre de usuario.
    public Task<string?> GetUserNameAsync(Usuario user, CancellationToken ct)
        => Task.FromResult<string?>(user.Email);

    public Task SetUserNameAsync(Usuario user, string? userName, CancellationToken ct)
    {
        user.Email = userName ?? user.Email;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(Usuario user, CancellationToken ct)
        => Task.FromResult<string?>(user.Email?.ToUpperInvariant());

    public Task SetNormalizedUserNameAsync(Usuario user, string? normalizedName, CancellationToken ct)
        => Task.CompletedTask; // se calcula al vuelo a partir del email

    public async Task<IdentityResult> CreateAsync(Usuario user, CancellationToken ct)
    {
        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync(ct);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(Usuario user, CancellationToken ct)
    {
        _context.Usuarios.Update(user);
        await _context.SaveChangesAsync(ct);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(Usuario user, CancellationToken ct)
    {
        _context.Usuarios.Remove(user);
        await _context.SaveChangesAsync(ct);
        return IdentityResult.Success;
    }

    public Task<Usuario?> FindByIdAsync(string userId, CancellationToken ct)
    {
        if (!int.TryParse(userId, out var id))
            return Task.FromResult<Usuario?>(null);

        return _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id, ct);
    }

    public Task<Usuario?> FindByNameAsync(string normalizedUserName, CancellationToken ct)
        => _context.Usuarios.FirstOrDefaultAsync(
            u => u.Email.ToUpper() == normalizedUserName, ct);

    // ---------- IUserPasswordStore ----------

    public Task SetPasswordHashAsync(Usuario user, string? passwordHash, CancellationToken ct)
    {
        user.PasswordHash = passwordHash ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(Usuario user, CancellationToken ct)
        => Task.FromResult<string?>(user.PasswordHash);

    public Task<bool> HasPasswordAsync(Usuario user, CancellationToken ct)
        => Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

    // ---------- IUserEmailStore ----------

    public Task SetEmailAsync(Usuario user, string? email, CancellationToken ct)
    {
        user.Email = email ?? user.Email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(Usuario user, CancellationToken ct)
        => Task.FromResult<string?>(user.Email);

    // No tenemos columna de confirmación de email: se asume confirmado.
    public Task<bool> GetEmailConfirmedAsync(Usuario user, CancellationToken ct)
        => Task.FromResult(true);

    public Task SetEmailConfirmedAsync(Usuario user, bool confirmed, CancellationToken ct)
        => Task.CompletedTask;

    public Task<Usuario?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
        => _context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToUpper() == normalizedEmail, ct);

    public Task<string?> GetNormalizedEmailAsync(Usuario user, CancellationToken ct)
        => Task.FromResult<string?>(user.Email?.ToUpperInvariant());

    public Task SetNormalizedEmailAsync(Usuario user, string? normalizedEmail, CancellationToken ct)
        => Task.CompletedTask;

    public void Dispose() { /* el DbContext lo libera el contenedor de DI */ }
}
