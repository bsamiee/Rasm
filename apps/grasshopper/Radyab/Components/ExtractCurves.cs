using Grasshopper2.Components;
using Grasshopper2.Types.Numeric;
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

[IoId("B88A1248-28B8-4F87-A178-6BCBD2458B33")]
[Nomen(
    name: "Extract Curves",
    info: "All, boundary, mid-domain U/V iso, and index-clamped curves from Rhino curve, surface, Brep, SubD, and mesh topology values.",
    category: "Radyab",
    section: "Extraction")]
public sealed class ExtractCurves : ShapeComponent<ExtractCurves.Spec> {
    public sealed class Spec : IComponentSpec<Shape, ShapeState> {
        private static readonly Port<int> Index = Port.Index(info: "Zero-based curve selector; missing Index defaults to curve 0 and supplied values clamp to [0, count-1].");
        private static readonly Port<Vector3d> Direction = Port.Optional<Vector3d>(name: "Direction", code: "D", info: "Parallel projection or pull direction for silhouette and draft extraction. Missing Direction uses world Z.");
        private static readonly Port<Angle> DraftAngle = Port.Optional<Angle>(param: Param.Angle, name: "Draft Angle", code: "A", info: "Draft angle for draft-curve extraction. Missing Draft Angle uses 0 radians.");
        private static Seq<IPort> Controls { get; } = Seq<IPort>(Index, Direction, DraftAngle);

        public static Seq<IPort> Inputs =>
            ShapeFoundation.Inputs(controls: Controls);
        public static Seq<IOutputGroup<ShapeState>> Outputs { get; } = Seq<IOutputGroup<ShapeState>>(
            Output.Prepared<ShapeState, CurveProjection>(
                source: static (_, state) => Curves(state: state, aspect: Rasm.Analysis.Curves.All),
                CurvesSlot(port: Port.List<Curve>(name: "All Curves", code: "AC", info: "All native curve pieces: structural curve segments, Brep/SubD edges, surface boundary iso curves, or mesh topology edge curves.")),
                Output.Slot<ShapeState, CurveProjection, ComponentIndex>(
                    port: Port.List<ComponentIndex>(param: Param.Generic, name: "Curve Sources", code: "CS", info: "Component index aligned with All Curves."),
                    project: static (_, curves) => Fin.Succ(curves.Map(static curve => OutputValue.Plain(value: curve.Source)))),
                Output.Slot<ShapeState, CurveProjection, CurveFeature>(
                    port: Port.List<CurveFeature>(param: Param.Enum(initial: CurveFeature.Input), name: "Curve Features", code: "CF", info: "Feature label aligned with All Curves."),
                    project: static (_, curves) => Fin.Succ(curves.Map(static curve => OutputValue.Plain(value: curve.Feature))))),
            CurvesOutput(port: Port.List<Curve>(name: "Segments", code: "S", info: "Structural segments from the curve data model, Brep/SubD edges, or mesh topology edge lines."), aspect: Rasm.Analysis.Curves.Segments),
            CurvesOutput(port: Port.List<Curve>(name: "Sub Curves", code: "SCV", info: "Geometry-based curve subcurves, matching Rhino explode-style curve decomposition."), aspect: Rasm.Analysis.Curves.SubCurves),
            CurvesOutput(port: Port.List<Curve>(name: "Boundary Curves", code: "BC", info: "Boundary curves: naked Brep/mesh edges, surface/face boundary iso curves, or the input curve itself."), aspect: Rasm.Analysis.Curves.Boundary),
            CurvesOutput(port: Port.List<Curve>(name: "Naked Outer", code: "NO", info: "Outer naked Brep edges; mesh naked boundary polylines where mesh topology has no inner/outer split."), aspect: Rasm.Analysis.Curves.NakedOuter),
            CurvesOutput(port: Port.List<Curve>(name: "Naked Inner", code: "NI", info: "Inner naked Brep loop edges."), aspect: Rasm.Analysis.Curves.NakedInner),
            CurvesOutput(port: Port.List<Curve>(name: "Interior Edges", code: "IE", info: "Brep interior edges and mesh topology edges with exactly two connected faces."), aspect: Rasm.Analysis.Curves.Interior),
            CurvesOutput(port: Port.List<Curve>(name: "Non-Manifold Edges", code: "NE", info: "Brep non-manifold edges and mesh topology edges with more than two connected faces."), aspect: Rasm.Analysis.Curves.NonManifold),
            CurvesOutput(port: Port.List<Curve>(name: "Outer Loops", code: "OL", info: "Brep outer loop curves."), aspect: Rasm.Analysis.Curves.OuterLoop),
            CurvesOutput(port: Port.List<Curve>(name: "Inner Loops", code: "IL", info: "Brep inner loop curves."), aspect: Rasm.Analysis.Curves.InnerLoop),
            CurvesOutput(port: Port.List<Curve>(name: "U Iso Curves", code: "U", info: "Trim-aware mid-domain U-direction iso curves for Brep faces and surface values."), aspect: Rasm.Analysis.Curves.IsoU),
            CurvesOutput(port: Port.List<Curve>(name: "V Iso Curves", code: "V", info: "Trim-aware mid-domain V-direction iso curves for Brep faces and surface values."), aspect: Rasm.Analysis.Curves.IsoV),
            Output.Prepared<ShapeState, CurveProjection>(
                source: static (access, state) => Curves(state: state, aspect: Rasm.Analysis.Curves.Silhouette(direction: state.Hints.Value(access: access, port: Direction).Map(static value => (Vector3d?)value).IfNone(static () => null))),
                emptyUnsupported: true,
                slots: [CurvesSlot(port: Port.List<Curve>(name: "Silhouette Curves", code: "SC", info: "Parallel-projection silhouette curves using Direction, defaulting to world Z."))]),
            Output.Prepared<ShapeState, CurveProjection>(
                source: static (access, state) => Curves(state: state, aspect: Rasm.Analysis.Curves.Draft(
                    direction: state.Hints.Value(access: access, port: Direction).Map(static value => (Vector3d?)value).IfNone(static () => null),
                    angle: state.Hints.Value(access: access, port: DraftAngle).Map(static value => (double?)value.Radians).IfNone(static () => null))),
                emptyUnsupported: true,
                slots: [CurvesSlot(port: Port.List<Curve>(name: "Draft Curves", code: "DC", info: "Draft transition curves using Direction as pull direction and Draft Angle, defaulting to 0 radians."))]),
            Output.Prepared<ShapeState, CurveProjection>(
                source: static (access, state) => Indexed(access: access, state: state),
                CurvesSlot(port: Port.List<Curve>(name: "Indexed Curve", code: "IC", info: "Curve at Index input; missing Index defaults to 0, supplied values clamp to [0, count-1]. Empty when zero curves."))));

        public static Fin<Shape> Read(IDataAccess access) =>
            ShapeFoundation.Read(access: access);
        public static Fin<ShapeState> Prepare(IDataAccess access, Analyze.Scope scope, Hints hints, Shape input) =>
            ShapeFoundation.Prepare(access: access, scope: scope, hints: hints, input: input);

        private static IOutputGroup<ShapeState> CurvesOutput(Port<Curve> port, Rasm.Analysis.Curves aspect) =>
            Output.Prepared<ShapeState, CurveProjection>(
                source: (_, state) => Curves(state: state, aspect: aspect),
                emptyUnsupported: true,
                slots: [CurvesSlot(port: port)]);
        private static IOutputSlot<ShapeState, CurveProjection> CurvesSlot(Port<Curve> port) =>
            Output.Slot<ShapeState, CurveProjection, Curve>(
                port: port,
                project: static (_, curves) => Fin.Succ(curves.Map(static curve => OutputValue.Plain(value: curve.Curve))));
        private static Fin<Seq<CurveProjection>> Curves(ShapeState state, Rasm.Analysis.Curves aspect) =>
            state.Scope.Context.Bind(context => Query.CurveProjections<object>(geometry: state.Geometry, aspect: aspect).Run(env: context));
        private static Fin<Seq<CurveProjection>> Indexed(IDataAccess access, ShapeState state) =>
            Curves(state: state, aspect: Rasm.Analysis.Curves.All).Map(curves => curves.Count switch {
                0 => Seq<CurveProjection>(),
                int count => Seq(curves[Math.Clamp(value: state.Hints.Index(access: access, port: Index, limit: count).IfNone(static () => 0), min: 0, max: count - 1)]),
            });
    }

    public ExtractCurves() : base(nomen: ComponentNomen.Of<ExtractCurves>()) { }
    public ExtractCurves(IReader reader) : base(reader: reader) { }
}
