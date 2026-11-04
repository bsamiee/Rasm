using System.Text.Json.Serialization.Metadata;
using Darkroom.Chain;
using Microsoft.VisualStudio.Threading;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Sessions;
using Rhino;
using Rhino.Render;

namespace Darkroom.Review;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderingState(Guid Session, string Engine, Guid EngineId, Duration Elapsed, Option<string> Note) {
    internal static readonly JsonTypeInfo<RenderingState> Info = ArchivableDictionaries.Info<RenderingState>();
}

public sealed record Rendering(Rimage Image, PixelExtent Extent, Instant Saved, Option<RenderingState> State) {
    public static ChainState Ungraded { get; } = new([], ValueSet.Empty);

    public Fin<ChainState> Chain =>
        Image.Running.Exists(static id => id == typeof(DarkroomEffect).GUID) ? EffectList.State(Image, typeof(DarkroomEffect).GUID, ChainState.Default) : Ungraded;

    public IO<PixelFrame> Frame => Plane(RenderWindow.StandardChannels.RGBA);

    public IO<PixelFrame> Plane(RenderWindow.StandardChannels channel) =>
        Image.Channel(channel).Map(pixels => new PixelFrame(System.Drawing.Point.Empty, Extent, Extent, block => pixels.Span.CopyTo(block)));

    public IO<ChainState> RestoreChain(RhinoDoc doc) => IO.lift(Chain).Bind(state => new ChainEdit.Restore(state).Committed(doc));
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class RenderHistory {
    private sealed record Ledger(Option<string> Folder, HashMap<string, Rendering> Renderings) {
        public static Ledger Empty { get; } = new(None, HashMap<string, Rendering>());
    }

    private static readonly AsyncSemaphore Recording = new(1);
    private static readonly Ref<Ledger> Held = Ref(Ledger.Empty);

    // --- [ACQUISITION]
    public static IO<Unit> Load { get; } =
        from recording in use(static () => Recording)
        from saved in Subscriptions.On<ImageFileEventArgs>(static h => ImageFile.Saved += h, static h => ImageFile.Saved -= h).Attach(static args => Forked(Saved(args)))
        from loaded in Subscriptions.On<ImageFileEventArgs>(static h => ImageFile.Loaded += h, static h => ImageFile.Loaded -= h).Attach(static args => Forked(Claimed(args.FileName)))
        from deleted in Subscriptions.On<ImageFileEventArgs>(static h => ImageFile.Deleted += h, static h => ImageFile.Deleted -= h).Attach(static args => Forked(Deleted(args)))
        select unit;

    // --- [READS]
    public static HostEvent<Unit> Changed { get; } = Subscriptions.Changed(Held).Choose(static _ => Some(unit));

    public static IO<Seq<Rendering>> Renderings =>
        from ledger in Held.ValueIO
        from keep in IO.lift(SupportOptions.AutoSaveKeepAmount)
        select toSeq(ledger.Renderings.Values.OrderByDescending(static rendering => rendering.Saved).ThenBy(static rendering => rendering.Image.FileName, StringComparer.Ordinal)).Take(keep);

    // --- [WRITES]
    public static IO<Rendering> Annotated(Rendering rendering, Option<string> note) =>
        (from entered in use(IO.liftAsync(static environment => Recording.EnterAsync(environment.Token)))
         from state in IO.lift(rendering.State.ToFin(new DarkroomFault((FormattableString)$"render state of {rendering.Image.FileName}", DarkroomReason.Missing)))
         from recorded in Recorded(rendering, state with { Note = note })
         select recorded).Bracket();

    public static IO<Unit> Delete(Rendering rendering) =>
        (from entered in use(IO.liftAsync(static environment => Recording.EnterAsync(environment.Token)))
         from deleted in IO.lift(() => File.Delete(rendering.Image.FileName))
         from sidecar in IO.lift(() => File.Delete(Sidecar(rendering.Image.FileName)))
         from removed in Swapped(ledger => ledger with { Renderings = ledger.Renderings.Remove(rendering.Image.FileName) })
         select unit).Bracket();

    private static IO<Ledger> Swapped(Func<Ledger, Ledger> change) => atomicIO(() => Held.Swap(change));

    // --- [EVENTS]
    private static IO<Unit> Forked(IO<Unit> work) => work.Fork().Bind(static forked => forked.Await);

    private static IO<Unit> Saved(ImageFileEventArgs args) =>
        (from entered in use(IO.liftAsync(static environment => Recording.EnterAsync(environment.Token)))
         from claimed in Claimed(args.FileName)
         from opened in Opened(args.FileName, None)
         from recorded in Recorded(opened, new RenderingState(args.SessionId, args.RenderEngine, args.RenderEngineId, Duration.FromSeconds(args.EllapsedTime), None))
         select unit).Bracket();

    private static IO<Unit> Deleted(ImageFileEventArgs args) =>
        (from entered in use(IO.liftAsync(static environment => Recording.EnterAsync(environment.Token)))
         from claimed in Claimed(args.FileName)
         from removed in Swapped(ledger => ledger with { Renderings = ledger.Renderings.Remove(args.FileName) })
         from sidecar in IO.lift(() => File.Delete(Sidecar(args.FileName)))
         select unit).Bracket();

    private static IO<Unit> Claimed(string fileName) =>
        from folder in IO.lift(() => Optional(Path.GetDirectoryName(fileName)).ToFin(new RhinoFault(fileName, RhinoReason.Unqualified)))
        from prior in atomicIO(() => (Before: Held.Value, After: Held.Value = Held.Value with { Folder = Held.Value.Folder | Some(folder) }))
        from scanned in when(prior.Before.Folder.IsNone, IO.lift(() => Session.Start(Scanned(folder)))).As()
        select unit;

    // --- [FILES]
    private static IO<Unit> Scanned(string folder) =>
        (from entered in use(IO.liftAsync(static environment => Recording.EnterAsync(environment.Token)))
         from fileNames in IO.lift(() => toSeq(Directory.EnumerateFiles(folder, "*.rimage")).Strict())
         from sidecars in IO.lift(() => toSeq(Directory.EnumerateFiles(folder, $"*{Suffix}")).Strict())
         let kept = toHashSet(fileNames.Map(Sidecar))
         from orphans in IO.lift(() => sidecars.Filter(sidecar => !kept.Contains(sidecar)).Map(static sidecar => Try.lift(fun(() => File.Delete(sidecar))).Run()).Strict())
         from read in fileNames.Map<K<IO, Rendering>>(static fileName => Stated(Sidecar(fileName)).Bind(state => Opened(fileName, state))).PartitionFallible().As()
         from listed in Swapped(ledger => ledger with { Renderings = ledger.Renderings.TryAddRange(read.Succs.Map(static rendering => (rendering.Image.FileName, rendering))) })
         from failed in IO.lift((orphans + read.Fails.Map(static error => Fin.Fail<Unit>(error))).Traverse(static deleted => deleted).As().Map(static _ => unit))
         select unit).Bracket();

    private static IO<Rendering> Opened(string fileName, Option<RenderingState> state) =>
        from image in Rimage.Open(fileName)
        from extent in IO.lift(PixelExtent.Validated(image.Size.Width, image.Size.Height))
        from saved in IO.lift(() => Instant.FromDateTimeUtc(File.GetLastWriteTimeUtc(fileName)))
        select new Rendering(image, extent, saved, state);

    private static IO<Option<RenderingState>> Stated(string sidecar) => FilePublication.Read(sidecar, RenderingState.Info, static state => Fin.Succ(state));

    private static IO<Rendering> Recorded(Rendering rendering, RenderingState state) =>
        from path in IO.lift(OutputPath.Of(Sidecar(rendering.Image.FileName)))
        from written in FilePublication.Write(path, state, RenderingState.Info)
        let noted = rendering with { State = Some(state) }
        from listed in Swapped(ledger => ledger with { Renderings = ledger.Renderings.AddOrUpdate(noted.Image.FileName, noted) })
        select noted;

    private static string Suffix => $".{Session.PlugIn.Id:N}.json";

    private static string Sidecar(string fileName) => Path.ChangeExtension(fileName, Suffix);
}
