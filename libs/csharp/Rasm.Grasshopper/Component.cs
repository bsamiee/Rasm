using System.Reflection;
using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using Rasm.Analysis;
using Rasm.Domain;
using Rhino.Geometry;
namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IComponentSpec<TInput, TState>
    where TInput : notnull
    where TState : notnull {
    public static abstract Seq<IPort> Inputs { get; }
    public static abstract Seq<IOutputGroup<TState>> Outputs { get; }
    public static abstract Fin<TInput> Read(IDataAccess access);
    public static abstract Fin<TState> Prepare(IDataAccess access, Analyze.Scope scope, Hints hints, TInput input);
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct ShapeState(Analyze.Scope Scope, Hints Hints, Shape Input) {
    public object Geometry =>
        Input.Inner;
}

public static class ComponentNomen {
    public static Nomen Of<TSelf>() =>
        typeof(TSelf).GetCustomAttribute<NomenAttribute>()?.Nomen
            ?? new Nomen(name: typeof(TSelf).Name, info: string.Empty);
}

public static class ShapeFoundation {
    public static readonly Port<Shape> Geometry = Port.Required<Shape>(
        param: Param.Generic,
        name: "Geometry",
        code: "G",
        info: "Geometry to analyse.");

    public static Fin<Shape> Read(IDataAccess access) =>
        access.ReadShape(slot: 0, port: Geometry);
    public static Fin<ShapeState> Prepare(IDataAccess access, Analyze.Scope scope, Hints hints, Shape input) {
        ArgumentNullException.ThrowIfNull(argument: access);
        ArgumentNullException.ThrowIfNull(argument: scope);
        return Fin.Succ(new ShapeState(Scope: scope, Hints: hints, Input: input));
    }
    public static Seq<IPort> Inputs(Seq<IPort> controls) =>
        toSeq(Seq<IPort>(Geometry).Concat(second: controls));
    public static Analysis.Query<Shape, TOut> EdgeMidpoints<TOut>() =>
        OverShape(query: Analysis.Query.EdgeMidpoints<object, TOut>());
    public static Analysis.Query<Shape, TOut> SpatialMidpoint<TOut>() =>
        OverShape(query: Analysis.Query.SpatialMidpoint<object, TOut>());
    public static Analysis.Query<Shape, TOut> Bounds<TOut>(Bounds aspect) =>
        OverShape(query: Analysis.Query.Bounds<object, TOut>(aspect: aspect));
    public static Analysis.Query<Shape, TOut> BoundingCorners<TOut>() =>
        OverShape(query: Analysis.Query.BoundingCorners<object, TOut>());
    public static Analysis.Query<Shape, TOut> Quadrants<TOut>() =>
        OverShape(query: Analysis.Query.Quadrants<object, TOut>());
    public static Analysis.Query<Shape, TOut> Vertices<TOut>() =>
        OverShape(query: Analysis.Query.Vertices<object, TOut>());
    public static Analysis.Query<Shape, TOut> Locate<TOut>(Location aspect) =>
        OverShape(query: Analysis.Query.Locate<object, TOut>(aspect: aspect));
    public static Analysis.Query<Shape, TOut> Kind<TOut>() =>
        OverShape(query: Analysis.Query.Kind<object, TOut>());
    public static Analysis.Query<Shape, TOut> Curves<TOut>(Curves aspect) =>
        OverShape(query: Analysis.Query.Curves<object, TOut>(aspect: aspect));
    public static Analysis.Query<Shape, TOut> Faces<TOut>(Faces aspect) =>
        OverShape(query: Analysis.Query.Faces<object, TOut>(aspect: aspect));
    public static IOutputGroup<ShapeState> Query<TOut>(Port<TOut> port, Func<Analysis.Query<Shape, TOut>> operation, bool emptyUnsupported = false) =>
        Query(port: port, operation: (_, _) => operation(), emptyUnsupported: emptyUnsupported);
    public static IOutputGroup<ShapeState> Query<TOut>(Port<TOut> port, Func<IDataAccess, ShapeState, Analysis.Query<Shape, TOut>> operation, bool emptyUnsupported = false) =>
        Output.Prepared<ShapeState, TOut>(
            source: (access, state) => Bridge.Values(access: access, scope: state.Scope, input: state.Input, operation: operation(arg1: access, arg2: state)),
            emptyUnsupported: emptyUnsupported,
            slots: [Output.Slot<ShapeState, TOut>(port: port)]);
    public static IOutputGroup<ShapeState> CurveDetails(Port<Curve> curves, Port<ComponentIndex> sources, Port<CurveFeature> features, Func<IDataAccess, ShapeState, Curves> aspect, bool emptyUnsupported = false) =>
        Output.Prepared<ShapeState, CurveProjection>(
            source: (access, state) => state.Scope.Context.Bind(context => Analysis.Query.CurveProjections<object>(geometry: state.Geometry, aspect: aspect(arg1: access, arg2: state)).Run(env: Bridge.Runtime(access: access, context: context))),
            emptyUnsupported: emptyUnsupported,
            slots: [
                Output.Slot<ShapeState, CurveProjection, Curve>(port: curves, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.Curve)))),
                Output.Slot<ShapeState, CurveProjection, ComponentIndex>(port: sources, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.Source)))),
                Output.Slot<ShapeState, CurveProjection, CurveFeature>(port: features, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.Feature)))),
            ]);
    public static IOutputGroup<ShapeState> FaceDetails(Faces selector, Port<Brep> breps, Port<int> indices) =>
        Output.Prepared<ShapeState, FaceProjection>(
            source: (access, state) => state.Scope.Context.Bind(context => Analysis.Query.FaceProjections<object>(geometry: state.Geometry, selector: selector).Run(env: Bridge.Runtime(access: access, context: context))),
            slots: [
                Output.Slot<ShapeState, FaceProjection, Brep>(port: breps, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.Brep)))),
                Output.Slot<ShapeState, FaceProjection, int>(port: indices, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.FaceIndex)))),
            ]);
    public static IOutputGroup<ShapeState> IndexedFaceDetails(Port<int> index, Port<Brep> brep, Port<Plane> frame, Port<Point3d> center, Port<Vector3d> normal, Port<int> face, Port<ComponentIndex> component, Port<Interval> domains) =>
        Output.Prepared<ShapeState, FaceProjection>(
            source: (access, state) => state.Scope.Context.Bind(context => Analysis.Query.FaceProjections<object>(geometry: state.Geometry, selector: Analysis.Faces.At(index: state.Hints.Index(access: access, port: index, limit: int.MaxValue).Map(static value => (int?)value).IfNone(static () => null))).Run(env: Bridge.Runtime(access: access, context: context))),
            slots: [
                Output.Slot<ShapeState, FaceProjection, Brep>(port: brep, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.Brep)))),
                Output.Slot<ShapeState, FaceProjection, Plane>(port: frame, project: static (state, values) => state.Scope.Context.Bind(context => values.Traverse(value => Analysis.Query.FrameAtCentroid(face: value, runtime: context)).Map(static items => items.Map(static value => OutputValue.Plain(value: value))).As())),
                Output.Slot<ShapeState, FaceProjection, Point3d>(port: center, project: static (state, values) => state.Scope.Context.Bind(context => values.Traverse(value => Analysis.Query.FaceCentroid(face: value, runtime: context)).Map(static items => items.Map(static value => OutputValue.Plain(value: value))).As())),
                Output.Slot<ShapeState, FaceProjection, Vector3d>(port: normal, project: static (state, values) => state.Scope.Context.Bind(context => values.Traverse(value => Analysis.Query.FrameAtCentroid(face: value, runtime: context).Map(static value => value.ZAxis)).Map(static items => items.Map(static value => OutputValue.Plain(value: value))).As())),
                Output.Slot<ShapeState, FaceProjection, int>(port: face, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.FaceIndex)))),
                Output.Slot<ShapeState, FaceProjection, ComponentIndex>(port: component, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: new ComponentIndex(type: ComponentIndexType.BrepFace, index: value.FaceIndex))))),
                Output.Slot<ShapeState, FaceProjection, Interval>(port: domains, project: static (_, values) => Fin.Succ(values.Bind(static value => Seq(value.Brep.Faces[0].Domain(direction: 0), value.Brep.Faces[0].Domain(direction: 1))).Map(static value => OutputValue.Plain(value: value)))),
            ]);
    private static Analysis.Query<Shape, TOut> OverShape<TOut>(Analysis.Query<object, TOut> query) =>
        query.Contramap<Shape>(map: static shape => shape.Inner);
}

// --- [SERVICES] ------------------------------------------------------------------------

public abstract class Component<TSpec, TInput, TState> : Grasshopper2.Components.Component
    where TSpec : IComponentSpec<TInput, TState>
    where TInput : notnull
    where TState : notnull {
    protected Component(Nomen nomen) : base(nomen: nomen) { }
    protected Component(IReader reader) : base(reader: reader) { }

    protected sealed override void AddInputs(InputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        _ = TSpec.Inputs.Iter(port => port.Param.Bind(adder: inputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, requirement: port.Requirement, policy: port.Policy));
    }
    protected sealed override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        _ = TSpec.Outputs.Bind(static group => group.Ports).Iter(port => port.Param.Bind(adder: outputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, policy: port.Policy));
    }
    protected sealed override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Hints hints = Hints.Capture(inputs: TSpec.Inputs, access: access);
        _ = (
            from scope in access.Scope()
            from input in TSpec.Read(access: access)
            from state in TSpec.Prepare(access: access, scope: scope, hints: hints, input: input)
            select state)
            .Match(
                Succ: state => TSpec.Outputs.Fold(
                    initialState: 0,
                    f: (slot, group) => (group.Run(access: access, slot: slot, state: state), slot + group.Ports.Count).Item2) switch {
                        _ => Unit.Default,
                    },
                Fail: error => (
                    access.MissingInput(error: error),
                    TSpec.Outputs.Fold(
                        initialState: 0,
                        f: (slot, group) => (group.Empty(access: access, slot: slot), slot + group.Ports.Count).Item2) switch {
                            _ => Unit.Default,
                        }).Item2);
    }
}

public abstract class ShapeComponent<TSpec> : Component<TSpec, Shape, ShapeState>
    where TSpec : IComponentSpec<Shape, ShapeState> {
    protected ShapeComponent(Nomen nomen) : base(nomen: nomen) { }
    protected ShapeComponent(IReader reader) : base(reader: reader) { }
}
