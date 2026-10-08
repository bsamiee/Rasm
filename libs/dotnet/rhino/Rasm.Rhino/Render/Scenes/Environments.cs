using System.Diagnostics;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Render.Content;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Display;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Render.Scenes;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SkylightIntensity : System.Numerics.IMinMaxValue<SkylightIntensity> {
    public static SkylightIntensity MinValue { get; } = new(0d);
    public static SkylightIntensity MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record BackgroundState(
    BackgroundStyle BackgroundStyle, Swatch BackgroundColorTop, Swatch BackgroundColorBottom, Option<Guid> BackdropEnvironment,
    bool ScaleBackgroundToFit, bool ReflectionOverride, Option<Guid> ReflectionEnvironment)
    : ISceneRecord<BackgroundState, BackgroundParameter> {
    public static BackgroundState Default { get; } = new(
        BackgroundStyle: BackgroundStyle.SolidColor, BackgroundColorTop: Swatch.White, BackgroundColorBottom: Swatch.Backdrop, BackdropEnvironment: None,
        ScaleBackgroundToFit: false, ReflectionOverride: true, ReflectionEnvironment: None);

    public static string Owner => "background";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => None;

    public static IO<BackgroundState> Read(SceneWindow window) =>
        IO.lift(() => window.Settings switch {
            var settings => new BackgroundState(
                settings.BackgroundStyle, Conversions.Rgb(settings.BackgroundColorTop), Conversions.Rgb(settings.BackgroundColorBottom),
                Conversions.Present(settings.RenderEnvironmentId(RenderSettings.EnvironmentUsage.Background, RenderSettings.EnvironmentPurpose.Standard)), settings.ScaleBackgroundToFit,
                settings.RenderEnvironmentOverride(RenderSettings.EnvironmentUsage.Reflection), Conversions.Present(settings.RenderEnvironmentId(RenderSettings.EnvironmentUsage.Reflection, RenderSettings.EnvironmentPurpose.Standard))),
        });

    public static IO<Unit> Write(SceneWindow window, BackgroundState state) =>
        IO.lift(() => {
            BackgroundMapper.Update(state, window.Settings);
            window.Settings.SetRenderEnvironmentId(RenderSettings.EnvironmentUsage.Background, Conversions.Unset(state.BackdropEnvironment));
            window.Settings.SetRenderEnvironmentOverride(RenderSettings.EnvironmentUsage.Reflection, state.ReflectionOverride);
            window.Settings.SetRenderEnvironmentId(RenderSettings.EnvironmentUsage.Reflection, Conversions.Unset(state.ReflectionEnvironment));
        });

    public static ParameterText Text(BackgroundParameter parameter) =>
        parameter.Map(
            style: ParameterText.Enumeration<BackgroundStyle>("Background", "What the rendering shows behind the scene",
                static style => style switch {
                    BackgroundStyle.SolidColor => "Solid color",
                    BackgroundStyle.WallpaperImage => "Wallpaper",
                    BackgroundStyle.Gradient => "Gradient",
                    BackgroundStyle.Environment => "360˚ Environment",
                    _ => throw new UnreachableException(),
                }),
            backgroundColorTop: ParameterText.Of("Solid color", "Background color, the top of a gradient"),
            backgroundColorBottom: ParameterText.Of("Gradient", "Bottom color of a gradient background"),
            scaleBackgroundToFit: ParameterText.Of("Stretch to fit", "Stretches the viewport's wallpaper to the rendering"),
            reflectionOverride: ParameterText.Of("Use custom environment for reflections", "Reflects a chosen environment in place of the background's"));

    public static RowRules Rules(RowSource<BackgroundState> source, BackgroundParameter parameter) =>
        parameter.Map(
            style: RowRules.Always,
            backgroundColorTop: Shown(source, static style => style is BackgroundStyle.SolidColor or BackgroundStyle.Gradient),
            backgroundColorBottom: Shown(source, static style => style is BackgroundStyle.Gradient),
            scaleBackgroundToFit: Shown(source, static style => style is BackgroundStyle.WallpaperImage),
            reflectionOverride: RowRules.Always);

    private static RowRules Shown(RowSource<BackgroundState> source, Func<BackgroundStyle, bool> styles) =>
        new(None, Some(RowRule.When(source, state => styles(state.BackgroundStyle), BackgroundParameter.Style)));
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BackgroundParameter : IStateParameter<BackgroundState> {
    public static readonly BackgroundParameter Style = new("background-style", new StateParameter<BackgroundState>.Enumerated<BackgroundStyle>(
        Lens<BackgroundState, BackgroundStyle>.New(static state => state.BackgroundStyle, static style => state => state with { BackgroundStyle = style })));
    public static readonly BackgroundParameter BackgroundColorTop = new("background-color-top", new StateParameter<BackgroundState>.Color(
        Lens<BackgroundState, Swatch>.New(static state => state.BackgroundColorTop, static color => state => state with { BackgroundColorTop = color })));
    public static readonly BackgroundParameter BackgroundColorBottom = new("background-color-bottom", new StateParameter<BackgroundState>.Color(
        Lens<BackgroundState, Swatch>.New(static state => state.BackgroundColorBottom, static color => state => state with { BackgroundColorBottom = color })));
    public static readonly BackgroundParameter ScaleBackgroundToFit = new("scale-background-to-fit", new StateParameter<BackgroundState>.Toggle(
        Lens<BackgroundState, bool>.New(static state => state.ScaleBackgroundToFit, static on => state => state with { ScaleBackgroundToFit = on })));
    public static readonly BackgroundParameter ReflectionOverride = new("custom-env-for-refl-and-refr-on", new StateParameter<BackgroundState>.Toggle(
        Lens<BackgroundState, bool>.New(static state => state.ReflectionOverride, static on => state => state with { ReflectionOverride = on })));

    public StateParameter<BackgroundState> Kind { get; }
}

public sealed record LightingState(bool SkylightEnabled, bool SkylightingOverride, Option<Guid> SkylightingEnvironment)
    : ISceneRecord<LightingState, LightingParameter> {
    public static LightingState Default { get; } = new(SkylightEnabled: true, SkylightingOverride: true, SkylightingEnvironment: None);

    public static string Owner => "lighting";

    public static Option<DocumentEvent<RenderPropertyChangedEvent>> Changed => EventKind.SkylightChanged;

    public static IO<LightingState> Read(SceneWindow window) =>
        Sources.SubOwner(window, static settings => settings.Skylight, skylight => IO.lift(() => new LightingState(
            skylight.Enabled,
            window.Settings.RenderEnvironmentOverride(RenderSettings.EnvironmentUsage.Skylighting),
            Conversions.Present(window.Settings.RenderEnvironmentId(RenderSettings.EnvironmentUsage.Skylighting, RenderSettings.EnvironmentPurpose.Standard)))));

    public static IO<Unit> Write(SceneWindow window, LightingState state) =>
        Sources.SubOwner(window, static settings => settings.Skylight, skylight => IO.lift(() => {
            skylight.Enabled = state.SkylightEnabled;
            window.Settings.SetRenderEnvironmentOverride(RenderSettings.EnvironmentUsage.Skylighting, state.SkylightingOverride);
            window.Settings.SetRenderEnvironmentId(RenderSettings.EnvironmentUsage.Skylighting, Conversions.Unset(state.SkylightingEnvironment));
        }));

    public static ParameterText Text(LightingParameter parameter) =>
        parameter.Map(
            skylightEnabled: ParameterText.Of("Skylight", "Lights the scene with the skylighting environment"),
            skylightingOverride: ParameterText.Of("Use custom environment for skylighting", "Lights with a chosen environment in place of the background's"));

    public static RowRules Rules(RowSource<LightingState> source, LightingParameter parameter) =>
        parameter.Map(
            skylightEnabled: RowRules.Always,
            skylightingOverride: new RowRules(None, Some(RowRule.When(source, static state => state.SkylightEnabled, LightingParameter.SkylightEnabled))));
}

[SmartEnum<string>]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LightingParameter : IStateParameter<LightingState> {
    public static readonly LightingParameter SkylightEnabled = new("skylight-on", new StateParameter<LightingState>.Toggle(
        Lens<LightingState, bool>.New(static state => state.SkylightEnabled, static on => state => state with { SkylightEnabled = on })));
    public static readonly LightingParameter SkylightingOverride = new("skylight-custom-environment-on", new StateParameter<LightingState>.Toggle(
        Lens<LightingState, bool>.New(static state => state.SkylightingOverride, static on => state => state with { SkylightingOverride = on })));

    public StateParameter<LightingState> Kind { get; }
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IntensityParameter {
    public static readonly IntensityParameter Intensity = new("intensity");
    public static readonly IntensityParameter Multiplier = new("multiplier");
    public static readonly IntensityParameter AdjustMultiplier = new("rdk-texture-adjust-multiplier");
}

public sealed record PhysicalSky(string Name, bool UseDocumentSun, SkylightIntensity Intensity);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class BackgroundMapper {
    [MapperIgnoreSource(nameof(BackgroundState.BackdropEnvironment), Justification = "Written through SetRenderEnvironmentId")]
    [MapperIgnoreSource(nameof(BackgroundState.ReflectionOverride), Justification = "Written through SetRenderEnvironmentOverride")]
    [MapperIgnoreSource(nameof(BackgroundState.ReflectionEnvironment), Justification = "Written through SetRenderEnvironmentId")]
    internal static partial void Update(BackgroundState state, RenderSettings settings);
}

public static class Environments {
    // --- [CURRENT]
    public static IO<Option<RenderEnvironment>> Current(RhinoDoc doc, RenderSettings.EnvironmentUsage usage) =>
        Sources.Read(doc, window => IO.lift(() => Optional(window.Settings.RenderEnvironment(usage, RenderSettings.EnvironmentPurpose.ForRendering))));

    // --- [INTENSITY]
    public static IO<Option<SkylightIntensity>> Intensity(RenderEnvironment lighting) =>
        IO.lift(() => Optional(lighting.FirstChild))
            .Bind(static child => child.Match(Some: Held, None: static () => IO.pure(Option<(IntensityParameter Parameter, double Gain)>.None)))
            .Bind(static held => IO.lift(held.Traverse(static found => Conversions.Validated<SkylightIntensity, double, InvalidRhinoValue>(found.Gain)).As()));

    public static IO<Unit> SetIntensity(RenderEnvironment lighting, SkylightIntensity intensity) =>
        from texture in IO.lift(() => Missing.Unless(lighting.FirstChild, nameof(RenderContent.FirstChild)))
        from written in Contents.WithinContentChange(texture, RenderContent.ChangeContexts.Program, Gain(texture, intensity))
        select written;

    private static IO<Option<(IntensityParameter Parameter, double Gain)>> Held(RenderContent texture) =>
        toSeq(IntensityParameter.Items)
            .TraverseM(parameter => ContentFields.GetParameter<double>(texture, parameter.Key).Map(gain => gain.Map(held => (Parameter: parameter, Gain: held))))
            .As()
            .Map(static found => found.Somes().Head);

    private static IO<Unit> Gain(RenderContent texture, SkylightIntensity intensity) =>
        from held in Held(texture).Bind(static found => IO.lift(found.ToFin(new Missing(nameof(RenderContent.GetParameter)))))
        from written in ContentFields.SetParameter(texture, held.Parameter.Key, intensity.ToValue())
        select written;

    // --- [PHYSICAL_SKY]
    public static IO<Guid> AddPhysicalSky(RhinoDoc doc, PhysicalSky sky) =>
        (from created in use(Contents.FreeFloating(doc, ContentUuids.BasicEnvironmentType))
         from environment in IO.lift(WrongType.Unless<RenderEnvironment>(created))
         from slot in IO.lift(() => environment.TextureChildSlotName)
         from applied in Contents.Apply(doc, environment, RenderContent.ChangeContexts.Program, Seq<ContentEdit>(
             new ContentEdit.SetName(sky.Name, RenameEvents: false, EnsureNameUnique: false),
             new ContentEdit.SetChild(ContentUuids.PhysicalSkyTextureType, slot)))
         from texture in IO.lift(() => EmptySlot.Unless(environment.FindChild(slot), environment.Id, slot))
         from configured in Contents.WithinContentChange(texture, RenderContent.ChangeContexts.Program,
             ContentFields.SetParameter(texture, "use-document-sun", sky.UseDocumentSun).Bind(_ => Gain(texture, sky.Intensity)))
         from attached in Contents.Attach(doc, environment)
         select environment.Id).Bracket();
}
