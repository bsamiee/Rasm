using System.Drawing;
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
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FrameSpan(Seq<SequenceNumber> Numbers) {
    public const double Rate = 24d;

    public static SequenceNumber First { get; } = SequenceNumber.Create(0);

    public Option<SequenceNumber> Named(SequenceNumber frame) => Numbers.Count > 1 ? Some(frame) : None;

    public Option<TimeSpan> Time(SequenceNumber frame) => Named(frame).Map(static number => TimeSpan.FromSeconds((int)number / Rate));

    public static FrameSpan Of(SunWindow? sun) =>
        new(toSeq(LanguageExt.List.unfold(Some(First), static at => at.Map(static number => (number, number.Next())))).Take(Optional(sun).Map(static window => window.Frames.Count).IfNone(1)));
}

public sealed record QueuedEntry(bool Renders, RenderSettings.RenderingSources Source, string? View, Size Extent) {
    public float? Clay { get; init; }
    public double Overscan { get; init; }
    public RenderRegion? Region { get; init; }
    public SunWindow? Sun { get; init; }
}

public sealed record Placement(RenderSettings.RenderingSources Source) {
    public static Placement Active { get; } = new(RenderSettings.RenderingSources.ActiveViewport);

    public Option<string> View { get; init; }
    public Option<SunWindow> Sun { get; init; }

    public static Placement Named(string name) => Active with { Source = RenderSettings.RenderingSources.NamedView, View = name };

    public static Placement Snapshot(string name) => Active with { Source = RenderSettings.RenderingSources.SnapShot, View = name };

    // --- [READS]
    public static IO<Seq<Placement>> Views(RhinoDoc doc) => NamedTable.Views.Entries(doc).Map(static entries => entries.Map(static entry => Named(entry.Name)));

    public static IO<Seq<Placement>> Snapshots(RhinoDoc doc) => NamedTable.Snapshots.Entries(doc).Map(static entries => entries.Map(static entry => Snapshot(entry.Name)));

    public static IO<Seq<Placement>> Study(RhinoDoc doc) =>
        DisposalOps.Within(() => doc.AnimationProperties, static copy => IO.lift(() => SunWindow.Read(copy)
            .Bind(static found => found.ToFin(new Refused(nameof(RhinoDoc.AnimationProperties), Refused.Absent)))
            .Map(static window => Seq(Active with { Sun = window }))));

    public static IO<Seq<QueuedEntry>> Entries(RhinoDoc doc, Seq<Placement> placements, Option<float> clay) =>
        from extent in IO.lift(() => RenderPipeline.RenderSize(doc, fromRenderSources: true))
        select placements.Map(placement => new QueuedEntry(Renders: true, placement.Source, placement.View.ValueUnsafe(), extent) { Clay = clay.ToNullable(), Sun = placement.Sun.ValueUnsafe() });
}

public sealed record QueueState(ImmutableArray<QueuedEntry> Entries) {
    public static QueueState Empty { get; } = new([]);

    internal static DictionaryCodec<QueueState> Codec { get; } = ArchivableDictionaries.Json<QueueState>(nameof(QueueState), 2);

    public static IO<QueueState> Read(RhinoDoc doc) =>
        ArchivableDictionaries.Read(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), Codec).Map(static held => held.IfNone(Empty));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QueueEdit {
    public IO<QueueState> Committed(RhinoDoc doc) =>
        from prior in QueueState.Read(doc)
        let next = Applied(toSeq(prior.Entries))
        from committed in when(!next.Touched.IsEmpty, Commits.Commit(
                doc,
                $"{GetType().Name} {string.Join(", ", next.Touched.Map(static entry => entry.View ?? entry.Source.ToString()))}",
                new RedrawPolicy.Silent(),
                ArchivableDictionaries.Write(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), QueueState.Codec, next.Queue))
            .Map(static _ => unit)).As()
        select next.Queue;

    private (QueueState Queue, Seq<QueuedEntry> Touched) Applied(Seq<QueuedEntry> prior) =>
        Switch(prior,
            add: static (held, add) => add.Entries.Fold(Seq<QueuedEntry>(), (fresh, entry) =>
                    (held + fresh).Exists(kept => kept with { Renders = entry.Renders } == entry) ? fresh : fresh.Add(entry)) switch {
                        var fresh => (new QueueState([.. held + fresh]), fresh),
                    },
            remove: static (held, remove) => (
                new QueueState([.. held.Where((_, index) => !remove.Indices.Contains(index))]),
                toSeq(held.Where((_, index) => remove.Indices.Contains(index)))),
            toggle: static (held, toggle) => (
                new QueueState([.. held.Map((entry, index) => index == toggle.Index ? entry with { Renders = toggle.Renders } : entry)]),
                held.At(toggle.Index).Filter(entry => entry.Renders != toggle.Renders).ToSeq()));

    public sealed record Add(Seq<QueuedEntry> Entries) : QueueEdit;

    public sealed record Remove(LanguageExt.HashSet<int> Indices) : QueueEdit;

    public sealed record Toggle(int Index, bool Renders) : QueueEdit;
}

public sealed record QueueOutput<TPath>(OutputTarget Target, OutputName Name, TPath Path);

public sealed record QueueEntry<TPath>(QueuedEntry Queued, RenderOverride Override, string Folder, NamePart Stem, bool Overwrite, Seq<QueueOutput<TPath>> Outputs) {
    public FrameSpan Frames => FrameSpan.Of(Queued.Sun);

    public ContentKey Key => (ContentKey)XxHash128.HashToUInt128(JsonSerializer.SerializeToUtf8Bytes(QueueJob.Identity(this), JobContext.Default.Entry));

    public Fin<OverscanFrame> Rendered => PixelExtent.Validated(Queued.Extent.Width, Queued.Extent.Height).Bind(extent => OverscanFrame.Around(extent, Queued.Overscan));

    public Option<LocalDateTime> Moment(SequenceNumber frame) => Optional(Queued.Sun).Bind(window => window.Frames.At((int)frame));

    public QueueEntry<TPlaced> Placed<TPlaced>(Seq<QueueOutput<TPlaced>> outputs) => new(Queued, Override, Folder, Stem, Overwrite, outputs);
}

public sealed record RenderBatch(Seq<QueueEntry<OutputPath>> Entries) {
    public long Frames => Entries.Fold(0L, static (sum, entry) => sum + entry.Frames.Numbers.Count);

    public static IO<RenderBatch> Placed(RhinoDoc doc, Seq<QueueEntry<Unit>> entries, bool overwrite) =>
        from groups in Callbacks.Each(toSeq(entries.GroupBy(entry => (entry.Folder, entry.Stem, Overwrite: entry.Overwrite || overwrite))).Map(batch =>
            from files in IO.pure(from entry in toSeq(batch) from output in entry.Outputs select (entry.Key, Output: output))
            from placed in Destinations.Resolve(doc, new OutputFolder.Absolute(batch.Key.Folder), new Stem.Literal(batch.Key.Stem), batch.Key.Overwrite, files.Map(static file => file.Output.Name))
            select files.Zip(placed, static (file, row) => (file.Key, Output: new QueueOutput<OutputPath>(file.Output.Target, row.Name, row.Path)))))
        let outputs = groups.Bind(static batch => batch).ToLookup(static row => row.Key, static row => row.Output)
        select new RenderBatch(entries.Map(entry => entry.Placed(toSeq(outputs[entry.Key]))));
}

public sealed record QueuePlan(OutputBook Book, Seq<QueueEntry<Unit>> Entries) {
    public static IO<QueuePlan> Read(RhinoDoc doc) =>
        from queue in QueueState.Read(doc)
        from book in OutputBook.Read(doc)
        from settings in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), static scope => IO.lift(scope.Settings.Duplicate))
        from saved in IO.lift(() => Conversions.Present(doc.Path))
        from plan in IO.lift(Of(book, saved, settings, toSeq(queue.Entries).Filter(static entry => entry.Renders)))
        select plan;

    public static Fin<QueuePlan> Of(OutputBook book, Option<string> saved, RenderSettings settings, Seq<QueuedEntry> entries) =>
        from located in Destinations.Located(book.Folder, book.Stem, saved)
        from planned in entries.Traverse(entry =>
                from scope in Optional(entry.View).Traverse(static view => NamePart.TryCreate(view, out NamePart? part, out RhinoFault? fault) ? Fin.Succ(part) : fault).As()
                select new QueueEntry<Unit>(entry, new RenderOverride(settings, Optional(entry.Clay)), located.Folder, located.Stem, book.Overwrite,
                    book.Names(scope.ToSeq()).Map(static held => new QueueOutput<Unit>(held.Target, held.Name, unit))))
            .As()
        from unique in Callbacks.Unique(planned, static entry => entry.Key, nameof(QueuePlan))
        select new QueuePlan(book, unique);

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

public sealed record QueueSettings(int Retries, TimeSpan Delay, bool SampleEstimate, bool KeepAwake, bool ShutDownAfterSuccess, QueueHeartbeat Heartbeat, NoticeSettings Notices, FlamencoSettings Flamenco) {
    public static QueueSettings Default { get; } = new(
        Retries: 3, TimeSpan.FromSeconds(1), SampleEstimate: true, KeepAwake: true, ShutDownAfterSuccess: false, QueueHeartbeat.Default, NoticeSettings.Default, FlamencoSettings.Default);

    public static IO<QueueSettings> Read(IPlugInViews views) => PlugInSettings.Record(views.Settings, Default, Applied.Live).Read.Map(static held => held.IfNone(Default));
}
