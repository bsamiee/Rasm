using System.Globalization;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerReader : ICaptioned {
    public static readonly LayerReader Blender = new("blender", "Blender", new ReaderConvention.Blender(NormalSpace.World));
    public static readonly LayerReader NukeWorld = new("nuke-world", "Nuke, world normals", new ReaderConvention.Nuke(NormalSpace.World));
    public static readonly LayerReader NukeView = new("nuke-view", "Nuke, view normals", new ReaderConvention.Nuke(NormalSpace.View));

    public string Caption { get; }

    public ReaderConvention Convention { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TargetWrite {
    public static ColorEncoding SceneLinear { get; } = new(EffectPipeline.Working, TransferCurve.Linear, Nits.ReferenceWhite);

    public sealed record Raster(OutputDepth Depth, Func<PixelFrame, Option<ColorEncoding>, Fin<StillFormat>> Still) : TargetWrite;

    public sealed record Dib(Func<PixelFrame, Fin<StillFormat>> Still) : TargetWrite;

    public sealed record Raw(Func<PixelFrame, Option<RenderRun>, Fin<StillFormat>> Still) : TargetWrite;

    public sealed record Layers(Fin<ExrLayout> Layout, LayerReader Reader) : TargetWrite;

    public sealed record Layered : TargetWrite;
}

[Union]
public abstract partial record TargetVerdict {
    public static TargetVerdict Unless(bool held, TargetVerdict lost) => held ? new Honored() : lost;

    public static TargetVerdict Dithered(OutputDepth depth, Dither dithering) => Unless(depth.Levels.IsSome || dithering == Dither.Off, new NoLevels());

    public sealed record Honored : TargetVerdict;

    public sealed record RawChannels : TargetVerdict;

    public sealed record LossyColor : TargetVerdict;

    public sealed record NoSample : TargetVerdict;

    public sealed record NativeDisplay : TargetVerdict;

    public sealed record NoLevels : TargetVerdict;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class TargetFile : ICaptioned {
    public static readonly TargetFile Png = new RasterFile("png", StillFormat.PngRaster.Extensions.Head, "PNG", StillFormat.PngRaster.Depths, StillFormat.PngRaster.From);
    public static readonly TargetFile Tiff = new RasterFile("tiff", StillFormat.TiffRaster.Extensions.Head, "TIFF", StillFormat.TiffRaster.Depths, StillFormat.TiffRaster.From);
    public static readonly TargetFile Exr = new ExrFile("exr", StillFormat.ExrRaster.Extensions.Head, "OpenEXR");
    public static readonly TargetFile Dng = new DngFile("dng", StillFormat.Dng.Extensions.Head, "DNG");
    public static readonly TargetFile Layered = new LayeredFile("layered", StillFormat.TiffRaster.Extensions.Head, "Layered TIFF");
    public static readonly TargetFile Jpeg = new JpegFile("jpeg", StillFormat.JpegRaster.Extensions.Head, "JPEG");
    public static readonly TargetFile Bmp = new DibFile("bmp", StillFormat.BmpRaster.Extensions.Head, "BMP", static (frame, alpha) => new StillFormat.BmpRaster(frame, alpha));
    public static readonly TargetFile Targa = new DibFile("targa", StillFormat.TgaRaster.Extensions.Head, "Targa", static (frame, alpha) => new StillFormat.TgaRaster(frame, alpha));

    public string Extension { get; }

    public string Caption { get; }

    public abstract TargetWrite Write(OutputTarget target);

    public abstract Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target);

    private static Option<TargetVerdict> Referred(TargetParameter parameter, OutputTarget target, Seq<OutputDepth> depths, Option<TargetVerdict> alpha) =>
        parameter.Map(
            fileFormat: new TargetVerdict.NativeDisplay(),
            alpha: alpha,
            colorDepth: TargetVerdict.Unless(depths.Exists(depth => depth == target.ColorDepth), new TargetVerdict.NoSample()),
            codec: None,
            reader: None,
            position: None,
            displayDevice: new TargetVerdict.Honored(),
            dithering: TargetVerdict.Dithered(target.ColorDepth, target.Dithering));

    private sealed class RasterFile(string key, string extension, string caption, Seq<OutputDepth> depths, Func<PixelFrame, OutputDepth, Option<ColorEncoding>, bool, Fin<StillFormat>> build)
        : TargetFile(key, extension, caption) {
        public override TargetWrite Write(OutputTarget target) =>
            new TargetWrite.Raster(target.ColorDepth, (frame, encoding) => build(frame, target.ColorDepth, encoding, target.Alpha));

        public override Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target) =>
            Referred(parameter, target, depths, new TargetVerdict.Honored());
    }

    private sealed class ExrFile(string key, string extension, string caption) : TargetFile(key, extension, caption) {
        public override TargetWrite Write(OutputTarget target) =>
            target.Reader.Match<TargetWrite>(
                Some: reader => new TargetWrite.Layers(Layout(target), reader),
                None: () => new TargetWrite.Raw((frame, _) => Layout(target).Bind(layout => StillFormat.ExrRaster.From(frame, layout, target.Alpha, None))));

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

    private sealed class DngFile(string key, string extension, string caption) : TargetFile(key, extension, caption) {
        public override TargetWrite Write(OutputTarget target) =>
            new TargetWrite.Raw(static (frame, framing) => StillFormat.Dng.From(frame, EffectPipeline.Working, None,
                framing.Bind(static held => held.FocalLength), framing.Bind(static held => held.Aperture)));

        public override Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target) =>
            parameter.Map<Option<TargetVerdict>>(
                fileFormat: new TargetVerdict.RawChannels(), alpha: None, colorDepth: None, codec: None, reader: None, position: None, displayDevice: None, dithering: None);
    }

    private sealed class LayeredFile(string key, string extension, string caption) : TargetFile(key, extension, caption) {
        public override TargetWrite Write(OutputTarget target) => new TargetWrite.Layered();

        public override Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target) =>
            Referred(parameter, target, LayeredDocument.Depths, None);
    }

    private sealed class JpegFile(string key, string extension, string caption) : TargetFile(key, extension, caption) {
        public override TargetWrite Write(OutputTarget target) =>
            new TargetWrite.Raster(OutputDepth.UInt8, static (frame, encoding) => StillFormat.JpegRaster.From(frame, encoding, JpegQuality.Export));

        public override Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target) =>
            parameter.Map<Option<TargetVerdict>>(
                fileFormat: new TargetVerdict.NativeDisplay(),
                alpha: None, colorDepth: None, codec: None, reader: None, position: None,
                displayDevice: new TargetVerdict.Honored(), dithering: new TargetVerdict.Honored());
    }

    private sealed class DibFile(string key, string extension, string caption, Func<PixelFrame, bool, StillFormat> build) : TargetFile(key, extension, caption) {
        public override TargetWrite Write(OutputTarget target) => new TargetWrite.Dib(frame => build(frame, target.Alpha));

        public override Option<TargetVerdict> Verdict(TargetParameter parameter, OutputTarget target) =>
            parameter.Map<Option<TargetVerdict>>(
                fileFormat: new TargetVerdict.NativeDisplay(),
                alpha: new TargetVerdict.Honored(),
                colorDepth: None, codec: None, reader: None, position: None,
                displayDevice: new TargetVerdict.Honored(), dithering: new TargetVerdict.Honored());
    }
}

public sealed record OutputTarget(
    TargetFile FileFormat, bool Alpha, OutputDepth ColorDepth, ExrCompression Codec, Option<LayerReader> Reader, bool Position,
    Imaging.Tone.Display DisplayDevice, Dither Dithering)
    : IStateRecord<OutputTarget, TargetParameter> {
    public static OutputTarget Default { get; } =
        new(TargetFile.Png, Alpha: false, EffectPipeline.Depth, ExrCompression.Zip, None, Position: false, EffectPipeline.Display, Dither.WhiteNoise);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TargetParameter : IStateParameter<OutputTarget> {
    public static readonly TargetParameter FileFormat = new("file-format", formation: false, new StateParameter<OutputTarget>.Choice<TargetFile>(
            Lens<OutputTarget, TargetFile>.New(static target => target.FileFormat, static value => target => target with { FileFormat = value })),
        new("File format", "Format the file is written in"));
    public static readonly TargetParameter Alpha = new("alpha", formation: false, new StateParameter<OutputTarget>.Toggle(
            Lens<OutputTarget, bool>.New(static target => target.Alpha, static value => target => target with { Alpha = value })),
        new("Alpha", "Includes alpha where the format supports it"));
    public static readonly TargetParameter ColorDepth = new("color-depth", formation: false, new StateParameter<OutputTarget>.Choice<OutputDepth>(
            Lens<OutputTarget, OutputDepth>.New(static target => target.ColorDepth, static value => target => target with { ColorDepth = value })),
        new("Storage depth", "Bits stored per channel; a native bitmap source has 8-bit precision"));
    public static readonly TargetParameter Codec = new("codec", formation: false, new StateParameter<OutputTarget>.Choice<ExrCompression>(
            Lens<OutputTarget, ExrCompression>.New(static target => target.Codec, static value => target => target with { Codec = value })),
        new("Codec", "OpenEXR compression of the color parts"));
    public static readonly TargetParameter Reader = new("reader", formation: false, new StateParameter<OutputTarget>.OptionalChoice<LayerReader>(
            Lens<OutputTarget, Option<LayerReader>>.New(static target => target.Reader, static value => target => target with { Reader = value }), "Single part"),
        new("Layers", "Writes each render channel as a part named for its reader"));
    public static readonly TargetParameter Position = new("position", formation: false, new StateParameter<OutputTarget>.Toggle(
            Lens<OutputTarget, bool>.New(static target => target.Position, static value => target => target with { Position = value })),
        new("Position", "Adds world position from the distance channel and the rendering's camera"));
    public static readonly TargetParameter DisplayDevice = new("display-device", formation: true, new StateParameter<OutputTarget>.Choice<Imaging.Tone.Display>(
            Lens<OutputTarget, Imaging.Tone.Display>.New(static target => target.DisplayDevice, static value => target => target with { DisplayDevice = value })),
        new("Display", "Display for a captured callback or history frame; native bitmap saves preserve Rhino's display"));
    public static readonly TargetParameter Dithering = new("dithering", formation: true, new StateParameter<OutputTarget>.Choice<Dither>(
            Lens<OutputTarget, Dither>.New(static target => target.Dithering, static value => target => target with { Dithering = value })),
        new("Dither", "Final integer quantization of a captured callback or history frame; native bitmap saves preserve Rhino's dither"));

    public bool Formation { get; }

    public StateParameter<OutputTarget> Kind { get; }

    public ParameterText Text { get; }
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

    public static DictionaryCodec<ValueSet> Codec { get; } = DictionaryCodec.Json<ValueSet>(nameof(OutputBook), 2, new RecordUpgrade<ValueSet>(ValueSet.ArchiveV1, ValueSet.Codec.Name));

    public ValueSet Captured =>
        new(Targets.Map(static (target, index) => ValueSet.Fields(Owned(index), target).Entries)
            .Fold(ValueSet.Fields(Owner, Destination).Entries, static (entries, target) => entries.Union(target))
            .Union(Current.Map(static index => (new EntryKey(Owner, [CurrentKey]), index.ToString(CultureInfo.InvariantCulture))).ToSeq()));

    public static IO<OutputBook> Recalled(ValueSet set) =>
        from targets in toSeq(Range(0, int.MaxValue).TakeWhile(index => set.Entries.Keys.Exists(key => string.Equals(key.Owner, Owned(index), StringComparison.Ordinal))))
            .TraverseM(index => set.Recalled(Owned(index), OutputTarget.Default))
            .As()
        from destination in set.Recalled(Owner, Destination.Default)
        select new OutputBook(targets, set.Entries.Find(new EntryKey(Owner, [CurrentKey])).Bind(parseInt).Filter(index => targets.At(index).IsSome), destination);

    private static string Owned(int index) => string.Create(CultureInfo.InvariantCulture, $"{Owner}-target-{index}");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TargetRows {
    // --- [TEXT]
    private static string VerdictText(TargetVerdict verdict) =>
        RowText.Localize(verdict.Map(
            honored: "Written as chosen",
            rawChannels: "Scene-linear samples: renderer channels for native windows, captured scene input for callback or history exports",
            lossyColor: "Color parts lossy, data parts lossless",
            noSample: "Refused, the format holds no sample at this depth",
            nativeDisplay: "Native windows supply 8-bit display for integer files and renderer channels for float files; callback and history exports form their captured pixels",
            noLevels: "Refused, a float depth holds no levels to dither")).Local;

    // --- [BOOK]
    public static IO<OutputBook> Book(RhinoDoc doc) =>
        ArchivableDictionaries.Read(RenderSettingsSources.Dictionary(doc), OutputBook.Codec)
            .Bind(static stored => stored.Match(Some: OutputBook.Recalled, None: static () => IO.pure(OutputBook.Empty)));

    public static ReadModel<OutputTarget> Shown { get; } = new(
        [ReadModel.Marks(EventKind.DocumentPropertiesChanged), ReadModel.Marks(EventKind.UndoRedo)],
        static doc => Book(doc).Map(static book => book.Shown.IfNone(OutputTarget.Default)));

    private static IO<Unit> Write(RhinoDoc doc, OutputBook book) =>
        ArchivableDictionaries.Write(RenderSettingsSources.Dictionary(doc), OutputBook.Codec, book.Captured);

    private static Option<HostEvent<Unit>> Signal(RowScope scope) =>
        scope.Document.Map(static doc => ValueStore.Signal(EventKind.DocumentPropertiesChanged.In(doc.RuntimeSerialNumber), static _ => true));

    private static ValueStore<T> Booked<T>(RowScope scope, Func<OutputBook, Option<T>> read, Func<OutputBook, Option<T>, OutputBook> write) where T : notnull =>
        ValueStore.Of(
            from doc in scope.DocumentIO
            from book in Book(doc)
            select read(book),
            value =>
                from doc in scope.DocumentIO
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
        RowSource.Fields(Owner, [.. TargetParameter.Items], static scope => IO.pure(Seq(Booked(scope,
            static book => book.Shown,
            static (book, target) => book with { Targets = book.Targets.Map((held, index) => book.Current.Exists(at => at == index) ? target.IfNone(OutputTarget.Default) : held) }))));

    private static readonly RowSource<Destination> Destined =
        RowSource.Fields(Owner, [.. DestinationParameter.Items], static scope => IO.pure(Seq(Booked(scope,
            static book => Some(book.Destination),
            static (book, destination) => book with { Destination = destination.IfNone(Destination.Default) }))));

    private static RowRules Rules(TargetParameter parameter) =>
        new(None, Some(RowRule.When(Chosen, target => target.FileFormat.Verdict(parameter, target).IsSome, TargetParameter.FileFormat, TargetParameter.ColorDepth, TargetParameter.Reader)));

    // --- [PAGE]
    public static Seq<ControlRow> Formation { get; } = SectionRows.Of(Chosen, toSeq(TargetParameter.Items).Filter(static parameter => parameter.Formation), None, Rules);

    public static Seq<ControlRow> Page { get; } = [
        ListRows.List("Targets", "Files each save writes from one rendering", new ListRow<(OutputTarget Target, int Index), int>(
                IterableNE.create<ListColumn<(OutputTarget Target, int Index)>>(
                    new ListColumn<(OutputTarget Target, int Index)>.Label(None, static row => row.Target.FileFormat.Caption, None, None),
                    new ListColumn<(OutputTarget Target, int Index)>.Label(None, static row => row.Target.FileFormat.Verdict(TargetParameter.ColorDepth, row.Target).Map(_ => row.Target.ColorDepth.Caption).IfNone(""), None, None)),
                static row => row.Index,
                Listed,
                Picked) {
            Pair = Some((
                new CommandRow.Run(new CommandFace("output-target-add", "Add Target", "Add a file each save writes", None, None), None, static scope =>
                    from doc in scope.DocumentIO
                    from book in Book(doc)
                    from written in Write(doc, book with { Targets = book.Targets.Add(OutputTarget.Default), Current = Some(book.Targets.Count) })
                    select written),
                new CommandRow.Run(new CommandFace("output-target-remove", "Remove Targets", "Remove the selected targets", None, None),
                    Some(RowRule.When(Picked.Source, static picked => !picked.IsEmpty, Picked.Field.Parameter)), static scope =>
                        from doc in scope.DocumentIO
                        from picked in scope.Read(Picked.Source)
                        from book in Book(doc)
                        let kept = toSeq(book.Targets.Where((_, index) => !picked.Contains(index)))
                        from written in Write(doc, book with { Targets = kept, Current = kept.IsEmpty ? None : Some(0) })
                        select written))),
        },
            RowRules.Always),
        .. SectionRows.Of(Chosen, toSeq(TargetParameter.Items).Filter(static parameter => !parameter.Formation), None, Rules),
        new ControlRow.Group("Written as", RowRules.Always),
        .. toSeq(TargetParameter.Items).Filter(static parameter => !parameter.Formation).Map(static parameter => TextRows.Readout(
            parameter.Text.Caption, parameter.Text.Help, new ReadoutSource<OutputTarget>.Model(Shown),
            target => target.FileFormat.Verdict(parameter, target).Match(Some: VerdictText, None: static () => ""),
            Rules(parameter))),
        .. SectionRows.Of(Destined, Destined.Fields, None, static _ => RowRules.Always),
        ListRows.List("Paths", "Files the next save writes", new ListRow<(OutputName Name, OutputPath Path), string>(
                IterableNE.create<ListColumn<(OutputName Name, OutputPath Path)>>(new ListColumn<(OutputName Name, OutputPath Path)>.Label(None, static row => row.Path, None, None)),
                static row => row.Path,
                RowSource.Opaque(Owner, "paths", Seq<(OutputName Name, OutputPath Path)>(), static scope => ValueStore.Readout(
                    from doc in scope.DocumentIO
                    from book in Book(doc)
                    from listed in Saves.Preview(doc, book)
                    select listed,
                    Signal(scope))),
                RowSource.Opaque(Owner, "path-picked", LanguageExt.HashSet<string>.Empty)),
            RowRules.Always),
    ];
}
