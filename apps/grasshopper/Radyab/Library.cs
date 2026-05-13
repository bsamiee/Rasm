using Grasshopper2;

namespace Radyab;

[Author(Library.Name)]
[Icon("RadyabPlugin")]
public sealed class Library : Plugin {
    public const string Name = "Radyab";
    public const string Extraction = "Extraction";
    private static readonly Guid PluginId = Guid.Parse(input: "6fbf37d9-7fb6-49d1-a7c2-668f4eb36db1");
    public Library() : base(
        id: PluginId,
        nomen: new Nomen(name: Name, info: "GH2 plugin boundary for Radyab on RhinoWIP."),
        version: typeof(Library).Assembly.GetName().Version) { }
}
