using System.Reflection;
using System.Runtime.CompilerServices;
using Grasshopper2;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;
using Rasm.Grasshopper.Components;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Styling;

namespace Rasm.Grasshopper;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GrasshopperPluginDefinition {
    public Option<(IGlyph Glyph, int Master)> Mark { get; init; }
    public Seq<Func<DefinedGrasshopperPlugin, IO<Unit>>> DataTypes { get; init; }
    public required TimeProvider Clock { get; init; }
    public Seq<Action<ILoggingBuilder>> Logging { get; init; }
    public Seq<Func<DefinedGrasshopperPlugin, Action<IDisposable>, IO<Unit>>> Acquisitions { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GrasshopperPlugins {
    public static IO<Unit> Load(string location) =>
        from scoped in IO.lift(() => PluginServer.ScopePlugin(location))
        from loaded in IO.lift(Fin<Unit> () => PluginServer.LoadPlugin(location, out FailureInfo failure)
            ? unit
            : new Expected($"{failure.Location} failed to load with {failure.Kind}: {failure.Reason}", 0, Optional(failure.Exception).Map(static exception => Error.New(exception))))
        select loaded;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedGrasshopperPlugin : Plugin, IPlugInLifetime {
    // --- [STATE]
    private readonly GrasshopperPluginDefinition definition;

    protected DefinedGrasshopperPlugin(GrasshopperPluginDefinition definition) {
        this.definition = definition;
        SettingsFile = SettingsFile.InDefaultFolder(Path.GetFileNameWithoutExtension(Location));
        Session = new(IO.lift(() => Refused.Unless(SettingsFile.TrySaveSettingsToFile(), nameof(SettingsFile.TrySaveSettingsToFile))), definition.Logging);
    }

    public SettingsFile SettingsFile { get; }

    public PlugInSession Session { get; }

    // --- [IDENTITY]
    public sealed override IIcon Icon =>
        Callbacks.Answer(
            definition.Mark.Map(mark => CanvasIcons.Vector(this, mark.Glyph, mark.Master)),
            () => base.Icon,
            () => base.Icon,
            CallbackSite.Of(this));

    // --- [HARVEST]
    protected sealed override void HarvestCustomTypes() =>
        Callbacks.Run(from derived in IO.lift(() => {
            static Seq<(Type Type, Func<DefinedGrasshopperPlugin, IO<Unit>> Register)> Registrations<TSelf>(ComponentRow<TSelf> row) where TSelf : ModularComponent =>
                row.Inputs.Concat(row.Outputs).Bind(static parameter => parameter.DataTypes);
            Func<ComponentRow<ModularComponent>, Seq<(Type Type, Func<DefinedGrasshopperPlugin, IO<Unit>> Register)>> registrations = Registrations;
            MethodInfo derive = registrations.Method.GetGenericMethodDefinition();
            return toSeq((
                from type in PluginDocumentObjectTypes.OrderBy(static type => type.FullName, StringComparer.Ordinal)
                from contract in type.GetInterfaces()
                where contract.IsConstructedGenericType && contract.GetGenericTypeDefinition() == typeof(IComponentDefinition<>)
                from getter in type.GetInterfaceMap(contract).TargetMethods
                from row in (Seq<(Type Type, Func<DefinedGrasshopperPlugin, IO<Unit>> Register)>)derive.MakeGenericMethod(getter.ReturnType.GenericTypeArguments).Invoke(
                    null, BindingFlags.DoNotWrapExceptions, binder: null, [getter.Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, parameters: null, culture: null)], culture: null)!
                select row).DistinctBy(static row => row.Type)).Strict();
        })
                      from custom in Callbacks.Each(definition.DataTypes.Map(row => IO.pure(this).Bind(row)))
                      from registered in Callbacks.Each(derived.Map(row => IO.pure(this).Bind(row.Register)))
                      select unit, CallbackSite.Of(this));

    // --- [LOAD]
    public sealed override void OnLoaded() =>
        _ = Callbacks.Settled(
                (from initialized in IO.lift(static () => AssemblyLoading.Initialized)
                 from opened in Session.Open(this, [
                     static (owner, retain) => EventKind.RhinoAppClosing.Inline(_ => owner.Session.Close, owner, retain),
                     static (owner, retain) => PreviewTiles.Open(owner, owner.definition.Clock, None, retain).Bind(tiles => PreviewTiles.Current.Publish(owner, tiles)),
                     .. definition.Acquisitions])
                 select unit)
                .Catch(error => IO.lift(() => Report(error, GetType(), nameof(OnLoaded))).Bind(_ => IO.fail<Unit>(error)))
                .As(), CancellationToken.None)
            .ThrowIfFail();

    // --- [SINK]
    public void Report(Error error, Type owner, string member) => Session.Report(error, owner, member);

    public static CallbackSite Site(IDocumentObject constructed, [CallerMemberName] string member = "") =>
        new((IPlugInSink)PluginServer.FindPluginForObject(constructed), constructed.GetType(), member);
}
