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
            Point: raw switch { GetResult.Point => Some(getter.Point()), _ => Option<Point3d>.None },
            WindowPoint: raw switch { GetResult.Point or GetResult.Point2d => Some<DrawingPoint>(getter.Point2d()), _ => Option<DrawingPoint>.None },
            View: Optional(getter.View()),
            Reference: CommandSelection.Reference.Of(getter: getter),
            NumberPreview: getter.NumberPreview(number: out double number) switch { true => Some(number), false => Option<double>.None },
            Osnap: getter.OsnapEventType,
            BasePoint: getter.TryGetBasePoint(out Point3d basePoint) switch { true => Some(basePoint), false => Option<Point3d>.None },
            PlanarConstraint: PlanarConstraintOf(getter: getter),
            SnapPoints: toSeq(getter.GetSnapPoints()),
            ConstructionPoints: toSeq(getter.GetConstructionPoints()));

    private static Option<(RhinoViewport Viewport, Plane Plane)> PlanarConstraintOf(GetPoint getter) =>
        default(RhinoViewport) switch {
            RhinoViewport viewport when getter.GetPlanarConstraint(vp: ref viewport, plane: out Plane plane) => Some((Viewport: viewport, Plane: plane)),
            _ => Option<(RhinoViewport Viewport, Plane Plane)>.None,
        };
}

public readonly record struct CommandGet<T>(
    Option<GetResult> Raw,
    Result CommandResult,
    CommandOutcome Outcome,
    Option<T> Value,
    Option<CommandOptionValue> Option,
    bool GotDefault,
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

    internal static CommandGet<T> Of(GetBaseClass getter, GetResult raw, Option<T> value, Option<CommandOptionValue> option) =>
        new(
            Raw: Some(raw),
            CommandResult: getter.CommandResult(),
            Outcome: OutcomeOf(raw: raw, result: getter.CommandResult()),
            Value: value,
            Option: option,
            GotDefault: getter.GotDefault(),
            View: Optional(getter.View()),
            WindowPoint: raw switch { GetResult.Point or GetResult.Point2d => Some<DrawingPoint>(getter.Point2d()), _ => Option<DrawingPoint>.None },
            Number: raw switch { GetResult.Number => Some(getter.Number()), _ => Option<double>.None },
            Text: raw switch { GetResult.String => Optional(getter.StringResult()), _ => Option<string>.None },
            CustomMessage: raw switch { GetResult.CustomMessage => Optional(getter.CustomMessage()), _ => Option<object>.None },
            Point: raw switch { GetResult.Point => Some(getter.Point()), _ => Option<Point3d>.None },
            Vector: raw switch { GetResult.Point => Some(getter.Vector()), _ => Option<Vector3d>.None },
            Color: raw switch { GetResult.Color => Some(getter.Color()), _ => Option<Color>.None },
            PickRectangle: raw switch { GetResult.Object => Some(getter.PickRectangle()), _ => Option<DrawingRectangle>.None },
            Rectangle2d: raw switch { GetResult.Rectangle2d => Some(getter.Rectangle2d()), _ => Option<DrawingRectangle>.None },
            Line2d: raw switch { GetResult.Line2d => toSeq(getter.Line2d()), _ => Seq<DrawingPoint>() });

    internal static CommandGet<T> Native(Result result, Option<T> value) =>
        new(
            Raw: Option<GetResult>.None,
            CommandResult: result,
            Outcome: OutcomeOf(result: result),
            Value: value,
            Option: Option<CommandOptionValue>.None,
            GotDefault: false,
            View: Option<RhinoView>.None,
            WindowPoint: Option<DrawingPoint>.None,
            Number: value.Case is double number ? Some(number) : Option<double>.None,
            Text: value.Case is string text ? Some(text) : Option<string>.None,
            CustomMessage: Option<object>.None,
            Point: value.Case is Point3d point ? Some(point) : Option<Point3d>.None,
            Vector: value.Case is Vector3d vector ? Some(vector) : Option<Vector3d>.None,
            Color: value.Case is Color color ? Some(color) : Option<Color>.None,
            PickRectangle: Option<DrawingRectangle>.None,
            Rectangle2d: Option<DrawingRectangle>.None,
            Line2d: Seq<DrawingPoint>());

    private static CommandOutcome OutcomeOf(GetResult raw, Result result) =>
        raw switch {
            GetResult.Cancel => CommandOutcome.Cancelled,
            GetResult.Nothing => CommandOutcome.Nothing,
            GetResult.Undo => CommandOutcome.Undo,
            GetResult.Miss => CommandOutcome.Miss,
            GetResult.Timeout => CommandOutcome.Timeout,
            GetResult.ExitRhino => CommandOutcome.ExitRhino,
            GetResult.CustomMessage => CommandOutcome.Custom,
            GetResult.Option => CommandOutcome.Option,
            GetResult.User1 or GetResult.User2 or GetResult.User3 or GetResult.User4 or GetResult.User5 => CommandOutcome.User,
            _ => OutcomeOf(result: result),
        };

    private static CommandOutcome OutcomeOf(Result result) =>
        result switch {
            Result.Cancel or Result.CancelModelessDialog => CommandOutcome.Cancelled,
            Result.Nothing => CommandOutcome.Nothing,
            Result.ExitRhino => CommandOutcome.ExitRhino,
            Result.Failure or Result.UnknownCommand => CommandOutcome.Failure,
            _ => CommandOutcome.Value,
        };
}

public enum CommandOutcome { Value, Cancelled, Nothing, Undo, Miss, Timeout, ExitRhino, Custom, Option, User, Failure }

public sealed record CommandObjectSelectionPolicy(
    ObjectType GeometryFilter = ObjectType.AnyObject,
    GeometryAttributeFilter GeometryAttributeFilter = default,
    Option<GetObjectGeometryFilter> Geometry = default,
    bool PreSelect = true,
    bool IgnoreUnacceptablePreselectedObjects = true,
    bool PostSelect = true,
    bool DeselectAllBeforePostSelect = false,
    bool OneByOnePostSelect = false,
    bool SubObjectSelect = true,
    bool ChooseOneQuestion = false,
    bool BottomObjectPreference = false,
    bool GroupSelect = false,
    bool ReferenceObjectSelect = true,
    bool LockedObjectSelect = false,
    bool AlreadySelectedObjectSelect = false,
    bool ProxyBrepFromSubD = false,
    bool InactiveDetailPickEnabled = false,
    bool SelPrevious = true,
    bool Highlight = true,
    bool IgnoreGrips = true,
    bool ClearObjectsOnEntry = true,
    bool UnselectObjectsOnExit = true,
    bool PressEnterWhenDonePrompt = true,
    string? PressEnterWhenDoneText = null) {
    internal static CommandObjectSelectionPolicy Default { get; } = new();

    internal Fin<Unit> Apply(GetObject getter) =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(CommandObjectSelectionPolicy)).InvalidInput())
            .Map(valid => {
                valid.GeometryFilter = GeometryFilter;
                valid.GeometryAttributeFilter = GeometryAttributeFilter;
                _ = Geometry.Iter(valid.SetCustomGeometryFilter);
                valid.EnablePreSelect(enable: PreSelect, ignoreUnacceptablePreselectedObjects: IgnoreUnacceptablePreselectedObjects);
                valid.EnablePostSelect(enable: PostSelect);
                valid.DeselectAllBeforePostSelect = DeselectAllBeforePostSelect;
                valid.OneByOnePostSelect = OneByOnePostSelect;
                valid.SubObjectSelect = SubObjectSelect;
                valid.ChooseOneQuestion = ChooseOneQuestion;
                valid.BottomObjectPreference = BottomObjectPreference;
                valid.GroupSelect = GroupSelect;
                valid.ReferenceObjectSelect = ReferenceObjectSelect;
                valid.LockedObjectSelect = LockedObjectSelect;
                valid.AlreadySelectedObjectSelect = AlreadySelectedObjectSelect;
                valid.ProxyBrepFromSubD = ProxyBrepFromSubD;
                valid.InactiveDetailPickEnabled = InactiveDetailPickEnabled;
                valid.EnableSelPrevious(enable: SelPrevious);
                valid.EnableHighlight(enable: Highlight);
                valid.EnableIgnoreGrips(enable: IgnoreGrips);
                valid.EnableClearObjectsOnEntry(enable: ClearObjectsOnEntry);
                valid.EnableUnselectObjectsOnExit(enable: UnselectObjectsOnExit);
                valid.EnablePressEnterWhenDonePrompt(enable: PressEnterWhenDonePrompt);
                _ = Optional(PressEnterWhenDoneText).Iter(valid.SetPressEnterWhenDonePrompt);
                return unit;
            });
}

public readonly record struct CommandPolicy(
    string? Prompt = null,
    string? PromptDefault = null,
    object? DefaultValue = null,
    int? WaitMilliseconds = null,
    bool AcceptNothing = false,
    bool AcceptUndo = false,
    bool AcceptNumber = false,
    bool AcceptPoint = false,
    bool AcceptColor = false,
    bool AcceptString = false,
    bool AcceptCustomMessage = false,
    bool AcceptEnterWhenDone = false,
    IEnumerable<CommandOption>? Options = null) {
    internal static Fin<Unit> Apply<TGetter>(Seq<CommandPolicy> policies, TGetter getter) where TGetter : GetBaseClass =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(CommandPolicy)).InvalidInput())
            .Bind(valid => policies.TraverseM(policy => policy.ApplyTo(getter: valid)).Map(static _ => unit));

    internal static Seq<CommandOption> OptionSet(Seq<CommandPolicy> policies) =>
        policies.Bind(static policy => Optional(policy.Options).Map(static values => toSeq(values)).IfNone(Seq<CommandOption>()));

    private Fin<Unit> ApplyTo<TGetter>(TGetter getter) where TGetter : GetBaseClass {
        _ = Optional(Prompt).Iter(getter.SetCommandPrompt);
        _ = Optional(PromptDefault).Iter(getter.SetCommandPromptDefault);
        _ = Optional(WaitMilliseconds).Iter(getter.SetWaitDuration);
        Seq<(bool Enabled, Action<TGetter> Apply)> actions = Seq<(bool Enabled, Action<TGetter> Apply)>(
            (AcceptNothing, static g => g.AcceptNothing(enable: true)),
            (AcceptUndo, static g => g.AcceptUndo(enable: true)),
            (AcceptNumber, static g => g.AcceptNumber(enable: true, acceptZero: true)),
            (AcceptPoint, static g => g.AcceptPoint(enable: true)),
            (AcceptColor, static g => g.AcceptColor(enable: true)),
            (AcceptString, static g => g.AcceptString(enable: true)),
            (AcceptCustomMessage, static g => g.AcceptCustomMessage(enable: true)),
            (AcceptEnterWhenDone, static g => g.AcceptEnterWhenDone(enable: true)));
        _ = actions.Filter(static action => action.Enabled).Iter(action => action.Apply(obj: getter));
        return Optional(DefaultValue).Case switch {
            Point3d point => Applied(apply: () => getter.SetDefaultPoint(point: point)),
            double number => Applied(apply: () => getter.SetDefaultNumber(number)),
            int integer => Applied(apply: () => getter.SetDefaultInteger(integer)),
            string text => Applied(apply: () => getter.SetDefaultString(text)),
            Color color => Applied(apply: () => getter.SetDefaultColor(color)),
            null => Fin.Succ(value: unit),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(CommandPolicy)).InvalidInput()),
        };

        static Fin<Unit> Applied(Action apply) {
            apply();
            return Fin.Succ(value: unit);
        }
    }
}

public abstract partial record CommandInputRequest<TValue> {
    private protected CommandInputRequest() { }

    internal abstract Fin<CommandGet<TValue>> Run(CommandInput input);

    private protected static Fin<CommandGet<T>> Read<TGetter, T>(
        CommandInput input,
        Seq<CommandPolicy> policies,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<CommandInput, TGetter, GetResult, Option<T>> value,
        Func<TGetter, Fin<Unit>>? configure = null,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)>? transition = null) where TGetter : GetBaseClass, IDisposable =>
        Optional(input)
            .ToFin(Fail: Op.Of(name: nameof(CommandInputRequest<TValue>)).InvalidInput())
            .Bind(valid => {
                using TGetter getter = create();
                return from configured in configure switch { Func<TGetter, Fin<Unit>> apply => apply(arg: getter), _ => Fin.Succ(value: unit) }
                       from applied in CommandPolicy.Apply(policies: policies, getter: getter)
                       from result in CommandOption.Bind(options: CommandPolicy.OptionSet(policies: policies), getter: getter).Bind(scope => {
                           using CommandOption.Scope active = scope;
                           return ReadLoop(
                               getter: getter,
                               scope: active,
                               receive: () => receive(arg: getter),
                               value: (g, raw) => value(arg1: valid, arg2: g, arg3: raw),
                               selected: Option<CommandOptionValue>.None,
                               transition: transition ?? (static (_, _, selected) => (Continue: false, Selected: selected)));
                       })
                       select result;
            });

    private static Fin<CommandGet<T>> ReadLoop<TGetter, T>(
        TGetter getter,
        CommandOption.Scope scope,
        Func<GetResult> receive,
        Func<TGetter, GetResult, Option<T>> value,
        Option<CommandOptionValue> selected,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)> transition) where TGetter : GetBaseClass =>
        receive() switch {
            GetResult.Cancel => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            GetResult.Option => scope.Snapshot(getter: getter).Bind(option => transition(getter, GetResult.Option, Some(option)) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, transition: transition),
                (false, Option<CommandOptionValue> next) => Snapshot(getter: getter, raw: GetResult.Option, value: value(getter, GetResult.Option), option: next),
            }),
            GetResult raw => transition(getter, raw, selected) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, transition: transition),
                (false, Option<CommandOptionValue> next) => Snapshot(getter: getter, raw: raw, value: value(getter, raw), option: next),
            },
        };

    private static Fin<CommandGet<T>> Snapshot<TGetter, T>(TGetter getter, GetResult raw, Option<T> value, Option<CommandOptionValue> option) where TGetter : GetBaseClass {
        Option<T> projected = value.Case switch {
            T typed => Some(typed),
            _ => option.Bind(static selected => selected is T selectedValue ? Some(selectedValue) : Option<T>.None),
        };
        CommandGet<T> snapshot = CommandGet<T>.Of(getter: getter, raw: raw, value: projected, option: option);
        return (raw, projected.IsSome || (raw == GetResult.Option && option.IsSome)) switch {
            (GetResult.Object or GetResult.Point or GetResult.Point2d or GetResult.Number or GetResult.String or GetResult.Color or GetResult.Rectangle2d or GetResult.Line2d, false) =>
                Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Snapshot)).InvalidResult()),
            _ => Fin.Succ(value: snapshot),
        };
    }
}

public static class CommandInputs {
    public static CommandInputRequest<CommandSelection> Objects(
        int minimum = 1,
        int maximum = 1,
        CommandObjectSelectionPolicy? selection = null,
        params CommandPolicy[] policies) =>
        (minimum, maximum, Optional(selection).IfNone(CommandObjectSelectionPolicy.Default), toSeq(policies)) switch {
            ( < 0, _, _, _) or (_, < -1, _, _) => Invalid<CommandSelection>(name: nameof(Objects)),
            (int lo, int hi, _, _) when hi > 0 && hi < lo => Invalid<CommandSelection>(name: nameof(Objects)),
            (int lo, int hi, CommandObjectSelectionPolicy policy, Seq<CommandPolicy> active) => Getter(
                policies: Seq(new CommandPolicy(AcceptNothing: lo == 0, AcceptEnterWhenDone: hi == 0)) + active,
                create: static () => new GetObject(),
                receive: getter => getter.GetMultiple(minimumNumber: lo, maximumNumber: hi),
                value: static (source, getter, raw) => SelectionOf(document: source.Document, getter: getter, raw: raw),
                configure: policy.Apply,
                transition: static (getter, raw, selected) => raw switch {
                    GetResult.Option => ForcePostSelect(getter: getter, selected: selected),
                    _ => (Continue: false, Selected: selected),
                }),
        };

    public static CommandInputRequest<CommandPoint> Point(bool onMouseUp = false, bool twoDimensional = false, params CommandPolicy[] policies) =>
        PointOf<CommandPoint>(onMouseUp: onMouseUp, twoDimensional: twoDimensional, policies: toSeq(policies));

    public static CommandInputRequest<Point3d> Point3d(bool onMouseUp = false, params CommandPolicy[] policies) =>
        PointOf<Point3d>(onMouseUp: onMouseUp, twoDimensional: false, policies: toSeq(policies));

    public static CommandInputRequest<DrawingPoint> WindowPoint(bool onMouseUp = false, params CommandPolicy[] policies) =>
        PointOf<DrawingPoint>(onMouseUp: onMouseUp, twoDimensional: true, policies: toSeq(policies));

    public static CommandInputRequest<string> Text(bool literal = false, params CommandPolicy[] policies) =>
        TextOf<string>(literal: literal, policies: toSeq(policies), project: static (_, text) => Optional(text).Map(static value => (object)value));

    public static CommandInputRequest<double> TextNumber(bool literal = false, params CommandPolicy[] policies) =>
        TextOf<double>(literal: literal, policies: toSeq(policies), project: static (_, text) => ParseNumber(text: text).Map(static value => (object)value));

    public static CommandInputRequest<double> Length(bool literal = false, params CommandPolicy[] policies) =>
        TextOf<double>(literal: literal, policies: toSeq(policies), project: static (input, text) => ParseLength(text: text, units: input.Document.ModelUnitSystem).Map(static value => (object)value));

    public static CommandInputRequest<double> AngleDegrees(bool literal = false, params CommandPolicy[] policies) =>
        TextOf<double>(literal: literal, policies: toSeq(policies), project: static (_, text) => ParseAngle(text: text, units: AngleUnitSystem.Degrees).Map(static value => (object)value));

    public static CommandInputRequest<double> AngleRadians(bool literal = false, params CommandPolicy[] policies) =>
        TextOf<double>(literal: literal, policies: toSeq(policies), project: static (_, text) => ParseAngle(text: text, units: AngleUnitSystem.Radians).Map(static value => (object)value));

    public static CommandInputRequest<double> Number(Option<double> lower = default, Option<double> upper = default, bool strictlyLower = false, bool strictlyUpper = false, params CommandPolicy[] policies) =>
        Numeric<GetNumber, double>(
            policies: toSeq(policies),
            lower: lower,
            upper: upper,
            strictlyLower: strictlyLower,
            strictlyUpper: strictlyUpper,
            create: static () => new GetNumber(),
            receive: static getter => getter.Get(),
            current: static getter => getter.Number(),
            setLower: static (getter, value, strict) => getter.SetLowerLimit(value, strict),
            setUpper: static (getter, value, strict) => getter.SetUpperLimit(value, strict));

    public static CommandInputRequest<int> Integer(Option<int> lower = default, Option<int> upper = default, bool strictlyLower = false, bool strictlyUpper = false, params CommandPolicy[] policies) =>
        Numeric<GetInteger, int>(
            policies: toSeq(policies),
            lower: lower,
            upper: upper,
            strictlyLower: strictlyLower,
            strictlyUpper: strictlyUpper,
            create: static () => new GetInteger(),
            receive: static getter => getter.Get(),
            current: static getter => getter.Number(),
            setLower: static (getter, value, strict) => getter.SetLowerLimit(value, strict),
            setUpper: static (getter, value, strict) => getter.SetUpperLimit(value, strict));

    public static CommandInputRequest<CommandOptionValue> Options(params CommandPolicy[] policies) =>
        Getter(
            policies: toSeq(policies),
            create: static () => new GetOption(),
            receive: static getter => getter.Get(),
            value: static (_, _, _) => Option<CommandOptionValue>.None);

    public static CommandInputRequest<Line> Line() =>
        Native(static (out Line value) => RhinoGet.GetLine(line: out value));

    public static CommandInputRequest<Polyline> Polyline() =>
        Native(static (out Polyline value) => RhinoGet.GetPolyline(polyline: out value));

    public static CommandInputRequest<Circle> Circle() =>
        Native(static (out Circle value) => RhinoGet.GetCircle(circle: out value));

    public static CommandInputRequest<Arc> Arc() =>
        Native(static (out Arc value) => RhinoGet.GetArc(arc: out value));

    public static CommandInputRequest<Plane> Plane() =>
        Native(static (out Plane value) => RhinoGet.GetPlane(plane: out value));

    public static CommandInputRequest<Seq<Point3d>> Rectangle(string prompt = "") =>
        Native(run: () => {
            Point3d[] corners;
            Result result = string.IsNullOrWhiteSpace(value: prompt) switch {
                true => RhinoGet.GetRectangle(corners: out corners),
                false => RhinoGet.GetRectangle(firstPrompt: prompt, corners: out corners),
            };
            return (Result: result, Value: toSeq(corners));
        });

    public static CommandInputRequest<Box> Box(
        GetBoxMode mode = GetBoxMode.All,
        Point3d? basePoint = null,
        string? prompt1 = null,
        string? prompt2 = null,
        string? prompt3 = null) =>
        Native(run: () => {
            Box value;
            Result result = (mode, basePoint, prompt1, prompt2, prompt3) switch {
                (GetBoxMode.All, null, null, null, null) => RhinoGet.GetBox(box: out value),
                _ => RhinoGet.GetBox(box: out value, mode: mode, basePoint: basePoint ?? global::Rhino.Geometry.Point3d.Unset, prompt1: prompt1, prompt2: prompt2, prompt3: prompt3),
            };
            return (Result: result, Value: value);
        });

    public static CommandInputRequest<Color> Color(string prompt = "", bool acceptNothing = false) =>
        Native(run: () => {
            Color color = global::System.Drawing.Color.Empty;
            Result result = RhinoGet.GetColor(prompt: prompt, acceptNothing: acceptNothing, color: ref color);
            return (Result: result, Value: color);
        });

    private delegate Result GetNative<TNative>(out TNative value);

    private static CommandInputRequest<TNative> Native<TNative>(GetNative<TNative> get) =>
        Native(run: () => {
            Result result = get(value: out TNative value);
            return (Result: result, Value: value);
        });

    private static CommandInputRequest<TNative> Native<TNative>(Func<(Result Result, TNative Value)> run) =>
        new NativeCase<TNative>(NativeRun: run);

    private sealed record GetterCase<TGetter, T>(
        Seq<CommandPolicy> Policies,
        Func<TGetter> Create,
        Func<TGetter, GetResult> Receive,
        Func<CommandInput, TGetter, GetResult, Option<T>> Value,
        Func<TGetter, Fin<Unit>>? Configure = null,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)>? Transition = null) : CommandInputRequest<T> where TGetter : GetBaseClass, IDisposable {
        internal override Fin<CommandGet<T>> Run(CommandInput input) =>
            Read(
                input: input,
                policies: Policies,
                create: Create,
                receive: Receive,
                value: Value,
                configure: Configure,
                transition: Transition);
    }

    private static CommandInputRequest<T> Getter<TGetter, T>(
        Seq<CommandPolicy> policies,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<CommandInput, TGetter, GetResult, Option<T>> value,
        Func<TGetter, Fin<Unit>>? configure = null,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)>? transition = null) where TGetter : GetBaseClass, IDisposable =>
        new GetterCase<TGetter, T>(Policies: policies, Create: create, Receive: receive, Value: value, Configure: configure, Transition: transition);

    private static CommandInputRequest<T> Invalid<T>(string name) =>
        new InvalidCase<T>(Reason: Op.Of(name: name).InvalidInput());

    private static CommandInputRequest<T> PointOf<T>(bool onMouseUp, bool twoDimensional, Seq<CommandPolicy> policies) =>
        Getter(
            policies: policies,
            create: static () => new GetPoint(),
            receive: getter => getter.Get(onMouseUp: onMouseUp, get2DPoint: twoDimensional),
            value: static (_, getter, raw) => raw switch { GetResult.Point or GetResult.Point2d => PointValue<T>(getter: getter, raw: raw), _ => Option<T>.None });

    private static CommandInputRequest<T> TextOf<T>(bool literal, Seq<CommandPolicy> policies, Func<CommandInput, string, Option<object>> project) =>
        Getter(
            policies: policies,
            create: static () => new GetString(),
            receive: getter => literal switch { true => getter.GetLiteralString(), false => getter.Get() },
            value: (source, getter, raw) => raw switch { GetResult.String => project(arg1: source, arg2: getter.StringResult()).Bind(Cast<T>), _ => Option<T>.None });

    private static CommandInputRequest<TValue> Numeric<TGetter, TValue>(
        Seq<CommandPolicy> policies,
        Option<TValue> lower,
        Option<TValue> upper,
        bool strictlyLower,
        bool strictlyUpper,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<TGetter, TValue> current,
        Action<TGetter, TValue, bool> setLower,
        Action<TGetter, TValue, bool> setUpper) where TGetter : GetBaseClass, IDisposable where TValue : IComparable<TValue> =>
        Getter(
            policies: policies,
            create: create,
            receive: receive,
            configure: getter => Limits(getter: getter, lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper, setLower: setLower, setUpper: setUpper),
            value: (_, getter, raw) => raw switch { GetResult.Number => Some(current(arg: getter)), _ => Option<TValue>.None });

    private sealed record NativeCase<T>(Func<(Result Result, T Value)> NativeRun) : CommandInputRequest<T> {
        internal override Fin<CommandGet<T>> Run(CommandInput input) =>
            Optional(input)
                .ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput())
                .Bind(_ => RunNative());

        private Fin<CommandGet<T>> RunNative() {
            (Result Result, T Value) pair = NativeRun();
            return pair.Result switch {
                Result.Success => CommandGet<T>.Native(result: pair.Result, value: Some(pair.Value)),
                Result.Cancel or Result.CancelModelessDialog => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
                Result.Failure or Result.UnknownCommand => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Native)).InvalidResult()),
                _ => Fin.Succ(value: CommandGet<T>.Native(result: pair.Result, value: Option<T>.None)),
            };
        }
    }

    private sealed record InvalidCase<T>(Error Reason) : CommandInputRequest<T> {
        internal override Fin<CommandGet<T>> Run(CommandInput input) =>
            Fin.Fail<CommandGet<T>>(error: Reason);
    }

    private static Option<CommandSelection> SelectionOf(RhinoDoc document, GetObject getter, GetResult raw) =>
        raw switch {
            GetResult.Object => getter.Objects() switch {
                ObjRef[] references => Optional(CommandSelection.From(document: document, references: toSeq(references), preselected: PreselectedReferences(getter: getter, references: references))),
                _ => Option<CommandSelection>.None,
            },
            _ => Option<CommandSelection>.None,
        };

    private static Seq<(Guid ObjectId, ComponentIndex ComponentIndex)> PreselectedReferences(GetObject getter, ObjRef[] references) =>
        getter.ObjectsWerePreselected switch {
            true => toSeq(references)
                .Filter(static reference => Optional(reference.Object()).Map(static item => item.IsSelected(checkSubObjects: true) > 0).IfNone(false))
                .Map(static reference => (reference.ObjectId, reference.GeometryComponentIndex)),
            false => Seq<(Guid ObjectId, ComponentIndex ComponentIndex)>(),
        };

    private static (bool Continue, Option<CommandOptionValue> Selected) ForcePostSelect(GetObject getter, Option<CommandOptionValue> selected) {
        getter.EnablePreSelect(enable: false, ignoreUnacceptablePreselectedObjects: true);
        return (Continue: true, Selected: selected);
    }

    private static Option<TOut> PointValue<TOut>(GetPoint getter, GetResult raw) =>
        (raw, typeof(TOut)) switch {
            (GetResult.Point, Type t) when t == typeof(Point3d) => Cast<TOut>(getter.Point()),
            (GetResult.Point or GetResult.Point2d, Type t) when t == typeof(DrawingPoint) => Cast<TOut>(getter.Point2d()),
            (GetResult.Point or GetResult.Point2d, _) => Cast<TOut>(CommandPoint.Of(getter: getter, raw: raw)),
            _ => Option<TOut>.None,
        };

    private static Option<TOut> Cast<TOut>(object? value) =>
        value switch {
            TOut typed => Some(typed),
            _ => Option<TOut>.None,
        };

    internal static Option<double> ParseNumber(string text) {
        StringParserSettings results = new();
        StringParserSettings settings = StringParserSettings.DefaultParseSettings;
        try {
            return StringParser.ParseNumber(expression: text, max_count: text.Length, settings_in: settings, settings_out: ref results, answer: out double value) switch {
                int count when count == text.Length => Some(value),
                _ => Option<double>.None,
            };
        } finally {
            results.Dispose();
        }
    }

    internal static Option<double> ParseLength(string text, UnitSystem units) {
        LengthValue value = LengthValue.Create(s: text, ps: StringParserSettings.DefaultParseSettings, parsedAll: out bool parsedAll);
        try {
            return (value, parsedAll) switch {
                (LengthValue length, true) when !length.IsUnset() => Some(length.Length(units: units)),
                _ => Option<double>.None,
            };
        } finally {
            value.Dispose();
        }
    }

    internal static Option<double> ParseAngle(string text, AngleUnitSystem units) {
        StringParserSettings results = new();
        AngleUnitSystem parsed = AngleUnitSystem.None;
        try {
            return StringParser.ParseAngleExpession(text, 0, text.Length, StringParserSettings.DefaultParseSettings, units, out double value, ref results, ref parsed) switch {
                int count when count == text.Length => Some(value),
                _ => Option<double>.None,
            };
        } finally {
            results.Dispose();
        }
    }

    private static Fin<Unit> Limits<TGetter, TValue>(
        TGetter getter,
        Option<TValue> lower,
        Option<TValue> upper,
        bool strictlyLower,
        bool strictlyUpper,
        Action<TGetter, TValue, bool> setLower,
        Action<TGetter, TValue, bool> setUpper) where TGetter : GetBaseClass where TValue : IComparable<TValue> =>
        (lower.Case, upper.Case) switch {
            (TValue left, TValue right) when left.CompareTo(other: right) > 0 => Fin.Fail<Unit>(error: Op.Of(name: nameof(Limits)).InvalidInput()),
            (TValue left, TValue right) when left.CompareTo(other: right) == 0 && (strictlyLower || strictlyUpper) => Fin.Fail<Unit>(error: Op.Of(name: nameof(Limits)).InvalidInput()),
            _ => Optional(getter)
                .ToFin(Fail: Op.Of(name: nameof(Limits)).InvalidInput())
                .Map(valid => {
                    _ = lower.Iter(value => setLower(arg1: valid, arg2: value, arg3: strictlyLower));
                    _ = upper.Iter(value => setUpper(arg1: valid, arg2: value, arg3: strictlyUpper));
                    return unit;
                }),
        };
}

// --- [SERVICES] -------------------------------------------------------------------------
public sealed record CommandInput {
    internal CommandInput(RhinoDoc document) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Document = document;
    }

    public RhinoDoc Document { get; }

    public Fin<CommandGet<TValue>> Get<TValue>(CommandInputRequest<TValue> request) =>
        Optional(request)
            .ToFin(Fail: Op.Of(name: nameof(Get)).InvalidInput())
            .Bind(valid => valid.Run(input: this));
}
