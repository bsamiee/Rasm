using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using Rasm.Grasshopper;

namespace Radyab;

// --- [EXPORTS] -------------------------------------------------------------------------

public sealed class Library : Plugin {
    private static readonly Guid PluginId = Guid.Parse(input: "6fbf37d9-7fb6-49d1-a7c2-668f4eb36db1");

    public Library() : base(
        id: PluginId,
        nomen: new Nomen(name: "Radyab", info: "GH2 plugin boundary for Radyab on RhinoWIP."),
        version: typeof(Library).Assembly.GetName().Version,
        author: "Radyab",
        copyright: string.Empty) =>
        Icon = AbstractIcon.FromResource("RadyabPlugin", typeof(Library));

    public override IIcon Icon { get; }
}
