# [RASM_ANALYSIS_SELECT]

Selection unions decompose host sub-geometry for the measured-query runtime: `Curves`, `Faces`, and `Points` name which curves, faces, and points a geometry yields, each selected by feature, rank, or index and projected onto a typed output through the `Domain/normalization` `TopologyProjection` carrier under a leak-free transfer fold. Edge selection is data-driven — an internal `EdgeDescriptor` describes what an edge is and one `Features` projection derives the `CurveFeature` rows it carries, so selection tests membership through `Matches(descriptor)`, never a per-source branch.

Every projection duplicating host geometry travels as a `TopologyProjection` carrying its source `ComponentIndex`, so selection, repair, and host drains address one component space; the carrier's `Project` fold releases every non-transferred duplicate and the `Domain/rails` `Lease` with `DetachFrom` decides ownership, never a caller flag. Capability admission rides the `Domain/normalization` row vocabulary, evaluation composes `Domain/evaluation`, statistics `Domain/stats`, the spread eigendecomposition `Numerics/matrix`, and direction with planar decomposition the `Processing/intent` `VectorIntent` rail. Factory spellings bind the Grasshopper component surface by name, so a rename breaks the host contract.

## [01]-[INDEX]

- [02]-[CURVES]: `Curves` `[Union]` selection over the data-driven `CurveFeature`/`EdgeDescriptor` taxonomy and the `CurveProject` disposal fold.
- [03]-[FACES]: `Faces` `[Union]` decomposition fanned across typed projections on one builder, lease-aware through `DetachFrom`.
- [04]-[POINTS]: `Points` `[Union]` extraction and the `SpreadAspect` PCA spread family.

## [02]-[CURVES]

- Owner: `CurveFeature` `[SmartEnum<int>]` is the closed curve-provenance vocabulary — every extracted curve names what it was on the source, so downstream filtering reads a row rather than re-deriving adjacency. `EdgeDescriptor` internal `[Union]` describes an edge; its one `Features` projection derives the `CurveFeature` rows the edge carries and `IsSelectableEdge` names the selectable subset. `Curves` `[Union]` resolves the emitted feature per source stratum through `Feature(Topology)`, tests selection through the data-driven `Matches(EdgeDescriptor)`, and applies the shared index law through `Select`.
- Cases: the eight edge-feature spellings are one `EdgesCase` parameterized by `CurveFeature`, and silhouette and draft share one `SilhouetteCase` whose draft-angle presence selects the host call; a new selection spelling is a factory over an existing case, never a sibling case.
- Entry: `Curves.Operation<TGeometry, TOut>()` is the family seam `Analysis/query` forwards to; admission gates through `CanProject` (universal ingress, else `Kind.Of` topology dispatch against the case's source lattice), and the output type discriminates the projection one `CurveProject` builder fans.
- Auto: one fold owns extraction, selection, projection, and disposal — `CurveProject` resolves the source kind, derives the emitted feature, extracts every candidate, applies `Select`, projects the chosen subset, and releases every non-transferred projection through `TopologyProjection.Project`, so a leaked duplicate is impossible on the success and failure branches alike; the per-source extraction lattice and the trim-aware iso kernel live in the fence.
- Packages: RhinoCommon supplies brep, mesh, and SubD topology, iso extraction, and silhouette capture; `Rasm.Domain` supplies the capability vocabulary, form recoveries, the `TopologyProjection` carrier and its `Project` fold, and the `Lease`; `Rasm.Processing` supplies `VectorIntent`; Thinktecture.Runtime.Extensions and LanguageExt.Core the union and rail substrate.
- Growth: a new edge feature is one `CurveFeature` row with one `Features` arm; a new extraction source is one lattice arm emitting `TopologyProjection`s; a new typed output is one projection row on the fan; a new silhouette flavor is a flag or policy value on the existing case — selection, projection, and disposal untouched.
- Boundary: the edge taxonomy is data — `EdgeDescriptor.Features` is the one place adjacency becomes provenance, and a per-source feature `if` ladder is the wrong move it forecloses; every duplicate rides `TopologyProjection` with its true `ComponentIndex` so host drains and repair pages address one component space; owned lowering (`Surface`/`SubD` to brep) disposes through the `Lease` window on every branch; `Select` rejects an out-of-range index through the one `IndexSelection.At` fold both the curve and face families dispatch, so a family-local re-spelling of the empty/first/out-of-range arms is the wrong move; the silhouette arm is host capture beside the `Drawing/view` robust owner, so a local hidden-line kernel here is the altitude violation.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;
using Rasm.Domain;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CurveFeature {
    public static readonly CurveFeature Input = new(key: 0);
    public static readonly CurveFeature Segment = new(key: 1);
    public static readonly CurveFeature Edge = new(key: 2);
    public static readonly CurveFeature Boundary = new(key: 3);
    public static readonly CurveFeature NakedOuter = new(key: 4);
    public static readonly CurveFeature NakedInner = new(key: 5);
    public static readonly CurveFeature Interior = new(key: 6);
    public static readonly CurveFeature NonManifold = new(key: 7);
    public static readonly CurveFeature OuterLoop = new(key: 8);
    public static readonly CurveFeature InnerLoop = new(key: 9);
    public static readonly CurveFeature Iso = new(key: 10);
    public static readonly CurveFeature Silhouette = new(key: 11);
    public static readonly CurveFeature SubCurve = new(key: 12);
    public static readonly CurveFeature Draft = new(key: 13);
}

[Union]
internal abstract partial record EdgeDescriptor {
    private EdgeDescriptor() { }
    public sealed record OfBrep(EdgeAdjacency Valence, Seq<BrepLoopType> Loops) : EdgeDescriptor;
    public sealed record OfMesh(int ConnectedFaces) : EdgeDescriptor;
    public sealed record OfLoop(BrepLoopType LoopType) : EdgeDescriptor;
    internal bool IsSelectableEdge => this is OfBrep or OfMesh;
    internal Seq<CurveFeature> Features => this switch {
        OfBrep { Valence: EdgeAdjacency.Naked, Loops: Seq<BrepLoopType> loops } =>
            Seq(CurveFeature.Boundary) + loops.Choose(static loop => loop == BrepLoopType.Outer ? Some(CurveFeature.NakedOuter) : loop == BrepLoopType.Inner ? Some(CurveFeature.NakedInner) : Option<CurveFeature>.None),
        OfBrep { Valence: EdgeAdjacency.Interior } => Seq(CurveFeature.Interior),
        OfBrep { Valence: EdgeAdjacency.NonManifold } => Seq(CurveFeature.NonManifold),
        OfMesh { ConnectedFaces: 1 } => Seq(CurveFeature.Boundary),
        OfMesh { ConnectedFaces: 2 } => Seq(CurveFeature.Interior),
        OfMesh { ConnectedFaces: > 2 } => Seq(CurveFeature.NonManifold),
        OfLoop { LoopType: BrepLoopType.Outer } => Seq(CurveFeature.OuterLoop),
        OfLoop { LoopType: BrepLoopType.Inner } => Seq(CurveFeature.InnerLoop),
        _ => Seq<CurveFeature>(),
    };
}

[Union]
public abstract partial record Curves {
    private Curves() { }
    public sealed record EdgesCase(Option<CurveFeature> Kind) : Curves;
    public sealed record SegmentsCase(bool Smooth) : Curves;
    public sealed record IsoCase(IsoStatus Direction, double Normalized) : Curves;
    public sealed record SilhouetteCase(Vector3d? Direction, Option<double> DraftAngle) : Curves;
    public sealed record AtCase(int? Value) : Curves;
    public sealed record FormCase(int? Index = null) : Curves;
    internal static readonly Op Key = Op.Of(name: nameof(Curves));
    public static Curves All => new EdgesCase(Kind: Option<CurveFeature>.None);
    public static Curves Boundary => new EdgesCase(Kind: Some(CurveFeature.Boundary));
    public static Curves NakedOuter => new EdgesCase(Kind: Some(CurveFeature.NakedOuter));
    public static Curves NakedInner => new EdgesCase(Kind: Some(CurveFeature.NakedInner));
    public static Curves Interior => new EdgesCase(Kind: Some(CurveFeature.Interior));
    public static Curves NonManifold => new EdgesCase(Kind: Some(CurveFeature.NonManifold));
    public static Curves OuterLoop => new EdgesCase(Kind: Some(CurveFeature.OuterLoop));
    public static Curves InnerLoop => new EdgesCase(Kind: Some(CurveFeature.InnerLoop));
    public static Curves Segments(bool smooth = false) => new SegmentsCase(Smooth: smooth);
    public static Curves Iso(IsoStatus direction, double normalized = 0.5) => new IsoCase(Direction: direction, Normalized: normalized);
    public static Curves Silhouette(Vector3d? direction = null) => new SilhouetteCase(Direction: direction, DraftAngle: Option<double>.None);
    public static Curves Draft(Vector3d? direction = null, double? angle = null) => new SilhouetteCase(Direction: direction, DraftAngle: Some(Optional(angle).IfNone(0.0)));
    public static Curves At(int? index = null) => new AtCase(Value: index);
    public static Curves Form(int? index = null) => new FormCase(Index: index);

    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull =>
        CanProject(type: typeof(TGeometry)) switch {
            false => Key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Curve) => Analyze.CurveProject<TGeometry, TOut, Curve>(key: Key, aspect: this, project: static (p, _, _, _) => Fin.Succ(p.As<Curve>())),
                Type t when t == typeof(TopologyProjection) => Analyze.CurveProject<TGeometry, TOut, TopologyProjection>(key: Key, aspect: this, project: static (p, _, _, _) => Fin.Succ(Some(p))),
                Type t when t == typeof(CurveFeature) => Analyze.CurveProject<TGeometry, TOut, CurveFeature>(key: Key, aspect: this, project: static (_, feature, _, _) => Fin.Succ(Some(feature))),
                Type t when t == typeof(ComponentIndex) => Analyze.CurveProject<TGeometry, TOut, ComponentIndex>(key: Key, aspect: this, project: static (p, _, _, _) => Fin.Succ(Some(p.Source))),
                Type t when t == typeof(CurveForm) && this is FormCase => Analyze.CurveProject<TGeometry, TOut, CurveForm>(key: Key, aspect: this, project: static (p, _, context, op) => Analyze.ClassifyCurveForm(projection: p, context: context, op: op)),
                _ => Key.Unsupported<TGeometry, TOut>(),
            },
        };

    internal bool CanProject(Type type) =>
        Capability.Universal(type: type)
        || Kind.Of(type: type).Map(kind => CanProject(topology: kind.Topology, type: type)).IfNone(noneValue: false);
    private bool CanProject(Topology topology, Type type) => Switch(
        state: (Topology: topology, Type: type),
        edgesCase: static (state, e) => e.Kind.Case switch {
            null => Capability.CurveForm.Admits(type: state.Type) || Capability.BrepForm.Admits(type: state.Type) || Capability.Native(state.Type, state.Topology, (Topology.Mesh, typeof(Mesh)), (Topology.SubD, typeof(SubD))),
            CurveFeature feature when feature.Equals(CurveFeature.Boundary) => Capability.CurveForm.Admits(type: state.Type) || Capability.BrepForm.Admits(type: state.Type) || Capability.Native(state.Type, state.Topology, (Topology.Mesh, typeof(Mesh))),
            CurveFeature feature when FeatureIsAny(feature, CurveFeature.NakedOuter, CurveFeature.NakedInner, CurveFeature.OuterLoop, CurveFeature.InnerLoop) => Capability.Native(state.Type, state.Topology, (Topology.Brep, typeof(Brep))),
            CurveFeature feature when FeatureIsAny(feature, CurveFeature.Interior, CurveFeature.NonManifold) => Capability.Native(state.Type, state.Topology, (Topology.Brep, typeof(Brep)), (Topology.Mesh, typeof(Mesh))),
            _ => false,
        },
        segmentsCase: static (state, _) => Capability.CurveForm.Admits(type: state.Type) || Capability.Native(state.Type, state.Topology, (Topology.SubD, typeof(SubD))),
        isoCase: static (state, _) => Capability.Native(state.Type, state.Topology, (Topology.Brep, typeof(Brep))) || Capability.SurfaceForm.Admits(type: state.Type),
        silhouetteCase: static (state, _) =>
            Capability.SurfaceForm.Admits(type: state.Type) || typeof(Extrusion).IsAssignableFrom(c: state.Type)
            || Capability.Native(state.Type, state.Topology, (Topology.Brep, typeof(Brep)), (Topology.Mesh, typeof(Mesh)), (Topology.SubD, typeof(SubD)), (Topology.Extrusion, typeof(Extrusion))),
        atCase: static (state, _) =>
            Capability.CurveForm.Admits(type: state.Type) || Capability.SurfaceForm.Admits(type: state.Type) || Capability.Native(state.Type, state.Topology, (Topology.Brep, typeof(Brep)), (Topology.Mesh, typeof(Mesh)), (Topology.SubD, typeof(SubD))),
        formCase: static (state, _) =>
            Capability.CurveForm.Admits(type: state.Type) || Capability.Native(state.Type, state.Topology, (Topology.Brep, typeof(Brep)), (Topology.Mesh, typeof(Mesh)), (Topology.SubD, typeof(SubD))));

    internal Fin<Seq<TopologyProjection>> Select(Seq<TopologyProjection> curves) =>
        this switch {
            AtCase at => curves.At(index: Optional(at.Value), key: Key),
            FormCase { Index: int index } => curves.At(index: Some(index), key: Key),
            _ => Fin.Succ(curves),
        };
    internal CurveFeature Feature(Topology topology) => Switch(
        state: topology,
        edgesCase: static (t, e) => e.Kind.IfNone(EdgeFeatureFor(topology: t)),
        segmentsCase: static (_, s) => s.Smooth ? CurveFeature.SubCurve : CurveFeature.Segment,
        isoCase: static (_, _) => CurveFeature.Iso,
        silhouetteCase: static (_, s) => s.DraftAngle.IsSome ? CurveFeature.Draft : CurveFeature.Silhouette,
        atCase: static (t, _) => EdgeFeatureFor(topology: t),
        formCase: static (t, _) => EdgeFeatureFor(topology: t));
    internal bool Matches(EdgeDescriptor descriptor) =>
        this switch {
            EdgesCase { Kind.IsNone: true } or AtCase or FormCase => descriptor.IsSelectableEdge,
            EdgesCase { Kind.Case: CurveFeature feature } => descriptor.Features.Exists(candidate => candidate.Equals(feature)),
            _ => false,
        };
    internal static bool HasEdgeFeature(Curves aspect, bool allowNone, params ReadOnlySpan<CurveFeature> features) =>
        aspect is EdgesCase edges && ((allowNone && edges.Kind.IsNone) || FeatureIsAny(edges.Kind, features));
    private static CurveFeature EdgeFeatureFor(Topology topology) =>
        topology == Topology.Curve ? CurveFeature.Input : topology == Topology.Surface ? CurveFeature.Boundary : CurveFeature.Edge;
    private static bool FeatureIsAny(Option<CurveFeature> kind, params ReadOnlySpan<CurveFeature> features) =>
        kind.Case is CurveFeature feature && FeatureIsAny(feature, features);
    private static bool FeatureIsAny(CurveFeature feature, params ReadOnlySpan<CurveFeature> features) =>
        features.Contains(feature);
}

// One index-reject law both the curve and face selection families dispatch, spelled once here.
internal static class IndexSelection {
    extension(Seq<TopologyProjection> items) {
        internal Fin<Seq<TopologyProjection>> At(Option<int> index, Op key) =>
            (items.Count, index.Case) switch {
                (0, _) => Fin.Succ(Seq<TopologyProjection>()),
                (int count, int at) when at < 0 || at >= count => Fin.Fail<Seq<TopologyProjection>>(key.InvalidInput()),
                (_, int at) => Fin.Succ(Seq(items[at])),
                _ => Fin.Succ(Seq(items[0])),
            };
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class Analyze {
    internal static Operation<TGeometry, TOut> CurveProject<TGeometry, TOut, TValue>(Op key, Curves aspect, Func<TopologyProjection, CurveFeature, Context, Op, Fin<Option<TValue>>> project) where TGeometry : notnull =>
        Operation<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Aspect: aspect, Project: project), requiresContext: true,
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from kind in geometry.KindOf(context: runtime.Context).ToEff()
                let feature = state.Aspect.Feature(topology: kind.Topology)
                from curves in CurveProjections(geometry: geometry, aspect: state.Aspect, context: runtime.Context, op: state.Key, cancel: runtime.Cancellation).ToEff()
                from chosen in state.Aspect.Select(curves: curves).ToEff()
                from result in TopologyProjection.Project(all: curves, chosen: chosen, project: values => values.TraverseM(projection => state.Project(arg1: projection, arg2: feature, arg3: runtime.Context, arg4: state.Key)).As().Bind(projected => state.Key.Accept(values: projected.Choose(static value => value)))).ToEff()
                select result).As<TGeometry, TOut>(key: key);

    internal static Fin<Seq<TopologyProjection>> CurveProjections<TGeometry>(TGeometry geometry, Curves aspect, Context context, Op op, CancellationToken cancel) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => (g, aspect) switch {
            (Curve or Line or Polyline or Circle or Arc or Ellipse, Curves candidate) when Curves.HasEdgeFeature(candidate, allowNone: true, CurveFeature.Boundary) || candidate is Curves.AtCase or Curves.SegmentsCase or Curves.FormCase =>
                CurveInput(source: g, aspect: aspect, op: op),
            (Brep brep, Curves candidate) when Curves.HasEdgeFeature(candidate, allowNone: true, CurveFeature.Boundary, CurveFeature.NakedOuter, CurveFeature.NakedInner, CurveFeature.Interior, CurveFeature.NonManifold) || candidate is Curves.AtCase or Curves.FormCase =>
                SelectTopologyFeatures(source: brep.Edges, selector: aspect,
                    describe: static edge => new EdgeDescriptor.OfBrep(Valence: edge.Valence, Loops: toSeq(edge.TrimIndices()).Choose(t => Optional(edge.Brep.Trims[t].Loop).Map(static loop => loop.LoopType))),
                    project: edge => Optional(edge.DuplicateCurve()).Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex)))),
            (Brep brep, Curves candidate) when Curves.HasEdgeFeature(candidate, allowNone: false, CurveFeature.OuterLoop, CurveFeature.InnerLoop) =>
                SelectTopologyFeatures(source: brep.Loops, selector: aspect,
                    describe: static loop => new EdgeDescriptor.OfLoop(LoopType: loop.LoopType),
                    project: loop => Optional(loop.To3dCurve()).Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.BrepLoop, loop.LoopIndex)))),
            (Brep brep, Curves.IsoCase iso) =>
                toSeq(brep.Faces).TraverseM(face => IsoSeq(surface: face, iso: iso.Direction, normalized: iso.Normalized, op: op)
                    .Map(curves => curves.Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.BrepFace, face.FaceIndex))))).As()
                    .Map(static nested => nested.Bind(static seq => seq)),
            (BrepFace face, Curves candidate) when Curves.HasEdgeFeature(candidate, allowNone: true, CurveFeature.Boundary) || candidate is Curves.AtCase or Curves.FormCase =>
                FaceBoundaryEdgesOf(face: face, selector: aspect),
            (Mesh mesh, Curves candidate) when Curves.HasEdgeFeature(candidate, allowNone: true, CurveFeature.Boundary, CurveFeature.Interior, CurveFeature.NonManifold) || candidate is Curves.AtCase or Curves.FormCase =>
                SelectTopologyFeatures(source: Enumerable.Range(start: 0, count: mesh.TopologyEdges.Count), selector: aspect,
                    describe: i => new EdgeDescriptor.OfMesh(ConnectedFaces: mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: i).Length),
                    project: i => Some(TopologyProjection.Of(curve: mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: i).ToNurbsCurve(), source: new ComponentIndex(ComponentIndexType.MeshTopologyEdge, i)))),
            (Surface surface, Curves.IsoCase iso) =>
                IsoSeq(surface: surface, iso: iso.Direction, normalized: iso.Normalized, op: op)
                    .Map(curves => curves.Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.NoType, 0)))),
            (object surfaceLike, Curves.IsoCase iso) when Capability.SurfaceForm.Admits(type: surfaceLike.GetType()) =>
                Normalization.SurfaceForm(source: surfaceLike, key: op).Bind(lease => lease.Use(surface =>
                    IsoSeq(surface: surface, iso: iso.Direction, normalized: iso.Normalized, op: op)
                        .Map(curves => curves.Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.NoType, 0)))))),
            (object brepLike, Curves candidate) when (Curves.HasEdgeFeature(candidate, allowNone: true, CurveFeature.Boundary) || candidate is Curves.AtCase or Curves.FormCase) && Capability.BrepForm.Admits(type: brepLike.GetType()) =>
                Normalization.BrepForm(source: brepLike, key: op).Bind(lease => lease.Use(brep => SelectTopologyFeatures(source: brep.Edges, selector: aspect,
                    describe: static edge => new EdgeDescriptor.OfBrep(Valence: edge.Valence, Loops: toSeq(edge.TrimIndices()).Choose(t => Optional(edge.Brep.Trims[t].Loop).Map(static loop => loop.LoopType))),
                    project: edge => Optional(edge.DuplicateCurve()).Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex)))))),
            (SubD subd, Curves.EdgesCase { Kind.Case: null } or Curves.AtCase or Curves.SegmentsCase or Curves.FormCase) => SubDEdges(subd: subd),
            (GeometryBase native, Curves.SilhouetteCase silhouette) => SilhouettesOf(geometry: native, silhouette: silhouette, context: context, op: op, cancel: cancel),
            _ => Fin.Fail<Seq<TopologyProjection>>(op.Unsupported(g.GetType(), typeof(Curve))),
        });

    internal static Fin<Seq<Curve>> IsoSeq(Surface surface, IsoStatus iso, double normalized, Op op) => (iso, normalized is >= 0.0 and <= 1.0) switch {
        (IsoStatus.West, _) when surface is BrepFace face => Fin.Succ(toSeq(face.TrimAwareIsoCurve(1, face.Domain(0).T0))),
        (IsoStatus.East, _) when surface is BrepFace face => Fin.Succ(toSeq(face.TrimAwareIsoCurve(1, face.Domain(0).T1))),
        (IsoStatus.South, _) when surface is BrepFace face => Fin.Succ(toSeq(face.TrimAwareIsoCurve(0, face.Domain(1).T0))),
        (IsoStatus.North, _) when surface is BrepFace face => Fin.Succ(toSeq(face.TrimAwareIsoCurve(0, face.Domain(1).T1))),
        (IsoStatus.West or IsoStatus.South or IsoStatus.East or IsoStatus.North, _) => Optional(surface.IsoCurve(iso)).ToFin(op.InvalidResult()).Map(static curve => Seq(curve)),
        (IsoStatus.X or IsoStatus.Y, true) when surface.Domain(iso == IsoStatus.X ? 0 : 1) is { IsValid: true } domain =>
            surface is BrepFace face
                ? Fin.Succ(toSeq(face.TrimAwareIsoCurve(iso == IsoStatus.X ? 1 : 0, domain.ParameterAt(normalized))))
                : Optional(surface.IsoCurve(iso, domain.ParameterAt(normalized))).ToFin(op.InvalidResult()).Map(static curve => Seq(curve)),
        _ => Fin.Fail<Seq<Curve>>(op.InvalidInput()),
    };
    internal static Fin<Option<CurveForm>> ClassifyCurveForm(TopologyProjection projection, Context context, Op op) =>
        projection.As<Curve>(key: op).Bind(curve => Normalization.CurveFormOf(curve: curve, context: context).Map(static form => Some(form)));

    private static Fin<Seq<TopologyProjection>> CurveInput(object source, Curves aspect, Op op) =>
        Normalization.CurveForm(source: source, key: op).Bind(lease => lease.Use(native => aspect switch {
            Curves candidate when Curves.HasEdgeFeature(candidate, allowNone: true, CurveFeature.Boundary) && native.TryGetPolyline(polyline: out Polyline polyline) && polyline.SegmentCount > 0 =>
                Fin.Succ(toSeq(polyline.GetSegments().Select((segment, i) => TopologyProjection.Of(curve: new LineCurve(segment), source: new ComponentIndex(ComponentIndexType.PolycurveSegment, i))))),
            Curves.SegmentsCase segments => Optional(segments.Smooth ? native.GetSubCurves() : native.DuplicateSegments()) switch {
                Option<Curve[]> pieces when pieces.Case is Curve[] found && found.Length > 0 =>
                    Fin.Succ(toSeq(found.Select((piece, i) => TopologyProjection.Of(curve: piece, source: new ComponentIndex(ComponentIndexType.PolycurveSegment, i))))),
                _ => Optional(native.DuplicateCurve()).ToFin(op.InvalidResult()).Map(whole => Seq(TopologyProjection.Of(curve: whole, source: new ComponentIndex(ComponentIndexType.PolycurveSegment, 0)))),
            },
            _ => Optional(native.DuplicateCurve()).ToFin(op.InvalidResult()).Map(whole => Seq(TopologyProjection.Of(curve: whole, source: new ComponentIndex(ComponentIndexType.NoType, 0)))),
        }));
    private static Fin<Seq<TopologyProjection>> SelectTopologyFeatures<TPrimitive>(IEnumerable<TPrimitive> source, Curves selector, Func<TPrimitive, EdgeDescriptor> describe, Func<TPrimitive, Option<TopologyProjection>> project) =>
        Fin.Succ(toSeq(source).Choose(item => selector.Matches(descriptor: describe(arg: item)) ? project(arg: item) : Option<TopologyProjection>.None));
    private static Fin<Seq<TopologyProjection>> FaceBoundaryEdgesOf(BrepFace face, Curves selector) =>
        Fin.Succ(toSeq(face.Loops).Bind(loop => toSeq(loop.Trims).Choose(trim => (selector, trim.Edge) switch {
            (Curves candidate, BrepEdge edge) when Curves.HasEdgeFeature(candidate, allowNone: true, CurveFeature.Boundary) || candidate is Curves.AtCase or Curves.FormCase =>
                Optional(edge.DuplicateCurve()).Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex))),
            _ => Option<TopologyProjection>.None,
        })));
    private static Fin<Seq<TopologyProjection>> SubDEdges(SubD subd) {
        _ = subd.UpdateSurfaceMeshCache(lazyUpdate: true);
        return Fin.Succ(toSeq(subd.DuplicateEdgeCurves().Select((curve, i) => TopologyProjection.Of(curve: curve, source: new ComponentIndex(type: ComponentIndexType.SubdEdge, index: i)))));
    }
    private static Fin<Seq<TopologyProjection>> SilhouettesOf(GeometryBase geometry, Curves.SilhouetteCase silhouette, Context context, Op op, CancellationToken cancel) =>
        cancel.IsCancellationRequested
            ? Fin.Fail<Seq<TopologyProjection>>(new Fault.Cancelled())
            : VectorIntent.Direction(value: Optional(silhouette.Direction).IfNone(Vector3d.ZAxis)).Project<Vector3d>(context: context, key: op)
                .Bind(direction => (geometry switch {
                    Brep or BrepFace or Mesh or Extrusion => Fin.Succ<Lease<GeometryBase>>(new Lease<GeometryBase>.Borrowed(Value: geometry)),
                    Surface surface => Optional(surface.ToBrep()).ToFin(op.InvalidResult()).Map(static brep => (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: brep)),
                    SubD subd => Optional(subd.ToBrep(SubDToBrepOptions.Default)).ToFin(op.InvalidResult()).Map(static brep => (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: brep)),
                    _ => Fin.Fail<Lease<GeometryBase>>(op.Unsupported(geometry.GetType(), typeof(Curve))),
                }).Bind(lease => lease.Use(shape =>
                    Optional(silhouette.DraftAngle.Case switch {
                        double angle => Rhino.Geometry.Silhouette.ComputeDraftCurve(shape, angle, direction, context.Absolute.Value, context.Angle.Value, cancel),
                        _ => Rhino.Geometry.Silhouette.Compute(shape, SilhouetteType.Projecting | SilhouetteType.TangentProjects | SilhouetteType.Tangent | SilhouetteType.Crease | SilhouetteType.Boundary, direction, context.Absolute.Value, context.Angle.Value, [], cancel),
                    }).ToFin(cancel.IsCancellationRequested ? new Fault.Cancelled() : op.InvalidResult())
                    .Map(found => toSeq(found).Map(sil => TopologyProjection.Of(curve: sil.Curve, source: sil.GeometryComponentIndex))))));
}
```

## [03]-[FACES]

- Owner: `Faces` `[Union]` decomposes a geometry's faces by all, axis-rank, or index — top and bottom are one `RankedCase` whose `Domain/stats` `ExtremumDirection` sign selects the extremum, never two operations. One `FaceOperation` builder fans the union across the typed projection rows, each row binding its own `Requirement` — `SurfaceEvaluation` where it evaluates the face surface, `None` where it reads structure.
- Cases: three selection cases — `AllCase`, `RankedCase` (axis and direction), `AtCase` — fanned across the typed projection rows one builder owns; the eight outputs are projections of one operation.
- Entry: `Faces.Operation<TGeometry, TOut>()` is the family seam; admission gates through `Capability.DecomposeFaces.Admits` (universal ingress, `BrepFace` directly, any brep-coercible kind), and the output type selects the projection row at build time.
- Auto: `DecomposeFaces` derives ownership from the `Lease` case — a borrowed brep yields carriers addressing the live `BrepFace` list, an owned brep (coerced) yields carriers detached through `TopologyProjection.DetachFrom` before the lease disposes at scope exit, so ownership never rides a caller flag; ranking admits the axis through `VectorIntent.Direction`, scores each mass-centroid against it, and selects through the one `Stat.Extrema` fold at model tolerance, so every coplanar-tie face returns; `FrameAtFaceCentroid` composes `Analysis/measure`'s `CentroidOf` and `Domain/evaluation`'s `FrameAt`, and the `Interval` row reads `Analysis/inspect`'s `DomainsOf`.
- Packages: RhinoCommon supplies brep face access and the closest-point pull-back; `Rasm.Domain` supplies the decompose capability, form coercion, the carrier with `DetachFrom` and `Project`, the frame evaluation, and the extremum fold; `Rasm.Processing` supplies `VectorIntent`; Thinktecture.Runtime.Extensions and LanguageExt.Core the union and rail substrate.
- Growth: a new face projection is one output arm on the fan; a new selection strategy is one case whose score projection feeds the same `Stat.Extrema` fold — zero new operations.
- Boundary: eight outputs ride one builder — a `FacePlanes`/`FaceCentroids`/`FaceNormals` operation family is the proliferation this fan forecloses; the borrowed/owned asymmetry is the resource law, borrowed carriers transferring live faces and owned decompositions detaching so no emitted face dangles after the coerced brep disposes; ranking and index reject an out-of-range index through the same `IndexSelection.At` fold the curve family dispatches; the centroid frame composes `Analysis/measure` and `Domain/evaluation`, so a local mass or frame computation here is the wrong move.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Linq;
using LanguageExt;
using Rasm.Domain;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [TYPES] --------------------------------------------------------------------------------
[Union]
public abstract partial record Faces {
    private Faces() { }
    public sealed record AllCase : Faces;
    public sealed record RankedCase(Vector3d Axis, ExtremumDirection Direction) : Faces;
    public sealed record AtCase(int? Value) : Faces;
    internal static readonly Op Key = Op.Of(name: nameof(Faces));
    public static Faces All => new AllCase();
    public static Faces Top(Vector3d? axis = null) => new RankedCase(Axis: axis ?? Vector3d.ZAxis, Direction: ExtremumDirection.Maximum);
    public static Faces Bottom(Vector3d? axis = null) => new RankedCase(Axis: axis ?? Vector3d.ZAxis, Direction: ExtremumDirection.Minimum);
    public static Faces At(int? index = null) => new AtCase(Value: index);
    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull =>
        Capability.DecomposeFaces.Admits(type: typeof(TGeometry)) switch {
            false => Key.Unsupported<TGeometry, TOut>(),
            true => typeof(TOut) switch {
                Type t when t == typeof(Brep) => Analyze.FaceOperation<TGeometry, TOut, Brep>(key: Key, selector: this, requirement: Requirement.None,
                    project: static (chosen, _) => Key.Accept(values: chosen.Choose(static face => face.As<Brep>()))),
                Type t when t == typeof(TopologyProjection) => Analyze.FaceOperation<TGeometry, TOut, TopologyProjection>(key: Key, selector: this, requirement: Requirement.None,
                    project: static (chosen, _) => Key.Accept(values: chosen)),
                Type t when t == typeof(Plane) => Analyze.FaceOperation<TGeometry, TOut, Plane>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation,
                    project: static (chosen, runtime) => chosen.TraverseM(face => face.As<BrepFace>(key: Key).Bind(native => Analyze.FrameAtFaceCentroid(face: native, context: runtime, op: Key))).As()),
                Type t when t == typeof(Point3d) => Analyze.FaceOperation<TGeometry, TOut, Point3d>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation,
                    project: static (chosen, runtime) => chosen.TraverseM(face => face.As<BrepFace>(key: Key).Bind(native => Analyze.CentroidOf(geometry: native, context: runtime, op: Key))).As()),
                Type t when t == typeof(Vector3d) => Analyze.FaceOperation<TGeometry, TOut, Vector3d>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation,
                    project: static (chosen, runtime) => chosen.TraverseM(face => face.As<BrepFace>(key: Key)
                        .Bind(native => Analyze.FrameAtFaceCentroid(face: native, context: runtime, op: Key))
                        .Bind(frame => VectorIntent.Direction(value: frame.ZAxis).Project<Vector3d>(context: runtime, key: Key))).As()),
                Type t when t == typeof(ComponentIndex) => Analyze.FaceOperation<TGeometry, TOut, ComponentIndex>(key: Key, selector: this, requirement: Requirement.None,
                    project: static (chosen, _) => Key.Accept(values: chosen.Map(static face => face.Source))),
                Type t when t == typeof(int) => Analyze.FaceOperation<TGeometry, TOut, int>(key: Key, selector: this, requirement: Requirement.None,
                    project: static (chosen, _) => Key.Accept(values: chosen.Map(static face => face.Source.Index))),
                Type t when t == typeof(Interval) => Analyze.FaceOperation<TGeometry, TOut, Interval>(key: Key, selector: this, requirement: Requirement.SurfaceEvaluation,
                    project: static (chosen, _) => chosen.TraverseM(face => face.As<BrepFace>(key: Key).Bind(native => Analyze.DomainsOf(geometry: native, op: Key))).As().Map(static nested => nested.Bind(static domains => domains))),
                _ => Key.Unsupported<TGeometry, TOut>(),
            },
        };
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class Analyze {
    internal static Operation<TGeometry, TOut> FaceOperation<TGeometry, TOut, TValue>(Op key, Faces selector, Requirement requirement, Func<Seq<TopologyProjection>, Context, Fin<Seq<TValue>>> project) where TGeometry : notnull =>
        Operation<TGeometry, TValue>.Build(
            key: key, state: (Key: key, Selector: selector, Project: project), requirement: requirement, requiresContext: true,
            evaluator: static (state, geometry) =>
                from context in Env.Asks
                from faces in DecomposeFaces(key: state.Key, geometry: geometry).ToEff()
                from chosen in SelectFaces(key: state.Key, faces: faces, selector: state.Selector, runtime: context).ToEff()
                from result in TopologyProjection.Project(all: faces, chosen: chosen, project: values => state.Project(arg1: values, arg2: context)).ToEff()
                select result).As<TGeometry, TOut>(key: key);
    internal static Fin<Seq<TopologyProjection>> DecomposeFaces<TGeometry>(Op key, TGeometry geometry) where TGeometry : notnull =>
        Optional(geometry).ToFin(key.InvalidInput()).Bind(g => g switch {
            BrepFace face => Fin.Succ(Seq(TopologyProjection.Of(face: face))),
            object brepLike when Capability.BrepForm.Admits(type: brepLike.GetType()) => Normalization.BrepForm(source: brepLike, key: key).Bind(lease => lease.Switch(
                borrowed: static borrowed => Fin.Succ(toSeq(borrowed.Value.Faces.Select(static face => TopologyProjection.Of(face: face)).ToArray())),
                owned: static owned => owned.Project(static brep => Fin.Succ(toSeq(brep.Faces.Select(face => TopologyProjection.Of(face: face).DetachFrom(source: brep)).ToArray()))))),
            _ => Fin.Fail<Seq<TopologyProjection>>(key.Unsupported(g.GetType(), typeof(Seq<TopologyProjection>))),
        });
    internal static Fin<Seq<TopologyProjection>> SelectFaces(Op key, Seq<TopologyProjection> faces, Faces selector, Context runtime) => selector.Switch(
        state: (Key: key, Faces: faces, Runtime: runtime),
        allCase: static (s, _) => Fin.Succ(s.Faces),
        rankedCase: static (s, ranked) => RankFaces(state: s, axis: ranked.Axis, direction: ranked.Direction),
        atCase: static (s, at) => s.Faces.At(index: Optional(at.Value), key: s.Key));
    internal static Fin<Plane> FrameAtFaceCentroid(BrepFace face, Context context, Op op) =>
        CentroidOf(geometry: face, context: context, op: op).Bind(centroid =>
            face.ClosestPointOnFace(testPoint: centroid, u: out double u, v: out double v, maximumDistance: 0.0)
                ? Evaluation.FrameAt(surface: face, uv: new Point2d(x: u, y: v), key: op)
                : Fin.Fail<Plane>(op.InvalidResult()));
    private static Fin<Seq<TopologyProjection>> RankFaces((Op Key, Seq<TopologyProjection> Faces, Context Runtime) state, Vector3d axis, ExtremumDirection direction) =>
        state.Faces.IsEmpty switch {
            true => Fin.Succ(Seq<TopologyProjection>()),
            false => from vector in VectorIntent.Direction(value: axis).Project<Vector3d>(context: state.Runtime, key: state.Key)
                     from ranked in state.Faces.TraverseM(face => face.As<BrepFace>(key: state.Key)
                         .Bind(native => CentroidOf(geometry: native, context: state.Runtime, op: state.Key))
                         .Map(point => (Face: face, Score: new Vector3d(x: point.X, y: point.Y, z: point.Z) * vector))).As()
                     select Stat.Extrema(items: ranked, projection: static item => item.Score, tolerance: state.Runtime.Absolute.Value, direction: direction).Map(static item => item.Face),
        };
}
```

## [04]-[POINTS]

- Owner: `SpreadAspect` `[SmartEnum<int>]` asks what a point set's spread is, each row binding its own typed `Output` (`Plane`, `Stat`, or `bool`). `Points` `[Union]` extracts directional extrema, edge midpoints, vertices, control points, or spread — one case per extraction kind.
- Cases: five extraction cases; `ExtremaCase` admits caller directions or derives the world quadrant set, and `SpreadCase` carries its `SpreadAspect`; a new aspect is a `SpreadAspect` row, a new extraction a case.
- Entry: `Points.Operation<TGeometry, TOut>()` is the family seam; every arm gates capability through the `Domain/normalization` vocabulary and the output type before building.
- Auto: extrema resolves directions through `VectorIntent.Axes` (planar curves collapse to the in-plane pair, absent directions derive the quadrant set) then folds `Curve.ExtremeParameters` through the one `Stat.Extrema` fold; edge midpoints composes the `Curves` rail so the edge walk lives once in the curve family; vertices routes `Domain/evaluation`'s `VerticesOf`; control points unfolds NURBS nets, lowering non-NURBS sources through owned leases; spread reads vertices and either folds centroid distances into `Stat.Of` or fits a plane and derives frame, principal frame, coplanarity, or collinearity — the principal angle is the PCA of the fit-plane coordinates, every point decomposing through `VectorIntent.Components`, the rows folding through `Domain/stats`'s `SampleMoment` covariance into a `Numerics/matrix` `SymmetricMatrix`, and the dominant eigenpair (selected by `Stat.Extrema` over eigenvalues, independent of decomposition return order) giving the axis.
- Packages: RhinoCommon supplies curve extrema, NURBS control nets, and plane fitting; `Rasm.Domain` supplies the capability and kind columns, the vertex lattice, statistics, and the lease; `Rasm.Processing` supplies `VectorIntent`; `Rasm.Numerics` supplies `SymmetricMatrix`; Thinktecture.Runtime.Extensions and LanguageExt.Core the union and rail substrate.
- Growth: a new spread aspect is one `SpreadAspect` row with one `SpreadProject` arm over the same moment fold; a new extraction source is one lattice arm; a new extremum policy is a parameter on the existing fold.
- Boundary: spread mathematics is composed — `SampleMoment` owns the covariance, `SymmetricMatrix` owns the spectrum, `Stat.Extrema` owns the dominant-pair selection; a local covariance accumulation or eigen-ordering assumption is the double-owner defect, and selecting the dominant eigenvalue keeps the result order-independent where a first-returned-pair convention couples correctness to an upstream sort; planar-coordinate projection failures abort the fold, since a zero-row substitution biases the covariance toward the origin; `EdgeMidpoints` composes the `Curves` rail, so a second topology-edge walker is the wrong move; control-point extraction leases every minted NURBS form so conversion never leaks.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Rasm.Csp;
using LanguageExt;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Processing;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
// CS0104 guard: Rhino.Geometry declares Matrix/Dimension homonyms under the dual usings.
using Dimension = Rasm.Numerics.Dimension;

namespace Rasm.Analysis;

// --- [TYPES] --------------------------------------------------------------------------------
[BoundaryAdapter, SmartEnum<int>]
public sealed partial class SpreadAspect {
    public static readonly SpreadAspect Frame = new(key: 0, output: typeof(Plane));
    public static readonly SpreadAspect PrincipalFrame = new(key: 1, output: typeof(Plane));
    public static readonly SpreadAspect Distribution = new(key: 2, output: typeof(Stat));
    public static readonly SpreadAspect Collinear = new(key: 3, output: typeof(bool));
    public static readonly SpreadAspect Coplanar = new(key: 4, output: typeof(bool));
    public Type Output { get; }
}

[Union]
public abstract partial record Points {
    private Points() { }
    public sealed record ExtremaCase(Option<Seq<Vector3d>> Directions) : Points;
    public sealed record EdgeMidpointsCase : Points;
    public sealed record VerticesCase : Points;
    public sealed record ControlPointsCase : Points;
    public sealed record SpreadCase(SpreadAspect Aspect) : Points;
    private static readonly Op ExtremaKey = Op.Of(name: nameof(Extrema));
    private static readonly Op EdgeMidpointsKey = Op.Of(name: nameof(EdgeMidpoints));
    private static readonly Op VerticesKey = Op.Of(name: nameof(Vertices));
    private static readonly Op ControlPointsKey = Op.Of(name: nameof(ControlPoints));
    private static readonly Op SpreadKey = Op.Of(name: nameof(Spread));
    public static Points Quadrants => new ExtremaCase(Directions: Option<Seq<Vector3d>>.None);
    public static Points Extrema(Seq<Vector3d> directions) => new ExtremaCase(Directions: Some(value: directions));
    public static Points EdgeMidpoints => new EdgeMidpointsCase();
    public static Points Vertices => new VerticesCase();
    public static Points ControlPoints => new ControlPointsCase();
    public static Points Spread(SpreadAspect aspect) => new SpreadCase(Aspect: aspect);

    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        extremaCase: static c => typeof(TOut) == typeof(Point3d) && Capability.CurveForm.Admits(type: typeof(TGeometry))
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: ExtremaKey, requirement: Requirement.Basic, requiresContext: true, state: (Key: ExtremaKey, c.Directions),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from lease in Normalization.CurveForm(source: geometry, key: state.Key).ToEff()
                    from points in lease.Use((Curve curve) => curve.IsValid switch {
                        false => Fin.Fail<Seq<Point3d>>(state.Key.InvalidInput()),
                        true => DirectionsFor(custom: state.Directions, planar: curve.IsPlanar(tolerance: context.Absolute.Value), context: context, key: state.Key)
                            .Bind((Seq<Vector3d> directions) => directions.TraverseM((Vector3d direction) => Stat.Extrema(
                                    items: toSeq(curve.ExtremeParameters(direction: direction) ?? []).Map(curve.PointAt),
                                    projection: point => (Vector3d)point * direction,
                                    tolerance: 0.0,
                                    direction: ExtremumDirection.Maximum)
                                .Head.ToFin(state.Key.InvalidResult()))
                            .As()),
                    }).ToEff()
                    select points).As<TGeometry, TOut>(key: ExtremaKey)
            : ExtremaKey.Unsupported<TGeometry, TOut>(),
        edgeMidpointsCase: static _ => typeof(TOut) == typeof(Point3d) && (Capability.Universal(type: typeof(TGeometry)) || Kind.Of(type: typeof(TGeometry)).Map(static kind => kind.CanReadEdges).IfNone(noneValue: false))
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: EdgeMidpointsKey, requiresContext: true, state: EdgeMidpointsKey,
                evaluator: static (op, geometry) => Analyze.CurveProject<TGeometry, Point3d, Point3d>(
                    key: op,
                    aspect: Curves.All,
                    project: static (projection, _, _, _) => Fin.Succ(projection.As<Curve>().Map(static curve => curve.PointAtNormalizedLength(length: 0.5))))
                    .Apply(geometry: Seq(geometry))).As<TGeometry, TOut>(key: EdgeMidpointsKey)
            : EdgeMidpointsKey.Unsupported<TGeometry, TOut>(),
        verticesCase: static _ => typeof(TOut) == typeof(Point3d) && Capability.ReadVertices.Admits(type: typeof(TGeometry))
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: VerticesKey, state: VerticesKey,
                evaluator: static (op, geometry) =>
                    from points in geometry.VerticesOf(key: op).ToEff()
                    from result in op.Accept(values: points).ToEff()
                    select result).As<TGeometry, TOut>(key: VerticesKey)
            : VerticesKey.Unsupported<TGeometry, TOut>(),
        controlPointsCase: static _ => typeof(TOut) == typeof(Point3d) && (Capability.Universal(type: typeof(TGeometry)) || Kind.Of(type: typeof(TGeometry)).Map(static kind => kind.CanReadControlPoints).IfNone(noneValue: false))
            ? Analysis.Operation<TGeometry, Point3d>.Build(
                key: ControlPointsKey, state: ControlPointsKey,
                evaluator: static (op, geometry) =>
                    from points in Analyze.ControlPointsOf(geometry: geometry, op: op).ToEff()
                    from result in op.Accept(values: points).ToEff()
                    select result).As<TGeometry, TOut>(key: ControlPointsKey)
            : ControlPointsKey.Unsupported<TGeometry, TOut>(),
        spreadCase: static s => s.Aspect.Output == typeof(TOut) && Capability.ReadVertices.Admits(type: typeof(TGeometry))
            ? Analysis.Operation<TGeometry, TOut>.Build(
                key: SpreadKey, requiresContext: true, state: (Key: SpreadKey, s.Aspect),
                evaluator: static (state, geometry) =>
                    from context in Env.Asks
                    from points in geometry.VerticesOf(key: state.Key).ToEff()
                    from result in Analyze.SpreadProject<TOut>(aspect: state.Aspect, points: points, geometry: geometry, context: context, op: state.Key).ToEff()
                    select result)
            : SpreadKey.Unsupported<TGeometry, TOut>());
    private static Fin<Seq<Vector3d>> DirectionsFor(Option<Seq<Vector3d>> custom, bool planar, Context context, Op key) =>
        VectorIntent.Axes(values: custom, planar: planar).Project<Seq<Vector3d>>(context: context, key: key);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class Analyze {
    internal static Fin<Seq<Point3d>> ControlPointsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            NurbsCurve nurbs => Fin.Succ(toSeq(Enumerable.Range(0, nurbs.Points.Count).Select(i => nurbs.Points[i].Location).ToArray())),
            Curve curve => Optional(curve.ToNurbsCurve()).ToFin(op.InvalidResult())
                .Map(static minted => new Lease<NurbsCurve>.Owned(Value: minted).Use(static owned => toSeq(Enumerable.Range(0, owned.Points.Count).Select(i => owned.Points[i].Location).ToArray()))),
            NurbsSurface nurbs => Fin.Succ(toSeq(Enumerable.Range(0, nurbs.Points.CountU).SelectMany(u => Enumerable.Range(0, nurbs.Points.CountV).Select(v => nurbs.Points.GetControlPoint(u, v).Location)).ToArray())),
            Surface surface => Optional(surface.ToNurbsSurface()).ToFin(op.InvalidResult())
                .Map(static minted => new Lease<NurbsSurface>.Owned(Value: minted).Use(static owned => toSeq(Enumerable.Range(0, owned.Points.CountU).SelectMany(u => Enumerable.Range(0, owned.Points.CountV).Select(v => owned.Points.GetControlPoint(u, v).Location)).ToArray()))),
            Brep brep => toSeq(brep.Faces).TraverseM(face => Optional(face.ToNurbsSurface()).ToFin(op.InvalidResult())
                .Map(static minted => new Lease<NurbsSurface>.Owned(Value: minted).Use(static owned => toSeq(Enumerable.Range(0, owned.Points.CountU).SelectMany(u => Enumerable.Range(0, owned.Points.CountV).Select(v => owned.Points.GetControlPoint(u, v).Location)).ToArray())))).As()
                .Map(static nested => nested.Bind(static points => points)),
            object surfaceLike when Capability.SurfaceForm.Admits(type: surfaceLike.GetType()) =>
                Normalization.SurfaceForm(source: surfaceLike, key: op).Bind(lease => lease.Use(surface => ControlPointsOf(geometry: surface, op: op))),
            _ => Fin.Fail<Seq<Point3d>>(op.Unsupported(g.GetType(), typeof(Point3d))),
        });
    internal static Fin<Seq<TOut>> SpreadProject<TOut>(SpreadAspect aspect, Seq<Point3d> points, object geometry, Context context, Op op) =>
        aspect.Equals(SpreadAspect.Distribution)
            ? CentroidOf(geometry: geometry, context: context, op: op)
                .Bind(centroid => Stat.Of(values: points.Map(point => point.DistanceTo(other: centroid)), key: op))
                .Bind(stat => op.AcceptResults<Stat, TOut>(values: Seq(stat)))
            : (Plane.FitPlaneToPoints(points: points.AsIterable(), plane: out Plane fit, maximumDeviation: out double deviation), fit.IsValid) switch {
                (PlaneFitResult.Success, true) => aspect switch {
                    SpreadAspect row when row.Equals(SpreadAspect.Frame) => op.AcceptResults<Plane, TOut>(values: Seq(fit)),
                    SpreadAspect row when row.Equals(SpreadAspect.PrincipalFrame) => OrientedFrame(fit: fit, points: points, context: context, op: op).Bind(plane => op.AcceptResults<Plane, TOut>(values: Seq(plane))),
                    SpreadAspect row when row.Equals(SpreadAspect.Coplanar) => op.AcceptResults<bool, TOut>(values: Seq(deviation <= context.Absolute.Value)),
                    SpreadAspect row when row.Equals(SpreadAspect.Collinear) => MinorSpread(fit: fit, points: points, context: context, op: op).Bind(spread => op.AcceptResults<bool, TOut>(values: Seq(spread <= context.Absolute.Value))),
                    _ => Fin.Fail<Seq<TOut>>(op.Unsupported(geometryType: typeof(SpreadAspect), outputType: typeof(TOut))),
                },
                _ when (aspect.Equals(SpreadAspect.Coplanar) || aspect.Equals(SpreadAspect.Collinear)) && points.Count <= 2 => op.AcceptResults<bool, TOut>(values: Seq(value: true)),
                _ => Fin.Fail<Seq<TOut>>(op.InvalidResult()),
            };
    // Dominant eigenpair selected by eigenvalue: correct under any decomposition return order.
    private static Fin<double> PrincipalAngle(Seq<Point3d> points, Plane fit, Context context, Op op) =>
        points.TraverseM(point => VectorIntent.Components(anchor: fit.Origin, value: point - fit.Origin, frame: fit).Project<(double X, double Y)>(context: context, key: op)).As()
            .Map(static planar => planar.Map(static row => new Arr<double>([row.X, row.Y])))
            .Bind(rows => SampleMoment.Of(rows: rows, dimension: 2, key: op))
            .Bind(moment => SymmetricMatrix.Of(dim: Dimension.Create(value: moment.Dimension), upper: moment.UpperCovariance, key: op)
                .Bind(covariance => covariance.DecomposeEigen(key: op)))
            .Bind(pairs => Stat.Extrema(items: pairs, projection: static pair => pair.Eigenvalue, tolerance: 0.0, direction: ExtremumDirection.Maximum).Head.ToFin(op.InvalidResult()))
            .Map(static dominant => Math.Atan2(y: dominant.Eigenvector[1], x: dominant.Eigenvector[0]));
    private static Fin<Plane> OrientedFrame(Plane fit, Seq<Point3d> points, Context context, Op op) =>
        from angle in PrincipalAngle(points: points, fit: fit, context: context, op: op)
        from xAxis in VectorIntent.Direction(value: (fit.XAxis * Math.Cos(d: angle)) + (fit.YAxis * Math.Sin(a: angle))).Project<Vector3d>(context: context, key: op)
        from yAxis in VectorIntent.Direction(value: Vector3d.CrossProduct(a: fit.ZAxis, b: xAxis)).Project<Vector3d>(context: context, key: op)
        from plane in op.AcceptValue(value: new Plane(origin: fit.Origin, xDirection: xAxis, yDirection: yAxis))
        select plane;
    private static Fin<double> MinorSpread(Plane fit, Seq<Point3d> points, Context context, Op op) =>
        from angle in PrincipalAngle(points: points, fit: fit, context: context, op: op)
        from offsets in points.TraverseM(point => VectorIntent.Components(anchor: fit.Origin, value: point - fit.Origin, frame: fit)
            .Project<(double X, double Y)>(context: context, key: op)
            .Map(components => Math.Abs(value: (components.X * -Math.Sin(a: angle)) + (components.Y * Math.Cos(d: angle))))).As()
        from spread in Stat.Extrema(items: offsets, projection: static offset => offset, tolerance: 0.0, direction: ExtremumDirection.Maximum).Head.ToFin(op.InvalidResult())
        select spread;
}
```

```mermaid
flowchart LR
    Curves -->|EdgeDescriptor.Features data taxonomy| Extraction[edges · loops · segments · iso · silhouette]
    Extraction -->|TopologyProjection + ComponentIndex| Carrier[provenance carriers]
    Carrier -->|Select → Project disposal fold| Outputs[Curve · TopologyProjection · CurveFeature · ComponentIndex · CurveForm]
    Faces -->|DecomposeFaces: Borrowed live / Owned DetachFrom| Carrier
    Faces -->|CentroidOf × Stat.Extrema| Ranked[axis-ranked selection]
    Points -->|VerticesOf · ControlPointsOf · Curves rail midpoints| Point3d
    Points -->|SampleMoment → SymmetricMatrix → dominant eigenpair| Spread[Frame · PrincipalFrame · Stat · bool]
    Curves & Faces & Points -->|Operation builders| Query[Analysis/query dispatch]
```

## [05]-[DENSITY_BAR]

One owner per axis; a new feature, projection, or aspect is a row, a case, or a fan arm — never a sibling surface.

| [INDEX] | [CONCERN]         | [OWNER]          | [KIND]                                         | [RAIL]                            | [CASES] |
| :-----: | :---------------- | :--------------- | :--------------------------------------------- | :-------------------------------- | :-----: |
|  [01]   | Curve provenance  | `CurveFeature`   | `[SmartEnum<int>]` closed feature vocabulary   | row (pure)                        |   14    |
|  [02]   | Edge taxonomy     | `EdgeDescriptor` | internal `[Union]` + the `Features` derivation | `Matches → bool` (data-driven)    |    3    |
|  [03]   | Curve selection   | `Curves`         | `[Union]` selection over the feature taxonomy  | `Operation → Eff<Env, Seq<TOut>>` |    6    |
|  [04]   | Face selection    | `Faces`          | `[Union]` fanned across projection rows        | `Operation → Eff<Env, Seq<TOut>>` |    3    |
|  [05]   | Point extraction  | `Points`         | `[Union]` extraction family                    | `Operation → Eff<Env, Seq<TOut>>` |    5    |
|  [06]   | Spread vocabulary | `SpreadAspect`   | `[SmartEnum<int>]` + typed `Output` column     | `SpreadProject → Fin<Seq<TOut>>`  |    5    |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
