# [RASM_SUPPORT]

`SupportSpace` and `SupportProjection` own the corpus proximity gate: one closed `[Union]` over every closest-point-capable host shape, discriminated ONCE at admission by the proximity regime its reads need, and one `[SmartEnum]` owning the closest-hit output modalities behind a single capability-gated `Project<TOut>`. Each new proximity answer is one vocabulary row carrying its required capability and its projection. Every proximity read in the corpus routes through this gate.

This page composes settled `Domain` vocabulary: `evaluation.md` owns `ClosestHit`, the `EvaluationRequest` algebra this adapter drives, and the one erased `extension(object? geometry) { Evaluate<TOut>(request, key) }` typed ingress that algebra publishes; `normalization.md` owns the `Capability` admission rows whose verdicts gate admission once and every projection after; `validation.md` owns `ICapability`/`CapabilitySet` and `atoms.md` the `ResultProjection` raw→typed fold its canonical carriers (`ClosestHit`, `Direction`, `VectorSpan`) project through. Parametric `(u,v)` evaluation homes at `Parametric/projections.md`; this page owns proximity alone.

## [01]-[INDEX]

- [02]-[SUPPORT_SPACE]: `SupportSpace` `[Union]` over the proximity regimes; admission, capability capture, closest, signed-distance, containment-distance.
- [03]-[SUPPORT_PROJECTION]: `SupportProjection`'s capability-gated closest-hit projections behind one `Project<TOut>`; canonical-owner egress resolution.
- [04]-[DENSITY_BAR]: one owner per axis.

## [02]-[SUPPORT_SPACE]

- Owner: `SupportSpace` is the ONE proximity handle over any closest-point-capable host shape; its cases are the proximity REGIMES the page's reads discriminate — `Cluster` a point set with no surface, `Analytic` a closed analytic region whose signed distance is exact and total, and `Native` the open closest-capable host set `normalization.md` governs, carrying the capability set admission captured.
- Cases: `Analytic` admits a CLOSED set — `Plane`, `Sphere`, `Box`, `BoundingBox` — through one type-pattern arm at `Of` and carries the admitted payload as `object`, because no read discriminates the species after admission: every regime hands the boxed payload to the one `Evaluate` ingress, so the case NAME is the whole distinction and a species union would re-box on every read. `Native` keeps `object` because `Capability.Closest` is an open predicate over types `normalization.md` widens without editing this page; a solid `Brep`/`Mesh`, an open shell, and every other closest-capable kind share identity, admission, payload timing, capability storage, closest evaluation, and signed evaluation, so they are ONE case, and the `IsSolid` split lives inside the sole read that consumes it — `ContainmentDistance` — where an open `Brep`/`Mesh` refuses the containment its geometry cannot decide instead of falling through to a signed answer.
- Law: the case is decided ONCE, at `Of`. Every read after — `SignedReach`, `Closest`, `ContainmentDistance` — is a generated `Switch` over that case (`Closest` the `SwitchPartially` naming its cluster arm and `@default`), so no arm can drift from another; the one host-type re-test is `ContainmentDistance`'s native arm, the sole read the solid-versus-shell split decides.
- Entry: `Of(object?)` is the ONE admission — a `ClusterCase` admits by construction (`cloud.md`'s factory proved its vertices, dedup, and mass); any other candidate admits by `Capability.Closest.Admits` on the runtime type (`object` roots refused) and the `Acceptance.ValidityOf` oracle (`Domain/validation.md`, whose compiled-lambda table covers the value structs), then routes to its regime and captures its capability set. `Held` is the `normalization.md` `Capability` rows admitting the runtime type, folded ONCE from `Capability.Items` into one `CapabilitySet<Capability>` — a SET, never a tuple of booleans — inside the `Native` arm alone, the one regime that reads it, and handed to `Native`'s `internal` constructor, the sole door minting that case, so a native space cannot exist without its set; every projection gate after is one `AdmitsAll` membership test against it, never a re-derivation from `SourceType`, and a new proximity answer is one `normalization.md` row every space widens by with no arm edited.
- Auto: `Closest` dispatches the cluster arm to `ClusterCase.ClosestVertex` and every other regime to `geometry.Evaluate<ClosestHit>(new EvaluationRequest.Closest(target), key)`, the typed egress landing the hit directly; `SignedDistance` composes `EvaluationRequest.Signed(sample)` the same way; `ContainmentDistance` signs the hit distance on a solid `Brep`/`Mesh` through `IsPointInside`, refuses an open shell, and routes every other native kind to the signed evaluation behind a normal guard.
- Packages: RhinoCommon (`Brep.IsPointInside`/`Mesh.IsPointInside`/`Brep.IsSolid`/`Mesh.IsSolid`), LanguageExt.Core, Thinktecture.Runtime.Extensions, `Rasm.Domain` (`Capability`, `CapabilitySet`).
- Growth: a newly closest-capable Rhino kind is one `normalization.md` capability-row membership, changing zero lines here; a new proximity regime is one case with its arms; a new analytic species is one type in the `Analytic` admission pattern; a new solid containment host is one pattern arm inside `ContainmentDistance`'s native arm.
- Boundary: `SupportSpace` is the ONE proximity adapter, and `Domain/evaluation`'s landed ingress is `extension(object? geometry)`, so every regime hands it the boxed payload and the case NAME carries the regime the payload's type never carries. NAMED LOSS on `SignedDistance`: `EvaluationRequest.Signed` re-solves the closest hit inside `evaluation.md`, so the caller's already-computed `ClosestHit` is not reused and a signed read costs one extra closest solve — the price of one evaluation ingress instead of two, paid where the hit is cheap and the second entrypoint was not. Its cluster arm composes `cloud.md`'s indexed closest-vertex probe; a second `PointCloud` index minted here doubles the `ClusterCase` cache. Admission runs once and crosses pages, so no read re-validates the factory-proven payload.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Thinktecture;

namespace Rasm.Spatial;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record SupportSpace {
    private SupportSpace() { }

    public sealed record Cluster(VectorCloud.ClusterCase Value) : SupportSpace;
    public sealed record Analytic(object Value) : SupportSpace;
    public sealed record Native : SupportSpace {
        internal Native(object value, CapabilitySet<Capability> held) { Value = value; Held = held; }
        public object Value { get; }
        internal CapabilitySet<Capability> Held { get; }
    }

    public static Fin<SupportSpace> Of(object? value) {
        return value switch {
            VectorCloud.ClusterCase cluster => Fin.Succ((SupportSpace)new Cluster(Value: cluster)),
            _ => from source in Optional(value).ToFin(new KernelFault.InvalidInput())
                 let type = source.GetType()
                 from _ in guard(type != typeof(object) && Capability.Closest.Admits(type: type), new KernelFault.Unsupported(type, typeof(ClosestHit)))
                 from valid in Acceptance.ValidityOf(source: source).Filter(static ok => ok).ToFin(new KernelFault.InvalidInput())
                 select source switch {
                     Plane or Sphere or Box or BoundingBox => (SupportSpace)new Analytic(Value: source),
                     _ => new Native(source, CapabilitySet<Capability>.Of([.. Capability.Items.Where(row => row.Admits(type: type))])),
                 },
        };
    }

    internal object Payload => Switch(
        cluster: static c => (object)c.Value,
        analytic: static a => a.Value,
        native: static n => n.Value);

    internal Type SourceType => Payload.GetType();

    internal CapabilitySet<Capability> Capabilities => Switch(
        cluster: static _ => CapabilitySet<Capability>.None,
        analytic: static _ => CapabilitySet<Capability>.Of(Capability.SignedDistance),
        native: static n => n.Held);

    internal bool SignedReach(ClosestHit hit) => Switch(
        state: hit,
        cluster: static (_, _) => false,
        analytic: static (probe, _) => probe.Distance.IsSome,
        native: static (probe, _) => probe.Normal.IsSome);

    internal Fin<ClosestHit> Closest(Point3d sample) => SwitchPartially(
        state: sample,
        @default: static (s, space) => space.Payload
            .Evaluate<ClosestHit>(request: new EvaluationRequest.Closest(Target: s)),
        cluster: static (s, c) => c.Value.ClosestVertex(sample: s));

    internal Fin<double> SignedDistance(Point3d sample) =>
        Payload.Evaluate<double>(request: new EvaluationRequest.Signed(Sample: sample));

    internal Fin<double> ContainmentDistance(ClosestHit hit, Point3d sample, Context context) => Switch(
        state: (Hit: hit, Sample: sample, Context: context),
        cluster: static (s, c) => Fin.Fail<double>(new KernelFault.Unsupported(c.Value.GetType(), typeof(double))),
        analytic: static (s, a) => a.Value.Evaluate<double>(new EvaluationRequest.Signed(s.Sample), s.Key),
        native: static (s, n) => n.Value switch {
            Brep or Mesh => (n.Value switch {
                Brep { IsSolid: true } b => Some(b.IsPointInside(s.Sample, s.Context.For(ToleranceLane.Closure).Value, strictlyIn: false)),
                Mesh { IsSolid: true } m => Some(m.IsPointInside(s.Sample, s.Context.For(ToleranceLane.Closure).Value, strictlyIn: false)),
                _ => Option<bool>.None,
            }).ToFin(new KernelFault.Unsupported(n.Value.GetType(), typeof(double)))
                .Bind(inside => s.Hit.Distance.ToFin(new KernelFault.InvalidResult()).Map(d => (inside ? -1.0 : 1.0) * d)),
            _ => guard(s.Hit.Normal.IsSome, new KernelFault.Unsupported(n.Value.GetType(), typeof(double))).ToFin()
                >> n.Value.Evaluate<double>(new EvaluationRequest.Signed(s.Sample), s.Key),
        });
}
```

## [03]-[SUPPORT_PROJECTION]

- Owner: `SupportProjection` `[SmartEnum]` mints one row per closest-hit modality, each row two gate columns — `Requires` (the `CapabilitySet<Capability>` the space must hold whole, empty where the hit alone answers) and `Accepts` (which `TOut` shapes it projects) — beside its `ProjectRaw` body, and one internal `Project<TOut>` is the sole egress.
- Cases: the roster's ONE upstream is `ClosestHit`'s own facet set at `Domain/evaluation#CLOSEST_HIT` — one row per `Option` facet, with the two signed evaluations `SupportSpace` owns and the two span senses a displacement carries. `Requires` is a per-row DECLARATION naming a capability SET, never a derivation from one: the capability vocabulary answers what a SPACE holds, the facet set what a HIT carries, and the row pairs them — so a modality wanting a signed frame states both rows and the gate stays one `AdmitsAll`. `ClosestHit`-field rows lift one facet through the shared `HitValue<T>` builder; span rows fold the sample→hit displacement with sign ±1 a factory parameter, so one `SpanOf(sign)` builder mints `Span` and `SignedSpanAway`.
- Entry: `Project<TOut>` is a three-gate switch — hit validity, admission, output shape, in evidence order so a fault names the first real refusal; `ProjectRaw` then yields a canonical value the egress resolves, a canonical carrier (`ClosestHit`/`Direction`/`VectorSpan`) delegating to its own `Project<TOut>`.
- Auto: `Admits` is ONE `AdmitsAll` of the row's whole requirement against the space's set, so the rows carry no per-row capability lambda between them; the two hit-dependent rows carry their evidence gate inside their own body — `SignedDistance` guards `SignedReach` ahead of the evaluation, `ContainmentDistance` dispatches the regime whole — rather than filling a hit-gate column every other row would allocate an always-true delegate for. `Accepts<TOut>()` publishes the output-shape column alone — the gate a `Parametric/locate` builder reads before any space exists — and `CanProject<TOut>` DERIVES from it and the same `Admits` read, so a consumer asks the row whether it projects an output shape instead of mirroring row identities, and a new row projecting that shape joins both unasked. `Direction`-family rows admit through `Direction.Of` so a degenerate direction faults at the atom, and span rows read the `Vector3d`/`double` answers raw.
- Packages: RhinoCommon (`Point3d`/`Vector3d`/`Plane`/`Line`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new proximity modality is one `static readonly` row with its two gate columns and body; a new output shape is one row on the canonical carrier's `Project<TOut>`, picked up by the egress unchanged; zero new entrypoints.
- Boundary: raw→typed resolves once at the egress by delegating to the canonical owners' `Project<TOut>`; the capability half of every gate reads the space's admission-captured set, never re-deriving the `normalization.md` rows per call.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = Hit(
        accepts: static output => output == typeof(Point3d) || output == typeof(ClosestHit),
        projectRaw: static s => Fin.Succ<object>(s.Hit));
    public static readonly SupportProjection Direction = Hit(accepts: DirectionOrVector,
        projectRaw: static s => DirectionOf(vector: s.Hit.Point - s.Sample, context: s.Context));
    public static readonly SupportProjection Span = SpanOf(sign: +1.0);
    public static readonly SupportProjection Normal = Hit(accepts: DirectionOrVector,
        requires: CapabilitySet<Capability>.Of(Capability.ClosestNormal),
        projectRaw: static s => s.Hit.Normal.ToFin(Fail: new KernelFault.InvalidResult()).Bind(n => DirectionOf(vector: n, context: s.Context)));
    public static readonly SupportProjection Distance = HitValue(choose: static hit => hit.Distance);
    public static readonly SupportProjection Parameter = HitValue(choose: static hit => hit.Parameter);
    public static readonly SupportProjection Uv = HitValue(choose: static hit => hit.Uv);
    public static readonly SupportProjection Component = HitValue(choose: static hit => hit.Component);
    public static readonly SupportProjection MeshPoint = HitValue(choose: static hit => hit.MeshPoint);
    public static readonly SupportProjection SignedDistance = Hit(
        accepts: static output => output == typeof(double), requires: CapabilitySet<Capability>.Of(Capability.SignedDistance),
        projectRaw: static s => guard(s.Space.SignedReach(hit: s.Hit), new KernelFault.Unsupported(s.Space.SourceType, typeof(double))).ToFin()
            >> s.Space.SignedDistance(s.Sample, s.Key).Map(static d => (object)d));
    public static readonly SupportProjection ContainmentDistance = Hit(
        accepts: static output => output == typeof(double), requires: CapabilitySet<Capability>.Of(Capability.SignedDistance),
        projectRaw: static s => s.Space.ContainmentDistance(s.Hit, s.Sample, s.Context, s.Key).Map(static d => (object)d));
    public static readonly SupportProjection Tangent = Hit(accepts: DirectionOrVector,
        requires: CapabilitySet<Capability>.Of(Capability.ClosestTangent),
        projectRaw: static s => s.Hit.Tangent.ToFin(Fail: new KernelFault.InvalidResult()).Bind(t => DirectionOf(vector: t, context: s.Context)));
    public static readonly SupportProjection Frame = HitValue(choose: static hit => hit.Frame,
        requires: CapabilitySet<Capability>.Of(Capability.ClosestFrame));
    public static readonly SupportProjection SignedSpanAway = SpanOf(sign: -1.0);

    private CapabilitySet<Capability> Requires { get; }
    [UseDelegateFromConstructor] private partial bool Accepts(Type output);
    [UseDelegateFromConstructor] private partial Fin<object> ProjectRaw((SupportSpace Space, ClosestHit Hit, Point3d Sample, Context Context, Type Output) state);

    private bool Admits(SupportSpace space) => space.Capabilities.AdmitsAll(required: Requires);

    private static SupportProjection Hit(Func<Type, bool> accepts, Func<(SupportSpace Space, ClosestHit Hit, Point3d Sample, Context Context, Type Output), Fin<object>> projectRaw,
        CapabilitySet<Capability>? requires = null) =>
        new(requires: requires ?? CapabilitySet<Capability>.None, accepts: accepts, projectRaw: projectRaw);
    private static SupportProjection HitValue<T>(Func<ClosestHit, Option<T>> choose, CapabilitySet<Capability>? requires = null) where T : notnull =>
        Hit(accepts: static output => output == typeof(T), requires: requires,
            projectRaw: state => choose(state.Hit).ToFin(Fail: new KernelFault.InvalidResult())
                .Bind(value => Acceptance.Value(value: value).Map(static accepted => (object)accepted)));
    private static SupportProjection SpanOf(double sign) =>
        Hit(
            accepts: static output => output == typeof(VectorSpan) || output == typeof(Vector3d) || output == typeof(Line) || output == typeof(double),
            projectRaw: state => state.Output switch {
                Type t when t == typeof(double) => Acceptance.Value(value: sign * (state.Hit.Point - state.Sample).Length).Map(static d => (object)d),
                Type t when t == typeof(Vector3d) => Acceptance.Value(value: sign * (state.Hit.Point - state.Sample)).Map(static v => (object)v),
                _ => VectorSpan.Of(anchor: state.Sample, vector: sign * (state.Hit.Point - state.Sample), context: state.Context)
                    .Map(static span => (object)span),
            });

    internal bool Accepts<TOut>() => Accepts(output: typeof(TOut));
    internal bool CanProject<TOut>(SupportSpace space) => Accepts<TOut>() && Admits(space: space);

    internal Fin<TOut> Project<TOut>(SupportSpace space, ClosestHit hit, Point3d sample, Context context) =>
        (hit.IsValid, Admits(space: space), Accepts(output: typeof(TOut))) switch {
            (false, _, _) => Fin.Fail<TOut>(error: new KernelFault.InvalidResult()),
            (_, false, _) => Fin.Fail<TOut>(error: new KernelFault.Unsupported(InputType: space.SourceType, OutputType: typeof(TOut))),
            (_, _, false) => Fin.Fail<TOut>(error: new KernelFault.Unsupported(InputType: typeof(SupportProjection), OutputType: typeof(TOut))),
            _ => ProjectRaw(state: (Space: space, Hit: hit, Sample: sample, Context: context, Output: typeof(TOut)))
                .Bind(value => value switch {
                    TOut output => Fin.Succ(output),
                    ClosestHit owner => owner.Project<TOut>(),
                    Numerics.Direction owner => owner.Project<TOut>(),
                    VectorSpan owner => owner.Project<TOut>(),
                    _ => Fin.Fail<TOut>(error: new KernelFault.InvalidResult()),
                }),
        };

    private static bool DirectionOrVector(Type output) => output == typeof(Direction) || output == typeof(Vector3d);
    private static Fin<object> DirectionOf(Vector3d vector, Context context) =>
        Numerics.Direction.Of(value: vector, context: context).Map(static direction => (object)direction);
}
```

## [04]-[DENSITY_BAR]

One owner per axis; each `[RESULT]` cell names the owner's return type and `[CASES]` its bounded-vocabulary count.

| [INDEX] | [AXIS_CONCERN]       | [OWNER]             | [KIND]                | [RESULT]                                              | [CASES] |
| :-----: | :------------------- | :------------------ | :-------------------- | :---------------------------------------------------- | :-----: |
|  [01]   | Proximity regime     | `SupportSpace`      | `[Union]`             | `Of → Fin<SupportSpace>`; `Closest → Fin<ClosestHit>` |    3    |
|  [02]   | Closest-hit output   | `SupportProjection` | `[SmartEnum]`         | `Project<TOut> → Fin<TOut>`                           |   14    |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
