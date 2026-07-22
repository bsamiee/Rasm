# [APPLICATION_SETTINGS]

`AppSettingsFamily` closes each `Rhino.ApplicationSettings` static owner as one keyed behavior vocabulary whose rows carry capture, default, apply, and reset delegates; `AppState` carries each serializable `*State` snapshot as one typed case, so a preference family round-trips as one immutable value and never as mid-flight mutable statics. `AppSettings.Commit` interprets state round-trips, theme and color policy, registry custody, initial window placement, and analysis solves into detached answers with prior/current observation receipts.

Owner boundary against the settings tree is settled: `SettingsRoot.ApplicationCase` (settings.md) resolves the raw `PersistentSettings.RhinoAppSettings` node tree — a lazy `FromPlugInId(RhinoApp.CurrentRhinoId)` property, not a typed surface — while this page owns the distinct `Rhino.ApplicationSettings` static families whose storage merely rides that tree. A typed preference edit enters here through the owning family's state round-trip; a raw node read or write enters settings.md; neither owner reaches across.

## [01]-[STATE_AND_FAMILY]

`AppState` closes host `*State` payloads; `Family` derives the owning row from the case, so an apply request carries no parallel family parameter. `AppSettingsFamily.Of` binds each stateful owner's supported verbs, and absent host verbs refuse with a typed unsupported fault; `Bare` marks owners without a `*State` type. `General` state remains read-only while `GeneralConduct` owns its writable knobs.

```csharp signature
namespace Rasm.Rhino.Persistence;

using LanguageExt;
using Rasm.Domain;
using Rhino.ApplicationSettings;
using Thinktecture;
using static LanguageExt.Prelude;

[Union]
public abstract partial record AppState
{
    public sealed record AppearanceCase(AppearanceSettingsState Value) : AppState;
    public sealed record ModelAidCase(ModelAidSettingsState Value) : AppState;
    public sealed record FilesCase(FileSettingsState Value) : AppState;
    public sealed record GeneralCase(GeneralSettingsState Value) : AppState;
    public sealed record ViewsCase(ViewSettingsState Value) : AppState;
    public sealed record OpenGlCase(OpenGLSettingsState Value) : AppState;
    public sealed record CursorTooltipCase(CursorTooltipSettingsState Value) : AppState;
    public sealed record SmartTrackCase(SmartTrackSettingsState Value) : AppState;
    public sealed record GumballCase(GumballSettingsState Value) : AppState;
    public sealed record SelectionFilterCase(SelectionFilterSettingsState Value) : AppState;
    public sealed record ChooseOneObjectCase(ChooseOneObjectSettingsState Value) : AppState;
    public sealed record CurvatureCase(CurvatureAnalysisSettingsState Value) : AppState;
    public sealed record CurvatureGraphCase(CurvatureGraphSettingsState Value) : AppState;
    public sealed record DraftAngleCase(DraftAngleAnalysisSettingsState Value) : AppState;
    public sealed record EdgeCase(EdgeAnalysisSettingsState Value) : AppState;
    public sealed record EndCase(EndAnalysisSettingsState Value) : AppState;
    public sealed record DirectionCase(DirectionAnalysisSettingsState Value) : AppState;
    public sealed record EmapCase(EmapAnalysisSettingsState Value) : AppState;
    public sealed record ZebraCase(ZebraAnalysisSettingsState Value) : AppState;
    public sealed record ThicknessCase(ThicknessAnalysisSettingsState Value) : AppState;

    public AppSettingsFamily Family => Switch<AppSettingsFamily>(
        appearanceCase: static _ => AppSettingsFamily.Appearance,
        modelAidCase: static _ => AppSettingsFamily.ModelAid,
        filesCase: static _ => AppSettingsFamily.Files,
        generalCase: static _ => AppSettingsFamily.General,
        viewsCase: static _ => AppSettingsFamily.Views,
        openGlCase: static _ => AppSettingsFamily.OpenGl,
        cursorTooltipCase: static _ => AppSettingsFamily.CursorTooltip,
        smartTrackCase: static _ => AppSettingsFamily.SmartTrack,
        gumballCase: static _ => AppSettingsFamily.Gumball,
        selectionFilterCase: static _ => AppSettingsFamily.SelectionFilter,
        chooseOneObjectCase: static _ => AppSettingsFamily.ChooseOneObject,
        curvatureCase: static _ => AppSettingsFamily.Curvature,
        curvatureGraphCase: static _ => AppSettingsFamily.CurvatureGraph,
        draftAngleCase: static _ => AppSettingsFamily.DraftAngle,
        edgeCase: static _ => AppSettingsFamily.Edge,
        endCase: static _ => AppSettingsFamily.End,
        directionCase: static _ => AppSettingsFamily.Direction,
        emapCase: static _ => AppSettingsFamily.Emap,
        zebraCase: static _ => AppSettingsFamily.Zebra,
        thicknessCase: static _ => AppSettingsFamily.Thickness);
}

[SmartEnum<string>]
public sealed partial class AppSettingsFamily
{
    public static readonly AppSettingsFamily Appearance = Of(
        key: "appearance",
        current: AppearanceSettings.GetCurrentState,
        preset: static () => AppearanceSettings.GetDefaultState(),
        update: AppearanceSettings.UpdateFromState,
        restore: AppearanceSettings.RestoreDefaults,
        lift: static state => new AppState.AppearanceCase(Value: state),
        lower: static state => state is AppState.AppearanceCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily ModelAid = Of(
        key: "model-aid",
        current: ModelAidSettings.GetCurrentState,
        preset: ModelAidSettings.GetDefaultState,
        lift: static state => new AppState.ModelAidCase(Value: state),
        lower: static state => state is AppState.ModelAidCase { Value: not null } typed ? Some(typed.Value) : None,
        update: ModelAidSettings.UpdateFromState);
    public static readonly AppSettingsFamily Files = Of(
        key: "files",
        current: FileSettings.GetCurrentState,
        preset: FileSettings.GetDefaultState,
        lift: static state => new AppState.FilesCase(Value: state),
        lower: static state => state is AppState.FilesCase { Value: not null } typed ? Some(typed.Value) : None,
        update: FileSettings.UpdateFromState);
    public static readonly AppSettingsFamily General = Of(
        key: "general",
        current: GeneralSettings.GetCurrentState,
        preset: GeneralSettings.GetDefaultState,
        lift: static state => new AppState.GeneralCase(Value: state),
        lower: static state => state is AppState.GeneralCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily Views = Of(
        key: "views",
        current: ViewSettings.GetCurrentState,
        preset: ViewSettings.GetDefaultState,
        update: ViewSettings.UpdateFromState,
        restore: ViewSettings.RestoreDefaults,
        lift: static state => new AppState.ViewsCase(Value: state),
        lower: static state => state is AppState.ViewsCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily OpenGl = Of(
        key: "opengl",
        current: OpenGLSettings.GetCurrentState,
        preset: OpenGLSettings.GetDefaultState,
        update: OpenGLSettings.UpdateFromState,
        restore: OpenGLSettings.RestoreDefaults,
        lift: static state => new AppState.OpenGlCase(Value: state),
        lower: static state => state is AppState.OpenGlCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily CursorTooltip = Of(
        key: "cursor-tooltip",
        current: CursorTooltipSettings.GetCurrentState,
        preset: CursorTooltipSettings.GetDefaultState,
        lift: static state => new AppState.CursorTooltipCase(Value: state),
        lower: static state => state is AppState.CursorTooltipCase { Value: not null } typed ? Some(typed.Value) : None,
        update: CursorTooltipSettings.UpdateFromState);
    public static readonly AppSettingsFamily SmartTrack = Of(
        key: "smart-track",
        current: SmartTrackSettings.GetCurrentState,
        preset: SmartTrackSettings.GetDefaultState,
        lift: static state => new AppState.SmartTrackCase(Value: state),
        lower: static state => state is AppState.SmartTrackCase { Value: not null } typed ? Some(typed.Value) : None,
        update: SmartTrackSettings.UpdateFromState);
    public static readonly AppSettingsFamily Gumball = Of(
        key: "gumball",
        current: GumballSettings.GetCurrentState,
        preset: GumballSettings.GetDefaultState,
        update: GumballSettings.UpdateFromState,
        restore: GumballSettings.RestoreDefaults,
        lift: static state => new AppState.GumballCase(Value: state),
        lower: static state => state is AppState.GumballCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily SelectionFilter = Of(
        key: "selection-filter",
        current: SelectionFilterSettings.GetCurrentState,
        preset: SelectionFilterSettings.GetDefaultState,
        update: SelectionFilterSettings.UpdateFromState,
        restore: SelectionFilterSettings.RestoreDefaults,
        lift: static state => new AppState.SelectionFilterCase(Value: state),
        lower: static state => state is AppState.SelectionFilterCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily ChooseOneObject = Of(
        key: "choose-one-object",
        current: ChooseOneObjectSettings.GetCurrentState,
        preset: ChooseOneObjectSettings.GetDefaultState,
        update: ChooseOneObjectSettings.UpdateFromState,
        restore: ChooseOneObjectSettings.RestoreDefaults,
        lift: static state => new AppState.ChooseOneObjectCase(Value: state),
        lower: static state => state is AppState.ChooseOneObjectCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily SoftTransform = Bare(key: "soft-transform");
    public static readonly AppSettingsFamily PackageManager = Bare(key: "package-manager");
    public static readonly AppSettingsFamily Curvature = Of(
        key: "curvature",
        current: CurvatureAnalysisSettings.GetCurrentState,
        preset: CurvatureAnalysisSettings.GetDefaultState,
        update: CurvatureAnalysisSettings.UpdateFromState,
        restore: CurvatureAnalysisSettings.RestoreDefaults,
        lift: static state => new AppState.CurvatureCase(Value: state),
        lower: static state => state is AppState.CurvatureCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily CurvatureGraph = Of(
        key: "curvature-graph",
        current: CurvatureGraphSettings.GetCurrentState,
        preset: CurvatureGraphSettings.GetDefaultState,
        update: CurvatureGraphSettings.UpdateFromState,
        restore: CurvatureGraphSettings.RestoreDefaults,
        lift: static state => new AppState.CurvatureGraphCase(Value: state),
        lower: static state => state is AppState.CurvatureGraphCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily DraftAngle = Of(
        key: "draft-angle",
        current: DraftAngleAnalysisSettings.GetCurrentState,
        preset: DraftAngleAnalysisSettings.GetDefaultState,
        update: DraftAngleAnalysisSettings.UpdateFromState,
        restore: DraftAngleAnalysisSettings.RestoreDefaults,
        lift: static state => new AppState.DraftAngleCase(Value: state),
        lower: static state => state is AppState.DraftAngleCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily Edge = Of(
        key: "edge",
        current: EdgeAnalysisSettings.GetCurrentState,
        preset: EdgeAnalysisSettings.GetDefaultState,
        update: EdgeAnalysisSettings.UpdateFromState,
        restore: EdgeAnalysisSettings.RestoreDefaults,
        lift: static state => new AppState.EdgeCase(Value: state),
        lower: static state => state is AppState.EdgeCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily End = Of(
        key: "end",
        current: EndAnalysisSettings.GetCurrentState,
        preset: EndAnalysisSettings.GetDefaultState,
        lift: static state => new AppState.EndCase(Value: state),
        lower: static state => state is AppState.EndCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily Direction = Of(
        key: "direction",
        current: DirectionAnalysisSettings.GetCurrentState,
        preset: DirectionAnalysisSettings.GetDefaultState,
        lift: static state => new AppState.DirectionCase(Value: state),
        lower: static state => state is AppState.DirectionCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily Emap = Of(
        key: "emap",
        current: EmapAnalysisSettings.GetCurrentState,
        preset: EmapAnalysisSettings.GetDefaultState,
        lift: static state => new AppState.EmapCase(Value: state),
        lower: static state => state is AppState.EmapCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily Zebra = Of(
        key: "zebra",
        current: ZebraAnalysisSettings.GetCurrentState,
        preset: ZebraAnalysisSettings.GetDefaultState,
        update: ZebraAnalysisSettings.UpdateFromState,
        restore: ZebraAnalysisSettings.RestoreDefaults,
        lift: static state => new AppState.ZebraCase(Value: state),
        lower: static state => state is AppState.ZebraCase { Value: not null } typed ? Some(typed.Value) : None);
    public static readonly AppSettingsFamily Thickness = Of(
        key: "thickness",
        current: ThicknessAnalysisSettings.GetCurrentState,
        preset: ThicknessAnalysisSettings.GetDefaultState,
        update: ThicknessAnalysisSettings.UpdateFromState,
        restore: ThicknessAnalysisSettings.RestoreDefaults,
        lift: static state => new AppState.ThicknessCase(Value: state),
        lower: static state => state is AppState.ThicknessCase { Value: not null } typed ? Some(typed.Value) : None);

    [UseDelegateFromConstructor]
    internal partial Fin<AppState> Capture(Op op);

    [UseDelegateFromConstructor]
    internal partial Fin<AppState> Fallback(Op op);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Apply(AppState state, Op op);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Reset(Op op);

    private static AppSettingsFamily Of<TState>(
        string key,
        Func<TState> current,
        Func<TState> preset,
        Func<TState, AppState> lift,
        Func<AppState, Option<TState>> lower,
        Action<TState>? update = null,
        Action? restore = null) where TState : class =>
        new(
            key,
            capture: op => op.Catch(() => Fin.Succ(value: lift(arg: current()))),
            fallback: op => op.Catch(() => Fin.Succ(value: lift(arg: preset()))),
            apply: (state, op) => update is null
                ? Fin.Fail<Unit>(error: op.Unsupported(geometryType: typeof(AppState), outputType: typeof(Unit)))
                : lower(arg: state)
                    .ToFin(Fail: op.InvalidInput())
                    .Bind(typed => op.Catch(() =>
                    {
                        update(obj: typed);
                        return Fin.Succ(value: unit);
                    })),
            reset: op => restore is null
                ? Fin.Fail<Unit>(error: op.Unsupported(geometryType: typeof(AppSettingsFamily), outputType: typeof(Unit)))
                : op.Catch(() =>
                {
                    restore();
                    return Fin.Succ(value: unit);
                }));

    private static AppSettingsFamily Bare(string key) =>
        new(
            key,
            capture: op => Fin.Fail<AppState>(error: op.Unsupported(geometryType: typeof(AppSettingsFamily), outputType: typeof(AppState))),
            fallback: op => Fin.Fail<AppState>(error: op.Unsupported(geometryType: typeof(AppSettingsFamily), outputType: typeof(AppState))),
            apply: (_, op) => Fin.Fail<Unit>(error: op.Unsupported(geometryType: typeof(AppState), outputType: typeof(Unit))),
            reset: op => Fin.Fail<Unit>(error: op.Unsupported(geometryType: typeof(AppSettingsFamily), outputType: typeof(Unit))));
}

[SmartEnum<string>]
public sealed partial class AppTheme
{
    public static readonly AppTheme Dark = new(
        "dark",
        adopt: static () => AppearanceSettings.SetToDarkMode(),
        adopted: static () => AppearanceSettings.UsingDefaultDarkModeColors(),
        preset: static () => AppearanceSettings.GetDefaultState(darkMode: true));
    public static readonly AppTheme Light = new(
        "light",
        adopt: static () => AppearanceSettings.SetToLightMode(),
        adopted: static () => AppearanceSettings.UsingDefaultLightModeColors(),
        preset: static () => AppearanceSettings.GetDefaultState(darkMode: false));

    [UseDelegateFromConstructor]
    internal partial bool Adopt();

    [UseDelegateFromConstructor]
    internal partial bool Adopted();

    [UseDelegateFromConstructor]
    internal partial AppearanceSettingsState Preset();
}
```

## [02]-[REQUEST_ALGEBRA]

`AppOperation` closes every application-settings verb; each registry carries its own nested edit union so a new alias, shortcut, path, or repeat verb lands as one case under an existing arm, and `GeneralConduct` carries the per-knob enum policies that are `General`'s only write path beside its read-only state pair. `MacroText` and `AliasName` admit registry text once; folder paths reuse the Document `DocumentPath` owner. Foreign `PaintColor`, `WidgetColor`, `KeyboardKey`, `ModifierKey`, `MouseSelectMode`, and `MiddleMouseMode` ordinals stop at these case payloads — every answer detaches into typed receipts.

```csharp signature
namespace Rasm.Rhino.Persistence;

using System.Drawing;
using LanguageExt;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.ApplicationSettings;
using Rhino.Geometry;
using Rhino.UI;
using Thinktecture;
using static LanguageExt.Prelude;

[ValueObject<string>]
public readonly partial struct AliasName
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        value = value.Trim();
        validationError = string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace)
            ? new ValidationError(message: "Alias name is blank or carries interior whitespace.")
            : null;
    }
}

[ValueObject<string>]
public readonly partial struct MacroText
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        value = value.Trim();
        validationError = string.IsNullOrWhiteSpace(value)
            ? new ValidationError(message: "Command macro is blank.")
            : null;
    }
}

[SmartEnum<string>]
public sealed partial class RegistryMerge
{
    public static readonly RegistryMerge Extend = new("extend", replaceAll: false);
    public static readonly RegistryMerge Replace = new("replace", replaceAll: true);

    internal bool ReplaceAll { get; }
}

public sealed record AliasBinding(AliasName Name, MacroText Macro, bool Instant);

public sealed record ShortcutBinding(KeyboardKey Key, ModifierKey Modifier, MacroText Macro);

[Union]
public abstract partial record AliasEdit
{
    public sealed record RosterCase : AliasEdit;
    public sealed record PresetCase : AliasEdit;
    public sealed record ProbeCase(AliasName Name) : AliasEdit;
    public sealed record PutCase(AliasBinding Binding) : AliasEdit;
    public sealed record DeleteCase(AliasName Name) : AliasEdit;
    public sealed record MergeCase(Seq<AliasBinding> Bindings, RegistryMerge Merge) : AliasEdit;
}

[Union]
public abstract partial record ShortcutEdit
{
    public sealed record RosterCase : ShortcutEdit;
    public sealed record PresetCase : ShortcutEdit;
    public sealed record AssignCase(ShortcutBinding Binding) : ShortcutEdit;
    public sealed record MergeCase(Seq<ShortcutBinding> Bindings, RegistryMerge Merge) : ShortcutEdit;
}

[Union]
public abstract partial record RepeatEdit
{
    public sealed record RosterCase : RepeatEdit;
    public sealed record ReplaceCase(Seq<string> CommandNames) : RepeatEdit;
}

[Union]
public abstract partial record GeneralConduct
{
    public sealed record MouseSelectCase(MouseSelectMode Mode) : GeneralConduct;
    public sealed record MiddleMouseCase(MiddleMouseMode Mode) : GeneralConduct;
}

public sealed record RepeatRoster(bool Enabled, Seq<string> CommandNames);

[Union]
public abstract partial record PathEdit
{
    public sealed record RosterCase : PathEdit;
    public sealed record AddCase(DocumentPath Folder, int IndexAt) : PathEdit;
    public sealed record RemoveCase(DocumentPath Folder) : PathEdit;
    public sealed record FindCase(string FileName) : PathEdit;
    public sealed record AutosaveCase(Option<Seq<string>> Commands) : PathEdit;
    public sealed record RecentCase : PathEdit;
    public sealed record DataFolderCase(bool CurrentUser) : PathEdit;
    public sealed record TemplateFolderCase(int LanguageId) : PathEdit;
}

[Union]
public abstract partial record AppOperation
{
    public sealed record CaptureCase(AppSettingsFamily Family) : AppOperation;
    public sealed record FallbackCase(AppSettingsFamily Family, Option<AppTheme> Theme) : AppOperation;
    public sealed record ApplyCase(AppState State) : AppOperation;
    public sealed record ResetCase(AppSettingsFamily Family) : AppOperation;
    public sealed record ThemeCase(AppTheme Theme) : AppOperation;
    public sealed record PaintCase(PaintColor Slot, Option<Color> Value) : AppOperation;
    public sealed record WidgetCase(WidgetColor Slot, Option<Color> Value) : AppOperation;
    public sealed record AliasCase(AliasEdit Edit) : AppOperation;
    public sealed record ShortcutCase(ShortcutEdit Edit) : AppOperation;
    public sealed record RepeatCase(RepeatEdit Edit) : AppOperation;
    public sealed record PathCase(PathEdit Edit) : AppOperation;
    public sealed record ConductCase(GeneralConduct Conduct) : AppOperation;
    public sealed record WindowPositionCase : AppOperation;
    public sealed record AutoRangeCase(CurvatureAnalysisSettingsState Seed, Seq<Mesh> Meshes) : AppOperation;
}

[Union]
public abstract partial record AppObservation
{
    public sealed record ObservedCase(AppState State) : AppObservation;
    public sealed record UnobservableCase(AppSettingsFamily Family) : AppObservation;
}

public sealed record AppMutationReceipt(
    AppSettingsFamily Family,
    AppObservation Prior,
    AppObservation Current,
    bool Changed);

public sealed record SwatchReceipt(
    string Slot,
    Color Prior,
    Color Current,
    Color Preset);

[Union]
public abstract partial record AppAnswer
{
    public sealed record StateCase(AppState State) : AppAnswer;
    public sealed record MutationCase(AppMutationReceipt Receipt) : AppAnswer;
    public sealed record SwatchCase(SwatchReceipt Receipt) : AppAnswer;
    public sealed record AliasesCase(Seq<AliasBinding> Bindings) : AppAnswer;
    public sealed record ShortcutsCase(Seq<ShortcutBinding> Bindings) : AppAnswer;
    public sealed record MacroCase(Option<MacroText> Macro) : AppAnswer;
    public sealed record RosterCase(Seq<string> Names) : AppAnswer;
    public sealed record RepeatCase(RepeatRoster Roster) : AppAnswer;
    public sealed record ResolvedCase(Option<DocumentPath> Path) : AppAnswer;
    public sealed record BoundsCase(Rectangle Bounds) : AppAnswer;
}
```

## [03]-[INTERPRETER]

`AppSettings.Commit` admits the complete nested operation before dispatching the family with no document session — application settings are process-global, so no `DocumentSession`, no `UndoBracket`, and no host undo record participate. Every raw host enum crosses the admission fold before static access, and every mutation captures prior and current state around the host write so the receipt carries real observation; the two stateless families report `UnobservableCase` evidence instead of inventing a snapshot. Mutation custody is app-root single-writer — the host statics are last-writer-wins process state (the `Document/events.md` process-global custody census row for this surface), so exactly one app-root composition owns every `AppSettings.Commit` write while plugins and features consume captured state; a second writer beside the root is the collision the census names, never a supported topology.

Host static setters, registry mutators, and the `ref`-state auto-range solve form the platform-forced statement seam; generated dispatch keeps the operation, edit, observation, and answer families exhaustive around it.

```csharp signature
namespace Rasm.Rhino.Persistence;

using System.Drawing;
using LanguageExt;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.ApplicationSettings;
using Rhino.UI;
using Thinktecture;
using static LanguageExt.Prelude;

public static class AppSettings
{
    public static Fin<AppAnswer> Commit(AppOperation operation, Op? key = null)
    {
        Op op = key.OrDefault();
        return Optional(operation).ToFin(Fail: op.InvalidInput())
            .Bind(active => Admit(active, op))
            .Bind(active => active.Switch<Op, Fin<AppAnswer>>(
                op,
            captureCase: static (op, capture) => capture.Family.Capture(op: op)
                .Map(static state => (AppAnswer)new AppAnswer.StateCase(State: state)),
            fallbackCase: static (op, fallback) => fallback.Theme.Match(
                Some: theme => fallback.Family == AppSettingsFamily.Appearance
                    ? op.Catch(() => Fin.Succ(value: (AppAnswer)new AppAnswer.StateCase(
                        State: new AppState.AppearanceCase(Value: theme.Preset()))))
                    : Fin.Fail<AppAnswer>(error: op.InvalidInput()),
                None: () => fallback.Family.Fallback(op: op)
                    .Map(static state => (AppAnswer)new AppAnswer.StateCase(State: state))),
            applyCase: static (op, apply) => Mutated(
                family: apply.State.Family,
                write: () => apply.State.Family.Apply(state: apply.State, op: op),
                op: op),
            resetCase: static (op, reset) => Mutated(
                family: reset.Family,
                write: () => reset.Family.Reset(op: op),
                op: op),
            themeCase: static (op, theme) => Mutated(
                family: AppSettingsFamily.Appearance,
                write: () => op.Catch(() => op.Confirm(success: theme.Theme.Adopt())),
                op: op),
            paintCase: static (op, paint) => Swatch(
                slot: paint.Slot.ToString(),
                read: () => AppearanceSettings.GetPaintColor(whichColor: paint.Slot),
                write: value => AppearanceSettings.SetPaintColor(whichColor: paint.Slot, c: value),
                preset: () => AppearanceSettings.DefaultPaintColor(whichColor: paint.Slot),
                value: paint.Value,
                op: op),
            widgetCase: static (op, widget) => Swatch(
                slot: widget.Slot.ToString(),
                read: () => AppearanceSettings.GetWidgetColor(whichColor: widget.Slot),
                write: value => AppearanceSettings.SetWidgetColor(whichColor: widget.Slot, c: value),
                preset: () => AppearanceSettings.DefaultWidgetColor(whichColor: widget.Slot),
                value: widget.Value,
                op: op),
            aliasCase: static (op, alias) => Aliases(edit: alias.Edit, op: op),
            shortcutCase: static (op, shortcut) => Shortcuts(edit: shortcut.Edit, op: op),
            repeatCase: static (op, repeat) => Repeats(edit: repeat.Edit, op: op),
            pathCase: static (op, path) => Paths(edit: path.Edit, op: op),
            conductCase: static (op, conduct) => Mutated(
                family: AppSettingsFamily.General,
                write: () => op.Catch(() =>
                {
                    conduct.Conduct.Switch(
                        mouseSelectCase: static mouse => GeneralSettings.MouseSelectMode = mouse.Mode,
                        middleMouseCase: static middle => GeneralSettings.MiddleMouseMode = middle.Mode);
                    return Fin.Succ(value: unit);
                }),
                op: op),
            windowPositionCase: static (op, _) => op.Catch(() =>
                AppearanceSettings.InitialMainWindowPosition(out Rectangle bounds)
                    ? Fin.Succ<AppAnswer>(new AppAnswer.BoundsCase(bounds))
                    : Fin.Fail<AppAnswer>(op.InvalidResult())),
            autoRangeCase: static (op, range) => op.Catch(() =>
            {
                CurvatureAnalysisSettingsState state = range.Seed;
                return op.Confirm(success: CurvatureAnalysisSettings.CalculateCurvatureAutoRange(
                        meshes: range.Meshes,
                        settings: ref state))
                    .Map(_ => (AppAnswer)new AppAnswer.StateCase(State: new AppState.CurvatureCase(Value: state)));
            })));
    }

    private static Fin<AppOperation> Admit(AppOperation operation, Op op) => operation.Switch<
        (AppOperation Operation, Op Op), Fin<AppOperation>>(
        state: (operation, op),
        captureCase: static (state, value) => Accepted(value.Family is not null, state),
        fallbackCase: static (state, value) => Accepted(
            value.Family is not null && value.Theme.ForAll(static theme => theme is not null),
            state),
        applyCase: static (state, value) => Accepted(value.State is not null, state),
        resetCase: static (state, value) => Accepted(value.Family is not null, state),
        themeCase: static (state, value) => Accepted(value.Theme is not null, state),
        paintCase: static (state, value) => Accepted(Defined(value.Slot), state),
        widgetCase: static (state, value) => Accepted(Defined(value.Slot), state),
        aliasCase: static (state, value) => Optional(value.Edit).ToFin(Fail: state.Op.InvalidInput())
            .Bind(edit => Admit(edit, state.Op))
            .Map(edit => (AppOperation)new AppOperation.AliasCase(edit)),
        shortcutCase: static (state, value) => Optional(value.Edit).ToFin(Fail: state.Op.InvalidInput())
            .Bind(edit => Admit(edit, state.Op))
            .Map(edit => (AppOperation)new AppOperation.ShortcutCase(edit)),
        repeatCase: static (state, value) => Optional(value.Edit).ToFin(Fail: state.Op.InvalidInput())
            .Bind(edit => Admit(edit, state.Op))
            .Map(edit => (AppOperation)new AppOperation.RepeatCase(edit)),
        pathCase: static (state, value) => Optional(value.Edit).ToFin(Fail: state.Op.InvalidInput())
            .Bind(edit => Admit(edit, state.Op))
            .Map(edit => (AppOperation)new AppOperation.PathCase(edit)),
        conductCase: static (state, value) => Optional(value.Conduct).ToFin(Fail: state.Op.InvalidInput())
            .Bind(conduct => Admit(conduct, state.Op))
            .Map(conduct => (AppOperation)new AppOperation.ConductCase(conduct)),
        windowPositionCase: static (_, _) => Fin.Succ<AppOperation>(new AppOperation.WindowPositionCase()),
        autoRangeCase: static (state, value) => Accepted(
            value.Seed is not null && !value.Meshes.IsEmpty && value.Meshes.ForAll(static mesh => mesh is not null),
            state));

    private static Fin<AppOperation> Accepted(bool condition, (AppOperation Operation, Op Op) state) =>
        condition ? Fin.Succ(state.Operation) : Fin.Fail<AppOperation>(state.Op.InvalidInput());

    private static Fin<AliasEdit> Admit(AliasEdit edit, Op op) => edit.Switch<Op, Fin<AliasEdit>>(
        state: op,
        rosterCase: static (_, _) => Succ<AliasEdit>(new AliasEdit.RosterCase()),
        presetCase: static (_, _) => Succ<AliasEdit>(new AliasEdit.PresetCase()),
        probeCase: static (op, value) => op.AcceptValidated<AliasName>(value.Name.Value)
            .Map(name => (AliasEdit)new AliasEdit.ProbeCase(name)),
        putCase: static (op, value) => Admit(value.Binding, op)
            .Map(binding => (AliasEdit)new AliasEdit.PutCase(binding)),
        deleteCase: static (op, value) => op.AcceptValidated<AliasName>(value.Name.Value)
            .Map(name => (AliasEdit)new AliasEdit.DeleteCase(name)),
        mergeCase: static (op, value) => Optional(value.Merge).ToFin(Fail: op.InvalidInput())
            .Bind(merge => value.Bindings
                .Map(binding => Admit(binding, op))
                .Traverse(static binding => binding)
                .Map(bindings => (AliasEdit)new AliasEdit.MergeCase(bindings, merge))));

    private static Fin<AliasBinding> Admit(AliasBinding? binding, Op op) =>
        from present in Optional(binding).ToFin(Fail: op.InvalidInput())
        from name in op.AcceptValidated<AliasName>(present.Name.Value)
        from macro in op.AcceptValidated<MacroText>(present.Macro.Value)
        select new AliasBinding(name, macro, present.Instant);

    private static Fin<ShortcutEdit> Admit(ShortcutEdit edit, Op op) => edit.Switch<Op, Fin<ShortcutEdit>>(
        state: op,
        rosterCase: static (_, _) => Succ<ShortcutEdit>(new ShortcutEdit.RosterCase()),
        presetCase: static (_, _) => Succ<ShortcutEdit>(new ShortcutEdit.PresetCase()),
        assignCase: static (op, value) => Admit(value.Binding, op)
            .Map(binding => (ShortcutEdit)new ShortcutEdit.AssignCase(binding)),
        mergeCase: static (op, value) => Optional(value.Merge).ToFin(Fail: op.InvalidInput())
            .Bind(merge => value.Bindings
                .Map(binding => Admit(binding, op))
                .Traverse(static binding => binding)
                .Map(bindings => (ShortcutEdit)new ShortcutEdit.MergeCase(bindings, merge))));

    private static Fin<ShortcutBinding> Admit(ShortcutBinding? binding, Op op) =>
        from present in Optional(binding).ToFin(Fail: op.InvalidInput())
        from admitted in Admit(present.Key, present.Modifier, present.Macro.Value, op)
        select admitted;

    private static Fin<ShortcutBinding> Admit(
        KeyboardKey key,
        ModifierKey modifier,
        string? macro,
        Op op) =>
        from _key in guard(
            Defined(key)
            && FlagsDefined(modifier)
            && ShortcutKeySettings.IsAcceptableKeyCombo(key: key, modifier: modifier),
            op.InvalidInput()).ToFin()
        from admittedMacro in op.AcceptValidated<MacroText>(macro)
        select new ShortcutBinding(key, modifier, admittedMacro);

    private static Fin<RepeatEdit> Admit(RepeatEdit edit, Op op) => edit.Switch<Op, Fin<RepeatEdit>>(
        state: op,
        rosterCase: static (_, _) => Succ<RepeatEdit>(new RepeatEdit.RosterCase()),
        replaceCase: static (op, value) => value.CommandNames
            .Traverse(name => op.AcceptText(value: name).ToValidation())
            .As()
            .ToFin()
            .Map(names => (RepeatEdit)new RepeatEdit.ReplaceCase(names)));

    private static Fin<PathEdit> Admit(PathEdit edit, Op op) => edit.Switch<Op, Fin<PathEdit>>(
        state: op,
        rosterCase: static (_, _) => Succ<PathEdit>(new PathEdit.RosterCase()),
        addCase: static (op, value) =>
            from folder in DocumentPath.Of(value.Folder.Value, op)
            from _index in guard(value.IndexAt >= -1, op.InvalidInput()).ToFin()
            select (PathEdit)new PathEdit.AddCase(folder, value.IndexAt),
        removeCase: static (op, value) => DocumentPath.Of(value.Folder.Value, op)
            .Map(folder => (PathEdit)new PathEdit.RemoveCase(folder)),
        findCase: static (op, value) => op.AcceptText(value.FileName)
            .Map(name => (PathEdit)new PathEdit.FindCase(name)),
        autosaveCase: static (op, value) => value.Commands.Match(
            Some: commands => commands
                .Traverse(name => op.AcceptText(value: name).ToValidation())
                .As()
                .ToFin()
                .Map(names => (PathEdit)new PathEdit.AutosaveCase(Some(names))),
            None: static () => Succ<PathEdit>(new PathEdit.AutosaveCase(None))),
        recentCase: static (_, _) => Succ<PathEdit>(new PathEdit.RecentCase()),
        dataFolderCase: static (_, value) => Succ<PathEdit>(new PathEdit.DataFolderCase(value.CurrentUser)),
        templateFolderCase: static (_, value) => Succ<PathEdit>(new PathEdit.TemplateFolderCase(value.LanguageId)));

    private static Fin<GeneralConduct> Admit(GeneralConduct conduct, Op op) => conduct.Switch<Op, Fin<GeneralConduct>>(
        state: op,
        mouseSelectCase: static (op, value) => Defined(value.Mode)
            ? Succ<GeneralConduct>(new GeneralConduct.MouseSelectCase(value.Mode))
            : Fail<GeneralConduct>(op.InvalidInput()),
        middleMouseCase: static (op, value) => Defined(value.Mode)
            ? Succ<GeneralConduct>(new GeneralConduct.MiddleMouseCase(value.Mode))
            : Fail<GeneralConduct>(op.InvalidInput()));

    private static bool Defined<T>(T value) where T : struct, System.Enum => System.Enum.IsDefined(value);

    private static bool FlagsDefined<T>(T value) where T : struct, System.Enum
    {
        ulong admitted = System.Enum.GetValues<T>()
            .Aggregate(0UL, static (mask, item) => mask | Convert.ToUInt64(item));
        return (Convert.ToUInt64(value) & ~admitted) == 0UL;
    }

    private static Fin<AppAnswer> Mutated(AppSettingsFamily family, Func<Fin<Unit>> write, Op op)
    {
        AppObservation Observe() => family.Capture(op: op).Match(
            Succ: state => (AppObservation)new AppObservation.ObservedCase(State: state),
            Fail: _ => new AppObservation.UnobservableCase(Family: family));
        AppObservation prior = Observe();
        return write().Map(_ =>
        {
            AppObservation current = Observe();
            return (AppAnswer)new AppAnswer.MutationCase(Receipt: new AppMutationReceipt(
                Family: family,
                Prior: prior,
                Current: current,
                Changed: (prior, current) is not (AppObservation.ObservedCase before, AppObservation.ObservedCase after)
                    || before.State != after.State));
        });
    }

    private static Fin<AppAnswer> Swatch(
        string slot,
        Func<Color> read,
        Action<Color> write,
        Func<Color> preset,
        Option<Color> value,
        Op op) =>
        op.Catch(() =>
        {
            Color prior = read();
            value.IfSome(write);
            return Fin.Succ(value: (AppAnswer)new AppAnswer.SwatchCase(Receipt: new SwatchReceipt(
                Slot: slot,
                Prior: prior,
                Current: read(),
                Preset: preset())));
        });

    private static Fin<AppAnswer> Aliases(AliasEdit edit, Op op) => edit.Switch<Op, Fin<AppAnswer>>(
        op,
        rosterCase: static (op, _) => AliasBindings(
            source: () => CommandAliasList.GetNames().Select(name => (
                Name: name,
                Macro: CommandAliasList.GetMacro(alias: name),
                Instant: Optional(CommandAliasList.FindAlias(alias: name))
                    .Map(static found => found.Instant)
                    .IfNone(noneValue: false))),
            op: op),
        presetCase: static (op, _) => AliasBindings(
            source: () => CommandAliasList.GetDefaults().Select(static binding => (
                Name: binding.Key,
                Macro: binding.Value,
                Instant: false)),
            op: op),
        probeCase: static (op, probe) => op.Catch(() => Fin.Succ(value: (AppAnswer)new AppAnswer.MacroCase(
            Macro: CommandAliasList.IsAlias(alias: probe.Name.Value)
                ? op.AcceptValidated<MacroText>(CommandAliasList.GetMacro(alias: probe.Name.Value)).ToOption()
                : None))),
        putCase: static (op, put) => op.Catch(() => op.Confirm(success: CommandAliasList.IsAlias(alias: put.Binding.Name.Value)
                ? CommandAliasList.SetMacro(alias: put.Binding.Name.Value, macro: put.Binding.Macro.Value)
                : CommandAliasList.Add(alias: put.Binding.Name.Value, macro: put.Binding.Macro.Value))
            .Map(_ => (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(CommandAliasList.GetNames())))),
        deleteCase: static (op, delete) => op.Catch(() => op.Confirm(success: CommandAliasList.Delete(alias: delete.Name.Value))
            .Map(_ => (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(CommandAliasList.GetNames())))),
        mergeCase: static (op, merge) => op.Catch(() =>
        {
            CommandAliasList.Update(
                aliases: merge.Bindings.Map(static binding => new CommandAlias(
                    alias: binding.Name.Value,
                    macro: binding.Macro.Value,
                    instant: binding.Instant)).AsIterable(),
                replaceAll: merge.Merge.ReplaceAll);
            return Fin.Succ(value: (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(CommandAliasList.GetNames())));
        }));

    private static Fin<AppAnswer> AliasBindings(
        Func<IEnumerable<(string Name, string Macro, bool Instant)>> source,
        Op op) =>
        op.Catch(() => toSeq(source())
            .Traverse(binding =>
                (from name in op.AcceptValidated<AliasName>(binding.Name)
                 from macro in op.AcceptValidated<MacroText>(binding.Macro)
                 select new AliasBinding(name, macro, binding.Instant)).ToValidation())
            .As()
            .ToFin()
            .Map(static bindings => (AppAnswer)new AppAnswer.AliasesCase(Bindings: bindings)));

    private static Fin<AppAnswer> Shortcuts(ShortcutEdit edit, Op op) => edit.Switch<Op, Fin<AppAnswer>>(
        op,
        rosterCase: static (op, _) => Bindings(source: ShortcutKeySettings.GetShortcuts, op: op),
        presetCase: static (op, _) => Bindings(source: ShortcutKeySettings.GetDefaults, op: op),
        assignCase: static (op, assign) =>
            from written in op.Catch(() =>
            {
                ShortcutKeySettings.SetMacro(
                    key: assign.Binding.Key,
                    modifier: assign.Binding.Modifier,
                    macro: assign.Binding.Macro.Value);
                return Fin.Succ(value: unit);
            })
            from roster in Bindings(source: ShortcutKeySettings.GetShortcuts, op: op)
            select roster,
        mergeCase: static (op, merge) => op.Catch(() =>
        {
            ShortcutKeySettings.Update(
                shortcuts: merge.Bindings.Map(static binding => new KeyboardShortcut
                {
                    Key = binding.Key,
                    Modifier = binding.Modifier,
                    Macro = binding.Macro.Value,
                }).AsIterable(),
                replaceAll: merge.Merge.ReplaceAll);
            return Fin.Succ(value: unit);
        }).Bind(_ => Bindings(source: ShortcutKeySettings.GetShortcuts, op: op)));

    private static Fin<AppAnswer> Bindings(Func<KeyboardShortcut[]> source, Op op) =>
        op.Catch(() => toSeq(source())
            .Filter(static shortcut => !string.IsNullOrWhiteSpace(value: shortcut.Macro))
            .Traverse(shortcut => Admit(shortcut.Key, shortcut.Modifier, shortcut.Macro, op).ToValidation())
            .As()
            .ToFin()
            .Map(static bindings => (AppAnswer)new AppAnswer.ShortcutsCase(Bindings: bindings)));

    private static Fin<AppAnswer> Repeats(RepeatEdit edit, Op op) => edit.Switch<Op, Fin<AppAnswer>>(
        op,
        rosterCase: static (op, _) => op.Catch(() => Fin.Succ(value: (AppAnswer)new AppAnswer.RepeatCase(
            Roster: new RepeatRoster(
                Enabled: NeverRepeatList.UseNeverRepeatList,
                CommandNames: toSeq(NeverRepeatList.CommandNames()))))),
        replaceCase: static (op, replace) =>
            from landed in op.Catch(() => op.Confirm(
                success: NeverRepeatList.SetList(commandNames: replace.CommandNames.ToArray()) >= 0))
            select (AppAnswer)new AppAnswer.RepeatCase(Roster: new RepeatRoster(
                Enabled: NeverRepeatList.UseNeverRepeatList,
                CommandNames: toSeq(NeverRepeatList.CommandNames()))));

    private static Fin<AppAnswer> Paths(PathEdit edit, Op op) => edit.Switch<Op, Fin<AppAnswer>>(
        op,
        rosterCase: static (op, _) => op.Catch(() => Fin.Succ(value: (AppAnswer)new AppAnswer.RosterCase(
            Names: toSeq(FileSettings.GetSearchPaths())))),
        addCase: static (op, add) => op.Catch(() => op.Confirm(
                success: FileSettings.AddSearchPath(folder: add.Folder.Value, index: add.IndexAt) >= 0))
            .Map(_ => (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(FileSettings.GetSearchPaths()))),
        removeCase: static (op, remove) => op.Catch(() => op.Confirm(success: FileSettings.DeleteSearchPath(folder: remove.Folder.Value))
            .Map(_ => (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(FileSettings.GetSearchPaths())))),
        findCase: static (op, find) =>
            from resolved in op.Catch(() => Optional(FileSettings.FindFile(fileName: find.FileName))
                .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
                .Traverse(value => DocumentPath.Of(value: value, key: op))
                .As())
            select (AppAnswer)new AppAnswer.ResolvedCase(Path: resolved),
        autosaveCase: static (op, autosave) => autosave.Commands.Match(
            Some: commands => op.Catch(() =>
                {
                    FileSettings.SetAutoSaveBeforeCommands(commands: commands.ToArray());
                    return Fin.Succ(value: unit);
                })
                .Map(_ => (AppAnswer)new AppAnswer.RosterCase(Names: toSeq(FileSettings.AutoSaveBeforeCommands()))),
            None: () => op.Catch(() => Fin.Succ(value: (AppAnswer)new AppAnswer.RosterCase(
                Names: toSeq(FileSettings.AutoSaveBeforeCommands()))))),
        recentCase: static (op, _) => op.Catch(() => Fin.Succ(value: (AppAnswer)new AppAnswer.RosterCase(
            Names: toSeq(FileSettings.RecentlyOpenedFiles())))),
        dataFolderCase: static (op, data) => op.Catch(() => Optional(FileSettings.GetDataFolder(currentUser: data.CurrentUser))
            .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
            .Traverse(value => DocumentPath.Of(value: value, key: op))
            .As()
            .Map(static resolved => (AppAnswer)new AppAnswer.ResolvedCase(Path: resolved))),
        templateFolderCase: static (op, template) => op.Catch(() => Optional(
                FileSettings.DefaultTemplateFolderForLanguageID(languageID: template.LanguageId))
            .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
            .Traverse(value => DocumentPath.Of(value: value, key: op))
            .As()
            .Map(static resolved => (AppAnswer)new AppAnswer.ResolvedCase(Path: resolved))));
}
```

## [04]-[LIFECYCLE]

`AppOperation` admits once, family rows own every host static, and one total interpreter lands a detached answer. State mutations follow capture → write → capture, so `AppMutationReceipt` carries real prior/current evidence with `Changed` derived from state equality; `SoftTransform` and `PackageManager` report `UnobservableCase` on both sides, and unsupported family verbs refuse with a typed fault. Registry mutations return the landed roster in the same answer, and initial window placement returns admitted bounds without exposing the host out-parameter.

Theme adoption routes through `AppTheme` rows — `SetToDarkMode`/`SetToLightMode` behind one `Adopt` column with `GetDefaultState(darkMode)` as the themed fallback — and the paint/widget surface stays keyed: a color slot is a `PaintColor`/`WidgetColor` enum row inside one `Option`-discriminated read/write case, never a per-slot member family.

## [05]-[SEAMS]

`SettingsRoot.ApplicationCase` (settings.md) owns the raw `PersistentSettings.RhinoAppSettings` node tree these families persist through; this page never writes a node, and settings.md never reaches a typed `Rhino.ApplicationSettings` owner. `HistorySettings` stays with Document undo governance, `ViewSettings.DefinedViewSet*` restore-scope flags and analysis states feed display-mode attachment, and `PlugIn.GetPluginSettings` custody stays with the plug-in root.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
