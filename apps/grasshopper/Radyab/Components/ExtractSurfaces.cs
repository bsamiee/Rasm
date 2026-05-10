using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Grasshopper;
using Rhino.Geometry;
using static LanguageExt.Prelude;
using Query = Rasm.Analysis.Query;

namespace Radyab.Components;

// --- [EXPORTS] -------------------------------------------------------------------------

[IoId("F51A09A8-A5A5-467A-ADBA-C950511A0020")]
[Nomen(
    name: "Extract Surfaces",
    info: "Trimmed faces, world-Z top/bottom faces, indexed face details, and centroid UV frame for Brep, BrepFace, Surface, and SubD values.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractSurfaces : ShapeComponent<ExtractSurfaces.Spec> {
    public sealed class Spec : IComponentSpec<Shape, ShapeState> {
        private static readonly Port<int> Index = Port.Index(info: "Zero-based face selector; missing Index defaults to face 0 and supplied values clamp to [0, count-1].");
        private static Seq<IPort> Controls { get; } = Seq<IPort>(Index);

        public static Seq<IPort> Inputs =>
            ShapeFoundation.Inputs(controls: Controls);
        public static Seq<IOutputGroup<ShapeState>> Outputs { get; } = Seq<IOutputGroup<ShapeState>>(
            Output.Prepared<ShapeState, FaceProjection>(
                source: static (_, state) => Faces(state: state, selector: Rasm.Analysis.Faces.All),
                Output.Slot<ShapeState, FaceProjection, Brep>(
                    port: Port.List<Brep>(name: "All Surfaces", code: "AS", info: "Every face as a trimmed single-face Brep. Mesh input is intentionally rejected."),
                    project: static (_, faces) => Fin.Succ(faces.Map(static face => OutputValue.Plain(value: face.Brep)))),
                Output.Slot<ShapeState, FaceProjection, int>(
                    port: Port.List<int>(name: "Surface Indices", code: "SI", info: "Source Brep face index aligned with every extracted surface."),
                    project: static (_, faces) => Fin.Succ(faces.Map(static face => OutputValue.Plain(value: face.FaceIndex))))),
            ShapeQuery(port: Port.List<Brep>(name: "Top Surface", code: "TS", info: "Trimmed face(s) with maximum world-Z centroid; ties within tolerance."), query: static () => Query.Faces<object, Brep>(aspect: Rasm.Analysis.Faces.Top)),
            ShapeQuery(port: Port.List<Brep>(name: "Bottom Surface", code: "BS", info: "Trimmed face(s) with minimum world-Z centroid; ties within tolerance."), query: static () => Query.Faces<object, Brep>(aspect: Rasm.Analysis.Faces.Bottom)),
            Output.Prepared<ShapeState, FaceProjection>(
                source: static (access, state) => Indexed(access: access, state: state),
                Output.Slot<ShapeState, FaceProjection, Brep>(
                    port: Port.List<Brep>(name: "Indexed Surface", code: "IS", info: "Trimmed single-face Brep at Index input; missing Index defaults to 0, supplied values clamp to [0, count-1]. Empty when zero faces."),
                    project: static (_, faces) => Fin.Succ(faces.Map(static face => OutputValue.Plain(value: face.Brep)))),
                Output.Slot<ShapeState, FaceProjection, Plane>(
                    port: Port.List<Plane>(name: "UV Frame", code: "UV", info: "Native U/V frame at the indexed face centroid. X=surface U direction, Z=orientation-corrected normal, Y completes the basis."),
                    project: static (state, faces) => Spec.WithContext<Plane>(state: state, project: context => faces.Traverse(face => Query.FrameAtCentroid(face: face, runtime: context)).Map(static frames => frames.Map(static frame => OutputValue.Plain(value: frame))).As())),
                Output.Slot<ShapeState, FaceProjection, Point3d>(
                    port: Port.List<Point3d>(name: "Face Center", code: "FC", info: "Area centroid of the indexed trimmed face."),
                    project: static (state, faces) => Spec.WithContext<Point3d>(state: state, project: context => faces.Traverse(face => Query.FaceCentroid(face: face, runtime: context)).Map(static points => points.Map(static point => OutputValue.Plain(value: point))).As())),
                Output.Slot<ShapeState, FaceProjection, Vector3d>(
                    port: Port.List<Vector3d>(name: "Face Normal", code: "FN", info: "Orientation-corrected indexed face normal at the face centroid."),
                    project: static (state, faces) => Spec.WithContext<Vector3d>(state: state, project: context => faces.Traverse(face => Query.FrameAtCentroid(face: face, runtime: context).Map(static frame => frame.ZAxis)).Map(static normals => normals.Map(static normal => OutputValue.Plain(value: normal))).As())),
                Output.Slot<ShapeState, FaceProjection, int>(
                    port: Port.List<int>(name: "Face Index", code: "FI", info: "Source Brep face index selected by the clamped Index input."),
                    project: static (_, faces) => Fin.Succ(faces.Map(static face => OutputValue.Plain(value: face.FaceIndex)))),
                Output.Slot<ShapeState, FaceProjection, ComponentIndex>(
                    port: Port.List<ComponentIndex>(param: Param.Generic, name: "Face Component", code: "CI", info: "Source Brep face component index selected by the clamped Index input."),
                    project: static (_, faces) => Fin.Succ(faces.Map(static face => OutputValue.Plain(value: new ComponentIndex(type: ComponentIndexType.BrepFace, index: face.FaceIndex))))),
                Output.Slot<ShapeState, FaceProjection, Interval>(
                    port: Port.List<Interval>(name: "UV Domains", code: "UD", info: "Indexed face domains as two intervals: U first, then V."),
                    project: static (_, faces) => Fin.Succ(faces.Bind(static face => Seq(face.Brep.Faces[0].Domain(direction: 0), face.Brep.Faces[0].Domain(direction: 1))).Map(static domain => OutputValue.Plain(value: domain))))));

        public static Fin<Shape> Read(IDataAccess access) =>
            ShapeFoundation.Read(access: access);
        public static Fin<ShapeState> Prepare(IDataAccess access, Analyze.Scope scope, Hints hints, Shape input) =>
            ShapeFoundation.Prepare(access: access, scope: scope, hints: hints, input: input);

        private static IOutputGroup<ShapeState> ShapeQuery<TOut>(Port<TOut> port, Func<Query<object, TOut>> query) =>
            Output.Query<ShapeState, object, TOut>(
                scope: static state => state.Scope,
                select: static state => state.Geometry,
                query: _ => query(),
                slots: Output.Slot<ShapeState, TOut>(port: port));
        private static Fin<Seq<FaceProjection>> Faces(ShapeState state, Rasm.Analysis.Faces selector) =>
            state.Scope.Context.Bind(context => Query.FaceProjections<object>(geometry: state.Geometry, selector: selector).Run(env: context));
        private static Fin<Seq<FaceProjection>> Indexed(IDataAccess access, ShapeState state) =>
            Faces(state: state, selector: Rasm.Analysis.Faces.All).Map(faces => faces.Count switch {
                0 => Seq<FaceProjection>(),
                int count => Seq(faces[Math.Clamp(value: state.Hints.Index(access: access, port: Index, limit: count).IfNone(static () => 0), min: 0, max: count - 1)]),
            });
        private static Fin<Seq<OutputValue<TOut>>> WithContext<TOut>(ShapeState state, Func<Context, Fin<Seq<OutputValue<TOut>>>> project) =>
            state.Scope.Context.Bind(project);
    }

    public ExtractSurfaces() : base(nomen: ComponentNomen.Of<ExtractSurfaces>()) { }
    public ExtractSurfaces(IReader reader) : base(reader: reader) { }
}
