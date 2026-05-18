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
    internal static CommandPoint Of(GetPoint getter, GetResult raw) =>
        new(
            Point: raw is GetResult.Point ? Some(getter.Point()) : Option<Point3d>.None,
            WindowPoint: raw is GetResult.Point or GetResult.Point2d ? Some<DrawingPoint>(getter.Point2d()) : Option<DrawingPoint>.None,
            View: Optional(getter.View()),
            Reference: CommandSelection.Reference.Of(getter: getter),
            NumberPreview: getter.NumberPreview(number: out double number) ? Some(number) : Option<double>.None,
            Osnap: getter.OsnapEventType,
            BasePoint: getter.TryGetBasePoint(out Point3d basePoint) ? Some(basePoint) : Option<Point3d>.None,
            PlanarConstraint: default(RhinoViewport) switch {
                RhinoViewport viewport when getter.GetPlanarConstraint(vp: ref viewport, plane: out Plane plane) => Some((Viewport: viewport, Plane: plane)),
                _ => Option<(RhinoViewport Viewport, Plane Plane)>.None,
            },
            SnapPoints: toSeq(getter.GetSnapPoints()),
            ConstructionPoints: toSeq(getter.GetConstructionPoints()));
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

    internal static CommandSnapshot Native<T>(Option<T> value) =>
        Empty with {
            Number = value.Case is double number ? Some(number) : Option<double>.None,
            Text = value.Case is string text ? Some(text) : Option<string>.None,
            Point = value.Case is Point3d point ? Some(point) : Option<Point3d>.None,
            Vector = value.Case is Vector3d vector ? Some(vector) : Option<Vector3d>.None,
            Color = value.Case is Color color ? Some(color) : Option<Color>.None,
        };

    internal static CommandSnapshot Of(GetBaseClass getter, GetResult raw) =>
        new(
            View: Optional(getter.View()),
            WindowPoint: raw is GetResult.Point or GetResult.Point2d ? Some<DrawingPoint>(getter.Point2d()) : Option<DrawingPoint>.None,
            Number: raw is GetResult.Number ? Some(getter.Number()) : Option<double>.None,
            Text: raw is GetResult.String ? Optional(getter.StringResult()) : Option<string>.None,
            CustomMessage: raw is GetResult.CustomMessage ? Optional(getter.CustomMessage()) : Option<object>.None,
            Point: raw is GetResult.Point ? Some(getter.Point()) : Option<Point3d>.None,
            Vector: raw is GetResult.Point ? Some(getter.Vector()) : Option<Vector3d>.None,
            Color: raw is GetResult.Color ? Some(getter.Color()) : Option<Color>.None,
            PickRectangle: raw is GetResult.Object ? Some(getter.PickRectangle()) : Option<DrawingRectangle>.None,
            Rectangle2d: raw is GetResult.Rectangle2d ? Some(getter.Rectangle2d()) : Option<DrawingRectangle>.None,
            Line2d: raw is GetResult.Line2d ? toSeq(getter.Line2d()) : Seq<DrawingPoint>());

    public Option<T> As<T>() =>
        typeof(T) switch {
            Type t when t == typeof(DrawingPoint) => WindowPoint.Bind(static value => Cast<T>(value)),
            Type t when t == typeof(double) => Number.Bind(static value => Cast<T>(value)),
            Type t when t == typeof(string) => Text.Bind(static value => Cast<T>(value)),
            Type t when t == typeof(object) => CustomMessage.Bind(static value => Cast<T>(value)),
            Type t when t == typeof(Point3d) => Point.Bind(static value => Cast<T>(value)),
            Type t when t == typeof(Vector3d) => Vector.Bind(static value => Cast<T>(value)),
            Type t when t == typeof(Color) => Color.Bind(static value => Cast<T>(value)),
            Type t when t == typeof(DrawingRectangle) => PickRectangle.Bind(static value => Cast<T>(value)) | Rectangle2d.Bind(static value => Cast<T>(value)),
            Type t when t == typeof(Seq<DrawingPoint>) => Cast<T>(Line2d),
            _ => Option<T>.None,
        };

    private static Option<T> Cast<T>(object value) =>
        value is T typed ? Some(typed) : Option<T>.None;
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
        new(Raw: Option<GetResult>.None, CommandResult: result, Value: value, Option: Option<CommandOptionValue>.None, GotDefault: false, Snapshot: CommandSnapshot.Native(value: value));

    public Option<TOut> As<TOut>() =>
        Value.Bind(static value => value is TOut typed ? Some(typed) : Option<TOut>.None) | Snapshot.As<TOut>();
}

public sealed record CommandInputRequest<T> {
    private readonly Func<CommandInput, Fin<CommandGet<T>>> run;

    internal CommandInputRequest(Func<CommandInput, Fin<CommandGet<T>>> run) => this.run = run;

    internal Fin<CommandGet<T>> Run(CommandInput input) =>
        Optional(run).ToFin(Fail: Op.Of(name: nameof(CommandInputRequest<T>)).InvalidInput()).Bind(valid => valid(arg: input));
}

public sealed record CommandPolicy {
    private CommandPolicy(
        Seq<Action<GetBaseClass>> baseActions = default,
        Seq<Action<GetObject>> objectActions = default,
        Seq<CommandOption> options = default,
        Option<(int Min, int Max)> objects = default,
        Option<(bool OnMouseUp, bool TwoDimensional)> point = default,
        Option<Scalar> scalar = default,
        Option<Bounds> bounds = default,
        Option<BoxSpec> box = default,
        Option<string> prompt = default) {
        BaseActions = baseActions;
        ObjectActions = objectActions;
        Options = options;
        Objects = objects;
        Point = point;
        ScalarMode = scalar;
        BoundsMode = bounds;
        Box = box;
        PromptText = prompt;
    }

    private Seq<Action<GetBaseClass>> BaseActions { get; }
    private Seq<Action<GetObject>> ObjectActions { get; }
    internal Seq<CommandOption> Options { get; }
    internal Option<(int Min, int Max)> Objects { get; }
    internal Option<(bool OnMouseUp, bool TwoDimensional)> Point { get; }
    internal Option<Scalar> ScalarMode { get; }
    internal Option<Bounds> BoundsMode { get; }
    internal Option<BoxSpec> Box { get; }
    internal Option<string> PromptText { get; }
    internal bool AcceptsNothing => BaseActions.Exists(static action => ReferenceEquals(objA: action, objB: AcceptNothingAction));
    internal static CommandPolicy Empty { get; } = new();

    public static CommandPolicy Configure(Action<GetBaseClass> apply) => new(baseActions: Seq(apply));
    public static CommandPolicy ConfigureObject(Action<GetObject> apply) => new(objectActions: Seq(apply));
    public static CommandPolicy Prompt(string value) => new(baseActions: Seq<Action<GetBaseClass>>(getter => getter.SetCommandPrompt(value)), prompt: Optional(value));
    public static CommandPolicy PromptDefault(string value) => Configure(apply: getter => getter.SetCommandPromptDefault(value));
    public static CommandPolicy Default(object value) => Configure(apply: getter => ApplyDefault(getter: getter, value: value));
    public static CommandPolicy Wait(int milliseconds) => milliseconds >= 0 ? Configure(apply: getter => getter.SetWaitDuration(milliseconds)) : Empty;
    public static CommandPolicy AcceptNothing() => Configure(apply: AcceptNothingAction);
    public static CommandPolicy AcceptUndo() => Configure(apply: static getter => getter.AcceptUndo(enable: true));
    public static CommandPolicy AcceptNumber() => Configure(apply: static getter => getter.AcceptNumber(enable: true, acceptZero: true));
    public static CommandPolicy AcceptPoint() => Configure(apply: static getter => getter.AcceptPoint(enable: true));
    public static CommandPolicy AcceptColor() => Configure(apply: static getter => getter.AcceptColor(enable: true));
    public static CommandPolicy AcceptString() => Configure(apply: static getter => getter.AcceptString(enable: true));
    public static CommandPolicy AcceptCustomMessage() => Configure(apply: static getter => getter.AcceptCustomMessage(enable: true));
    public static CommandPolicy EnterWhenDone() => Configure(apply: static getter => getter.AcceptEnterWhenDone(enable: true));
    public static CommandPolicy OptionsOf(params CommandOption[] values) => new(options: toSeq(values));
    public static CommandPolicy ObjectsOf(int minimum = 1, int maximum = 1) => new(objects: Some((Min: minimum, Max: maximum)));
    public static CommandPolicy PointOf(bool onMouseUp = false, bool twoDimensional = false) => new(point: Some((OnMouseUp: onMouseUp, TwoDimensional: twoDimensional)));
    public static CommandPolicy BoundsOf<T>(Option<T> lower = default, Option<T> upper = default, bool strictlyLower = false, bool strictlyUpper = false) where T : IComparable<T> =>
        new(bounds: Some(new Bounds(Lower: lower.Map(static value => (object)value), Upper: upper.Map(static value => (object)value), StrictlyLower: strictlyLower, StrictlyUpper: strictlyUpper)));
    public static CommandPolicy Length(Option<UnitSystem> units = default) => new(scalar: Some(new Scalar(Kind: ScalarKind.Length, LengthUnits: units, AngleUnits: Option<AngleUnitSystem>.None)));
    public static CommandPolicy Angle(AngleUnitSystem units) => new(scalar: Some(new Scalar(Kind: ScalarKind.Angle, LengthUnits: Option<UnitSystem>.None, AngleUnits: Some(units))));
    public static CommandPolicy BoxOf(GetBoxMode mode = GetBoxMode.All, Point3d? basePoint = null, string? prompt1 = null, string? prompt2 = null, string? prompt3 = null) =>
        new(box: Some(new BoxSpec(Mode: mode, BasePoint: basePoint, Prompt1: prompt1, Prompt2: prompt2, Prompt3: prompt3)));

    public static CommandPolicy operator +(CommandPolicy left, CommandPolicy right) => Add(left: left, right: right);

    public static CommandPolicy Add(CommandPolicy left, CommandPolicy right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        ArgumentNullException.ThrowIfNull(argument: right);
        return new(
            baseActions: left.BaseActions + right.BaseActions,
            objectActions: left.ObjectActions + right.ObjectActions,
            options: left.Options + right.Options,
            objects: Pick(left.Objects, right.Objects),
            point: Pick(left.Point, right.Point),
            scalar: Pick(left.ScalarMode, right.ScalarMode),
            bounds: Pick(left.BoundsMode, right.BoundsMode),
            box: Pick(left.Box, right.Box),
            prompt: Pick(left.PromptText, right.PromptText));
    }

    internal static CommandPolicy Merge(Seq<CommandPolicy> policies) =>
        policies.Fold(Empty, static (state, policy) => state + policy);

    internal Fin<Unit> Apply<TGetter>(TGetter getter) where TGetter : GetBaseClass =>
        Optional(getter).ToFin(Fail: Op.Of(name: nameof(CommandPolicy)).InvalidInput()).Map(valid => {
            _ = BaseActions.Iter(action => action(obj: valid));
            _ = valid is GetObject objects ? DefaultObjectActions.Concat(ObjectActions).Iter(action => action(obj: objects)) : unit;
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

    private static Action<GetBaseClass> AcceptNothingAction { get; } = static getter => getter.AcceptNothing(enable: true);
    private static Option<T> Pick<T>(Option<T> left, Option<T> right) => right.IsSome ? right : left;
    private static Unit ApplyDefault(GetBaseClass getter, object value) {
        _ = value switch {
            Point3d point => Applied(apply: () => getter.SetDefaultPoint(point: point)),
            double number => Applied(apply: () => getter.SetDefaultNumber(number)),
            int integer => Applied(apply: () => getter.SetDefaultInteger(integer)),
            string text => Applied(apply: () => getter.SetDefaultString(text)),
            Color color => Applied(apply: () => getter.SetDefaultColor(color)),
            _ => unit,
        };
        return unit;
    }
    private static Unit Applied(Action apply) {
        apply();
        return unit;
    }

    internal enum ScalarKind { Number, Length, Angle }
    internal sealed record Scalar(ScalarKind Kind, Option<UnitSystem> LengthUnits, Option<AngleUnitSystem> AngleUnits);
    internal sealed record Bounds(Option<object> Lower, Option<object> Upper, bool StrictlyLower, bool StrictlyUpper);
    internal sealed record BoxSpec(GetBoxMode Mode, Point3d? BasePoint, string? Prompt1, string? Prompt2, string? Prompt3);
}

public static class CommandInputs {
    public static CommandInputRequest<T> Get<T>(params CommandPolicy[] policies) {
        Seq<CommandPolicy> active = toSeq(policies);
        CommandPolicy policy = CommandPolicy.Merge(policies: active);
        return typeof(T) switch {
            Type t when t == typeof(CommandSelection) => Objects<T>(policy: policy, policies: active),
            Type t when t == typeof(CommandOptionValue) => Getter<GetOption, T>(policies: active, create: static () => new GetOption(), receive: static getter => getter.Get(), value: static (_, _, _) => Option<T>.None),
            Type t when t == typeof(CommandPoint) || t == typeof(Point3d) || t == typeof(DrawingPoint) => Point<T>(policy: policy, policies: active),
            Type t when t == typeof(string) || t == typeof(double) => Text<T>(policy: policy, policies: active),
            Type t when t == typeof(int) => Number<GetInteger, int, T>(policies: active, policy: policy, create: static () => new GetInteger(), receive: static getter => getter.Get(), current: static getter => getter.Number(), setLower: static (getter, value, strict) => getter.SetLowerLimit(value, strict), setUpper: static (getter, value, strict) => getter.SetUpperLimit(value, strict)),
            Type t when t == typeof(Line) => Native<T>(run: static () => GetNative<T, Line>(static (out Line value) => RhinoGet.GetLine(line: out value))),
            Type t when t == typeof(Polyline) => Native<T>(run: static () => GetNative<T, Polyline>(static (out Polyline value) => RhinoGet.GetPolyline(polyline: out value))),
            Type t when t == typeof(Circle) => Native<T>(run: static () => GetNative<T, Circle>(static (out Circle value) => RhinoGet.GetCircle(circle: out value))),
            Type t when t == typeof(Arc) => Native<T>(run: static () => GetNative<T, Arc>(static (out Arc value) => RhinoGet.GetArc(arc: out value))),
            Type t when t == typeof(Plane) => Native<T>(run: static () => GetNative<T, Plane>(static (out Plane value) => RhinoGet.GetPlane(plane: out value))),
            Type t when t == typeof(Seq<Point3d>) => Native<T>(run: () => Rectangle<T>(prompt: policy.PromptText.IfNone(string.Empty))),
            Type t when t == typeof(Box) => Native<T>(run: () => Box<T>(spec: policy.Box.IfNone(new CommandPolicy.BoxSpec(Mode: GetBoxMode.All, BasePoint: null, Prompt1: null, Prompt2: null, Prompt3: null)))),
            Type t when t == typeof(Color) => Native<T>(run: () => Color<T>(prompt: policy.PromptText.IfNone(string.Empty), acceptNothing: policy.AcceptsNothing)),
            _ => Invalid<T>(name: nameof(Get)),
        };
    }

    private delegate Result NativeGetter<TNative>(out TNative value);

    private static CommandInputRequest<T> Objects<T>(CommandPolicy policy, Seq<CommandPolicy> policies) =>
        policy.Objects.IfNone((Min: 1, Max: 1)) switch {
            ( < 0, _) or (_, < -1) => Invalid<T>(name: nameof(Get)),
            (int lo, int hi) when hi > 0 && hi < lo => Invalid<T>(name: nameof(Get)),
            (int lo, int hi) => Getter<GetObject, T>(
                policies: Seq(lo == 0 ? CommandPolicy.AcceptNothing() : CommandPolicy.Empty, hi == 0 ? CommandPolicy.EnterWhenDone() : CommandPolicy.Empty) + policies,
                create: static () => new GetObject(),
                receive: getter => getter.GetMultiple(minimumNumber: lo, maximumNumber: hi),
                value: static (source, getter, raw) => SelectionOf(document: source.Document, getter: getter, raw: raw).Bind(Cast<T>),
                transition: static (getter, raw, selected) => raw is GetResult.Option ? ForcePostSelect(getter: getter, selected: selected) : (Continue: false, Selected: selected)),
        };

    private static CommandInputRequest<T> Point<T>(CommandPolicy policy, Seq<CommandPolicy> policies) {
        (bool onMouseUp, bool configuredTwoDimensional) = policy.Point.IfNone((OnMouseUp: false, TwoDimensional: false));
        bool twoDimensional = configuredTwoDimensional || typeof(T) == typeof(DrawingPoint);
        return Getter<GetPoint, T>(
            policies: policies,
            create: static () => new GetPoint(),
            receive: getter => getter.Get(onMouseUp: onMouseUp, get2DPoint: twoDimensional),
            value: static (_, getter, raw) => raw is GetResult.Point or GetResult.Point2d ? PointValue<T>(getter: getter, raw: raw) : Option<T>.None);
    }

    private static CommandInputRequest<T> Text<T>(CommandPolicy policy, Seq<CommandPolicy> policies) =>
        policy.BoundsMode.IsSome && typeof(T) == typeof(double)
            ? Number<GetNumber, double, T>(policies: policies, policy: policy, create: static () => new GetNumber(), receive: static getter => getter.Get(), current: static getter => getter.Number(), setLower: static (getter, value, strict) => getter.SetLowerLimit(value, strict), setUpper: static (getter, value, strict) => getter.SetUpperLimit(value, strict))
            : Getter<GetString, T>(
                policies: policies,
                create: static () => new GetString(),
                receive: static getter => getter.Get(),
                value: (source, getter, raw) => raw is GetResult.String ? Parse<T>(input: source, text: getter.StringResult(), scalar: policy.ScalarMode).Bind(Cast<T>) : Option<T>.None);

    private static CommandInputRequest<TOut> Number<TGetter, TValue, TOut>(
        Seq<CommandPolicy> policies,
        CommandPolicy policy,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<TGetter, TValue> current,
        Action<TGetter, TValue, bool> setLower,
        Action<TGetter, TValue, bool> setUpper) where TGetter : GetBaseClass, IDisposable where TValue : IComparable<TValue> =>
        Getter(
            policies: policies,
            create: create,
            receive: receive,
            configure: getter => Limits(getter: getter, bounds: policy.BoundsMode, setLower: setLower, setUpper: setUpper),
            value: (_, getter, raw) => raw is GetResult.Number ? Cast<TOut>(current(arg: getter)!) : Option<TOut>.None);

    private static CommandInputRequest<T> Getter<TGetter, T>(
        Seq<CommandPolicy> policies,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<CommandInput, TGetter, GetResult, Option<T>> value,
        Func<TGetter, Fin<Unit>>? configure = null,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)>? transition = null) where TGetter : GetBaseClass, IDisposable =>
        new(run: input => Optional(input).ToFin(Fail: Op.Of(name: nameof(Get)).InvalidInput()).Bind(valid => {
            using TGetter getter = create();
            return from configured in configure is Func<TGetter, Fin<Unit>> apply ? apply(arg: getter) : Fin.Succ(value: unit)
                   from applied in CommandPolicy.Merge(policies: policies).Apply(getter: getter)
                   from result in CommandOption.Bind(options: CommandPolicy.Merge(policies: policies).Options, getter: getter).Bind(scope => {
                       using CommandOption.Scope active = scope;
                       return ReadLoop(getter: getter, scope: active, receive: () => receive(arg: getter), value: (g, raw) => value(arg1: valid, arg2: g, arg3: raw), selected: Option<CommandOptionValue>.None, transition: transition ?? (static (_, _, selected) => (Continue: false, Selected: selected)));
                   })
                   select result;
        }));

    private static Fin<CommandGet<T>> ReadLoop<TGetter, T>(
        TGetter getter,
        CommandOption.Scope scope,
        Func<GetResult> receive,
        Func<TGetter, GetResult, Option<T>> value,
        Option<CommandOptionValue> selected,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)> transition) where TGetter : GetBaseClass =>
        receive() switch {
            GetResult.Cancel => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            GetResult.Option => scope.Snapshot(getter: getter).Bind(option => Next(getter: getter, raw: GetResult.Option, value: value, selected: Some(option), transition: transition, scope: scope, receive: receive)),
            GetResult raw => Next(getter: getter, raw: raw, value: value, selected: selected, transition: transition, scope: scope, receive: receive),
        };

    private static Fin<CommandGet<T>> Next<TGetter, T>(TGetter getter, GetResult raw, Func<TGetter, GetResult, Option<T>> value, Option<CommandOptionValue> selected, Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)> transition, CommandOption.Scope scope, Func<GetResult> receive) where TGetter : GetBaseClass =>
        transition(getter, raw, selected) switch {
            (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, transition: transition),
            (false, Option<CommandOptionValue> next) => Snapshot(getter: getter, raw: raw, value: value(getter, raw), option: next),
        };

    private static Fin<CommandGet<T>> Snapshot<TGetter, T>(TGetter getter, GetResult raw, Option<T> value, Option<CommandOptionValue> option) where TGetter : GetBaseClass {
        Option<T> projected = value | option.Bind(static selected => selected is T selectedValue ? Some(selectedValue) : Option<T>.None);
        CommandGet<T> snapshot = CommandGet<T>.Of(getter: getter, raw: raw, value: projected, option: option);
        return (raw, projected.IsSome || (raw == GetResult.Option && option.IsSome)) switch {
            (GetResult.Object or GetResult.Point or GetResult.Point2d or GetResult.Number or GetResult.String or GetResult.Color or GetResult.Rectangle2d or GetResult.Line2d, false) => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Snapshot)).InvalidResult()),
            _ => Fin.Succ(value: snapshot),
        };
    }

    private static CommandInputRequest<T> Native<T>(Func<(Result Result, Option<T> Value)> run) =>
        new(input => Optional(input).ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput()).Bind(_ => run() switch {
            (Result.Success, Option<T> value) => Fin.Succ(value: CommandGet<T>.Native(result: Result.Success, value: value)),
            (Result.Cancel or Result.CancelModelessDialog, _) => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            (Result.Failure or Result.UnknownCommand, _) => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Native)).InvalidResult()),
            (Result result, _) => Fin.Succ(value: CommandGet<T>.Native(result: result, value: Option<T>.None)),
        }));

    private static (Result Result, Option<T> Value) GetNative<T, TNative>(NativeGetter<TNative> get) {
        Result result = get(value: out TNative value);
        return (Result: result, Value: result == Result.Success ? Cast<T>(value!) : Option<T>.None);
    }

    private static (Result Result, Option<T> Value) Rectangle<T>(string prompt) {
        Result result = string.IsNullOrWhiteSpace(value: prompt) ? RhinoGet.GetRectangle(corners: out Point3d[] corners) : RhinoGet.GetRectangle(firstPrompt: prompt, corners: out corners);
        return (Result: result, Value: result == Result.Success ? Cast<T>(toSeq(corners)) : Option<T>.None);
    }

    private static (Result Result, Option<T> Value) Box<T>(CommandPolicy.BoxSpec spec) {
        Result result = spec is { Mode: GetBoxMode.All, BasePoint: null, Prompt1: null, Prompt2: null, Prompt3: null }
            ? RhinoGet.GetBox(box: out Box value)
            : RhinoGet.GetBox(box: out value, mode: spec.Mode, basePoint: spec.BasePoint ?? Point3d.Unset, prompt1: spec.Prompt1, prompt2: spec.Prompt2, prompt3: spec.Prompt3);
        return (Result: result, Value: result == Result.Success ? Cast<T>(value) : Option<T>.None);
    }

    private static (Result Result, Option<T> Value) Color<T>(string prompt, bool acceptNothing) {
        Color color = global::System.Drawing.Color.Empty;
        Result result = RhinoGet.GetColor(prompt: prompt, acceptNothing: acceptNothing, color: ref color);
        return (Result: result, Value: result == Result.Success ? Cast<T>(color) : Option<T>.None);
    }

    private static CommandInputRequest<T> Invalid<T>(string name) =>
        new(run: _ => Fin.Fail<CommandGet<T>>(error: Op.Of(name: name).InvalidInput()));

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

    private static Option<object> Parse<T>(CommandInput input, string text, Option<CommandPolicy.Scalar> scalar) =>
        typeof(T) switch {
            Type t when t == typeof(string) => Optional(text).Map(static value => (object)value),
            Type t when t == typeof(double) => scalar.IfNone(new CommandPolicy.Scalar(Kind: default, LengthUnits: Option<UnitSystem>.None, AngleUnits: Option<AngleUnitSystem>.None)) switch {
                { LengthUnits.IsSome: true, AngleUnits.IsSome: true } => Option<object>.None,
                { Kind: CommandPolicy.ScalarKind.Length } => ParseLength(text: text, units: scalar.Bind(static value => value.LengthUnits).IfNone(input.Document.ModelUnitSystem)).Map(static value => (object)value),
                { Kind: CommandPolicy.ScalarKind.Angle } => scalar.Bind(static value => value.AngleUnits).Bind(units => ParseAngle(text: text, units: units)).Map(static value => (object)value),
                _ => ParseNumber(text: text).Map(static value => (object)value),
            },
            _ => Option<object>.None,
        };

    private static Option<TOut> Cast<TOut>(object value) => value is TOut typed ? Some(typed) : Option<TOut>.None;

    private static Option<double> ParseNumber(string text) {
        StringParserSettings results = new();
        try {
            return StringParser.ParseNumber(expression: text, max_count: text.Length, settings_in: StringParserSettings.DefaultParseSettings, settings_out: ref results, answer: out double value) == text.Length ? Some(value) : Option<double>.None;
        } finally {
            results.Dispose();
        }
    }

    private static Option<double> ParseLength(string text, UnitSystem units) {
        LengthValue value = LengthValue.Create(s: text, ps: StringParserSettings.DefaultParseSettings, parsedAll: out bool parsedAll);
        try {
            return parsedAll && !value.IsUnset() ? Some(value.Length(units: units)) : Option<double>.None;
        } finally {
            value.Dispose();
        }
    }

    private static Option<double> ParseAngle(string text, AngleUnitSystem units) {
        StringParserSettings results = new();
        AngleUnitSystem parsed = AngleUnitSystem.None;
        try {
            return StringParser.ParseAngleExpession(text, 0, text.Length, StringParserSettings.DefaultParseSettings, units, out double value, ref results, ref parsed) == text.Length ? Some(value) : Option<double>.None;
        } finally {
            results.Dispose();
        }
    }

    private static Fin<Unit> Limits<TGetter, TValue>(TGetter getter, Option<CommandPolicy.Bounds> bounds, Action<TGetter, TValue, bool> setLower, Action<TGetter, TValue, bool> setUpper) where TGetter : GetBaseClass where TValue : IComparable<TValue> =>
        bounds.Case switch {
            CommandPolicy.Bounds b => (b.Lower.Bind(Cast<TValue>), b.Upper.Bind(Cast<TValue>)) switch {
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
}

public sealed record CommandInput {
    internal CommandInput(RhinoDoc document) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Document = document;
    }

    public RhinoDoc Document { get; }

    public Fin<CommandGet<TValue>> Get<TValue>(CommandInputRequest<TValue> request) =>
        Optional(request).ToFin(Fail: Op.Of(name: nameof(Get)).InvalidInput()).Bind(valid => valid.Run(input: this));
}
