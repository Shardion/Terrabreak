namespace Shardion.Terrabreak.Services.Options
{
    public readonly struct OptionsPermissions
    {
        public OptionsAccessibility Bot { get; init; }
        public OptionsAccessibility Servers { get; init; }
        public OptionsAccessibility Users { get; init; }

        public bool AccessibleTo(OptionsAccessor accessor, OptionsAccessibility accessibility) => accessor switch
        {
            OptionsAccessor.Bot => IsAccessible(Bot, accessibility),
            OptionsAccessor.Server => IsAccessible(Servers, accessibility),
            OptionsAccessor.User => IsAccessible(Users, accessibility),
            _ => false,
        };

        public static bool IsAccessible(OptionsAccessibility baseline, OptionsAccessibility check) => baseline switch
        {
            OptionsAccessibility.ReadWrite => true,
            OptionsAccessibility.Read => IsReadable(check),
            OptionsAccessibility.Write => IsWritable(check),
            _ => false,
        };

        public static bool IsReadable(OptionsAccessibility accessibility) => accessibility switch
        {
            OptionsAccessibility.Read => true,
            OptionsAccessibility.ReadWrite => true,
            _ => false,
        };

        public static bool IsWritable(OptionsAccessibility accessibility) => accessibility switch
        {
            OptionsAccessibility.Write => true,
            OptionsAccessibility.ReadWrite => true,
            _ => false,
        };
    }
}
