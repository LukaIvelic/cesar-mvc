namespace cesar.Features.Identity;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string AdminOrManager = Admin + "," + Manager;

    public static readonly string[] All = [Admin, Manager];
}
