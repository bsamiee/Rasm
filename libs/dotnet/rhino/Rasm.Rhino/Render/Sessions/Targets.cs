using System.Globalization;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class LayerReader {
    public static readonly LayerReader Blender = new("blender", new ReaderConvention.Blender(NormalSpace.World));
    public static readonly LayerReader NukeWorld = new("nuke-world", new ReaderConvention.Nuke(NormalSpace.World));
    public static readonly LayerReader NukeView = new("nuke-view", new ReaderConvention.Nuke(NormalSpace.View));

    public ReaderConvention Convention { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TargetWrite {
    public static ColorEncoding SceneLinear { get; } = new(EffectPipeline.Working, TransferCurve.Linear, Nits.ReferenceWhite);

    public sealed record Dib : TargetWrite;

    public sealed record Formed(Func<PixelFrame, Fin<StillFormat>> Still) : TargetWrite;

    public sealed record Raw(Func<PixelFrame, Fin<StillFormat>> Still) : TargetWrite;

    public sealed record Layers(Fin<ExrLayout> Layout, LayerReader Reader) : TargetWrite;

    public sealed record Layered : TargetWrite;
}

[Union]
public abstract partial record TargetVerdict {
    public static TargetVerdict Unless(bool held, TargetVerdict lost) => held ? new Honored() : lost;

    public sealed record Honored : TargetVerdict;

    public sealed record RawChannels : TargetVerdict;

    public sealed record LossyColor : TargetVerdict;

    public sealed record NoSample : TargetVerdict;

    public sealed record NoLevels : TargetVerdict;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidRhinoValue>]
public abstract partial class TargetFile {
    public static readonly TargetFile Png = new FormedFile("png", StillFormat.PngRaster.Extensions[0], StillFormat.PngRaster.Depths,
        static (frame, depth, encoding, alpha) => StillFormat.PngRaster.From(frame, depth, Some(encoding), alpha));
    public static readonly TargetFile Tiff = new FormedFile("tiff", StillFormat.TiffRaster.Extensions[0], StillFormat.TiffRaster.Depths, StillFormat.TiffRaster.From);
    public static readonly TargetFile Exr = new ExrFile("exr", StillFormat.ExrRaster.Extensions[0]);
    public static readonly TargetFile Dng = new DngFile("dng", StillFormat.Dng.Extensions[0]);
    public static readonly TargetFile Layered = new LayeredFile("layered", StillFormat.TiffRaster.Extensions[0]);
    public static readonly TargetFile Jpeg = new DibFile("jpeg", StillFormat.JpegRaster.Extensions[0], holdsAlpha: false);
    public static readonly TargetFile Bmp = new DibFile("bmp", ".bmp", holdsAlpha: true);
    public static readonly TargetFile Targa = new DibFile("targa", ".tga", holdsAlpha: true);

    public string Extension { get; }

    public abstract TargetWrite Write(OutputTarget target);

    public abstract Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target);

    private static Option<TargetVerdict> Referred(TargetParameter parameter, OutputTarget target, Seq<OutputDepth> depths, Option<TargetVerdict> alpha) =>
        parameter.Map(
            fileFormat: TargetVerdict.Unless(target.ColorDepth.Levels.IsSome, new TargetVerdict.RawChannels()),
            alpha: alpha,
            colorDepth: TargetVerdict.Unless(depths.Exists(depth => depth == target.ColorDepth), new TargetVerdict.NoSample()),
            codec: None,
            reader: None,
            position: None,
            displayDevice: target.ColorDepth.Levels.Map<TargetVerdict>(static _ => new TargetVerdict.Honored()),
            dithering: TargetVerdict.Unless(target.ColorDepth.Levels.IsSome || target.Dithering == Dither.Off, new TargetVerdict.NoLevels()));

    private sealed class FormedFile(string key, string extension, Seq<OutputDepth> depths, Func<PixelFrame, OutputDepth, ColorEncoding, bool, Fin<StillFormat>> build)
        : TargetFile(key, extension) {
        public override TargetWrite Write(OutputTarget target) =>
            target.ColorDepth.Levels.IsSome
                ? new TargetWrite.Formed(frame => build(frame, target.ColorDepth, target.DisplayDevice.Encoding, target.Alpha))
                : new TargetWrite.Raw(frame => build(frame, target.ColorDepth, TargetWrite.SceneLinear, target.Alpha));

        public override Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target) =>
            Referred(parameter, target, depths, new TargetVerdict.Honored());
    }

    private sealed class ExrFile(string key, string extension) : TargetFile(key, extension) {
        public override TargetWrite Write(OutputTarget target) =>
            target.Reader.Match<TargetWrite>(
                Some: reader => new TargetWrite.Layers(Layout(target), reader),
                None: () => new TargetWrite.Raw(frame => Layout(target).Bind(layout => StillFormat.ExrRaster.From(frame, layout, target.Alpha, None).ToFin())));

        public override Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target) =>
            parameter.Map(
                fileFormat: new TargetVerdict.RawChannels(),
                alpha: new TargetVerdict.Honored(),
                colorDepth: TargetVerdict.Unless(ExrLayout.Depths.Exists(depth => depth == target.ColorDepth), new TargetVerdict.NoSample()),
                codec: TargetVerdict.Unless(target.Codec.Codec == target.Codec.Data, new TargetVerdict.LossyColor()),
                reader: new TargetVerdict.Honored(),
                position: target.Reader.Map<TargetVerdict>(static _ => new TargetVerdict.Honored()),
                displayDevice: None,
                dithering: None);

        private static Fin<ExrLayout> Layout(OutputTarget target) => ExrLayout.From(target.ColorDepth, target.Codec, EffectPipeline.Working);
    }

    private sealed class DngFile(string key, string extension) : TargetFile(key, extension) {
        public override TargetWrite Write(OutputTarget target) =>
            new TargetWrite.Raw(static frame => StillFormat.Dng.From(frame, EffectPipeline.Working, Exposure.Neutral, None, None).ToFin());

        public override Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target) =>
            parameter.Map<Option<TargetVerdict>>(
                fileFormat: new TargetVerdict.RawChannels(), alpha: None, colorDepth: None, codec: None, reader: None, position: None, displayDevice: None, dithering: None);
    }

    private sealed class LayeredFile(string key, string extension) : TargetFile(key, extension) {
        public override TargetWrite Write(OutputTarget target) => new TargetWrite.Layered();

        public override Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target) =>
            Referred(parameter, target, LayeredDocument.Depths, None);
    }

    private sealed class DibFile(string key, string extension, bool holdsAlpha) : TargetFile(key, extension) {
        public override TargetWrite Write(OutputTarget target) => new TargetWrite.Dib();

        public override Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target) =>
            parameter.Map(
                fileFormat: new TargetVerdict.Honored(),
                alpha: holdsAlpha ? Some<TargetVerdict>(new TargetVerdict.Honored()) : None,
                colorDepth: None, codec: None, reader: None, position: None, displayDevice: None, dithering: None);
    }
}

public sealed record OutputTarget(
    TargetFile FileFormat, bool Alpha, OutputDepth ColorDepth, ExrCompression Codec, Option<LayerReader> Reader, bool Position,
    Imaging.Tone.Formations.Display DisplayDevice, Dither Dithering)
    : IStateRecord<OutputTarget, TargetParameter, InvalidRhinoValue> {
    public static OutputTarget Default { get; } =
        new(TargetFile.Png, Alpha: false, EffectPipeline.Depth, ExrCompression.Zip, None, Position: false, EffectPipeline.Display, Dither.WhiteNoise);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class TargetParameter : IStateParameter<OutputTarget> {
    public static readonly TargetParameter FileFormat = new("file-format", new StateParameter<OutputTarget>.Choice<TargetFile, InvalidRhinoValue>(
        Lens<OutputTarget, TargetFile>.New(static target => target.FileFormat, static value => target => target with { FileFormat = value })));
    public static readonly TargetParameter Alpha = new("alpha", new StateParameter<OutputTarget>.Toggle(
        Lens<OutputTarget, bool>.New(static target => target.Alpha, static value => target => target with { Alpha = value })));
    public static readonly TargetParameter ColorDepth = new("color-depth", new StateParameter<OutputTarget>.Choice<OutputDepth, InvalidQuantization>(
        Lens<OutputTarget, OutputDepth>.New(static target => target.ColorDepth, static value => target => target with { ColorDepth = value })));
    public static readonly TargetParameter Codec = new("codec", new StateParameter<OutputTarget>.Choice<ExrCompression, InvalidOutput>(
        Lens<OutputTarget, ExrCompression>.New(static target => target.Codec, static value => target => target with { Codec = value })));
    public static readonly TargetParameter Reader = new("reader", new StateParameter<OutputTarget>.OptionalChoice<LayerReader, InvalidRhinoValue>(
        Lens<OutputTarget, Option<LayerReader>>.New(static target => target.Reader, static value => target => target with { Reader = value })));
    public static readonly TargetParameter Position = new("position", new StateParameter<OutputTarget>.Toggle(
        Lens<OutputTarget, bool>.New(static target => target.Position, static value => target => target with { Position = value })));
    public static readonly TargetParameter DisplayDevice = new("display-device", new StateParameter<OutputTarget>.Choice<Imaging.Tone.Formations.Display, InvalidToneValue>(
        Lens<OutputTarget, Imaging.Tone.Formations.Display>.New(static target => target.DisplayDevice, static value => target => target with { DisplayDevice = value })));
    public static readonly TargetParameter Dithering = new("dithering", new StateParameter<OutputTarget>.Choice<Dither, InvalidQuantization>(
        Lens<OutputTarget, Dither>.New(static target => target.Dithering, static value => target => target with { Dithering = value })));

    public StateParameter<OutputTarget> Kind { get; }
}

public sealed record OutputBook(Seq<OutputTarget> Targets, Option<int> Current, Destination Destination) {
    // --- [TARGETS]
    public static OutputBook Empty { get; } = new(Seq(OutputTarget.Default), Some(0), Destination.Default);

    public Option<OutputTarget> Shown => Current.Bind(index => Targets.At(index));

    public Seq<(OutputTarget Target, Seq<NamePart> Parts)> Named =>
        Targets.Map((target, index) => (Target: target, Parts: Targets.Take(index).Exists(earlier => string.Equals(earlier.FileFormat.Extension, target.FileFormat.Extension, StringComparison.Ordinal))
            ? Seq(NamePart.Ordinal(index + 1))
            : []));

    // --- [ARCHIVE]
    private const string Owner = "output";
    private const string CurrentKey = "current";

    public ValueSet Captured =>
        new(toHashMap(
            Targets.Map(static (target, index) => Entries(Owned(index), target)).Flatten()
            + Entries(Owner, Destination)
            + Current.Map(static index => (Key: new EntryKey(Owner, [CurrentKey]), Text: index.ToString(CultureInfo.InvariantCulture))).ToSeq()));

    public static IO<OutputBook> Recalled(ValueSet set) =>
        from targets in toSeq(Range(0, int.MaxValue).TakeWhile(index => set.Entries.Keys.Exists(key => string.Equals(key.Owner, Owned(index), StringComparison.Ordinal))))
            .TraverseM(index => FieldTexts.Recalled<OutputTarget>(Owned(index), path => set.Entries.Find(new EntryKey(Owned(index), path))))
            .As()
        from destination in FieldTexts.Recalled<Destination>(Owner, path => set.Entries.Find(new EntryKey(Owner, path)))
        select new OutputBook(targets, set.Entries.Find(new EntryKey(Owner, [CurrentKey])).Bind(parseInt).Filter(index => targets.At(index).IsSome), destination);

    private static string Owned(int index) => string.Create(CultureInfo.InvariantCulture, $"{Owner}-target-{index}");

    private static Seq<(EntryKey Key, string Text)> Entries<TRecord>(string owner, TRecord record) where TRecord : IStateRecord<TRecord> =>
        FieldTexts<TRecord>.Items.Choose(item => item.Capture(record).Map(text => (Key: new EntryKey(owner, item.Path), Text: text)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TargetRows {
    // --- [TEXT]
    public static string DitherText(Dither method) =>
        method.Map(off: "Off", whiteNoise: "White noise", blueNoise: "Blue noise", floydSteinberg: "Floyd-Steinberg");

    private static string FileText(TargetFile file) =>
        file.Map(png: "PNG", tiff: "TIFF", exr: "OpenEXR", dng: "DNG", layered: "Layered TIFF", jpeg: "JPEG", bmp: "BMP", targa: "Targa");

    private static string DepthText(OutputDepth depth) =>
        depth.Map(uInt8: "8", uInt10: "10", uInt12: "12", uInt14: "14", uInt16: "16", float16: "Float (Half)", float32: "Float (Full)");

    private static ParameterText Text(TargetParameter parameter) =>
        parameter.Map(
            fileFormat: ParameterText.Choice<TargetFile, InvalidRhinoValue>("File format", "Format the file is written in", FileText),
            alpha: ParameterText.Of("Alpha", "Writes RGBA, off writes RGB over black"),
            colorDepth: ParameterText.Choice<OutputDepth, InvalidQuantization>("Color depth", "Bits per channel", DepthText),
            codec: ParameterText.Choice<ExrCompression, InvalidOutput>("Codec", "OpenEXR compression of the color parts", static codec => codec.Map(
                uncompressed: "None", rle: "RLE (lossless)", zips: "ZIPS (lossless)", zip: "ZIP (lossless)", piz: "PIZ (lossless)", pxr24: "Pxr24 (lossy)", b44: "B44 (lossy)", b44A: "B44A (lossy)")),
            reader: ParameterText.Choice<LayerReader, InvalidRhinoValue>("Layers", "Writes each render channel as a part named for its reader", "Single part",
                static reader => reader.Map(blender: "Blender", nukeWorld: "Nuke, world normals", nukeView: "Nuke, view normals")),
            position: ParameterText.Of("Position", "Adds world position from the distance channel and the rendering's camera"),
            displayDevice: ParameterText.Choice<Imaging.Tone.Formations.Display, InvalidToneValue>("Display", "Display the file is formed for and tagged as", static display => display.Map(
                srgb: "sRGB", displayP3: "Display P3", rec1886: "Rec.1886", rec2020: "Rec.2020", rec2100PqHdr: "Rec.2100-PQ, HDR 1000 nits", rec2100PqSdr: "Rec.2100-PQ, SDR",
                rec2100HlgHdr: "Rec.2100-HLG, HDR 1000 nits", rec2100HlgSdr: "Rec.2100-HLG, SDR")),
            dithering: ParameterText.Choice<Dither, InvalidQuantization>("Dither", "Noise or diffused error added before the file's integer codes", DitherText));

    private static ParameterText DestinationText(DestinationParameter parameter) =>
        parameter.Map(
            folder: ParameterText.Of("Folder", "Folder the files are written to"),
            stem: ParameterText.Of("Name", "File name stem every target extends"),
            overwrite: ParameterText.Of("Overwrite", "Replaces a file at the path, off takes the next free version"));

    private static string VerdictText(TargetVerdict verdict) =>
        RowText.Localize(verdict.Map(
            honored: "Written as chosen",
            rawChannels: "Written from the renderer's channels, ahead of every post effect",
            lossyColor: "Color parts lossy, data parts lossless",
            noSample: "Refused, the format holds no sample at this depth",
            noLevels: "Refused, a float depth holds no levels to dither")).Local;

    private static readonly Seq<(TargetParameter Parameter, ParameterText Text)> Texts = ParameterText.Join<OutputTarget, TargetParameter, InvalidRhinoValue>(Text);
    private static readonly Seq<(DestinationParameter Parameter, ParameterText Text)> DestinationTexts = ParameterText.Join<Destination, DestinationParameter, InvalidRhinoValue>(DestinationText);

    // --- [BOOK]
    public static IO<OutputBook> Book(RhinoDoc doc) =>
        ArchivableDictionaries.Read(SceneSources.Dictionary(doc), ValueSet.Codec)
            .Bind(static stored => stored.Match(Some: OutputBook.Recalled, None: static () => IO.pure(OutputBook.Empty)));

    public static ReadModel<OutputTarget> Shown { get; } = new(
        [ReadModel.Marks(EventKind.DocumentPropertiesChanged), ReadModel.Marks(EventKind.UndoRedo)],
        static doc => Book(doc).Map(static book => book.Shown.IfNone(OutputTarget.Default)));

    private static IO<Unit> Write(RhinoDoc doc, OutputBook book) =>
        ArchivableDictionaries.Write(SceneSources.Dictionary(doc), ValueSet.Codec, book.Captured);

    private static IO<RhinoDoc> Document(RowScope scope) => IO.lift(() => scope.Document.ToFin(new Missing(nameof(RowScope.Document))));

    private static Option<HostEvent<Unit>> Signal(RowScope scope) =>
        scope.Document.Map(static doc => ValueStore.Signal(EventKind.DocumentPropertiesChanged.In(doc.RuntimeSerialNumber), static _ => true));

    private static ValueStore<T> Booked<T>(RowScope scope, Func<OutputBook, Option<T>> read, Func<OutputBook, Option<T>, OutputBook> write) where T : notnull =>
        ValueStore.Of(
            from doc in Document(scope)
            from book in Book(doc)
            select read(book),
            value =>
                from doc in Document(scope)
                from book in Book(doc)
                from written in Write(doc, write(book, value))
                select written,
            Applied.Live,
            Signal(scope));

    // --- [ROWS]
    private const string Owner = "output-targets";

    private static readonly (RowSource<Seq<(OutputTarget Target, int Index)>> Source, RowField<Seq<(OutputTarget Target, int Index)>> Field) Listed =
        RowSource.Opaque(Owner, "targets", Seq<(OutputTarget Target, int Index)>(), static scope => Booked(scope,
            static book => Some(book.Targets.Map(static (target, index) => (Target: target, Index: index))),
            static (book, rows) => book with { Targets = rows.Map(static held => held.Map(static row => row.Target)).IfNone(OutputBook.Empty.Targets) }));

    private static readonly (RowSource<LanguageExt.HashSet<int>> Source, RowField<LanguageExt.HashSet<int>> Field) Picked =
        RowSource.Opaque(Owner, "current", [], static scope => Booked(scope,
            static book => Some(toHashSet(book.Current.ToSeq())),
            static (book, picked) => book with { Current = picked.Bind(static set => toSeq(set).Head) }));

    private static readonly RowSource<OutputTarget> Chosen =
        SectionRows.Source(Owner, Texts, static scope => IO.pure(Seq(Booked(scope,
            static book => book.Shown,
            static (book, target) => book with { Targets = book.Targets.Map((held, index) => book.Current.Exists(at => at == index) ? target.IfNone(OutputTarget.Default) : held) }))));

    private static readonly RowSource<Destination> Destined =
        SectionRows.Source(Owner, DestinationTexts, static scope => IO.pure(Seq(Booked(scope,
            static book => Some(book.Destination),
            static (book, destination) => book with { Destination = destination.IfNone(Destination.Default) }))));

    private static RowRules Rules(TargetParameter parameter) =>
        new(None, Some(RowRule.When(Chosen, target => target.FileFormat.Verdict(parameter, target).IsSome, TargetParameter.FileFormat, TargetParameter.ColorDepth, TargetParameter.Reader)));

    // --- [PAGE]
    public static Seq<ControlRow> Page { get; } =
        Seq(ListRows.List("Targets", "Files each save writes from one rendering", new ListRow<(OutputTarget Target, int Index), int>(
                IterableNE.create<ListColumn<(OutputTarget Target, int Index)>>(
                    new ListColumn<(OutputTarget Target, int Index)>.Label(None, static row => FileText(row.Target.FileFormat), None, None),
                    new ListColumn<(OutputTarget Target, int Index)>.Label(None, static row => DepthText(row.Target.ColorDepth), None, None)),
                static row => row.Index,
                Listed,
                Picked) {
            Pair = Some((
                new CommandRow.Run(new CommandFace("output-target-add", "Add Target", "Add a file each save writes", None, None), None, static scope =>
                    from doc in Document(scope)
                    from book in Book(doc)
                    from written in Write(doc, book with { Targets = book.Targets.Add(OutputTarget.Default), Current = Some(book.Targets.Count) })
                    select written),
                new CommandRow.Run(new CommandFace("output-target-remove", "Remove Targets", "Remove the selected targets", None, None),
                    Some(RowRule.When(Picked.Source, static picked => !picked.IsEmpty, Picked.Field.Parameter)), static scope =>
                        from doc in Document(scope)
                        from picked in scope.Read(Picked.Source)
                        from book in Book(doc)
                        let kept = toSeq(book.Targets.Where((_, index) => !picked.Contains(index)))
                        from written in Write(doc, book with { Targets = kept, Current = kept.IsEmpty ? None : Some(0) })
                        select written))),
        },
            RowRules.Always))
        + SectionRows.Of(Chosen, Texts, SectionRows.Unstaged, Rules)
        + Seq<ControlRow>(new ControlRow.Group("Written as", RowRules.Always))
        + Texts.Map(static pair => TextRows.Readout(
            pair.Text.Caption, pair.Text.Help, new ReadoutSource<OutputTarget>.Model(Shown),
            target => target.FileFormat.Verdict(pair.Parameter, target).Match(Some: VerdictText, None: static () => ""),
            Rules(pair.Parameter)))
        + SectionRows.Of(Destined, DestinationTexts, SectionRows.Unstaged, static _ => RowRules.Always)
        + Seq(
            ListRows.List("Paths", "Files the next save writes", new ListRow<(OutputName Name, OutputPath Path), string>(
                    IterableNE.create<ListColumn<(OutputName Name, OutputPath Path)>>(new ListColumn<(OutputName Name, OutputPath Path)>.Label(None, static row => row.Path, None, None)),
                    static row => row.Path,
                    RowSource.Opaque(Owner, "paths", Seq<(OutputName Name, OutputPath Path)>(), static scope => ValueStore.Of(
                        from doc in Document(scope)
                        from book in Book(doc)
                        from listed in Saves.Preview(doc, book)
                        select Some(listed),
                        static _ => IO.pure(unit),
                        Applied.Live,
                        Signal(scope))),
                    RowSource.Opaque(Owner, "path-picked", LanguageExt.HashSet<string>.Empty)),
                RowRules.Always),
            SectionRows.Pushed(new CommandRow.Run(new CommandFace("output-save", "Save rendering", "Write every target of the newest rendering", None, None), None, static scope =>
                from doc in Document(scope)
                from book in Book(doc)
                from written in Saves.Newest(doc, scope.Sink, book)
                select unit)));
}
