using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using Rasm.Imaging.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LutFile {
    private static readonly DecoderOptions LookupOptions = new() {
        Configuration = new Configuration(new PngConfigurationModule(), new TiffConfigurationModule()),
        SkipMetadata = true,
    };

    private LutFile(string path, LutTable table) => (Path, Table) = (path, table);

    private static FrozenDictionary<string, Func<string, IO<LutTable>>> Readers { get; } =
        toSeq(LookupOptions.Configuration.ImageFormats).Bind(static format => toSeq(format.FileExtensions)).Map(static extension => $".{extension}") switch {
            var lookups => lookups.Map(static extension => (Extension: extension, Read: (Func<string, IO<LutTable>>)ReadLookup))
                .Concat(toSeq(LutFormat.Items)
                    .Filter(format => !lookups.Exists(extension => string.Equals(extension, format.Extension, StringComparison.OrdinalIgnoreCase)))
                    .Map(static format => (format.Extension, Read: (Func<string, IO<LutTable>>)(path => LutFiles.Read(path, format)))))
                .ToFrozenDictionary(static reader => reader.Extension, static reader => reader.Read, StringComparer.OrdinalIgnoreCase),
        };

    public string Path { get; }
    public LutTable Table { get; }

    public static IO<LutFile> Read(string path) =>
        IO.lift(Optional(Readers.GetValueOrDefault(System.IO.Path.GetExtension(path))).ToFin(new ImagingFault($"LUT path {path} names none of {string.Join(", ", Readers.Keys)}")))
            .Bind(read => read(path))
            .Map(table => new LutFile(path, table))
            .Catch(
                static error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>() || error.HasException<ImageFormatException>(),
                error => IO.fail<LutFile>(Error.New($"LUT file {path} is unreadable", error)));

    private static IO<LutTable> ReadLookup(string path) =>
        from token in cancelToken
        from table in use(IO.liftAsync(() => Image.LoadAsync<Rgb48>(LookupOptions, path, token))).Bind(static image => IO.lift(Lattice(image))).Bracket()
        select table;

    private static Fin<LutTable> Lattice(Image<Rgb48> image) =>
        toSeq(LookupLayout.Items)
            .Choose(layout => from edge in layout.Edge(image.Width, image.Height) from extent in layout.Image(edge) select (Layout: layout, Edge: edge, Extent: extent))
            .Head.ToFin(Error.New(string.Create(CultureInfo.InvariantCulture, $"Lookup image of {image.Width}x{image.Height} holds no lattice layout")))
            .Bind(found => {
                using MemoryOwner<float> samples = MemoryOwner<float>.Allocate(3 * found.Edge * found.Edge * found.Edge);
                image.ProcessPixelRows(rows => {
                    Span<Vector3> nodes = MemoryMarshal.Cast<float, Vector3>(samples.Span);
                    for (int pixel = 0; pixel < rows.Width * rows.Height; pixel++)
                        nodes[found.Layout.Node(pixel % rows.Width, pixel / rows.Width, found.Extent, found.Edge)] = rows.GetRowSpan(pixel / rows.Width)[pixel % rows.Width].ToScaledVector4().AsVector3();
                });
                return TinyEXR.Lut3D.From(found.Edge, samples.Memory, Vector3.Zero, Vector3.One).Map(static lattice => (LutTable)new LutTable.Cube(lattice, TinyEXR.LutInterpolation.Tetrahedral));
            });
}

