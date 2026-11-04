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
    public Error Error => Switch(
        queueEmpty: static _ => new DarkroomFault("render queue", DarkroomReason.Empty),
        sourceRefused: static held => new DarkroomFault((FormattableString)$"entry source {held.Source}", held.Reason, held.Index),
        entryRefused: static held => new DarkroomFault("entry", DarkroomReason.Unrenderable, held.Index, held.Cause),
        destinationUnresolved: static held => held.Cause,
        folderUnwritable: static held => new RhinoFault(held.Folder, RhinoReason.Unwritable),
        spaceLow: static held => new DarkroomFault((FormattableString)$"render at {held.Folder} with {held.Free} bytes free", DarkroomReason.Space),
        copyTooLarge: static held => new DarkroomFault((FormattableString)$"copy of {held.Copy} bytes at {held.Folder} with {held.Free} bytes free", DarkroomReason.Space),
        linkBroken: static held => new DarkroomFault((FormattableString)$"{held.Definition} link {held.File}", DarkroomReason.Unreadable, Inner: held.Cause),
        rendererMissing: static held => new DarkroomFault((FormattableString)$"current renderer {held.Renderer}", DarkroomReason.Missing),
        renderingsUnsaved: static _ => new DarkroomFault("automatic saving of finished renderings", DarkroomReason.Off),
        queueActive: static held => new DarkroomFault((FormattableString)$"render queue in process {held.ProcessId}", DarkroomReason.Running),
        untitledEdits: static held => new DarkroomFault((FormattableString)$"untitled document {held.Document}", DarkroomReason.Edited),
        linkUpdatePrompts: static _ => new DarkroomFault("prompting linked block updates", DarkroomReason.Unattended),
        linkStale: static held => new DarkroomFault((FormattableString)$"{held.Definition} link {held.File}", DarkroomReason.Stale),
        linkCycle: static held => new DarkroomFault((FormattableString)$"linked files {string.Join(", ", held.Files)}", DarkroomReason.Cyclic),
        linkUnitsDiffer: static held => new DarkroomFault((FormattableString)$"{held.Definition} in {held.Units} linking {held.File} in {held.FileUnits}", DarkroomReason.Units),
        fileRefused: static held => new DarkroomFault(held.Subject, held.Reason),
        renderOnCpu: static _ => new DarkroomFault("Rhino Render in safe mode", DarkroomReason.Cpu));

    public sealed record QueueEmpty() : PreflightFinding;

    public sealed record SourceRefused(int Index, RenderSettings.RenderingSources Source, DarkroomReason Reason) : PreflightFinding;

    public sealed record EntryRefused(int Index, Error Cause) : PreflightFinding;

    public sealed record DestinationUnresolved(Error Cause) : PreflightFinding;

    public sealed record FolderUnwritable(string Folder) : PreflightFinding;

    public sealed record SpaceLow(string Folder, long Free) : PreflightFinding;

    public sealed record CopyTooLarge(string Folder, long Copy, long Free) : PreflightFinding;

    public sealed record LinkBroken(string Definition, string File, Error Cause) : PreflightFinding;

    public sealed record RendererMissing(Guid Renderer) : PreflightFinding;

    public sealed record RenderingsUnsaved() : PreflightFinding;

    public sealed record QueueActive(int ProcessId) : PreflightFinding;

    public sealed record UntitledEdits(uint Document) : PreflightFinding;

    public sealed record LinkUpdatePrompts() : PreflightFinding;

    public sealed record LinkStale(string Definition, string File) : PreflightFinding;

    public sealed record LinkCycle(Seq<string> Files) : PreflightFinding;

    public sealed record LinkUnitsDiffer(string Definition, string File, UnitSystem Units, UnitSystem FileUnits) : PreflightFinding;

    public sealed record FileRefused(string Subject, DarkroomReason Reason) : PreflightFinding;

    public sealed record RenderOnCpu() : PreflightFinding;
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
        from channels in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), static scope => (from held in use(() => scope.Settings.RenderChannels) select QueueEstimate.Components(held)).Bracket())
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
        Option<Seq<RenderWindow.StandardChannels>> channels, Option<RunState> run, Host host, Disk disk, Textures textures) =>
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
            toMap(plan.Entries.Map((entry, index) => from frame in entry.Rendered.ToOption() from components in channels select (index, QueueEstimate.Memory(statistics, frame.Padded, components))).Somes()));

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
            IO.lift(() => toSeq(LanguageExt.List.unfold(Some(new DirectoryInfo(folder)), static current =>
                current.Map(static held => (held, Optional(held.Parent))))).Find(static parent => parent.Exists)).Bind(found => found.Match(
                Some: existing => IO.lift(() => Some(new DriveInfo(existing.FullName).AvailableFreeSpace))
                    .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.pure(Option<long>.None))
                    .Map(free => new Volume(folder, !existing.Attributes.HasFlag(FileAttributes.ReadOnly), free)),
                None: () => IO.pure(new Volume(folder, Writable: false, Free: None))));
    }

    private sealed record Textures(Seq<string> Absent, Seq<string> Unpacked);
}
