using System.Globalization;
using System.Numerics;
using System.Text;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
internal readonly record struct ExrSample(OutputDepth Depth, PixelType Type);

public sealed record ExrLayout {
    private ExrLayout(ExrSample sample, ExrCompression compression, Option<Gamut> gamut) => (Sample, Compression, Gamut) = (sample, compression, gamut);

    public ExrCompression Compression { get; }
    public Option<Gamut> Gamut { get; }
    internal ExrSample Sample { get; }

    internal static ExrSample Data { get; } = new(OutputDepth.Float32, PixelType.Float);

    public static ExrLayout Coordinates { get; } = new(Data, ExrCompression.Zip, None);

    public static Seq<OutputDepth> Depths => OutputDepth.Where(Table);

    public static Fin<ExrLayout> From(OutputDepth depth, ExrCompression compression, Gamut gamut) =>
        OutputDepth.Sampled("EXR", depth, Table).Map(sample => new ExrLayout(sample, compression, Some(gamut)));

    internal static Option<ExrSample> Table(OutputDepth depth) =>
        depth.Map<Option<ExrSample>>(uInt8: None, uInt10: None, uInt12: None, uInt14: None, uInt16: None, float16: new ExrSample(depth, PixelType.Half), float32: Data);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExrCompression : ICaptioned {
    public static readonly ExrCompression Uncompressed = new("uncompressed", Compression.None, Compression.None, "None");
    public static readonly ExrCompression Rle = new("rle", Compression.RLE, Compression.RLE, "RLE (lossless)");
    public static readonly ExrCompression Zips = new("zips", Compression.ZIPS, Compression.ZIPS, "ZIPS (lossless)");
    public static readonly ExrCompression Zip = new("zip", Compression.ZIP, Compression.ZIP, "ZIP (lossless)");
    public static readonly ExrCompression Piz = new("piz", Compression.PIZ, Compression.PIZ, "PIZ (lossless)");
    public static readonly ExrCompression Pxr24 = new("pxr24", Compression.PXR24, Compression.ZIP, "Pxr24 (lossy)");
    public static readonly ExrCompression B44 = new("b44", Compression.B44, Compression.ZIP, "B44 (lossy)");
    public static readonly ExrCompression B44A = new("b44a", Compression.B44A, Compression.ZIP, "B44A (lossy)");

    public Compression Codec { get; }
    public Compression Data { get; }
    public string Caption { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
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
public sealed partial class PassName {
    private const int NameBytes = 255;

    internal static PassName Combined { get; } = new("Combined");
    internal static PassName Depth { get; } = new("Depth");
    internal static PassName Position { get; } = new("Position");
    internal static PassName Rgba { get; } = new("rgba");
    internal static PassName Z { get; } = new("z");
    internal static PassName Pworld { get; } = new("Pworld");

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Length > 0 && !value.Contains('\0', StringComparison.Ordinal) && Encoding.UTF8.GetByteCount(value) + 2 <= NameBytes ? null
            : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"pass name '{value}' must be non-empty, free of NUL, and at most {NameBytes - 2} UTF-8 bytes"));
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

internal readonly record struct ExrLayer(Option<PassName> Name, PassKind Kind, PixelFrame Source, Action<ReadOnlySpan<Vector4>, (long X, long Y), Span<Vector4>> Row) {
    internal static Action<ReadOnlySpan<Vector4>, (long X, long Y), Span<Vector4>> Held { get; } = static (source, _, part) => source.CopyTo(part);
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

    internal Fin<Func<Vector3, Vector3>> Normal(Option<Camera> camera) =>
        Normals.Switch((Convention: this, Camera: camera),
            world: static state => Fin.Succ(state.Convention.Axes),
            view: static state => state.Camera.ToFin(Error.New("view-space normals require the frame's camera")).Map(held => {
                Quaternion toView = Quaternion.Conjugate(held.Orientation);
                return (Func<Vector3, Vector3>)(normal => state.Convention.Screen(Vector3.Transform(normal, toView)));
            }));

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
    internal abstract Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera);

    public sealed record Depth(PixelFrame Frame) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) => Seq(new ExrLayer(convention.Depth, PassKind.Z, Frame, ExrLayer.Held));
    }

    public sealed record InverseDepth(PassName Name, PixelFrame Distance) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) =>
            Seq(new ExrLayer(Name, PassKind.Z, Distance, static (source, _, part) => {
                for (int x = 0; x < source.Length; x++)
                    part[x] = new Vector4(1f / source[x].X);
            }));
    }

    public sealed record Position(PixelFrame Distance) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) =>
            camera.ToFin(Error.New("position pass requires the frame's camera")).Map(held => Seq(new ExrLayer(convention.Position, PassKind.Xyz, Distance, (source, first, part) => {
                for (int x = 0; x < source.Length; x++)
                    part[x] = new Vector4(convention.Axes(held.WorldPoint((first.X + x + 0.5d, first.Y + 0.5d), source[x].X)), 1f);
            })));
    }

    public sealed record Normal(PassName Name, PixelFrame Frame) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) =>
            convention.Normal(camera).Map(turned => Seq(new ExrLayer(Name, PassKind.Xyz, Frame, (source, _, part) => {
                for (int x = 0; x < source.Length; x++)
                    part[x] = new Vector4(turned(source[x].AsVector3()), 1f);
            })));
    }

    public sealed record Identities(PassName Name, PixelFrame Ids, Seq<IdentityLayer> Entries) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) =>
            (Name, Entries).Cons(Entries.Map(static entry => (entry.Name, Entries: Seq(entry))))
                .Map(layer => IdentityLayer.Fill(layer.Entries) switch {
                    var fill => new ExrLayer(layer.Name, PassKind.Rgba, Ids, (source, _, part) => fill(source, part)),
                });
    }

    public sealed record Named(PassName Name, PassKind Kind, PixelFrame Frame) : ExrPass {
        internal override Fin<Seq<ExrLayer>> Layers(ReaderConvention convention, Option<Camera> camera) => Seq(new ExrLayer(Name, Kind, Frame, ExrLayer.Held));
    }
}

[SmartEnum]
public sealed partial class PassLayout {
    public static readonly PassLayout Parts = new();
    public static readonly PassLayout Interleaved = new();
}

public sealed record PassSet {
    private PassSet(ReaderConvention convention, PassLayout layout, Seq<ExrLayer> layers) => (Convention, Layout, Layers) = (convention, layout, layers);

    public ReaderConvention Convention { get; }
    public PassLayout Layout { get; }
    internal Seq<ExrLayer> Layers { get; }

    public static Validation<Error, PassSet> From(ReaderConvention convention, Option<Camera> camera, PassLayout layout, Seq<ExrPass> passes) =>
        from groups in passes.Traverse(pass => pass.Layers(convention, camera).ToValidation()).As()
        let layers = groups.Flatten().Strict()
        from names in toSeq(convention.Beauty.Cons(layers.Map(static layer => layer.Name).Somes()).CountBy(static name => name))
            .Traverse(static count => count.Value is 1 ? Validation.Success<Error, Unit>(unit) : Error.New($"pass name {count.Key} repeats in one pass set"))
            .As()
        select new PassSet(convention, layout, layers);

    internal Validation<Error, Unit> Fits(PixelFrame beauty) =>
        Layers.Map(static (layer, index) => (Frame: layer.Source, Index: index + 1))
            .Traverse(entry => entry.Frame.Extent == beauty.Extent && Layout.Map(parts: true, interleaved: entry.Frame.Window == beauty.Window)
                ? Validation.Success<Error, Unit>(unit)
                : Error.New(string.Create(CultureInfo.InvariantCulture, $"pass layer {entry.Index} window {entry.Frame.Window} on {entry.Frame.Extent} differs from the beauty canvas")))
            .As()
            .Map(static _ => unit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ExrParts {
    internal static IO<Unit> Write(StillFormat.ExrRaster exr, FileMetadata metadata, FileStream stream) {
        static Seq<ChannelBuffer> Buffers(ExrLayout layout, ExrLayer layer) {
            ExrSample sample = layer.Kind.Sample(layout);
            byte[][] planes = StillWriter.Planar(sample.Depth, layer.Source.Size, layer.Kind.Channels.Count, (y, row) => {
                layer.Row(layer.Source.View.Span.GetRowSpan(y), (layer.Source.Origin.X, (long)layer.Source.Origin.Y + y), row);
                layer.Kind.Associate(row);
            });
            return layer.Kind.Channels
                .Map((letter, lane) => new ChannelBuffer(layer.Name.Match(Some: name => $"{name}.{letter}", None: () => letter), sample.Type, planes[lane]))
                .Strict();
        }

        static Fin<Part> Part(StillFormat.ExrRaster exr, Seq<HeaderAttribute> attributes, Option<PassName> name, (long Left, long Top, long Right, long Bottom) window, Seq<ExrLayer> layers) {
            if (window.Right - 1L > int.MaxValue || window.Bottom - 1L > int.MaxValue)
                return Error.New($"EXR window {window} exceeds the coordinates EXR holds");
            Seq<ChannelBuffer> buffers = layers.Bind(layer => Buffers(exr.Layout, layer)).Strict();
            Header header = new(
                PartType.Scanline,
                new Box2i((int)window.Left, (int)window.Top, (int)(window.Right - 1L), (int)(window.Bottom - 1L)),
                buffers.Map(static buffer => new Channel(buffer.Name, buffer.PixelType)),
                compression: layers.Fold(exr.Layout.Compression.Codec, (codec, layer) => layer.Kind.Codec(exr.Layout.Compression, codec)),
                displayWindow: new Box2i(0, 0, exr.Beauty.Extent.Width - 1, exr.Beauty.Extent.Height - 1),
                name: name.Map(static part => (string)part).ValueUnsafe(),
                chromaticities: exr.Layout.Gamut.Map(static gamut => gamut.Chromaticities).ToNullable(),
                attributes: attributes);
            return new Part(header, [new FlatLevel(0, 0, header.DataWindow, buffers)], isComplete: true);
        }

        static Fin<Seq<Part>> Parts(StillFormat.ExrRaster exr, Seq<HeaderAttribute> attributes) {
            PassKind beauty = exr.Alpha ? PassKind.Rgba : PassKind.Rgb;
            Seq<Fin<Part>> parts = exr.Passes.Match(
                Some: set => set.Layout.Switch(
                    (Exr: exr, Attributes: attributes, Layers: new ExrLayer(set.Convention.Beauty, beauty, exr.Beauty, ExrLayer.Held).Cons(set.Layers)),
                    parts: static state => state.Layers.Map(layer => Part(state.Exr, state.Attributes, layer.Name, layer.Source.Window, Seq(layer))),
                    interleaved: static state => Seq(Part(state.Exr, state.Attributes, None, state.Exr.Beauty.Window, state.Layers))),
                None: () => Seq(Part(exr, attributes, None, exr.Beauty.Window, Seq(new ExrLayer(None, beauty, exr.Beauty, ExrLayer.Held)))));
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
               from _ in IO.lift(result.IsSuccess ? Fin.Succ(unit) : new Expected($"EXR codec refused writing {stream.Name} with {result.Status}", 0, Optional(result.Error).Map(Error.New)))
               select unit;
    }
}
