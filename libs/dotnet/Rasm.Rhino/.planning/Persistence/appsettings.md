# [RASM_RHINO_PERSISTENCE_APPSETTINGS]

`AppSettingsFamily` closes each `Rhino.ApplicationSettings` static owner as one row carrying capture, default, apply, and reset delegates beside the verb set they derive from; `AppState` carries each serializable `*State` snapshot as one case naming its own payload and owning family, so a family round-trips as one detached snapshot, never as mid-flight mutable statics. `AppSettings.Commit` interprets state round-trips, theme and swatch policy, registry custody, window placement, and analysis solves into detached answers carrying prior/current observations.

Owner boundary against the settings tree is settled: `SettingsRoot.ApplicationCase` (settings.md) resolves the raw `PersistentSettings.RhinoAppSettings` node tree — a lazy `FromPlugInId(RhinoApp.CurrentRhinoId)` property, not a typed surface — while this page owns the distinct `Rhino.ApplicationSettings` static families whose storage merely rides that tree. A typed preference edit enters here through the owning family's state round-trip; a raw node read or write enters settings.md; neither owner reaches across.

## [01]-[INDEX]

- [02]-[STATE_AND_FAMILY]: `SoftTransformState`, `PackageManagerState`, `AppState`, `FamilyVerb`, `AppSettingsFamily`, `AppTheme` — the preference payloads, the family rows, and the theme rows.
- [03]-[REQUEST_ALGEBRA]: `AliasName`, `MacroText`, `RegistryMerge`, `AliasBinding`, `ShortcutBinding`, `AliasEdit`, `ShortcutEdit`, `RepeatEdit`, `PathEdit`, `ExtrusionDefault`, `CreasePolicy`, `GeneralConduct`, `RepeatRoster`, `SwatchSlot`, `AppOperation`, `AppObservation`, `AppMutation`, `SwatchMutation`, `AppAnswer` — the request, edit, and answer vocabularies.
- [04]-[INTERPRETER]: `AppSettings` — the writer seat, the admission fold, and the total dispatch over the host statics.

## [02]-[STATE_AND_FAMILY]

- Owner: `AppState` — the closed preference-payload family; `AppSettingsFamily` — one row per `Rhino.ApplicationSettings` static owner, carrying the capture, default, apply, and reset columns beside `Verbs`; `AppTheme` — the two dark/light rows behind one adopt, preset, and seated-probe trio; `FamilyVerb` — the verb vocabulary a family's coverage is stated in.
- Entry: an `AppState` case declares its payload, its owning family, and the boxed carrier the family lowers through in ONE declaration, so `Family` and `Payload` are base columns rather than two parallel twenty-two-arm folds and a new preference owner is one case beside one row.
- Law: capture is the one column no row omits — a family that cannot be observed is unspellable. `SoftTransformSettings` and `PackageManagerSettings` publish reachable read/write statics but no `*State` type, no `GetDefaultState`, and no `RestoreDefaults`, so their whole-state value is minted here from the host's own knobs while default and reset refuse typed.
- Law: `Verbs` is the boundary gate and the delegate refusal is its floor. Both derive from the SAME factory argument — a supplied preset reader, state writer, or restore verb — so they cannot disagree; the floor survives because a family row is public and a composition can hold one directly without passing admission.
- Law: the family lowers a state through the base `Payload` column with an unconditional cast, because `Family` is the case's OWN declaration: the row that receives an apply is the row the state named, and a per-row lowering probe answering `None` was a branch no call site reaches.
- Law: the case payload is the HOST `*State` class, not a mirrored record. `GetCurrentState` mints a fresh instance per call rather than handing out a live view of the statics, so the value that crosses this boundary is already detached and the host's own `GetCurrentState` → mutate → `UpdateFromState` triple is the atomic edit the catalog's owner-law names. A twenty-record projection beside it buys no detachment and loses two things: twenty rosters to keep aligned against host drift, and `CurvatureAnalysisSettings.CalculateCurvatureAutoRange`, whose `ref` parameter is the host state type itself, so every mirror marshals back through the shape it replaced.
- Law: `AppTheme` keys on the host `darkMode` flag itself, so `GetDefaultState(darkMode)` and `DefaultPaintColor(slot, darkMode)` read the row's key and no member restates it.
- Law: `PackageManagerState` refuses a source containing the host's join character at construction. The host stores its roster as one semicolon-joined string, so a source carrying that character round-trips as two sources — the corruption is unrepresentable rather than guarded per call site.
- Growth: a new preference owner is one `AppState` case beside one `AppSettingsFamily` row; a new verb is one `FamilyVerb` row and one column.
- Packages: Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum<TKey>]`, `[Union]`, `[ComplexValueObject]`, `[UseDelegateFromConstructor]`, `[ValidationError]`); LanguageExt.Core (`api-languageext.md` — `Fin`, `Option`, `Seq`); kernel `Domain/results` (`Try.lift`), `Domain/validation` (`ICapability`, `CapabilitySet`); `Persistence/presets` (`PersistenceFault`); RhinoCommon application settings (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-appsettings.md` — the `GetCurrentState`/`GetDefaultState`/`UpdateFromState`/`RestoreDefaults` quartet on every state-carrying owner, `AppearanceSettings.GetDefaultState(bool darkMode)`, `SetToDarkMode`/`SetToLightMode`, `UsingDefaultDarkModeColors`/`UsingDefaultLightModeColors`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Drawing;
using Rasm.Domain;
using Rhino.ApplicationSettings;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class FamilyVerb : ICapability<FamilyVerb> {
    public static readonly FamilyVerb Capture = new("capture");
    public static readonly FamilyVerb Preset = new("preset");
    public static readonly FamilyVerb Apply = new("apply");
    public static readonly FamilyVerb Reset = new("reset");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SoftTransformState(
    bool Enabled,
    double Radius,
    int Shape,
    bool MeasureDistanceAlong,
    int CvCountU,
    int CvCountV,
    bool ShowConstraintWidgets,
    Color FalloffColor);

[ComplexValueObject]
[ValidationError]
public sealed partial record PackageManagerState(Seq<string> Sources) {
    internal const char Separator = ';';

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<string> sources) {
        Seq<string> spliced = sources.Filter(static row => row.Contains(Separator, StringComparison.Ordinal));
        validationError = spliced.IsEmpty
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { $"Package-manager sources must not contain '{Separator}': [{string.Join(", ", spliced)}]." }));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AppState(object Payload, AppSettingsFamily Family) {
    public sealed record AppearanceCase(AppearanceSettingsState Value)
        : AppState(Value, AppSettingsFamily.Appearance);
    public sealed record ModelAidCase(ModelAidSettingsState Value)
        : AppState(Value, AppSettingsFamily.ModelAid);
    public sealed record FilesCase(FileSettingsState Value)
        : AppState(Value, AppSettingsFamily.Files);
    public sealed record GeneralCase(GeneralSettingsState Value)
        : AppState(Value, AppSettingsFamily.General);
    public sealed record ViewsCase(ViewSettingsState Value)
        : AppState(Value, AppSettingsFamily.Views);
    public sealed record OpenGlCase(OpenGLSettingsState Value)
        : AppState(Value, AppSettingsFamily.OpenGl);
    public sealed record CursorTooltipCase(CursorTooltipSettingsState Value)
        : AppState(Value, AppSettingsFamily.CursorTooltip);
    public sealed record SmartTrackCase(SmartTrackSettingsState Value)
        : AppState(Value, AppSettingsFamily.SmartTrack);
    public sealed record GumballCase(GumballSettingsState Value)
        : AppState(Value, AppSettingsFamily.Gumball);
    public sealed record SelectionFilterCase(SelectionFilterSettingsState Value)
        : AppState(Value, AppSettingsFamily.SelectionFilter);
    public sealed record ChooseOneObjectCase(ChooseOneObjectSettingsState Value)
        : AppState(Value, AppSettingsFamily.ChooseOneObject);
    public sealed record SoftTransformCase(SoftTransformState Value)
        : AppState(Value, AppSettingsFamily.SoftTransform);
    public sealed record PackageManagerCase(PackageManagerState Value)
        : AppState(Value, AppSettingsFamily.PackageManager);
    public sealed record CurvatureCase(CurvatureAnalysisSettingsState Value)
        : AppState(Value, AppSettingsFamily.Curvature);
    public sealed record CurvatureGraphCase(CurvatureGraphSettingsState Value)
        : AppState(Value, AppSettingsFamily.CurvatureGraph);
    public sealed record DraftAngleCase(DraftAngleAnalysisSettingsState Value)
        : AppState(Value, AppSettingsFamily.DraftAngle);
    public sealed record EdgeCase(EdgeAnalysisSettingsState Value)
        : AppState(Value, AppSettingsFamily.Edge);
    public sealed record EndCase(EndAnalysisSettingsState Value)
        : AppState(Value, AppSettingsFamily.End);
    public sealed record DirectionCase(DirectionAnalysisSettingsState Value)
        : AppState(Value, AppSettingsFamily.Direction);
    public sealed record EmapCase(EmapAnalysisSettingsState Value)
        : AppState(Value, AppSettingsFamily.Emap);
    public sealed record ZebraCase(ZebraAnalysisSettingsState Value)
        : AppState(Value, AppSettingsFamily.Zebra);
    public sealed record ThicknessCase(ThicknessAnalysisSettingsState Value)
        : AppState(Value, AppSettingsFamily.Thickness);
}

[SmartEnum<string>]
public sealed partial class AppSettingsFamily {
    public static readonly AppSettingsFamily Appearance = Of<AppearanceSettingsState>(
        key: "appearance",
        current: AppearanceSettings.GetCurrentState,
        lift: static state => new AppState.AppearanceCase(Value: state),
        preset: static () => AppearanceSettings.GetDefaultState(),
        update: AppearanceSettings.UpdateFromState,
        restore: AppearanceSettings.RestoreDefaults);
    public static readonly AppSettingsFamily ModelAid = Of<ModelAidSettingsState>(
        key: "model-aid",
        current: ModelAidSettings.GetCurrentState,
        lift: static state => new AppState.ModelAidCase(Value: state),
        preset: ModelAidSettings.GetDefaultState,
        update: ModelAidSettings.UpdateFromState,
        restore: ModelAidSettings.RestoreDefaults);
    public static readonly AppSettingsFamily Files = Of<FileSettingsState>(
        key: "files",
        current: FileSettings.GetCurrentState,
        lift: static state => new AppState.FilesCase(Value: state),
        preset: FileSettings.GetDefaultState,
        update: FileSettings.UpdateFromState,
        restore: FileSettings.RestoreDefaults);
    public static readonly AppSettingsFamily General = Of<GeneralSettingsState>(
        key: "general",
        current: GeneralSettings.GetCurrentState,
        lift: static state => new AppState.GeneralCase(Value: state),
        preset: GeneralSettings.GetDefaultState,
        update: GeneralSettings.UpdateFromState,
        restore: GeneralSettings.RestoreDefaults);
    public static readonly AppSettingsFamily Views = Of<ViewSettingsState>(
        key: "views",
        current: ViewSettings.GetCurrentState,
        lift: static state => new AppState.ViewsCase(Value: state),
        preset: ViewSettings.GetDefaultState,
        update: ViewSettings.UpdateFromState,
        restore: ViewSettings.RestoreDefaults);
    public static readonly AppSettingsFamily OpenGl = Of<OpenGLSettingsState>(
        key: "opengl",
        current: OpenGLSettings.GetCurrentState,
        lift: static state => new AppState.OpenGlCase(Value: state),
        preset: OpenGLSettings.GetDefaultState,
        update: OpenGLSettings.UpdateFromState,
        restore: OpenGLSettings.RestoreDefaults);
    public static readonly AppSettingsFamily CursorTooltip = Of<CursorTooltipSettingsState>(
        key: "cursor-tooltip",
        current: CursorTooltipSettings.GetCurrentState,
        lift: static state => new AppState.CursorTooltipCase(Value: state),
        preset: CursorTooltipSettings.GetDefaultState,
        update: CursorTooltipSettings.UpdateFromState,
        restore: CursorTooltipSettings.RestoreDefaults);
    public static readonly AppSettingsFamily SmartTrack = Of<SmartTrackSettingsState>(
        key: "smart-track",
        current: SmartTrackSettings.GetCurrentState,
        lift: static state => new AppState.SmartTrackCase(Value: state),
        preset: SmartTrackSettings.GetDefaultState,
        update: SmartTrackSettings.UpdateFromState,
        restore: SmartTrackSettings.RestoreDefaults);
    public static readonly AppSettingsFamily Gumball = Of<GumballSettingsState>(
        key: "gumball",
        current: GumballSettings.GetCurrentState,
        lift: static state => new AppState.GumballCase(Value: state),
        preset: GumballSettings.GetDefaultState,
        update: GumballSettings.UpdateFromState,
        restore: GumballSettings.RestoreDefaults);
    public static readonly AppSettingsFamily SelectionFilter = Of<SelectionFilterSettingsState>(
        key: "selection-filter",
        current: SelectionFilterSettings.GetCurrentState,
        lift: static state => new AppState.SelectionFilterCase(Value: state),
        preset: SelectionFilterSettings.GetDefaultState,
        update: SelectionFilterSettings.UpdateFromState,
        restore: SelectionFilterSettings.RestoreDefaults);
    public static readonly AppSettingsFamily ChooseOneObject = Of<ChooseOneObjectSettingsState>(
        key: "choose-one-object",
        current: ChooseOneObjectSettings.GetCurrentState,
        lift: static state => new AppState.ChooseOneObjectCase(Value: state),
        preset: ChooseOneObjectSettings.GetDefaultState,
        update: ChooseOneObjectSettings.UpdateFromState,
        restore: ChooseOneObjectSettings.RestoreDefaults);
    public static readonly AppSettingsFamily SoftTransform = Of<SoftTransformState>(
        key: "soft-transform",
        current: static () => new SoftTransformState(
            SoftTransformSettings.Enabled,
            SoftTransformSettings.Radius,
            SoftTransformSettings.Shape,
            SoftTransformSettings.MeasureDistanceAlong,
            SoftTransformSettings.CvCountU,
            SoftTransformSettings.CvCountV,
            SoftTransformSettings.ShowConstraintWidgets,
            SoftTransformSettings.FalloffColor),
        lift: static state => new AppState.SoftTransformCase(Value: state),
        update: static state => {
            SoftTransformSettings.Enabled = state.Enabled;
            SoftTransformSettings.Radius = state.Radius;
            SoftTransformSettings.Shape = state.Shape;
            SoftTransformSettings.MeasureDistanceAlong = state.MeasureDistanceAlong;
            SoftTransformSettings.CvCountU = state.CvCountU;
            SoftTransformSettings.CvCountV = state.CvCountV;
            SoftTransformSettings.ShowConstraintWidgets = state.ShowConstraintWidgets;
            SoftTransformSettings.FalloffColor = state.FalloffColor;
        });
    public static readonly AppSettingsFamily PackageManager = Of<PackageManagerState>(
        key: "package-manager",
        current: static () => PackageManagerState.Create(toSeq(PackageManagerSettings.Sources.Split(
            PackageManagerState.Separator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))),
        lift: static state => new AppState.PackageManagerCase(Value: state),
        update: static state => PackageManagerSettings.Sources =
            string.Join(PackageManagerState.Separator, state.Sources));
    public static readonly AppSettingsFamily Curvature = Of<CurvatureAnalysisSettingsState>(
        key: "curvature",
        current: CurvatureAnalysisSettings.GetCurrentState,
        lift: static state => new AppState.CurvatureCase(Value: state),
        preset: CurvatureAnalysisSettings.GetDefaultState,
        update: CurvatureAnalysisSettings.UpdateFromState,
        restore: CurvatureAnalysisSettings.RestoreDefaults);
    public static readonly AppSettingsFamily CurvatureGraph = Of<CurvatureGraphSettingsState>(
        key: "curvature-graph",
        current: CurvatureGraphSettings.GetCurrentState,
        lift: static state => new AppState.CurvatureGraphCase(Value: state),
        preset: CurvatureGraphSettings.GetDefaultState,
        update: CurvatureGraphSettings.UpdateFromState,
        restore: CurvatureGraphSettings.RestoreDefaults);
    public static readonly AppSettingsFamily DraftAngle = Of<DraftAngleAnalysisSettingsState>(
        key: "draft-angle",
        current: DraftAngleAnalysisSettings.GetCurrentState,
        lift: static state => new AppState.DraftAngleCase(Value: state),
        preset: DraftAngleAnalysisSettings.GetDefaultState,
        update: DraftAngleAnalysisSettings.UpdateFromState,
        restore: DraftAngleAnalysisSettings.RestoreDefaults);
    public static readonly AppSettingsFamily Edge = Of<EdgeAnalysisSettingsState>(
        key: "edge",
        current: EdgeAnalysisSettings.GetCurrentState,
        lift: static state => new AppState.EdgeCase(Value: state),
        preset: EdgeAnalysisSettings.GetDefaultState,
        update: EdgeAnalysisSettings.UpdateFromState,
        restore: EdgeAnalysisSettings.RestoreDefaults);
    public static readonly AppSettingsFamily End = Of<EndAnalysisSettingsState>(
        key: "end",
        current: EndAnalysisSettings.GetCurrentState,
        lift: static state => new AppState.EndCase(Value: state),
        preset: EndAnalysisSettings.GetDefaultState,
        update: EndAnalysisSettings.UpdateFromState,
        restore: EndAnalysisSettings.RestoreDefaults);
    public static readonly AppSettingsFamily Direction = Of<DirectionAnalysisSettingsState>(
        key: "direction",
        current: DirectionAnalysisSettings.GetCurrentState,
        lift: static state => new AppState.DirectionCase(Value: state),
        preset: DirectionAnalysisSettings.GetDefaultState,
        update: DirectionAnalysisSettings.UpdateFromState,
        restore: DirectionAnalysisSettings.RestoreDefaults);
    public static readonly AppSettingsFamily Emap = Of<EmapAnalysisSettingsState>(
        key: "emap",
        current: EmapAnalysisSettings.GetCurrentState,
        lift: static state => new AppState.EmapCase(Value: state),
        preset: EmapAnalysisSettings.GetDefaultState,
        update: EmapAnalysisSettings.UpdateFromState,
        restore: EmapAnalysisSettings.RestoreDefaults);
    public static readonly AppSettingsFamily Zebra = Of<ZebraAnalysisSettingsState>(
        key: "zebra",
        current: ZebraAnalysisSettings.GetCurrentState,
        lift: static state => new AppState.ZebraCase(Value: state),
        preset: ZebraAnalysisSettings.GetDefaultState,
        update: ZebraAnalysisSettings.UpdateFromState,
        restore: ZebraAnalysisSettings.RestoreDefaults);
    public static readonly AppSettingsFamily Thickness = Of<ThicknessAnalysisSettingsState>(
        key: "thickness",
        current: ThicknessAnalysisSettings.GetCurrentState,
        lift: static state => new AppState.ThicknessCase(Value: state),
        preset: ThicknessAnalysisSettings.GetDefaultState,
        update: ThicknessAnalysisSettings.UpdateFromState,
        restore: ThicknessAnalysisSettings.RestoreDefaults);

    public CapabilitySet<FamilyVerb> Verbs { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<AppState> Capture();

    [UseDelegateFromConstructor]
    internal partial Fin<AppState> Fallback();

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Apply(AppState state);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Reset();

    private static AppSettingsFamily Of<TState>(
        string key,
        Func<TState> current,
        Func<TState, AppState> lift,
        Func<TState>? preset = null,
        Action<TState>? update = null,
        Action? restore = null) where TState : class =>
        new(verbs: CapabilitySet<FamilyVerb>.Of(Seq(
                    (Held: true, Verb: FamilyVerb.Capture),
                    (Held: preset is not null, Verb: FamilyVerb.Preset),
                    (Held: update is not null, Verb: FamilyVerb.Apply),
                    (Held: restore is not null, Verb: FamilyVerb.Reset))
                .Filter(static row => row.Held)
                .Map(static row => row.Verb)
                .ToArray()),
            capture: op => Try.lift(() => lift(arg: current())).Run(),
            fallback: preset is null
                ? op => Fin.Fail<AppState>(error: new KernelFault.Unsupported(
                    InputType: typeof(AppSettingsFamily), OutputType: typeof(AppState)))
                : op => Try.lift(() => lift(arg: preset())).Run(),
            apply: update is null
                ? static (_, op) => Fin.Fail<Unit>(error: new KernelFault.Unsupported(
                    InputType: typeof(AppState), OutputType: typeof(Unit)))
                : (state, op) => Try.lift(() => {
                    update(obj: (TState)state.Payload);
                    return Fin.Succ(value: unit);
                }).Run().Bind(static inner => inner),
            reset: restore is null
                ? op => Fin.Fail<Unit>(error: new KernelFault.Unsupported(
                    InputType: typeof(AppSettingsFamily), OutputType: typeof(Unit)))
                : op => Try.lift(() => {
                    restore();
                    return Fin.Succ(value: unit);
                }).Run().Bind(static inner => inner));
}

[SmartEnum<bool>]
public sealed partial class AppTheme {
    public static readonly AppTheme Light = new(
        key: false,
        adopt: AppearanceSettings.SetToLightMode,
        preset: static () => AppearanceSettings.GetDefaultState(darkMode: false),
        seated: AppearanceSettings.UsingDefaultLightModeColors);
    public static readonly AppTheme Dark = new(
        key: true,
        adopt: AppearanceSettings.SetToDarkMode,
        preset: static () => AppearanceSettings.GetDefaultState(darkMode: true),
        seated: AppearanceSettings.UsingDefaultDarkModeColors);

    [UseDelegateFromConstructor]
    internal partial bool Adopt();

    [UseDelegateFromConstructor]
    internal partial AppearanceSettingsState Preset();

    [UseDelegateFromConstructor]
    internal partial bool Seated();
}
```

## [03]-[REQUEST_ALGEBRA]

- Owner: `AppOperation` — every application-settings verb behind one closed family; `AliasEdit`/`ShortcutEdit`/`RepeatEdit`/`PathEdit` — the nested registry edits; `SwatchSlot` — the keyed UI-color slot with its own read, write, and preset columns; `GeneralConduct` — `General`'s per-knob policies beside the family's whole-state quartet.
- Entry: a new alias, shortcut, path, or repeat verb lands as one case under an existing arm; a new UI-color family lands as one `SwatchSlot` case, and the swatch operation is unchanged.
- Law: a swatch is ONE operation case over a keyed slot. The slot owns its host read, write, and preset, so paint and widget are two rows of one concern rather than two operation cases with five delegate parameters threaded from the interpreter.
- Law: the themed preset is a PAINT capability. The host publishes `DefaultPaintColor(slot, darkMode)` but only `DefaultWidgetColor(slot)`, so a themed ask on a widget slot refuses through `Unsupported` naming the slot rather than answering a theme-blind default as though the theme had been honoured.
- Law: `Mutates` names each case's own custody side, so a write can never enter the seat gate by omission; a swatch case with no value and a path case with no roster read the host without touching it, so both derive their side from the payload rather than the case name.
- Law: instant is measured on the LIVE roster alone — the host default roster carries no instant flag, so the preset path answers `None` instead of publishing an unmeasured false, and the write boundary lowers an absent flag to the host's own non-instant default.
- Law: no `Changed` column on `AppMutation` — the host `*State` classes carry reference equality and every capture mints a FRESH instance, so a computed change flag publishes a constant; a consumer with family knowledge projects change from the prior/current pair itself.
- Law: foreign `PaintColor`, `WidgetColor`, `KeyboardKey`, `ModifierKey`, `MouseSelectMode`, and `MiddleMouseMode` ordinals stop at these case payloads; `MacroText` and `AliasName` admit registry text once, and folder paths reuse the Document `DocumentPath` owner.
- Growth: a new registry verb is one case on the owning edit union; a new general knob is one `GeneralConduct` case with its own two-row policy.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<TKey>]`, `[Union]`, `[ValueObject<T>]`, `[ValidationError]`, `IDisallowDefaultValue`); LanguageExt.Core (`Fin`, `Option`, `Seq`); kernel `Domain/results` ; `Document/session` (`DocumentPath`), `Persistence/presets` (`PersistenceFault`); RhinoCommon application settings (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-appsettings.md` — `GetPaintColor`/`SetPaintColor`/`DefaultPaintColor`, `GetWidgetColor`/`SetWidgetColor`/`DefaultWidgetColor`, `GeneralSettings.UseExtrusions`/`SplitCreasedSurfaces`, `CommandAlias`, `KeyboardShortcut`), RhinoCommon UI (`api-rhino-ui.md` — `KeyboardKey`, `ModifierKey`), RhinoCommon geometry (`api-rhinocommon-geometry.md` — `Mesh`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Drawing;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.ApplicationSettings;
using Rhino.Geometry;
using Rhino.UI;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError]
public readonly partial struct AliasName : IDisallowDefaultValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        validationError = string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace)
            ? new ValidationError(string.Join(" | ", new object?[] { "Alias name is blank or carries interior whitespace." }))
            : null;
    }
}

[ValueObject<string>]
[ValidationError]
public readonly partial struct MacroText : IDisallowDefaultValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        validationError = string.IsNullOrWhiteSpace(value)
            ? new ValidationError(string.Join(" | ", new object?[] { "Command macro is blank." }))
            : null;
    }
}

[SmartEnum<bool>]
public sealed partial class RegistryMerge {
    public static readonly RegistryMerge Extend = new(key: false);
    public static readonly RegistryMerge Replace = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class ExtrusionDefault {
    public static readonly ExtrusionDefault PolySurface = new(key: false);
    public static readonly ExtrusionDefault Extrusion = new(key: true);
}

[SmartEnum<bool>]
public sealed partial class CreasePolicy {
    public static readonly CreasePolicy Keep = new(key: false);
    public static readonly CreasePolicy Split = new(key: true);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AliasBinding(AliasName Name, MacroText Macro, Option<bool> Instant);

public sealed record ShortcutBinding(KeyboardKey Key, ModifierKey Modifier, MacroText Macro);

public sealed record RepeatRoster(bool Enabled, Seq<string> CommandNames);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AliasEdit {
    private AliasEdit() { }

    public sealed record RosterCase : AliasEdit;
    public sealed record PresetCase : AliasEdit;
    public sealed record ProbeCase(AliasName Name) : AliasEdit;
    public sealed record PutCase(AliasBinding Binding) : AliasEdit;
    public sealed record DeleteCase(AliasName Name) : AliasEdit;
    public sealed record MergeCase(Seq<AliasBinding> Bindings, RegistryMerge Merge) : AliasEdit;

    internal bool Mutates => Switch<bool>(
        rosterCase: static _ => false,
        presetCase: static _ => false,
        probeCase: static _ => false,
        putCase: static _ => true,
        deleteCase: static _ => true,
        mergeCase: static _ => true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShortcutEdit {
    private ShortcutEdit() { }

    public sealed record RosterCase : ShortcutEdit;
    public sealed record PresetCase : ShortcutEdit;
    public sealed record AssignCase(ShortcutBinding Binding) : ShortcutEdit;
    public sealed record MergeCase(Seq<ShortcutBinding> Bindings, RegistryMerge Merge) : ShortcutEdit;

    internal bool Mutates => Switch<bool>(
        rosterCase: static _ => false,
        presetCase: static _ => false,
        assignCase: static _ => true,
        mergeCase: static _ => true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RepeatEdit {
    private RepeatEdit() { }

    public sealed record RosterCase : RepeatEdit;
    public sealed record ReplaceCase(Seq<string> CommandNames) : RepeatEdit;

    internal bool Mutates => Switch<bool>(
        rosterCase: static _ => false,
        replaceCase: static _ => true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PathEdit {
    private PathEdit() { }

    public sealed record RosterCase : PathEdit;
    public sealed record AddCase(DocumentPath Folder, int IndexAt) : PathEdit;
    public sealed record RemoveCase(DocumentPath Folder) : PathEdit;
    public sealed record FindCase(string FileName) : PathEdit;
    public sealed record AutosaveCase(Option<Seq<string>> Commands) : PathEdit;
    public sealed record RecentCase : PathEdit;
    public sealed record DataFolderCase(bool CurrentUser) : PathEdit;
    public sealed record TemplateFolderCase(int LanguageId) : PathEdit;

    internal bool Mutates => Switch<bool>(
        rosterCase: static _ => false,
        addCase: static _ => true,
        removeCase: static _ => true,
        findCase: static _ => false,
        autosaveCase: static autosave => autosave.Commands.IsSome,
        recentCase: static _ => false,
        dataFolderCase: static _ => false,
        templateFolderCase: static _ => false);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeneralConduct {
    private GeneralConduct() { }

    public sealed record MouseSelectCase(MouseSelectMode Mode) : GeneralConduct;
    public sealed record MiddleMouseCase(MiddleMouseMode Mode) : GeneralConduct;
    public sealed record ExtrusionCase(ExtrusionDefault Mode) : GeneralConduct;
    public sealed record CreaseCase(CreasePolicy Mode) : GeneralConduct;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SwatchSlot {
    private SwatchSlot() { }

    public sealed record PaintCase(PaintColor Slot) : SwatchSlot;
    public sealed record WidgetCase(WidgetColor Slot) : SwatchSlot;

    internal bool Defined => Switch<bool>(
        paintCase: static row => System.Enum.IsDefined(row.Slot),
        widgetCase: static row => System.Enum.IsDefined(row.Slot));

    internal Color Read() => Switch<Color>(
        paintCase: static row => AppearanceSettings.GetPaintColor(whichColor: row.Slot),
        widgetCase: static row => AppearanceSettings.GetWidgetColor(whichColor: row.Slot));

    internal Unit Write(Color value) => Switch<Color, Unit>(
        state: value,
        paintCase: static (color, row) => HostEdge.Side(() => AppearanceSettings.SetPaintColor(whichColor: row.Slot, c: color)),
        widgetCase: static (color, row) => HostEdge.Side(() => AppearanceSettings.SetWidgetColor(whichColor: row.Slot, c: color)));

    internal Fin<Color> Preset(Option<AppTheme> theme) => Switch<(Option<AppTheme> Theme), Fin<Color>>(
        state: (theme),
        paintCase: static (s, row) => Fin.Succ(value: s.Theme.Match(
            Some: mode => AppearanceSettings.DefaultPaintColor(whichColor: row.Slot, darkMode: mode.Key),
            None: () => AppearanceSettings.DefaultPaintColor(whichColor: row.Slot))),
        widgetCase: static (s, row) => s.Theme.IsSome
            ? Fin.Fail<Color>(error: new KernelFault.Unsupported(InputType: typeof(AppTheme), OutputType: typeof(WidgetColor)))
            : Fin.Succ(value: AppearanceSettings.DefaultWidgetColor(whichColor: row.Slot)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AppOperation {
    private AppOperation() { }

    public sealed record CaptureCase(AppSettingsFamily Family) : AppOperation;
    public sealed record FallbackCase(AppSettingsFamily Family, Option<AppTheme> Theme) : AppOperation;
    public sealed record ApplyCase(AppState State) : AppOperation;
    public sealed record ResetCase(AppSettingsFamily Family) : AppOperation;
    public sealed record ThemeCase(AppTheme Theme) : AppOperation;
    public sealed record ThemeProbeCase(AppTheme Theme) : AppOperation;
    public sealed record SwatchCase(SwatchSlot Slot, Option<Color> Value, Option<AppTheme> Theme) : AppOperation;
    public sealed record AliasCase(AliasEdit Edit) : AppOperation;
    public sealed record ShortcutCase(ShortcutEdit Edit) : AppOperation;
    public sealed record RepeatCase(RepeatEdit Edit) : AppOperation;
    public sealed record PathCase(PathEdit Edit) : AppOperation;
    public sealed record ConductCase(GeneralConduct Conduct) : AppOperation;
    public sealed record WindowPositionCase : AppOperation;
    public sealed record AutoRangeCase(CurvatureAnalysisSettingsState Seed, Seq<Mesh> Meshes) : AppOperation;

    internal bool Mutates => Switch<bool>(
        captureCase: static _ => false,
        fallbackCase: static _ => false,
        applyCase: static _ => true,
        resetCase: static _ => true,
        themeCase: static _ => true,
        themeProbeCase: static _ => false,
        swatchCase: static swatch => swatch.Value.IsSome,
        aliasCase: static alias => alias.Edit.Mutates,
        shortcutCase: static shortcut => shortcut.Edit.Mutates,
        repeatCase: static repeat => repeat.Edit.Mutates,
        pathCase: static path => path.Edit.Mutates,
        conductCase: static _ => true,
        windowPositionCase: static _ => false,
        autoRangeCase: static _ => false);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AppObservation {
    private AppObservation() { }

    public sealed record ObservedCase(AppState State) : AppObservation;
    public sealed record FaultedCase(AppSettingsFamily Family, Error Fault) : AppObservation;
}

public sealed record AppMutation(AppSettingsFamily Family, AppObservation Prior, AppObservation Current);

public sealed record SwatchMutation(SwatchSlot Slot, Color Prior, Color Current, Color Preset);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AppAnswer {
    private AppAnswer() { }

    public sealed record StateCase(AppState State) : AppAnswer;
    public sealed record MutationCase(AppMutation Mutation) : AppAnswer;
    public sealed record SwatchCase(SwatchMutation Mutation) : AppAnswer;
    public sealed record ThemeCase(AppTheme Theme, bool Seated) : AppAnswer;
    public sealed record AliasesCase(Seq<AliasBinding> Bindings) : AppAnswer;
    public sealed record ShortcutsCase(Seq<ShortcutBinding> Bindings) : AppAnswer;
    public sealed record MacroCase(Option<MacroText> Macro) : AppAnswer;
    public sealed record RosterCase(Seq<string> Names) : AppAnswer;
    public sealed record RepeatCase(RepeatRoster Roster) : AppAnswer;
    public sealed record ResolvedCase(Option<DocumentPath> Path) : AppAnswer;
    public sealed record BoundsCase(Rectangle Bounds) : AppAnswer;
}
```

## [04]-[INTERPRETER]

- Owner: `AppSettings` — the one application-settings entry: `Mount` seats the process writer, `Commit` admits the complete nested operation and dispatches it.
- Entry: application settings are process-global, so no `DocumentSession`, no `UndoBracket`, and no host undo record participate; every raw host enum crosses the admission fold before static access, and every mutation captures prior and current state around the host write so `AppMutation` carries real observation.
- Law: mutation custody is an enforced app-root SEAT, not a convention. The host statics are last-writer-wins PROCESS state, so `Mount` seats one writer first-mount-wins on the kernel transition, every mutating operation presents it, and a second composition's write refuses typed instead of interleaving. Reads never touch the seat — capture, fallback, and every roster read answer for any composition, mounted or not.
- Law: the seat is keyed on `PluginKey`, the branch's typed plug-in identity, so the process's single-writer fact is stated in the same vocabulary every other plug-in-scoped owner uses and a raw `Guid` never addresses it.
- Law: a family verb the host does not publish refuses at ADMISSION off the row's `Verbs` set, before the writer seat and before any host static, so an unsupported ask never reaches a mutation path and never spends a capture.
- Law: state mutations follow capture → write → capture, so `AppMutation` carries real prior/current evidence on every family; a refused capture lands as `FaultedCase` beside the write rather than as a structural absence.
- Law: the admitted flag mask is a per-type CONSTANT resolved at type init, and it is folded through the enum's own underlying width — a signed row folded through `Convert.ToUInt64` throws on any negative value, which is a static-initializer fault that poisons the type for the process rather than a refusal on the carrier.
- Law: registry mutations return the landed roster in the same answer, and the repeat roster is read ONCE per answer through one local — reading the host's enabled flag and command names twice inside one expression publishes two observations of a racing process global as though they were one.
- Law: initial window placement returns admitted bounds without exposing the host out-parameter, and the auto-range solve keeps its `ref` state a local inside the catch frame.
- Boundary: `SettingsRoot.ApplicationCase` (settings.md) owns the raw `PersistentSettings.RhinoAppSettings` node tree these families persist through; this page never writes a node, and settings.md never reaches a typed `Rhino.ApplicationSettings` owner. `HistorySettings` stays with Document undo governance, `ViewSettings.DefinedViewSet*` restore-scope flags and analysis states feed display-mode attachment, and `PlugIn.GetPluginSettings` custody stays with the plug-in root.
- Growth: a new operation is one case, one `Mutates` arm, and one dispatch arm; the seat and the mutation fold are untouched.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` with the generated total `Switch`, `[SmartEnum<TKey>]`); LanguageExt.Core (`Fin`, `Option`, `Seq`, `Atom`, `Traverse`, `Validation`); kernel `Domain/results` (`Try.lift`, `Admit.Need`, `Admit.Confirm`, `HostEdge.Side`, `FactoryBridge.Accept`, `Acceptance.Text`, `Cell.Seat`, `Cell.Step`, `Transition`), `Domain/validation` (`CapabilitySet`); `Document/events` (`PluginKey`), `Document/lifetime` (`Subscription`), `Document/session` (`DocumentPath`); RhinoCommon application settings (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-appsettings.md` — `CommandAliasList` roster, `ShortcutKeySettings` roster with `IsAcceptableKeyCombo`, `NeverRepeatList`, `FileSettings` path roster, `AppearanceSettings.InitialMainWindowPosition`, `CurvatureAnalysisSettings.CalculateCurvatureAutoRange`, `GeneralSettings.MouseSelectMode`/`MiddleMouseMode`/`UseExtrusions`/`SplitCreasedSurfaces`), RhinoCommon UI (`api-rhino-ui.md` — `KeyboardKey`, `ModifierKey`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Drawing;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.ApplicationSettings;
using Rhino.UI;

namespace Rasm.Rhino.Persistence;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AppSettings {
    private static readonly Atom<Option<PluginKey>> Seat = Atom(Option<PluginKey>.None);

    public static Fin<Subscription> Mount(PluginKey writer) {
        return from admitted in writer.Admit()
               from _seated in Cell.Seat(cell: Seat, mint: () => writer).Switch(
                   committed: static _ => Fin.Succ(unit),
                   ceded: static (_) => Fin.Fail<Unit>(error: new KernelFault.InvalidContext()),
                   refused: static declined => Fin.Fail<Unit>(error: declined.Cause),
                   contended: static (_) => Fin.Fail<Unit>(error: new KernelFault.InvalidResult()))
               select Subscription.Of(detach: () => ignore(Cell.Step(
                   cell: Seat,
                   step: held => held.Exists(live => live == writer) ? Some(Option<PluginKey>.None) : None,
                   declined: new KernelFault.InvalidContext())));
    }

    public static Fin<AppAnswer> Commit(AppOperation operation, Option<PluginKey> writer = default) {
        return Admit.Need(operation)
            .Bind(active => Admit(active))
            .Bind(active => Seated(active, writer))
            .Bind(active => active.Switch< Fin<AppAnswer>>(captureCase: static (op, capture) => Error.New(op: op.Message)
                .Map(static state => (AppAnswer)new AppAnswer.StateCase(State: state)),
            fallbackCase: static (fallback) => fallback.Theme.Match(
                Some: theme => Try.lift(() => (AppAnswer)new AppAnswer.StateCase(
                    State: new AppState.AppearanceCase(Value: theme.Preset()))).Run(),
                None: () => fallback.Family.Fallback()
                    .Map(static state => (AppAnswer)new AppAnswer.StateCase(State: state))),
            applyCase: static (apply) => Mutated(
                family: apply.State.Family,
                write: () => apply.State.Family.Apply(state: apply.State)),
            resetCase: static (reset) => Mutated(
                family: reset.Family,
                write: () => reset.Family.Reset()),
            themeCase: static (theme) => Mutated(
                family: AppSettingsFamily.Appearance,
                write: () => Try.lift(() => Admit.Confirm(success: theme.Theme.Adopt())).Run().Bind(static inner => inner)),
            themeProbeCase: static (op, probe) => Try.lift(() => (AppAnswer)new AppAnswer.ThemeCase(
                Theme: probe.Theme,
                Seated: probe.Theme.Seated())).Run(),
            swatchCase: static (op, swatch) => Try.lift(() => {
                Color prior = swatch.Slot.Read();
                swatch.Value.IfSome(swatch.Slot.Write);
                return swatch.Slot.Preset(swatch.Theme).Map(preset => (AppAnswer)new AppAnswer.SwatchCase(
                    Mutation: new SwatchMutation(
                        Slot: swatch.Slot,
                        Prior: prior,
                        Current: swatch.Slot.Read(),
                        Preset: preset)));
            }).Run().Bind(static inner => inner),
            aliasCase: static (op, alias) => Aliases(edit: alias.Edit),
            shortcutCase: static (op, shortcut) => Shortcuts(edit: shortcut.Edit),
            repeatCase: static (op, repeat) => Repeats(edit: repeat.Edit),
            pathCase: static (op, path) => Paths(edit: path.Edit),
            conductCase: static (conduct) => Mutated(
                family: AppSettingsFamily.General,
                write: () => Try.lift(() => {
                    conduct.Conduct.Switch(
                        mouseSelectCase: static mouse => GeneralSettings.MouseSelectMode = mouse.Mode,
                        middleMouseCase: static middle => GeneralSettings.MiddleMouseMode = middle.Mode,
                        extrusionCase: static extrusion => GeneralSettings.UseExtrusions = extrusion.Mode.Key,
                        creaseCase: static crease => GeneralSettings.SplitCreasedSurfaces = crease.Mode.Key);
                    return Fin.Succ(value: unit);
                }).Run().Bind(static inner => inner)),
            windowPositionCase: static (op, _) => Try.lift(() =>
                AppearanceSettings.InitialMainWindowPosition(out Rectangle bounds)
                    ? Fin.Succ<AppAnswer>(new AppAnswer.BoundsCase(bounds))
                    : Fin.Fail<AppAnswer>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner),
            autoRangeCase: static (op, range) => Try.lift(() => {
                CurvatureAnalysisSettingsState state = range.Seed;
                return Admit.Confirm(success: CurvatureAnalysisSettings.CalculateCurvatureAutoRange(
                        meshes: range.Meshes,
                        settings: ref state))
                    .Map(_ => (AppAnswer)new AppAnswer.StateCase(State: new AppState.CurvatureCase(Value: state)));
            }).Run().Bind(static inner => inner)));
    }

    private static Fin<AppOperation> Seated(AppOperation operation, Option<PluginKey> writer) =>
        operation.Mutates
            ? Seat.Value
                .Filter(seat => writer.Exists(held => held == seat))
                .ToFin(Fail: new KernelFault.InvalidContext())
                .Map(_ => operation)
            : Fin.Succ(operation);

    private static Fin<AppOperation> Admit(AppOperation operation) => operation.Switch< Fin<AppOperation>>(
        captureCase: static (value) => Verb(value.Family, FamilyVerb.Capture).Map(_ => (AppOperation)value),
        fallbackCase: static (value) => value.Theme.Match(
            Some: _ => guard(value.Family == AppSettingsFamily.Appearance, new KernelFault.InvalidInput())
                .ToFin()
                .Map(_ => (AppOperation)value),
            None: () => Verb(value.Family, FamilyVerb.Preset).Map(_ => (AppOperation)value)),
        applyCase: static (value) => Admit.Need(value.State)
            .Bind(state => Verb(state.Family, FamilyVerb.Apply))
            .Map(_ => (AppOperation)value),
        resetCase: static (value) => Verb(value.Family, FamilyVerb.Reset).Map(_ => (AppOperation)value),
        themeCase: static (value) => Admit.Need(value.Theme).Map(_ => (AppOperation)value),
        themeProbeCase: static (value) => Admit.Need(value.Theme).Map(_ => (AppOperation)value),
        swatchCase: static (value) => Admit.Need(value.Slot)
            .Bind(slot => guard(slot.Defined, new KernelFault.InvalidInput()).ToFin())
            .Map(_ => (AppOperation)value),
        aliasCase: static (value) => Admit.Need(value.Edit)
            .Bind(edit => Admit(edit))
            .Map(edit => (AppOperation)new AppOperation.AliasCase(edit)),
        shortcutCase: static (value) => Admit.Need(value.Edit)
            .Bind(edit => Admit(edit))
            .Map(edit => (AppOperation)new AppOperation.ShortcutCase(edit)),
        repeatCase: static (value) => Admit.Need(value.Edit)
            .Bind(edit => Admit(edit))
            .Map(edit => (AppOperation)new AppOperation.RepeatCase(edit)),
        pathCase: static (value) => Admit.Need(value.Edit)
            .Bind(edit => Admit(edit))
            .Map(edit => (AppOperation)new AppOperation.PathCase(edit)),
        conductCase: static (value) => Admit.Need(value.Conduct)
            .Bind(conduct => Admit(conduct))
            .Map(conduct => (AppOperation)new AppOperation.ConductCase(conduct)),
        windowPositionCase: static _ => Fin.Succ<AppOperation>(new AppOperation.WindowPositionCase()),
        autoRangeCase: static (value) => (
                Admit.Need(value.Seed).ToValidation(),
                guard(!value.Meshes.IsEmpty, new KernelFault.InvalidInput()).ToFin().ToValidation(),
                value.Meshes.Traverse(mesh => Admit.Need(mesh).ToValidation()).As())
            .Apply(static (seed, meshes) => (AppOperation)new AppOperation.AutoRangeCase(seed, meshes))
            .As()
            .ToFin());

    private static Fin<AppSettingsFamily> Verb(AppSettingsFamily family, FamilyVerb verb) =>
        Admit.Need(family).Bind(row => row.Verbs.Admits(verb)
            ? Fin.Succ(value: row)
            : Fin.Fail<AppSettingsFamily>(error: new KernelFault.Unsupported(
                InputType: typeof(AppSettingsFamily), OutputType: typeof(FamilyVerb))));

    private static Fin<AliasEdit> Admit(AliasEdit edit) => edit.Switch< Fin<AliasEdit>>(
        rosterCase: static _ => Fin.Succ<AliasEdit>(new AliasEdit.RosterCase()),
        presetCase: static _ => Fin.Succ<AliasEdit>(new AliasEdit.PresetCase()),
        probeCase: static (value) => FactoryBridge.Accept<AliasName>(value.Name.Value)
            .Map(name => (AliasEdit)new AliasEdit.ProbeCase(name)),
        putCase: static (value) => Admit(value.Binding)
            .Map(binding => (AliasEdit)new AliasEdit.PutCase(binding)),
        deleteCase: static (value) => FactoryBridge.Accept<AliasName>(value.Name.Value)
            .Map(name => (AliasEdit)new AliasEdit.DeleteCase(name)),
        mergeCase: static (value) => Admit.Need(value.Merge)
            .Bind(merge => value.Bindings
                .Map(binding => Admit(binding).ToValidation())
                .Traverse(static binding => binding)
                .As()
                .ToFin()
                .Map(bindings => (AliasEdit)new AliasEdit.MergeCase(bindings, merge))));

    private static Fin<AliasBinding> Admit(AliasBinding? binding) =>
        from present in Admit.Need(binding)
        from admitted in (
                FactoryBridge.Accept<AliasName>(present.Name.Value).ToValidation(),
                FactoryBridge.Accept<MacroText>(present.Macro.Value).ToValidation())
            .Apply((name, macro) => new AliasBinding(name, macro, present.Instant))
            .As()
            .ToFin()
        select admitted;

    private static Fin<ShortcutEdit> Admit(ShortcutEdit edit) => edit.Switch< Fin<ShortcutEdit>>(
        rosterCase: static _ => Fin.Succ<ShortcutEdit>(new ShortcutEdit.RosterCase()),
        presetCase: static _ => Fin.Succ<ShortcutEdit>(new ShortcutEdit.PresetCase()),
        assignCase: static (value) => Admit(value.Binding)
            .Map(binding => (ShortcutEdit)new ShortcutEdit.AssignCase(binding)),
        mergeCase: static (value) => Admit.Need(value.Merge)
            .Bind(merge => value.Bindings
                .Map(binding => Admit(binding).ToValidation())
                .Traverse(static binding => binding)
                .As()
                .ToFin()
                .Map(bindings => (ShortcutEdit)new ShortcutEdit.MergeCase(bindings, merge))));

    private static Fin<ShortcutBinding> Admit(ShortcutBinding? binding) =>
        from present in Admit.Need(binding)
        from admitted in Admit(present.Modifier, present.Macro.Value)
        select admitted;

    private static Fin<ShortcutBinding> Admit(KeyboardKey key, ModifierKey modifier, string? macro) =>
        from _key in guard(
            Defined()
            && FlagsDefined(modifier)
            && ShortcutKeySettings.IsAcceptableKeyCombo(modifier: modifier),
            new KernelFault.InvalidInput()).ToFin()
        from admittedMacro in FactoryBridge.Accept<MacroText>(macro)
        select new ShortcutBinding(modifier, admittedMacro);

    private static Fin<RepeatEdit> Admit(RepeatEdit edit) => edit.Switch< Fin<RepeatEdit>>(
        rosterCase: static _ => Fin.Succ<RepeatEdit>(new RepeatEdit.RosterCase()),
        replaceCase: static (value) => value.CommandNames
            .Traverse(name => Acceptance.Text(value: name).ToValidation())
            .As()
            .ToFin()
            .Map(names => (RepeatEdit)new RepeatEdit.ReplaceCase(names)));

    private static Fin<PathEdit> Admit(PathEdit edit) => edit.Switch< Fin<PathEdit>>(
        rosterCase: static _ => Fin.Succ<PathEdit>(new PathEdit.RosterCase()),
        addCase: static (value) => (
                DocumentPath.Of(value.Folder.Value).ToValidation(),
                guard(value.IndexAt >= -1, new KernelFault.InvalidInput()).ToFin().ToValidation())
            .Apply((folder) => (PathEdit)new PathEdit.AddCase(folder, value.IndexAt))
            .As()
            .ToFin(),
        removeCase: static (value) => DocumentPath.Of(value.Folder.Value)
            .Map(folder => (PathEdit)new PathEdit.RemoveCase(folder)),
        findCase: static (value) => Acceptance.Text(value.FileName)
            .Map(name => (PathEdit)new PathEdit.FindCase(name)),
        autosaveCase: static (value) => value.Commands.Match(
            Some: commands => commands
                .Traverse(name => Acceptance.Text(value: name).ToValidation())
                .As()
                .ToFin()
                .Map(names => (PathEdit)new PathEdit.AutosaveCase(Some(names))),
            None: static () => Fin.Succ<PathEdit>(new PathEdit.AutosaveCase(None))),
        recentCase: static _ => Fin.Succ<PathEdit>(new PathEdit.RecentCase()),
        dataFolderCase: static value => Fin.Succ<PathEdit>(new PathEdit.DataFolderCase(value.CurrentUser)),
        templateFolderCase: static value => Fin.Succ<PathEdit>(new PathEdit.TemplateFolderCase(value.LanguageId)));

    private static Fin<GeneralConduct> Admit(GeneralConduct conduct) => conduct.Switch< Fin<GeneralConduct>>(
        mouseSelectCase: static (value) => Defined(value.Mode)
            ? Fin.Succ<GeneralConduct>(value)
            : Fin.Fail<GeneralConduct>(new KernelFault.InvalidInput()),
        middleMouseCase: static (value) => Defined(value.Mode)
            ? Fin.Succ<GeneralConduct>(value)
            : Fin.Fail<GeneralConduct>(new KernelFault.InvalidInput()),
        extrusionCase: static (value) => Admit.Need(value.Mode).Map(_ => (GeneralConduct)value),
        creaseCase: static (value) => Admit.Need(value.Mode).Map(_ => (GeneralConduct)value));

    private static bool Defined<T>(T value) where T : struct, System.Enum => System.Enum.IsDefined(value);

    private static class FlagMask<T> where T : struct, System.Enum {
        internal static readonly ulong Admitted = System.Enum.GetValues<T>().Aggregate(0UL, static (mask, item) => mask | Bits(item));
    }

    private static ulong Bits<T>(T value) where T : struct, System.Enum =>
        System.Type.GetTypeCode(typeof(T)) is TypeCode.UInt64
            ? Convert.ToUInt64(value)
            : unchecked((ulong)Convert.ToInt64(value));

    private static bool FlagsDefined<T>(T value) where T : struct, System.Enum =>
        (Bits(value) & ~FlagMask<T>.Admitted) == 0UL;

    private static Fin<AppAnswer> Mutated(AppSettingsFamily family, Func<Fin<Unit>> write) {
        AppObservation Observe() => Error.New(op: op.Message).Match(
            Succ: state => (AppObservation)new AppObservation.ObservedCase(State: state),
            Fail: error => new AppObservation.FaultedCase(Family: family, Fault: error));
        AppObservation prior = Observe();
        return write().Map(_ => (AppAnswer)new AppAnswer.MutationCase(Mutation: new AppMutation(
            Family: family,
            Prior: prior,
            Current: Observe())));
    }

    private static Fin<AppAnswer> Aliases(AliasEdit edit) => edit.Switch< Fin<AppAnswer>>(rosterCase: static (op, _) => AliasBindings(
            source: () => CommandAliasList.GetNames().Select(name =>
                CommandAliasList.FindAlias(alias: name) is { } found
                    ? (Name: name, Macro: found.Macro, Instant: Some(found.Instant))
                    : (Name: name, Macro: CommandAliasList.GetMacro(alias: name), Instant: Option<bool>.None))),
        presetCase: static (_) => AliasBindings(
            source: () => CommandAliasList.GetDefaults().Select(static binding => (
                Name: binding.Key,
                Macro: binding.Value,
                Instant: Option<bool>.None))),
        probeCase: static (op, probe) => Try.lift(() => CommandAliasList.IsAlias(alias: probe.Name.Value)
            ? FactoryBridge.Accept<MacroText>(CommandAliasList.GetMacro(alias: probe.Name.Value))
                .Map(static macro => (AppAnswer)new AppAnswer.MacroCase(Macro: Some(macro)))
            : Fin.Succ(value: (AppAnswer)new AppAnswer.MacroCase(Macro: None))).Run().Bind(static inner => inner),
        putCase: static (op, put) => Try.lift(() => Admit.Confirm(success: CommandAliasList.IsAlias(alias: put.Binding.Name.Value)
                ? CommandAliasList.SetMacro(alias: put.Binding.Name.Value, macro: put.Binding.Macro.Value)
                : CommandAliasList.Add(alias: put.Binding.Name.Value, macro: put.Binding.Macro.Value))
            .Map(_ => (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(CommandAliasList.GetNames())))).Run().Bind(static inner => inner),
        deleteCase: static (op, delete) => Try.lift(() => Admit.Confirm(success: CommandAliasList.Delete(alias: delete.Name.Value))
            .Map(_ => (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(CommandAliasList.GetNames())))).Run().Bind(static inner => inner),
        mergeCase: static (op, merge) => Try.lift(() => {
            CommandAliasList.Update(
                aliases: merge.Bindings.Map(static binding => new CommandAlias(
                    alias: binding.Name.Value,
                    macro: binding.Macro.Value,
                    instant: binding.Instant.IfNone(noneValue: false))).AsIterable(),
                replaceAll: merge.Merge.Key);
            return Fin.Succ(value: (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(CommandAliasList.GetNames())));
        }).Run().Bind(static inner => inner));

    private static Fin<AppAnswer> AliasBindings(
        Func<IEnumerable<(string Name, string Macro, Option<bool> Instant)>> source) =>
        Try.lift(() => toSeq(source())
            .Traverse(binding =>
                (from name in FactoryBridge.Accept<AliasName>(binding.Name)
                 from macro in FactoryBridge.Accept<MacroText>(binding.Macro)
                 select new AliasBinding(name, macro, binding.Instant)).ToValidation())
            .As()
            .ToFin()
            .Map(static bindings => (AppAnswer)new AppAnswer.AliasesCase(Bindings: bindings))).Run().Bind(static inner => inner);

    private static Fin<AppAnswer> Shortcuts(ShortcutEdit edit) => edit.Switch< Fin<AppAnswer>>(rosterCase: static (op, _) => Bindings(source: ShortcutKeySettings.GetShortcuts),
        presetCase: static (op, _) => Bindings(source: ShortcutKeySettings.GetDefaults),
        assignCase: static (assign) =>
            from _written in Try.lift(() => ShortcutKeySettings.SetMacro(modifier: assign.Binding.Modifier,
                macro: assign.Binding.Macro.Value)).Run().Bind(static inner => inner)
            from roster in Bindings(source: ShortcutKeySettings.GetShortcuts)
            select roster,
        mergeCase: static (merge) =>
            from _updated in Try.lift(() => ShortcutKeySettings.Update(
                shortcuts: merge.Bindings.Map(static binding => new KeyboardShortcut {
                    Key = binding.Key,
                    Modifier = binding.Modifier,
                    Macro = binding.Macro.Value,
                }).AsIterable(),
                replaceAll: merge.Merge.Key)).Run().Bind(static inner => inner)
            from roster in Bindings(source: ShortcutKeySettings.GetShortcuts)
            select roster);

    private static Fin<AppAnswer> Bindings(Func<KeyboardShortcut[]> source) =>
        Try.lift(() => toSeq(source())
            .Filter(static shortcut => !string.IsNullOrWhiteSpace(value: shortcut.Macro))
            .Traverse(shortcut => Admit(shortcut.Modifier, shortcut.Macro).ToValidation())
            .As()
            .ToFin()
            .Map(static bindings => (AppAnswer)new AppAnswer.ShortcutsCase(Bindings: bindings))).Run().Bind(static inner => inner);

    private static Fin<AppAnswer> Repeats(RepeatEdit edit) => edit.Switch< Fin<AppAnswer>>(rosterCase: static (op, _) => Roster(),
        replaceCase: static (replace) =>
            from _landed in Try.lift(() => Admit.Confirm(
                success: NeverRepeatList.SetList(commandNames: replace.CommandNames.ToArray()) >= 0)).Run().Bind(static inner => inner)
            from roster in Roster()
            select roster);

    private static Fin<AppAnswer> Roster() => Try.lift(() => (AppAnswer)new AppAnswer.RepeatCase(
        Roster: new RepeatRoster(
            Enabled: NeverRepeatList.UseNeverRepeatList,
            CommandNames: toSeq(NeverRepeatList.CommandNames())))).Run();

    private static Fin<AppAnswer> Paths(PathEdit edit) => edit.Switch< Fin<AppAnswer>>(rosterCase: static (op, _) => Try.lift(() => (AppAnswer)new AppAnswer.RosterCase(
            Names: toSeq(FileSettings.GetSearchPaths()))).Run(),
        addCase: static (op, add) => Try.lift(() => Admit.Confirm(
                success: FileSettings.AddSearchPath(folder: add.Folder.Value, index: add.IndexAt) >= 0)
            .Map(_ => (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(FileSettings.GetSearchPaths())))).Run().Bind(static inner => inner),
        removeCase: static (op, remove) => Try.lift(() => Admit.Confirm(success: FileSettings.DeleteSearchPath(folder: remove.Folder.Value))
            .Map(_ => (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(FileSettings.GetSearchPaths())))).Run().Bind(static inner => inner),
        findCase: static (find) =>
            from resolved in Try.lift(() => Optional(FileSettings.FindFile(fileName: find.FileName))
                .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
                .Traverse(value => DocumentPath.Of(value: value))
                .As()).Run().Bind(static inner => inner)
            select (AppAnswer)new AppAnswer.ResolvedCase(Path: resolved),
        autosaveCase: static (autosave) => autosave.Commands.Match(
            Some: commands => Try.lift(() => FileSettings.SetAutoSaveBeforeCommands(commands: commands.ToArray())).Run().Bind(static inner => inner)
                .Map(_ => (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(FileSettings.AutoSaveBeforeCommands()))),
            None: () => Try.lift(() => (AppAnswer)new AppAnswer.RosterCase(
                Names: toSeq(FileSettings.AutoSaveBeforeCommands()))).Run()),
        recentCase: static (op, _) => Try.lift(() => (AppAnswer)new AppAnswer.RosterCase(
            Names: toSeq(FileSettings.RecentlyOpenedFiles()))).Run(),
        dataFolderCase: static (op, data) => Try.lift(() => Optional(FileSettings.GetDataFolder(currentUser: data.CurrentUser))
            .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
            .Traverse(value => DocumentPath.Of(value: value))
            .As()
            .Map(static resolved => (AppAnswer)new AppAnswer.ResolvedCase(Path: resolved))).Run().Bind(static inner => inner),
        templateFolderCase: static (op, template) => Try.lift(() => Optional(
                FileSettings.DefaultTemplateFolderForLanguageID(languageID: template.LanguageId))
            .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
            .Traverse(value => DocumentPath.Of(value: value))
            .As()
            .Map(static resolved => (AppAnswer)new AppAnswer.ResolvedCase(Path: resolved))).Run().Bind(static inner => inner));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
