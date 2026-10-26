using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Darkroom.Chain;
using Darkroom.Queue;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Rhino;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using SkiaSharp;

namespace Darkroom.Output;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IdentitySource(ushort Id, Option<string> Name, Color Color);

[SmartEnum<RenderWindow.StandardChannels>(SkipIComparable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
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
        Some<Func<RhinoDoc, Seq<IdentitySource>>>(static document => toSeq(document.Objects.GetObjectList(ObjectType.AnyObject)).Map(found =>
            new IdentitySource((ushort)(RhinoMath.CRC32(0u, found.Id.ToByteArray()) & IdMask), Conversions.Present(found.Attributes.Name), found.Attributes.DrawColor(document))).Strict()),
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel WireframePoints = new(RenderWindow.StandardChannels.WireframePointsRGBA, nameof(RenderWindow.StandardChannels.WireframePointsRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel WireframeIsocurves = new(RenderWindow.StandardChannels.WireframeIsocurvesRGBA, nameof(RenderWindow.StandardChannels.WireframeIsocurvesRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel WireframeCurves = new(RenderWindow.StandardChannels.WireframeCurvesRGBA, nameof(RenderWindow.StandardChannels.WireframeCurvesRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel WireframeAnnotations = new(RenderWindow.StandardChannels.WireframeAnnotationsRGBA, nameof(RenderWindow.StandardChannels.WireframeAnnotationsRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));

    public string PartName { get; }

    public Option<Func<RhinoDoc, Seq<IdentitySource>>> Sources { get; }

    [UseDelegateFromConstructor]
    internal partial ExrPass Part(PassName name, PixelFrame frame);

    // --- [PARTS]
    public IO<Seq<ExrPass>> Passes(RhinoDoc doc, PixelFrame frame) =>
        from name in IO.lift(PassName.Validated(PartName))
        from identities in IO.lift(Sources.Traverse(sources => Identified(frame, sources(doc), Some(SaveSource.SceneLinear))).As())
        from key in IO.lift(PassName.Validated(Key.ToString()))
        from inverse in IO.lift(PassName.Validated(nameof(ExrPass.InverseDepth)))
        select Part(name, frame).Cons(identities.Map<ExrPass>(entries => new ExrPass.Identities(key, frame, entries)).ToSeq())
            + (this == Distance ? Seq<ExrPass>(new ExrPass.InverseDepth(inverse, frame)) : []);

    public IO<DocumentLayer> Layer(RhinoDoc doc, PixelFrame frame, Option<ColorEncoding> encoding) =>
        from name in IO.lift(PassName.Validated(PartName))
        from layer in IO.lift(Sources.Match(
            Some: sources => Identified(frame, sources(doc), encoding).Map(entries => DocumentLayer.Identities(name, frame, entries)),
            None: () => Fin.Succ<DocumentLayer>(new DocumentLayer.Raster(name, frame, BlendingMode.Mix, 1f, Visible: false, Mask: None))))
        select layer;

    private static Fin<Seq<IdentityLayer>> Identified(PixelFrame ids, Seq<IdentitySource> sources, Option<ColorEncoding> encoding) {
        System.Collections.Generic.HashSet<ushort> present = [];
        foreach (Vector4 pixel in MemoryMarshal.Cast<float, Vector4>(ids.Block))
            _ = present.Add((ushort)pixel.X);
        Seq<Action<Span<Vector4>>> steps = encoding.Map(DisplayDevice.Srgb.Encoding.To).IfNone([]);
        return Callbacks.Each(
            toSeq(sources
                .Filter(source => present.Contains(source.Id))
                .Fold(Map<ushort, (Color Color, Seq<string> Names)>(), static (held, source) =>
                    held.AddOrUpdate(source.Id, same => (same.Color, same.Names + source.Name.ToSeq()), (source.Color, source.Name.ToSeq())))
                .AsIterable()),
            (entry, _) => PassName.Validated(entry.Value.Names.IsEmpty ? entry.Key.ToString(CultureInfo.InvariantCulture) : string.Join(", ", entry.Value.Names))
                .Map(layer => new IdentityLayer(entry.Key, layer, Encoded(entry.Value.Color))));

        Vector3 Encoded(Color color) {
            Vector4[] pixel = [new Vector4(color.R, color.G, color.B, byte.MaxValue) / byte.MaxValue];
            _ = steps.Iter(step => step(pixel));
            return pixel[0].AsVector3();
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SaveSource {
    private static readonly IProgress<int> Unreported = new Progress<int>();

    public static ColorEncoding SceneLinear { get; } = new(DisplayFormation.Working, new Transfer.Linear());

    public abstract ChainScene Scene { get; }

    public abstract IO<PixelFrame> Linear { get; }

    public abstract IO<(PixelFrame Frame, Option<ColorEncoding> Encoding)> Formed(OutputTarget target, OutputDepth depth);

    public abstract IO<HashMap<SavedChannel, PixelFrame>> Channels(OutputTarget target);

    // --- [WRITES]
    public IO<Unit> Write(RhinoDoc doc, FileMetadata metadata, OutputTarget target, OutputPath path, bool overwrite) =>
        target.FileFormat.Still(doc, this, target).Bind(still => FilePublication.Write(path, overwrite, stream => StillWriter.Encode(still, metadata, stream)));

    public IO<Seq<(OutputName Name, OutputPath Path)>> Saved(RhinoDoc doc, IPlugInViews views) =>
        from book in OutputBook.Read(doc)
        from named in IO.lift(book.Names([]))
        from placed in Destinations.Resolve(doc, book.Destination, named.Map(static held => held.Name))
        from metadata in Metadata(doc, views)
        from written in named.Zip(placed).TraverseM(pair => Write(doc, metadata, pair.First.Target, pair.Second.Path, book.Destination.Overwrite)).As()
        select placed;

    public static IO<FileMetadata> Metadata(RhinoDoc doc, IPlugInViews views) =>
        from output in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), ImageOutputState.Read)
        from created in IO.lift(() => views.Clock.ToZonedClock().GetCurrentZonedDateTime().ToDateTimeOffset())
        from metadata in IO.lift(() => (
                SoftwareName.Validated(views.PlugIn.Name),
                PixelsPerInch.Validated((double)output.ImageDpi),
                Field(nameof(views.PlugIn.Version), views.PlugIn.Version),
                Field(nameof(views.PlugIn.Id), views.PlugIn.Id.ToString("D", CultureInfo.InvariantCulture)))
            .Apply((software, density, version, id) => new FileMetadata(software, created, density, toMap(Seq(version, id))))
            .As())
        select metadata;

    private static Fin<(MetadataName Name, MetadataText Text)> Field(string name, string text) =>
        (MetadataName.Validated(name), MetadataText.Validated(text)).Apply(static (field, value) => (field, value)).As();

    // --- [STILLS]
    public IO<StillFormat> Passes(RhinoDoc doc, OutputTarget target, LayerReader reader) =>
        from beauty in Linear
        from channels in Channels(target)
        from layout in IO.lift(ExrLayout.From(target.ColorDepth, target.Codec, DisplayFormation.Working))
        from position in IO.lift(target.Position
            ? (channels.Find(SavedChannel.Distance).ToFin(new Refused(nameof(SavedChannel.Distance), Refused.Absent)), Scene.Meters.ToFin(new Refused(nameof(ChainScene.Meters), Refused.Absent)))
                .Apply(static (distance, meters) => Some<ExrPass>(new ExrPass.Position(GuideChannel.Depth.Measured(distance, meters))))
                .As()
            : Fin.Succ(Option<ExrPass>.None))
        from parts in Held(channels).TraverseM(held => held.Channel.Passes(doc, held.Frame)).As()
        from set in IO.lift(PassSet.From(reader.Convention, Scene.Camera, PassLayout.Parts, parts.Flatten() + position.ToSeq()))
        from still in IO.lift(StillFormat.ExrRaster.From(beauty, layout, target.Alpha, Some(set)))
        select still;

    public IO<StillFormat> Layered(RhinoDoc doc, OutputTarget target) =>
        from beauty in target.ColorDepth.Levels.IsSome ? Formed(target, target.ColorDepth) : Linear.Map(static frame => (Frame: frame, Encoding: Some(SceneLinear)))
        from channels in Channels(target)
        from layers in Held(channels).Filter(held => target.ColorDepth.Levels.IsNone || held.Channel.Sources.IsSome)
            .TraverseM(held => held.Channel.Layer(doc, held.Frame, beauty.Encoding)).As()
        from name in IO.lift(PassName.Validated(nameof(RenderWindow.StandardChannels.RGBA)))
        from document in IO.lift(LayeredDocument.From(target.ColorDepth, beauty.Encoding,
            new DocumentLayer.Raster(name, beauty.Frame, BlendingMode.Mix, 1f, Visible: true, Mask: None).Cons(layers)))
        select (StillFormat)new StillFormat.Document(document);

    private static Seq<(SavedChannel Channel, PixelFrame Frame)> Held(HashMap<SavedChannel, PixelFrame> channels) =>
        toSeq(SavedChannel.Items).Choose(channel => channels.Find(channel).Map(frame => (channel, frame)));

    // --- [CASES]
    public sealed record Window(RenderWindow Rendering, ChainScene Framing, OverscanFrame Frame, Option<RenderRegion> Region) : SaveSource {
        public override ChainScene Scene => Framing;

        public override IO<PixelFrame> Linear => Read(RenderWindow.StandardChannels.RGBA);

        public override IO<(PixelFrame Frame, Option<ColorEncoding> Encoding)> Formed(OutputTarget target, OutputDepth depth) =>
            depth.Levels.IsSome
                ? DisposalOps.Found(Rendering.GetBitmap, nameof(RenderWindow.GetBitmap), static bitmap => DisposalOps.Within(Conversions.ToSKImage(bitmap), static image => IO.lift(() => Pixels(image)), static image => IO.lift(image.Dispose)))
                    .Bind(read => IO.lift(Placed(read.Size, read.Pixels)))
                    .Map(static frame => (frame, Option<ColorEncoding>.None))
                : Linear.Map(static frame => (frame, Some(SceneLinear)));

        public override IO<HashMap<SavedChannel, PixelFrame>> Channels(OutputTarget target) =>
            from state in RenderWindows.ChannelState(Rendering)
            let requested = toHashSet(state.Requested)
            from frames in toSeq(SavedChannel.Items)
                .Filter(channel => state.Available.Contains(channel.Key) && (requested.Contains(channel.Key) || (target.Position && channel == SavedChannel.Distance)))
                .TraverseM(channel => Read(channel.Key).Map(frame => (channel, frame)))
                .As()
            select toHashMap(frames);

        private IO<PixelFrame> Read(RenderWindow.StandardChannels channel) =>
            RenderWindows.Read(Rendering, channel).Bind(read => IO.lift(Placed(read.Size, read.Pixels)));

        private static (Size Size, float[] Pixels) Pixels(SKImage image) {
            using SKBitmap bitmap = new(new SKImageInfo(image.Width, image.Height, SKColorType.RgbaF32, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb()));
            _ = image.ReadPixels(bitmap.PeekPixels());
            return (new Size(image.Width, image.Height), MemoryMarshal.Cast<byte, float>(bitmap.GetPixelSpan()).ToArray());
        }

        private Fin<PixelFrame> Placed(Size size, float[] pixels) =>
            Region.Map(region => region.Pixels(Frame.Frame)).IfNone(Frame.DataWindow) switch {
                var window => Refused.Unless(size == new Size(Frame.Padded.Width, Frame.Padded.Height), nameof(RenderWindows.Read))
                    .Bind(_ => PixelExtent.Validated(window.Width, window.Height))
                    .Map(extent => new PixelFrame(window.Location, extent, Frame.Frame, block =>
                        MemoryMarshal.Cast<float, Vector4>(pixels.AsSpan()).AsSpan2D(size.Height, size.Width)
                            .Slice(window.Y + Frame.Pad.Height, window.X + Frame.Pad.Width, extent.Height, extent.Width)
                            .CopyTo(MemoryMarshal.Cast<float, Vector4>(block.AsSpan()).AsSpan2D(extent.Height, extent.Width)))),
            };
    }

    public sealed record Run(RenderRun Captured, ChainState State) : SaveSource {
        public override ChainScene Scene => Captured.Scene;

        public override IO<PixelFrame> Linear => IO.pure(Captured.Input);

        public override IO<(PixelFrame Frame, Option<ColorEncoding> Encoding)> Formed(OutputTarget target, OutputDepth depth) =>
            Captured.After.IsEmpty
                ? DisposalOps.Within(static () => new Derivations(), derivations =>
                    from formed in new ChainRequest(Captured.Input.Copy(), State).Formed(Captured.Context(derivations, depth.Levels.Map(_ => depth)) with { Display = target.DisplayDevice })
                    from shown in DisplayFormation.Displayed(formed.Context).TraverseM(pass => IO.lift(() => pass.Run(formed.Frame, Unreported))).As()
                    select (formed.Frame, Some(target.DisplayDevice.Encoding)))
                : target.DisplayDevice == DisplayFormation.Display
                    ? IO.pure((Captured.Formed, Some(DisplayFormation.Display.Encoding)))
                    : IO.fail<(PixelFrame Frame, Option<ColorEncoding> Encoding)>(new SaveRefused(Captured.After.Map(static effect => effect.Name), target.DisplayDevice));

        public override IO<HashMap<SavedChannel, PixelFrame>> Channels(OutputTarget target) =>
            IO.pure(toHashMap(toSeq(Captured.Guides.AsIterable()).Choose(static guide => SavedChannel.TryGet(guide.Key.Key, out SavedChannel? channel) ? Some((channel, guide.Value)) : None)));
    }
}

// --- [ERRORS] --------------------------------------------------------------------------
public sealed record SaveRefused(Seq<string> After, DisplayDevice Display)
    : Expected($"post effects {string.Join(", ", After)} follow Darkroom, a capture is formed only for {DisplayFormation.Display} and not for {Display}", 0);
