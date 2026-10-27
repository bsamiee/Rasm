using System.Reflection;
using Grasshopper2.Framework;
using Rasm.Grasshopper.UI.Surfaces;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Display;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;

namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IGrasshopperPlugin : IPlugInLifetime {
    public Plugin Plugin { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GrasshopperPluginDefinition {
    public Seq<Func<IGrasshopperPlugin, IO<Unit>>> DataTypes { get; init; }
    public required TimeProvider Clock { get; init; }
    public Seq<Action<ILoggingBuilder>> Logging { get; init; }
    public Seq<Func<IGrasshopperPlugin, IO<Unit>>> Acquisitions { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GrasshopperPlugins {
    private static readonly AtomHashMap<Assembly, IGrasshopperPlugin> Owners = AtomHashMap<Assembly, IGrasshopperPlugin>();

    // --- [DISCOVERY]
    public static IO<Unit> Load(string location) =>
        from scoped in IO.lift(() => PluginServer.ScopePlugin(location))
        from loaded in IO.lift(Fin<Unit> () => PluginServer.LoadPlugin(location, out FailureInfo failure)
            ? unit
            : new GrasshopperFault(failure.Location, GrasshopperReason.Unloaded, Inner: Error.New($"{failure.Kind}: {failure.Reason}", failure.Exception)))
        select loaded;

    // --- [OWNERS]
    public static Fin<IGrasshopperPlugin> Owner(Type type) => Conversions.Found(Owners.Find(type.Assembly));

    public static void Register(IGrasshopperPlugin plugin) => Owners.AddOrUpdate(plugin.Plugin.Assembly, plugin);

    // --- [ACQUISITIONS]
    public static Func<IGrasshopperPlugin, IO<Unit>> Modes(DisplayModeSpec spec) =>
        plugin => DisplayModes.Declared(plugin.Plugin.Id, plugin.Plugin.Assembly, spec, plugin);

    // --- [LIFECYCLE]
    public static IO<Unit> Harvest(IGrasshopperPlugin plugin, GrasshopperPluginDefinition definition) =>
        Callbacks.Each(definition.DataTypes.Map(row => row(plugin))).Map(unit);

    public static IO<Unit> Loaded(IGrasshopperPlugin plugin, GrasshopperPluginDefinition definition) =>
        from initialized in IO.lift(static () => AssemblyLoading.Initialized)
        from opened in plugin.Session.Open<IGrasshopperPlugin>(plugin, [
            static owner => EventKind.RhinoAppClosing.Inline(_ => owner.Session.Close, owner),
            owner => Guest.Opened(owner, definition.Clock),
            .. definition.Acquisitions])
        select unit;
}
