using CustomizeAuthAppJWT.Data;
using Microsoft.EntityFrameworkCore;

namespace CustomizeAuthAppJWT.Services;

public class PermissionService
{
    private readonly AppDbContext _db;
    public PermissionService(AppDbContext db) => _db = db;

    public async Task<bool> RoleHasPermissionAsync(string role, string permissionName)
    {
        return await _db.RolePermissions.AnyAsync(rp => rp.Role == role && rp.PermissionName == permissionName);
    }
}