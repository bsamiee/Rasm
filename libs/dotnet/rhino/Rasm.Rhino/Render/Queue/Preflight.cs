using Rasm.Rhino.Blocks;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Scenes;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;

namespace Rasm.Rhino.Render.Queue;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Preflight(Seq<Error> Refusals, Seq<Error> Cautions, DocumentStatistics Statistics, Map<int, ulong> Memory) {
    // --- [VERDICT]
    public Validation<Error, Unit> Verdict =>
        Refusals.Traverse(static refusal => Validation.Fail<Error, Unit>(refusal)).As().Map(static _ => unit);

    // --- [CHECK]
    public static IO<Preflight> Check(QueueContext context, RhinoDoc doc, QueuePlan plan, QueueTarget target) =>
        Read(context, doc, plan, target.Switch(
            inProcess: static _ => new Reach(Local: true, Quits: false, Package: None),
            handoff: static handoff => new Reach(Local: true, Quits: true, Package: Some(handoff.Package)),
            jobFile: static file => new Reach(Local: false, Quits: false, Package: Some(file.Package))));

    private static IO<Preflight> Read(QueueContext context, RhinoDoc doc, QueuePlan plan, Reach reach) =>
        (DocumentStatistics.Read(doc),
         Links.Closure(doc.RuntimeSerialNumber),
         Links.Style(doc).Read,
         NamedSnapshots.Names(doc).Map(names => plan.Entries.Map((entry, index) => Entered(doc, names, entry, index)).Flatten().Strict()),
         Sources.Read(new SceneSource.Document(doc), RenderChannelsState.Read).Map(QueueEstimate.Channels),
         reach.Local
             ? QueueProgress.Read(context.Record).Bind(prior => QueueProgress.State(prior, context.Record, context.Clock)).Map(static state => Some(state))
             : IO.pure(Option<RunState>.None),
         IO.lift(Host.Read),
         Measured(doc, plan, reach),
         IO.lift(() => Textured(doc)))
        .Apply((statistics, links, style, entered, channels, run, host, disk, textures) => Judged(reach, plan, statistics, links, style, entered, channels, run, host, disk, textures))
        .As();

    // --- [READS]
    private static Seq<Error> Entered(RhinoDoc doc, Seq<string> snapshots, QueueEntry<PlannedOutput> entry, int index) =>
        (Held(doc, snapshots, entry.Source) ? Seq<Error>() : Seq<Error>(new SourceViewMissing(index, entry.Source.RenderSource)))
        + (entry.Overscan == Overscan.Off || entry.Source.RenderSource == RenderSettings.RenderingSources.ActiveViewport
            ? Seq<Error>()
            : Seq<Error>(new OverscanSourceRefused(index, entry.Source.RenderSource)))
        + Seq(entry.Rendered.Map(static _ => unit), entry.Frames.Numbers.TraverseM(frame => entry.Moment(frame)).As().Map(static _ => unit))
            .Partition()
            .Fails
            .Map(error => (Error)new EntryRefused(index, error));

    private static bool Held(RhinoDoc doc, Seq<string> snapshots, RenderSourceState source) =>
        source.RenderSource switch {
            RenderSettings.RenderingSources.ActiveViewport => doc.Views.ActiveView is not null,
            RenderSettings.RenderingSources.SpecificViewport => source.SpecificViewport.Exists(name => doc.Views.Find(name, compareCase: false) is not null),
            RenderSettings.RenderingSources.NamedView => source.NamedView.Exists(name => Conversions.Present(doc.NamedViews.FindByName(name)).IsSome),
            RenderSettings.RenderingSources.SnapShot => source.Snapshot
                .Bind(static name => Conversions.Validated<SnapshotName, string, InvalidRhinoValue>(name).ToOption())
                .Exists(wanted => snapshots.Exists(held => wanted == held)),
        };

    private static IO<Disk> Measured(RhinoDoc doc, QueuePlan plan, Reach reach) =>
        from saved in IO.lift(() => Conversions.Present(doc.Path))
        from output in Located(plan.Destination, saved)
            .Traverse(folder => reach.Local ? Volume.Of(folder).Map(static volume => Some(volume)) : IO.pure(Option<Volume>.None))
            .As()
        from package in reach.Package.Traverse(package => Located(package, saved).Traverse(Volume.Of).As()).As()
        select new Disk(output, package);

    private static Fin<string> Located(Destination destination, Option<string> saved) =>
        (Destinations.FolderOf(destination.Folder, saved), Destinations.StemOf(destination.Stem, saved)).Apply(static (folder, _) => folder).As();

    private static Textures Textured(RhinoDoc doc) =>
        Conversions.Rows(doc.Materials).Filter(static material => !material.IsDeleted).Strict() switch {
            var materials => new Textures(
                Paths(materials).Filter(path => Conversions.Present(Utilities.FindFile(doc, path)).IsNone),
                Paths(materials.Filter(static material => Conversions.Present(material.RenderMaterialInstanceId).IsNone))),
        };

    private static Seq<string> Paths(Seq<Material> materials) =>
        materials.Bind(static material => Conversions.Rows(material.GetTextures())).Choose(static texture => Conversions.Present(texture.FileName)).Distinct().Strict();

    // --- [JUDGE]
    private static Preflight Judged(
        Reach reach,
        QueuePlan plan,
        DocumentStatistics statistics,
        LinkClosure links,
        Option<LinkedInstanceDefinitionUpdateStyle> style,
        Seq<Error> entered,
        RenderWindow.StandardChannels channels,
        Option<RunState> run,
        Host host,
        Disk disk,
        Textures textures) =>
        new(
            (plan.Entries.IsEmpty ? Seq<Error>(new QueueEmpty()) : Seq<Error>())
            + entered
            + disk.Output.Match(Succ: static held => held.ToSeq().Bind(static volume => volume.Refusals(None)), Fail: static error => Seq(error))
            + links.Broken.Choose(static broken => Stored(broken.Link.Target).Map(file => (Error)new LinkBroken(broken.Link.Name, file, broken.Error)))
            + (reach.Local ? host.Refusals : Seq<Error>())
            + run.Bind(static state => state.Switch(
                absent: static _ => Option<Error>.None,
                active: static active => Some<Error>(new QueueActive(active.ProcessId)),
                ended: static _ => Option<Error>.None,
                crashed: static _ => Option<Error>.None)).ToSeq()
            + (reach.Quits ? host.Untitled.Map(static serial => (Error)new UntitledEdits(serial)) : Seq<Error>())
            + (reach.Package.IsSome && style.IsNone ? Seq<Error>(new LinkUpdatePrompts()) : Seq<Error>())
            + disk.Package.ToSeq().Bind(volume => volume.Match(Succ: held => held.Refusals(statistics.FileLength), Fail: static error => Seq(error))),
            links.Stale.Choose(static link => Stored(link.Target).Map(file => (Error)new LinkStale(link.Name, file)))
            + links.Cycles.Map(static files => (Error)new LinkCycle(files.Choose(Stored)))
            + links.Mismatched.Choose(static row => Stored(row.Link.Target).Map(file => (Error)new LinkUnitsDiffer(row.Link.Name, file, row.Link.Units, row.FileUnits)))
            + textures.Absent.Map(static path => (Error)new TextureMissing(path))
            + (reach.Local
                ? (host.SafeMode && host.Renderer == CyclesSetting.PlugInId ? Seq<Error>(new RenderOnCpu()) : Seq<Error>())
                : (textures.Unpacked + toSeq(links.Graph.Edges).Choose(static link => Stored(link.Target))).Distinct().Map(static path => (Error)new FileOutsideCopy(path))),
            statistics,
            toMap(plan.Entries.Map((entry, index) => entry.Rendered.ToOption().Map(rendered => (index, QueueEstimate.Memory(statistics, rendered, channels)))).Somes()));

    private static Option<string> Stored(LinkedModel model) =>
        model.Switch(opened: static _ => Option<string>.None, stored: static stored => Some(stored.Path));

    private readonly record struct Reach(bool Local, bool Quits, Option<Destination> Package);

    private sealed record Host(Guid Renderer, bool Installed, bool SafeMode, bool AutoSaved, Option<string> Pending, Seq<uint> Untitled) {
        public Seq<Error> Refusals =>
            (Installed ? Seq<Error>() : Seq<Error>(new RendererMissing(Renderer)))
            + Pending.Map(static prompt => (Error)new CommandPending(prompt)).ToSeq()
            + (AutoSaved ? Seq<Error>() : Seq<Error>(new RenderingsUnsaved()));

        public static Host Read() =>
            Utilities.DefaultRenderPlugInId switch {
                var renderer => new Host(
                    renderer,
                    PlugIn.PlugInExists(renderer, out _, out _),
                    RhinoApp.IsSafeModeEnabled,
                    SupportOptions.AutoSaveRenderings(),
                    Callbacks.Found(Command.InCommand(), RhinoApp.CommandPrompt),
                    Conversions.Rows(RhinoDoc.OpenDocuments(includeHeadless: false))
                        .Filter(static open => open.Modified && Conversions.Present(open.Path).IsNone)
                        .Map(static open => open.RuntimeSerialNumber)
                        .Strict()),
            };
    }

    private sealed record Disk(Fin<Option<Volume>> Output, Option<Fin<Volume>> Package);

    private readonly record struct Volume(string Folder, bool Writable, Option<long> Free) {
        private const long Floor = 256L << 20;

        public Seq<Error> Refusals(Option<long> copy) =>
            (Writable ? Seq<Error>() : Seq<Error>(new FolderUnwritable(Folder)))
            + Free.Filter(static free => free < Floor).Map(free => (Error)new SpaceLow(Folder, free)).ToSeq()
            + (from free in Free from size in copy where size > free select (Error)new CopyTooLarge(Folder, size, free)).ToSeq();

        public static IO<Volume> Of(string folder) =>
            IO.lift(() => Nearest(new DirectoryInfo(folder))).Bind(found => found.Match(
                Some: existing => Freed(existing).Map(free => new Volume(folder, !existing.Attributes.HasFlag(FileAttributes.ReadOnly), free)),
                None: () => IO.pure(new Volume(folder, Writable: false, Free: None))));

        private static Option<DirectoryInfo> Nearest(DirectoryInfo folder) =>
            folder.Exists ? Some(folder) : Optional(folder.Parent).Bind(Nearest);

        private static IO<Option<long>> Freed(DirectoryInfo existing) =>
            IO.lift(() => Some(new DriveInfo(existing.FullName).AvailableFreeSpace))
                .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.pure(Option<long>.None));
    }

    private sealed record Textures(Seq<string> Absent, Seq<string> Unpacked);
}
