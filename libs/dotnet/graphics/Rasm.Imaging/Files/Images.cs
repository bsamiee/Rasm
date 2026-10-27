using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Cicp;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ImageFile {
    private ImageFile(string path, PixelFrame frame) => (Path, Frame) = (path, frame);

    private static FrozenDictionary<string, Func<FileInfo, IO<PixelFrame>>> Reads { get; } =
        new KeyValuePair<string, Func<FileInfo, IO<PixelFrame>>>[] {
            new(".exr", static file => Exr(file, "").Map(static read => read.Frame)),
            new(".png", Png),
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public string Path { get; }
    public PixelFrame Frame { get; }

    public static IO<ImageFile> Load(string path) =>
        from read in IO.lift(Optional(Reads.GetValueOrDefault(System.IO.Path.GetExtension(path))).ToFin(new ImagingFault($"image path {path} beside {string.Join(", ", Reads.Keys)}", ImagingReason.UnknownFormat)))
        from frame in read(new FileInfo(path))
        select new ImageFile(path, frame);

    public static IO<(PixelFrame Frame, Option<Gamut> Primaries)> Exr(FileInfo file, string layer) =>
        from result in IO.liftVAsync(env => ExrFile.LoadFromFileAsync(file.FullName, cancellationToken: env.Token))
        from image in IO.lift(Optional(result.Value).ToFin(new ImagingFault($"EXR read of {file.FullName} with {result.Status}", ImagingReason.Refused, Inner: Optional(result.Error).Map(Error.New))))
        from read in IO.lift(() => Layer(toSeq(image.Parts), layer))
        select read;

    private static IO<PixelFrame> Png(FileInfo file) =>
        (from stream in use(() => new FileStream(file.FullName, new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read, Options = FileOptions.Asynchronous }))
         from info in IO.liftAsync(env => PngDecoder.Instance.IdentifyAsync(Decoding(ColorProfileHandling.Preserve), stream, env.Token))
         let cicp = Optional(info.Metadata.CicpProfile)
         from encoding in IO.lift(Encoding(cicp))
         from _ in IO.lift(() => stream.Seek(0, SeekOrigin.Begin))
         from frame in (
                 from image in use(IO.liftAsync(env => PngDecoder.Instance.DecodeAsync<RgbaVector>(
                     Decoding(cicp.Match(Some: static _ => ColorProfileHandling.Preserve, None: static () => ColorProfileHandling.Convert)), stream, env.Token)))
                 from size in IO.lift(PixelExtent.Validated(image.Width, image.Height))
                 select new PixelFrame(Point.Empty, size, size, block => {
                     image.CopyPixelDataTo(MemoryMarshal.Cast<float, RgbaVector>(block.AsSpan()));
                     encoding.Transfer.Decode(MemoryMarshal.Cast<float, Vector4>(block.AsSpan()));
                 }))
             .Bracket()
         select frame)
            .Bracket()
            .Catch(
                static error => error.HasException<SixLabors.ImageSharp.ImageFormatException>() || error.HasException<InvalidIccProfileException>()
                    || error.HasException<NotSupportedException>() || error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>(),
                error => IO.fail<PixelFrame>(new ImagingFault($"PNG read of {file.FullName}", ImagingReason.Refused, Inner: error)));

    private static Fin<(PixelFrame Frame, Option<Gamut> Primaries)> Layer(Seq<Part> parts, string layer) =>
        from part in (parts.Find(candidate => string.Equals(candidate.Header.Name, layer, StringComparison.Ordinal)) | parts.Find(static candidate => candidate.Header.Name.Length is 0))
            .ToFin(new ImagingFault($"EXR part for layer '{layer}'", ImagingReason.Empty))
        from header in (
                from samples in part.Header.IsDeep
                     ? Fin.Fail<InterleavedFloatImage>(new ImagingFault($"EXR deep part '{part.Header.Name}'", ImagingReason.Unsupported))
                     : layer.Length is 0 && PartConversion.IsLuminanceChroma(part) ? PartConversion.LuminanceChromaToRgbaFloat(part) : PartConversion.ToInterleavedFloat(part)
                from layout in Components(samples.ChannelNames, layer)
                select (Samples: samples, Layout: layout),
                PixelExtent.Validated(int.CreateSaturating(part.Width), int.CreateSaturating(part.Height)),
                PixelExtent.Validated(int.CreateSaturating(part.Header.DisplayWindow.Width), int.CreateSaturating(part.Header.DisplayWindow.Height)))
            .Apply(static (decoded, size, extent) => (decoded.Samples, decoded.Layout, Size: size, Extent: extent))
            .As()
        select (new PixelFrame(
            new Point(part.Header.DataWindow.MinX - part.Header.DisplayWindow.MinX, part.Header.DataWindow.MinY - part.Header.DisplayWindow.MinY),
            header.Size,
            header.Extent,
            block => {
                Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
                Span2D<float> samples = block.AsSpan().AsSpan2D(pixels.Length, 4);
                ReadOnlySpan2D<float> source = header.Samples.Data.AsSpan2D(pixels.Length, header.Samples.Channels);
                pixels.Fill(Vector4.UnitW);
                foreach ((int component, int channel) in header.Layout.Rows)
                    source.GetColumn(channel).CopyTo(samples.GetColumn(component));
                if (header.Layout.Associated)
                    foreach (ref Vector4 pixel in pixels)
                        pixel = pixel.W > 0f ? new(pixel.AsVector3() / pixel.W, pixel.W) : new(Vector3.Zero, pixel.W);
            }), Optional(part.Header.Chromaticities).Map(Gamut.Of));

    private static Fin<((int Component, int Channel)[] Rows, bool Associated)> Components(IReadOnlyList<string> names, string layer) =>
        toSeq(
            (from family in Seq("RGBA", "XYZW", "UV")
             from entry in toSeq(names).Map(static (name, channel) => (Name: name, Channel: channel, Dot: name.LastIndexOf('.')))
             where entry.Name.Length == entry.Dot + 2 && entry.Name.AsSpan(0, int.Max(entry.Dot, 0)).SequenceEqual(layer)
             let component = family.IndexOf(entry.Name[^1], StringComparison.OrdinalIgnoreCase)
             where component >= 0
             select (Component: component, entry.Channel, Letter: entry.Name[^1])).DistinctBy(static row => row.Component)) switch {
                 [] => new ImagingFault($"EXR channel of layer '{layer}'", ImagingReason.Empty),
                 var filled => (
                     (filled.Filter(static row => row.Component < 3) is [var lone]
                         ? filled + Seq(0, 1, 2).Filter(component => component != lone.Component).Map(component => (Component: component, lone.Channel, lone.Letter))
                         : filled).Map(static row => (row.Component, row.Channel)).ToArray(),
                     filled.Exists(static row => row is { Component: 3, Letter: 'A' or 'a' })),
             };

    private static Fin<ColorEncoding> Encoding(Option<CicpProfile> cicp) =>
        cicp.Match(
            Some: static chunk =>
                (ColorTags.Primaries.Find(row => row.Code == chunk.ColorPrimaries).Map(static row => row.Gamut), ColorTags.Transfers.Find(row => row.Code == chunk.TransferCharacteristics))
                    .Apply(static (gamut, row) => new ColorEncoding(gamut, row.Transfer))
                    .As()
                    .Filter(_ => chunk.MatrixCoefficients is CicpMatrixCoefficients.Identity && chunk.FullRange)
                    .ToFin(new ImagingFault($"PNG cICP {chunk.ColorPrimaries}/{chunk.TransferCharacteristics}/{chunk.MatrixCoefficients} full range {chunk.FullRange}", ImagingReason.UnknownFormat)),
            None: static () => new ColorEncoding(Gamut.StandardRgb, new Transfer.Srgb()));

    private static DecoderOptions Decoding(ColorProfileHandling handling) =>
        new() { MaxFrames = 1, SegmentIntegrityHandling = SegmentIntegrityHandling.Strict, ColorProfileHandling = handling };
}
