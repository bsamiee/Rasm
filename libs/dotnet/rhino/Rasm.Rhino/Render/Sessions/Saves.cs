using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Scenes;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.PlugIns;
using Rhino.Render;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SaveSource(RhinoDoc Document, RenderWindow Window);

public sealed record FileOrigin(SoftwareName Software, ZonedClock Clock, Map<FieldName, FieldText> Fields) {
    public static IO<FileOrigin> At(IPlugInSink sink) =>
        IO.lift(() => (PlugIn)sink switch {
            var plugIn => (
                    Conversions.Validated<SoftwareName, string, InvalidOutput>(plugIn.Name).ToValidation(),
                    Field("Version", plugIn.Version),
                    Field("SettingsKey", plugIn.Id.ToString("D", CultureInfo.InvariantCulture)))
                .Apply((software, version, settings) => new FileOrigin(software, ((IPlugInViews)sink).Clock.ToZonedClock(), Map(version, settings)))
                .As()
                .ToFin(),
        });

    internal static Validation<Error, (FieldName Name, FieldText Text)> Field(string name, string text) =>
        (Conversions.Validated<FieldName, string, InvalidOutput>(name).ToValidation(), Conversions.Validated<FieldText, string, InvalidOutput>(text).ToValidation())
            .Apply(static (field, value) => (field, value))
            .As();
}

public sealed record IdentitySource(ushort Id, Option<string> Name, Color Colour);

[SmartEnum<RenderWindow.StandardChannels>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public sealed partial class SavedChannel {
    private const ushort IdMask = 0x7FFF;

    public static readonly SavedChannel Distance = new(RenderWindow.StandardChannels.DistanceFromCamera, "Depth", None, static (_, frame) => new ExrPass.Depth(frame));
    public static readonly SavedChannel Normals = new(RenderWindow.StandardChannels.NormalXYZ, "Normal", None, static (name, frame) => new ExrPass.Normal(name, frame));
    public static readonly SavedChannel Albedo = new(RenderWindow.StandardChannels.AlbedoRGB, "DiffCol", None, static (name, frame) => new ExrPass.Named(name, PassKind.Rgb, frame));
    public static readonly SavedChannel MaterialIds = new(RenderWindow.StandardChannels.MaterialIds, "IndexMA",
        Some<Func<RhinoDoc, Seq<IdentitySource>>>(static document => toSeq(document.RenderMaterials).Map(static material => {
            using Material simulated = material.ToMaterial(RenderTexture.TextureGeneration.Skip);
            return new IdentitySource((ushort)(material.RenderHash & IdMask), Conversions.Present(material.Name), simulated.DiffuseColor);
        }).Strict()),
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel ObjectIds = new(RenderWindow.StandardChannels.ObjectIds, "IndexOB",
        Some<Func<RhinoDoc, Seq<IdentitySource>>>(static document => toSeq(document.Objects.GetObjectList(ObjectType.AnyObject)).Map(rhinoObject =>
            new IdentitySource((ushort)(RhinoMath.CRC32(0u, rhinoObject.Id.ToByteArray()) & IdMask), Conversions.Present(rhinoObject.Attributes.Name), rhinoObject.Attributes.DrawColor(document))).Strict()),
        static (name, frame) => new ExrPass.Named(name, PassKind.V, frame));
    public static readonly SavedChannel WireframePoints = new(RenderWindow.StandardChannels.WireframePointsRGBA, nameof(RenderWindow.StandardChannels.WireframePointsRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.Rgba, frame));
    public static readonly SavedChannel WireframeIsocurves = new(RenderWindow.StandardChannels.WireframeIsocurvesRGBA, nameof(RenderWindow.StandardChannels.WireframeIsocurvesRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.Rgba, frame));
    public static readonly SavedChannel WireframeCurves = new(RenderWindow.StandardChannels.WireframeCurvesRGBA, nameof(RenderWindow.StandardChannels.WireframeCurvesRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.Rgba, frame));
    public static readonly SavedChannel WireframeAnnotations = new(RenderWindow.StandardChannels.WireframeAnnotationsRGBA, nameof(RenderWindow.StandardChannels.WireframeAnnotationsRGBA), None,
        static (name, frame) => new ExrPass.Named(name, PassKind.Rgba, frame));

    public string Layer { get; }
    public Option<Func<RhinoDoc, Seq<IdentitySource>>> Sources { get; }

    [UseDelegateFromConstructor]
    internal partial ExrPass Part(PassName name, PixelFrame frame);

    public static bool operator <(SavedChannel left, SavedChannel right) => Comparer<SavedChannel>.Default.Compare(left, right) < 0;
    public static bool operator <=(SavedChannel left, SavedChannel right) => Comparer<SavedChannel>.Default.Compare(left, right) <= 0;
    public static bool operator >(SavedChannel left, SavedChannel right) => Comparer<SavedChannel>.Default.Compare(left, right) > 0;
    public static bool operator >=(SavedChannel left, SavedChannel right) => Comparer<SavedChannel>.Default.Compare(left, right) >= 0;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Saves {
    // --- [WRITES]
    public static IO<Unit> Write(SaveSource source, FileOrigin origin, OutputTarget target, OutputPath path, Option<RenderRegion> region, OverscanFrame overscan) =>
        target.FileFormat.Write(target).Switch(
                (Source: source, Target: target, Place: Placed(region, overscan)),
                dib: static (_, _) => Option<IO<StillFormat>>.None,
                formed: static (save, formed) => Some(Displayed(save.Source.Window, save.Target, save.Place).Bind(frame => IO.lift(formed.Still(frame)))),
                raw: static (save, raw) => Some(Read(save.Source.Window, RenderWindow.StandardChannels.RGBA, save.Place).Bind(frame => IO.lift(raw.Still(frame)))),
                layers: static (save, layers) => Some(Passes(save.Source, layers, save.Target, save.Place)),
                layered: static (save, _) => Some(Layered(save.Source, save.Target, save.Place)))
            .Match(
                Some: still =>
                    from metadata in Metadata(source.Document, origin)
                    from built in still
                    from _ in StillWriter.Write(built, metadata, path)
                    select unit,
                None: () => IO.lift(() => source.Window.SaveRenderImageAs(path, target.Alpha)));

    public static IO<OverscanFrame> Whole(RenderWindow window) =>
        IO.lift(() => EffectPipeline.Extent(window.Size())).Map(static final => Overscan.Off.Around(final));

    public static IO<Seq<(OutputName Name, OutputPath Path)>> Write(
        SaveSource source, FileOrigin origin, Destination destination, Seq<(OutputTarget Target, Seq<NamePart> Scope, OutputVersioning Version, Seq<NamePart> Parts)> outputs) =>
        from names in IO.lift(Named(outputs))
        from placed in Destinations.Resolve(source.Document, destination, names)
        from whole in Whole(source.Window)
        from written in Callbacks.Each(outputs.Zip(placed).Map(pair => Write(source, origin, pair.First.Target, pair.Second.Path, None, whole).Map(_ => pair.Second)))
        select written;

    public static IO<Seq<(OutputName Name, OutputPath Path)>> Newest(RhinoDoc document, IPlugInSink sink, OutputBook book) =>
        from rendering in IO.lift(static () => RenderRuns.Newest.ToFin(new Missing(nameof(RenderRuns.Newest))))
        from origin in FileOrigin.At(sink)
        from window in new RenderWindowSource.Rendering(rendering).Open()
        from written in Write(new SaveSource(document, window), origin, book.Destination, Fresh(book))
        select written;

    public static IO<Seq<(OutputName Name, OutputPath Path)>> Preview(RhinoDoc document, OutputBook book) =>
        IO.lift(Named(Fresh(book))).Bind(names => Destinations.Preview(document, book.Destination, names));

    private static Seq<(OutputTarget Target, Seq<NamePart> Scope, OutputVersioning Version, Seq<NamePart> Parts)> Fresh(OutputBook book) =>
        book.Named.Map(static named => (named.Target, Seq<NamePart>(), (OutputVersioning)new OutputVersioning.NextFree(), named.Parts));

    private static Fin<Seq<OutputName>> Named(Seq<(OutputTarget Target, Seq<NamePart> Scope, OutputVersioning Version, Seq<NamePart> Parts)> outputs) =>
        Callbacks.Each(outputs, static (output, _) => Conversions.Validated<FileExtension, string, InvalidRhinoValue>(output.Target.FileFormat.Extension)
            .Map(extension => new OutputName(output.Scope, output.Version, output.Parts, None, extension)));

    private static Func<PixelFrame, Fin<PixelFrame>> Placed(Option<RenderRegion> region, OverscanFrame overscan) =>
        region.Map(held => held.Pixels(overscan.Frame)).IfNone(overscan.DataWindow) switch {
            var window => read => EffectPipeline.Extent(window.Size).Map(size => new PixelFrame(window.Location, size, overscan.Frame, block => {
                for (int line = 0; line < size.Height; line++)
                    read.Block.Slice(4 * (((window.Y + overscan.Pad.Height + line) * read.Size.Width) + window.X + overscan.Pad.Width), 4 * size.Width)
                        .CopyTo(block.AsSpan(4 * line * size.Width));
            })),
        };

    // --- [FRAMES]
    private static IO<PixelFrame> Displayed(RenderWindow window, OutputTarget target, Func<PixelFrame, Fin<PixelFrame>> place) =>
        from run in IO.lift(() => RenderRuns.Find(window.SessionId).ToFin(new FrameUnformed(window.SessionId)))
        from extent in IO.lift(() => EffectPipeline.Extent(window.Size()))
        let context = new PassContext(
            extent, EffectPipeline.Working, target.DisplayDevice, target.ColorDepth, TransferCurve.Linear, Exposure.Neutral,
            run.Camera, HashMap<GuideChannel, PixelFrame>(), None, SceneLights.Empty, new Derivations())
        from source in IO.lift(() => target.DisplayDevice == EffectPipeline.Display
            ? run.Formed.Map(Copied).ToFin(new FrameUnformed(window.SessionId))
            : (run.Scene, run.Formation).Apply(static (scene, formation) => (Scene: scene, Formation: formation)).As()
                .ToFin(new SceneUntapped(window.SessionId))
                .Bind(held => Passed(Copied(held.Scene), held.Formation.Selected.Pass(held.Formation, context).Cons(run.Late))))
        from formed in IO.lift(() => Passed(source, target.ColorDepth.Levels.Map(target.Dithering.Pass).ToSeq()).Bind(place))
        select formed;

    private static IO<PixelFrame> Read(RenderWindow window, RenderWindow.StandardChannels channel, Func<PixelFrame, Fin<PixelFrame>> place) =>
        RenderWindows.Read(window, channel).Bind(read => IO.lift(place(read)));

    private static IO<Seq<SavedChannel>> Saved(RenderWindow window) =>
        RenderWindows.Channels(window).Map(static held => held.Requested.Choose(static channel => Callbacks.Found(SavedChannel.TryGet(channel, out SavedChannel? item), item!)));

    private static IO<(PassName Name, PixelFrame Frame)> Channel(RenderWindow window, SavedChannel channel, Func<PixelFrame, Fin<PixelFrame>> place) =>
        from frame in Read(window, channel.Key, place)
        from name in IO.lift(Conversions.Validated<PassName, string, InvalidOutput>(channel.Layer))
        select (name, frame);

    private static PixelFrame Copied(PixelFrame tap) => new(tap.Origin, tap.Size, tap.Extent, block => tap.Block.CopyTo(block));

    private static Fin<PixelFrame> Passed(PixelFrame frame, Seq<PixelPass> passes) =>
        passes.TraverseM(pass => pass.Run(frame, new Progress<int>())).As().Map(_ => frame);

    // --- [PASS_SETS]
    private static IO<StillFormat> Passes(SaveSource source, TargetWrite.Layers layers, OutputTarget target, Func<PixelFrame, Fin<PixelFrame>> place) =>
        from layout in IO.lift(layers.Layout)
        from framed in IO.lift(() => RenderRuns.Find(source.Window.SessionId)
            .Bind(static run => (run.Camera, run.Metres).Apply(static (camera, metres) => (Camera: camera, Metres: metres)).As())
            .ToFin(new Missing(nameof(RenderRun.Camera))))
        from beauty in Read(source.Window, RenderWindow.StandardChannels.RGBA, place)
        from channels in Saved(source.Window)
        from named in channels.TraverseM(channel => Channel(source.Window, channel, place).Map(held => channel.Part(held.Name, held.Frame))).As()
        from position in target.Position
            ? Read(source.Window, RenderWindow.StandardChannels.DistanceFromCamera, place).Map(distance => Seq<ExrPass>(new ExrPass.Position(
                new PixelFrame(distance.Origin, distance.Size, distance.Extent, block => {
                    for (int index = 0; index < block.Length; index++)
                        block[index] = distance.Block[index] * framed.Metres;
                }))))
            : IO.pure(Seq<ExrPass>())
        from set in IO.lift(PassSet.From(layers.Reader.Convention, framed.Camera, PassLayout.Parts, named + position).ToFin())
        from still in IO.lift(StillFormat.ExrRaster.From(beauty, layout, target.Alpha, Some(set)).ToFin())
        select still;

    // --- [DOCUMENTS]
    private static IO<StillFormat> Layered(SaveSource source, OutputTarget target, Func<PixelFrame, Fin<PixelFrame>> place) =>
        target.ColorDepth.Levels.IsSome switch {
            var formed =>
                from beauty in formed ? Displayed(source.Window, target, place) : Read(source.Window, RenderWindow.StandardChannels.RGBA, place)
                let encoding = formed ? target.DisplayDevice.Encoding : TargetWrite.SceneLinear
                from channels in Saved(source.Window)
                from layers in channels.Filter(channel => !formed || channel.Sources.IsSome).TraverseM(channel => Layer(source, channel, encoding, place)).As()
                from beautyName in IO.lift(Conversions.Validated<PassName, string, InvalidOutput>(nameof(RenderWindow.StandardChannels.RGBA)))
                from document in IO.lift(LayeredDocument.From(
                    target.ColorDepth, encoding, new DocumentLayer.Raster(beautyName, beauty, BlendingMode.Mix, Mix.Full, Visible: true, Mask: None).Cons(layers)).ToFin())
                select (StillFormat)new StillFormat.Document(document),
        };

    private static IO<DocumentLayer> Layer(SaveSource source, SavedChannel channel, ColorEncoding encoding, Func<PixelFrame, Fin<PixelFrame>> place) =>
        from held in Channel(source.Window, channel, place)
        from layer in IO.lift(() => channel.Sources.Match(
            Some: sources => Identified(held.Name, held.Frame, sources(source.Document), encoding),
            None: () => Fin.Succ<DocumentLayer>(new DocumentLayer.Raster(held.Name, held.Frame, BlendingMode.Mix, Mix.Full, Visible: false, Mask: None))))
        select layer;

    private static Fin<DocumentLayer> Identified(PassName name, PixelFrame ids, Seq<IdentitySource> sources, ColorEncoding encoding) {
        System.Collections.Generic.HashSet<ushort> present = [];
        foreach (Vector4 pixel in MemoryMarshal.Cast<float, Vector4>(ids.Block))
            _ = present.Add((ushort)pixel.X);
        Seq<Action<Span<Vector4>>> steps = Imaging.Tone.Formations.Display.Srgb.Encoding.To(encoding);
        return Callbacks.Each(
                toSeq(sources
                    .Filter(source => present.Contains(source.Id))
                    .Fold(Map<ushort, (Color Colour, Seq<string> Names)>(), static (held, source) =>
                        held.AddOrUpdate(source.Id, same => (same.Colour, same.Names + source.Name.ToSeq()), (source.Colour, source.Name.ToSeq())))
                    .AsIterable()),
                (entry, _) => Conversions.Validated<PassName, string, InvalidOutput>(
                        entry.Value.Names.IsEmpty ? entry.Key.ToString(CultureInfo.InvariantCulture) : string.Join(", ", entry.Value.Names))
                    .Map(layer => new IdentityLayer(entry.Key, layer, Encoded(entry.Value.Colour))))
            .Map(identified => DocumentLayer.Identities(name, ids, identified));

        Vector3 Encoded(Color colour) {
            Vector4[] pixel = [new Vector4(colour.R, colour.G, colour.B, byte.MaxValue) / byte.MaxValue];
            _ = steps.Iter(step => step(pixel));
            return pixel[0].AsVector3();
        }
    }

    // --- [METADATA]
    internal static IO<FileMetadata> Metadata(RhinoDoc document, FileOrigin origin) =>
        from created in IO.lift(() => origin.Clock.GetCurrentZonedDateTime().ToDateTimeOffset())
        from density in SceneSources.Store<ImageOutputState>(document).Read.Map(static held => held.IfNone(ImageOutputState.Default).ImageDpi)
        from active in RenderSets.Book(document).Map(static book => book.Active)
        from set in IO.lift(active.Traverse(static name => FileOrigin.Field("RenderSet", name)).As().ToFin())
        select new FileMetadata(origin.Software, created, density, origin.Fields.AddRange(set.ToSeq()));

    // --- [PROMPTS]
    internal static IO<A> Measured<A>(RhinoDoc document, Func<RenderSettings, PixelExtent, IO<A>> body) =>
        SceneSources.Read(document, window =>
            IO.lift(() => EffectPipeline.Extent(RenderPipeline.RenderSize(document, fromRenderSources: true))).Bind(extent => body(window.Settings, extent)));

    internal static IO<OutputPath> Prompted(RhinoDoc document, RunMode mode, string title, string extension) =>
        from saved in IO.lift(() => Conversions.Present(document.Path))
        from typed in IO.lift(Conversions.Validated<FileExtension, string, InvalidRhinoValue>(extension))
        from suggested in IO.lift(saved.Traverse(file => Destinations.StemOf(new Stem.DocumentName(), file)
            .Map(stem => Naming.Compose(stem, new OutputName(Seq<NamePart>(), new OutputVersioning.Unversioned(), Seq<NamePart>(), None, typed), None))).As())
        from picked in Modals.GetFileName(mode, GetFileNameMode.Export, Conversions.Unset(suggested), title, None)
        from path in IO.lift(Conversions.Validated<OutputPath, string, InvalidOutput>(picked))
        select path;
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class FrameExportCommand(IPlugInSink sink, Guid id, string englishName, Func<RenderSettings, PixelExtent, IO<PixelFrame>> frame)
    : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        Saves.Measured(doc, (settings, extent) =>
            from coordinates in frame(settings, extent)
            from extension in IO.lift(StillFormat.ExrRaster.Extensions.Head.ToFin(new Missing(nameof(StillFormat.ExrRaster.Extensions))))
            from path in Saves.Prompted(doc, mode, LocalName, extension)
            from origin in FileOrigin.At(site.Sink)
            from metadata in Saves.Metadata(doc, origin)
            from still in IO.lift(StillFormat.ExrRaster.From(coordinates, ExrLayout.Coordinates, alpha: true, None).ToFin())
            from _ in StillWriter.Write(still, metadata, path)
            select unit);
}

public sealed class LutExportCommand(IPlugInSink sink, Guid id, string englishName) : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        Saves.Measured(doc, (settings, extent) =>
            from entries in EffectRegistry.Apply(settings, Seq<EffectRequest>())
            let rendering = (IPlugInRendering)site.Sink
            from history in IPlugInRendering.Served(rendering.History, nameof(IPlugInRendering.History))
            from grading in IO.lift(history.Grading(doc).ToFin())
            from set in grading.Capture
            let ordered = EffectPipeline.Ordered(entries, rendering.Effects)
            from context in EffectPipeline.Detached(extent, set, ordered, new Derivations())
            from chain in EffectPipeline.Chain(set, ordered, context)
            from chosen in Getters.Stages<LutChoice, LutChoice>(LutChoice.Default, (choice, accepts) => Getters.Choice(doc, new GetterRequest<GetOption, StageStep<LutChoice, LutChoice>>(LocalName, site) {
                Accept = accepts with { Nothing = Some((Option<string>.None, IO.pure<StageStep<LutChoice, LutChoice>>(new StageStep<LutChoice, LutChoice>.Done(choice)))) },
                Options = choice.Options,
            }))
            from path in Saves.Prompted(doc, mode, LocalName, chosen.Format.Extension)
            from table in Getters.Await(doc, LocalName, None, _ => IO.lift(Bakes.Bake(chosen.Bake, EffectPipeline.Baked(chain), EffectPipeline.Working)))
            from origin in FileOrigin.At(site.Sink)
            from metadata in Saves.Metadata(doc, origin)
            from _ in toSeq(LookupLayout.Items).Find(layout => Equals(layout.Format, chosen.Format)).Match(
                Some: layout => LutFiles.WriteLookup(table, layout, metadata, path),
                None: () => LutFiles.Write(table, chosen.Format, path))
            select unit);

    private sealed record LutChoice(LatticeSize Size, AllocationStop Low, AllocationSpan Span, Gamut Gamut, Transfer Transfer, LutFormat Format) {
        public static LutChoice Default { get; } =
            new(LutBake.Default.Size, AllocationStop.Default, AllocationSpan.Default, LutBake.Default.Output.Gamut, LutBake.Default.Output.Transfer, LutFormat.Cube);

        public LutBake Bake => new(Size, new LatticeInput.Allocated(EffectPipeline.Working, Low, Span), new ColorEncoding(Gamut, Transfer, Nits.ReferenceWhite));

        public Seq<OptionSpec<StageStep<LutChoice, LutChoice>>> Options =>
            Seq(
                OptionSpec.Of("Size", OptionType.Integer<LatticeSize, int, InvalidColor>, Size, size => Next(this with { Size = size })),
                OptionSpec.Of("Low", OptionType.Number<AllocationStop, float, InvalidColor>, Low, low => Next(this with { Low = low })),
                OptionSpec.Of("Span", OptionType.Number<AllocationSpan, float, InvalidColor>, Span, span => Next(this with { Span = span })),
                OptionSpec.Of("Gamut", static (getter, name, plugIn, initial) => OptionType.List(toSeq(Gamut.Items), static gamut => gamut.Map(
                    standardRgb: "Rec.709", displayP3: "Display P3", rec2020: "Rec.2020", acescg: "ACEScg", aces20651: "ACES2065-1", proPhoto: "ProPhoto", eGamut: "E-Gamut",
                    arriWideGamut3: "ARRI Wide Gamut 3", arriWideGamut4: "ARRI Wide Gamut 4", sGamut3: "S-Gamut3", sGamut3Cine: "S-Gamut3.Cine", veniceSGamut3: "Venice S-Gamut3",
                    veniceSGamut3Cine: "Venice S-Gamut3.Cine", cinemaGamut: "Cinema Gamut", redWideGamutRgb: "REDWideGamutRGB", vGamut: "V-Gamut", dGamut: "D-Gamut",
                    protuneNative: "Protune Native", blackmagicWideGamut: "Blackmagic Wide Gamut", daVinciWideGamut: "DaVinci Wide Gamut", appleWideGamut: "Apple Wide Gamut"),
                    getter, name, plugIn, initial),
                    Gamut, gamut => Next(this with { Gamut = gamut })),
                OptionSpec.Of("Transfer", OptionType.Text<Transfer, InvalidColor>, Transfer, transfer => Next(this with { Transfer = transfer })),
                OptionSpec.Of("Format", static (getter, name, plugIn, initial) => OptionType.List(toSeq(LutFormat.Items), static format => format.Map(
                    cube: "Cube", spi1d: "SPI 1D", spi3d: "SPI 3D", spiMatrix: "SPI Matrix", autodesk: "Autodesk", clf: "CLF", ctf: "CTF", hald: "Hald", keyShotLookup: "KeyShot LookUp"),
                    getter, name, plugIn, initial),
                    Format, format => Next(this with { Format = format })));

        private static IO<StageStep<LutChoice, LutChoice>> Next(LutChoice choice) => IO.pure<StageStep<LutChoice, LutChoice>>(new StageStep<LutChoice, LutChoice>.Advance(choice));
    }
}
