using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ExrLayout {
    private ExrLayout(PixelType sample, ExrCompression compression, Option<Gamut> gamut) => (Sample, Compression, Gamut) = (sample, compression, gamut);

    public ExrCompression Compression { get; }
    public Option<Gamut> Gamut { get; }
    internal PixelType Sample { get; }

    public static ExrLayout Coordinates { get; } = new(PixelType.Float, ExrCompression.Zip, None);

    public static Seq<OutputDepth> Depths => OutputDepth.Where(Table);

    public static Fin<ExrLayout> From(OutputDepth depth, ExrCompression compression, Gamut gamut) =>
        OutputDepth.Sampled("EXR", depth, Table).Map(sample => new ExrLayout(sample, compression, Some(gamut)));

    internal static Option<PixelType> Table(OutputDepth depth) =>
        depth.Map<Option<PixelType>>(uInt8: None, uInt10: None, uInt12: None, uInt14: None, uInt16: None, float16: PixelType.Half, float32: PixelType.Float);
}

[SmartEnum<string>]
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
public abstract partial class PassKind {
    public static readonly PassKind Rgba = new ColorPass("rgba", Seq("R", "G", "B", "A"));
    public static readonly PassKind Rgb = new ColorPass("rgb", Seq("R", "G", "B"));
    public static readonly PassKind Xyz = new DataPass("xyz", Seq("X", "Y", "Z"));
    public static readonly PassKind Z = new DataPass("z", Seq("Z"));
    public static readonly PassKind V = new DataPass("v", Seq("V"));

    public Seq<string> Channels { get; }

    internal abstract PixelType Sample(ExrLayout layout);
    internal abstract Compression Codec(ExrCompression compression, Compression held);
    internal abstract void Associate(Span<Vector4> row);

    private sealed class ColorPass(string key, Seq<string> channels) : PassKind(key, channels) {
        internal override PixelType Sample(ExrLayout layout) => layout.Sample;
        internal override Compression Codec(ExrCompression compression, Compression held) => held;
        internal override void Associate(Span<Vector4> row) => StillWriter.Associate(row, row);
    }

    private sealed class DataPass(string key, Seq<string> channels) : PassKind(key, channels) {
        internal override PixelType Sample(ExrLayout layout) => PixelType.Float;
        internal override Compression Codec(ExrCompression compression, Compression held) => compression.Data;
        internal override void Associate(Span<Vector4> row) { }
    }
}

public sealed record IdentityLayer(ushort Id, string Name, Vector3 Color) {
    internal static Action<ReadOnlySpan<Vector4>, Span<Vector4>> Fill(Seq<IdentityLayer> entries) {
        FrozenDictionary<float, Vector4> colors = entries.DistinctBy(static entry => entry.Id).ToFrozenDictionary(static entry => (float)entry.Id, static entry => new Vector4(entry.Color, 1f));
        return (ids, matte) => {
            for (int x = 0; x < ids.Length; x++)
                matte[x] = colors.GetValueOrDefault(ids[x].X, Vector4.Zero);
        };
    }
}

internal sealed record ExrLayer(PassKind Kind, PixelFrame Source, Action<ReadOnlySpan<Vector4>, (long X, long Y), Span<Vector4>> Row) {
    internal Option<string> Name { get; init; }

    internal static Action<ReadOnlySpan<Vector4>, (long X, long Y), Span<Vector4>> Held { get; } = static (source, _, part) => source.CopyTo(part);
}

[SmartEnum]
public sealed partial class NormalSpace {
    public static readonly NormalSpace World = new(static (convention, _) => Fin.Succ<Func<Vector3, Vector3>>(convention.Axes));
    public static readonly NormalSpace View = new(static (convention, camera) =>
        camera.ToFin(new ImagingFault("View-space normals need frame camera")).Map(held => {
            Quaternion toView = Quaternion.Conjugate(held.Orientation);
            return (Func<Vector3, Vector3>)(normal => convention.Screen(Vector3.Transform(normal, toView)));
        }));

    [UseDelegateFromConstructor]
    internal partial Fin<Func<Vector3, Vector3>> Normal(ReaderConvention convention, Option<Camera> camera);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReaderConvention {
    private ReaderConvention(NormalSpace normals) => Normals = normals;

    public NormalSpace Normals { get; }

    internal abstract string Beauty { get; }
    internal abstract string Depth { get; }
    internal abstract string Position { get; }

    internal abstract Vector3 Axes(Vector3 world);
    internal abstract Vector3 Screen(Vector3 view);

    public sealed record Blender(NormalSpace Normals) : ReaderConvention(Normals) {
        internal override string Beauty => "Combined";
        internal override string Depth => "Depth";
        internal override string Position => "Position";

        internal override Vector3 Axes(Vector3 world) => world;
        internal override Vector3 Screen(Vector3 view) => view;
    }

    public sealed record Nuke(NormalSpace Normals) : ReaderConvention(Normals) {
        internal override string Beauty => "rgba";
        internal override string Depth => "z";
        internal override string Position => "Pworld";

        internal override Vector3 Axes(Vector3 world) => new(world.X, world.Z, -world.Y);
        internal override Vector3 Screen(Vector3 view) => new(view.X, view.Y, -view.Z);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExrPass {
    internal abstract Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera);

    public sealed record Depth(PixelFrame Frame) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) => Seq(new ExrLayer(PassKind.Z, Frame, ExrLayer.Held) { Name = convention.Depth });
    }

    public sealed record InverseDepth(string Name, PixelFrame Distance) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) =>
            Seq(new ExrLayer(PassKind.Z, Distance, static (source, _, part) => {
                for (int x = 0; x < source.Length; x++)
                    part[x] = new Vector4(1f / source[x].X);
            }) { Name = Name });
    }

    public sealed record Position(PixelFrame Distance) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) =>
            camera.ToFin(new ImagingFault("Position pass needs frame camera")).Map(held => Seq(new ExrLayer(PassKind.Xyz, Distance, (source, first, part) => {
                for (int x = 0; x < source.Length; x++)
                    part[x] = new Vector4(convention.Axes(held.WorldPoint((first.X + x + 0.5d, first.Y + 0.5d), source[x].X)), 1f);
            }) { Name = convention.Position }));
    }

    public sealed record Normal(string Name, PixelFrame Frame) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) =>
            convention.Normals.Normal(convention, camera).Map(turned => Seq(new ExrLayer(PassKind.Xyz, Frame, (source, _, part) => {
                for (int x = 0; x < source.Length; x++)
                    part[x] = new Vector4(turned(source[x].AsVector3()), 1f);
            }) { Name = Name }));
    }

    public sealed record Identities(string Name, PixelFrame Ids, Seq<IdentityLayer> Entries) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) =>
            (Name, Entries).Cons(Entries.Map(static entry => (entry.Name, Entries: Seq(entry))))
                .Map(layer => IdentityLayer.Fill(layer.Entries) switch {
                    var fill => new ExrLayer(PassKind.Rgba, Ids, (source, _, part) => fill(source, part)) { Name = layer.Name },
                });
    }

    public sealed record Named(string Name, PassKind Kind, PixelFrame Frame) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) => Seq(new ExrLayer(Kind, Frame, ExrLayer.Held) { Name = Name });
    }
}

[SmartEnum]
public sealed partial class PassLayout {
    public static readonly PassLayout Parts = new(static (layers, _) => layers.Map(static layer => (layer.Name, layer.Source.Window, Seq(layer))));
    public static readonly PassLayout Interleaved = new(static (layers, beauty) => Seq((Option<string>.None, beauty.Window, layers)));

    [UseDelegateFromConstructor]
    internal partial Seq<(Option<string> Name, (long Left, long Top, long Right, long Bottom) Window, Seq<ExrLayer> Layers)> Grouped(Seq<ExrLayer> layers, PixelFrame beauty);
}

public sealed record PassSet {
    private PassSet(ReaderConvention convention, PassLayout layout, Seq<ExrLayer> layers) => (Convention, Layout, Layers) = (convention, layout, layers);

    public ReaderConvention Convention { get; }
    public PassLayout Layout { get; }
    internal Seq<ExrLayer> Layers { get; }

    public static Fin<PassSet> From(ReaderConvention convention, Option<Camera> camera, PassLayout layout, Seq<ExrPass> passes) =>
        from groups in passes.Traverse(pass => pass.Layers(convention, camera)).As()
        let layers = groups.Flatten().Strict()
        from names in toSeq(convention.Beauty.Cons(layers.Map(static layer => layer.Name).Somes()).CountBy(static name => name))
            .Traverse(static count => count.Value is 1 ? Fin.Succ(unit) : new ImagingFault($"Pass name {count.Key} repeats in pass set"))
            .As()
        select new PassSet(convention, layout, layers);

    internal Fin<Unit> Fits(PixelFrame beauty) =>
        Layers.Map(static (layer, index) => (Layer: layer, Frame: layer.Source, Index: index + 1))
            .Traverse(entry => entry.Frame.Extent == beauty.Extent && Layout.Grouped([entry.Layer], beauty).ForAll(group => group.Window == entry.Frame.Window)
                ? Fin.Succ(unit)
                : new ImagingFault(string.Create(CultureInfo.InvariantCulture, $"Pass layer {entry.Index} window {entry.Frame.Window} on {entry.Frame.Extent} differs from beauty canvas")))
            .As()
            .Map(static _ => unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ExrParts {
    internal static IO<Unit> Write(StillFormat.ExrRaster exr, FileMetadata metadata, Stream stream) {
        static Seq<ChannelBuffer> Buffers(ExrLayout layout, ExrLayer layer) {
            (int width, int height, int channels) = (layer.Source.Size.Width, layer.Source.Size.Height, layer.Kind.Channels.Count);
            float[] source = new float[width * height * channels];
            Span2D<float> lines = new(source, height, width * channels);
            using SpanOwner<Vector4> row = SpanOwner<Vector4>.Allocate(width);
            for (int y = 0; y < height; y++) {
                layer.Row(layer.Source.View.Span.GetRowSpan(y), (layer.Source.Origin.X, (long)layer.Source.Origin.Y + y), row.Span);
                layer.Kind.Associate(row.Span);
                MemoryMarshal.Cast<Vector4, float>(row.Span).AsSpan2D(width, 4).Slice(0, 0, width, channels).CopyTo(lines.GetRowSpan(y));
            }
            return toSeq(PartConversion.FromInterleavedFloat(source, width, height, channels, layer.Kind.Sample(layout), Compression.None).Levels[0].Channels)
                .Zip(layer.Kind.Channels)
                .Map(pair => new ChannelBuffer(layer.Name.Match(Some: name => $"{name}.{pair.Second}", None: () => pair.Second), pair.First.PixelType, pair.First.Data))
                .Strict();
        }

        static Fin<Part> Part(StillFormat.ExrRaster exr, Seq<HeaderAttribute> attributes, Option<string> name, (long Left, long Top, long Right, long Bottom) window, Seq<ExrLayer> layers) {
            if (window.Right - 1L > int.MaxValue || window.Bottom - 1L > int.MaxValue)
                return new ImagingFault($"EXR window {window} exceeds EXR coordinate range");
            Seq<ChannelBuffer> buffers = layers.Bind(layer => Buffers(exr.Layout, layer)).Strict();
            Header header = new(
                PartType.Scanline,
                new Box2i((int)window.Left, (int)window.Top, (int)(window.Right - 1L), (int)(window.Bottom - 1L)),
                buffers.Map(static buffer => new Channel(buffer.Name, buffer.PixelType)),
                compression: layers.Fold(exr.Layout.Compression.Codec, (codec, layer) => layer.Kind.Codec(exr.Layout.Compression, codec)),
                displayWindow: new Box2i(0, 0, exr.Beauty.Extent.Width - 1, exr.Beauty.Extent.Height - 1),
                name: name.ValueUnsafe(),
                chromaticities: exr.Layout.Gamut.Map(static gamut => gamut.Chromaticities).ToNullable(),
                attributes: attributes);
            return new Part(header, [new FlatLevel(0, 0, header.DataWindow, buffers)], isComplete: true);
        }

        static Fin<Seq<Part>> Parts(StillFormat.ExrRaster exr, Seq<HeaderAttribute> attributes) {
            PassKind beauty = exr.Alpha ? PassKind.Rgba : PassKind.Rgb;
            Seq<Fin<Part>> parts = exr.Passes.Match(
                Some: set => set.Layout.Grouped(new ExrLayer(beauty, exr.Beauty, ExrLayer.Held) { Name = set.Convention.Beauty }.Cons(set.Layers), exr.Beauty)
                    .Map(group => Part(exr, attributes, group.Name, group.Window, group.Layers)),
                None: () => Seq(Part(exr, attributes, None, exr.Beauty.Window, Seq(new ExrLayer(beauty, exr.Beauty, ExrLayer.Held)))));
            return parts.Traverse(static part => part).As();
        }

        WriterOptions options = new(new WriterLimits(
            maximumParts: int.MaxValue, maximumChannelsPerPart: int.MaxValue, maximumAttributesPerPart: int.MaxValue,
            maximumHeaderByteCount: System.Array.MaxLength, maximumBlocksPerPart: int.MaxValue, maximumTotalBlocks: long.MaxValue,
            maximumOffsetTableByteCount: System.Array.MaxLength, maximumDimension: long.MaxValue,
            maximumUncompressedBlockByteCount: System.Array.MaxLength, maximumEncodedBlockByteCount: System.Array.MaxLength,
            maximumDeepSampleCount: long.MaxValue));
        return from image in IO.lift(() => Parts(exr, metadata.Exr(exr.Layout.Gamut)).Map(static parts => new Image(parts)))
               from token in cancelToken
               from result in IO.liftVAsync(() => ExrFile.SaveToStreamAsync(image, stream, options, token))
               from _ in IO.lift(result.IsSuccess ? Fin.Succ(unit) : new ImagingFault($"EXR codec refused writing with {result.Status}", Optional(result.Error).Map(Error.New)))
               select unit;
    }
}
