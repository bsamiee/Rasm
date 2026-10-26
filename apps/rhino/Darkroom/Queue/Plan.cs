using System.Globalization;
using System.IO.Hashing;
using System.Text.Json;
using Darkroom.Output;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.UI;
using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.Render;
using UnitsNet;
using UnitsNet.Units;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct RetryCount : IPresented<RetryCount, int> {
    public static RetryCount MinValue { get; } = new(0);
    public static RetryCount MaxValue { get; } = new(20);
    public static Presentation<int> Presentation { get; } = new();

    public Option<RetryCount> Spent => _value > MinValue._value ? new RetryCount(_value - 1) : None;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Off", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct RetryDelay : IPresented<RetryDelay, int> {
    public static RetryDelay MinValue => Off;
    public static RetryDelay MaxValue { get; } = new(600);
    public static Presentation<int> Presentation { get; } = new() { Unit = Quantity.GetUnitInfo(DurationUnit.Second) };

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

    public bool Holds(SequenceNumber frame) => frame >= First && frame <= Last;

    public static Fin<FrameSpan> Of(int first, int last) =>
        (SequenceNumber.Validated(first), SequenceNumber.Validated(last))
            .Apply(static (from, until) => (From: from, Until: until))
            .As()
            .Bind(static span => ValidationResults.ToFin(Validate(span.From, span.Until, out FrameSpan? spanned), spanned));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref SequenceNumber first, ref SequenceNumber last) =>
        validationError = first <= last && (long)(int)last - (int)first + 1 <= int.MaxValue
            ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"frame span {first} to {last} is reversed or longer than {int.MaxValue} frames"));
}

public sealed record QueuedEntry(
    bool Renders, Option<Albedo> Clay, RenderSourceState Source, FrameSpan Frames, PixelExtent Extent, Overscan Overscan, Option<RenderRegion> Region, Option<SunWindow> Sun) {
    public Option<string> View => Source.NamedView | Source.Snapshot | Source.SpecificViewport;
}

public sealed record Placement(RenderSourceState Source, Option<SunWindow> Sun) {
    public static Placement Active { get; } = new(RenderSourceState.Default, None);

    public static Placement Named(string name) => Active with { Source = Active.Source with { RenderSource = RenderSettings.RenderingSources.NamedView, NamedView = Some(name) } };

    public static Placement Snapshot(string name) => Active with { Source = Active.Source with { RenderSource = RenderSettings.RenderingSources.SnapShot, Snapshot = Some(name) } };

    // --- [READS]
    public static IO<Seq<Placement>> Views(RhinoDoc doc) => NamedTable.Views.Entries(doc).Map(static entries => entries.Map(static entry => Named(entry.Name)));

    public static IO<Seq<Placement>> Snapshots(RhinoDoc doc) => NamedTable.Snapshots.Entries(doc).Map(static entries => entries.Map(static entry => Snapshot(entry.Name)));

    public static IO<Seq<Placement>> Study(RhinoDoc doc) =>
        DisposalOps.Within(() => doc.AnimationProperties, static copy => IO.lift(() => SunWindow.Read(copy)
            .Bind(static found => found.ToFin(new Refused(nameof(RhinoDoc.AnimationProperties), Refused.Absent)))
            .Map(static window => Seq(Active with { Sun = Some(window) }))));

    public static IO<Seq<QueuedEntry>> Entries(RhinoDoc doc, Seq<Placement> placements, Option<Albedo> clay) =>
        from view in IO.lift(() => Refused.UnlessFound(doc.Views.ActiveView, nameof(ViewTable.ActiveView)))
        from output in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), ImageOutputState.Read)
        from extent in IO.lift(() => Framing.Final(output, view.ActiveViewport.Size) switch { var size => PixelExtent.Validated(size.Width, size.Height) })
        from entries in IO.lift(placements.Traverse(placement => placement.Queued(clay, extent)).As())
        select entries;

    private Fin<QueuedEntry> Queued(Option<Albedo> clay, PixelExtent extent) =>
        FrameSpan.Of(0, Sun.Map(static window => window.Frames.Count - 1).IfNone(0))
            .Map(frames => new QueuedEntry(Renders: true, clay, Source, frames, extent, Overscan.Off, None, Sun));
}

public sealed record RenderQueue(Seq<QueuedEntry> Entries) {
    public static RenderQueue Empty { get; } = new(Seq<QueuedEntry>());

    internal static DictionaryCodec<RenderQueue> Codec { get; } = DictionaryCodec.Json<RenderQueue>(nameof(RenderQueue), 1);

    public static IO<RenderQueue> Read(RhinoDoc doc) =>
        ArchivableDictionaries.Read(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), Codec).Map(static held => held.IfNone(Empty));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QueueEdit {
    public IO<RenderQueue> Committed(RhinoDoc doc) =>
        from prior in RenderQueue.Read(doc)
        let next = Applied(prior)
        from committed in when(!next.Touched.IsEmpty, Commits.Commit(
                doc,
                $"{Captions.Of(GetType())} {string.Join(", ", next.Touched.Map(static entry => entry.View.IfNone(() => Captions.Of(entry.Source.RenderSource.ToString()))))}",
                new RedrawPolicy.Silent(),
                ArchivableDictionaries.Write(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), RenderQueue.Codec, next.Queue))
            .Map(static _ => unit)).As()
        select next.Queue;

    private (RenderQueue Queue, Seq<QueuedEntry> Touched) Applied(RenderQueue prior) =>
        Switch(prior,
            add: static (held, add) => add.Entries.Fold(Seq<QueuedEntry>(), (fresh, entry) =>
                    (held.Entries + fresh).Exists(kept => kept with { Renders = entry.Renders } == entry) ? fresh : fresh.Add(entry)) switch {
                var fresh => (new RenderQueue(held.Entries + fresh), fresh),
            },
            remove: static (held, remove) => (
                new RenderQueue(toSeq(held.Entries.Where((_, index) => !remove.Indices.Contains(index)))),
                toSeq(held.Entries.Where((_, index) => remove.Indices.Contains(index)))),
            toggle: static (held, toggle) => (
                new RenderQueue(held.Entries.Map((entry, index) => index == toggle.Index ? entry with { Renders = toggle.Renders } : entry)),
                held.Entries.At(toggle.Index).Filter(entry => entry.Renders != toggle.Renders).ToSeq()));

    public sealed record Add(Seq<QueuedEntry> Entries) : QueueEdit;

    public sealed record Remove(LanguageExt.HashSet<int> Indices) : QueueEdit;

    public sealed record Toggle(int Index, bool Renders) : QueueEdit;
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
    public ContentKey Key => (ContentKey)XxHash128.HashToUInt128(JsonSerializer.SerializeToUtf8Bytes(QueueJob.Identity(this), JobContext.Default.Entry));

    public Fin<OverscanFrame> Rendered => OverscanFrame.Around(Extent, Overscan);

    public Fin<Option<SunMoment>> Moment(SequenceNumber frame) =>
        Sun.Traverse(window => window.Frames.At((int)frame)
                .ToFin(Error.New(string.Create(CultureInfo.InvariantCulture, $"frame {frame} lies past sun study's {window.Frames.Count} moments")))
                .Bind(ValidationResults.Validated<SunMoment, LocalDateTime>))
            .As();

    public QueueEntry<TPlaced> Placed<TPlaced>(Seq<QueueOutput<TPlaced>> outputs) => new(Override, Source, Frames, Extent, Overscan, Region, Sun, Destination, outputs);
}

public sealed record RenderBatch(Seq<QueueEntry<OutputPath>> Entries) {
    public long Frames => Entries.Fold(0L, static (sum, entry) => sum + entry.Frames.Count);

    public static IO<RenderBatch> Placed(RhinoDoc doc, Seq<QueueEntry<Unit>> entries, bool overwrite) =>
        from groups in Callbacks.Each(toSeq(entries.GroupBy(entry => entry.Destination with { Overwrite = entry.Destination.Overwrite || overwrite })).Map(batch =>
            from files in IO.pure(from entry in toSeq(batch) from output in entry.Outputs select (entry.Key, Output: output))
            from placed in Destinations.Resolve(doc, batch.Key, files.Map(static file => file.Output.Name))
            select files.Zip(placed, static (file, row) => (file.Key, Output: new QueueOutput<OutputPath>(file.Output.Target, row.Name, row.Path)))))
        let outputs = groups.Bind(static batch => batch).ToLookup(static row => row.Key, static row => row.Output)
        select new RenderBatch(entries.Map(entry => entry.Placed(toSeq(outputs[entry.Key]))));
}

public sealed record QueuePlan(Destination Destination, Seq<QueueEntry<Unit>> Entries) {
    public static IO<QueuePlan> Read(RhinoDoc doc) =>
        from queue in RenderQueue.Read(doc)
        from book in OutputBook.Read(doc)
        from bindings in RenderOverride.Bindings(doc)
        from values in bindings.Capture
        from saved in IO.lift(() => Conversions.Present(doc.Path))
        from plan in IO.lift(Of(book, saved, queue.Entries.Filter(static entry => entry.Renders).Map(entry => (Entry: entry, Override: new RenderOverride(values, entry.Clay)))))
        select plan;

    public static Fin<QueuePlan> Of(OutputBook book, Option<string> saved, Seq<(QueuedEntry Entry, RenderOverride Override)> entries) =>
        from destination in Destinations.Resolved(book.Destination, saved)
        from planned in entries.Traverse(item =>
                from scope in item.Entry.View.Traverse(NamePart.Validated).As()
                from named in book.Names(scope.ToSeq())
                select new QueueEntry<Unit>(item.Override, item.Entry.Source, item.Entry.Frames, item.Entry.Extent, item.Entry.Overscan, item.Entry.Region, item.Entry.Sun,
                    destination, named.Map(static held => new QueueOutput<Unit>(held.Target, held.Name, unit))))
            .As()
        from unique in Callbacks.Unique(planned, static entry => entry.Key, nameof(QueuePlan))
        select new QueuePlan(book.Destination, unique);

    public IO<RenderBatch> Resolved(RhinoDoc doc, Option<QueueProgress> resumed) =>
        RenderBatch.Placed(doc, Entries.Map(entry => entry.Placed(
            from frame in entry.Frames.Numbers
            from output in entry.Outputs
            select output with {
                Name = output.Name with {
                    Number = entry.Frames.Named(frame),
                    Version = resumed.Bind(record => record.Entries.Find(entry.Key)).Bind(held => held.Versions.Find(output.Name.Scope))
                        .Match<OutputVersioning>(Some: static version => new OutputVersioning.Pinned(version), None: static () => new OutputVersioning.NextFree()),
                },
            })), resumed.IsSome);
}

public sealed record QueueSettings(
    RetryCount Retries, RetryDelay Delay, bool SampleEstimate, bool KeepAwake, bool ShutDownAfterSuccess, QueueHeartbeat Heartbeat, NoticeSettings Notices, FlamencoSettings Flamenco)
    : IStateRecord<QueueSettings, QueueParameter> {
    public static QueueSettings Default { get; } = new(
        RetryCount.Create(3), RetryDelay.Create(1), SampleEstimate: true, KeepAwake: true, ShutDownAfterSuccess: false, QueueHeartbeat.Default, NoticeSettings.Default, FlamencoSettings.Default);

    public static IO<QueueSettings> Read(IPlugInViews views) => PlugInSettings.Record<QueueSettings>(views.Settings, Applied.Live).Read.Map(static held => held.IfNone(Default));
}

[SmartEnum<string>]
public sealed partial class QueueParameter : IStateParameter<QueueSettings> {
    public static readonly QueueParameter Retries = new("retries", StateParameter.Bounded(
            Lens<QueueSettings, RetryCount>.New(static settings => settings.Retries, static retries => settings => settings with { Retries = retries })),
        new("Retries", "Rhino restarts that resume a crashed queue"));
    public static readonly QueueParameter Delay = new("retry-delay", StateParameter.Bounded(
            Lens<QueueSettings, RetryDelay>.New(static settings => settings.Delay, static delay => settings => settings with { Delay = delay })),
        new("Retry delay", "Wait before each restart"));
    public static readonly QueueParameter SampleEstimate = new("sample-estimate", new StateParameter<QueueSettings>.Toggle(
            Lens<QueueSettings, bool>.New(static settings => settings.SampleEstimate, static sampled => settings => settings with { SampleEstimate = sampled })),
        new("Estimate from first frames", "Renders 2 frames per entry first to post an estimated finish"));
    public static readonly QueueParameter KeepAwake = new("keep-awake", new StateParameter<QueueSettings>.Toggle(
            Lens<QueueSettings, bool>.New(static settings => settings.KeepAwake, static awake => settings => settings with { KeepAwake = awake })),
        new("Keep computer awake", "Prevents system sleep while the queue renders"));
    public static readonly QueueParameter ShutDownAfterSuccess = new("shut-down-after-success", new StateParameter<QueueSettings>.Toggle(
            Lens<QueueSettings, bool>.New(static settings => settings.ShutDownAfterSuccess, static shut => settings => settings with { ShutDownAfterSuccess = shut })),
        new("Shut down when done", "Shuts the computer down after the queue succeeds"));
    public static readonly QueueParameter Heartbeat = new("heartbeat", new StateParameter<QueueSettings>.Record<QueueHeartbeat>(
            Lens<QueueSettings, QueueHeartbeat>.New(static settings => settings.Heartbeat, static heartbeat => settings => settings with { Heartbeat = heartbeat })),
        new("Heartbeat", "Liveness writes to the queue record"));
    public static readonly QueueParameter Notices = new("notices", new StateParameter<QueueSettings>.Record<NoticeSettings>(
            Lens<QueueSettings, NoticeSettings>.New(static settings => settings.Notices, static notices => settings => settings with { Notices = notices })),
        new("Notices", "ntfy topic queue notices post to"));
    public static readonly QueueParameter Flamenco = new("flamenco", new StateParameter<QueueSettings>.Record<FlamencoSettings>(
            Lens<QueueSettings, FlamencoSettings>.New(static settings => settings.Flamenco, static flamenco => settings => settings with { Flamenco = flamenco })),
        new("Flamenco", "Flamenco Manager the queue submits farm jobs to"));

    public StateParameter<QueueSettings> Kind { get; }

    public ParameterText Text { get; }
}
