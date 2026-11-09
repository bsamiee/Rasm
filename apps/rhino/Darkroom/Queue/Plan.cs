using System.Drawing;
using System.Text.Json;
using Darkroom.Chain;
using Darkroom.Output;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Render;
using Thinktecture.Collections;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record FrameSpan(Seq<SequenceNumber> Numbers) {
    public const double Rate = 24d;

    public Option<SequenceNumber> Named(SequenceNumber frame) => Numbers.Count > 1 ? Some(frame) : None;

    public Option<Duration> Time(SequenceNumber frame) => Named(frame).Map(static number => Duration.FromSeconds((int)number / Rate));

    public static Fin<FrameSpan> Of(Option<SunWindow> sun) =>
        toSeq(Enumerable.Range(0, sun.Map(static window => window.Frames.Count).IfNone(1))).Traverse(SequenceNumber.Validated).As().Map(static numbers => new FrameSpan(numbers));
}

internal sealed record QueuedEntry(bool Renders, RenderSettings.RenderingSources Source, Option<string> View, Size Extent) {
    public Option<float> Clay { get; init; }
    public double Overscan { get; init; }
    public Option<RenderRegion> Region { get; init; }
    public Option<SunWindow> Sun { get; init; }

    public string Label => View.IfNone(Source.ToString);
}

internal sealed record QueuePlacement(RenderSettings.RenderingSources Source) {

    // --- [STATE]

    public static QueuePlacement Active { get; } = new(RenderSettings.RenderingSources.ActiveViewport);

    public Option<string> View { get; init; }
    public Option<SunWindow> Sun { get; init; }

    // --- [READS]

    public static IO<Seq<QueuePlacement>> Viewed(RhinoDoc doc) =>
        NamedViews.Table.Entries(doc).Map(static entries => entries.Map(static entry => Active with { Source = RenderSettings.RenderingSources.NamedView, View = entry.Name }));

    public static IO<Seq<QueuePlacement>> Snapped(RhinoDoc doc) =>
        Snapshots.Table.Entries(doc).Map(static entries => entries.Map(static entry => Active with { Source = RenderSettings.RenderingSources.SnapShot, View = entry.Name }));

    public static IO<Seq<QueuePlacement>> Studied(RhinoDoc doc) =>
        (from copy in use(() => doc.AnimationProperties)
         from window in IO.lift(SunWindow.Read(copy).Bind(static found => found.ToFin(new DarkroomFault("document sun study", DarkroomReason.Missing))))
         select Seq(Active with { Sun = window })).Bracket();

    public static IO<Seq<QueuedEntry>> Entries(RhinoDoc doc, Seq<QueuePlacement> placements, QueueRequest request) =>
        from extent in SaveSource.Extent(doc)
        let size = new Size(extent.Width, extent.Height)
        from region in request.Region.Read(doc, size)
        select placements.Map(placement => new QueuedEntry(Renders: true, placement.Source, placement.View, size) {
            Clay = request.Clay, Overscan = request.Overscan, Region = region, Sun = placement.Sun,
        });
}

internal sealed record QueueState(Seq<QueuedEntry> Entries) {
    private static readonly DictionaryCodec<QueueState> Codec = ArchivableDictionaries.Json<QueueState>(nameof(QueueState), 3);

    public static QueueState Empty { get; } = new([]);

    public static IO<QueueState> Read(RhinoDoc doc) =>
        ArchivableDictionaries.Read(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), Codec).Map(static held => held.IfNone(Empty));

    public IO<Unit> Written(RhinoDoc doc) => ArchivableDictionaries.Write(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), Codec, this);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record QueueEdit {
    public IO<QueueState> Committed(RhinoDoc doc) =>
        from prior in QueueState.Read(doc)
        let next = Applied(toSeq(prior.Entries))
        from committed in when(!next.Touched.IsEmpty, Commits.Commit(
                doc, $"{GetType().Name} {string.Join(", ", next.Touched.Map(static entry => entry.Label))}", new RedrawPolicy.Silent(), next.Queue.Written(doc))
            .Map(static _ => unit)).As()
        select next.Queue;

    private (QueueState Queue, Seq<QueuedEntry> Touched) Applied(Seq<QueuedEntry> prior) =>
        Switch(prior,
            add: static (held, add) => toSeq(add.Entries.Except(held, new ProjectionEqualityComparer<QueuedEntry, QueuedEntry>(static entry => entry with { Renders = false }))) switch {
                var fresh => (new QueueState(held + fresh), fresh),
            },
            remove: static (held, remove) => held.Map(static (entry, index) => (Entry: entry, Index: index)).Partition(row => remove.Indices.Contains(row.Index)) switch {
                var (removed, remaining) => (new QueueState(remaining.Map(static row => row.Entry)), removed.Map(static row => row.Entry)),
            },
            toggle: static (held, toggle) => (
                new QueueState(held.Map((entry, index) => index == toggle.Index ? entry with { Renders = toggle.Renders } : entry)),
                held.At(toggle.Index).Filter(entry => entry.Renders != toggle.Renders).ToSeq()));

    public sealed record Add(Seq<QueuedEntry> Entries) : QueueEdit;

    public sealed record Remove(HashSet<int> Indices) : QueueEdit;

    public sealed record Toggle(int Index, bool Renders) : QueueEdit;
}

internal sealed record QueueOutput<TPath>(OutputTarget Target, OutputName Name, TPath Path);

internal sealed record QueueEntry<TPath>(QueuedEntry Queued, RenderSettings Settings, string Folder, NamePart Stem, bool Overwrite, FrameSpan Frames, Seq<QueueOutput<TPath>> Outputs) {
    public UInt128 Key => ContentKeys.Of<QueuedEntry>(hash => hash.Bytes(JsonSerializer.SerializeToUtf8Bytes(QueueJob.Identity(this), ArchivableDictionaries.Options)));

    public Fin<OverscanFrame> Rendered => PixelExtent.Validated(Queued.Extent.Width, Queued.Extent.Height).Bind(extent => OverscanFrame.Around(extent, Queued.Overscan));

    public Option<LocalDateTime> Moment(SequenceNumber frame) => Queued.Sun.Bind(window => window.Frames.At((int)frame));

    public QueueEntry<TPlaced> Placed<TPlaced>(Seq<QueueOutput<TPlaced>> outputs) => new(Queued, Settings, Folder, Stem, Overwrite, Frames, outputs);
}

internal sealed record RenderBatch(Seq<QueueEntry<OutputPath>> Entries) {
    public long Frames => Entries.Fold(0L, static (sum, entry) => sum + entry.Frames.Numbers.Count);

    public static IO<RenderBatch> Placed(RhinoDoc doc, Seq<QueueEntry<Unit>> entries, bool overwrite) =>
        from groups in Products.Each(toSeq(entries.GroupBy(entry => (entry.Folder, entry.Stem, Overwrite: entry.Overwrite || overwrite))).Map(batch =>
            from files in IO.pure(from entry in toSeq(batch) let key = entry.Key from output in entry.Outputs select (Key: key, Output: output))
            from placed in Destinations.Resolve(doc, new OutputFolder.Absolute(batch.Key.Folder), new Stem.Literal(batch.Key.Stem), batch.Key.Overwrite, OutputConvention.Default,
                files.Map(static file => file.Output.Name))
            select files.Zip(placed, static (file, row) => (file.Key, Output: new QueueOutput<OutputPath>(file.Output.Target, row.Name, row.Path)))))
        let outputs = groups.Bind(static batch => batch).ToLookup(static row => row.Key, static row => row.Output)
        select new RenderBatch(entries.Map(entry => entry.Placed(toSeq(outputs[entry.Key]))));
}

internal sealed record QueuePlan {

    // --- [STATE]

    private QueuePlan(OutputBook book, Seq<QueueEntry<Unit>> entries) => (Book, Entries) = (book, entries);

    public OutputBook Book { get; }
    public Seq<QueueEntry<Unit>> Entries { get; }

    // --- [PLANS]

    public static IO<QueuePlan> Read(RhinoDoc doc) =>
        from queue in QueueState.Read(doc)
        from book in OutputBook.Read(doc)
        from settings in Products.Acquired(RenderSettingsSources.Snapshot(doc))
        from saved in IO.lift(() => Conversions.Present(doc.Path))
        from plan in IO.lift(Of(book, saved, settings, toSeq(queue.Entries).Filter(static entry => entry.Renders)))
        select plan;

    public static Fin<QueuePlan> Of(OutputBook book, Option<string> saved, RenderSettings settings, Seq<QueuedEntry> entries) =>
        from location in (book.Folder.Of(saved), book.Stem.Of(saved), toSeq(book.Targets).Traverse(static target => target.Encodable).As())
            .Apply(static (folder, stem, _) => (Folder: folder, Stem: stem)).As()
        from planned in entries.Map(static (entry, index) => (Entry: entry, Index: index)).Traverse(held =>
                (held.Entry.View.Traverse(NamePart.Validated).As(), FrameSpan.Of(held.Entry.Sun))
                    .Apply((scope, frames) => new QueueEntry<Unit>(held.Entry, settings, location.Folder, location.Stem, book.Overwrite, frames, book.Names(scope.ToSeq())))
                    .As()
                    .MapFail(error => new DarkroomFault(held.Entry.Label, DarkroomReason.Invalid, held.Index, error)))
            .As()
        from unique in Conversions.Unique(planned, static entry => entry.Key)
        select new QueuePlan(book, unique);

    public IO<RenderBatch> Resolved(RhinoDoc doc, Option<QueueProgress> resumed) =>
        RenderBatch.Placed(doc, Entries.Map(entry => resumed.Bind(record => record.Entries.Find(entry.Key)) switch {
            var prior => entry.Placed(
                from frame in entry.Frames.Numbers
                from output in entry.Outputs
                select output with {
                    Name = output.Name with {
                        Number = entry.Frames.Named(frame),
                        Version = prior.Bind(held => held.Versions.Find(output.Name.Scope))
                            .Match<OutputVersioning>(Some: static version => new OutputVersioning.Pinned(version), None: static () => new OutputVersioning.Next()),
                    },
                }),
        }), resumed.IsSome);
}

internal sealed record QueueHeartbeat(Duration Interval, int Misses) {
    public Duration Silence => Interval * Misses;
}

internal sealed record QueueSettings(int Retries, Duration Delay, bool SampleEstimate, bool KeepAwake, bool ShutDownAfterSuccess, QueueHeartbeat Heartbeat, NoticeSettings Notices, FlamencoSettings Flamenco) {

    // --- [SETTINGS]

    public static IO<QueueSettings> Read =>
        IO.lift(static () => new SettingsNode(new SettingsRoot.PlugInNode(Session.PlugIn.Id)).Child("Queue")).Bind(static node =>
            (Preferences.Setting(node, "Retries", 3, Some<Func<int, bool>>(static count => count >= 0)).Read,
             Preferences.Setting(node, "DelaySeconds", 1d, Some<Func<double, bool>>(static seconds => seconds >= 0d)).Read,
             Preferences.Setting(node, "SampleEstimate", fallback: true).Read,
             Preferences.Setting(node, "KeepAwake", fallback: true).Read,
             Preferences.Setting(node, "ShutDownAfterSuccess", fallback: false).Read,
             Preferences.Setting(node, "HeartbeatSeconds", 3d, Some<Func<double, bool>>(static seconds => seconds > 0d)).Read,
             Preferences.Setting(node, "HeartbeatMisses", 10, Some<Func<int, bool>>(static misses => misses > 0)).Read,
             Notified(node),
             Farmed(node))
            .Apply(static (retries, delay, sample, awake, shutDown, beat, misses, notices, farm) => new QueueSettings(
                retries, Duration.FromSeconds(delay), sample, awake, shutDown, new QueueHeartbeat(Duration.FromSeconds(beat), misses), notices, farm))
            .As());

    private static IO<NoticeSettings> Notified(SettingsNode node) =>
        (Preferences.Setting(node, "NoticeServer", "https://ntfy.sh").Read, Preferences.Setting(node, "NoticeTopic").Read)
            .Apply(static (server, topic) => Located(server).Map(located => new NoticeSettings(located, topic)))
            .As()
            .Bind(IO.lift);

    private static IO<FlamencoSettings> Farmed(SettingsNode node) =>
        (Preferences.Setting(node, "FlamencoManager", "http://localhost:8080").Read,
         Preferences.Setting(node, "FlamencoPlatform", WorkerPlatform.Current().Key).Read,
         Preferences.Setting(node, "FlamencoPriority", 50, Some<Func<int, bool>>(static priority => priority is >= 0 and <= 100)).Read,
         Preferences.Setting(node, "FlamencoScripts").Read,
         Preferences.Setting(node, "FlamencoWorkerTag").Read)
            .Apply(static (manager, platform, priority, scripts, tag) => (
                    Located(manager),
                    WorkerPlatform.TryGet(platform, out WorkerPlatform? worker) ? Fin.Succ(worker) : new DarkroomFault(platform, DarkroomReason.Invalid),
                    tag.Traverse(static text => Guid.TryParse(text, out Guid id) ? Fin.Succ(id) : new DarkroomFault(text, DarkroomReason.Invalid)).As())
                .Apply((located, worker, workerTag) => new FlamencoSettings(located, worker, priority, scripts, workerTag))
                .As())
            .As()
            .Bind(IO.lift);

    private static Fin<Uri> Located(string text) => Uri.TryCreate(text, UriKind.Absolute, out Uri? uri) ? uri : new DarkroomFault(text, DarkroomReason.Invalid);
}
