using System.Reflection;
using Grasshopper2.Framework;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;

namespace Radyab.Boundary;

// --- [PLUGIN] --------------------------------------------------------------------------

public sealed class RadyabLibrary : Plugin {
    private static readonly Assembly SourceAssembly = typeof(RadyabLibrary).Assembly;

    public RadyabLibrary()
        : base(
            new Guid("6fbf37d9-7fb6-49d1-a7c2-668f4eb36db1"),
            new Nomen(
                SourceAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Radyab",
                SourceAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty),
            SourceAssembly.GetName().Version) =>
        Icon = AbstractIcon.FromResource("RadyabPlugin", typeof(RadyabLibrary));

    public override string Author =>
        SourceAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Radyab";

    public override IIcon Icon { get; }

    public override string Copyright =>
        SourceAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? base.Copyright;
}
