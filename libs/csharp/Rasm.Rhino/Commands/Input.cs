using System.Globalization;
using Color = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using Result = Rhino.Commands.Result;
using UiGumball = Rasm.Rhino.UI.UiGumball;
using UiGumballSnapshot = Rasm.Rhino.UI.UiGumballSnapshot;
using UiViewportPreview = Rasm.Rhino.UI.UiViewportPreview;

namespace Rasm.Rhino.Commands;

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
    Seq<Point3d> ConstructionPoints) {
    internal static CommandPoint Of(GetPoint getter, GetResult raw) {
        RhinoViewport? viewport = getter.View()?.ActiveViewport;
        Option<(RhinoViewport Viewport, Plane Plane)> constraint = viewport switch {
            RhinoViewport value when getter.GetPlanarConstraint(vp: ref value, plane: out Plane plane) => Some((Viewport: value, Plane: plane)),
            _ => Option<(RhinoViewport Viewport, Plane Plane)>.None,
        };
        return new(Point: raw is GetResult.Point ? Some(getter.Point()) : Option<Point3d>.None, WindowPoint: raw is GetResult.Point or GetResult.Point2d ? Some<DrawingPoint>(getter.Point2d()) : Option<DrawingPoint>.None, View: Optional(getter.View()), Reference: CommandSelection.Reference.Of(getter: getter), NumberPreview: getter.NumberPreview(number: out double number) ? Some(number) : Option<double>.None, Osnap: getter.OsnapEventType, BasePoint: getter.TryGetBasePoint(out Point3d basePoint) ? Some(basePoint) : Option<Point3d>.None, PlanarConstraint: constraint, SnapPoints: toSeq(getter.GetSnapPoints()), ConstructionPoints: toSeq(getter.GetConstructionPoints()));
    }
}

public readonly record struct CommandPointConstraint {
    private readonly Func<GetPoint, bool>? apply;

    private CommandPointConstraint(Func<GetPoint, bool> apply) =>
        this.apply = apply;

    public static CommandPointConstraint AlongLine(Line value) => new(getter => getter.Constrain(line: value));
    public static CommandPointConstraint AlongArc(Arc value) => new(getter => getter.Constrain(arc: value));
    public static CommandPointConstraint AlongCircle(Circle value) => new(getter => getter.Constrain(circle: value));
    public static CommandPointConstraint InPlane(Plane value, bool throughCursor = true) => new(getter => getter.Constrain(value, throughCursor));
    public static CommandPointConstraint OnSphere(Sphere value) => new(getter => getter.Constrain(sphere: value));
    public static CommandPointConstraint OnCylinder(Cylinder value) => new(getter => getter.Constrain(cylinder: value));
    public static CommandPointConstraint OnCurve(Curve value, bool allowPickingPointOffObject = false) => new(getter => getter.Constrain(curve: value, allowPickingPointOffObject: allowPickingPointOffObject));
    public static CommandPointConstraint OnSurface(Surface value, bool allowPickingPointOffObject = false) => new(getter => getter.Constrain(surface: value, allowPickingPointOffObject: allowPickingPointOffObject));
    public static CommandPointConstraint OnMesh(Mesh value, bool allowPickingPointOffObject = false) => new(getter => getter.Constrain(mesh: value, allowPickingPointOffObject: allowPickingPointOffObject));
    internal bool Apply(GetPoint getter) => Optional(apply).Map(valid => valid(arg: getter)).IfNone(false);
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
        new(View: Optional(getter.View()), WindowPoint: raw is GetResult.Point or GetResult.Point2d ? Some<DrawingPoint>(getter.Point2d()) : Option<DrawingPoint>.None, Number: raw is GetResult.Number ? Some(getter.Number()) : Option<double>.None, Text: raw is GetResult.String ? Optional(getter.StringResult()) : Option<string>.None, CustomMessage: raw is GetResult.CustomMessage ? Optional(getter.CustomMessage()) : Option<object>.None, Point: raw is GetResult.Point ? Some(getter.Point()) : Option<Point3d>.None, Vector: (raw, getter.Vector()) switch { (GetResult.Point, Vector3d vector) when vector.IsValid && !vector.IsTiny() => Some(vector), _ => Option<Vector3d>.None }, Color: raw is GetResult.Color ? Some(getter.Color()) : Option<Color>.None, PickRectangle: raw is GetResult.Object ? Some(getter.PickRectangle()) : Option<DrawingRectangle>.None, Rectangle2d: raw is GetResult.Rectangle2d ? Some(getter.Rectangle2d()) : Option<DrawingRectangle>.None, Line2d: raw is GetResult.Line2d ? toSeq(getter.Line2d()) : Seq<DrawingPoint>());

    internal bool Has(GetResult raw) => raw switch { GetResult.Number => Number.IsSome, GetResult.String => Text.IsSome, GetResult.CustomMessage => CustomMessage.IsSome, GetResult.Point => Point.IsSome || WindowPoint.IsSome || Vector.IsSome, GetResult.Point2d => WindowPoint.IsSome, GetResult.Color => Color.IsSome, GetResult.Object => PickRectangle.IsSome, GetResult.Rectangle2d => Rectangle2d.IsSome, GetResult.Line2d => !Line2d.IsEmpty, _ => false };
}

public readonly record struct CommandGet<T>(
    Option<GetResult> Raw,
    Result CommandResult,
    Option<T> Value,
    Option<CommandOptionValue> Option,
    bool GotDefault,
    CommandSnapshot Snapshot) {
    internal static CommandGet<T> Of(GetBaseClass getter, GetResult raw, Option<T> value, Option<CommandOptionValue> option) =>
        new(Raw: Some(raw), CommandResult: getter.CommandResult(), Value: value, Option: option, GotDefault: getter.GotDefault(), Snapshot: CommandSnapshot.Of(getter: getter, raw: raw));

    internal static CommandGet<T> Native(Result result, Option<T> value) =>
        new(Raw: Option<GetResult>.None, CommandResult: result, Value: value, Option: Option<CommandOptionValue>.None, GotDefault: false, Snapshot: CommandSnapshot.Empty);

    internal static CommandGet<T> ScriptOption(CommandOptionValue option) =>
        new(Raw: Some(GetResult.Option), CommandResult: Result.Success, Value: Option<T>.None, Option: Some(option), GotDefault: false, Snapshot: CommandSnapshot.Empty);

    public Option<TOut> As<TOut>() =>
        Value.Bind(static value => value is TOut typed ? Some(typed) : Option<TOut>.None);

    public bool IsUndo => Raw.Map(static raw => raw == GetResult.Undo).IfNone(false);
}

internal enum CommandInputEventKind { Value, Option, Undo, Nothing, NoResult, Miss, Timeout, Cancel, Exit }

internal readonly record struct CommandInputEvent<T>(CommandInputEventKind Kind, Option<CommandGet<T>> Result) {
    internal static CommandInputEvent<T> Of(CommandGet<T> result) =>
        new(
            Kind: result.Raw.Map(static raw => raw switch {
                GetResult.Option => CommandInputEventKind.Option,
                GetResult.Undo => CommandInputEventKind.Undo,
                GetResult.Nothing => CommandInputEventKind.Nothing,
                GetResult.NoResult => CommandInputEventKind.NoResult,
                GetResult.Miss => CommandInputEventKind.Miss,
                GetResult.Timeout => CommandInputEventKind.Timeout,
                _ => CommandInputEventKind.Value,
            }).IfNone(result.CommandResult switch {
                global::Rhino.Commands.Result.Nothing => CommandInputEventKind.Nothing,
                _ => CommandInputEventKind.Value,
            }),
            Result: Some(result));

    internal static CommandInputEvent<T> Cancelled { get; } =
        new(Kind: CommandInputEventKind.Cancel, Result: Option<CommandGet<T>>.None);

    internal static CommandInputEvent<T> Exit { get; } =
        new(Kind: CommandInputEventKind.Exit, Result: Option<CommandGet<T>>.None);

    internal Fin<CommandGet<T>> ToGet() =>
        Kind switch {
            CommandInputEventKind.Cancel or CommandInputEventKind.Exit => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            _ => Result.ToFin(Fail: Op.Of(name: nameof(CommandInputEvent<T>)).InvalidResult()),
        };
}

public sealed record CommandInputRequest<T> {
    private readonly Func<CommandInput, Fin<CommandInputEvent<T>>> run;
    private readonly Func<Seq<CommandInputPolicy>, CommandInputRequest<T>> rebind;
    private readonly Func<CommandInput, string, Fin<CommandInputEvent<T>>> scripted;

    internal CommandInputRequest(Func<CommandInput, Fin<CommandGet<T>>> run, Func<Seq<CommandInputPolicy>, CommandInputRequest<T>>? rebind = null, Func<CommandInput, string, Fin<CommandInputEvent<T>>>? scripted = null) {
        this.run = input => run(arg: input).Map(CommandInputEvent<T>.Of);
        this.rebind = rebind ?? (_ => this);
        this.scripted = scripted ?? ((_, _) => Fin.Fail<CommandInputEvent<T>>(error: Op.Of(name: nameof(CommandInputRequest<T>)).InvalidInput()));
    }

    internal CommandInputRequest(Func<CommandInput, Fin<CommandInputEvent<T>>> runEvent, Func<Seq<CommandInputPolicy>, CommandInputRequest<T>>? rebind, Func<CommandInput, string, Fin<CommandInputEvent<T>>> scripted) {
        run = runEvent;
        this.rebind = rebind ?? (_ => this);
        this.scripted = scripted;
    }

    internal Fin<CommandGet<T>> Run(CommandInput input) =>
        RunEvent(input: input).Bind(static inputEvent => inputEvent.ToGet());

    internal Fin<CommandInputEvent<T>> RunEvent(CommandInput input) =>
        run(arg: input);

    internal Fin<CommandInputEvent<T>> Script(CommandInput input, string token) =>
        scripted(arg1: input, arg2: token);

    internal CommandInputRequest<T> With(Seq<CommandInputPolicy> policies) =>
        rebind(arg: policies);
}

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
    TransparentCommands = 256
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
    private CommandInputPolicy(
        Seq<Action<GetBaseClass>> baseActions = default,
        Seq<Action<GetObject>> objectActions = default,
        Seq<Action<GetPoint>> pointActions = default,
        Option<Func<CommandPointEvent, Fin<Unit>>> pointEvents = default,
        Option<UiGumball> gumball = default,
        Seq<CommandOption> options = default,
        Option<PointSpec> point = default,
        Option<Func<RhinoViewport, Point3d, global::Rhino.Geometry.Transform>> transform = default,
        Option<Scalar> scalar = default,
        Option<LimitSpec> bounds = default,
        Option<BoxSpec> box = default,
        Option<CommandObjectSelection> objectSelection = default,
        Option<string> prompt = default,
        bool literalText = false,
        CommandInputAccept accept = CommandInputAccept.None) {
        BaseActions = baseActions; ObjectActions = objectActions; PointActions = pointActions; PointEvent = pointEvents; Gumball = gumball; OptionList = options; PointMode = point; TransformMode = transform; ScalarMode = scalar; LimitsMode = bounds; BoxMode = box; ObjectSelection = objectSelection; PromptText = prompt; IsLiteralText = literalText; AcceptModes = accept;
    }

    private Seq<Action<GetBaseClass>> BaseActions { get; }
    private Seq<Action<GetObject>> ObjectActions { get; }
    private Seq<Action<GetPoint>> PointActions { get; }
    internal Option<Func<CommandPointEvent, Fin<Unit>>> PointEvent { get; }
    internal Option<UiGumball> Gumball { get; }
    internal Seq<CommandOption> OptionList { get; }
    internal Option<PointSpec> PointMode { get; }
    internal Option<Func<RhinoViewport, Point3d, global::Rhino.Geometry.Transform>> TransformMode { get; }
    internal Option<Scalar> ScalarMode { get; }
    internal Option<LimitSpec> LimitsMode { get; }
    internal Option<BoxSpec> BoxMode { get; }
    internal Option<CommandObjectSelection> ObjectSelection { get; }
    internal Option<string> PromptText { get; }
    internal bool IsLiteralText { get; }
    internal CommandInputAccept AcceptModes { get; }
    internal bool AcceptsNothing => Accepts(mode: CommandInputAccept.Nothing);
    internal bool AcceptsUndo => Accepts(mode: CommandInputAccept.Undo);
    internal static CommandInputPolicy Empty { get; } = new();

    private static CommandInputPolicy Configure<TGetter>(Action<TGetter> apply) where TGetter : GetBaseClass =>
        (typeof(TGetter), Optional(apply).Case) switch {
            (Type type, Action<TGetter> action) when type == typeof(GetObject) => new(objectActions: Seq<Action<GetObject>>(getter => action((TGetter)(GetBaseClass)getter))),
            (Type type, Action<TGetter> action) when type == typeof(GetPoint) => new(pointActions: Seq<Action<GetPoint>>(getter => action((TGetter)(GetBaseClass)getter))),
            (_, Action<TGetter> action) => new(baseActions: Seq<Action<GetBaseClass>>(getter => _ = getter is TGetter typed ? ((Func<Unit>)(() => { action(typed); return unit; }))() : unit)),
            _ => Empty,
        };
    public static CommandInputPolicy Prompt(string value) => new(baseActions: Seq<Action<GetBaseClass>>(getter => getter.SetCommandPrompt(value)), prompt: Optional(value));
    public static CommandInputPolicy Default(object value) =>
        value switch {
            Point3d point => Configure<GetBaseClass>(apply: getter => getter.SetDefaultPoint(point: point)),
            double number => Configure<GetBaseClass>(apply: getter => getter.SetDefaultNumber(number)),
            int integer => Configure<GetBaseClass>(apply: getter => getter.SetDefaultInteger(integer)),
            string text => Configure<GetBaseClass>(apply: getter => getter.SetDefaultString(text)),
            Color color => Configure<GetBaseClass>(apply: getter => getter.SetDefaultColor(color)),
            _ => Empty,
        };
    public static CommandInputPolicy Accept(CommandInputAccept modes, bool acceptZero = true) =>
        new(
            baseActions: toSeq(new (CommandInputAccept Mode, Action<GetBaseClass> Apply)[] {
                (CommandInputAccept.Nothing, static getter => getter.AcceptNothing(enable: true)),
                (CommandInputAccept.Undo, static getter => getter.AcceptUndo(enable: true)),
                (CommandInputAccept.EnterWhenDone, static getter => getter.AcceptEnterWhenDone(enable: true)),
                (CommandInputAccept.Point, static getter => getter.AcceptPoint(enable: true)),
                (CommandInputAccept.Color, static getter => getter.AcceptColor(enable: true)),
                (CommandInputAccept.Text, static getter => getter.AcceptString(enable: true)),
                (CommandInputAccept.CustomMessage, static getter => getter.AcceptCustomMessage(enable: true)),
                (CommandInputAccept.TransparentCommands, static getter => getter.EnableTransparentCommands(enable: true)),
                (CommandInputAccept.Number, getter => getter.AcceptNumber(enable: true, acceptZero: acceptZero)),
            }).Filter(row => (modes & row.Mode) == row.Mode).Map(static row => row.Apply),
            accept: modes);
    public static CommandInputPolicy TransparentCommands(bool enabled = true) => Configure<GetBaseClass>(apply: getter => getter.EnableTransparentCommands(enable: enabled));
    public static CommandInputPolicy Options(params CommandOption[] values) => new(options: toSeq(values));
    public static CommandInputPolicy PointEvents(Func<CommandPointEvent, Fin<Unit>> change) =>
        Optional(change).Map(apply => new CommandInputPolicy(pointEvents: Some(apply))).IfNone(Empty);
    public static CommandInputPolicy PointGumball(UiGumball gumball) =>
        Optional(gumball).Map(active => new CommandInputPolicy(gumball: Some(active), pointEvents: Some<Func<CommandPointEvent, Fin<Unit>>>(static _ => Fin.Succ(value: unit)))).IfNone(Empty);
    public static CommandInputPolicy Transform(Func<RhinoViewport, Point3d, global::Rhino.Geometry.Transform> calculate) =>
        Optional(calculate).Map(project => new CommandInputPolicy(transform: Some(project))).IfNone(Empty);
    public static CommandInputPolicy Objects(CommandObjectSelection selection) =>
        Optional(selection)
            .Map(static valid => new CommandInputPolicy(objectSelection: Some(valid)))
            .IfNone(Empty);

    public static CommandInputPolicy Point(
        bool onMouseUp = false,
        bool twoDimensional = false,
        Option<global::Rhino.UI.CursorStyle> cursor = default,
        Option<bool> objectSnapCursors = default,
        Option<Point3d> basePoint = default,
        bool drawLineFromBasePoint = false,
        bool snapToCurves = false,
        bool permitConstraintOptions = true,
        bool noRedrawOnExit = false,
        bool permitFromOption = true,
        bool permitTabMode = true,
        int permitElevatorMode = 0,
        IEnumerable<Point3d>? snapPoints = null,
        IEnumerable<Point3d>? constructionPoints = null,
        IEnumerable<CommandPointConstraint>? constraints = null) =>
        new(point: Some(DefaultPointSpec with {
            OnMouseUp = onMouseUp,
            TwoDimensional = twoDimensional,
            Cursor = cursor,
            ObjectSnapCursors = objectSnapCursors,
            BasePoint = basePoint,
            DrawLineFromBasePoint = drawLineFromBasePoint,
            SnapToCurves = snapToCurves,
            PermitConstraintOptions = permitConstraintOptions,
            NoRedrawOnExit = noRedrawOnExit,
            PermitFromOption = permitFromOption,
            PermitTabMode = permitTabMode,
            PermitElevatorMode = permitElevatorMode,
            SnapPoints = Optional(snapPoints).Map(static points => toSeq(points)).IfNone(Seq<Point3d>()),
            ConstructionPoints = Optional(constructionPoints).Map(static points => toSeq(points)).IfNone(Seq<Point3d>()),
            Constraints = Optional(constraints).Map(static values => toSeq(values)).IfNone(Seq<CommandPointConstraint>()),
        }));
    public static CommandInputPolicy Bounds<T>(Option<T> lower = default, Option<T> upper = default, bool strictlyLower = false, bool strictlyUpper = false) where T : IComparable<T> => new(bounds: Some(new LimitSpec(Lower: lower.Map(static value => (object)value), Upper: upper.Map(static value => (object)value), StrictlyLower: strictlyLower, StrictlyUpper: strictlyUpper)));
    public static CommandInputPolicy Bounds(CommandScalarBounds bounds) => new(bounds: Some(new LimitSpec(Lower: bounds.Lower.Map(static value => (object)value), Upper: bounds.Upper.Map(static value => (object)value), StrictlyLower: false, StrictlyUpper: false)));
    public static CommandInputPolicy Number() => new(scalar: Some(new Scalar(Kind: ScalarKind.Number, LengthUnits: Option<UnitSystem>.None, AngleUnits: Option<AngleUnitSystem>.None)));
    public static CommandInputPolicy Number<T>(Option<T> lower = default, Option<T> upper = default, bool strictlyLower = false, bool strictlyUpper = false) where T : IComparable<T> => Bounds(lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper) + Number();
    public static CommandInputPolicy Length(Option<UnitSystem> units = default) => new(scalar: Some(new Scalar(Kind: ScalarKind.Length, LengthUnits: units, AngleUnits: Option<AngleUnitSystem>.None)));
    public static CommandInputPolicy Angle(AngleUnitSystem units) => new(scalar: Some(new Scalar(Kind: ScalarKind.Angle, LengthUnits: Option<UnitSystem>.None, AngleUnits: Some(units))));
    public static CommandInputPolicy LiteralText() => new(literalText: true);
    public static CommandInputPolicy Box(GetBoxMode mode = GetBoxMode.All, Point3d? basePoint = null, string? prompt1 = null, string? prompt2 = null, string? prompt3 = null) => new(box: Some(new BoxSpec(Mode: mode, BasePoint: basePoint, Prompt1: prompt1, Prompt2: prompt2, Prompt3: prompt3)));

    public static CommandInputPolicy operator +(CommandInputPolicy left, CommandInputPolicy right) => Add(left: left, right: right);

    public static CommandInputPolicy Add(CommandInputPolicy left, CommandInputPolicy right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        ArgumentNullException.ThrowIfNull(argument: right);
        return new(baseActions: left.BaseActions + right.BaseActions, objectActions: left.ObjectActions + right.ObjectActions, pointActions: left.PointActions + right.PointActions, pointEvents: Compose(left.PointEvent, right.PointEvent), gumball: Pick(left.Gumball, right.Gumball), options: left.OptionList + right.OptionList, point: Pick(left.PointMode, right.PointMode), transform: Pick(left.TransformMode, right.TransformMode), scalar: Pick(left.ScalarMode, right.ScalarMode), bounds: Pick(left.LimitsMode, right.LimitsMode), box: Pick(left.BoxMode, right.BoxMode), objectSelection: Pick(left.ObjectSelection, right.ObjectSelection), prompt: Pick(left.PromptText, right.PromptText), literalText: left.IsLiteralText || right.IsLiteralText, accept: left.AcceptModes | right.AcceptModes);
    }

    internal static CommandInputPolicy Merge(Seq<CommandInputPolicy> policies) =>
        policies.Fold(Empty, static (state, policy) => state + policy);

    internal Fin<Unit> Apply<TGetter>(TGetter getter) where TGetter : GetBaseClass =>
        Optional(getter).ToFin(Fail: Op.Of(name: nameof(CommandInputPolicy)).InvalidInput()).Bind(valid => {
            _ = BaseActions.Iter(action => action(obj: valid));
            return valid switch {
                GetObject objects => ApplyObject(getter: objects),
                GetPoint point => ApplyPoint(getter: point),
                _ => Fin.Succ(value: unit),
            };
        });

    private bool Accepts(CommandInputAccept mode) => (AcceptModes & mode) == mode;
    private static Option<T> Pick<T>(Option<T> left, Option<T> right) => right | left;
    private static Option<Func<CommandPointEvent, Fin<Unit>>> Compose(Option<Func<CommandPointEvent, Fin<Unit>>> left, Option<Func<CommandPointEvent, Fin<Unit>>> right) =>
        (left.Case, right.Case) switch {
            (Func<CommandPointEvent, Fin<Unit>> a, Func<CommandPointEvent, Fin<Unit>> b) => Some<Func<CommandPointEvent, Fin<Unit>>>(pointEvent => a(arg: pointEvent).Bind(_ => b(arg: pointEvent))),
            (_, Func<CommandPointEvent, Fin<Unit>> b) => Some(b),
            (Func<CommandPointEvent, Fin<Unit>> a, _) => Some(a),
            _ => Option<Func<CommandPointEvent, Fin<Unit>>>.None,
        };
    internal enum ScalarKind { Number, Length, Angle }
    internal sealed record PointSpec(bool OnMouseUp, bool TwoDimensional, Option<global::Rhino.UI.CursorStyle> Cursor, Option<bool> ObjectSnapCursors, Option<Point3d> BasePoint, bool DrawLineFromBasePoint, bool SnapToCurves, bool PermitConstraintOptions, bool NoRedrawOnExit, bool PermitFromOption, bool PermitTabMode, int PermitElevatorMode, Seq<Point3d> SnapPoints, Seq<Point3d> ConstructionPoints, Seq<CommandPointConstraint> Constraints);
    internal static PointSpec DefaultPointSpec { get; } = new(OnMouseUp: false, TwoDimensional: false, Cursor: Option<global::Rhino.UI.CursorStyle>.None, ObjectSnapCursors: Option<bool>.None, BasePoint: Option<Point3d>.None, DrawLineFromBasePoint: false, SnapToCurves: false, PermitConstraintOptions: true, NoRedrawOnExit: false, PermitFromOption: true, PermitTabMode: true, PermitElevatorMode: 0, SnapPoints: Seq<Point3d>(), ConstructionPoints: Seq<Point3d>(), Constraints: Seq<CommandPointConstraint>());
    internal sealed record Scalar(ScalarKind Kind, Option<UnitSystem> LengthUnits, Option<AngleUnitSystem> AngleUnits);
    internal sealed record LimitSpec(Option<object> Lower, Option<object> Upper, bool StrictlyLower, bool StrictlyUpper) {
        internal Fin<(Option<TValue> Lower, Option<TValue> Upper)> Project<TValue>(Op op) where TValue : IComparable<TValue> => (Lower.Bind(ScalarBound<TValue>), Upper.Bind(ScalarBound<TValue>)) switch { (Option<TValue> lower, _) when Lower.IsSome && !lower.IsSome => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: op.InvalidInput()), (_, Option<TValue> upper) when Upper.IsSome && !upper.IsSome => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: op.InvalidInput()), (Option<TValue> lower, Option<TValue> upper) when lower.Case is TValue left && upper.Case is TValue right && left.CompareTo(other: right) > 0 => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: op.InvalidInput()), (Option<TValue> lower, Option<TValue> upper) when lower.Case is TValue left && upper.Case is TValue right && left.CompareTo(other: right) == 0 && (StrictlyLower || StrictlyUpper) => Fin.Fail<(Option<TValue> Lower, Option<TValue> Upper)>(error: op.InvalidInput()), (Option<TValue> lower, Option<TValue> upper) => Fin.Succ(value: (Lower: lower, Upper: upper)) };
        internal Option<TValue> Accept<TValue>(TValue value) where TValue : IComparable<TValue> =>
            Project<TValue>(op: Op.Of(name: nameof(LimitSpec))).ToOption().Bind(bounds => bounds switch {
                (Option<TValue> lower, _) when lower.Case is TValue lo && (StrictlyLower ? value.CompareTo(other: lo) <= 0 : value.CompareTo(other: lo) < 0) => Option<TValue>.None,
                (_, Option<TValue> upper) when upper.Case is TValue hi && (StrictlyUpper ? value.CompareTo(other: hi) >= 0 : value.CompareTo(other: hi) > 0) => Option<TValue>.None,
                _ => Some(value),
            });

        private static Option<TValue> ScalarBound<TValue>(object value) where TValue : IComparable<TValue> { try { return value switch { TValue typed => Some(typed), IConvertible convertible when typeof(TValue) == typeof(double) => Convert.ToDouble(value: convertible, provider: CultureInfo.InvariantCulture) switch { double number when double.IsFinite(d: number) => Some((TValue)(object)number), _ => Option<TValue>.None }, IConvertible convertible when typeof(TValue) == typeof(int) => Convert.ToDouble(value: convertible, provider: CultureInfo.InvariantCulture) switch { double number when double.IsFinite(d: number) && Math.Truncate(d: number) == number && number >= int.MinValue && number <= int.MaxValue => Some((TValue)(object)(int)number), _ => Option<TValue>.None }, _ => Option<TValue>.None }; } catch (Exception e) when (e is FormatException or InvalidCastException or OverflowException) { return Option<TValue>.None; } }
    }
    internal sealed record BoxSpec(GetBoxMode Mode, Point3d? BasePoint, string? Prompt1, string? Prompt2, string? Prompt3);

    private Fin<Unit> ApplyPoint(GetPoint getter) {
        _ = PointActions.Iter(action => action(obj: getter));
        return PointMode.Map(spec => {
            _ = spec.Cursor.Iter(cursor => getter.SetCursor(cursor));
            _ = spec.ObjectSnapCursors.Iter(enabled => getter.EnableObjectSnapCursors(enable: enabled));
            getter.PermitConstraintOptions(spec.PermitConstraintOptions);
            getter.EnableNoRedrawOnExit(spec.NoRedrawOnExit);
            getter.PermitFromOption(spec.PermitFromOption);
            getter.PermitTabMode(spec.PermitTabMode);
            getter.PermitElevatorMode(spec.PermitElevatorMode);
            getter.EnableSnapToCurves(enable: spec.SnapToCurves);
            _ = spec.SnapPoints.Iter(point => getter.AddSnapPoint(point: point));
            _ = spec.ConstructionPoints.Iter(point => getter.AddConstructionPoint(point: point));
            _ = spec.BasePoint.Iter(point => {
                getter.SetBasePoint(point, true);
                _ = spec.DrawLineFromBasePoint switch {
                    true => ((Func<Unit>)(() => { getter.DrawLineFromPoint(point, true); return unit; }))(),
                    false => unit,
                };
            });
            return spec.Constraints.Fold(
                Fin.Succ(value: unit),
                (state, value) => state.Bind(_ => value.Apply(getter: getter) switch {
                    true => Fin.Succ(value: unit),
                    false => Fin.Fail<Unit>(error: Op.Of(name: nameof(CommandPointConstraint)).InvalidInput()),
                }));
        }).IfNone(Fin.Succ(value: unit));
    }

    private Fin<Unit> ApplyObject(GetObject getter) =>
        from valid in Optional(getter).ToFin(Fail: Op.Of(name: nameof(ApplyObject)).InvalidInput())
        from applied in ObjectSelection.IfNone(CommandObjectSelection.Default).Apply(getter: valid)
        from _ in ObjectActions.TraverseM(action => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => {
            action(obj: valid);
            return Fin.Succ(value: unit);
        })).As().Map(static _ => unit)
        select unit;
}

public enum CommandPointEventPhase { MouseMove, MouseDown, DynamicDraw, PostDrawObjects }

public readonly record struct CommandPointEvent(
    CommandPointEventPhase Phase,
    RhinoDoc Document,
    GetPoint Getter,
    Option<GetPointMouseEventArgs> Mouse,
    Option<GetPointDrawEventArgs> Draw,
    Option<DrawEventArgs> PostDraw,
    Option<UiGumball> Gumball) {
    public Option<Point3d> Point =>
        Mouse.Map(static args => args.Point) | Draw.Map(static args => args.CurrentPoint);

    public Option<System.Drawing.Point> WindowPoint =>
        Mouse.Map(static args => args.WindowPoint);

    public Option<RhinoViewport> Viewport =>
        Mouse.Map(static args => args.Viewport) | Draw.Map(static args => args.Viewport) | PostDraw.Map(static args => args.Viewport);

    public Option<DisplayPipeline> Display =>
        Draw.Map(static args => args.Display) | PostDraw.Map(static args => args.Display);

    public Option<UiGumballSnapshot> GumballSnapshot =>
        Gumball.Map(static value => value.Snapshot);

    public Fin<bool> PickGumball(global::Rhino.Input.Custom.PickContext pick) {
        Option<UiGumball> gumball = Gumball;
        GetPoint getter = Getter;
        return from active in gumball.ToFin(Fail: Op.Of(name: nameof(PickGumball)).InvalidInput())
               from valid in Optional(pick).ToFin(Fail: Op.Of(name: nameof(PickGumball)).InvalidInput())
               from picked in active.Pick(pick: valid, point: getter)
               select picked;
    }

    public Fin<bool> UpdateGumball(Line worldLine) {
        Option<UiGumball> gumball = Gumball;
        Option<Point3d> point = Point;
        return from active in gumball.ToFin(Fail: Op.Of(name: nameof(UpdateGumball)).InvalidInput())
               from current in point.ToFin(Fail: Op.Of(name: nameof(UpdateGumball)).InvalidInput())
               from _ in active.CheckKeys()
               from changed in active.Update(point: current, line: worldLine)
               select changed;
    }

    public Fin<bool> UpdateGumball(Plane frame) {
        Option<UiGumball> gumball = Gumball;
        return from active in gumball.ToFin(Fail: Op.Of(name: nameof(UpdateGumball)).InvalidInput())
               from changed in active.Update(frame: frame)
               select changed;
    }

    public Fin<Unit> Preview(UiViewportPreview preview) {
        CommandPointEvent current = this;
        return from validPreview in Optional(preview).ToFin(Fail: Op.Of(name: nameof(Preview)).InvalidInput())
               from viewport in current.Viewport.ToFin(Fail: Op.Of(name: nameof(Preview)).InvalidInput())
               from display in current.Display.ToFin(Fail: Op.Of(name: nameof(Preview)).InvalidInput())
               from _ in validPreview.Draw(context: new Rasm.Rhino.UI.UiPreviewContext(Document: current.Document, Phase: current.Phase is CommandPointEventPhase.DynamicDraw ? Rasm.Rhino.UI.OverlayPhase.Overlay : Rasm.Rhino.UI.OverlayPhase.PostDraw, Viewport: viewport, Display: display, Gumball: current.GumballSnapshot))
               select unit;
    }
}

public static class CommandInputs {
    public static CommandInputRequest<T> Get<T>(params CommandInputPolicy[] policies) {
        Seq<CommandInputPolicy> active = toSeq(policies);
        CommandInputPolicy policy = CommandInputPolicy.Merge(policies: active);
        CommandInputRequest<T> request = typeof(T) switch {
            Type t when t == typeof(CommandSelection) => Objects<T>(policy: policy, policies: active),
            Type t when t == typeof(CommandOptionValue) => Getter<GetOption, T>(policy: policy, create: static () => new GetOption(), receive: static getter => getter.Get(), value: static (_, _, _) => Option<T>.None),
            Type t when t == typeof(CommandPoint) || t == typeof(Point3d) || t == typeof(DrawingPoint) => Point<T>(policy: policy),
            Type t when t == typeof(Transform) => Transform<T>(policy: policy),
            Type t when t == typeof(string) || t == typeof(double) => Text<T>(policy: policy),
            Type t when t == typeof(int) => Number<GetInteger, int, T>(policy: policy, create: static () => new GetInteger(), receive: static getter => getter.Get(), current: static getter => getter.Number(), setLower: static (getter, value, strict) => getter.SetLowerLimit(value, strict), setUpper: static (getter, value, strict) => getter.SetUpperLimit(value, strict)),
            Type t when t == typeof(bool) => new CommandInputRequest<T>(run: static _ => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Get)).InvalidInput()), scripted: (input, token) => Script<T>(input: input, token: token, policy: policy)),
            Type t when t == typeof(Line) => Native<T, Line>(get: static (out value) => RhinoGet.GetLine(line: out value)),
            Type t when t == typeof(Polyline) => Native<T, Polyline>(get: static (out value) => RhinoGet.GetPolyline(polyline: out value)),
            Type t when t == typeof(Circle) => Native<T, Circle>(get: static (out value) => RhinoGet.GetCircle(circle: out value)),
            Type t when t == typeof(Arc) => Native<T, Arc>(get: static (out value) => RhinoGet.GetArc(arc: out value)),
            Type t when t == typeof(Plane) => Native<T, Plane>(get: static (out value) => RhinoGet.GetPlane(plane: out value)),
            Type t when t == typeof(Seq<Point3d>) => Native<T>(run: () => Rectangle<T>(prompt: policy.PromptText.IfNone(string.Empty))),
            Type t when t == typeof(Box) => Native<T>(run: () => Box<T>(spec: policy.BoxMode.IfNone(new CommandInputPolicy.BoxSpec(Mode: GetBoxMode.All, BasePoint: null, Prompt1: null, Prompt2: null, Prompt3: null)))),
            Type t when t == typeof(Color) => Native<T>(run: () => Color<T>(prompt: policy.PromptText.IfNone(string.Empty), acceptNothing: policy.AcceptsNothing)),
            _ => Invalid<T>(name: nameof(Get)),
        };
        return request.BindPolicy(rebind: static values => Get<T>(policies: [.. values]));
    }

    private sealed class CommandTransformGetter(Func<RhinoViewport, Point3d, global::Rhino.Geometry.Transform> calculate) : GetTransform {
        public override global::Rhino.Geometry.Transform CalculateTransform(RhinoViewport viewport, Point3d point) => Optional(calculate).Map(project => project(arg1: viewport, arg2: point)).IfNone(global::Rhino.Geometry.Transform.Identity);
    }

    private delegate Result NativeGetter<TNative>(out TNative value);

    private static CommandInputRequest<T> Objects<T>(CommandInputPolicy policy, Seq<CommandInputPolicy> policies) =>
        policy.ObjectSelection.IfNone(CommandObjectSelection.Default) switch { { Minimum: < 0 } or { Maximum: < -1 } => Invalid<T>(name: nameof(Get)),
            CommandObjectSelection selection when selection.Maximum > 0 && selection.Maximum < selection.Minimum => Invalid<T>(name: nameof(Get)),
            CommandObjectSelection selection => Getter<GetObject, T>(
                policy: CommandInputPolicy.Merge(
                    policies: Seq(
                        selection.Minimum == 0 ? CommandInputPolicy.Accept(modes: CommandInputAccept.Nothing | CommandInputAccept.EnterWhenDone) : CommandInputPolicy.Empty,
                        selection.Maximum == 0 ? CommandInputPolicy.Accept(modes: CommandInputAccept.EnterWhenDone) : CommandInputPolicy.Empty) + policies),
                create: static () => new GetObject(),
                receive: getter => getter.GetMultiple(minimumNumber: selection.Minimum, maximumNumber: selection.Maximum),
                value: (source, getter, raw) => SelectionValue<T>(source: source, getter: getter, raw: raw, policy: selection),
                transition: static (getter, raw, selected) => raw is GetResult.Option ? ((Func<(bool Continue, Option<CommandOptionValue> Selected)>)(() => { getter.EnablePreSelect(enable: false, ignoreUnacceptablePreselectedObjects: true); return (Continue: true, Selected: selected); }))() : (Continue: false, Selected: selected)),
        };

    private static CommandInputRequest<T> Point<T>(CommandInputPolicy policy) {
        CommandInputPolicy.PointSpec spec = policy.PointMode.IfNone(CommandInputPolicy.DefaultPointSpec); bool twoDimensional = spec.TwoDimensional || typeof(T) == typeof(DrawingPoint);
        return Getter<GetPoint, T>(
            policy: policy,
            create: static () => new GetPoint(),
            receive: getter => getter.Get(onMouseUp: spec.OnMouseUp, get2DPoint: twoDimensional),
            value: static (_, getter, raw) => raw is GetResult.Point or GetResult.Point2d ? PointValue<T>(getter: getter, raw: raw) : Option<T>.None);
    }

    private static CommandInputRequest<T> Transform<T>(CommandInputPolicy policy) =>
        Getter<CommandTransformGetter, T>(policy: policy, create: () => new CommandTransformGetter(calculate: policy.TransformMode.IfNone(static (_, _) => global::Rhino.Geometry.Transform.Identity)), receive: static getter => getter.GetXform(), value: static (_, getter, _) => getter.HaveTransform && getter.Transform.IsValid ? Cast<T>(getter.Transform) : Option<T>.None);

    private static CommandInputRequest<T> Text<T>(CommandInputPolicy policy) =>
        policy.ScalarMode.Map(static scalar => scalar.Kind == CommandInputPolicy.ScalarKind.Number).IfNone(policy.LimitsMode.IsSome) && typeof(T) == typeof(double)
            ? Number<GetNumber, double, T>(policy: policy, create: static () => new GetNumber(), receive: static getter => getter.Get(), current: static getter => getter.Number(), setLower: static (getter, value, strict) => getter.SetLowerLimit(value, strict), setUpper: static (getter, value, strict) => getter.SetUpperLimit(value, strict))
            : Getter<GetString, T>(
                policy: policy,
                create: static () => new GetString(),
                receive: policy.IsLiteralText ? static getter => getter.GetLiteralString() : static getter => getter.Get(),
                value: (source, getter, raw) => raw is GetResult.String ? Parse<T>(input: source, text: getter.StringResult(), scalar: policy.ScalarMode, bounds: policy.LimitsMode).Bind(Cast<T>) : Option<T>.None);

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
            value: (_, getter, raw) => raw is GetResult.Number ? Cast<TOut>(current(arg: getter)!) : Option<TOut>.None);

    private static CommandInputRequest<T> Getter<TGetter, T>(
        CommandInputPolicy policy,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<CommandInput, TGetter, GetResult, Option<T>> value,
        Func<TGetter, Fin<Unit>>? configure = null,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)>? transition = null) where TGetter : GetBaseClass, IDisposable =>
        new(runEvent: input => Optional(input).ToFin(Fail: Op.Of(name: nameof(Get)).InvalidInput()).Bind(valid => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => {
            using TGetter getter = create();
            Atom<Option<Error>> eventFault = Atom(Option<Error>.None);
            return from configured in configure is Func<TGetter, Fin<Unit>> apply ? apply(arg: getter) : Fin.Succ(value: unit)
                   from applied in policy.Apply(getter: getter)
                   from events in getter is GetPoint point ? BindPointEvents(document: valid.Document, getter: point, events: policy.PointEvent, gumball: policy.Gumball, fault: eventFault) : Fin.Succ(value: unit)
                   from result in CommandOption.Bind(options: policy.OptionList, getter: getter).Bind(scope => {
                       using CommandOption.Scope active = scope;
                       return ReadLoop(getter: getter, scope: active, receive: () => receive(arg: getter), value: (g, raw) => value(arg1: valid, arg2: g, arg3: raw), selected: Option<CommandOptionValue>.None, acceptUndo: policy.AcceptsUndo, transition: transition ?? (static (_, _, selected) => (Continue: false, Selected: selected)));
                   })
                   from checkedResult in eventFault.Value.Case switch {
                       Error error => Fin.Fail<CommandInputEvent<T>>(error: error),
                       _ => Fin.Succ(value: result),
                   }
                   select checkedResult;
        })), rebind: null, scripted: (input, token) => Script<T>(input: input, token: token, policy: policy));

    private static Fin<Unit> BindPointEvents(RhinoDoc document, GetPoint getter, Option<Func<CommandPointEvent, Fin<Unit>>> events, Option<UiGumball> gumball, Atom<Option<Error>> fault) =>
        events.Map(change => {
            Unit Apply(CommandPointEvent pointEvent) =>
                Rasm.Rhino.UI.RhinoUi.Protect(valid: () => change(arg: pointEvent)).Match(Succ: static _ => unit, Fail: error => {
                    _ = fault.Swap(_ => Some(error));
                    _ = getter.InterruptMouseMove();
                    return unit;
                });
            getter.MouseMove += (_, args) => _ = Apply(pointEvent: new CommandPointEvent(Phase: CommandPointEventPhase.MouseMove, Document: document, Getter: getter, Mouse: Some(args), Draw: Option<GetPointDrawEventArgs>.None, PostDraw: Option<DrawEventArgs>.None, Gumball: gumball));
            getter.MouseDown += (_, args) => _ = Apply(pointEvent: new CommandPointEvent(Phase: CommandPointEventPhase.MouseDown, Document: document, Getter: getter, Mouse: Some(args), Draw: Option<GetPointDrawEventArgs>.None, PostDraw: Option<DrawEventArgs>.None, Gumball: gumball));
            getter.DynamicDraw += (_, args) => _ = Apply(pointEvent: new CommandPointEvent(Phase: CommandPointEventPhase.DynamicDraw, Document: document, Getter: getter, Mouse: Option<GetPointMouseEventArgs>.None, Draw: Some(args), PostDraw: Option<DrawEventArgs>.None, Gumball: gumball));
            getter.PostDrawObjects += (_, args) => _ = Apply(pointEvent: new CommandPointEvent(Phase: CommandPointEventPhase.PostDrawObjects, Document: document, Getter: getter, Mouse: Option<GetPointMouseEventArgs>.None, Draw: Option<GetPointDrawEventArgs>.None, PostDraw: Some(args), Gumball: gumball));
            getter.FullFrameRedrawDuringGet = true;
            return unit;
        }).Map(Fin.Succ).IfNone(Fin.Succ(value: unit));

    private static Fin<CommandInputEvent<T>> ReadLoop<TGetter, T>(
        TGetter getter,
        CommandOption.Scope scope,
        Func<GetResult> receive,
        Func<TGetter, GetResult, Option<T>> value,
        Option<CommandOptionValue> selected,
        bool acceptUndo,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)> transition) where TGetter : GetBaseClass =>
        receive() switch {
            GetResult.Cancel => Fin.Succ(value: CommandInputEvent<T>.Cancelled),
            GetResult.ExitRhino => Fin.Succ(value: CommandInputEvent<T>.Exit),
            GetResult.Option => scope.Selected(getter: getter).Bind(option => transition(getter, GetResult.Option, Some(option)) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, acceptUndo: acceptUndo, transition: transition),
                (false, Option<CommandOptionValue> next) => Read(getter: getter, raw: GetResult.Option, value: value(getter, GetResult.Option), option: next),
            }),
            GetResult.Undo when acceptUndo => Read(getter: getter, raw: GetResult.Undo, value: Option<T>.None, option: selected),
            GetResult.Undo => Fin.Fail<CommandInputEvent<T>>(error: Op.Of(name: nameof(ReadLoop)).InvalidResult()),
            GetResult raw and (GetResult.NoResult or GetResult.Nothing or GetResult.Miss or GetResult.Timeout) => Read(getter: getter, raw: raw, value: Option<T>.None, option: selected),
            GetResult raw => transition(getter, raw, selected) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, acceptUndo: acceptUndo, transition: transition),
                (false, Option<CommandOptionValue> next) => Read(getter: getter, raw: raw, value: value(getter, raw), option: next),
            },
        };

    private static Fin<CommandInputEvent<T>> Read<TGetter, T>(TGetter getter, GetResult raw, Option<T> value, Option<CommandOptionValue> option) where TGetter : GetBaseClass {
        Option<T> projected = value | option.Bind(static selected => selected is T selectedValue ? Some(selectedValue) : Option<T>.None);
        CommandGet<T> snapshot = CommandGet<T>.Of(getter: getter, raw: raw, value: projected, option: option);
        return (raw, projected.IsSome || (raw == GetResult.Option && option.IsSome) || snapshot.Snapshot.Has(raw: raw)) switch {
            (GetResult.Object or GetResult.Point or GetResult.Point2d or GetResult.Number or GetResult.String or GetResult.Color or GetResult.Rectangle2d or GetResult.Line2d, false) => Fin.Fail<CommandInputEvent<T>>(error: Op.Of(name: nameof(Read)).InvalidResult()),
            _ => Fin.Succ(value: CommandInputEvent<T>.Of(result: snapshot)),
        };
    }

    private static CommandInputRequest<T> Native<T>(Func<(Result Result, Option<T> Value)> run) =>
        new(input => Optional(input).ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput()).Bind(_ => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => run() switch {
            (Result.Success, Option<T> value) => Fin.Succ(value: CommandGet<T>.Native(result: Result.Success, value: value)),
            (Result.Cancel or Result.CancelModelessDialog, _) => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            (Result.Failure or Result.UnknownCommand, _) => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Native)).InvalidResult()),
            (Result result, _) => Fin.Succ(value: CommandGet<T>.Native(result: result, value: Option<T>.None)),
        })), scripted: (input, token) => Script<T>(input: input, token: token));

    private static Fin<CommandInputEvent<T>> Script<T>(CommandInput input, string token, CommandInputPolicy? policy = null) =>
        from source in Optional(input).ToFin(Fail: Op.Of(name: nameof(Script)).InvalidInput())
        from text in Optional(token).ToFin(Fail: Op.Of(name: nameof(Script)).InvalidInput())
        from result in CommandOption.Script(options: Optional(policy).IfNone(CommandInputPolicy.Empty).OptionList, token: text).Case switch {
            CommandOptionValue option => Fin.Succ(value: CommandGet<T>.ScriptOption(option: option)),
            _ => from value in ScriptValue<T>(input: source, text: text, policy: Optional(policy).IfNone(CommandInputPolicy.Empty)).ToFin(Fail: Op.Of(name: nameof(Script)).InvalidResult())
                 select CommandGet<T>.Native(result: Result.Success, value: Some(value)),
        }
        select CommandInputEvent<T>.Of(result: result);

    private static Option<T> ScriptValue<T>(CommandInput input, string text, CommandInputPolicy policy) =>
        typeof(T) switch {
            Type t when t == typeof(string) => Cast<T>(text),
            Type t when t == typeof(bool) => text switch {
                string value when string.Equals(a: value, b: "1", comparisonType: StringComparison.Ordinal) || string.Equals(a: value, b: "true", comparisonType: StringComparison.OrdinalIgnoreCase) || string.Equals(a: value, b: "yes", comparisonType: StringComparison.OrdinalIgnoreCase) => Cast<T>(true),
                string value when string.Equals(a: value, b: "0", comparisonType: StringComparison.Ordinal) || string.Equals(a: value, b: "false", comparisonType: StringComparison.OrdinalIgnoreCase) || string.Equals(a: value, b: "no", comparisonType: StringComparison.OrdinalIgnoreCase) => Cast<T>(false),
                _ => Option<T>.None,
            },
            Type t when t == typeof(Color) => CommandOption.ColorValue(text: text).Bind(value => Cast<T>(value)),
            Type t when t == typeof(double) => Parse<T>(input: input, text: text, scalar: policy.ScalarMode, bounds: policy.LimitsMode).Bind(Cast<T>),
            Type t when t == typeof(int) => int.TryParse(s: text, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out int value) ? policy.LimitsMode.Case switch { CommandInputPolicy.LimitSpec spec => spec.Accept(value: value).Bind(valid => Cast<T>(valid)), _ => Cast<T>(value) } : Option<T>.None,
            Type t when t == typeof(CommandOptionValue) => Option<T>.None,
            _ => Option<T>.None,
        };

    private static CommandInputRequest<T> Native<T, TNative>(NativeGetter<TNative> get) =>
        Native<T>(run: () => {
            Result result = get(value: out TNative value);
            return (Result: result, Value: result == Result.Success ? Cast<T>(value!) : Option<T>.None);
        });

    private static (Result Result, Option<T> Value) Rectangle<T>(string prompt) { Result result = string.IsNullOrWhiteSpace(value: prompt) ? RhinoGet.GetRectangle(corners: out Point3d[] corners) : RhinoGet.GetRectangle(firstPrompt: prompt, corners: out corners); return (Result: result, Value: result == Result.Success ? Cast<T>(toSeq(corners)) : Option<T>.None); }

    private static (Result Result, Option<T> Value) Box<T>(CommandInputPolicy.BoxSpec spec) { Result result = spec is { Mode: GetBoxMode.All, BasePoint: null, Prompt1: null, Prompt2: null, Prompt3: null } ? RhinoGet.GetBox(box: out Box value) : RhinoGet.GetBox(box: out value, mode: spec.Mode, basePoint: spec.BasePoint ?? Point3d.Unset, prompt1: spec.Prompt1, prompt2: spec.Prompt2, prompt3: spec.Prompt3); return (Result: result, Value: result == Result.Success ? Cast<T>(value) : Option<T>.None); }

    private static (Result Result, Option<T> Value) Color<T>(string prompt, bool acceptNothing) { Color color = global::System.Drawing.Color.Empty; Result result = RhinoGet.GetColor(prompt: prompt, acceptNothing: acceptNothing, color: ref color); return (Result: result, Value: result == Result.Success ? Cast<T>(color) : Option<T>.None); }

    private static CommandInputRequest<T> Invalid<T>(string name) => new(run: _ => Fin.Fail<CommandGet<T>>(error: Op.Of(name: name).InvalidInput()));

    private static Option<CommandSelection> SelectionOf(RhinoDoc document, GetObject getter, GetResult raw) =>
        raw is GetResult.Object && getter.Objects() is ObjRef[] references
            ? Optional(CommandSelection.From(
                document: document,
                references: toSeq(references),
                preselected: getter.ObjectsWerePreselected
                    ? toSeq(references).Filter(static reference => Optional(reference.Object()).Map(static item => item.IsSelected(checkSubObjects: true) > 0).IfNone(false)).Map(static reference => (reference.ObjectId, reference.GeometryComponentIndex))
                    : Seq<(Guid ObjectId, ComponentIndex ComponentIndex)>()))
            : Option<CommandSelection>.None;

    private static Option<T> SelectionValue<T>(CommandInput source, GetObject getter, GetResult raw, CommandObjectSelection policy) =>
        SelectionOf(document: source.Document, getter: getter, raw: raw)
            .Bind(selection => selection.Trim(policy: policy).ToOption())
            .Bind(Cast<T>);

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

    private static Option<double> ParseNumber(string text) { StringParserSettings results = new(); try { return StringParser.ParseNumber(expression: text, max_count: text.Length, settings_in: StringParserSettings.DefaultParseSettings, settings_out: ref results, answer: out double value) == text.Length ? Some(value) : Option<double>.None; } finally { results.Dispose(); } }

    private static Option<double> ParseLength(string text, UnitSystem units) { LengthValue value = LengthValue.Create(s: text, ps: StringParserSettings.DefaultParseSettings, parsedAll: out bool parsedAll); try { return parsedAll && !value.IsUnset() ? Some(value.Length(units: units)) : Option<double>.None; } finally { value.Dispose(); } }

    private static Option<double> ParseAngle(string text, AngleUnitSystem units) { StringParserSettings results = new(); AngleUnitSystem parsed = AngleUnitSystem.None; try { return StringParser.ParseAngleExpession(text, 0, text.Length, StringParserSettings.DefaultParseSettings, units, out double value, ref results, ref parsed) == text.Length ? Some(value) : Option<double>.None; } finally { results.Dispose(); } }

    private static Option<double> Within(double value, Option<CommandInputPolicy.LimitSpec> bounds) =>
        bounds.Case switch {
            CommandInputPolicy.LimitSpec spec => spec.Accept(value: value),
            _ => Some(value),
        };

    private static Fin<Unit> Limits<TGetter, TValue>(TGetter getter, Option<CommandInputPolicy.LimitSpec> bounds, Action<TGetter, TValue, bool> setLower, Action<TGetter, TValue, bool> setUpper) where TGetter : GetBaseClass where TValue : IComparable<TValue> =>
        bounds.Case switch {
            CommandInputPolicy.LimitSpec b =>
                from projected in b.Project<TValue>(op: Op.Of(name: nameof(Limits)))
                from valid in Optional(getter).ToFin(Fail: Op.Of(name: nameof(Limits)).InvalidInput())
                select ((Func<Unit>)(() => {
                    _ = projected.Lower.Iter(value => setLower(arg1: valid, arg2: value, arg3: b.StrictlyLower));
                    _ = projected.Upper.Iter(value => setUpper(arg1: valid, arg2: value, arg3: b.StrictlyUpper));
                    return unit;
                }))(),
            _ => Fin.Succ(value: unit),
        };

    private static CommandInputRequest<T> BindPolicy<T>(this CommandInputRequest<T> request, Func<Seq<CommandInputPolicy>, CommandInputRequest<T>> rebind) =>
        new(runEvent: request.RunEvent, rebind: rebind, scripted: request.Script);
}

public sealed record CommandInput {
    internal CommandInput(RhinoDoc document, Rasm.Domain.Context domain) {
        ArgumentNullException.ThrowIfNull(argument: document);
        ArgumentNullException.ThrowIfNull(argument: domain);
        Document = document;
        Domain = domain;
    }

    public RhinoDoc Document { get; }
    public Rasm.Domain.Context Domain { get; }

    public Fin<CommandGet<TValue>> Get<TValue>(CommandInputRequest<TValue> request) =>
        Optional(request).ToFin(Fail: Op.Of(name: nameof(Get)).InvalidInput()).Bind(valid => valid.Run(input: this));

    internal Fin<CommandInputEvent<TValue>> GetEvent<TValue>(CommandInputRequest<TValue> request) =>
        Optional(request).ToFin(Fail: Op.Of(name: nameof(GetEvent)).InvalidInput()).Bind(valid => valid.RunEvent(input: this));

    internal Fin<CommandInputEvent<TValue>> Script<TValue>(CommandInputRequest<TValue> request, string token) =>
        Optional(request).ToFin(Fail: Op.Of(name: nameof(Script)).InvalidInput()).Bind(valid => valid.Script(input: this, token: token));
}
