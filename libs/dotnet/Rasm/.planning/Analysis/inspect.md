# [RASM_ANALYSIS_INSPECT]

Inspection owns the measured-query runtime's topology-scalar and mesh-quality diagnostics: `Topologies` `[Union]` closes structural interrogation over both Brep and Mesh, and `Meshes` `[Union]` closes mesh census, per-polygon metrics, and boundary extraction. Brep/mesh polymorphism collapses into one `OnGeometry` gate lowering brep-coercible inputs through the leased brep form, so every scalar, orientation, containment, and component fold is written once and a per-operation geometry switch is the deleted repetition. Both unions publish their operation builders on THEMSELVES — the `Analyze` facade lives once on `Analysis/query` and this page adds no fragment to it.

Rebuilds compose law legislated elsewhere: ring-metric mathematics is `Spatial/cloud` law projected through `Processing/intent` `VectorIntent`, the streaming moment fold is `Domain/stats` law on the `Scalar` carrier, and the `Kind` web, the `BrepForm`/`SurfaceForm` leases, and the `TopologyProjection` carrier are `Domain/normalization` law. Every receipt declares `IValidityEvidence`, admitted through the one `Domain/validation` acceptance oracle `Analysis/query` owns, and each family union exposes `internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>()` as that dispatch seam.

## [01]-[INDEX]

- [02]-[TOPOLOGY]: `Topologies` structural interrogation and the `TopologyScalar` rows over the one `OnGeometry` mesh/brep gate — Euler, genus, boundary, and component folds, solid orientation, point containment, and kind classification.
- [03]-[MESH]: `Meshes` mesh inspection — the banded `MeshSampleGroup` census over its `CensusSource` capture column, `MeshMetric` visible-polygon measurement over the `Spatial/cloud` ring, and the `MeshSample`/`MeshMetricSample`/`MeshFaceShape` evidence receipts.
- [04]-[DENSITY_BAR]: one owner per axis; a new scalar, sample, or metric is a row, never a sibling surface.

## [02]-[TOPOLOGY]

- Owner: `Topologies` `[Union]` closes structural interrogation over any admitted geometry — classification, interval domains, solid orientation, connected components, point containment, and the scalar family; `TopologyScalar` `[BoundaryAdapter]` `[SmartEnum<int>]` binds each scalar to an `OutputBinding` column, an accessor-derived `Op`, and an `Extract` delegate folding through the one `OnGeometry` gate, the count rows sharing one `CountOf` projection over mesh or brep. `MeasuredValue` `[Union]` is the measurement carrier both rosters answer on — flag, count, or statistic — so no row publishes an `object`.
- Cases: the structural-interrogation cases with `Scalar` carrying a `TopologyScalar` row; the flat scalar factories collapse onto `ScalarCase`, the fence declaring both closed sets.
- Entry: `Topologies.Operation<TGeometry, TOut>()` is the family seam every arm gates at build — `Capability.EvaluateTopology` admits scalar, orientation, and containment; curve-or-surface form admits domains; output type is checked per arm. Context is demanded only where read: classification and containment declare it, the scalar, domain, orientation, and component rows run scope-less, and an operation demanding context it never reads is the deleted over-requirement.
- Auto: `OnGeometry` is the one mesh/brep gate — `Mesh` and `Brep` dispatch directly, brep-coercible natives lower through `Capability.BrepForm`, everything else rejects — so brep-like admission is written once and every scalar, orientation, containment, and component fold routes through it; its third arity takes ONE `GeometryBase` delegate for folds polymorphic over the lowered geometry, collapsing `HoleCountOf` and `GenusOf` to a single body each. Derived scalars fold the primitive Euler, boundary-loop, and component counts through one applicative `Apply` gated on `OrientableOf`, and `PieceCount` brackets the disposal of every piece it counts.
- Receipt: no dedicated receipt rail — scalars project onto scalar, interval, kind, orientation, and geometry values through the acceptance oracle; `Components` re-emits owned `GeometryBase` pieces.
- Packages: RhinoCommon (`Mesh`/`Brep` topology, loop, and orientation surface), `Rasm.Domain` (`Kind` capability web, `BrepForm`/`SurfaceForm` leases, `Requirement.SolidTopology`, `Op`/`Fault` rail), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new topology scalar is one `TopologyScalar` row — key, output, and extract delegate over the same `OnGeometry` fold; a new structural interrogation is one `Topologies` case; a new geometry family entering the gate is one `OnGeometry` arm serving every row at once.
- Boundary: brep/mesh polymorphism lives in one gate — a per-operation `is Mesh`/`is Brep` switch is the deleted repetition, and a fold identical on both families takes the `onAny` arity rather than two drifting delegates; release brackets the ACQUISITION at both piece folds, so a throw mid-projection frees what a failure-rail `BindFail` never reaches; the genus, hole, and Euler family is derived from three primitive rows and a stored genus beside the formula is the killed form; `Capability.EvaluateTopology` is the single topology-evaluation admission row, containment escalating through `Requirement.SolidTopology`; `SolidOrientationOf` maps the mesh orientation int onto `BrepSolidOrientation` so both families answer in one enum, never a mesh-specific parallel vocabulary; component extraction owns its disposal — a piece failing the typed projection is disposed before the fault leaves.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using LanguageExt;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [TYPES] ---------------------------------------------------------------------------
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
    internal Op Key => Keys.Value[GetType()];
    private static readonly Lazy<FrozenDictionary<Type, Op>> Keys = new(static () =>
        typeof(Topologies).GetNestedTypes().ToFrozenDictionary(static row => row, static row => Op.Of(name: row.Name)));
    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        state: Key,
        kindCase: static (key, _) => Classify<TGeometry, TOut>(key: key),
        domainsCase: static (key, _) => Spans<TGeometry, TOut>(key: key),
        solidOrientationCase: static (key, _) => Orientation<TGeometry, TOut>(key: key),
        componentsCase: static (key, _) => Pieces<TGeometry, TOut>(key: key),
        containsPointCase: static (key, cp) => Inside<TGeometry, TOut>(point: cp.Point, key: key),
        scalarCase: static (_, scalar) => Scalar<TGeometry, TOut>(scalar: scalar.Scalar));

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
        from _ in guard(ValidityClaim.Finite(target).Holds, op.InvalidInput())
        from contained in OnGeometry(geometry: geometry, op: op,
            onMesh: mesh => Fin.Succ(mesh.IsPointInside(point: target, tolerance: context.For(lane: ToleranceLane.Distance).Value, strictlyIn: false)),
            onBrep: brep => Fin.Succ(brep.IsPointInside(point: target, tolerance: context.For(lane: ToleranceLane.Distance).Value, strictlyIn: false)))
        select contained;
    internal static Fin<Seq<GeometryBase>> ComponentsOf<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Mesh mesh => Fin.Succ(toSeq(mesh.SplitDisjointPieces().Cast<GeometryBase>())),
            Brep brep => BrepPieces(brep: brep, op: op),
            GeometryBase { HasBrepForm: true } native => Normalization.BrepForm(source: native, key: op).Bind(lease => lease.Use(brep => BrepPieces(brep: brep, op: op))),
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
            onMesh: m => Optional(m.GetNakedEdges()).ToFin(op.InvalidResult()).Map(static loops => loops.Length),
            onBrep: static b => Fin.Succ(BoundaryCount(brep: b, predicate: static loop => loop.LoopType is BrepLoopType.Outer or BrepLoopType.Inner)));
    internal static Fin<bool> OrientableOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op,
            onMesh: static m => Fin.Succ(m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented),
            onBrep: static b => Fin.Succ(b.IsManifold));
    internal static Fin<int> GenusOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op, onAny: native =>
            OrientableOf(geometry: native, op: op).Bind(orientable => orientable
                ? (EulerOf(geometry: native, op: op), BoundaryLoopsOf(geometry: native, op: op), PieceCount(geometry: native, op: op))
                    .Apply(static (euler, boundaries, components) => ((2 * components) - euler - boundaries) / 2).As()
                : Fin.Fail<int>(op.Unsupported(inputType: native.GetType(), outputType: typeof(int)))));
    internal static Fin<int> HoleCountOf<TG>(TG geometry, Op op) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op, onAny: native =>
            (BoundaryLoopsOf(geometry: native, op: op), PieceCount(geometry: native, op: op))
                .Apply(static (boundaries, components) => Math.Max(val1: 0, val2: boundaries - components)).As());
    internal static Fin<int> CountOf<TG>(TG geometry, Op op, Func<Mesh, int> meshCount, Func<Brep, int> brepCount) where TG : notnull =>
        OnGeometry(geometry: geometry, op: op, onMesh: m => Fin.Succ(meshCount(arg: m)), onBrep: b => Fin.Succ(brepCount(arg: b)));

    private static Operation<TGeometry, TOut> Classify<TGeometry, TOut>(Op key) where TGeometry : notnull =>
        (Capability.Universal(type: typeof(TGeometry)) || Rasm.Domain.Kind.Of(type: typeof(TGeometry)).IsSome)
            ? typeof(TOut) switch {
                Type t when t == typeof(Kind) => Lift<TGeometry, Kind, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k)), requiresContext: true).As<TGeometry, TOut>(key: key),
                Type t when t == typeof(string) => Lift<TGeometry, string, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k.ToString(format: null, formatProvider: CultureInfo.InvariantCulture))), requiresContext: true).As<TGeometry, TOut>(key: key),
                Type t when t == typeof(Topology) => Lift<TGeometry, Topology, Op>(key: key, state: key, extract: static (op, g, ctx) => g.KindOf(context: ctx).Bind(k => op.Accept(value: k.Topology)), requiresContext: true).As<TGeometry, TOut>(key: key),
                _ => key.Unsupported<TGeometry, TOut>(),
            }
            : key.Unsupported<TGeometry, TOut>();
    private static Operation<TGeometry, TOut> Spans<TGeometry, TOut>(Op key) where TGeometry : notnull =>
        typeof(TOut) == typeof(Interval) && (Capability.CurveForm.Admits(type: typeof(TGeometry)) || Capability.SurfaceForm.Admits(type: typeof(TGeometry)))
            ? Lift<TGeometry, Interval, Op>(key: key, state: key, extract: static (op, g, _) => DomainsOf(geometry: g, op: op).Bind(domains => op.Accept(values: domains))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    private static Operation<TGeometry, TOut> Orientation<TGeometry, TOut>(Op key) where TGeometry : notnull =>
        typeof(TOut) == typeof(BrepSolidOrientation) && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
            ? Lift<TGeometry, BrepSolidOrientation, Op>(key: key, state: key, extract: static (op, g, _) => SolidOrientationOf(geometry: g, op: op).Bind(orientation => op.Accept(value: orientation))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    private static Operation<TGeometry, TOut> Pieces<TGeometry, TOut>(Op key) where TGeometry : notnull =>
        Decomposes<TGeometry, TOut>()
            ? Lift<TGeometry, TOut, Op>(key: key, state: key, extract: static (op, g, _) => ComponentsOf(geometry: g, op: op).Bind(components => ProjectPieces<TOut>(components: components, op: op))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    private static bool Decomposes<TGeometry, TOut>() =>
        (typeof(TOut) == typeof(Brep) || typeof(TOut) == typeof(Mesh))
        && (Capability.Universal(type: typeof(TGeometry)) || typeof(TOut).IsAssignableFrom(c: typeof(TGeometry)));
    private static Operation<TGeometry, TOut> Inside<TGeometry, TOut>(Point3d point, Op key) where TGeometry : notnull =>
        ValidityClaim.Finite(point).Holds && typeof(TOut) == typeof(bool) && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
            ? Lift<TGeometry, bool, (Op Key, Point3d Target)>(key: key, state: (Key: key, Target: point), requirement: Some(Requirement.SolidTopology),
                extract: static (s, g, ctx) => ContainsPoint(geometry: g, target: s.Target, context: ctx, op: s.Key).Bind(contained => s.Key.Accept(value: contained))).As<TGeometry, TOut>(key: key)
            : key.Unsupported<TGeometry, TOut>();
    private static Operation<TGeometry, TOut> Scalar<TGeometry, TOut>(TopologyScalar scalar) where TGeometry : notnull =>
        scalar.Output.Serves<TOut>() && Capability.EvaluateTopology.Admits(type: typeof(TGeometry))
            ? Lift<TGeometry, TOut, TopologyScalar>(key: scalar.Op, state: scalar,
                extract: static (row, g, _) => OnGeometry(geometry: g, op: row.Op, onAny: native => row.Extract(geometry: native, op: row.Op))
                    .Bind(value => row.Output.Admit<TOut>(values: Seq(value.Boxed), key: row.Op)))
            : scalar.Op.Unsupported<TGeometry, TOut>();
    private static Operation<TGeometry, TValue> Lift<TGeometry, TValue, TState>(Op key, TState state, Func<TState, TGeometry, Context, Fin<Seq<TValue>>> extract, Option<Requirement> requirement = default, bool requiresContext = false) where TGeometry : notnull =>
        Analysis.Operation<TGeometry, TValue>.Build(
            key: key, requirement: requirement, requiresContext: requiresContext, state: (State: state, Extract: extract),
            evaluator: static (s, geometry) =>
                from context in Env.Asks
                from result in s.Extract(arg1: s.State, arg2: geometry, arg3: context).ToEff()
                select result);
    private static Fin<TResult> OnGeometry<TGeometry, TResult>(TGeometry geometry, Op op, Func<Mesh, Fin<TResult>> onMesh, Func<Brep, Fin<TResult>> onBrep) where TGeometry : notnull =>
        Optional(geometry).ToFin(op.InvalidInput()).Bind(g => g switch {
            Mesh mesh => onMesh(arg: mesh),
            Brep brep => onBrep(arg: brep),
            object brepLike when Capability.BrepForm.Admits(type: brepLike.GetType()) => Normalization.BrepForm(source: brepLike, key: op).Bind(lease => lease.Use(project: onBrep)),
            _ => Fin.Fail<TResult>(op.Unsupported(g.GetType(), typeof(TResult))),
        });
    private static Fin<TResult> OnGeometry<TGeometry, TResult>(TGeometry geometry, Op op, Func<GeometryBase, Fin<TResult>> onAny) where TGeometry : notnull =>
        OnGeometry(geometry: geometry, op: op, onMesh: mesh => onAny(arg: mesh), onBrep: brep => onAny(arg: brep));
    private static Fin<Seq<GeometryBase>> BrepPieces(Brep brep, Op op) =>
        brep.GetConnectedComponents() switch {
            Brep[] components when components.Length > 0 => Fin.Succ(toSeq(components.Cast<GeometryBase>())),
            _ when brep.IsValid => op.AcceptValue(brep).Map(static valid => Seq((GeometryBase)valid.DuplicateBrep())),
            _ => Fin.Fail<Seq<GeometryBase>>(op.InvalidResult()),
        };
    private static Fin<Seq<TOut>> ProjectPieces<TOut>(Seq<GeometryBase> components, Op op) =>
        IO.pure(components).Bracket(
            Use: owned => IO.lift(() => owned.TraverseM(component => component is TOut typed
                ? Fin.Succ(typed)
                : Fin.Fail<TOut>(op.Unsupported(inputType: component.GetType(), outputType: typeof(TOut)))).As()),
            Fin: static owned => IO.lift(() => owned.Filter(static component => component is not TOut).Iter(static component => component.Dispose())))
            .Run();
    private static int BoundaryCount(Brep brep, Func<BrepLoop, bool> predicate) =>
        toSeq(brep.Loops).Filter(loop => predicate(arg: loop) && toSeq(loop.Trims).Exists(static trim => trim.Edge is { Valence: EdgeAdjacency.Naked })).Count;
    private static Fin<int> PieceCount<TGeometry>(TGeometry geometry, Op op) where TGeometry : notnull =>
        ComponentsOf(geometry: geometry, op: op).Bind(components => IO.pure(components).Bracket(
                Use: owned => IO.lift(() => owned.Count > 0 ? Fin.Succ(owned.Count) : Fin.Fail<int>(op.InvalidResult())),
                Fin: static owned => IO.lift(() => owned.Iter(static component => component.Dispose())))
            .Run());
}

[Union]
public abstract partial record MeasuredValue {
    private MeasuredValue() { }
    public sealed record FlagCase(bool Value) : MeasuredValue;
    public sealed record CountCase(int Value) : MeasuredValue;
    public sealed record StatisticCase(double Value) : MeasuredValue;
    public static MeasuredValue Flag(bool held) => new FlagCase(Value: held);
    public static MeasuredValue Count(int tally) => new CountCase(Value: tally);
    public static MeasuredValue Statistic(double value) => new StatisticCase(Value: value);
    internal object Boxed => Switch(
        flagCase: static row => (object)row.Value,
        countCase: static row => (object)row.Value,
        statisticCase: static row => (object)row.Value);
    internal ValidityClaim Admissible => Switch(
        flagCase: static _ => new ValidityClaim(Holds: true),
        countCase: static row => ValidityClaim.CountAtLeast(count: row.Value, floor: 0),
        statisticCase: static row => ValidityClaim.Finite(row.Value));
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class TopologyScalar {
    public static readonly TopologyScalar Manifold = new(key: 0, label: nameof(Manifold), output: OutputBinding.Of<bool>(), extract: static (g, op) => Topologies.ManifoldOf(geometry: g, op: op).Map(MeasuredValue.Flag));
    public static readonly TopologyScalar Euler = new(key: 1, label: nameof(Euler), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.EulerOf(geometry: g, op: op).Map(MeasuredValue.Count));
    public static readonly TopologyScalar BoundaryLoops = new(key: 2, label: nameof(BoundaryLoops), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.BoundaryLoopsOf(geometry: g, op: op).Map(MeasuredValue.Count));
    public static readonly TopologyScalar Genus = new(key: 3, label: nameof(Genus), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.GenusOf(geometry: g, op: op).Map(MeasuredValue.Count));
    public static readonly TopologyScalar HoleCount = new(key: 4, label: nameof(HoleCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.HoleCountOf(geometry: g, op: op).Map(MeasuredValue.Count));
    public static readonly TopologyScalar FaceCount = new(key: 5, label: nameof(FaceCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.Faces.Count, brepCount: static b => b.Faces.Count).Map(MeasuredValue.Count));
    public static readonly TopologyScalar EdgeCount = new(key: 6, label: nameof(EdgeCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.TopologyEdges.Count, brepCount: static b => b.Edges.Count).Map(MeasuredValue.Count));
    public static readonly TopologyScalar VertexCount = new(key: 7, label: nameof(VertexCount), output: OutputBinding.Of<int>(), extract: static (g, op) => Topologies.CountOf(geometry: g, op: op, meshCount: static m => m.Vertices.Count, brepCount: static b => b.Vertices.Count).Map(MeasuredValue.Count));
    public string Label { get; }
    public OutputBinding Output { get; }
    internal Op Op => Keys.Value[this];
    private static readonly Lazy<FrozenDictionary<TopologyScalar, Op>> Keys = new(static () =>
        Items.ToFrozenDictionary(static row => row, static row => Op.Of(name: row.Label)));
    [UseDelegateFromConstructor] internal partial Fin<MeasuredValue> Extract(GeometryBase geometry, Op op);
}
```

## [03]-[MESH]

- Owner: `MeshSampleGroup` `[BoundaryAdapter]` `[SmartEnum<int>]` bands the sample census, carries the `Decade` column its rows key against, and names one `CensusSource` row deciding where the band's `MeshCheckParameters` come from; band membership rides an accessor-backed `Lazy` index, never a per-census rescan. `MeshSampleKind` `[SmartEnum<int>]` derives its band off the key decade and binds each row to a `Sample(Mesh, MeshCheckParameters, Op)` delegate across validity flags, census counts (the topology rows reusing the `TopologyScalar` extractors), defect counters reading the threaded capture, and valence-quality folds. `MeshMetric` `[BoundaryAdapter]` `[SmartEnum<int>]` binds each face metric to a measure delegate over one visible polygon and publishes ONE `Measure` builder whose `OutputBinding` pair picks the terminal fold. `Meshes` `[Union]` closes sample groups, face quality and shape, visible-polygon addressing and count, naked edges, and plane outlines.
- Cases: the mesh-inspection cases with `Samples` carrying a `MeshSampleGroup` and `FaceQuality` a `MeshMetric`; the fence declares the band rows and metric rows.
- Entry: `Meshes.Operation<TGeometry, TOut>()` lifts every arm through `Lift` — the mesh specialization applying a typed `Operation<Mesh, TValue>` to any geometry that is a mesh and rejecting the rest — so the family accepts `object`-typed pipelines and stays mesh-strict at evaluation.
- Law: absence is `Option`, never a null-object row. `Meshes.FaceQuality(Option<MeshMetric>)` defaults through `IfNone(MeshMetric.EdgeAspect)`, so the census `MeshMetric.None`, `MeshSampleGroup.None`, and `MeshSampleKind.None` rows — each existing only to be rejected — DELETE with the `Equals(None)` guards that read them.
- Auto: `CensusSource` decides the capture with one row column — `Native` answers the host defaults and `Checked` runs `Requirement.MeshReport` once per census — so the standalone check operation and its nested `Apply` hop delete and every band row reads one capture. Visible-polygon resolution maps an ngon-or-face onto the canonical `ComponentIndex` (`MeshNgon` for ngon members, `MeshFace` otherwise) and extracts its boundary ring, so every per-polygon metric addresses one component vocabulary. Metric measurement short-circuits host fast paths where they exist and folds the `Spatial/cloud` ring metric everywhere else; ngon area sums constituent faces, ngon normals area-weight constituent normals, and the dihedral fold walks ngon-external adjacency for the maximum inter-polygon angle at `ToleranceLane.Angle`. Every per-polygon fold reads the runtime cancellation token between polygons, returning `Errors.Cancelled` mid-census rather than finishing a stale sweep; `Requirement.MeshCheck` gates every metric operation so no defective mesh reaches a measurement.
- Receipt: `MeshSample`, `MeshMetricSample`, and `MeshFaceShape` carry the sample, the addressed measurement, and the addressed `Spatial/cloud` shape classification; each declares `IValidityEvidence` through the `Domain/rails` `ValidityClaim` fold.
- Packages: RhinoCommon (`Mesh.Check`/`MeshCheckParameters`, `MeshNgon` census, face and ngon accessors, outlines, `ComponentIndex`), `Rasm.Spatial` (`VectorCloud` ring metrics), `Rasm.Processing` (`VectorIntent`), `Rasm.Domain` (`Requirement.MeshCheck`/`MeshReport`, `Scalar`/`Stat`, `Tolerance`, `TopologyProjection`, `Op`/`Fault` rail), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new mesh sample is one `MeshSampleKind` row in its band, the census and receipt machinery untouched; a new face metric is one `MeshMetric` row binding a measure delegate over the same polygon resolution, and a new metric OUTPUT is one `OutputBinding` arm on `Measure` over the same census; a new capture source is one `CensusSource` row; a new polygon-level extraction is one `Meshes` case lifted through `Lift`.
- Boundary: a row's band derives from its key decade — the `group:` column beside it was a second authority one edit could contradict, and the index is read through its accessor so the generator has filled `Items` first; defect rows read the one threaded `MeshCheckParameters` capture and a per-row `Mesh.Check` re-run is the killed N-fold host cost; face metrics measure visible polygons through the canonical `ComponentIndex` addressing, never a triangle-level parallel family; ring measurement routes through the `Spatial/cloud` metric surface exclusively, never a local perimeter/skewness/area loop; `AtVisiblePolygon` re-emits the `Domain/normalization` `TopologyProjection` carrier on its `Fin` rail so downstream extraction shares the corpus transfer/disposal protocol.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using LanguageExt;
using Rasm.Domain;
using Rasm.Processing;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Analysis;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>][KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CensusSource {
    public static readonly CensusSource Native = new(key: "native", capture: static _ => Fin.Succ(MeshCheckParameters.Defaults()));
    public static readonly CensusSource Checked = new(key: "checked", capture: static mesh => Requirement.MeshReport(mesh: mesh, check: Checked.Key));
    [UseDelegateFromConstructor] internal partial Fin<MeshCheckParameters> Capture(Mesh mesh);
}

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
    internal Op Key => Keys.Value[GetType()];
    private static readonly Lazy<FrozenDictionary<Type, Op>> Keys = new(static () =>
        typeof(Meshes).GetNestedTypes().ToFrozenDictionary(static row => row, static row => Op.Of(name: row.Name)));
    public static Meshes Validity => new SamplesCase(Group: MeshSampleGroup.Validity);
    public static Meshes Counts => new SamplesCase(Group: MeshSampleGroup.Count);
    public static Meshes Defects => new SamplesCase(Group: MeshSampleGroup.Defect);
    public static Meshes Quality => new SamplesCase(Group: MeshSampleGroup.Quality);
    public static Meshes FaceQuality(Option<MeshMetric> metric = default) => new FaceQualityCase(Metric: metric.IfNone(MeshMetric.EdgeAspect));
    public static Meshes FaceShape => new FaceShapeCase();
    public static Meshes AtVisiblePolygon(Option<int> index = default) => new AtVisiblePolygonCase(Value: index);
    public static Meshes VisiblePolygonCount => new VisiblePolygonCountCase();
    public static Meshes NakedEdges => new NakedEdgesCase();
    public static Meshes Outline(Plane plane) => new OutlineCase(Plane: plane);
    internal Operation<TGeometry, TOut> Operation<TGeometry, TOut>() where TGeometry : notnull => Switch(
        state: Key,
        samplesCase: static (key, s) => Lift<TGeometry, TOut, MeshSample>(key: key, source: s.Group.Census(key: key)),
        faceQualityCase: static (key, fq) => fq.Metric.Measure<TGeometry, TOut>(key: key),
        faceShapeCase: static (key, _) => typeof(TOut) == typeof(MeshFaceShape)
            ? Lift<TGeometry, TOut, MeshFaceShape>(key: key, source: MeshMetric.Shapes(key: key))
            : key.Unsupported<TGeometry, TOut>(),
        atVisiblePolygonCase: static (key, at) => Lift<TGeometry, TOut, TopologyProjection>(key: key, source: Addressed(index: at.Value, key: key)),
        visiblePolygonCountCase: static (key, _) => Lift<TGeometry, TOut, int>(key: key, source: Tally(key: key)),
        nakedEdgesCase: static (key, _) => Lift<TGeometry, TOut, Polyline>(key: key, source: Boundaries(key: key)),
        outlineCase: static (key, o) => Lift<TGeometry, TOut, Polyline>(key: key, source: Section(plane: o.Plane, key: key)));

    internal static Fin<ComponentIndex> SourceOf(Mesh mesh, MeshNgon polygon, Op key) =>
        Optional(polygon.BoundaryVertexIndexList()).Filter(static vertices => vertices.Length >= 3).ToFin(key.InvalidResult())
            .Bind(_ => Optional(polygon.FaceIndexList()).ToFin(key.InvalidResult()).Bind(faces => faces switch {
                uint[] values when values.Length == 1 && values[0] <= int.MaxValue && mesh.Ngons.NgonIndexFromFaceIndex((int)values[0]) < 0 => Fin.Succ(new ComponentIndex(ComponentIndexType.MeshFace, (int)values[0])),
                uint[] values when values.Length > 0 && values[0] <= int.MaxValue && mesh.Ngons.NgonIndexFromFaceIndex((int)values[0]) is >= 0 and int ngon => Fin.Succ(new ComponentIndex(ComponentIndexType.MeshNgon, ngon)),
                _ => Fin.Fail<ComponentIndex>(key.InvalidInput()),
            }));
    internal static Fin<Seq<MeshNgon>> PolygonsOf(Mesh mesh, Op key) =>
        Optional(mesh.GetNgonAndFacesEnumerable()).ToFin(key.InvalidResult()).Map(static polygons => toSeq(polygons));

    internal static Operation<TGeometry, TOut> Lift<TGeometry, TOut, TValue>(Op key, Operation<Mesh, TValue> source) where TGeometry : notnull =>
        Analysis.Operation<TGeometry, TOut>.Native<Mesh, TValue, Operation<Mesh, TValue>>(key: key, state: source, requirement: Some(source.Requirement), requiresContext: source.RequiresContext,
            project: static (operation, mesh) => operation.Apply(geometry: Seq(mesh)));
    private static Operation<Mesh, TopologyProjection> Addressed(Option<int> index, Op key) =>
        Analysis.Operation<Mesh, TopologyProjection>.Build(key: key, state: (Key: key, Selector: index),
            evaluator: static (state, geometry) => PolygonsOf(mesh: geometry, key: state.Key).Bind(polygons => (Source: polygons, Index: state.Selector.IfNone(0)) switch {
                (Seq<MeshNgon> source, _) when source.Count == 0 => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidResult()),
                (Seq<MeshNgon> source, int selected) when selected < 0 || selected >= source.Count => Fin.Fail<Seq<TopologyProjection>>(state.Key.InvalidInput()),
                (Seq<MeshNgon> source, int selected) => SourceOf(mesh: geometry, polygon: source[selected], key: state.Key)
                    .Bind(component => TopologyProjection.Of(mesh: geometry, source: component))
                    .Bind(projection => state.Key.Accept(value: projection)),
            }).ToEff());
    private static Operation<Mesh, int> Tally(Op key) =>
        Analysis.Operation<Mesh, int>.Build(key: key, state: key,
            evaluator: static (op, mesh) => op.Accept(value: mesh.GetNgonAndFacesCount()).ToEff());
    private static Operation<Mesh, Polyline> Boundaries(Op key) =>
        Analysis.Operation<Mesh, Polyline>.Build(key: key, state: key,
            evaluator: static (op, mesh) => Optional(mesh.GetNakedEdges()).Map(loops => op.Accept(values: loops)).IfNone(Fin.Succ(Seq<Polyline>())).ToEff());
    private static Operation<Mesh, Polyline> Section(Plane plane, Op key) =>
        plane.IsValid
            ? Analysis.Operation<Mesh, Polyline>.Build(key: key, state: (Key: key, Plane: plane),
                evaluator: static (state, mesh) => state.Key.Accept(values: mesh.GetOutlines(plane: state.Plane)).ToEff())
            : Analysis.Operation<Mesh, Polyline>.Reject(key: key, fault: key.InvalidInput());
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MeshSampleGroup {
    public static readonly MeshSampleGroup Validity = new(key: 1, decade: 0, label: nameof(Validity), source: CensusSource.Native);
    public static readonly MeshSampleGroup Count = new(key: 2, decade: 1, label: nameof(Count), source: CensusSource.Native);
    public static readonly MeshSampleGroup Defect = new(key: 3, decade: 2, label: nameof(Defect), source: CensusSource.Checked);
    public static readonly MeshSampleGroup Quality = new(key: 4, decade: 4, label: nameof(Quality), source: CensusSource.Native);
    public string Label { get; }
    internal int Decade { get; }
    internal CensusSource Source { get; }
    internal static MeshSampleGroup OfDecade(int decade) => Decades.Value[decade];
    private static readonly Lazy<FrozenDictionary<int, MeshSampleGroup>> Decades = new(static () =>
        Items.ToFrozenDictionary(static row => row.Decade, static row => row));
    internal Seq<MeshSampleKind> Kinds => Bands.Value[this];
    private static readonly Lazy<FrozenDictionary<MeshSampleGroup, Seq<MeshSampleKind>>> Bands = new(static () =>
        Items.ToFrozenDictionary(static row => row, static row => toSeq(MeshSampleKind.Items).Filter(kind => kind.Group.Equals(row))));
    internal Operation<Mesh, MeshSample> Census(Op key) =>
        Operation<Mesh, MeshSample>.Build(key: key, state: (Key: key, Kinds, Source),
            evaluator: static (state, mesh) =>
                from parameters in state.Source.Capture(mesh: mesh).ToEff()
                from samples in state.Kinds.TraverseM(kind => kind.Sample(mesh: mesh, parameters: parameters, key: kind.Op).Bind(value => state.Key.AcceptValue(value: new MeshSample(Kind: kind, Value: value)))).As().ToEff()
                select samples);
}

[SmartEnum<int>]
public sealed partial class MeshSampleKind {
    public static readonly MeshSampleKind Valid = new(key: 1, label: nameof(Valid), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsValid)));
    public static readonly MeshSampleKind Closed = new(key: 2, label: nameof(Closed), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsClosed)));
    public static readonly MeshSampleKind Oriented = new(key: 3, label: nameof(Oriented), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool _) && oriented)));
    public static readonly MeshSampleKind Solid = new(key: 4, label: nameof(Solid), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsSolid)));
    public static readonly MeshSampleKind Manifold = new(key: 5, label: nameof(Manifold), sample: static (m, _, key) => Topologies.ManifoldOf(geometry: m, op: key).Map(MeasuredValue.Flag));
    public static readonly MeshSampleKind BoundaryFree = new(key: 6, label: nameof(BoundaryFree), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Flag(m.IsManifold(topologicalTest: true, isOriented: out bool _, hasBoundary: out bool boundary) && !boundary)));
    public static readonly MeshSampleKind Vertices = new(key: 10, label: nameof(Vertices), sample: static (m, _, key) => TopologyScalar.VertexCount.Extract(geometry: m, op: key));
    public static readonly MeshSampleKind Faces = new(key: 11, label: nameof(Faces), sample: static (m, _, key) => TopologyScalar.FaceCount.Extract(geometry: m, op: key));
    public static readonly MeshSampleKind Triangles = new(key: 12, label: nameof(Triangles), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Count(m.Faces.TriangleCount)));
    public static readonly MeshSampleKind Quads = new(key: 13, label: nameof(Quads), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Count(m.Faces.QuadCount)));
    public static readonly MeshSampleKind Edges = new(key: 14, label: nameof(Edges), sample: static (m, _, key) => TopologyScalar.EdgeCount.Extract(geometry: m, op: key));
    public static readonly MeshSampleKind Euler = new(key: 15, label: nameof(Euler), sample: static (m, _, key) => TopologyScalar.Euler.Extract(geometry: m, op: key));
    public static readonly MeshSampleKind VisiblePolygons = new(key: 16, label: nameof(VisiblePolygons), sample: static (m, _, _) => Fin.Succ(MeasuredValue.Count(m.GetNgonAndFacesCount())));
    public static readonly MeshSampleKind DegenerateFaces = new(key: 20, label: nameof(DegenerateFaces), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.DegenerateFaceCount)));
    public static readonly MeshSampleKind DisjointMeshes = new(key: 21, label: nameof(DisjointMeshes), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.DisjointMeshCount)));
    public static readonly MeshSampleKind DuplicateFaces = new(key: 22, label: nameof(DuplicateFaces), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.DuplicateFaceCount)));
    public static readonly MeshSampleKind ExtremelyShortEdges = new(key: 23, label: nameof(ExtremelyShortEdges), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.ExtremelyShortEdgeCount)));
    public static readonly MeshSampleKind InvalidNgons = new(key: 24, label: nameof(InvalidNgons), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.InvalidNgonCount)));
    public static readonly MeshSampleKind NakedEdges = new(key: 25, label: nameof(NakedEdges), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.NakedEdgeCount)));
    public static readonly MeshSampleKind NonManifoldEdges = new(key: 26, label: nameof(NonManifoldEdges), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.NonManifoldEdgeCount)));
    public static readonly MeshSampleKind NonUnitVectorNormals = new(key: 27, label: nameof(NonUnitVectorNormals), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.NonUnitVectorNormalCount)));
    public static readonly MeshSampleKind RandomFaceNormals = new(key: 28, label: nameof(RandomFaceNormals), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.RandomFaceNormalCount)));
    public static readonly MeshSampleKind SelfIntersectingPairs = new(key: 29, label: nameof(SelfIntersectingPairs), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.SelfIntersectingPairsCount)));
    public static readonly MeshSampleKind UnusedVertices = new(key: 30, label: nameof(UnusedVertices), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.UnusedVertexCount)));
    public static readonly MeshSampleKind VertexFaceNormalsDiffer = new(key: 31, label: nameof(VertexFaceNormalsDiffer), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.VertexFaceNormalsDifferCount)));
    public static readonly MeshSampleKind ZeroLengthNormals = new(key: 32, label: nameof(ZeroLengthNormals), sample: static (_, p, _) => Fin.Succ(MeasuredValue.Count(p.ZeroLengthNormalCount)));
    public static readonly MeshSampleKind MaximumValence = new(key: 40, label: nameof(MaximumValence), sample: static (m, _, key) => Valence(mesh: m, key: key, project: static stat => MeasuredValue.Count((int)stat.Maximum.Value)));
    public static readonly MeshSampleKind MinimumValence = new(key: 41, label: nameof(MinimumValence), sample: static (m, _, key) => Valence(mesh: m, key: key, project: static stat => MeasuredValue.Count((int)stat.Minimum.Value)));
    public static readonly MeshSampleKind BoundaryLoopCount = new(key: 42, label: nameof(BoundaryLoopCount), sample: static (m, _, key) => TopologyScalar.BoundaryLoops.Extract(geometry: m, op: key));
    public static readonly MeshSampleKind Genus = new(key: 43, label: nameof(Genus), sample: static (m, _, key) => TopologyScalar.Genus.Extract(geometry: m, op: key));
    public static readonly MeshSampleKind AverageValence = new(key: 44, label: nameof(AverageValence), sample: static (m, _, key) => Valence(mesh: m, key: key, project: static stat => MeasuredValue.Statistic(stat.Mean)));
    public string Label { get; }
    internal MeshSampleGroup Group => MeshSampleGroup.OfDecade(decade: Key / 10);
    internal Op Op => Keys.Value[this];
    private static readonly Lazy<FrozenDictionary<MeshSampleKind, Op>> Keys = new(static () =>
        Items.ToFrozenDictionary(static row => row, static row => Op.Of(name: row.Label)));
    [UseDelegateFromConstructor] internal partial Fin<MeasuredValue> Sample(Mesh mesh, MeshCheckParameters parameters, Op key);
    private static Fin<MeasuredValue> Valence(Mesh mesh, Op key, Func<Stat<Scalar>, MeasuredValue> project) =>
        toSeq(Enumerable.Range(0, mesh.TopologyVertices.Count).Select(mesh.TopologyVertices.ConnectedEdgesCount)) switch {
            { IsEmpty: true } => Fin.Fail<MeasuredValue>(key.InvalidResult()),
            Seq<int> valences => Stat<Scalar>.Of(values: valences.Map(static valence => (Scalar)(double)valence), key: key).Map(project),
        };
}

internal readonly record struct FaceMoments(Atom<HashMap<int, (Vector3d Normal, double Area)>> Held) {
    internal static FaceMoments Seeded() => new(Held: Atom(HashMap<int, (Vector3d Normal, double Area)>()));
    internal Fin<(Vector3d Normal, double Area)> At(int face, Func<Fin<(Vector3d Normal, double Area)>> measure) =>
        Held.Value.Find(key: face).Match(
            Some: Fin.Succ,
            None: () => measure().Map(moment => Cell.Claim(cell: Held, key: face, mint: () => moment).Current[face]));
}

internal readonly record struct PolygonProbe(Mesh Mesh, ComponentIndex Source, Option<Seq<Point3d>> Vertices, FaceMoments Moments) {
    internal PolygonProbe AtFace(int face) => this with { Source = new ComponentIndex(ComponentIndexType.MeshFace, face), Vertices = None };
}

[BoundaryAdapter, SmartEnum<int>]
public sealed partial class MeshMetric {
    public static readonly MeshMetric EdgeAspect = new(key: 0, measure: EdgeAspectOf);
    public static readonly MeshMetric Area = new(key: 1, measure: AreaOf);
    public static readonly MeshMetric Perimeter = new(key: 2, measure: static (probe, context, key) => Ring<double>(metric: VectorCloudMetric.Perimeter, probe: probe, context: context, key: key));
    public static readonly MeshMetric Skewness = new(key: 3, measure: static (probe, context, key) => Ring<double>(metric: VectorCloudMetric.Skewness, probe: probe, context: context, key: key));
    public static readonly MeshMetric DihedralAngle = new(key: 4, measure: DihedralOf);
    [UseDelegateFromConstructor] private partial Fin<double> Measure(PolygonProbe probe, Context context, Op key);
    private static readonly OutputBinding SampleBinding = OutputBinding.Of<MeshMetricSample>();
    private static readonly OutputBinding SummaryBinding = OutputBinding.Of<Stat<Scalar>>();
    internal Operation<TGeometry, TOut> Measure<TGeometry, TOut>(Op key) where TGeometry : notnull =>
        SampleBinding.Serves<TOut>()
            ? Meshes.Lift<TGeometry, TOut, MeshMetricSample>(key: key, source: Folded(key: key, terminal: static (samples, _) => Fin.Succ(samples)))
        : SummaryBinding.Serves<TOut>()
            ? Meshes.Lift<TGeometry, TOut, Stat<Scalar>>(key: key, source: Folded(key: key, terminal: static (samples, op) =>
                Stat<Scalar>.Of(values: samples.Map(static sample => (Scalar)sample.Value), key: op).Bind(stat => op.Accept(value: stat))))
        : key.Unsupported<TGeometry, TOut>();
    private Operation<Mesh, TValue> Folded<TValue>(Op key, Func<Seq<MeshMetricSample>, Op, Fin<Seq<TValue>>> terminal) where TValue : notnull =>
        Operation<Mesh, TValue>.Build(key: key, state: (Key: key, Metric: this, Terminal: terminal), requirement: Some(Requirement.MeshCheck), requiresContext: true,
            evaluator: static (state, mesh) =>
                from runtime in Env.EnvAsks
                from values in Census(mesh: mesh, metric: state.Metric, context: runtime.Context, key: state.Key, cancel: runtime.Cancellation)
                    .Bind(samples => state.Terminal(arg1: samples, arg2: state.Key)).ToEff()
                select values);
    internal static Operation<Mesh, MeshFaceShape> Shapes(Op key) =>
        Operation<Mesh, MeshFaceShape>.Build(key: key, state: key, requirement: Some(Requirement.MeshCheck), requiresContext: true,
            evaluator: static (op, mesh) =>
                from runtime in Env.EnvAsks
                from shapes in Meshes.PolygonsOf(mesh: mesh, key: op).Bind(polygons => (Moments: FaceMoments.Seeded(), Polygons: polygons) switch {
                    var run => run.Polygons.TraverseM(polygon => runtime.Cancellation.IsCancellationRequested
                        ? Fin.Fail<MeshFaceShape>(Errors.Cancelled)
                        : Shape(mesh: mesh, polygon: polygon, moments: run.Moments, context: runtime.Context, key: op)).As(),
                }).ToEff()
                select shapes);
    internal Fin<MeshMetricSample> Sample(Mesh mesh, MeshNgon polygon, FaceMoments moments, Context context, Op key) =>
        Probe(mesh: mesh, polygon: polygon, moments: moments, key: key)
            .Bind(probe => Measure(probe: probe, context: context, key: key).Map(value => (probe.Source, Value: value)))
            .Bind(state => key.AcceptValue(value: new MeshMetricSample(Source: state.Source, Value: state.Value)));
    internal static Fin<MeshFaceShape> Shape(Mesh mesh, MeshNgon polygon, FaceMoments moments, Context context, Op key) =>
        Probe(mesh: mesh, polygon: polygon, moments: moments, key: key)
            .Bind(probe => Ring<VectorCloudShape>(metric: VectorCloudMetric.Shape, probe: probe, context: context, key: key)
                .Map(shape => new MeshFaceShape(Source: probe.Source, Shape: shape)));
    private static Fin<PolygonProbe> Probe(Mesh mesh, MeshNgon polygon, FaceMoments moments, Op key) =>
        Meshes.SourceOf(mesh: mesh, polygon: polygon, key: key)
            .Bind(source => VerticesOf(mesh: mesh, source: source, key: key)
                .Map(vertices => new PolygonProbe(Mesh: mesh, Source: source, Vertices: Some(vertices), Moments: moments)));
    private static Fin<Seq<MeshMetricSample>> Census(Mesh mesh, MeshMetric metric, Context context, Op key, CancellationToken cancel) =>
        Meshes.PolygonsOf(mesh: mesh, key: key).Bind(polygons => (Moments: FaceMoments.Seeded(), Polygons: polygons) switch {
            var run => run.Polygons.TraverseM(polygon => cancel.IsCancellationRequested
                ? Fin.Fail<MeshMetricSample>(Errors.Cancelled)
                : metric.Sample(mesh: mesh, polygon: polygon, moments: run.Moments, context: context, key: key)).As(),
        });
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
    private static Fin<TOut> Ring<TOut>(VectorCloudMetric metric, PolygonProbe probe, Context context, Op key) =>
        probe.Vertices.Match(Some: Fin.Succ, None: () => VerticesOf(mesh: probe.Mesh, source: probe.Source, key: key))
            .Bind(points => VectorCloud.Ring(points: points, context: context, key: key))
            .Bind(cloud => VectorIntent.Cloud(cloud: cloud, metric: metric, key: key))
            .Bind(intent => intent.Project<TOut>(context: context, key: key));
    private static Fin<Vector3d> NormalOf(PolygonProbe probe, Context context, Op key) => probe.Source switch {
        { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < probe.Mesh.Faces.Count =>
            probe.Moments.At(face: face, measure: () => FaceMomentOf(probe: probe, face: face, context: context, key: key)).Map(static moment => moment.Normal),
        { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < probe.Mesh.Ngons.Count =>
            FaceIndicesOf(mesh: probe.Mesh, ngon: ngon, key: key)
                .Bind(faces => Normals(mesh: probe.Mesh)
                    ? faces.TraverseM(face => probe.Moments.At(face: face, measure: () => FaceMomentOf(probe: probe, face: face, context: context, key: key))).As()
                        .Bind(moments => VectorIntent.Direction(value: moments.Fold(initialState: Vector3d.Zero, f: static (sum, moment) => sum + (moment.Normal * moment.Area))).Project<Vector3d>(context: context, key: key))
                    : Ring<Vector3d>(metric: VectorCloudMetric.Normal, probe: probe, context: context, key: key)),
        _ => Fin.Fail<Vector3d>(key.InvalidInput()),
    };
    private static Fin<(Vector3d Normal, double Area)> FaceMomentOf(PolygonProbe probe, int face, Context context, Op key) =>
        Normals(mesh: probe.Mesh)
            ? from normal in VectorIntent.Direction(value: new Vector3d(probe.Mesh.FaceNormals[face])).Project<Vector3d>(context: context, key: key)
              from area in Ring<double>(metric: VectorCloudMetric.Area, probe: probe.AtFace(face: face), context: context, key: key)
              select (Normal: normal, Area: area)
            : Fin.Fail<(Vector3d Normal, double Area)>(key.InvalidResult());
    private static bool Normals(Mesh mesh) => mesh.FaceNormals.Count >= mesh.Faces.Count || mesh.FaceNormals.ComputeFaceNormals();
    private static Fin<double> EdgeAspectOf(PolygonProbe probe, Context context, Op key) =>
        (probe.Source switch {
            { ComponentIndexType: ComponentIndexType.MeshFace, Index: int index } when index >= 0 && index < probe.Mesh.Faces.Count => Fin.Succ(probe.Mesh.Faces.GetFaceAspectRatio(index: index)),
            _ => Fin.Fail<double>(key.InvalidInput()),
        }).BindFail(_ => Ring<double>(metric: VectorCloudMetric.EdgeAspect, probe: probe, context: context, key: key));
    private static Fin<double> AreaOf(PolygonProbe probe, Context context, Op key) =>
        probe.Source switch {
            { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < probe.Mesh.Ngons.Count =>
                FaceIndicesOf(mesh: probe.Mesh, ngon: ngon, key: key)
                    .Bind(faces => faces.TraverseM(face => probe.Moments.At(face: face, measure: () => FaceMomentOf(probe: probe, face: face, context: context, key: key))).As())
                    .Map(static moments => moments.Fold(initialState: 0.0, f: static (total, moment) => total + moment.Area)),
            _ => Ring<double>(metric: VectorCloudMetric.Area, probe: probe, context: context, key: key),
        };
    private static Fin<double> DihedralOf(PolygonProbe probe, Context context, Op key) =>
        NormalOf(probe: probe, context: context, key: key).Bind(normal =>
            (probe.Source switch {
                { ComponentIndexType: ComponentIndexType.MeshFace, Index: int face } when face >= 0 && face < probe.Mesh.Faces.Count => Fin.Succ(toSeq(probe.Mesh.Faces.AdjacentFaces(faceIndex: face))),
                { ComponentIndexType: ComponentIndexType.MeshNgon, Index: int ngon } when ngon >= 0 && ngon < probe.Mesh.Ngons.Count =>
                    FaceIndicesOf(mesh: probe.Mesh, ngon: ngon, key: key)
                        .Map((Seq<int> parts) => parts.Bind((int face) => toSeq(probe.Mesh.Faces.AdjacentFaces(faceIndex: face))).Filter((int other) => !parts.Exists((int face) => face == other)).Distinct()),
                _ => Fin.Fail<Seq<int>>(key.InvalidInput()),
            }).Bind((Seq<int> neighbours) => neighbours
                .TraverseM(other => NormalOf(probe: probe.AtFace(face: other), context: context, key: key)
                    .Bind(neighbour => VectorIntent.Angular(a: normal, b: neighbour).Project<double>(context: context, key: key))).As()
                .Bind(angles => Stat.Extrema(items: angles, projection: static angle => angle, band: context.For(lane: ToleranceLane.Angle), direction: ExtremumDirection.Maximum)
                    .Head.ToFin(key.InvalidResult()))));
}

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshSample(MeshSampleKind Kind, MeasuredValue Value) : IValidityEvidence {
    public bool IsValid => Value is MeasuredValue measured && measured.Admissible;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshMetricSample(ComponentIndex Source, double Value) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Source is { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 },
        ValidityClaim.Nonnegative(Value));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshFaceShape(ComponentIndex Source, VectorCloudShape Shape) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(Source is { ComponentIndexType: not ComponentIndexType.InvalidType, Index: >= 0 });
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Topology and mesh inspection census
    accDescr: Topologies resolving Euler characteristic, boundary loops, and components into genus, meshes capturing one check through the census source row into a thirteen-row defect census and visible polygons into metric samples summarized as carrier statistics, both feeding the validity oracle and entering the query dispatch as operation builders.
    Topologies -->|OnGeometry gate| Duality[Mesh · Brep · brep-coercible]
    Duality -->|Euler · BoundaryLoops · Components| Genus["g = (2C − χ − B) / 2"]
    Meshes -->|CensusSource.Capture once| Capture[MeshCheckParameters]
    Capture -->|13 defect rows| Samples[MeshSample census]
    Meshes -->|GetNgonAndFacesEnumerable| Polygons[visible polygons → ComponentIndex]
    Polygons -->|VectorCloud.Ring × VectorCloudMetric| Metrics[MeshMetricSample · MeshFaceShape]
    Metrics -->|Stat Scalar Of| Summary[Stat Scalar]
    Samples & Metrics -.->|IValidityEvidence| Oracle[one validity oracle]
    Topologies & Meshes -->|Operation builders| Query[Analysis/query dispatch]
```

## [04]-[DENSITY_BAR]

One owner per axis; a new scalar, sample, or metric is a row, never a sibling surface.

| [INDEX] | [CONCERN]          | [OWNER]               | [KIND]                                      | [RAIL]                            | [CASES] |
| :-----: | :----------------- | :-------------------- | :------------------------------------------ | :-------------------------------- | :-----: |
|  [01]   | Structural queries | `Topologies`          | `[Union]` structural-query algebra          | `Operation → Eff<Env, Seq<TOut>>` |    6    |
|  [02]   | Topology scalars   | `TopologyScalar`      | `[SmartEnum<int>]` delegate extract rows    | `Fin<MeasuredValue>` at op gate   |    8    |
|  [03]   | Mesh queries       | `Meshes`              | `[Union]` mesh-inspection algebra           | `Operation → Eff<Env, Seq<TOut>>` |    7    |
|  [04]   | Census capture     | `CensusSource`        | `[SmartEnum<string>]` capture delegate      | `Fin<MeshCheckParameters>`        |    2    |
|  [05]   | Sample census      | `MeshSampleKind`      | `[SmartEnum<int>]` — 4 bands, delegate rows | `Fin<MeasuredValue>` per row      |   31    |
|  [06]   | Face metrics       | `MeshMetric`          | `[SmartEnum<int>]` ngon measure delegates   | `Measure → Operation<TG, TOut>`   |    5    |
|  [07]   | Measured value     | `MeasuredValue`       | `[Union]` flag/count/statistic carrier      | total `Switch` + one boxed seam   |    3    |
|  [08]   | Receipts           | `MeshSample` receipts | evidence `readonly record struct`           | `IValidityEvidence` carrier       |    —    |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
