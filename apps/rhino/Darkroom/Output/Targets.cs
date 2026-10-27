using System.Text.Json.Serialization;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Rhino;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Settings;
using Rhino;
using Rhino.Render;
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
    public static readonly TargetFile Bmp = new WindowFile("bmp", FileExtension.Create(".bmp"));
    public static readonly TargetFile Targa = new WindowFile("targa", FileExtension.Create(".tga"));

    public FileExtension Extension { get; }

    public abstract IO<Unit> Write(RhinoDoc doc, SaveSource source, OutputTarget target, FileMetadata metadata, OutputPath path, bool overwrite);

    private abstract class StillFile(string key, FileExtension extension) : TargetFile(key, extension) {
        public override IO<Unit> Write(RhinoDoc doc, SaveSource source, OutputTarget target, FileMetadata metadata, OutputPath path, bool overwrite) =>
            Still(doc, source, target).Bind(still => FilePublication.Write(path, overwrite, stream => StillWriter.Encode(still, metadata, stream)));

        protected abstract IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target);
    }

    private sealed class RasterFile(string key, FileExtension extension, Func<PixelFrame, OutputDepth, Option<ColorEncoding>, bool, Fin<StillFormat>> build)
        : StillFile(key, extension) {
        protected override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Formed(target.ColorDepth).Bind(formed => IO.lift(build(formed.Frame, target.ColorDepth, formed.Encoding, target.Alpha)));
    }

    private sealed class ExrFile(string key, FileExtension extension) : StillFile(key, extension) {
        protected override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            Optional(target.Reader).Match(
                Some: reader => source.Passes(doc, target, reader),
                None: () =>
                    from beauty in source.Linear
                    from layout in IO.lift(ExrLayout.From(target.ColorDepth, target.Codec, DisplayFormation.Working))
                    from still in IO.lift(StillFormat.ExrRaster.From(beauty, layout, target.Alpha, None))
                    select still);
    }

    private sealed class DngFile(string key, FileExtension extension) : StillFile(key, extension) {
        protected override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Linear.Bind(frame => IO.lift(StillFormat.Dng.From(
                frame, DisplayFormation.Working, None, source.Scene.FocalLength, source.Scene.Aperture.Map(static aperture => (float)aperture))));
    }

    private sealed class LayeredFile(string key, FileExtension extension) : StillFile(key, extension) {
        protected override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) => source.Layered(doc, target);
    }

    private sealed class JpegFile(string key, FileExtension extension) : StillFile(key, extension) {
        protected override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Formed(OutputDepth.UInt8).Map(formed => StillFormat.JpegRaster.From(formed.Frame, formed.Encoding, target.Quality));
    }

    private sealed class WindowFile(string key, FileExtension extension) : TargetFile(key, extension) {
        public override IO<Unit> Write(RhinoDoc doc, SaveSource source, OutputTarget target, FileMetadata metadata, OutputPath path, bool overwrite) =>
            IO.lift(() => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{Extension}")).Bind(scratch =>
                (from saved in IO.lift(() => source.Rendering.SaveRenderImageAs(scratch, target.Alpha))
                 from found in IO.lift(() => RhinoFault.Unless(File.Exists(scratch)))
                 from written in FilePublication.Write(path, overwrite, stream =>
                     from read in use(() => File.OpenRead(scratch))
                     from copied in IO.liftAsync(async environment => {
                         await read.CopyToAsync(stream, environment.Token).ConfigureAwait(false);
                         return unit;
                     })
                     select unit)
                 select unit)
                .Finally(IO.lift(() => File.Delete(scratch))));
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
