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
            Reference: PointObject(getter: getter),
            NumberPreview: NumberPreviewOf(getter: getter),
            Osnap: getter.OsnapEventType,
            BasePoint: BasePointOf(getter: getter),
            PlanarConstraint: PlanarConstraintOf(getter: getter),
            SnapPoints: toSeq(getter.GetSnapPoints()),
            ConstructionPoints: toSeq(getter.GetConstructionPoints()));

    private static Option<CommandSelection.Reference> PointObject(GetPoint getter) =>
        Optional(getter.PointOnObject()).Map(reference => {
            using ObjRef owned = reference;
            return CommandSelection.Reference.Of(reference: owned, preselected: false);
        });

    private static Option<double> NumberPreviewOf(GetPoint getter) =>
        getter.NumberPreview(number: out double number) switch { true => Some(number), false => Option<double>.None };

    private static Option<Point3d> BasePointOf(GetPoint getter) =>
        getter.TryGetBasePoint(out Point3d point) switch { true => Some(point), false => Option<Point3d>.None };

    private static Option<(RhinoViewport Viewport, Plane Plane)> PlanarConstraintOf(GetPoint getter) =>
        default(RhinoViewport) switch {
            RhinoViewport viewport when getter.GetPlanarConstraint(vp: ref viewport, plane: out Plane plane) => Some((Viewport: viewport, Plane: plane)),
            _ => Option<(RhinoViewport Viewport, Plane Plane)>.None,
        };
}

public readonly record struct CommandGet<T>(
    GetResult Raw,
    Result CommandResult,
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
            Raw: raw,
            CommandResult: getter.CommandResult(),
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
}

public abstract partial record CommandQuery<TValue> {
    private protected CommandQuery() { }

    internal abstract Fin<CommandGet<TValue>> Run(CommandInput input);

    private protected static Fin<CommandGet<T>> Read<TGetter, T>(
        CommandInput input,
        Seq<CommandQuery.GetterPolicy<TGetter>> policies,
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
                       from applied in CommandQuery.GetterPolicy<TGetter>.Apply(policies: policies, getter: getter)
                       from result in ReadWith(
                           getter: getter,
                           options: CommandQuery.GetterPolicy<TGetter>.OptionSet(policies: policies),
                           receive: () => receive(arg: getter),
                           value: (g, raw) => value(arg1: valid, arg2: g, arg3: raw),
                           transition: transition ?? (static (_, _, selected) => (Continue: false, Selected: selected)))
                       select result;
            });

    private static Fin<CommandGet<T>> ReadWith<TGetter, T>(
        TGetter getter,
        Seq<CommandOption> options,
        Func<GetResult> receive,
        Func<TGetter, GetResult, Option<T>> value,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)> transition) where TGetter : GetBaseClass =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(ReadWith)).InvalidInput())
            .Bind(g => CommandOption.Bind(options: options, getter: g).Bind(scope => {
                using CommandOption.Scope active = scope;
                return ReadLoop(
                    getter: g,
                    scope: active,
                    receive: receive,
                    value: value,
                    selected: Option<CommandOptionValue>.None,
                    transition: transition);
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
            GetResult.Option => scope.Snapshot(getter: getter).Bind(option => transition(getter, GetResult.Option, Some(option)) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, transition: transition),
                (false, Option<CommandOptionValue> next) => Fin.Succ(value: CommandGet<T>.Of(getter: getter, raw: GetResult.Option, value: Project(value: value(getter, GetResult.Option), option: next), option: next)),
            }),
            GetResult raw => transition(getter, raw, selected) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, transition: transition),
                (false, Option<CommandOptionValue> next) => Fin.Succ(value: CommandGet<T>.Of(getter: getter, raw: raw, value: Project(value: value(getter, raw), option: next), option: next)),
            },
        };

    private static Option<T> Project<T>(Option<T> value, Option<CommandOptionValue> option) =>
        value.Case switch {
            T typed => Some(typed),
            _ => option.Bind(static selected => selected is T projected ? Some(projected) : Option<T>.None),
        };
}

public static class CommandQuery {
    public readonly record struct GetterPolicy<TGetter>(
        Func<TGetter, Fin<Unit>> Effect,
        Seq<CommandOption> CommandOptions) where TGetter : GetBaseClass {
        internal static Fin<Unit> Apply(Seq<GetterPolicy<TGetter>> policies, TGetter getter) =>
            Optional(getter)
                .ToFin(Fail: Op.Of(name: nameof(GetterPolicy<TGetter>)).InvalidInput())
                .Bind(g => policies.TraverseM(policy => policy.Run(getter: g)).Map(static _ => unit));

        internal static Seq<CommandOption> OptionSet(Seq<GetterPolicy<TGetter>> policies) =>
            policies.Bind(static policy => policy.CommandOptions);

        private Fin<Unit> Run(TGetter getter) =>
            Optional(Effect)
                .ToFin(Fail: Op.Of(name: nameof(GetterPolicy<TGetter>)).InvalidInput())
                .Bind(apply => apply(arg: getter));
    }

    public static GetterPolicy<TGetter> Native<TGetter>(Func<TGetter, Fin<Unit>> apply) where TGetter : GetBaseClass =>
        new(Effect: apply, CommandOptions: Seq<CommandOption>());

    public static GetterPolicy<TGetter> Native<TGetter>(Action<TGetter> apply) where TGetter : GetBaseClass =>
        Native<TGetter>(getter => Optional(apply)
            .ToFin(Fail: Op.Of(name: nameof(CommandQuery)).InvalidInput())
            .Map(valid => {
                valid(obj: getter);
                return unit;
            }));

    public static GetterPolicy<TGetter> Options<TGetter>(params CommandOption[] options) where TGetter : GetBaseClass =>
        new(Effect: static _ => Fin.Succ(value: unit), CommandOptions: toSeq(options));

    public static CommandQuery<CommandSelection> Objects(int minimum = 1, int maximum = 1, params GetterPolicy<GetObject>[] policies) =>
        new ObjectsCase(Minimum: minimum, Maximum: maximum, Policies: toSeq(policies));

    public static CommandQuery<CommandPoint> Point(bool onMouseUp = false, bool twoDimensional = false, params GetterPolicy<GetPoint>[] policies) =>
        new PointCase(OnMouseUp: onMouseUp, TwoDimensional: twoDimensional, Policies: toSeq(policies));

    public static CommandQuery<string> Text(bool literal = false, params GetterPolicy<GetString>[] policies) =>
        new TextCase(Literal: literal, Policies: toSeq(policies));

    public static CommandQuery<double> Number(Option<double> lower = default, Option<double> upper = default, bool strictlyLower = false, bool strictlyUpper = false, params GetterPolicy<GetNumber>[] policies) =>
        new NumberCase(Lower: lower, Upper: upper, StrictlyLower: strictlyLower, StrictlyUpper: strictlyUpper, Policies: toSeq(policies));

    public static CommandQuery<int> Integer(Option<int> lower = default, Option<int> upper = default, bool strictlyLower = false, bool strictlyUpper = false, params GetterPolicy<GetInteger>[] policies) =>
        new IntegerCase(Lower: lower, Upper: upper, StrictlyLower: strictlyLower, StrictlyUpper: strictlyUpper, Policies: toSeq(policies));

    public static CommandQuery<CommandOptionValue> Options(params CommandOption[] options) =>
        new OptionsCase(Options: toSeq(options));

    private static Seq<GetterPolicy<GetObject>> ObjectDefaults =>
        Seq(
            Native<GetObject>(static getter => {
                getter.GeometryFilter = ObjectType.AnyObject;
                getter.GeometryAttributeFilter = default;
            }),
            Native<GetObject>(static getter => getter.EnablePreSelect(enable: true, ignoreUnacceptablePreselectedObjects: true)),
            Native<GetObject>(static getter => getter.EnablePostSelect(enable: true)),
            Native<GetObject>(static getter => getter.EnableIgnoreGrips(enable: true)),
            Native<GetObject>(static getter => getter.EnableSelPrevious(enable: true)),
            Native<GetObject>(static getter => getter.EnableHighlight(enable: true)));

    private sealed record ObjectsCase(int Minimum, int Maximum, Seq<GetterPolicy<GetObject>> Policies) : CommandQuery<CommandSelection> {
        internal override Fin<CommandGet<CommandSelection>> Run(CommandInput input) =>
            Read(
                input: input,
                policies: ObjectDefaults + (Maximum switch {
                    0 => Seq(Native<GetObject>(static getter => getter.AcceptEnterWhenDone(enable: true))),
                    _ => Seq<GetterPolicy<GetObject>>(),
                }) + Policies,
                create: static () => new GetObject(),
                receive: getter => getter.GetMultiple(minimumNumber: Minimum, maximumNumber: Maximum),
                value: static (source, getter, raw) => SelectionOf(document: source.Document, getter: getter, raw: raw),
                transition: ObjectTransition);
    }

    private sealed record PointCase(bool OnMouseUp, bool TwoDimensional, Seq<GetterPolicy<GetPoint>> Policies) : CommandQuery<CommandPoint> {
        internal override Fin<CommandGet<CommandPoint>> Run(CommandInput input) =>
            Read(
                input: input,
                policies: Policies,
                create: static () => new GetPoint(),
                receive: getter => getter.Get(onMouseUp: OnMouseUp, get2DPoint: TwoDimensional),
                value: static (_, getter, raw) => raw switch {
                    GetResult.Point or GetResult.Point2d => Some(CommandPoint.Of(getter: getter, raw: raw)),
                    _ => Option<CommandPoint>.None,
                });
    }

    private sealed record TextCase(bool Literal, Seq<GetterPolicy<GetString>> Policies) : CommandQuery<string> {
        internal override Fin<CommandGet<string>> Run(CommandInput input) =>
            Read(
                input: input,
                policies: Policies,
                create: static () => new GetString(),
                receive: getter => Literal switch { true => getter.GetLiteralString(), false => getter.Get() },
                value: static (_, getter, raw) => raw switch { GetResult.String => Optional(getter.StringResult()), _ => Option<string>.None });
    }

    private sealed record NumberCase(Option<double> Lower, Option<double> Upper, bool StrictlyLower, bool StrictlyUpper, Seq<GetterPolicy<GetNumber>> Policies) : CommandQuery<double> {
        internal override Fin<CommandGet<double>> Run(CommandInput input) =>
            Read(
                input: input,
                policies: Policies,
                create: static () => new GetNumber(),
                configure: getter => Limits(
                    getter: getter,
                    lower: Lower,
                    upper: Upper,
                    strictlyLower: StrictlyLower,
                    strictlyUpper: StrictlyUpper,
                    setLower: static (g, value, strict) => g.SetLowerLimit(value, strict),
                    setUpper: static (g, value, strict) => g.SetUpperLimit(value, strict)),
                receive: static getter => getter.Get(),
                value: static (_, getter, raw) => raw switch { GetResult.Number => Some(getter.Number()), _ => Option<double>.None });
    }

    private sealed record IntegerCase(Option<int> Lower, Option<int> Upper, bool StrictlyLower, bool StrictlyUpper, Seq<GetterPolicy<GetInteger>> Policies) : CommandQuery<int> {
        internal override Fin<CommandGet<int>> Run(CommandInput input) =>
            Read(
                input: input,
                policies: Policies,
                create: static () => new GetInteger(),
                configure: getter => Limits(
                    getter: getter,
                    lower: Lower,
                    upper: Upper,
                    strictlyLower: StrictlyLower,
                    strictlyUpper: StrictlyUpper,
                    setLower: static (g, value, strict) => g.SetLowerLimit(value, strict),
                    setUpper: static (g, value, strict) => g.SetUpperLimit(value, strict)),
                receive: static getter => getter.Get(),
                value: static (_, getter, raw) => raw switch { GetResult.Number => Some(getter.Number()), _ => Option<int>.None });
    }

    private sealed record OptionsCase(Seq<CommandOption> Options) : CommandQuery<CommandOptionValue> {
        internal override Fin<CommandGet<CommandOptionValue>> Run(CommandInput input) =>
            Read(
                input: input,
                policies: Seq(CommandQuery.Options<GetOption>([.. Options])),
                create: static () => new GetOption(),
                receive: static getter => getter.Get(),
                value: static (_, _, _) => Option<CommandOptionValue>.None);
    }

    private static Option<CommandSelection> SelectionOf(RhinoDoc document, GetObject getter, GetResult raw) =>
        raw switch {
            GetResult.Object => Optional(CommandSelection.From(
                document: document,
                references: toSeq(Enumerable.Range(start: 0, count: getter.ObjectCount).Select(index => getter.Object(index))),
                preselected: getter.ObjectsWerePreselected switch {
                    true => toSeq(Enumerable.Range(start: 0, count: getter.ObjectCount).Select(index => ObjectIdAt(getter: getter, index: index))),
                    false => Seq<Guid>(),
                })),
            _ => Option<CommandSelection>.None,
        };

    private static Guid ObjectIdAt(GetObject getter, int index) {
        using ObjRef reference = getter.Object(index);
        return reference.ObjectId;
    }

    private static (bool Continue, Option<CommandOptionValue> Selected) ObjectTransition(GetObject getter, GetResult raw, Option<CommandOptionValue> selected) =>
        raw switch {
            GetResult.Option => DisablePreSelect(getter: getter, selected: selected),
            _ => (Continue: false, Selected: selected),
        };

    private static (bool Continue, Option<CommandOptionValue> Selected) DisablePreSelect(GetObject getter, Option<CommandOptionValue> selected) {
        getter.EnablePreSelect(false, true);
        return (Continue: true, Selected: selected);
    }

    private static Fin<Unit> Limits<TGetter, TValue>(
        TGetter getter,
        Option<TValue> lower,
        Option<TValue> upper,
        bool strictlyLower,
        bool strictlyUpper,
        Action<TGetter, TValue, bool> setLower,
        Action<TGetter, TValue, bool> setUpper) where TGetter : GetBaseClass =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(Limits)).InvalidInput())
            .Map(valid => {
                _ = lower.Iter(value => setLower(arg1: valid, arg2: value, arg3: strictlyLower));
                _ = upper.Iter(value => setUpper(arg1: valid, arg2: value, arg3: strictlyUpper));
                return unit;
            });
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
