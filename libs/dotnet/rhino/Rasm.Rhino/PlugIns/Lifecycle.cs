using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Eto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rasm.RhinoCommon.Persistence.Settings;
using Rasm.RhinoCommon.PlugIns.Licenses;
using Rasm.RhinoCommon.UI;
using Rasm.RhinoCommon.UI.Surfaces;
using Rhino.Commands;
using Rhino.PlugIns;
using Rhino.Runtime;

namespace Rasm.RhinoCommon.PlugIns;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPlugInLifetime {
    public PlugInSession Session { get; }
}

internal interface IDefinedPlugIn : IPlugInViews {
    public Guid Id { get; }

    public PlugInLifetime Lifetime { get; }

    PlugInSession IPlugInLifetime.Session => Lifetime.Session;

    PlugIn IPlugInViews.PlugIn => (PlugIn)this;

    ViewCatalog IPlugInViews.Views => Lifetime.Views;

    TimeProvider IPlugInViews.Clock => Lifetime.Definition.Clock;

    Option<HelpTopic> IPlugInViews.Help => Lifetime.Definition.Help;

    Seq<DocumentFrame> IPlugInViews.Frames => Lifetime.Definition.DocumentFrames;

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PlugInSession(Seq<Action<ILoggingBuilder>> logging) {
    // --- [STATE]
    private static readonly Action<ILoggingBuilder> Window = static builder => builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CommandWindowLoggerProvider>());

    private readonly EnvIO environment = EnvIO.New(token: CancellationToken.None);
    private readonly ILoggerFactory loggers = LoggerFactory.Create(builder => (Window + logging).Iter(row => row(builder)));
    private readonly AtomHashMap<object, object> slots = AtomHashMap<object, object>();

    // --- [SCOPE]
    public IO<T> Held<T>(IO<T> effect) => effect.Under(environment);

    public IO<Unit> Open<TOwner>(TOwner owner, Seq<Func<TOwner, IO<Unit>>> rows) =>
        Held(rows.TraverseM(row => row(owner)).As().Map(unit)).Catch(error => IO.lift(environment.DisposeResources) >> IO.fail<Unit>(error)).As();

    public IO<Unit> Close => Callbacks.Drained >> IO.lift(environment.Dispose) >> IO.lift(loggers.Dispose);

    // --- [LOGS]
    public ILogger Logger(Type category) => loggers.CreateLogger(category);

    // --- [SLOTS]
    internal IO<Unit> Publish(object slot, object value) => IO.lift(() => slots.AddOrUpdate(slot, value)).Map(unit);

    internal Option<object> Find(object slot) => slots.Find(slot);
}

public sealed class PlugInSlot<T> where T : notnull {
    public IO<Unit> Publish(IPlugInLifetime owner, T value) => owner.Session.Publish(this, value);

    public IO<Option<T>> Find(IPlugInLifetime owner) => IO.lift(() => owner.Session.Find(this).Map(static value => (T)value));

    public IO<T> Of(IPlugInLifetime owner) => Find(owner).Bind(static found => IO.lift(found.ToFin(new RhinoFault(typeof(T).Name, RhinoReason.Absent))));
}

internal sealed class PlugInLifetime(PlugInDefinition definition, Seq<View> kindViews) {
    // --- [STATE]
    private readonly Atom<ViewCatalog> views = Atom(ViewCatalog.Empty);

    public PlugInDefinition Definition => definition;

    public PlugInSession Session { get; } = new(definition.Logging);

    public ViewCatalog Views => views.Value;

    // --- [READS]
    public PlugInLoadTime LoadTime(PlugInLoadTime inherited) =>
        definition.LoadTime.IfNone(() =>
            (definition.Views.Exists(static view => view is View.OptionsPage or View.DocumentPage), definition.Views.Exists(static view => view is View.Panel)) switch {
                (true, false) => PlugInLoadTime.WhenNeededOrOptionsDialog,
                (false, true) => PlugInLoadTime.WhenNeededOrTabbedDockBar,
                _ => inherited,
            });

    public IO<bool> DisplayHelp => definition.Help.Map(static topic => topic.Show.Map(value: true)).IfNone(IO.pure(value: false));

    // --- [LOAD]
    public LoadReturnCode Load<TPlugIn>(
        TPlugIn plugIn, Func<LicenseCapabilities, string?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> getLicense, ref string errorMessage, [CallerMemberName] string member = "")
        where TPlugIn : PlugIn, IDefinedPlugIn {
        (LoadReturnCode code, errorMessage) = plugIn.Answer(
            Loading(plugIn, getLicense)
                .Map(static _ => (LoadReturnCode.Success, ""))
                .Catch(ErrorOps.Cancellation, static _ => IO.pure((LoadReturnCode.ErrorNoDialog, "")))
                .Catch(error => IO.lift(() => PlugInLog.Of(plugIn).Failed(error, plugIn.GetType(), member)).Map(_ => (LoadReturnCode.ErrorShowDialog, ErrorOps.Messages(error))))
                .As(),
            failed: (LoadReturnCode.ErrorShowDialog, ""),
            member);
        return code;
    }

    private IO<Unit> Loading<TPlugIn>(TPlugIn plugIn, Func<LicenseCapabilities, string?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> getLicense) where TPlugIn : PlugIn, IDefinedPlugIn =>
        from initialized in IO.lift(static () => AssemblyLoading.Initialized)
        from licensed in definition.License.TraverseM(license => Licensing.Request(plugIn, license, getLicense)).As()
        let collection = new ViewCatalog(definition.Views.Concat(kindViews))
        from published in views.SwapIO(_ => collection)
        from dependencies in PlugInRegistry.LoadPlugIns(definition.Dependencies)
        from registered in PlugInSettings.RegisterAll(((IPlugInViews)plugIn).Settings, HostDialogs.Suppressions(collection))
        from opened in Session.Open<IPlugInViews>(plugIn, [
            Notifications.NotificationActivated,
            ViewRegistration.RegisterPanels,
            ViewRegistration.AddCustomUISections,
            Toolbars.Registered,
            DocumentArchive.Rewrites,
            .. SceneView.Opened(collection, definition.Clock),
            .. definition.Acquisitions,
            _ => IO.lift(() => definition.Endpoints.Iter(static endpoint => HostUtils.RegisterComputeEndpoint(endpoint.Path, endpoint.Type))),
        ])
        select unit;

    // --- [COMMANDS]
    public IO<Unit> Commands(Func<Command, bool> register) =>
        Callbacks.Each(definition.Commands.Map((row, index) => IO.lift(row).Bind(command => IO.lift(() => RhinoFault.Unless(register(command), index))))).Map(unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class PlugInOwners {
    internal static IPlugInViews Owner(Type type) =>
        PlugIn.Find(type.Assembly) as IPlugInViews ?? throw new UnreachableException($"{type.FullName} belongs to no defined plug-in");
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class AssemblyLoading {
    // --- [INITIALIZATION]
    public static readonly Unit Initialized = Initialize(typeof(AssemblyLoading).Assembly);

    private static Unit Initialize(Assembly copy) {
        Platform.Instance.LoadAssembly(copy);
        return (from context in Optional(AssemblyLoadContext.GetLoadContext(copy))
                from folder in Optional(Path.GetDirectoryName(copy.Location))
                from executables in Optional(Path.GetDirectoryName(Environment.ProcessPath))
                select (Context: context, Folder: folder, Frameworks: Path.Join(executables, "..", "Frameworks")))
            .Iter(static held => held.Context.ResolvingUnmanagedDll += (importer, name) =>
                string.Equals(Path.GetDirectoryName(importer.Location), held.Folder, StringComparison.Ordinal) ? Library(held.Frameworks, name) : 0);
    }

    // --- [NATIVE_LIBRARIES]
    private static nint Library(string frameworks, string name) {
        string path = Path.Join(frameworks, $"lib{name}.dylib");
        using RiskyAction risky = new(path);
        return NativeLibrary.TryLoad(path, out nint handle) ? handle : 0;
    }
}
