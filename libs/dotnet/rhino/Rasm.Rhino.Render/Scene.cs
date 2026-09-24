using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.FileIO;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SettingsSource {
    public sealed record Live(RhinoDoc Doc) : SettingsSource;

    public sealed record Archive(File3dm File) : SettingsSource;

    public sealed record FreeFloating(RenderSettings Settings) : SettingsSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderOutput {
    public sealed record ViewportSized() : RenderOutput;

    public sealed record Sized(Size Pixels, double Dpi, UnitSystem Units) : RenderOutput;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderSource {
    public sealed record ActiveViewport() : RenderSource;

    public sealed record SpecificViewport(string Name) : RenderSource;

    public sealed record NamedView(string Name) : RenderSource;

    public sealed record Snapshot(string Name) : RenderSource;
}

public sealed record EnvironmentBinding(Option<Guid> Content, Option<Guid> Rendering, bool Override);

public sealed record RenderSettingsState(
    Color AmbientLight,
    Color BackgroundColorTop,
    Color BackgroundColorBottom,
    BackgroundStyle BackgroundStyle,
    AntialiasLevel AntialiasLevel,
    int ShadowmapLevel,
    bool UseHiddenLights,
    bool DepthCue,
    bool FlatShade,
    bool RenderBackfaces,
    bool RenderPoints,
    bool RenderCurves,
    bool RenderIsoparams,
    bool RenderMeshEdges,
    bool RenderAnnotations,
    bool TransparentBackground,
    bool ScaleBackgroundToFit,
    RenderOutput Output,
    RenderSource Source,
    Option<Guid> BackgroundEnvironment,
    Map<RenderSettings.EnvironmentUsage, EnvironmentBinding> Environments);

public sealed record GroundPlaneState(
    bool Enabled,
    bool ShadowOnly,
    bool AutoAltitude,
    bool ShowUnderside,
    double Altitude,
    Option<Guid> MaterialInstanceId,
    Vector2d TextureOffset,
    Vector2d TextureSize,
    double TextureRotation,
    bool TextureSizeLocked,
    bool TextureOffsetLocked);

public sealed record SkylightState(bool Enabled, double ShadowIntensity);

public sealed record LinearWorkflowState(bool PreProcessColors, bool PreProcessTextures, bool PostProcessFrameBuffer, bool PostProcessGammaOn, float PreProcessGamma, float PostProcessGamma);

public sealed record DitheringState(Dithering.Methods Method, bool Enabled);

public sealed record SafeFrameState(
    bool Enabled,
    bool PerspectiveOnly,
    bool FieldsOn,
    bool LiveFrameOn,
    bool ActionFrameOn,
    bool ActionFrameLinked,
    double ActionFrameXScale,
    double ActionFrameYScale,
    bool TitleFrameOn,
    bool TitleFrameLinked,
    double TitleFrameXScale,
    double TitleFrameYScale);

public sealed record RenderChannelsState(RenderChannels.Modes Mode, Seq<Guid> CustomList);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SunPlacement {
    public sealed record Automatic(double Latitude, double Longitude, double TimeZone, Option<int> DaylightSavingMinutes, DateTime LocalDateTime) : SunPlacement;

    public sealed record ManualAngles(double Azimuth, double Altitude) : SunPlacement;

    public sealed record ManualVector(Vector3d Direction) : SunPlacement;
}

public sealed record SunState(bool Enabled, double Intensity, Sun.Accuracies Accuracy, double North, SunPlacement Placement);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Scene {
    // --- [SETTINGS]
    public static IO<TValue> WithSettings<TValue>(SettingsSource source, Func<RenderSettings, IO<TValue>> body) =>
        source.Switch(
            body,
            live: static (work, live) => Disposal.Using(() => live.Doc.RenderSettings, work),
            archive: static (work, archive) => work(archive.File.Settings.RenderSettings),
            freeFloating: static (work, floating) => work(floating.Settings));

    public static IO<RenderSettingsState> ReadSettings(RenderSettings settings) =>
        IO.lift(() => SceneMapper.ToState(settings));

    public static IO<Unit> WriteSettings(RenderSettings settings, RenderSettingsState state) =>
        IO.lift(() => {
            SceneMapper.Update(state, settings);
            state.Output.Switch(
                settings,
                viewportSized: static (target, _) => target.UseViewportSize = true,
                sized: static (target, sized) => {
                    target.UseViewportSize = false;
                    target.ImageSize = sized.Pixels;
                    target.ImageDpi = sized.Dpi;
                    target.ImageUnitSystem = sized.Units;
                });
            state.Source.Switch(
                settings,
                activeViewport: static (target, _) => target.RenderSource = RenderSettings.RenderingSources.ActiveViewport,
                specificViewport: static (target, source) => {
                    target.RenderSource = RenderSettings.RenderingSources.SpecificViewport;
                    target.SpecificViewport = source.Name;
                },
                namedView: static (target, source) => {
                    target.RenderSource = RenderSettings.RenderingSources.NamedView;
                    target.NamedView = source.Name;
                },
                snapshot: static (target, source) => {
                    target.RenderSource = RenderSettings.RenderingSources.SnapShot;
                    target.Snapshot = source.Name;
                });
            _ = state.Environments.Iter((usage, binding) => settings.SetRenderEnvironmentOverride(usage, binding.Override));
            settings.SetRenderEnvironmentId(RenderSettings.EnvironmentUsage.Background, state.BackgroundEnvironment.IfNone(Guid.Empty));
            _ = state.Environments.Iter((usage, binding) => settings.SetRenderEnvironmentId(usage, binding.Content.IfNone(Guid.Empty)));
        });

    // --- [SETTINGS_GROUPS]
    public static IO<GroundPlaneState> ReadGroundPlane(RenderSettings settings) =>
        IO.lift(() => SceneMapper.ToState(settings.GroundPlane));

    public static IO<Unit> WriteGroundPlane(RenderSettings settings, GroundPlaneState state) =>
        IO.lift(() => SceneMapper.Update(state, settings.GroundPlane));

    public static IO<SkylightState> ReadSkylight(RenderSettings settings) =>
        IO.lift(() => SceneMapper.ToState(settings.Skylight));

    public static IO<Unit> WriteSkylight(RenderSettings settings, SkylightState state) =>
        IO.lift(() => SceneMapper.Update(state, settings.Skylight));

    public static IO<LinearWorkflowState> ReadLinearWorkflow(RenderSettings settings) =>
        IO.lift(() => SceneMapper.ToState(settings.LinearWorkflow));

    public static IO<Unit> WriteLinearWorkflow(RenderSettings settings, LinearWorkflowState state) =>
        IO.lift(() => SceneMapper.Update(state, settings.LinearWorkflow));

    public static IO<DitheringState> ReadDithering(RenderSettings settings) =>
        IO.lift(() => SceneMapper.ToState(settings.Dithering));

    public static IO<Unit> WriteDithering(RenderSettings settings, DitheringState state) =>
        IO.lift(() => SceneMapper.Update(state, settings.Dithering));

    public static IO<SafeFrameState> ReadSafeFrame(RenderSettings settings) =>
        IO.lift(() => SceneMapper.ToState(settings.SafeFrame));

    public static IO<Unit> WriteSafeFrame(RenderSettings settings, SafeFrameState state) =>
        IO.lift(() => SceneMapper.Update(state, settings.SafeFrame));

    public static IO<RenderChannelsState> ReadRenderChannels(RenderSettings settings) =>
        IO.lift(() => SceneMapper.ToState(settings.RenderChannels));

    public static IO<Unit> WriteRenderChannels(RenderSettings settings, RenderChannelsState state) =>
        from channels in IO.lift(() => settings.RenderChannels)
        from written in IO.lift(() => {
            channels.CustomList = [.. state.CustomList];
            channels.Mode = state.Mode;
        })
        select written;

    // --- [SUN]
    public static IO<SunState> ReadSun(RenderSettings settings) =>
        IO.lift(() => SceneMapper.ToState(settings.Sun));

    public static IO<Unit> WriteSun(RenderSettings settings, SunState state) =>
        from placed in IO.lift(() => Placed(state.Placement))
        from sun in IO.lift(() => settings.Sun)
        from written in IO.lift(() => {
            SceneMapper.Update(state, sun);
            placed(sun);
        })
        select written;

    private static Fin<Action<Sun>> Placed(SunPlacement placement) =>
        placement.Switch(
            automatic: static automatic => Invalid.Unless<Action<Sun>>(
                automatic.LocalDateTime.Kind == DateTimeKind.Local,
                target => {
                    SceneMapper.Update(automatic, target);
                    target.SetDateTime(automatic.LocalDateTime, DateTimeKind.Local);
                    target.ManualControlOn = false;
                },
                nameof(DateTime.Kind)),
            manualAngles: static angles => Fin.Succ<Action<Sun>>(target => {
                target.ManualControlOn = true;
                SceneMapper.Update(angles, target);
            }),
            manualVector: static vector => Fin.Succ<Action<Sun>>(target => {
                target.ManualControlOn = true;
                target.Vector = vector.Direction;
            }));

    public static IO<TValue> WithSunLight<TValue>(RenderSettings settings, Func<Light, IO<TValue>> body) =>
        Disposal.Using(() => settings.Sun.Light, body);

    public static IO<Option<(double Latitude, double Longitude)>> Here { get; } =
        IO.lift(static () => Answers.Found(Sun.Here(out double latitude, out double longitude), (Latitude: latitude, Longitude: longitude)));
}

[Mapper]
public static partial class SceneMapper {
    [MapPropertyFromSource(nameof(RenderSettingsState.Output), Use = nameof(Output))]
    [MapPropertyFromSource(nameof(RenderSettingsState.Source), Use = nameof(Source))]
    [MapPropertyFromSource(nameof(RenderSettingsState.BackgroundEnvironment), Use = nameof(BackgroundEnvironment))]
    [MapPropertyFromSource(nameof(RenderSettingsState.Environments), Use = nameof(Environments))]
    internal static partial RenderSettingsState ToState(RenderSettings settings);

    internal static partial GroundPlaneState ToState(GroundPlane ground);

    internal static partial SkylightState ToState(Skylight skylight);

    public static partial LinearWorkflowState ToState(LinearWorkflow workflow);

    internal static partial DitheringState ToState(Dithering dithering);

    internal static partial SafeFrameState ToState(SafeFrame frame);

    internal static partial RenderChannelsState ToState(RenderChannels channels);

    [MapPropertyFromSource(nameof(SunState.Placement), Use = nameof(Placement))]
    internal static partial SunState ToState(Sun sun);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(RenderSettingsState.Output), Justification = "Written through the RenderOutput cases")]
    [MapperIgnoreSource(nameof(RenderSettingsState.Source), Justification = "Written through the RenderSource cases")]
    [MapperIgnoreSource(nameof(RenderSettingsState.BackgroundEnvironment), Justification = "Written through SetRenderEnvironmentId")]
    [MapperIgnoreSource(nameof(RenderSettingsState.Environments), Justification = "Written through the per-usage environment setters")]
    internal static partial void Update(RenderSettingsState state, RenderSettings settings);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(GroundPlaneState state, GroundPlane ground);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(SkylightState state, Skylight skylight);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(LinearWorkflowState state, LinearWorkflow workflow);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(DitheringState state, Dithering dithering);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(SafeFrameState state, SafeFrame frame);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(SunState.Placement), Justification = "Written through the SunPlacement cases")]
    internal static partial void Update(SunState state, Sun sun);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(@SunPlacement.Automatic.DaylightSavingMinutes.IsSome), nameof(Sun.DaylightSavingOn))]
    [MapperIgnoreSource(nameof(SunPlacement.Automatic.LocalDateTime), Justification = "Written through SetDateTime")]
    internal static partial void Update(SunPlacement.Automatic automatic, Sun sun);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(SunPlacement.ManualAngles angles, Sun sun);

    [UserMapping]
    private static Guid MaterialInstanceId(Option<Guid> id) => id.IfNone(Guid.Empty);

    [UserMapping]
    private static Seq<Guid> ToSeq(Guid[]? ids) => toSeq(ids);

    private static RenderOutput Output(RenderSettings settings) =>
        settings.UseViewportSize ? new RenderOutput.ViewportSized() : new RenderOutput.Sized(settings.ImageSize, settings.ImageDpi, settings.ImageUnitSystem);

    private static RenderSource Source(RenderSettings settings) =>
        settings.RenderSource switch {
            RenderSettings.RenderingSources.ActiveViewport => new RenderSource.ActiveViewport(),
            RenderSettings.RenderingSources.SpecificViewport => new RenderSource.SpecificViewport(settings.SpecificViewport),
            RenderSettings.RenderingSources.NamedView => new RenderSource.NamedView(settings.NamedView),
            RenderSettings.RenderingSources.SnapShot => new RenderSource.Snapshot(settings.Snapshot),
        };

    private static Option<Guid> BackgroundEnvironment(RenderSettings settings) =>
        Answers.Present(settings.RenderEnvironmentId(RenderSettings.EnvironmentUsage.Background, RenderSettings.EnvironmentPurpose.Standard));

    private static Map<RenderSettings.EnvironmentUsage, EnvironmentBinding> Environments(RenderSettings settings) =>
        toMap(toSeq(Enum.GetValues<RenderSettings.EnvironmentUsage>())
            .Filter(static usage => usage != RenderSettings.EnvironmentUsage.Background)
            .Map(usage => (Usage: usage, Binding: new EnvironmentBinding(
                Answers.Present(settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.Standard)),
                Answers.Present(settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.ForRendering)),
                settings.RenderEnvironmentOverride(usage)))));

    private static SunPlacement Placement(Sun sun) =>
        sun.ManualControlOn
            ? new SunPlacement.ManualAngles(sun.Azimuth, sun.Altitude)
            : new SunPlacement.Automatic(
                sun.Latitude,
                sun.Longitude,
                sun.TimeZone,
                sun.DaylightSavingOn ? Some(sun.DaylightSavingMinutes) : Option<int>.None,
                sun.GetDateTime(DateTimeKind.Local));

    [UserMapping]
    private static int DaylightSavingMinutes(Option<int> minutes, [MappingTargetOriginalValue] int current) => minutes.IfNone(current);
}
