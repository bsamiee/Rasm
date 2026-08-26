# [RASM_SUPPORT]

`SupportSpace` and `SupportProjection` own the corpus proximity gate: one closed `[Union]` over every closest-point-capable host shape, discriminated ONCE at admission by the proximity regime its reads need, and one `[SmartEnum<int>]` owning the closest-hit output modalities behind a single capability-gated `Project<TOut>`. Each new proximity answer is one vocabulary row carrying its required capability and its projection. Every proximity read in the corpus routes through this gate.

This page composes settled `Domain` vocabulary: `evaluation.md` owns `ClosestHit`, the `EvaluationRequest`/`EvaluationResult` algebra this adapter drives, and the one erased `extension(object? geometry) { Evaluate(request, key) }` ingress that algebra publishes; `normalization.md` owns the `Capability` admission rows whose verdicts gate admission once and every projection after; `validation.md` owns `ICapability`/`CapabilitySet` and `atoms.md` the `AtomProjection` raw→typed fold its canonical carriers (`ClosestHit`, `Direction`, `VectorSpan`) project through. Parametric `(u,v)` evaluation homes at `Parametric/projections.md`; this page owns proximity alone.

## [01]-[INDEX]

- [02]-[SUPPORT_CAPABILITY]: `SupportCapability` is the four-row proximity vocabulary and `CapabilitySet<SupportCapability>` the one column a space carries.
- [03]-[SUPPORT_SPACE]: `SupportSpace` `[Union]` over the proximity regimes; admission, closest, signed-distance, containment-distance.
- [04]-[SUPPORT_PROJECTION]: `SupportProjection`'s capability-gated closest-hit projections behind one `Project<TOut>`; canonical-owner egress resolution.
- [05]-[DENSITY_BAR]: one owner per axis.

## [02]-[SUPPORT_CAPABILITY]

- Owner: `SupportCapability` names the four proximity answers a support may or may not carry, and each row holds the `normalization.md` `Capability` row it projects, so admission derives the whole set from one type read rather than four sibling probes.
- Law: capability is a SET on the space, never a tuple of booleans — a four-bool tuple has sixteen corners, twelve of which no admission produces, and a caller reading `.Frame` had no way to ask "does this space carry everything this fold needs" in one test. `AdmitsAll` is that test.
- Auto: `Of(Type)` folds `Items` once at admission; the space stores the resulting set and every later read is a membership test against it, never a re-derivation from `SourceType`.
- Growth: a new proximity answer is one row here with one `SupportProjection` row naming it; every existing space widens by the same fold with no arm edited.
- Packages: Thinktecture.Runtime.Extensions, `Rasm.Domain` (`ICapability`, `CapabilitySet`, `Capability`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Spatial;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SupportCapability : ICapability<SupportCapability> {
    public static readonly SupportCapability Normal = new(key: "normal", rank: 0, reach: Capability.ClosestNormal);
    public static readonly SupportCapability Tangent = new(key: "tangent", rank: 1, reach: Capability.ClosestTangent);
    public static readonly SupportCapability Frame = new(key: "frame", rank: 2, reach: Capability.ClosestFrame);
    public static readonly SupportCapability Signed = new(key: "signed", rank: 3, reach: Capability.SignedDistance);

    public int Rank { get; }
    private Capability Reach { get; }

    internal static CapabilitySet<SupportCapability> Of(Type source) =>
        CapabilitySet<SupportCapability>.Of([.. Items.Where(row => row.Reach.Admits(type: source))]);
}
```

## [03]-[SUPPORT_SPACE]

- Owner: `SupportSpace` is the ONE proximity handle over any closest-point-capable host shape; its cases are the proximity REGIMES the page's reads discriminate — `Cluster` a point set with no surface, `Analytic` a closed analytic region whose signed distance is exact and total, `Region` a solid whose inside `IsPointInside` decides, `Sheet` an open shell that has normals but no inside, and `Form` the open closest-capable set `normalization.md` governs.
- Cases: `Analytic` carries an `AnalyticShape` `[Union]` rather than an `object`, because its admitted set is CLOSED — `Plane`, `Sphere`, `Box`, `BoundingBox` — and a closed set of host value structs with no common base is what a union is for; only the `Form` arm keeps `object`, and it keeps it because `Capability.Closest` is an open predicate over types `normalization.md` widens without editing this page. `Region` carries its containment probe as a `ContainProbe` ROW, so the `Brep`-versus-`Mesh` split resolves once at the factory, no read re-tests the host type, and the case stays value-comparable — a captured `Func` compares by reference, so two `Region` values over one `Brep` would never key, hash, or memoize alike. `Sheet` exists precisely to keep an open `Brep`/`Mesh` from falling through to a signed answer reporting a containment its geometry cannot decide.
- Law: the case is decided ONCE, at `Of`. Every read after — `SignedReach`, `ContainReach`, `Closest`, `ContainmentDistance` — is a generated total `Switch` over that case, so the runtime type is never probed twice and no arm can drift from another.
- Entry: `Of(object?, Op?)` is the ONE admission — a `ClusterCase` admits by construction (`cloud.md`'s factory proved its vertices, dedup, and mass); any other candidate admits by `Capability.Closest.Admits` on the runtime type (`object` roots refused) and the `OpAcceptance.ValidityOf` oracle (`Domain/validation.md`, whose compiled-lambda table covers the value structs), then routes to its regime and captures its capability set.
- Auto: `Closest` dispatches the cluster arm to `ClusterCase.ClosestVertex` and every other regime to `geometry.Evaluate(new EvaluationRequest.Closest(target), key)`, projecting the returned `EvaluationResult` through its own `Project<ClosestHit>`; `SignedDistance` composes `EvaluationRequest.Signed(sample)` the same way; `ContainmentDistance` signs the hit distance on `Region` through its captured probe and refuses on `Sheet`.
- Packages: RhinoCommon (`Brep.IsPointInside`/`Mesh.IsPointInside`/`Brep.IsSolid`/`Mesh.IsSolid`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a newly closest-capable Rhino kind is one `normalization.md` capability-row membership, changing zero lines here; a new proximity regime is one case with its arms; a new analytic species is one `AnalyticShape` case and one `Of` arm; a new solid containment host is one `ContainProbe` row and one `Of` arm.
- Boundary: `SupportSpace` is the ONE proximity adapter, and `Domain/evaluation`'s landed ingress is `extension(object? geometry)`, so every regime hands it the boxed payload and the case NAME carries the regime the payload's type never carries. NAMED LOSS on `SignedDistance`: `EvaluationRequest.Signed` re-solves the closest hit inside `evaluation.md`, so the caller's already-computed `ClosestHit` is not reused and a signed read costs one extra closest solve — the price of one evaluation ingress instead of two, paid where the hit is cheap and the second entrypoint was not. Its cluster arm composes `cloud.md`'s indexed closest-vertex probe; a second `PointCloud` index minted here doubles the `ClusterCase` cache. Admission runs once and crosses pages, so no read re-validates the factory-proven payload.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ContainProbe {
    public static readonly ContainProbe Brep = new(key: 0,
        inside: static (solid, sample, tolerance) => solid is Rhino.Geometry.Brep shell && shell.IsPointInside(sample, tolerance, strictlyIn: false));
    public static readonly ContainProbe Mesh = new(key: 1,
        inside: static (solid, sample, tolerance) => solid is Rhino.Geometry.Mesh shell && shell.IsPointInside(sample, tolerance, strictlyIn: false));

    [UseDelegateFromConstructor] internal partial bool Inside(GeometryBase solid, Point3d sample, double tolerance);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalyticShape {
    private AnalyticShape() { }

    public sealed record PlaneCase(Plane Value) : AnalyticShape;
    public sealed record SphereCase(Sphere Value) : AnalyticShape;
    public sealed record BoxCase(Box Value) : AnalyticShape;
    public sealed record BoundCase(BoundingBox Value) : AnalyticShape;

    internal static Option<AnalyticShape> Of(object source) => source switch {
        Plane plane => Some((AnalyticShape)new PlaneCase(Value: plane)),
        Sphere sphere => Some((AnalyticShape)new SphereCase(Value: sphere)),
        Box box => Some((AnalyticShape)new BoxCase(Value: box)),
        BoundingBox bound => Some((AnalyticShape)new BoundCase(Value: bound)),
        _ => Option<AnalyticShape>.None,
    };

    internal object Payload => Switch(
        planeCase: static p => (object)p.Value,
        sphereCase: static s => s.Value,
        boxCase: static b => b.Value,
        boundCase: static b => b.Value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record SupportSpace {
    private SupportSpace() { }

    public sealed record Cluster(VectorCloud.ClusterCase Value) : SupportSpace;
    public sealed record Analytic(AnalyticShape Shape) : SupportSpace;
    public sealed record Region(GeometryBase Value, ContainProbe Probe, CapabilitySet<SupportCapability> Held) : SupportSpace;
    public sealed record Sheet(GeometryBase Value, CapabilitySet<SupportCapability> Held) : SupportSpace;
    public sealed record Form(object Value, CapabilitySet<SupportCapability> Held) : SupportSpace;

    public static Fin<SupportSpace> Of(object? value, Op? key = null) {
        Op op = key.OrDefault();
        return value switch {
            VectorCloud.ClusterCase cluster => Fin.Succ((SupportSpace)new Cluster(Value: cluster)),
            _ => from source in Optional(value).ToFin(op.InvalidInput())
                 let type = source.GetType()
                 from _ in guard(type != typeof(object) && Capability.Closest.Admits(type: type), op.Unsupported(type, typeof(ClosestHit)))
                 from valid in OpAcceptance.ValidityOf(source: source).Filter(static ok => ok).ToFin(op.InvalidInput())
                 let held = SupportCapability.Of(source: type)
                 select AnalyticShape.Of(source: source).Match(
                     Some: shape => (SupportSpace)new Analytic(Shape: shape),
                     None: () => source switch {
                         Brep { IsSolid: true } brep => new Region(Value: brep, Probe: ContainProbe.Brep, Held: held),
                         Mesh { IsSolid: true } mesh => new Region(Value: mesh, Probe: ContainProbe.Mesh, Held: held),
                         Brep or Mesh => new Sheet(Value: (GeometryBase)source, Held: held),
                         _ => new Form(Value: source, Held: held),
                     }),
        };
    }

    internal object Payload => Switch(
        cluster: static c => (object)c.Value,
        analytic: static a => a.Shape.Payload,
        region: static r => r.Value,
        sheet: static s => s.Value,
        form: static f => f.Value);

    internal Type SourceType => Payload.GetType();

    internal CapabilitySet<SupportCapability> Capabilities => Switch(
        cluster: static _ => CapabilitySet<SupportCapability>.None,
        analytic: static _ => CapabilitySet<SupportCapability>.Of(SupportCapability.Signed),
        region: static r => r.Held,
        sheet: static s => s.Held,
        form: static f => f.Held);

    internal bool SignedReach(ClosestHit hit) => Switch(
        state: hit,
        cluster: static (_, _) => false,
        analytic: static (probe, _) => probe.Distance.IsSome,
        region: static (probe, _) => probe.Normal.IsSome,
        sheet: static (probe, _) => probe.Normal.IsSome,
        form: static (probe, _) => probe.Normal.IsSome);

    internal bool ContainReach(ClosestHit hit) => Switch(
        state: hit,
        cluster: static (_, _) => false,
        analytic: static (probe, _) => probe.Distance.IsSome,
        region: static (probe, _) => probe.Distance.IsSome,
        sheet: static (_, _) => false,
        form: static (probe, _) => probe.Normal.IsSome);

    internal Fin<ClosestHit> Closest(Point3d sample, Op key) => SwitchPartially(
        state: (Sample: sample, Key: key),
        @default: static (s, space) => space.Payload
            .Evaluate(request: new EvaluationRequest.Closest(Target: s.Sample), key: s.Key)
            .Bind(result => result.Project<ClosestHit>(key: s.Key)),
        cluster: static (s, c) => c.Value.ClosestVertex(sample: s.Sample, key: s.Key));

    internal Fin<double> SignedDistance(Point3d sample, Op key) =>
        Payload.Evaluate(request: new EvaluationRequest.Signed(Sample: sample), key: key)
            .Bind(result => result.Project<double>(key: key));

    internal Fin<double> ContainmentDistance(ClosestHit hit, Point3d sample, Context context, Op key) => SwitchPartially(
        state: (Hit: hit, Sample: sample, Context: context, Key: key),
        @default: static (s, space) => space.SignedDistance(sample: s.Sample, key: s.Key),
        region: static (s, r) => s.Hit.Distance.ToFin(Fail: s.Key.InvalidResult())
            .Map(d => (r.Probe.Inside(solid: r.Value, sample: s.Sample,
                tolerance: s.Context.For(lane: ToleranceLane.Closure).Value) ? -1.0 : 1.0) * d),
        sheet: static (s, _) => Fin.Fail<double>(error: s.Key.InvalidInput()));
}
```

## [04]-[SUPPORT_PROJECTION]

- Owner: `SupportProjection` `[SmartEnum<int>]` mints one row per closest-hit modality, each row three columns — `Requires` (the `CapabilitySet<SupportCapability>` the space must hold whole, empty where the hit alone answers), `Accepts` (which `TOut` shapes it projects), `Reach` (the hit-shape gate) — and one internal `Project<TOut>` is the sole egress.
- Cases: the roster's ONE upstream is `ClosestHit`'s own facet set at `Domain/evaluation#CLOSEST_HIT` — one row per `Option` facet, with the two signed evaluations `SupportSpace` owns and the two span senses a displacement carries. `Requires` is a per-row DECLARATION naming a capability SET, never a derivation from one: the capability vocabulary answers what a SPACE holds, the facet set what a HIT carries, and the row pairs them — so a modality wanting a signed frame states both rows and the gate stays one `AdmitsAll`. `ClosestHit`-field rows lift one facet through the shared `HitValue<T>` builder; span rows fold the sample→hit displacement with sign ±1 a factory parameter, so one `SpanOf(key, sign)` builder mints `Span` and `SignedSpanAway`.
- Entry: `Project<TOut>` is a three-gate switch — hit validity, admission, output shape, in evidence order so a fault names the first real refusal; `ProjectRaw` then yields a canonical value the egress resolves, a canonical carrier (`ClosestHit`/`Direction`/`VectorSpan`) delegating to its own `Project<TOut>`.
- Auto: `Admits` is ONE body over the two columns — `AdmitsAll` of the row's whole requirement against the space's set, then the row's own hit-shape reach — so the fourteen rows carry no per-row capability lambda between them. `CanProjectVector` DERIVES from `Accepts(typeof(Vector3d))` and the same set read, so the hand disjunction over five row identities is gone and a new vector-valued row joins it unasked. `Direction`-family rows admit through `Direction.Of` so a degenerate direction faults at the atom, and span rows read the `Vector3d`/`double` answers raw.
- Packages: RhinoCommon (`Point3d`/`Vector3d`/`Plane`/`Line`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new proximity modality is one `static readonly` row with its three columns; a new output shape is one row on the canonical carrier's `Project<TOut>`, picked up by the egress unchanged; zero new entrypoints.
- Boundary: raw→typed resolves once at the egress by delegating to the canonical owners' `Project<TOut>`; the capability half of every gate reads the space's admission-captured set, never re-deriving the `normalization.md` rows per call.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = Hit(key: 0,
        accepts: static output => output == typeof(Point3d) || output == typeof(ClosestHit),
        projectRaw: static s => Fin.Succ<object>(s.Hit));
    public static readonly SupportProjection Direction = Hit(key: 1, accepts: DirectionOrVector,
        projectRaw: static s => DirectionOf(vector: s.Hit.Point - s.Sample, state: s));
    public static readonly SupportProjection Span = SpanOf(key: 2, sign: +1.0);
    public static readonly SupportProjection Normal = Hit(key: 3, accepts: DirectionOrVector,
        requires: CapabilitySet<SupportCapability>.Of(SupportCapability.Normal),
        projectRaw: static s => s.Hit.Normal.ToFin(Fail: s.Key.InvalidResult()).Bind(n => DirectionOf(vector: n, state: s)));
    public static readonly SupportProjection Distance = HitValue(key: 4, choose: static hit => hit.Distance);
    public static readonly SupportProjection Parameter = HitValue(key: 5, choose: static hit => hit.Parameter);
    public static readonly SupportProjection Uv = HitValue(key: 6, choose: static hit => hit.Uv);
    public static readonly SupportProjection Component = HitValue(key: 7, choose: static hit => hit.Component);
    public static readonly SupportProjection MeshPoint = HitValue(key: 8, choose: static hit => hit.MeshPoint);
    public static readonly SupportProjection SignedDistance = Hit(key: 9,
        accepts: static output => output == typeof(double), requires: CapabilitySet<SupportCapability>.Of(SupportCapability.Signed),
        reach: Some<Func<SupportSpace, ClosestHit, bool>>(static (space, hit) => space.SignedReach(hit: hit)),
        projectRaw: static s => s.Space.SignedDistance(sample: s.Sample, key: s.Key).Map(static d => (object)d));
    public static readonly SupportProjection ContainmentDistance = Hit(key: 10,
        accepts: static output => output == typeof(double), requires: CapabilitySet<SupportCapability>.Of(SupportCapability.Signed),
        reach: Some<Func<SupportSpace, ClosestHit, bool>>(static (space, hit) => space.ContainReach(hit: hit)),
        projectRaw: static s => s.Space.ContainmentDistance(hit: s.Hit, sample: s.Sample, context: s.Context, key: s.Key).Map(static d => (object)d));
    public static readonly SupportProjection Tangent = Hit(key: 11, accepts: DirectionOrVector,
        requires: CapabilitySet<SupportCapability>.Of(SupportCapability.Tangent),
        projectRaw: static s => s.Hit.Tangent.ToFin(Fail: s.Key.InvalidResult()).Bind(t => DirectionOf(vector: t, state: s)));
    public static readonly SupportProjection Frame = HitValue(key: 12, choose: static hit => hit.Frame,
        requires: CapabilitySet<SupportCapability>.Of(SupportCapability.Frame));
    public static readonly SupportProjection SignedSpanAway = SpanOf(key: 13, sign: -1.0);

    private CapabilitySet<SupportCapability> Requires { get; }
    [UseDelegateFromConstructor] private partial bool Accepts(Type output);
    [UseDelegateFromConstructor] private partial bool Reach(SupportSpace space, ClosestHit hit);
    [UseDelegateFromConstructor] private partial Fin<object> ProjectRaw(SupportState state);

    private readonly record struct SupportState(SupportSpace Space, ClosestHit Hit, Point3d Sample, Context Context, Op Key, Type Output);

    private bool Admits(SupportSpace space, ClosestHit hit) =>
        space.Capabilities.AdmitsAll(required: Requires) && Reach(space: space, hit: hit);

    private static SupportProjection Hit(int key, Func<Type, bool> accepts, Func<SupportState, Fin<object>> projectRaw,
        CapabilitySet<SupportCapability>? requires = null, Option<Func<SupportSpace, ClosestHit, bool>> reach = default) =>
        new(key: key, requires: requires ?? CapabilitySet<SupportCapability>.None, accepts: accepts,
            reach: reach.IfNone(static () => new Func<SupportSpace, ClosestHit, bool>(static (_, _) => true)), projectRaw: projectRaw);
    private static SupportProjection HitValue<T>(int key, Func<ClosestHit, Option<T>> choose, CapabilitySet<SupportCapability>? requires = null) where T : notnull =>
        Hit(key: key, accepts: static output => output == typeof(T), requires: requires,
            projectRaw: state => choose(state.Hit).ToFin(Fail: state.Key.InvalidResult())
                .Bind(value => state.Key.AcceptValue(value: value).Map(static accepted => (object)accepted)));
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
        Accepts(output: typeof(Vector3d)) && space.Capabilities.AdmitsAll(required: Requires);

    internal Fin<TOut> Project<TOut>(SupportSpace space, ClosestHit hit, Point3d sample, Context context, Op key) =>
        (hit.IsValid, Admits(space: space, hit: hit), Accepts(output: typeof(TOut))) switch {
            (false, _, _) => Fin.Fail<TOut>(error: key.InvalidResult()),
            (_, false, _) => Fin.Fail<TOut>(error: key.Unsupported(inputType: space.SourceType, outputType: typeof(TOut))),
            (_, _, false) => Fin.Fail<TOut>(error: key.Unsupported(inputType: typeof(SupportProjection), outputType: typeof(TOut))),
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

## [05]-[DENSITY_BAR]

One owner per axis; each `[RESULT]` cell names the owner's return type and `[CASES]` its bounded-vocabulary count.

| [INDEX] | [AXIS_CONCERN]       | [OWNER]             | [KIND]                | [RESULT]                                              | [CASES] |
| :-----: | :------------------- | :------------------ | :-------------------- | :---------------------------------------------------- | :-----: |
|  [01]   | Proximity capability | `SupportCapability` | `[SmartEnum<string>]` | `Of → CapabilitySet<SupportCapability>`               |    4    |
|  [02]   | Analytic species     | `AnalyticShape`     | `[Union]`             | `Of → Option<AnalyticShape>`; `Payload → object`      |    4    |
|  [03]   | Containment probe    | `ContainProbe`      | `[SmartEnum<int>]`    | `Inside → bool`                                       |    2    |
|  [04]   | Proximity regime     | `SupportSpace`      | `[Union]`             | `Of → Fin<SupportSpace>`; `Closest → Fin<ClosestHit>` |    5    |
|  [05]   | Closest-hit output   | `SupportProjection` | `[SmartEnum<int>]`    | `Project<TOut> → Fin<TOut>`                           |   14    |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
