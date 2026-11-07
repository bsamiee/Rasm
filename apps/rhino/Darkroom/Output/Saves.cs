using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using Darkroom.Chain;
using Darkroom.Queue;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Rhino;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using SixLabors.ImageSharp.PixelFormats;

namespace Darkroom.Output;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record IdentitySource(ushort Id, Option<string> Name, Vector3 Color);

[SmartEnum<RenderWindow.StandardChannels>(SkipIComparable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SavedChannel {

    // --- [ITEMS]

    private const ushort IdMask = 0x7FFF;

    public static readonly SavedChannel Distance = new(RenderWindow.StandardChannels.DistanceFromCamera, "Depth", None,
        static (_, frame, reader, _) => Seq(reader.Naming.Depth(frame), ExrLayer.Reciprocal("InverseDepth", frame)));
    public static readonly SavedChannel Normals = new(RenderWindow.StandardChannels.NormalXYZ, "Normal", None,
        static (name, frame, reader, camera) => Seq(reader.Naming.Normals(name, frame, camera.Filter(_ => reader.View))));
    public static readonly SavedChannel Albedo = new(RenderWindow.StandardChannels.AlbedoRGB, "DiffCol", None, static (name, frame, _, _) => Seq(new ExrLayer(name, frame, ["R", "G", "B"])));
    public static readonly SavedChannel MaterialIds = new(RenderWindow.StandardChannels.MaterialIds, "IndexMA",
        Some<Func<RhinoDoc, Seq<IdentitySource>>>(static document => toSeq(document.RenderMaterials).Map(static material => {
            using Material simulated = material.ToMaterial(RenderTexture.TextureGeneration.Skip);
            return new IdentitySource((ushort)(material.RenderHash & IdMask), Conversions.Present(material.Name), Components(new Color4f(simulated.DiffuseColor)));
        }).Strict()),
        static (name, frame, _, _) => Seq(new ExrLayer(name, frame, ["V"])));
    public static readonly SavedChannel ObjectIds = new(RenderWindow.StandardChannels.ObjectIds, "IndexOB",
        Some<Func<RhinoDoc, Seq<IdentitySource>>>(static document => toSeq(document.Objects.GetObjectList(ObjectType.AnyObject)).Map(found =>
            new IdentitySource((ushort)(RhinoMath.CRC32(0u, found.Id.ToByteArray()) & IdMask), Conversions.Present(found.Attributes.Name), Components(new Color4f(found.Attributes.DrawColor(document))))).Strict()),
        static (name, frame, _, _) => Seq(new ExrLayer(name, frame, ["V"])));
    public static readonly SavedChannel WireframePoints = Wireframe(RenderWindow.StandardChannels.WireframePointsRGBA);
    public static readonly SavedChannel WireframeIsocurves = Wireframe(RenderWindow.StandardChannels.WireframeIsocurvesRGBA);
    public static readonly SavedChannel WireframeCurves = Wireframe(RenderWindow.StandardChannels.WireframeCurvesRGBA);
    public static readonly SavedChannel WireframeAnnotations = Wireframe(RenderWindow.StandardChannels.WireframeAnnotationsRGBA);

    public string PartName { get; }

    public Option<Func<RhinoDoc, Seq<IdentitySource>>> Sources { get; }

    [UseDelegateFromConstructor]
    public partial Seq<ExrLayer> Layers(string name, PixelFrame frame, LayerReader reader, Option<Camera> camera);

    private static SavedChannel Wireframe(RenderWindow.StandardChannels channel) =>
        new(channel, channel.ToString(), None, static (name, frame, _, _) => Seq(new ExrLayer(name, frame, ["R", "G", "B", "A"])));

    private static Vector3 Components(Color4f color) => new(color.R, color.G, color.B);

    // --- [IDENTITIES]

    public Map<ushort, IdentitySource> Identified(RhinoDoc doc, PixelFrame ids) =>
        Sources.Match(
            Some: sources => Present(ids) switch {
                var present => sources(doc).Filter(source => present.Contains(source.Id)).Fold(Map<ushort, IdentitySource>(), static (held, source) =>
                    held.AddOrUpdate(source.Id, same => same with { Name = Conversions.Present(string.Join(", ", same.Name.ToSeq() + source.Name.ToSeq())) }, source)),
            },
            None: static () => Map<ushort, IdentitySource>());

    private static HashSet<ushort> Present(PixelFrame ids) {
        HashSet<ushort> present = [];
        foreach (Vector4 pixel in ids.View.Span)
            present = present.TryAdd((ushort)pixel.X);
        return present;
    }
}

public sealed record SaveSource(RenderWindow Rendering, ChainScene Scene, OverscanFrame Frame, Option<RenderRegion> Region) {

    // --- [FRAMES]

    public IO<PixelFrame> Linear => Read(RenderWindow.StandardChannels.RGBA);

    public IO<(PixelFrame Frame, Option<ColorEncoding> Encoding)> Formed(OutputDepth depth) =>
        depth.Levels.IsSome
            ? (from bitmap in Products.Of(Rendering.GetBitmap)
               from frame in IO.lift(() => {
                   using Bitmap pixels = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format32bppArgb);
                   BitmapData data = pixels.LockBits(new(0, 0, pixels.Width, pixels.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                   try { return Placed(data); } finally { pixels.UnlockBits(data); }
               })
               select (frame, Option<ColorEncoding>.None)).Bracket()
            : Linear.Map(static frame => (frame, Some(ChainScene.Working)));

    public IO<Seq<(SavedChannel Channel, PixelFrame Frame)>> Channels(bool position) =>
        from state in RenderWindows.ChannelState(Rendering)
        let requested = toHashSet(state.Requested)
        from frames in toSeq(SavedChannel.Items)
            .Filter(channel => state.Available.Contains(channel.Key) && (requested.Contains(channel.Key) || (position && channel == SavedChannel.Distance)))
            .TraverseM(channel => Read(channel.Key).Map(frame => (channel, frame)))
            .As()
        select frames;

    // --- [SCENES]

    public static IO<PixelExtent> Extent(RhinoDoc doc) =>
        IO.lift(() => RenderPipeline.RenderSize(doc, fromRenderSources: true) switch { var size => PixelExtent.Validated(size.Width, size.Height) });

    // --- [WRITES]

    public IO<Seq<(OutputName Name, OutputPath Path)>> Saved(RhinoDoc doc, TimeProvider clock) =>
        from book in OutputBook.Read(doc)
        let named = book.Names([])
        from placed in Destinations.Resolve(doc, book.Folder, book.Stem, book.Overwrite, OutputConvention.Default, named.Map(static held => held.Name))
        from metadata in Metadata(doc, clock)
        from written in named.Zip(placed).TraverseM(pair => pair.First.Target.FileFormat.Write(doc, this, pair.First.Target, metadata, pair.Second.Path, book.Overwrite)).As()
        select placed;

    public static IO<FileMetadata> Metadata(RhinoDoc doc, TimeProvider clock) =>
        from density in RenderSettingsSources.Read(new RenderSettingsSource.Live(doc), static scope => IO.lift(() => scope.Settings.ImageDpi))
        from created in IO.lift(() => clock.ToZonedClock().GetCurrentZonedDateTime().ToDateTimeOffset())
        select new FileMetadata(Session.PlugIn.Name, created, density,
            toMap(Seq(("Version", Session.PlugIn.Version), ("Id", Session.PlugIn.Id.ToString("D", CultureInfo.InvariantCulture)))));

    // --- [PIXELS]

    private IO<PixelFrame> Read(RenderWindow.StandardChannels channel) =>
        RenderWindows.Read(Rendering, channel).Bind(read => IO.lift(Placed(read)));

    private Fin<PixelFrame> Placed(BitmapData pixels) =>
        Placed(pixels.Width, pixels.Height, (window, extent) => PixelFrame.From<Bgra32>(
            pixels.Scan0 + ((window.Y + Frame.Pad.Height) * pixels.Stride) + ((window.X + Frame.Pad.Width) * Unsafe.SizeOf<Bgra32>()),
            pixels.Stride, window.Location, extent, Frame.Frame));

    private Fin<PixelFrame> Placed(Memory2D<float> pixels) =>
        Placed(pixels.Width / 4, pixels.Height, (window, extent) => new PixelFrame(window.Location, extent, Frame.Frame, block =>
            pixels.Slice(window.Y + Frame.Pad.Height, 4 * (window.X + Frame.Pad.Width), extent.Height, 4 * extent.Width).Span.CopyTo(block.AsSpan2D(extent.Height, 4 * extent.Width))));

    private Fin<PixelFrame> Placed(int width, int height, Func<Rectangle, PixelExtent, PixelFrame> decode) =>
        Region.Map(region => region.Pixels(Frame.Frame)).IfNone(Frame.DataWindow) switch {
            var window => DarkroomFault.Unless(width == Frame.Padded.Width && height == Frame.Padded.Height, DarkroomReason.Invalid, (width, height))
                .Bind(_ => PixelExtent.Validated(window.Width, window.Height))
                .Map(extent => decode(window, extent)),
        };
}
