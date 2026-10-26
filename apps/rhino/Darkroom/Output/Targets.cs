using System.Text.Json.Serialization;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Settings;
using Rhino;
using Thinktecture.Text.Json.Serialization;

namespace Darkroom.Output;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class LayerReader {
    public static readonly LayerReader Blender = new("blender", new ReaderConvention.Blender(NormalSpace.World));
    public static readonly LayerReader NukeWorld = new("nuke-world", new ReaderConvention.Nuke(NormalSpace.World));
    public static readonly LayerReader NukeView = new("nuke-view", new ReaderConvention.Nuke(NormalSpace.View));

    public ReaderConvention Convention { get; }
}

[SmartEnum<string>]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public abstract partial class TargetFile {
    public static readonly TargetFile Png = new RasterFile("png", FileExtension.Create(StillFormat.PngRaster.Extensions.Head), StillFormat.PngRaster.From);
    public static readonly TargetFile Tiff = new RasterFile("tiff", FileExtension.Create(StillFormat.TiffRaster.Extensions.Head), StillFormat.TiffRaster.From);
    public static readonly TargetFile Exr = new ExrFile("exr", FileExtension.Create(StillFormat.ExrRaster.Extensions.Head));
    public static readonly TargetFile Dng = new DngFile("dng", FileExtension.Create(StillFormat.Dng.Extensions.Head));
    public static readonly TargetFile Layered = new LayeredFile("layered", FileExtension.Create(StillFormat.TiffRaster.Extensions.Head));
    public static readonly TargetFile Jpeg = new JpegFile("jpeg", FileExtension.Create(StillFormat.JpegRaster.Extensions.Head));
    public static readonly TargetFile Bmp = new DibFile("bmp", FileExtension.Create(StillFormat.BmpRaster.Extensions.Head), static (frame, alpha) => new StillFormat.BmpRaster(frame, alpha));
    public static readonly TargetFile Targa = new DibFile("targa", FileExtension.Create(StillFormat.TgaRaster.Extensions.Head), static (frame, alpha) => new StillFormat.TgaRaster(frame, alpha));

    public FileExtension Extension { get; }

    public abstract IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target);

    private sealed class RasterFile(string key, FileExtension extension, Func<PixelFrame, OutputDepth, Option<ColorEncoding>, bool, Fin<StillFormat>> build)
        : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Formed(target, target.ColorDepth).Bind(formed => IO.lift(build(formed.Frame, target.ColorDepth, formed.Encoding, target.Alpha)));
    }

    private sealed class ExrFile(string key, FileExtension extension) : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            Optional(target.Reader).Match(
                Some: reader => source.Passes(doc, target, reader),
                None: () =>
                    from beauty in source.Linear
                    from layout in IO.lift(ExrLayout.From(target.ColorDepth, target.Codec, DisplayFormation.Working))
                    from still in IO.lift(StillFormat.ExrRaster.From(beauty, layout, target.Alpha, None))
                    select still);
    }

    private sealed class DngFile(string key, FileExtension extension) : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Linear.Bind(frame => IO.lift(StillFormat.Dng.From(
                frame, DisplayFormation.Working, None, source.Scene.FocalLength, source.Scene.Aperture.Map(static aperture => (float)aperture))));
    }

    private sealed class LayeredFile(string key, FileExtension extension) : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) => source.Layered(doc, target);
    }

    private sealed class JpegFile(string key, FileExtension extension) : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Formed(target, OutputDepth.UInt8).Map(formed => StillFormat.JpegRaster.From(formed.Frame, formed.Encoding, target.Quality));
    }

    private sealed class DibFile(string key, FileExtension extension, Func<PixelFrame, bool, StillFormat> build) : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Formed(target, OutputDepth.UInt8).Map(formed => build(formed.Frame, target.Alpha));
    }
}

public sealed record OutputTarget(
    TargetFile FileFormat, bool Alpha, OutputDepth ColorDepth, ExrCompression Codec, LayerReader? Reader, bool Position, DisplayDevice DisplayDevice, int Quality) {
    public static OutputTarget Default { get; } =
        new(TargetFile.Png, Alpha: false, OutputDepth.UInt8, ExrCompression.Zip, Reader: null, Position: false, DisplayFormation.Display, Quality: 95);
}

public sealed record OutputBook(ImmutableArray<OutputTarget> Targets, OutputFolder Folder, Stem Stem, bool Overwrite) {
    private static readonly DictionaryCodec<OutputBook> Codec = ArchivableDictionaries.Json<OutputBook>(nameof(OutputBook), 4);

    public static OutputBook Empty { get; } = new([OutputTarget.Default], new OutputFolder.BesideDocument([]), new Stem.DocumentName(), Overwrite: false);

    // --- [NAMES]
    public Seq<(OutputTarget Target, OutputName Name)> Names(Seq<NamePart> scope) =>
        toHashMap(Targets.Index().DistinctBy(static held => held.Item.FileFormat.Extension).Select(static held => (held.Item.FileFormat.Extension, held.Index))) switch {
            var first => toSeq(Targets).Map((target, index) => (Target: target, Name: new OutputName(
                scope,
                new OutputVersioning.NextFree(),
                first[target.FileFormat.Extension] < index ? [NamePart.Ordinal(index + 1)] : [],
                None,
                target.FileFormat.Extension))),
        };

    // --- [ARCHIVE]
    public static IO<OutputBook> Read(RhinoDoc doc) =>
        ArchivableDictionaries.Read(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), Codec).Map(static stored => stored.IfNone(Empty));

    public IO<Unit> Written(RhinoDoc doc) => ArchivableDictionaries.Write(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), Codec, this);
}
