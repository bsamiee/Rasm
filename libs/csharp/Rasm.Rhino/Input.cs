using Color = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using Result = Rhino.Commands.Result;

namespace Rasm.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct CommandPointMode(bool OnMouseUp = false, bool TwoDimensional = false);

public readonly record struct CommandPoint(
    Option<Point3d> Point,
    Option<DrawingPoint> WindowPoint,
    Option<RhinoView> View,
    Option<CommandSelection.Reference> Object,
    Option<double> CurveParameter,
    Option<Point2d> SurfaceParameter,
    Option<Point2d> BrepParameter,
    Option<double> NumberPreview,
    global::Rhino.ApplicationSettings.OsnapModes Osnap) {
    internal static CommandPoint Of(GetPoint getter, GetResult raw) =>
        new(
            Point: raw switch { GetResult.Point => Some(getter.Point()), _ => Option<Point3d>.None },
            WindowPoint: raw switch { GetResult.Point or GetResult.Point2d => Some<DrawingPoint>(getter.Point2d()), _ => Option<DrawingPoint>.None },
            View: Optional(getter.View()),
            Object: PointObject(getter: getter),
            CurveParameter: CurveParameterOf(getter: getter),
            SurfaceParameter: SurfaceParameterOf(getter: getter),
            BrepParameter: BrepParameterOf(getter: getter),
            NumberPreview: NumberPreviewOf(getter: getter),
            Osnap: getter.OsnapEventType);

    private static Option<CommandSelection.Reference> PointObject(GetPoint getter) =>
        Optional(getter.PointOnObject()).Map(reference => {
            using ObjRef owned = reference;
            return CommandSelection.Reference.Of(reference: owned, preselected: false);
        });

    private static Option<double> CurveParameterOf(GetPoint getter) =>
        getter.PointOnCurve(t: out double parameter) switch { Curve => Some(parameter), _ => Option<double>.None };

    private static Option<Point2d> SurfaceParameterOf(GetPoint getter) =>
        getter.PointOnSurface(u: out double u, v: out double v) switch { Surface => Some(new Point2d(u, v)), _ => Option<Point2d>.None };

    private static Option<Point2d> BrepParameterOf(GetPoint getter) =>
        getter.PointOnBrep(u: out double u, v: out double v) switch { BrepFace => Some(new Point2d(u, v)), _ => Option<Point2d>.None };

    private static Option<double> NumberPreviewOf(GetPoint getter) =>
        getter.NumberPreview(number: out double number) switch { true => Some(number), false => Option<double>.None };
}

public readonly record struct CommandObjectSelection(bool PreSelect = true, bool IgnoreUnacceptablePreselectedObjects = true, bool PostSelect = true, bool DeselectAllBeforePostSelect = false, bool OneByOnePostSelect = false, bool SubObjectSelect = false, bool ChooseOneQuestion = false, bool BottomObjectPreference = false, bool GroupSelect = false, bool ProxyBrepFromSubD = false, bool InactiveDetailPick = false, bool ReferenceObjectSelect = false, bool LockedObjectSelect = false, bool IgnoreGrips = true, bool SelPrevious = true, bool Highlight = true, bool AlreadySelectedObjectSelect = false, bool ClearObjectsOnEntry = false, bool UnselectObjectsOnExit = false) {
    public static CommandObjectSelection Default => new(
        PreSelect: true,
        IgnoreUnacceptablePreselectedObjects: true,
        PostSelect: true,
        IgnoreGrips: true,
        SelPrevious: true,
        Highlight: true);
}

public readonly record struct CommandPointConstraintMode(bool AllowPickingPointOffObject = false, bool AllowElevator = true, int WireDensity = 0, int FaceIndex = -1);

public readonly record struct CommandPointSetup(
    Option<(Point3d Value, bool ShowDistance)> BasePoint = default,
    Option<double> DistanceFromBase = default,
    Option<(Point3d Value, bool ShowDistance)> DrawLineFrom = default,
    Option<Color> DynamicDrawColor = default,
    Option<global::Rhino.UI.CursorStyle> Cursor = default,
    Option<bool> ObjectSnapCursors = default,
    Option<bool> DrawLine = default,
    Option<bool> NoRedrawOnExit = default,
    Option<bool> OrthoSnap = default,
    Option<bool> FromOption = default,
    Option<bool> ConstraintOptions = default,
    Option<bool> TabMode = default,
    Option<int> ElevatorMode = default,
    Option<bool> ObjectSnap = default,
    Seq<Point3d> SnapPoints = default,
    Seq<Point3d> ConstructionPoints = default,
    bool ClearSnapPoints = false,
    bool ClearConstructionPoints = false,
    Option<(bool Draw, bool Endpoints)> TangentBar = default,
    Option<(bool Draw, bool Endpoints)> PerpBar = default,
    Option<(bool Draw, bool Reverse)> Arrow = default,
    Option<bool> SnapToCurves = default,
    EventHandler<GetPointDrawEventArgs>? DynamicDraw = null,
    EventHandler<GetPointMouseEventArgs>? MouseMove = null,
    EventHandler<GetPointMouseEventArgs>? MouseDown = null,
    EventHandler<DrawEventArgs>? PostDrawObjects = null,
    Option<bool> FullFrameRedrawDuringGet = default) {
    internal Fin<Unit> Apply(GetPoint getter) {
        _ = BasePoint.Iter(value => getter.SetBasePoint(basePoint: value.Value, showDistanceInStatusBar: value.ShowDistance));
        _ = DistanceFromBase.Iter(getter.ConstrainDistanceFromBasePoint);
        _ = DrawLineFrom.Iter(value => getter.DrawLineFromPoint(startPoint: value.Value, showDistanceInStatusBar: value.ShowDistance));
        _ = DynamicDrawColor.Iter(color => getter.DynamicDrawColor = color);
        _ = Cursor.Iter(getter.SetCursor);
        _ = ObjectSnapCursors.Iter(getter.EnableObjectSnapCursors);
        _ = DrawLine.Iter(getter.EnableDrawLineFromPoint);
        _ = NoRedrawOnExit.Iter(getter.EnableNoRedrawOnExit);
        _ = OrthoSnap.Iter(getter.PermitOrthoSnap);
        _ = FromOption.Iter(getter.PermitFromOption);
        _ = ConstraintOptions.Iter(getter.PermitConstraintOptions);
        _ = TabMode.Iter(getter.PermitTabMode);
        _ = ElevatorMode.Iter(getter.PermitElevatorMode);
        _ = ObjectSnap.Iter(getter.PermitObjectSnap);
        _ = ClearSnapPoints switch { true => Effect(action: getter.ClearSnapPoints), false => unit };
        _ = ClearConstructionPoints switch { true => Effect(action: getter.ClearConstructionPoints), false => unit };
        Fin<Unit> snaps = CountResult(count: SnapPoints.IsEmpty ? 0 : getter.AddSnapPoints(points: [.. SnapPoints]));
        Fin<Unit> construction = CountResult(count: ConstructionPoints.IsEmpty ? 0 : getter.AddConstructionPoints(points: [.. ConstructionPoints]));
        _ = TangentBar.Iter(value => getter.EnableCurveSnapTangentBar(drawTangentBarAtSnapPoint: value.Draw, drawEndPoints: value.Endpoints));
        _ = PerpBar.Iter(value => getter.EnableCurveSnapPerpBar(drawPerpBarAtSnapPoint: value.Draw, drawEndPoints: value.Endpoints));
        _ = Arrow.Iter(value => getter.EnableCurveSnapArrow(drawDirectionArrowAtSnapPoint: value.Draw, reverseArrow: value.Reverse));
        _ = SnapToCurves.Iter(getter.EnableSnapToCurves);
        _ = Optional(DynamicDraw).Iter(handler => getter.DynamicDraw += handler);
        _ = Optional(MouseMove).Iter(handler => getter.MouseMove += handler);
        _ = Optional(MouseDown).Iter(handler => getter.MouseDown += handler);
        _ = Optional(PostDrawObjects).Iter(handler => getter.PostDrawObjects += handler);
        _ = FullFrameRedrawDuringGet.Iter(enabled => getter.FullFrameRedrawDuringGet = enabled);
        return snaps.Bind(_ => construction);
    }

    private static Fin<Unit> CountResult(int count) =>
        count switch {
            >= 0 => Fin.Succ(value: unit),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(CommandPointSetup)).InvalidResult()),
        };

    private static Unit Effect(Action action) {
        action();
        return unit;
    }
}

public abstract record CommandInputPolicy {
    private protected CommandInputPolicy() { }

    public static CommandInputPolicy Use<TGetter>(Func<TGetter, Fin<Unit>> apply) where TGetter : GetBaseClass =>
        new ConfigureCase<TGetter>(Run: apply);
    public static CommandInputPolicy Prompt(string value) => Use<GetBaseClass>(getter => Effect(action: () => getter.SetCommandPrompt(value)));
    public static CommandInputPolicy PromptDefault(string value) => Use<GetBaseClass>(getter => Effect(action: () => getter.SetCommandPromptDefault(value)));
    public static CommandInputPolicy Default(Point3d value) => Use<GetBaseClass>(getter => Effect(action: () => getter.SetDefaultPoint(value)));
    public static CommandInputPolicy Default(double value) => Use<GetBaseClass>(getter => Effect(action: () => getter.SetDefaultNumber(value)));
    public static CommandInputPolicy Default(int value) => Use<GetBaseClass>(getter => Effect(action: () => getter.SetDefaultInteger(value)));
    public static CommandInputPolicy Default(string value) => Use<GetBaseClass>(getter => Effect(action: () => getter.SetDefaultString(value)));
    public static CommandInputPolicy Default(Color value) => Use<GetBaseClass>(getter => Effect(action: () => getter.SetDefaultColor(value)));
    public static CommandInputPolicy Timeout(int milliseconds) => Use<GetBaseClass>(getter => Effect(action: () => getter.SetWaitDuration(milliseconds)));
    public static CommandInputPolicy Accept(GetResult result, bool acceptZero = true) => Use<GetBaseClass>(getter => result switch {
        GetResult.Nothing => Effect(action: () => getter.AcceptNothing(enable: true)),
        GetResult.Undo => Effect(action: () => getter.AcceptUndo(enable: true)),
        GetResult.Number => Effect(action: () => getter.AcceptNumber(enable: true, acceptZero: acceptZero)),
        GetResult.Point => Effect(action: () => getter.AcceptPoint(enable: true)),
        GetResult.Color => Effect(action: () => getter.AcceptColor(enable: true)),
        GetResult.String => Effect(action: () => getter.AcceptString(enable: true)),
        GetResult.CustomMessage => Effect(action: () => getter.AcceptCustomMessage(enable: true)),
        _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Accept)).InvalidInput()),
    });
    public static CommandInputPolicy EnterWhenDone => Use<GetBaseClass>(static getter => Effect(action: () => getter.AcceptEnterWhenDone(enable: true)));
    public static CommandInputPolicy TransparentCommands(bool enabled = true) => Use<GetBaseClass>(getter => Effect(action: () => getter.EnableTransparentCommands(enable: enabled)));
    public static CommandInputPolicy Object(ObjectType filter = ObjectType.AnyObject, GeometryAttributeFilter attributeFilter = default, CommandObjectSelection? selection = null) =>
        Use<GetObject>(getter => ApplyObject(getter: getter, filter: filter, attributeFilter: attributeFilter, selection: selection ?? CommandObjectSelection.Default));
    public static CommandInputPolicy Object(GetObjectGeometryFilter filter) =>
        Use<GetObject>(getter => Optional(filter).ToFin(Fail: Op.Of(name: nameof(Object)).InvalidInput()).Bind(valid => Effect(action: () => getter.SetCustomGeometryFilter(valid))));
    public static CommandInputPolicy ObjectPressEnterWhenDone(bool enabled = true, string? prompt = null) => Use<GetObject>(getter => Effect(action: () => {
        getter.EnablePressEnterWhenDonePrompt(enable: enabled);
        _ = Optional(prompt).Iter(value => getter.SetPressEnterWhenDonePrompt(prompt: value));
    }));
    public static CommandInputPolicy ObjectPickList(RhinoDoc document, params CommandSelection.Reference[] references) =>
        Use<GetObject>(getter => Optional(document)
            .ToFin(Fail: Op.Of(name: nameof(ObjectPickList)).InvalidInput())
            .Bind(doc => toSeq(references).TraverseM(reference => Fin.Succ(reference.UseObjRef(document: doc, project: objRef => {
                getter.AppendToPickList(objref: objRef);
                return unit;
            }))).As())
            .Map(static _ => unit));
    public static CommandInputPolicy ClearObjectPickList => Use<GetObject>(static getter => Effect(action: getter.ClearObjects));
    public static CommandInputPolicy Point(CommandPointSetup setup) => Use<GetPoint>(setup.Apply);
    public static CommandInputPolicy PointTag(object? value) => Use<GetPoint>(getter => Effect(action: () => getter.Tag = value));
    public static CommandInputPolicy Constrain(Point3d from, Point3d to) => Use<GetPoint>(getter => Applied(success: getter.Constrain(from: from, to: to)));
    public static CommandInputPolicy Constrain<TConstraint>(TConstraint constraint, CommandPointConstraintMode mode = default) => Use<GetPoint>(getter => Constrain(getter: getter, constraint: constraint, mode: mode));
    public static CommandInputPolicy ConstrainToConstructionPlane(bool throughBasePoint = false) => Use<GetPoint>(getter => Applied(success: getter.ConstrainToConstructionPlane(throughBasePoint: throughBasePoint)));
    public static CommandInputPolicy ConstrainToTargetPlane => Use<GetPoint>(static getter => Effect(action: getter.ConstrainToTargetPlane));
    public static CommandInputPolicy ConstrainToVirtualCPlaneIntersection(Plane plane) => Use<GetPoint>(getter => Applied(success: getter.ConstrainToVirtualCPlaneIntersection(plane: plane)));
    public static CommandInputPolicy ClearPointConstraints => Use<GetPoint>(static getter => Effect(action: getter.ClearConstraints));
    public static CommandInputPolicy Options(params CommandOption[] options) => new OptionsCase(Values: toSeq(options));

    internal static Fin<Seq<CommandOption>> Apply(Seq<CommandInputPolicy> policies, GetBaseClass getter) =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(CommandInputPolicy)).InvalidInput())
            .Bind(g => policies.TraverseM(policy => policy.Apply(getter: g)).As())
            .Map(static values => values.Bind(static options => options));

    internal static Fin<Unit> ApplyObjectDefaults(GetObject getter) =>
        ApplyObject(getter: getter, filter: ObjectType.AnyObject, attributeFilter: default, selection: CommandObjectSelection.Default);

    private Fin<Seq<CommandOption>> Apply(GetBaseClass getter) =>
        this switch {
            ConfigureCase configure => configure.RunOn(getter: getter).Map(static _ => Seq<CommandOption>()),
            OptionsCase options => Fin.Succ(value: options.Values),
            _ => Fin.Fail<Seq<CommandOption>>(error: Op.Of(name: nameof(CommandInputPolicy)).InvalidInput()),
        };

    private static Fin<Unit> ApplyObject(GetObject getter, ObjectType filter, GeometryAttributeFilter attributeFilter, CommandObjectSelection selection) {
        getter.GeometryFilter = filter;
        getter.GeometryAttributeFilter = attributeFilter;
        getter.EnablePreSelect(selection.PreSelect, selection.IgnoreUnacceptablePreselectedObjects);
        getter.EnablePostSelect(enable: selection.PostSelect);
        getter.DeselectAllBeforePostSelect = selection.DeselectAllBeforePostSelect;
        getter.OneByOnePostSelect = selection.OneByOnePostSelect;
        getter.SubObjectSelect = selection.SubObjectSelect;
        getter.ChooseOneQuestion = selection.ChooseOneQuestion;
        getter.BottomObjectPreference = selection.BottomObjectPreference;
        getter.GroupSelect = selection.GroupSelect;
        getter.ProxyBrepFromSubD = selection.ProxyBrepFromSubD;
        getter.InactiveDetailPickEnabled = selection.InactiveDetailPick;
        getter.ReferenceObjectSelect = selection.ReferenceObjectSelect;
        getter.LockedObjectSelect = selection.LockedObjectSelect;
        getter.EnableIgnoreGrips(enable: selection.IgnoreGrips);
        getter.EnableSelPrevious(enable: selection.SelPrevious);
        getter.EnableHighlight(enable: selection.Highlight);
        getter.AlreadySelectedObjectSelect = selection.AlreadySelectedObjectSelect;
        getter.EnableClearObjectsOnEntry(enable: selection.ClearObjectsOnEntry);
        getter.EnableUnselectObjectsOnExit(enable: selection.UnselectObjectsOnExit);
        return Fin.Succ(value: unit);
    }

    private static Fin<Unit> Constrain<TConstraint>(GetPoint getter, TConstraint constraint, CommandPointConstraintMode mode) =>
        constraint switch {
            Line value => Applied(success: getter.Constrain(line: value)),
            Arc value => Applied(success: getter.Constrain(arc: value)),
            Circle value => Applied(success: getter.Constrain(circle: value)),
            Sphere value => Applied(success: getter.Constrain(sphere: value)),
            Cylinder value => Applied(success: getter.Constrain(cylinder: value)),
            Plane value => Applied(success: getter.Constrain(plane: value, allowElevator: mode.AllowElevator)),
            Curve value => Applied(success: getter.Constrain(curve: value, allowPickingPointOffObject: mode.AllowPickingPointOffObject)),
            Surface value => Applied(success: getter.Constrain(surface: value, allowPickingPointOffObject: mode.AllowPickingPointOffObject)),
            Brep value => Applied(success: getter.Constrain(brep: value, wireDensity: mode.WireDensity, faceIndex: mode.FaceIndex, allowPickingPointOffObject: mode.AllowPickingPointOffObject)),
            Mesh value => Applied(success: getter.Constrain(mesh: value, allowPickingPointOffObject: mode.AllowPickingPointOffObject)),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Constrain)).InvalidInput()),
        };

    private static Fin<Unit> Applied(bool success) =>
        success switch {
            true => Fin.Succ(value: unit),
            false => Fin.Fail<Unit>(error: Op.Of(name: nameof(CommandInputPolicy)).InvalidResult()),
        };

    private static Fin<Unit> Effect(Action action) {
        action();
        return Fin.Succ(value: unit);
    }

    private abstract record ConfigureCase : CommandInputPolicy {
        internal abstract Fin<Unit> RunOn(GetBaseClass getter);
    }

    private sealed record ConfigureCase<TGetter>(Func<TGetter, Fin<Unit>> Run) : ConfigureCase where TGetter : GetBaseClass {
        internal override Fin<Unit> RunOn(GetBaseClass getter) =>
            getter switch {
                TGetter typed => Run(arg: typed),
                _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(CommandInputPolicy)).InvalidInput()),
            };
    }

    private sealed record OptionsCase(Seq<CommandOption> Values) : CommandInputPolicy;
}

public readonly record struct CommandGet<T>(
    GetResult Raw,
    Result CommandResult,
    Option<T> Value,
    Option<object> AcceptedValue,
    Option<CommandOptionValue> Option,
    bool GotDefault,
    Option<RhinoView> View,
    Option<DrawingPoint> WindowPoint) {
    public Option<TValue> Accepted<TValue>() =>
        AcceptedValue.Bind(As<TValue>).Case switch {
            TValue accepted => Some(accepted),
            _ => Option.Bind(static option => option.Value.Bind(As<TValue>)),
        };

    internal static CommandGet<T> Of(GetBaseClass getter, GetResult raw, Option<T> value, Option<CommandOptionValue> option) =>
        new(
            Raw: raw,
            CommandResult: getter.CommandResult(),
            Value: value,
            AcceptedValue: value.Case switch { T primary => Some((object)primary!), _ => RawValue(getter: getter, raw: raw) },
            Option: option,
            GotDefault: getter.GotDefault(),
            View: Optional(getter.View()),
            WindowPoint: raw switch { GetResult.Point or GetResult.Point2d => Some<DrawingPoint>(getter.Point2d()), _ => Option<DrawingPoint>.None });

    private static Option<TValue> As<TValue>(object value) =>
        value switch { TValue typed => Some(typed), _ => Option<TValue>.None };

    private static Option<object> RawValue(GetBaseClass getter, GetResult raw) =>
        raw switch {
            GetResult.Nothing or GetResult.Undo or GetResult.Timeout => Some((object)raw),
            GetResult.Number => Some((object)getter.Number()),
            GetResult.Color => Some((object)getter.Color()),
            GetResult.Point => Some((object)getter.Point()),
            GetResult.Point2d => Some((object)getter.Point2d()),
            GetResult.String => Optional(getter.StringResult()).Map(static value => (object)value),
            GetResult.CustomMessage => Optional(getter.CustomMessage()).Map(static value => value),
            _ => Some((object)raw),
        };
}

public sealed class CommandInputRequest<T> {
    private readonly Func<CommandInput, Fin<CommandGet<T>>> read;

    private CommandInputRequest(Func<CommandInput, Fin<CommandGet<T>>> read) => this.read = read;

    internal static CommandInputRequest<T> Of(Func<CommandInput, Fin<CommandGet<T>>> read) => new(read: read);
    internal Fin<CommandGet<T>> Read(CommandInput input) => read(arg: input);
    internal readonly record struct ReadState(Option<CommandOptionValue> Selected, Seq<Guid> Preselected);
    internal readonly record struct ReadTransition(ReadState State, bool Repeat);
}

public static class CommandInputs {
    public static CommandInputRequest<CommandSelection> Objects(int minimum = 1, int maximum = 1, params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetObject(),
            configure: static getter => CommandInputPolicy.ApplyObjectDefaults(getter: getter),
            receive: getter => getter.GetMultiple(minimumNumber: minimum, maximumNumber: maximum),
            value: static (input, getter, raw, preselected) => SelectionOf(document: input.Document, getter: getter, raw: raw, preselected: preselected),
            transition: ObjectTransition);

    public static CommandInputRequest<CommandPoint> Point(CommandPointMode mode = default, params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetPoint(),
            receive: getter => getter.Get(onMouseUp: mode.OnMouseUp, get2DPoint: mode.TwoDimensional),
            value: static (_, getter, raw, _) => raw switch { GetResult.Point or GetResult.Point2d => Some(CommandPoint.Of(getter: getter, raw: raw)), _ => Option<CommandPoint>.None });

    public static CommandInputRequest<string> Text(bool literal = false, params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetString(),
            receive: getter => literal ? getter.GetLiteralString() : getter.Get(),
            value: static (_, getter, raw, _) => raw switch { GetResult.String => Optional(getter.StringResult()), _ => Option<string>.None });

    public static CommandInputRequest<double> Number(Option<double> lower = default, Option<double> upper = default, bool strictlyLower = false, bool strictlyUpper = false, params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetNumber(),
            configure: getter => Limits(getter: getter, lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper),
            receive: static getter => getter.Get(),
            value: static (_, getter, raw, _) => raw switch { GetResult.Number => Some(getter.Number()), _ => Option<double>.None });

    public static CommandInputRequest<int> Integer(Option<int> lower = default, Option<int> upper = default, bool strictlyLower = false, bool strictlyUpper = false, params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetInteger(),
            configure: getter => Limits(getter: getter, lower: lower, upper: upper, strictlyLower: strictlyLower, strictlyUpper: strictlyUpper),
            receive: static getter => getter.Get(),
            value: static (_, getter, raw, _) => raw switch { GetResult.Number => Some(getter.Number()), _ => Option<int>.None });

    public static CommandInputRequest<CommandOptionValue> Options(params CommandInputPolicy[] policies) =>
        Request(
            policies: toSeq(policies),
            create: static () => new GetOption(),
            receive: static getter => getter.Get(),
            value: static (_, _, _, _) => Option<CommandOptionValue>.None,
            terminalOptions: true);

    private static CommandInputRequest<T> Request<T, TGetter>(
        Seq<CommandInputPolicy> policies,
        Func<TGetter> create,
        Func<TGetter, GetResult> receive,
        Func<CommandInput, TGetter, GetResult, Seq<Guid>, Option<T>> value,
        Func<TGetter, Fin<Unit>>? configure = null,
        Func<GetBaseClass, GetResult, CommandInputRequest<T>.ReadState, CommandInputRequest<T>.ReadTransition>? transition = null,
        bool terminalOptions = false) where TGetter : GetBaseClass, IDisposable =>
        CommandInputRequest<T>.Of(read: input => {
            using TGetter getter = create();
            return from _ in configure switch { Func<TGetter, Fin<Unit>> apply => apply(arg: getter), _ => Fin.Succ(value: unit) }
                   from options in CommandInputPolicy.Apply(policies: policies, getter: getter)
                   from result in input.ReadWith(
                       getter: getter,
                       options: options,
                       receive: () => receive(arg: getter),
                       value: (g, raw, preselected) => value(arg1: input, arg2: (TGetter)g, arg3: raw, arg4: preselected),
                       transition: transition,
                       terminalOptions: terminalOptions)
                   select result;
        });

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000", Justification = "Returned CommandSelection is the accepted command value; command caller owns disposal.")]
    private static Option<CommandSelection> SelectionOf(RhinoDoc document, GetObject getter, GetResult raw, Seq<Guid> preselected) =>
        raw switch {
            GetResult.Object => Optional(CommandSelection.From(
                document: document,
                references: toSeq(Enumerable.Range(start: 0, count: getter.ObjectCount).Select(index => getter.Object(index))),
                preselected: preselected)),
            _ => Option<CommandSelection>.None,
        };

    private static CommandInputRequest<T>.ReadTransition ObjectTransition<T>(GetBaseClass getter, GetResult raw, CommandInputRequest<T>.ReadState state) =>
        (getter, raw) switch {
            (GetObject objects, GetResult.Option) => DisablePreSelect(getter: objects, state: state, repeat: true),
            (GetObject objects, GetResult.Object) when objects.ObjectsWerePreselected => DisablePreSelect(
                getter: objects,
                state: state with { Preselected = state.Preselected + toSeq(Enumerable.Range(start: 0, count: objects.ObjectCount).Select(index => ObjectIdAt(getter: objects, index: index))) },
                repeat: true),
            _ => new CommandInputRequest<T>.ReadTransition(State: state, Repeat: false),
        };

    private static Guid ObjectIdAt(GetObject getter, int index) {
        using ObjRef reference = getter.Object(index);
        return reference.ObjectId;
    }

    private static CommandInputRequest<T>.ReadTransition DisablePreSelect<T>(GetObject getter, CommandInputRequest<T>.ReadState state, bool repeat) {
        getter.EnablePreSelect(false, true);
        return new CommandInputRequest<T>.ReadTransition(State: state, Repeat: repeat);
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

public sealed record CommandInput {
    public CommandInput(RhinoDoc document) {
        ArgumentNullException.ThrowIfNull(argument: document);
        Document = document;
    }

    public RhinoDoc Document { get; }

    public Fin<CommandGet<T>> Read<T>(CommandInputRequest<T> request) =>
        Optional(request)
            .ToFin(Fail: Op.Of(name: nameof(Read)).InvalidInput())
            .Bind(input => input.Read(input: this));

    internal Fin<CommandGet<T>> ReadWith<T>(
        GetBaseClass getter,
        Seq<CommandOption> options,
        Func<GetResult> receive,
        Func<GetBaseClass, GetResult, Seq<Guid>, Option<T>> value,
        Func<GetBaseClass, GetResult, CommandInputRequest<T>.ReadState, CommandInputRequest<T>.ReadTransition>? transition = null,
        bool terminalOptions = false) =>
        Optional(getter)
            .ToFin(Fail: Op.Of(name: nameof(ReadWith)).InvalidInput())
            .Bind(g => CommandOption.Bind(options: options, getter: g).Bind(scope => {
                using CommandOption.Scope active = scope;
                return ReadLoop(
                    getter: g,
                    scope: active,
                    receive: receive,
                    value: value,
                    state: new CommandInputRequest<T>.ReadState(Selected: Option<CommandOptionValue>.None, Preselected: Seq<Guid>()),
                    transition: transition ?? Stay,
                    terminalOptions: terminalOptions);
            }));

    private static Fin<CommandGet<T>> ReadLoop<T>(
        GetBaseClass getter,
        CommandOption.Scope scope,
        Func<GetResult> receive,
        Func<GetBaseClass, GetResult, Seq<Guid>, Option<T>> value,
        CommandInputRequest<T>.ReadState state,
        Func<GetBaseClass, GetResult, CommandInputRequest<T>.ReadState, CommandInputRequest<T>.ReadTransition> transition,
        bool terminalOptions) =>
        receive() switch {
            GetResult.Cancel => Fin.Fail<CommandGet<T>>(error: new Fault.Cancelled()),
            GetResult.Option when terminalOptions => scope.Snapshot(getter: getter).Bind(option => option is T typed
                ? Fin.Succ(value: CommandGet<T>.Of(getter: getter, raw: GetResult.Option, value: Some(typed), option: Some(option)))
                : Fin.Fail<CommandGet<T>>(error: Op.Of(name: nameof(ReadLoop)).InvalidResult())),
            GetResult.Option => scope.Snapshot(getter: getter).Bind(option => transition(getter, GetResult.Option, state with { Selected = Some(option) }).State switch {
                CommandInputRequest<T>.ReadState next => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, state: next, transition: transition, terminalOptions: terminalOptions),
            }),
            GetResult raw => transition(getter, raw, state) switch {
                { Repeat: true, State: CommandInputRequest<T>.ReadState next } => ReadLoop(getter: getter, scope: scope, receive: receive, value: value, state: next, transition: transition, terminalOptions: terminalOptions),
                { State: CommandInputRequest<T>.ReadState next } => Fin.Succ(value: CommandGet<T>.Of(getter: getter, raw: raw, value: value(getter, raw, next.Preselected), option: next.Selected)),
            },
        };

    private static CommandInputRequest<T>.ReadTransition Stay<T>(GetBaseClass getter, GetResult raw, CommandInputRequest<T>.ReadState state) =>
        new(State: state, Repeat: false);
}
