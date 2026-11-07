using System.Diagnostics;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Darkroom.Chain;
using Darkroom.Output;
using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Commands;
using Rhino.Render;
using Thinktecture.Text.Json.Serialization;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class RunEnd {
    public static readonly RunEnd Succeeded = new("succeeded");
    public static readonly RunEnd Stopped = new("stopped");
    public static readonly RunEnd Failed = new("failed");
}

[Union]
public abstract partial record RunState {
    public sealed record Absent : RunState;
    public sealed record Active(int ProcessId, Instant Heard) : RunState;
    public sealed record Ended(RunEnd End) : RunState;
    public sealed record Crashed(Instant Heard) : RunState;
}

public sealed record Relaunch(string Job, Duration Delay);

public sealed record EntryProgress(Option<SequenceNumber> Last, Seq<Duration> Times, HashMap<Seq<NamePart>, OutputVersion> Versions) {
    public static EntryProgress Empty { get; } = new(None, Seq<Duration>(), HashMap<Seq<NamePart>, OutputVersion>());
}

public sealed record QueueProgress(
    Option<string> Job, int ProcessId, Instant ProcessStart, int Retries, Duration Delay, Option<RunEnd> Ended, HashMap<UInt128, EntryProgress> Entries) {

    // --- [STATE]

    public bool Succeeded => Ended.Exists(static end => end == RunEnd.Succeeded);

    public long Completed => Entries.Values.Fold(0L, static (sum, entry) => sum + entry.Times.Count);

    public Option<Relaunch> Relaunch => Job.Map(job => new Relaunch(job, Delay));

    // --- [FILES]

    public static IO<FileStream> Acquire(OutputPath path) => use(() => File.Open($"{(string)path}.lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None));

    public static IO<Option<QueueProgress>> Read(OutputPath path) => FilePublication.Read(path, ProgressRecord.Info, static record => Fin.Succ(record.Progress));

    public static IO<Unit> Write(OutputPath path, QueueProgress progress) => FilePublication.Write(path, ProgressRecord.Of(progress), ProgressRecord.Info);

    public static IO<Unit> Touch(OutputPath path, TimeProvider clock) => IO.lift(() => File.SetLastWriteTimeUtc(path, clock.GetUtcNow().UtcDateTime));

    public static IO<QueueProgress> Started(Option<string> job, RenderBatch batch, Option<QueueProgress> resumed, QueueSettings settings) =>
        (from self in use(Process.GetCurrentProcess)
         select new QueueProgress(
             job,
             self.Id,
             self.StartTime.ToUniversalTime().ToInstant(),
             resumed.Map(static record => record.Retries).IfNone(settings.Retries),
             settings.Delay,
             None,
             toHashMap(batch.Entries.Map(entry => entry.Key switch {
                 var key => (key, resumed.Bind(record => record.Entries.Find(key)).IfNone(EntryProgress.Empty) with {
                     Versions = toHashMap(entry.Outputs.Choose(static output => output.Name.Version is OutputVersioning.Pinned pinned ? Some((output.Name.Scope, pinned.Version)) : None)),
                 }),
             })))).Bracket();

    public static IO<(QueueSettings Settings, Option<QueueProgress> Prior, RunState State)> Observed(QueueContext context) =>
        from settings in context.Settings
        from prior in Read(context.Record)
        from state in prior.Match(
            Some: held => held.Ended.Match(
                Some: static end => IO.pure<RunState>(new RunState.Ended(end)),
                None: () =>
                    from heard in IO.lift(() => File.GetLastWriteTimeUtc(context.Record).ToInstant())
                    from now in IO.lift(context.Clock.GetCurrentInstant)
                    from live in Live(held)
                    select live && now - heard < settings.Heartbeat.Silence ? (RunState)new RunState.Active(held.ProcessId, heard) : new RunState.Crashed(heard)),
            None: static () => IO.pure<RunState>(new RunState.Absent()))
        select (settings, prior, state);

    private static IO<bool> Live(QueueProgress record) =>
        (from process in use(() => Process.GetProcessById(record.ProcessId)) select process.StartTime.ToUniversalTime().ToInstant() == record.ProcessStart).Bracket()
            | @catch(static error => error.HasException<ArgumentException>() || error.HasException<InvalidOperationException>(), static _ => IO.pure(value: false));
}

internal sealed record ProgressRecord(Option<string> Job, int ProcessId, Instant ProcessStart, int Retries, Duration Delay, Option<RunEnd> Ended, Seq<ProgressRecord.Entry> Entries) {
    internal static readonly JsonTypeInfo<ProgressRecord> Info = ArchivableDictionaries.Info<ProgressRecord>();

    internal sealed record Entry(UInt128 Key, Option<SequenceNumber> Last, Seq<Duration> Times, Seq<Version> Versions);

    internal sealed record Version(Seq<NamePart> Scope, OutputVersion Number);

    internal QueueProgress Progress =>
        new(Job, ProcessId, ProcessStart, Retries, Delay, Ended, toHashMap(Entries.Map(static entry =>
            (entry.Key, new EntryProgress(entry.Last, entry.Times, toHashMap(entry.Versions.Map(static row => (row.Scope, row.Number))))))));

    internal static ProgressRecord Of(QueueProgress progress) =>
        new(progress.Job, progress.ProcessId, progress.ProcessStart, progress.Retries, progress.Delay, progress.Ended,
            [.. progress.Entries.AsIterable().Map(static row => new Entry(
                row.Key,
                row.Value.Last,
                row.Value.Times,
                [.. row.Value.Versions.AsIterable().Map(static version => new Version(version.Key, version.Value))]))]);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class QueueRuns {

    // --- [EXECUTION]

    public static IO<Unit> Run(QueueContext context, RhinoDoc doc, RenderBatch batch, Option<string> job, Option<QueueProgress> resumed) =>
        (from lease in QueueProgress.Acquire(context.Record)
         from observed in QueueProgress.Observed(context)
         let settings = observed.Settings
         from free in IO.lift(observed.State.Switch<Fin<Unit>>(
             absent: static _ => unit,
             active: static active => new PreflightFinding.QueueActive(active.ProcessId).Error,
             ended: static _ => unit,
             crashed: static _ => unit))
         from started in QueueProgress.Started(job, batch, resumed, settings)
         from held in atomIO(started)
         from began in IO.lift(context.Clock.GetTimestamp)
         from ran in (from written in QueueProgress.Write(context.Record, started)
                      from noticed in unless(resumed.IsSome, context.Notify(new QueueEvent.Started(batch.Entries.Count, batch.Frames))).As()
                      from rendered in (from beat in Heartbeat(context, settings.Heartbeat)
                                        from frames in Frames(context, doc, batch, held, resumed.IsNone && settings.SampleEstimate && batch.Entries.Count > 1)
                                        select frames).Bracket() switch {
                                            var run => settings.KeepAwake ? MachinePower.HeldAwake(run) : run,
                                        }
                      from closed in Closed(context, held, began, RunEnd.Succeeded, static (record, elapsed) => new QueueEvent.Complete(record.Completed, elapsed))
                      select rendered)
                 .Catch(error => (Stopping(error)
                     ? Closed(context, held, began, RunEnd.Stopped, static (record, _) => new QueueEvent.Stopped(record.Completed))
                     : Closed(context, held, began, RunEnd.Failed, (_, _) => new QueueEvent.Failed(error))) >> IO.fail<Unit>(error))
                 .Bind(_ => when(settings.ShutDownAfterSuccess, MachinePower.ShutDown).As())
         select unit).Bracket().Fork().Bind(static forked => forked.Await);

    public static bool Stopping(Error error) =>
        error.Is(Errors.Cancelled) || error.AsIterable().Exists(static cause => cause is RhinoFault fault && fault.Reason == RhinoReason.Exited);

    private static IO<ForkIO<Unit>> Heartbeat(QueueContext context, QueueHeartbeat heartbeat) =>
        use(QueueProgress.Touch(context.Record, context.Clock)
            .Catch(static error => IO.lift(() => Session.Failed(error)))
            .As()
            .Repeat(Schedule.spaced(heartbeat.Interval.ToTimeSpan()))
            .Fork(), static fork => fork.Cancel);

    private static IO<Unit> Closed(QueueContext context, Atom<QueueProgress> held, long began, RunEnd end, Func<QueueProgress, Duration, QueueEvent> notice) =>
        (from record in held.SwapIO(was => was with { Ended = Some(end) })
         from elapsed in IO.lift(() => context.Clock.GetElapsedTime(began).ToDuration())
         from published in Products.Each(Seq(QueueProgress.Write(context.Record, record), context.Notify(notice(record, elapsed))))
         select unit).Uninterruptible();

    // --- [FRAMES]

    private const string RenderCommand = "Render";

    private sealed record Pending(QueueEntry<OutputPath> Entry, Seq<SequenceNumber> Frames);

    private sealed record Request(Pending Run, Conduit<Fin<Unit>, Fin<Unit>> Reply);

    private sealed record Heard {
        public static Heard Empty { get; } = new();

        public Option<long> Began { get; init; }
        public Option<(Guid Session, long At)> Saved { get; init; }
        public Option<Result> Ended { get; init; }
    }

    private sealed record RunScope(QueueContext Context, RhinoDoc Doc, RenderBatch Batch, Atom<QueueProgress> Held, Atom<Heard> Heard);

    private static IO<Unit> Frames(QueueContext context, RhinoDoc doc, RenderBatch batch, Atom<QueueProgress> held, bool sampled) =>
        (from render in IO.lift(static () => Conversions.Required(Conversions.Present(Command.LookupCommandId(RenderCommand, searchForEnglishName: true)))).OnUiThread()
         from heard in atomIO(Heard.Empty)
         from listened in Listened(render, heard, context.Clock).OnUiThread()
         from ran in Mailed(new RunScope(context, doc, batch, held, heard), sampled)
         select ran).Bracket();

    private static IO<Unit> Listened(Guid render, Atom<Heard> heard, TimeProvider clock) =>
        from began in Subscriptions.Inline<CommandEventArgs>(static h => Command.BeginCommand += h, static h => Command.BeginCommand -= h).Attach(args => when(args.CommandId == render,
            IO.lift(clock.GetTimestamp).Bind(at => heard.SwapIO(_ => Heard.Empty with { Began = Some(at) })).Map(unit)).As())
        from saved in Subscriptions.Inline<ImageFileEventArgs>(static h => ImageFile.Saved += h, static h => ImageFile.Saved -= h).Attach(args =>
            IO.lift(clock.GetTimestamp).Bind(at => heard.SwapIO(was => was with { Saved = Some((args.SessionId, at)) })).Map(unit))
        from ended in Subscriptions.Inline<CommandEventArgs>(static h => Command.EndCommand += h, static h => Command.EndCommand -= h).Attach(args => when(args.CommandId == render,
            heard.SwapIO(was => was with { Ended = Some(args.CommandResult) }).Map(unit)).As())
        select unit;

    private static IO<Unit> Mailed(RunScope scope, bool sampled) =>
        (from post in Mailboxes.Idle<Unit, Request>(pending => toSeq(pending.Values).TraverseM(request => Turn(scope, request)).As().Map(unit)).OnUiThread()
         from order in IO.lift(() => Order(scope.Batch, scope.Held.Value, sampled))
         from heads in order.Sampled.TraverseM(run => Requested(post, run)).As()
         from estimate in unless(order.Sampled.IsEmpty, IO.lift(scope.Context.Clock.GetCurrentInstant)
             .Bind(now => scope.Context.Notify(new QueueEvent.Estimate(QueueEstimate.Sampled(scope.Batch, scope.Held.Value, now))))).As()
         from remaining in order.Remaining.TraverseM(run => Requested(post, run)).As()
         select unit).Bracket();

    private static (Seq<Pending> Sampled, Seq<Pending> Remaining) Order(RenderBatch batch, QueueProgress record, bool sampled) =>
        (from entry in batch.Entries
         let done = record.Entries.Find(entry.Key).Bind(static held => held.Last)
         let frames = entry.Frames.Numbers.Filter(frame => done.ForAll(last => frame > last))
         where !frames.IsEmpty
         select new Pending(entry, frames)) switch {
             var pending => sampled
                 ? (pending.Map(static run => run with { Frames = run.Frames.Take(2) }),
                    pending.Map(static run => run with { Frames = run.Frames.Skip(2) }).Filter(static run => !run.Frames.IsEmpty))
                 : (Seq<Pending>(), pending),
         };

    private static IO<Unit> Requested(Func<Unit, Request, IO<Unit>> post, Pending run) =>
        (from reply in use(IO.lift(static () => Conduit.make<Fin<Unit>>()), static open => open.Complete())
         from posted in post(unit, new Request(run, reply))
         from answer in reply.Source.Take(1).Last()
         from ran in IO.lift(answer)
         select ran).Bracket();

    private static IO<Unit> Turn(RunScope scope, Request request) =>
        Ran(scope, request.Run).Bind(_ => request.Reply.Post(unit)).Catch(error => request.Reply.Post(error)).As();

    private static IO<Unit> Ran(RunScope scope, Pending run) =>
        from rendered in IO.lift(run.Entry.Rendered)
        from ran in RenderSettingsSources.Within(scope.Doc, run.Entry.Settings,
            (from clay in run.Entry.Queued.Clay.Traverse(albedo => ClayMeshProvider.Register(scope.Doc, albedo)).As()
             from sized in Sized(scope.Doc, run.Entry.Queued, rendered)
             from widened in Widened(scope, run, rendered)
             select widened).Bracket())
        select ran;

    private static IO<Unit> Sized(RhinoDoc doc, QueuedEntry entry, OverscanFrame rendered) =>
        RenderSettingsSources.Edit(new RenderSettingsSource.Live(doc), scope => IO.lift(() => {
            RenderSettings settings = scope.Settings;
            (settings.RenderSource, settings.UseViewportSize, settings.ImageSize) = (entry.Source, false, new Size(rendered.Padded.Width, rendered.Padded.Height));
            Option<Action<string>> placed = entry.Source switch {
                RenderSettings.RenderingSources.SpecificViewport => Some<Action<string>>(view => settings.SpecificViewport = view),
                RenderSettings.RenderingSources.NamedView => Some<Action<string>>(view => settings.NamedView = view),
                RenderSettings.RenderingSources.SnapShot => Some<Action<string>>(view => settings.Snapshot = view),
                _ => None,
            };
            _ = placed.Iter(place => entry.View.Iter(place));
        }));

    private static IO<Unit> Widened(RunScope scope, Pending run, OverscanFrame rendered) =>
        run.Frames.TraverseM(frame => Frame(scope, run.Entry, rendered, frame)).As().Map(unit) switch {
            var frames => run.Entry.Queued.Overscan is 0d
                ? frames
                : Viewports.With(scope.Doc, new ViewportTarget.Active(), held => Cameras.Within(held.Viewport, rendered.Widen, frames)),
        };

    private static IO<Unit> Frame(RunScope scope, QueueEntry<OutputPath> entry, OverscanFrame rendered, SequenceNumber frame) =>
        from started in IO.lift(scope.Context.Clock.GetTimestamp)
        from lit in entry.Moment(frame).Traverse(moment => RenderSettingsSources.Edit(new RenderSettingsSource.Live(scope.Doc), held =>
            (from sun in use(() => held.Settings.Sun)
             from site in IO.lift(SunSite.Read(sun))
             from set in site.SetDateTime(sun, moment)
             select set).Bracket())).As()
        from framing in ChainScene.Sourced(scope.Doc, rendered.Padded, entry.Frames.Time(frame))
        from launched in Documents.RunScript(scope.Doc, $"_-{RenderCommand}", echo: false, display: None)
        from heard in scope.Heard.ValueIO
        let lost = new DarkroomFault(frame, DarkroomReason.Unsaved, Expression: "frame")
        from ended in IO.lift(heard.Ended.ToFin(lost).Bind(result => Results.Ended(result).MapFail(error => Stopping(error) ? error : lost + error)))
        from began in IO.lift(heard.Began.ToFin(lost))
        from saved in IO.lift(heard.Saved.ToFin(lost))
        from metadata in SaveSource.Metadata(scope.Doc, scope.Context.Clock)
        from written in (from window in Products.Of(() => RenderWindow.FromSessionId(saved.Session))
                         let source = new SaveSource(window, framing, rendered, entry.Queued.Region)
                         from outputs in entry.Outputs.Filter(output => output.Name.Number.IfNone(SequenceNumber.MinValue) == frame)
                             .TraverseM(output => output.Target.FileFormat.Write(scope.Doc, source, output.Target, metadata, output.Path, entry.Overwrite)).As()
                         select unit).Bracket()
        from closed in Documents.RunScript(scope.Doc, "_CloseRenderWindow", echo: false, display: None)
        from finished in IO.lift(scope.Context.Clock.GetTimestamp)
        from peak in OperatingSystem.IsMacOS()
            ? IO.lift(SafeNativeMethods.PeakResident)
            : (from self in use(Process.GetCurrentProcess) select self.PeakWorkingSet64).Bracket()
        from recorded in Recorded(scope, new FrameTiming(entry.Key, frame, started, began, saved.At, finished, peak))
        select unit;

    private static IO<Unit> Recorded(RunScope scope, FrameTiming timing) =>
        from record in scope.Held.SwapIO(was => was with {
            Entries = was.Entries.AddOrUpdate(
                timing.Entry,
                entry => entry with { Last = Some(timing.Frame), Times = entry.Times.Add(timing.Time(scope.Context.Clock)) },
                () => EntryProgress.Empty with { Last = Some(timing.Frame), Times = Seq(timing.Time(scope.Context.Clock)) }),
        })
        from written in QueueProgress.Write(scope.Context.Record, record)
        from now in IO.lift(scope.Context.Clock.GetCurrentInstant)
        from estimate in IO.lift(() => QueueEstimate.Running(scope.Batch, record, now).Description(scope.Context.Clock.ToZonedClock().Zone))
        let stages = timing.Stages(scope.Context.Clock)
        let frame = (int)timing.Frame
        from logged in IO.lift(() => Rendered(Session.Logger, frame, stages, timing.PeakWorkingSet, estimate))
        select unit;

    // --- [RESUMPTION]

    public static IO<Option<Relaunch>> Resumable(QueueContext context) =>
        (from lease in QueueProgress.Acquire(context.Record)
         from observed in QueueProgress.Observed(context)
         let crashed = observed.State is RunState.Crashed ? observed.Prior.Filter(static record => record.Relaunch.IsSome) : None
         from relaunch in crashed.Match(
             Some: record => record.Retries > 0
                 ? QueueProgress.Write(context.Record, record with { Retries = record.Retries - 1 }).Map(_ => record.Relaunch)
                 : (from noticed in context.Notify(new QueueEvent.RetriesExhausted(observed.Settings.Retries))
                    from written in QueueProgress.Write(context.Record, record with { Ended = Some(RunEnd.Failed) })
                    select Option<Relaunch>.None),
             None: static () => IO.pure(Option<Relaunch>.None))
         select relaunch).Bracket();

    // --- [LOGS]

    [LoggerMessage(Level = LogLevel.Information, Message = "Render queue frame {Frame}: {Stages}, peak working set {Peak} bytes, {Estimate}")]
    private static partial void Rendered(ILogger logger, int frame, string stages, long peak, string estimate);
}
