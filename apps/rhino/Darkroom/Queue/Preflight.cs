using System.Globalization;
using Darkroom.Chain;
using Rasm.Rhino;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record PreflightFinding {
    private PreflightFinding(Error error) => Error = error;

    public Error Error { get; }

    public sealed record QueueEmpty() : PreflightFinding(new DarkroomFault("render queue", DarkroomReason.Empty));

    public sealed record SourceRefused(int Index, RenderSettings.RenderingSources Source, DarkroomReason Reason) : PreflightFinding(new DarkroomFault($"entry source {Source}", Reason, Index));

    public sealed record EntryRefused(int Index, Error Cause) : PreflightFinding(new DarkroomFault("entry", DarkroomReason.Unrenderable, Index, Cause));

    public sealed record DestinationUnresolved(Error Cause) : PreflightFinding(Cause);

    public sealed record FolderUnwritable(string Folder) : PreflightFinding(new RhinoFault(Folder, RhinoReason.Unwritable));

    public sealed record SpaceLow(string Folder, long Free)
        : PreflightFinding(new DarkroomFault(string.Create(CultureInfo.InvariantCulture, $"render at {Folder} with {Free} bytes free"), DarkroomReason.Space));

    public sealed record CopyTooLarge(string Folder, long Copy, long Free)
        : PreflightFinding(new DarkroomFault(string.Create(CultureInfo.InvariantCulture, $"copy of {Copy} bytes at {Folder} with {Free} bytes free"), DarkroomReason.Space));

    public sealed record LinkBroken(string Definition, string File, Error Cause) : PreflightFinding(new DarkroomFault($"{Definition} link {File}", DarkroomReason.Unreadable, Inner: Cause));

    public sealed record RendererMissing(Guid Renderer) : PreflightFinding(new DarkroomFault($"current renderer {Renderer}", DarkroomReason.Missing));

    public sealed record RenderingsUnsaved() : PreflightFinding(new DarkroomFault("automatic saving of finished renderings", DarkroomReason.Off));

    public sealed record QueueActive(int ProcessId)
        : PreflightFinding(new DarkroomFault(string.Create(CultureInfo.InvariantCulture, $"render queue in process {ProcessId}"), DarkroomReason.Running));

    public sealed record UntitledEdits(uint Document) : PreflightFinding(new DarkroomFault($"untitled document {Document}", DarkroomReason.Edited));

    public sealed record LinkUpdatePrompts() : PreflightFinding(new DarkroomFault("prompting linked block updates", DarkroomReason.Unattended));

    public sealed record LinkStale(string Definition, string File) : PreflightFinding(new DarkroomFault($"{Definition} link {File}", DarkroomReason.Stale));

    public sealed record LinkCycle(Seq<string> Files) : PreflightFinding(new DarkroomFault($"linked files {string.Join(", ", Files)}", DarkroomReason.Cyclic));

    public sealed record LinkUnitsDiffer(string Definition, string File, UnitSystem Units, UnitSystem FileUnits)
        : PreflightFinding(new DarkroomFault($"{Definition} in {Units} linking {File} in {FileUnits}", DarkroomReason.Units));

    public sealed record FileRefused(string Subject, DarkroomReason Reason) : PreflightFinding(new DarkroomFault(Subject, Reason));

    public sealed record RenderOnCpu() : PreflightFinding(new DarkroomFault("Rhino Render in safe mode", DarkroomReason.Cpu));
}

public sealed record Preflight(Seq<PreflightFinding> Refusals, Seq<PreflightFinding> Cautions, Map<int, ulong> Memory) {
    // --- [VERDICT]
    public Fin<Unit> Verdict => Refusals.Traverse(static refusal => Fin.Fail<Unit>(refusal.Error)).As().Map(static _ => unit);

    // --- [CHECK]
    public static IO<Preflight> Check(QueueContext context, RhinoDoc doc, QueuePlan plan, QueueTarget target) =>
        from statistics in DocumentStatistics.Read(doc)
        from links in Links.Closure(new LinkedModel.Opened(doc.RuntimeSerialNumber))
        from prompts in IO.lift(() => doc.LinkedInstanceDefinitionUpdate is LinkedInstanceDefinitionUpdateStyle.Prompt)
        from named in NamedTable.Read(doc, [NamedViews.Table, Snapshots.Table])
        from channels in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), static scope => (from held in use(() => scope.Settings.RenderChannels) select QueueEstimate.Channels(held)).Bracket())
        from run in target.RendersHere ? QueueProgress.Observed(context).Map(static observed => Some(observed.State)) : IO.pure(Option<RunState>.None)
        from host in Host.Read
        from disk in Measured(doc, plan, target)
        from textures in IO.lift(() => Textured(doc))
        select Judged(target, plan, doc, named, statistics, links, prompts, channels, run, host, disk, textures);

    // --- [READS]
    private static Seq<PreflightFinding> Entered(RhinoDoc doc, Seq<NamedEntry> named, QueueEntry<Unit> entry, int index) =>
        (Held(doc, named, entry.Queued) ? Seq<PreflightFinding>() : Seq<PreflightFinding>(new PreflightFinding.SourceRefused(index, entry.Queued.Source, DarkroomReason.Unviewed)))
        + (entry.Queued.Overscan is 0d || entry.Queued.Source is RenderSettings.RenderingSources.ActiveViewport
            ? Seq<PreflightFinding>()
            : Seq<PreflightFinding>(new PreflightFinding.SourceRefused(index, entry.Queued.Source, DarkroomReason.Overscan)))
        + entry.Rendered.Match(Succ: static _ => Seq<PreflightFinding>(), Fail: error => Seq<PreflightFinding>(new PreflightFinding.EntryRefused(index, error)));

    private static bool Held(RhinoDoc doc, Seq<NamedEntry> named, QueuedEntry entry) =>
        entry.Source switch {
            RenderSettings.RenderingSources.ActiveViewport => doc.Views.ActiveView is not null,
            RenderSettings.RenderingSources.SpecificViewport => entry.View.Exists(name => doc.Views.Find(name, compareCase: false) is not null),
            RenderSettings.RenderingSources.NamedView => entry.View.Exists(name => NamedViews.Table.Find(named, name).IsSome),
            RenderSettings.RenderingSources.SnapShot => entry.View.Exists(name => Snapshots.Table.Find(named, name).IsSome),
        };

    private static IO<Disk> Measured(RhinoDoc doc, QueuePlan plan, QueueTarget target) =>
        from saved in IO.lift(() => Conversions.Present(doc.Path))
        from output in plan.Book.Folder.Of(saved)
            .Traverse(folder => target.RendersHere ? Volume.Of(folder).Map(static volume => Some(volume)) : IO.pure(Option<Volume>.None))
            .As()
        from package in target.Package.Traverse(package => package.Folder.Of(saved).Traverse(Volume.Of).As()).As()
        select new Disk(output, package);

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
        QueueTarget target, QueuePlan plan, RhinoDoc doc, Seq<NamedEntry> named, DocumentStatistics statistics, LinkClosure links, bool prompts,
        RenderWindow.StandardChannels channels, Option<RunState> run, Host host, Disk disk, Textures textures) =>
        new(
            (plan.Entries.IsEmpty ? Seq<PreflightFinding>(new PreflightFinding.QueueEmpty()) : Seq<PreflightFinding>())
            + plan.Entries.Map((entry, index) => Entered(doc, named, entry, index)).Flatten()
            + disk.Output.Match(
                Succ: static held => held.ToSeq().Bind(static volume => volume.Refusals(None)),
                Fail: static error => Seq<PreflightFinding>(new PreflightFinding.DestinationUnresolved(error)))
            + links.Broken.Choose(static broken => Stored(broken.Link.Target).Map<PreflightFinding>(file => new PreflightFinding.LinkBroken(broken.Link.Name, file, broken.Error)))
            + (target.RendersHere ? host.Refusals : Seq<PreflightFinding>())
            + run.Bind(static state => state is RunState.Active active ? Some<PreflightFinding>(new PreflightFinding.QueueActive(active.ProcessId)) : None).ToSeq()
            + (target.Quits ? host.Untitled.Map<PreflightFinding>(static serial => new PreflightFinding.UntitledEdits(serial)) : Seq<PreflightFinding>())
            + (target.Package.IsSome && prompts ? Seq<PreflightFinding>(new PreflightFinding.LinkUpdatePrompts()) : Seq<PreflightFinding>())
            + disk.Package.ToSeq().Bind(volume => volume.Match(
                Succ: held => held.Refusals(statistics.FileLength),
                Fail: static error => Seq<PreflightFinding>(new PreflightFinding.DestinationUnresolved(error)))),
            links.Stale.Choose(static link => Stored(link.Target).Map<PreflightFinding>(file => new PreflightFinding.LinkStale(link.Name, file)))
            + links.Graph.Condensed.Cycles.Map<PreflightFinding>(static files => new PreflightFinding.LinkCycle(files.Choose(Stored)))
            + links.Mismatched.Choose(static row => Stored(row.Link.Target).Map<PreflightFinding>(file => new PreflightFinding.LinkUnitsDiffer(row.Link.Name, file, row.Link.Units, row.FileUnits)))
            + textures.Absent.Map<PreflightFinding>(static path => new PreflightFinding.FileRefused($"texture {path}", DarkroomReason.Missing))
            + (target.RendersHere
                ? (host.SafeMode && host.Renderer == host.RhinoRender ? Seq<PreflightFinding>(new PreflightFinding.RenderOnCpu()) : Seq<PreflightFinding>())
                : (textures.Unpacked + toSeq(links.Graph.Edges).Choose(static link => Stored(link.Target))).Distinct().Map<PreflightFinding>(static path => new PreflightFinding.FileRefused(path, DarkroomReason.Uncopied))),
            toMap(plan.Entries.Map((entry, index) => entry.Rendered.ToOption().Map(frame => (index, QueueEstimate.Memory(statistics, frame.Padded, channels)))).Somes()));

    private static Option<string> Stored(LinkedModel model) => model.Switch(opened: static _ => Option<string>.None, stored: static stored => Some(stored.Path));

    private sealed record Host(Guid Renderer, Guid RhinoRender, bool Installed, bool SafeMode, bool AutoSaved, Seq<uint> Untitled) {
        public Seq<PreflightFinding> Refusals =>
            (Installed ? Seq<PreflightFinding>() : Seq<PreflightFinding>(new PreflightFinding.RendererMissing(Renderer)))
            + (AutoSaved ? Seq<PreflightFinding>() : Seq<PreflightFinding>(new PreflightFinding.RenderingsUnsaved()));

        public static IO<Host> Read =>
            Cycles.PlugInId.Bind(static rhinoRender => IO.lift(() => Utilities.DefaultRenderPlugInId switch {
                var renderer => new Host(
                    renderer,
                    rhinoRender,
                    PlugIn.PlugInExists(renderer, out _, out _),
                    RhinoApp.IsSafeModeEnabled,
                    SupportOptions.AutoSaveRenderings(),
                    Conversions.Rows(RhinoDoc.OpenDocuments()).Filter(static open => open.Modified && Conversions.Present(open.Path).IsNone).Map(static open => open.RuntimeSerialNumber).Strict()),
            }));
    }

    private sealed record Disk(Fin<Option<Volume>> Output, Option<Fin<Volume>> Package);

    private sealed record Volume(string Folder, bool Writable, Option<long> Free) {
        private const long Floor = 256L << 20;

        public Seq<PreflightFinding> Refusals(Option<long> copy) =>
            (Writable ? Seq<PreflightFinding>() : Seq<PreflightFinding>(new PreflightFinding.FolderUnwritable(Folder)))
            + Free.Filter(static free => free < Floor).Map<PreflightFinding>(free => new PreflightFinding.SpaceLow(Folder, free)).ToSeq()
            + (from free in Free from size in copy where size > free select (PreflightFinding)new PreflightFinding.CopyTooLarge(Folder, size, free)).ToSeq();

        public static IO<Volume> Of(string folder) =>
            IO.lift(() => Nearest(new DirectoryInfo(folder))).Bind(found => found.Match(
                Some: existing => IO.lift(() => Some(new DriveInfo(existing.FullName).AvailableFreeSpace))
                    .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.pure(Option<long>.None))
                    .Map(free => new Volume(folder, !existing.Attributes.HasFlag(FileAttributes.ReadOnly), free)),
                None: () => IO.pure(new Volume(folder, Writable: false, Free: None))));

        private static Option<DirectoryInfo> Nearest(DirectoryInfo folder) => folder.Exists ? Some(folder) : Optional(folder.Parent).Bind(Nearest);
    }

    private sealed record Textures(Seq<string> Absent, Seq<string> Unpacked);
}
