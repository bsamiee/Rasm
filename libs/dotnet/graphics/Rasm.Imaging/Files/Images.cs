using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Win32.SafeHandles;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using TinyEXR;
using Wacton.Unicolour;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
public abstract partial record ImageFile {
    private static readonly ColorEncoding Untagged = new(Gamut.StandardRgb, Transfer.Gamma.Srgb);

    public static IO<(PixelFrame Frame, Option<ColorEncoding> Encoding)> Read(string path, string layer = "") =>
        (from stream in use(() => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, FileOptions.Asynchronous))
         from exr in IO.lift(() => Claimed(stream.SafeFileHandle))
         from read in exr
             ? Loaded(stream, layer)
             : Decoded(stream) | @catch(static error => error.HasException<SixLabors.ImageSharp.UnknownImageFormatException>(), _ => Imread(path))
         select read)
            .Bracket()
            .Catch(static error => error.IsExceptional, error => IO.fail<(PixelFrame, Option<ColorEncoding>)>(ImagingFault.Of(ImagingReason.Unreadable, path, inner: error)));

    private static bool Claimed(SafeFileHandle file) {
        Span<byte> magic = stackalloc byte[sizeof(uint)];
        return ExrFile.IsExr(magic[..RandomAccess.Read(file, magic, 0)]);
    }

    private static IO<(PixelFrame Frame, Option<ColorEncoding> Encoding)> Loaded(Stream stream, string layer) =>
        from result in IO.liftVAsync(environment => ExrFile.LoadFromStreamAsync(stream, cancellationToken: environment.Token))
        from image in IO.lift(Optional(result.Value).ToFin(ImagingFault.Of(ImagingReason.Refused, result.Status, inner: Optional(result.Error).Map(Error.New))))
        from read in IO.lift(() => Layer(toSeq(image.Parts), layer))
        select read;

    private static IO<(PixelFrame Frame, Option<ColorEncoding> Encoding)> Decoded(Stream stream) =>
        from info in IO.liftAsync(environment => SixLabors.ImageSharp.Image.IdentifyAsync(Decoding(ColorProfileHandling.Preserve), stream, environment.Token))
        let cicp = Optional(info.Metadata.CicpProfile)
        from encoding in IO.lift(cicp.Match(Some: ColorEncoding.Of, None: static () => Untagged))
        from rewound in IO.lift(() => stream.Seek(0, SeekOrigin.Begin))
        from frame in (
                from image in use(IO.liftAsync(environment => SixLabors.ImageSharp.Image.LoadAsync<RgbaVector>(
                    Decoding(cicp.IsSome ? ColorProfileHandling.Preserve : ColorProfileHandling.Convert), stream, environment.Token)))
                from size in IO.lift(PixelExtent.Validated(image.Width, image.Height))
                select new PixelFrame(Point.Empty, size, size, block => image.CopyPixelDataTo(MemoryMarshal.Cast<float, RgbaVector>(block))))
            .Bracket()
        select (frame, Some(encoding));

    private static IO<(PixelFrame Frame, Option<ColorEncoding> Encoding)> Imread(string path) =>
        (from claimed in IO.lift(ImagingFault.Unless(CvInvoke.HaveImageReader(path), ImagingReason.UnknownFormat, path))
         from image in use(IO.lift(() => CvInvoke.Imread(path, ImreadModes.Unchanged)))
         from size in IO.lift(PixelExtent.Validated(image.Width, image.Height))
         from code in IO.lift(image.NumberOfChannels switch {
             1 => Fin.Succ(ColorConversion.Gray2Rgba),
             3 => ColorConversion.Bgr2Rgba,
             4 => ColorConversion.Bgra2Rgba,
             var channels => ImagingFault.Of(ImagingReason.Unsupported, channels),
         })
         let scale = image.Depth switch { DepthType.Cv8U => Some(1d / byte.MaxValue), DepthType.Cv16U => Some(1d / ushort.MaxValue), _ => Option<double>.None }
         let frame = new PixelFrame(Point.Empty, size, size)
         from filled in IO.lift(frame.Filter(header => {
             CvInvoke.CvtColor(image, image, code);
             image.ConvertTo(header, DepthType.Cv32F, scale.IfNone(1d));
             return unit;
         }))
         select (frame, scale.Map(static _ => Untagged))).Bracket();

    private static Fin<(PixelFrame Frame, Option<ColorEncoding> Encoding)> Layer(Seq<Part> parts, string layer) =>
        from part in (parts.Find(candidate => string.Equals(candidate.Header.Name, layer, StringComparison.Ordinal)) | parts.Find(static candidate => candidate.Header.Name.Length is 0))
            .ToFin(ImagingFault.Of(ImagingReason.Empty, layer))
        from header in (
                from samples in part.Header.IsDeep
                     ? ImagingFault.Of(ImagingReason.Unsupported, part.Header.Name)
                     : Fin.Succ(layer.Length is 0 && PartConversion.IsLuminanceChroma(part) ? PartConversion.LuminanceChromaToRgbaFloat(part) : PartConversion.ToInterleavedFloat(part))
                from layout in Components(samples.ChannelNames, layer)
                select (Samples: samples, Layout: layout),
                PixelExtent.Validated(int.CreateSaturating(part.Width), int.CreateSaturating(part.Height)),
                PixelExtent.Validated(int.CreateSaturating(part.Header.DisplayWindow.Width), int.CreateSaturating(part.Header.DisplayWindow.Height)))
            .Apply(static (decoded, size, extent) => (decoded.Samples, decoded.Layout, Size: size, Extent: extent))
            .As()
        from primaries in Primaries(part.Header)
        select (new PixelFrame(
            new Point(part.Header.DataWindow.MinX - part.Header.DisplayWindow.MinX, part.Header.DataWindow.MinY - part.Header.DisplayWindow.MinY),
            header.Size,
            header.Extent,
            block => {
                Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block);
                ReadOnlySpan2D<float> source = header.Samples.Data.AsSpan2D(pixels.Length, header.Samples.Channels);
                pixels.Fill(Vector4.UnitW);
                foreach ((int component, int channel) in header.Layout.Rows)
                    source.GetColumn(channel).CopyTo(block.AsSpan2D(pixels.Length, 4).GetColumn(component));
                if (header.Layout.Associated)
                    PixelFrame.Unassociated(pixels);
            }), primaries.Map(static gamut => new ColorEncoding(gamut, new Transfer.Linear())));

    private static Fin<((int Component, int Channel)[] Rows, bool Associated)> Components(IReadOnlyList<string> names, string layer) =>
        toSeq(
            (from family in Seq("RGBA", "XYZW", "UV")
             from entry in toSeq(names).Map(static (name, channel) => (Name: name, Channel: channel, Dot: name.LastIndexOf('.')))
             where entry.Name.Length == entry.Dot + 2 && entry.Name.AsSpan(0, int.Max(entry.Dot, 0)).SequenceEqual(layer)
             let component = family.IndexOf(entry.Name[^1], StringComparison.OrdinalIgnoreCase)
             where component >= 0
             select (Component: component, entry.Channel, Letter: entry.Name[^1])).DistinctBy(static row => row.Component)) switch {
                 [] => ImagingFault.Of(ImagingReason.Empty, layer),
                 var filled => (
                     (filled.Filter(static row => row.Component < 3) is [var lone]
                         ? filled + Seq(0, 1, 2).Filter(component => component != lone.Component).Map(component => (Component: component, lone.Channel, lone.Letter))
                         : filled).Map(static row => (row.Component, row.Channel)).ToArray(),
                     filled.Exists(static row => row is { Component: 3, Letter: 'A' or 'a' })),
             };

    private static Fin<Option<Gamut>> Primaries(Header header) =>
        toSeq(header.Attributes).Find(static attribute => attribute is { Name: FileMetadata.InteropId, TypeName: "string" })
            .Bind(static attribute => Gamut.Known.Find(known => known.InteropId == Some(System.Text.Encoding.UTF8.GetString(attribute.Data))))
            .Match(
                Some: static known => Some(known),
                None: () => Optional(header.Chromaticities).Traverse(static stated => Gamut.Of(
                    new Chromaticity(stated.RedX, stated.RedY), new Chromaticity(stated.GreenX, stated.GreenY), new Chromaticity(stated.BlueX, stated.BlueY),
                    new WhitePoint(stated.WhiteX, stated.WhiteY))).As());

    private static DecoderOptions Decoding(ColorProfileHandling handling) =>
        new() { MaxFrames = 1, SegmentIntegrityHandling = SegmentIntegrityHandling.Strict, ColorProfileHandling = handling };
}
