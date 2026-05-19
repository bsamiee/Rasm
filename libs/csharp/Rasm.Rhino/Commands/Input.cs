using System.Globalization;
using Color = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using Result = Rhino.Commands.Result;

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

    public Option<TOut> As<TOut>() =>
        Value.Bind(static value => value is TOut typed ? Some(typed) : Option<TOut>.None);

    public bool IsUndo => Raw.Map(static raw => raw == GetResult.Undo).IfNone(false);
}

public sealed record CommandInputRequest<T> {
    private readonly Func<CommandInput, Fin<CommandGet<T>>> run;

    internal CommandInputRequest(Func<CommandInput, Fin<CommandGet<T>>> run) => this.run = run;

    internal Fin<CommandGet<T>> Run(CommandInput input) => run(arg: input);
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

public sealed record CommandInputPolicy {
    private CommandInputPolicy(
        Seq<Action<GetBaseClass>> baseActions = default,
        Seq<Action<GetObject>> objectActions = default,
        Seq<Action<GetPoint>> pointActions = default,
        Seq<CommandOption> options = default,
        Option<(int Min, int Max)> objects = default,
        Option<PointSpec> point = default,
        Option<Scalar> scalar = default,
        Option<LimitSpec> bounds = default,
        Option<BoxSpec> box = default,
        Option<ObjectType> objectTypes = default,
        Option<string> prompt = default,
        bool literalText = false,
        CommandInputAccept accept = CommandInputAccept.None) {
        BaseActions = baseActions; ObjectActions = objectActions; PointActions = pointActions; OptionList = options; ObjectRange = objects; PointMode = point; ScalarMode = scalar; LimitsMode = bounds; BoxMode = box; ObjectTypes = objectTypes; PromptText = prompt; IsLiteralText = literalText; AcceptModes = accept;
    }

    private Seq<Action<GetBaseClass>> BaseActions { get; }
    private Seq<Action<GetObject>> ObjectActions { get; }
    private Seq<Action<GetPoint>> PointActions { get; }
    internal Seq<CommandOption> OptionList { get; }
    internal Option<(int Min, int Max)> ObjectRange { get; }
    internal Option<PointSpec> PointMode { get; }
    internal Option<Scalar> ScalarMode { get; }
    internal Option<LimitSpec> LimitsMode { get; }
    internal Option<BoxSpec> BoxMode { get; }
    internal Option<ObjectType> ObjectTypes { get; }
    internal Option<string> PromptText { get; }
    internal bool IsLiteralText { get; }
    internal CommandInputAccept AcceptModes { get; }
    internal bool AcceptsNothing => Accepts(mode: CommandInputAccept.Nothing);
    internal bool AcceptsUndo => Accepts(mode: CommandInputAccept.Undo);
    internal static CommandInputPolicy Empty { get; } = new();

    public static CommandInputPolicy Configure(Action<GetBaseClass> apply) => new(baseActions: Seq(apply));
    public static CommandInputPolicy ConfigureObject(Action<GetObject> apply) => new(objectActions: Seq(apply));
    public static CommandInputPolicy ConfigurePoint(Action<GetPoint> apply) => new(pointActions: Seq(apply));
    public static CommandInputPolicy Prompt(string value) => new(baseActions: Seq<Action<GetBaseClass>>(getter => getter.SetCommandPrompt(value)), prompt: Optional(value));
    public static CommandInputPolicy Default(object value) =>
        value switch {
            Point3d point => Configure(apply: getter => getter.SetDefaultPoint(point: point)),
            double number => Configure(apply: getter => getter.SetDefaultNumber(number)),
            int integer => Configure(apply: getter => getter.SetDefaultInteger(integer)),
            string text => Configure(apply: getter => getter.SetDefaultString(text)),
            Color color => Configure(apply: getter => getter.SetDefaultColor(color)),
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
    public static CommandInputPolicy AcceptNothing() => Accept(modes: CommandInputAccept.Nothing);
    public static CommandInputPolicy AcceptUndo() => Accept(modes: CommandInputAccept.Undo);
    public static CommandInputPolicy Options(params CommandOption[] values) => new(options: toSeq(values));
    public static CommandInputPolicy Objects(int minimum = 1, int maximum = 1, ObjectType types = ObjectType.AnyObject) =>
        new(
            objects: Some((Min: minimum, Max: maximum)),
            objectTypes: types switch {
                ObjectType.AnyObject => Option<ObjectType>.None,
                _ => Some(types),
            });
    public static CommandInputPolicy Point(
        bool onMouseUp = false,
        bool twoDimensional = false,
        Option<global::Rhino.UI.CursorStyle> cursor = default,
        Option<bool> objectSnapCursors = default,
        Option<Point3d> basePoint = default,
        bool drawLineFromBasePoint = false,
        bool snapToCurves = false,
        bool permitConstraintOptions = true) =>
        new(point: Some(DefaultPointSpec with { OnMouseUp = onMouseUp, TwoDimensional = twoDimensional, Cursor = cursor, ObjectSnapCursors = objectSnapCursors, BasePoint = basePoint, DrawLineFromBasePoint = drawLineFromBasePoint, SnapToCurves = snapToCurves, PermitConstraintOptions = permitConstraintOptions }));
    public static CommandInputPolicy Bounds<T>(Option<T> lower = default, Option<T> upper = default, bool strictlyLower = false, bool strictlyUpper = false) where T : IComparable<T> => new(bounds: Some(new LimitSpec(Lower: lower.Map(static value => (object)value), Upper: upper.Map(static value => (object)value), StrictlyLower: strictlyLower, StrictlyUpper: strictlyUpper)));
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
        return new(baseActions: left.BaseActions + right.BaseActions, objectActions: left.ObjectActions + right.ObjectActions, pointActions: left.PointActions + right.PointActions, options: left.OptionList + right.OptionList, objects: Pick(left.ObjectRange, right.ObjectRange), point: Pick(left.PointMode, right.PointMode), scalar: Pick(left.ScalarMode, right.ScalarMode), bounds: Pick(left.LimitsMode, right.LimitsMode), box: Pick(left.BoxMode, right.BoxMode), objectTypes: Pick(left.ObjectTypes, right.ObjectTypes), prompt: Pick(left.PromptText, right.PromptText), literalText: left.IsLiteralText || right.IsLiteralText, accept: left.AcceptModes | right.AcceptModes);
    }

    internal static CommandInputPolicy Merge(Seq<CommandInputPolicy> policies) =>
        policies.Fold(Empty, static (state, policy) => state + policy);

    internal Fin<Unit> Apply<TGetter>(TGetter getter) where TGetter : GetBaseClass =>
        Optional(getter).ToFin(Fail: Op.Of(name: nameof(CommandInputPolicy)).InvalidInput()).Map(valid => {
            _ = BaseActions.Iter(action => action(obj: valid));
            _ = valid is GetObject objects ? ApplyObject(getter: objects) : unit;
            _ = valid is GetPoint point ? ApplyPoint(getter: point) : unit;
            return unit;
        });

    private static Seq<Action<GetObject>> DefaultObjectActions { get; } = Seq<Action<GetObject>>(
        static getter => getter.EnablePreSelect(enable: true, ignoreUnacceptablePreselectedObjects: true),
        static getter => getter.EnablePostSelect(enable: true),
        static getter => getter.SubObjectSelect = true,
        static getter => getter.ReferenceObjectSelect = true,
        static getter => getter.EnableSelPrevious(enable: true),
        static getter => getter.EnableHighlight(enable: true),
        static getter => getter.EnableIgnoreGrips(enable: true),
        static getter => getter.EnableClearObjectsOnEntry(enable: true),
        static getter => getter.EnableUnselectObjectsOnExit(enable: true),
        static getter => getter.EnablePressEnterWhenDonePrompt(enable: true));

    private bool Accepts(CommandInputAccept mode) => (AcceptModes & mode) == mode;
    private static Option<T> Pick<T>(Option<T> left, Option<T> right) => right | left;
    internal enum ScalarKind { Number, Length, Angle }
    internal sealed record PointSpec(bool OnMouseUp, bool TwoDimensional, Option<global::Rhino.UI.CursorStyle> Cursor, Option<bool> ObjectSnapCursors, Option<Point3d> BasePoint, bool DrawLineFromBasePoint, bool SnapToCurves, bool PermitConstraintOptions);
    internal static PointSpec DefaultPointSpec { get; } = new(OnMouseUp: false, TwoDimensional: false, Cursor: Option<global::Rhino.UI.CursorStyle>.None, ObjectSnapCursors: Option<bool>.None, BasePoint: Option<Point3d>.None, DrawLineFromBasePoint: false, SnapToCurves: false, PermitConstraintOptions: true);
    internal sealed record Scalar(ScalarKind Kind, Option<UnitSystem> LengthUnits, Option<AngleUnitSystem> AngleUnits);
    internal sealed record LimitSpec(Option<object> Lower, Option<object> Upper, bool StrictlyLower, bool StrictlyUpper);
    internal sealed record BoxSpec(GetBoxMode Mode, Point3d? BasePoint, string? Prompt1, string? Prompt2, string? Prompt3);

    private Unit ApplyPoint(GetPoint getter) {
        _ = PointActions.Iter(action => action(obj: getter));
        _ = PointMode.Iter(spec => {
            _ = spec.Cursor.Iter(cursor => getter.SetCursor(cursor));
            _ = spec.ObjectSnapCursors.Iter(enabled => getter.EnableObjectSnapCursors(enable: enabled));
            getter.PermitConstraintOptions(spec.PermitConstraintOptions);
            getter.EnableSnapToCurves(enable: spec.SnapToCurves);
            _ = spec.BasePoint.Iter(point => {
                getter.SetBasePoint(point, true);
                _ = spec.DrawLineFromBasePoint switch {
                    true => ((Func<Unit>)(() => { getter.DrawLineFromPoint(point, true); return unit; }))(),
                    false => unit,
                };
            });
        });
        return unit;
    }

    private Unit ApplyObject(GetObject getter) {
        _ = ObjectTypes.Iter(types => getter.GeometryFilter = types);
        _ = DefaultObjectActions.Concat(ObjectActions).Iter(action => action(obj: getter));
        return unit;
    }
}

public static class CommandInputs {
    public static CommandInputRequest<T> Get<T>(params CommandInputPolicy[] policies) {
        Seq<CommandInputPolicy> active = toSeq(policies);
        CommandInputPolicy policy = CommandInputPolicy.Merge(policies: active);
        return typeof(T) switch {
            Type t when t == typeof(CommandSelection) => Objects<T>(policy: policy, policies: active),
            Type t when t == typeof(CommandOptionValue) => Getter<GetOption, T>(policy: policy, create: static () => new GetOption(), receive: static getter => getter.Get(), value: static (_, _, _) => Option<T>.None),
            Type t when t == typeof(CommandPoint) || t == typeof(Point3d) || t == typeof(DrawingPoint) => Point<T>(policy: policy),
            Type t when t == typeof(string) || t == typeof(double) => Text<T>(policy: policy),
            Type t when t == typeof(int) => Number<GetInteger, int, T>(policy: policy, create: static () => new GetInteger(), receive: static getter => getter.Get(), current: static getter => getter.Number(), setLower: static (getter, value, strict) => getter.SetLowerLimit(value, strict), setUpper: static (getter, value, strict) => getter.SetUpperLimit(value, strict)),
            Type t when t == typeof(Line) => Native<T>(run: static () => GetNative<T, Line>(static (out value) => RhinoGet.GetLine(line: out value))),
            Type t when t == typeof(Polyline) => Native<T>(run: static () => GetNative<T, Polyline>(static (out value) => RhinoGet.GetPolyline(polyline: out value))),
            Type t when t == typeof(Circle) => Native<T>(run: static () => GetNative<T, Circle>(static (out value) => RhinoGet.GetCircle(circle: out value))),
            Type t when t == typeof(Arc) => Native<T>(run: static () => GetNative<T, Arc>(static (out value) => RhinoGet.GetArc(arc: out value))),
            Type t when t == typeof(Plane) => Native<T>(run: static () => GetNative<T, Plane>(static (out value) => RhinoGet.GetPlane(plane: out value))),
            Type t when t == typeof(Seq<Point3d>) => Native<T>(run: () => Rectangle<T>(prompt: policy.PromptText.IfNone(string.Empty))),
            Type t when t == typeof(Box) => Native<T>(run: () => Box<T>(spec: policy.BoxMode.IfNone(new CommandInputPolicy.BoxSpec(Mode: GetBoxMode.All, BasePoint: null, Prompt1: null, Prompt2: null, Prompt3: null)))),
            Type t when t == typeof(Color) => Native<T>(run: () => Color<T>(prompt: policy.PromptText.IfNone(string.Empty), acceptNothing: policy.AcceptsNothing)),
            _ => Invalid<T>(name: nameof(Get)),
        };
    }

    private delegate Result NativeGetter<TNative>(out TNative value);

    private static CommandInputRequest<T> Objects<T>(CommandInputPolicy policy, Seq<CommandInputPolicy> policies) =>
        policy.ObjectRange.IfNone((Min: 1, Max: 1)) switch {
            ( < 0, _) or (_, < -1) => Invalid<T>(name: nameof(Get)),
            (int lo, int hi) when hi > 0 && hi < lo => Invalid<T>(name: nameof(Get)),
            (int lo, int hi) => Getter<GetObject, T>(
                policy: CommandInputPolicy.Merge(policies: Seq(lo == 0 ? CommandInputPolicy.AcceptNothing() : CommandInputPolicy.Empty, hi == 0 ? CommandInputPolicy.Accept(modes: CommandInputAccept.EnterWhenDone) : CommandInputPolicy.Empty) + policies),
                create: static () => new GetObject(),
                receive: getter => getter.GetMultiple(minimumNumber: lo, maximumNumber: hi),
                value: static (source, getter, raw) => SelectionOf(document: source.Document, getter: getter, raw: raw).Bind(Cast<T>),
                transition: static (getter, raw, selected) => raw is GetResult.Option ? ForcePostSelect(getter: getter, selected: selected) : (Continue: false, Selected: selected)),
        };

    private static CommandInputRequest<T> Point<T>(CommandInputPolicy policy) {
        CommandInputPolicy.PointSpec spec = policy.PointMode.IfNone(CommandInputPolicy.DefaultPointSpec); bool twoDimensional = spec.TwoDimensional || typeof(T) == typeof(DrawingPoint);
        return Getter<GetPoint, T>(
            policy: policy,
            create: static () => new GetPoint(),
            receive: getter => getter.Get(onMouseUp: spec.OnMouseUp, get2DPoint: twoDimensional),
            value: static (_, getter, raw) => raw is GetResult.Point or GetResult.Point2d ? PointValue<T>(getter: getter, raw: raw) : Option<T>.None);
    }

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
        new(run: input => Optional(input).ToFin(Fail: Op.Of(name: nameof(Get)).InvalidInput()).Bind(valid => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => {
            using TGetter getter = create();
            return from configured in configure is Func<TGetter, Fin<Unit>> apply ? apply(arg: getter) : Fin.Succ(value: unit)
                   from applied in policy.Apply(getter: getter)
                   from result in CommandOption.Bind(options: policy.OptionList, getter: getter).Bind(scope => {
                       using CommandOption.Scope active = scope;
                       return ReadLoop(getter: getter, scope: active, receive: () => receive(arg: getter), value: (g, raw) => value(arg1: valid, arg2: g, arg3: raw), selected: Option<CommandOptionValue>.None, acceptUndo: policy.AcceptsUndo, transition: transition ?? (static (_, _, selected) => (Continue: false, Selected: selected)));
                   })
                   select result;
        })));

    private static Fin<CommandGet<T>> ReadLoop<TGetter, T>(
        TGetter getter,
        CommandOption.Scope scope,
        Func<GetResult> receive,
        Func<TGetter, GetResult, Option<T>> value,
        Option<CommandOptionValue> selected,
        bool acceptUndo,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)> transition) where TGetter : GetBaseClass =>
        receive() switch {
            GetResult.Cancel or GetResult.ExitRhino => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            GetResult.Option => scope.Selected(getter: getter).Bind(option => transition(getter, GetResult.Option, Some(option)) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, acceptUndo: acceptUndo, transition: transition),
                (false, Option<CommandOptionValue> next) => Read(getter: getter, raw: GetResult.Option, value: value(getter, GetResult.Option), option: next),
            }),
            GetResult.Undo when acceptUndo => Read(getter: getter, raw: GetResult.Undo, value: Option<T>.None, option: selected),
            GetResult.Undo => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(ReadLoop)).InvalidResult()),
            GetResult raw and (GetResult.NoResult or GetResult.Nothing or GetResult.Miss or GetResult.Timeout) => Read(getter: getter, raw: raw, value: Option<T>.None, option: selected),
            GetResult raw => transition(getter, raw, selected) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, acceptUndo: acceptUndo, transition: transition),
                (false, Option<CommandOptionValue> next) => Read(getter: getter, raw: raw, value: value(getter, raw), option: next),
            },
        };

    private static Fin<CommandGet<T>> Read<TGetter, T>(TGetter getter, GetResult raw, Option<T> value, Option<CommandOptionValue> option) where TGetter : GetBaseClass {
        Option<T> projected = value | option.Bind(static selected => selected is T selectedValue ? Some(selectedValue) : Option<T>.None);
        CommandGet<T> snapshot = CommandGet<T>.Of(getter: getter, raw: raw, value: projected, option: option);
        return (raw, projected.IsSome || (raw == GetResult.Option && option.IsSome) || snapshot.Snapshot.Has(raw: raw)) switch {
            (GetResult.Object or GetResult.Point or GetResult.Point2d or GetResult.Number or GetResult.String or GetResult.Color or GetResult.Rectangle2d or GetResult.Line2d, false) => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Read)).InvalidResult()),
            _ => Fin.Succ(value: snapshot),
        };
    }

    private static CommandInputRequest<T> Native<T>(Func<(Result Result, Option<T> Value)> run) =>
        new(input => Optional(input).ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput()).Bind(_ => Rasm.Rhino.UI.RhinoUi.Protect(valid: () => run() switch {
            (Result.Success, Option<T> value) => Fin.Succ(value: CommandGet<T>.Native(result: Result.Success, value: value)),
            (Result.Cancel or Result.CancelModelessDialog, _) => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            (Result.Failure or Result.UnknownCommand, _) => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Native)).InvalidResult()),
            (Result result, _) => Fin.Succ(value: CommandGet<T>.Native(result: result, value: Option<T>.None)),
        })));

    private static (Result Result, Option<T> Value) GetNative<T, TNative>(NativeGetter<TNative> get) {
        Result result = get(value: out TNative value);
        return (Result: result, Value: result == Result.Success ? Cast<T>(value!) : Option<T>.None);
    }

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

    private static (bool Continue, Option<CommandOptionValue> Selected) ForcePostSelect(GetObject getter, Option<CommandOptionValue> selected) {
        getter.EnablePreSelect(enable: false, ignoreUnacceptablePreselectedObjects: true);
        return (Continue: true, Selected: selected);
    }

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
            CommandInputPolicy.LimitSpec spec => (Lower: spec.Lower.Bind(ScalarBound<double>), Upper: spec.Upper.Bind(ScalarBound<double>)) switch {
                (Option<double> lower, _) when spec.Lower.IsSome && !lower.IsSome => Option<double>.None,
                (_, Option<double> upper) when spec.Upper.IsSome && !upper.IsSome => Option<double>.None,
                (Option<double> lower, _) when lower.Case is double lo && (spec.StrictlyLower ? value <= lo : value < lo) => Option<double>.None,
                (_, Option<double> upper) when upper.Case is double hi && (spec.StrictlyUpper ? value >= hi : value > hi) => Option<double>.None,
                (Option<double> lower, Option<double> upper) when lower.Case is double lo && upper.Case is double hi && lo.CompareTo(value: hi) > 0 => Option<double>.None,
                (Option<double> lower, Option<double> upper) when lower.Case is double lo && upper.Case is double hi && lo.CompareTo(value: hi) == 0 && (spec.StrictlyLower || spec.StrictlyUpper) => Option<double>.None,
                _ => Some(value),
            },
            _ => Some(value),
        };

    private static Fin<Unit> Limits<TGetter, TValue>(TGetter getter, Option<CommandInputPolicy.LimitSpec> bounds, Action<TGetter, TValue, bool> setLower, Action<TGetter, TValue, bool> setUpper) where TGetter : GetBaseClass where TValue : IComparable<TValue> =>
        bounds.Case switch {
            CommandInputPolicy.LimitSpec b => (Lower: b.Lower.Bind(ScalarBound<TValue>), Upper: b.Upper.Bind(ScalarBound<TValue>)) switch {
                (Option<TValue> lower, _) when b.Lower.IsSome && !lower.IsSome => Fin.Fail<Unit>(error: Op.Of(name: nameof(Limits)).InvalidInput()),
                (_, Option<TValue> upper) when b.Upper.IsSome && !upper.IsSome => Fin.Fail<Unit>(error: Op.Of(name: nameof(Limits)).InvalidInput()),
                (Option<TValue> lower, Option<TValue> upper) when lower.Case is TValue left && upper.Case is TValue right && left.CompareTo(other: right) > 0 => Fin.Fail<Unit>(error: Op.Of(name: nameof(Limits)).InvalidInput()),
                (Option<TValue> lower, Option<TValue> upper) when lower.Case is TValue left && upper.Case is TValue right && left.CompareTo(other: right) == 0 && (b.StrictlyLower || b.StrictlyUpper) => Fin.Fail<Unit>(error: Op.Of(name: nameof(Limits)).InvalidInput()),
                (Option<TValue> lower, Option<TValue> upper) => Optional(getter).ToFin(Fail: Op.Of(name: nameof(Limits)).InvalidInput()).Map(valid => {
                    _ = lower.Iter(value => setLower(arg1: valid, arg2: value, arg3: b.StrictlyLower));
                    _ = upper.Iter(value => setUpper(arg1: valid, arg2: value, arg3: b.StrictlyUpper));
                    return unit;
                }),
            },
            _ => Fin.Succ(value: unit),
        };

    private static Option<TValue> ScalarBound<TValue>(object value) where TValue : IComparable<TValue> {
        try {
            return value switch {
                TValue typed => Some(typed),
                IConvertible convertible when typeof(TValue) == typeof(double) => Convert.ToDouble(value: convertible, provider: CultureInfo.InvariantCulture) switch {
                    double number when double.IsFinite(d: number) => Cast<TValue>(value: number),
                    _ => Option<TValue>.None,
                },
                IConvertible convertible when typeof(TValue) == typeof(int) => Convert.ToDouble(value: convertible, provider: CultureInfo.InvariantCulture) switch {
                    double number when double.IsFinite(d: number) && Math.Truncate(d: number) == number && number >= int.MinValue && number <= int.MaxValue => Cast<TValue>(value: (int)number),
                    _ => Option<TValue>.None,
                },
                _ => Option<TValue>.None,
            };
        } catch (FormatException) {
            return Option<TValue>.None;
        } catch (InvalidCastException) {
            return Option<TValue>.None;
        } catch (OverflowException) {
            return Option<TValue>.None;
        }
    }
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
}
