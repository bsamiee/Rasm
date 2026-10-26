using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.Json.Serialization;
using Darkroom.Chain;
using Darkroom.Output;
using NodaTime.Text;
using Rasm.Imaging.Files;
using Rasm.Rhino;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.Viewport;
using Rasm.UI;
using Rhino;
using Rhino.Commands;
using Rhino.Render;
using UnitsNet;
using UnitsNet.Units;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct HeartbeatInterval : IPresented<HeartbeatInterval, double> {
    public static HeartbeatInterval MinValue { get; } = new(0.001d);
    public static HeartbeatInterval MaxValue { get; } = new((uint.MaxValue - 1) / 1000d);
    public static Presentation<double> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    public Duration Span => Duration.FromSeconds(_value);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct MissedBeats : IPresented<MissedBeats, int> {
    public static MissedBeats MinValue { get; } = new(2);
    public static MissedBeats MaxValue { get; } = new(int.MaxValue);
    public static Presentation<int> Presentation { get; } = new();

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record QueueHeartbeat(HeartbeatInterval Interval, MissedBeats Misses) : IStateRecord<QueueHeartbeat, HeartbeatParameter> {
    public static QueueHeartbeat Default { get; } = new(HeartbeatInterval.Create(3d), MissedBeats.Create(10));

    public Duration Silence => Interval.Span * (int)Misses;
}

[SmartEnum<string>]
public sealed partial class HeartbeatParameter : IStateParameter<QueueHeartbeat> {
    public static readonly HeartbeatParameter Interval = new("interval", StateParameter.Bounded(
            Lens<QueueHeartbeat, HeartbeatInterval>.New(static beat => beat.Interval, static interval => beat => beat with { Interval = interval })),
        new("Heartbeat interval", "Time between heartbeat writes to the queue record"));
    public static readonly HeartbeatParameter Misses = new("misses", StateParameter.Bounded(
            Lens<QueueHeartbeat, MissedBeats>.New(static beat => beat.Misses, static misses => beat => beat with { Misses = misses })),
        new("Missed heartbeats", "Missed intervals before the queue counts as crashed"));

    public StateParameter<QueueHeartbeat> Kind { get; }

    public ParameterText Text { get; }
}

[SmartEnum<string>]
public sealed partial class RunEnd : ICaptioned {
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

public sealed record Relaunch(string Job, RetryDelay Delay);

public sealed record EntryProgress(Option<SequenceNumber> Last, Seq<Duration> Times, HashMap<Seq<NamePart>, OutputVersion> Versions) {
    public static EntryProgress Empty { get; } = new(None, Seq<Duration>(), HashMap<Seq<NamePart>, OutputVersion>());
}

public sealed record QueueProgress(
    Option<string> Job, int ProcessId, Instant ProcessStart, RetryCount Retries, RetryDelay Delay, Option<RunEnd> Ended, HashMap<ContentKey, EntryProgress> Entries) {
    // --- [STATE]
    public bool Succeeded => Ended.Exists(static end => end == RunEnd.Succeeded);

    public long Landed => Entries.Values.Fold(0L, static (sum, entry) => sum + entry.Times.Count);

    public Option<Relaunch> Relaunch => Job.Map(job => new Relaunch(job, Delay));

    // --- [FILE]
    public static IO<FileStream> Acquire(OutputPath path, Action<FileStream> retain) =>
        DisposalOps.Acquire(IO.lift(() => File.Open($"{(string)path}.lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)), retain);

    public static IO<Option<QueueProgress>> Read(OutputPath path) => JsonFiles.Read(path, ProgressContext.Default.ProgressRecord, ProgressRecord.Decoded);

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
            .Catch(static error => error.HasException<ArgumentException>() || error.HasException<InvalidOperationException>(), static _ => IO.pure(false));
}

internal sealed record ProgressRecord(string? Job, int ProcessId, string ProcessStart, int Retries, int Delay, string? Ended, ProgressRecord.Entry[] Entries) {
    internal sealed record Entry(string Key, int? Last, string[] Times, Version[] Versions);

    internal sealed record Version(string[] Scope, int Number);

    // --- [OUTBOUND]
    internal static ProgressRecord Of(QueueProgress progress) =>
        new(progress.Job.ValueUnsafe(), progress.ProcessId, InstantPattern.ExtendedIso.Format(progress.ProcessStart), progress.Retries, progress.Delay,
            progress.Ended.Map(static end => end.Key).ValueUnsafe(),
            [.. progress.Entries.AsIterable().Map(static row => new Entry(
                ((UInt128)row.Key).ToString("x32", CultureInfo.InvariantCulture),
                row.Value.Last.Map(static last => (int)last).ToNullable(),
                [.. row.Value.Times.Map(static time => DurationPattern.Roundtrip.Format(time))],
                [.. row.Value.Versions.AsIterable().Map(static version => new Version([.. version.Key.Map(static part => (string)part)], version.Value))]))]);

    // --- [INBOUND]
    internal static Fin<QueueProgress> Decoded(ProgressRecord record) =>
        (Parsed(InstantPattern.ExtendedIso.Parse(record.ProcessStart), record.ProcessStart),
         RetryCount.Validated(record.Retries),
         RetryDelay.Validated(record.Delay),
         Optional(record.Ended).Traverse(static end => RunEnd.Validated(end)).As(),
         toSeq(record.Entries).Traverse(Decoded).As())
            .Apply((start, retries, delay, end, entries) => new QueueProgress(Optional(record.Job), record.ProcessId, start, retries, delay, end, toHashMap(entries)))
            .As();

    internal static Fin<T> Parsed<T>(ParseResult<T> parsed, string text) =>
        parsed.Success ? parsed.Value : Errors.ParseError($"queue record text {text} did not parse");

    private static Fin<(ContentKey Key, EntryProgress Progress)> Decoded(Entry entry) =>
        (Invariant.Number<UInt128>(entry.Key, NumberStyles.HexNumber).ToFin(Errors.ParseError($"queue record key {entry.Key} did not parse")).Bind(ContentKey.Validated),
         Optional(entry.Last).Traverse(static last => SequenceNumber.Validated(last)).As(),
         toSeq(entry.Times).Traverse(static time => Parsed(DurationPattern.Roundtrip.Parse(time), time)).As(),
         toSeq(entry.Versions).Traverse(static row => (toSeq(row.Scope).Traverse(NamePart.Validated).As(), OutputVersion.Validated(row.Number))
             .Apply(static (scope, version) => (scope, version)).As()).As())
            .Apply(static (key, last, times, versions) => (key, new EntryProgress(last, times, toHashMap(versions))))
            .As();
}

[JsonSerializable(typeof(ProgressRecord))]
[JsonSourceGenerationOptions(System.Text.Json.JsonSerializerDefaults.Strict, WriteIndented = true)]
internal sealed partial class ProgressContext : JsonSerializerContext;

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record FrameLost(int Frame) : Expected($"frame {Frame} ended without a saved rendering", 0);

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
            .Catch(error => IO.lift(() => context.Views.Report(error, typeof(QueueRuns), nameof(Heartbeat))))
            .As()
            .Repeat(Schedule.spaced(heartbeat.Interval.Span.ToTimeSpan())), retain);

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
            Sized(scope.Doc, run.Entry, rendered).Bind(_ => Widened(scope, run, rendered)))
        select ran;

    private static IO<Unit> Sized(RhinoDoc doc, QueueEntry<OutputPath> entry, OverscanFrame rendered) =>
        RenderSettingsSources.Edit(new RenderSettingsSource.Live(doc), scope =>
            from source in RenderSourceState.Write(scope, entry.Source)
            from output in ImageOutputState.Read(scope)
            from sized in IO.lift(Framing.Sized(output with { UseViewportSize = false }, new Size(rendered.Padded.Width, rendered.Padded.Height)))
            from written in ImageOutputState.Write(scope, sized)
            select unit);

    private static IO<Unit> Widened(RunScope scope, Pending run, OverscanFrame rendered) =>
        run.Entry.Overscan == Overscan.Off
            ? Lit(scope, run, rendered)
            : Viewports.With(scope.Doc, new ViewportTarget.Active(), held => Cameras.Within(held.Viewport, rendered.Widen, Lit(scope, run, rendered)));

    private static IO<Unit> Lit(RunScope scope, Pending run, OverscanFrame rendered) =>
        run.Entry.Sun.Match(
            Some: _ => SunState.Moment(new RenderSettingsSource.Live(scope.Doc), scope.Context.Views.Clock) switch {
                var moment => moment.Read.Bind(prior => DisposalOps.Within(() => prior, _ => Looped(scope, run, rendered), value => moment.Write(value).Map(unit))),
            },
            None: () => Looped(scope, run, rendered));

    private static IO<Unit> Looped(RunScope scope, Pending run, OverscanFrame rendered) =>
        run.Frames.TraverseM(frame => Frame(scope, run.Entry, rendered, frame)).As().Map(unit);

    private static IO<Unit> Frame(RunScope scope, QueueEntry<OutputPath> entry, OverscanFrame rendered, SequenceNumber frame) =>
        from started in IO.lift(scope.Context.Views.Clock.GetTimestamp)
        from moment in IO.lift(entry.Moment(frame))
        from lit in moment.Traverse(held => SunState.Moment(new RenderSettingsSource.Live(scope.Doc), scope.Context.Views.Clock).Write(Some(held))).As()
        from framing in DisposalOps.Within(() => new RenderSourceView(scope.Doc), source =>
            DisposalOps.Found(source.GetViewInfo, nameof(RenderSourceView.GetViewInfo), view => ChainScene.Read(scope.Doc, view, rendered.Padded, entry.Frames.Time(frame))))
        from launched in Documents.RunScript(scope.Doc, $"_-{RenderCommand}", echo: false, display: None)
        from heard in scope.Heard.ValueIO
        from ended in IO.lift(heard.Ended.ToFin(new FrameLost((int)frame))
            .Bind(result => Conversions.FromResult(result, RenderCommand).MapFail(error => Stopping(error) ? error : new FrameLost((int)frame) + error)))
        from began in IO.lift(heard.Began.ToFin(new FrameLost((int)frame)))
        from saved in IO.lift(heard.Saved.ToFin(new FrameLost((int)frame)))
        from written in new RenderWindowSource.Rendering(saved.Event.SessionId).Open(window => new SaveSource.Window(window, framing, rendered, entry.Region) switch {
            var source => entry.Outputs.Filter(output => output.Name.Number.IfNone(entry.Frames.First) == frame)
                .TraverseM(output => source.Write(scope.Doc, scope.Metadata, output.Target, output.Path, entry.Destination.Overwrite)).As(),
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
                Some: record => record.Retries.Spent.Match(
                    Some: left => QueueProgress.Write(context.Record, record with { Retries = left }).Map(_ => record.Relaunch),
                    None: () =>
                        from noticed in context.Notify(new QueueEvent.RetriesExhausted(observed.Settings.Retries))
                        from written in QueueProgress.Write(context.Record, record with { Ended = Some(RunEnd.Failed) })
                        select Option<Relaunch>.None),
                None: static () => IO.pure(Option<Relaunch>.None))
            select relaunch, DisposalOps.Release);
}
