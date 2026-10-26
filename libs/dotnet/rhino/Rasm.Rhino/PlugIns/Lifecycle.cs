using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns.Licenses;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Surfaces;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects.Custom;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.Runtime;

namespace Rasm.Rhino.PlugIns;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPlugInLifetime : IPlugInSink {
    public PlugInSession Session { get; }
}

internal interface IDefinedPlugIn : IPlugInViews, IPlugInLifetime {
    public Guid Id { get; }

    public PlugInLifetime Lifetime { get; }

    void IPlugInSink.Report(Error error, Type owner, string member) => Lifetime.Session.Report(error, owner, member);

    PlugInSession IPlugInLifetime.Session => Lifetime.Session;

    PlugIn IPlugInViews.PlugIn => (PlugIn)this;

    ViewCatalog IPlugInViews.Views => Lifetime.Views;

    TimeProvider IPlugInViews.Clock => Lifetime.Definition.Clock;

    Option<HelpTopic> IPlugInViews.Help => Lifetime.Definition.Help;

    AtomHashMap<Guid, ValueHistory> IPlugInViews.Histories => Lifetime.Histories;

    IO<HistoryLimits> IPlugInViews.Limits => Lifetime.Definition.History(Settings);

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PlugInSession(Seq<Action<ILoggingBuilder>> logging) {
    // --- [STATE]
    private static readonly Action<ILoggingBuilder> Host = static builder => builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, CommandWindowLoggerProvider>());
    private static readonly ILoggerFactory Window = LoggerFactory.Create(Host);

    private readonly ILoggerFactory loggers = LoggerFactory.Create(builder => (Host + logging).Iter(row => row(builder)));
    private readonly Ref<Option<Held>> state = Ref(Option<Held>.None);

    private sealed record Held(Seq<IDisposable> Releases, HashMap<object, object> Slots);

    public static Fin<PlugInSession> Of(IPlugInSink sink) => sink is IPlugInLifetime lifetime ? lifetime.Session : new NotFound("plug-in session", $"{sink}");

    private Fin<Unit> Changed(Func<Held, Held> change) => atomic(() => state.Swap(held => held.Map(change))).ToFin(Errors.Cancelled).Map(static _ => unit);

    // --- [OPEN]
    public IO<Unit> Open<TOwner>(TOwner owner, Seq<Func<TOwner, Action<IDisposable>, IO<Unit>>> rows) =>
        DisposalOps.OnFailure(Opened(owner, rows, lease => Changed(open => open with { Releases = open.Releases.Add(lease) }).ThrowIfFail()), Released);

    private IO<Unit> Opened<TOwner>(TOwner owner, Seq<Func<TOwner, Action<IDisposable>, IO<Unit>>> rows, Action<IDisposable> retain) =>
        from opened in atomicIO(() => state.Swap(static held => held || Some(new Held([], []))))
        from acquired in rows.TraverseM(row => row(owner, retain)).As()
        select unit;

    // --- [SLOTS]
    internal IO<Unit> Publish(object slot, object value) => IO.lift(() => Changed(open => open with { Slots = open.Slots.AddOrUpdate(slot, value) }));

    internal Option<object> Find(object slot) => state.Value.Bind(held => held.Slots.Find(slot));

    // --- [REPORT]
    public void Report(Error error, Type owner, string member) =>
        Try.lift(fun(() => ErrorOps.Log(Try.lift(() => loggers.CreateLogger(owner)).Run().IfFail(_ => Window.CreateLogger(owner)), error, member)))
            .Run()
            .IfFail(fault => HostUtils.ExceptionReport($"{owner.FullName}.{member}", (error + fault).ToException()));

    // --- [CLOSE]
    public IO<Unit> Close =>
        (from settled in Callbacks.Each(Seq(Callbacks.Drained, Released)).Map(unit).Catch(error => IO.lift(() => Report(error, typeof(PlugInSession), nameof(Close)))).As()
         from disposed in IO.lift(loggers.Dispose)
         select unit).Uninterruptible();

    private IO<Unit> Released =>
        (from taken in atomicIO(() => {
            Option<Held> held = state.Value;
            state.Value = None;
            return held;
        })
         from released in taken.TraverseM(static held => DisposalOps.Release(held.Releases)).As()
         select unit).Uninterruptible();
}

public sealed class PlugInSlot<T> where T : notnull {
    public IO<Unit> Publish(IPlugInSink owner, T value) => IO.lift(() => PlugInSession.Of(owner)).Bind(session => session.Publish(this, value));

    public IO<Option<T>> Find(IPlugInSink owner) =>
        IO.lift(() => PlugInSession.Of(owner).Map(session => session.Find(this).Map(static value => (T)value)));

    public IO<T> Of(IPlugInSink owner) => Find(owner).Bind(static found => IO.lift(found.ToFin(Error.New($"plug-in session holds no {typeof(T).Name}"))));
}

internal static class HostedTypes {
    // --- [STATE]
    private static readonly Seq<Type> Scanned = [
        typeof(RenderContent), typeof(PostEffect), typeof(RealtimeDisplayMode), typeof(RealtimeDisplayModeClassInfo),
        typeof(LightManagerSupport), typeof(UserData), typeof(VisualAnalysisMode), typeof(CustomObjectGrips),
    ];

    private static readonly AtomHashMap<Type, IPlugInViews> Claimed = AtomHashMap<Type, IPlugInViews>();

    // --- [CLAIMS]
    public static Seq<Type> Declared(PlugIn plugIn, ViewCatalog views) =>
        views.Rows.Bind(ViewCatalog.Tree).Filter(static view => view is View.Panel or View.Tab or View.Pane).Map(static view => view.Identity)
        + toSeq(plugIn.Assembly.GetExportedTypes()).Filter(static type => !type.IsAbstract && Scanned.Exists(type.IsSubclassOf));

    public static IO<Unit> Claim(IPlugInViews owner, Seq<Type> types, Action<IDisposable> retain) =>
        DisposalOps.Acquire(
            IO.lift(() => types.Iter(type => Claimed.AddOrUpdate(type, owner))).Map(types),
            static held => IO.lift(() => held.Iter(static type => Claimed.Remove(type))),
            retain).Map(unit);

    // --- [OWNERS]
    public static IPlugInViews Owner(Type type) => Claimed.Find(type).ToFin(new NotFound("hosted type", type.FullName ?? type.Name)).ThrowIfFail();
}

internal sealed class PlugInLifetime(PlugInDefinition definition, Seq<View> kindViews) {
    // --- [STATE]
    private readonly Atom<ViewCatalog> views = Atom(ViewCatalog.Empty);

    public PlugInDefinition Definition => definition;

    public PlugInSession Session { get; } = new(definition.Logging);

    public ViewCatalog Views => views.Value;

    public AtomHashMap<Guid, ValueHistory> Histories { get; } = AtomHashMap<Guid, ValueHistory>();

    // --- [READS]
    public PlugInLoadTime LoadTime(PlugInLoadTime inherited) =>
        definition.LoadTime.IfNone(() =>
            (definition.Views.Exists(static view => view is View.OptionsPage or View.DocumentPage), definition.Views.Exists(static view => view is View.Panel)) switch {
                (true, false) => PlugInLoadTime.WhenNeededOrOptionsDialog,
                (false, true) => PlugInLoadTime.WhenNeededOrTabbedDockBar,
                _ => inherited,
            });

    // --- [LOAD]
    public LoadReturnCode Load<TPlugIn>(TPlugIn plugIn, Func<LicenseCapabilities, string?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> getLicense, CallbackSite site, ref string errorMessage)
        where TPlugIn : PlugIn, IDefinedPlugIn {
        (LoadReturnCode code, errorMessage) = Callbacks.Answer(
            Loading(plugIn, getLicense)
                .Map(static _ => (LoadReturnCode.Success, ""))
                .Catch(ErrorOps.Cancellation, static _ => IO.pure((LoadReturnCode.ErrorNoDialog, "")))
                .Catch(error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member)).Map(_ => (LoadReturnCode.ErrorShowDialog, ErrorOps.Localize(error))))
                .As(),
            static () => (LoadReturnCode.ErrorShowDialog, ""),
            site);
        return code;
    }

    private IO<Unit> Loading<TPlugIn>(TPlugIn plugIn, Func<LicenseCapabilities, string?, ValidateProductKeyDelegate, OnLeaseChangedDelegate, bool> getLicense) where TPlugIn : PlugIn, IDefinedPlugIn =>
        from initialized in IO.lift(static () => AssemblyLoading.Initialized)
        from licensed in definition.License.TraverseM(license => Licensing.Request(plugIn, license, getLicense)).As()
        let collection = new ViewCatalog(definition.Views.Concat(kindViews))
        from published in views.SwapIO(_ => collection)
        from dependencies in PlugInRegistry.LoadPlugIns(definition.Dependencies)
        from registered in PlugInSettings.RegisterAll(((IPlugInViews)plugIn).Settings, [
            .. collection.Rows.Bind(ViewCatalog.Tree).Bind(ViewOps.Sources).Choose(static source => source.Setting).Distinct(),
            .. HostDialogs.Suppressions(collection)])
        from opened in Session.Open(plugIn, [
            (owner, retain) => HostedTypes.Claim(owner, HostedTypes.Declared(plugIn, collection), retain),
            Notifications.NotificationActivated,
            ViewRegistration.RegisterPanels,
            ViewRegistration.AddCustomUISections,
            Toolbars.Registered,
            DocumentArchive.Rewrites,
            .. SceneView.Opened(collection, definition.Clock),
            .. definition.Acquisitions,
            (_, _) => IO.lift(() => definition.Endpoints.Iter(static endpoint => HostUtils.RegisterComputeEndpoint(endpoint.Path, endpoint.Type))),
        ])
        select unit;

    // --- [COMMANDS]
    public void Register(Func<Command, bool> register, CallbackSite site) =>
        Callbacks.Run(
            Callbacks.Each(definition.Commands.Map(row =>
                from command in IO.lift(() => row(site.Sink))
                from registered in IO.lift(() => register(command) ? Fin.Succ(unit) : new Refused(nameof(register), $"refused {command.EnglishName} {command.Id}"))
                select unit)),
            site);

    // --- [HELP]
    public bool DisplayHelp(CallbackSite site) => Callbacks.Succeeded(definition.Help.Map(static topic => topic.Show), static () => false, site);

    // --- [DIALOGS]
    public void ResetMessageBoxes(IPlugInViews plugIn, CallbackSite site) => Callbacks.Run(HostDialogs.ResetMessageBoxes(Views, plugIn.Settings), site);

    // --- [SHUTDOWN]
    public void Shutdown(CallbackSite site) => Callbacks.Run(Session.Close, site);
}
