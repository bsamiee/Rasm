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

public abstract record CommandIntent {
    private CommandIntent() { }

    public static CommandIntent Objects(int minimum = 1, int maximum = 1) => new ObjectsCase(Minimum: minimum, Maximum: maximum);
    public static CommandIntent Point(bool onMouseUp = false, bool twoDimensional = false) => new PointCase(OnMouseUp: onMouseUp, TwoDimensional: twoDimensional);
    public static CommandIntent Text(bool literal = false) => new TextCase(Literal: literal, Project: static (_, text) => Optional(text).Map(static value => (object)value));
    public static CommandIntent TextNumber(bool literal = false) => new TextCase(Literal: literal, Project: static (_, text) => CommandQuery.ParseNumber(text).Map(static value => (object)value));
    public static CommandIntent Length(bool literal = false) => new TextCase(Literal: literal, Project: static (input, text) => CommandQuery.ParseLength(text: text, units: input.Document.ModelUnitSystem).Map(static value => (object)value));
    public static CommandIntent AngleDegrees(bool literal = false) => new TextCase(Literal: literal, Project: static (_, text) => CommandQuery.ParseAngle(text: text, units: AngleUnitSystem.Degrees).Map(static value => (object)value));
    public static CommandIntent AngleRadians(bool literal = false) => new TextCase(Literal: literal, Project: static (_, text) => CommandQuery.ParseAngle(text: text, units: AngleUnitSystem.Radians).Map(static value => (object)value));
    public static CommandIntent Number(Option<double> lower = default, Option<double> upper = default, bool strictlyLower = false, bool strictlyUpper = false) =>
        new NumberCase(Lower: lower, Upper: upper, StrictlyLower: strictlyLower, StrictlyUpper: strictlyUpper);
    public static CommandIntent Integer(Option<int> lower = default, Option<int> upper = default, bool strictlyLower = false, bool strictlyUpper = false) =>
        new IntegerCase(Lower: lower, Upper: upper, StrictlyLower: strictlyLower, StrictlyUpper: strictlyUpper);
    public static CommandIntent Options() => new OptionsCase();
    public static CommandIntent Line() => Native(static (out Line value) => RhinoGet.GetLine(line: out value));
    public static CommandIntent Polyline() => Native(static (out Polyline value) => RhinoGet.GetPolyline(polyline: out value));
    public static CommandIntent Circle() => Native(static (out Circle value) => RhinoGet.GetCircle(circle: out value));
    public static CommandIntent Arc() => Native(static (out Arc value) => RhinoGet.GetArc(arc: out value));
    public static CommandIntent Plane() => Native(static (out Plane value) => RhinoGet.GetPlane(plane: out value));
    public static CommandIntent Rectangle(string prompt = "") => Native(run: () => {
        Point3d[] corners;
        Result result = string.IsNullOrWhiteSpace(value: prompt) switch {
            true => RhinoGet.GetRectangle(corners: out corners),
            false => RhinoGet.GetRectangle(firstPrompt: prompt, corners: out corners),
        };
        return (Result: result, Value: toSeq(corners));
    }, exactOutput: false);
    public static CommandIntent Box() => Native(static (out Box value) => RhinoGet.GetBox(box: out value));
    public static CommandIntent Color(string prompt = "", bool acceptNothing = false) => Native(run: () => {
        Color color = global::System.Drawing.Color.Empty;
        Result result = RhinoGet.GetColor(prompt: prompt, acceptNothing: acceptNothing, color: ref color);
        return (Result: result, Value: color);
    }, exactOutput: false);

    private delegate Result GetNative<TNative>(out TNative value);

    private static NativeCase Native<TNative>(GetNative<TNative> get, bool exactOutput = true) =>
        Native(run: () => {
            Result result = get(value: out TNative value);
            return (Result: result, Value: value);
        }, exactOutput: exactOutput);

    private static NativeCase Native<TNative>(Func<(Result Result, TNative Value)> run, bool exactOutput = true) =>
        new(
            Output: typeof(TNative),
            ExactOutput: exactOutput,
            Run: () => {
                (Result Result, TNative Value) pair = run();
                return (pair.Result, pair.Value);
            });

    internal sealed record ObjectsCase(int Minimum, int Maximum) : CommandIntent;
    internal sealed record PointCase(bool OnMouseUp, bool TwoDimensional) : CommandIntent;
    internal sealed record TextCase(bool Literal, Func<CommandInput, string, Option<object>> Project) : CommandIntent;
    internal sealed record NumberCase(Option<double> Lower, Option<double> Upper, bool StrictlyLower, bool StrictlyUpper) : CommandIntent;
    internal sealed record IntegerCase(Option<int> Lower, Option<int> Upper, bool StrictlyLower, bool StrictlyUpper) : CommandIntent;
    internal sealed record OptionsCase : CommandIntent;
    internal sealed record NativeCase(Type Output, bool ExactOutput, Func<(Result Result, object? Value)> Run) : CommandIntent;
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

public abstract partial record CommandQuery<TValue> {
    private protected CommandQuery() { }

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
            .ToFin(Fail: Op.Of(name: nameof(CommandQuery<TValue>)).InvalidInput())
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

public static class CommandQuery {
    public static CommandQuery<T> Expect<T>(CommandIntent intent, params CommandPolicy[] policies) =>
        new ExpectCase<T>(Intent: intent, Policies: toSeq(policies));

    private sealed record ExpectCase<T>(CommandIntent Intent, Seq<CommandPolicy> Policies) : CommandQuery<T> {
        internal override Fin<CommandGet<T>> Run(CommandInput input) =>
            Optional(Intent)
                .ToFin(Fail: Op.Of(name: nameof(Expect)).InvalidInput())
                .Bind(intent => intent switch {
                    CommandIntent.ObjectsCase objects => Objects(input: input, intent: objects),
                    CommandIntent.PointCase point => Point(input: input, intent: point),
                    CommandIntent.TextCase text => Text(input: input, intent: text),
                    CommandIntent.NumberCase number => Number(input: input, intent: number),
                    CommandIntent.IntegerCase integer => Integer(input: input, intent: integer),
                    CommandIntent.OptionsCase => Options(input: input),
                    CommandIntent.NativeCase when !Policies.IsEmpty => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Expect)).InvalidInput()),
                    CommandIntent.NativeCase native when native.ExactOutput && native.Output == typeof(T) => Native(run: native.Run),
                    CommandIntent.NativeCase native when !native.ExactOutput => Native(run: native.Run),
                    _ => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Expect)).InvalidInput()),
                });

        private Fin<CommandGet<T>> Objects(CommandInput input, CommandIntent.ObjectsCase intent) =>
            (intent.Minimum, intent.Maximum) switch {
                ( < 0, _) or (_, < -1) => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(CommandIntent.Objects)).InvalidInput()),
                (int minimum, int maximum) when maximum > 0 && maximum < minimum => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(CommandIntent.Objects)).InvalidInput()),
                _ => Read(
                    input: input,
                    policies: Seq(new CommandPolicy(AcceptNothing: intent.Minimum == 0, AcceptEnterWhenDone: intent.Maximum == 0)) + Policies,
                    create: static () => new GetObject(),
                    receive: getter => getter.GetMultiple(minimumNumber: intent.Minimum, maximumNumber: intent.Maximum),
                    configure: ConfigureObjects,
                    value: static (source, getter, raw) => SelectionOf(document: source.Document, getter: getter, raw: raw).Bind(Cast<T>),
                    transition: static (getter, raw, selected) => raw switch {
                        GetResult.Option => DisablePreSelect(getter: getter, selected: selected),
                        _ => (Continue: false, Selected: selected),
                    }),
            };

        private Fin<CommandGet<T>> Point(CommandInput input, CommandIntent.PointCase intent) =>
            Read(
                input: input,
                policies: Policies,
                create: static () => new GetPoint(),
                receive: getter => getter.Get(onMouseUp: intent.OnMouseUp, get2DPoint: intent.TwoDimensional),
                value: static (_, getter, raw) => raw switch {
                    GetResult.Point or GetResult.Point2d => PointValue<T>(getter: getter, raw: raw),
                    _ => Option<T>.None,
                });

        private Fin<CommandGet<T>> Text(CommandInput input, CommandIntent.TextCase intent) =>
            Read(
                input: input,
                policies: Policies,
                create: static () => new GetString(),
                receive: getter => intent.Literal switch { true => getter.GetLiteralString(), false => getter.Get() },
                value: (source, getter, raw) => raw switch {
                    GetResult.String => intent.Project(arg1: source, arg2: getter.StringResult()).Bind(Cast<T>),
                    _ => Option<T>.None,
                });

        private Fin<CommandGet<T>> Number(CommandInput input, CommandIntent.NumberCase intent) =>
            Read(
                input: input,
                policies: Policies,
                create: static () => new GetNumber(),
                configure: getter => Limits(
                    getter: getter,
                    lower: intent.Lower,
                    upper: intent.Upper,
                    strictlyLower: intent.StrictlyLower,
                    strictlyUpper: intent.StrictlyUpper,
                    setLower: static (g, value, strict) => g.SetLowerLimit(value, strict),
                    setUpper: static (g, value, strict) => g.SetUpperLimit(value, strict)),
                receive: static getter => getter.Get(),
                value: static (_, getter, raw) => raw switch { GetResult.Number => Cast<T>(getter.Number()), _ => Option<T>.None });

        private Fin<CommandGet<T>> Integer(CommandInput input, CommandIntent.IntegerCase intent) =>
            Read(
                input: input,
                policies: Policies,
                create: static () => new GetInteger(),
                configure: getter => Limits(
                    getter: getter,
                    lower: intent.Lower,
                    upper: intent.Upper,
                    strictlyLower: intent.StrictlyLower,
                    strictlyUpper: intent.StrictlyUpper,
                    setLower: static (g, value, strict) => g.SetLowerLimit(value, strict),
                    setUpper: static (g, value, strict) => g.SetUpperLimit(value, strict)),
                receive: static getter => getter.Get(),
                value: static (_, getter, raw) => raw switch { GetResult.Number => Cast<T>(getter.Number()), _ => Option<T>.None });

        private Fin<CommandGet<T>> Options(CommandInput input) =>
            Read(
                input: input,
                policies: Policies,
                create: static () => new GetOption(),
                receive: static getter => getter.Get(),
                value: static (_, _, _) => Option<T>.None);

        private static Fin<Unit> ConfigureObjects(GetObject getter) =>
            Optional(getter)
                .ToFin(Fail: Op.Of(name: nameof(ConfigureObjects)).InvalidInput())
                .Map(valid => {
                    valid.GeometryFilter = ObjectType.AnyObject;
                    valid.GeometryAttributeFilter = default;
                    valid.EnablePreSelect(enable: true, ignoreUnacceptablePreselectedObjects: true);
                    valid.EnablePostSelect(enable: true);
                    valid.EnableIgnoreGrips(enable: true);
                    valid.EnableSelPrevious(enable: true);
                    valid.EnableHighlight(enable: true);
                    return unit;
                });

        private static Option<CommandSelection> SelectionOf(RhinoDoc document, GetObject getter, GetResult raw) =>
            raw switch {
                GetResult.Object => getter.Objects() switch {
                    ObjRef[] references => Optional(CommandSelection.From(
                        document: document,
                        references: toSeq(references),
                        preselected: getter.ObjectsWerePreselected switch {
                            true => toSeq(references).Map(static reference => (reference.ObjectId, reference.GeometryComponentIndex)),
                            false => Seq<(Guid ObjectId, ComponentIndex ComponentIndex)>(),
                        })),
                    _ => Option<CommandSelection>.None,
                },
                _ => Option<CommandSelection>.None,
            };

        private static (bool Continue, Option<CommandOptionValue> Selected) DisablePreSelect(GetObject getter, Option<CommandOptionValue> selected) {
            getter.DisablePreSelect();
            return (Continue: true, Selected: selected);
        }

        private static Fin<CommandGet<T>> Native(Func<(Result Result, object? Value)> run) {
            (Result Result, object? Value) pair = run();
            return pair.Result switch {
                Result.Success => Cast<T>(pair.Value)
                    .ToFin(Fail: Op.Of(name: nameof(Native)).InvalidInput())
                    .Map(value => CommandGet<T>.Native(result: pair.Result, value: Some(value))),
                Result.Cancel or Result.CancelModelessDialog => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
                Result.Failure or Result.UnknownCommand => Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(Native)).InvalidResult()),
                _ => Fin.Succ(value: CommandGet<T>.Native(result: pair.Result, value: Option<T>.None)),
            };
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
    }

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

    public Fin<CommandGet<TValue>> Get<TValue>(CommandQuery<TValue> query) =>
        Optional(query)
            .ToFin(Fail: Op.Of(name: nameof(Get)).InvalidInput())
            .Bind(valid => valid.Run(input: this));
}
