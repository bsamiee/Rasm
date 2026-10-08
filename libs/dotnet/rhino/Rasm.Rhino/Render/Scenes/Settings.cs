using System.Diagnostics;
using System.Drawing;
using Rasm.Imaging.Output;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.DataSources;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render.Scenes;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct PixelDimension : System.Numerics.IMinMaxValue<PixelDimension> {
    public static PixelDimension MinValue { get; } = new(1);
    public static PixelDimension MaxValue { get; } = new(1_000_000);
    public static PixelDimension DefaultWidth { get; } = new(800);
    public static PixelDimension DefaultHeight { get; } = new(600);
    public static Presentation<PixelDimension, int> Presentation { get; } = new() { Form = NumberForm.Field };

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct Gamma : System.Numerics.IMinMaxValue<Gamma> {
    public static Gamma MinValue { get; } = new(0.2f);
    public static Gamma MaxValue { get; } = new(5f);
    public static Gamma Default { get; } = new(2.2f);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref float value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DitherMethod {
    public static readonly DitherMethod FloydSteinberg = new("floyd-steinberg", Dithering.Methods.FloydSteinberg);
    public static readonly DitherMethod SimpleNoise = new("simple-noise", Dithering.Methods.SimpleNoise);

    public static readonly Memo<HashMap<Dithering.Methods, DitherMethod>> ByHost = memo(static () => toHashMap(toSeq(Items).Map(static item => (item.Host, item))));

    public Dithering.Methods Host { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShadowmapQuality {
    public static readonly ShadowmapQuality Off = new("none", 0);
    public static readonly ShadowmapQuality Normal = new("normal", 1);
    public static readonly ShadowmapQuality Best = new("best", 2);

    public static readonly Memo<HashMap<int, ShadowmapQuality>> ByLevel = memo(static () => toHashMap(toSeq(Items).Map(static item => (item.Level, item))));

    public int Level { get; }
}

public sealed record ImageOutputState(bool UseViewportSize, PixelDimension ImageWidth, PixelDimension ImageHeight, PixelDensity ImageDpi, UnitSystem ImageUnitSystem)
    : ISceneRecord<ImageOutputState, ImageOutputParameter> {
    public static ImageOutputState Default { get; } = new(
        UseViewportSize: true, ImageWidth: PixelDimension.DefaultWidth, ImageHeight: PixelDimension.DefaultHeight, ImageDpi: PixelDensity.Create(72d), ImageUnitSystem: UnitSystem.Inches);

    public static string Owner => "image-output";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<ImageOutputState> Read(SceneWindow window) =>
        IO.lift(() => window.Settings switch {
            var settings => (Conversions.Validated<PixelDimension, int, InvalidRhinoValue>(settings.ImageSize.Width).ToValidation(),
                             Conversions.Validated<PixelDimension, int, InvalidRhinoValue>(settings.ImageSize.Height).ToValidation(),
                             Conversions.Validated<PixelDensity, double, InvalidOutput>(settings.ImageDpi).ToValidation())
                .Apply((width, height, dpi) => new ImageOutputState(settings.UseViewportSize, width, height, dpi, settings.ImageUnitSystem))
                .As()
                .ToFin(),
        });

    public static IO<Unit> Write(SceneWindow window, ImageOutputState state) =>
        IO.lift(() => RenderSettingsMapper.Update(state, window.Settings));

    public static ParameterText Text(ImageOutputParameter parameter) =>
        parameter.Map(
            useViewportSize: ParameterText.Of("Viewport size", "Renders at the size of the view being rendered, off at the stored width and height"),
            imageWidth: ParameterText.Of("Width", "Rendered image width in pixels"),
            imageHeight: ParameterText.Of("Height", "Rendered image height in pixels"),
            imageDpi: ParameterText.Of("DPI", "Pixels per inch stamped into captured and saved images"),
            imageUnitSystem: ParameterText.Enumeration<UnitSystem>("Print units", "Unit the print size of the image shows in",
                static system => Localization.UnitSystemName(system, capitalize: true, singular: false, abbreviate: false)));

    public static RowRules Rules(RowSource<ImageOutputState> source, ImageOutputParameter parameter) =>
        new RowRules(Some(RowRule.When(source, static state => !state.UseViewportSize, ImageOutputParameter.UseViewportSize)), None) switch {
            var custom => parameter.Map(useViewportSize: RowRules.Always, imageWidth: custom, imageHeight: custom, imageDpi: RowRules.Always, imageUnitSystem: RowRules.Always),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ImageOutputParameter : IStateParameter<ImageOutputState> {
    public static readonly ImageOutputParameter UseViewportSize = new("use-viewport-size", new StateParameter<ImageOutputState>.Toggle(
        Lens<ImageOutputState, bool>.New(static state => state.UseViewportSize, static on => state => state with { UseViewportSize = on })));
    public static readonly ImageOutputParameter ImageWidth = new("image-width", new StateParameter<ImageOutputState>.Bounded<PixelDimension, int, InvalidRhinoValue>(
        Lens<ImageOutputState, PixelDimension>.New(static state => state.ImageWidth, static width => state => state with { ImageWidth = width }), PixelDimension.Presentation));
    public static readonly ImageOutputParameter ImageHeight = new("image-height", new StateParameter<ImageOutputState>.Bounded<PixelDimension, int, InvalidRhinoValue>(
        Lens<ImageOutputState, PixelDimension>.New(static state => state.ImageHeight, static height => state => state with { ImageHeight = height }), PixelDimension.Presentation));
    public static readonly ImageOutputParameter ImageDpi = new("image-dpi", new StateParameter<ImageOutputState>.Bounded<PixelDensity, double, InvalidOutput>(
        Lens<ImageOutputState, PixelDensity>.New(static state => state.ImageDpi, static dpi => state => state with { ImageDpi = dpi }),
        new Presentation<PixelDensity, double> { Form = NumberForm.Field, Step = 1d, Decimals = 0 }));
    public static readonly ImageOutputParameter ImageUnitSystem = new("image-unit-system", new StateParameter<ImageOutputState>.Enumerated<UnitSystem>(
        Lens<ImageOutputState, UnitSystem>.New(static state => state.ImageUnitSystem, static system => state => state with { ImageUnitSystem = system })));

    public StateParameter<ImageOutputState> Kind { get; }
}

public sealed record RenderSourceState(RenderSettings.RenderingSources RenderSource, Option<string> SpecificViewport, Option<string> NamedView, Option<string> Snapshot)
    : ISceneRecord<RenderSourceState, RenderSourceParameter> {
    public static RenderSourceState Default { get; } = new(RenderSettings.RenderingSources.ActiveViewport, None, None, None);

    public static string Owner => "render-source";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<RenderSourceState> Read(SceneWindow window) =>
        IO.lift(() => RenderSettingsMapper.ToState(window.Settings));

    public static IO<Unit> Write(SceneWindow window, RenderSourceState state) =>
        IO.lift(() => RenderSettingsMapper.Update(state, window.Settings));

    public static ParameterText Text(RenderSourceParameter parameter) =>
        parameter.Map(
            renderSource: ParameterText.Enumeration<RenderSettings.RenderingSources>("Source", "View the rendering is taken from",
                static source => source switch {
                    RenderSettings.RenderingSources.ActiveViewport => "Current Viewport",
                    RenderSettings.RenderingSources.SpecificViewport => "Specific Viewport",
                    RenderSettings.RenderingSources.NamedView => "Named View",
                    RenderSettings.RenderingSources.SnapShot => "Snapshot",
                    _ => throw new UnreachableException(),
                }),
            specificViewport: ParameterText.Of("Name", "Viewport the rendering is taken from"),
            namedView: ParameterText.Of("Name", "Named view the rendering is taken from"),
            snapshot: ParameterText.Of("Name", "Snapshot the rendering is taken from"));

    public static RowRules Rules(RowSource<RenderSourceState> source, RenderSourceParameter parameter) =>
        parameter.Map(
            renderSource: RowRules.Always,
            specificViewport: Shown(source, RenderSettings.RenderingSources.SpecificViewport),
            namedView: Shown(source, RenderSettings.RenderingSources.NamedView),
            snapshot: Shown(source, RenderSettings.RenderingSources.SnapShot));

    private static RowRules Shown(RowSource<RenderSourceState> source, RenderSettings.RenderingSources chosen) =>
        new(None, Some(RowRule.When(source, state => state.RenderSource == chosen, RenderSourceParameter.RenderSource)));
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RenderSourceParameter : IStateParameter<RenderSourceState> {
    public static readonly RenderSourceParameter RenderSource = new("render-source", new StateParameter<RenderSourceState>.Enumerated<RenderSettings.RenderingSources>(
        Lens<RenderSourceState, RenderSettings.RenderingSources>.New(static state => state.RenderSource, static source => state => state with { RenderSource = source })));
    public static readonly RenderSourceParameter SpecificViewport = new("specific-viewport", new StateParameter<RenderSourceState>.OptionalRaw<string>(
        Lens<RenderSourceState, Option<string>>.New(static state => state.SpecificViewport, static name => state => state with { SpecificViewport = name })));
    public static readonly RenderSourceParameter NamedView = new("named-view", new StateParameter<RenderSourceState>.OptionalRaw<string>(
        Lens<RenderSourceState, Option<string>>.New(static state => state.NamedView, static name => state => state with { NamedView = name })));
    public static readonly RenderSourceParameter Snapshot = new("snapshot", new StateParameter<RenderSourceState>.OptionalRaw<string>(
        Lens<RenderSourceState, Option<string>>.New(static state => state.Snapshot, static name => state => state with { Snapshot = name })));

    public StateParameter<RenderSourceState> Kind { get; }
}

public sealed record FrameState(
    Swatch AmbientLight, AntialiasLevel AntialiasLevel, ShadowmapQuality ShadowmapLevel, bool TransparentBackground,
    bool RenderBackfaces, bool RenderPoints, bool RenderCurves, bool RenderIsoparams, bool RenderMeshEdges,
    bool RenderAnnotations, bool UseHiddenLights, bool DepthCue, bool FlatShade)
    : ISceneRecord<FrameState, FrameParameter> {
    public static FrameState Default { get; } = new(
        AmbientLight: Swatch.Black, AntialiasLevel: AntialiasLevel.Draft, ShadowmapLevel: ShadowmapQuality.Normal, TransparentBackground: false,
        RenderBackfaces: true, RenderPoints: false, RenderCurves: false, RenderIsoparams: false, RenderMeshEdges: false,
        RenderAnnotations: false, UseHiddenLights: false, DepthCue: false, FlatShade: false);

    public static string Owner => "frame";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<FrameState> Read(SceneWindow window) =>
        IO.lift(() => window.Settings switch {
            var settings => ShadowmapQuality.ByLevel.Value.Find(settings.ShadowmapLevel).ToFin(new InvalidAnswer(nameof(RenderSettings.ShadowmapLevel)))
                .Map(shadowmap => new FrameState(
                    Conversions.Rgb(settings.AmbientLight), settings.AntialiasLevel, shadowmap, settings.TransparentBackground,
                    settings.RenderBackfaces, settings.RenderPoints, settings.RenderCurves, settings.RenderIsoparams, settings.RenderMeshEdges,
                    settings.RenderAnnotations, settings.UseHiddenLights, settings.DepthCue, settings.FlatShade)),
        });

    public static IO<Unit> Write(SceneWindow window, FrameState state) =>
        IO.lift(() => RenderSettingsMapper.Update(state, window.Settings));

    public static ParameterText Text(FrameParameter parameter) =>
        parameter.Map(
            ambientLight: ParameterText.Of("Ambient light", "Color of the light reaching every surface from no source"),
            antialias: ParameterText.Enumeration<AntialiasLevel>("Quality", "Antialiasing quality the renderer samples at",
                static level => level switch {
                    AntialiasLevel.None => "Low quality",
                    AntialiasLevel.Draft => "Draft quality",
                    AntialiasLevel.Good => "Good quality",
                    AntialiasLevel.High => "Final quality",
                    _ => throw new UnreachableException(),
                }),
            shadowmapLevel: ParameterText.Choice<ShadowmapQuality, InvalidRhinoValue>("Shadow maps", "Shadow map quality the document stores",
                static quality => quality.Map(off: "None", normal: "Normal", best: "Best")),
            transparentBackground: ParameterText.Of("Transparent background", "Renders the background transparent under every background style"),
            renderBackfaces: ParameterText.Of("Render backfaces", "Renders the back faces of surfaces and meshes"),
            renderPoints: ParameterText.Of("Render points and point cloud", "Draws points and point clouds into the rendering"),
            renderCurves: ParameterText.Of("Render curves", "Draws curves into the rendering"),
            renderIsoparams: ParameterText.Of("Render surface edges and isocurves", "Draws surface edges and isocurves into the rendering"),
            renderMeshEdges: ParameterText.Of("Render mesh edges", "Draws mesh edges into the rendering"),
            renderAnnotations: ParameterText.Of("Render dimensions and text", "Draws dimensions and text into the rendering"),
            useHiddenLights: ParameterText.Of("Use lights on layers that are off", "Renders lights on layers that are off"),
            depthCue: ParameterText.Of("Depth cue", "Depth cue flag the document stores"),
            flatShade: ParameterText.Of("Flat shade", "Flat shading flag the document stores"));

    public static RowRules Rules(RowSource<FrameState> source, FrameParameter parameter) => RowRules.Always;
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FrameParameter : IStateParameter<FrameState> {
    public static readonly FrameParameter AmbientLight = new("ambient-light", new StateParameter<FrameState>.Color(
        Lens<FrameState, Swatch>.New(static state => state.AmbientLight, static color => state => state with { AmbientLight = color })));
    public static readonly FrameParameter Antialias = new("antialias-level", new StateParameter<FrameState>.Enumerated<AntialiasLevel>(
        Lens<FrameState, AntialiasLevel>.New(static state => state.AntialiasLevel, static level => state => state with { AntialiasLevel = level })));
    public static readonly FrameParameter ShadowmapLevel = new("shadowmap-level", new StateParameter<FrameState>.Choice<ShadowmapQuality, InvalidRhinoValue>(
        Lens<FrameState, ShadowmapQuality>.New(static state => state.ShadowmapLevel, static quality => state => state with { ShadowmapLevel = quality })));
    public static readonly FrameParameter TransparentBackground = new("transparent-background", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.TransparentBackground, static on => state => state with { TransparentBackground = on })));
    public static readonly FrameParameter RenderBackfaces = new("render-backfaces", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderBackfaces, static on => state => state with { RenderBackfaces = on })));
    public static readonly FrameParameter RenderPoints = new("render-points", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderPoints, static on => state => state with { RenderPoints = on })));
    public static readonly FrameParameter RenderCurves = new("render-curves", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderCurves, static on => state => state with { RenderCurves = on })));
    public static readonly FrameParameter RenderIsoparams = new("render-isoparams", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderIsoparams, static on => state => state with { RenderIsoparams = on })));
    public static readonly FrameParameter RenderMeshEdges = new("render-mesh-edges", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderMeshEdges, static on => state => state with { RenderMeshEdges = on })));
    public static readonly FrameParameter RenderAnnotations = new("render-annotations", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderAnnotations, static on => state => state with { RenderAnnotations = on })));
    public static readonly FrameParameter UseHiddenLights = new("use-hidden-lights", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.UseHiddenLights, static on => state => state with { UseHiddenLights = on })));
    public static readonly FrameParameter DepthCue = new("depth-cue", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.DepthCue, static on => state => state with { DepthCue = on })));
    public static readonly FrameParameter FlatShade = new("flat-shade", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.FlatShade, static on => state => state with { FlatShade = on })));

    public StateParameter<FrameState> Kind { get; }
}

public sealed record LinearWorkflowState(bool PreProcessColors, Gamma PostProcessGamma, bool PostProcessGammaOn)
    : ISceneRecord<LinearWorkflowState, LinearWorkflowParameter> {
    public static LinearWorkflowState Default { get; } = new(PreProcessColors: true, PostProcessGamma: Gamma.Default, PostProcessGammaOn: true);

    public static string Owner => "linear-workflow";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<LinearWorkflowState> Read(SceneWindow window) =>
        Sources.SubOwner(window, static settings => settings.LinearWorkflow, static workflow => IO.lift(() =>
            Conversions.Validated<Gamma, float, InvalidRhinoValue>(workflow.PostProcessGamma)
                .Map(gamma => new LinearWorkflowState(workflow.PreProcessColors, gamma, workflow.PostProcessGammaOn))));

    public static IO<Unit> Write(SceneWindow window, LinearWorkflowState state) =>
        Sources.SubOwner(window, static settings => settings.LinearWorkflow, workflow => IO.lift(() => RenderSettingsMapper.Update(state, workflow)));

    public static ParameterText Text(LinearWorkflowParameter parameter) =>
        parameter.Map(
            preProcessColors: ParameterText.Of("Use linear workflow", "Linearizes colors and textures by the gamma before rendering"),
            postProcessGamma: ParameterText.Of("Gamma", "Gamma the rendering is encoded with and colors are linearized by"),
            postProcessGammaOn: ParameterText.Of("Gamma", "Encodes the rendering with the gamma"));

    public static RowRules Rules(RowSource<LinearWorkflowState> source, LinearWorkflowParameter parameter) => RowRules.Always;
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LinearWorkflowParameter : IStateParameter<LinearWorkflowState> {
    public static readonly LinearWorkflowParameter PreProcessColors = new("use-linear-workflow", new StateParameter<LinearWorkflowState>.Toggle(
        Lens<LinearWorkflowState, bool>.New(static state => state.PreProcessColors, static on => state => state with { PreProcessColors = on })));
    public static readonly LinearWorkflowParameter PostProcessGamma = new("gamma", new StateParameter<LinearWorkflowState>.Bounded<Gamma, float, InvalidRhinoValue>(
        Lens<LinearWorkflowState, Gamma>.New(static state => state.PostProcessGamma, static gamma => state => state with { PostProcessGamma = gamma }),
        new Presentation<Gamma, float> { Step = 0.01f, Decimals = 2 }));
    public static readonly LinearWorkflowParameter PostProcessGammaOn = new("use-post-process-gamma", new StateParameter<LinearWorkflowState>.Toggle(
        Lens<LinearWorkflowState, bool>.New(static state => state.PostProcessGammaOn, static on => state => state with { PostProcessGammaOn = on })));

    public StateParameter<LinearWorkflowState> Kind { get; }
}

public sealed record DitheringState(bool Enabled, DitherMethod Method)
    : ISceneRecord<DitheringState, DitheringParameter> {
    public static DitheringState Default { get; } = new(Enabled: false, Method: DitherMethod.FloydSteinberg);

    public static string Owner => "dithering";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<DitheringState> Read(SceneWindow window) =>
        Sources.SubOwner(window, static settings => settings.Dithering, static dithering => IO.lift(() => RenderSettingsMapper.ToState(dithering)));

    public static IO<Unit> Write(SceneWindow window, DitheringState state) =>
        Sources.SubOwner(window, static settings => settings.Dithering, dithering => IO.lift(() => RenderSettingsMapper.Update(state, dithering)));

    public static ParameterText Text(DitheringParameter parameter) =>
        parameter.Map(
            enabled: ParameterText.Of("Dithering", "Adds noise before the rendering is quantized to display codes"),
            method: ParameterText.Choice<DitherMethod, InvalidRhinoValue>("Dithering", "Dither the renderer adds",
                static method => method.Map(floydSteinberg: "Floyd-Steinberg", simpleNoise: "Simple noise")));

    public static RowRules Rules(RowSource<DitheringState> source, DitheringParameter parameter) => RowRules.Always;
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DitheringParameter : IStateParameter<DitheringState> {
    public static readonly DitheringParameter Enabled = new("use-dithering", new StateParameter<DitheringState>.Toggle(
        Lens<DitheringState, bool>.New(static state => state.Enabled, static on => state => state with { Enabled = on })));
    public static readonly DitheringParameter Method = new("dithering", new StateParameter<DitheringState>.Choice<DitherMethod, InvalidRhinoValue>(
        Lens<DitheringState, DitherMethod>.New(static state => state.Method, static method => state => state with { Method = method })));

    public StateParameter<DitheringState> Kind { get; }
}

public sealed record RenderChannelsState(RenderChannels.Modes Mode, Set<Guid> CustomList)
    : ISceneRecord<RenderChannelsState, RenderChannelsParameter> {
    public static RenderChannelsState Default =>
        new(RenderChannels.Modes.Automatic, Set(RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA), RenderWindow.ChannelId(RenderWindow.StandardChannels.DistanceFromCamera)));

    public static string Owner => "render-channels";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => EventKind.RenderChannelsChanged;

    public static IO<RenderChannelsState> Read(SceneWindow window) =>
        Sources.SubOwner(window, static settings => settings.RenderChannels, static channels => IO.lift(() => RenderSettingsMapper.ToState(channels)));

    public static IO<Unit> Write(SceneWindow window, RenderChannelsState state) =>
        Sources.SubOwner(window, static settings => settings.RenderChannels, channels => IO.lift(() => RenderSettingsMapper.Update(state, channels)));

    public static ParameterText Text(RenderChannelsParameter parameter) =>
        parameter.Map(
            mode: ParameterText.Enumeration<RenderChannels.Modes>("Render channels", "Channels the renderer writes, Automatic adding each post effect's required channels",
                static mode => mode switch {
                    RenderChannels.Modes.Automatic => "Automatic",
                    RenderChannels.Modes.Custom => "Custom",
                    _ => throw new UnreachableException(),
                }),
            distance: ParameterText.Of("Distance", "Distance from the camera per pixel"),
            normals: ParameterText.Of("Normals", "Surface normal per pixel"),
            albedo: ParameterText.Of("Albedo", "Surface base color per pixel"),
            materialIds: ParameterText.Of("Material IDs", "Material id per pixel"),
            objectIds: ParameterText.Of("Object IDs", "Object id per pixel"));

    public static RowRules Rules(RowSource<RenderChannelsState> source, RenderChannelsParameter parameter) =>
        new RowRules(None, Some(RowRule.When(source, static state => state.Mode == RenderChannels.Modes.Custom, RenderChannelsParameter.Mode))) switch {
            var custom => parameter.Map(mode: RowRules.Always, distance: custom, normals: custom, albedo: custom, materialIds: custom, objectIds: custom),
        };
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RenderChannelsParameter : IStateParameter<RenderChannelsState> {
    public static readonly RenderChannelsParameter Mode = new("mode", new StateParameter<RenderChannelsState>.Enumerated<RenderChannels.Modes>(
        Lens<RenderChannelsState, RenderChannels.Modes>.New(static state => state.Mode, static mode => state => state with { Mode = mode })));
    public static readonly RenderChannelsParameter Distance = new("distance", Member(RenderWindow.StandardChannels.DistanceFromCamera));
    public static readonly RenderChannelsParameter Normals = new("normals", Member(RenderWindow.StandardChannels.NormalXYZ));
    public static readonly RenderChannelsParameter Albedo = new("albedo", Member(RenderWindow.StandardChannels.AlbedoRGB));
    public static readonly RenderChannelsParameter MaterialIds = new("material-ids", Member(RenderWindow.StandardChannels.MaterialIds));
    public static readonly RenderChannelsParameter ObjectIds = new("object-ids", Member(RenderWindow.StandardChannels.ObjectIds));

    public StateParameter<RenderChannelsState> Kind { get; }

    private static StateParameter<RenderChannelsState> Member(RenderWindow.StandardChannels channel) =>
        new StateParameter<RenderChannelsState>.Toggle(Lens<RenderChannelsState, bool>.New(
            state => state.CustomList.Contains(RenderWindow.ChannelId(channel)),
            on => state => RenderWindow.ChannelId(channel) switch {
                var id => state with { CustomList = on ? state.CustomList.TryAdd(id) : state.CustomList.Remove(id) },
            }));
}

public sealed record EditorState(Guid CurrentRenderer, Option<Guid> RenderingViewport, Seq<Size> CustomRenderSizes, bool CustomImageSizeIsPreset, Seq<(Guid Id, string Name)> Renderers) {
    public static IO<EditorState> Read(RhinoSettings settings) =>
        IO.lift(() => new EditorState(
            settings.GetCurrentRenderer(),
            Optional(settings.RenderingView()).Map(static view => view.Viewport.Id),
            toSeq(settings.GetCustomRenderSizes()),
            settings.CustomImageSizeIsPreset,
            toSeq(PlugIn.GetInstalledPlugIns())
                .Filter(static plugIn => Optional(PlugIn.GetPlugInInfo(plugIn.Key)).Exists(static info => info.PlugInType == PlugInType.Render))
                .Map(static plugIn => (Id: plugIn.Key, Name: plugIn.Value))
                .Strict()));

    public static IO<Unit> Choose(Guid renderer) =>
        IO.lift(() => Refused.Unless(Utilities.SetDefaultRenderPlugIn(renderer), nameof(Utilities.SetDefaultRenderPlugIn)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source, EnabledConversions = MappingConversionType.Queryable | MappingConversionType.Enumerable | MappingConversionType.Dictionary
    | MappingConversionType.Span | MappingConversionType.Memory | MappingConversionType.EnumToEnum | MappingConversionType.ImplicitCast)]
internal static partial class RenderSettingsMapper {
    // --- [RENDER_SETTINGS]
    [MapPropertyFromSource(nameof(RenderSettings.ImageSize), Use = nameof(ImageSize))]
    [MapperIgnoreSource(nameof(ImageOutputState.ImageWidth), Justification = "Written through ImageSize")]
    [MapperIgnoreSource(nameof(ImageOutputState.ImageHeight), Justification = "Written through ImageSize")]
    internal static partial void Update(ImageOutputState state, RenderSettings settings);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial RenderSourceState ToState(RenderSettings settings);

    internal static partial void Update(RenderSourceState state, RenderSettings settings);

    internal static partial void Update(FrameState state, RenderSettings settings);

    // --- [SUB_OWNERS]
    internal static partial void Update(LinearWorkflowState state, LinearWorkflow workflow);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial DitheringState ToState(Dithering dithering);

    internal static partial void Update(DitheringState state, Dithering dithering);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial RenderChannelsState ToState(RenderChannels channels);

    internal static partial void Update(RenderChannelsState state, RenderChannels channels);

    // --- [VALUES]
    [UserMapping(Default = false)]
    private static Size ImageSize(ImageOutputState state) => new(state.ImageWidth, state.ImageHeight);

    [UserMapping]
    private static int Level(ShadowmapQuality quality) => quality.Level;

    [UserMapping]
    private static Dithering.Methods Host(DitherMethod method) => method.Host;

    [UserMapping]
    private static DitherMethod Method(Dithering.Methods method) => DitherMethod.ByHost.Value[method];

    [UserMapping]
    private static Set<Guid> Ids(Guid[] ids) => toSet(ids);
}
