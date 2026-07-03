# [RASM_SUPPORT]

The proximity boundary adapter: ONE `SupportSpace` `[BoundaryAdapter]` admitting any closest-point-capable Rhino geometry or cloud cluster behind one polymorphic handle, and ONE `SupportProjection` `[SmartEnum<int>]` owning all fourteen closest-hit output modalities behind a single capability-gated `Project<TOut>`. The pair is the corpus model of one-enum-all-modalities: a new proximity answer is one vocabulary row with its capability, acceptance, and projection columns — never a sibling method, never an output-typed overload family. Every proximity read in the corpus — field distance cases, hit-directed vector fields, conformance residuals, containment classification — routes through this one gate.

The page composes the `Domain` floor as settled vocabulary: `evaluation.md` owns `ClosestHit` (the 9-field evaluation receipt) and the `ClosestOf`/`SignedDistanceOf` polymorphic evaluation lattice this adapter drives; `normalization.md` owns the `Capability` admission rows (`Closest`/`ClosestNormal`/`ClosestTangent`/`ClosestFrame`/`SignedDistance`) whose verdicts gate admission once and every projection row after; `atoms.md` owns the `AtomProjection` raw→typed output rail — the canonical carriers (`ClosestHit`, `Direction`, `VectorSpan`) each project through it, and this enum's egress delegates to THOSE projections. `SurfaceSpace` is NOT here — parametric `(u,v)` evaluation is `Parametric/projections.md`'s family; this page owns proximity alone, and the two were welded in the retired source only by filename.

## [01]-[INDEX]

- [02]-[SUPPORT_PROJECTION]: fourteen capability-gated closest-hit projections behind one `Project<TOut>`; the projection state record; the canonical-owner egress resolution.
- [03]-[SUPPORT_SPACE]: the polymorphic proximity handle over Rhino geometry and cloud clusters; admission, closest, signed-distance, containment-distance.

## [02]-[SUPPORT_PROJECTION]

- Owner: `SupportProjection` `[SmartEnum<int>]` — fourteen rows (`Closest`/`Direction`/`Span`/`Normal`/`Distance`/`Parameter`/`Uv`/`Component`/`MeshPoint`/`SignedDistance`/`ContainmentDistance`/`Tangent`/`Frame`/`SignedSpanAway`), each carrying three `[UseDelegateFromConstructor]` columns: `Capability(SupportSpace, ClosestHit)` (may this space answer this row), `Accepts(Type)` (which `TOut` shapes this row projects), and `ProjectRaw(SupportState)` (the row's canonical-value resolution). One internal `Project<TOut>(SupportSpace, ClosestHit, Point3d, Context, Op)` is the sole egress.
- Cases: 14 — `Closest` (hit point or the whole `ClosestHit`), `Direction`/`Normal`/`Tangent` (unit carriers, `Direction` or raw `Vector3d`), `Span`/`SignedSpanAway` (the sample→hit displacement as `VectorSpan`/`Vector3d`/`Line`/`double`, sign ±1 a factory parameter — one `SpanOf(key, sign)` row builder, two rows), `Distance`/`Parameter`/`Uv`/`Component`/`MeshPoint`/`Frame` (`Option`-carried `ClosestHit` fields lifted through one `HitValue<T>` row builder), `SignedDistance`/`ContainmentDistance` (space-delegated signed evaluations).
- Entry: `internal Fin<TOut> Project<TOut>(SupportSpace space, ClosestHit hit, Point3d sample, Context context, Op key)` — a three-gate switch: invalid hit → `key.InvalidResult()`; capability refused → `key.Unsupported(space.SourceType, typeof(TOut))`; output shape refused → `key.Unsupported(typeof(SupportProjection), typeof(TOut))`; then `ProjectRaw` yields a CANONICAL value and the egress resolves it — a direct `TOut` passes, a canonical carrier (`ClosestHit`/`Direction`/`VectorSpan`) delegates to its OWN `Project<TOut>`. The gates run in evidence order — hit validity before capability before shape — so a fault names the first real refusal.
- Auto: row bodies are one-expression folds over the hit — `HitValue` rows lift an `Option` hit field with `ToFin(key.InvalidResult())` and accept it; `Direction`-family rows admit through `Direction.Of(vector, context, key)` so a degenerate direction faults at the atom, never as a raw zero vector; span rows read `state.Output` twice by design — the `Vector3d` and `double` answers are RAW reads (a sample lying ON the support has a legal zero displacement, and the signed scalar `sign·‖hit−sample‖` cannot ride the positive-by-construction magnitude), while `VectorSpan`/`Line` admit through `VectorSpan.Of`, whose direction gate is correct for the unit-carrying shapes. `CanProjectVector(space)` is the derived predicate (`Direction`/`Span`/`SignedSpanAway` unconditional; `Normal`/`Tangent` gated on the captured verdicts) the `fields.md` hit-field admission reads.
- Receipt: none minted here — `ClosestHit` IS the evidence carrier and it is `evaluation.md`'s; a projection returns the typed answer or the typed fault.
- Packages: RhinoCommon (`Point3d`/`Vector3d`/`Plane`/`Line` — composed), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new proximity modality is one `static readonly` row with its three columns; a new output shape on an existing row is one row on the canonical carrier's `Project<TOut>` (the egress picks it up unchanged); zero new entrypoints.
- Boundary: the raw→typed resolution happens ONCE at the egress by delegating to the canonical owners' own `Project<TOut>` (`ClosestHit`, `Direction`, `VectorSpan` — each an `atoms.md` `AtomProjection` consumer) — a per-row `typeof` ladder or a local box-and-cast switch re-deriving those owners' dispatch is the deleted form; a `ProjectClosest`/`ProjectNormal`/`ProjectDistance` method family beside the enum is the rejected sibling surface; capability reads the handle's captured verdicts (`space.Admits.Normal`), never a per-call re-derivation of the `normalization.md` admission rows.

```csharp signature
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

    // Row builders: one shape per row family — hit-field lift, direction lift, signed span. Rows return
    // CANONICAL owner values; the egress delegates owner -> requested shape through the owner's Project<TOut>.
    private static SupportProjection Hit(int key, Func<Type, bool> accepts, Func<SupportState, Fin<object>> projectRaw,
        Func<SupportSpace, ClosestHit, bool>? capability = null) =>
        new(key: key, capability: capability ?? (static (_, _) => true), accepts: accepts, projectRaw: projectRaw);
    private static SupportProjection HitValue<T>(int key, Func<ClosestHit, Option<T>> choose,
        Func<SupportSpace, ClosestHit, bool>? capability = null) =>
        Hit(key: key, accepts: static output => output == typeof(T), capability: capability,
            projectRaw: state => choose(state.Hit).ToFin(Fail: state.Key.InvalidResult())
                .Bind(value => state.Key.AcceptValue(value: value).Map(static accepted => (object)accepted!)));
    // Vector3d and double are RAW state.Output reads: a sample ON the support yields a legal zero
    // displacement, and the signed scalar cannot ride VectorSpan's positive-by-construction magnitude.
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
                    Vectors.Direction owner => owner.Project<TOut>(key: key),
                    VectorSpan owner => owner.Project<TOut>(key: key),
                    _ => Fin.Fail<TOut>(error: key.InvalidResult()),
                }),
        };

    private static bool DirectionOrVector(Type output) => output == typeof(Direction) || output == typeof(Vector3d);
    private static Fin<object> DirectionOf(Vector3d vector, SupportState state) =>
        Vectors.Direction.Of(value: vector, context: state.Context, key: state.Key).Map(static direction => (object)direction);
}
```

## [03]-[SUPPORT_SPACE]

- Owner: `SupportSpace` `[BoundaryAdapter]` sealed record — the ONE proximity handle over `Plane`/`Sphere`/`Box`/`BoundingBox`/`Line`/`Polyline`/`Curve`/`Surface`/`BrepFace`/`Brep`/`Mesh`/`PointCloud` reference-or-value geometry AND `VectorCloud.ClusterCase` clusters. The polymorphic `Value: object` slot plus the `Admits` verdict column read once at admission is the honest carrier: the admitted set is open over every closest-capable kind, so a closed union would re-mint the `normalization.md` taxonomy.
- Entry: `public static Fin<SupportSpace> Of(object? value, Op? key = null)` — the ONE admission: a `ClusterCase` admits BY CONSTRUCTION — `cloud.md`'s factory already proved vertices, dedup, and mass, so the arm only captures the all-false projection verdicts, and re-traversing its vertices or re-proving `MassOf` here is the deleted cross-page double-admission; any other candidate admits by `Capability.Closest.Admits` on the runtime type (`object` roots refused) plus the `OpAcceptance.ValidityOf` oracle (`Domain/validation.md` — its compiled-lambda table covers the `Plane`/`Sphere`/`Box` value structs, so no per-shape validity special case survives here); the four projection verdicts (`ClosestNormal`/`ClosestTangent`/`ClosestFrame`/`SignedDistance`) are read from the `Capability` rows ONCE and captured as the `Admits` column.
- Auto: `Closest(sample, key)` dispatches cluster → `ClusterCase.ClosestVertex` (the indexed native probe `cloud.md` owns) and geometry → the `evaluation.md` extension `Value.ClosestOf(target, key)` — the whole Point/PointCloud/Line/Polyline/Plane/Sphere/Box/Curve/BrepFace/Surface/Brep/Mesh evaluation lattice arrives composed, never re-derived. `SignedDistance` delegates to `Value.SignedDistanceOf(hit, sample, key)`. `ContainmentDistance` is the solid-aware refinement: solid `Brep`/`Mesh` sign the hit distance by `IsPointInside(sample, context.Absolute.Value, strictlyIn: false)`; non-solid `Brep`/`Mesh` refuse (`key.InvalidInput()` — an open shell has no interior); analytic half-space carriers fall through to `SignedDistance`. `AdmitsSignedDistance` is per-shape evidence: analytic carriers need only a hit distance, everything else needs the captured `Signed` verdict plus the hit normal; `AdmitsContainmentDistance` needs solidity.
- Receipt: none — the handle carries admission evidence (`Admits`, `SourceType`) as columns; proximity answers travel as `ClosestHit`.
- Packages: RhinoCommon (`Brep.IsPointInside`/`Mesh.IsPointInside`/`Brep.IsSolid`/`Mesh.IsSolid` — the containment coupling IS the asset), LanguageExt.Core.
- Growth: a newly closest-capable Rhino kind is one `normalization.md` capability-row membership — this page changes ZERO lines; a new signed-distance species is one arm in `AdmitsSignedDistance`/`ContainmentDistance`.
- Boundary: `SupportSpace` is the ONE proximity adapter and a `PlaneSpace`/`MeshSpaceProximity`/`ClusterSpace` per-kind wrapper family is the deleted form; capability is READ from the handle's captured verdicts (`space.Admits.Normal`), never re-derived per call from `SourceType`; the cluster arm reaches `cloud.md`'s indexed closest-vertex probe as a composed seam — a second native `PointCloud` index minted here would double the `ClusterCase` cache; admission is once and the law crosses pages — `Closest`/`SignedDistance`/`ContainmentDistance` never re-validate the payload the factory already proved, and a `ClusterCase` arrives factory-proven, its vertices never re-traversed here.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record SupportSpace {
    private SupportSpace(object value, (bool Normal, bool Tangent, bool Frame, bool Signed) admits) { Value = value; Admits = admits; }

    public static Fin<SupportSpace> Of(object? value, Op? key = null) {
        Op op = key.OrDefault();
        return value switch {
            // ClusterCase is valid by construction (cloud.md factory-proven vertices/dedup/mass) —
            // re-proof here is the deleted double-admission; the arm only captures verdicts.
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

One owner per axis; a proximity capability is a row or a column, never a sibling surface.

| [INDEX] | [AXIS/CONCERN]              | [OWNER]             | [KIND]                                                             | [RAIL]                                    | [CASES] |
| :-----: | :-------------------------- | :------------------ | :----------------------------------------------------------------- | :---------------------------------------- | :-----: |
|  [01]   | Closest-hit output modality | `SupportProjection` | `[SmartEnum<int>]` + capability/accepts/projectRaw columns          | `Project<TOut> → Fin<TOut>`               |   14    |
|  [02]   | Proximity handle            | `SupportSpace`      | `[BoundaryAdapter]` polymorphic record + captured `Admits` verdicts | `Of → Fin<SupportSpace>`; `Closest → Fin<ClosestHit>` |    —    |
