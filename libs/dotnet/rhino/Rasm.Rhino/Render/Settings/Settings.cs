using System.Diagnostics;
using System.Drawing;
using Rasm.Imaging.Files;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.DataSources;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PixelDimension : IPresented<PixelDimension, int> {
    public static PixelDimension MinValue { get; } = new(1);
    public static PixelDimension MaxValue { get; } = new(int.MaxValue);
    public static PixelDimension DefaultWidth { get; } = new(800);
    public static PixelDimension DefaultHeight { get; } = new(600);
    public static Presentation<int> Presentation { get; } = new() { Form = NumberForm.Field };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = Admission.Refusal(value, MinValue._value, MaxValue._value);
}

[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Gamma : System.Numerics.IMinMaxValue<Gamma> {
    public static Gamma MinValue { get; } = new(0.2f);
    public static Gamma MaxValue { get; } = new(5f);
    public static Gamma Default { get; } = new(2.2f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = Admission.Refusal(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DitherMethod : ICaptioned {
    public static readonly DitherMethod FloydSteinberg = new("floyd-steinberg", "Floyd-Steinberg", Dithering.Methods.FloydSteinberg);
    public static readonly DitherMethod SimpleNoise = new("simple-noise", "Simple noise", Dithering.Methods.SimpleNoise);

    public static readonly Memo<HashMap<Dithering.Methods, DitherMethod>> ByHost = memo(static () => toHashMap(toSeq(Items).Map(static item => (item.Host, item))));

    public string Caption { get; }

    public Dithering.Methods Host { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShadowmapQuality : ICaptioned {
    public static readonly ShadowmapQuality Off = new("none", "None", 0);
    public static readonly ShadowmapQuality Normal = new("normal", "Normal", 1);
    public static readonly ShadowmapQuality Best = new("best", "Best", 2);

    public static readonly Memo<HashMap<int, ShadowmapQuality>> ByLevel = memo(static () => toHashMap(toSeq(Items).Map(static item => (item.Level, item))));

    public string Caption { get; }

    public int Level { get; }
}

public sealed record ImageOutputState(bool UseViewportSize, PixelDimension ImageWidth, PixelDimension ImageHeight, PixelDensity ImageDpi, UnitSystem ImageUnitSystem)
    : IRenderSettingsRecord<ImageOutputState, ImageOutputParameter> {
    public static ImageOutputState Default { get; } = new(
        UseViewportSize: true, ImageWidth: PixelDimension.DefaultWidth, ImageHeight: PixelDimension.DefaultHeight, ImageDpi: PixelDensity.Create(72d), ImageUnitSystem: UnitSystem.Inches);

    public static string Owner => "image-output";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<ImageOutputState> Read(RenderSettingsScope scope) =>
        IO.lift(() => scope.Settings switch {
            var settings => (PixelDimension.Validated(settings.ImageSize.Width).ToValidation(),
                             PixelDimension.Validated(settings.ImageSize.Height).ToValidation(),
                             PixelDensity.Validated(settings.ImageDpi).ToValidation())
                .Apply((width, height, dpi) => new ImageOutputState(settings.UseViewportSize, width, height, dpi, settings.ImageUnitSystem))
                .As()
                .ToFin(),
        });

    public static IO<ImageOutputState> Write(RenderSettingsScope scope, ImageOutputState state) =>
        IO.lift(() => { RenderSettingsMapper.Update(state, scope.Settings); return state; });

    public static RowRules Rules(RowSource<ImageOutputState> source, ImageOutputParameter parameter) =>
        new RowRules(Some(RowRule.When(source, static state => !state.UseViewportSize, ImageOutputParameter.UseViewportSize)), None) switch {
            var custom => parameter.Map(useViewportSize: RowRules.Always, imageWidth: custom, imageHeight: custom, imageDpi: RowRules.Always, imageUnitSystem: RowRules.Always),
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ImageOutputParameter : IStateParameter<ImageOutputState> {
    public static readonly ImageOutputParameter UseViewportSize = new("use-viewport-size", new StateParameter<ImageOutputState>.Toggle(
        Lens<ImageOutputState, bool>.New(static state => state.UseViewportSize, static on => state => state with { UseViewportSize = on })),
        new("Viewport size", "Renders at the size of the view being rendered, off at the stored width and height"));
    public static readonly ImageOutputParameter ImageWidth = new("image-width", StateParameter.Bounded(Lens<ImageOutputState, PixelDimension>.New(static state => state.ImageWidth, static width => state => state with { ImageWidth = width })),
        new("Width", "Rendered image width in pixels"));
    public static readonly ImageOutputParameter ImageHeight = new("image-height", StateParameter.Bounded(Lens<ImageOutputState, PixelDimension>.New(static state => state.ImageHeight, static height => state => state with { ImageHeight = height })),
        new("Height", "Rendered image height in pixels"));
    public static readonly ImageOutputParameter ImageDpi = new("image-dpi", StateParameter.Bounded(Lens<ImageOutputState, PixelDensity>.New(static state => state.ImageDpi, static dpi => state => state with { ImageDpi = dpi }),
    new Presentation<double> { Form = NumberForm.Field, Step = 1d, Decimals = 0 }),
        new("DPI", "Pixels per inch stamped into captured and saved images"));
    public static readonly ImageOutputParameter ImageUnitSystem = new("image-unit-system", new StateParameter<ImageOutputState>.Enumerated<UnitSystem>(
        Lens<ImageOutputState, UnitSystem>.New(static state => state.ImageUnitSystem, static system => state => state with { ImageUnitSystem = system }),
        static system => Localization.UnitSystemName(system, capitalize: true, singular: false, abbreviate: false)),
        new("Print units", "Unit the print size of the image shows in"));

    public StateParameter<ImageOutputState> Kind { get; }

    public ParameterText Text { get; }
}

public sealed record RenderSourceState(RenderSettings.RenderingSources RenderSource, Option<string> SpecificViewport, Option<string> NamedView, Option<string> Snapshot)
    : IRenderSettingsRecord<RenderSourceState, RenderSourceParameter> {
    public static RenderSourceState Default { get; } = new(RenderSettings.RenderingSources.ActiveViewport, None, None, None);

    public static string Owner => "render-source";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<RenderSourceState> Read(RenderSettingsScope scope) =>
        IO.lift(() => RenderSettingsMapper.ToState(scope.Settings));

    public static IO<RenderSourceState> Write(RenderSettingsScope scope, RenderSourceState state) =>
        IO.pure(state with {
            SpecificViewport = state.SpecificViewport.Bind(Conversions.Present),
            NamedView = state.NamedView.Bind(Conversions.Present),
            Snapshot = state.Snapshot.Bind(Conversions.Present),
        }).Bind(accepted => IO.lift(() => { RenderSettingsMapper.Update(accepted, scope.Settings); return accepted; }));

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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RenderSourceParameter : IStateParameter<RenderSourceState> {
    public static readonly RenderSourceParameter RenderSource = new("render-source", new StateParameter<RenderSourceState>.Enumerated<RenderSettings.RenderingSources>(
        Lens<RenderSourceState, RenderSettings.RenderingSources>.New(static state => state.RenderSource, static source => state => state with { RenderSource = source }),
        static source => source switch {
            RenderSettings.RenderingSources.ActiveViewport => "Current viewport",
            RenderSettings.RenderingSources.SpecificViewport => "Specific viewport",
            RenderSettings.RenderingSources.NamedView => "Named view",
            RenderSettings.RenderingSources.SnapShot => "Snapshot",
            _ => throw new UnreachableException(),
        }),
        new("Source", "View the rendering is taken from"));
    public static readonly RenderSourceParameter SpecificViewport = new("specific-viewport", new StateParameter<RenderSourceState>.OptionalRaw<string>(
        Lens<RenderSourceState, Option<string>>.New(static state => state.SpecificViewport, static name => state => state with { SpecificViewport = name })),
        new("Name", "Viewport the rendering is taken from"));
    public static readonly RenderSourceParameter NamedView = new("named-view", new StateParameter<RenderSourceState>.OptionalRaw<string>(
        Lens<RenderSourceState, Option<string>>.New(static state => state.NamedView, static name => state => state with { NamedView = name })),
        new("Name", "Named view the rendering is taken from"));
    public static readonly RenderSourceParameter Snapshot = new("snapshot", new StateParameter<RenderSourceState>.OptionalRaw<string>(
        Lens<RenderSourceState, Option<string>>.New(static state => state.Snapshot, static name => state => state with { Snapshot = name })),
        new("Name", "Snapshot the rendering is taken from"));

    public StateParameter<RenderSourceState> Kind { get; }

    public ParameterText Text { get; }
}

public sealed record FrameState(
    Swatch AmbientLight, AntialiasLevel AntialiasLevel, ShadowmapQuality ShadowmapLevel, bool TransparentBackground,
    bool RenderBackfaces, bool RenderPoints, bool RenderCurves, bool RenderIsoparams, bool RenderMeshEdges,
    bool RenderAnnotations, bool UseHiddenLights, bool DepthCue, bool FlatShade)
    : IRenderSettingsRecord<FrameState, FrameParameter> {
    public static FrameState Default { get; } = new(
        AmbientLight: Swatch.Black, AntialiasLevel: AntialiasLevel.Draft, ShadowmapLevel: ShadowmapQuality.Normal, TransparentBackground: false,
        RenderBackfaces: true, RenderPoints: false, RenderCurves: false, RenderIsoparams: false, RenderMeshEdges: false,
        RenderAnnotations: false, UseHiddenLights: false, DepthCue: false, FlatShade: false);

    public static string Owner => "frame";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<FrameState> Read(RenderSettingsScope scope) =>
        IO.lift(() => scope.Settings switch {
            var settings => ShadowmapQuality.ByLevel.Value.Find(settings.ShadowmapLevel).ToFin(new Refused(nameof(RenderSettings.ShadowmapLevel), Refused.Invalid))
                .Map(shadowmap => new FrameState(
                    Conversions.Rgb(settings.AmbientLight), settings.AntialiasLevel, shadowmap, settings.TransparentBackground,
                    settings.RenderBackfaces, settings.RenderPoints, settings.RenderCurves, settings.RenderIsoparams, settings.RenderMeshEdges,
                    settings.RenderAnnotations, settings.UseHiddenLights, settings.DepthCue, settings.FlatShade)),
        });

    public static IO<FrameState> Write(RenderSettingsScope scope, FrameState state) =>
        IO.lift(() => { RenderSettingsMapper.Update(state, scope.Settings); return state; });

    public static RowRules Rules(RowSource<FrameState> source, FrameParameter parameter) => RowRules.Always;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FrameParameter : IStateParameter<FrameState> {
    public static readonly FrameParameter AmbientLight = new("ambient-light", new StateParameter<FrameState>.Color(
        Lens<FrameState, Swatch>.New(static state => state.AmbientLight, static color => state => state with { AmbientLight = color })),
        new("Ambient light", "Color of the light reaching every surface from no source"));
    public static readonly FrameParameter Antialias = new("antialias-level", new StateParameter<FrameState>.Enumerated<AntialiasLevel>(
        Lens<FrameState, AntialiasLevel>.New(static state => state.AntialiasLevel, static level => state => state with { AntialiasLevel = level }),
        static level => level switch {
            AntialiasLevel.None => "Low quality",
            AntialiasLevel.Draft => "Draft quality",
            AntialiasLevel.Good => "Good quality",
            AntialiasLevel.High => "Final quality",
            _ => throw new UnreachableException(),
        }),
        new("Quality", "Antialiasing quality the renderer samples at"));
    public static readonly FrameParameter ShadowmapLevel = new("shadowmap-level", new StateParameter<FrameState>.Choice<ShadowmapQuality>(
        Lens<FrameState, ShadowmapQuality>.New(static state => state.ShadowmapLevel, static quality => state => state with { ShadowmapLevel = quality })),
        new("Shadow maps", "Shadow map quality the document stores"));
    public static readonly FrameParameter TransparentBackground = new("transparent-background", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.TransparentBackground, static on => state => state with { TransparentBackground = on })),
        new("Transparent background", "Renders the background transparent under every background style"));
    public static readonly FrameParameter RenderBackfaces = new("render-backfaces", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderBackfaces, static on => state => state with { RenderBackfaces = on })),
        new("Render backfaces", "Renders the back faces of surfaces and meshes"));
    public static readonly FrameParameter RenderPoints = new("render-points", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderPoints, static on => state => state with { RenderPoints = on })),
        new("Render points and point cloud", "Draws points and point clouds into the rendering"));
    public static readonly FrameParameter RenderCurves = new("render-curves", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderCurves, static on => state => state with { RenderCurves = on })),
        new("Render curves", "Draws curves into the rendering"));
    public static readonly FrameParameter RenderIsoparams = new("render-isoparams", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderIsoparams, static on => state => state with { RenderIsoparams = on })),
        new("Render surface edges and isocurves", "Draws surface edges and isocurves into the rendering"));
    public static readonly FrameParameter RenderMeshEdges = new("render-mesh-edges", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderMeshEdges, static on => state => state with { RenderMeshEdges = on })),
        new("Render mesh edges", "Draws mesh edges into the rendering"));
    public static readonly FrameParameter RenderAnnotations = new("render-annotations", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.RenderAnnotations, static on => state => state with { RenderAnnotations = on })),
        new("Render dimensions and text", "Draws dimensions and text into the rendering"));
    public static readonly FrameParameter UseHiddenLights = new("use-hidden-lights", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.UseHiddenLights, static on => state => state with { UseHiddenLights = on })),
        new("Use lights on layers that are off", "Renders lights on layers that are off"));
    public static readonly FrameParameter DepthCue = new("depth-cue", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.DepthCue, static on => state => state with { DepthCue = on })),
        new("Depth cue", "Depth cue flag the document stores"));
    public static readonly FrameParameter FlatShade = new("flat-shade", new StateParameter<FrameState>.Toggle(
        Lens<FrameState, bool>.New(static state => state.FlatShade, static on => state => state with { FlatShade = on })),
        new("Flat shade", "Flat shading flag the document stores"));

    public StateParameter<FrameState> Kind { get; }

    public ParameterText Text { get; }
}

public sealed record LinearWorkflowState(bool PreProcessColors, Gamma PostProcessGamma, bool PostProcessGammaOn)
    : IRenderSettingsRecord<LinearWorkflowState, LinearWorkflowParameter> {
    public static LinearWorkflowState Default { get; } = new(PreProcessColors: true, PostProcessGamma: Gamma.Default, PostProcessGammaOn: true);

    public static string Owner => "linear-workflow";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<LinearWorkflowState> Read(RenderSettingsScope scope) =>
        RenderSettingsSources.SubOwner(scope, static settings => settings.LinearWorkflow, static workflow => IO.lift(() =>
            Gamma.Validated(workflow.PostProcessGamma)
                .Map(gamma => new LinearWorkflowState(workflow.PreProcessColors, gamma, workflow.PostProcessGammaOn))));

    public static IO<LinearWorkflowState> Write(RenderSettingsScope scope, LinearWorkflowState state) =>
        RenderSettingsSources.SubOwner(scope, static settings => settings.LinearWorkflow, workflow => IO.lift(() => { RenderSettingsMapper.Update(state, workflow); return state; }));

    public static RowRules Rules(RowSource<LinearWorkflowState> source, LinearWorkflowParameter parameter) => RowRules.Always;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LinearWorkflowParameter : IStateParameter<LinearWorkflowState> {
    public static readonly LinearWorkflowParameter PreProcessColors = new("use-linear-workflow", new StateParameter<LinearWorkflowState>.Toggle(
        Lens<LinearWorkflowState, bool>.New(static state => state.PreProcessColors, static on => state => state with { PreProcessColors = on })),
        new("Use linear workflow", "Linearizes colors and textures by the gamma before rendering"));
    public static readonly LinearWorkflowParameter PostProcessGamma = new("gamma", StateParameter.Bounded(Lens<LinearWorkflowState, Gamma>.New(static state => state.PostProcessGamma, static gamma => state => state with { PostProcessGamma = gamma }),
    new Presentation<float> { Step = 0.01f, Decimals = 2 }),
        new("Gamma", "Gamma the rendering is encoded with and colors are linearized by"));
    public static readonly LinearWorkflowParameter PostProcessGammaOn = new("use-post-process-gamma", new StateParameter<LinearWorkflowState>.Toggle(
        Lens<LinearWorkflowState, bool>.New(static state => state.PostProcessGammaOn, static on => state => state with { PostProcessGammaOn = on })),
        new("Gamma", "Encodes the rendering with the gamma"));

    public StateParameter<LinearWorkflowState> Kind { get; }

    public ParameterText Text { get; }
}

public sealed record DitheringState(bool Enabled, DitherMethod Method)
    : IRenderSettingsRecord<DitheringState, DitheringParameter> {
    public static DitheringState Default { get; } = new(Enabled: false, Method: DitherMethod.FloydSteinberg);

    public static string Owner => "dithering";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<DitheringState> Read(RenderSettingsScope scope) =>
        RenderSettingsSources.SubOwner(scope, static settings => settings.Dithering, static dithering => IO.lift(() => RenderSettingsMapper.ToState(dithering)));

    public static IO<DitheringState> Write(RenderSettingsScope scope, DitheringState state) =>
        RenderSettingsSources.SubOwner(scope, static settings => settings.Dithering, dithering => IO.lift(() => { RenderSettingsMapper.Update(state, dithering); return state; }));

    public static RowRules Rules(RowSource<DitheringState> source, DitheringParameter parameter) => RowRules.Always;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DitheringParameter : IStateParameter<DitheringState> {
    public static readonly DitheringParameter Enabled = new("use-dithering", new StateParameter<DitheringState>.Toggle(
        Lens<DitheringState, bool>.New(static state => state.Enabled, static on => state => state with { Enabled = on })),
        new("Dithering", "Adds noise before the rendering is quantized to display codes"));
    public static readonly DitheringParameter Method = new("dithering", new StateParameter<DitheringState>.Choice<DitherMethod>(
        Lens<DitheringState, DitherMethod>.New(static state => state.Method, static method => state => state with { Method = method })),
        new("Dithering", "Dither the renderer adds"));

    public StateParameter<DitheringState> Kind { get; }

    public ParameterText Text { get; }
}

public sealed record RenderChannelsState(RenderChannels.Modes Mode, Set<Guid> CustomList)
    : IRenderSettingsRecord<RenderChannelsState, RenderChannelsParameter> {
    public static RenderChannelsState Default =>
        new(RenderChannels.Modes.Automatic, Set(RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA), RenderWindow.ChannelId(RenderWindow.StandardChannels.DistanceFromCamera)));

    public static string Owner => "render-channels";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => EventKind.RenderChannelsChanged;

    public static IO<RenderChannelsState> Read(RenderSettingsScope scope) =>
        RenderSettingsSources.SubOwner(scope, static settings => settings.RenderChannels, static channels => IO.lift(() => RenderSettingsMapper.ToState(channels)));

    public static IO<RenderChannelsState> Write(RenderSettingsScope scope, RenderChannelsState state) =>
        RenderSettingsSources.SubOwner(scope, static settings => settings.RenderChannels, channels => IO.lift(() => { RenderSettingsMapper.Update(state, channels); return state; }));

    public static RowRules Rules(RowSource<RenderChannelsState> source, RenderChannelsParameter parameter) =>
        new RowRules(None, Some(RowRule.When(source, static state => state.Mode is RenderChannels.Modes.Custom, RenderChannelsParameter.Mode))) switch {
            var custom => parameter.Map(mode: RowRules.Always, distance: custom, normals: custom, albedo: custom, materialIds: custom, objectIds: custom),
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RenderChannelsParameter : IStateParameter<RenderChannelsState> {
    public static readonly RenderChannelsParameter Mode = new("mode", new StateParameter<RenderChannelsState>.Enumerated<RenderChannels.Modes>(
        Lens<RenderChannelsState, RenderChannels.Modes>.New(static state => state.Mode, static mode => state => state with { Mode = mode }),
        static mode => mode switch {
            RenderChannels.Modes.Automatic => "Automatic",
            RenderChannels.Modes.Custom => "Custom",
            _ => throw new UnreachableException(),
        }),
        new("Render channels", "Channels the renderer writes, Automatic adding each post effect's required channels"));
    public static readonly RenderChannelsParameter Distance = new("distance", Member(RenderWindow.StandardChannels.DistanceFromCamera), new("Distance", "Distance from the camera per pixel"));
    public static readonly RenderChannelsParameter Normals = new("normals", Member(RenderWindow.StandardChannels.NormalXYZ), new("Normals", "Surface normal per pixel"));
    public static readonly RenderChannelsParameter Albedo = new("albedo", Member(RenderWindow.StandardChannels.AlbedoRGB), new("Albedo", "Surface base color per pixel"));
    public static readonly RenderChannelsParameter MaterialIds = new("material-ids", Member(RenderWindow.StandardChannels.MaterialIds), new("Material IDs", "Material id per pixel"));
    public static readonly RenderChannelsParameter ObjectIds = new("object-ids", Member(RenderWindow.StandardChannels.ObjectIds), new("Object IDs", "Object id per pixel"));

    public StateParameter<RenderChannelsState> Kind { get; }

    public ParameterText Text { get; }

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
                .Filter(static plugIn => Optional(PlugIn.GetPlugInInfo(plugIn.Key)).Exists(static info => info.PlugInType is PlugInType.Render))
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
    private static float Scalar(Gamma gamma) => gamma;

    [UserMapping]
    private static int Level(ShadowmapQuality quality) => quality.Level;

    [UserMapping]
    private static Dithering.Methods Host(DitherMethod method) => method.Host;

    [UserMapping]
    private static DitherMethod Method(Dithering.Methods method) => DitherMethod.ByHost.Value[method];

    [UserMapping]
    private static Set<Guid> Ids(Guid[]? ids) => toSet(new ReadOnlySpan<Guid>(ids));
}
