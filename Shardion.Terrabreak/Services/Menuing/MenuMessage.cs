using System.Collections.Generic;
using NetCord;
using NetCord.Rest;

namespace Shardion.Terrabreak.Services.Menuing;

public class MenuMessage(IEnumerable<IMessageComponentProperties> components)
{
    public IEnumerable<IMessageComponentProperties> Components { get; set; } = components;
    public IEnumerable<AttachmentProperties>? Attachments { get; set; }
    public MessageFlags Flags { get; set; } = 0;
    public AllowedMentionsProperties? AllowedMentions { get; set; }

    public MenuMessage WithComponents(IEnumerable<IMessageComponentProperties> components)
    {
        Components = components;
        return this;
    }

    public MenuMessage AddComponents(params IEnumerable<IMessageComponentProperties> components)
    {
        return WithComponents(System.Linq.Enumerable.Concat(Components, components));
    }
}
