using System.Reflection;
using Grasshopper2;
using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;
using Rhino;

namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IGrasshopperPlugin {
    public Plugin Plugin { get; }

    public PluginSession Session { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GrasshopperPluginDefinition {
    public required ILoggerFactory Loggers { get; init; }
    public Option<IIcon> Icon { get; init; }
    public Seq<Func<IGrasshopperPlugin, IO<Unit>>> DataTypes { get; init; }
    public Seq<Func<IGrasshopperPlugin, IO<Unit>>> Acquisitions { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GrasshopperPlugins {
    private static readonly AtomHashMap<Assembly, IGrasshopperPlugin> Owners = AtomHashMap<Assembly, IGrasshopperPlugin>();

    // --- [DISCOVERY]
    public static IO<Unit> Load(string location) =>
        IO.lift(() => ignore(PluginServer.ScopePlugin(location)))
        >> IO.lift(Fin<Unit> () => PluginServer.LoadPlugin(location, out FailureInfo failure)
            ? unit
            : new GrasshopperFault(failure.Location, GrasshopperReason.Unloaded, Inner: Error.New($"{failure.Kind}: {failure.Reason}", failure.Exception)));

    // --- [OWNERS]
    public static IGrasshopperPlugin Owner(Type type) => Owners[type.Assembly];

    public static void Register(IGrasshopperPlugin plugin) => Owners.AddOrUpdate(plugin.Plugin.Assembly, plugin);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedGrasshopperPlugin : Plugin, IGrasshopperPlugin {
    private readonly GrasshopperPluginDefinition definition;

    protected DefinedGrasshopperPlugin(GrasshopperPluginDefinition definition) {
        this.definition = definition;
        SettingsFile = SettingsFile.InDefaultFolder(Path.GetFileNameWithoutExtension(Location));
        Session = new(definition.Loggers.CreateLogger(GetType()));
        GrasshopperPlugins.Register(this);
    }

    public SettingsFile SettingsFile { get; }

    public PluginSession Session { get; }

    public Plugin Plugin => this;

    public sealed override IIcon Icon => definition.Icon.IfNone(() => base.Icon);

    protected sealed override void HarvestCustomTypes() => Session.Run(definition.DataTypes.TraverseM(row => row(this)).As());

    public sealed override void OnLoaded() =>
        Session.Run(Session.Open(this, [
            static owner => Hooks.On(static h => RhinoApp.Closing += h, static h => RhinoApp.Closing -= h, owner.Session.Close, owner),
            .. definition.Acquisitions]));
}
