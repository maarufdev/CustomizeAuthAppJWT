namespace CustomizeAuthAppJWT.Models;

public class RolePermission
{
    public int Id { get; set; }
    public required string Role { get; set; }
    public required string PermissionName { get; set; }
}