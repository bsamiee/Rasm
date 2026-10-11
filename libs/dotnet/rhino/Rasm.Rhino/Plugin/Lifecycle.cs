using Rasm.Rhino.Events;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Plugin.Hosting;
using Rasm.Rhino.Plugin.Licenses;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI.Chrome;
using Rasm.Rhino.UI.Views;
using Rhino.Commands;
using Rhino.PlugIns;
using Rhino.Runtime;

namespace Rasm.Rhino.Plugin;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PlugInSession {
    // --- [STATE]
    private readonly Ref<Option<Held>> state = Ref(Option<Held>.None);

    private sealed record Held(Func<(Error Error, Type Owner, string Member), IO<Unit>> Post, Seq<IDisposable> Releases);

    // --- [OPEN]
    public IO<Unit> Open(Seq<Func<Action<IDisposable>, IO<Unit>>> rows) =>
        IO.pure<Action<IDisposable>>(lease => _ = atomic(() => state.Swap(held => held.Map(open => open with { Releases = open.Releases.Add(lease) })))
                .IfNone(static () => Errors.Cancelled.Throw<Held>())).Bind(retain => DisposalOps.OnFailure(
            from opened in atomicIO(() => state.Swap(static _ => Some(new Held(static report => ErrorOps.Report(report.Error, report.Owner, report.Member), []))))
            from post in Subscriptions.Queued(
                Buffer<(Error Error, Type Owner, string Member)>.Unbounded,
                static report => ErrorOps.Report(report.Error, report.Owner, report.Member).Post(),
                static fault => ErrorOps.Report(fault, typeof(ErrorOps), nameof(ErrorOps.Report)).Post(), retain)
            from published in atomicIO(() => state.Swap(held => held.Map(open => open with { Post = post })))
                .Bind(static held => held.Match(Some: static _ => IO.pure(unit), None: static () => IO.fail<Unit>(Errors.Cancelled)))
            from acquired in rows.TraverseM(row => IO.pure(retain).Bind(row)).As()
            select unit,
            Close()));

    // --- [REPORT]
    public void Report(Error error, Type owner, string member) =>
        _ = state.Value.Match(
                Some: held => held.Post((error, owner, member)),
                None: () => ErrorOps.Report(error, owner, member))
            .RunSafe();

    // --- [CLOSE]
    public IO<Unit> Close() =>
        (from taken in atomicIO(() => {
            Option<Held> held = state.Value;
            state.Value = None;
            return held;
        })
         from released in taken.TraverseM(static held => DisposalOps.Release(held.Releases)).As()
         select unit).Uninterruptible();
}

internal sealed class PlugInLifetime(PlugInDefinition definition, Seq<View> kindViews) {
    // --- [STATE]
    private readonly Atom<ViewCatalog> views = Atom(ViewCatalog.Empty);

    public PlugInSession Session { get; } = new();

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
    public (LoadReturnCode Code, string Message) Load<TPlugIn>(TPlugIn plugIn, CallbackSite site) where TPlugIn : PlugIn, IPlugInViews =>
        Callbacks.Answer(
            Loading(plugIn)
                .Map(static _ => (LoadReturnCode.Success, ""))
                .Catch(static error => error.Count == 1 && error.Is(Errors.Cancelled), static _ => IO.pure((LoadReturnCode.ErrorNoDialog, "")))
                .Catch(error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member)).Map(_ => (LoadReturnCode.ErrorShowDialog, ErrorOps.Localize(error))))
                .As(),
            static () => (LoadReturnCode.ErrorShowDialog, ""),
            site);

    private IO<Unit> Loading<TPlugIn>(TPlugIn plugIn) where TPlugIn : PlugIn, IPlugInViews =>
        from initialized in IO.lift(static () => AssemblyLoading.Initialized)
        from licensed in definition.License.TraverseM(license => Licensing.Request(plugIn, license)).As()
        let collection = new ViewCatalog(definition.Views.Concat(kindViews))
        from published in views.SwapIO(_ => collection)
        from dependencies in PlugInRegistry.LoadPlugIns(definition.Dependencies)
        from registered in PlugInSettings.RegisterAll(((IPlugInViews)plugIn).Settings, [
            .. definition.Settings,
            .. HostDialogs.Suppressions(collection),
        ])
        from opened in DisposalOps.OnFailure(Session.Open(
            Seq(
                    Presence.NotificationActivated,
                    ViewRegistration.RegisterPanels,
                    ViewRegistration.AddCustomUISections,
                    EffectRegistry.Register(definition.Effects),
                    static (_, sink, retain) => Framing.Extent.Attach(sink, retain),
                    static (_, sink, retain) => TargetRows.Shown.Attach(sink, retain))
                .Concat(definition.Acquisitions)
                .Map(row => fun((Action<IDisposable> retain) => IO.pure(plugIn).Bind(owner => row(owner, owner, retain)))))
            .Bind(_ => IO.lift(() => definition.Endpoints.Iter(static endpoint => HostUtils.RegisterComputeEndpoint(endpoint.Path, endpoint.Type)))), Session.Close())
        select unit;

    // --- [COMMANDS]
    public void Register(Func<Command, bool> register, CallbackSite site) =>
        Callbacks.Run(
            Callbacks.Each(definition.Commands.Map(row =>
                from command in IO.lift(() => row(site.Sink))
                from registered in IO.lift(() => CommandRefused.Unless(register(command), command))
                from chrome in Optional(command as HostCommand).TraverseM(Toolbars.RegisterCommand).As()
                select unit)),
            site);

    // --- [SHUTDOWN]
    public IO<Unit> Close =>
        Callbacks.Each(Seq(Session.Close(), IO.lift(PlugIn.FlushSettingsSavedQueue))).Map(static _ => unit).Uninterruptible();

    public void Shutdown(CallbackSite site) => Callbacks.Run(Close, site);
}
