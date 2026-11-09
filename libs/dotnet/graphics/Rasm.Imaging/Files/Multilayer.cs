using System.Drawing;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using TinyEXR;
using Configuration = SixLabors.ImageSharp.Configuration;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record ExrLayer(Option<string> Name, PixelFrame Frame, Seq<string> Channels) {
    public static ExrLayer Reciprocal(string name, PixelFrame distance) => new(name, ImageFile.Mapped(distance, static (depth, _) => new Vector4(1f / depth.X)), ["Z"]);
}

public sealed record ChannelNaming(string BeautyLayer, string DepthLayer, string PositionLayer, Matrix4x4 FromZUp, Matrix4x4 ViewAxes) {
    public static ChannelNaming Blender { get; } = new("Combined", "Depth", "Position", Matrix4x4.Identity, Matrix4x4.Identity);
    public static ChannelNaming Nuke { get; } =
        new("rgba", "z", "Pworld", new Matrix4x4(1f, 0f, 0f, 0f, 0f, 0f, -1f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 1f), Matrix4x4.CreateScale(1f, 1f, -1f));

    public ExrLayer Beauty(PixelFrame frame, bool alpha) => new(BeautyLayer, frame, alpha ? ["R", "G", "B", "A"] : ["R", "G", "B"]);

    public ExrLayer Depth(PixelFrame distance) => new(DepthLayer, distance, ["Z"]);

    public ExrLayer Position(PixelFrame distance, Camera camera) =>
        new(PositionLayer, ImageFile.Mapped(distance, (pixel, point) => new Vector4(camera.WorldPoint(point, pixel.X).Map(at => Vector3.Transform(at, FromZUp)).IfNone(new Vector3(float.NaN)), 1f)), ["X", "Y", "Z"]);

    public ExrLayer Normals(string name, PixelFrame normals, Option<Camera> view = default) =>
        view.Map(camera => Matrix4x4.CreateFromQuaternion(Quaternion.Conjugate(camera.Orientation)) * ViewAxes).IfNone(FromZUp) switch {
            var turn => new(name, ImageFile.Mapped(normals, (pixel, _) => new Vector4(Vector3.TransformNormal(pixel.AsVector3(), turn), 1f)), ["X", "Y", "Z"]),
        };
}

[SmartEnum<string>]
public sealed partial class ExrCompression {
    public static readonly ExrCompression None = new("none", Compression.None);
    public static readonly ExrCompression Rle = new("rle", Compression.RLE);
    public static readonly ExrCompression Zips = new("zips", Compression.ZIPS);
    public static readonly ExrCompression Zip = new("zip", Compression.ZIP);
    public static readonly ExrCompression Piz = new("piz", Compression.PIZ);
    public static readonly ExrCompression Pxr24 = new("pxr24", Compression.PXR24);
    public static readonly ExrCompression B44 = new("b44", Compression.B44);
    public static readonly ExrCompression B44A = new("b44a", Compression.B44A);
    public static readonly ExrCompression Dwaa = new("dwaa", Compression.DWAA);
    public static readonly ExrCompression Dwab = new("dwab", Compression.DWAB);
    public static readonly ExrCompression Htj2k256 = new("htj2k256", Compression.HTJ2K256);
    public static readonly ExrCompression Htj2k32 = new("htj2k32", Compression.HTJ2K32);

    internal Compression Method { get; }
}

public abstract partial record ImageFile {
    public static PixelFrame Matte(PixelFrame ids, Map<ushort, Vector3> colors) {
        FrozenDictionary<float, Vector4> keyed = colors.AsEnumerable().ToFrozenDictionary(static entry => (float)entry.Key, static entry => new Vector4(entry.Value, 1f));
        return Mapped(ids, (pixel, _) => keyed.TryGetValue(pixel.X, out Vector4 color) ? color : Vector4.Zero);
    }

    internal static PixelFrame Mapped(PixelFrame source, Func<Vector4, Vector2, Vector4> map) {
        PixelFrame mapped = new(source.Origin, source.Size, source.Extent);
        mapped.Rows((row, x, y) => {
            ReadOnlySpan<Vector4> input = source.View.Span.GetRowSpan(y - source.Origin.Y);
            for (int column = 0; column < row.Length; column++)
                row[column] = map(input[column], new Vector2(x + column + 0.5f, y + 0.5f));
        });
        return mapped;
    }

    public sealed record Exr(Seq<ExrLayer> Layers, OutputDepth Depth, ExrCompression Codec, Option<Gamut> Gamut, bool Multipart) : ImageFile {
        public override IO<Unit> Write(FileMetadata metadata, Stream stream, Option<Configuration> configuration = default) =>
            from floating in IO.lift(ImagingFault.Unless(Depth.Levels.IsNone, ImagingReason.Unsupported, Depth))
            from canvas in IO.lift(Canvas(Layers.Map(static layer => layer.Frame), windowed: !Multipart))
            let attributes = metadata.Exr(Gamut)
            from image in IO.lift(() => new Image((Multipart ? Layers.Map(static layer => (layer.Name, Layers: Seq(layer))) : Seq((Name: Option<string>.None, Layers))).Map(part => Assembled(part.Name, part.Layers, canvas, attributes)).Strict()))
                .Catch(static error => error.HasException<ArgumentException>(), error => IO.fail<Image>(ImagingFault.Of(ImagingReason.Refused, Layers.Map(static layer => layer.Name), inner: error)))
            from result in IO.liftVAsync(environment => ExrFile.SaveToStreamAsync(image, stream, cancellationToken: environment.Token))
            from written in IO.lift(result.IsSuccess ? Fin.Succ(unit) : ImagingFault.Of(ImagingReason.Refused, result.Status, inner: Optional(result.Error).Map(Error.New)))
            select written;

        private Part Assembled(Option<string> name, Seq<ExrLayer> layers, PixelFrame canvas, Seq<HeaderAttribute> attributes) {
            Rectangle window = layers.Head.Map(static layer => layer.Frame.Window).IfNone(canvas.Window);
            Seq<ChannelBuffer> buffers = layers.Bind(Buffers).Strict();
            Header header = new(
                PartType.Scanline,
                new Box2i(window.Left, window.Top, window.Right - 1, window.Bottom - 1),
                buffers.Map(static buffer => new Channel(buffer.Name, buffer.PixelType)),
                compression: Codec == ExrCompression.Pxr24 && !layers.ForAll(Colored) ? Compression.ZIP : Codec.Method,
                displayWindow: new Box2i(0, 0, canvas.Extent.Width - 1, canvas.Extent.Height - 1),
                name: name.ValueUnsafe(),
                chromaticities: Gamut.Map(static gamut => new Chromaticities(
                    (float)gamut.Rgb.ChromaticityR.X, (float)gamut.Rgb.ChromaticityR.Y, (float)gamut.Rgb.ChromaticityG.X, (float)gamut.Rgb.ChromaticityG.Y,
                    (float)gamut.Rgb.ChromaticityB.X, (float)gamut.Rgb.ChromaticityB.Y, (float)gamut.Rgb.WhitePoint.Chromaticity.X, (float)gamut.Rgb.WhitePoint.Chromaticity.Y)).ToNullable(),
                attributes: attributes);
            return new Part(header, [new FlatLevel(0, 0, header.DataWindow, buffers)], isComplete: true);
        }

        private Seq<ChannelBuffer> Buffers(ExrLayer layer) {
            int count = layer.Frame.Size.Width * layer.Frame.Size.Height;
            bool colored = Colored(layer);
            using SpanOwner<Vector4> associated = colored ? SpanOwner<Vector4>.Allocate(count) : default;
            ReadOnlySpan<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(layer.Frame.Block);
            if (colored) {
                PixelFrame.Associated(pixels, associated.Span);
                pixels = associated.Span;
            }
            ReadOnlySpan2D<float> samples = MemoryMarshal.Cast<Vector4, float>(pixels).AsSpan2D(count, 4);
            using SpanOwner<float> plane = SpanOwner<float>.Allocate(count);
            ChannelBuffer[] buffers = new ChannelBuffer[layer.Channels.Count];
            for (int component = 0; component < buffers.Length; component++) {
                string channel = layer.Channels[component];
                string name = layer.Name.Match(Some: named => $"{named}.{channel}", None: () => channel);
                samples.GetColumn(component).CopyTo(plane.Span);
                buffers[component] = colored && Depth == OutputDepth.Float16
                    ? Buffer<Half>(name, PixelType.Half, plane.Span)
                    : new ChannelBuffer(name, PixelType.Float, MemoryMarshal.AsBytes(plane.Span));
            }
            return toSeq(buffers);
        }

        private static bool Colored(ExrLayer layer) => layer.Channels.ForAll(static channel => channel is "R" or "G" or "B" or "A");

        private static ChannelBuffer Buffer<T>(string name, PixelType type, ReadOnlySpan<float> samples) where T : unmanaged, INumberBase<T> {
            using SpanOwner<T> stored = SpanOwner<T>.Allocate(samples.Length);
            TensorPrimitives.ConvertSaturating(samples, stored.Span);
            return new ChannelBuffer(name, type, MemoryMarshal.AsBytes(stored.Span));
        }
    }
}
