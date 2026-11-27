using Darkroom.Chain;
using Rasm.Rhino;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;

namespace Darkroom.Queue;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record Preflight(Seq<Error> Refusals, Seq<Error> Cautions, Map<int, ulong> Memory) {

    // --- [CHECKS]

    public static IO<Preflight> Check(QueueContext context, RhinoDoc doc, QueuePlan plan, QueueTarget target) =>
        from statistics in DocumentStatistics.Read(doc)
        from links in Links.Closure(new LinkedModel.Opened(doc.RuntimeSerialNumber))
        let prompts = doc.LinkedInstanceDefinitionUpdate is LinkedInstanceDefinitionUpdateStyle.Prompt
        from named in NamedTable.Read(doc, [NamedViews.Table, Snapshots.Table])
        from channels in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), static scope =>
            (from held in use(() => scope.Settings.RenderChannels)
             select (held.Mode is RenderChannels.Modes.Custom ? toSeq(held.CustomList) : Seq<Guid>())
                 .Traverse(static K<Option, RenderWindow.StandardChannels> (id) => RenderWindow.StandardChannelForGuid(id) switch {
                     RenderWindow.StandardChannels.None => Option<RenderWindow.StandardChannels>.None,
                     var channel => Some(channel),
                 }).As()
                 .Map(static custom => RenderWindow.StandardChannels.RGBA.Cons(custom).Bind(Channels.Components).Distinct().Strict())).Bracket())
        from run in target.RendersHere ? QueueProgress.Observed(context).Map(static observed => Some(observed.State)) : IO.pure(Option<RunState>.None)
        from host in Host.Read
        from disk in Measured(doc, plan, target)
        from textures in IO.lift(() => Textured(doc))
        select Judged(target, plan, doc, named, statistics, links, prompts, channels, run, host, disk, textures);

    // --- [READS]

    private static Seq<Error> Entered(RhinoDoc doc, Seq<NamedEntry> named, QueueEntry<Unit> entry, int index) =>
        (Held(doc, named, entry.Queued) ? [] : Seq<Error>(Fault.Of(DarkroomReason.Unviewed, entry.Queued.Source, index)))
        + (entry.Queued.Overscan is 0d || entry.Queued.Source is RenderSettings.RenderingSources.ActiveViewport
            ? []
            : Seq<Error>(Fault.Of(DarkroomReason.Overscan, entry.Queued.Source, index)))
        + entry.Rendered.Match(Succ: static _ => Seq<Error>(), Fail: error => Seq<Error>(Fault.Of(DarkroomReason.Unrenderable, "entry", index, error)));

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

    // --- [JUDGMENT]

    private static Preflight Judged(
        QueueTarget target, QueuePlan plan, RhinoDoc doc, Seq<NamedEntry> named, DocumentStatistics statistics, LinkClosure links, bool prompts,
        Option<Seq<RenderWindow.StandardChannels>> channels, Option<RunState> run, Host host, Disk disk, Textures textures) =>
        new(
            (plan.Entries.IsEmpty ? Seq<Error>(Fault.Of(DarkroomReason.Empty, "render queue")) : [])
            + plan.Entries.Map((entry, index) => Entered(doc, named, entry, index)).Flatten()
            + disk.Output.Match(Succ: static held => held.ToSeq().Bind(static volume => volume.Refusals(None)), Fail: static error => Seq(error))
            + links.Broken.Choose(static broken => Stored(broken.Link.Target).Map<Error>(file => Fault.Of(DarkroomReason.Unreadable, (broken.Link.Name, file), inner: broken.Error)))
            + (target.RendersHere
                ? (host.Installed ? [] : Seq<Error>(Fault.Of(DarkroomReason.Missing, host.Renderer)))
                    + (host.AutoSaved ? [] : Seq<Error>(Fault.Of(DarkroomReason.Off, "automatic saving of finished renderings")))
                : [])
            + run.Bind(static state => state.Free.Match(Succ: static _ => Option<Error>.None, Fail: Some)).ToSeq()
            + (target.Quits ? host.Untitled.Map<Error>(static serial => Fault.Of(DarkroomReason.Edited, serial)) : [])
            + (target.Package.IsSome && prompts ? Seq<Error>(Fault.Of(DarkroomReason.Unattended, "prompting linked block updates")) : [])
            + disk.Package.ToSeq().Bind(volume => volume.Match(Succ: held => held.Refusals(statistics.FileLength), Fail: static error => Seq(error))),
            links.Stale.Choose(static link => Stored(link.Target).Map<Error>(file => Fault.Of(DarkroomReason.Stale, (link.Name, file))))
            + links.Graph.Condensed.Cycles.Map<Error>(static files => Fault.Of(DarkroomReason.Cyclic, files.Choose(Stored)))
            + links.Mismatched.Choose(static row => Stored(row.Link.Target).Map<Error>(file =>
                Fault.Of(DarkroomReason.Units, (row.Link.Name, row.Link.Units, file, row.FileUnits))))
            + textures.Absent.Map<Error>(static path => Fault.Of(DarkroomReason.Missing, $"texture {path}"))
            + (target.RendersHere
                ? (host.SafeMode && host.Renderer == host.RhinoRender ? Seq<Error>(Fault.Of(DarkroomReason.Cpu, "Rhino Render in safe mode")) : [])
                : (textures.Unpacked + toSeq(links.Graph.Edges).Choose(static link => Stored(link.Target))).Distinct().Map<Error>(static path => Fault.Of(DarkroomReason.Uncopied, path))),
            toMap(plan.Entries.Map((entry, index) => from frame in entry.Rendered.ToOption() from components in channels select (index, QueueEstimate.Memory(statistics, frame.Padded, components))).Somes()));

    private static Option<string> Stored(LinkedModel model) => model.Switch(opened: static _ => Option<string>.None, stored: static stored => Some(stored.Path));

    private sealed record Host(Guid Renderer, Guid RhinoRender, bool Installed, bool SafeMode, bool AutoSaved, Seq<uint> Untitled) {
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

        public Seq<Error> Refusals(Option<long> copy) =>
            (Writable ? [] : Seq<Error>(Fault.Of(RhinoReason.Unwritable, Folder)))
            + Free.Filter(static free => free < Floor).Map<Error>(free => Fault.Of(DarkroomReason.Reserve, (Folder, free, Floor))).ToSeq()
            + (from free in Free from size in copy where size > free select (Error)Fault.Of(DarkroomReason.Space, (Folder, free, size))).ToSeq();

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
