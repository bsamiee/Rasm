using System.Reflection;
using System.Runtime.CompilerServices;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;
using Rasm.Grasshopper.Components;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Effects;

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

    // --- [LIFECYCLE]
    public static void Register(IGrasshopperPlugin plugin) => Owners.AddOrUpdate(plugin.Plugin.Assembly, plugin);

    public static Option<IIcon> Icon(IGrasshopperPlugin plugin, CallbackSite site) =>
        Callbacks.Answer(CanvasIcons.PlugIn(plugin), static () => None, site);

    public static void Harvest(IGrasshopperPlugin plugin, GrasshopperPluginDefinition definition, CallbackSite site) =>
        Callbacks.Run(from derived in IO.lift(() => {
            static Seq<(Type Type, Func<IGrasshopperPlugin, IO<Unit>> Register)> Registrations<TSelf>(ComponentRow<TSelf> row) where TSelf : ModularComponent =>
                row.Inputs.Concat(row.Outputs).Bind(static parameter => parameter.DataTypes);
            Func<ComponentRow<ModularComponent>, Seq<(Type Type, Func<IGrasshopperPlugin, IO<Unit>> Register)>> registrations = Registrations;
            MethodInfo derive = registrations.Method.GetGenericMethodDefinition();
            return toSeq((
                from type in plugin.Plugin.PluginDocumentObjectTypes.OrderBy(static type => type.FullName, StringComparer.Ordinal)
                from contract in type.GetInterfaces()
                where contract.IsConstructedGenericType && contract.GetGenericTypeDefinition() == typeof(IComponentDefinition<>)
                from getter in type.GetInterfaceMap(contract).TargetMethods
                from row in (Seq<(Type Type, Func<IGrasshopperPlugin, IO<Unit>> Register)>)derive.MakeGenericMethod(getter.ReturnType.GenericTypeArguments).Invoke(
                    null, BindingFlags.DoNotWrapExceptions, binder: null, [getter.Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, parameters: null, culture: null)], culture: null)!
                select row).DistinctBy(static row => row.Type)).Strict();
        })
                      from custom in Callbacks.Each(definition.DataTypes.Map(row => IO.pure(plugin).Bind(row)))
                      from registered in Callbacks.Each(derived.Map(row => IO.pure(plugin).Bind(row.Register)))
                      select unit, site);

    public static void Loaded(IGrasshopperPlugin plugin, GrasshopperPluginDefinition definition, CallbackSite site) =>
        _ = Callbacks.Settled(
                (from initialized in IO.lift(static () => AssemblyLoading.Initialized)
                 from opened in plugin.Session.Open(plugin, [
                     static (owner, retain) => EventKind.RhinoAppClosing.Inline(_ => owner.Session.Close, owner, retain),
                     (owner, retain) => PreviewTiles.Open(owner, definition.Clock, None, retain).Bind(tiles => PreviewTiles.Current.Publish(owner, tiles)),
                     .. definition.Acquisitions])
                 select unit)
                .Catch(error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member)).Bind(_ => IO.fail<Unit>(error)))
                .As(), CancellationToken.None)
            .ThrowIfFail();
}
