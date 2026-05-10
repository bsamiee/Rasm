using Grasshopper2.Components;
using LanguageExt;
using Rasm.Analysis;
using Rasm.Domain;
using Rhino.Geometry;
namespace Rasm.Grasshopper;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ShapeQuery {
    public static Query<Shape, TOut> BoundingCorners<TOut>() => FromObject(query: Rasm.Analysis.Query.BoundingCorners<object, TOut>()); public static Query<Shape, TOut> Bounds<TOut>(Bounds aspect) => FromObject(query: Rasm.Analysis.Query.Bounds<object, TOut>(aspect: aspect));
    public static Query<Shape, TOut> Curves<TOut>(Curves aspect) => FromObject(query: Rasm.Analysis.Query.Curves<object, TOut>(aspect: aspect)); public static Query<Shape, TOut> EdgeMidpoints<TOut>() => FromObject(query: Rasm.Analysis.Query.EdgeMidpoints<object, TOut>());
    public static Query<Shape, TOut> Faces<TOut>(Faces aspect) => FromObject(query: Rasm.Analysis.Query.Faces<object, TOut>(aspect: aspect)); public static Query<Shape, TOut> Kind<TOut>() => FromObject(query: Rasm.Analysis.Query.Kind<object, TOut>());
    public static Query<Shape, TOut> Locate<TOut>(Location aspect) => FromObject(query: Rasm.Analysis.Query.Locate<object, TOut>(aspect: aspect)); public static Query<Shape, TOut> Quadrants<TOut>() => FromObject(query: Rasm.Analysis.Query.Quadrants<object, TOut>());
    public static Query<Shape, TOut> SpatialMidpoint<TOut>() => FromObject(query: Rasm.Analysis.Query.SpatialMidpoint<object, TOut>()); public static Query<Shape, TOut> Vertices<TOut>() => FromObject(query: Rasm.Analysis.Query.Vertices<object, TOut>());

    private static Query<Shape, TOut> FromObject<TOut>(Query<object, TOut> query) =>
        query.Contramap<Shape>(static shape => shape.Inner);
}
public static class ShapeOutput {
    public static IOutputGroup Query<TOut>(Port<Shape> input, Port<TOut> port, Func<IDataAccess, GrasshopperRuntime, Query<Shape, TOut>> operation, bool emptyUnsupported = false) =>
        Output.Prepared(
            source: (access, runtime) =>
                from shape in runtime.Shape(access: access, port: input)
                from context in runtime.Scope.Context
                from values in operation(arg1: access, arg2: runtime).Apply(geometry: shape).Run(env: Bridge.Runtime(access: access, context: context))
                select values,
            emptyUnsupported: emptyUnsupported,
            slots: [Output.Slot(port: port)]);
    private static IOutputSlot<TSource> Plain<TSource, TOut>(Port<TOut> port, Func<TSource, TOut> project) =>
        Output.Slot<TSource, TOut>(port: port, project: (_, values) => Fin.Succ(values.Map(value => OutputValue.Plain(value: project(arg: value)))));
    private static IOutputSlot<FaceProjection> FaceValue<TOut>(Port<TOut> port, Func<FaceProjection, Context, Fin<TOut>> project) =>
        Output.Slot<FaceProjection, TOut>(port: port, project: (runtime, values) =>
            runtime.Scope.Context
                .Bind(context => values.Traverse(face => project(arg1: face, arg2: context)).As())
                .Map(values => values.Map(static value => OutputValue.Plain(value: value))));
    public static IOutputGroup CurveDetails(Port<Shape> input, Port<Curve> curves, Port<ComponentIndex> sources, Port<CurveFeature> features, Func<IDataAccess, GrasshopperRuntime, Curves> aspect, bool emptyUnsupported = false) =>
        Output.Prepared(
            source: (access, runtime) =>
                from shape in runtime.Shape(access: access, port: input)
                from context in runtime.Scope.Context
                from values in Rasm.Analysis.Query.CurveProjections(shape: shape, aspect: aspect(arg1: access, arg2: runtime)).Run(env: Bridge.Runtime(access: access, context: context))
                select values,
            emptyUnsupported: emptyUnsupported,
            slots: [
                Plain<CurveProjection, Curve>(port: curves, project: static value => value.Curve),
                Plain<CurveProjection, ComponentIndex>(port: sources, project: static value => value.Source),
                Plain<CurveProjection, CurveFeature>(port: features, project: static value => value.Feature),
            ]);
    public static IOutputGroup FaceDetails(Port<Shape> input, Port<Brep> faces, Port<int> indices, Func<IDataAccess, GrasshopperRuntime, Faces> selector) =>
        Output.Prepared(
            source: (access, runtime) =>
                from shape in runtime.Shape(access: access, port: input)
                from context in runtime.Scope.Context
                from values in Rasm.Analysis.Query.FaceProjections(shape: shape, selector: selector(arg1: access, arg2: runtime)).Run(env: Bridge.Runtime(access: access, context: context))
                select values,
            slots: [
                Plain<FaceProjection, Brep>(port: faces, project: static value => value.Brep),
                Plain<FaceProjection, int>(port: indices, project: static value => value.FaceIndex),
            ]);
    public static IOutputGroup IndexedFaceDetails(Port<Shape> input, Port<int> index, Port<Brep> faces, Port<Plane> frames, Port<Point3d> centers, Port<Vector3d> normals, Port<int> indices, Port<ComponentIndex> components, Port<Interval> domains) =>
        Output.Prepared(
            source: (access, runtime) =>
                from shape in runtime.Shape(access: access, port: input)
                from context in runtime.Scope.Context
                from values in Rasm.Analysis.Query.FaceProjections(
                        shape: shape,
                        selector: Faces.At(index: runtime.Hints.Index(access: access, port: index, limit: int.MaxValue).Map(static value => (int?)value).IfNone(static () => null)))
                    .Run(env: Bridge.Runtime(access: access, context: context))
                select values,
            slots: [
                Plain<FaceProjection, Brep>(port: faces, project: static value => value.Brep),
                FaceValue(port: frames, project: static (face, context) => Rasm.Analysis.Query.FrameAtCentroid(face: face, runtime: context)),
                FaceValue(port: centers, project: static (face, context) => Rasm.Analysis.Query.FaceCentroid(face: face, runtime: context)),
                FaceValue(port: normals, project: static (face, context) => Rasm.Analysis.Query.FrameAtCentroid(face: face, runtime: context).Map(static frame => frame.ZAxis)),
                Plain<FaceProjection, int>(port: indices, project: static value => value.FaceIndex),
                Plain<FaceProjection, ComponentIndex>(port: components, project: static value => new ComponentIndex(type: ComponentIndexType.BrepFace, index: value.FaceIndex)),
                Output.Slot<FaceProjection, Interval>(port: domains, project: static (_, values) => values.Traverse(static face => (face.Brep.Faces[0].Domain(direction: 0), face.Brep.Faces[0].Domain(direction: 1)) switch {
                    (Interval u, Interval v) when u.IsValid && v.IsValid => Fin.Succ(Seq(
                        OutputValue.Plain(value: u),
                        OutputValue.Plain(value: v))),
                    _ => Fin.Fail<Seq<OutputValue<Interval>>>(LanguageExt.Common.Error.New(message: "Indexed face produced invalid UV domains.")),
                }).Map(static nested => nested.Bind(static value => value)).As()),
            ]);
}
