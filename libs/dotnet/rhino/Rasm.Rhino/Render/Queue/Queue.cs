using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using NodaTime.Text;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Commands;
using Rhino.Render;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct RetryCount : System.Numerics.IMinMaxValue<RetryCount> {
    public static RetryCount MinValue { get; } = new(0);
    public static RetryCount MaxValue { get; } = new(20);
    public static RetryCount Default { get; } = new(3);

    public Option<RetryCount> Spent => _value > MinValue._value ? new RetryCount(_value - 1) : None;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct RetryDelay : System.Numerics.IMinMaxValue<RetryDelay> {
    public static RetryDelay MinValue { get; } = new(0);
    public static RetryDelay MaxValue { get; } = new(600);
    public static RetryDelay Default { get; } = new(1);

    public static Presentation<int> Presentation { get; } = new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    public Duration Span => Duration.FromSeconds(_value);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject]
public sealed partial class FrameSpan {
    public const double Rate = 24d;

    public SequenceNumber First { get; }
    public SequenceNumber Last { get; }

    public long Count => (long)(int)Last - (int)First + 1;

    public Seq<SequenceNumber> Numbers =>
        toSeq(LanguageExt.List.unfold(Some(First), at => at.Filter(number => number <= Last).Map(static number => (number, number.Next()))));

    public Option<SequenceNumber> Named(SequenceNumber frame) => Count > 1 ? Some(frame) : None;

    public Option<TimeSpan> Time(SequenceNumber frame) => Named(frame).Map(static number => TimeSpan.FromSeconds((int)number / Rate));

    public static Fin<FrameSpan> Of(int first, int last) =>
        (SequenceNumber.Validated(first), SequenceNumber.Validated(last))
            .Apply(static (from, until) => (From: from, Until: until))
            .As()
            .Bind(static span => ValidationResults.ToFin(Validate(span.From, span.Until, out FrameSpan? spanned), spanned));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref SequenceNumber first, ref SequenceNumber last) =>
        validationError = first <= last && (long)(int)last - (int)first + 1 <= int.MaxValue ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"Frame span {first} to {last} is reversed or longer than {int.MaxValue} frames"));
}

public sealed record QueueOutput<TPath>(OutputTarget Target, OutputName Name, TPath Path);

public sealed record QueueEntry<TPath>(
    RenderOverride Override,
    RenderSourceState Source,
    FrameSpan Frames,
    PixelExtent Extent,
    Overscan Overscan,
    Option<RenderRegion> Region,
    Option<SunWindow> Sun,
    Destination Destination,
    Seq<QueueOutput<TPath>> Outputs) {
    public ContentKey Key =>
        ContentKey.Of(KeyDomain.QueueEntry, stream => stream
            .Rows(toSeq(Override.Values.Entries.AsIterable().OrderBy(static entry => entry.Key)), static (fields, entry) =>
                fields.Text(entry.Key.Owner).Rows(entry.Key.Path, static (path, part) => path.Text(part)).Text(entry.Value))
            .Rows(Override.Clay.ToSeq(), static (fields, clay) => fields.Number(clay))
            .Integer((int)Source.RenderSource)
            .Rows(Seq(Source.SpecificViewport, Source.NamedView, Source.Snapshot), static (fields, name) => fields.Rows(name.ToSeq(), static (names, text) => names.Text(text)))
            .Integer<int>(Frames.First).Integer<int>(Frames.Last).Integer(Extent.Width).Integer(Extent.Height).Number(Overscan)
            .Rows(Region.ToSeq(), static (fields, region) => fields.Number(region.Left).Number(region.Top).Number(region.Right).Number(region.Bottom))
            .Rows(Sun.ToSeq(), static (fields, window) => fields
                .Text(LocalDateTimePattern.ExtendedIso.Format(window.Start))
                .Text(LocalDateTimePattern.ExtendedIso.Format(window.End))
                .Text(PeriodPattern.Roundtrip.Format(window.Step)))
            .Rows(FieldTexts.Items<Destination>().Choose(item => item.Capture(Destination).Map(text => (item.Path, Text: text))),
                static (fields, item) => fields.Rows(item.Path, static (path, part) => path.Text(part)).Text(item.Text))
            .Rows(Outputs.Map(static output => (output.Target, output.Name.Scope, output.Name.Parts)).Distinct(), static (fields, output) => fields
                .Rows(output.Scope, static (scope, part) => scope.Text(part))
                .Rows(output.Parts, static (parts, part) => parts.Text(part))
                .Rows(FieldTexts.Items<OutputTarget>().Choose(item => item.Capture(output.Target).Map(text => (item.Path, Text: text))),
                    static (target, item) => target.Rows(item.Path, static (path, part) => path.Text(part)).Text(item.Text))));

    public Fin<OverscanFrame> Rendered => OverscanFrame.Around(Extent, Overscan);

    public Fin<Option<SunMoment>> Moment(SequenceNumber frame) =>
        Sun.Traverse(window => window.Frames switch {
            var moments => moments.At((int)frame).ToFin(Error.New(string.Create(CultureInfo.InvariantCulture, $"Frame {frame} lies past sun study's {moments.Count} moments")))
                .Bind(ValidationResults.Validated<SunMoment, LocalDateTime>),
        }).As();

    public QueueEntry<TPlaced> Placed<TPlaced>(Seq<QueueOutput<TPlaced>> outputs) => new(Override, Source, Frames, Extent, Overscan, Region, Sun, Destination, outputs);
}

public sealed class QueuePlan {
    private QueuePlan(Destination destination, Seq<QueueEntry<Unit>> entries) => (Destination, Entries) = (destination, entries);

    public Destination Destination { get; }
    public Seq<QueueEntry<Unit>> Entries { get; }

    public static Fin<Seq<QueueEntry<TPath>>> Unique<TPath>(Seq<QueueEntry<TPath>> entries) =>
        Callbacks.Unique(entries, static entry => entry.Key, nameof(QueuePlan));

    public static Fin<QueuePlan> Create(Destination destination, Seq<QueueEntry<Unit>> entries) =>
        Unique(entries).Map(unique => new QueuePlan(destination, unique));
}

public sealed record RenderBatch(Seq<QueueEntry<OutputPath>> Entries) {
    public long Frames => Entries.Fold(0L, static (sum, entry) => sum + entry.Frames.Count);
}

public sealed record QueueSettings(RetryCount Retries, RetryDelay Delay, bool SampleEstimate, bool KeepAwake, bool ShutDownAfterSuccess, QueueHeartbeat Heartbeat) {
    public static QueueSettings Default { get; } = new(RetryCount.Default, RetryDelay.Default, SampleEstimate: true, KeepAwake: true, ShutDownAfterSuccess: false, QueueHeartbeat.Default);
}

public sealed record QueueSettingRows(
    PlugInSetting<RetryCount, int> Retries,
    PlugInSetting<RetryDelay, int> Delay,
    PlugInSetting<bool, bool> SampleEstimate,
    PlugInSetting<bool, bool> KeepAwake,
    PlugInSetting<bool, bool> ShutDownAfterSuccess,
    PlugInSetting<Duration, double> HeartbeatInterval,
    PlugInSetting<int, int> HeartbeatMisses,
    PlugInSetting<HttpServer, string> Server,
    PlugInSetting<Option<NoticeTopic>, string> Topic,
    FlamencoSettingRows Flamenco) {
    public IO<QueueSettings> Settings(ISettingReader reader) =>
        IO.lift(() => (reader.Current(Retries), reader.Current(Delay), reader.Current(SampleEstimate),
                reader.Current(KeepAwake), reader.Current(ShutDownAfterSuccess), reader.Current(HeartbeatInterval), reader.Current(HeartbeatMisses))
            .Apply(static (retries, delay, sampled, awake, shutdown, interval, misses) =>
                ValidationResults.ToFin(QueueHeartbeat.Validate(interval, misses, out QueueHeartbeat? heartbeat), heartbeat)
                    .Map(held => new QueueSettings(retries, delay, sampled, awake, shutdown, held)))
            .As()
            .Bind(static settings => settings));

    public IO<NoticeSettings> Notices(ISettingReader reader) =>
        IO.lift(() => (reader.Current(Server), reader.Current(Topic)).Apply(static (server, topic) => new NoticeSettings(server, topic)).As());
}

public sealed record QueueContext(
    OutputPath Record,
    IO<QueueSettings> Settings,
    IPlugInViews Sink,
    Func<RhinoDoc, IO<BindingGroup>> Bindings,
    Option<RenderHistory> History,
    Atom<QueueEstimate> Estimate,
    TrayRow Tray,
    Func<QueueEvent, IO<Unit>> Notify,
    string JobCommand,
    string FarmScript,
    Action<IDisposable> Retain) {
    public static PlugInSlot<QueueContext> Current { get; } = new();
}

public sealed record Relaunch(string Job, RetryDelay Delay);

public sealed record QueuedEntry(
    bool Renders,
    Option<Albedo> Clay,
    RenderSourceState Source,
    FrameSpan Frames,
    PixelExtent Extent,
    Overscan Overscan,
    Option<RenderRegion> Region,
    Option<SunWindow> Sun) {
    public Option<string> Name => Source.NamedView | Source.Snapshot | Source.SpecificViewport;
}

public sealed record QueueBook(Seq<QueuedEntry> Entries) {
    // --- [RECORD]
    public static QueueBook Empty { get; } = new(Seq<QueuedEntry>());

    public static DictionaryCodec<QueueBook> Codec { get; } = DictionaryCodec.Json<QueueBook>(nameof(QueueBook), 3);

    // --- [TRANSITIONS]
    public QueueBook Added(Seq<QueuedEntry> entries) => new(entries.Fold(Entries, static (held, entry) => held.Exists(kept => kept with { Renders = entry.Renders } == entry) ? held : held.Add(entry)));

    public QueueBook Removed(LanguageExt.HashSet<int> indices) => new(toSeq(Entries.Where((_, index) => !indices.Contains(index))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Queues {
    // --- [RESOLUTION]
    public static IO<RenderBatch> Resolve(RhinoDoc doc, QueuePlan plan, Option<QueueProgress> resumed) =>
        Placed(doc, plan.Entries.Map(entry => entry.Placed(
            from frame in entry.Frames.Numbers
            from output in entry.Outputs
            select output with {
                Name = output.Name with {
                    Number = entry.Frames.Named(frame),
                    Version = resumed.Bind(record => record.Entries.Find(entry.Key)).Bind(held => held.Versions.Find(output.Name.Scope))
                        .Match<OutputVersioning>(Some: static version => version, None: static () => new OutputVersioning.NextFree()),
                },
            })), resumed.IsSome);

    internal static IO<RenderBatch> Placed(RhinoDoc doc, Seq<QueueEntry<Unit>> entries, bool overwrite) =>
        from groups in Callbacks.Each(toSeq(entries.GroupBy(entry => entry.Destination with { Overwrite = entry.Destination.Overwrite || overwrite })).Map(batch =>
            from files in IO.pure(from entry in toSeq(batch) from output in entry.Outputs select (entry.Key, Output: output))
            from placed in Destinations.Resolve(doc, batch.Key, files.Map(static file => file.Output.Name))
            select files.Zip(placed, static (file, row) => (file.Key, Output: new QueueOutput<OutputPath>(file.Output.Target, row.Name, row.Path)))))
        let outputs = groups.Bind(static batch => batch).ToLookup(static row => row.Key, static row => row.Output)
        select new RenderBatch(entries.Map(entry => entry.Placed(toSeq(outputs[entry.Key]))));

    private static (Seq<QueueRun> Sampled, Seq<QueueRun> Remaining) Order(RenderBatch queue, QueueProgress record, bool sampled) =>
        (from entry in queue.Entries
         let done = record.Entries.Find(entry.Key).Bind(static held => held.Last)
         let frames = entry.Frames.Numbers.Filter(frame => done.ForAll(last => frame > last))
         where !frames.IsEmpty
         select new QueueRun(entry, frames)) switch {
             var pending => sampled
                 ? (pending.Map(static run => run with { Frames = run.Frames.Take(2) }),
                    pending.Map(static run => run with { Frames = run.Frames.Skip(2) }).Filter(static run => !run.Frames.IsEmpty))
                 : (Seq<QueueRun>(), pending),
         };

    private sealed record QueueRun(QueueEntry<OutputPath> Entry, Seq<SequenceNumber> Frames);

    // --- [PLAN]
    public static IO<QueueBook> Book(RhinoDoc doc) =>
        ArchivableDictionaries.Read(RenderSettingsSources.Dictionary(doc), QueueBook.Codec).Map(static book => book.IfNone(QueueBook.Empty));

    public static IO<QueueBook> Change(RhinoDoc doc, string caption, Func<QueueBook, QueueBook> change) =>
        Commits.Commit(doc, caption, new RedrawPolicy.Silent(), RenderSettingsSources.Edit(doc, scope =>
            from before in ArchivableDictionaries.Read(RenderSettingsSources.Dictionary(scope), QueueBook.Codec)
            let next = change(before.IfNone(QueueBook.Empty))
            from written in ArchivableDictionaries.Write(RenderSettingsSources.Dictionary(scope), QueueBook.Codec, next)
            select next)).Map(static committed => committed.Value);

    public static Fin<QueuePlan> Plan(OutputBook outputs, Option<string> savedDocument, Seq<(QueuedEntry Entry, RenderOverride Override)> entries) =>
        Destinations.Resolved(outputs.Destination, savedDocument).Bind(destination =>
            entries.Traverse(item =>
                    item.Entry.Name.Traverse(NamePart.Validated).As()
                        .Bind(part => outputs.Named.Traverse(target =>
                                FileExtension.Validated(target.Target.FileFormat.Extension)
                                    .Map(extension => new QueueOutput<Unit>(target.Target, new OutputName(part.ToSeq(), new OutputVersioning.NextFree(), target.Parts, None, extension), unit)))
                            .As())
                        .Map(named => new QueueEntry<Unit>(item.Override, item.Entry.Source, item.Entry.Frames, item.Entry.Extent, item.Entry.Overscan,
                            item.Entry.Region, item.Entry.Sun, destination, named)))
                .As())
            .Bind(planned => QueuePlan.Create(outputs.Destination, planned));

    public static IO<QueuePlan> Plan(QueueContext context, RhinoDoc doc) =>
        from book in Book(doc)
        from outputs in TargetRows.Book(doc)
        from saved in IO.lift(() => Conversions.Present(doc.Path))
        from values in context.Bindings(doc).Bind(static bound => bound.Capture)
        from plan in IO.lift(Plan(outputs, saved, book.Entries.Filter(static entry => entry.Renders).Map(entry => (Entry: entry, Override: new RenderOverride(values, entry.Clay)))))
        select plan;

    // --- [RUN]
    public static IO<Unit> Run(QueueContext context, RhinoDoc doc, RenderBatch queue, Option<string> job, Option<QueueProgress> resumed) =>
        DisposalOps.Within<IDisposable, Unit>(retain =>
            from lease in QueueProgress.Acquire(context.Record, retain)
            from observed in QueueProgress.Observed(context)
            let settings = observed.Settings
            from free in IO.lift(observed.State.Switch(
                absent: static _ => Fin.Succ(unit),
                active: static active => new PreflightFinding.QueueActive(active.ProcessId).Error,
                ended: static _ => unit,
                crashed: static _ => unit))
            from started in QueueProgress.Started(job, queue, resumed, settings)
            from held in atomIO(started)
            from began in IO.lift(context.Sink.Clock.GetTimestamp)
            from ran in DisposalOps.OnFailure(
                        from written in QueueProgress.Write(context.Record, started)
                        from now in IO.lift(context.Sink.Clock.GetCurrentInstant)
                        from shown in context.Estimate.SwapIO(_ => QueueEstimate.Running(queue, started, None, now))
                        from noticed in unless(resumed.IsSome, context.Notify(new QueueEvent.Started(queue.Entries.Count, queue.Frames))).As()
                        from rendered in DisposalOps.Within<IDisposable, Unit>(acquired =>
                            from beat in Heartbeat(context, settings.Heartbeat, acquired)
                            from frames in Frames(context, doc, queue, held, resumed.IsNone && settings.SampleEstimate && queue.Entries.Count > 1)
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

    internal static bool Stopping(Error error) =>
        error.Is(Errors.Cancelled) || error.FoldM(static cause => cause is Ended ended ? Seq(ended.Result) : Seq<Result>()).As().Exists(static result => result is Result.ExitRhino);

    private static IO<ForkIO<Unit>> Heartbeat(QueueContext context, QueueHeartbeat heartbeat, Action<IDisposable> retain) =>
        DisposalOps.Fork(QueueProgress.Touch(context.Record, context.Sink.Clock)
            .Catch(error => IO.lift(() => context.Sink.Report(error, typeof(Queues), nameof(Heartbeat))))
            .As()
            .Repeat(Schedule.spaced(heartbeat.Interval.ToTimeSpan())), retain);

    private static IO<Unit> Closed(QueueContext context, Atom<QueueProgress> held, long began, RunEnd end, Func<QueueProgress, Duration, QueueEvent> notice) =>
        (from record in held.SwapIO(was => was with { Ended = Some(end) })
         from elapsed in IO.lift(() => context.Sink.Clock.GetElapsedTime(began).ToDuration())
         from published in Callbacks.Each(Seq(
             QueueProgress.Write(context.Record, record),
             context.Notify(notice(record, elapsed))))
         select unit).Uninterruptible();

    // --- [FRAMES]
    private const string RenderCommand = "Render";

    private sealed record RunRequest(QueueRun Run, Conduit<Fin<Unit>, Fin<Unit>> Reply);

    private sealed record RunScope(QueueContext Context, RhinoDoc Doc, RenderBatch Queue, Atom<QueueProgress> Held, Disposal<Eto.Forms.Application> Dock, Atom<Heard> Heard);

    private sealed record Heard(Option<long> Began, Option<(ImageFileEventArgs Event, long At)> Saved, Option<Result> Ended) {
        public static Heard Empty { get; } = new(None, None, None);
    }

    private static IO<Unit> Frames(QueueContext context, RhinoDoc doc, RenderBatch queue, Atom<QueueProgress> held, bool sampled) =>
        DisposalOps.Within<IDisposable, Unit>(retain =>
        from render in IO.lift(static () => Conversions.Required(Command.LookupCommandId(RenderCommand, searchForEnglishName: true), nameof(Command.LookupCommandId))).OnUiThread()
        from heard in atomIO(Heard.Empty)
        from dock in Notifications.Acquire(retain).OnUiThread()
        from listened in Listened(context, render, heard).TraverseM(acquire => acquire(retain)).As().OnUiThread()
        from tray in Notifications.Tray(context.Tray, context.Sink, retain).OnUiThread()
        from ran in Mailed(new RunScope(context, doc, queue, held, dock, heard), sampled)
        select ran, static owned => DisposalOps.Release(owned).OnUiThread());

    private static Seq<Func<Action<IDisposable>, IO<Unit>>> Listened(QueueContext context, Guid render, Atom<Heard> heard) =>
        [
            retain => EventKind.BeginCommand.Inline(args => when(args.CommandId == render,
                IO.lift(context.Sink.Clock.GetTimestamp).Bind(at => heard.SwapIO(_ => Heard.Empty with { Began = Some(at) })).Map(unit)).As(), context.Sink, retain),
            retain => EventKind.ImageFileSaved.Inline(args =>
                IO.lift(context.Sink.Clock.GetTimestamp).Bind(at => heard.SwapIO(was => was with { Saved = Some((args, at)) })).Map(unit), context.Sink, retain),
            retain => EventKind.EndCommand.Inline(args => when(args.CommandId == render,
                heard.SwapIO(was => was with { Ended = Some(args.CommandResult) }).Map(unit)).As(), context.Sink, retain),
        ];

    private static IO<Unit> Mailed(RunScope scope, bool sampled) =>
        DisposalOps.Within<IDisposable, Unit>(retain =>
                    from post in Subscriptions.Idle<Unit, RunRequest>(pending => toSeq(pending.Values).TraverseM(request => Turn(scope, request)).As().Map(unit))(
                        new CallbackSite(scope.Context.Sink, typeof(Queues), nameof(Frames)), retain).OnUiThread()
                    from order in IO.lift(() => Order(scope.Queue, scope.Held.Value, sampled))
                    from heads in order.Sampled.TraverseM(run => Requested(post, run)).As()
                    from estimate in unless(order.Sampled.IsEmpty,
                        IO.lift(scope.Context.Sink.Clock.GetCurrentInstant).Bind(now => scope.Context.Notify(new QueueEvent.Estimate(QueueEstimate.Sampled(scope.Queue, scope.Held.Value, now))))).As()
                    from remaining in order.Remaining.TraverseM(run => Requested(post, run)).As()
                    select unit,
                static owned => DisposalOps.Release(owned).OnUiThread());

    private static IO<Unit> Requested(Func<(Unit Key, RunRequest Value), IO<Unit>> post, QueueRun run) =>
        DisposalOps.Within(static () => Conduit.make<Fin<Unit>>(), reply =>
        from posted in post((unit, new RunRequest(run, reply)))
        from answer in reply.Source.Take(1).Last()
        from ran in IO.lift(answer)
        select ran, static reply => reply.Complete());

    private static IO<Unit> Turn(RunScope scope, RunRequest request) =>
        Ran(scope, request.Run).Bind(_ => request.Reply.Post(unit)).Catch(error => request.Reply.Post(error)).As();

    private static IO<Unit> Ran(RunScope scope, QueueRun run) =>
        from rendered in IO.lift(run.Entry.Rendered)
        from bindings in scope.Context.Bindings(scope.Doc)
        from bound in RenderOverrides.Group(bindings)
        from ran in RenderOverrides.Around(
            run.Entry.Override,
            new OverrideScope(scope.Doc, bound, scope.Context.Sink),
            Sized(scope.Doc, run.Entry, rendered).Bind(_ => Widened(scope, run, rendered)))
        select ran;

    private static IO<Unit> Sized(RhinoDoc doc, QueueEntry<OutputPath> entry, OverscanFrame rendered) =>
        RenderSettingsSources.Edit(new RenderSettingsSource.Live(doc), scope =>
            from source in RenderSourceState.Write(scope, entry.Source)
            from output in IO.lift(() => {
                scope.Settings.UseViewportSize = false;
                scope.Settings.ImageSize = new Size(rendered.Padded.Width, rendered.Padded.Height);
            })
            select unit);

    private static IO<Unit> Widened(RunScope scope, QueueRun run, OverscanFrame rendered) =>
        run.Entry.Overscan == Overscan.Off
            ? Lit(scope, run, rendered)
            : Viewports.With(scope.Doc, new ViewportTarget.Active(), row => Cameras.Within(row.Viewport, rendered.Widen, Lit(scope, run, rendered)));

    private static IO<Unit> Lit(RunScope scope, QueueRun run, OverscanFrame rendered) =>
        run.Entry.Sun.Match(
            Some: _ => SunState.Moment(new RenderSettingsSource.Live(scope.Doc), scope.Context.Sink.Clock) switch {
                var moment => moment.Read.Bind(prior => DisposalOps.Within(() => prior, _ => Looped(scope, run, rendered), value => moment.Put(value).Map(unit))),
            },
            None: () => Looped(scope, run, rendered));

    private static IO<Unit> Looped(RunScope scope, QueueRun run, OverscanFrame rendered) =>
        run.Frames.TraverseM(frame => Frame(scope, run.Entry, rendered, frame)).As().Map(unit);

    private static IO<Unit> Frame(RunScope scope, QueueEntry<OutputPath> entry, OverscanFrame rendered, SequenceNumber frame) =>
        from started in IO.lift(scope.Context.Sink.Clock.GetTimestamp)
        from moment in IO.lift(entry.Moment(frame))
        from lit in moment.Traverse(held => SunState.Moment(new RenderSettingsSource.Live(scope.Doc), scope.Context.Sink.Clock).Put(Some(held))).As()
        from framing in DisposalOps.Within(() => new RenderSourceView(scope.Doc), source =>
            DisposalOps.Found(source.GetViewInfo, nameof(RenderSourceView.GetViewInfo),
                view => RenderRuns.Framing(scope.Doc, view, rendered.Padded, entry.Frames.Time(frame),
                    error => IO.lift(() => scope.Context.Sink.Report(error, typeof(Queues), nameof(Frame))))))
        let lost = string.Create(CultureInfo.InvariantCulture, $"Frame {frame} ended without saved rendering")
        let render = from launched in Documents.RunScript(scope.Doc, $"_-{RenderCommand}", echo: false, display: None)
                     from heard in scope.Heard.ValueIO
                     from ended in IO.lift(heard.Ended.ToFin(Error.New(lost))
                         .Bind(result => Conversions.FromResult(result, RenderCommand).MapFail(error => Stopping(error) ? error : Error.New(lost, error))))
                     from began in IO.lift(heard.Began.ToFin(Error.New(lost)))
                     from saved in IO.lift(heard.Saved.ToFin(Error.New(lost)))
                     select (Saved: saved.Event, Value: (Began: began, Saved: saved))
        from timing in scope.Context.History.Match(
            Some: history => history.Launched(scope.Doc, render),
            None: () => render.Map(static completed => completed.Value))
        from written in new RenderWindowSource.Rendering(timing.Saved.Event.SessionId).Open(window =>
            Saves.Write(new SaveSource(scope.Doc, window, Some(framing)), scope.Context.Sink,
                entry.Outputs.Filter(output => output.Name.Number.IfNone(entry.Frames.First) == frame)
                    .Map(output => (output.Target, output.Path, entry.Region, rendered))))
        from closed in Documents.RunScript(scope.Doc, "_CloseRenderWindow", echo: false, display: None)
        from peak in DisposalOps.Within(Process.GetCurrentProcess, static self => IO.lift(() => self.PeakWorkingSet64))
        from at in IO.lift(scope.Context.Sink.Clock.GetTimestamp)
        from landed in Landed(scope, new FrameMark(entry.Key, frame, started, timing.Began, timing.Saved.At, at, peak))
        select unit;

    private static IO<Unit> Landed(RunScope scope, FrameMark mark) =>
        from record in scope.Held.SwapIO(was => mark.Time(scope.Context.Sink.Clock) switch {
            var time => was with {
                Entries = was.Entries.AddOrUpdate(
                    mark.Entry,
                    entry => entry with { Last = Some(mark.Frame), Times = entry.Times.Add(time) },
                    () => EntryProgress.Empty with { Last = Some(mark.Frame), Times = Seq(time) }),
            },
        })
        from written in QueueProgress.Write(scope.Context.Record, record)
        from now in IO.lift(scope.Context.Sink.Clock.GetCurrentInstant)
        from estimate in scope.Context.Estimate.SwapIO(_ => QueueEstimate.Running(scope.Queue, record, Some(mark), now))
        from docked in Docked(scope, estimate)
        select unit;

    private static IO<Unit> Docked(RunScope scope, QueueEstimate estimate) =>
        from progress in Notifications.SetProgress(scope.Dock, estimate.Work)
        from badge in Notifications.BadgeLabel(scope.Dock, Some(estimate.Total - estimate.Landed)
            .Filter(static left => left > 0).Map(static left => left.ToString(RowText.Culture)))
        select unit;

    // --- [RESUME]
    public static IO<Option<Relaunch>> Resumable(QueueContext context) =>
        DisposalOps.Within<IDisposable, Option<Relaunch>>(retain =>
        from lease in QueueProgress.Acquire(context.Record, retain)
        from observed in QueueProgress.Observed(context)
        let crashed = observed.State.Map(absent: false, active: false, ended: false, crashed: true) ? observed.Prior.Filter(static record => record.Relaunch.IsSome) : None
        from relaunch in crashed.Match(
            Some: record => record.Retries.Spent.Match(
                Some: left => QueueProgress.Write(context.Record, record with { Retries = left }).Map(_ => record.Relaunch),
                None: () => Exhausted(context, record)),
            None: static () => IO.pure(Option<Relaunch>.None))
        select relaunch, DisposalOps.Release);

    private static IO<Option<Relaunch>> Exhausted(QueueContext context, QueueProgress record) =>
        from settings in context.Settings
        from noticed in context.Notify(new QueueEvent.RetriesExhausted(settings.Retries))
        from written in QueueProgress.Write(context.Record, record with { Ended = Some(RunEnd.Failed) })
        select Option<Relaunch>.None;

    // --- [REGISTRATION]
    public static Action<ILoggingBuilder> Logging { get; } = static logging => logging.AddConsole(static options => options.LogToStandardErrorThreshold = LogLevel.Error);

    public static IO<QueueContext> Acquire(
        OutputPath record,
        IO<QueueSettings> settings,
        IPlugInViews sink,
        Func<RhinoDoc, IO<BindingGroup>> bindings,
        Option<RenderHistory> history,
        TrayRow tray,
        NoticeOptions notices,
        string jobCommand,
        string farmScript,
        Action<IDisposable> retain) =>
        from sender in NoticeSender.Acquire(notices, sink.Clock, sink, retain)
        from estimate in atomIO(QueueEstimate.Idle)
        select new QueueContext(record, settings, sink, bindings, history, estimate, tray, sender.Notify, jobCommand, farmScript, retain);

    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Register(QueueSettingRows settings, View.Panel reveal) =>
        (sink, retain) => DisposalOps.Acquire(acquired =>
            from icon in IO.lift(reveal.Icon.ToFin(new Refused(nameof(View.Panel.Icon), Refused.Absent)))
            from record in IO.lift(() => OutputPath.Validated(Path.Combine(Directory.CreateDirectory(sink.PlugIn.SettingsDirectory).FullName, "render-queue.json")))
            from history in RenderHistory.Current.Of(sink)
            from commands in IO.lift(() => toSeq(sink.PlugIn.GetCommands()))
            from named in IO.lift((Named<QueueJobCommand>(commands), Named<QueueFarmJobCommand>(commands))
                .Apply(static (job, farm) => (Job: job, Farm: $"_-{farm}")).As())
            from context in Acquire(record, settings.Settings(sink.Settings), sink, history.Bindings, Some(history),
                new TrayRow(reveal.Caption, icon, [[new MenuEntry.Listed(reveal.Caption, IO.pure(Seq<MenuPick>(
                    new MenuPick.Item(reveal.Caption, Marked: false, Showing.Open(reveal)))))]]),
                new NoticeOptions(settings.Notices(sink.Settings), Some(icon), Some(reveal)),
                named.Job, named.Farm, acquired)
            from filled in QueueContext.Current.Publish(sink, context)
            from farm in Dispatch.FarmStartup(named.Farm)
            from relaunched in unless(farm, Dispatch.Relaunched(context)).As()
            select unit, retain);

    private static Fin<string> Named<TCommand>(Seq<Command> commands) where TCommand : Command =>
        commands.Find(static command => command is TCommand).Map(static command => command.EnglishName).ToFin(Error.New($"Plug-in registers no {typeof(TCommand).Name}"));
}
