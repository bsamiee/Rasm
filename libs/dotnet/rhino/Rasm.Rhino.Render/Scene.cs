using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<ValidationFailure>]
public sealed partial class ImageOutput {
    public Size Pixels { get; }

    public double Dpi { get; }

    public UnitSystem Units { get; }

    public static Fin<ImageOutput> From(Size pixels, double dpi, UnitSystem units) =>
        Validate(pixels, dpi, units, out ImageOutput? output) is { } error ? error : output!;

    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref Size pixels, ref double dpi, ref UnitSystem units) =>
        validationError = Limits.AtLeast(1).Violated(Math.Min(pixels.Width, pixels.Height), nameof(Pixels))
            ?? Limits.Above(0.0).Violated(dpi, nameof(Dpi))
            ?? Answers.FirstInvalid((units is not (UnitSystem.None or UnitSystem.Inches or UnitSystem.Millimeters or UnitSystem.Centimeters), nameof(Units)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderSource {
    public sealed record ActiveViewport() : RenderSource;

    public sealed record SpecificViewport(string Name) : RenderSource;

    public sealed record NamedView(string Name) : RenderSource;

    public sealed record Snapshot(string Name) : RenderSource;
}

public sealed record RenderSettingsState(
    Color AmbientLight,
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
    Option<ImageOutput> Output,
    RenderSource Source);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Backdrop {
    public sealed record SolidColor(Color Color) : Backdrop;

    public sealed record Gradient(Color Top, Color Bottom) : Backdrop;

    public sealed record Wallpaper(bool StretchToFit) : Backdrop;

    public sealed record Environment(Option<Guid> Id) : Backdrop;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnvironmentSource {
    public sealed record FromBackdrop() : EnvironmentSource;

    public sealed record Overridden(Option<Guid> Id) : EnvironmentSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GroundEffect {
    public sealed record ShadowCatcher() : GroundEffect;

    public sealed record Material(Option<Guid> Instance) : GroundEffect;
}

public sealed record GroundPlaneState(
    bool Enabled,
    bool ShowUnderside,
    Option<double> Altitude,
    GroundEffect Effect,
    Vector2d TextureOffset,
    bool TextureOffsetLocked,
    Vector2d TextureSize,
    bool TextureSizeLocked,
    double TextureRotation);

public sealed record SunState(
    bool Enabled,
    double Intensity,
    double North,
    double Latitude,
    double Longitude,
    double TimeZone,
    Option<int> DaylightSavingMinutes,
    DateTime LocalMoment,
    Option<(double Azimuth, double Altitude)> Manual);

public sealed record LinearWorkflowState(bool PreProcessColors, float PostProcessGamma, bool PostProcessGammaOn);

public sealed class SceneSetting<T> {
    internal SceneSetting(Func<RenderSettings, Fin<T>> read, Action<RenderSettings, T> write) => (Read, Write) = (read, write);

    public Func<RenderSettings, Fin<T>> Read { get; }

    public Func<RenderSettings, IO<Unit>> Set(T value) =>
        settings => IO.lift(() => Write(settings, value));

    private Action<RenderSettings, T> Write { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class SceneMapper {
    [MapPropertyFromSource(nameof(RenderSettingsState.Source), Use = nameof(Source))]
    internal static partial RenderSettingsState ToState(RenderSettings settings, Option<ImageOutput> output);

    [MapPropertyFromSource(nameof(GroundPlaneState.Altitude), Use = nameof(Altitude))]
    [MapPropertyFromSource(nameof(GroundPlaneState.Effect), Use = nameof(Effect))]
    internal static partial GroundPlaneState ToState(GroundPlane ground);

    [MapPropertyFromSource(nameof(SunState.DaylightSavingMinutes), Use = nameof(DaylightSaving))]
    [MapPropertyFromSource(nameof(SunState.LocalMoment), Use = nameof(LocalMoment))]
    [MapPropertyFromSource(nameof(SunState.Manual), Use = nameof(Manual))]
    internal static partial SunState ToState(Sun sun);

    internal static partial LinearWorkflowState ToState(LinearWorkflow workflow);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(@RenderSettingsState.Output.IsNone), nameof(RenderSettings.UseViewportSize))]
    [MapperIgnoreSource(nameof(RenderSettingsState.Source), Justification = "Written through the RenderSource cases")]
    internal static partial void Update(RenderSettingsState state, RenderSettings settings);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(ImageOutput.Pixels), nameof(RenderSettings.ImageSize))]
    [MapProperty(nameof(ImageOutput.Dpi), nameof(RenderSettings.ImageDpi))]
    [MapProperty(nameof(ImageOutput.Units), nameof(RenderSettings.ImageUnitSystem))]
    internal static partial void Update(ImageOutput output, RenderSettings settings);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(@GroundPlaneState.Altitude.IsNone), nameof(GroundPlane.AutoAltitude))]
    [MapperIgnoreSource(nameof(GroundPlaneState.Effect), Justification = "Written through the GroundEffect cases")]
    internal static partial void Update(GroundPlaneState state, GroundPlane ground);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(@SunState.DaylightSavingMinutes.IsSome), nameof(Sun.DaylightSavingOn))]
    [MapperIgnoreSource(nameof(SunState.LocalMoment), Justification = "Written through SetDateTime")]
    [MapperIgnoreSource(nameof(SunState.Manual), Justification = "ManualControlOn is written before the angles")]
    internal static partial void Update(SunState state, Sun sun);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(LinearWorkflowState state, LinearWorkflow workflow);

    [UserMapping]
    private static T Kept<T>(Option<T> value, [MappingTargetOriginalValue] T current) where T : struct =>
        value.IfNone(current);

    private static RenderSource Source(RenderSettings settings) =>
        settings.RenderSource switch {
            RenderSettings.RenderingSources.ActiveViewport => new RenderSource.ActiveViewport(),
            RenderSettings.RenderingSources.SpecificViewport => new RenderSource.SpecificViewport(settings.SpecificViewport),
            RenderSettings.RenderingSources.NamedView => new RenderSource.NamedView(settings.NamedView),
            RenderSettings.RenderingSources.SnapShot => new RenderSource.Snapshot(settings.Snapshot),
        };

    private static Option<double> Altitude(GroundPlane ground) =>
        Answers.Found(!ground.AutoAltitude, ground.Altitude);

    private static GroundEffect Effect(GroundPlane ground) =>
        ground.ShadowOnly
            ? new GroundEffect.ShadowCatcher()
            : new GroundEffect.Material(Answers.Present(ground.MaterialInstanceId).Filter(static id => id != ContentUuids.DefaultMaterialInstance));

    private static Option<int> DaylightSaving(Sun sun) =>
        Answers.Found(sun.DaylightSavingOn, sun.DaylightSavingMinutes);

    private static DateTime LocalMoment(Sun sun) =>
        sun.GetDateTime(DateTimeKind.Local);

    private static Option<(double Azimuth, double Altitude)> Manual(Sun sun) =>
        Answers.Found(sun.ManualControlOn, (sun.Azimuth, sun.Altitude));
}

public static class Scene {
    // --- [SETTINGS]
    public static readonly SceneSetting<RenderSettingsState> Settings = new(
        static settings => (settings.UseViewportSize
                ? Option<ImageOutput>.None
                : ImageOutput.From(settings.ImageSize, settings.ImageDpi, settings.ImageUnitSystem).Map(static output => Some(output)))
            .Map(output => SceneMapper.ToState(settings, output)),
        static (settings, state) => {
            SceneMapper.Update(state, settings);
            _ = state.Output.Iter(output => SceneMapper.Update(output, settings));
            state.Source.Switch(
                settings,
                activeViewport: static (target, _) => target.RenderSource = RenderSettings.RenderingSources.ActiveViewport,
                specificViewport: static (target, source) => (target.RenderSource, target.SpecificViewport) = (RenderSettings.RenderingSources.SpecificViewport, source.Name),
                namedView: static (target, source) => (target.RenderSource, target.NamedView) = (RenderSettings.RenderingSources.NamedView, source.Name),
                snapshot: static (target, source) => (target.RenderSource, target.Snapshot) = (RenderSettings.RenderingSources.SnapShot, source.Name));
        });

    public static readonly SceneSetting<Backdrop> Background = new(
        static settings => settings.BackgroundStyle switch {
            BackgroundStyle.SolidColor => new Backdrop.SolidColor(settings.BackgroundColorTop),
            BackgroundStyle.WallpaperImage => new Backdrop.Wallpaper(settings.ScaleBackgroundToFit),
            BackgroundStyle.Gradient => new Backdrop.Gradient(settings.BackgroundColorTop, settings.BackgroundColorBottom),
            BackgroundStyle.Environment => new Backdrop.Environment(Answers.Present(settings.RenderEnvironmentId(RenderSettings.EnvironmentUsage.Background, RenderSettings.EnvironmentPurpose.Standard))),
        },
        static (settings, backdrop) => backdrop.Switch(
            settings,
            solidColor: static (target, solid) => (target.BackgroundStyle, target.BackgroundColorTop) = (BackgroundStyle.SolidColor, solid.Color),
            gradient: static (target, gradient) => (target.BackgroundStyle, (target.BackgroundColorTop, target.BackgroundColorBottom)) = (BackgroundStyle.Gradient, gradient),
            wallpaper: static (target, wallpaper) => (target.BackgroundStyle, target.ScaleBackgroundToFit) = (BackgroundStyle.WallpaperImage, wallpaper.StretchToFit),
            environment: static (target, environment) => {
                target.BackgroundStyle = BackgroundStyle.Environment;
                target.SetRenderEnvironmentId(RenderSettings.EnvironmentUsage.Background, Answers.Unset(environment.Id));
            }));

    public static readonly SceneSetting<EnvironmentSource> Reflection = Channel(RenderSettings.EnvironmentUsage.Reflection);

    public static readonly SceneSetting<EnvironmentSource> Skylighting = Channel(RenderSettings.EnvironmentUsage.Skylighting);

    public static readonly SceneSetting<LinearWorkflowState> LinearWorkflow = new(
        static settings => SceneMapper.ToState(settings.LinearWorkflow),
        static (settings, state) => SceneMapper.Update(state, settings.LinearWorkflow));

    public static readonly SceneSetting<Dithering.Methods> Dither = new(
        static settings => settings.Dithering.Enabled ? settings.Dithering.Method : Dithering.Methods.None,
        static (settings, method) => {
            Dithering dithering = settings.Dithering;
            dithering.Enabled = method != Dithering.Methods.None;
            if (method != Dithering.Methods.None)
                dithering.Method = method;
        });

    public static readonly SceneSetting<Option<Set<Guid>>> Channels = new(
        static settings => Answers.Found(settings.RenderChannels.Mode == RenderChannels.Modes.Custom, toSet(settings.RenderChannels.CustomList)),
        static (settings, custom) => {
            RenderChannels channels = settings.RenderChannels;
            _ = custom.Iter(ids => channels.CustomList = [.. ids]);
            channels.Mode = custom.IsSome ? RenderChannels.Modes.Custom : RenderChannels.Modes.Automatic;
        });

    public static readonly SceneSetting<GroundPlaneState> GroundPlane = new(
        static settings => SceneMapper.ToState(settings.GroundPlane),
        static (settings, state) => {
            GroundPlane ground = settings.GroundPlane;
            SceneMapper.Update(state, ground);
            state.Effect.Switch(
                ground,
                shadowCatcher: static (target, _) => target.ShadowOnly = true,
                material: static (target, material) => (target.ShadowOnly, target.MaterialInstanceId) = (false, Answers.Unset(material.Instance)));
        });

    public static readonly SceneSetting<SunState> Sun = new(
        static settings => SceneMapper.ToState(settings.Sun),
        static (settings, state) => {
            Sun sun = settings.Sun;
            SceneMapper.Update(state, sun);
            sun.SetDateTime(state.LocalMoment, DateTimeKind.Local);
            sun.ManualControlOn = state.Manual.IsSome;
            _ = state.Manual.Iter(angles => (sun.Azimuth, sun.Altitude) = angles);
        });

    public static IO<Unit> Edit(RhinoDoc doc, Seq<Func<RenderSettings, IO<Unit>>> edits) =>
        DisposalOps.Using(() => doc.RenderSettings, live => DisposalOps.Using(() => live.Duplicate(), staged =>
            from edited in edits.TraverseM(edit => edit(staged)).As()
            from committed in IO.lift(() => { doc.RenderSettings = staged; })
            select committed));

    private static SceneSetting<EnvironmentSource> Channel(RenderSettings.EnvironmentUsage usage) =>
        new(
            settings => settings.RenderEnvironmentOverride(usage)
                ? new EnvironmentSource.Overridden(Answers.Present(settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.Standard)))
                : new EnvironmentSource.FromBackdrop(),
            (settings, source) => source.Switch(
                (Settings: settings, Usage: usage),
                fromBackdrop: static (state, _) => state.Settings.SetRenderEnvironmentOverride(state.Usage, on: false),
                overridden: static (state, overridden) => {
                    state.Settings.SetRenderEnvironmentId(state.Usage, Answers.Unset(overridden.Id));
                    state.Settings.SetRenderEnvironmentOverride(state.Usage, on: true);
                }));

    // --- [SKYLIGHT]
    public static IO<double> ReadSkylightIntensity(RhinoDoc doc) =>
        SkyTexture(doc).Bind(Intensity).Map(static held => held.Value);

    public static IO<Unit> WriteSkylightIntensity(RhinoDoc doc, double intensity) =>
        from texture in SkyTexture(doc)
        from held in Intensity(texture)
        from written in SetParameters(texture, Seq<(string Name, FieldValue Value)>((held.Name, new FieldValue.Double(intensity))))
        select written;

    public static IO<TValue> WithPhysicalSky<TValue>(RhinoDoc doc, string name, bool useDocumentSun, double multiplier, Func<RenderEnvironment, IO<TValue>> body) =>
        Contents.WithCreated(doc, new ContentSource.FromTypeId(ContentUuids.BasicEnvironmentType), content =>
            from environment in IO.lift(() => Optional(content as RenderEnvironment).ToFin(new InvalidAnswer(nameof(RenderContentType.NewContentFromTypeId))))
            from value in Contents.WithCreated(doc, new ContentSource.FromTypeId(ContentUuids.PhysicalSkyTextureType), texture =>
                from parameters in SetParameters(texture, Seq<(string Name, FieldValue Value)>(("use-document-sun", new FieldValue.Bool(useDocumentSun)), (AdjustMultiplier, new FieldValue.Double(multiplier))))
                from named in Contents.Apply(environment, RenderContent.ChangeContexts.Program, new ContentOp.Rename(name, RenameEvents: false, EnsureUnique: false))
                from child in Contents.Apply(environment, RenderContent.ChangeContexts.Program, new ContentOp.SetChild(environment.TextureChildSlotName, texture))
                from result in body(environment)
                select result)
            select value);

    private const string AdjustMultiplier = "rdk-texture-adjust-multiplier";

    /// <summary>Parameters the Rendering panel reads as a sky texture's intensity, the first one the texture holds answering</summary>
    private static readonly Seq<string> IntensityParameters = Seq("intensity", "multiplier", AdjustMultiplier);

    private static IO<Unit> SetParameters(RenderContent content, Seq<(string Name, FieldValue Value)> parameters) =>
        Contents.WithinContentChange(
            content,
            RenderContent.ChangeContexts.Program,
            parameters.TraverseM(parameter => ContentFields.WriteParameter(content, parameter.Name, parameter.Value)).As().Map(static _ => unit));

    private static IO<RenderContent> SkyTexture(RhinoDoc doc) =>
        from environment in DisposalOps.Using(() => doc.RenderSettings, static settings => IO.lift(() =>
            Seq(RenderSettings.EnvironmentUsage.Skylighting, RenderSettings.EnvironmentUsage.Background)
                .Find(settings.RenderEnvironmentOverride)
                .Bind(usage => Optional(settings.RenderEnvironment(usage, RenderSettings.EnvironmentPurpose.Standard)))
                .ToFin(new Missing(nameof(RenderSettings.RenderEnvironment)))))
        from texture in IO.lift(() => Missing.Unless(environment.FirstChild, nameof(RenderContent.FirstChild)))
        select texture;

    private static IO<(string Name, double Value)> Intensity(RenderContent texture) =>
        IntensityParameters.FoldBack(
            IO.fail<(string Name, double Value)>(new Missing(nameof(RenderContent.GetParameter))),
            (next, name) => ContentFields.ReadParameter<double>(texture, new ParameterRef.Named(name))
                .Bind(read => read.Map(value => IO.pure((Name: name, Value: value))).IfNone(next)));
}
