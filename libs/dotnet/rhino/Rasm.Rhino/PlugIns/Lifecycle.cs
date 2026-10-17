using Rasm.Rhino.Commands;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns.Licenses;
using Rasm.Rhino.UI;
using Rhino.Commands;
using Rhino.PlugIns;
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

    AtomHashMap<Guid, ValueHistory> IPlugInViews.Histories => Lifetime.Histories;

    IO<HistoryLimits> IPlugInViews.Limits => Lifetime.Definition.History(Settings);

    SettingsNode IPlugInViews.Settings => new(new SettingsRoot.PlugInNode(Id));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PlugInSession(IO<Unit> flush) {
    // --- [STATE]
    private readonly Ref<Option<Held>> state = Ref(Option<Held>.None);

    private sealed record Held(Func<(Error Error, Type Owner, string Member), IO<Unit>> Post, Seq<IDisposable> Releases, HashMap<object, object> Slots);

    public static Fin<PlugInSession> Of(IPlugInSink sink) => sink is IPlugInLifetime lifetime ? lifetime.Session : new NotFound("plug-in session", $"{sink}");

    private Fin<Unit> Changed(Func<Held, Held> change) => atomic(() => state.Swap(held => held.Map(change))).ToFin(Errors.Cancelled).Map(static _ => unit);

    // --- [OPEN]
    public IO<Unit> Open<TOwner>(TOwner owner, Seq<Func<TOwner, Action<IDisposable>, IO<Unit>>> rows) =>
        DisposalOps.OnFailure(Opened(owner, rows, lease => Changed(open => open with { Releases = open.Releases.Add(lease) }).ThrowIfFail()), Released);

    private IO<Unit> Opened<TOwner>(TOwner owner, Seq<Func<TOwner, Action<IDisposable>, IO<Unit>>> rows, Action<IDisposable> retain) =>
        from opened in atomicIO(() => state.Swap(static _ => Some(new Held(static report => ErrorOps.Report(report.Error, report.Owner, report.Member), [], []))))
        from post in Subscriptions.Queued(
            Buffer<(Error Error, Type Owner, string Member)>.Unbounded,
            static report => ErrorOps.Report(report.Error, report.Owner, report.Member).OnUiThread(),
            static fault => ErrorOps.Report(fault, typeof(ErrorOps), nameof(ErrorOps.Report)).OnUiThread(), retain)
        from published in IO.lift(() => Changed(open => open with { Post = post }))
        from acquired in rows.TraverseM(row => row(owner, retain)).As()
        select unit;

    // --- [SLOTS]
    internal IO<Unit> Publish(object slot, object value) => IO.lift(() => Changed(open => open with { Slots = open.Slots.AddOrUpdate(slot, value) }));

    internal Option<object> Find(object slot) => state.Value.Bind(held => held.Slots.Find(slot));

    // --- [REPORT]
    public void Report(Error error, Type owner, string member) =>
        _ = state.Value.Match(
                Some: held => held.Post((error, owner, member)),
                None: () => ErrorOps.Report(error, owner, member))
            .RunSafe();

    // --- [CLOSE]
    public IO<Unit> Close => Callbacks.Each(Seq(Callbacks.Drained, Released, flush)).Map(unit).Uninterruptible();

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

    public IO<T> Of(IPlugInSink owner) =>
        IO.lift(() => PlugInSession.Of(owner).Bind(session => session.Find(this).Map(static value => (T)value).ToFin(Error.New($"plug-in session holds no {typeof(T).Name}"))));
}

internal sealed class PlugInLifetime(PlugInDefinition definition, Seq<View> kindViews) {
    // --- [STATE]
    private readonly Atom<ViewCatalog> views = Atom(ViewCatalog.Empty);

    public PlugInDefinition Definition => definition;

    public PlugInSession Session { get; } = new(IO.lift(PlugIn.FlushSettingsSavedQueue));

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
    public (LoadReturnCode Code, string Message) Load<TPlugIn>(TPlugIn plugIn, CallbackSite site) where TPlugIn : PlugIn, IDefinedPlugIn =>
        Callbacks.Answer(
            Loading(plugIn)
                .Map(static _ => (LoadReturnCode.Success, ""))
                .Catch(static error => error.Count is 1 && error.Is(Errors.Cancelled), static _ => IO.pure((LoadReturnCode.ErrorNoDialog, "")))
                .Catch(error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member)).Map(_ => (LoadReturnCode.ErrorShowDialog, ErrorOps.Localize(error))))
                .As(),
            static () => (LoadReturnCode.ErrorShowDialog, ""),
            site);

    private IO<Unit> Loading<TPlugIn>(TPlugIn plugIn) where TPlugIn : PlugIn, IDefinedPlugIn =>
        from initialized in IO.lift(static () => AssemblyLoading.Initialized)
        from licensed in definition.License.TraverseM(license => Licensing.Request(plugIn, license)).As()
        let collection = new ViewCatalog(definition.Views.Concat(kindViews))
        from published in views.SwapIO(_ => collection)
        from dependencies in PlugInRegistry.LoadPlugIns(definition.Dependencies)
        from registered in PlugInSettings.RegisterAll(((IPlugInViews)plugIn).Settings, [.. definition.Settings, .. HostDialogs.Suppressions(collection)])
        from opened in Session.Open(plugIn, [
            Notifications.NotificationActivated,
            ViewRegistration.RegisterPanels,
            ViewRegistration.AddCustomUISections,
            DocumentArchive.Rewrites,
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
                from chrome in Optional(command as HostCommand).TraverseM(Toolbars.RegisterCommand).As()
                select unit)),
            site);

    // --- [SHUTDOWN]
    public void Shutdown(CallbackSite site) => Callbacks.Run(Session.Close, site);
}
