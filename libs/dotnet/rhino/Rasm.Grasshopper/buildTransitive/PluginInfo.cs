using System;
using System.IO;
using System.Runtime.Loader;
using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;

namespace Rasm.Grasshopper.Plugins;

// --- [COMPOSITION] ---------------------------------------------------------------------

public sealed class PluginInfo : Plugin {
    private readonly (Action OnLoaded, IIcon Icon) members;

    public PluginInfo() {
        _ = AssemblyLoadContext.GetLoadContext(Assembly)?.LoadFromAssemblyPath(Path.ChangeExtension(Path.ChangeExtension(Location, extension: null), Path.GetExtension(Location)));
        members = Attached(this);
    }

    public override IIcon Icon => members.Icon;

    public override void OnLoaded() => members.OnLoaded();

    private static (Action OnLoaded, IIcon Icon) Attached(Plugin host) => ((Func<Plugin, (Action OnLoaded, IIcon Icon)>)GrasshopperShell.Attach)(host);
}
