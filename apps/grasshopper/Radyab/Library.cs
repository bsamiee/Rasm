using Grasshopper2;

namespace Radyab;

[Author(Name)]
[Icon("RadyabPlugin")]
[IoId(PluginId)]
public sealed class Library : Plugin {
    public const string Name = "Radyab";
    public const string Extraction = "Extraction";
    private const string PluginId = "6FBF37D9-7FB6-49D1-A7C2-668F4EB36DB1";
    public Library() : base(
        id: Guid.Parse(input: PluginId),
        nomen: new Nomen(name: Name, info: "GH2 plugin boundary for Radyab on RhinoWIP."),
        version: typeof(Library).Assembly.GetName().Version) { }
}
public sealed class Host : PlugIn { }
