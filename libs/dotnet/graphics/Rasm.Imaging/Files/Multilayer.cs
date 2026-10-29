using System.Drawing;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Enumerables;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ExrLayer(string Name, PixelFrame Frame, Seq<string> Channels) {
    public static ExrLayer Reciprocal(string name, PixelFrame distance) => new(name, ImageFile.Mapped(distance, static (depth, _) => new Vector4(1f / depth.X)), ["Z"]);
}

[SmartEnum]
public sealed partial class ChannelNaming {
    public static readonly ChannelNaming Blender = new("Combined", "Depth", "Position", Matrix4x4.Identity, Matrix4x4.Identity);
    public static readonly ChannelNaming Nuke = new("rgba", "z", "Pworld", new Matrix4x4(1f, 0f, 0f, 0f, 0f, 0f, -1f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 1f), Matrix4x4.CreateScale(1f, 1f, -1f));

    private readonly string beauty;
    private readonly string depth;
    private readonly string position;
    private readonly Matrix4x4 world;
    private readonly Matrix4x4 screen;

    public ExrLayer Beauty(PixelFrame frame, bool alpha) => new(beauty, frame, alpha ? ["R", "G", "B", "A"] : ["R", "G", "B"]);

    public ExrLayer Depth(PixelFrame distance) => new(depth, distance, ["Z"]);

    public ExrLayer Position(PixelFrame distance, Camera camera) =>
        new(position, ImageFile.Mapped(distance, (pixel, point) => new Vector4(camera.WorldPoint(point, pixel.X).Map(at => Vector3.Transform(at, world)).IfNone(new Vector3(float.NaN)), 1f)), ["X", "Y", "Z"]);

    public ExrLayer Normals(string name, PixelFrame normals, Option<Camera> view = default) =>
        view.Map(camera => Matrix4x4.CreateFromQuaternion(Quaternion.Conjugate(camera.Orientation)) * screen).IfNone(world) switch {
            var turn => new(name, ImageFile.Mapped(normals, (pixel, _) => new Vector4(Vector3.TransformNormal(pixel.AsVector3(), turn), 1f)), ["X", "Y", "Z"]),
        };
}

public abstract partial record ImageFile {
    public static PixelFrame Matte(PixelFrame ids, Map<ushort, Vector3> colors) {
        FrozenDictionary<float, Vector4> keyed = colors.AsEnumerable().ToFrozenDictionary(static entry => (float)entry.Key, static entry => new Vector4(entry.Value, 1f));
        return Mapped(ids, (pixel, _) => keyed.TryGetValue(pixel.X, out Vector4 color) ? color : Vector4.Zero);
    }

    internal static PixelFrame Mapped(PixelFrame source, Func<Vector4, Vector2, Vector4> map) =>
        new(source.Origin, source.Size, source.Extent, block => {
            Span2D<Vector4> mapped = MemoryMarshal.Cast<float, Vector4>(block).AsSpan2D(source.Size.Height, source.Size.Width);
            ReadOnlySpan2D<Vector4> input = source.View.Span;
            for (int y = 0; y < mapped.Height; y++)
                for (int x = 0; x < mapped.Width; x++)
                    mapped[y, x] = map(input[y, x], new Vector2(source.Origin.X + x + 0.5f, source.Origin.Y + y + 0.5f));
        });

    public sealed record Exr(Seq<ExrLayer> Layers, PixelType Sample, Compression Compression, Option<Gamut> Gamut, bool Multipart) : ImageFile {
        public override IO<Unit> Write(FileMetadata metadata, Stream stream) =>
            from canvas in IO.lift(Canvas(Layers.Map(static layer => layer.Frame), windowed: !Multipart))
            let attributes = metadata.Exr(Gamut)
            from image in IO.lift(() => new Image((Multipart ? Layers.Map(static layer => (layer.Name, Layers: Seq(layer))) : Seq((Name: "", Layers))).Map(part => Assembled(part.Name, part.Layers, canvas, attributes)).Strict()))
                .Catch(static error => error.HasException<ArgumentException>(), error => IO.fail<Image>(ImagingFault.Of(ImagingReason.Refused, Layers.Map(static layer => layer.Name), inner: error)))
            from result in IO.liftVAsync(environment => ExrFile.SaveToStreamAsync(image, stream, cancellationToken: environment.Token))
            from written in IO.lift(result.IsSuccess ? Fin.Succ(unit) : ImagingFault.Of(ImagingReason.Refused, result.Status, inner: Optional(result.Error).Map(Error.New)))
            select written;

        private Part Assembled(string name, Seq<ExrLayer> layers, PixelFrame canvas, Seq<HeaderAttribute> attributes) {
            Rectangle window = layers.Head.Map(static layer => layer.Frame.Window).IfNone(canvas.Window);
            Seq<ChannelBuffer> buffers = layers.Bind(Buffers).Strict();
            Header header = new(
                PartType.Scanline,
                new Box2i(window.Left, window.Top, window.Right - 1, window.Bottom - 1),
                buffers.Map(static buffer => new Channel(buffer.Name, buffer.PixelType)),
                compression: Compression is Compression.PXR24 && !layers.ForAll(Colored) ? Compression.ZIP : Compression,
                displayWindow: new Box2i(0, 0, canvas.Extent.Width - 1, canvas.Extent.Height - 1),
                name: name,
                chromaticities: Gamut.Map(static gamut => new Chromaticities(
                    (float)gamut.Rgb.ChromaticityR.X, (float)gamut.Rgb.ChromaticityR.Y, (float)gamut.Rgb.ChromaticityG.X, (float)gamut.Rgb.ChromaticityG.Y,
                    (float)gamut.Rgb.ChromaticityB.X, (float)gamut.Rgb.ChromaticityB.Y, (float)gamut.Rgb.WhitePoint.Chromaticity.X, (float)gamut.Rgb.WhitePoint.Chromaticity.Y)).ToNullable(),
                attributes: attributes);
            return new Part(header, [new FlatLevel(0, 0, header.DataWindow, buffers)], isComplete: true);
        }

        private Seq<ChannelBuffer> Buffers(ExrLayer layer) {
            int count = layer.Frame.Size.Width * layer.Frame.Size.Height;
            using SpanOwner<Vector4> pixels = SpanOwner<Vector4>.Allocate(count);
            if (Colored(layer))
                PixelFrame.Associated(MemoryMarshal.Cast<float, Vector4>(layer.Frame.Block), pixels.Span);
            else
                MemoryMarshal.Cast<float, Vector4>(layer.Frame.Block).CopyTo(pixels.Span);
            Span2D<float> samples = MemoryMarshal.Cast<Vector4, float>(pixels.Span).AsSpan2D(count, 4);
            ChannelBuffer[] buffers = new ChannelBuffer[layer.Channels.Count];
            for (int component = 0; component < buffers.Length; component++) {
                string name = layer.Name.Length is 0 ? layer.Channels[component] : $"{layer.Name}.{layer.Channels[component]}";
                buffers[component] = (Colored(layer) ? Sample : PixelType.Float) switch {
                    PixelType.Half => Buffer<Half>(name, PixelType.Half, samples.GetColumn(component)),
                    PixelType.UInt => Buffer<uint>(name, PixelType.UInt, samples.GetColumn(component)),
                    _ => Buffer<float>(name, PixelType.Float, samples.GetColumn(component)),
                };
            }
            return toSeq(buffers);
        }

        private static bool Colored(ExrLayer layer) => layer.Channels.ForAll(static channel => channel is "R" or "G" or "B" or "A");

        private static ChannelBuffer Buffer<T>(string name, PixelType type, RefEnumerable<float> column) where T : unmanaged, INumberBase<T> {
            using SpanOwner<float> plane = SpanOwner<float>.Allocate(column.Length);
            using SpanOwner<T> stored = SpanOwner<T>.Allocate(column.Length);
            column.CopyTo(plane.Span);
            TensorPrimitives.ConvertSaturating(plane.Span, stored.Span);
            return new ChannelBuffer(name, type, MemoryMarshal.AsBytes(stored.Span));
        }
    }
}
