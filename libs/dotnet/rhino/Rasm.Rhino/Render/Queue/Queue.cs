using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using NodaTime.Text;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Geolocation;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Slots;
using Rasm.Rhino.UI.Chrome;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct RetryCount : System.Numerics.IMinMaxValue<RetryCount> {
    public static RetryCount MinValue { get; } = new(0);
    public static RetryCount MaxValue { get; } = new(20);
    public static RetryCount Default { get; } = new(3);

    public Option<RetryCount> Spent => _value > MinValue._value ? new RetryCount(_value - 1) : None;

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct RetryDelay : System.Numerics.IMinMaxValue<RetryDelay> {
    public static RetryDelay MinValue { get; } = new(0);
    public static RetryDelay MaxValue { get; } = new(600);
    public static RetryDelay Default { get; } = new(1);

    public static Presentation<RetryDelay, int> Presentation { get; } = new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

    public Duration Span => Duration.FromSeconds(_value);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class FrameSpan {
    public const double Rate = 24d;

    public SequenceNumber First { get; }
    public SequenceNumber Last { get; }

    public int Count => (int)Last - (int)First + 1;

    public Seq<SequenceNumber> Numbers =>
        toSeq(LanguageExt.List.unfold(Some(First), at => at.Filter(number => number <= Last).Map(static number => (number, number.Next()))));

    public Option<SequenceNumber> Named(SequenceNumber frame) => Count > 1 ? Some(frame) : None;

    public Option<TimeSpan> Time(SequenceNumber frame) => Named(frame).Map(static number => TimeSpan.FromSeconds((int)number / Rate));

    public static Fin<FrameSpan> Of(int first, int last) =>
        (Conversions.Validated<SequenceNumber, int, InvalidRhinoValue>(first).ToValidation(), Conversions.Validated<SequenceNumber, int, InvalidRhinoValue>(last).ToValidation())
            .Apply(static (from, until) => (From: from, Until: until))
            .As()
            .ToFin()
            .Bind(static span => Validate(span.From, span.Until, out FrameSpan? spanned) is { } error ? Fin.Fail<FrameSpan>(error) : Fin.Succ(spanned!));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref SequenceNumber first, ref SequenceNumber last) =>
        validationError = first <= last ? null : new InvalidRhinoValue();
}

public sealed record PlannedOutput(OutputTarget Target, Seq<NamePart> Scope, Seq<NamePart> Parts) {
    public Fin<OutputName> Name(Option<SequenceNumber> number, OutputVersioning version) =>
        Conversions.Validated<FileExtension, string, InvalidRhinoValue>(Target.FileFormat.Extension).Map(extension => new OutputName(Scope, version, Parts, number, extension));
}

public sealed record QueueOutput(SequenceNumber Frame, OutputTarget Target, OutputName Name, OutputPath Path);

public sealed record QueueEntry<TOutput>(
    RenderOverride Scene,
    RenderSourceState Source,
    FrameSpan Frames,
    PixelExtent Extent,
    Overscan Overscan,
    Option<RenderRegion> Region,
    Option<SunWindow> Sun,
    Seq<TOutput> Outputs) {
    public ContentKey Key =>
        ContentKey.Of(KeyDomain.QueueEntry, stream => stream
            .Rows(toSeq(Scene.Values.Entries.AsIterable().OrderBy(static entry => entry.Key)), static (fields, entry) =>
                fields.Text(entry.Key.Owner).Rows(entry.Key.Path, static (path, part) => path.Text(part)).Text(entry.Value))
            .Rows(Scene.Clay.ToSeq(), static (fields, clay) => fields.Number(clay))
            .Integer((int)Source.RenderSource)
            .Rows(Seq(Source.SpecificViewport, Source.NamedView, Source.Snapshot), static (fields, name) => fields.Rows(name.ToSeq(), static (names, text) => names.Text(text)))
            .Integer<int>(Frames.First).Integer<int>(Frames.Last).Integer(Extent.Width).Integer(Extent.Height).Number(Overscan)
            .Rows(Region.ToSeq(), static (fields, region) => fields.Number(region.Left).Number(region.Top).Number(region.Right).Number(region.Bottom))
            .Rows(Sun.ToSeq(), static (fields, window) => fields
                .Text(LocalDateTimePattern.ExtendedIso.Format(window.Start))
                .Text(LocalDateTimePattern.ExtendedIso.Format(window.End))
                .Text(PeriodPattern.Roundtrip.Format(window.Step))));

    public Fin<PixelExtent> Rendered =>
        Overscan.Around(Extent).Padded switch {
            var padded => PixelExtent.Validate(padded.Width, padded.Height, out PixelExtent rendered) is { } error ? error : rendered,
        };

    public Fin<Option<SunMoment>> Moment(SequenceNumber frame) =>
        Sun.Traverse(window => window.Frames switch {
            var moments => moments.At((int)frame).ToFin(new FrameOutsideSequence(frame, moments.Count))
                .Bind(static local => Conversions.Validated<SunMoment, LocalDateTime, InvalidRhinoValue>(local)),
        }).As();

    public QueueEntry<TPlaced> Placed<TPlaced>(Seq<TPlaced> outputs) => new(Scene, Source, Frames, Extent, Overscan, Region, Sun, outputs);
}

public sealed record QueuePlan(Destination Destination, Seq<QueueEntry<PlannedOutput>> Entries);

public sealed record RenderBatch(Seq<QueueEntry<QueueOutput>> Entries) {
    public int Frames => Entries.Fold(0, static (sum, entry) => sum + entry.Frames.Count);
}

public sealed record QueueSettings(RetryCount Retries, RetryDelay Delay, bool SampleEstimate, bool KeepAwake, bool ShutDownAfterSuccess) {
    public static QueueSettings Default { get; } = new(RetryCount.Default, RetryDelay.Default, SampleEstimate: true, KeepAwake: true, ShutDownAfterSuccess: false);
}

public sealed record QueueSettingRows(
    PlugInSetting<RetryCount, int, InvalidRhinoValue> Retries,
    PlugInSetting<RetryDelay, int, InvalidRhinoValue> Delay,
    PlugInSetting<bool, bool, InvalidRhinoValue> SampleEstimate,
    PlugInSetting<bool, bool, InvalidRhinoValue> KeepAwake,
    PlugInSetting<bool, bool, InvalidRhinoValue> ShutDownAfterSuccess,
    PlugInSetting<NoticeServer, string, InvalidRhinoValue> Server,
    PlugInSetting<Option<NoticeTopic>, string, InvalidRhinoValue> Topic) {
    public Seq<PlugInSetting> Rows => Seq<PlugInSetting>(Retries, Delay, SampleEstimate, KeepAwake, ShutDownAfterSuccess, Server, Topic);

    public Seq<Child> Children =>
        (Seq<ControlRow>(new ControlRow.Group(RowText.Localize("Retries and power"), RowRules.Always) { Wording = Wording.Localized })
         + SectionRows.Setting(Retries, new Presentation<RetryCount, int> { Form = NumberForm.Field }, RowRules.Always)
         + SectionRows.Setting(Delay, RetryDelay.Presentation, RowRules.Always)
         + SectionRows.Setting(SampleEstimate, RowRules.Always)
         + SectionRows.Setting(KeepAwake, RowRules.Always)
         + SectionRows.Setting(ShutDownAfterSuccess, RowRules.Always)
         + Seq<ControlRow>(new ControlRow.Group(RowText.Localize("Notices"), RowRules.Always) { Wording = Wording.Localized })
         + SectionRows.Setting(Server, RowRules.Always)
         + SectionRows.Setting(Topic, RowRules.Always))
            .Map(static row => (Child)row);

    public IO<QueueSettings> Settings(SettingsNode node) =>
        (PlugInSettings.Current(node, Retries), PlugInSettings.Current(node, Delay), PlugInSettings.Current(node, SampleEstimate),
         PlugInSettings.Current(node, KeepAwake), PlugInSettings.Current(node, ShutDownAfterSuccess))
            .Apply(static (retries, delay, sampled, awake, shutdown) => new QueueSettings(retries, delay, sampled, awake, shutdown))
            .As();

    public IO<NoticeSettings> Notices(SettingsNode node) =>
        (PlugInSettings.Current(node, Server), PlugInSettings.Current(node, Topic)).Apply(static (server, topic) => new NoticeSettings(server, topic)).As();
}

public sealed record QueueContext(
    OutputPath Record,
    IO<QueueSettings> Settings,
    PlugIn PlugIn,
    IPlugInSink Sink,
    RenderSets Sets,
    RenderHistory History,
    TrayRow Tray,
    Func<QueueEvent, IO<Unit>> Notify,
    TimeProvider Clock);

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

public sealed record QueueBook {
    // --- [RECORD]
    public const uint TypeCode = 0x5251_5545u;

    private QueueBook(Seq<QueuedEntry> entries) => Entries = entries;

    public static QueueBook Empty { get; } = new(Seq<QueuedEntry>());

    public static DictionaryCodec<QueueBook> Codec { get; } = new(nameof(QueueBook), 1, Encoded, Decoded);

    public Seq<QueuedEntry> Entries { get; }

    // --- [TRANSITIONS]
    public QueueBook Added(Seq<QueuedEntry> entries) => new(entries.Fold(Entries, static (held, entry) => held.Exists(entry.Equals) ? held : held.Add(entry)));

    public QueueBook Removed(LanguageExt.HashSet<QueuedEntry> entries) => new(Entries.Filter(entry => !entries.Contains(entry)));

    // --- [ARCHIVE]
    private static Fin<ArchivableDictionary> Encoded(QueueBook book) =>
        ArchivableDictionaries.Nested(Seq((nameof(Entries), ArchivableDictionaries.Nested(book.Entries.Map(static (entry, index) => (index.ToString(CultureInfo.InvariantCulture), Encoded(entry)))))));

    private static Fin<ArchivableDictionary> Encoded(QueuedEntry entry) =>
        ArchivableDictionaries.Nested(
                entry.Region.Map(static region => (nameof(QueuedEntry.Region), Encoded(JobMapper.Region(region)))).ToSeq()
                + entry.Sun.Map(static window => JobMapper.Study(window)).Map(static study =>
                    (study.Switch(day: static _ => nameof(QueueJob.Study.Day), season: static _ => nameof(QueueJob.Study.Season)), Encoded(study))).ToSeq())
            .Bind(target => (
                    Stored(target, Seq(
                            (nameof(QueuedEntry.Source), Some(entry.Source.RenderSource.ToString())),
                            (nameof(RenderSourceState.SpecificViewport), entry.Source.SpecificViewport),
                            (nameof(RenderSourceState.NamedView), entry.Source.NamedView),
                            (nameof(RenderSourceState.Snapshot), entry.Source.Snapshot),
                            (nameof(QueuedEntry.Set), entry.Set.Map(static name => (string)name)))
                        .Choose(static row => row.Item2.Map(text => (row.Item1, text)))).ToValidation(),
                    Stored(target, Seq(
                        (nameof(FrameSpan.First), (int)entry.Frames.First), (nameof(FrameSpan.Last), (int)entry.Frames.Last),
                        (nameof(PixelExtent.Width), entry.Extent.Width), (nameof(PixelExtent.Height), entry.Extent.Height))).ToValidation(),
                    ArchivableDictionaries.Set(target, nameof(QueuedEntry.Overscan), (double)entry.Overscan).ToValidation(),
                    entry.Clay.Traverse(clay => ArchivableDictionaries.Set(target, nameof(QueuedEntry.Clay), (float)clay)).As().ToValidation())
                .Apply((_, _, _, _) => target)
                .As()
                .ToFin());

    private static Fin<ArchivableDictionary> Encoded(QueueJob.Region region) =>
        from target in Fin.Succ(new ArchivableDictionary())
        from edges in Stored(target, Seq((nameof(region.Left), region.Left), (nameof(region.Top), region.Top), (nameof(region.Right), region.Right), (nameof(region.Bottom), region.Bottom)))
        select target;

    private static Fin<ArchivableDictionary> Encoded(QueueJob.Study study) =>
        from target in Fin.Succ(new ArchivableDictionary())
        from written in study.Switch(
            target,
            day: static (held, day) => Stored(held, Seq((nameof(day.Date), day.Date), (nameof(day.From), day.From), (nameof(day.Until), day.Until)))
                .Bind(_ => ArchivableDictionaries.Set(held, nameof(day.Minutes), day.Minutes)),
            season: static (held, season) => Stored(held, Seq((nameof(season.From), season.From), (nameof(season.Until), season.Until), (nameof(season.At), season.At)))
                .Bind(_ => ArchivableDictionaries.Set(held, nameof(season.Days), season.Days)))
        select target;

    private static Fin<Unit> Stored<T>(ArchivableDictionary target, Seq<(string Key, T Value)> rows) where T : notnull =>
        Callbacks.Each(rows, (row, _) => ArchivableDictionaries.Set(target, row.Key, row.Value)).Map(static _ => unit);

    private static Fin<QueueBook> Decoded(ArchivableDictionary source) =>
        from entries in ArchivableDictionaries.Required<ArchivableDictionary>(source, nameof(Entries))
        from decoded in Callbacks.Each(toSeq(Range(0, entries.Count)), (index, _) =>
            ArchivableDictionaries.Required<ArchivableDictionary>(entries, index.ToString(CultureInfo.InvariantCulture)).Bind(Entry))
        select new QueueBook(decoded);

    private static Fin<QueuedEntry> Entry(ArchivableDictionary row) =>
        (
            ArchivableDictionaries.Find<string>(row, nameof(QueuedEntry.Set)).Bind(static name => name.Traverse(Conversions.Validated<SetName, string, InvalidRhinoValue>).As()).ToValidation(),
            ArchivableDictionaries.Find<float>(row, nameof(QueuedEntry.Clay)).Bind(static clay => clay.Traverse(Conversions.Validated<Albedo, float, InvalidRhinoValue>).As()).ToValidation(),
            Source(row).ToValidation(),
            (ArchivableDictionaries.Required<int>(row, nameof(FrameSpan.First)).ToValidation(), ArchivableDictionaries.Required<int>(row, nameof(FrameSpan.Last)).ToValidation())
                .Apply(static (first, last) => (First: first, Last: last))
                .As()
                .Bind(static span => FrameSpan.Of(span.First, span.Last).ToValidation()),
            (ArchivableDictionaries.Required<int>(row, nameof(PixelExtent.Width)).ToValidation(), ArchivableDictionaries.Required<int>(row, nameof(PixelExtent.Height)).ToValidation())
                .Apply(static (width, height) => (Width: width, Height: height))
                .As()
                .Bind(static size => (PixelExtent.Validate(size.Width, size.Height, out PixelExtent sized) is { } refused ? Fin.Fail<PixelExtent>(refused) : Fin.Succ(sized)).ToValidation()),
            ArchivableDictionaries.Required<double>(row, nameof(QueuedEntry.Overscan)).Bind(Conversions.Validated<Overscan, double, InvalidRhinoValue>).ToValidation(),
            ArchivableDictionaries.Find<ArchivableDictionary>(row, nameof(QueuedEntry.Region)).Bind(static found => found.Traverse(Region).As()).ToValidation(),
            Window(row).ToValidation()
        )
            .Apply(static (set, clay, source, frames, extent, overscan, region, sun) => new QueuedEntry(set, clay, source, frames, extent, overscan, region, sun))
            .As()
            .ToFin();

    private static Fin<RenderSourceState> Source(ArchivableDictionary row) =>
        (
            ArchivableDictionaries.FindEnum<RenderSettings.RenderingSources>(row, nameof(QueuedEntry.Source)).Bind(static found => found.ToFin(new Missing(nameof(QueuedEntry.Source)))).ToValidation(),
            ArchivableDictionaries.Find<string>(row, nameof(RenderSourceState.SpecificViewport)).ToValidation(),
            ArchivableDictionaries.Find<string>(row, nameof(RenderSourceState.NamedView)).ToValidation(),
            ArchivableDictionaries.Find<string>(row, nameof(RenderSourceState.Snapshot)).ToValidation()
        )
            .Apply(static (source, viewport, named, snapshot) => new RenderSourceState(source, viewport, named, snapshot))
            .As()
            .ToFin();

    private static Fin<RenderRegion> Region(ArchivableDictionary source) =>
        (
            ArchivableDictionaries.Required<double>(source, nameof(QueueJob.Region.Left)).ToValidation(),
            ArchivableDictionaries.Required<double>(source, nameof(QueueJob.Region.Top)).ToValidation(),
            ArchivableDictionaries.Required<double>(source, nameof(QueueJob.Region.Right)).ToValidation(),
            ArchivableDictionaries.Required<double>(source, nameof(QueueJob.Region.Bottom)).ToValidation()
        )
            .Apply(static (left, top, right, bottom) => new QueueJob.Region(left, top, right, bottom))
            .As()
            .Bind(JobMapper.Region)
            .ToFin();

    private static Fin<Option<SunWindow>> Window(ArchivableDictionary row) =>
        (ArchivableDictionaries.Find<ArchivableDictionary>(row, nameof(QueueJob.Study.Day)).ToValidation(),
         ArchivableDictionaries.Find<ArchivableDictionary>(row, nameof(QueueJob.Study.Season)).ToValidation())
            .Apply(static (day, season) => day.Map(Day) | season.Map(Season))
            .As()
            .ToFin()
            .Bind(static study => study.Traverse(static held => held.Bind(JobMapper.Window).ToFin()).As());

    private static Validation<Error, QueueJob.Study> Day(ArchivableDictionary source) =>
        (
            ArchivableDictionaries.Required<string>(source, nameof(QueueJob.Study.Day.Date)).ToValidation(),
            ArchivableDictionaries.Required<string>(source, nameof(QueueJob.Study.Day.From)).ToValidation(),
            ArchivableDictionaries.Required<string>(source, nameof(QueueJob.Study.Day.Until)).ToValidation(),
            ArchivableDictionaries.Required<int>(source, nameof(QueueJob.Study.Day.Minutes)).ToValidation()
        ).Apply(static (date, from, until, minutes) => (QueueJob.Study)new QueueJob.Study.Day(date, from, until, minutes)).As();

    private static Validation<Error, QueueJob.Study> Season(ArchivableDictionary source) =>
        (
            ArchivableDictionaries.Required<string>(source, nameof(QueueJob.Study.Season.From)).ToValidation(),
            ArchivableDictionaries.Required<string>(source, nameof(QueueJob.Study.Season.Until)).ToValidation(),
            ArchivableDictionaries.Required<string>(source, nameof(QueueJob.Study.Season.At)).ToValidation(),
            ArchivableDictionaries.Required<int>(source, nameof(QueueJob.Study.Season.Days)).ToValidation()
        ).Apply(static (from, until, at, days) => (QueueJob.Study)new QueueJob.Study.Season(from, until, at, days)).As();
}

public sealed record QueueReadout(RunState State, QueueEstimate Estimate) {
    public static QueueReadout Idle { get; } = new(new RunState.Absent(), QueueEstimate.Idle);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Queues {
    // --- [RESOLUTION]
    public static IO<RenderOverride> Scene(QueueContext context, RhinoDoc doc, Option<SetName> set, Option<Albedo> clay) =>
        set.Match(
            Some: name =>
                from values in context.Sets.Values(doc, name)
                from renderable in RenderSets.Renderable(doc)
                from member in unless(renderable.Exists(held => held == name), IO.fail<Unit>(new ExcludedRenderSet(name))).As()
                select new RenderOverride(name, values, clay),
            None: () => IO.lift(() => context.History.Bindings(doc).ToFin()).Bind(static bound => bound.Capture).Map(values => new RenderOverride("Current settings", values, clay)));

    public static IO<RenderBatch> Resolve(RhinoDoc doc, QueuePlan plan, Option<QueueProgress> resumed) =>
        plan.Entries
            .TraverseM(entry => Resolved(doc, plan.Destination, entry, resumed.Bind(record => record.Entries.Find(entry.Key))))
            .As()
            .Map(static entries => new RenderBatch(entries));

    private static IO<QueueEntry<QueueOutput>> Resolved(RhinoDoc doc, Destination destination, QueueEntry<PlannedOutput> entry, Option<EntryProgress> resumed) =>
        (from frame in entry.Frames.Numbers from output in entry.Outputs select (Output: output, Frame: frame)) switch {
            var files =>
                from names in IO.lift(files
                    .Traverse(file => file.Output.Name(
                        entry.Frames.Named(file.Frame),
                        resumed.Bind(held => held.Versions.Find(file.Output.Scope)).Match<OutputVersioning>(Some: static version => version, None: static () => new OutputVersioning.NextFree())).ToValidation())
                    .As()
                    .ToFin())
                from placed in Destinations.Resolve(doc, destination with { Overwrite = destination.Overwrite || resumed.IsSome }, names)
                select entry.Placed(files.Zip(placed, static (file, row) => new QueueOutput(file.Frame, file.Output.Target, row.Name, row.Path))),
        };

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

    private sealed record QueueRun(QueueEntry<QueueOutput> Entry, Seq<SequenceNumber> Frames);

    // --- [PLAN]
    public static IO<Atom<QueueBook>> Held(RhinoDoc doc) =>
        IO.lift(() => doc.RuntimeData.GetValue(typeof(QueueBook), static _ => Atom(QueueBook.Empty)));

    public static IO<QueueBook> Book(RhinoDoc doc) => Held(doc).Bind(static held => held.ValueIO);

    public static IO<QueuePlan> Plan(QueueContext context, RhinoDoc doc) =>
        from book in Book(doc)
        from outputs in TargetRows.Book(doc)
        from entries in book.Entries.TraverseM(entry =>
                from scene in Scene(context, doc, entry.Set, entry.Clay)
                from scope in IO.lift(entry.Name.Traverse(Conversions.Validated<NamePart, string, InvalidRhinoValue>).As().Map(static part => part.ToSeq()))
                select new QueueEntry<PlannedOutput>(scene, entry.Source, entry.Frames, entry.Extent, entry.Overscan, entry.Region, entry.Sun,
                    outputs.Named.Map(target => new PlannedOutput(target.Target, scope, target.Parts))))
            .As()
        select new QueuePlan(outputs.Destination, entries);

    // --- [RUN]
    public static IO<Unit> Run(QueueContext context, RhinoDoc doc, RenderBatch queue, Option<string> job, Option<QueueProgress> resumed) =>
        from settings in context.Settings
        from prior in QueueProgress.Read(context.Record)
        from state in QueueProgress.State(prior, context.Record, context.Clock)
        from free in IO.lift(state.Switch(
            absent: static _ => Fin.Succ(unit),
            active: static active => Fin.Fail<Unit>(new QueueActive(active.ProcessId)),
            ended: static _ => Fin.Succ(unit),
            crashed: static _ => Fin.Succ(unit)))
        from started in QueueProgress.Started(job, queue, resumed, settings)
        from held in IO.lift(() => Atom(started))
        from written in QueueProgress.Write(context.Record, started)
        from now in IO.lift(context.Clock.GetCurrentInstant)
        from shown in Readout.SwapIO(_ => new QueueReadout(new RunState.Active(started.ProcessId, now), QueueEstimate.Running(queue, started, None, now)))
        from began in IO.lift(context.Clock.GetTimestamp)
        from noticed in unless(resumed.IsSome, context.Notify(new QueueEvent.Started(queue.Entries.Count, queue.Frames))).As()
        from ran in (Heartbeat(context).Bind(beat => Frames(context, doc, queue, held, resumed.IsNone && settings.SampleEstimate && queue.Entries.Count > 1).Finally(beat.Cancel)) switch {
            var run => settings.KeepAwake ? MachinePower.HeldAwake(run) : run,
        })
            .Catch(error => (Stopping(error)
                    ? Closed(context, held, began, RunEnd.Stopped, static (record, _) => new QueueEvent.Stopped(record.Landed))
                    : Closed(context, held, began, RunEnd.Failed, (_, _) => new QueueEvent.Failed(error)))
                .Bind(_ => IO.fail<Unit>(error)))
            .As()
            .Bind(_ => Closed(context, held, began, RunEnd.Succeeded, static (record, elapsed) => new QueueEvent.Complete(record.Landed, elapsed)))
            .Bind(_ => when(settings.ShutDownAfterSuccess, MachinePower.ShutDown).As())
        select unit;

    private static bool Stopping(Error error) =>
        error.Is(Errors.Cancelled) || error.FoldM(static cause => cause is Ended ended ? Seq(ended.Result) : Seq<Result>()).As().Exists(static result => result == Result.ExitRhino);

    private static IO<ForkIO<Unit>> Heartbeat(QueueContext context) =>
        QueueProgress.Touch(context.Record, context.Clock)
            .Catch(error => IO.lift(() => context.Sink.Report(error, typeof(Queues), nameof(Heartbeat))))
            .As()
            .Repeat(Schedule.spaced(QueueProgress.Heartbeat.ToTimeSpan()))
            .Fork();

    private static IO<Unit> Closed(QueueContext context, Atom<QueueProgress> held, long began, RunEnd end, Func<QueueProgress, Duration, QueueEvent> notice) =>
        from record in held.SwapIO(was => was with { Ended = Some(end) })
        from written in QueueProgress.Write(context.Record, record)
        from shown in Readout.SwapIO(was => was with { State = new RunState.Ended(end) })
        from idle in Work.SwapIO(static _ => new WorkState.Inactive())
        from elapsed in IO.lift(() => context.Clock.GetElapsedTime(began).ToDuration())
        from noticed in context.Notify(notice(record, elapsed))
        select unit;

    // --- [FRAMES]
    private const string RenderCommand = "Render";

    private sealed record RunRequest(QueueRun Run, Conduit<Fin<Unit>, Fin<Unit>> Reply);

    private sealed record RunScope(QueueContext Context, RhinoDoc Doc, RenderBatch Queue, Atom<QueueProgress> Held, Disposal<Eto.Forms.Application> Dock, Atom<Heard> Heard);

    private sealed record Heard(Option<long> Began, Option<(Guid Session, long At)> Saved, Option<Result> Ended) {
        public static Heard Empty { get; } = new(None, None, None);
    }

    private static IO<Unit> Frames(QueueContext context, RhinoDoc doc, RenderBatch queue, Atom<QueueProgress> held, bool sampled) =>
        from render in IO.lift(static () => Conversions.Required(Command.LookupCommandId(RenderCommand, searchForEnglishName: true), nameof(Command.LookupCommandId))).Post()
        from heard in IO.lift(static () => Atom(Heard.Empty))
        from ran in Presence.Acquire().Post().Bracket(
            Use: dock => DisposalOps.AcquireAll(Listened(context, render, heard).Add(Presence.Tray(context.Tray, context.Sink)), DisposalOps.Release).Post().Bracket(
                Use: _ => Mailed(new RunScope(context, doc, queue, held, dock, heard), sampled),
                Fin: static listened => DisposalOps.Release(listened).Post()),
            Fin: static dock => IO.lift(dock.Dispose).Post())
        select ran;

    private static Seq<IO<IDisposable>> Listened(QueueContext context, Guid render, Atom<Heard> heard) =>
        Seq(
            EventKind.BeginCommand.Inline(args => when(args.CommandId == render,
                IO.lift(context.Clock.GetTimestamp).Bind(at => heard.SwapIO(_ => Heard.Empty with { Began = Some(at) })).Map(static _ => unit)).As(), context.Sink),
            EventKind.ImageFileSaved.Inline(args =>
                IO.lift(context.Clock.GetTimestamp).Bind(at => heard.SwapIO(was => was with { Saved = Some((args.SessionId, at)) })).Map(static _ => unit), context.Sink),
            EventKind.EndCommand.Inline(args => when(args.CommandId == render,
                heard.SwapIO(was => was with { Ended = Some(args.CommandResult) }).Map(static _ => unit)).As(), context.Sink));

    private static IO<Unit> Mailed(RunScope scope, bool sampled) =>
        Subscriptions.Idle<Unit, RunRequest>(pending => toSeq(pending.Values).TraverseM(request => Turn(scope, request)).As().Map(static _ => unit))(
                new CallbackSite(scope.Context.Sink, typeof(Queues), nameof(Frames)))
            .Post()
            .Bracket(
                Use: mailbox =>
                    from order in IO.lift(() => Order(scope.Queue, scope.Held.Value, sampled))
                    from heads in order.Sampled.TraverseM(run => Requested(mailbox.Post, run)).As()
                    from estimate in unless(order.Sampled.IsEmpty,
                        IO.lift(scope.Context.Clock.GetCurrentInstant).Bind(now => scope.Context.Notify(new QueueEvent.Estimate(QueueEstimate.Sampled(scope.Queue, scope.Held.Value, now))))).As()
                    from remaining in order.Remaining.TraverseM(run => Requested(mailbox.Post, run)).As()
                    select unit,
                Fin: static mailbox => IO.lift(mailbox.Release.Dispose).Post());

    private static IO<Unit> Requested(Func<(Unit Key, RunRequest Value), IO<Unit>> post, QueueRun run) =>
        from reply in IO.lift(static () => Conduit.make(Buffer<Fin<Unit>>.Unbounded))
        from posted in post((unit, new RunRequest(run, reply)))
        from answer in SourceT.lift<IO, Fin<Unit>>(reply.Source).Take(1).Last().As()
        from ran in IO.lift(answer)
        select ran;

    private static IO<Unit> Turn(RunScope scope, RunRequest request) =>
        Ran(scope, request.Run).Bind(_ => request.Reply.Post(unit)).Catch(error => request.Reply.Post(error)).As();

    private static IO<Unit> Ran(RunScope scope, QueueRun run) =>
        from rendered in IO.lift(run.Entry.Rendered)
        from bound in IO.lift(() => scope.Context.History.Bindings(scope.Doc).Bind(RenderOverrides.Group).ToFin())
        from ran in RenderOverrides.Around(
            run.Entry.Scene,
            new OverrideScope(scope.Doc, bound, scope.Context.PlugIn, scope.Context.Sink),
            Sized(scope.Doc, run.Entry, rendered).Bind(_ => Widened(scope, run, rendered)))
        select ran;

    private static IO<Unit> Sized(RhinoDoc doc, QueueEntry<QueueOutput> entry, PixelExtent rendered) =>
        SceneSources.Edit(new SceneSource.Live(doc), window =>
            from source in RenderSourceState.Write(window, entry.Source)
            from held in ImageOutputState.Read(window)
            from sized in IO.lift(Framing.Sized(held with { UseViewportSize = false }, new Size(rendered.Width, rendered.Height)))
            from output in ImageOutputState.Write(window, sized)
            select unit);

    private static IO<Unit> Widened(RunScope scope, QueueRun run, PixelExtent rendered) =>
        run.Entry.Overscan == Overscan.Off
            ? Lit(scope, run, rendered)
            : from viewport in IO.lift(() => Missing.Unless(scope.Doc.Views.ActiveView, nameof(ViewTable.ActiveView)).Map(static view => view.ActiveViewport))
              from ran in use(() => new ViewportInfo(viewport))
                  .Bind(held => Projected(viewport, run.Entry.Overscan.Around(run.Entry.Extent)).Bracket(
                      Use: _ => Lit(scope, run, rendered),
                      Fin: _ => IO.lift(() => Refused.Unless(viewport.SetViewProjection(held, updateTargetLocation: false), nameof(RhinoViewport.SetViewProjection)))))
                  .Bracket()
              select ran;

    private static IO<Unit> Projected(RhinoViewport viewport, OverscanFrame frame) =>
        (from copy in use(() => new ViewportInfo(viewport))
         from widened in frame.Widen(copy)
         from applied in IO.lift(() => Refused.Unless(viewport.SetViewProjection(copy, updateTargetLocation: false), nameof(RhinoViewport.SetViewProjection)))
         select applied).Bracket();

    private static IO<Unit> Lit(RunScope scope, QueueRun run, PixelExtent rendered) =>
        run.Entry.Sun.Match(
            Some: _ => SunState.Moment(new SceneSource.Live(scope.Doc), scope.Context.Clock) switch {
                var moment => moment.Read.Bracket(Use: _ => Looped(scope, run, rendered), Fin: moment.Put),
            },
            None: () => Looped(scope, run, rendered));

    private static IO<Unit> Looped(RunScope scope, QueueRun run, PixelExtent rendered) =>
        run.Frames.TraverseM(frame => Frame(scope, run.Entry, rendered, frame)).As().Map(static _ => unit);

    private static IO<Unit> Frame(RunScope scope, QueueEntry<QueueOutput> entry, PixelExtent rendered, SequenceNumber frame) =>
        from started in IO.lift(scope.Context.Clock.GetTimestamp)
        from moment in IO.lift(entry.Moment(frame))
        from lit in moment.Traverse(held => SunState.Moment(new SceneSource.Live(scope.Doc), scope.Context.Clock).Put(Some(held))).As()
        from framing in use(() => new RenderSourceView(scope.Doc))
            .Bind(static source => IO.lift(() => Missing.Unless(source.GetViewInfo(), nameof(RenderSourceView.GetViewInfo))))
            .Bind(view => RenderRuns.Framing(scope.Doc, view, rendered, entry.Frames.Time(frame), new CallbackSite(scope.Context.Sink, typeof(Queues), nameof(Frame))))
            .Bracket()
        from launched in RenderRuns.Launched(framing, scope.Context.History.Launched(scope.Doc, Documents.RunScript(scope.Doc, $"_-{RenderCommand}", echo: false, display: None)))
        from heard in scope.Heard.ValueIO
        from ended in IO.lift(heard.Ended.ToFin(new FrameLost(frame))
            .Bind(static result => Conversions.FromResult(result, RenderCommand))
            .MapFail(error => Stopping(error) ? error : new FrameLost(frame)))
        from began in IO.lift(heard.Began.ToFin(new FrameLost(frame)))
        from saved in IO.lift(heard.Saved.ToFin(new FrameLost(frame)))
        from window in new RenderWindowSource.Rendering(saved.Session).Open()
        from origin in FileOrigin.At(scope.Context.Sink)
        from written in Callbacks.Each(entry.Outputs
            .Filter(output => output.Frame == frame)
            .Map(output => Saves.Write(new SaveSource(scope.Doc, window), origin, output.Target, output.Path, entry.Region, entry.Overscan.Around(entry.Extent))))
        from closed in Documents.RunScript(scope.Doc, "_CloseRenderWindow", echo: false, display: None)
        from peak in use(Process.GetCurrentProcess).Map(static self => self.PeakWorkingSet64).Bracket()
        from at in IO.lift(scope.Context.Clock.GetTimestamp)
        from landed in Landed(scope, new FrameMark(entry.Key, frame, started, began, saved.At, at, peak))
        select unit;

    private static IO<Unit> Landed(RunScope scope, FrameMark mark) =>
        from record in scope.Held.SwapIO(was => mark.Time(scope.Context.Clock) switch {
            var time => was with {
                Entries = was.Entries.AddOrUpdate(
                    mark.Entry,
                    entry => entry with { Last = Some(mark.Frame), Times = entry.Times.Add(time) },
                    EntryProgress.Empty with { Last = Some(mark.Frame), Times = Seq(time) }),
            },
        })
        from written in QueueProgress.Write(scope.Context.Record, record)
        from now in IO.lift(scope.Context.Clock.GetCurrentInstant)
        from shown in Readout.SwapIO(was => was with { Estimate = QueueEstimate.Running(scope.Queue, record, Some(mark), now) })
        from docked in Docked(scope, record)
        select unit;

    private static IO<Unit> Docked(RunScope scope, QueueProgress record) =>
        from work in IO.lift(WorkCount.Validate(record.Landed, scope.Queue.Frames, out WorkCount count) is { } error ? Fin.Fail<WorkCount>(error) : Fin.Succ(count))
            .Bind(static counted => Work.SwapIO(_ => new WorkState.Counted(counted)))
        from progress in Presence.SetProgress(scope.Dock, new DockState(work, DockPhase.Working))
        from badge in IO.lift(Some(scope.Queue.Frames - record.Landed).Filter(static left => left > 0)
                .Traverse(static left => Conversions.Validated<BadgeCount, int, InvalidRhinoValue>(left)).As())
            .Bind(count => Presence.BadgeLabel(scope.Dock, count))
        select unit;

    // --- [RESUME]
    public static IO<Option<Relaunch>> Resumable(QueueContext context) =>
        from prior in QueueProgress.Read(context.Record)
        from state in QueueProgress.State(prior, context.Record, context.Clock)
        let crashed = state.Map(absent: false, active: false, ended: false, crashed: true) ? prior.Filter(static record => record.Relaunch.IsSome) : None
        from relaunch in crashed.Match(
            Some: record => record.Retries.Spent.Match(
                Some: left => QueueProgress.Write(context.Record, record with { Retries = left }).Map(_ => record.Relaunch),
                None: () => Exhausted(context, record)),
            None: static () => IO.pure(Option<Relaunch>.None))
        select relaunch;

    private static IO<Option<Relaunch>> Exhausted(QueueContext context, QueueProgress record) =>
        from settings in context.Settings
        from noticed in context.Notify(new QueueEvent.RetriesExhausted(settings.Retries))
        from written in QueueProgress.Write(context.Record, record with { Ended = Some(RunEnd.Failed) })
        select Option<Relaunch>.None;

    // --- [REGISTRATION]
    private static readonly Atom<QueueReadout> Readout = Atom(QueueReadout.Idle);

    private static readonly Atom<WorkState> Work = Atom<WorkState>(new WorkState.Inactive());

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(QueueSettingRows settings, View.Panel reveal) =>
        (plugIn, sink) =>
            from glyph in IO.lift(reveal.Icon.ToFin(new Missing(nameof(View.Panel.Icon))))
            let views = (IPlugInViews)plugIn
            let rendering = (IPlugInRendering)plugIn
            from record in IO.lift(() => Conversions.Validated<OutputPath, string, InvalidOutput>(Path.Combine(plugIn.SettingsDirectory, "render-queue.json")))
            from held in DisposalOps.AcquireAll(
                Seq(
                    NoticeSender.Register(new NoticeOptions(settings.Notices(views.Settings), Some(glyph), Some(reveal)), views.Clock)(plugIn, sink),
                    from sets in IPlugInRendering.Served(rendering.Sets, nameof(IPlugInRendering.Sets))
                    from history in IPlugInRendering.Served(rendering.History, nameof(IPlugInRendering.History))
                    from notices in IPlugInRendering.Served(rendering.Notices, nameof(IPlugInRendering.Notices))
                    let context = new QueueContext(record, settings.Settings(views.Settings), plugIn, sink, sets, history,
                        new TrayRow(reveal.Caption, glyph, Seq(Seq<MenuEntry>(new MenuEntry.Listed(reveal.Caption, IO.pure(Seq<MenuPick>(
                            new MenuPick.Item(reveal.Caption, Marked: false, IO.lift(() => Panels.OpenPanel(reveal.Identity, makeSelectedPanel: true))))))))),
                        notices.Notify, views.Clock)
                    from prior in QueueProgress.Read(record)
                    from state in QueueProgress.State(prior, record, views.Clock)
                    from shown in Readout.SwapIO(_ => QueueReadout.Idle with { State = state })
                    from filled in rendering.QueueContext.SwapIO(_ => Some(context))
                    select (IDisposable)new Disposal<Atom<Option<QueueContext>>>(rendering.QueueContext, static cell => _ = cell.Swap(static _ => None)),
                    IPlugInRendering.Served(rendering.QueueContext, nameof(IPlugInRendering.QueueContext)).Bind(context => Dispatch.Relaunched(context)(plugIn, sink))),
                DisposalOps.Release)
            select DisposalOps.Composite(held, new CallbackSite(sink, typeof(Queues), nameof(Register)));

    // --- [ROWS]
    private const string Owner = "render-queue";

    private static readonly Func<Destination, Fin<QueueTarget>> Local = static _ => new QueueTarget.Local();

    private static readonly Func<QueueContext, IO<Option<QueueProgress>>> Fresh = static _ => IO.pure(Option<QueueProgress>.None);

    private static readonly Func<QueueContext, IO<Option<QueueProgress>>> Unfinished =
        static context => QueueProgress.Read(context.Record).Map(static prior => prior.Filter(static record => !record.Succeeded));

    private static readonly (RowSource<Seq<QueuedEntry>> Source, RowField<Seq<QueuedEntry>> Field) Listed =
        RowSource.Opaque(Owner, "entries", Seq<QueuedEntry>(), static scope => ValueStore.Of(
            Opened(scope).Bind(Book).Map(static book => Some(book.Entries)),
            static _ => IO.pure(unit),
            Applied.Live,
            None));

    private static readonly (RowSource<LanguageExt.HashSet<QueuedEntry>> Source, RowField<LanguageExt.HashSet<QueuedEntry>> Field) Picked =
        RowSource.Opaque(Owner, "picked", LanguageExt.HashSet<QueuedEntry>.Empty);

    private static readonly (RowSource<bool> Source, RowField<bool> Field) Clayed = RowSource.Opaque(Owner, "clay", @default: false);

    private static readonly (RowSource<Option<Preflight>> Source, RowField<Option<Preflight>> Field) Report = RowSource.Opaque(Owner, "check", Option<Preflight>.None);

    private static readonly ReadoutSource<(QueueReadout Shown, DateTimeZone Zone)> Zoned =
        new ReadoutSource<(QueueReadout Shown, DateTimeZone Zone)>.Scoped(static scope => IO.lift(() => (Readout.Value, ((IPlugInViews)scope.Sink).Clock.ToZonedClock().Zone)));

    public static Seq<Child> Rows(QueueSettingRows settings) =>
        Seq<Child>(
            new ControlRow.Group(RowText.Localize("Entries"), RowRules.Always) { Wording = Wording.Localized },
            ListRows.List(RowText.Localize("Entries"), RowText.Localize("Renders of this document's queue, in add order"), new ListRow<QueuedEntry, QueuedEntry>(
                    IterableNE.create<ListColumn<QueuedEntry>>(
                        new ListColumn<QueuedEntry>.Label(None, static entry => entry.Name.IfNone(static () => RowText.Localize("Active viewport")), None, None),
                        new ListColumn<QueuedEntry>.Label(None, static entry => entry.Frames.Count > 1
                            ? RowText.Localize("{0} to {1}", arguments: [(int)entry.Frames.First, (int)entry.Frames.Last])
                            : ((int)entry.Frames.First).ToString(RowText.Culture), None, None),
                        new ListColumn<QueuedEntry>.Label(None, static entry => RowText.Localize("{0} × {1}", arguments: [entry.Extent.Width, entry.Extent.Height]), None, None)),
                    static entry => entry,
                    Listed,
                    Picked) {
                AllowMultipleSelection = true,
                Wording = Wording.Localized,
                Pair = Some((
                        new CommandRow.Run(Face("render-queue-add-view", "Add Active Viewport", "Queue the active viewport at the document's render size"), None,
                            static scope => Added(scope, ActiveEntry)),
                        new CommandRow.Run(Face("render-queue-remove", "Remove Entries", "Remove the selected entries"), None, static scope =>
                            from doc in Opened(scope)
                            from picked in scope.Read(Picked.Source)
                            from book in Held(doc)
                            from removed in book.SwapIO(was => was.Removed(picked))
                            select unit))),
                Commands = static _ => Seq<CommandRow>(
                    new CommandRow.Run(Face("render-queue-add-named-views", "Add Named Views", "Queue every named view of the document"), None, static scope => Added(scope, NamedViewEntries)),
                    new CommandRow.Run(Face("render-queue-add-snapshots", "Add Snapshots", "Queue every snapshot of the document"), None, static scope => Added(scope, SnapshotEntries)),
                    new CommandRow.Run(Face("render-queue-add-sets", "Add Render Sets", "Queue every render set that renders"), None, static scope => Added(scope, SetEntries)),
                    new CommandRow.Run(Face("render-queue-add-study", "Add Sun Study", "Queue the document's sun study, one frame per moment"), None, static scope => Added(scope, StudyEntries)),
                    new CommandRow.Check(Face("render-queue-clay", "Clay", "Render each entry added next with every object in clay"), None,
                        static scope => scope.Read(Clayed.Source), static (scope, on) => scope.Store(Clayed.Source).Put(Some(on))),
                    new CommandRow.Run(Face("render-queue-check", "Check", "Check the queue before it renders"), None, static scope => Checked(scope, Local).Map(static _ => unit)),
                    new CommandRow.Run(Face("render-queue-render", "Render", "Render the queue in this Rhino"), None, static scope => Sent(scope, Local, Fresh)),
                    new CommandRow.Run(Face("render-queue-handoff", "Render in New Rhino", "Save, quit, and render the queue in a new Rhino behind the frontmost window"), None,
                        static scope => Sent(scope, Packaged(static package => new QueueTarget.Handoff(package)), Fresh)),
                    new CommandRow.Run(Face("render-queue-job", "Write Job", "Write the copy, the job, and its schema beside the outputs"), None,
                        static scope => Sent(scope, Packaged(static package => new QueueTarget.JobFile(package)), Fresh)),
                    new CommandRow.Run(Face("render-queue-resume", "Resume", "Render the stopped, failed, or crashed queue from its last frame"), None,
                        static scope => Sent(scope, Local, Unfinished)),
                    new CommandRow.Run(Face("render-queue-test-notice", "Send Test Notice", "Post a test notice to Rhino, the system, and the ntfy topic"), None, static scope =>
                        IPlugInRendering.Served(((IPlugInRendering)scope.Sink).Notices, nameof(IPlugInRendering.Notices))
                            .Bind(static sender => sender.Notify(new QueueEvent.Test()))
                            .Fork()
                            .Map(static _ => unit))),
            },
                RowRules.Always),
            new ControlRow.Group(RowText.Localize("Run"), RowRules.Always) { Wording = Wording.Localized },
            TextRows.Progress(RowText.Localize("Progress"), RowText.Localize("Frames rendered of the queue's total"), new ReadoutSource<WorkState>.Cell(Work), RowRules.Always) with { Wording = Wording.Localized },
            TextRows.Readout(RowText.Localize("State"), RowText.Localize("The queue record's state"), Zoned, static shown => Stated(shown.Shown.State, shown.Zone), RowRules.Always) with { Wording = Wording.Localized },
            TextRows.Readout(RowText.Localize("Remaining"), RowText.Localize("Time left and finish, from the frames rendered"), Zoned, static shown => shown.Shown.Estimate.Description(shown.Zone), RowRules.Always) with { Wording = Wording.Localized },
            ListRows.List(RowText.Localize("Check"), RowText.Localize("Refusals and cautions of the last check"), new ListRow<Error, Error>(
                    IterableNE.create<ListColumn<Error>>(new ListColumn<Error>.Label(None, static error => ErrorOps.Localize(error), None, None)),
                    static error => error,
                    RowSource.Opaque(Owner, "findings", Seq<Error>(), static scope => ValueStore.Of(
                        scope.Read(Report.Source).Map(static check => Some(check.Map(static held => held.Refusals + held.Cautions).IfNone(Seq<Error>()))),
                        static _ => IO.pure(unit),
                        Applied.Live,
                        None)),
                    RowSource.Opaque(Owner, "finding", LanguageExt.HashSet<Error>.Empty)) { Wording = Wording.Localized },
                RowRules.Always))
        + settings.Children;

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
        new(name, RowText.Localize(caption), RowText.Localize(help), None, None) { Wording = Wording.Localized };

    private static IO<RhinoDoc> Opened(RowScope scope) => IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))));

    private static IO<Unit> Added(RowScope scope, Func<RhinoDoc, IO<Seq<Placement>>> read) =>
        from doc in Opened(scope)
        from clay in scope.Read(Clayed.Source)
        from view in IO.lift(() => Missing.Unless(doc.Views.ActiveView, nameof(ViewTable.ActiveView)))
        from output in SceneSources.Read(new SceneSource.Live(doc), ImageOutputState.Read)
        from extent in IO.lift(Framing.Final(output, view.ActiveViewport.Size))
        from placed in read(doc)
        from entries in IO.lift(Callbacks.Each(placed, (row, _) => row.Queued(Callbacks.Found(clay, Albedo.Clay), extent)))
        from book in Held(doc)
        from added in book.SwapIO(was => was.Added(entries))
        select unit;

    private static readonly Func<RhinoDoc, IO<Seq<Placement>>> ActiveEntry = static _ => IO.pure(Seq(Placement.Active));

    private static IO<Seq<Placement>> NamedViewEntries(RhinoDoc doc) =>
        IO.lift(() => Conversions.NonEmpty(Conversions.Rows(doc.NamedViews).Map(Placement.Named), nameof(RhinoDoc.NamedViews)));

    private static IO<Seq<Placement>> SnapshotEntries(RhinoDoc doc) =>
        NamedSnapshots.Names(doc).Map(static names => names.Map(Placement.Snapshot)).Bind(static rows => IO.lift(Conversions.NonEmpty(rows, nameof(NamedSnapshots))));

    private static IO<Seq<Placement>> SetEntries(RhinoDoc doc) =>
        RenderSets.Renderable(doc).Map(static names => names.Map(Placement.Rendered)).Bind(static rows => IO.lift(Conversions.NonEmpty(rows, nameof(RenderSets.Renderable))));

    private static IO<Seq<Placement>> StudyEntries(RhinoDoc doc) =>
        (from copy in use(() => doc.AnimationProperties)
         from study in IO.lift(() => SunWindow.Read(copy))
         select study).Bracket()
            .Bind(static study => IO.lift(study.ToFin(new Missing(nameof(RhinoDoc.AnimationProperties)))))
            .Map(static window => Seq(Placement.Studied(window)));

    private static IO<(QueueContext Context, RhinoDoc Doc, QueuePlan Plan, QueueTarget Target, Preflight Report)> Checked(RowScope scope, Func<Destination, Fin<QueueTarget>> target) =>
        from doc in Opened(scope)
        from context in IPlugInRendering.Served(((IPlugInRendering)scope.Sink).QueueContext, nameof(IPlugInRendering.QueueContext))
        from plan in Plan(context, doc)
        from chosen in IO.lift(target(plan.Destination))
        from report in Preflight.Check(context, doc, plan, chosen)
        from shown in scope.Store(Report.Source).Put(Some(Some(report)))
        select (context, doc, plan, chosen, report);

    private static IO<Unit> Sent(RowScope scope, Func<Destination, Fin<QueueTarget>> target, Func<QueueContext, IO<Option<QueueProgress>>> resumed) =>
        from run in Checked(scope, target)
        from verdict in IO.lift(run.Report.Verdict.ToFin())
        from record in resumed(run.Context)
        from stamp in IO.lift(() => run.Context.Clock.ToZonedClock().GetCurrentLocalDateTime())
        from sent in Dispatch.Send(run.Context, run.Doc, run.Plan, run.Target, stamp, record)
        select sent;

    private static Func<Destination, Fin<QueueTarget>> Packaged(Func<Destination, QueueTarget> target) =>
        destination => Conversions.Validated<NamePart, string, InvalidRhinoValue>("jobs").Map(jobs => target(destination with {
            Folder = destination.Folder.Switch<NamePart, OutputFolder>(
                jobs,
                absolute: static (part, absolute) => new OutputFolder.Absolute(Path.Combine(absolute.Folder, part)),
                besideDocument: static (part, beside) => new OutputFolder.BesideDocument(beside.Segments.Add(part))),
        }));

    private static string Stated(RunState state, DateTimeZone zone) =>
        state.Switch(
            zone,
            absent: static (_, _) => "",
            active: static (at, active) => RowText.Localize("Rendering since {0:g}", arguments: [active.Heard.InZone(at).LocalDateTime]),
            ended: static (_, ended) => RowText.Localize(ended.End.Map(succeeded: "Succeeded", stopped: "Stopped", failed: "Failed")),
            crashed: static (at, crashed) => RowText.Localize("Crashed, last heard {0:g}", arguments: [crashed.Heard.InZone(at).LocalDateTime]));
}
