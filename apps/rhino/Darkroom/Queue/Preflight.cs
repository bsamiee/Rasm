using System.Globalization;
using Rasm.Rhino;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Render.Settings;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record PreflightFinding {
    public abstract Error Error { get; }

    public sealed record QueueEmpty : PreflightFinding {
        public override Error Error => Error.New("render queue holds no entry");
    }

    public sealed record SourceViewMissing(int Index, RenderSettings.RenderingSources Source) : PreflightFinding {
        public override Error Error => Error.New(string.Create(CultureInfo.InvariantCulture, $"entry {Index} source {Source} names no view in the document"));
    }

    public sealed record OverscanSourceRefused(int Index, RenderSettings.RenderingSources Source) : PreflightFinding {
        public override Error Error => Error.New(string.Create(CultureInfo.InvariantCulture, $"entry {Index} overscan needs the active viewport, its source is {Source}"));
    }

    public sealed record EntryRefused(int Index, Error Cause) : PreflightFinding {
        public override Error Error => Error.New(string.Create(CultureInfo.InvariantCulture, $"entry {Index} cannot render"), Cause);
    }

    public sealed record DestinationUnresolved(Error Cause) : PreflightFinding {
        public override Error Error => Cause;
    }

    public sealed record FolderUnwritable(string Folder) : PreflightFinding {
        public override Error Error => new FileRefused(Folder, FileReason.Unwritable);
    }

    public sealed record SpaceLow(string Folder, long Free) : PreflightFinding {
        public override Error Error => Error.New(string.Create(CultureInfo.InvariantCulture, $"{Folder} has only {Free} bytes free"));
    }

    public sealed record CopyTooLarge(string Folder, long Copy, long Free) : PreflightFinding {
        public override Error Error => Error.New(string.Create(CultureInfo.InvariantCulture, $"a copy of {Copy} bytes exceeds the {Free} bytes free at {Folder}"));
    }

    public sealed record LinkBroken(string Definition, string File, Error Cause) : PreflightFinding {
        public override Error Error => Error.New($"{Definition} links unreadable {File}", Cause);
    }

    public sealed record RendererMissing(Guid Renderer) : PreflightFinding {
        public override Error Error => Error.New($"current renderer {Renderer} is not installed");
    }

    public sealed record CommandPending(string Prompt) : PreflightFinding {
        public override Error Error => Error.New($"a command waits at prompt {Prompt}");
    }

    public sealed record RenderingsUnsaved : PreflightFinding {
        public override Error Error => Error.New("automatic saving of finished renderings is off");
    }

    public sealed record QueueActive(int ProcessId) : PreflightFinding {
        public override Error Error => Error.New(string.Create(CultureInfo.InvariantCulture, $"render queue runs in process {ProcessId}"));
    }

    public sealed record UntitledEdits(uint Document) : PreflightFinding {
        public override Error Error => Error.New($"untitled document {Document} has unsaved edits");
    }

    public sealed record LinkUpdatePrompts : PreflightFinding {
        public override Error Error => Error.New("prompting linked block updates stop unattended renders");
    }

    public sealed record LinkStale(string Definition, string File) : PreflightFinding {
        public override Error Error => Error.New($"{Definition} is out of date with {File}");
    }

    public sealed record LinkCycle(Seq<string> Files) : PreflightFinding {
        public override Error Error => Error.New($"linked files {string.Join(", ", Files)} link each other");
    }

    public sealed record LinkUnitsDiffer(string Definition, string File, UnitSystem Units, UnitSystem FileUnits) : PreflightFinding {
        public override Error Error => Error.New($"{Definition} in {Units} links {File} in {FileUnits}");
    }

    public sealed record TextureMissing(string Path) : PreflightFinding {
        public override Error Error => Error.New($"texture {Path} is missing");
    }

    public sealed record RenderOnCpu : PreflightFinding {
        public override Error Error => Error.New("Rhino Render runs on CPU in safe mode");
    }

    public sealed record FileOutsideCopy(string Path) : PreflightFinding {
        public override Error Error => Error.New($"the document copy omits {Path}");
    }
}

public sealed record Preflight(Seq<PreflightFinding> Refusals, Seq<PreflightFinding> Cautions, DocumentStatistics Statistics, Map<int, ulong> Memory) {
    // --- [VERDICT]
    public Fin<Unit> Verdict => Refusals.Traverse(static refusal => Fin.Fail<Unit>(refusal.Error)).As().Map(static _ => unit);

    // --- [CHECK]
    public static IO<Preflight> Check(QueueContext context, RhinoDoc doc, QueuePlan plan, QueueTarget target) =>
        target.Switch(
            local: static _ => new Reach(Local: true, Quits: false, Package: None),
            packaged: static packaged => new Reach(Local: packaged.Use.Quits, Quits: packaged.Use.Quits, Package: Some((packaged.Folder, packaged.Stem))),
            flamenco: static farm => new Reach(Local: false, Quits: false, Package: Some((farm.Folder, farm.Stem)))) switch {
                var reach =>
                    from statistics in DocumentStatistics.Read(doc)
                    from links in Links.Closure(new LinkedModel.Opened(doc.RuntimeSerialNumber))
                    from style in Links.Style(doc).Read
                    from named in NamedEntry.Read(doc)
                    from channels in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), static scope => DisposalOps.Within(() => scope.Settings.RenderChannels, static held => IO.lift(() => QueueEstimate.Channels(held))))
                    from run in reach.Local ? QueueProgress.Observed(context).Map(static observed => Some(observed.State)) : IO.pure(Option<RunState>.None)
                    from host in IO.lift(Host.Read)
                    from disk in Measured(doc, plan, reach)
                    from textures in IO.lift(() => Textured(doc))
                    select Judged(reach, plan, doc, named, statistics, links, style, channels, run, host, disk, textures),
            };

    // --- [READS]
    private static Seq<PreflightFinding> Entered(RhinoDoc doc, Seq<NamedEntry> named, QueueEntry<Unit> entry, int index) =>
        (Held(doc, named, entry.Queued) ? Seq<PreflightFinding>() : Seq<PreflightFinding>(new PreflightFinding.SourceViewMissing(index, entry.Queued.Source)))
        + (entry.Queued.Overscan is 0d || entry.Queued.Source is RenderSettings.RenderingSources.ActiveViewport
            ? Seq<PreflightFinding>()
            : Seq<PreflightFinding>(new PreflightFinding.OverscanSourceRefused(index, entry.Queued.Source)))
        + entry.Rendered.Match(Succ: static _ => Seq<PreflightFinding>(), Fail: error => Seq<PreflightFinding>(new PreflightFinding.EntryRefused(index, error)));

    private static bool Held(RhinoDoc doc, Seq<NamedEntry> named, QueuedEntry entry) =>
        entry.Source switch {
            RenderSettings.RenderingSources.ActiveViewport => doc.Views.ActiveView is not null,
            RenderSettings.RenderingSources.SpecificViewport => Optional(entry.View).Exists(name => doc.Views.Find(name, compareCase: false) is not null),
            RenderSettings.RenderingSources.NamedView => Optional(entry.View).Exists(name => NamedTable.Views.Find(named, name).IsSome),
            RenderSettings.RenderingSources.SnapShot => Optional(entry.View).Exists(name => NamedTable.Snapshots.Find(named, name).IsSome),
        };

    private static IO<Disk> Measured(RhinoDoc doc, QueuePlan plan, Reach reach) =>
        from saved in IO.lift(() => Conversions.Present(doc.Path))
        from output in Destinations.Located(plan.Book.Folder, plan.Book.Stem, saved)
            .Traverse(located => reach.Local ? Volume.Of(located.Folder).Map(static volume => Some(volume)) : IO.pure(Option<Volume>.None))
            .As()
        from package in reach.Package.Traverse(package => Destinations.Located(package.Folder, package.Stem, saved).Traverse(static located => Volume.Of(located.Folder)).As()).As()
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
        Reach reach, QueuePlan plan, RhinoDoc doc, Seq<NamedEntry> named, DocumentStatistics statistics, LinkClosure links, Option<LinkedInstanceDefinitionUpdateStyle> style,
        RenderWindow.StandardChannels channels, Option<RunState> run, Host host, Disk disk, Textures textures) =>
        new(
            (plan.Entries.IsEmpty ? Seq<PreflightFinding>(new PreflightFinding.QueueEmpty()) : Seq<PreflightFinding>())
            + plan.Entries.Map((entry, index) => Entered(doc, named, entry, index)).Flatten()
            + disk.Output.Match(
                Succ: static held => held.ToSeq().Bind(static volume => volume.Refusals(None)),
                Fail: static error => Seq<PreflightFinding>(new PreflightFinding.DestinationUnresolved(error)))
            + links.Broken.Choose(static broken => Stored(broken.Link.Target).Map<PreflightFinding>(file => new PreflightFinding.LinkBroken(broken.Link.Name, file, broken.Error)))
            + (reach.Local ? host.Refusals : Seq<PreflightFinding>())
            + run.Bind(static state => state is RunState.Active active ? Some<PreflightFinding>(new PreflightFinding.QueueActive(active.ProcessId)) : None).ToSeq()
            + (reach.Quits ? host.Untitled.Map<PreflightFinding>(static serial => new PreflightFinding.UntitledEdits(serial)) : Seq<PreflightFinding>())
            + (reach.Package.IsSome && style.IsNone ? Seq<PreflightFinding>(new PreflightFinding.LinkUpdatePrompts()) : Seq<PreflightFinding>())
            + disk.Package.ToSeq().Bind(volume => volume.Match(
                Succ: held => held.Refusals(statistics.FileLength),
                Fail: static error => Seq<PreflightFinding>(new PreflightFinding.DestinationUnresolved(error)))),
            links.Stale.Choose(static link => Stored(link.Target).Map<PreflightFinding>(file => new PreflightFinding.LinkStale(link.Name, file)))
            + links.Cycles.Map<PreflightFinding>(static files => new PreflightFinding.LinkCycle(files.Choose(Stored)))
            + links.Mismatched.Choose(static row => Stored(row.Link.Target).Map<PreflightFinding>(file => new PreflightFinding.LinkUnitsDiffer(row.Link.Name, file, row.Link.Units, row.FileUnits)))
            + textures.Absent.Map<PreflightFinding>(static path => new PreflightFinding.TextureMissing(path))
            + (reach.Local
                ? (host.SafeMode && host.Renderer == CyclesSetting.PlugInId ? Seq<PreflightFinding>(new PreflightFinding.RenderOnCpu()) : Seq<PreflightFinding>())
                : (textures.Unpacked + toSeq(links.Graph.Edges).Choose(static link => Stored(link.Target))).Distinct().Map<PreflightFinding>(static path => new PreflightFinding.FileOutsideCopy(path))),
            statistics,
            toMap(plan.Entries.Map((entry, index) => entry.Rendered.ToOption().Map(frame => (index, QueueEstimate.Memory(statistics, frame.Padded, channels)))).Somes()));

    private static Option<string> Stored(LinkedModel model) => model.Switch(opened: static _ => Option<string>.None, stored: static stored => Some(stored.Path));

    private sealed record Reach(bool Local, bool Quits, Option<(OutputFolder Folder, Stem Stem)> Package);

    private sealed record Host(Guid Renderer, bool Installed, bool SafeMode, bool AutoSaved, Option<string> Pending, Seq<uint> Untitled) {
        public Seq<PreflightFinding> Refusals =>
            (Installed ? Seq<PreflightFinding>() : Seq<PreflightFinding>(new PreflightFinding.RendererMissing(Renderer)))
            + Pending.Map<PreflightFinding>(static prompt => new PreflightFinding.CommandPending(prompt)).ToSeq()
            + (AutoSaved ? Seq<PreflightFinding>() : Seq<PreflightFinding>(new PreflightFinding.RenderingsUnsaved()));

        public static Host Read() =>
            Utilities.DefaultRenderPlugInId switch {
                var renderer => new Host(
                    renderer,
                    PlugIn.PlugInExists(renderer, out _, out _),
                    RhinoApp.IsSafeModeEnabled,
                    SupportOptions.AutoSaveRenderings(),
                    Callbacks.Found(Command.InCommand(), RhinoApp.CommandPrompt),
                    Conversions.Rows(RhinoDoc.OpenDocuments()).Filter(static open => open.Modified && Conversions.Present(open.Path).IsNone).Map(static open => open.RuntimeSerialNumber).Strict()),
            };
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
