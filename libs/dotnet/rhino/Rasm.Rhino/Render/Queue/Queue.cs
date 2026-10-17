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
using Rasm.Rhino.Render.Presets;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
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

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct RetryDelay : System.Numerics.IMinMaxValue<RetryDelay> {
    public static RetryDelay MinValue { get; } = new(0);
    public static RetryDelay MaxValue { get; } = new(600);
    public static RetryDelay Default { get; } = new(1);

    public static Presentation<int> Presentation { get; } = new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    public Duration Span => Duration.FromSeconds(_value);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
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
        (SequenceNumber.Validated(first).ToValidation(), SequenceNumber.Validated(last).ToValidation())
            .Apply(static (from, until) => (From: from, Until: until))
            .As()
            .ToFin()
            .Bind(static span => Admission.Admitted(Validate(span.From, span.Until, out FrameSpan? spanned), spanned));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref SequenceNumber first, ref SequenceNumber last) =>
        validationError = first <= last && (long)(int)last - (int)first + 1 <= int.MaxValue ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"frame span {first} to {last} is reversed or longer than {int.MaxValue} frames"));
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
            var moments => moments.At((int)frame).ToFin(Error.New(string.Create(CultureInfo.InvariantCulture, $"Frame {frame} lies past the {moments.Count} moments of the sun study")))
                .Bind(Admission.Validated<SunMoment, LocalDateTime>),
        }).As();

    public QueueEntry<TPlaced> Placed<TPlaced>(Seq<QueueOutput<TPlaced>> outputs) => new(Override, Source, Frames, Extent, Overscan, Region, Sun, Destination, outputs);
}

public sealed class QueuePlan {
    private QueuePlan(Destination destination, Seq<QueueEntry<Unit>> entries) => (Destination, Entries) = (destination, entries);

    public Destination Destination { get; }
    public Seq<QueueEntry<Unit>> Entries { get; }

    public static Fin<Seq<QueueEntry<TPath>>> Unique<TPath>(Seq<QueueEntry<TPath>> entries) =>
        Callbacks.Unique(entries, static entry => entry.Key, nameof(QueuePlan)).ToFin();

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
    PlugInSetting<NoticeServer, string> Server,
    PlugInSetting<Option<NoticeTopic>, string> Topic,
    FlamencoSettingRows Flamenco) {
    public static QueueSettingRows Default { get; } = new(
        Retries: new(ValueKey.Numeric("QueueRetries", "Retries", "Times a crashed queue reopens Rhino and resumes", QueueSettings.Default.Retries,
            NumericDomain.Bounded<RetryCount, int>(new() { Form = NumberForm.Field })), SettingType.Integer),
        Delay: new(ValueKey.Numeric("QueueRetryDelay", "Retry delay", "Wait before a crashed queue reopens", QueueSettings.Default.Delay,
            NumericDomain.Bounded<RetryDelay, int>(RetryDelay.Presentation)), SettingType.Integer),
        SampleEstimate: new(ValueKey.Raw("QueueSampleEstimate", "Estimate from first frames",
            "Renders each entry's first two frames ahead of the rest and posts the finish they estimate", QueueSettings.Default.SampleEstimate), SettingType.Bool),
        KeepAwake: new(ValueKey.Raw("KeepAwake", "Keep computer awake", "Holds off system sleep from the first queued render to the last", QueueSettings.Default.KeepAwake), SettingType.Bool),
        ShutDownAfterSuccess: new(ValueKey.Raw("ShutDownAfterSuccess", "Shut down when done", "Shuts the computer down 60 seconds after every queued render succeeds",
            QueueSettings.Default.ShutDownAfterSuccess), SettingType.Bool),
        HeartbeatInterval: new(ValueKey.Numeric("QueueHeartbeatInterval", "Heartbeat interval", "Time between queue heartbeat updates", QueueHeartbeat.Default.Interval,
            new NumericDomain<Duration, double>(static seconds => Fin.Succ(Duration.FromSeconds(seconds)), static value => value.TotalSeconds,
                (QueueHeartbeat.Intervals.Low.TotalSeconds, QueueHeartbeat.Intervals.High.TotalSeconds), new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Second) })),
            SettingType.Double),
        HeartbeatMisses: new(ValueKey.Numeric("QueueHeartbeatMisses", "Missed heartbeats", "Missed heartbeat intervals before a queue is considered crashed", QueueHeartbeat.Default.Misses,
            new NumericDomain<int, int>(Fin.Succ, identity, QueueHeartbeat.MissedIntervals, new() { Form = NumberForm.Field })), SettingType.Integer),
        Server: new(ValueKey.Of<NoticeServer, string>("NtfyServer", "Server", "ntfy server the render queue posts notices to", NoticeSettings.Default.Server), SettingType.String),
        Topic: new(ValueKey.Optional<NoticeTopic>("NtfyTopic", "Topic", "ntfy topic the render queue posts notices to, empty for none"), SettingType.String),
        Flamenco: FlamencoSettingRows.Default);

    public Seq<PlugInSetting> Rows => [Retries, Delay, SampleEstimate, KeepAwake, ShutDownAfterSuccess, HeartbeatInterval, HeartbeatMisses, Server, Topic, .. Flamenco.Rows];

    public Seq<Child> Children => [
        new ControlRow.Group("Retries and power", RowRules.Always) { Wording = Wording.Library },
        .. SectionRows.Setting(Retries, RowRules.Always),
        .. SectionRows.Setting(Delay, RowRules.Always),
        .. SectionRows.Setting(SampleEstimate, RowRules.Always),
        .. SectionRows.Setting(KeepAwake, RowRules.Always),
        .. SectionRows.Setting(ShutDownAfterSuccess, RowRules.Always),
        new ControlRow.Group("Heartbeat", RowRules.Always) { Wording = Wording.Library },
        .. SectionRows.Setting(HeartbeatInterval, RowRules.Always),
        .. SectionRows.Setting(HeartbeatMisses, RowRules.Always),
        new ControlRow.Group("Notices", RowRules.Always) { Wording = Wording.Library },
        .. SectionRows.Setting(Server, RowRules.Always),
        .. SectionRows.Setting(Topic, RowRules.Always),
        new ControlRow.Group("Flamenco", RowRules.Always) { Wording = Wording.Library },
        .. Flamenco.Children,
    ];

    public IO<QueueSettings> Settings(ISettingReader reader) =>
        (reader.Current(Retries), reader.Current(Delay), reader.Current(SampleEstimate),
         reader.Current(KeepAwake), reader.Current(ShutDownAfterSuccess), reader.Current(HeartbeatInterval), reader.Current(HeartbeatMisses))
            .Apply(static (retries, delay, sampled, awake, shutdown, interval, misses) =>
                Admission.Admitted(QueueHeartbeat.Validate(interval, misses, out QueueHeartbeat? heartbeat), heartbeat)
                    .Map(held => new QueueSettings(retries, delay, sampled, awake, shutdown, held)))
            .As().Bind(IO.lift);

    public IO<NoticeSettings> Notices(ISettingReader reader) =>
        (reader.Current(Server), reader.Current(Topic))
            .Apply(static (server, topic) => new NoticeSettings(server, topic)).As();
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
    Option<SetName> Set,
    Option<Albedo> Clay,
    RenderSourceState Source,
    FrameSpan Frames,
    PixelExtent Extent,
    Overscan Overscan,
    Option<RenderRegion> Region,
    Option<SunWindow> Sun) {
    public Option<string> Name => Set.Map(static name => (string)name) | Source.NamedView | Source.Snapshot | Source.SpecificViewport;
}

public sealed record QueueBook(Seq<QueuedEntry> Entries) {
    // --- [RECORD]
    public static QueueBook Empty { get; } = new(Seq<QueuedEntry>());

    public static DictionaryCodec<QueueBook> Codec { get; } = DictionaryCodec.Json<QueueBook>(nameof(QueueBook), 2);

    // --- [TRANSITIONS]
    public QueueBook Added(Seq<QueuedEntry> entries) => new(entries.Fold(Entries, static (held, entry) => held.Exists(entry.Equals) ? held : held.Add(entry)));

    public QueueBook Removed(LanguageExt.HashSet<QueuedEntry> entries) => new(Entries.Filter(entry => !entries.Contains(entry)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Queues {
    // --- [RESOLUTION]
    public static IO<RenderOverride> Override(QueueContext context, RhinoDoc doc, Option<SetName> set, Option<Albedo> clay) =>
        set.Match(
            Some: name =>
                from book in RenderSets.Book(doc)
                from setValue in IO.lift(book.Find(name))
                from member in unless(setValue.Membership.Renders, IO.fail<Unit>(Error.New($"Render set {name} is excluded from render"))).As()
                from values in setValue.Stored.Match(Some: static stored => IO.pure(stored), None: () => context.Bindings(doc).Bind(static @group => @group.Capture))
                select new RenderOverride(name, values, clay),
            None: () => context.Bindings(doc).Bind(static @group => @group.Capture).Map(values => new RenderOverride("Current settings", values, clay)));

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
                    item.Entry.Name.Traverse(NamePart.Validated).As().ToValidation()
                        .Bind(part => outputs.Named.Traverse(target =>
                                FileExtension.Validated(target.Target.FileFormat.Extension).ToValidation()
                                    .Map(extension => new QueueOutput<Unit>(target.Target, new OutputName(part.ToSeq(), new OutputVersioning.NextFree(), target.Parts, None, extension), unit)))
                            .As())
                        .Map(named => new QueueEntry<Unit>(item.Override, item.Entry.Source, item.Entry.Frames, item.Entry.Extent, item.Entry.Overscan,
                            item.Entry.Region, item.Entry.Sun, destination, named)))
                .As().ToFin())
            .Bind(planned => QueuePlan.Create(outputs.Destination, planned));

    public static IO<QueuePlan> Plan(QueueContext context, RhinoDoc doc) =>
        from book in Book(doc)
        from outputs in TargetRows.Book(doc)
        from saved in IO.lift(() => Conversions.Present(doc.Path))
        from entries in book.Entries.TraverseM(entry =>
                Override(context, doc, entry.Set, entry.Clay).Map(held => (Entry: entry, Override: held)))
            .As()
        from plan in IO.lift(Plan(outputs, saved, entries))
        select plan;

    // --- [RUN]
    public static IO<Unit> Run(QueueContext context, RhinoDoc doc, RenderBatch queue, Option<string> job, Option<QueueProgress> resumed) =>
        DisposalOps.Within<IDisposable, Unit>(retain =>
        from lease in QueueProgress.Acquire(context.Record, retain)
        from observed in QueueProgress.Observed(context)
        let settings = observed.Settings
        from free in IO.lift(observed.State.Switch(
            absent: static _ => Fin.Succ(unit),
            active: static active => Fin.Fail<Unit>(new PreflightFinding.QueueActive(active.ProcessId).Error),
            ended: static _ => Fin.Succ(unit),
            crashed: static _ => Fin.Succ(unit)))
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
        Seq<Func<Action<IDisposable>, IO<Unit>>>(
            retain => EventKind.BeginCommand.Inline(args => when(args.CommandId == render,
                IO.lift(context.Sink.Clock.GetTimestamp).Bind(at => heard.SwapIO(_ => Heard.Empty with { Began = Some(at) })).Map(unit)).As(), context.Sink, retain),
            retain => EventKind.ImageFileSaved.Inline(args =>
                IO.lift(context.Sink.Clock.GetTimestamp).Bind(at => heard.SwapIO(was => was with { Saved = Some((args, at)) })).Map(unit), context.Sink, retain),
            retain => EventKind.EndCommand.Inline(args => when(args.CommandId == render,
                heard.SwapIO(was => was with { Ended = Some(args.CommandResult) }).Map(unit)).As(), context.Sink, retain));

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
            : from viewport in IO.lift(() => Refused.UnlessFound(scope.Doc.Views.ActiveView, nameof(ViewTable.ActiveView)).Map(static view => view.ActiveViewport))
              from ran in DisposalOps.Within(() => new ViewportInfo(viewport),
                  _ => Projected(viewport, rendered).Bind(_ => Lit(scope, run, rendered)),
                  held => Callbacks.Each(Seq(
                      IO.lift(() => Refused.Unless(viewport.SetViewProjection(held, updateTargetLocation: false), nameof(RhinoViewport.SetViewProjection))),
                      IO.lift(held.Dispose))).Map(unit))
              select ran;

    private static IO<Unit> Projected(RhinoViewport viewport, OverscanFrame frame) =>
        DisposalOps.Within(() => new ViewportInfo(viewport), copy =>
            from widened in frame.Widen(copy)
            from applied in IO.lift(() => Refused.Unless(viewport.SetViewProjection(copy, updateTargetLocation: false), nameof(RhinoViewport.SetViewProjection)))
            select applied);

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
        let lost = string.Create(CultureInfo.InvariantCulture, $"Frame {frame} ended without a saved rendering")
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
            from named in IO.lift((Named<QueueJobCommand>(commands).ToValidation(), Named<QueueFarmJobCommand>(commands).ToValidation())
                .Apply(static (job, farm) => (Job: job, Farm: $"_-{farm}")).As().ToFin())
            from context in Acquire(record, settings.Settings(sink.Settings), sink, history.Bindings, Some(history),
                new TrayRow(reveal.Caption, icon, Seq(Seq<MenuEntry>(new MenuEntry.Listed(reveal.Caption, IO.pure(Seq<MenuPick>(
                    new MenuPick.Item(reveal.Caption, Marked: false, Showing.Open(reveal)))))))),
                new NoticeOptions(settings.Notices(sink.Settings), Some(icon), Some(reveal)),
                named.Job, named.Farm, acquired)
            from filled in QueueContext.Current.Publish(sink, context)
            from farm in Dispatch.FarmStartup(named.Farm)
            from relaunched in unless(farm, Dispatch.Relaunched(context)).As()
            select unit, retain);

    private static Fin<string> Named<TCommand>(Seq<Command> commands) where TCommand : Command =>
        commands.Find(static command => command is TCommand).Map(static command => command.EnglishName).ToFin(Error.New($"the plug-in registers no {typeof(TCommand).Name}"));

    // --- [ROWS]
    private const string Owner = "render-queue";

    private static readonly Func<Destination, IO<QueueTarget>> Local = static _ => IO.pure<QueueTarget>(new QueueTarget.Local());

    private static readonly Func<QueueContext, IO<Option<QueueProgress>>> Fresh = static _ => IO.pure(Option<QueueProgress>.None);

    private static readonly Func<QueueContext, IO<Option<QueueProgress>>> Unfinished =
        static context => QueueProgress.Read(context.Record).Map(static prior => prior.Filter(static record => !record.Succeeded));

    private static readonly (RowSource<Seq<QueuedEntry>> Source, RowField<Seq<QueuedEntry>> Field) Listed =
        RowSource.Opaque(Owner, "entries", Seq<QueuedEntry>(), static scope => ValueStore.Readout(scope.DocumentIO.Bind(Book).Map(static book => book.Entries), None));

    private static readonly (RowSource<LanguageExt.HashSet<QueuedEntry>> Source, RowField<LanguageExt.HashSet<QueuedEntry>> Field) Picked =
        RowSource.Opaque(Owner, "picked", LanguageExt.HashSet<QueuedEntry>.Empty);

    private static readonly (RowSource<bool> Source, RowField<bool> Field) Clayed = RowSource.Opaque(Owner, "clay", @default: false);

    private static readonly (RowSource<Option<Preflight>> Source, RowField<Option<Preflight>> Field) Report = RowSource.Opaque(Owner, "check", Option<Preflight>.None);

    private static readonly ReadoutSource<(QueueEstimate Estimate, TimeProvider Clock, DateTimeZone Zone)> Zoned =
        new ReadoutSource<(QueueEstimate Estimate, TimeProvider Clock, DateTimeZone Zone)>.Scoped(static scope =>
            from context in QueueContext.Current.Of(scope.Sink)
            from estimate in context.Estimate.ValueIO
            from zone in IO.lift(() => context.Sink.Clock.ToZonedClock().Zone)
            select (estimate, context.Sink.Clock, zone));

    public static Seq<Child> Rows(QueueSettingRows settings) {
        ReadoutSource<Option<Preflight>> check = new ReadoutSource<Option<Preflight>>.Scoped(static scope => scope.Read(Report.Source));
        Func<RowScope, IO<RunState>> state = static scope =>
            QueueContext.Current.Of(scope.Sink).Bind(QueueProgress.Observed).Map(static observed => observed.State);
        RowRule ready = new([], [], scope => state(scope).Map(static current => current.Map(absent: true, active: false, ended: true, crashed: true)));
        RowRule resumable = new([], [], scope => state(scope).Map(static current => current.Switch(
            absent: static _ => false, active: static _ => false, ended: static ended => ended.End != RunEnd.Succeeded, crashed: static _ => true)));
        ReadoutSource<(RunState State, DateTimeZone Zone)> stated = new ReadoutSource<(RunState State, DateTimeZone Zone)>.Scoped(scope =>
            from current in state(scope)
            from zone in IO.lift(() => ((IPlugInViews)scope.Sink).Clock.ToZonedClock().Zone)
            select (current, zone));
        const string entryCaption = "Entry";
        const string count = "{0:N0}";
        const string bytes = "{0:N0} bytes";
        Seq<(string Caption, string Format, Func<Preflight, Option<object>> Value)> measures = Seq<(string, string, Func<Preflight, Option<object>>)>(
            ("Objects", count, static report => Some<object>(report.Statistics.ObjectCount)),
            ("Layer levels", count, static report => Some<object>(report.Statistics.LayerLevelCount)),
            ("Block placements", count, static report => Some<object>(report.Statistics.PlacementCount)),
            ("Geometry memory", bytes, static report => Some<object>(report.Statistics.MemoryEstimate)),
            ("File size", bytes, static report => report.Statistics.FileLength.Map(static length => (object)length)));
        return [
            new ControlRow.Group("Entries", RowRules.Always) { Wording = Wording.Library },
            ListRows.List("Entries", "Renders of this document's queue, in add order", new ListRow<QueuedEntry, QueuedEntry>(
                    IterableNE.create<ListColumn<QueuedEntry>>(
                        new ListColumn<QueuedEntry>.Label(Some(entryCaption), static entry => entry.Name.IfNone(static () => RowText.Localize("Active viewport")), None, None),
                        new ListColumn<QueuedEntry>.Label(Some("Frames"), static entry => entry.Frames.Count > 1
                            ? RowText.Localize("{0} to {1}", arguments: [(int)entry.Frames.First, (int)entry.Frames.Last])
                            : ((int)entry.Frames.First).ToString(RowText.Culture), None, None),
                        new ListColumn<QueuedEntry>.Label(Some("Size"), static entry => RowText.Localize("{0} × {1}", arguments: [entry.Extent.Width, entry.Extent.Height]), None, None)),
                    static entry => entry,
                    Listed,
                    Picked) {
                AllowMultipleSelection = true,
                Wording = Wording.Library,
                Pair = Some((
                        new CommandRow.Run(Face("render-queue-add-view", "Add Active Viewport", "Queue the active viewport at the document's render size"), None,
                            static scope => Added(scope, static _ => IO.pure(Seq(Placement.Active)))),
                        new CommandRow.Run(Face("render-queue-remove", "Remove Entries", "Remove the selected entries"), None, static scope =>
                            from doc in scope.DocumentIO
                            from picked in scope.Read(Picked.Source)
                            from removed in Change(doc, RowText.Localize("Remove Entries"), was => was.Removed(picked))
                            select unit))),
                Commands = _ => Seq<CommandRow>(
                    new CommandRow.Run(Face("render-queue-add-named-views", "Add Named Views", "Queue every named view of the document"), None, static scope => Added(scope, NamedViewEntries)),
                    new CommandRow.Run(Face("render-queue-add-snapshots", "Add Snapshots", "Queue every snapshot of the document"), None, static scope => Added(scope, SnapshotEntries)),
                    new CommandRow.Run(Face("render-queue-add-sets", "Add Render Sets", "Queue every render set that renders"), None, static scope => Added(scope, SetEntries)),
                    new CommandRow.Run(Face("render-queue-add-study", "Add Sun Study", "Queue the document's sun study, one frame per moment"), None, static scope => Added(scope, StudyEntries)),
                    new CommandRow.Check(Face("render-queue-clay", "Clay", "Render each entry added next with every object in clay"), None,
                        static scope => scope.Read(Clayed.Source), static (scope, on) => scope.Store(Clayed.Source).Put(Some(on)).Map(unit)),
                    new CommandRow.Run(Face("render-queue-check", "Check", "Check the queue before it renders"), None, static scope => Checked(scope, Local)),
                    new CommandRow.Run(Face("render-queue-render", "Render", "Render the queue in this Rhino"), Some(ready), static scope => Sent(scope, Local, Fresh)),
                    new CommandRow.Run(Face("render-queue-handoff", "Render in New Rhino", "Save, quit, and render the queue in a new Rhino behind the frontmost window"), Some(ready),
                        static scope => Sent(scope, Packaged(static package => IO.pure<QueueTarget>(new QueueTarget.Handoff(package))), Fresh)),
                    new CommandRow.Run(Face("render-queue-job", "Write Job", "Write the copy, the job, and its schema beside the outputs"), Some(ready),
                        static scope => Sent(scope, Packaged(static package => IO.pure<QueueTarget>(new QueueTarget.JobFile(package))), Fresh)),
                    new CommandRow.Run(Face("render-queue-flamenco", "Submit to Flamenco", "Package the queue and submit it to the configured Flamenco Manager"), Some(ready),
                        scope => Sent(scope, Packaged(package => settings.Flamenco.Target(((IPlugInViews)scope.Sink).Settings, package)), Fresh)),
                    new CommandRow.Run(Face("render-queue-resume", "Resume", "Render the stopped, failed, or crashed queue from its last frame"), Some(resumable),
                        static scope => Sent(scope, Local, Unfinished)),
                    new CommandRow.Run(Face("render-queue-test-notice", "Send Test Notice", "Post a test notice to Rhino, the system, and the ntfy topic"), None, static scope =>
                        QueueContext.Current.Of(scope.Sink)
                            .Bind(static context => DisposalOps.Fork(context.Notify(new QueueEvent.Test()), context.Retain))
                            .Map(unit))),
            },
                RowRules.Always),
            new ControlRow.Group("Run", RowRules.Always) { Wording = Wording.Library },
            TextRows.Progress("Progress", "Frames rendered of the queue's total", new ReadoutSource<WorkState>.Scoped(static scope =>
                QueueContext.Current.Of(scope.Sink).Bind(static context => context.Estimate.ValueIO).Map(static estimate => estimate.Work)), RowRules.Always) with { Wording = Wording.Library, },
            TextRows.Readout("State", "The queue record's state", stated, static shown => Stated(shown.State, shown.Zone), RowRules.Always) with { Wording = Wording.Library },
            TextRows.Readout("Remaining", "Time left and finish, from the frames rendered", Zoned, static shown => shown.Estimate.Description(shown.Zone), RowRules.Always) with { Wording = Wording.Library },
            .. toSeq(FrameStage.Items).Map(static stage => TextRows.Readout(stage.Caption,
                "Time spent in this stage of the last completed frame", Zoned,
                shown => shown.Estimate.Last.Map<string>(mark => RowText.Localize("{0:N3} s", arguments: [stage.Of(mark, shown.Clock).TotalSeconds])).IfNone(""),
                RowRules.Always) with { Wording = Wording.Library, }),
            TextRows.Readout("Peak memory", "Peak process memory after the last completed frame", Zoned,
                static shown => shown.Estimate.Last.Map<string>(static mark => RowText.Localize(bytes, arguments: [mark.PeakWorkingSet])).IfNone(""), RowRules.Always) with { Wording = Wording.Library, },
            .. measures.Map(measure => TextRows.Readout(measure.Caption, "Measured in the last queue check", check,
                report => report.Bind(measure.Value).Map<string>(value => RowText.Localize(measure.Format, arguments: [value])).IfNone(""), RowRules.Always) with { Wording = Wording.Library, }),
            ListRows.List("Estimated memory", "Scene and requested render channels per queue entry, from the last check", new ListRow<(int Entry, ulong Memory), int>(
                    IterableNE.create<ListColumn<(int Entry, ulong Memory)>>(
                        new ListColumn<(int Entry, ulong Memory)>.Label(Some(entryCaption), static entry => (entry.Entry + 1).ToString(RowText.Culture), None, None),
                        new ListColumn<(int Entry, ulong Memory)>.Label(Some("Memory"), static entry => RowText.Localize(bytes, arguments: [entry.Memory]), None, None)),
                    static entry => entry.Entry,
                    RowSource.Opaque(Owner, "memory", Seq<(int Entry, ulong Memory)>(), static scope => ValueStore.Readout(
                        scope.Read(Report.Source).Map(static report => report.Map(static held => toSeq(held.Memory.AsIterable()).Map(static entry => (Entry: entry.Key, Memory: entry.Value))).IfNone(Seq<(int Entry, ulong Memory)>())),
                        None)),
                    RowSource.Opaque(Owner, "memory-picked", LanguageExt.HashSet<int>.Empty)) { Wording = Wording.Library }, RowRules.Always),
            ListRows.List("Check", "Refusals and cautions of the last check", new ListRow<PreflightFinding, PreflightFinding>(
                    IterableNE.create<ListColumn<PreflightFinding>>(new ListColumn<PreflightFinding>.Label(None, static finding => ErrorOps.Localize(finding.Error), None, None)),
                    static finding => finding,
                    RowSource.Opaque(Owner, "findings", Seq<PreflightFinding>(), static scope => ValueStore.Readout(
                        scope.Read(Report.Source).Map(static check => check.Map(static held => held.Refusals + held.Cautions).IfNone(Seq<PreflightFinding>())),
                        None)),
                    RowSource.Opaque(Owner, "finding", LanguageExt.HashSet<PreflightFinding>.Empty)) { Wording = Wording.Library },
                RowRules.Always),
            .. settings.Children,
        ];
    }

    private sealed record Placement(Option<SetName> Set, RenderSourceState Source, Option<SunWindow> Sun) {
        public static Placement Active { get; } = new(None, new RenderSourceState(RenderSettings.RenderingSources.ActiveViewport, None, None, None), None);

        public static Placement Named(ViewInfo view) => Active with { Source = new RenderSourceState(RenderSettings.RenderingSources.NamedView, None, Some(view.Name), None) };

        public static Placement Snapshot(string name) => Active with { Source = new RenderSourceState(RenderSettings.RenderingSources.SnapShot, None, None, Some(name)) };

        public static Placement Rendered(SetName set) => Active with { Set = Some(set) };

        public static Placement Studied(SunWindow window) => Active with { Sun = Some(window) };

        public Fin<QueuedEntry> Queued(Option<Albedo> clay, PixelExtent extent) =>
            FrameSpan.Of(0, Sun.Map(static window => window.Frames.Count - 1).IfNone(0))
                .Map(frames => new QueuedEntry(Set, clay, Source, frames, extent, Overscan.Off, None, Sun));
    }

    private static CommandFace Face(string name, string caption, string help) =>
        new(name, caption, help, None, None) { Wording = Wording.Library };

    private static IO<Unit> Added(RowScope scope, Func<RhinoDoc, IO<Seq<Placement>>> read) =>
        from doc in scope.DocumentIO
        from clay in scope.Read(Clayed.Source)
        from view in IO.lift(() => Refused.UnlessFound(doc.Views.ActiveView, nameof(ViewTable.ActiveView)))
        from output in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), ImageOutputState.Read)
        from extent in IO.lift(Framing.Final(output, view.ActiveViewport.Size))
        from placed in read(doc)
        from entries in IO.lift(Callbacks.Each(placed, (row, _) => row.Queued(Callbacks.Found(clay, Albedo.Clay), extent)))
        from added in Change(doc, RowText.Localize("Add Entries"), was => was.Added(entries))
        select unit;

    private static IO<Seq<Placement>> NamedViewEntries(RhinoDoc doc) =>
        IO.lift(() => doc.NamedViews).Bind(static table =>
            toSeq(Enumerable.Range(0, table.Count)).TraverseM(index => DisposalOps.Found(() => table[index], nameof(NamedViewTable),
                static view => IO.pure(Placement.Named(view)))).As());

    private static IO<Seq<Placement>> SnapshotEntries(RhinoDoc doc) =>
        NamedSnapshots.Names(doc).Map(static names => names.Map(Placement.Snapshot));

    private static IO<Seq<Placement>> SetEntries(RhinoDoc doc) =>
        RenderSets.Renderable(doc).Map(static names => names.Map(Placement.Rendered));

    private static IO<Seq<Placement>> StudyEntries(RhinoDoc doc) =>
        DisposalOps.Within(() => doc.AnimationProperties, static copy => IO.lift(() => SunWindow.Read(copy)))
            .Bind(static study => IO.lift(study.ToFin(new Refused(nameof(RhinoDoc.AnimationProperties), Refused.Absent))))
            .Map(static window => Seq(Placement.Studied(window)));

    private static IO<Unit> Checked(RowScope scope, Func<Destination, IO<QueueTarget>> target) =>
        from doc in scope.DocumentIO
        from context in QueueContext.Current.Of(scope.Sink)
        from plan in Plan(context, doc)
        from chosen in target(plan.Destination)
        from report in Preflight.Check(context, doc, plan, chosen)
        from shown in Shown(scope)(report)
        select shown;

    private static IO<Unit> Sent(RowScope scope, Func<Destination, IO<QueueTarget>> target, Func<QueueContext, IO<Option<QueueProgress>>> resumed) =>
        from doc in scope.DocumentIO
        from context in QueueContext.Current.Of(scope.Sink)
        from plan in Plan(context, doc)
        from chosen in target(plan.Destination)
        from record in resumed(context)
        from stamp in IO.lift(() => context.Sink.Clock.ToZonedClock().GetCurrentLocalDateTime())
        from sent in Dispatch.Send(context, new DocumentSource.Live(doc), plan, chosen, stamp, record, Some(Shown(scope)))
        select sent;

    private static Func<Preflight, IO<Unit>> Shown(RowScope scope) => report => scope.Store(Report.Source).Put(Some(Some(report))).Map(unit);

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
