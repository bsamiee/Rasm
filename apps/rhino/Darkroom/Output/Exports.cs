using System.Numerics;
using Darkroom.Catalog;
using Darkroom.Chain;
using Rasm.Imaging.Color;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Commands;
using Rhino.Input.Custom;
using Rhino.Render;

namespace Darkroom.Output;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LutChoice(int Size, float Low, float Span, Primaries Gamut, Transfer Transfer, LutFormat Format) {
    public static LutChoice Default { get; } = new(Size: 33, Low: -10f, Span: 20f, Primaries.StandardRgb, new Transfer.Linear(), LutFormat.Cube);

    public LutBake Bake => new(Size, new LatticeInput.Allocated(DisplayFormation.Working, Low, Span), new ColorEncoding(Gamut.Gamut, Transfer));

    public Seq<OptionSpec<StageStep<LutChoice, LutChoice>>> Options =>
        [
            OptionSpecs.Of(nameof(Size), OptionType.Integer(2, 256), Size, size => Next(this with { Size = size })),
            OptionSpecs.Of(nameof(Low), OptionType.Number(-24d, 16d), (double)Low, low => Next(this with { Low = (float)low })),
            OptionSpecs.Of(nameof(Span), OptionType.Number(1d, 40d), (double)Span, span => Next(this with { Span = (float)span })),
            OptionSpecs.Of(nameof(Gamut), OptionType.List(toSeq(Primaries.Items), static primaries => primaries.Key), Gamut, gamut => Next(this with { Gamut = gamut })),
            OptionSpecs.Of(nameof(Transfer), OptionType.Text(allowEmptyString: false), Transfer.ToValue(), text =>
                Transfer.Read(text).Match(
                    Succ: transfer => Next(this with { Transfer = transfer }),
                    Fail: static fault => IO.pure<StageStep<LutChoice, LutChoice>>(new StageStep<LutChoice, LutChoice>.Retry(fault)))),
            OptionSpecs.Of(nameof(Format), OptionType.List(toSeq(LutFormat.Items), static format => format.Key), Format, format => Next(this with { Format = format })),
        ];

    private static IO<StageStep<LutChoice, LutChoice>> Next(LutChoice choice) => IO.pure<StageStep<LutChoice, LutChoice>>(new StageStep<LutChoice, LutChoice>.Advance(choice));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract class ExportCommand(IPlugInSink sink, Guid id, string englishName) : HostCommand(sink, id, englishName, None) {
    protected abstract IO<(string Extension, Func<Stream, IO<Unit>> Encode)> Export(RhinoDoc doc, CallbackSite site, IPlugInViews views, PixelExtent extent);

    protected sealed override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        from views in IO.lift(Conversions.Cast<IPlugInViews>(site.Sink))
        from extent in IO.lift(() => RenderPipeline.RenderSize(doc, fromRenderSources: true) switch { var size => PixelExtent.Validated(size.Width, size.Height) })
        from export in Export(doc, site, views, extent)
        from saved in IO.lift(() => Conversions.Present(doc.Path))
        from suggested in IO.lift(saved.Traverse(file => Destinations.StemOf(new Stem.DocumentName(), Some(file)).Map(stem => $"{stem}{export.Extension}")).As())
        from picked in Modals.GetFileName(mode, GetFileNameMode.Export, Conversions.Unset(suggested), LocalName, None)
        from path in IO.lift(OutputPath.Of(picked))
        from written in FilePublication.Write(path, overwrite: true, export.Encode)
        select written;
}

public sealed class LutExportCommand(IPlugInSink sink) : ExportCommand(sink, new Guid(0x09625191, 0xac85, 0x44eb, 0xa7, 0x67, 0x23, 0x07, 0x6c, 0x4c, 0x64, 0xcf), "DarkroomExportLut") {
    protected override IO<(string Extension, Func<Stream, IO<Unit>> Encode)> Export(RhinoDoc doc, CallbackSite site, IPlugInViews views, PixelExtent extent) =>
        DisposalOps.Within(static () => new Derivations(), derivations =>
            from state in ChainState.Read(doc)
            from colors in state.Baked(ChainScene.Unframed.Context(extent, HashMap<GuideChannel, PixelFrame>(), derivations, rendering: false, None) with { Lights = Some(new SceneLights(None, HashMap<Guid, Vector3>(), HashMap<Guid, Vector3>())) })
            from chosen in Getters.Stages<LutChoice, LutChoice>(LutChoice.Default, (choice, accepts) => Getters.Choice(doc, new GetterRequest<GetOption, StageStep<LutChoice, LutChoice>>(LocalName, site) {
                Accept = accepts with { Nothing = Some((Option<string>.None, IO.pure<StageStep<LutChoice, LutChoice>>(new StageStep<LutChoice, LutChoice>.Done(choice)))) },
                Options = choice.Options,
            }))
            from table in Getters.Await(doc, LocalName, None, _ => IO.lift(Bakes.Bake(chosen.Bake, colors, DisplayFormation.Working)))
            from metadata in SaveSource.Metadata(doc, views)
            from encode in IO.lift(chosen.Format.Encode(table, LocalName, metadata))
            select (chosen.Format.Extension, encode));
}

public sealed class StMapExportCommand(IPlugInSink sink) : ExportCommand(sink, new Guid(0x66a517fb, 0x68c5, 0x42c9, 0x86, 0xe4, 0x30, 0x01, 0x03, 0x63, 0xa4, 0x57), "DarkroomExportStMap") {
    protected override IO<(string Extension, Func<Stream, IO<Unit>> Encode)> Export(RhinoDoc doc, CallbackSite site, IPlugInViews views, PixelExtent extent) =>
        from state in ChainState.Read(doc)
        from lens in IO.lift(state.Values.Recalled(Kinds.Lens.Owner, LensDistortion.Default))
        from still in IO.lift(StillFormat.ExrRaster.From(lens.Lens.Map(lens.Wrap).Bake(extent), ExrLayout.Coordinates, alpha: true, None))
        from metadata in SaveSource.Metadata(doc, views)
        select (StillFormat.ExrRaster.Extensions.Head, (Func<Stream, IO<Unit>>)(stream => StillWriter.Encode(still, metadata, stream)));
}
