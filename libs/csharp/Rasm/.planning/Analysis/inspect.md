# [RASM_ANALYSIS_INSPECT]

Inspection owns the measured-query runtime's topology-scalar and mesh-quality diagnostics: `Topologies` `[Union]` closes structural interrogation over both Brep and Mesh, and `Meshes` `[Union]` closes mesh census, per-polygon metrics, and boundary extraction. Brep/mesh polymorphism collapses into one `OnGeometry` gate lowering brep-coercible inputs through the leased brep form, so every scalar, orientation, containment, and component fold is written once and a per-operation geometry switch is the deleted repetition.

A rebuild composes law legislated elsewhere: ring-metric mathematics is `Spatial/cloud` law projected through `Processing/intent` `VectorIntent`, the streaming `Stat` fold is `Domain/stats` law, and the `Kind` web, the `BrepForm`/`SurfaceForm` leases, and the `TopologyProjection` carrier are `Domain/normalization` law. Every receipt declares `IValidityEvidence`, admitted through the one `Domain/validation` acceptance oracle `Analysis/query` owns, and each family union exposes `internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>()` as that dispatch seam.

## [01]-[INDEX]

- [02]-[TOPOLOGY]: `Topologies` structural interrogation and the `TopologyScalar` rows over the one `OnGeometry` mesh/brep gate — Euler, genus, boundary, and component folds, solid orientation, point containment, and kind classification.
- [03]-[MESH]: `Meshes` mesh inspection — the banded `MeshSampleGroup` census, `MeshMetric` visible-polygon measurement over the `Spatial/cloud` ring, and the `MeshSample`/`MeshMetricSample`/`MeshFaceShape` evidence receipts.

## [02]-[TOPOLOGY]

- Owner: `Topologies` `[Union]` closes structural interrogation over any admitted geometry — classification, interval domains, solid orientation, connected components, point containment, and the scalar family; `TopologyScalar` `[BoundaryAdapter]` `[SmartEnum<int>]` binds each scalar to a typed `Output` and an `Extract` delegate folding through the one `OnGeometry` gate, the count rows sharing one `ElementCountOf` projection over mesh or brep.
- Cases: the structural-interrogation cases with `Scalar` carrying a `TopologyScalar` row; the flat scalar factories collapse onto `ScalarCase`, the fence declaring both closed sets.
- Entry: `Topologies.Operation<TGeometry, TOut>()` is the family seam every arm gates at build — `Capability.EvaluateTopology` admits scalar, orientation, and containment; curve-or-surface form admits domains; output type is checked per arm. Context is demanded only where read: classification and containment declare it, the scalar, domain, orientation, and component rows run scope-less, and an operation demanding context it never reads is the deleted over-requirement.
- Auto: `OnGeometry` is the one mesh/brep gate — `Mesh` and `Brep` dispatch directly, brep-coercible natives lower through the leased brep form, everything else rejects — so brep-like admission is written once and every scalar, orientation, containment, and component fold routes through it; the derived scalars fold the primitive Euler, boundary-loop, and component counts through one applicative `Apply` gated on oriented-manifold admission, and `ComponentCountOf` disposes the pieces it counts.
- Receipt: no dedicated receipt rail — scalars project onto scalar, interval, kind, orientation, and geometry values through the acceptance oracle; `Components` re-emits owned `GeometryBase` pieces.
- Packages: RhinoCommon (`Mesh`/`Brep` topology, loop, and orientation surface), `Rasm.Domain` (`Kind` capability web, `BrepForm`/`SurfaceForm` leases, `Requirement.SolidTopology`, `Op`/`Fault` rail), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new topology scalar is one `TopologyScalar` row — key, output, and extract delegate over the same `OnGeometry` fold; a new structural interrogation is one `Topologies` case; a new geometry family entering the gate is one `OnGeometry` arm serving every row at once.
- Boundary: brep/mesh polymorphism lives in one gate — a per-operation `is Mesh`/`is Brep` switch is the deleted repetition; the genus, hole, and Euler family is derived from three primitive rows and a stored genus beside the formula is the killed form; `Capability.EvaluateTopology` is the single topology-evaluation admission row, containment escalating through `Requirement.SolidTopology`; `SolidOrientationOf` maps the mesh orientation int onto `BrepSolidOrientation` so both families answer in one enum, never a mesh-specific parallel vocabulary; component extraction owns its disposal — a piece failing the typed projection is disposed before the fault leaves.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Globalization;
using Rasm.Csp;
using LanguageExt;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [TYPES] --------------------------------------------------------------------------------
[Union]
public abstract partial record Topologies {
    private Topologies() { }
    public sealed record KindCase : Topologies;
    public sealed record DomainsCase : Topologies;
    public sealed record SolidOrientationCase : Topologies;
    public sealed record ComponentsCase : Topologies;
    public sealed record ContainsPointCase(Point3d Point) : Topologies;
    public sealed record ScalarCase(TopologyScalar Scalar) : Topologies;
    public static Topologies Kind => new KindCase();
    public static Topologies Domains => new DomainsCase();
    public static Topologies SolidOrientation => new SolidOrientationCase();
    public static Topologies Components => new ComponentsCase();
    public static Topologies ContainsPoint(Point3d point) => new ContainsPointCase(Point: point);
    public static Topologies Manifold => new ScalarCase(Scalar: TopologyScalar.Manifold);
    public static Topologies Euler => new ScalarCase(Scalar: TopologyScalar.Euler);
    public static Topologies BoundaryLoops => new ScalarCase(Scalar: TopologyScalar.BoundaryLoops);
    public static Topologies Genus => new ScalarCase(Scalar: TopologyScalar.Genus);
    public static Topologies HoleCount => new ScalarCase(Scalar: TopologyScalar.HoleCount);
    public static Topologies FaceCount => new ScalarCase(Scalar: TopologyScalar.FaceCount);
    public static Topologies EdgeCount => new ScalarCase(Scalar: TopologyScalar.EdgeCount);
    public static Topologies VertexCount => new ScalarCase(Scalar: TopologyScalar.VertexCount);
    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        kindCase: static _ => Analyze.Kind<TGeometry, TOut>(),
        domainsCase: static _ => Analyze.TopologyDomains<TGeometry, TOut>(),
        solidOrientationCase: static _ => Analyze.TopologySolidOrientation<TGeometry, TOut>(),
        componentsCase: static _ => Analyze.TopologyComponents<TGeometry, TOut>(),
        containsPointCase: static cp => Analyze.TopologyContains<TGeometry, TOut>(point: cp.Point),
        scalarCase: static scalar => Analyze.TopologyScalar<TGeometry, TOut>(scalar: scalar.Scalar));
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class TopologyScalar {
    public static readonly TopologyScalar Manifold = new(key: 0, output: typeof(bool), extract: static (g, op) => Analyze.ManifoldOf(geometry: g, op: op).Map(static value => (object)value));
    public static readonly TopologyScalar Euler = new(key: 1, output: typeof(int), extract: static (g, op) => Analyze.EulerOf(geometry: g, op: op).Map(static value => (object)value));
    public static readonly TopologyScalar BoundaryLoops = new(key: 2, output: typeof(int), extract: static (g, op) => Analyze.BoundaryLoopsOf(geometry: g, op: op).Map(static value => (object)value));
    public static readonly TopologyScalar Genus = new(key: 3, output: typeof(int), extract: static (g, op) => Analyze.GenusOf(geometry: g, op: op).Map(static value => (object)value));
    public static readonly TopologyScalar HoleCount = new(key: 4, output: typeof(int), extract: static (g, op) => Analyze.HoleCountOf(geometry: g, op: op).Map(static value => (object)value));
    public static readonly TopologyScalar FaceCount = new(key: 5, output: typeof(int), extract: static (g, op) => Analyze.ElementCountOf(geometry: g, op: op, meshCount: static m => m.Faces.Count, brepCount: static b => b.Faces.Count).Map(static value => (object)value));
    public static readonly TopologyScalar EdgeCount = new(key: 6, output: typeof(int), extract: static (g, op) => Analyze.ElementCountOf(geometry: g, op: op, meshCount: static m => m.TopologyEdges.Count, brepCount: static b => b.Edges.Count).Map(static value => (object)value));
    public static readonly TopologyScalar VertexCount = new(key: 7, output: typeof(int), extract: static (g, op) => Analyze.ElementCountOf(geometry: g, op: op, meshCount: static m => m.Vertices.Count, brepCount: static b => b.Vertices.Count).Map(static value => (object)value));
    public Type Output { get; }
    [UseDelegateFromConstructor] internal partial Fin<object> Extract(GeometryBase geometry, Op op);
    internal Fin<int> IntegerOf(GeometryBase geometry, Op op) => Extract(geometry: geometry, op: op).Bind(value => value is int count ? Fin.Succ(count) : Fin.Fail<int>(op.InvalidResult()));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class Analyze {
    internal static Operation<TGeometry, TOut> Kind<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return (Capability.Universal(type: typeof(TGeometry)) || Rasm.Domain.Kind.Of(type: typeof(TGeometry)).IsSome)
            ? typeof(TOut) switch {
                Type t when t == typeof(Kind) => KernelLift<TGeometry, Kind, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k)), requiresContext: true).As<TGeometry, TOut>(key: key),
                Type t when t == typeof(string) => KernelLift<TGeometry, string, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k.ToString(format: null, formatProvider: CultureInfo.InvariantCulture))), requiresContext: true).As<TGeometry, TOut>(key: key),
                Type t when t == typeof(Topology) => KernelLift<TGeometry, Topology, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k.Topology)), requiresContext: true).As<TGeometry, TOut>(key: key),
                _ => key.Unsupported<TGeometry, TOut>(),
            }
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologyDomains<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(Interval) && (Capability.CurveForm.Admits(type: typeof(TGeometry)) || Capability.SurfaceForm.Admits(type: typeof(TGeometry)))
            ? KernelLift<TGeometry, Interval, Op>(key: key, state: key, extract: static (op, g, _) => DomainsOf(geometry: g, op: op).Bind(domains => op.Accept(values: domains))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologySolidOrientation<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == typeof(BrepSolidOrientation) && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
            ? KernelLift<TGeometry, BrepSolidOrientation, Op>(key: key, state: key, extract: static (op, g, _) => SolidOrientationOf(geometry: g, op: op).Bind(orientation => op.Accept(value: orientation))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologyComponents<TGeometry, TOut>() where TGeometry : notnull {
        Op key = Op.Of();
        return ((typeof(Brep).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(Brep))
            || ((typeof(Mesh).IsAssignableFrom(c: typeof(TGeometry)) || typeof(TGeometry) == typeof(object) || typeof(TGeometry) == typeof(GeometryBase)) && typeof(TOut) == typeof(Mesh))
            ? KernelLift<TGeometry, TOut, Op>(key: key, state: key, extract: static (op, g, _) => ComponentsOf(geometry: g, op: op).Bind(components => ProjectComponents<TOut>(components: components, op: op))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologyContains<TGeometry, TOut>(Point3d point) where TGeometry : notnull {
        Op key = Op.Of();
        return point.IsValid && typeof(TOut) == typeof(bool) && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
            ? KernelLift<TGeometry, bool, (Op Key, Point3d Target)>(key: key, state: (Key: key, Target: point), requirement: Requirement.SolidTopology,
                extract: static (s, g, ctx) => ContainsPoint(geometry: g, target: s.Target, context: ctx, op: s.Key).Bind(contained => s.Key.Accept(value: contained))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Operation<TGeometry, TOut> TopologyScalar<TGeometry, TOut>(TopologyScalar scalar) where TGeometry : notnull {
        Op key = Op.Of();
        return typeof(TOut) == scalar.Output && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
            ? KernelLift<TGeometry, TOut, (Op Key, TopologyScalar Scalar)>(key: key, state: (Key: key, Scalar: scalar),
                extract: static (s, g, _) => OnGeometry(geometry: g, op: s.Key, onMesh: mesh => s.Scalar.Extract(geometry: mesh, op: s.Key), onBrep: brep => s.Scalar.Extract(geometry: brep, op: s.Key))
                    .Bind(value => value is TOut typed ? s.Key.Accept(value: typed) : Fin.Fail<Seq<TOut>>(s.Key.Unsupported(geometryType: value.GetType(), outputType: typeof(TOut)))))
            : key.Unsupported<TGeometry, TOut>();
    }
    internal static Fin<Seq<Interval>> DomainsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Curve curve => Fin.Succ(Seq(curve.Domain)),
            Surface surface => Fin.Succ(Seq(surface.Domain(direction: 0), surface.Domain(direction: 1))),
            object surfaceLike when Capability.SurfaceForm.Admits(type: surfaceLike.GetType()) => Normalization.SurfaceForm(source: surfaceLike, key: op).Bind(lease => lease.Use(surface => DomainsOf(geometry: surface, op: op))),
            _ => Fin.Fail<Seq<Interval>>(op.Unsupported(g.GetType(), typeof(Interval))),
        });
    internal static Fin<BrepSolidOrientation> SolidOrientationOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: mesh => Fin.Succ(mesh.SolidOrientation() switch {
                1 => BrepSolidOrientation.Outward,
                -1 => BrepSolidOrientation.Inward,
                _ => BrepSolidOrientation.None,
            }),
            onBrep: brep => Fin.Succ(brep.SolidOrientation));
    internal static Fin<bool> ContainsPoint<TGeometry>(TGeometry geometry, Point3d target, Context context, Op op) where TGeometry : notnull =>
        from _ in guard(target.IsValid, op.InvalidInput())
        from contained in OnGeometry(geometry: geometry, op: op,
            onMesh: mesh => Fin.Succ(mesh.IsPointInside(point: target, tolerance: context.Absolute.Value, strictlyIn: false)),
            onBrep: brep => Fin.Succ(brep.IsPointInside(point: target, tolerance: context.Absolute.Value, strictlyIn: false)))
        select contained;
    internal static Fin<Seq<GeometryBase>> ComponentsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Mesh mesh => Fin.Succ(toSeq(mesh.SplitDisjointPieces().Cast<GeometryBase>())),
            Brep brep => BrepComponentsOf(brep: brep, op: op),
            GeometryBase { HasBrepForm: true } native => Normalization.BrepForm(source: native, key: op).Bind(lease => lease.Use(brep => BrepComponentsOf(brep: brep, op: op))),
            _ => Fin.Fail<Seq<GeometryBase>>(op.Unsupported(g.GetType(), typeof(Seq<GeometryBase>))),
        });
    internal static Fin<bool> ManifoldOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool _)),
            onBrep: static b => Fin.Succ(b.IsManifold));
    internal static Fin<int> EulerOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(m.TopologyVertices.Count - m.TopologyEdges.Count + m.Faces.Count),
            onBrep: b => guard(b.IsManifold, op.Unsupported(typeof(Brep), typeof(int))).ToFin().Map(_ => b.Vertices.Count - b.Edges.Count + b.Faces.Count));
    internal static Fin<int> BoundaryLoopsOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(Optional(m.GetNakedEdges()).Map(static loops => loops.Length).IfNone(0)),
            onBrep: static b => Fin.Succ(BrepBoundaryCount(brep: b, predicate: static loop => loop.LoopType is BrepLoopType.Outer or BrepLoopType.Inner)));
    internal static Fin<int> GenusOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: m => m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented
                ? (EulerOf(geometry: m, op: op), BoundaryLoopsOf(geometry: m, op: op), ComponentCountOf(geometry: m, op: op)).Apply(static (euler, boundaries, components) => ((2 * components) - euler - boundaries) / 2).As()
                : Fin.Fail<int>(op.Unsupported(typeof(Mesh), typeof(int))),
            onBrep: b => b.IsManifold
                ? (EulerOf(geometry: b, op: op), BoundaryLoopsOf(geometry: b, op: op), ComponentCountOf(geometry: b, op: op)).Apply(static (euler, boundaries, components) => ((2 * components) - euler - boundaries) / 2).As()
                : Fin.Fail<int>(op.Unsupported(typeof(Brep), typeof(int))));
    internal static Fin<int> HoleCountOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: m => (BoundaryLoopsOf(geometry: m, op: op), ComponentCountOf(geometry: m, op: op)).Apply(static (boundaries, components) => Math.Max(val1: 0, val2: boundaries - components)).As(),
            onBrep: b => (BoundaryLoopsOf(geometry: b, op: op), ComponentCountOf(geometry: b, op: op)).Apply(static (boundaries, components) => Math.Max(val1: 0, val2: boundaries - components)).As());
    internal static Fin<int> ElementCountOf<TG>(TG geometry, Op op, Func<Mesh, int> meshCount, Func<Brep, int> brepCount) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op, onMesh: m => Fin.Succ(meshCount(arg: m)), onBrep: b => Fin.Succ(brepCount(arg: b)));
    private static Operation<TGeometry, TValue> KernelLift<TGeometry, TValue, TState>(Op key, TState state, Func<TState, TGeometry, Context, Fin<Seq<TValue>>> extract, Requirement? requirement = null, bool requiresContext = false) where TGeometry : notnull =>
        Operation<TGeometry, TValue>.Build(
            key: key, requirement: requirement, requiresContext: requiresContext, state: (State: state, Extract: extract),
            evaluator: static (s, geometry) =>
                from context in Env.Asks
                from result in s.Extract(arg1: s.State, arg2: geometry, arg3: context).ToEff()
                select result);
    private static Fin<TResult> OnGeometry<TGeometry, TResult>(TGeometry geometry, Op op, Func<Mesh, Fin<TResult>> onMesh, Func<Brep, Fin<TResult>> onBrep) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Mesh mesh => onMesh(arg: mesh),
            Brep brep => onBrep(arg: brep),
            GeometryBase { HasBrepForm: true } native => Normalization.BrepForm(source: native, key: op).Bind(lease => lease.Use(project: onBrep)),
            object brepLike when Capability.BrepForm.Admits(type: brepLike.GetType()) => Normalization.BrepForm(source: brepLike, key: op).Bind(lease => lease.Use(project: onBrep)),
            _ => Fin.Fail<TResult>(op.Unsupported(g.GetType(), typeof(TResult))),
        });
    private static Fin<Seq<GeometryBase>> BrepComponentsOf(Brep brep, Op op) =>
        brep.GetConnectedComponents() switch {
            Brep[] components when components.Length > 0 => Fin.Succ(toSeq(components.Cast<GeometryBase>())),
            _ when brep.IsValid => op.AcceptValue(brep).Map(static valid => Seq((GeometryBase)valid.DuplicateBrep())),
            _ => Fin.Fail<Seq<GeometryBase>>(op.InvalidResult()),
        };
    private static Fin<Seq<TOut>> ProjectComponents<TOut>(Seq<GeometryBase> components, Op op) =>
        components.TraverseM(component => component is TOut typed ? Fin.Succ(typed) : Fin.Fail<TOut>(op.Unsupported(geometryType: component.GetType(), outputType: typeof(TOut)))).As()
            .BindFail(error => components.Iter(static component => component.Dispose()) switch { _ => Fin.Fail<Seq<TOut>>(error) });
    private static int BrepBoundaryCount(Brep brep, Func<BrepLoop, bool> predicate) =>
        toSeq(brep.Loops).Filter(loop => predicate(arg: loop) && toSeq(loop.Trims).Exists(static trim => trim.Edge is { Valence: EdgeAdjacency.Naked })).Count;
    private static Fin<int> ComponentCountOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        ComponentsOf(geometry: geometry, op: op)
            .Map(static components => components.Iter(static component => component.Dispose()) switch { _ => Math.Max(val1: 1, val2: components.Count) });
}
```

## [03]-[MESH]

- Owner: `MeshSampleGroup` `[BoundaryAdapter]` `[SmartEnum<int>]` bands the sample census and derives each band's row set from `MeshSampleKind.Items` by band membership — the membership is the query, never a hand-kept list — with one band marked `Inspect` to demand a `Mesh.Check` capture. `MeshSampleKind` `[SmartEnum<int>]` binds each row to its band and a `Sample(Mesh, MeshCheckParameters)` delegate across validity flags, census counts (the topology rows reusing the `TopologyScalar` extractors), defect counters reading the threaded capture, and valence-quality folds. `MeshMetric` `[BoundaryAdapter]` `[SmartEnum<int>]` binds each face metric to a measure delegate over one visible polygon. `Meshes` `[Union]` closes sample groups, face quality and shape, visible-polygon addressing and count, naked edges, and plane outlines.
- Cases: the mesh-inspection cases with `Samples` carrying a `MeshSampleGroup` and `FaceQuality` a `MeshMetric`; the fence declares the band rows and metric rows.
- Entry: `Meshes.Operation<TGeometry, TOut>()` lifts every arm through `MeshLift` — the mesh specialization applying a typed `Operation<Mesh, TValue>` to any geometry that is a mesh and rejecting the rest — so the family accepts `object`-typed pipelines and stays mesh-strict at evaluation.
- Auto: the sample census runs `Mesh.Check` once — `MeshCheck` captures `MeshCheckParameters` through `Requirement.MeshReport` and every band row reads the same capture. Visible-polygon resolution maps an ngon-or-face onto the canonical `ComponentIndex` (`MeshNgon` for ngon members, `MeshFace` otherwise) and extracts its boundary ring, so every per-polygon metric addresses one component vocabulary. Metric measurement short-circuits host fast paths where they exist and folds the `Spatial/cloud` ring metric everywhere else; ngon area sums constituent faces, ngon normals area-weight constituent normals, and the dihedral fold walks ngon-external adjacency for the maximum inter-polygon angle. Every per-polygon fold reads the runtime cancellation token between polygons, faulting `Fault.Cancelled` mid-census rather than finishing a stale sweep; `Requirement.MeshCheck` gates every metric operation so no defective mesh reaches a measurement.
- Receipt: `MeshSample`, `MeshMetricSample`, and `MeshFaceShape` carry the sample, the addressed measurement, and the addressed `Spatial/cloud` shape classification; each declares `IValidityEvidence` through the `Domain/rails` `ValidityClaim` fold.
- Packages: RhinoCommon (`Mesh.Check`/`MeshCheckParameters`, `MeshNgon` census, face and ngon accessors, outlines, `ComponentIndex`), `Rasm.Spatial` (`VectorCloud` ring metrics), `Rasm.Processing` (`VectorIntent`), `Rasm.Domain` (`Requirement.MeshCheck`/`MeshReport`, `Stat`, `TopologyProjection`, `Op`/`Fault` rail), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new mesh sample is one `MeshSampleKind` row in its band, the census and receipt machinery untouched; a new face metric is one `MeshMetric` row binding a measure delegate over the same polygon resolution; a new polygon-level extraction is one `Meshes` case lifted through `MeshLift`.
- Boundary: the census bands derive from row membership — a hand-maintained per-band list is the deleted drift form; defect rows read the one threaded `MeshCheckParameters` capture and a per-row `Mesh.Check` re-run is the killed N-fold host cost; face metrics measure visible polygons through the canonical `ComponentIndex` addressing, never a triangle-level parallel family; ring measurement routes through the `Spatial/cloud` metric surface exclusively, never a local perimeter/skewness/area loop; `AtVisiblePolygon` re-emits the `Domain/normalization` `TopologyProjection` carrier so downstream extraction shares the corpus transfer/disposal protocol.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Rasm.Csp;
using LanguageExt;
using Rasm.Domain;
using Rasm.Processing;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [TYPES] --------------------------------------------------------------------------------
[Union]
public abstract partial record Meshes {
    private Meshes() { }
    public sealed record SamplesCase(MeshSampleGroup Group) : Meshes;
    public sealed record FaceQualityCase(MeshMetric Metric) : Meshes;
    public sealed record FaceShapeCase : Meshes;
    public sealed record AtVisiblePolygonCase(Option<int> Value) : Meshes;
    public sealed record VisiblePolygonCountCase : Meshes;
    public sealed record NakedEdgesCase : Meshes;
    public sealed record OutlineCase(Plane Plane) : Meshes;
    private static readonly Op SamplesKey = Op.Of(name: "MeshSamples"), FaceQualityKey = Op.Of(name: "MeshFaceQuality"), FaceShapeKey = Op.Of(name: "MeshFaceShape");
    private static readonly Op AtVisiblePolygonKey = Op.Of(name: "MeshAtVisiblePolygon"), VisiblePolygonCountKey = Op.Of(name: "MeshVisiblePolygonCount"), NakedEdgesKey = Op.Of(name: "MeshNakedEdges"), OutlineKey = Op.Of(name: "MeshOutline");
    public static Meshes Validity => new SamplesCase(Group: MeshSampleGroup.Validity);
    public static Meshes Counts => new SamplesCase(Group: MeshSampleGroup.Count);
    public static Meshes Defects => new SamplesCase(Group: MeshSampleGroup.Defect);
    public static Meshes Quality => new SamplesCase(Group: MeshSampleGroup.Quality);
    public static Meshes FaceQuality(MeshMetric? metric = null) => new FaceQualityCase(Metric: metric ?? MeshMetric.EdgeAspect);
    public static Meshes FaceShape => new FaceShapeCase();
    public static Meshes AtVisiblePolygon(int? index = null) => new AtVisiblePolygonCase(Value: Optional(index));
    public static Meshes VisiblePolygonCount => new VisiblePolygonCountCase();
    public static Meshes NakedEdges => new NakedEdgesCase();
    public static Meshes Outline(Plane plane) => new OutlineCase(Plane: plane);
    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        samplesCase: static s => Analyze.MeshLift<TGeometry, TOut, MeshSample>(key: SamplesKey, source: Analyze.MeshSamples(group: s.Group)),
        faceQualityCase: static fq => fq.Metric.Equals(MeshMetric.None)
            ? Analysis.Operation<TGeometry, TOut>.Reject(key: FaceQualityKey, fault: FaceQualityKey.InvalidInput())
            : typeof(TOut) switch {
                Type output when output == typeof(MeshMetricSample) => Analyze.MeshLift<TGeometry, TOut, MeshMetricSample>(key: FaceQualityKey, source: Analyze.MeshMetricSamplesOp(metric: fq.Metric, key: FaceQualityKey)),
                Type output when output == typeof(Stat) => Analyze.MeshLift<TGeometry, TOut, Stat>(key: FaceQualityKey, source: Analyze.MeshMetricStatOp(metric: fq.Metric, key: FaceQualityKey)),
                _ => FaceQualityKey.Unsupported<TGeometry, TOut>(),
            },
        faceShapeCase: static _ => typeof(TOut) == typeof(MeshFaceShape)
            ? Analyze.MeshLift<TGeometry, TOut, MeshFaceShape>(key: FaceShapeKey, source: Analyze.MeshFaceShapesOp(key: FaceShapeKey))
            : FaceShapeKey.Unsupported<TGeometry, TOut>(),
        atVisiblePolygonCase: static at => Analyze.MeshLift<TGeometry, TOut, TopologyProjection>(key: AtVisiblePolygonKey, source: Analyze.MeshAtVisiblePolygon(index: at.Value)),
        visiblePolygonCountCase: static _ => Analyze.MeshLift<TGeometry, TOut, int>(key: VisiblePolygonCountKey, source: Analyze.MeshVisiblePolygonCount),
        nakedEdgesCase: static _ => Analyze.MeshLift<TGeometry, TOut, Polyline>(key: NakedEdgesKey, source: Analyze.MeshNakedEdges),
        outlineCase: static o => Analyze.MeshLift<TGeometry, TOut, Polyline>(key: OutlineKey, source: Analyze.MeshOutline(plane: o.Plane)));
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MeshSampleGroup {
    public static readonly MeshSampleGroup None = new(key: 0, label: nameof(None), inspect: false);
    public static readonly MeshSampleGroup Validity = new(key: 1, label: nameof(Validity), inspect: false);
    public static readonly MeshSampleGroup Count = new(key: 2, label: nameof(Count), inspect: false);
    public static readonly MeshSampleGroup Defect = new(key: 3, label: nameof(Defect), inspect: true);
    public static readonly MeshSampleGroup Quality = new(key: 4, label: nameof(Quality), inspect: false);
    public string Label { get; }
    internal bool Inspect { get; }
    internal Seq<MeshSampleKind> Kinds => toSeq(MeshSampleKind.Items).Filter(kind => kind.Group.Equals(this));
}

[SmartEnum<int>]
public sealed partial class MeshSampleKind {
    public static readonly MeshSampleKind None = new(key: 0, group: MeshSampleGroup.None, sample: static (_, _) => Fin.Succ(0));
    public static readonly MeshSampleKind Valid = new(key: 1, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ(m.IsValid ? 1 : 0));
    public static readonly MeshSampleKind Closed = new(key: 2, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ(m.IsClosed ? 1 : 0));
    public static readonly MeshSampleKind Oriented = new(key: 3, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ((m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented) ? 1 : 0));
    public static readonly MeshSampleKind Solid = new(key: 4, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ(m.IsSolid ? 1 : 0));
    public static readonly MeshSampleKind Manifold = new(key: 5, group: MeshSampleGroup.Validity, sample: static (m, _) => Analyze.ManifoldOf(geometry: m, op: ManifoldKey).Map(static value => value ? 1 : 0));
    public static readonly MeshSampleKind BoundaryFree = new(key: 6, group: MeshSampleGroup.Validity, sample: static (m, _) => Fin.Succ((m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool boundary) && !boundary) ? 1 : 0));
    public static readonly MeshSampleKind Vertices = new(key: 10, group: MeshSampleGroup.Count, sample: static (m, _) => TopologyScalar.VertexCount.IntegerOf(geometry: m, op: VertexCountKey));
    public static readonly MeshSampleKind Faces = new(key: 11, group: MeshSampleGroup.Count, sample: static (m, _) => TopologyScalar.FaceCount.IntegerOf(geometry: m, op: FaceCountKey));
    public static readonly MeshSampleKind Triangles = new(key: 12, group: MeshSampleGroup.Count, sample: static (m, _) => Fin.Succ(m.Faces.TriangleCount));
    public static readonly MeshSampleKind Quads = new(key: 13, group: MeshSampleGroup.Count, sample: static (m, _) => Fin.Succ(m.Faces.QuadCount));
    public static readonly MeshSampleKind Edges = new(key: 14, group: MeshSampleGroup.Count, sample: static (m, _) => TopologyScalar.EdgeCount.IntegerOf(geometry: m, op: EdgeCountKey));
    public static readonly MeshSampleKind Euler = new(key: 15, group: MeshSampleGroup.Count, sample: static (m, _) => Analyze.EulerOf(geometry: m, op: EulerKey));
    public static readonly MeshSampleKind VisiblePolygons = new(key: 16, group: MeshSampleGroup.Count, sample: static (m, _) => Fin.Succ(m.GetNgonAndFacesCount()));
    public static readonly MeshSampleKind DegenerateFaces = new(key: 20, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.DegenerateFaceCount));
    public static readonly MeshSampleKind DisjointMeshes = new(key: 21, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.DisjointMeshCount));
    public static readonly MeshSampleKind DuplicateFaces = new(key: 22, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.DuplicateFaceCount));
    public static readonly MeshSampleKind ExtremelyShortEdges = new(key: 23, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.ExtremelyShortEdgeCount));
    public static readonly MeshSampleKind InvalidNgons = new(key: 24, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.InvalidNgonCount));
    public static readonly MeshSampleKind NakedEdges = new(key: 25, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.NakedEdgeCount));
    public static readonly MeshSampleKind NonManifoldEdges = new(key: 26, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.NonManifoldEdgeCount));
    public static readonly MeshSampleKind NonUnitVectorNormals = new(key: 27, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.NonUnitVectorNormalCount));
    public static readonly MeshSampleKind RandomFaceNormals = new(key: 28, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.RandomFaceNormalCount));
    public static readonly MeshSampleKind SelfIntersectingPairs = new(key: 29, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.SelfIntersectingPairsCount));
    public static readonly MeshSampleKind UnusedVertices = new(key: 30, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.UnusedVertexCount));
    public static readonly MeshSampleKind VertexFaceNormalsDiffer = new(key: 31, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.VertexFaceNormalsDifferCount));
    public static readonly MeshSampleKind ZeroLengthNormals = new(key: 32, group: MeshSampleGroup.Defect, sample: static (_, p) => Fin.Succ(p.ZeroLengthNormalCount));
    public static readonly MeshSampleKind MaximumValence = new(key: 40, group: MeshSampleGroup.Quality, sample: static (m, _) => ValenceSummary(mesh: m, project: static stat => stat.Maximum));
    public static readonly MeshSampleKind MinimumValence = new(key: 41, group: MeshSampleGroup.Quality, sample: static (m, _) => ValenceSummary(mesh: m, project: static stat => stat.Minimum));
    public static readonly MeshSampleKind BoundaryLoopCount = new(key: 42, group: MeshSampleGroup.Quality, sample: static (m, _) => Analyze.BoundaryLoopsOf(geometry: m, op: BoundaryLoopsKey));
    public static readonly MeshSampleKind Genus = new(key: 43, group: MeshSampleGroup.Quality, sample: static (m, _) => Analyze.GenusOf(geometry: m, op: GenusKey));
    public static readonly MeshSampleKind AverageValence = new(key: 44, group: MeshSampleGroup.Quality, sample: static (m, _) => ValenceSummary(mesh: m, project: static stat => stat.Mean));
    private static readonly Op ManifoldKey = Op.Of(name: "MeshManifold"), VertexCountKey = Op.Of(name: "MeshVertexCount"), FaceCountKey = Op.Of(name: "MeshFaceCount"), EdgeCountKey = Op.Of(name: "MeshEdgeCount");
    private static readonly Op EulerKey = Op.Of(name: "MeshEuler"), BoundaryLoopsKey = Op.Of(name: "MeshBoundaryLoops"), GenusKey = Op.Of(name: "MeshGenus"), ValenceKey = Op.Of(name: "MeshValence");
    internal MeshSampleGroup Group { get; }
    [UseDelegateFromConstructor] internal partial Fin<int> Sample(Mesh mesh, MeshCheckParameters parameters);
    private static Seq<int> Valences(Mesh mesh) =>
        toSeq(Enumerable.Range(0, mesh.TopologyVertices.Count).Select(mesh.TopologyVertices.ConnectedEdgesCount));
    private static Fin<int> ValenceSummary(Mesh mesh, Func<Stat, double> project) =>
        Valences(mesh: mesh) switch {
            { IsEmpty: true } => Fin.Succ(0),
            Seq<int> valences => Stat.Of(values: valences.Map(static valence => (double)valence), key: ValenceKey)
                .Map(stat => (int)Math.Round(value: project(arg: stat), mode: MidpointRounding.ToEven)),
        };
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MeshMetric {
    public static readonly MeshMetric None = new(key: 0, measure: static (_, _, _, _, key) => Fin.Fail<double>(key.InvalidInput()));
    public static readonly MeshMetric EdgeAspect = new(key: 1, measure: FaceEdgeAspect);
    public static readonly MeshMetric Area = new(key: 2, measure: FaceArea);
    public static readonly MeshMetric Perimeter = new(key: 3, measure: static (mesh, source, vertices, context, key) => RingMetric<double>(metric: VectorCloudMetric.Perimeter, mesh: mesh, source: source, vertices: vertices, context: context, key: key));
    public static readonly MeshMetric Skewness = new(key: 4, measure: static (mesh, source, vertices, context, key) => RingMetric<double>(metric: VectorCloudMetric.Skewness, mesh: mesh, source: source, vertices: vertices, context: context, key: key));
    public static readonly MeshMetric DihedralAngle = new(key: 5, measure: FaceMaxDihedral);
    [UseDelegateFromConstructor] private partial Fin<double> Measure(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key);
    internal Fin<MeshMetricSample> Sample(Mesh? mesh, MeshNgon polygon, Context context, Op key) =>
        Optional(mesh).ToFin(key.InvalidInput())
            .Bind(native => Analyze.VisiblePolygonSourceOf(mesh: native, polygon: polygon, key: key)
                .Bind(source => VerticesOf(mesh: native, source: source, key: key).Map(vertices => (Mesh: native, Source: source, Vertices: vertices))))
            .Bind(state => Measure(mesh: state.Mesh, source: state.Source, vertices: state.Vertices, context: context, key: key).Map(value => (state.Source, Value: value)))
            .Bind(state => key.AcceptValue(value: new MeshMetricSample(Source: state.Source, Value: state.Value)));
    internal static Fin<MeshFaceShape> Shape(Mesh? mesh, MeshNgon polygon, Context context, Op key) =>
        Optional(mesh).ToFin(key.InvalidInput())
            .Bind(native => Analyze.VisiblePolygonSourceOf(mesh: native, polygon: polygon, key: key)
                .Bind(source => VerticesOf(mesh: native, source: source, key: key).Map(vertices => (Mesh: native, Source: source, Vertices: vertices))))
            .Bind(state => RingMetric<VectorCloudShape>(metric: VectorCloudMetric.Shape, mesh: state.Mesh, source: state.Source, vertices: state.Vertices, context: context, key: key)
                .Map(shape => new MeshFaceShape(Source: state.Source, Shape: shape)));
    private static Fin<Seq<Point3d>> VerticesOf(Mesh mesh, ComponentIndex source, Op key) => source switch {
        { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < mesh.Faces.Count =>
            mesh.Faces.GetFaceVertices(face, out Point3f a, out Point3f b, out Point3f c, out Point3f d) switch {
                true when mesh.Faces[face].IsQuad => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c, (Point3d)d)),
                true => Fin.Succ(Seq((Point3d)a, (Point3d)b, (Point3d)c)),
                false => Fin.Fail<Seq<Point3d>>(key.InvalidResult()),
            },
        { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
            Optional(mesh.Ngons.NgonBoundaryVertexList(ngon: mesh.Ngons[ngon], bAppendStartPoint: false)).ToFin(key.InvalidResult()).Map(static points => toSeq(points)),
        _ => Fin.Fail<Seq<Point3d>>(key.InvalidInput()),
    };
    private static Fin<Seq<int>> FaceIndicesOf(Mesh mesh, int ngon, Op key) =>
        Optional(mesh.Ngons[ngon].FaceIndexList()).ToFin(key.InvalidResult())
            .Bind(faces => toSeq(faces).TraverseM(face => face <= int.MaxValue && (int)face < mesh.Faces.Count ? Fin.Succ((int)face) : Fin.Fail<int>(key.InvalidResult())).As()
                .Bind(indices => indices.IsEmpty ? Fin.Fail<Seq<int>>(key.InvalidResult()) : Fin.Succ(indices)));
    private static Fin<TOut> RingMetric<TOut>(VectorCloudMetric metric, Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key) =>
        (vertices.IsEmpty ? VerticesOf(mesh: mesh, source: source, key: key) : Fin.Succ(vertices))
            .Bind(points => VectorCloud.Ring(points: points, context: context, key: key))
            .Bind(cloud => VectorIntent.Cloud(cloud: cloud, metric: metric, key: key))
            .Bind(intent => intent.Project<TOut>(context: context, key: key));
    private static Fin<Vector3d> NormalOf(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key) => source switch {
        { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < mesh.Faces.Count =>
            (mesh.FaceNormals.Count > face || mesh.FaceNormals.ComputeFaceNormals()) && mesh.FaceNormals.Count > face
                ? VectorIntent.Direction(value: new Vector3d(mesh.FaceNormals[face])).Project<Vector3d>(context: context, key: key)
                : Fin.Fail<Vector3d>(key.InvalidResult()),
        { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
            FaceIndicesOf(mesh: mesh, ngon: ngon, key: key)
                .Bind(faces => ((mesh.FaceNormals.Count >= mesh.Faces.Count || mesh.FaceNormals.ComputeFaceNormals()) && mesh.FaceNormals.Count >= mesh.Faces.Count) switch {
                    true => faces.TraverseM(face => FaceArea(mesh: mesh, source: new ComponentIndex(ComponentIndexType.MeshFace, face), vertices: Seq<Point3d>(), context: context, key: key)
                            .Map(area => new Vector3d(mesh.FaceNormals[face]) * area)).As()
                        .Bind(weighted => VectorIntent.Direction(value: weighted.Fold(initialState: Vector3d.Zero, f: static (sum, normal) => sum + normal)).Project<Vector3d>(context: context, key: key)),
                    false => RingMetric<Vector3d>(metric: VectorCloudMetric.Normal, mesh: mesh, source: source, vertices: vertices, context: context, key: key),
                }),
        _ => Fin.Fail<Vector3d>(key.InvalidInput()),
    };
    private static Fin<double> FaceEdgeAspect(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key) =>
        (source switch {
            { ComponentIndexType: ComponentIndexType.MeshFace, Index: int index } when index >= 0 && index < mesh.Faces.Count => Fin.Succ(mesh.Faces.GetFaceAspectRatio(index: index)),
            _ => Fin.Fail<double>(key.InvalidInput()),
        }).BindFail(_ => RingMetric<double>(metric: VectorCloudMetric.EdgeAspect, mesh: mesh, source: source, vertices: vertices, context: context, key: key));
    private static Fin<double> FaceArea(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key) =>
        source switch {
            { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
                FaceIndicesOf(mesh: mesh, ngon: ngon, key: key)
                    .Bind(faces => faces.TraverseM(face => FaceArea(mesh: mesh, source: new ComponentIndex(ComponentIndexType.MeshFace, face), vertices: Seq<Point3d>(), context: context, key: key)).As())
                    .Map(static areas => areas.Fold(initialState: 0.0, f: static (total, area) => total + area)),
            _ => RingMetric<double>(metric: VectorCloudMetric.Area, mesh: mesh, source: source, vertices: vertices, context: context, key: key),
        };
    private static Fin<double> FaceMaxDihedral(Mesh mesh, ComponentIndex source, Seq<Point3d> vertices, Context context, Op key) =>
        NormalOf(mesh: mesh, source: source, vertices: vertices, context: context, key: key).Bind(normal =>
            (source switch {
                { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < mesh.Faces.Count => Fin.Succ(toSeq(mesh.Faces.AdjacentFaces(faceIndex: face))),
                { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < mesh.Ngons.Count =>
                    FaceIndicesOf(mesh: mesh, ngon: ngon, key: key)
                        .Map((Seq<int> parts) => parts.Bind((int face) => toSeq(mesh.Faces.AdjacentFaces(faceIndex: face))).Filter((int other) => !parts.Exists((int face) => face == other)).Distinct()),
                _ => Fin.Fail<Seq<int>>(key.InvalidInput()),
            }).Bind((Seq<int> neighbours) => neighbours
                .TraverseM(other => NormalOf(mesh: mesh, source: new ComponentIndex(ComponentIndexType.MeshFace, other), vertices: Seq<Point3d>(), context: context, key: key)
                    .Bind(neighbour => VectorIntent.Angular(a: normal, b: neighbour).Project<double>(context: context, key: key))).As()
                .Map(angles => Stat.Extrema(items: angles, projection: static angle => angle, tolerance: 0.0, direction: ExtremumDirection.Maximum).Head.IfNone(0.0))));
}

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshSample(MeshSampleKind Kind, int Value) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(!Kind.Group.Equals(MeshSampleGroup.None)),
        ValidityClaim.Of(Value >= 0));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshMetricSample(ComponentIndex Source, double Value) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(Source is { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 }),
        ValidityClaim.Nonnegative(Value));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshFaceShape(ComponentIndex Source, VectorCloudShape Shape) : IValidityEvidence {
    public bool IsValid => ValidityClaim.Of(Source is { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 });
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class Analyze {
    internal static Operation<TGeometry, TOut> MeshLift<TGeometry, TOut, TValue>(Op key, Operation<Mesh, TValue> source) where TGeometry : notnull =>
        Native<TGeometry, TOut, Mesh, TValue, Operation<Mesh, TValue>>(key: key, state: source, requirement: source.Requirement, requiresContext: source.RequiresContext,
            project: static (operation, mesh) => operation.Apply(geometry: Seq(mesh)));
    internal static Operation<Mesh, MeshCheckParameters> MeshCheck {
        get {
            Op key = Op.Of();
            return Operation<Mesh, MeshCheckParameters>.Build(key: key, state: key,
                evaluator: static (op, geometry) =>
                    from parameters in Requirement.MeshReport(mesh: geometry, check: op.ToString()).ToEff()
                    from result in op.Accept(value: parameters).ToEff()
                    select result);
        }
    }
    internal static Operation<Mesh, MeshSample> MeshSamples(MeshSampleGroup group) {
        Op key = Op.Of(name: $"Mesh{group.Label}");
        return Operation<Mesh, MeshSample>.Build(key: key, state: (Key: key, group.Kinds, group.Inspect),
            evaluator: static (state, geometry) => state.Inspect switch {
                true => from parameters in MeshCheck.Apply(geometry: Seq(geometry))
                        from head in parameters.Head.ToFin(state.Key.InvalidResult()).ToEff()
                        from samples in state.Kinds.TraverseM(kind => kind.Sample(mesh: geometry, parameters: head).Bind(value => state.Key.AcceptValue(value: new MeshSample(Kind: kind, Value: value)))).As().ToEff()
                        select samples,
                false => state.Kinds.TraverseM(kind => kind.Sample(mesh: geometry, parameters: default).Bind(value => state.Key.AcceptValue(value: new MeshSample(Kind: kind, Value: value)))).As().ToEff(),
            });
    }
    internal static Operation<Mesh, MeshMetricSample> MeshMetricSamplesOp(MeshMetric metric, Op key) =>
        Operation<Mesh, MeshMetricSample>.Build(key: key, state: (Key: key, Metric: metric), requirement: Requirement.MeshCheck, requiresContext: true,
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from samples in MeshMetricSamples(mesh: geometry, metric: state.Metric, context: runtime.Context, key: state.Key, cancel: runtime.Cancellation).ToEff()
                select samples);
    internal static Operation<Mesh, Stat> MeshMetricStatOp(MeshMetric metric, Op key) =>
        Operation<Mesh, Stat>.Build(key: key, state: (Key: key, Metric: metric), requirement: Requirement.MeshCheck, requiresContext: true,
            evaluator: static (state, geometry) =>
                from runtime in Env.EnvAsks
                from stat in MeshMetricSamples(mesh: geometry, metric: state.Metric, context: runtime.Context, key: state.Key, cancel: runtime.Cancellation)
                    .Bind(samples => Stat.Of(values: samples.Map(static sample => sample.Value), key: state.Key))
                    .Bind(stat => state.Key.Accept(value: stat)).ToEff()
                select stat);
    internal static Operation<Mesh, MeshFaceShape> MeshFaceShapesOp(Op key) =>
        Operation<Mesh, MeshFaceShape>.Build(key: key, state: key, requirement: Requirement.MeshCheck, requiresContext: true,
            evaluator: static (op, geometry) =>
                from runtime in Env.EnvAsks
                from shapes in VisiblePolygonsOf(mesh: geometry, key: op).Bind(polygons => polygons.TraverseM(polygon => runtime.Cancellation.IsCancellationRequested
                    ? Fin.Fail<MeshFaceShape>(new Fault.Cancelled())
                    : MeshMetric.Shape(mesh: geometry, polygon: polygon, context: runtime.Context, key: op)).As()).ToEff()
                select shapes);
    internal static Operation<Mesh, TopologyProjection> MeshAtVisiblePolygon(Option<int> index = default) {
        Op key = Op.Of();
        return Operation<Mesh, TopologyProjection>.Build(key: key, state: (Key: key, Selector: index),
            evaluator: static (state, geometry) => VisiblePolygonsOf(mesh: geometry, key: state.Key).Bind(polygons => (Source: polygons, Index: state.Selector.IfNone(0)) switch {
                (Seq<MeshNgon> source, _) when source.Count == 0 => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidResult()),
                (Seq<MeshNgon> source, int selected) when selected < 0 || selected >= source.Count => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidInput()),
                (Seq<MeshNgon> source, int selected) => VisiblePolygonSourceOf(mesh: geometry, polygon: source[selected], key: state.Key)
                    .Bind(component => TopologyProjection.FromMesh(mesh: geometry, source: component))
                    .Bind(projection => state.Key.Accept(value: projection)),
            }).ToEff());
    }
    internal static Operation<Mesh, int> MeshVisiblePolygonCount {
        get {
            Op key = Op.Of();
            return Operation<Mesh, int>.Build(key: key, state: key,
                evaluator: static (op, mesh) => op.Accept(value: mesh.GetNgonAndFacesCount()).ToEff());
        }
    }
    internal static Operation<Mesh, Polyline> MeshNakedEdges {
        get {
            Op key = Op.Of(name: "MeshNakedEdges");
            return Operation<Mesh, Polyline>.Build(key: key, state: key,
                evaluator: static (op, mesh) => Optional(mesh.GetNakedEdges()).Map(loops => op.Accept(values: loops)).IfNone(Fin.Succ(Seq<Polyline>())).ToEff());
        }
    }
    internal static Operation<Mesh, Polyline> MeshOutline(Plane plane) {
        Op key = Op.Of(name: "MeshOutline");
        return plane.IsValid switch {
            true => Operation<Mesh, Polyline>.Build(key: key, state: (Key: key, Plane: plane),
                evaluator: static (state, mesh) => state.Key.Accept(values: mesh.GetOutlines(plane: state.Plane)).ToEff()),
            false => Operation<Mesh, Polyline>.Reject(key: key, fault: key.InvalidInput()),
        };
    }
    internal static Fin<ComponentIndex> VisiblePolygonSourceOf(Mesh mesh, MeshNgon polygon, Op key) =>
        Optional(polygon.BoundaryVertexIndexList()).Filter(static vertices => vertices.Length >= 3).ToFin(key.InvalidResult())
            .Bind(_ => Optional(polygon.FaceIndexList()).ToFin(key.InvalidResult()).Bind(faces => faces switch {
                uint[] values when values.Length == 1 && values[0] <= int.MaxValue && mesh.Ngons.NgonIndexFromFaceIndex((int)values[0]) < 0 => Fin.Succ(new ComponentIndex(ComponentIndexType.MeshFace, (int)values[0])),
                uint[] values when values.Length > 0 && values[0] <= int.MaxValue && mesh.Ngons.NgonIndexFromFaceIndex((int)values[0]) is >= 0 and int ngon => Fin.Succ(new ComponentIndex(ComponentIndexType.MeshNgon, ngon)),
                _ => Fin.Fail<ComponentIndex>(key.InvalidInput()),
            }));
    private static Fin<Seq<MeshMetricSample>> MeshMetricSamples(Mesh mesh, MeshMetric metric, Context context, Op key, CancellationToken cancel) =>
        VisiblePolygonsOf(mesh: mesh, key: key).Bind(polygons => polygons.TraverseM(polygon => cancel.IsCancellationRequested
            ? Fin.Fail<MeshMetricSample>(new Fault.Cancelled())
            : metric.Sample(mesh: mesh, polygon: polygon, context: context, key: key)).As());
    private static Fin<Seq<MeshNgon>> VisiblePolygonsOf(Mesh mesh, Op key) =>
        Optional(mesh.GetNgonAndFacesEnumerable()).ToFin(key.InvalidResult()).Map(static polygons => toSeq(polygons));
}
```

```mermaid
flowchart LR
    Topologies -->|OnGeometry gate| Duality[Mesh · Brep · brep-coercible]
    Duality -->|Euler · BoundaryLoops · Components| Genus["g = (2C − χ − B) / 2"]
    Meshes -->|MeshCheck once| Capture[MeshCheckParameters]
    Capture -->|13 defect rows| Samples[MeshSample census]
    Meshes -->|GetNgonAndFacesEnumerable| Polygons[visible polygons → ComponentIndex]
    Polygons -->|VectorCloud.Ring × VectorCloudMetric| Metrics[MeshMetricSample · MeshFaceShape]
    Metrics -->|Stat.Of| Summary[Stat]
    Samples & Metrics -.->|IValidityEvidence| Oracle[one validity oracle]
    Topologies & Meshes -->|Operation builders| Query[Analysis/query dispatch]
```

## [04]-[DENSITY_BAR]

One owner per axis; a new scalar, sample, or metric is a row, never a sibling surface.

| [INDEX] | [CONCERN]          | [OWNER]               | [KIND]                                      | [RAIL]                            | [CASES] |
| :-----: | :----------------- | :-------------------- | :------------------------------------------ | :-------------------------------- | :-----: |
|  [01]   | Structural queries | `Topologies`          | `[Union]` structural-query algebra          | `Operation → Eff<Env, Seq<TOut>>` |    6    |
|  [02]   | Topology scalars   | `TopologyScalar`      | `[SmartEnum<int>]` delegate extract rows    | `Fin<object>` at op gate          |    8    |
|  [03]   | Mesh queries       | `Meshes`              | `[Union]` mesh-inspection algebra           | `Operation → Eff<Env, Seq<TOut>>` |    7    |
|  [04]   | Sample census      | `MeshSampleKind`      | `[SmartEnum<int>]` — 5 bands, delegate rows | `Fin<int>` per row                |   32    |
|  [05]   | Face metrics       | `MeshMetric`          | `[SmartEnum<int>]` ngon measure delegates   | `Fin<MeshMetricSample>`           |    6    |
|  [06]   | Receipts           | `MeshSample` receipts | evidence `readonly record struct`           | `IValidityEvidence` carrier       |    —    |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
