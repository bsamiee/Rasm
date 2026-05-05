using System.Reflection;
using Grasshopper2.Framework;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;

namespace Rasm.Grasshopper.Boundary;

// --- [PLUGIN] --------------------------------------------------------------------------

public sealed class RasmGrasshopperLibrary : Plugin {
    private static readonly Assembly SourceAssembly = typeof(RasmGrasshopperLibrary).Assembly;

    public RasmGrasshopperLibrary()
        : base(
            new Guid("3561519e-4266-4865-94ee-06f5fa20861b"),
            new Nomen(
                SourceAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Rasm Grasshopper",
                SourceAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty),
            SourceAssembly.GetName().Version) =>
        Icon = AbstractIcon.FromResource("RasmGrasshopperPlugin", typeof(RasmGrasshopperLibrary));

    public override string Author =>
        SourceAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Rasm";

    public override IIcon Icon { get; }

    public override string Copyright =>
        SourceAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? base.Copyright;
}
