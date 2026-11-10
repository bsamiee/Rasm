using Darkroom.Catalog;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Input.Custom;
using Rhino.Render.PostEffects;

namespace Darkroom.Output;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record ExportFile(FileExtension Extension, Func<Stream, IO<Unit>> Encode);

internal sealed record LutChoice(int Size, float Low, float Span, Gamut Gamut, Transfer Transfer, LutFormat Format) {
    public static LutChoice Default { get; } = new(Size: 33, Low: -10f, Span: 20f, Gamut.StandardRgb, new Transfer.Linear(), LutFormat.Cube);

    public Fin<LutTable> Bake(Seq<PixelPass.Point> chain, Gamut working) =>
        LutSpace.Of(working, LogCurve.Allocation(Low, Low + Span, float.Exp2(Low))).Bake(Size, chain.Map(static step => step.Row), working, new ColorEncoding(Gamut, Transfer));
}

internal sealed record OutputEdit(OutputBook Book, int Current) {
    private Option<OutputTarget> Target => toSeq(Book.Targets).At(Current);

    public Seq<OptionSpec<StageStep<OutputEdit, OutputBook>>> Options(StageKit<OutputEdit, OutputBook> kit) =>
        [
            OptionSpecs.Of(OptionType.List(toSeq(TargetFile.Items), static file => file.Key), TargetFile.Png, file =>
                kit.Advance(new OutputEdit(Book with { Targets = Book.Targets.Add(OutputTarget.Default.Retyped(file)) }, Book.Targets.Count)), caption: "Add"),
            OptionSpecs.Of(OptionType.Text(allowEmptyString: false), Book.Folder.ToValue(), text =>
                OutputFolder.Validated(text).Match(Succ: held => kit.Advance(this with { Book = Book with { Folder = held } }), Fail: kit.Retry), caption: "Folder"),
            OptionSpecs.Of(OptionType.Text(allowEmptyString: true), Book.Stem.ToValue(), text =>
                Stem.Validated(text).Match(Succ: held => kit.Advance(this with { Book = Book with { Stem = held } }), Fail: kit.Retry), caption: "Stem"),
            OptionSpecs.Of(OptionType.Toggle(LabelPair.NoYes), Book.Overwrite, overwrite => kit.Advance(this with { Book = Book with { Overwrite = overwrite } })),
            .. Book.Targets.IsEmpty ? [] : Seq(OptionSpecs.Of(OptionType.Integer(1, Book.Targets.Count), Current + 1, index => kit.Advance(this with { Current = index - 1 }), caption: nameof(Target))),
            .. Target.ToSeq().Bind(target => Targeted(target, kit)),
        ];

    private Seq<OptionSpec<StageStep<OutputEdit, OutputBook>>> Targeted(OutputTarget target, StageKit<OutputEdit, OutputBook> kit) =>
        [
            OptionSpecs.Plain("Remove", None, hidden: false, _ => kit.Advance(new OutputEdit(Book with { Targets = Book.Targets.RemoveAt(Current) }, int.Max(Current - 1, 0)))),
            OptionSpecs.Of(OptionType.List(toSeq(TargetFile.Items), static file => file.Key), target.FileFormat, file => Edited(kit, target.Retyped(file))),
            OptionSpecs.Of(OptionType.Toggle(LabelPair.NoYes), target.Alpha, alpha => Edited(kit, target with { Alpha = alpha })),
            OptionSpecs.Of(OptionType.List(target.FileFormat.Depths, static depth => depth.Key), target.ColorDepth, depth => Edited(kit, target with { ColorDepth = depth })),
            .. target.FileFormat == TargetFile.Exr
                ? Seq(OptionSpecs.Of(OptionType.List(toSeq(ExrCompression.Items), static codec => codec.Key), target.Codec, codec => Edited(kit, target with { Codec = codec })))
                : [],
            OptionSpecs.Of(OptionType.List(Option<LayerReader>.None.Cons(toSeq(LayerReader.Items).Map(Some)), static reader => reader.Map(static held => held.Key).IfNone(LabelPair.OffOn.Off)),
                target.Reader, reader => Edited(kit, target with { Reader = reader })),
            OptionSpecs.Of(OptionType.Toggle(LabelPair.NoYes), target.Position, position => Edited(kit, target with { Position = position })),
            OptionSpecs.Of(OptionType.Integer(1, 100), target.Quality, quality => Edited(kit, target with { Quality = quality })),
        ];

    private IO<StageStep<OutputEdit, OutputBook>> Edited(StageKit<OutputEdit, OutputBook> kit, OutputTarget target) =>
        kit.Advance(this with { Book = Book with { Targets = Book.Targets.SetItem(Current, target) } });
}

// --- [COMPOSITION] ---------------------------------------------------------------------

internal static class Exports {

    // --- [COMMANDS]

    public static Seq<PlugInCommand> Commands { get; } = [
        new CommandDefinition<CommandInput.Run>("DarkroomExportLut", new CommandInput.Run(static (doc, _, _) => Exported(doc, Lut))),
        new CommandDefinition<CommandInput.Run>("DarkroomExportStMap", new CommandInput.Run(static (doc, _, _) => Exported(doc, StMap))),
        new CommandDefinition<CommandInput.Run>("DarkroomOutput", new CommandInput.Run(static (doc, _, _) => Booked(doc))),
    ];

    // --- [EXPORTS]

    private static IO<Unit> Exported(RhinoDoc doc, Func<RhinoDoc, PixelExtent, FileMetadata, IO<ExportFile>> export) =>
        from extent in SaveSource.Extent(doc)
        from metadata in SaveSource.Metadata(doc, DarkroomPlugIn.Clock)
        from file in export(doc, extent, metadata)
        from saved in IO.lift(() => Conversions.Present(doc.Path))
        from suggested in IO.lift(saved.Traverse(path => new Stem.DocumentName().Of(Some(path)).Map(stem => Path.Join(Path.GetDirectoryName(path), $"{stem}{file.Extension}"))).As())
        from picked in Getters.Text(doc, new EntryRequest<GetString, string, string>("File") { Default = suggested }, literal: true, static entered => IO.pure(entered.Value))
        from path in IO.lift(OutputPath.Of(picked))
        from written in FilePublication.Write(path, overwrite: true, stream => file.Encode(stream))
        select written;

    private static IO<ExportFile> Lut(RhinoDoc doc, PixelExtent extent, FileMetadata metadata) =>
        from state in ChainState.Read(doc)
        from integration in DarkroomPlugIn.Integration
        let context = ChainScene.Unframed.Context(extent, [], DarkroomEffect.Derived, None, integration) with { Lights = Some(new SceneLights(None, [], [])) }
        from colors in IO.lift(state.Passes(context).Map(static chain => chain.Passes
            .Filter(step => chain.Context.Formed || step.Kind.Stage is PostEffectType.Early)
            .Choose(static step => step.Pass is PixelPass.Point point ? Some(point) : None)))
        from chosen in Getters.Stages<LutChoice, LutChoice>(LutChoice.Default, (choice, kit) => Getters.Choice(doc, new GetterRequest<GetOption, StageStep<LutChoice, LutChoice>>("LUT") {
            Accept = kit.Accept with { Nothing = Some(new NothingEntry<StageStep<LutChoice, LutChoice>>(None, kit.Done(choice))) },
            Options = [
                OptionSpecs.Of(OptionType.Integer(2, 256), choice.Size, size => kit.Advance(choice with { Size = size })),
                OptionSpecs.Of(OptionType.Number(-24d, 16d), (double)choice.Low, low => kit.Advance(choice with { Low = (float)low })),
                OptionSpecs.Of(OptionType.Number(1d, 40d), (double)choice.Span, span => kit.Advance(choice with { Span = (float)span })),
                OptionSpecs.Of(OptionType.List(Gamut.Known, static gamut => gamut.Rgb.Name), choice.Gamut, gamut => kit.Advance(choice with { Gamut = gamut })),
                OptionSpecs.Of(OptionType.Text(allowEmptyString: false), choice.Transfer.ToValue(), text =>
                    Transfer.Read(text).Match(Succ: transfer => kit.Advance(choice with { Transfer = transfer }), Fail: kit.Retry), caption: nameof(Transfer)),
                OptionSpecs.Of(OptionType.List(toSeq(LutFormat.Items), static format => format.Key), choice.Format, format => kit.Advance(choice with { Format = format })),
            ],
        }))
        from table in Getters.Await(doc, "Bake", None, _ => IO.lift(chosen.Bake(colors, context.Working)))
        from encode in IO.lift(chosen.Format.Encode(table, Session.PlugIn.Name, metadata))
        from extension in IO.lift(FileExtension.Validated(chosen.Format.Extension))
        select new ExportFile(extension, encode);

    private static IO<ExportFile> StMap(RhinoDoc doc, PixelExtent extent, FileMetadata metadata) =>
        from state in ChainState.Read(doc)
        from lens in IO.lift(state.Values.Recalled(EntryKey.TypeOwner(Kinds.Lens.Id), LensProfile.Default))
        from scene in ChainScene.Sourced(doc, extent, None)
        from rendering in IO.lift(scene.Camera.Bind(static camera => camera.Lens is Projection.Rectilinear rectilinear ? Some(rectilinear) : None)
            .ToFin(new DarkroomFault(nameof(Projection.Rectilinear), DarkroomReason.Missing)))
        from map in IO.lift(lens.Map(rendering))
        let image = new ImageFile.Exr([new ExrLayer(None, map.ToSTMap(extent, extent), ExrChannels.Standard.Rgba)], OutputDepth.Float32, OutputTarget.Default.Codec, None, Multipart: false)
        select new ExportFile(TargetFile.Exr.Extension, stream => image.Write(metadata, stream));

    // --- [BOOK]

    private static IO<Unit> Booked(RhinoDoc doc) =>
        from book in OutputBook.Read(doc)
        from edited in Getters.Stages<OutputEdit, OutputBook>(new OutputEdit(book, 0), (edit, kit) => Getters.Choice(doc, new GetterRequest<GetOption, StageStep<OutputEdit, OutputBook>>("Output") {
            Accept = kit.Accept with { Nothing = Some(new NothingEntry<StageStep<OutputEdit, OutputBook>>(None, kit.Done(edit.Book))) },
            Options = edit.Options(kit),
        }))
        from written in edited.Written(doc)
        select written;
}
