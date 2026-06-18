using System.Runtime.InteropServices;
using Rasm.Rhino.Events;
using Color = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using Result = Rhino.Commands.Result;
using UiGumball = Rasm.Rhino.UI.UiGumball;
using UiGumballSnapshot = Rasm.Rhino.UI.UiGumballSnapshot;
using UiViewportPreview = Rasm.Rhino.UI.UiViewportPreview;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ------------------------------------------------------------------------------
[Flags]
public enum CommandInputAccept {
    None = 0,
    Nothing = 1,
    Undo = 2,
    EnterWhenDone = 4,
    Number = 8,
    Point = 16,
    Color = 32,
    Text = 64,
    CustomMessage = 128,
    TransparentCommands = 256,
}

public enum CommandInputOutcomeKind { Value, Option, Undo, Nothing, Rejected, NoResult, Miss, Timeout, Cancel, Exit }

[Flags]
public enum CommandPointEventPhase {
    None = 0,
    MouseMove = 1,
    MouseDown = 2,
    DynamicDraw = 4,
    PostDrawObjects = 8,
    Preview = DynamicDraw | PostDrawObjects,
    Input = MouseMove | MouseDown,
    All = MouseMove | MouseDown | DynamicDraw | PostDrawObjects,
}

[Flags] public enum PointerButtons { None = 0, Left = 1, Right = 2, Middle = 4 }
[Flags] public enum PointerModifiers { None = 0, Shift = 1, Control = 2 }

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct CommandGet<T>(
    Option<GetResult> Raw,
    Result CommandResult,
    Option<T> Value,
    Option<CommandOptionValue> Option,
    Option<CommandSelection.TrimResult> SelectionTrim,
    bool GotDefault,
    CommandSnapshot Snapshot) {
    internal static CommandGet<T> Of(GetBaseClass getter, GetResult raw, Option<T> value, Option<CommandOptionValue> option, Option<CommandSelection.TrimResult> selectionTrim = default) =>
        new(Raw: Some(raw), CommandResult: getter.CommandResult(), Value: value, Option: option, SelectionTrim: selectionTrim, GotDefault: getter.GotDefault(), Snapshot: CommandSnapshot.Of(getter: getter, raw: raw));

    internal static CommandGet<T> Native(Result result, Option<T> value) =>
        new(Raw: Option<GetResult>.None, CommandResult: result, Value: value, Option: Option<CommandOptionValue>.None, SelectionTrim: Option<CommandSelection.TrimResult>.None, GotDefault: false, Snapshot: CommandSnapshot.Empty);

    internal static CommandGet<T> ScriptOption(CommandOptionValue option) =>
        new(Raw: Some(GetResult.Option), CommandResult: Result.Success, Value: Option<T>.None, Option: Some(option), SelectionTrim: Option<CommandSelection.TrimResult>.None, GotDefault: false, Snapshot: CommandSnapshot.Empty);

    public Option<TOut> As<TOut>() =>
        Value.Bind(static value => value is TOut typed ? Some(typed) : Option<TOut>.None);

    public bool IsUndo => Raw.Map(static raw => raw == GetResult.Undo).IfNone(noneValue: false);
}

public readonly record struct CommandSnapshot(
    Option<RhinoView> View,
    Option<DrawingPoint> WindowPoint,
    Option<double> Number,
    Option<string> Text,
    Option<object> CustomMessage,
    Option<Point3d> Point,
    Option<Vector3d> Vector,
    Option<Color> Color,
    Option<DrawingRectangle> PickRectangle,
    Option<DrawingRectangle> Rectangle2d,
    Seq<DrawingPoint> Line2d) {
    internal static CommandSnapshot Empty { get; } = new(default, default, default, default, default, default, default, default, default, default, Seq<DrawingPoint>());

    internal static CommandSnapshot Of(GetBaseClass getter, GetResult raw) =>
        new(View: Optional(getter.View()), WindowPoint: raw is GetResult.Point or GetResult.Point2d ? Some(getter.Point2d()) : Option<DrawingPoint>.None, Number: raw is GetResult.Number ? Some(getter.Number()) : Option<double>.None, Text: raw is GetResult.String ? Optional(getter.StringResult()) : Option<string>.None, CustomMessage: raw is GetResult.CustomMessage ? Optional(getter.CustomMessage()) : Option<object>.None, Point: raw is GetResult.Point ? Some(getter.Point()) : Option<Point3d>.None, Vector: (raw, getter.Vector()) switch { (GetResult.Point, Vector3d vector) when vector.IsValid && !vector.IsTiny() => Some(vector), _ => Option<Vector3d>.None }, Color: raw is GetResult.Color ? Some(getter.Color()) : Option<Color>.None, PickRectangle: raw is GetResult.Object ? Some(getter.PickRectangle()) : Option<DrawingRectangle>.None, Rectangle2d: raw is GetResult.Rectangle2d ? Some(getter.Rectangle2d()) : Option<DrawingRectangle>.None, Line2d: raw is GetResult.Line2d ? toSeq(getter.Line2d()) : Seq<DrawingPoint>());

    internal bool Has(GetResult raw) => raw switch { GetResult.Number => Number.IsSome, GetResult.String => Text.IsSome, GetResult.CustomMessage => CustomMessage.IsSome, GetResult.Point => Point.IsSome || WindowPoint.IsSome || Vector.IsSome, GetResult.Point2d => WindowPoint.IsSome, GetResult.Color => Color.IsSome, GetResult.Object => PickRectangle.IsSome, GetResult.Rectangle2d => Rectangle2d.IsSome, GetResult.Line2d => !Line2d.IsEmpty, _ => false };
}

public readonly record struct CommandInputOutcome<T>(CommandInputOutcomeKind Kind, Option<CommandGet<T>> Result) {
    internal static CommandInputOutcome<T> Of(CommandGet<T> result) =>
        new(
            Kind: (Rejected: result.SelectionTrim.IsSome && result.Value.IsNone, result.Raw.Case, result.CommandResult) switch {
                (true, _, _) => CommandInputOutcomeKind.Rejected,
                (_, GetResult.Option, _) => CommandInputOutcomeKind.Option,
                (_, GetResult.Undo, _) => CommandInputOutcomeKind.Undo,
                (_, GetResult.Nothing, _) => CommandInputOutcomeKind.Nothing,
                (_, GetResult.NoResult, _) => CommandInputOutcomeKind.NoResult,
                (_, GetResult.Miss, _) => CommandInputOutcomeKind.Miss,
                (_, GetResult.Timeout, _) => CommandInputOutcomeKind.Timeout,
                (_, GetResult, _) => CommandInputOutcomeKind.Value,
                (_, _, global::Rhino.Commands.Result.Nothing) => CommandInputOutcomeKind.Nothing,
                _ => CommandInputOutcomeKind.Value,
            },
            Result: Some(result));

    internal static CommandInputOutcome<T> Cancelled { get; } =
        new(Kind: CommandInputOutcomeKind.Cancel, Result: Option<CommandGet<T>>.None);

    internal static CommandInputOutcome<T> Exit { get; } =
        new(Kind: CommandInputOutcomeKind.Exit, Result: Option<CommandGet<T>>.None);

    internal Fin<CommandGet<T>> ToGet() =>
        Kind switch {
            CommandInputOutcomeKind.Exit => Fin.Fail<CommandGet<T>>(error: CommandInput.ExitRhinoSignal),
            CommandInputOutcomeKind.Cancel => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            _ => Result.ToFin(Fail: Op.Of(name: nameof(CommandInputOutcome<>)).InvalidResult()),
        };
}

public sealed record CommandObjectSelection(
    int Minimum = 1,
    int Maximum = 1,
    ObjectType Types = ObjectType.AnyObject,
    GeometryAttributeFilter Attributes = default,
    Option<GetObjectGeometryFilter> Filter = default,
    bool PreSelect = true,
    bool IgnoreUnacceptablePreselected = true,
    bool PostSelect = true,
    bool SubObjects = true,
    bool Groups = true,
    bool References = true,
    bool Locked = false,
    bool AlreadySelected = false,
    bool SelectPrevious = true,
    bool Highlight = true,
    bool IgnoreGrips = true,
    bool ClearOnEntry = true,
    bool UnselectOnExit = true,
    bool DeselectAllBeforePostSelect = false,
    bool OneByOnePostSelect = false,
    bool PressEnterWhenDonePrompt = true,
    Option<string> PressEnterWhenDoneText = default,
    bool ChooseOneQuestion = false,
    bool BottomObjectPreference = false,
    bool ProxyBrepFromSubD = false,
    bool InactiveDetailPickEnabled = false) {
    public static CommandObjectSelection Default { get; } = new();

    internal Fin<Unit> Apply(GetObject getter) =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(CommandObjectSelection)).InvalidInput())
            .Map(valid => {
                valid.GeometryFilter = Types;
                valid.GeometryAttributeFilter = Attributes;
                _ = Filter.Iter(active => valid.SetCustomGeometryFilter(active));
                valid.EnablePreSelect(enable: PreSelect, ignoreUnacceptablePreselectedObjects: IgnoreUnacceptablePreselected);
                valid.EnablePostSelect(enable: PostSelect);
                valid.SubObjectSelect = SubObjects;
                valid.GroupSelect = Groups;
                valid.ReferenceObjectSelect = References;
                valid.LockedObjectSelect = Locked;
                valid.AlreadySelectedObjectSelect = AlreadySelected;
                valid.EnableSelPrevious(enable: SelectPrevious);
                valid.EnableHighlight(enable: Highlight);
                valid.EnableIgnoreGrips(enable: IgnoreGrips);
                valid.EnableClearObjectsOnEntry(enable: ClearOnEntry);
                valid.EnableUnselectObjectsOnExit(enable: UnselectOnExit);
                valid.DeselectAllBeforePostSelect = DeselectAllBeforePostSelect;
                valid.OneByOnePostSelect = OneByOnePostSelect;
                valid.EnablePressEnterWhenDonePrompt(enable: PressEnterWhenDonePrompt);
                _ = PressEnterWhenDoneText.Iter(valid.SetPressEnterWhenDonePrompt);
                valid.ChooseOneQuestion = ChooseOneQuestion;
                valid.BottomObjectPreference = BottomObjectPreference;
                valid.ProxyBrepFromSubD = ProxyBrepFromSubD;
                valid.InactiveDetailPickEnabled = InactiveDetailPickEnabled;
                return unit;
            });
}

public sealed record CommandInputPolicy {
    public Seq<Action<GetBaseClass>> Applies { get; init; }
    public Option<Func<CommandPointEvent, Fin<Unit>>> PointEvent { get; init; }
    public CommandPointEventPhase PointEventPhases { get; init; }
    public Option<UiGumball> Gumball { get; init; }
    public Seq<CommandOption> OptionList { get; init; }
    public Option<PointSpec> PointMode { get; init; }
    public Option<Func<RhinoViewport, Point3d, Transform>> TransformMode { get; init; }
    public Option<CommandSelection> DonorSelection { get; init; }
    internal Option<Scalar> ScalarMode { get; init; }
    public Option<LimitSpec> LimitsMode { get; init; }
    internal Option<BoxSpec> BoxMode { get; init; }
    public Option<CommandObjectSelection> ObjectSelection { get; init; }
    public Option<string> PromptText { get; init; }
    public Option<Color> ColorSeed { get; init; }
    public Option<(string Off, string On)> BoolLabels { get; init; }
    public bool FullFrameRedrawDuringGet { get; init; }
    public bool IsLiteralText { get; init; }
    public CommandInputAccept AcceptModes { get; init; }

    internal bool AcceptsNothing => Accepts(mode: CommandInputAccept.Nothing);
    internal bool AcceptsUndo => Accepts(mode: CommandInputAccept.Undo);
    internal static CommandInputPolicy Empty { get; } = new();

    internal enum ScalarKind { Number, Length, Angle }
    internal sealed record Scalar(ScalarKind Kind, Option<UnitSystem> LengthUnits, Option<AngleUnitSystem> AngleUnits);

    public sealed record PointSpec(bool OnMouseUp, bool TwoDimensional, Option<global::Rhino.UI.CursorStyle> Cursor, Option<bool> ObjectSnapCursors, Option<Point3d> BasePoint, bool DrawLineFromBasePoint, bool SnapToCurves, bool PermitConstraintOptions, bool NoRedrawOnExit, bool PermitFromOption, bool PermitTabMode, int PermitElevatorMode, Seq<Point3d> SnapPoints, Seq<Point3d> ConstructionPoints, Seq<CommandPointConstraint> Constraints, Option<double> DistanceFromBasePoint, Option<bool> PermitOrthoSnap, Option<bool> PermitObjectSnap, Option<Color> DynamicDrawColor, Option<(bool Enabled, bool Ends)> CurveSnapTangent, Option<(bool Enabled, bool Ends)> CurveSnapPerp, Option<(bool Enabled, bool Reverse)> CurveSnapArrow, bool ClearConstraints);
    public static PointSpec DefaultPointSpec { get; } = new(OnMouseUp: false, TwoDimensional: false, Cursor: Option<global::Rhino.UI.CursorStyle>.None, ObjectSnapCursors: Option<bool>.None, BasePoint: Option<Point3d>.None, DrawLineFromBasePoint: false, SnapToCurves: false, PermitConstraintOptions: true, NoRedrawOnExit: false, PermitFromOption: true, PermitTabMode: true, PermitElevatorMode: 0, SnapPoints: Seq<Point3d>(), ConstructionPoints: Seq<Point3d>(), Constraints: Seq<CommandPointConstraint>(), DistanceFromBasePoint: Option<double>.None, PermitOrthoSnap: Option<bool>.None, PermitObjectSnap: Option<bool>.None, DynamicDrawColor: Option<Color>.None, CurveSnapTangent: Option<(bool Enabled, bool Ends)>.None, CurveSnapPerp: Option<(bool Enabled, bool Ends)>.None, CurveSnapArrow: Option<(bool Enabled, bool Reverse)>.None, ClearConstraints: false);

    public sealed record LimitSpec(Option<object> Lower, Option<object> Upper, bool StrictlyLower, bool StrictlyUpper) {
        internal Fin<(Option<TValue> Lower, Option<TValue> Upper)> Project<TValue>(Op op) where TValue : IComparable<TValue> => (Lower.Bind(ScalarBound<TValue>), Upper.Bind(ScalarBound<TValue>)) switch { (Option<TValue> lower, _) when Lower.IsSome && !lower.IsSome => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: op.InvalidInput()), (_, Option<TValue> upper) when Upper.IsSome && !upper.IsSome => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: op.InvalidInput()), (Option<TValue> lower, Option<TValue> upper) when lower.Case is TValue left && upper.Case is TValue right && left.CompareTo(other: right) > 0 => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: op.InvalidInput()), (Option<TValue> lower, Option<TValue> upper) when lower.Case is TValue left && upper.Case is TValue right && left.CompareTo(other: right) == 0 && (StrictlyLower || StrictlyUpper) => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: op.InvalidInput()), (Option<TValue> lower, Option<TValue> upper) => Fin.Succ(value: (Lower: lower, Upper: upper)) };
        internal Option<TValue> Accept<TValue>(TValue value) where TValue : IComparable<TValue> =>
            Project<TValue>(op: Op.Of(name: nameof(LimitSpec))).ToOption().Bind(bounds => bounds switch {
                (Option<TValue> lower, _) when lower.Case is TValue lo && (StrictlyLower ? value.CompareTo(other: lo) <= 0 : value.CompareTo(other: lo) < 0) => Option<TValue>.None,
                (_, Option<TValue> upper) when upper.Case is TValue hi && (StrictlyUpper ? value.CompareTo(other: hi) >= 0 : value.CompareTo(other: hi) > 0) => Option<TValue>.None,
                _ => Some(value),
            });

        private static Option<TValue> ScalarBound<TValue>(object value) where TValue : IComparable<TValue> =>
            value switch {
                TValue typed => Some(typed),
                double d when typeof(TValue) == typeof(double) && RhinoMath.IsValidDouble(x: d) => Some((TValue)(object)d),
                double d when typeof(TValue) == typeof(int) && RhinoMath.IsValidDouble(x: d) && Math.Truncate(d) == d && d >= int.MinValue && d <= int.MaxValue => Some((TValue)(object)(int)d),
                int i when typeof(TValue) == typeof(double) => Some((TValue)(object)(double)i),
                _ => Option<TValue>.None,
            };
    }

    public static LimitSpec Limit<T>(Option<T> lower = default, Option<T> upper = default, bool strictlyLower = false, bool strictlyUpper = false) where T : IComparable<T> =>
        new(Lower: lower.Map(static value => (object)value), Upper: upper.Map(static value => (object)value), StrictlyLower: strictlyLower, StrictlyUpper: strictlyUpper);

    internal sealed record BoxSpec(
        GetBoxMode Mode,
        Option<Point3d> BasePoint,
        Option<string> Prompt1,
        Option<string> Prompt2,
        Option<string> Prompt3) {
        internal static BoxSpec Default { get; } = new(
            Mode: GetBoxMode.All,
            BasePoint: Option<Point3d>.None,
            Prompt1: Option<string>.None,
            Prompt2: Option<string>.None,
            Prompt3: Option<string>.None);
        internal bool IsDefault =>
            Mode == GetBoxMode.All && BasePoint.IsNone && Prompt1.IsNone && Prompt2.IsNone && Prompt3.IsNone;
    }

    public static CommandInputPolicy Configure<TGetter>(Action<TGetter> apply) where TGetter : GetBaseClass =>
        (typeof(TGetter), Optional(apply).Case) switch {
            (Type type, Action<TGetter> action) when type == typeof(GetObject) => Action(getter => _ = getter is GetObject value ? Op.Side(() => action((TGetter)(GetBaseClass)value)) : unit),
            (Type type, Action<TGetter> action) when type == typeof(GetPoint) => Action(getter => _ = getter is GetPoint value ? Op.Side(() => action((TGetter)(GetBaseClass)value)) : unit),
            (_, Action<TGetter> action) => Action(getter => _ = getter is TGetter typed ? Op.Side(() => action(typed)) : unit),
            _ => Empty,
        };
    public static CommandInputPolicy Prompt(string value) => Action(getter => getter.SetCommandPrompt(value)) with { PromptText = Optional(value) };
    public static CommandInputPolicy PromptDefault(string value) => Optional(value).Map(defaultText => Configure<GetBaseClass>(apply: getter => getter.SetCommandPromptDefault(defaultText))).IfNone(Empty);
    public static CommandInputPolicy Wait(int milliseconds) =>
        milliseconds switch {
            > 0 => Configure<GetBaseClass>(apply: getter => getter.SetWaitDuration(milliseconds)),
            _ => Empty,
        };
    public static CommandInputPolicy ClearDefault() => Configure<GetBaseClass>(apply: static getter => getter.ClearDefault());
    public static CommandInputPolicy ClearOptions() => Configure<GetBaseClass>(apply: static getter => getter.ClearCommandOptions());
    // pure acceptance gate; PostCustomMessage stores into a process-static slot and must be issued by the caller AFTER the get is in flight to avoid the cross-get race and pre-loop message loss
    public static CommandInputPolicy CustomMessage() =>
        Action(static getter => getter.AcceptCustomMessage(enable: true)) with { AcceptModes = CommandInputAccept.CustomMessage };
    public static CommandInputPolicy Default(object value) =>
        value switch {
            Point3d point => Configure<GetBaseClass>(apply: getter => getter.SetDefaultPoint(point: point)),
            double number => Configure<GetBaseClass>(apply: getter => getter.SetDefaultNumber(number)),
            int integer => Configure<GetBaseClass>(apply: getter => getter.SetDefaultInteger(integer)),
            string text => Configure<GetBaseClass>(apply: getter => getter.SetDefaultString(text)),
            Color color => Configure<GetBaseClass>(apply: getter => getter.SetDefaultColor(color)),
            _ => Empty,
        };
    // Text-literal and TransparentCommands are mutually exclusive on one get; construction drops TransparentCommands when Text is requested so the conflicting policy is unrepresentable downstream.
    public static CommandInputPolicy Accept(CommandInputAccept modes, bool acceptZero = true) {
        CommandInputAccept admitted = modes.HasFlag(CommandInputAccept.Text)
            ? modes & ~CommandInputAccept.TransparentCommands
            : modes;
        return toSeq(new (CommandInputAccept Mode, Action<GetBaseClass> Apply)[] {
            (CommandInputAccept.Nothing, static getter => getter.AcceptNothing(enable: true)),
            (CommandInputAccept.Undo, static getter => getter.AcceptUndo(enable: true)),
            (CommandInputAccept.EnterWhenDone, static getter => getter.AcceptEnterWhenDone(enable: true)),
            (CommandInputAccept.Number, getter => getter.AcceptNumber(enable: true, acceptZero: acceptZero)),
            (CommandInputAccept.Point, static getter => getter.AcceptPoint(enable: true)),
            (CommandInputAccept.Color, static getter => getter.AcceptColor(enable: true)),
            (CommandInputAccept.Text, static getter => getter.AcceptString(enable: true)),
            (CommandInputAccept.CustomMessage, static getter => getter.AcceptCustomMessage(enable: true)),
            (CommandInputAccept.TransparentCommands, static getter => getter.EnableTransparentCommands(enable: true)),
        }).Filter(row => (admitted & row.Mode) == row.Mode)
          .Fold(Empty, static (policy, row) => policy with { Applies = policy.Applies.Add(value: row.Apply), AcceptModes = policy.AcceptModes | row.Mode });
    }
    public static CommandInputPolicy TransparentCommands(bool enabled = true) => Configure<GetBaseClass>(apply: getter => getter.EnableTransparentCommands(enable: enabled));
    public static CommandInputPolicy Options(params CommandOption[] values) => Empty with { OptionList = toSeq(values) };
    // resolves the option set by the caller's state-derived key at policy-construction time, producing the same flat OptionList shape as Options() so the merge fold and Apply<TGetter> path are unaffected; a missing key collapses to Empty rather than branching at the call site
    public static CommandInputPolicy ConditionalOptions<TKey>(TKey key, HashMap<TKey, Seq<CommandOption>> branches) where TKey : notnull =>
        branches.Find(key).Map(static options => Empty with { OptionList = options }).IfNone(Empty);
    public static CommandInputPolicy PointEvents(Func<CommandPointEvent, Fin<Unit>> change, CommandPointEventPhase phases = CommandPointEventPhase.All, bool fullFrameRedraw = false) =>
        Optional(change).Map(apply => Empty with { PointEvent = Some(apply), PointEventPhases = phases, FullFrameRedrawDuringGet = fullFrameRedraw }).IfNone(Empty);
    public static CommandInputPolicy PointGumball(UiGumball gumball) =>
        Optional(gumball).Map(active => Empty with { PointEvent = Some<Func<CommandPointEvent, Fin<Unit>>>(static _ => Fin.Succ(value: unit)), PointEventPhases = CommandPointEventPhase.All, Gumball = Some(active) }).IfNone(Empty);
    public static CommandInputPolicy Transform(Func<RhinoViewport, Point3d, Transform> calculate, Option<CommandSelection> donor = default) =>
        Optional(calculate).Map(project => Empty with { TransformMode = Some(project), DonorSelection = donor }).IfNone(Empty);
    public static CommandInputPolicy Objects(CommandObjectSelection selection) =>
        Optional(selection)
            .Map(static valid => Empty with { ObjectSelection = Some(valid) })
            .IfNone(Empty);

    public static CommandInputPolicy Point(PointSpec spec) =>
        Optional(spec).Map(active => Empty with { PointMode = Some(active) }).IfNone(Empty);
    public static CommandInputPolicy Point(Func<PointSpec, PointSpec> update) =>
        Optional(update).Map(active => Point(spec: active(arg: DefaultPointSpec))).IfNone(Empty);
    public static CommandInputPolicy Bounds<T>(Option<T> lower = default, Option<T> upper = default, bool strictlyLower = false, bool strictlyUpper = false) where T : IComparable<T> =>
        Empty with { LimitsMode = Some(Limit(lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper)) };
    public static CommandInputPolicy Number() => Empty with { ScalarMode = Some(new Scalar(Kind: ScalarKind.Number, LengthUnits: Option<UnitSystem>.None, AngleUnits: Option<AngleUnitSystem>.None)) };
    public static CommandInputPolicy Number<T>(Option<T> lower = default, Option<T> upper = default, bool strictlyLower = false, bool strictlyUpper = false) where T : IComparable<T> => Bounds(lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper) + Number();
    public static CommandInputPolicy Length(Option<UnitSystem> units = default) => Empty with { ScalarMode = Some(new Scalar(Kind: ScalarKind.Length, LengthUnits: units, AngleUnits: Option<AngleUnitSystem>.None)) };
    public static CommandInputPolicy Angle(AngleUnitSystem units) => Empty with { ScalarMode = Some(new Scalar(Kind: ScalarKind.Angle, LengthUnits: Option<UnitSystem>.None, AngleUnits: Some(units))) };
    public static CommandInputPolicy LiteralText() => Empty with { IsLiteralText = true };
    public static CommandInputPolicy Bool(string off = "No", string on = "Yes") => Empty with { BoolLabels = Some((Off: off, On: on)) };
    public static CommandInputPolicy SeedColor(Color value) => Empty with { ColorSeed = Some(value) };
    public static CommandInputPolicy Box(
        GetBoxMode mode = GetBoxMode.All,
        Point3d? basePoint = null,
        string? prompt1 = null,
        string? prompt2 = null,
        string? prompt3 = null) =>
        Empty with {
            BoxMode = Some(new BoxSpec(
                Mode: mode,
                BasePoint: Optional(basePoint),
                Prompt1: Optional(prompt1),
                Prompt2: Optional(prompt2),
                Prompt3: Optional(prompt3))),
        };

    // each field combines by its own monoid law: deferred getter mutations and the option list concatenate (order-preserving),
    // PointEvent sequences (left then right, first failure short-circuits), phases and accept modes flag-OR, scalar/limit/box/etc. are last-of-two Option-choice
    public static CommandInputPolicy operator +(CommandInputPolicy left, CommandInputPolicy right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        ArgumentNullException.ThrowIfNull(argument: right);
        return new() {
            Applies = left.Applies + right.Applies,
            PointEvent = (left.PointEvent.Case, right.PointEvent.Case) switch {
                (Func<CommandPointEvent, Fin<Unit>> a, Func<CommandPointEvent, Fin<Unit>> b) =>
                    Some<Func<CommandPointEvent, Fin<Unit>>>(e => a(arg: e).Bind(_ => b(arg: e))),
                (_, Func<CommandPointEvent, Fin<Unit>> b) => Some(b),
                (Func<CommandPointEvent, Fin<Unit>> a, _) => Some(a),
                _ => Option<Func<CommandPointEvent, Fin<Unit>>>.None,
            },
            PointEventPhases = left.PointEventPhases | right.PointEventPhases,
            Gumball = Pick(left: left.Gumball, right: right.Gumball),
            OptionList = left.OptionList + right.OptionList,
            PointMode = Pick(left: left.PointMode, right: right.PointMode),
            TransformMode = Pick(left: left.TransformMode, right: right.TransformMode),
            DonorSelection = Pick(left: left.DonorSelection, right: right.DonorSelection),
            ScalarMode = Pick(left: left.ScalarMode, right: right.ScalarMode),
            LimitsMode = Pick(left: left.LimitsMode, right: right.LimitsMode),
            BoxMode = Pick(left: left.BoxMode, right: right.BoxMode),
            ObjectSelection = Pick(left: left.ObjectSelection, right: right.ObjectSelection),
            PromptText = Pick(left: left.PromptText, right: right.PromptText),
            ColorSeed = Pick(left: left.ColorSeed, right: right.ColorSeed),
            BoolLabels = Pick(left: left.BoolLabels, right: right.BoolLabels),
            FullFrameRedrawDuringGet = left.FullFrameRedrawDuringGet || right.FullFrameRedrawDuringGet,
            IsLiteralText = left.IsLiteralText || right.IsLiteralText,
            AcceptModes = left.AcceptModes | right.AcceptModes,
        };
    }

    internal static CommandInputPolicy Merge(Seq<CommandInputPolicy> policies) =>
        policies.Fold(Empty, static (state, policy) => state + policy);

    internal Fin<Unit> Apply<TGetter>(TGetter getter) where TGetter : GetBaseClass =>
        from valid in Optional(getter).ToFin(Fail: Op.Of(name: nameof(CommandInputPolicy)).InvalidInput())
        from applied in Applies.TraverseM(action => UI.RhinoUi.Protect(valid: () => { action(obj: valid); return Fin.Succ(value: unit); })).As()
        from result in valid switch {
            GetObject objects => ApplyObject(getter: objects),
            GetPoint point => ApplyPoint(getter: point),
            _ => Fin.Succ(value: unit),
        }
        select result;

    private static CommandInputPolicy Action(Action<GetBaseClass> apply) => Empty with { Applies = Seq(apply) };
    private bool Accepts(CommandInputAccept mode) => (AcceptModes & mode) == mode;
    private static Option<T> Pick<T>(Option<T> left, Option<T> right) => right | left;

    private Fin<Unit> ApplyPoint(GetPoint getter) {
        return PointMode.Map(spec => {
            _ = spec.Cursor.Iter(cursor => getter.SetCursor(cursor));
            _ = spec.ObjectSnapCursors.Iter(enabled => getter.EnableObjectSnapCursors(enable: enabled));
            _ = spec.PermitOrthoSnap.Iter(enabled => getter.PermitOrthoSnap(permit: enabled));
            _ = spec.PermitObjectSnap.Iter(enabled => getter.PermitObjectSnap(permit: enabled));
            _ = spec.DynamicDrawColor.Iter(color => getter.DynamicDrawColor = color);
            _ = spec.CurveSnapTangent.Iter(value => getter.EnableCurveSnapTangentBar(drawTangentBarAtSnapPoint: value.Enabled, drawEndPoints: value.Ends));
            _ = spec.CurveSnapPerp.Iter(value => getter.EnableCurveSnapPerpBar(drawPerpBarAtSnapPoint: value.Enabled, drawEndPoints: value.Ends));
            _ = spec.CurveSnapArrow.Iter(value => getter.EnableCurveSnapArrow(drawDirectionArrowAtSnapPoint: value.Enabled, reverseArrow: value.Reverse));
            getter.PermitConstraintOptions(spec.PermitConstraintOptions);
            getter.EnableNoRedrawOnExit(spec.NoRedrawOnExit);
            getter.PermitFromOption(spec.PermitFromOption);
            getter.PermitTabMode(spec.PermitTabMode);
            getter.PermitElevatorMode(spec.PermitElevatorMode);
            getter.EnableSnapToCurves(enable: spec.SnapToCurves);
            _ = Op.SideWhen(spec.ClearConstraints, getter.ClearConstraints);
            _ = Op.SideWhen(!spec.SnapPoints.IsEmpty, () => getter.AddSnapPoints(points: [.. spec.SnapPoints]));
            _ = Op.SideWhen(!spec.ConstructionPoints.IsEmpty, () => getter.AddConstructionPoints(points: [.. spec.ConstructionPoints]));
            _ = spec.BasePoint.Iter(point => {
                getter.SetBasePoint(point, showDistanceInStatusBar: true);
                _ = Op.SideWhen(spec.DrawLineFromBasePoint, () => getter.DrawLineFromPoint(point, showDistanceInStatusBar: true));
            });
            return (spec.DistanceFromBasePoint.Case switch {
                double value when RhinoMath.IsValidDouble(x: value) && value >= 0.0 => Fin.Succ(value: Op.Side(() => getter.ConstrainDistanceFromBasePoint(distance: value))),
                double => Fin.Fail<Unit>(error: Op.Of().InvalidInput()),
                _ => Fin.Succ(value: unit),
            }).Bind(_ => spec.Constraints.TraverseM(constraint => constraint.Apply(getter: getter)).As().Map(static _ => unit));
        }).IfNone(Fin.Succ(value: unit));
    }

    private Fin<Unit> ApplyObject(GetObject getter) =>
        from valid in Optional(getter).ToFin(Fail: Op.Of().InvalidInput())
        from applied in ObjectSelection.IfNone(CommandObjectSelection.Default).Apply(getter: valid)
        select unit;
}

public sealed record CommandInputRequest<T> {
    private readonly Func<CommandInput, Fin<CommandInputOutcome<T>>> run;
    private readonly Func<Seq<CommandInputPolicy>, CommandInputRequest<T>> rebind;
    private readonly Func<CommandInput, string, Fin<CommandInputOutcome<T>>> scripted;

    internal CommandInputRequest(Func<CommandInput, Fin<CommandGet<T>>> run, Func<Seq<CommandInputPolicy>, CommandInputRequest<T>>? rebind = null, Func<CommandInput, string, Fin<CommandInputOutcome<T>>>? scripted = null) {
        this.run = input => run(arg: input).Map(CommandInputOutcome<T>.Of);
        this.rebind = rebind ?? (_ => this);
        this.scripted = scripted ?? ((_, _) => Fin.Fail<CommandInputOutcome<T>>(error: Op.Of(name: nameof(CommandInputRequest<>)).InvalidInput()));
    }

    internal CommandInputRequest(Func<CommandInput, Fin<CommandInputOutcome<T>>> runEvent, Func<Seq<CommandInputPolicy>, CommandInputRequest<T>>? rebind, Func<CommandInput, string, Fin<CommandInputOutcome<T>>> scripted) {
        run = runEvent;
        this.rebind = rebind ?? (_ => this);
        this.scripted = scripted;
    }

    internal Fin<CommandGet<T>> Run(CommandInput input) =>
        RunEvent(input: input).Bind(static inputEvent => inputEvent.ToGet());

    internal Fin<CommandInputOutcome<T>> RunEvent(CommandInput input) =>
        run(arg: input);

    internal Fin<CommandInputOutcome<T>> Script(CommandInput input, string token) =>
        scripted(arg1: input, arg2: token);

    // a caller swapping the option SET here must re-validate any carried CommandOptionValue against the new set; rebind does not reconcile a key that no longer indexes a live option, so a stale selection would re-surface
    internal CommandInputRequest<T> With(Seq<CommandInputPolicy> policies) =>
        rebind(arg: policies);
}

public readonly record struct CommandPoint(
    Option<Point3d> Point,
    Option<DrawingPoint> WindowPoint,
    Option<RhinoView> View,
    Option<CommandSelection.Reference> Reference,
    Option<double> NumberPreview,
    global::Rhino.ApplicationSettings.OsnapModes Osnap,
    Option<Point3d> BasePoint,
    Option<(RhinoViewport Viewport, Plane Plane)> PlanarConstraint,
    Seq<Point3d> SnapPoints,
    Seq<Point3d> ConstructionPoints,
    Option<PickHit> Hit) {
    internal static CommandPoint Of(GetPoint getter, GetResult raw) {
        RhinoViewport? viewport = getter.View()?.ActiveViewport;
        Option<(RhinoViewport Viewport, Plane Plane)> constraint = viewport switch {
            RhinoViewport value when getter.GetPlanarConstraint(vp: ref value, plane: out Plane plane) => Some((Viewport: value, Plane: plane)),
            _ => Option<(RhinoViewport Viewport, Plane Plane)>.None,
        };
        using ObjRef? reference = getter.PointOnObject();
        return new(Point: raw is GetResult.Point ? Some(getter.Point()) : Option<Point3d>.None, WindowPoint: raw is GetResult.Point or GetResult.Point2d ? Some(getter.Point2d()) : Option<DrawingPoint>.None, View: Optional(getter.View()), Reference: CommandSelection.Reference.Of(reference: Optional(reference)), NumberPreview: getter.NumberPreview(number: out double number) ? Some(number) : Option<double>.None, Osnap: getter.OsnapEventType, BasePoint: getter.TryGetBasePoint(out Point3d basePoint) ? Some(basePoint) : Option<Point3d>.None, PlanarConstraint: constraint, SnapPoints: toSeq(getter.GetSnapPoints()), ConstructionPoints: toSeq(getter.GetConstructionPoints()), Hit: PickHit.Of(reference: Optional(reference)));
    }
}

[Union(SwitchMapStateParameterName = "getter")]
public abstract partial record CommandPointConstraint {
    private CommandPointConstraint() { }

    public sealed record OnSegment(Point3d From, Point3d To) : CommandPointConstraint;
    public sealed record OnLine(Line Value) : CommandPointConstraint;
    public sealed record OnArc(Arc Value) : CommandPointConstraint;
    public sealed record OnCircle(Circle Value) : CommandPointConstraint;
    public sealed record OnPlane(Plane Value, bool AllowElevator = true) : CommandPointConstraint;
    public sealed record OnSphere(Sphere Value) : CommandPointConstraint;
    public sealed record OnCylinder(Cylinder Value) : CommandPointConstraint;
    public sealed record OnCurve(Curve Value, bool AllowPickingPointOffObject = false) : CommandPointConstraint;
    public sealed record OnSurface(Surface Value, bool AllowPickingPointOffObject = false) : CommandPointConstraint;
    public sealed record OnBrep(Brep Value, int WireDensity = 1, int FaceIndex = -1, bool AllowPickingPointOffObject = false) : CommandPointConstraint;
    public sealed record OnMesh(Mesh Value, bool AllowPickingPointOffObject = false) : CommandPointConstraint;
    // throughBasePoint anchors the CPlane to the base point; the native API has no path to DISABLE CPlane confinement
    public sealed record OnConstructionPlane(bool ThroughBasePoint = true) : CommandPointConstraint;
    public sealed record OnTargetPlane : CommandPointConstraint;
    public sealed record OnCPlaneIntersection(Plane Value) : CommandPointConstraint;

    internal Fin<Unit> Apply(GetPoint getter) =>
        Switch(
            getter,
            onSegment: static (g, c) => Op.Of().Confirm(success: g.Constrain(from: c.From, to: c.To)),
            onLine: static (g, c) => Op.Of().Confirm(success: g.Constrain(line: c.Value)),
            onArc: static (g, c) => Op.Of().Confirm(success: g.Constrain(arc: c.Value)),
            onCircle: static (g, c) => Op.Of().Confirm(success: g.Constrain(circle: c.Value)),
            onPlane: static (g, c) => Op.Of().Confirm(success: g.Constrain(plane: c.Value, allowElevator: c.AllowElevator)),
            onSphere: static (g, c) => Op.Of().Confirm(success: g.Constrain(sphere: c.Value)),
            onCylinder: static (g, c) => Op.Of().Confirm(success: g.Constrain(cylinder: c.Value)),
            onCurve: static (g, c) => Op.Of().Confirm(success: g.Constrain(curve: c.Value, allowPickingPointOffObject: c.AllowPickingPointOffObject)),
            onSurface: static (g, c) => Op.Of().Confirm(success: g.Constrain(surface: c.Value, allowPickingPointOffObject: c.AllowPickingPointOffObject)),
            onBrep: static (g, c) => Op.Of().Confirm(success: g.Constrain(brep: c.Value, wireDensity: c.WireDensity, faceIndex: c.FaceIndex, allowPickingPointOffObject: c.AllowPickingPointOffObject)),
            onMesh: static (g, c) => Op.Of().Confirm(success: g.Constrain(mesh: c.Value, allowPickingPointOffObject: c.AllowPickingPointOffObject)),
            onConstructionPlane: static (g, c) => Op.Of().Confirm(success: g.ConstrainToConstructionPlane(throughBasePoint: c.ThroughBasePoint)),
            onTargetPlane: static (g, _) => Fin.Succ(value: Op.Side(g.ConstrainToTargetPlane)),
            onCPlaneIntersection: static (g, c) => Op.Of().Confirm(success: g.ConstrainToVirtualCPlaneIntersection(plane: c.Value)));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PointerState(PointerButtons Buttons, PointerModifiers Modifiers) {
    public bool IsDrag => Buttons != PointerButtons.None;
}

[Union]
public abstract partial record CommandPointPayload {
    private CommandPointPayload() { }
    public sealed record Mouse(GetPointMouseEventArgs Value) : CommandPointPayload;
    public sealed record Draw(GetPointDrawEventArgs Value) : CommandPointPayload;
    public sealed record PostDraw(DrawEventArgs Value) : CommandPointPayload;
    public Option<Point3d> Point => Switch(mouse: static args => Some(args.Value.Point), draw: static args => Some(args.Value.CurrentPoint), postDraw: static _ => Option<Point3d>.None);
    public Option<DrawingPoint> WindowPoint => Switch(mouse: static args => Some(args.Value.WindowPoint), draw: static _ => Option<DrawingPoint>.None, postDraw: static _ => Option<DrawingPoint>.None);
    public Option<RhinoViewport> Viewport => Switch(mouse: static args => Some(args.Value.Viewport), draw: static args => Some(args.Value.Viewport), postDraw: static args => Some(args.Value.Viewport));
    public Option<DisplayPipeline> Display => Switch(mouse: static _ => Option<DisplayPipeline>.None, draw: static args => Some(args.Value.Display), postDraw: static args => Some(args.Value.Display));
    public Option<PointerState> Pointer => Switch(mouse: static args => Some(new PointerState(Buttons: (args.Value.LeftButtonDown ? PointerButtons.Left : PointerButtons.None) | (args.Value.RightButtonDown ? PointerButtons.Right : PointerButtons.None) | (args.Value.MiddleButtonDown ? PointerButtons.Middle : PointerButtons.None), Modifiers: (args.Value.ShiftKeyDown ? PointerModifiers.Shift : PointerModifiers.None) | (args.Value.ControlKeyDown ? PointerModifiers.Control : PointerModifiers.None))), draw: static _ => Option<PointerState>.None, postDraw: static _ => Option<PointerState>.None);
}

public readonly record struct CommandPointEvent(
    CommandPointEventPhase Phase,
    RhinoDoc Document,
    GetPoint Getter,
    CommandPointPayload Payload,
    Option<UiGumball> Gumball) {
    public Option<Point3d> Point => Payload.Point;
    public Option<DrawingPoint> WindowPoint => Payload.WindowPoint;
    public Option<RhinoViewport> Viewport => Payload.Viewport;
    public Option<DisplayPipeline> Display => Payload.Display;
    public Option<PointerState> Pointer => Payload.Pointer;

    public Option<UiGumballSnapshot> GumballSnapshot =>
        Gumball.Map(static value => value.Snapshot);

    public Fin<bool> PickGumball(PickContext pick) {
        CommandPointEvent current = this;
        return from active in current.ActiveGumball(op: nameof(PickGumball))
               from valid in Optional(pick).ToFin(Fail: Op.Of().InvalidInput())
               from picked in active.Pick(pick: valid, point: current.Getter)
               select picked;
    }

    public Fin<bool> UpdateGumball(Line worldLine) {
        CommandPointEvent current = this;
        return from active in current.ActiveGumball(op: nameof(UpdateGumball))
               from origin in current.Point.ToFin(Fail: Op.Of().InvalidInput())
               from _ in active.CheckKeys()
               from changed in active.Update(point: origin, line: worldLine)
               select changed;
    }

    public Fin<bool> UpdateGumball(Plane frame) =>
        from active in ActiveGumball(op: nameof(UpdateGumball))
        from changed in active.Update(frame: frame)
        select changed;

    public Fin<Unit> Preview(UiViewportPreview preview) {
        CommandPointEvent current = this;
        return from validPreview in Optional(preview).ToFin(Fail: Op.Of().InvalidInput())
               from viewport in current.Viewport.ToFin(Fail: Op.Of().InvalidInput())
               from display in current.Display.ToFin(Fail: Op.Of().InvalidInput())
               from _ in validPreview.Draw(context: new UI.UiPreviewContext(Document: current.Document, Phase: current.Phase is CommandPointEventPhase.DynamicDraw ? UI.OverlayPhase.Overlay : UI.OverlayPhase.PostDraw, Viewport: viewport, Display: display, Gumball: current.GumballSnapshot))
               select unit;
    }

    private Fin<UiGumball> ActiveGumball(string op) =>
        Gumball.ToFin(Fail: Op.Of(name: op).InvalidInput());
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed record CommandInput {
    internal CommandInput(RhinoDoc document, Context domain) {
        ArgumentNullException.ThrowIfNull(argument: document);
        ArgumentNullException.ThrowIfNull(argument: domain);
        Document = document;
        Domain = domain;
    }

    public RhinoDoc Document { get; }
    public Context Domain { get; }

    // ExitRhino must reach Result.ExitRhino, not Result.Cancel, or host shutdown is left undefined. Carried as a Rasm.Rhino-local sentinel Error keyed on GetResult.ExitRhino's native code (0x0FFFFFFF) so the typed Fin rail stays in-target; RasmCommand.FailureResult discriminates on the code.
    internal const int ExitRhinoCode = 268435455;
    internal static readonly Error ExitRhinoSignal = Error.New(ExitRhinoCode, "Rhino is shutting down.");

    public Fin<CommandGet<TValue>> Get<TValue>(CommandInputRequest<TValue> request) =>
        Optional(request).ToFin(Fail: Op.Of().InvalidInput()).Bind(valid => valid.Run(input: this));

    internal Fin<CommandInputOutcome<TValue>> GetEvent<TValue>(CommandInputRequest<TValue> request) =>
        Optional(request).ToFin(Fail: Op.Of().InvalidInput()).Bind(valid => valid.RunEvent(input: this));

    internal Fin<CommandInputOutcome<TValue>> Script<TValue>(CommandInputRequest<TValue> request, string token) =>
        Optional(request).ToFin(Fail: Op.Of().InvalidInput()).Bind(valid => valid.Script(input: this, token: token));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CommandInputs {
    public static CommandInputRequest<T> Get<T>(params CommandInputPolicy[] policies) {
        Seq<CommandInputPolicy> active = toSeq(policies);
        CommandInputPolicy policy = CommandInputPolicy.Merge(policies: active);
        CommandInputRequest<T> request = Request<T>(policy: policy, policies: active);
        return new CommandInputRequest<T>(
            runEvent: request.RunEvent,
            rebind: static values => Get<T>(policies: [.. values]),
            scripted: request.Script);
    }

    private readonly record struct CommandInputKind<T>(
        Seq<Type> Types,
        Func<CommandInputPolicy, Seq<CommandInputPolicy>, CommandInputRequest<T>> Create) {
        internal bool Accepts(Type type) =>
            Types.Exists(candidate => candidate == type);
    }

    private static CommandInputRequest<T> Request<T>(CommandInputPolicy policy, Seq<CommandInputPolicy> policies) =>
        Kinds<T>()
            .Find(kind => kind.Accepts(type: typeof(T)))
            .Map(kind => kind.Create(arg1: policy, arg2: policies))
            .IfNone(Invalid<T>(name: nameof(Get)));

    private static Seq<CommandInputKind<T>> Kinds<T>() => Seq(
        Kind(typeof(CommandSelection), static (policy, policies) => Objects<T>(policy: policy, policies: policies)),
        Kind(typeof(CommandOptionValue), static (policy, _) => Getter(policy: policy, create: static () => new GetOption(), receive: static getter => getter.Get(), project: static (_, _, _) => (Value: Option<T>.None, Trim: Option<CommandSelection.TrimResult>.None))),
        Kind([typeof(CommandPoint), typeof(Point3d), typeof(DrawingPoint)], static (policy, _) => Point<T>(policy: policy)),
        Kind(typeof(Transform), static (policy, _) => Transform<T>(policy: policy)),
        Kind(typeof(double), static (policy, _) => Scalar<T>(policy: policy)),
        Kind(typeof(string), static (policy, _) => Text<T>(policy: policy)),
        Kind(typeof(int), static (policy, _) => Number<GetInteger, int, T>(policy: policy, create: static () => new GetInteger(), receive: static getter => getter.Get(), current: static getter => getter.Number(), setLower: static (getter, value, strict) => getter.SetLowerLimit(value, strict), setUpper: static (getter, value, strict) => getter.SetUpperLimit(value, strict))),
        Kind(typeof(bool), static (policy, _) => Native(run: () => {
            bool value = false;
            (string off, string on) = policy.BoolLabels.IfNone(("No", "Yes"));
            Result result = RhinoGet.GetBool(
                prompt: policy.PromptText.IfNone(string.Empty),
                acceptNothing: policy.AcceptsNothing,
                offPrompt: off, onPrompt: on,
                boolValue: ref value);
            return (Result: result, Value: result == Result.Success ? Cast<T>(value) : Option<T>.None);
        })),
        Kind(typeof(Line), static (_, _) => Native<T, Line>(get: static (out value) => RhinoGet.GetLine(line: out value))),
        Kind(typeof(Polyline), static (_, _) => Native<T, Polyline>(get: static (out value) => RhinoGet.GetPolyline(polyline: out value))),
        Kind(typeof(Circle), static (_, _) => Native<T, Circle>(get: static (out value) => RhinoGet.GetCircle(circle: out value))),
        Kind(typeof(Arc), static (_, _) => Native<T, Arc>(get: static (out value) => RhinoGet.GetArc(arc: out value))),
        Kind(typeof(Plane), static (_, _) => Native<T, Plane>(get: static (out value) => RhinoGet.GetPlane(plane: out value))),
        Kind(typeof(Seq<Point3d>), static (policy, _) => Native(run: () => {
            string prompt = policy.PromptText.IfNone(string.Empty);
            Result result = string.IsNullOrWhiteSpace(value: prompt)
                ? RhinoGet.GetRectangle(corners: out Point3d[] corners)
                : RhinoGet.GetRectangle(firstPrompt: prompt, corners: out corners);
            return (Result: result, Value: result == Result.Success ? Cast<T>(toSeq(corners)) : Option<T>.None);
        })),
        Kind(typeof(Box), static (policy, _) => Native(run: () => {
            // BOUNDARY ADAPTER — RhinoGet.GetBox lacks nullable annotations but its no-prompt overload passes null, so absent prompts thread null through the null-forgiving boundary.
            CommandInputPolicy.BoxSpec spec = policy.BoxMode.IfNone(CommandInputPolicy.BoxSpec.Default);
            Result result = spec.IsDefault
                ? RhinoGet.GetBox(box: out Box value)
                : RhinoGet.GetBox(box: out value, mode: spec.Mode,
                    basePoint: spec.BasePoint.IfNone(Point3d.Unset),
                    prompt1: spec.Prompt1.Case as string,
                    prompt2: spec.Prompt2.Case as string,
                    prompt3: spec.Prompt3.Case as string);
            return (Result: result, Value: result == Result.Success ? Cast<T>(value) : Option<T>.None);
        })),
        Kind(typeof(Color), static (policy, _) => Native(run: () => {
            Color color = policy.ColorSeed.IfNone(Color.Empty);
            Result result = RhinoGet.GetColor(prompt: policy.PromptText.IfNone(string.Empty), acceptNothing: policy.AcceptsNothing, color: ref color);
            return (Result: result, Value: result == Result.Success ? Cast<T>(color) : Option<T>.None);
        })),
        Kind(typeof(RhinoView), static (policy, _) => Native(run: () => {
            Result result = RhinoGet.GetView(commandPrompt: policy.PromptText.IfNone(string.Empty), view: out RhinoView view);
            return (Result: result, Value: result == Result.Success ? Cast<T>(view) : Option<T>.None);
        })),
        Kind(typeof(RhinoViewport[]), static (policy, _) => Native(run: () => {
            Result result = RhinoGet.GetViewports(commandPrompt: policy.PromptText.IfNone(string.Empty), viewports: out RhinoViewport[] viewports);
            return (Result: result, Value: result == Result.Success ? Cast<T>(viewports) : Option<T>.None);
        })));

    private static CommandInputKind<T> Kind<T>(Type type, Func<CommandInputPolicy, Seq<CommandInputPolicy>, CommandInputRequest<T>> create) =>
        new(Types: Seq(type), Create: create);

    private static CommandInputKind<T> Kind<T>(Type[] types, Func<CommandInputPolicy, Seq<CommandInputPolicy>, CommandInputRequest<T>> create) =>
        new(Types: toSeq(types), Create: create);

    private static CommandInputRequest<T> Scalar<T>(CommandInputPolicy policy) =>
        !policy.IsLiteralText && policy.ScalarMode.Map(static scalar => scalar.Kind is CommandInputPolicy.ScalarKind.Number).IfNone(noneValue: true)
            ? Number<GetNumber, double, T>(policy: policy, create: static () => new GetNumber(), receive: static getter => getter.Get(), current: static getter => getter.Number(), setLower: static (getter, value, strict) => getter.SetLowerLimit(lowerLimit: value, strictlyGreaterThan: strict), setUpper: static (getter, value, strict) => getter.SetUpperLimit(upperLimit: value, strictlyLessThan: strict))
            : Text<T>(policy: policy);

    private static CommandInputRequest<T> Objects<T>(CommandInputPolicy policy, Seq<CommandInputPolicy> policies) =>
        policy.ObjectSelection.IfNone(CommandObjectSelection.Default) switch { { Minimum: < 0 } or { Maximum: < -1 } => Invalid<T>(name: nameof(Get)),
            CommandObjectSelection selection when selection.Maximum > 0 && selection.Maximum < selection.Minimum => Invalid<T>(name: nameof(Get)),
            CommandObjectSelection selection => Getter(
                policy: CommandInputPolicy.Merge(
                    policies: Seq(
                        selection.Minimum == 0 ? CommandInputPolicy.Accept(modes: CommandInputAccept.Nothing | CommandInputAccept.EnterWhenDone) : CommandInputPolicy.Empty,
                        selection.Maximum == 0 ? CommandInputPolicy.Accept(modes: CommandInputAccept.EnterWhenDone) : CommandInputPolicy.Empty) + policies),
                create: static () => new GetObject(),
                receive: getter => getter.GetMultiple(minimumNumber: selection.Minimum, maximumNumber: selection.Maximum),
                project: (source, getter, raw) => SelectionTrim(source: source, getter: getter, raw: raw, policy: selection).Case switch {
                    CommandSelection.TrimResult trim => (Value: trim.Require(policy: selection).ToOption().Bind(Cast<T>), Trim: Some(trim)),
                    _ => (Value: Option<T>.None, Trim: Option<CommandSelection.TrimResult>.None),
                },
                transition: static (getter, raw, selected) => raw is GetResult.Option ? ((Func<(bool Continue, Option<CommandOptionValue> Selected)>)(() => { getter.EnablePreSelect(enable: false, ignoreUnacceptablePreselectedObjects: true); return (Continue: false, Selected: selected); }))() : (Continue: false, Selected: selected)),
        };

    private static CommandInputRequest<T> Point<T>(CommandInputPolicy policy) {
        CommandInputPolicy.PointSpec spec = policy.PointMode.IfNone(CommandInputPolicy.DefaultPointSpec); bool twoDimensional = spec.TwoDimensional || typeof(T) == typeof(DrawingPoint);
        return Getter(
            policy: policy,
            create: static () => new GetPoint(),
            receive: getter => getter.Get(onMouseUp: spec.OnMouseUp, get2DPoint: twoDimensional),
            project: static (_, getter, raw) => (raw is GetResult.Point or GetResult.Point2d ? PointValue<T>(getter: getter, raw: raw) : Option<T>.None, Option<CommandSelection.TrimResult>.None));
    }

    private sealed class CommandTransformGetter(Func<RhinoViewport, Point3d, Transform> calculate) : GetTransform {
        public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) => Optional(calculate).Map(project => project(arg1: viewport, arg2: point)).IfNone(Transform.Identity);
    }

    private static CommandInputRequest<T> Transform<T>(CommandInputPolicy policy) =>
        Getter(policy: policy, create: () => {
            CommandTransformGetter getter = new(calculate: policy.TransformMode.IfNone(static (_, _) => global::Rhino.Geometry.Transform.Identity));
            // donor objects feed TransformObjectList so Rhino ghost-draws the live preview during the drag; without an entry + DisplayFeedbackEnabled the native getter renders nothing
            _ = policy.DonorSelection.Iter(selection => {
                _ = selection.Items.Iter(reference => {
                    // BOUNDARY ADAPTER — ObjRef is a native disposable; scope it to a single Add call
                    using ObjRef objref = reference.ObjRef(document: selection.Document);
                    getter.ObjectList.Add(objref: objref);
                });
                getter.ObjectList.DisplayFeedbackEnabled = true;
            });
            return getter;
        }, receive: static getter => getter.GetXform(),
            project: static (_, getter, _) => (getter.HaveTransform && getter.Transform.IsValid ? Cast<T>(getter.Transform) : Option<T>.None, Option<CommandSelection.TrimResult>.None));

    private static CommandInputRequest<T> Text<T>(CommandInputPolicy policy) =>
        Getter(
            policy: policy,
            create: static () => new GetString(),
            receive: policy.IsLiteralText ? static getter => getter.GetLiteralString() : static getter => getter.Get(),
            project: (source, getter, raw) => (raw is GetResult.String ? Parse<T>(input: source, text: getter.StringResult(), scalar: policy.ScalarMode, bounds: policy.LimitsMode).Bind(Cast<T>) : Option<T>.None, Option<CommandSelection.TrimResult>.None));

    private static CommandInputRequest<TOut> Number<TGetter, TValue, TOut>(
        CommandInputPolicy policy,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<TGetter, TValue> current,
        Action<TGetter, TValue, bool> setLower,
        Action<TGetter, TValue, bool> setUpper) where TGetter : GetBaseClass, IDisposable where TValue : IComparable<TValue> =>
        Getter(
            policy: policy,
            create: create,
            receive: receive,
            configure: getter => Limits(getter: getter, bounds: policy.LimitsMode, setLower: setLower, setUpper: setUpper),
            project: (_, getter, raw) => (raw is GetResult.Number ? Cast<TOut>(current(arg: getter)!) : Option<TOut>.None, Option<CommandSelection.TrimResult>.None));

    private static CommandInputRequest<T> Getter<TGetter, T>(
        CommandInputPolicy policy,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<CommandInput, TGetter, GetResult, (Option<T> Value, Option<CommandSelection.TrimResult> Trim)> project,
        Func<TGetter, Fin<Unit>>? configure = null,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)>? transition = null) where TGetter : GetBaseClass, IDisposable =>
        new(runEvent: input =>
            Optional(input).ToFin(Fail: Op.Of(name: nameof(Get)).InvalidInput()).Bind(valid =>
                UI.RhinoUi.Protect(valid: () => {
                    using TGetter getter = create();
                    Atom<Option<Error>> eventFault = Atom(Option<Error>.None);
                    using Subscription pointEvents = (getter is GetPoint point
                        ? BindPointEvents(document: valid.Document, getter: point, events: policy.PointEvent, phases: policy.PointEventPhases, gumball: policy.Gumball, fullFrameRedraw: policy.FullFrameRedrawDuringGet, fault: eventFault)
                        : Fin.Succ(value: Subscription.Nothing)).IfFail(Subscription.Nothing);
                    return
                        from configured in configure is Func<TGetter, Fin<Unit>> apply ? apply(arg: getter) : Fin.Succ(value: unit)
                        from applied in policy.Apply(getter: getter)
                        from result in CommandOption.Bind(options: policy.OptionList, getter: getter).Bind(scope => {
                            using CommandOption.Scope active = scope;
                            return ReadLoop(getter: getter, scope: active, receive: () => receive(arg: getter), project: (g, raw) => project(arg1: valid, arg2: g, arg3: raw), selected: Option<CommandOptionValue>.None, acceptUndo: policy.AcceptsUndo, transition: transition ?? (static (_, _, selected) => (Continue: false, Selected: selected)));
                        })
                        from checkedResult in eventFault.Value switch {
                            { IsSome: true, Case: Error error } => Fin.Fail<CommandInputOutcome<T>>(error: error),
                            _ => Fin.Succ(value: result),
                        }
                        select checkedResult;
                })),
            rebind: null, scripted: (input, token) => Script<T>(input: input, token: token, policy: policy));

    private static Fin<Subscription> BindPointEvents(RhinoDoc document, GetPoint getter, Option<Func<CommandPointEvent, Fin<Unit>>> events, CommandPointEventPhase phases, Option<UiGumball> gumball, bool fullFrameRedraw, Atom<Option<Error>> fault) =>
        events.Map(change => {
            CommandPointEventPhase activePhases = phases | (fullFrameRedraw ? CommandPointEventPhase.PostDrawObjects : CommandPointEventPhase.None);
            // a faulted user handler records the error; the post-loop eventFault check (the only honest abort lever — GetPoint exposes no native cancel) fails the rail after the live Get() returns
            Unit Apply(CommandPointEvent pointEvent) =>
                UI.RhinoUi.Protect(valid: () => change(arg: pointEvent)).Match(Succ: static _ => unit, Fail: error => fault.Swap(_ => Some(error)) switch { _ => unit });
            Subscription Sub<TArgs>(Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, CommandPointEventPhase phase, Func<TArgs, CommandPointPayload> project) =>
                Subscription.Attach<TArgs>(active: (activePhases & phase) == phase, subscribe: add, unsubscribe: remove, handle: args => {
                    _ = Apply(pointEvent: new CommandPointEvent(Phase: phase, Document: document, Getter: getter, Payload: project(arg: args), Gumball: gumball));
                    return Fin.Succ(value: unit);
                });
            bool postDraw = activePhases.HasFlag(CommandPointEventPhase.PostDrawObjects);
            getter.FullFrameRedrawDuringGet = postDraw;
            return Fin.Succ(value:
                Sub<GetPointMouseEventArgs>(h => getter.MouseMove += h, h => getter.MouseMove -= h, CommandPointEventPhase.MouseMove, static args => new CommandPointPayload.Mouse(Value: args))
                | Sub<GetPointMouseEventArgs>(h => getter.MouseDown += h, h => getter.MouseDown -= h, CommandPointEventPhase.MouseDown, static args => new CommandPointPayload.Mouse(Value: args))
                | Sub<GetPointDrawEventArgs>(h => getter.DynamicDraw += h, h => getter.DynamicDraw -= h, CommandPointEventPhase.DynamicDraw, static args => new CommandPointPayload.Draw(Value: args))
                | (postDraw
                    ? Sub<DrawEventArgs>(h => getter.PostDrawObjects += h, h => getter.PostDrawObjects -= h, CommandPointEventPhase.PostDrawObjects, static args => new CommandPointPayload.PostDraw(Value: args))
                    : Subscription.Nothing));
        }).IfNone(Fin.Succ(value: Subscription.Nothing));

    private static Fin<CommandInputOutcome<T>> ReadLoop<TGetter, T>(
        TGetter getter,
        CommandOption.Scope scope,
        Func<GetResult> receive,
        Func<TGetter, GetResult, (Option<T> Value, Option<CommandSelection.TrimResult> Trim)> project,
        Option<CommandOptionValue> selected,
        bool acceptUndo,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)> transition) where TGetter : GetBaseClass =>
        receive() switch {
            GetResult.Cancel => Fin.Succ(value: CommandInputOutcome<T>.Cancelled),
            GetResult.ExitRhino => Fin.Succ(value: CommandInputOutcome<T>.Exit),
            GetResult.Option => scope.Selected(getter: getter).Bind(option => transition(getter, GetResult.Option, Some(option)) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, project: project, selected: next, acceptUndo: acceptUndo, transition: transition),
                (false, Option<CommandOptionValue> next) => Read(getter: getter, raw: GetResult.Option, projected: project(getter, GetResult.Option), option: next),
            }),
            GetResult.Undo when acceptUndo => Read(getter: getter, raw: GetResult.Undo, projected: (Option<T>.None, Option<CommandSelection.TrimResult>.None), option: selected),
            // unconsumed undo (getter still live) is a recoverable re-prompt, not a fatal error
            GetResult.Undo => ReadLoop(getter: getter, scope: scope, receive: receive, project: project, selected: selected, acceptUndo: acceptUndo, transition: transition),
            GetResult raw and (GetResult.NoResult or GetResult.Nothing or GetResult.Miss or GetResult.Timeout) => Read(getter: getter, raw: raw, projected: (Option<T>.None, Option<CommandSelection.TrimResult>.None), option: selected),
            GetResult raw => transition(getter, raw, selected) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, project: project, selected: next, acceptUndo: acceptUndo, transition: transition),
                (false, Option<CommandOptionValue> next) => Read(getter: getter, raw: raw, projected: project(getter, raw), option: next),
            },
        };

    private static Fin<CommandInputOutcome<T>> Read<TGetter, T>(TGetter getter, GetResult raw, (Option<T> Value, Option<CommandSelection.TrimResult> Trim) projected, Option<CommandOptionValue> option) where TGetter : GetBaseClass {
        Option<T> value = projected.Value | option.Bind(static selected => selected is T selectedValue ? Some(selectedValue) : Option<T>.None);
        CommandGet<T> snapshot = CommandGet<T>.Of(getter: getter, raw: raw, value: value, option: option, selectionTrim: projected.Trim);
        bool requiresValue = raw is GetResult.Object or GetResult.Point or GetResult.Point2d or GetResult.Number or GetResult.String
            or GetResult.Color or GetResult.Rectangle2d or GetResult.Line2d or GetResult.Circle or GetResult.Plane
            or GetResult.Cylinder or GetResult.Sphere or GetResult.Angle or GetResult.Distance
            or GetResult.Direction or GetResult.Frame or GetResult.CustomMessage;
        bool hasValue = value.IsSome || projected.Trim.IsSome || (raw == GetResult.Option && option.IsSome) || snapshot.Snapshot.Has(raw: raw);
        return (requiresValue, hasValue) switch {
            (true, false) => Fin.Fail<CommandInputOutcome<T>>(error: Op.Of().InvalidResult()),
            _ => Fin.Succ(value: CommandInputOutcome<T>.Of(result: snapshot)),
        };
    }

    private static CommandInputRequest<T> Native<T>(Func<(Result Result, Option<T> Value)> run) =>
        new(input => Optional(input).ToFin(Fail: Op.Of().InvalidInput()).Bind(_ => UI.RhinoUi.Protect(valid: () => run() switch {
            (Result.Success, Option<T> value) => Fin.Succ(value: CommandGet<T>.Native(result: Result.Success, value: value)),
            (Result.ExitRhino, _) => Fin.Fail<CommandGet<T>>(error: CommandInput.ExitRhinoSignal),
            (Result.Cancel or Result.CancelModelessDialog, _) => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            (Result.Failure or Result.UnknownCommand, _) => Fin.Fail<CommandGet<T>>(error: Op.Of().InvalidResult()),
            (Result result, _) => Fin.Succ(value: CommandGet<T>.Native(result: result, value: Option<T>.None)),
        })), scripted: (input, token) => Script<T>(input: input, token: token));

    private delegate Result NativeGetter<TNative>(out TNative value);

    private static CommandInputRequest<T> Native<T, TNative>(NativeGetter<TNative> get) =>
        Native(run: () => {
            Result result = get(value: out TNative value);
            return (Result: result, Value: result == Result.Success ? Cast<T>(value!) : Option<T>.None);
        });

    private static Fin<CommandInputOutcome<T>> Script<T>(CommandInput input, string token, CommandInputPolicy? policy = null) =>
        from source in Optional(input).ToFin(Fail: Op.Of().InvalidInput())
        from text in Optional(token).ToFin(Fail: Op.Of().InvalidInput())
        let activePolicy = Optional(policy).IfNone(CommandInputPolicy.Empty)
        from options in CommandOption.Validate(options: activePolicy.OptionList, op: Op.Of())
        from result in CommandOption.Script(options: options, token: text).Case switch {
            CommandOptionValue option => Fin.Succ(value: CommandGet<T>.ScriptOption(option: option)),
            _ => from value in ScriptValue<T>(input: source, text: text, policy: activePolicy).ToFin(Fail: Op.Of().InvalidResult())
                 select CommandGet<T>.Native(result: Result.Success, value: Some(value)),
        }
        select CommandInputOutcome<T>.Of(result: result);

    private static Option<T> ScriptValue<T>(CommandInput input, string text, CommandInputPolicy policy) =>
        typeof(T) switch {
            Type t when t == typeof(string) => Cast<T>(text),
            Type t when t == typeof(bool) => new CommandToken(Raw: text).Bool().Bind(value => Cast<T>(value)),
            Type t when t == typeof(Color) => new CommandToken(Raw: text).Color().Bind(value => Cast<T>(value)),
            Type t when t == typeof(double) => Parse<T>(input: input, text: text, scalar: policy.ScalarMode, bounds: policy.LimitsMode).Bind(Cast<T>),
            Type t when t == typeof(int) => new CommandToken(Raw: text).Integer().Bind(value => policy.LimitsMode.Case switch { CommandInputPolicy.LimitSpec spec => spec.Accept(value: value).Bind(valid => Cast<T>(valid)), _ => Cast<T>(value) }),
            Type t when t == typeof(CommandSelection) => ScriptSelection<T>(input: input, text: text, policy: policy),
            Type t when t == typeof(CommandOptionValue) => Option<T>.None,
            _ => Option<T>.None,
        };

    private static CommandInputRequest<T> Invalid<T>(string name) => new(run: _ => Fin.Fail<CommandGet<T>>(error: Op.Of(name: name).InvalidInput()));

    private static Option<CommandSelection.TrimResult> SelectionTrim(CommandInput source, GetObject getter, GetResult raw, CommandObjectSelection policy) =>
        CommandSelection.FromGetter(document: source.Document, getter: getter, raw: raw).ToOption()
            .Bind(selection => selection.Trimmed(policy: policy).ToOption());

    private static Option<T> ScriptSelection<T>(CommandInput input, string text, CommandInputPolicy policy) =>
        (Guid.TryParse(input: text, result: out Guid id), id) switch {
            (true, Guid value) when value != Guid.Empty =>
                from selection in CommandSelection.FromObjects(
                    document: input.Document,
                    objectIds: Seq(value),
                    policy: policy.ObjectSelection.IfNone(CommandObjectSelection.Default))
                    .ToOption()
                from typed in Cast<T>(selection)
                select typed,
            _ => Option<T>.None,
        };

    private static Option<T> PointValue<T>(GetPoint getter, GetResult raw) =>
        (raw, typeof(T)) switch {
            (GetResult.Point, Type t) when t == typeof(Point3d) => Cast<T>(getter.Point()),
            (GetResult.Point or GetResult.Point2d, Type t) when t == typeof(DrawingPoint) => Cast<T>(getter.Point2d()),
            (GetResult.Point or GetResult.Point2d, _) => Cast<T>(CommandPoint.Of(getter: getter, raw: raw)),
            _ => Option<T>.None,
        };

    private static Option<object> Parse<T>(CommandInput input, string text, Option<CommandInputPolicy.Scalar> scalar, Option<CommandInputPolicy.LimitSpec> bounds) =>
        typeof(T) switch {
            Type t when t == typeof(string) => Optional(text).Map(static value => (object)value),
            Type t when t == typeof(double) => scalar.IfNone(new CommandInputPolicy.Scalar(Kind: default, LengthUnits: Option<UnitSystem>.None, AngleUnits: Option<AngleUnitSystem>.None)) switch {
                { LengthUnits.IsSome: true, AngleUnits.IsSome: true } => Option<object>.None,
                { Kind: CommandInputPolicy.ScalarKind.Length } => ParseLength(text: text, units: scalar.Bind(static value => value.LengthUnits).IfNone(input.Domain.Units)).Bind(value => Within(value: value, bounds: bounds)).Map(static value => (object)value),
                { Kind: CommandInputPolicy.ScalarKind.Angle } => scalar.Bind(static value => value.AngleUnits).Bind(units => ParseAngle(text: text, units: units)).Bind(value => Within(value: value, bounds: bounds)).Map(static value => (object)value),
                _ => ParseNumber(text: text).Bind(value => Within(value: value, bounds: bounds)).Map(static value => (object)value),
            },
            _ => Option<object>.None,
        };

    private static Option<TOut> Cast<TOut>(object value) => value is TOut typed ? Some(typed) : Option<TOut>.None;

    private static Option<double> ParseNumber(string text) { StringParserSettings results = StringParserSettings.DefaultParseSettings; return StringParser.ParseNumber(expression: text, max_count: text.Length, settings_in: StringParserSettings.DefaultParseSettings, settings_out: ref results, answer: out double value) == text.Length ? Some(value) : Option<double>.None; }

    private static Option<double> ParseLength(string text, UnitSystem units) {
        using LengthValue value = LengthValue.Create(s: text, ps: StringParserSettings.DefaultParseSettings, parsedAll: out bool parsedAll);
        return parsedAll && !value.IsUnset() ? Some(value.Length(units: units)) : Option<double>.None;
    }

    private static Option<double> ParseAngle(string text, AngleUnitSystem units) { StringParserSettings results = StringParserSettings.DefaultParseSettings; AngleUnitSystem parsed = AngleUnitSystem.None; return StringParser.ParseAngleExpession(expression: text, start_offset: 0, expression_length: text.Length, parse_settings_in: StringParserSettings.DefaultParseSettings, output_angle_unit_system: units, value_out: out double value, parse_results: ref results, parsed_unit_system: ref parsed) == text.Length ? Some(value) : Option<double>.None; }

    private static Option<double> Within(double value, Option<CommandInputPolicy.LimitSpec> bounds) =>
        bounds.Case switch {
            CommandInputPolicy.LimitSpec spec => spec.Accept(value: value),
            _ => Some(value),
        };

    private static Fin<Unit> Limits<TGetter, TValue>(TGetter getter, Option<CommandInputPolicy.LimitSpec> bounds, Action<TGetter, TValue, bool> setLower, Action<TGetter, TValue, bool> setUpper) where TGetter : GetBaseClass where TValue : IComparable<TValue> =>
        bounds.Case switch {
            CommandInputPolicy.LimitSpec b =>
                from projected in b.Project<TValue>(op: Op.Of())
                from valid in Optional(getter).ToFin(Fail: Op.Of().InvalidInput())
                select Op.Side(() => {
                    _ = projected.Lower.Iter(value => setLower(arg1: valid, arg2: value, arg3: b.StrictlyLower));
                    _ = projected.Upper.Iter(value => setUpper(arg1: valid, arg2: value, arg3: b.StrictlyUpper));
                }),
            _ => Fin.Succ(value: unit),
        };
}
