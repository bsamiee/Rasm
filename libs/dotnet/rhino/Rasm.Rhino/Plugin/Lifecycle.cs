using Rasm.Rhino.Events;
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
    private readonly Atom<Option<Held>> state = Atom(Option<Held>.None);

    private sealed record Held(Func<(Error Error, Type Owner, string Member), IO<Unit>> Post, Seq<IDisposable> Releases);

    // --- [OPEN]
    public IO<Unit> Open(Seq<IO<IDisposable>> rows) =>
        from mailbox in Subscriptions.Queued(
            Buffer<(Error Error, Type Owner, string Member)>.Unbounded,
            static report => ErrorOps.Report(report.Error, report.Owner, report.Member).Post(),
            static fault => ErrorOps.Report(fault, typeof(ErrorOps), nameof(ErrorOps.Report)).Post())
        from opened in state.SwapIO(_ => Some(new Held(mailbox.Post, [mailbox.Release])))
        from acquired in DisposalOps.OnFailure(DisposalOps.AcquireAll(rows, DisposalOps.Release), Close())
        from appended in state.SwapIO(held => held.Map(open => open with { Releases = open.Releases.Concat(acquired) }))
        select unit;

    // --- [REPORT]
    public void Report(Error error, Type owner, string member) =>
        _ = state.Value.Match(
                Some: held => held.Post((error, owner, member)),
                None: () => ErrorOps.Report(error, owner, member))
            .RunSafe();

    // --- [CLOSE]
    public IO<Unit> Close() =>
        from taken in state.ValueIO
        from cleared in state.SwapIO(static _ => Option<Held>.None)
        from released in taken.TraverseM(static held => DisposalOps.Release(held.Releases)).As()
        select unit;
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

    public IO<HistoryLimits> Limits(SettingsNode node) =>
        definition.History.Match(
            Some: rows => (PlugInSettings.Current(node, rows.Capacity), PlugInSettings.Current(node, rows.Window))
                .Apply(static (capacity, window) => new HistoryLimits(capacity, window))
                .As(),
            None: static () => IO.pure(HistoryLimits.Default));

    // --- [LOAD]
    public (LoadReturnCode Code, string Message) Load<TPlugIn>(TPlugIn plugIn, CallbackSite site) where TPlugIn : PlugIn, IPlugInViews =>
        Callbacks.Answer(
            Loading(plugIn)
                .Map(static _ => (LoadReturnCode.Success, ""))
                .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure((LoadReturnCode.ErrorNoDialog, "")))
                .Catch(error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member)).Map(_ => (LoadReturnCode.ErrorShowDialog, ErrorOps.Localize(error))))
                .As(),
            static () => (LoadReturnCode.ErrorShowDialog, ""),
            site);

    private IO<Unit> Loading<TPlugIn>(TPlugIn plugIn) where TPlugIn : PlugIn, IPlugInViews =>
        from initialized in IO.lift(static () => AssemblyLoading.Initialized)
        from licensed in definition.License.TraverseM(license => Licensing.Request(plugIn, license)).As()
        from collection in IO.lift(ViewCatalog.Of(definition.Views.Concat(kindViews)).ToFin())
        from published in views.SwapIO(_ => collection)
        from dependencies in PlugInRegistry.LoadPlugIns(definition.Dependencies)
        from registered in PlugInSettings.RegisterAll(((IPlugInViews)plugIn).Settings, [
            .. definition.Settings,
            .. HostDialogs.Suppressions(collection),
            .. definition.History.ToSeq().Bind(static rows => Seq<PlugInSetting>(rows.Capacity, rows.Window)),
        ])
        from opened in Session.Open(
            Seq(
                    Presence.NotificationActivated,
                    ViewRegistration.RegisterPanels,
                    ViewRegistration.AddCustomUISections,
                    EffectRegistry.Register(definition.Effects),
                    RenderRuns.Register,
                    static (_, sink) => Framing.Extent.Attach(sink),
                    static (_, sink) => TargetRows.Shown.Attach(sink))
                .Concat(definition.Acquisitions)
                .Map(row => row(plugIn, plugIn)))
        from endpoints in DisposalOps.OnFailure(
            IO.lift(() => definition.Endpoints.Iter(static endpoint => HostUtils.RegisterComputeEndpoint(endpoint.Path, endpoint.Type))),
            Session.Close())
        select unit;

    // --- [COMMANDS]
    public void Register(Func<Command, bool> register, CallbackSite site) =>
        _ = Callbacks.Answer(
            IO.lift(() => Callbacks.Each(
                    definition.Commands.Map(row => row(site.Sink)),
                    (command, _) => CommandRefused.Unless(register(command), command)))
                .Map(static _ => unit),
            static () => unit,
            site);

    // --- [SHUTDOWN]
    public void Shutdown(CallbackSite site) => _ = Callbacks.Answer(Session.Close(), static () => unit, site);
}
