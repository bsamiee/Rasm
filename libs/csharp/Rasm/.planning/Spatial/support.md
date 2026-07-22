# [RASM_SUPPORT]

`SupportSpace` and `SupportProjection` own the corpus proximity gate: one `[BoundaryAdapter]` handle over every closest-point-capable Rhino geometry and cloud cluster, and one `[SmartEnum<int>]` owning the closest-hit output modalities behind a single capability-gated `Project<TOut>`. A new proximity answer is one vocabulary row carrying its capability, acceptance, and projection columns. Every proximity read in the corpus routes through this gate.

This page composes settled `Domain` vocabulary: `evaluation.md` owns `ClosestHit` and the `ClosestOf`/`SignedDistanceOf` evaluation lattice this adapter drives; `normalization.md` owns the `Capability` admission rows whose verdicts gate admission once and every projection after; `atoms.md` owns the `AtomProjection` raw→typed rail its canonical carriers (`ClosestHit`, `Direction`, `VectorSpan`) project through, and this enum's egress delegates to those. Parametric `(u,v)` evaluation homes at `Parametric/projections.md`; this page owns proximity alone.

## [01]-[INDEX]

- [02]-[SUPPORT_PROJECTION]: `SupportProjection`'s capability-gated closest-hit projections behind one `Project<TOut>`; the projection state record; canonical-owner egress resolution.
- [03]-[SUPPORT_SPACE]: `SupportSpace` polymorphic proximity handle over Rhino geometry and cloud clusters; admission, closest, signed-distance, containment-distance.

## [02]-[SUPPORT_PROJECTION]

- Owner: `SupportProjection` `[SmartEnum<int>]` mints one row per closest-hit modality, each row three `[UseDelegateFromConstructor]` columns — `Capability` (may this space answer), `Accepts` (which `TOut` shapes it projects), `ProjectRaw` (canonical-value resolution); one internal `Project<TOut>` is the sole egress.
- Cases: span rows fold the sample→hit displacement with sign ±1 a factory parameter — one `SpanOf(key, sign)` builder mints `Span` and `SignedSpanAway`; the `ClosestHit`-field rows lift one `Option` field through the shared `HitValue<T>` builder; `SignedDistance`/`ContainmentDistance` delegate to the space's signed evaluations.
- Entry: `Project<TOut>` is a three-gate switch — hit validity, capability, output shape, in evidence order so a fault names the first real refusal; `ProjectRaw` then yields a canonical value the egress resolves, a canonical carrier (`ClosestHit`/`Direction`/`VectorSpan`) delegating to its own `Project<TOut>`.
- Auto: row bodies fold over the hit — `Direction`-family rows admit through `Direction.Of` so a degenerate direction faults at the atom, and span rows read the `Vector3d`/`double` answers raw. `CanProjectVector` is the derived predicate `fields.md`'s hit-field admission reads.
- Receipt: none — `ClosestHit` (owned by `evaluation.md`) is the evidence carrier; a projection returns the typed answer or the typed fault.
- Packages: RhinoCommon (`Point3d`/`Vector3d`/`Plane`/`Line`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new proximity modality is one `static readonly` row with its three columns; a new output shape is one row on the canonical carrier's `Project<TOut>`, picked up by the egress unchanged; zero new entrypoints.
- Boundary: raw→typed resolves once at the egress by delegating to the canonical owners' `Project<TOut>`; capability reads the handle's captured verdicts (`space.Admits.Normal`), never re-deriving the `normalization.md` admission rows per call.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;
using Rasm.Domain;

namespace Rasm.Spatial;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = Hit(key: 0,
        accepts: static output => output == typeof(Point3d) || output == typeof(ClosestHit),
        projectRaw: static s => Fin.Succ<object>(s.Hit));
    public static readonly SupportProjection Direction = Hit(key: 1, accepts: DirectionOrVector,
        projectRaw: static s => DirectionOf(vector: s.Hit.Point - s.Sample, state: s));
    public static readonly SupportProjection Span = SpanOf(key: 2, sign: +1.0);
    public static readonly SupportProjection Normal = new(key: 3,
        capability: static (space, _) => space.Admits.Normal, accepts: DirectionOrVector,
        projectRaw: static s => s.Hit.Normal.ToFin(Fail: s.Key.InvalidResult()).Bind(n => DirectionOf(vector: n, state: s)));
    public static readonly SupportProjection Distance = HitValue(key: 4, choose: static hit => hit.Distance);
    public static readonly SupportProjection Parameter = HitValue(key: 5, choose: static hit => hit.Parameter);
    public static readonly SupportProjection Uv = HitValue(key: 6, choose: static hit => hit.Uv);
    public static readonly SupportProjection Component = HitValue(key: 7, choose: static hit => hit.Component);
    public static readonly SupportProjection MeshPoint = HitValue(key: 8, choose: static hit => hit.MeshPoint);
    public static readonly SupportProjection SignedDistance = new(key: 9,
        capability: static (space, hit) => space.AdmitsSignedDistance(hit: hit),
        accepts: static output => output == typeof(double),
        projectRaw: static s => s.Space.SignedDistance(hit: s.Hit, sample: s.Sample, key: s.Key).Map(static d => (object)d));
    public static readonly SupportProjection ContainmentDistance = new(key: 10,
        capability: static (space, hit) => space.AdmitsContainmentDistance(hit: hit),
        accepts: static output => output == typeof(double),
        projectRaw: static s => s.Space.ContainmentDistance(hit: s.Hit, sample: s.Sample, context: s.Context, key: s.Key).Map(static d => (object)d));
    public static readonly SupportProjection Tangent = new(key: 11,
        capability: static (space, _) => space.Admits.Tangent, accepts: DirectionOrVector,
        projectRaw: static s => s.Hit.Tangent.ToFin(Fail: s.Key.InvalidResult()).Bind(t => DirectionOf(vector: t, state: s)));
    public static readonly SupportProjection Frame = HitValue(key: 12, choose: static hit => hit.Frame,
        capability: static (space, _) => space.Admits.Frame);
    public static readonly SupportProjection SignedSpanAway = SpanOf(key: 13, sign: -1.0);

    [UseDelegateFromConstructor] private partial bool Capability(SupportSpace space, ClosestHit hit);
    [UseDelegateFromConstructor] private partial bool Accepts(Type output);
    [UseDelegateFromConstructor] private partial Fin<object> ProjectRaw(SupportState state);

    private readonly record struct SupportState(SupportSpace Space, ClosestHit Hit, Point3d Sample, Context Context, Op Key, Type Output);

    // Row builders: one shape per row family; rows return CANONICAL owner values, the egress delegating each owner to its own Project<TOut>.
    private static SupportProjection Hit(int key, Func<Type, bool> accepts, Func<SupportState, Fin<object>> projectRaw,
        Func<SupportSpace, ClosestHit, bool>? capability = null) =>
        new(key: key, capability: capability ?? (static (_, _) => true), accepts: accepts, projectRaw: projectRaw);
    private static SupportProjection HitValue<T>(int key, Func<ClosestHit, Option<T>> choose,
        Func<SupportSpace, ClosestHit, bool>? capability = null) =>
        Hit(key: key, accepts: static output => output == typeof(T), capability: capability,
            projectRaw: state => choose(state.Hit).ToFin(Fail: state.Key.InvalidResult())
                .Bind(value => state.Key.AcceptValue(value: value).Map(static accepted => (object)accepted!)));
    // Vector3d/double are RAW state.Output reads: a sample ON the support has a legal zero displacement no positive-magnitude VectorSpan can carry.
    private static SupportProjection SpanOf(int key, double sign) =>
        Hit(key: key,
            accepts: static output => output == typeof(VectorSpan) || output == typeof(Vector3d) || output == typeof(Line) || output == typeof(double),
            projectRaw: state => state.Output switch {
                Type t when t == typeof(double) => state.Key.AcceptValue(value: sign * (state.Hit.Point - state.Sample).Length).Map(static d => (object)d),
                Type t when t == typeof(Vector3d) => state.Key.AcceptValue(value: sign * (state.Hit.Point - state.Sample)).Map(static v => (object)v),
                _ => VectorSpan.Of(anchor: state.Sample, vector: sign * (state.Hit.Point - state.Sample), context: state.Context, key: state.Key)
                    .Map(static span => (object)span),
            });

    internal bool CanProjectVector(SupportSpace space) =>
        Equals(Direction) || Equals(Span) || Equals(SignedSpanAway)
        || (Equals(Normal) && space.Admits.Normal)
        || (Equals(Tangent) && space.Admits.Tangent);

    internal Fin<TOut> Project<TOut>(SupportSpace space, ClosestHit hit, Point3d sample, Context context, Op key) =>
        (hit.IsValid, Capability(space: space, hit: hit), Accepts(output: typeof(TOut))) switch {
            (false, _, _) => Fin.Fail<TOut>(error: key.InvalidResult()),
            (_, false, _) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: space.SourceType, outputType: typeof(TOut))),
            (_, _, false) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SupportProjection), outputType: typeof(TOut))),
            _ => ProjectRaw(state: new SupportState(Space: space, Hit: hit, Sample: sample, Context: context, Key: key, Output: typeof(TOut)))
                .Bind(value => value switch {
                    TOut output => Fin.Succ(output),
                    ClosestHit owner => owner.Project<TOut>(key: key),
                    Numerics.Direction owner => owner.Project<TOut>(key: key),
                    VectorSpan owner => owner.Project<TOut>(key: key),
                    _ => Fin.Fail<TOut>(error: key.InvalidResult()),
                }),
        };

    private static bool DirectionOrVector(Type output) => output == typeof(Direction) || output == typeof(Vector3d);
    private static Fin<object> DirectionOf(Vector3d vector, SupportState state) =>
        Numerics.Direction.Of(value: vector, context: state.Context, key: state.Key).Map(static direction => (object)direction);
}
```

## [03]-[SUPPORT_SPACE]

- Owner: `SupportSpace` `[BoundaryAdapter]` sealed record — the ONE proximity handle over any closest-point-capable Rhino geometry and `VectorCloud.ClusterCase` clusters, behind a polymorphic `Value: object` slot and the `Admits` verdicts captured once at admission; the admitted set is open over every closest-capable kind, owned by `normalization.md`.
- Entry: `Of(object?, Op?)` is the ONE admission — a `ClusterCase` admits by construction (`cloud.md`'s factory proved its vertices, dedup, and mass), capturing only the all-false verdicts; any other candidate admits by `Capability.Closest.Admits` on the runtime type (`object` roots refused) and the `OpAcceptance.ValidityOf` oracle (`Domain/validation.md`, whose compiled-lambda table covers the value structs); the four projection verdicts read from the `Capability` rows once, captured as `Admits`.
- Auto: `Closest` dispatches cluster to `ClusterCase.ClosestVertex` and geometry to `evaluation.md`'s `Value.ClosestOf`, the whole evaluation lattice arriving composed; `SignedDistance` delegates to `Value.SignedDistanceOf`; `ContainmentDistance` signs the hit distance for solid `Brep`/`Mesh` by `IsPointInside`, refuses non-solid (an open shell has no interior), and falls through to `SignedDistance` for analytic carriers.
- Receipt: none — the handle carries admission evidence (`Admits`, `SourceType`) as columns; proximity answers travel as `ClosestHit`.
- Packages: RhinoCommon (`Brep.IsPointInside`/`Mesh.IsPointInside`/`Brep.IsSolid`/`Mesh.IsSolid`), LanguageExt.Core.
- Growth: a newly closest-capable Rhino kind is one `normalization.md` capability-row membership, changing zero lines here; a new signed-distance species is one arm in `AdmitsSignedDistance`/`ContainmentDistance`.
- Boundary: `SupportSpace` is the ONE proximity adapter; capability reads the handle's captured verdicts (`space.Admits.Normal`), never re-deriving from `SourceType` per call. Its cluster arm composes `cloud.md`'s indexed closest-vertex probe; a second `PointCloud` index minted here doubles the `ClusterCase` cache. Admission runs once and crosses pages, so `Closest`/`SignedDistance`/`ContainmentDistance` never re-validate the factory-proven payload.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record SupportSpace {
    private SupportSpace(object value, (bool Normal, bool Tangent, bool Frame, bool Signed) admits) { Value = value; Admits = admits; }

    public static Fin<SupportSpace> Of(object? value, Op? key = null) {
        Op op = key.OrDefault();
        return value switch {
            // ClusterCase valid by construction (cloud.md factory-proven vertices/dedup/mass); the arm only captures verdicts, never re-proving here.
            VectorCloud.ClusterCase cluster =>
                Fin.Succ(new SupportSpace(value: cluster, admits: (Normal: false, Tangent: false, Frame: false, Signed: false))),
            _ => from source in Optional(value).ToFin(op.InvalidInput())
                 let type = source.GetType()
                 from _ in guard(type != typeof(object) && Capability.Closest.Admits(type: type), op.Unsupported(type, typeof(ClosestHit)))
                 from valid in OpAcceptance.ValidityOf(source: source).Filter(static ok => ok).ToFin(op.InvalidInput())
                 select new SupportSpace(value: source, admits: (
                     Normal: Capability.ClosestNormal.Admits(type: type),
                     Tangent: Capability.ClosestTangent.Admits(type: type),
                     Frame: Capability.ClosestFrame.Admits(type: type),
                     Signed: Capability.SignedDistance.Admits(type: type))),
        };
    }

    internal object Value { get; }
    internal (bool Normal, bool Tangent, bool Frame, bool Signed) Admits { get; }
    internal Type SourceType => Value.GetType();

    internal bool AdmitsSignedDistance(ClosestHit hit) => Value switch {
        Plane or Sphere or Box or BoundingBox => hit.Distance.IsSome,
        _ => Admits.Signed && hit.Normal.IsSome,
    };
    internal bool AdmitsContainmentDistance(ClosestHit hit) => Value switch {
        Brep { IsSolid: true } or Mesh { IsSolid: true } => hit.Distance.IsSome,
        Brep or Mesh => false,
        _ => AdmitsSignedDistance(hit: hit),
    };

    internal Fin<ClosestHit> Closest(Point3d sample, Op key) => Value switch {
        VectorCloud.ClusterCase cluster => cluster.ClosestVertex(sample: sample, key: key),
        _ => Value.ClosestOf(target: sample, key: key),
    };
    internal Fin<double> SignedDistance(ClosestHit hit, Point3d sample, Op key) =>
        Value.SignedDistanceOf(hit: hit, sample: sample, key: key);
    internal Fin<double> ContainmentDistance(ClosestHit hit, Point3d sample, Context context, Op key) => Value switch {
        Brep { IsSolid: true } brep => hit.Distance.ToFin(Fail: key.InvalidResult())
            .Map(d => (brep.IsPointInside(sample, context.Absolute.Value, strictlyIn: false) ? -1.0 : 1.0) * d),
        Mesh { IsSolid: true } mesh => hit.Distance.ToFin(Fail: key.InvalidResult())
            .Map(d => (mesh.IsPointInside(sample, context.Absolute.Value, strictlyIn: false) ? -1.0 : 1.0) * d),
        Brep or Mesh => Fin.Fail<double>(error: key.InvalidInput()),
        _ => SignedDistance(hit: hit, sample: sample, key: key),
    };
}
```

## [04]-[DENSITY_BAR]

One owner per axis; each `[RAIL]` cell names the owner's return rail and `[CASES]` its bounded-vocabulary count.

| [INDEX] | [AXIS_CONCERN]     | [OWNER]             | [KIND]              | [RAIL]                                                | [CASES] |
| :-----: | :----------------- | :------------------ | :------------------ | :---------------------------------------------------- | :-----: |
|  [01]   | Closest-hit output | `SupportProjection` | `[SmartEnum<int>]`  | `Project<TOut> → Fin<TOut>`                           |   14    |
|  [02]   | Proximity handle   | `SupportSpace`      | `[BoundaryAdapter]` | `Of → Fin<SupportSpace>`; `Closest → Fin<ClosestHit>` |    —    |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
