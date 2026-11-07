using System.Globalization;
using System.Numerics;
using Darkroom.Chain;
using Darkroom.Queue;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Settings;
using Rhino;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using TinyEXR;

namespace Darkroom.Output;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
internal sealed partial class LayerReader {
    public static readonly LayerReader Blender = new("blender", ChannelNaming.Blender, view: false);
    public static readonly LayerReader NukeWorld = new("nuke-world", ChannelNaming.Nuke, view: false);
    public static readonly LayerReader NukeView = new("nuke-view", ChannelNaming.Nuke, view: true);

    public ChannelNaming Naming { get; }
    public bool View { get; }
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class TargetFile {

    // --- [ITEMS]

    private static readonly Seq<string> Rgb = ["R", "G", "B"];

    internal static readonly Seq<string> Rgba = ["R", "G", "B", "A"];

    public static readonly TargetFile Png = new("png", FileExtension.Create(".png"), [OutputDepth.UInt8], Encoded(static (_, source, target) =>
        source.Formed(target.ColorDepth).Map(formed =>
            (ImageFile)new ImageFile.Raster(formed.Frame, formed.Encoding, target.Alpha, new PngEncoder { BitDepth = PngBitDepth.Bit8, ColorType = target.Alpha ? PngColorType.RgbWithAlpha : PngColorType.Rgb }))));
    public static readonly TargetFile Tiff = new("tiff", FileExtension.Create(".tif"), [OutputDepth.UInt8, OutputDepth.Float16, OutputDepth.Float32], Encoded(static (_, source, target) =>
        source.Formed(target.ColorDepth).Map(formed => (ImageFile)new ImageFile.Tiff(formed.Frame, target.ColorDepth, formed.Encoding, target.Alpha))));
    public static readonly TargetFile Exr = new("exr", FileExtension.Create(".exr"), [OutputDepth.Float16, OutputDepth.Float32], Encoded(static (doc, source, target) =>
        target.Reader.Match(
                Some: reader => Passes(doc, source, target, reader),
                None: () => source.Linear.Map(beauty => Seq(new ExrLayer("", beauty, target.Alpha ? Rgba : Rgb))))
            .Map(layers => (ImageFile)new ImageFile.Exr(
                layers, target.ColorDepth == OutputDepth.Float16 ? PixelType.Half : PixelType.Float, target.Codec, Some(ChainScene.Working.Gamut), Multipart: target.Reader.IsSome))));
    public static readonly TargetFile Dng = new("dng", FileExtension.Create(".dng"), [OutputDepth.Float32], Encoded(static (_, source, _) =>
        source.Linear.Map(frame => (ImageFile)new ImageFile.Dng(
            frame, ChainScene.Working.Gamut, None, source.Scene.FocalLength, source.Scene.Camera.Bind(static camera => camera.Focus).Map(static focus => (float)focus.FNumber)))));
    public static readonly TargetFile Layered = new("layered", FileExtension.Create(".tif"), [OutputDepth.UInt8, OutputDepth.Float32], Encoded(Document));
    public static readonly TargetFile Jpeg = new("jpeg", FileExtension.Create(".jpg"), [OutputDepth.UInt8], Encoded(static (_, source, target) =>
        source.Formed(OutputDepth.UInt8).Map(formed => (ImageFile)new ImageFile.Raster(formed.Frame, formed.Encoding, Alpha: false, new JpegEncoder { Quality = target.Quality }))));
    public static readonly TargetFile Bmp = new("bmp", FileExtension.Create(".bmp"), [OutputDepth.UInt8], Windowed);
    public static readonly TargetFile Targa = new("targa", FileExtension.Create(".tga"), [OutputDepth.UInt8], Windowed);

    public FileExtension Extension { get; }

    public Seq<OutputDepth> Depths { get; }

    [UseDelegateFromConstructor]
    public partial IO<Unit> Write(RhinoDoc doc, SaveSource source, OutputTarget target, FileMetadata metadata, OutputPath path, bool overwrite);

    // --- [WRITES]

    private static Func<RhinoDoc, SaveSource, OutputTarget, FileMetadata, OutputPath, bool, IO<Unit>> Encoded(Func<RhinoDoc, SaveSource, OutputTarget, IO<ImageFile>> still) =>
        (doc, source, target, metadata, path, overwrite) =>
            from file in still(doc, source, target)
            from written in FilePublication.Write(path, overwrite, stream => file.Write(metadata, stream))
            select written;

    private static IO<Unit> Windowed(RhinoDoc doc, SaveSource source, OutputTarget target, FileMetadata metadata, OutputPath path, bool overwrite) =>
        IO.lift(() => Path.Join(Path.GetTempPath(), $"{Guid.NewGuid():N}{Path.GetExtension(path)}")).Bind(scratch =>
            (from saved in IO.lift(() => source.Rendering.SaveRenderImageAs(scratch, target.Alpha))
             from written in FilePublication.Write(path, overwrite, stream =>
                 (from read in use(() => File.OpenRead(scratch))
                  from copied in IO.liftAsync(environment => read.CopyToAsync(stream, environment.Token).ToUnit())
                  select copied).Bracket())
             select written).Finally(IO.lift(() => File.Delete(scratch))));

    // --- [LAYERS]

    private static IO<Seq<ExrLayer>> Passes(RhinoDoc doc, SaveSource source, OutputTarget target, LayerReader reader) =>
        from beauty in source.Linear
        from channels in source.Channels(target.Position)
        from position in IO.lift(target.Position
            ? (channels.Find(static held => held.Channel == SavedChannel.Distance).ToFin(new DarkroomFault(nameof(SavedChannel.Distance), DarkroomReason.Missing)),
               source.Scene.Camera.ToFin(new DarkroomFault("scene camera", DarkroomReason.Missing)),
               source.Scene.Meters.ToFin(new DarkroomFault("scene meters", DarkroomReason.Missing)))
                .Apply((distance, camera, meters) => Seq(reader.Naming.Position(GuideChannel.Depth.Measured(distance.Frame, meters), camera)))
                .As()
            : Seq<ExrLayer>())
        let encode = ChainScene.Display.Encoding.To(ChainScene.Working)
        select reader.Naming.Beauty(beauty, target.Alpha).Cons(channels.Bind(held =>
            held.Channel.Layers(held.Channel.PartName, held.Frame, reader, source.Scene.Camera)
            + Mattes(held.Channel.Identified(doc, held.Frame), encode) switch {
                var mattes => mattes.IsEmpty ? Seq<ExrLayer>() : new ExrLayer(held.Channel.Key.ToString(), ImageFile.Matte(held.Frame, toMap(mattes.Map(static matte => (matte.Id, matte.Color)))), Rgba)
                    .Cons(mattes.Map(matte => new ExrLayer(Named(matte), ImageFile.Matte(held.Frame, Map((matte.Id, matte.Color))), Rgba))),
            })) + position;

    private static IO<ImageFile> Document(RhinoDoc doc, SaveSource source, OutputTarget target) =>
        from beauty in source.Formed(target.ColorDepth)
        from channels in source.Channels(target.Position)
        let encode = beauty.Encoding.Map(ChainScene.Display.Encoding.To).IfNone(LutTable.Of([]))
        from document in IO.lift(ImageFile.Document.From(target.ColorDepth, beauty.Encoding,
            new DocumentLayer.Raster("RGBA", beauty.Frame, BlendingMode.Normal, 1f, Visible: true)
                .Cons(channels.Filter(held => target.ColorDepth.Levels.IsNone || held.Channel.Sources.IsSome).Map(held => held.Channel.Sources.IsSome
                    ? new DocumentLayer.Folder(held.Channel.PartName, None, 1f, Visible: true, Open: false, Mattes(held.Channel.Identified(doc, held.Frame), encode).Map(matte =>
                        (DocumentLayer)new DocumentLayer.Raster(Named(matte), ImageFile.Matte(held.Frame, Map((matte.Id, matte.Color))), BlendingMode.Normal, 1f, Visible: true)))
                    : (DocumentLayer)new DocumentLayer.Raster(held.Channel.PartName, held.Frame, BlendingMode.Normal, 1f, Visible: false)))))
        select (ImageFile)document;

    private static Seq<IdentitySource> Mattes(Map<ushort, IdentitySource> identified, LutTable encode) =>
        identified.Values.Map(source => {
            Vector4[] pixel = [new(source.Color, 1f)];
            encode.Apply(pixel);
            return source with { Color = pixel[0].AsVector3() };
        }).ToSeq().Strict();

    private static string Named(IdentitySource source) => source.Name.IfNone(() => source.Id.ToString(CultureInfo.InvariantCulture));
}

internal sealed record OutputTarget(TargetFile FileFormat, bool Alpha, OutputDepth ColorDepth, TinyEXR.Compression Codec, Option<LayerReader> Reader, bool Position, int Quality) {
    public static OutputTarget Default { get; } = new(TargetFile.Png, Alpha: false, OutputDepth.UInt8, TinyEXR.Compression.ZIP, Reader: None, Position: false, Quality: 95);

    public Fin<Unit> Encodable => DarkroomFault.Unless(FileFormat.Depths.Exists(depth => depth == ColorDepth), DarkroomReason.Invalid, (FileFormat.Key, ColorDepth.Key));

    public OutputTarget Retyped(TargetFile file) => this with { FileFormat = file, ColorDepth = file.Depths.Exists(depth => depth == ColorDepth) ? ColorDepth : file.Depths[0] };
}

internal sealed record OutputBook(Lst<OutputTarget> Targets, OutputFolder Folder, Stem Stem, bool Overwrite) {

    // --- [STATE]

    private static readonly DictionaryCodec<OutputBook> Codec = ArchivableDictionaries.Json<OutputBook>(nameof(OutputBook), 5);

    public static OutputBook Empty { get; } = new([OutputTarget.Default], new OutputFolder.BesideDocument([]), new Stem.DocumentName(), Overwrite: false);

    // --- [NAMES]

    public Seq<QueueOutput<Unit>> Names(Seq<NamePart> scope) =>
        toHashMap(Targets.Index().DistinctBy(static held => held.Item.FileFormat.Extension).Select(static held => (held.Item.FileFormat.Extension, held.Index))) switch {
            var first => toSeq(Targets).Map((target, index) => new QueueOutput<Unit>(target, new OutputName(
                scope,
                new OutputVersioning.Next(),
                first[target.FileFormat.Extension] < index ? [NamePart.Ordinal(index + 1)] : [],
                None,
                target.FileFormat.Extension), unit)),
        };

    // --- [ARCHIVES]

    public static IO<OutputBook> Read(RhinoDoc doc) =>
        ArchivableDictionaries.Read(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), Codec).Map(static stored => stored.IfNone(Empty));

    public IO<Unit> Written(RhinoDoc doc) => ArchivableDictionaries.Write(RenderSettingsSources.Dictionary(new RenderSettingsSource.Live(doc)), Codec, this);
}
