public static class UserRole
{
    public const string SuperAdmin = "SUPER_ADMIN";
    public const string Admin = "ADMIN";
    public const string DataEntry = "DATA_ENTRY";
    public const string Student = "STUDENT";

    // Role Priority Mapping
    public static readonly Dictionary<string, int> RolePriority = new()
    {
        { SuperAdmin, 1 },
        { Admin, 2 },
        { DataEntry, 3 },
        { Student, 4 }
    };

    public static int GetRolePriority(string role)
    {
        return RolePriority.TryGetValue(role, out var priority) ? priority : int.MaxValue;
    }
}