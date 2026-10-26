using System.Reflection;
using System.Runtime.CompilerServices;
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;
using Rasm.Grasshopper.UI.Surfaces;
using Rasm.Rhino;
using Rasm.Rhino.Display;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;

namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IGrasshopperPlugin : IPlugInLifetime {
    public Plugin Plugin { get; }

    void IPlugInSink.Report(Error error, Type owner, string member) => Session.Report(error, owner, member);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GrasshopperPluginDefinition {
    public Seq<Func<IGrasshopperPlugin, IO<Unit>>> DataTypes { get; init; }
    public required TimeProvider Clock { get; init; }
    public Seq<Action<ILoggingBuilder>> Logging { get; init; }
    public Seq<Func<IGrasshopperPlugin, Action<IDisposable>, IO<Unit>>> Acquisitions { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GrasshopperPlugins {
    private static readonly AtomHashMap<Assembly, IGrasshopperPlugin> Owners = AtomHashMap<Assembly, IGrasshopperPlugin>();

    // --- [DISCOVERY]
    public static IO<Unit> Load(string location) =>
        from scoped in IO.lift(() => PluginServer.ScopePlugin(location))
        from loaded in IO.lift(Fin<Unit> () => PluginServer.LoadPlugin(location, out FailureInfo failure)
            ? unit
            : new Expected($"{failure.Location} failed to load ({failure.Kind}), {failure.Reason}", 0, Optional(failure.Exception).Map(static exception => Error.New(exception))))
        select loaded;

    // --- [OWNERS]
    public static IGrasshopperPlugin Owner(Type type) => Owners[type.Assembly];

    public static CallbackSite Site(IDocumentObject constructed, [CallerMemberName] string member = "") =>
        new(Owner(constructed.GetType()), constructed.GetType(), member);

    // --- [ACQUISITIONS]
    public static Func<IGrasshopperPlugin, Action<IDisposable>, IO<Unit>> DisplayModes(DisplayModeSpec spec) =>
        (plugin, retain) => Rhino.Display.DisplayModes.Declared(plugin.Plugin.Id, plugin.Plugin.Assembly, spec, plugin, retain);

    // --- [LIFECYCLE]
    public static void Register(IGrasshopperPlugin plugin) => Owners.AddOrUpdate(plugin.Plugin.Assembly, plugin);

    public static Option<IIcon> Icon(IGrasshopperPlugin plugin, CallbackSite site) =>
        Callbacks.Answer(CanvasIcons.PlugIn(plugin), static () => None, site);

    public static void Harvest(IGrasshopperPlugin plugin, GrasshopperPluginDefinition definition, CallbackSite site) =>
        Callbacks.Run(
            from custom in Callbacks.Each(definition.DataTypes.Map(row => IO.pure(plugin).Bind(row)))
            select unit, site);

    public static void Loaded(IGrasshopperPlugin plugin, GrasshopperPluginDefinition definition, CallbackSite site) =>
        _ = Callbacks.Settled(
                (from initialized in IO.lift(static () => AssemblyLoading.Initialized)
                 from opened in plugin.Session.Open(plugin, [
                     static (owner, retain) => EventKind.RhinoAppClosing.Inline(_ => owner.Session.Close, owner, retain),
                     (owner, _) => Guest.Opened(owner, definition.Clock),
                     .. definition.Acquisitions])
                 select unit)
                .Catch(error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member)).Bind(_ => IO.fail<Unit>(error)))
                .As(), CancellationToken.None)
            .ThrowIfFail();
}
