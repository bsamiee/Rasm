using System.Globalization;
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

// --- [MODELS] -----------------------------------------------------------------------------
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
    Option<PointOnGeometry> OnGeometry) {
    internal static CommandPoint Of(GetPoint getter, GetResult raw) {
        RhinoViewport? viewport = getter.View()?.ActiveViewport;
        Option<(RhinoViewport Viewport, Plane Plane)> constraint = viewport switch {
            RhinoViewport value when getter.GetPlanarConstraint(vp: ref value, plane: out Plane plane) => Some((Viewport: value, Plane: plane)),
            _ => Option<(RhinoViewport Viewport, Plane Plane)>.None,
        };
        using ObjRef? reference = getter.PointOnObject();
        return new(Point: raw is GetResult.Point ? Some(getter.Point()) : Option<Point3d>.None, WindowPoint: raw is GetResult.Point or GetResult.Point2d ? Some(getter.Point2d()) : Option<DrawingPoint>.None, View: Optional(getter.View()), Reference: CommandSelection.Reference.Of(reference: Optional(reference)), NumberPreview: getter.NumberPreview(number: out double number) ? Some(number) : Option<double>.None, Osnap: getter.OsnapEventType, BasePoint: getter.TryGetBasePoint(out Point3d basePoint) ? Some(basePoint) : Option<Point3d>.None, PlanarConstraint: constraint, SnapPoints: toSeq(getter.GetSnapPoints()), ConstructionPoints: toSeq(getter.GetConstructionPoints()), OnGeometry: PointOnGeometry.Of(reference: Optional(reference)));
    }
}

[Union]
public abstract partial record PointOnGeometry {
    private PointOnGeometry() { }
    public sealed record OnCurve(Curve Curve, double T) : PointOnGeometry;
    public sealed record OnSurface(Surface Surface, double U, double V) : PointOnGeometry;
    public sealed record OnBrep(BrepFace Face, double U, double V) : PointOnGeometry;

    public Point3d Location =>
        Switch(
            onCurve: static value => value.Curve.PointAt(t: value.T),
            onSurface: static value => value.Surface.PointAt(u: value.U, v: value.V),
            onBrep: static value => value.Face.PointAt(u: value.U, v: value.V));

    internal static Option<PointOnGeometry> Of(Option<ObjRef> reference) =>
        reference.Bind(active => {
            double t = double.NaN, u = double.NaN, v = double.NaN;
            bool picked = active.SelectionMethod() == SelectionMethod.MousePick;
            Curve? curve = picked ? active.CurveParameter(parameter: out t) : null;
            Surface? surface = picked ? active.SurfaceParameter(u: out u, v: out v) : null;
            BrepFace? face = picked ? active.Face() : null;
            return (curve, surface, face) switch {
                (Curve c, _, _) when RhinoMath.IsValidDouble(x: t) => Some<PointOnGeometry>(new OnCurve(Curve: c, T: t)),
                (_, _, BrepFace f) when RhinoMath.IsValidDouble(x: u) && RhinoMath.IsValidDouble(x: v) => Some<PointOnGeometry>(new OnBrep(Face: f, U: u, V: v)),
                (_, Surface s, _) when RhinoMath.IsValidDouble(x: u) && RhinoMath.IsValidDouble(x: v) => Some<PointOnGeometry>(new OnSurface(Surface: s, U: u, V: v)),
                _ => Option<PointOnGeometry>.None,
            };
        });
}

public readonly record struct CommandPointConstraint {
    private readonly Func<GetPoint, Fin<Unit>>? apply;

    private CommandPointConstraint(Func<GetPoint, Fin<Unit>> apply) =>
        this.apply = apply;

    public static CommandPointConstraint operator +(CommandPointConstraint left, CommandPointConstraint right) =>
        Add(left, right);

    public static CommandPointConstraint Add(params CommandPointConstraint[] values) =>
        new(getter =>
            from source in Optional(values).ToFin(Fail: Op.Of(name: nameof(Add)).InvalidInput())
            from applied in toSeq(source)
                .Filter(static value => value.apply is not null)
                .TraverseM(value => value.Apply(getter: getter))
                .As()
            select unit);

    internal Fin<Unit> Apply(GetPoint getter) =>
        Optional(apply)
            .ToFin(Fail: Op.Of(name: nameof(CommandPointConstraint)).InvalidInput())
            .Bind(valid => valid(arg: getter));

    public static CommandPointConstraint Of(Func<GetPoint, bool> apply) =>
        new(getter => Optional(apply).ToFin(Fail: Op.Of(name: nameof(CommandPointConstraint)).InvalidInput())
            .Bind(valid => valid(arg: getter) ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: Op.Of(name: nameof(CommandPointConstraint)).InvalidInput())));
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

internal enum CommandInputEventKind { Value, Option, Undo, Nothing, Rejected, NoResult, Miss, Timeout, Cancel, Exit }

internal readonly record struct CommandInputEvent<T>(CommandInputEventKind Kind, Option<CommandGet<T>> Result) {
    internal static CommandInputEvent<T> Of(CommandGet<T> result) =>
        new(
            Kind: (Rejected: result.SelectionTrim.IsSome && result.Value.IsNone, result.Raw.Case, result.CommandResult) switch {
                (true, _, _) => CommandInputEventKind.Rejected,
                (_, GetResult.Option, _) => CommandInputEventKind.Option,
                (_, GetResult.Undo, _) => CommandInputEventKind.Undo,
                (_, GetResult.Nothing, _) => CommandInputEventKind.Nothing,
                (_, GetResult.NoResult, _) => CommandInputEventKind.NoResult,
                (_, GetResult.Miss, _) => CommandInputEventKind.Miss,
                (_, GetResult.Timeout, _) => CommandInputEventKind.Timeout,
                (_, GetResult, _) => CommandInputEventKind.Value,
                (_, _, global::Rhino.Commands.Result.Nothing) => CommandInputEventKind.Nothing,
                _ => CommandInputEventKind.Value,
            },
            Result: Some(result));

    internal static CommandInputEvent<T> Cancelled { get; } =
        new(Kind: CommandInputEventKind.Cancel, Result: Option<CommandGet<T>>.None);

    internal static CommandInputEvent<T> Exit { get; } =
        new(Kind: CommandInputEventKind.Exit, Result: Option<CommandGet<T>>.None);

    internal Fin<CommandGet<T>> ToGet() =>
        Kind switch {
            CommandInputEventKind.Cancel or CommandInputEventKind.Exit => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            _ => Result.ToFin(Fail: Op.Of(name: nameof(CommandInputEvent<>)).InvalidResult()),
        };
}

public sealed record CommandInputRequest<T> {
    private readonly Func<CommandInput, Fin<CommandInputEvent<T>>> run;
    private readonly Func<Seq<CommandInputPolicy>, CommandInputRequest<T>> rebind;
    private readonly Func<CommandInput, string, Fin<CommandInputEvent<T>>> scripted;

    internal CommandInputRequest(Func<CommandInput, Fin<CommandGet<T>>> run, Func<Seq<CommandInputPolicy>, CommandInputRequest<T>>? rebind = null, Func<CommandInput, string, Fin<CommandInputEvent<T>>>? scripted = null) {
        this.run = input => run(arg: input).Map(CommandInputEvent<T>.Of);
        this.rebind = rebind ?? (_ => this);
        this.scripted = scripted ?? ((_, _) => Fin.Fail<CommandInputEvent<T>>(error: Op.Of(name: nameof(CommandInputRequest<>)).InvalidInput()));
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
    TransparentCommands = 256,
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
        CommandPointEventPhase pointEventPhases = CommandPointEventPhase.None,
        Option<UiGumball> gumball = default,
        Seq<CommandOption> options = default,
        Option<PointSpec> point = default,
        Option<Func<RhinoViewport, Point3d, Transform>> transform = default,
        Option<Scalar> scalar = default,
        Option<LimitSpec> bounds = default,
        Option<BoxSpec> box = default,
        Option<CommandObjectSelection> objectSelection = default,
        Option<string> prompt = default,
        Option<Color> colorSeed = default,
        bool fullFrameRedraw = false,
        bool literalText = false,
        CommandInputAccept accept = CommandInputAccept.None) {
        BaseActions = baseActions; ObjectActions = objectActions; PointActions = pointActions; PointEvent = pointEvents; PointEventPhases = pointEventPhases; Gumball = gumball; OptionList = options; PointMode = point; TransformMode = transform; ScalarMode = scalar; LimitsMode = bounds; BoxMode = box; ObjectSelection = objectSelection; PromptText = prompt; ColorSeed = colorSeed; FullFrameRedrawDuringGet = fullFrameRedraw; IsLiteralText = literalText; AcceptModes = accept;
    }

    private Seq<Action<GetBaseClass>> BaseActions { get; }
    private Seq<Action<GetObject>> ObjectActions { get; }
    private Seq<Action<GetPoint>> PointActions { get; }
    internal Option<Func<CommandPointEvent, Fin<Unit>>> PointEvent { get; }
    internal CommandPointEventPhase PointEventPhases { get; }
    internal Option<UiGumball> Gumball { get; }
    internal Seq<CommandOption> OptionList { get; }
    internal Option<PointSpec> PointMode { get; }
    internal Option<Func<RhinoViewport, Point3d, Transform>> TransformMode { get; }
    internal Option<Scalar> ScalarMode { get; }
    internal Option<LimitSpec> LimitsMode { get; }
    internal Option<BoxSpec> BoxMode { get; }
    internal Option<CommandObjectSelection> ObjectSelection { get; }
    internal Option<string> PromptText { get; }
    internal Option<Color> ColorSeed { get; }
    internal bool FullFrameRedrawDuringGet { get; }
    internal bool IsLiteralText { get; }
    internal CommandInputAccept AcceptModes { get; }
    internal bool AcceptsNothing => Accepts(mode: CommandInputAccept.Nothing);
    internal bool AcceptsUndo => Accepts(mode: CommandInputAccept.Undo);
    internal static CommandInputPolicy Empty { get; } = new();

    public static CommandInputPolicy Configure<TGetter>(Action<TGetter> apply) where TGetter : GetBaseClass =>
        (typeof(TGetter), Optional(apply).Case) switch {
            (Type type, Action<TGetter> action) when type == typeof(GetObject) => new(objectActions: Seq<Action<GetObject>>(getter => action((TGetter)(GetBaseClass)getter))),
            (Type type, Action<TGetter> action) when type == typeof(GetPoint) => new(pointActions: Seq<Action<GetPoint>>(getter => action((TGetter)(GetBaseClass)getter))),
            (_, Action<TGetter> action) => new(baseActions: Seq<Action<GetBaseClass>>(getter => _ = getter is TGetter typed ? Op.Side(() => action(typed)) : unit)),
            _ => Empty,
        };
    public static CommandInputPolicy Prompt(string value) => new(baseActions: Seq<Action<GetBaseClass>>(getter => getter.SetCommandPrompt(value)), prompt: Optional(value));
    public static CommandInputPolicy PromptDefault(string value) => Optional(value).Map(defaultText => Configure<GetBaseClass>(apply: getter => getter.SetCommandPromptDefault(defaultText))).IfNone(Empty);
    public static CommandInputPolicy Wait(int milliseconds) =>
        milliseconds switch {
            > 0 => Configure<GetBaseClass>(apply: getter => getter.SetWaitDuration(milliseconds)),
            _ => Empty,
        };
    public static CommandInputPolicy ClearDefault() => Configure<GetBaseClass>(apply: static getter => getter.ClearDefault());
    public static CommandInputPolicy ClearOptions() => Configure<GetBaseClass>(apply: static getter => getter.ClearCommandOptions());
    public static CommandInputPolicy CustomMessage(object value) =>
        Optional(value).Map(message => new CommandInputPolicy(baseActions: Seq<Action<GetBaseClass>>(getter => { getter.AcceptCustomMessage(enable: true); GetBaseClass.PostCustomMessage(messageData: message); }), accept: CommandInputAccept.CustomMessage)).IfNone(Empty);
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
    public static CommandInputPolicy PointEvents(Func<CommandPointEvent, Fin<Unit>> change, CommandPointEventPhase phases = CommandPointEventPhase.All, bool fullFrameRedraw = false) =>
        Optional(change).Map(apply => new CommandInputPolicy(pointEvents: Some(apply), pointEventPhases: phases, fullFrameRedraw: fullFrameRedraw)).IfNone(Empty);
    public static CommandInputPolicy PointGumball(UiGumball gumball) =>
        Optional(gumball).Map(active => new CommandInputPolicy(gumball: Some(active), pointEvents: Some<Func<CommandPointEvent, Fin<Unit>>>(static _ => Fin.Succ(value: unit)), pointEventPhases: CommandPointEventPhase.All)).IfNone(Empty);
    public static CommandInputPolicy Transform(Func<RhinoViewport, Point3d, Transform> calculate) =>
        Optional(calculate).Map(project => new CommandInputPolicy(transform: Some(project))).IfNone(Empty);
    public static CommandInputPolicy Objects(CommandObjectSelection selection) =>
        Optional(selection)
            .Map(static valid => new CommandInputPolicy(objectSelection: Some(valid)))
            .IfNone(Empty);

    public static CommandInputPolicy Point(PointSpec spec) =>
        Optional(spec).Map(active => new CommandInputPolicy(point: Some(active))).IfNone(Empty);
    public static CommandInputPolicy Point(Func<PointSpec, PointSpec> update) =>
        Optional(update).Map(active => Point(spec: active(arg: DefaultPointSpec))).IfNone(Empty);
    public static CommandInputPolicy Bounds<T>(Option<T> lower = default, Option<T> upper = default, bool strictlyLower = false, bool strictlyUpper = false) where T : IComparable<T> =>
        new(bounds: Some(Limit(lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper)));
    public static CommandInputPolicy Number() => new(scalar: Some(new Scalar(Kind: ScalarKind.Number, LengthUnits: Option<UnitSystem>.None, AngleUnits: Option<AngleUnitSystem>.None)));
    public static CommandInputPolicy Number<T>(Option<T> lower = default, Option<T> upper = default, bool strictlyLower = false, bool strictlyUpper = false) where T : IComparable<T> => Bounds(lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper) + Number();
    public static CommandInputPolicy Length(Option<UnitSystem> units = default) => new(scalar: Some(new Scalar(Kind: ScalarKind.Length, LengthUnits: units, AngleUnits: Option<AngleUnitSystem>.None)));
    public static CommandInputPolicy Angle(AngleUnitSystem units) => new(scalar: Some(new Scalar(Kind: ScalarKind.Angle, LengthUnits: Option<UnitSystem>.None, AngleUnits: Some(units))));
    public static CommandInputPolicy LiteralText() => new(literalText: true);
    public static CommandInputPolicy SeedColor(Color value) => new(colorSeed: Some(value));
    public static CommandInputPolicy Box(GetBoxMode mode = GetBoxMode.All, Point3d? basePoint = null, string? prompt1 = null, string? prompt2 = null, string? prompt3 = null) => new(box: Some(new BoxSpec(Mode: mode, BasePoint: basePoint, Prompt1: prompt1, Prompt2: prompt2, Prompt3: prompt3)));

    public static CommandInputPolicy operator +(CommandInputPolicy left, CommandInputPolicy right) => Add(left: left, right: right);

    public static CommandInputPolicy Add(CommandInputPolicy left, CommandInputPolicy right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        ArgumentNullException.ThrowIfNull(argument: right);
        return new(baseActions: left.BaseActions + right.BaseActions, objectActions: left.ObjectActions + right.ObjectActions, pointActions: left.PointActions + right.PointActions, pointEvents: Compose(left.PointEvent, right.PointEvent), pointEventPhases: left.PointEventPhases | right.PointEventPhases, gumball: Pick(left.Gumball, right.Gumball), options: left.OptionList + right.OptionList, point: Pick(left.PointMode, right.PointMode), transform: Pick(left.TransformMode, right.TransformMode), scalar: Pick(left.ScalarMode, right.ScalarMode), bounds: Pick(left.LimitsMode, right.LimitsMode), box: Pick(left.BoxMode, right.BoxMode), objectSelection: Pick(left.ObjectSelection, right.ObjectSelection), prompt: Pick(left.PromptText, right.PromptText), colorSeed: Pick(left.ColorSeed, right.ColorSeed), fullFrameRedraw: left.FullFrameRedrawDuringGet || right.FullFrameRedrawDuringGet, literalText: left.IsLiteralText || right.IsLiteralText, accept: left.AcceptModes | right.AcceptModes);
    }

    internal static CommandInputPolicy Merge(Seq<CommandInputPolicy> policies) =>
        policies.Fold(Empty, static (state, policy) => state + policy);

    internal Fin<Unit> Apply<TGetter>(TGetter getter) where TGetter : GetBaseClass =>
        from _ in guard(!(Accepts(mode: CommandInputAccept.Text) && Accepts(mode: CommandInputAccept.TransparentCommands)), Op.Of(name: nameof(CommandInputPolicy)).InvalidInput())
        from valid in Optional(getter).ToFin(Fail: Op.Of(name: nameof(CommandInputPolicy)).InvalidInput())
        from applied in Fin.Succ(value: BaseActions.Iter(action => action(obj: valid)))
        from result in valid switch {
            GetObject objects => ApplyObject(getter: objects),
            GetPoint point => ApplyPoint(getter: point),
            _ => Fin.Succ(value: unit),
        }
        select result;

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
    public sealed record PointSpec(bool OnMouseUp, bool TwoDimensional, Option<global::Rhino.UI.CursorStyle> Cursor, Option<bool> ObjectSnapCursors, Option<Point3d> BasePoint, bool DrawLineFromBasePoint, bool SnapToCurves, bool PermitConstraintOptions, bool NoRedrawOnExit, bool PermitFromOption, bool PermitTabMode, int PermitElevatorMode, Seq<Point3d> SnapPoints, Seq<Point3d> ConstructionPoints, Seq<CommandPointConstraint> Constraints, Option<double> DistanceFromBasePoint, Option<bool> PermitOrthoSnap, Option<bool> PermitObjectSnap, Option<Color> DynamicDrawColor, Option<(bool Enabled, bool Ends)> CurveSnapTangent, Option<(bool Enabled, bool Ends)> CurveSnapPerp, Option<(bool Enabled, bool Reverse)> CurveSnapArrow, bool ClearConstraints);
    public static PointSpec DefaultPointSpec { get; } = new(OnMouseUp: false, TwoDimensional: false, Cursor: Option<global::Rhino.UI.CursorStyle>.None, ObjectSnapCursors: Option<bool>.None, BasePoint: Option<Point3d>.None, DrawLineFromBasePoint: false, SnapToCurves: false, PermitConstraintOptions: true, NoRedrawOnExit: false, PermitFromOption: true, PermitTabMode: true, PermitElevatorMode: 0, SnapPoints: Seq<Point3d>(), ConstructionPoints: Seq<Point3d>(), Constraints: Seq<CommandPointConstraint>(), DistanceFromBasePoint: Option<double>.None, PermitOrthoSnap: Option<bool>.None, PermitObjectSnap: Option<bool>.None, DynamicDrawColor: Option<Color>.None, CurveSnapTangent: Option<(bool Enabled, bool Ends)>.None, CurveSnapPerp: Option<(bool Enabled, bool Ends)>.None, CurveSnapArrow: Option<(bool Enabled, bool Reverse)>.None, ClearConstraints: false);
    internal sealed record Scalar(ScalarKind Kind, Option<UnitSystem> LengthUnits, Option<AngleUnitSystem> AngleUnits);
    public static LimitSpec Limit<T>(Option<T> lower = default, Option<T> upper = default, bool strictlyLower = false, bool strictlyUpper = false) where T : IComparable<T> =>
        new(Lower: lower.Map(static value => (object)value), Upper: upper.Map(static value => (object)value), StrictlyLower: strictlyLower, StrictlyUpper: strictlyUpper);

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
    internal sealed record BoxSpec(GetBoxMode Mode, Point3d? BasePoint, string? Prompt1, string? Prompt2, string? Prompt3);

    private Fin<Unit> ApplyPoint(GetPoint getter) {
        _ = PointActions.Iter(action => action(obj: getter));
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
                double => Fin.Fail<Unit>(error: Op.Of(name: nameof(ApplyPoint)).InvalidInput()),
                _ => Fin.Succ(value: unit),
            }).Bind(_ => spec.Constraints.Fold(
                Fin.Succ(value: unit),
                (state, value) => state.Bind(_ => value.Apply(getter: getter))));
        }).IfNone(Fin.Succ(value: unit));
    }

    private Fin<Unit> ApplyObject(GetObject getter) =>
        from valid in Optional(getter).ToFin(Fail: Op.Of(name: nameof(ApplyObject)).InvalidInput())
        from applied in ObjectSelection.IfNone(CommandObjectSelection.Default).Apply(getter: valid)
        from _ in ObjectActions.TraverseM(action => UI.RhinoUi.Protect(valid: () => {
            action(obj: valid);
            return Fin.Succ(value: unit);
        })).As().Map(static _ => unit)
        select unit;
}

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

[StructLayout(LayoutKind.Auto)]
public readonly record struct PointerState(PointerButtons Buttons, PointerModifiers Modifiers) {
    public bool IsDrag => Buttons != PointerButtons.None;
}

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

    public Option<DrawingPoint> WindowPoint =>
        Mouse.Map(static args => args.WindowPoint);

    public Option<RhinoViewport> Viewport =>
        Mouse.Map(static args => args.Viewport) | Draw.Map(static args => args.Viewport) | PostDraw.Map(static args => args.Viewport);

    public Option<DisplayPipeline> Display =>
        Draw.Map(static args => args.Display) | PostDraw.Map(static args => args.Display);

    public Option<UiGumballSnapshot> GumballSnapshot =>
        Gumball.Map(static value => value.Snapshot);

    public Option<PointerState> Pointer =>
        Mouse.Map(static args => new PointerState(
            Buttons: (args.LeftButtonDown ? PointerButtons.Left : PointerButtons.None)
                   | (args.RightButtonDown ? PointerButtons.Right : PointerButtons.None)
                   | (args.MiddleButtonDown ? PointerButtons.Middle : PointerButtons.None),
            Modifiers: (args.ShiftKeyDown ? PointerModifiers.Shift : PointerModifiers.None)
                     | (args.ControlKeyDown ? PointerModifiers.Control : PointerModifiers.None)));

    public Fin<bool> PickGumball(PickContext pick) {
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
               from _ in validPreview.Draw(context: new UI.UiPreviewContext(Document: current.Document, Phase: current.Phase is CommandPointEventPhase.DynamicDraw ? UI.OverlayPhase.Overlay : UI.OverlayPhase.PostDraw, Viewport: viewport, Display: display, Gumball: current.GumballSnapshot))
               select unit;
    }
}

public sealed record CommandInput {
    internal CommandInput(RhinoDoc document, Context domain) {
        ArgumentNullException.ThrowIfNull(argument: document);
        ArgumentNullException.ThrowIfNull(argument: domain);
        Document = document;
        Domain = domain;
    }

    public RhinoDoc Document { get; }
    public Context Domain { get; }

    public Fin<CommandGet<TValue>> Get<TValue>(CommandInputRequest<TValue> request) =>
        Optional(request).ToFin(Fail: Op.Of(name: nameof(Get)).InvalidInput()).Bind(valid => valid.Run(input: this));

    internal Fin<CommandInputEvent<TValue>> GetEvent<TValue>(CommandInputRequest<TValue> request) =>
        Optional(request).ToFin(Fail: Op.Of(name: nameof(GetEvent)).InvalidInput()).Bind(valid => valid.RunEvent(input: this));

    internal Fin<CommandInputEvent<TValue>> Script<TValue>(CommandInputRequest<TValue> request, string token) =>
        Optional(request).ToFin(Fail: Op.Of(name: nameof(Script)).InvalidInput()).Bind(valid => valid.Script(input: this, token: token));
}

// --- [SERVICES] ---------------------------------------------------------------------------
public static class CommandInputs {
    public static CommandInputRequest<T> Get<T>(params CommandInputPolicy[] policies) {
        Seq<CommandInputPolicy> active = toSeq(policies);
        CommandInputPolicy policy = CommandInputPolicy.Merge(policies: active);
        CommandInputRequest<T> request = Build<T>(policy: policy, policies: active);
        return new CommandInputRequest<T>(
            runEvent: request.RunEvent,
            rebind: static values => Get<T>(policies: [.. values]),
            scripted: request.Script);
    }

    private static CommandInputRequest<T> Build<T>(CommandInputPolicy policy, Seq<CommandInputPolicy> policies) =>
        typeof(T) switch {
            Type t when t == typeof(CommandSelection) => Objects<T>(policy: policy, policies: policies),
            Type t when t == typeof(CommandOptionValue) => Getter(policy: policy, create: static () => new GetOption(), receive: static g => g.Get(), project: static (_, _, _) => (Value: Option<T>.None, Trim: Option<CommandSelection.TrimResult>.None)),
            Type t when t == typeof(CommandPoint) || t == typeof(Point3d) || t == typeof(DrawingPoint) => Point<T>(policy: policy),
            Type t when t == typeof(Transform) => Transform<T>(policy: policy),
            Type t when t == typeof(double) => Scalar<T>(policy: policy),
            Type t when t == typeof(string) => Text<T>(policy: policy),
            Type t when t == typeof(int) => Number<GetInteger, int, T>(policy: policy, create: static () => new GetInteger(), receive: static g => g.Get(), current: static g => g.Number(), setLower: static (g, v, s) => g.SetLowerLimit(v, s), setUpper: static (g, v, s) => g.SetUpperLimit(v, s)),
            Type t when t == typeof(bool) => Native(run: () => Bool<T>(prompt: policy.PromptText.IfNone(string.Empty), acceptNothing: policy.AcceptsNothing)),
            Type t when t == typeof(Line) => Native<T, Line>(get: static (out value) => RhinoGet.GetLine(line: out value)),
            Type t when t == typeof(Polyline) => Native<T, Polyline>(get: static (out value) => RhinoGet.GetPolyline(polyline: out value)),
            Type t when t == typeof(Circle) => Native<T, Circle>(get: static (out value) => RhinoGet.GetCircle(circle: out value)),
            Type t when t == typeof(Arc) => Native<T, Arc>(get: static (out value) => RhinoGet.GetArc(arc: out value)),
            Type t when t == typeof(Plane) => Native<T, Plane>(get: static (out value) => RhinoGet.GetPlane(plane: out value)),
            Type t when t == typeof(Seq<Point3d>) => Native(run: () => Rectangle<T>(prompt: policy.PromptText.IfNone(string.Empty))),
            Type t when t == typeof(Box) => Native(run: () => Box<T>(spec: policy.BoxMode.IfNone(new CommandInputPolicy.BoxSpec(Mode: GetBoxMode.All, BasePoint: null, Prompt1: null, Prompt2: null, Prompt3: null)))),
            Type t when t == typeof(Color) => Native(run: () => Color<T>(prompt: policy.PromptText.IfNone(string.Empty), acceptNothing: policy.AcceptsNothing, seed: policy.ColorSeed)),
            Type t when t == typeof(RhinoView) => Native(run: () => View<T>(prompt: policy.PromptText.IfNone(string.Empty))),
            Type t when t == typeof(RhinoViewport[]) => Native(run: () => Viewports<T>(prompt: policy.PromptText.IfNone(string.Empty))),
            _ => Invalid<T>(name: nameof(Get)),
        };

    private static CommandInputRequest<T> Scalar<T>(CommandInputPolicy policy) =>
        !policy.IsLiteralText && policy.ScalarMode.Map(static scalar => scalar.Kind is CommandInputPolicy.ScalarKind.Number).IfNone(noneValue: true)
            ? Number<GetNumber, double, T>(policy: policy, create: static () => new GetNumber(), receive: static getter => getter.Get(), current: static getter => getter.Number(), setLower: static (getter, value, strict) => getter.SetLowerLimit(lowerLimit: value, strictlyGreaterThan: strict), setUpper: static (getter, value, strict) => getter.SetUpperLimit(upperLimit: value, strictlyLessThan: strict))
            : Text<T>(policy: policy);

    private sealed class CommandTransformGetter(Func<RhinoViewport, Point3d, Transform> calculate) : GetTransform {
        public override Transform CalculateTransform(RhinoViewport viewport, Point3d point) => Optional(calculate).Map(project => project(arg1: viewport, arg2: point)).IfNone(Transform.Identity);
    }

    private delegate Result NativeGetter<TNative>(out TNative value);

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

    private static CommandInputRequest<T> Transform<T>(CommandInputPolicy policy) =>
        Getter(policy: policy, create: () => new CommandTransformGetter(calculate: policy.TransformMode.IfNone(static (_, _) => global::Rhino.Geometry.Transform.Identity)), receive: static getter => getter.GetXform(),
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
        new(runEvent: input => Optional(input).ToFin(Fail: Op.Of(name: nameof(Get)).InvalidInput()).Bind(valid => UI.RhinoUi.Protect(valid: () => {
            using TGetter getter = create();
            Atom<Option<Error>> eventFault = Atom(Option<Error>.None);
            return from configured in configure is Func<TGetter, Fin<Unit>> apply ? apply(arg: getter) : Fin.Succ(value: unit)
                   from applied in policy.Apply(getter: getter)
                   from pointEvents in getter is GetPoint point ? BindPointEvents(document: valid.Document, getter: point, events: policy.PointEvent, phases: policy.PointEventPhases, gumball: policy.Gumball, fullFrameRedraw: policy.FullFrameRedrawDuringGet, fault: eventFault) : Fin.Succ(value: Subscription.Nothing)
                   from result in Fin.Succ(value: unit).Bind(_ => {
                       // BOUNDARY ADAPTER — point-event detachers must close even when option binding fails, before the getter disposes.
                       try {
                           return CommandOption.Bind(options: policy.OptionList, getter: getter).Bind(scope => {
                               using CommandOption.Scope active = scope;
                               return ReadLoop(getter: getter, scope: active, receive: () => receive(arg: getter), project: (g, raw) => project(arg1: valid, arg2: g, arg3: raw), selected: Option<CommandOptionValue>.None, acceptUndo: policy.AcceptsUndo, transition: transition ?? (static (_, _, selected) => (Continue: false, Selected: selected)));
                           });
                       } finally {
                           pointEvents.Dispose();
                       }
                   })
                   from checkedResult in eventFault.Value.Case switch {
                       Error error => Fin.Fail<CommandInputEvent<T>>(error: error),
                       _ => Fin.Succ(value: result),
                   }
                   select checkedResult;
        })), rebind: null, scripted: (input, token) => Script<T>(input: input, token: token, policy: policy));

    private static Fin<Subscription> BindPointEvents(RhinoDoc document, GetPoint getter, Option<Func<CommandPointEvent, Fin<Unit>>> events, CommandPointEventPhase phases, Option<UiGumball> gumball, bool fullFrameRedraw, Atom<Option<Error>> fault) =>
        events.Map(change => {
            CommandPointEventPhase activePhases = phases | (fullFrameRedraw ? CommandPointEventPhase.PostDrawObjects : CommandPointEventPhase.None);
            Unit Apply(CommandPointEvent pointEvent) =>
                UI.RhinoUi.Protect(valid: () => change(arg: pointEvent)).Match(Succ: static _ => unit, Fail: error => {
                    _ = fault.Swap(_ => Some(error));
                    _ = getter.InterruptMouseMove();
                    return unit;
                });
            Subscription Sub<TArgs>(Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, CommandPointEventPhase phase, Func<TArgs, (Option<GetPointMouseEventArgs> Mouse, Option<GetPointDrawEventArgs> Draw, Option<DrawEventArgs> Post)> project) =>
                Subscription.Attach<TArgs>(active: (activePhases & phase) == phase, subscribe: add, unsubscribe: remove, handle: args => {
                    (Option<GetPointMouseEventArgs> mouse, Option<GetPointDrawEventArgs> draw, Option<DrawEventArgs> post) = project(arg: args);
                    _ = Apply(pointEvent: new CommandPointEvent(Phase: phase, Document: document, Getter: getter, Mouse: mouse, Draw: draw, PostDraw: post, Gumball: gumball));
                    return Fin.Succ(value: unit);
                });
            bool postDraw = (activePhases & CommandPointEventPhase.PostDrawObjects) == CommandPointEventPhase.PostDrawObjects;
            getter.FullFrameRedrawDuringGet = postDraw;
            return Fin.Succ(value:
                Sub<GetPointMouseEventArgs>(h => getter.MouseMove += h, h => getter.MouseMove -= h, CommandPointEventPhase.MouseMove, static a => (Some(a), Option<GetPointDrawEventArgs>.None, Option<DrawEventArgs>.None))
                | Sub<GetPointMouseEventArgs>(h => getter.MouseDown += h, h => getter.MouseDown -= h, CommandPointEventPhase.MouseDown, static a => (Some(a), Option<GetPointDrawEventArgs>.None, Option<DrawEventArgs>.None))
                | Sub<GetPointDrawEventArgs>(h => getter.DynamicDraw += h, h => getter.DynamicDraw -= h, CommandPointEventPhase.DynamicDraw, static a => (Option<GetPointMouseEventArgs>.None, Some(a), Option<DrawEventArgs>.None))
                | (postDraw
                    ? Sub<DrawEventArgs>(h => getter.PostDrawObjects += h, h => getter.PostDrawObjects -= h, CommandPointEventPhase.PostDrawObjects, static a => (Option<GetPointMouseEventArgs>.None, Option<GetPointDrawEventArgs>.None, Some(a)))
                    : Subscription.Nothing));
        }).IfNone(Fin.Succ(value: Subscription.Nothing));

    private static Fin<CommandInputEvent<T>> ReadLoop<TGetter, T>(
        TGetter getter,
        CommandOption.Scope scope,
        Func<GetResult> receive,
        Func<TGetter, GetResult, (Option<T> Value, Option<CommandSelection.TrimResult> Trim)> project,
        Option<CommandOptionValue> selected,
        bool acceptUndo,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)> transition) where TGetter : GetBaseClass =>
        receive() switch {
            GetResult.Cancel => Fin.Succ(value: CommandInputEvent<T>.Cancelled),
            GetResult.ExitRhino => Fin.Succ(value: CommandInputEvent<T>.Exit),
            GetResult.Option => scope.Selected(getter: getter).Bind(option => transition(getter, GetResult.Option, Some(option)) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, project: project, selected: next, acceptUndo: acceptUndo, transition: transition),
                (false, Option<CommandOptionValue> next) => Read(getter: getter, raw: GetResult.Option, projected: project(getter, GetResult.Option), option: next),
            }),
            GetResult.Undo when acceptUndo => Read(getter: getter, raw: GetResult.Undo, projected: (Option<T>.None, Option<CommandSelection.TrimResult>.None), option: selected),
            GetResult.Undo => Fin.Fail<CommandInputEvent<T>>(error: Op.Of(name: nameof(ReadLoop)).InvalidResult()),
            GetResult raw and (GetResult.NoResult or GetResult.Nothing or GetResult.Miss or GetResult.Timeout) => Read(getter: getter, raw: raw, projected: (Option<T>.None, Option<CommandSelection.TrimResult>.None), option: selected),
            GetResult raw => transition(getter, raw, selected) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, project: project, selected: next, acceptUndo: acceptUndo, transition: transition),
                (false, Option<CommandOptionValue> next) => Read(getter: getter, raw: raw, projected: project(getter, raw), option: next),
            },
        };

    private static Fin<CommandInputEvent<T>> Read<TGetter, T>(TGetter getter, GetResult raw, (Option<T> Value, Option<CommandSelection.TrimResult> Trim) projected, Option<CommandOptionValue> option) where TGetter : GetBaseClass {
        Option<T> value = projected.Value | option.Bind(static selected => selected is T selectedValue ? Some(selectedValue) : Option<T>.None);
        CommandGet<T> snapshot = CommandGet<T>.Of(getter: getter, raw: raw, value: value, option: option, selectionTrim: projected.Trim);
        bool requiresValue = raw is GetResult.Object or GetResult.Point or GetResult.Point2d or GetResult.Number or GetResult.String
            or GetResult.Color or GetResult.Rectangle2d or GetResult.Line2d or GetResult.Circle or GetResult.Plane
            or GetResult.Cylinder or GetResult.Sphere or GetResult.Angle or GetResult.Distance
            or GetResult.Direction or GetResult.Frame or GetResult.CustomMessage;
        bool hasValue = value.IsSome || projected.Trim.IsSome || (raw == GetResult.Option && option.IsSome) || snapshot.Snapshot.Has(raw: raw);
        return (requiresValue, hasValue) switch {
            (true, false) => Fin.Fail<CommandInputEvent<T>>(error: Op.Of(name: nameof(Read)).InvalidResult()),
            _ => Fin.Succ(value: CommandInputEvent<T>.Of(result: snapshot)),
        };
    }

    private static CommandInputRequest<T> Native<T>(Func<(Result Result, Option<T> Value)> run) =>
        new(input => Optional(input).ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput()).Bind(_ => UI.RhinoUi.Protect(valid: () => run() switch {
            (Result.Success, Option<T> value) => Fin.Succ(value: CommandGet<T>.Native(result: Result.Success, value: value)),
            (Result.Cancel or Result.CancelModelessDialog or Result.ExitRhino, _) => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            (Result.Failure or Result.UnknownCommand, _) => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Native)).InvalidResult()),
            (Result result, _) => Fin.Succ(value: CommandGet<T>.Native(result: result, value: Option<T>.None)),
        })), scripted: (input, token) => Script<T>(input: input, token: token));

    private static Fin<CommandInputEvent<T>> Script<T>(CommandInput input, string token, CommandInputPolicy? policy = null) =>
        from source in Optional(input).ToFin(Fail: Op.Of(name: nameof(Script)).InvalidInput())
        from text in Optional(token).ToFin(Fail: Op.Of(name: nameof(Script)).InvalidInput())
        let activePolicy = Optional(policy).IfNone(CommandInputPolicy.Empty)
        from options in CommandOption.Validate(options: activePolicy.OptionList, op: Op.Of(name: nameof(Script)))
        from result in CommandOption.Script(options: options, token: text).Case switch {
            CommandOptionValue option => Fin.Succ(value: CommandGet<T>.ScriptOption(option: option)),
            _ => from value in ScriptValue<T>(input: source, text: text, policy: activePolicy).ToFin(Fail: Op.Of(name: nameof(Script)).InvalidResult())
                 select CommandGet<T>.Native(result: Result.Success, value: Some(value)),
        }
        select CommandInputEvent<T>.Of(result: result);

    private static Option<T> ScriptValue<T>(CommandInput input, string text, CommandInputPolicy policy) =>
        typeof(T) switch {
            Type t when t == typeof(string) => Cast<T>(text),
            Type t when t == typeof(bool) => CommandOption.BoolValue(value: text).Bind(value => Cast<T>(value)),
            Type t when t == typeof(Color) => CommandOption.ColorValue(text: text).Bind(value => Cast<T>(value)),
            Type t when t == typeof(double) => Parse<T>(input: input, text: text, scalar: policy.ScalarMode, bounds: policy.LimitsMode).Bind(Cast<T>),
            Type t when t == typeof(int) => int.TryParse(s: text, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, result: out int value) ? policy.LimitsMode.Case switch { CommandInputPolicy.LimitSpec spec => spec.Accept(value: value).Bind(valid => Cast<T>(valid)), _ => Cast<T>(value) } : Option<T>.None,
            Type t when t == typeof(CommandSelection) => ScriptSelection<T>(input: input, text: text, policy: policy),
            Type t when t == typeof(CommandOptionValue) => Option<T>.None,
            _ => Option<T>.None,
        };

    private static CommandInputRequest<T> Native<T, TNative>(NativeGetter<TNative> get) =>
        Native(run: () => {
            Result result = get(value: out TNative value);
            return (Result: result, Value: result == Result.Success ? Cast<T>(value!) : Option<T>.None);
        });

    private static (Result Result, Option<T> Value) Rectangle<T>(string prompt) { Result result = string.IsNullOrWhiteSpace(value: prompt) ? RhinoGet.GetRectangle(corners: out Point3d[] corners) : RhinoGet.GetRectangle(firstPrompt: prompt, corners: out corners); return (Result: result, Value: result == Result.Success ? Cast<T>(toSeq(corners)) : Option<T>.None); }

    private static (Result Result, Option<T> Value) Box<T>(CommandInputPolicy.BoxSpec spec) { Result result = spec is { Mode: GetBoxMode.All, BasePoint: null, Prompt1: null, Prompt2: null, Prompt3: null } ? RhinoGet.GetBox(box: out Box value) : RhinoGet.GetBox(box: out value, mode: spec.Mode, basePoint: spec.BasePoint ?? Point3d.Unset, prompt1: spec.Prompt1, prompt2: spec.Prompt2, prompt3: spec.Prompt3); return (Result: result, Value: result == Result.Success ? Cast<T>(value) : Option<T>.None); }

    private static (Result Result, Option<T> Value) Color<T>(string prompt, bool acceptNothing, Option<Color> seed) { Color color = seed.IfNone(System.Drawing.Color.Empty); Result result = RhinoGet.GetColor(prompt: prompt, acceptNothing: acceptNothing, color: ref color); return (Result: result, Value: result == Result.Success ? Cast<T>(color) : Option<T>.None); }

    private static (Result Result, Option<T> Value) View<T>(string prompt) { Result result = RhinoGet.GetView(commandPrompt: prompt, view: out RhinoView view); return (Result: result, Value: result == Result.Success ? Cast<T>(view) : Option<T>.None); }

    private static (Result Result, Option<T> Value) Viewports<T>(string prompt) { Result result = RhinoGet.GetViewports(commandPrompt: prompt, viewports: out RhinoViewport[] viewports); return (Result: result, Value: result == Result.Success ? Cast<T>(viewports) : Option<T>.None); }

    private static (Result Result, Option<T> Value) Bool<T>(string prompt, bool acceptNothing) { bool value = false; Result result = RhinoGet.GetBool(prompt: prompt, acceptNothing: acceptNothing, offPrompt: "No", onPrompt: "Yes", boolValue: ref value); return (Result: result, Value: result == Result.Success ? Cast<T>(value) : Option<T>.None); }

    private static CommandInputRequest<T> Invalid<T>(string name) => new(run: _ => Fin.Fail<CommandGet<T>>(error: Op.Of(name: name).InvalidInput()));

    private static Option<CommandSelection> SelectionOf(RhinoDoc document, GetObject getter, GetResult raw) =>
        raw is GetResult.Object && getter.Objects() is ObjRef[] references
            ? Optional(CommandSelection.From(
                document: document,
                references: toSeq(references),
                preselected: getter.ObjectsWerePreselected
                    ? toSeq(references).Filter(static reference => Optional(reference.Object()).Map(static item => item.IsSelected(checkSubObjects: true) > 0).IfNone(noneValue: false)).Map(static reference => (reference.ObjectId, reference.GeometryComponentIndex))
                    : Seq<(Guid ObjectId, ComponentIndex ComponentIndex)>()))
            : Option<CommandSelection>.None;

    private static Option<CommandSelection.TrimResult> SelectionTrim(CommandInput source, GetObject getter, GetResult raw, CommandObjectSelection policy) =>
        SelectionOf(document: source.Document, getter: getter, raw: raw)
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

    private static Option<double> ParseNumber(string text) { StringParserSettings results = null!; return StringParser.ParseNumber(expression: text, max_count: text.Length, settings_in: StringParserSettings.DefaultParseSettings, settings_out: ref results, answer: out double value) == text.Length ? Some(value) : Option<double>.None; }

    private static Option<double> ParseLength(string text, UnitSystem units) {
        // BOUNDARY ADAPTER — LengthValue is native disposable state; `using` releases it on every return path.
        using LengthValue value = LengthValue.Create(s: text, ps: StringParserSettings.DefaultParseSettings, parsedAll: out bool parsedAll);
        return parsedAll && !value.IsUnset() ? Some(value.Length(units: units)) : Option<double>.None;
    }

    private static Option<double> ParseAngle(string text, AngleUnitSystem units) { StringParserSettings results = null!; AngleUnitSystem parsed = AngleUnitSystem.None; return StringParser.ParseAngleExpession(text, 0, text.Length, StringParserSettings.DefaultParseSettings, units, out double value, ref results, ref parsed) == text.Length ? Some(value) : Option<double>.None; }

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
                select Op.Side(() => {
                    _ = projected.Lower.Iter(value => setLower(arg1: valid, arg2: value, arg3: b.StrictlyLower));
                    _ = projected.Upper.Iter(value => setUpper(arg1: valid, arg2: value, arg3: b.StrictlyUpper));
                }),
            _ => Fin.Succ(value: unit),
        };
}
