using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.Json.Serialization;
using Thinktecture.Text.Json.Serialization;
using Darkroom.Chain;
using Darkroom.Output;
using Rasm.Imaging.Files;
using Rasm.Rhino;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Commands;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record QueueHeartbeat(TimeSpan Interval, int Misses) {
    public static QueueHeartbeat Default { get; } = new(TimeSpan.FromSeconds(3), 10);

    public Duration Silence => (Interval * Misses).ToDuration();
}

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

public sealed record Relaunch(string Job, TimeSpan Delay);

public sealed record EntryProgress(Option<SequenceNumber> Last, Seq<Duration> Times, HashMap<Seq<NamePart>, OutputVersion> Versions) {
    public static EntryProgress Empty { get; } = new(None, Seq<Duration>(), HashMap<Seq<NamePart>, OutputVersion>());
}

public sealed record QueueProgress(
    Option<string> Job, int ProcessId, Instant ProcessStart, int Retries, TimeSpan Delay, Option<RunEnd> Ended, HashMap<ContentKey, EntryProgress> Entries) {
    // --- [STATE]
    public bool Succeeded => Ended.Exists(static end => end == RunEnd.Succeeded);

    public long Landed => Entries.Values.Fold(0L, static (sum, entry) => sum + entry.Times.Count);

    public Option<Relaunch> Relaunch => Job.Map(job => new Relaunch(job, Delay));

    // --- [FILE]
    public static IO<FileStream> Acquire(OutputPath path, Action<FileStream> retain) =>
        DisposalOps.Acquire(IO.lift(() => File.Open($"{(string)path}.lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)), retain);

    public static IO<Option<QueueProgress>> Read(OutputPath path) => JsonFiles.Read(path, ProgressContext.Default.ProgressRecord, static record => Fin.Succ(record.Progress));

    public static IO<Unit> Write(OutputPath path, QueueProgress progress) => JsonFiles.Write(path, ProgressRecord.Of(progress), ProgressContext.Default.ProgressRecord);

    public static IO<Unit> Touch(OutputPath path, TimeProvider clock) => IO.lift(() => File.SetLastWriteTimeUtc(path, clock.GetUtcNow().UtcDateTime));

    public static IO<QueueProgress> Started(Option<string> job, RenderBatch batch, Option<QueueProgress> resumed, QueueSettings settings) =>
        DisposalOps.Within(Process.GetCurrentProcess, self => IO.lift(() => new QueueProgress(
            job,
            self.Id,
            self.StartTime.ToUniversalTime().ToInstant(),
            resumed.Map(static record => record.Retries).IfNone(settings.Retries),
            settings.Delay,
            None,
            toHashMap(batch.Entries.Map(entry => (entry.Key, resumed.Bind(record => record.Entries.Find(entry.Key)).IfNone(EntryProgress.Empty) with {
                Versions = toHashMap(entry.Outputs.Choose(static output => output.Name.Version.Taken.Map(version => (output.Name.Scope, version)))),
            }))))));

    public static IO<(QueueSettings Settings, Option<QueueProgress> Prior, RunState State)> Observed(QueueContext context) =>
        from settings in context.Settings
        from prior in Read(context.Record)
        from state in prior.Match(
            Some: held => held.Ended.Match(
                Some: static end => IO.pure<RunState>(new RunState.Ended(end)),
                None: () =>
                    from heard in IO.lift(() => File.GetLastWriteTimeUtc(context.Record).ToInstant())
                    from now in IO.lift(context.Views.Clock.GetCurrentInstant)
                    from live in Live(held)
                    select live && now - heard < settings.Heartbeat.Silence ? (RunState)new RunState.Active(held.ProcessId, heard) : new RunState.Crashed(heard)),
            None: static () => IO.pure<RunState>(new RunState.Absent()))
        select (settings, prior, state);

    private static IO<bool> Live(QueueProgress record) =>
        DisposalOps.Within(() => Process.GetProcessById(record.ProcessId), process => IO.lift(() => process.StartTime.ToUniversalTime().ToInstant() == record.ProcessStart))
            .Catch(static error => error.HasException<ArgumentException>() || error.HasException<InvalidOperationException>(), static _ => IO.pure(value: false));
}

internal sealed record ProgressRecord(string? Job, int ProcessId, DateTimeOffset ProcessStart, int Retries, TimeSpan Delay, RunEnd? Ended, ProgressRecord.Entry[] Entries) {
    internal sealed record Entry(ContentKey Key, SequenceNumber? Last, TimeSpan[] Times, Version[] Versions);

    internal sealed record Version(NamePart[] Scope, OutputVersion Number);

    internal QueueProgress Progress =>
        new(Optional(Job), ProcessId, ProcessStart.ToInstant(), Retries, Delay, Optional(Ended), toHashMap(toSeq(Entries).Map(static entry => (entry.Key, new EntryProgress(
            Optional(entry.Last), toSeq(entry.Times).Map(static time => time.ToDuration()), toHashMap(toSeq(entry.Versions).Map(static row => (toSeq(row.Scope), row.Number))))))));

    internal static ProgressRecord Of(QueueProgress progress) =>
        new(progress.Job.ValueUnsafe(), progress.ProcessId, progress.ProcessStart.ToDateTimeOffset(), progress.Retries, progress.Delay, progress.Ended.ValueUnsafe(),
            [.. progress.Entries.AsIterable().Map(static row => new Entry(
                row.Key,
                row.Value.Last.ValueUnsafe(),
                [.. row.Value.Times.Map(static time => time.ToTimeSpan())],
                [.. row.Value.Versions.AsIterable().Map(static version => new Version([.. version.Key], version.Value))]))]);
}

[JsonSerializable(typeof(ProgressRecord))]
[JsonSerializable(typeof(UInt128))]
[JsonSourceGenerationOptions(System.Text.Json.JsonSerializerDefaults.Strict, WriteIndented = true)]
internal sealed partial class ProgressContext : JsonSerializerContext;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record FrameLost(int Frame) : Expected(string.Create(CultureInfo.InvariantCulture, $"frame {Frame} ended without a saved rendering"), 0);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class QueueRuns {
    private const string RenderCommand = "Render";

    private sealed record Pending(QueueEntry<OutputPath> Entry, Seq<SequenceNumber> Frames);

    private sealed record Request(Pending Run, Conduit<Fin<Unit>, Fin<Unit>> Reply);

    private sealed record Heard {
        public static Heard Empty { get; } = new();

        public Option<long> Began { get; init; }
        public Option<(ImageFileEventArgs Event, long At)> Saved { get; init; }
        public Option<Result> Ended { get; init; }
    }

    private sealed record RunScope(QueueContext Context, RhinoDoc Doc, RenderBatch Batch, Atom<QueueProgress> Held, Atom<Heard> Heard, FileMetadata Metadata, Func<QueueEstimate, IO<Unit>> Docked);

    // --- [RUN]
    public static IO<Unit> Run(QueueContext context, RhinoDoc doc, RenderBatch batch, Option<string> job, Option<QueueProgress> resumed) =>
        DisposalOps.Within<IDisposable, Unit>(retain =>
            from lease in QueueProgress.Acquire(context.Record, retain)
            from observed in QueueProgress.Observed(context)
            let settings = observed.Settings
            from free in IO.lift(observed.State.Switch<Fin<Unit>>(
                absent: static _ => unit,
                active: static active => new PreflightFinding.QueueActive(active.ProcessId).Error,
                ended: static _ => unit,
                crashed: static _ => unit))
            from started in QueueProgress.Started(job, batch, resumed, settings)
            from held in atomIO(started)
            from began in IO.lift(context.Views.Clock.GetTimestamp)
            from ran in DisposalOps.OnFailure(
                    from written in QueueProgress.Write(context.Record, started)
                    from now in IO.lift(context.Views.Clock.GetCurrentInstant)
                    from shown in context.Estimate.SwapIO(_ => QueueEstimate.Running(batch, started, None, now))
                    from noticed in unless(resumed.IsSome, context.Notify(new QueueEvent.Started(batch.Entries.Count, batch.Frames))).As()
                    from rendered in DisposalOps.Within<IDisposable, Unit>(acquired =>
                        from beat in Heartbeat(context, settings.Heartbeat, acquired)
                        from frames in Frames(context, doc, batch, held, resumed.IsNone && settings.SampleEstimate && batch.Entries.Count > 1)
                        select frames, DisposalOps.Release) switch {
                            var run => settings.KeepAwake ? MachinePower.HeldAwake(run) : run,
                        }
                    from closed in Closed(context, held, began, RunEnd.Succeeded, static (record, elapsed) => new QueueEvent.Complete(record.Landed, elapsed))
                    select rendered,
                    error => Stopping(error)
                        ? Closed(context, held, began, RunEnd.Stopped, static (record, _) => new QueueEvent.Stopped(record.Landed))
                        : Closed(context, held, began, RunEnd.Failed, (_, _) => new QueueEvent.Failed(error)))
                .Bind(_ => when(settings.ShutDownAfterSuccess, MachinePower.ShutDown).As())
            select unit, DisposalOps.Release);

    public static bool Stopping(Error error) =>
        error.Is(Errors.Cancelled) || error.FoldM(static cause => cause is Ended ended ? Seq(ended.Result) : Seq<Result>()).As().Exists(static result => result is Result.ExitRhino);

    private static IO<ForkIO<Unit>> Heartbeat(QueueContext context, QueueHeartbeat heartbeat, Action<IDisposable> retain) =>
        DisposalOps.Fork(QueueProgress.Touch(context.Record, context.Views.Clock)
            .Catch(error => IO.lift(() => context.Views.Report(error, typeof(QueueRuns))))
            .As()
            .Repeat(Schedule.spaced(heartbeat.Interval)), retain);

    private static IO<Unit> Closed(QueueContext context, Atom<QueueProgress> held, long began, RunEnd end, Func<QueueProgress, Duration, QueueEvent> notice) =>
        (from record in held.SwapIO(was => was with { Ended = Some(end) })
         from elapsed in IO.lift(() => context.Views.Clock.GetElapsedTime(began).ToDuration())
         from published in Callbacks.Each(Seq(QueueProgress.Write(context.Record, record), context.Notify(notice(record, elapsed))))
         select unit).Uninterruptible();

    // --- [FRAMES]
    private static IO<Unit> Frames(QueueContext context, RhinoDoc doc, RenderBatch batch, Atom<QueueProgress> held, bool sampled) =>
        DisposalOps.Within<IDisposable, Unit>(retain =>
            from render in IO.lift(static () => Conversions.Required(Command.LookupCommandId(RenderCommand, searchForEnglishName: true), nameof(Command.LookupCommandId))).OnUiThread()
            from heard in atomIO(Heard.Empty)
            from dock in Notifications.Acquire(retain).OnUiThread()
            from listened in Listened(context, render, heard).TraverseM(acquire => acquire(retain)).As().OnUiThread()
            from metadata in SaveSource.Metadata(doc, context.Views)
            from ran in Mailed(new RunScope(context, doc, batch, held, heard, metadata, estimate =>
                    from progress in Notifications.SetProgress(dock, estimate.Work)
                    from badge in Notifications.BadgeLabel(dock, Some(estimate.Total - estimate.Landed).Filter(static left => left > 0).Map(static left => left.ToString(RowText.Culture)))
                    select unit), sampled)
            select ran, static owned => DisposalOps.Release(owned).OnUiThread());

    private static Seq<Func<Action<IDisposable>, IO<Unit>>> Listened(QueueContext context, Guid render, Atom<Heard> heard) =>
        [
            retain => EventKind.BeginCommand.Inline(args => when(args.CommandId == render,
                IO.lift(context.Views.Clock.GetTimestamp).Bind(at => heard.SwapIO(_ => Heard.Empty with { Began = Some(at) })).Map(unit)).As(), context.Views, retain),
            retain => EventKind.ImageFileSaved.Inline(args =>
                IO.lift(context.Views.Clock.GetTimestamp).Bind(at => heard.SwapIO(was => was with { Saved = Some((args, at)) })).Map(unit), context.Views, retain),
            retain => EventKind.EndCommand.Inline(args => when(args.CommandId == render,
                heard.SwapIO(was => was with { Ended = Some(args.CommandResult) }).Map(unit)).As(), context.Views, retain),
        ];

    private static IO<Unit> Mailed(RunScope scope, bool sampled) =>
        DisposalOps.Within<IDisposable, Unit>(retain =>
                from post in Subscriptions.Idle<Unit, Request>(pending => toSeq(pending.Values).TraverseM(request => Turn(scope, request)).As().Map(unit))(
                    new CallbackSite(scope.Context.Views, typeof(QueueRuns), nameof(Frames)), retain).OnUiThread()
                from order in IO.lift(() => Order(scope.Batch, scope.Held.Value, sampled))
                from heads in order.Sampled.TraverseM(run => Requested(post, run)).As()
                from estimate in unless(order.Sampled.IsEmpty, IO.lift(scope.Context.Views.Clock.GetCurrentInstant)
                    .Bind(now => scope.Context.Notify(new QueueEvent.Estimate(QueueEstimate.Sampled(scope.Batch, scope.Held.Value, now))))).As()
                from remaining in order.Remaining.TraverseM(run => Requested(post, run)).As()
                select unit,
            static owned => DisposalOps.Release(owned).OnUiThread());

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

    private static IO<Unit> Requested(Func<(Unit Key, Request Value), IO<Unit>> post, Pending run) =>
        DisposalOps.Within(static () => Conduit.make<Fin<Unit>>(), reply =>
            from posted in post((unit, new Request(run, reply)))
            from answer in reply.Source.Take(1).Last()
            from ran in IO.lift(answer)
            select ran, static reply => reply.Complete());

    private static IO<Unit> Turn(RunScope scope, Request request) =>
        Ran(scope, request.Run).Bind(_ => request.Reply.Post(unit)).Catch(error => request.Reply.Post(error)).As();

    private static IO<Unit> Ran(RunScope scope, Pending run) =>
        from rendered in IO.lift(run.Entry.Rendered)
        from ran in run.Entry.Override.Around(scope.Doc, scope.Context.Views,
            Sized(scope.Doc, run.Entry.Queued, rendered).Bind(_ => Widened(scope, run, rendered)))
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
            _ = placed.Iter(place => Optional(entry.View).Iter(place));
        }));

    private static IO<Unit> Widened(RunScope scope, Pending run, OverscanFrame rendered) =>
        run.Entry.Queued.Overscan is 0d
            ? Lit(scope, run, rendered)
            : Viewports.With(scope.Doc, new ViewportTarget.Active(), held => Cameras.Within(held.Viewport, rendered.Widen, Lit(scope, run, rendered)));

    private static IO<Unit> Lit(RunScope scope, Pending run, OverscanFrame rendered) =>
        Optional(run.Entry.Queued.Sun).Match(
            Some: _ => DisposalOps.Within(
                RenderSettingsSources.Read(new RenderSettingsSource.Live(scope.Doc), static held => DisposalOps.Within(() => held.Settings.Sun, static sun => IO.lift(() => SunSite.Moment(sun)))),
                _ => Looped(scope, run, rendered),
                prior => Moved(scope, prior)),
            None: () => Looped(scope, run, rendered));

    private static IO<Unit> Moved(RunScope scope, LocalDateTime moment) =>
        RenderSettingsSources.Edit(new RenderSettingsSource.Live(scope.Doc), held => DisposalOps.Within(() => held.Settings.Sun, sun => IO.lift(SunSite.SetDateTime(sun, moment, scope.Context.Views.Clock))));

    private static IO<Unit> Looped(RunScope scope, Pending run, OverscanFrame rendered) =>
        run.Frames.TraverseM(frame => Frame(scope, run.Entry, rendered, frame)).As().Map(unit);

    private static IO<Unit> Frame(RunScope scope, QueueEntry<OutputPath> entry, OverscanFrame rendered, SequenceNumber frame) =>
        from started in IO.lift(scope.Context.Views.Clock.GetTimestamp)
        from lit in entry.Moment(frame).Traverse(moment => Moved(scope, moment)).As()
        from framing in DisposalOps.Within(() => new RenderSourceView(scope.Doc), source =>
            DisposalOps.Found(source.GetViewInfo, nameof(RenderSourceView.GetViewInfo), view => ChainScene.Read(scope.Doc, view, rendered.Padded, entry.Frames.Time(frame))))
        from launched in Documents.RunScript(scope.Doc, $"_-{RenderCommand}", echo: false, display: None)
        from heard in scope.Heard.ValueIO
        from ended in IO.lift(heard.Ended.ToFin(new FrameLost((int)frame))
            .Bind(result => Conversions.FromResult(result, RenderCommand).MapFail(error => Stopping(error) ? error : new FrameLost((int)frame) + error)))
        from began in IO.lift(heard.Began.ToFin(new FrameLost((int)frame)))
        from saved in IO.lift(heard.Saved.ToFin(new FrameLost((int)frame)))
        from written in new RenderWindowSource.Rendering(saved.Event.SessionId).Open(window => new SaveSource.Window(window, framing, rendered, Optional(entry.Queued.Region)) switch {
            var source => entry.Outputs.Filter(output => output.Name.Number.IfNone(FrameSpan.First) == frame)
                .TraverseM(output => output.Target.FileFormat.Write(scope.Doc, source, output.Target, scope.Metadata, output.Path, entry.Overwrite)).As(),
        })
        from closed in Documents.RunScript(scope.Doc, "_CloseRenderWindow", echo: false, display: None)
        from peak in DisposalOps.Within(Process.GetCurrentProcess, static self => IO.lift(() => self.PeakWorkingSet64))
        from at in IO.lift(scope.Context.Views.Clock.GetTimestamp)
        from landed in Landed(scope, new FrameMark(entry.Key, frame, started, began, saved.At, at, peak))
        select unit;

    private static IO<Unit> Landed(RunScope scope, FrameMark mark) =>
        from record in scope.Held.SwapIO(was => mark.Time(scope.Context.Views.Clock) switch {
            var time => was with {
                Entries = was.Entries.AddOrUpdate(
                    mark.Entry,
                    entry => entry with { Last = Some(mark.Frame), Times = entry.Times.Add(time) },
                    () => EntryProgress.Empty with { Last = Some(mark.Frame), Times = Seq(time) }),
            },
        })
        from written in QueueProgress.Write(scope.Context.Record, record)
        from now in IO.lift(scope.Context.Views.Clock.GetCurrentInstant)
        from estimate in scope.Context.Estimate.SwapIO(_ => QueueEstimate.Running(scope.Batch, record, Some(mark), now))
        from docked in scope.Docked(estimate)
        select unit;

    // --- [RESUME]
    public static IO<Option<Relaunch>> Resumable(QueueContext context) =>
        DisposalOps.Within<IDisposable, Option<Relaunch>>(retain =>
            from lease in QueueProgress.Acquire(context.Record, retain)
            from observed in QueueProgress.Observed(context)
            let crashed = observed.State is RunState.Crashed ? observed.Prior.Filter(static record => record.Relaunch.IsSome) : None
            from relaunch in crashed.Match(
                Some: record => record.Retries > 0
                    ? QueueProgress.Write(context.Record, record with { Retries = record.Retries - 1 }).Map(_ => record.Relaunch)
                    : (from noticed in context.Notify(new QueueEvent.RetriesExhausted(observed.Settings.Retries))
                       from written in QueueProgress.Write(context.Record, record with { Ended = Some(RunEnd.Failed) })
                       select Option<Relaunch>.None),
                None: static () => IO.pure(Option<Relaunch>.None))
            select relaunch, DisposalOps.Release);
}
