using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Darkroom.Chain;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.FileIO;
using Rhino.Render;
using Rhino.Runtime;

namespace Darkroom.Review;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderingState(Guid Session, string Engine, Guid EngineId, Duration Elapsed, Option<RenderSettings> Settings, Option<string> Note);

public sealed record Rendering(Rimage Image, PixelExtent Extent, Instant Saved, Option<RenderingState> State) {
    private static readonly IProgress<int> Unreported = new Progress<int>();

    public static ChainState Ungraded { get; } = new([], ValueSet.Empty);

    public IO<ChainState> Chain =>
        IO.lift(Image.Running.Exists(static id => id == typeof(DarkroomEffect).GUID) ? EffectList.State(Image, typeof(DarkroomEffect).GUID, ChainState.Default) : Ungraded);

    public IO<PixelFrame> Frame => Plane(RenderWindow.StandardChannels.RGBA);

    public IO<PixelFrame> Plane(RenderWindow.StandardChannels channel) =>
        Image.Channel(channel).Map(pixels => new PixelFrame(System.Drawing.Point.Empty, Extent, Extent, block => pixels.AsSpan().CopyTo(block)));

    public IO<PixelFrame> Displayed(ChainState state, PixelFrame input, Derivations derivations) =>
        from needs in state.Needs
        from guides in ChainScene.Unframed.Guides(needs, channel => Plane(channel.Key).Map(plane => plane.Size == input.Size ? plane : plane.Downscaled(input.Size)))
        from formed in new ChainRequest(input, state).Formed(ChainScene.Unframed.Context(input.Extent, guides, derivations, rendering: false, None))
        from displayed in DisplayFormation.Displayed(formed.Context).TraverseM(pass => IO.lift(() => pass.Run(formed.Frame, Unreported))).As()
        select formed.Frame;

    public IO<Unit> RestoreSettings(RhinoDoc doc) =>
        from saved in IO.lift(State.Bind(static state => state.Settings).ToFin(new NotFound(nameof(RenderingState.Settings), Image.FileName)))
        from restored in Committed(doc, RenderSettingsSources.Edit(doc, saved))
        select restored;

    public IO<ChainState> RestoreChain(RhinoDoc doc) => Chain.Bind(state => new ChainEdit.Restore(state).Committed(doc));

    private IO<T> Committed<T>(RhinoDoc doc, IO<T> body, [CallerMemberName] string operation = "") =>
        Commits.Commit(doc, $"{operation} {Path.GetFileNameWithoutExtension(Image.FileName)}", new RedrawPolicy.AllViews(Deferred: true), body)
            .Map(static committed => committed.Value);
}

internal sealed record RenderingSidecar(Guid Session, string Engine, Guid EngineId, TimeSpan Elapsed, string? Settings, string? Note) {
    public Fin<RenderingState> State =>
        Optional(Settings).Traverse(static text => Try.lift(() => CommonObject.FromJSON(text)).Run()
                .Bind(static read => Refused.UnlessFound(read, nameof(CommonObject.FromJSON)))
                .Bind(Conversions.Cast<RenderSettings>))
            .As()
            .Map(settings => new RenderingState(Session, Engine, EngineId, Elapsed.ToDuration(), settings, Optional(Note)));

    public static RenderingSidecar Of(RenderingState state) =>
        new(state.Session, state.Engine, state.EngineId, state.Elapsed.ToTimeSpan(), state.Settings.Map(static settings => settings.ToJSON(new SerializationOptions())).ValueUnsafe(), state.Note.ValueUnsafe());
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Strict, WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(RenderingSidecar))]
internal sealed partial class RenderingSidecarContext : JsonSerializerContext;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RenderHistory {
    private readonly IPlugInViews views;
    private readonly string suffix;
    private readonly SemaphoreSlim recording;
    private readonly Action<IDisposable> retain;
    private readonly Ref<Ledger> held = Ref(Ledger.Empty);

    private RenderHistory(IPlugInViews views, SemaphoreSlim recording, Action<IDisposable> retain) =>
        (this.views, suffix, this.recording, this.retain, Changed) =
        (views, $".{views.PlugIn.Id:N}.json", recording, retain, Subscriptions.Changed(held).Choose(static _ => Some(unit)));

    private sealed record Ledger(Option<string> Folder, HashMap<string, Rendering> Renderings) {
        public static Ledger Empty { get; } = new(None, HashMap<string, Rendering>());

        public Seq<Rendering> Listed(int keep) =>
            toSeq(Renderings.Values.OrderByDescending(static rendering => rendering.Saved).ThenBy(static rendering => rendering.Image.FileName, StringComparer.Ordinal)).Take(keep);
    }

    // --- [ACQUISITION]
    public static PlugInSlot<RenderHistory> Slot { get; } = new();

    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Register { get; } = static (views, retain) =>
        DisposalOps.Acquire(owned =>
            from recording in DisposalOps.Acquire(IO.lift(static () => new SemaphoreSlim(1, 1)), owned)
            from history in IO.lift(() => new RenderHistory(views, recording, owned))
            from saved in EventKind.ImageFileSaved.Inline(history.Saved, views, owned)
            from loaded in EventKind.ImageFileLoaded.Inline(args => history.Claimed(args.FileName), views, owned)
            from deleted in EventKind.ImageFileDeleted.Inline(history.Deleted, views, owned)
            from published in Slot.Publish(views, history)
            select unit, retain);

    // --- [READS]
    public HostEvent<Unit> Changed { get; }

    public IO<Seq<Rendering>> Renderings =>
        from ledger in held.ValueIO
        from keep in IO.lift(SupportOptions.AutoSaveKeepAmount)
        select ledger.Listed(keep);

    // --- [WRITES]
    public IO<T> Launched<T>(RhinoDoc doc, IO<(ImageFileEventArgs Saved, T Value)> render) =>
        from captured in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), static scope => IO.lift(scope.Settings.Duplicate))
        from rendered in render
        from recorded in Recorded(rendered.Saved, Some(captured))
        select rendered.Value;

    public IO<Rendering> Annotated(Rendering rendering, Option<string> note) =>
        IO.lift(rendering.State.ToFin(new NotFound(nameof(RenderingState), rendering.Image.FileName))).Bind(state => Recorded(rendering, state with { Note = note }));

    public IO<Unit> Delete(Rendering rendering) =>
        from deleted in IO.lift(() => File.Delete(rendering.Image.FileName))
        from sidecar in IO.lift(() => File.Delete(Sidecar(rendering.Image.FileName)))
        from removed in Swapped(ledger => ledger with { Renderings = ledger.Renderings.Remove(rendering.Image.FileName) })
        select unit;

    private IO<Ledger> Swapped(Func<Ledger, Ledger> change) => atomicIO(() => held.Swap(change));

    // --- [EVENTS]
    private IO<Unit> Saved(ImageFileEventArgs args) =>
        from claimed in Claimed(args.FileName)
        from recorded in Recorded(args, None)
        select unit;

    private IO<Unit> Deleted(ImageFileEventArgs args) =>
        from claimed in Claimed(args.FileName)
        from removed in Swapped(ledger => ledger with { Renderings = ledger.Renderings.Remove(args.FileName) })
        from sidecar in Forked(IO.lift(() => File.Delete(Sidecar(args.FileName))))
        select unit;

    private IO<Unit> Claimed(string fileName) =>
        from folder in IO.lift(() => Optional(Path.GetDirectoryName(fileName)).ToFin(new FileRefused(fileName, FileReason.Unqualified)))
        from claimed in atomicIO(() => (Before: held.Value, After: held.Value = held.Value with { Folder = held.Value.Folder | Some(folder) }))
        from scanned in when(claimed.Before.Folder.IsNone, Forked(Scanned(folder))).As()
        select unit;

    private IO<Unit> Forked(IO<Unit> work, [CallerMemberName] string member = "") =>
        DisposalOps.Fork(work.Catch(error => IO.lift(() => views.Report(error, typeof(RenderHistory), member))).As(), retain).Map(static _ => unit);

    // --- [FILES]
    private IO<Unit> Scanned(string folder) =>
        from fileNames in IO.lift(() => toSeq(Directory.EnumerateFiles(folder, "*.rimage")).Strict())
        from sidecars in IO.lift(() => toSeq(Directory.EnumerateFiles(folder, $"*{suffix}")).Strict())
        let kept = toHashSet(fileNames.Map(Sidecar))
        from orphans in sidecars.Filter(sidecar => !kept.Contains(sidecar)).Map<K<IO, Unit>>(static sidecar => IO.lift(() => File.Delete(sidecar))).PartitionFallible().As()
        from read in fileNames.Map<K<IO, Rendering>>(fileName => Stated(Sidecar(fileName)).Bind(state => Opened(fileName, state))).PartitionFallible().As()
        from listed in Swapped(ledger => ledger with { Renderings = ledger.Renderings.TryAddRange(read.Succs.Map(static rendering => (rendering.Image.FileName, rendering))) })
        from failed in unless(orphans.Fails.IsEmpty && read.Fails.IsEmpty, IO.fail<Unit>(Error.Many(orphans.Fails + read.Fails))).As()
        select unit;

    private static IO<Rendering> Opened(string fileName, Option<RenderingState> state) =>
        from image in Rimage.Open(fileName)
        from extent in IO.lift(PixelExtent.Validated(image.Size.Width, image.Size.Height))
        from saved in IO.lift(() => Instant.FromDateTimeUtc(File.GetLastWriteTimeUtc(fileName)))
        select new Rendering(image, extent, saved, state);

    private static IO<Option<RenderingState>> Stated(string sidecar) =>
        JsonFiles.Read(sidecar, RenderingSidecarContext.Default.RenderingSidecar, static file => file.State);

    private IO<Rendering> Recorded(ImageFileEventArgs args, Option<RenderSettings> settings) =>
        from opened in Opened(args.FileName, None)
        from recorded in Recorded(opened, new RenderingState(args.SessionId, args.RenderEngine, args.RenderEngineId, Duration.FromSeconds(args.EllapsedTime), settings, None))
        select recorded;

    private IO<Rendering> Recorded(Rendering rendering, RenderingState state) =>
        DisposalOps.Within(IO.liftAsync(environment => recording.WaitAsync(environment.Token).ToUnit()), _ =>
            from path in IO.lift(OutputPath.Of(Sidecar(rendering.Image.FileName)))
            from written in JsonFiles.Write(path, RenderingSidecar.Of(state), RenderingSidecarContext.Default.RenderingSidecar)
            let noted = rendering with { State = Some(state) }
            from listed in Swapped(ledger => ledger with { Renderings = ledger.Renderings.AddOrUpdate(noted.Image.FileName, noted) })
            select noted,
            _ => IO.lift(() => recording.Release()).Map(static _ => unit));

    private string Sidecar(string fileName) => Path.ChangeExtension(fileName, suffix);
}
