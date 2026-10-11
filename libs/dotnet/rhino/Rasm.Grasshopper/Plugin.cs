using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Grasshopper2;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Framework;
using Grasshopper2.UI.Icon;
using Rasm.Grasshopper.Authoring;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.Plugin;
using Rasm.Rhino.Plugin.Hosting;
using Rasm.Rhino.UI.Assets;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Conversions))]

namespace Rasm.Grasshopper;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GrasshopperPluginDefinition {
    public Option<(IGlyph Glyph, int Master)> Mark { get; init; }
    public Seq<Func<DefinedGrasshopperPlugin, IO<Unit>>> DataTypes { get; init; }
    public Seq<Func<DefinedGrasshopperPlugin, Action<IDisposable>, IO<Unit>>> Acquisitions { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GrasshopperPlugins {
    public static IO<Unit> Load(string location) =>
        from scoped in IO.lift(() => PluginServer.ScopePlugin(location))
        from loaded in IO.lift(() => PluginRefused.Unless(PluginServer.LoadPlugin(location, out FailureInfo failure), failure))
        select loaded;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class DefinedGrasshopperPlugin : Plugin, IPlugInLifetime {
    // --- [STATE]
    private readonly GrasshopperPluginDefinition definition;
    private readonly PlugInSession session = new();

    protected DefinedGrasshopperPlugin(GrasshopperPluginDefinition definition) {
        this.definition = definition;
        SettingsFile = SettingsFile.InDefaultFolder(Path.GetFileNameWithoutExtension(Location));
    }

    public SettingsFile SettingsFile { get; }

    // --- [IDENTITY]
    public sealed override IIcon Icon =>
        Callbacks.Answer(
            definition.Mark.Map(mark => CanvasIcons.Vector(this, mark.Glyph, mark.Master)),
            () => base.Icon,
            () => base.Icon,
            new CallbackSite(this, GetType(), nameof(Icon)));

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
                from row in Expression.Lambda<Func<Seq<(Type Type, Func<DefinedGrasshopperPlugin, IO<Unit>> Register)>>>(
                    Expression.Call(derive.MakeGenericMethod(getter.ReturnType.GenericTypeArguments), Expression.Call(getter))).Compile()()
                select row).DistinctBy(static row => row.Type)).Strict();
        })
                      from custom in Callbacks.Each(definition.DataTypes.Map(row => IO.pure(this).Bind(row)))
                      from registered in Callbacks.Each(derived.Map(row => IO.pure(this).Bind(row.Register)))
                      select unit, new CallbackSite(this, GetType(), nameof(HarvestCustomTypes)));

    // --- [LOAD]
    public sealed override void OnLoaded() =>
        _ = DisposalOps.OnFailure(
            from initialized in IO.lift(static () => AssemblyLoading.Initialized)
            from opened in session.Open(fun((Action<IDisposable> retain) => EventKind.RhinoAppClosing.Inline(_ => Close, this, retain))
                .Cons(definition.Acquisitions.Map(row => fun((Action<IDisposable> retain) => IO.pure(this).Bind(owner => row(owner, retain))))))
            select unit,
            session.Close())
            .Catch(error => IO.lift(() => Report(error, GetType(), nameof(OnLoaded))).Bind(_ => IO.fail<Unit>(error)))
            .As()
            .Run();

    // --- [CLOSE]
    public IO<Unit> Close =>
        Callbacks.Each(Seq(session.Close(), IO.lift(() => Refused.Unless(SettingsFile.TrySaveSettingsToFile(), nameof(SettingsFile.TrySaveSettingsToFile)))))
            .Map(static _ => unit).Uninterruptible();

    // --- [SINK]
    public void Report(Error error, Type owner, string member) => session.Report(error, owner, member);

    public static CallbackSite Site(IDocumentObject constructed, [CallerMemberName] string member = "") =>
        new((IPlugInSink)PluginServer.FindPluginForObject(constructed), constructed.GetType(), member);
}
