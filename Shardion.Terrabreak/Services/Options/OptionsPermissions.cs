namespace Shardion.Terrabreak.Services.Options;

public readonly struct OptionsPermissions
{
    public OptionsAccessibility Bot { get; init; }
    public OptionsAccessibility Servers { get; init; }
    public OptionsAccessibility Users { get; init; }

    public OptionsAccessibility AccessibilityFor(OptionsAccessor accessor)
    {
        return accessor switch
        {
            OptionsAccessor.Bot => Bot,
            OptionsAccessor.Server => Servers,
            OptionsAccessor.User => Users,
            _ => OptionsAccessibility.None
        };
    }

    public bool IsAccessible(OptionsAccessor accessor, OptionsAccessibility desiredPermission)
    {
        return AccessibilityFor(accessor) switch
        {
            OptionsAccessibility.None => false,
            OptionsAccessibility.ReadWrite => true,
            OptionsAccessibility.Read => desiredPermission == OptionsAccessibility.Read,
            OptionsAccessibility.Write => desiredPermission == OptionsAccessibility.Write,
            _ => false
        };
    }
}
