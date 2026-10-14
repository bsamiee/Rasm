using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError<InvalidFileValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LutPath {
    static partial void ValidateFactoryArguments(ref InvalidFileValue? validationError, ref string value) =>
        validationError = Path.IsPathFullyQualified(value) && LutFile.Readers.ContainsKey(Path.GetExtension(value)) ? null : new InvalidFileValue();
}

public sealed record LutFile {
    private static readonly DecoderOptions LookupOptions = new() {
        Configuration = new Configuration(new PngConfigurationModule(), new TiffConfigurationModule()),
        SkipMetadata = true,
    };

    private LutFile(LutPath path, LutTable table) => (Path, Table) = (path, table);

    internal static FrozenDictionary<string, Func<LutPath, IO<LutTable>>> Readers { get; } =
        toSeq(LookupOptions.Configuration.ImageFormats).Bind(static format => toSeq(format.FileExtensions)).Map(static extension => $".{extension}") switch {
            var lookups => lookups.Map(static extension => (Extension: extension, Read: (Func<LutPath, IO<LutTable>>)ReadLookup))
                .Concat(toSeq(LutFormat.Items)
                    .Filter(format => !lookups.Exists(extension => string.Equals(extension, format.Extension, StringComparison.OrdinalIgnoreCase)))
                    .Map(static format => (format.Extension, Read: (Func<LutPath, IO<LutTable>>)(path => LutFiles.Read(path, format)))))
                .ToFrozenDictionary(static reader => reader.Extension, static reader => reader.Read, StringComparer.OrdinalIgnoreCase),
        };

    public LutPath Path { get; }
    public LutTable Table { get; }

    public static IO<LutFile> Read(LutPath path) =>
        Readers[System.IO.Path.GetExtension(path)](path)
            .Map(table => new LutFile(path, table))
            .Catch(
                static error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>() || error.HasException<ImageFormatException>(),
                error => IO.fail<LutFile>(new LutUnreadable(path, error)));

    public static IO<Unit> Write(LutTable table, LutFormat format, OutputPath path) =>
        from text in IO.lift(LutFiles.Render(table, format, System.IO.Path.GetFileNameWithoutExtension(path)))
        from _ in FilePublication.Write(path, text)
        select unit;

    public static IO<Unit> WriteLookup(LutTable table, LookupLayout layout, FileMetadata metadata, OutputPath path) =>
        from still in IO.lift(LutFiles.Lookup(table, layout).Bind(static frame => StillFormat.PngRaster.From(frame, OutputDepth.UInt16, None, alpha: false)))
        from _ in StillWriter.Write(still, metadata, path)
        select unit;

    private static IO<LutTable> ReadLookup(LutPath path) =>
        from token in cancelToken
        from table in use(IO.liftAsync(() => SixLabors.ImageSharp.Image.LoadAsync<Rgb48>(LookupOptions, path, token))).Bind(static image => IO.lift(Lattice(image))).Bracket()
        select table;

    private static Fin<LutTable> Lattice(Image<Rgb48> image) =>
        toSeq(LookupLayout.Items)
            .Choose(layout => from edge in layout.Edge(image.Width, image.Height) from extent in layout.Image(edge) select (Layout: layout, Edge: edge, Extent: extent))
            .Head.ToFin(new LookupShapeRefused(image.Width, image.Height))
            .Bind(found => {
                using MemoryOwner<float> samples = MemoryOwner<float>.Allocate(3 * found.Edge * found.Edge * found.Edge);
                image.ProcessPixelRows(rows => {
                    Span<Vector3> nodes = MemoryMarshal.Cast<float, Vector3>(samples.Span);
                    for (int pixel = 0; pixel < rows.Width * rows.Height; pixel++)
                        nodes[found.Layout.Node(pixel % rows.Width, pixel / rows.Width, found.Extent, found.Edge)] = rows.GetRowSpan(pixel / rows.Width)[pixel % rows.Width].ToScaledVector4().AsVector3();
                });
                return Lut3D.From(found.Edge, samples.Memory, Vector3.Zero, Vector3.One).Map(static lattice => (LutTable)new LutTable.Cube(lattice, LutInterpolation.Tetrahedral));
            });
}

public sealed record LutLook(Option<LutFile> Lut, LutSpace Space)
    : IStateRecord<LutLook, LutLookParameter, InvalidFileValue>, IPixelStage<LutLook> {
    public static LutLook Default { get; } = new(None, LutSpace.Encoded.Default);

    public static Option<PixelPass> Pass(LutLook state, PassContext context) =>
        state.Lut.Map(file => (new ColorEncoding(context.Working, context.Signal, Nits.ReferenceWhite), state.Space.Bracket) switch {
            var (input, (space, into, back)) when input.Transfer == TransferCurve.Linear && space.Transfer == TransferCurve.Linear =>
                (PixelPass)new PixelPass.Color(new LutTable.Sequence([.. LutTables.Between(input.Gamut, space.Gamut), .. into, file.Table, .. back, .. LutTables.Between(space.Gamut, input.Gamut)])),
            var (input, (space, into, back)) => (input.To(space) + Seq(new LutTable.Sequence([.. into, file.Table, .. back]).Apply) + space.To(input)) switch {
                var steps => new PixelPass.Color(row => {
                    foreach (Action<Span<Vector4>> step in steps) step(row);
                }),
            },
        });
}

[SmartEnum<string>]
[ValidationError<InvalidFileValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LutLookParameter : IStateParameter<LutLook> {
    public static readonly LutLookParameter Lut = new("lut", new StateParameter<LutLook>.Loaded<LutFile, LutPath, string, InvalidFileValue>(
        Lens<LutLook, Option<LutFile>>.New(static look => look.Lut, static lut => look => look with { Lut = lut }),
        LutFile.Read,
        static file => file.Path));
    public static readonly LutLookParameter Space = new("space", new StateParameter<LutLook>.Variant<LutSpace>(
        Lens<LutLook, LutSpace>.New(static look => look.Space, static space => look => look with { Space = space })));

    public StateParameter<LutLook> Kind { get; }
}
