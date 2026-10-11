using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SaveSource(RhinoDoc Document, RenderWindow Window, Option<RenderRun> Framing);

public sealed record FileOrigin(SoftwareName Software, ZonedClock Clock, Map<FieldName, FieldText> Fields) {
    public static IO<FileOrigin> At(IPlugInSink sink) =>
        IO.lift(() => (PlugIn)sink switch {
            var plugIn => (
                    Conversions.Validated<SoftwareName, string, InvalidOutput>(plugIn.Name).ToValidation(),
                    Field("Version", plugIn.Version),
                    Field("SettingsKey", plugIn.Id.ToString("D", CultureInfo.InvariantCulture)))
                .Apply((software, version, settings) => new FileOrigin(software, ((IPlugInViews)sink).Clock.ToZonedClock(), Map(version, settings)))
                .As()
                .ToFin(),
        });

    internal static Validation<Error, (FieldName Name, FieldText Text)> Field(string name, string text) =>
        (Conversions.Validated<FieldName, string, InvalidOutput>(name).ToValidation(), Conversions.Validated<FieldText, string, InvalidOutput>(text).ToValidation())
            .Apply(static (field, value) => (field, value))
            .As();
}

public sealed record IdentitySource(ushort Id, Option<string> Name, Color Colour);

[SmartEnum<RenderWindow.StandardChannels>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public sealed partial class SavedChannel {
    private const ushort IdMask = 0x7FFF;

    public static readonly SavedChannel Distance = new(RenderWindow.StandardChannels.DistanceFromCamera, "Depth", None, static (_, frame) => new ExrPass.Depth(frame));
    public static readonly SavedChannel Normals = new(RenderWindow.StandardChannels.NormalXYZ, "Normal", None, static (name, frame) => new ExrPass.Normal(name, frame));
    public static readonly SavedChannel Albedo = new(RenderWindow.StandardChannels.AlbedoRGB, "DiffCol", None, static (name, frame) => new ExrPass.Named(name, PassKind.Rgb, frame));
    public static readonly SavedChannel MaterialIds = new(RenderWindow.StandardChannels.MaterialIds, "IndexMA",
        Some<Func<RhinoDoc, Seq<IdentitySource>>>(static document => toSeq(document.RenderMaterials).Map(static material => {
            using Material simulated = material.ToMaterial(RenderTexture.TextureGeneration.Skip);
            return new IdentitySource((ushort)(material.RenderHash & IdMask), Conversions.Present(material.Name), simulated.DiffuseColor);
        }).Strict()),
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel ObjectIds = new(RenderWindow.StandardChannels.ObjectIds, "IndexOB",
        Some<Func<RhinoDoc, Seq<IdentitySource>>>(static document => toSeq(document.Objects.GetObjectList(ObjectType.AnyObject)).Map(rhinoObject =>
            new IdentitySource((ushort)(RhinoMath.CRC32(0u, rhinoObject.Id.ToByteArray()) & IdMask), Conversions.Present(rhinoObject.Attributes.Name), rhinoObject.Attributes.DrawColor(document))).Strict()),
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel WireframePoints = new(RenderWindow.StandardChannels.WireframePointsRGBA, nameof(RenderWindow.StandardChannels.WireframePointsRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel WireframeIsocurves = new(RenderWindow.StandardChannels.WireframeIsocurvesRGBA, nameof(RenderWindow.StandardChannels.WireframeIsocurvesRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel WireframeCurves = new(RenderWindow.StandardChannels.WireframeCurvesRGBA, nameof(RenderWindow.StandardChannels.WireframeCurvesRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel WireframeAnnotations = new(RenderWindow.StandardChannels.WireframeAnnotationsRGBA, nameof(RenderWindow.StandardChannels.WireframeAnnotationsRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));

    public string Layer { get; }
    public Option<Func<RhinoDoc, Seq<IdentitySource>>> Sources { get; }

    [UseDelegateFromConstructor]
    internal partial ExrPass Part(PassName name, PixelFrame frame);

    public static bool operator <(SavedChannel left, SavedChannel right) => Comparer<SavedChannel>.Default.Compare(left, right) < 0;
    public static bool operator <=(SavedChannel left, SavedChannel right) => Comparer<SavedChannel>.Default.Compare(left, right) <= 0;
    public static bool operator >(SavedChannel left, SavedChannel right) => Comparer<SavedChannel>.Default.Compare(left, right) > 0;
    public static bool operator >=(SavedChannel left, SavedChannel right) => Comparer<SavedChannel>.Default.Compare(left, right) >= 0;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Saves {
    // --- [WRITES]
    public static IO<Unit> Write(SaveSource source, FileOrigin origin,
        Seq<(OutputTarget Target, OutputPath Path, Option<RenderRegion> Region, OverscanFrame Overscan)> outputs) =>
        from metadata in Metadata(source.Document, origin)
        from written in Callbacks.Each(outputs.Map(output => Write(source, metadata, output.Target, output.Path, output.Region, output.Overscan)))
        select unit;

    private static IO<Unit> Write(SaveSource source, FileMetadata metadata,
        OutputTarget target, OutputPath path, Option<RenderRegion> region, OverscanFrame overscan) =>
        from built in target.FileFormat.Write(target).Switch(
                (Source: source, Target: target, Place: Placed(region, overscan)),
                raster: static (save, raster) => (raster.Depth.Levels.IsSome
                        ? Dib(save.Source.Window).Bind(frame => IO.lift(save.Place(frame))).Map(static frame => (Frame: frame, Encoding: Option<ColorEncoding>.None))
                        : Read(save.Source.Window, RenderWindow.StandardChannels.RGBA, save.Place).Map(static frame => (Frame: frame, Encoding: Some(TargetWrite.SceneLinear))))
                    .Bind(held => IO.lift(raster.Still(held.Frame, held.Encoding))),
                dib: static (save, dib) => Dib(save.Source.Window).Bind(frame => IO.lift(save.Place(frame))).Bind(frame => IO.lift(dib.Still(frame))),
                raw: static (save, raw) => Read(save.Source.Window, RenderWindow.StandardChannels.RGBA, save.Place).Bind(frame => IO.lift(() => raw.Still(frame, save.Source.Framing))),
                layers: static (save, layers) => Passes(save.Source, layers, save.Target, save.Place),
                layered: static (save, _) => Layered(save.Source, save.Target, save.Place))
        from _ in StillWriter.Write(built, metadata, path)
        select unit;

    public static IO<OverscanFrame> Whole(RenderWindow window) =>
        IO.lift(() => EffectPipeline.Extent(window.Size()).Bind(static final => OverscanFrame.Around(final, Overscan.Off)));

    public static IO<Seq<(OutputName Name, OutputPath Path)>> Write(
        SaveSource source, FileOrigin origin, Destination destination, Seq<(OutputTarget Target, Seq<NamePart> Scope, OutputVersioning Version, Seq<NamePart> Parts)> outputs) =>
        from names in IO.lift(Named(outputs))
        from placed in Destinations.Resolve(source.Document, destination, names)
        from whole in Whole(source.Window)
        from written in Write(source, origin, outputs.Zip(placed)
            .Map(pair => (pair.First.Target, pair.Second.Path, Option<RenderRegion>.None, whole)))
        select placed;

    public static IO<Seq<(OutputName Name, OutputPath Path)>> Preview(RhinoDoc document, OutputBook book) =>
        IO.lift(Named(Fresh(book))).Bind(names => Destinations.Preview(document, book.Destination, names));

    private static Seq<(OutputTarget Target, Seq<NamePart> Scope, OutputVersioning Version, Seq<NamePart> Parts)> Fresh(OutputBook book) =>
        book.Named.Map(static named => (named.Target, Seq<NamePart>(), (OutputVersioning)new OutputVersioning.NextFree(), named.Parts));

    private static Fin<Seq<OutputName>> Named(Seq<(OutputTarget Target, Seq<NamePart> Scope, OutputVersioning Version, Seq<NamePart> Parts)> outputs) =>
        Callbacks.Each(outputs, static (output, _) => Conversions.Validated<FileExtension, string, InvalidRhinoValue>(output.Target.FileFormat.Extension)
            .Map(extension => new OutputName(output.Scope, output.Version, output.Parts, None, extension)));

    public static IO<Seq<(OutputName Name, OutputPath Path)>> Write(
        RhinoDoc document, FileOrigin origin, OutputBook book,
        Either<(Guid Rendering, RenderRun Run), (PixelFrame Scene, TileRequest Request)> source) =>
        from names in IO.lift(Named(Fresh(book)))
        from placed in Destinations.Resolve(document, book.Destination, names)
        from metadata in Metadata(document, origin)
        from channels in book.Targets.Exists(static target => (target.FileFormat == TargetFile.Exr && target.Reader.IsSome)
                || (target.FileFormat == TargetFile.Layered && target.ColorDepth.Levels.IsNone))
            ? source.Match(
                Left: static captured => IO.lift(captured.Run.Channels.ToFin(new Missing(nameof(RenderRun.Channels)))),
                Right: static recorded => recorded.Request.Slot.Channels)
            : IO.pure(HashMap<RenderWindow.StandardChannels, PixelFrame>())
        from written in DisposalOps.Within(static () => new Derivations(), derivations =>
            Callbacks.Each(book.Targets.Zip(placed).Map(output =>
                from built in output.First.FileFormat.Write(output.First).Switch(
                    (Source: source, Target: output.First, Derivations: derivations, Channels: channels),
                    raster: static (save, raster) => Displayed(save.Source, save.Target, raster.Depth, save.Derivations)
                        .Bind(frame => IO.lift(raster.Still(frame, Some(save.Target.DisplayDevice.Encoding)))),
                    dib: static (save, dib) => Displayed(save.Source, save.Target, OutputDepth.UInt8, save.Derivations)
                        .Bind(frame => IO.lift(dib.Still(frame))),
                    raw: static (save, raw) => save.Source.Match(
                        Left: captured => IO.lift(captured.Run.Scene.ToFin(new SceneUntapped(captured.Rendering)))
                            .Bind(scene => IO.lift(() => raw.Still(scene, Some(captured.Run)))),
                        Right: recorded => IO.lift(() => raw.Still(recorded.Scene, None))),
                    layers: static (save, layers) => save.Source.Match(
                        Left: captured => IO.lift(captured.Run.Scene.ToFin(new SceneUntapped(captured.Rendering)))
                            .Bind(scene => Passes(None, scene, save.Channels, Some(captured.Run), layers, save.Target)),
                        Right: recorded => Passes(None, recorded.Scene, save.Channels, None, layers, save.Target)),
                    layered: static (save, _) =>
                        from frame in Displayed(save.Source, save.Target, save.Target.ColorDepth, save.Derivations)
                        from name in IO.lift(Conversions.Validated<PassName, string, InvalidOutput>(nameof(RenderWindow.StandardChannels.RGBA)))
                        from auxiliary in toSeq(SavedChannel.Items).Filter(channel => save.Target.ColorDepth.Levels.IsNone && save.Channels.ContainsKey(channel.Key))
                            .TraverseM(channel => Layer(None, channel, save.Channels[channel.Key], Some(save.Target.DisplayDevice.Encoding))).As()
                        from layeredDocument in IO.lift(LayeredDocument.From(save.Target.ColorDepth, Some(save.Target.DisplayDevice.Encoding),
                            new DocumentLayer.Raster(name, frame, BlendingMode.Mix, Mix.Full, Visible: true, Mask: None).Cons(auxiliary)).ToFin())
                        select (StillFormat)new StillFormat.Document(layeredDocument))
                from saved in StillWriter.Write(built, metadata, output.Second.Path)
                select unit)), static derivations => IO.lift(derivations.Dispose))
        select placed;

    private static IO<PixelFrame> Displayed(
        Either<(Guid Rendering, RenderRun Run), (PixelFrame Scene, TileRequest Request)> source,
        OutputTarget target, OutputDepth depth, Derivations derivations) =>
        from quantizer in unless(depth.Levels.IsSome || target.Dithering == Dither.Off, IO.fail<Unit>(new InvalidQuantization())).As()
        from extent in IO.lift(source.Match(
            Left: static captured => (captured.Run.Scene || captured.Run.Formed).Map(static frame => frame.Extent).ToFin(new FrameUnformed(captured.Rendering)),
            Right: static recorded => Fin.Succ(recorded.Scene.Extent)))
        let context = EffectPipeline.Detached(extent, derivations) with {
            Display = target.DisplayDevice,
            Depth = depth,
            Signal = target.DisplayDevice.Encoding.Transfer,
            Lights = None,
        }
        from prepared in source.Match(
            Left: captured => (from formation in captured.Run.Formation
                               from scene in captured.Run.Scene
                               where captured.Run.NativeLate.IsEmpty
                               select (Frame: scene.Copy(), Passes: formation(context).ToSeq() + captured.Run.Late.Choose(pass => pass(context)))).Match(
                Some: static held => IO.pure(held),
                None: () => captured.Run.NativeLate.IsEmpty || target.DisplayDevice == EffectPipeline.Display
                    ? IO.lift(captured.Run.Output.ToFin(new FrameUnformed(captured.Rendering)))
                        .Bind(output => IO.lift(output.Frame.Map(held => (Frame: held.Frame.Copy(),
                            Passes: held.Encoding.To(context.Display.Encoding).Map<PixelPass>(static step => new PixelPass.Color(step))
                                + output.Prefix.ToSeq().Bind(prefix => captured.Run.Late.Skip(prefix).Choose(pass => pass(context)))))))
                    : IO.fail<(PixelFrame Frame, Seq<PixelPass> Passes)>(new NativeOutputUnavailable(captured.Rendering, captured.Run.NativeLate))),
            Right: recorded => EffectPipeline.Chain(recorded.Request.Set,
                    recorded.Request.Chain.Filter(static kind => kind.State != typeof(Dither)),
                    context with { Signal = TransferCurve.Linear, Lights = None })
                .Map(chain => (Frame: recorded.Scene.Copy(), Passes: chain.Map(static stage => stage.Pass)
                    + (chain.Exists(static stage => stage.Stage == PostEffectType.ToneMapping)
                        ? Seq<PixelPass>()
                        : TargetWrite.SceneLinear.To(context.Display.Encoding).Map<PixelPass>(static step => new PixelPass.Color(step))))))
        from formed in prepared.Passes.TraverseM(pass => IO.lift(() => pass.Run(prepared.Frame, new Progress<int>()))).As()
        from quantized in depth.Levels.Traverse(levels => IO.lift(() => target.Dithering.Pass(levels).Run(prepared.Frame, new Progress<int>()))).As()
        select prepared.Frame;

    private static Func<PixelFrame, Fin<PixelFrame>> Placed(Option<RenderRegion> region, OverscanFrame overscan) =>
        region.Map(held => held.Pixels(overscan.Frame)).IfNone(overscan.DataWindow) switch {
            var window => read => EffectPipeline.Extent(window.Size).Map(size => new PixelFrame(window.Location, size, overscan.Frame, block => {
                for (int line = 0; line < size.Height; line++)
                    read.Block.Slice(4 * (((window.Y + overscan.Pad.Height + line) * read.Size.Width) + window.X + overscan.Pad.Width), 4 * size.Width)
                        .CopyTo(block.AsSpan(4 * line * size.Width));
            })),
        };

    // --- [FRAMES]
    private static IO<PixelFrame> Dib(RenderWindow window) =>
        RenderWindows.Snapshot(window, static bitmap =>
            from extent in IO.lift(() => EffectPipeline.Extent(bitmap.Size))
            from frame in DisposalOps.Within(
                () => bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb),
                data => IO.lift(() => new PixelFrame(System.Drawing.Point.Empty, extent, extent, block => {
                    byte[] line = new byte[4 * extent.Width];
                    Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
                    for (int y = 0; y < extent.Height; y++) {
                        Marshal.Copy(data.Scan0 + (y * data.Stride), line, 0, line.Length);
                        for (int x = 0; x < extent.Width; x++)
                            pixels[(y * extent.Width) + x] = new Vector4(line[(4 * x) + 2], line[(4 * x) + 1], line[4 * x], line[(4 * x) + 3]) / byte.MaxValue;
                    }
                })),
                data => IO.lift(() => bitmap.UnlockBits(data)))
            select frame);

    private static IO<PixelFrame> Read(RenderWindow window, RenderWindow.StandardChannels channel, Func<PixelFrame, Fin<PixelFrame>> place) =>
        RenderWindows.Read(window, channel).Bind(read => IO.lift(place(read)));

    private static IO<Seq<SavedChannel>> Saved(RenderWindow window) =>
        RenderWindows.Channels(window).Map(static held => held.Requested.Choose(static channel => Callbacks.Found(SavedChannel.TryGet(channel, out SavedChannel? item), item!)));

    // --- [PASS_SETS]
    private static IO<StillFormat> Passes(SaveSource source, TargetWrite.Layers layers, OutputTarget target, Func<PixelFrame, Fin<PixelFrame>> place) =>
        from beauty in Read(source.Window, RenderWindow.StandardChannels.RGBA, place)
        from channels in Saved(source.Window)
        let requested = target.Position && !channels.Exists(static channel => channel == SavedChannel.Distance) ? channels.Add(SavedChannel.Distance) : channels
        from frames in requested.TraverseM(channel => Read(source.Window, channel.Key, place).Map(frame => (channel.Key, frame))).As()
        from passes in Passes(Some(source.Document), beauty, toHashMap(frames), source.Framing, layers, target)
        select passes;

    private static IO<StillFormat> Passes(Option<RhinoDoc> document, PixelFrame beauty, HashMap<RenderWindow.StandardChannels, PixelFrame> channels,
        Option<RenderRun> framing, TargetWrite.Layers layers, OutputTarget target) =>
        from layout in IO.lift(layers.Layout)
        from position in IO.lift(target.Position
            ? (channels.Find(SavedChannel.Distance.Key).ToFin(new Missing(nameof(RenderWindow.StandardChannels.DistanceFromCamera))),
                framing.Bind(static run => run.Metres).ToFin(new Missing(nameof(RenderRun.Metres))))
                .Apply(static (distance, metres) => Some(EffectPipeline.Metric(distance, metres))).As()
            : Fin.Succ(Option<PixelFrame>.None))
        from named in toSeq(SavedChannel.Items).Filter(channel => channels.ContainsKey(channel.Key)).TraverseM(channel =>
            from name in IO.lift(Conversions.Validated<PassName, string, InvalidOutput>(channel.Layer))
            let frame = channels[channel.Key]
            from entries in IO.lift((from sources in channel.Sources from doc in document select sources(doc))
                .Traverse(sources => Identified(frame, sources, TargetWrite.SceneLinear)).As())
            let parts = channel.Part(name, frame).Cons(entries.Map<ExrPass>(identified =>
                new ExrPass.Identities(PassName.Create(channel.Key.ToString()), frame, identified)).ToSeq())
            select channel == SavedChannel.Distance
                ? parts.Add(new ExrPass.InverseDepth(PassName.Create(nameof(ExrPass.InverseDepth)), frame))
                : parts).As()
        from set in IO.lift(PassSet.From(layers.Reader.Convention, framing.Bind(static run => run.Camera), PassLayout.Parts,
            named.Flatten() + position.Map<ExrPass>(static distance => new ExrPass.Position(distance)).ToSeq()).ToFin())
        from still in IO.lift(StillFormat.ExrRaster.From(beauty, layout, target.Alpha, Some(set)).ToFin())
        select still;

    // --- [DOCUMENTS]
    private static IO<StillFormat> Layered(SaveSource source, OutputTarget target, Func<PixelFrame, Fin<PixelFrame>> place) =>
        target.ColorDepth.Levels.IsSome switch {
            var formed =>
                from beauty in formed ? Dib(source.Window).Bind(frame => IO.lift(place(frame))) : Read(source.Window, RenderWindow.StandardChannels.RGBA, place)
                let encoding = Callbacks.Found(!formed, TargetWrite.SceneLinear)
                from channels in Saved(source.Window)
                from layers in channels.Filter(channel => !formed || channel.Sources.IsSome).TraverseM(channel => Layer(source, channel, encoding, place)).As()
                from beautyName in IO.lift(Conversions.Validated<PassName, string, InvalidOutput>(nameof(RenderWindow.StandardChannels.RGBA)))
                from document in IO.lift(LayeredDocument.From(
                    target.ColorDepth, encoding, new DocumentLayer.Raster(beautyName, beauty, BlendingMode.Mix, Mix.Full, Visible: true, Mask: None).Cons(layers)).ToFin())
                select (StillFormat)new StillFormat.Document(document),
        };

    private static IO<DocumentLayer> Layer(SaveSource source, SavedChannel channel, Option<ColorEncoding> encoding, Func<PixelFrame, Fin<PixelFrame>> place) =>
        from frame in Read(source.Window, channel.Key, place)
        from layer in Layer(Some(source.Document), channel, frame, encoding)
        select layer;

    private static IO<DocumentLayer> Layer(Option<RhinoDoc> document, SavedChannel channel, PixelFrame frame, Option<ColorEncoding> encoding) =>
        from name in IO.lift(Conversions.Validated<PassName, string, InvalidOutput>(channel.Layer))
        from layer in IO.lift(() => (from sources in channel.Sources from doc in document select sources(doc)).Match(
            Some: sources => Identified(frame, sources, encoding).Map(entries => DocumentLayer.Identities(name, frame, entries)),
            None: () => Fin.Succ<DocumentLayer>(new DocumentLayer.Raster(name, frame, BlendingMode.Mix, Mix.Full, Visible: false, Mask: None))))
        select layer;

    private static Fin<Seq<IdentityLayer>> Identified(PixelFrame ids, Seq<IdentitySource> sources, Option<ColorEncoding> encoding) {
        System.Collections.Generic.HashSet<ushort> present = [];
        foreach (Vector4 pixel in MemoryMarshal.Cast<float, Vector4>(ids.Block))
            _ = present.Add((ushort)pixel.X);
        Seq<Action<Span<Vector4>>> steps = encoding.Map(Imaging.Tone.Formations.Display.Srgb.Encoding.To).IfNone([]);
        return Callbacks.Each(
                toSeq(sources
                    .Filter(source => present.Contains(source.Id))
                    .Fold(Map<ushort, (Color Colour, Seq<string> Names)>(), static (held, source) =>
                        held.AddOrUpdate(source.Id, same => (same.Colour, same.Names + source.Name.ToSeq()), (source.Colour, source.Name.ToSeq())))
                    .AsIterable()),
                (entry, _) => Conversions.Validated<PassName, string, InvalidOutput>(
                        entry.Value.Names.IsEmpty ? entry.Key.ToString(CultureInfo.InvariantCulture) : string.Join(", ", entry.Value.Names))
                    .Map(layer => new IdentityLayer(entry.Key, layer, Encoded(entry.Value.Colour))));

        Vector3 Encoded(Color colour) {
            Vector4[] pixel = [new Vector4(colour.R, colour.G, colour.B, byte.MaxValue) / byte.MaxValue];
            _ = steps.Iter(step => step(pixel));
            return pixel[0].AsVector3();
        }
    }

    // --- [METADATA]
    internal static IO<FileMetadata> Metadata(RhinoDoc document, FileOrigin origin) =>
        from created in IO.lift(() => origin.Clock.GetCurrentZonedDateTime().ToDateTimeOffset())
        from density in SceneSources.Store<ImageOutputState>(document).Read.Map(static held => held.IfNone(ImageOutputState.Default).ImageDpi)
        from active in RenderSets.Book(document).Map(static book => book.Active)
        from set in IO.lift(active.Traverse(static name => FileOrigin.Field("RenderSet", name)).As().ToFin())
        select new FileMetadata(origin.Software, created, density, origin.Fields.AddRange(set.ToSeq()));

    // --- [PROMPTS]
    internal static IO<A> Measured<A>(RhinoDoc document, Func<RenderSettings, PixelExtent, IO<A>> body) =>
        SceneSources.Read(document, window =>
            IO.lift(() => EffectPipeline.Extent(RenderPipeline.RenderSize(document, fromRenderSources: true))).Bind(extent => body(window.Settings, extent)));

    internal static IO<OutputPath> Prompted(RhinoDoc document, RunMode mode, string title, string extension) =>
        from saved in IO.lift(() => Conversions.Present(document.Path))
        from typed in IO.lift(Conversions.Validated<FileExtension, string, InvalidRhinoValue>(extension))
        from suggested in IO.lift(saved.Traverse(file => Destinations.StemOf(new Stem.DocumentName(), file)
            .Map(stem => Naming.Compose(stem, new OutputName(Seq<NamePart>(), new OutputVersioning.Unversioned(), Seq<NamePart>(), None, typed), None))).As())
        from picked in Modals.GetFileName(mode, GetFileNameMode.Export, Conversions.Unset(suggested), title, None)
        from path in IO.lift(Conversions.Validated<OutputPath, string, InvalidOutput>(picked))
        select path;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class FrameExportCommand(IPlugInSink sink, Guid id, string englishName, Func<RenderSettings, PixelExtent, IO<PixelFrame>> frame)
    : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        Saves.Measured(doc, (settings, extent) =>
            from coordinates in frame(settings, extent)
            from extension in IO.lift(StillFormat.ExrRaster.Extensions.Head.ToFin(new Missing(nameof(StillFormat.ExrRaster.Extensions))))
            from path in Saves.Prompted(doc, mode, LocalName, extension)
            from origin in FileOrigin.At(site.Sink)
            from metadata in Saves.Metadata(doc, origin)
            from still in IO.lift(StillFormat.ExrRaster.From(coordinates, ExrLayout.Coordinates, alpha: true, None).ToFin())
            from _ in StillWriter.Write(still, metadata, path)
            select unit);
}

public sealed class LutExportCommand(IPlugInSink sink, Guid id, string englishName) : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        Saves.Measured(doc, (settings, extent) =>
            DisposalOps.Within(static () => new Derivations(), derivations =>
            from entries in EffectRegistry.Read(settings)
            let rendering = (IPlugInRendering)site.Sink
            from history in IPlugInRendering.Served(rendering.History, nameof(IPlugInRendering.History))
            let grading = history.Grading(doc)
            from set in grading.Capture
            let ordered = EffectPipeline.Ordered(entries, rendering.Effects)
            let context = EffectPipeline.Detached(extent, derivations) with { Lights = Some(SceneLights.Empty) }
            from chain in EffectPipeline.Chain(set, ordered, context)
            from chosen in Getters.Stages<LutChoice, LutChoice>(LutChoice.Default, (choice, accepts) => Getters.Choice(doc, new GetterRequest<GetOption, StageStep<LutChoice, LutChoice>>(LocalName, site) {
                Accept = accepts with { Nothing = Some((Option<string>.None, IO.pure<StageStep<LutChoice, LutChoice>>(new StageStep<LutChoice, LutChoice>.Done(choice)))) },
                Options = choice.Options,
            }))
            from path in Saves.Prompted(doc, mode, LocalName, chosen.Format.Extension)
            from table in Getters.Await(doc, LocalName, None, _ => IO.lift(Bakes.Bake(chosen.Bake, EffectPipeline.Baked(chain), EffectPipeline.Working)))
            from origin in FileOrigin.At(site.Sink)
            from metadata in Saves.Metadata(doc, origin)
            from _ in toSeq(LookupLayout.Items).Find(layout => Equals(layout.Format, chosen.Format)).Match(
                Some: layout => LutFiles.WriteLookup(table, layout, metadata, path),
                None: () => LutFiles.Write(table, chosen.Format, path))
            select unit, static derivations => IO.lift(derivations.Dispose)));

    private sealed record LutChoice(LatticeSize Size, AllocationStop Low, AllocationSpan Span, Gamut Gamut, Transfer Transfer, LutFormat Format) {
        public static LutChoice Default { get; } =
            new(LutBake.Default.Size, AllocationStop.Default, AllocationSpan.Default, LutBake.Default.Output.Gamut, LutBake.Default.Output.Transfer, LutFormat.Cube);

        public LutBake Bake => new(Size, new LatticeInput.Allocated(EffectPipeline.Working, Low, Span), new ColorEncoding(Gamut, Transfer, Nits.ReferenceWhite));

        public Seq<OptionSpec<StageStep<LutChoice, LutChoice>>> Options =>
            Seq(
                OptionSpec.Of("Size", OptionType.Integer<LatticeSize, int, InvalidColor>, Size, size => Next(this with { Size = size })),
                OptionSpec.Of("Low", OptionType.Number<AllocationStop, float, InvalidColor>, Low, low => Next(this with { Low = low })),
                OptionSpec.Of("Span", OptionType.Number<AllocationSpan, float, InvalidColor>, Span, span => Next(this with { Span = span })),
                OptionSpec.Of("Gamut", static (getter, name, plugIn, initial, retain) => OptionType.List(toSeq(Gamut.Items), static gamut => gamut.Map(
                    standardRgb: "Rec.709", displayP3: "Display P3", rec2020: "Rec.2020", acescg: "ACEScg", aces20651: "ACES2065-1", proPhoto: "ProPhoto", eGamut: "E-Gamut",
                    arriWideGamut3: "ARRI Wide Gamut 3", arriWideGamut4: "ARRI Wide Gamut 4", sGamut3: "S-Gamut3", sGamut3Cine: "S-Gamut3.Cine", veniceSGamut3: "Venice S-Gamut3",
                    veniceSGamut3Cine: "Venice S-Gamut3.Cine", cinemaGamut: "Cinema Gamut", redWideGamutRgb: "REDWideGamutRGB", vGamut: "V-Gamut", dGamut: "D-Gamut",
                    protuneNative: "Protune Native", blackmagicWideGamut: "Blackmagic Wide Gamut", daVinciWideGamut: "DaVinci Wide Gamut", appleWideGamut: "Apple Wide Gamut"),
                    getter, name, plugIn, initial, retain),
                    Gamut, gamut => Next(this with { Gamut = gamut })),
                OptionSpec.Of("Transfer", static (getter, name, sink, initial, retain) => OptionType.Text<Transfer, InvalidColor>(allowEmptyString: false, getter, name, sink, initial, retain), Transfer, transfer => Next(this with { Transfer = transfer })),
                OptionSpec.Of("Format", static (getter, name, plugIn, initial, retain) => OptionType.List(toSeq(LutFormat.Items), static format => format.Map(
                    cube: "Cube", spi1d: "SPI 1D", spi3d: "SPI 3D", spiMatrix: "SPI Matrix", autodesk: "Autodesk", clf: "CLF", ctf: "CTF", hald: "Hald", keyShotLookup: "KeyShot LookUp"),
                    getter, name, plugIn, initial, retain),
                    Format, format => Next(this with { Format = format })));

        private static IO<StageStep<LutChoice, LutChoice>> Next(LutChoice choice) => IO.pure<StageStep<LutChoice, LutChoice>>(new StageStep<LutChoice, LutChoice>.Advance(choice));
    }
}
