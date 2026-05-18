using Color = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using Result = Rhino.Commands.Result;

namespace Rasm.Rhino;

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

public sealed class CommandRequest<TValue> {
    private readonly Func<CommandInput, Fin<CommandGet<TValue>>> read;
    private CommandRequest(Func<CommandInput, Fin<CommandGet<TValue>>> read) => this.read = read;
    internal static CommandRequest<TValue> Of(Func<CommandInput, Fin<CommandGet<TValue>>> read) => new(read: read);
    internal Fin<CommandGet<TValue>> Read(CommandInput input) => read(arg: input);
}

public readonly record struct CommandPolicy<TGetter>(
    Func<TGetter, Fin<Unit>> Effect,
    Seq<CommandOption> CommandOptions) where TGetter : GetBaseClass {
    internal static Fin<Unit> Apply(Seq<CommandPolicy<TGetter>> policies, TGetter getter) =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(CommandPolicy<TGetter>)).InvalidInput())
            .Bind(g => policies.TraverseM(policy => policy.Run(getter: g)).Map(static _ => unit));

    internal static Seq<CommandOption> OptionSet(Seq<CommandPolicy<TGetter>> policies) =>
        policies.Bind(static policy => policy.CommandOptions);

    private Fin<Unit> Run(TGetter getter) =>
        Optional(Effect)
            .ToFin(Fail: Op.Of(name: nameof(CommandPolicy<TGetter>)).InvalidInput())
            .Bind(apply => apply(arg: getter));
}

public static class CommandPolicy {
    public static CommandPolicy<TGetter> Native<TGetter>(Func<TGetter, Fin<Unit>> apply) where TGetter : GetBaseClass =>
        new(Effect: apply, CommandOptions: Seq<CommandOption>());
    public static CommandPolicy<TGetter> Native<TGetter>(Action<TGetter> apply) where TGetter : GetBaseClass =>
        Native<TGetter>(getter => Optional(apply)
            .ToFin(Fail: Op.Of(name: nameof(CommandPolicy)).InvalidInput())
            .Map(valid => {
                valid(obj: getter);
                return unit;
            }));
    public static CommandPolicy<TGetter> Options<TGetter>(params CommandOption[] options) where TGetter : GetBaseClass =>
        new(Effect: static _ => Fin.Succ(value: unit), CommandOptions: toSeq(options));
}

// --- [SERVICES] -------------------------------------------------------------------------
public sealed record CommandInput {
    internal CommandInput(RhinoDoc document) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Document = document;
    }

    public RhinoDoc Document { get; }

    public CommandRequest<CommandSelection> Objects(int minimum = 1, int maximum = 1, params CommandPolicy<GetObject>[] policies) =>
        Request(
            document: Document,
            policies: ObjectDefaults + (maximum switch {
                0 => Seq(CommandPolicy.Native<GetObject>(static getter => getter.AcceptEnterWhenDone(enable: true))),
                _ => Seq<CommandPolicy<GetObject>>(),
            }) + toSeq(policies),
            create: static () => new GetObject(),
            receive: getter => getter.GetMultiple(minimumNumber: minimum, maximumNumber: maximum),
            value: static (input, getter, raw) => SelectionOf(document: input.Document, getter: getter, raw: raw),
            transition: ObjectTransition);

    public CommandRequest<CommandPoint> Point(bool onMouseUp = false, bool twoDimensional = false, params CommandPolicy<GetPoint>[] policies) =>
        Request(
            document: Document,
            policies: toSeq(policies),
            create: static () => new GetPoint(),
            receive: getter => getter.Get(onMouseUp: onMouseUp, get2DPoint: twoDimensional),
            value: static (_, getter, raw) => raw switch {
                GetResult.Point or GetResult.Point2d => Some(CommandPoint.Of(getter: getter, raw: raw)),
                _ => Option<CommandPoint>.None,
            });

    public CommandRequest<string> Text(bool literal = false, params CommandPolicy<GetString>[] policies) =>
        Request(
            document: Document,
            policies: toSeq(policies),
            create: static () => new GetString(),
            receive: getter => literal ? getter.GetLiteralString() : getter.Get(),
            value: static (_, getter, raw) => raw switch { GetResult.String => Optional(getter.StringResult()), _ => Option<string>.None });

    public CommandRequest<double> Number(Option<double> lower = default, Option<double> upper = default, bool strictlyLower = false, bool strictlyUpper = false, params CommandPolicy<GetNumber>[] policies) =>
        Request(
            document: Document,
            policies: toSeq(policies),
            create: static () => new GetNumber(),
            configure: getter => Limits(getter: getter, lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper),
            receive: static getter => getter.Get(),
            value: static (_, getter, raw) => raw switch { GetResult.Number => Some(getter.Number()), _ => Option<double>.None });

    public CommandRequest<int> Integer(Option<int> lower = default, Option<int> upper = default, bool strictlyLower = false, bool strictlyUpper = false, params CommandPolicy<GetInteger>[] policies) =>
        Request(
            document: Document,
            policies: toSeq(policies),
            create: static () => new GetInteger(),
            configure: getter => Limits(getter: getter, lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper),
            receive: static getter => getter.Get(),
            value: static (_, getter, raw) => raw switch { GetResult.Number => Some(getter.Number()), _ => Option<int>.None });

    public CommandRequest<CommandOptionValue> Options(params CommandOption[] options) =>
        Request(
            document: Document,
            policies: Seq(CommandPolicy.Options<GetOption>(options)),
            create: static () => new GetOption(),
            receive: static getter => getter.Get(),
            value: static (_, _, _) => Option<CommandOptionValue>.None);

    public Fin<CommandGet<TValue>> Read<TValue>(CommandRequest<TValue> request) =>
        Optional(request)
            .ToFin(Fail: Op.Of(name: nameof(Read)).InvalidInput())
            .Bind(input => input.Read(input: this));

    private static Seq<CommandPolicy<GetObject>> ObjectDefaults =>
        Seq(
            CommandPolicy.Native<GetObject>(static getter => {
                getter.GeometryFilter = ObjectType.AnyObject;
                getter.GeometryAttributeFilter = default;
            }),
            CommandPolicy.Native<GetObject>(static getter => getter.EnablePreSelect(enable: true, ignoreUnacceptablePreselectedObjects: true)),
            CommandPolicy.Native<GetObject>(static getter => getter.EnablePostSelect(enable: true)),
            CommandPolicy.Native<GetObject>(static getter => getter.EnableIgnoreGrips(enable: true)),
            CommandPolicy.Native<GetObject>(static getter => getter.EnableSelPrevious(enable: true)),
            CommandPolicy.Native<GetObject>(static getter => getter.EnableHighlight(enable: true)));

    private static CommandRequest<TValue> Request<TGetter, TValue>(
        RhinoDoc document,
        Seq<CommandPolicy<TGetter>> policies,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<CommandInput, TGetter, GetResult, Option<TValue>> value,
        Func<TGetter, Fin<Unit>>? configure = null,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)>? transition = null) where TGetter : GetBaseClass, IDisposable =>
        CommandRequest<TValue>.Of(read: input => {
            using TGetter getter = create();
            return from _ in SameDocument(expected: document, actual: input.Document)
                   from configured in configure switch { Func<TGetter, Fin<Unit>> apply => apply(arg: getter), _ => Fin.Succ(value: unit) }
                   from applied in CommandPolicy<TGetter>.Apply(policies: policies, getter: getter)
                   from result in input.ReadWith(
                       getter: getter,
                       options: CommandPolicy<TGetter>.OptionSet(policies: policies),
                       receive: () => receive(arg: getter),
                       value: (g, raw) => value(arg1: input, arg2: g, arg3: raw),
                       transition: transition ?? (static (_, _, selected) => (Continue: false, Selected: selected)))
                   select result;
        });

    private static Fin<Unit> SameDocument(RhinoDoc expected, RhinoDoc actual) =>
        ReferenceEquals(objA: expected, objB: actual) switch {
            true => Fin.Succ(value: unit),
            false => Fin.Fail<Unit>(error: Op.Of(name: nameof(CommandInput)).InvalidInput()),
        };

    private Fin<CommandGet<TValue>> ReadWith<TGetter, TValue>(
        TGetter getter,
        Seq<CommandOption> options,
        Func<GetResult> receive,
        Func<TGetter, GetResult, Option<TValue>> value,
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

    private static Fin<CommandGet<TValue>> ReadLoop<TGetter, TValue>(
        TGetter getter,
        CommandOption.Scope scope,
        Func<GetResult> receive,
        Func<TGetter, GetResult, Option<TValue>> value,
        Option<CommandOptionValue> selected,
        Func<TGetter, GetResult, Option<CommandOptionValue>, (bool Continue, Option<CommandOptionValue> Selected)> transition) where TGetter : GetBaseClass =>
        receive() switch {
            GetResult.Cancel => Fin.Fail<CommandGet<TValue>>(error: new Fault.Cancelled()),
            GetResult.Option => scope.Snapshot(getter: getter).Bind(option => transition(getter, GetResult.Option, Some(option)) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, transition: transition),
                (false, Option<CommandOptionValue> next) => Fin.Succ(value: CommandGet<TValue>.Of(getter: getter, raw: GetResult.Option, value: Project(value: value(getter, GetResult.Option), option: next), option: next)),
            }),
            GetResult raw => transition(getter, raw, selected) switch {
                (true, Option<CommandOptionValue> next) => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, selected: next, transition: transition),
                (false, Option<CommandOptionValue> next) => Fin.Succ(value: CommandGet<TValue>.Of(getter: getter, raw: raw, value: Project(value: value(getter, raw), option: next), option: next)),
            },
        };

    private static Option<TValue> Project<TValue>(Option<TValue> value, Option<CommandOptionValue> option) =>
        value.Case switch {
            TValue typed => Some(typed),
            _ => option.Bind(static selected => selected is TValue projected ? Some(projected) : Option<TValue>.None),
        };

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

    private static Fin<Unit> Limits(GetNumber getter, Option<double> lower, Option<double> upper, bool strictlyLower, bool strictlyUpper) {
        _ = lower.Iter(value => getter.SetLowerLimit(value, strictlyLower));
        _ = upper.Iter(value => getter.SetUpperLimit(value, strictlyUpper));
        return Fin.Succ(value: unit);
    }

    private static Fin<Unit> Limits(GetInteger getter, Option<int> lower, Option<int> upper, bool strictlyLower, bool strictlyUpper) {
        _ = lower.Iter(value => getter.SetLowerLimit(value, strictlyLower));
        _ = upper.Iter(value => getter.SetUpperLimit(value, strictlyUpper));
        return Fin.Succ(value: unit);
    }
}
