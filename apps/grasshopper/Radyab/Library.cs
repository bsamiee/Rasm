using Grasshopper;
using Grasshopper2.UI.Icon;

namespace Radyab;

// --- [EXPORTS] ----------------------------------------------------------------------------------

public sealed class Library : Plugin {
    private static readonly Guid PluginId = Guid.Parse(input: "6fbf37d9-7fb6-49d1-a7c2-668f4eb36db1");

    public Library() : base(id: PluginId, fallbackName: "Radyab") =>
        Icon = AbstractIcon.FromResource("RadyabPlugin", typeof(Library));

    public override IIcon Icon { get; }
}
