using System.Globalization;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Settings;
using Rasm.UI;
using Rhino;

namespace Darkroom.Output;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class LayerReader : ICaptioned {
    public static readonly LayerReader Blender = new("blender", new ReaderConvention.Blender(NormalSpace.World));
    public static readonly LayerReader NukeWorld = new("nuke-world", new ReaderConvention.Nuke(NormalSpace.World));
    public static readonly LayerReader NukeView = new("nuke-view", new ReaderConvention.Nuke(NormalSpace.View));

    public ReaderConvention Convention { get; }
}

[SmartEnum<string>]
public abstract partial class TargetFile : ICaptioned {
    public static readonly TargetFile Png = new RasterFile("png", StillFormat.PngRaster.Extensions.Head, StillFormat.PngRaster.From);
    public static readonly TargetFile Tiff = new RasterFile("tiff", StillFormat.TiffRaster.Extensions.Head, StillFormat.TiffRaster.From);
    public static readonly TargetFile Exr = new ExrFile("exr", StillFormat.ExrRaster.Extensions.Head);
    public static readonly TargetFile Dng = new DngFile("dng", StillFormat.Dng.Extensions.Head);
    public static readonly TargetFile Layered = new LayeredFile("layered", StillFormat.TiffRaster.Extensions.Head);
    public static readonly TargetFile Jpeg = new JpegFile("jpeg", StillFormat.JpegRaster.Extensions.Head);
    public static readonly TargetFile Bmp = new DibFile("bmp", StillFormat.BmpRaster.Extensions.Head, static (frame, alpha) => new StillFormat.BmpRaster(frame, alpha));
    public static readonly TargetFile Targa = new DibFile("targa", StillFormat.TgaRaster.Extensions.Head, static (frame, alpha) => new StillFormat.TgaRaster(frame, alpha));

    public string Extension { get; }

    public abstract IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target);

    private sealed class RasterFile(string key, string extension, Func<PixelFrame, OutputDepth, Option<ColorEncoding>, bool, Fin<StillFormat>> build)
        : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Formed(target, target.ColorDepth).Bind(formed => IO.lift(build(formed.Frame, target.ColorDepth, formed.Encoding, target.Alpha)));
    }

    private sealed class ExrFile(string key, string extension) : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            target.Reader.Match(
                Some: reader => source.Passes(doc, target, reader),
                None: () =>
                    from beauty in source.Linear
                    from layout in IO.lift(ExrLayout.From(target.ColorDepth, target.Codec, DisplayFormation.Working))
                    from still in IO.lift(StillFormat.ExrRaster.From(beauty, layout, target.Alpha, None))
                    select still);
    }

    private sealed class DngFile(string key, string extension) : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Linear.Bind(frame => IO.lift(StillFormat.Dng.From(
                frame, DisplayFormation.Working, None, source.Scene.FocalLength, source.Scene.Aperture.Map(static aperture => (float)aperture))));
    }

    private sealed class LayeredFile(string key, string extension) : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) => source.Layered(doc, target);
    }

    private sealed class JpegFile(string key, string extension) : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Formed(target, OutputDepth.UInt8).Map(formed => StillFormat.JpegRaster.From(formed.Frame, formed.Encoding, target.Quality));
    }

    private sealed class DibFile(string key, string extension, Func<PixelFrame, bool, StillFormat> build) : TargetFile(key, extension) {
        public override IO<StillFormat> Still(RhinoDoc doc, SaveSource source, OutputTarget target) =>
            source.Formed(target, OutputDepth.UInt8).Map(formed => build(formed.Frame, target.Alpha));
    }
}

public sealed record OutputTarget(
    TargetFile FileFormat, bool Alpha, OutputDepth ColorDepth, ExrCompression Codec, Option<LayerReader> Reader, bool Position, DisplayDevice DisplayDevice, JpegQuality Quality)
    : IStateRecord<OutputTarget, TargetParameter> {
    public static OutputTarget Default { get; } =
        new(TargetFile.Png, Alpha: false, OutputDepth.UInt8, ExrCompression.Zip, None, Position: false, DisplayFormation.Display, JpegQuality.Create(95));
}

[SmartEnum<string>]
public sealed partial class TargetParameter : IStateParameter<OutputTarget> {
    public static readonly TargetParameter FileFormat = new("file-format", new StateParameter<OutputTarget>.Choice<TargetFile>(
            Lens<OutputTarget, TargetFile>.New(static target => target.FileFormat, static value => target => target with { FileFormat = value })),
        new("File format", "Format of written file"));
    public static readonly TargetParameter Alpha = new("alpha", new StateParameter<OutputTarget>.Toggle(
            Lens<OutputTarget, bool>.New(static target => target.Alpha, static value => target => target with { Alpha = value })),
        new("Alpha", "Includes alpha where format holds it"));
    public static readonly TargetParameter ColorDepth = new("color-depth", new StateParameter<OutputTarget>.Choice<OutputDepth>(
            Lens<OutputTarget, OutputDepth>.New(static target => target.ColorDepth, static value => target => target with { ColorDepth = value })),
        new("Storage depth", "Bits per channel, native bitmap sources hold 8 bits"));
    public static readonly TargetParameter Codec = new("codec", new StateParameter<OutputTarget>.Choice<ExrCompression>(
            Lens<OutputTarget, ExrCompression>.New(static target => target.Codec, static value => target => target with { Codec = value })),
        new("Codec", "OpenEXR compression of color parts"));
    public static readonly TargetParameter Reader = new("reader", new StateParameter<OutputTarget>.OptionalChoice<LayerReader>(
            Lens<OutputTarget, Option<LayerReader>>.New(static target => target.Reader, static value => target => target with { Reader = value }), "Single part"),
        new("Layers", "Writes each render channel as a part named for chosen reader"));
    public static readonly TargetParameter Position = new("position", new StateParameter<OutputTarget>.Toggle(
            Lens<OutputTarget, bool>.New(static target => target.Position, static value => target => target with { Position = value })),
        new("Position", "Adds world position from distance channel and rendering camera"));
    public static readonly TargetParameter DisplayDevice = new("display-device", new StateParameter<OutputTarget>.Choice<DisplayDevice>(
            Lens<OutputTarget, DisplayDevice>.New(static target => target.DisplayDevice, static value => target => target with { DisplayDevice = value })),
        new("Display", "Display a captured frame is formed for, native bitmap saves keep Rhino display"));
    public static readonly TargetParameter Quality = new("quality", StateParameter.Bounded(
            Lens<OutputTarget, JpegQuality>.New(static target => target.Quality, static value => target => target with { Quality = value }), new Presentation<int>()),
        new("Quality", "JPEG compression quality"));

    public StateParameter<OutputTarget> Kind { get; }

    public ParameterText Text { get; }
}

public sealed record OutputBook(Seq<OutputTarget> Targets, Destination Destination) {
    private const string Owner = "output";

    private static readonly DictionaryCodec<ValueSet> Codec = DictionaryCodec.Json<ValueSet>(nameof(OutputBook), 3);

    public static OutputBook Empty { get; } = new([OutputTarget.Default], Destination.Default);

    // --- [NAMES]
    public Fin<Seq<(OutputTarget Target, OutputName Name)>> Names(Seq<NamePart> scope) =>
        Targets.Map((target, index) => (Target: target, Index: index)).Traverse(held =>
                FileExtension.Validated(held.Target.FileFormat.Extension).Map(extension => (held.Target, Name: new OutputName(
                    scope,
                    new OutputVersioning.NextFree(),
                    Targets.Take(held.Index).Exists(earlier => string.Equals(earlier.FileFormat.Extension, held.Target.FileFormat.Extension, StringComparison.Ordinal)) ? [NamePart.Ordinal(held.Index + 1)] : [],
                    None,
                    extension))))
            .As();

    // --- [ARCHIVE]
    public static IO<OutputBook> Read(RhinoDoc doc) =>
        ArchivableDictionaries.Read(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), Codec)
            .Bind(static stored => stored.Match(Some: Recalled, None: static () => IO.pure(Empty)));

    public IO<Unit> Written(RhinoDoc doc) =>
        ArchivableDictionaries.Write(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), Codec,
            new ValueSet(Targets.Map(static (target, index) => ValueSet.Fields(Owned(index), target).Entries).Fold(ValueSet.Fields(Owner, Destination).Entries, static (entries, target) => entries.Union(target))));

    private static IO<OutputBook> Recalled(ValueSet set) =>
        from targets in toSeq(Range(0, int.MaxValue).TakeWhile(index => set.Entries.Keys.Exists(key => string.Equals(key.Owner, Owned(index), StringComparison.Ordinal))))
            .TraverseM(index => set.Recalled(Owned(index), OutputTarget.Default))
            .As()
        from destination in set.Recalled(Owner, Destination.Default)
        select new OutputBook(targets, destination);

    private static string Owned(int index) => string.Create(CultureInfo.InvariantCulture, $"{Owner}-{index}");
}
