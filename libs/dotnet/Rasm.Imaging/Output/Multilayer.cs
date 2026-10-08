using System.Drawing;
using System.Numerics;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using TinyEXR;

namespace Rasm.Imaging.Output;

// --- [MODELS] --------------------------------------------------------------------------
internal readonly record struct ExrSample(OutputDepth Depth, PixelType Type);

public sealed record ExrLayout {
    private ExrLayout(ExrSample sample, ExrCompression compression, Option<Gamut> gamut) => (Sample, Compression, Gamut) = (sample, compression, gamut);

    public ExrCompression Compression { get; }
    public Option<Gamut> Gamut { get; }
    internal ExrSample Sample { get; }

    internal static ExrSample Data { get; } = new(OutputDepth.Float32, PixelType.Float);

    public static ExrLayout Coordinates { get; } = new(Data, ExrCompression.Zip, None);

    public static Seq<OutputDepth> Depths => toSeq(OutputDepth.Items).Filter(static depth => Table(depth).IsSome);

    public static Fin<ExrLayout> From(OutputDepth depth, ExrCompression compression, Gamut gamut) =>
        Table(depth).Map(sample => new ExrLayout(sample, compression, Some(gamut))).ToFin(new DepthRefused(depth, Depths));

    internal static Option<ExrSample> Table(OutputDepth depth) =>
        depth.Map<Option<ExrSample>>(uInt8: None, uInt10: None, uInt12: None, uInt14: None, uInt16: None, float16: new ExrSample(depth, PixelType.Half), float32: Data);
}

[SmartEnum<string>]
[ValidationError<InvalidOutput>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExrCompression {
    public static readonly ExrCompression Uncompressed = new("uncompressed", Compression.None, Compression.None);
    public static readonly ExrCompression Rle = new("rle", Compression.RLE, Compression.RLE);
    public static readonly ExrCompression Zips = new("zips", Compression.ZIPS, Compression.ZIPS);
    public static readonly ExrCompression Zip = new("zip", Compression.ZIP, Compression.ZIP);
    public static readonly ExrCompression Piz = new("piz", Compression.PIZ, Compression.PIZ);
    public static readonly ExrCompression Pxr24 = new("pxr24", Compression.PXR24, Compression.ZIP);
    public static readonly ExrCompression B44 = new("b44", Compression.B44, Compression.ZIP);
    public static readonly ExrCompression B44A = new("b44a", Compression.B44A, Compression.ZIP);

    public Compression Codec { get; }
    public Compression Data { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidOutput>]
public abstract partial class PassKind {
    public static readonly PassKind Rgba = new ColorPass("rgba", Seq("R", "G", "B", "A"));
    public static readonly PassKind Rgb = new ColorPass("rgb", Seq("R", "G", "B"));
    public static readonly PassKind Xyz = new DataPass("xyz", Seq("X", "Y", "Z"));
    public static readonly PassKind Z = new DataPass("z", Seq("Z"));
    public static readonly PassKind V = new DataPass("v", Seq("V"));

    public Seq<string> Channels { get; }

    internal abstract ExrSample Sample(ExrLayout layout);
    internal abstract Compression Codec(ExrCompression compression, Compression held);
    internal abstract void Associate(Span<Vector4> row);

    private sealed class ColorPass(string key, Seq<string> channels) : PassKind(key, channels) {
        internal override ExrSample Sample(ExrLayout layout) => layout.Sample;
        internal override Compression Codec(ExrCompression compression, Compression held) => held;
        internal override void Associate(Span<Vector4> row) => StillWriter.Associate(row, row);
    }

    private sealed class DataPass(string key, Seq<string> channels) : PassKind(key, channels) {
        internal override ExrSample Sample(ExrLayout layout) => ExrLayout.Data;
        internal override Compression Codec(ExrCompression compression, Compression held) => compression.Data;
        internal override void Associate(Span<Vector4> row) { }
    }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidOutput>]
public sealed partial class PassName {
    private const int NameBytes = 255;

    internal static PassName Combined { get; } = new("Combined");
    internal static PassName Depth { get; } = new("Depth");
    internal static PassName Position { get; } = new("Position");
    internal static PassName Rgba { get; } = new("rgba");
    internal static PassName Z { get; } = new("z");
    internal static PassName Pworld { get; } = new("Pworld");

    static partial void ValidateFactoryArguments(ref InvalidOutput? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.Contains('\0', StringComparison.Ordinal) && Encoding.UTF8.GetByteCount(value) + 2 <= NameBytes ? null : new InvalidOutput();
}

public sealed record IdentityLayer(ushort Id, PassName Name, Vector3 Colour) {
    internal static Action<ReadOnlySpan<Vector4>, Span<Vector4>> Fill(Seq<IdentityLayer> entries) {
        FrozenDictionary<float, Vector4> colours = entries.DistinctBy(static entry => entry.Id).ToFrozenDictionary(static entry => (float)entry.Id, static entry => new Vector4(entry.Colour, 1f));
        return (ids, matte) => {
            for (int x = 0; x < ids.Length; x++)
                matte[x] = colours.GetValueOrDefault(ids[x].X, Vector4.Zero);
        };
    }
}

internal readonly record struct ExrLayer(Option<PassName> Name, PassKind Kind, PixelFrame Source, Action<ReadOnlySpan<Vector4>, Point, Span<Vector4>> Row) {
    internal static Action<ReadOnlySpan<Vector4>, Point, Span<Vector4>> Held { get; } = static (source, _, part) => source.CopyTo(part);
}

[SmartEnum]
public sealed partial class NormalSpace {
    public static readonly NormalSpace World = new();
    public static readonly NormalSpace View = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReaderConvention {
    private ReaderConvention(NormalSpace normals) => Normals = normals;

    public NormalSpace Normals { get; }

    internal abstract PassName Beauty { get; }
    internal abstract PassName Depth { get; }
    internal abstract PassName Position { get; }

    internal abstract Vector3 Axes(Vector3 world);
    internal abstract Vector3 Screen(Vector3 view);

    internal Func<Vector3, Vector3> Normal(Camera camera) =>
        Quaternion.Conjugate(camera.Orientation) switch {
            var toView => Normals.Map(world: Axes, view: normal => Screen(Vector3.Transform(normal, toView))),
        };

    public sealed record Blender(NormalSpace Normals) : ReaderConvention(Normals) {
        internal override PassName Beauty => PassName.Combined;
        internal override PassName Depth => PassName.Depth;
        internal override PassName Position => PassName.Position;

        internal override Vector3 Axes(Vector3 world) => world;
        internal override Vector3 Screen(Vector3 view) => view;
    }

    public sealed record Nuke(NormalSpace Normals) : ReaderConvention(Normals) {
        internal override PassName Beauty => PassName.Rgba;
        internal override PassName Depth => PassName.Z;
        internal override PassName Position => PassName.Pworld;

        internal override Vector3 Axes(Vector3 world) => new(world.X, world.Z, -world.Y);
        internal override Vector3 Screen(Vector3 view) => new(view.X, view.Y, -view.Z);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExrPass {
    internal abstract Seq<ExrLayer> Layers(ReaderConvention convention, Camera camera);

    public sealed record Depth(PixelFrame Frame) : ExrPass {
        internal override Seq<ExrLayer> Layers(ReaderConvention convention, Camera camera) => Seq(new ExrLayer(convention.Depth, PassKind.Z, Frame, ExrLayer.Held));
    }

    public sealed record InverseDepth(PassName Name, PixelFrame Distance) : ExrPass {
        internal override Seq<ExrLayer> Layers(ReaderConvention convention, Camera camera) =>
            Seq(new ExrLayer(Name, PassKind.Z, Distance, static (source, _, part) => {
                for (int x = 0; x < source.Length; x++)
                    part[x] = new Vector4(1f / source[x].X);
            }));
    }

    public sealed record Position(PixelFrame Distance) : ExrPass {
        internal override Seq<ExrLayer> Layers(ReaderConvention convention, Camera camera) =>
            Seq(new ExrLayer(convention.Position, PassKind.Xyz, Distance, (source, first, part) => {
                for (int x = 0; x < source.Length; x++)
                    part[x] = new Vector4(convention.Axes(camera.WorldPoint(new Vector2(first.X + x + 0.5f, first.Y + 0.5f), source[x].X)), 1f);
            }));
    }

    public sealed record Normal(PassName Name, PixelFrame Frame) : ExrPass {
        internal override Seq<ExrLayer> Layers(ReaderConvention convention, Camera camera) =>
            convention.Normal(camera) switch {
                var turned => Seq(new ExrLayer(Name, PassKind.Xyz, Frame, (source, _, part) => {
                    for (int x = 0; x < source.Length; x++)
                        part[x] = new Vector4(turned(source[x].AsVector3()), 1f);
                })),
            };
    }

    public sealed record Identities(PassName Name, PixelFrame Ids, Seq<IdentityLayer> Entries) : ExrPass {
        internal override Seq<ExrLayer> Layers(ReaderConvention convention, Camera camera) =>
            (Name, Entries).Cons(Entries.Map(static entry => (entry.Name, Entries: Seq(entry))))
                .Map(layer => IdentityLayer.Fill(layer.Entries) switch {
                    var fill => new ExrLayer(layer.Name, PassKind.Rgba, Ids, (source, _, part) => fill(source, part)),
                });
    }

    public sealed record Named(PassName Name, PassKind Kind, PixelFrame Frame) : ExrPass {
        internal override Seq<ExrLayer> Layers(ReaderConvention convention, Camera camera) => Seq(new ExrLayer(Name, Kind, Frame, ExrLayer.Held));
    }
}

[SmartEnum]
public sealed partial class PassLayout {
    public static readonly PassLayout Parts = new();
    public static readonly PassLayout Interleaved = new();
}

public sealed record PassSet {
    private PassSet(ReaderConvention convention, Camera camera, PassLayout layout, Seq<ExrPass> passes) => (Convention, Camera, Layout, Passes) = (convention, camera, layout, passes);

    public ReaderConvention Convention { get; }
    public Camera Camera { get; }
    public PassLayout Layout { get; }
    public Seq<ExrPass> Passes { get; }

    internal Seq<ExrLayer> Layers => Passes.Bind(pass => pass.Layers(Convention, Camera));

    public static Validation<Error, PassSet> From(ReaderConvention convention, Camera camera, PassLayout layout, Seq<ExrPass> passes) =>
        new PassSet(convention, camera, layout, passes) switch {
            var set => toSeq(convention.Beauty.Cons(set.Layers.Map(static layer => layer.Name).Somes()).CountBy(static name => name))
                .Traverse(static count => count.Value == 1 ? Validation.Success<Error, Unit>(unit) : Validation.Fail<Error, Unit>(new PassTaken(count.Key)))
                .As()
                .Map(_ => set),
        };

    internal Validation<Error, Unit> Fits(PixelFrame beauty) =>
        beauty.Cons(Layers.Map(static layer => layer.Source))
            .Map(static (frame, index) => (Frame: frame, Index: index))
            .Traverse(entry => entry.Frame.Extent == Camera.Extent && Layout.Map(parts: true, interleaved: entry.Frame.Window == beauty.Window)
                ? Validation.Success<Error, Unit>(unit)
                : Validation.Fail<Error, Unit>(new LayerWindowRefused(entry.Index, entry.Frame.Window, entry.Frame.Extent)))
            .As()
            .Map(static _ => unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ExrParts {
    internal static IO<Unit> Write(StillFormat.ExrRaster exr, FileMetadata metadata, FileStream stream) {
        static Seq<ChannelBuffer> Buffers(ExrLayout layout, ExrLayer layer) {
            ExrSample sample = layer.Kind.Sample(layout);
            int plane = layer.Source.Size.Width * layer.Source.Size.Height * sample.Depth.SampleBytes;
            using MemoryOwner<byte> planes = MemoryOwner<byte>.Allocate(layer.Kind.Channels.Count * plane);
            StillWriter.Planar(sample.Depth, layer.Source.Size, layer.Kind.Channels.Count, (y, row) => {
                layer.Row(layer.Source.View.Span.GetRowSpan(y), new Point(layer.Source.Origin.X, layer.Source.Origin.Y + y), row);
                layer.Kind.Associate(row);
            }, planes.Span);
            return layer.Kind.Channels
                .Map((letter, lane) => new ChannelBuffer(layer.Name.Match(Some: name => $"{name}.{letter}", None: () => letter), sample.Type, planes.Span.Slice(lane * plane, plane)))
                .Strict();
        }

        static Part Part(StillFormat.ExrRaster exr, Seq<HeaderAttribute> attributes, Option<PassName> name, Rectangle window, Seq<ExrLayer> layers) {
            Seq<ChannelBuffer> buffers = layers.Bind(layer => Buffers(exr.Layout, layer)).Strict();
            Header header = new(
                PartType.Scanline,
                new Box2i(window.Left, window.Top, window.Right - 1, window.Bottom - 1),
                buffers.Map(static buffer => new Channel(buffer.Name, buffer.PixelType)),
                compression: layers.Fold(exr.Layout.Compression.Codec, (codec, layer) => layer.Kind.Codec(exr.Layout.Compression, codec)),
                displayWindow: new Box2i(0, 0, exr.Beauty.Extent.Width - 1, exr.Beauty.Extent.Height - 1),
                name: name.Map(static part => (string)part).ValueUnsafe(),
                chromaticities: exr.Layout.Gamut.Map(static gamut => gamut.Chromaticities).ToNullable(),
                attributes: attributes);
            return new Part(header, [new FlatLevel(0, 0, header.DataWindow, buffers)], isComplete: true);
        }

        static Seq<Part> Parts(StillFormat.ExrRaster exr, Seq<HeaderAttribute> attributes) {
            PassKind beauty = exr.Alpha ? PassKind.Rgba : PassKind.Rgb;
            return exr.Passes.Match(
                Some: set => set.Layout.Switch(
                    (Exr: exr, Attributes: attributes, Layers: new ExrLayer(set.Convention.Beauty, beauty, exr.Beauty, ExrLayer.Held).Cons(set.Layers)),
                    parts: static state => state.Layers.Map(layer => Part(state.Exr, state.Attributes, layer.Name, layer.Source.Window, Seq(layer))),
                    interleaved: static state => Seq(Part(state.Exr, state.Attributes, None, state.Exr.Beauty.Window, state.Layers))),
                None: () => Seq(Part(exr, attributes, None, exr.Beauty.Window, Seq(new ExrLayer(None, beauty, exr.Beauty, ExrLayer.Held)))));
        }

        return from image in IO.lift(() => new Image(Parts(exr, metadata.Exr(exr.Layout.Gamut))))
               from token in cancelToken
               from result in IO.liftVAsync(() => ExrFile.SaveToStreamAsync(image, stream, cancellationToken: token))
               from _ in IO.lift(CodecRefused.Unless(result))
               select unit;
    }
}
