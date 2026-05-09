using Grasshopper;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;

namespace Radyab.Boundary;

// --- [PLUGIN] ----------------------------------------------------------------------------------

public sealed class RadyabLibrary : PluginBase {
    public RadyabLibrary()
        : base(
            id: new Guid("6fbf37d9-7fb6-49d1-a7c2-668f4eb36db1"),
            fallbackName: "Radyab") =>
        Icon = AbstractIcon.FromResource("RadyabPlugin", typeof(RadyabLibrary));

    public override IIcon Icon { get; }
}
