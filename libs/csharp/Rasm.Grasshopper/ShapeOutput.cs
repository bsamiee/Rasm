using Grasshopper2.Components;
using Grasshopper2.Types.Numeric;
using Rasm.Analysis;
using Rasm.Domain;
using Rhino.Geometry;
namespace Rasm.Grasshopper;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ShapeOutput {
    public static IOutputGroup Query<TOut>(Port<Shape> input, Port<TOut> port, Func<IDataAccess, GrasshopperRuntime, Analysis.Query<object, TOut>> operation, bool emptyUnsupported = false) =>
        Output.Prepared(
            source: (access, runtime) =>
                from shape in runtime.Shape(access: access, port: input)
                from context in runtime.Scope.Context
                from values in operation(arg1: access, arg2: runtime).Apply(geometry: shape.Inner).Run(env: Bridge.Runtime(access: access, context: context))
                select values,
            emptyUnsupported: emptyUnsupported,
            slots: [Output.Slot(port: port)]);
    public static IOutputGroup CurveDetails(Port<Shape> input, Port<Curve> curves, Port<ComponentIndex> sources, Port<CurveFeature> features, Func<IDataAccess, GrasshopperRuntime, Curves> aspect, bool emptyUnsupported = false) =>
        Output.Prepared(
            source: (access, runtime) =>
                from shape in runtime.Shape(access: access, port: input)
                from context in runtime.Scope.Context
                from values in Analysis.Query.CurveProjections<object>(geometry: shape.Inner, aspect: aspect(arg1: access, arg2: runtime)).Run(env: Bridge.Runtime(access: access, context: context))
                select values,
            emptyUnsupported: emptyUnsupported,
            slots: [
                Output.Slot<CurveProjection, Curve>(port: curves, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.Curve)))),
                Output.Slot<CurveProjection, ComponentIndex>(port: sources, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.Source)))),
                Output.Slot<CurveProjection, CurveFeature>(port: features, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.Feature)))),
            ]);
    public static IOutputGroup FaceDetails(Port<Shape> input, Faces selector, Port<Brep> breps, Port<int> indices) =>
        Output.Prepared(
            source: (access, runtime) =>
                from shape in runtime.Shape(access: access, port: input)
                from context in runtime.Scope.Context
                from values in Analysis.Query.FaceProjections<object>(geometry: shape.Inner, selector: selector).Run(env: Bridge.Runtime(access: access, context: context))
                select values,
            slots: [
                Output.Slot<FaceProjection, Brep>(port: breps, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.Brep)))),
                Output.Slot<FaceProjection, int>(port: indices, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.FaceIndex)))),
            ]);
    public static IOutputGroup IndexedFaceDetails(Port<Shape> input, Port<int> index, Port<Brep> brep, Port<Plane> frame, Port<Point3d> center, Port<Vector3d> normal, Port<int> face, Port<ComponentIndex> component, Port<Interval> domains) =>
        Output.Prepared(
            source: (access, runtime) =>
                from shape in runtime.Shape(access: access, port: input)
                from context in runtime.Scope.Context
                from values in Analysis.Query.FaceProjections<object>(geometry: shape.Inner, selector: Analysis.Faces.At(index: runtime.Hints.Index(access: access, port: index, limit: int.MaxValue).Map(static value => (int?)value).IfNone(static () => null))).Run(env: Bridge.Runtime(access: access, context: context))
                select values,
            slots: [
                Output.Slot<FaceProjection, Brep>(port: brep, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.Brep)))),
                Output.Slot<FaceProjection, Plane>(port: frame, project: static (runtime, values) => runtime.Scope.Context.Bind(context => values.Traverse(value => Analysis.Query.FrameAtCentroid(face: value, runtime: context)).Map(static items => items.Map(static value => OutputValue.Plain(value: value))).As())),
                Output.Slot<FaceProjection, Point3d>(port: center, project: static (runtime, values) => runtime.Scope.Context.Bind(context => values.Traverse(value => Analysis.Query.FaceCentroid(face: value, runtime: context)).Map(static items => items.Map(static value => OutputValue.Plain(value: value))).As())),
                Output.Slot<FaceProjection, Vector3d>(port: normal, project: static (runtime, values) => runtime.Scope.Context.Bind(context => values.Traverse(value => Analysis.Query.FrameAtCentroid(face: value, runtime: context).Map(static value => value.ZAxis)).Map(static items => items.Map(static value => OutputValue.Plain(value: value))).As())),
                Output.Slot<FaceProjection, int>(port: face, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: value.FaceIndex)))),
                Output.Slot<FaceProjection, ComponentIndex>(port: component, project: static (_, values) => Fin.Succ(values.Map(static value => OutputValue.Plain(value: new ComponentIndex(type: ComponentIndexType.BrepFace, index: value.FaceIndex))))),
                Output.Slot<FaceProjection, Interval>(port: domains, project: static (_, values) => Fin.Succ(values.Bind(static value => Seq(value.Brep.Faces[0].Domain(direction: 0), value.Brep.Faces[0].Domain(direction: 1))).Map(static value => OutputValue.Plain(value: value)))),
            ]);
}
