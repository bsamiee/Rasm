using System.Runtime.InteropServices;
using Eto;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Queue;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.UI;
using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.Render;
using UnitsNet;
using UnitsNet.Units;

namespace Darkroom;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("5241D43D-4825-41EC-B908-468DFF1FF424")]
public sealed class RenderQueueSection(IPlugInViews owner) : DefinedSection(Row, owner) {
    // --- [SETTINGS]
    public static readonly QueueSettingRows Settings = new(
        Retries: new(ValueKey.Numeric("QueueRetries", "Retries", "Rhino restarts to resume crashed queue", QueueSettings.Default.Retries,
            NumericDomain.Bounded<RetryCount, int>(new() { Form = NumberForm.Field })), SettingType.Integer),
        Delay: new(ValueKey.Numeric("QueueRetryDelay", "Retry delay", "Wait before each restart", QueueSettings.Default.Delay,
            NumericDomain.Bounded<RetryDelay, int>(RetryDelay.Presentation)), SettingType.Integer),
        SampleEstimate: new(ValueKey.Raw("QueueSampleEstimate", "Estimate from first frames",
            "Renders 2 frames per entry first to post estimated finish", QueueSettings.Default.SampleEstimate), SettingType.Bool),
        KeepAwake: new(ValueKey.Raw("KeepAwake", "Keep computer awake", "Prevents system sleep while queue renders", QueueSettings.Default.KeepAwake), SettingType.Bool),
        ShutDownAfterSuccess: new(ValueKey.Raw("ShutDownAfterSuccess", "Shut down when done", "Shuts computer down 60 seconds after queue succeeds",
            QueueSettings.Default.ShutDownAfterSuccess), SettingType.Bool),
        HeartbeatInterval: new(ValueKey.Numeric("QueueHeartbeatInterval", "Heartbeat interval", "Time between heartbeat writes to queue record", QueueHeartbeat.Default.Interval,
            new NumericDomain<Duration, double>(static seconds => Duration.FromSeconds(seconds), static value => value.TotalSeconds,
                (QueueHeartbeat.Intervals.Low.TotalSeconds, QueueHeartbeat.Intervals.High.TotalSeconds), new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Second) })),
            SettingType.Double),
        HeartbeatMisses: new(ValueKey.Numeric("QueueHeartbeatMisses", "Missed heartbeats", "Missed intervals before queue counts as crashed", QueueHeartbeat.Default.Misses,
            new NumericDomain<int, int>(Fin.Succ, identity, QueueHeartbeat.MissedIntervals, new() { Form = NumberForm.Field })), SettingType.Integer),
        Server: new(ValueKey.Of<HttpServer, string>("NtfyServer", "Server", "ntfy server for queue notices", NoticeSettings.Default.Server), SettingType.String),
        Topic: new(ValueKey.Optional<NoticeTopic>("NtfyTopic", "Topic", "ntfy topic for queue notices, empty sends none"), SettingType.String),
        Flamenco: new(
            Manager: new(ValueKey.Of<HttpServer, string>("FlamencoManager", "Manager", "Flamenco Manager address", HttpServer.Create("http://localhost:8080")), SettingType.String),
            Scripts: new(new ValueKey<Option<string>, string>("FlamencoScripts", "Scripts folder", "Manager scripts folder for Rhino job compiler, empty for none", None,
                new StateParameter<Option<string>>.Opaque<Option<string>>(Lens.identity<Option<string>>()), static text => Conversions.Present(text), Conversions.Unset,
                static text => Conversions.Present(text)), SettingType.String),
            WorkerPlatform: new(new ValueKey<OSPlatform, string>("FlamencoWorkerPlatform", "Worker platform", "Rhino worker operating system, mixed farms need matching worker tag",
                Default: OperatingSystem.IsWindows() ? OSPlatform.Windows : OSPlatform.OSX,
                new StateParameter<OSPlatform>.Opaque<OSPlatform>(Lens.identity<OSPlatform>()), WorkerPlatform, static value => value.ToString(), WorkerPlatform), SettingType.String),
            Priority: new(ValueKey.Numeric("FlamencoPriority", "Priority", "Priority of submitted Flamenco jobs", 50,
                new NumericDomain<int, int>(Fin.Succ, identity, (1, 100), new() { Form = NumberForm.Field })), SettingType.Integer),
            WorkerTag: new(new ValueKey<Option<Guid>, string>("FlamencoWorkerTag", "Worker tag", "Tag selecting workers of chosen platform, empty for any Rhino worker", None,
                new StateParameter<Option<Guid>>.Opaque<Option<Guid>>(Lens.identity<Option<Guid>>()), WorkerTag, static value => Conversions.Unset(value.Map(StoredText.Format)), WorkerTag),
                SettingType.String)));

    public static Seq<Child> Page => [
        new ControlRow.Group("Retries and power", RowRules.Always),
        .. SectionRows.Setting(Settings.Retries, RowRules.Always),
        .. SectionRows.Setting(Settings.Delay, RowRules.Always),
        .. SectionRows.Setting(Settings.SampleEstimate, RowRules.Always),
        .. SectionRows.Setting(Settings.KeepAwake, RowRules.Always),
        .. SectionRows.Setting(Settings.ShutDownAfterSuccess, RowRules.Always),
        new ControlRow.Group("Heartbeat", RowRules.Always),
        .. SectionRows.Setting(Settings.HeartbeatInterval, RowRules.Always),
        .. SectionRows.Setting(Settings.HeartbeatMisses, RowRules.Always),
        new ControlRow.Group("Notices", RowRules.Always),
        .. SectionRows.Setting(Settings.Server, RowRules.Always),
        .. SectionRows.Setting(Settings.Topic, RowRules.Always),
        new ControlRow.Group("Flamenco", RowRules.Always),
        .. SectionRows.Setting(Settings.Flamenco.Manager, RowRules.Always),
        Text(Settings.Flamenco.Scripts, Some((FileAction.SelectFolder, Seq<FileTypeRow>()))),
        SectionRows.Setting(Settings.Flamenco.WorkerPlatform) switch {
            var pair => ChoiceRows.Choice(pair.Source, pair.Field, Lens.identity<OSPlatform>(), static _ => IO.pure(
                Seq(OSPlatform.OSX, OSPlatform.Windows).Map(static platform => new ChoiceItem<OSPlatform>(platform, platform.ToString(), None))), RowRules.Always),
        },
        .. SectionRows.Setting(Settings.Flamenco.Priority, RowRules.Always),
        Text(Settings.Flamenco.WorkerTag, None),
        SectionRows.Pushed(Command(nameof(Dispatch.InstallFlamencoCompiler), "Install Flamenco job type", "Write Rhino job compiler to Manager scripts folder",
            SectionRows.Setting(Settings.Flamenco.Scripts) switch { var folder => Some(RowRule.When(folder.Source, static held => held.IsSome, folder.Field.Parameter)) },
            static scope => IO.lift(() => ((IPlugInViews)scope.Sink).Settings.Current(Settings.Flamenco.Scripts)
                    .Bind(static held => held.ToFin(new Refused(Settings.Flamenco.Scripts.Value.Name, Refused.Absent))))
                .Bind(Dispatch.InstallFlamencoCompiler))),
    ];

    private static ControlRow Text<TValue>(PlugInSetting<TValue, string> row, Option<(FileAction Action, Seq<FileTypeRow> Types)> browse) where TValue : notnull =>
        SectionRows.Setting(row) switch {
            var pair => TextRows.Typed(pair.Source, pair.Field, row.Value.Text, (_, text) => row.Value.Read(text), new EntryForm.SingleLine(), browse, RowRules.Always),
        };

    private static Fin<OSPlatform> WorkerPlatform(string text) =>
        Conversions.Present(text).Map(OSPlatform.Create).ToFin(Error.New(ErrorCodes.ValidationFailed, "Worker platform is empty"));

    private static Fin<Option<Guid>> WorkerTag(string text) => Conversions.Present(text).Traverse(StoredText.Parse<Guid>).As();

    // --- [SOURCES]
    private const string Owner = "render-queue";

    private static readonly Func<Destination, IO<QueueTarget>> Local = static _ => IO.pure<QueueTarget>(new QueueTarget.Local());

    private static readonly Func<QueueContext, IO<Option<QueueProgress>>> Fresh = static _ => IO.pure(Option<QueueProgress>.None);

    private static readonly (RowSource<Seq<(QueuedEntry Entry, int Index)>> Source, RowField<Seq<(QueuedEntry Entry, int Index)>> Field) Listed =
        RowSource.Opaque(Owner, "entries", Seq<(QueuedEntry Entry, int Index)>(), static scope => ValueStore.Of(
            scope.DocumentIO.Bind(Queues.Book).Map(static book => Some(book.Entries.Map(static (entry, index) => (Entry: entry, Index: index)))),
            rows =>
                from doc in scope.DocumentIO
                from changed in Queues.Change(doc, RowText.Localize("Edit Entries"), _ => new QueueBook(rows.IfNone([]).Map(static row => row.Entry)))
                select unit,
            Applied.Live,
            RenderSettingsSources.DictionaryChanged(scope)));

    private static readonly (RowSource<LanguageExt.HashSet<int>> Source, RowField<LanguageExt.HashSet<int>> Field) Picked =
        RowSource.Opaque(Owner, "picked", LanguageExt.HashSet<int>.Empty);

    private static readonly (RowSource<bool> Source, RowField<bool> Field) Clayed = RowSource.Opaque(Owner, "clay", @default: false);

    private static readonly (RowSource<Option<Preflight>> Source, RowField<Option<Preflight>> Field) Report = RowSource.Opaque(Owner, "check", Option<Preflight>.None);

    private static readonly HostEvent<Unit> Estimated = new(typeof(QueueContext), nameof(QueueContext.Estimate), static (deliver, site, retain) =>
        QueueContext.Current.Of(site.Sink).Bind(context => Subscriptions.Changed(context.Estimate).Attach(_ => deliver(unit), site, retain)));

    private static readonly (RowSource<RunState> Source, RowField<RunState> Field) Observed =
        RowSource.Opaque(Owner, "run", new RunState.Absent(), static scope => ValueStore.Readout(
            QueueContext.Current.Of(scope.Sink).Bind(QueueProgress.Observed).Map(static observed => observed.State), Some(Estimated)));

    private static readonly ReadoutSource<(QueueEstimate Estimate, TimeProvider Clock, DateTimeZone Zone)> Zoned =
        new ReadoutSource<(QueueEstimate Estimate, TimeProvider Clock, DateTimeZone Zone)>.Scoped(static scope =>
            from context in QueueContext.Current.Of(scope.Sink)
            from estimate in context.Estimate.ValueIO
            from zone in IO.lift(() => context.Sink.Clock.ToZonedClock().Zone)
            select (estimate, context.Sink.Clock, zone));

    // --- [PANEL]
    public static readonly View.Section Row = new() {
        Identity = typeof(RenderQueueSection),
        Caption = "Render Queue",
        Children = Rows(),
    };

    private static Seq<Child> Rows() {
        ReadoutSource<Option<Preflight>> check = new ReadoutSource<Option<Preflight>>.Scoped(static scope => scope.Read(Report.Source));
        RowRules ran = new(None, Some(RowRule.When(Observed.Source, static current => current is not RunState.Absent)));
        RowRules reported = new(None, Some(RowRule.When(Report.Source, static report => report.IsSome)));
        RowRule ready = new([], [], static scope => scope.Read(Observed.Source).Map(static current => current.Map(absent: true, active: false, ended: true, crashed: true)));
        RowRule resumable = new([], [], static scope => scope.Read(Observed.Source).Map(static current => current.Switch(
            absent: static _ => false, active: static _ => false, ended: static ended => ended.End != RunEnd.Succeeded, crashed: static _ => true)));
        const string bytes = "{0:N0} bytes";
        Seq<(string Caption, string Format, Func<Preflight, Option<object>> Value)> measures = [
            ("Objects", "{0:N0}", static report => Some<object>(report.Statistics.ObjectCount)),
            ("Layer levels", "{0:N0}", static report => Some<object>(report.Statistics.LayerLevelCount)),
            ("Block placements", "{0:N0}", static report => Some<object>(report.Statistics.PlacementCount)),
            ("Geometry memory", bytes, static report => Some<object>(report.Statistics.MemoryEstimate)),
            ("File size", bytes, static report => report.Statistics.FileLength.Map(static length => (object)length)),
        ];
        return [
            ListRows.List("Entries", "Queued renders of document, in add order", new ListRow<(QueuedEntry Entry, int Index), int>(
                    IterableNE.create<ListColumn<(QueuedEntry Entry, int Index)>>(
                        new ListColumn<(QueuedEntry Entry, int Index)>.Check(Some("Render"), Lens<(QueuedEntry Entry, int Index), bool>.New(
                            static row => row.Entry.Renders, static renders => row => row with { Entry = row.Entry with { Renders = renders } })),
                        new ListColumn<(QueuedEntry Entry, int Index)>.Label(Some("Entry"), static row => row.Entry.Name.IfNone(static () => RowText.Localize("Active viewport")), None, None),
                        new ListColumn<(QueuedEntry Entry, int Index)>.Label(Some("Frames"), static row => row.Entry.Frames.Count > 1
                            ? RowText.Localize("{0} to {1}", arguments: [(int)row.Entry.Frames.First, (int)row.Entry.Frames.Last])
                            : ((int)row.Entry.Frames.First).ToString(RowText.Culture), None, None),
                        new ListColumn<(QueuedEntry Entry, int Index)>.Label(Some("Size"), static row => RowText.Localize("{0} × {1}", arguments: [row.Entry.Extent.Width, row.Entry.Extent.Height]), None, None)),
                    static row => row.Index,
                    Listed,
                    Picked) {
                AllowMultipleSelection = true,
                Empty = Some("No renders queued"),
                Pair = Some((
                    Command("render-queue-add-view", "Add Active Viewport", "Queue active viewport at document render size", None,
                        static scope => Added(scope, static _ => IO.pure(Seq(Placement.Active)))),
                    Command("render-queue-remove", "Remove Entries", "Remove selected entries", None, static scope =>
                        from doc in scope.DocumentIO
                        from picked in scope.Read(Picked.Source)
                        from removed in Queues.Change(doc, RowText.Localize("Remove Entries"), was => was.Removed(picked))
                        from cleared in scope.Store(Picked.Source).Put(Some(LanguageExt.HashSet<int>.Empty))
                        select unit))),
                Commands = _ => [
                    Command("render-queue-add-named-views", "Add Named Views", "Queue every named view", None,
                        static scope => Added(scope, static doc => NamedTable.Views.Entries(doc).Map(static entries => entries.Map(static entry => Placement.Named(entry.Name))))),
                    Command("render-queue-add-snapshots", "Add Snapshots", "Queue every snapshot", None,
                        static scope => Added(scope, static doc => NamedTable.Snapshots.Entries(doc).Map(static entries => entries.Map(static entry => Placement.Snapshot(entry.Name))))),
                    Command("render-queue-add-study", "Add Sun Study", "Queue sun study, one frame per moment", None, static scope => Added(scope, static doc =>
                        DisposalOps.Within(() => doc.AnimationProperties, static copy => IO.lift(() => SunWindow.Read(copy)))
                            .Bind(static study => IO.lift(study.ToFin(new Refused(nameof(RhinoDoc.AnimationProperties), Refused.Absent))))
                            .Map(static window => Seq(Placement.Active with { Sun = Some(window) })))),
                    CommandRows.Toggle(new CommandFace("render-queue-clay", "Clay", Some("Render entries added next with every object in clay"), None), None,
                        RowRule.When(Clayed.Source, static on => on, Clayed.Field.Parameter), static (scope, on) => scope.Store(Clayed.Source).Put(Some(on)).Map(unit)),
                    Command("render-queue-check", "Check", "Check queue for refusals and cautions", None, static scope =>
                        from doc in scope.DocumentIO
                        from context in QueueContext.Current.Of(scope.Sink)
                        from plan in Queues.Plan(context, doc)
                        from report in Preflight.Check(context, doc, plan, new QueueTarget.Local())
                        from shown in Reported(scope)(report)
                        select shown),
                    Command("render-queue-render", "Render", "Render queue in running Rhino", Some(ready), static scope => Sent(scope, Local, Fresh)),
                    Command("render-queue-handoff", "Render in New Rhino", "Save, quit, then render queue in new Rhino behind frontmost window", Some(ready),
                        static scope => Sent(scope, Packaged(static package => IO.pure<QueueTarget>(new QueueTarget.Handoff(package))), Fresh)),
                    Command("render-queue-job", "Write Job", "Write document copy, job, and job schema to output jobs folder", Some(ready),
                        static scope => Sent(scope, Packaged(static package => IO.pure<QueueTarget>(new QueueTarget.JobFile(package))), Fresh)),
                    Command("render-queue-flamenco", "Submit to Flamenco", "Package queue and submit to Flamenco Manager", Some(ready),
                        static scope => Sent(scope, Packaged(package => Settings.Flamenco.Target(((IPlugInViews)scope.Sink).Settings, package)), Fresh)),
                    Command("render-queue-resume", "Resume", "Render stopped, failed, or crashed queue from last finished frame", Some(resumable),
                        static scope => Sent(scope, Local, static context => QueueProgress.Read(context.Record).Map(static prior => prior.Filter(static record => !record.Succeeded)))),
                    Command("render-queue-test-notice", "Send Test Notice", "Post test notice to Rhino, system, and ntfy topic", None, static scope =>
                        QueueContext.Current.Of(scope.Sink)
                            .Bind(static context => DisposalOps.Fork(context.Notify(new QueueEvent.Test()), context.Retain))
                            .Map(unit)),
                ],
            },
                RowRules.Always),
            new ControlRow.Group("Run", ran),
            TextRows.Progress("Progress", "Frames rendered of queue total", new ReadoutSource<Content<Unit>>.Scoped(static scope =>
                QueueContext.Current.Of(scope.Sink).Bind(static context => context.Estimate.ValueIO).Map(static estimate => estimate.Work)), ran),
            TextRows.Readout("State", "Queue record state", new ReadoutSource<(RunState State, DateTimeZone Zone)>.Scoped(static scope =>
                from current in scope.Read(Observed.Source)
                from zone in IO.lift(() => ((IPlugInViews)scope.Sink).Clock.ToZonedClock().Zone)
                select (current, zone)), static shown => Stated(shown.State, shown.Zone), ran),
            TextRows.Readout("Remaining", "Time left and finish time, from rendered frames", Zoned, static shown => shown.Estimate.Description(shown.Zone), ran),
            .. toSeq(FrameStage.Items).Map(stage => TextRows.Readout(stage.Caption, "Time last finished frame spent in stage", Zoned,
                shown => shown.Estimate.Last.Map<string>(mark => RowText.Localize("{0:N3} s", arguments: [stage.Of(mark, shown.Clock).TotalSeconds])).IfNone(""), ran)),
            TextRows.Readout("Peak memory", "Peak process memory at last finished frame", Zoned,
                static shown => shown.Estimate.Last.Map<string>(static mark => RowText.Localize(bytes, arguments: [mark.PeakWorkingSet])).IfNone(""), ran),
            new ControlRow.Group("Check", reported),
            .. measures.Map(measure => TextRows.Readout(measure.Caption, "From last queue check", check,
                report => report.Bind(measure.Value).Map<string>(value => RowText.Localize(measure.Format, arguments: [value])).IfNone(""), reported)),
            ListRows.List("Estimated memory", "Scene and render channel memory per entry, from last check", new ListRow<(int Entry, ulong Memory), int>(
                    IterableNE.create<ListColumn<(int Entry, ulong Memory)>>(
                        new ListColumn<(int Entry, ulong Memory)>.Label(Some("Entry"), static entry => (entry.Entry + 1).ToString(RowText.Culture), None, None),
                        new ListColumn<(int Entry, ulong Memory)>.Label(Some("Memory"), static entry => RowText.Localize(bytes, arguments: [entry.Memory]), None, None)),
                    static entry => entry.Entry,
                    RowSource.Opaque(Owner, "memory", Seq<(int Entry, ulong Memory)>(), static scope => ValueStore.Readout(
                        scope.Read(Report.Source).Map(static report => report.Map(static held => toSeq(held.Memory.AsIterable()).Map(static entry => (Entry: entry.Key, Memory: entry.Value))).IfNone(Seq<(int Entry, ulong Memory)>())),
                        None)),
                    RowSource.Opaque(Owner, "memory-picked", LanguageExt.HashSet<int>.Empty)), reported),
            ListRows.List("Findings", "Refusals and cautions from last check", new ListRow<PreflightFinding, PreflightFinding>(
                    IterableNE.create<ListColumn<PreflightFinding>>(new ListColumn<PreflightFinding>.Label(None, static finding => ErrorOps.Localize(finding.Error), None, None)),
                    static finding => finding,
                    RowSource.Opaque(Owner, "findings", Seq<PreflightFinding>(), static scope => ValueStore.Readout(
                        scope.Read(Report.Source).Map(static check => check.Map(static held => held.Refusals + held.Cautions).IfNone(Seq<PreflightFinding>())),
                        None)),
                    RowSource.Opaque(Owner, "finding", LanguageExt.HashSet<PreflightFinding>.Empty)),
                reported),
        ];
    }

    private sealed record Placement(RenderSourceState Source, Option<SunWindow> Sun) {
        public static Placement Active { get; } = new(new RenderSourceState(RenderSettings.RenderingSources.ActiveViewport, None, None, None), None);

        public static Placement Named(string name) => Active with { Source = new RenderSourceState(RenderSettings.RenderingSources.NamedView, None, Some(name), None) };

        public static Placement Snapshot(string name) => Active with { Source = new RenderSourceState(RenderSettings.RenderingSources.SnapShot, None, None, Some(name)) };

        public Fin<QueuedEntry> Queued(Option<Albedo> clay, PixelExtent extent) =>
            FrameSpan.Of(0, Sun.Map(static window => window.Frames.Count - 1).IfNone(0))
                .Map(frames => new QueuedEntry(Renders: true, clay, Source, frames, extent, Overscan.Off, None, Sun));
    }

    private static Command<RowRule, Invoke>.Run Command(string name, string caption, string help, Option<RowRule> rule, Func<RowScope, IO<Unit>> execute) =>
        new(new CommandFace(name, caption, Some(help), None), rule, new Invoke.Effect(execute));

    private static IO<Unit> Added(RowScope scope, Func<RhinoDoc, IO<Seq<Placement>>> read) =>
        from doc in scope.DocumentIO
        from clay in scope.Read(Clayed.Source)
        from view in IO.lift(() => Refused.UnlessFound(doc.Views.ActiveView, nameof(ViewTable.ActiveView)))
        from output in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), ImageOutputState.Read)
        from extent in IO.lift(Framing.Final(output, view.ActiveViewport.Size))
        from placed in read(doc)
        from entries in IO.lift(Callbacks.Each(placed, (row, _) => row.Queued(Callbacks.Found(clay, Albedo.Clay), extent)))
        from added in Queues.Change(doc, RowText.Localize("Add Entries"), was => was.Added(entries))
        select unit;

    private static IO<Unit> Sent(RowScope scope, Func<Destination, IO<QueueTarget>> target, Func<QueueContext, IO<Option<QueueProgress>>> resumed) =>
        from doc in scope.DocumentIO
        from context in QueueContext.Current.Of(scope.Sink)
        from plan in Queues.Plan(context, doc)
        from chosen in target(plan.Destination)
        from record in resumed(context)
        from stamp in IO.lift(() => context.Sink.Clock.ToZonedClock().GetCurrentLocalDateTime())
        from sent in Dispatch.Send(context, new DocumentSource.Live(doc), plan, chosen, stamp, record, Some(Reported(scope)))
        select sent;

    private static Func<Preflight, IO<Unit>> Reported(RowScope scope) => report => scope.Store(Report.Source).Put(Some(Some(report))).Map(unit);

    private static Func<Destination, IO<QueueTarget>> Packaged(Func<Destination, IO<QueueTarget>> target) =>
        destination => IO.lift(NamePart.Validated("jobs")).Bind(jobs => target(destination with {
            Folder = destination.Folder.Switch<NamePart, OutputFolder>(
                jobs,
                absolute: static (part, absolute) => new OutputFolder.Absolute(Path.Combine(absolute.Folder, part)),
                besideDocument: static (part, beside) => new OutputFolder.BesideDocument(beside.Segments.Add(part))),
        }));

    private static string Stated(RunState state, DateTimeZone zone) =>
        state.Switch(
            zone,
            absent: static (_, _) => "",
            active: static (at, active) => RowText.Localize("Rendering, last heard {0:g}", arguments: [active.Heard.InZone(at).LocalDateTime]),
            ended: static (_, ended) => RowText.Localize(ended.End.Map(succeeded: "Succeeded", stopped: "Stopped", failed: "Failed")),
            crashed: static (at, crashed) => RowText.Localize("Crashed, last heard {0:g}", arguments: [crashed.Heard.InZone(at).LocalDateTime]));
}
