namespace Shardion.Terrabreak.Services.Options
{
    public struct OptionsPermissions
    {
        public OptionsAccessibility Bot;
        public OptionsAccessibility Servers;
        public OptionsAccessibility Users;

        public bool AccessibleTo(OptionsAccessor accessor, OptionsAccessibility accessibility) => accessor switch
        {
            OptionsAccessor.Bot => IsAccessible(Bot),
            OptionsAccessor.Server => IsAccessible(Servers),
            OptionsAccessor.User => IsAccessible(Users),
            _ => false,
        };

        public static bool IsAccessible(OptionsAccessibility accessibility) => accessibility switch
        {
            OptionsAccessibility.ReadWrite => true,
            OptionsAccessibility.Read => IsReadable(accessibility),
            OptionsAccessibility.Write => IsWritable(accessibility),
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
