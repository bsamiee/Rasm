# [RASM_RHINO_GEOMETRY]

`GeometryHandle` owns retained `GeometryBase` custody from raw crossing through typed observation, motion, bounds, clipping, kernel projection, and release. Every mutation commits a deep working copy, every failed disposal stays in the handle's retry roster until release settles it, and every custody transition is a `Transition<HandleState>` verdict off one atom rather than a field triple under six lock scopes. The clipping algebra seats here: `ClipOp` is the ONE `SetClipParticipation` writer in the folder, `Exchange/sheets` composes it for the document-attached half, and `FieldOverride<T>` — the three-state Keep/Set/Clear override vocabulary the sheets and dial pages read — declares at this Document tier so every composer points down.

## [01]-[INDEX]

- [02]-[CUSTODY]: `GeometryCrc`, `CustodyPosture`, `CrossingMode`, `HandleState`, `HandleRelease`, `GeometryHandle`, `GeometryCrossing` — the crossing policy rows, the one-atom custody state, and the leased handle.
- [03]-[PROGRAM]: `GeometryComparison`, `OpTrait`, `BoundsFidelity`, `BoundsFrame`, `GeometryBounds`, `BoundsEvidence`, `NativeBounds`, `GeometryMotion`, `AppliedMotion`, `TagOp`, `TagResult`, `GeometryOp`, `GeometryResult`, `GeometryTrait`, `GeometryFacts`, `GeometryOutcome` — the single-lease operation family and its typed evidence.
- [04]-[CLIPPING]: `FieldOverride<T>`, `ClipSet`, `ClippingPlaneSeed`, `ClipScope`, `ViewportOp`, `ClipOp`, `ClipState`, `ClipTransition` — clipping-plane scope, depth, viewport, and style transitions over one retained seed.

## [02]-[CUSTODY]

- Owner: `GeometryHandle` owns retained native custody; `HandleState` is the one value carrying the active lease, the losing-lease retry roster, and the release phase, held in ONE `Atom<HandleState>` so a custody read never sees a torn triple; `CustodyPosture` rows carry the mutation admission as their own consequence; `CrossingMode` rows carry the acquisition policy; `GeometryCrc` wraps the host running remainder.
- Entry: `GeometryCrossing.Cross` admits each foreign geometry form through one custody policy and returns the handle; `Apply`, `Compare`, and `Measure` are the three operation gates; `Dispose` retries the whole pending roster and settles release.
- Law: the state triple is ONE atom. Release, retain, and roster transitions compute as pure functions of `HandleState` and land through `Cell.Step`/`Cell.Commit`, so a losing transition reads its `Transition` case instead of assuming a swap; the six lock scopes the field triple demanded collapse to the two host-extent windows below.
- Law: the native EXTENT still serializes under one `Lock` gate — RhinoCommon geometry is not safe under concurrent access and a host call cannot ride a CAS body that retries — so the gate brackets exactly the host window while every state decision rides the atom. Exemption: this gate and the ordinal-ordered dual gate in `Compare` are the platform-forced custody kernel; the ordinal order makes two-handle comparison deadlock-free, and one cell holds the whole state so no multi-cell `atomic` fold is ever needed.
- Law: document-controlled ingress leaves document custody only through a deep copy; copy-on-write shares only non-document material and deepens before mutation.
- Law: `With` lends the native value only for the synchronous extent of the supplied body. Commit and rollback RETAIN each failed losing lease rather than discarding it, `Dispose` retries the complete roster, and every cleanup fault aggregates into the primary through the `Error` monoid — the retain-and-retry posture is this page's refinement of the shared release fold, because a disposal the host refused today may settle on the release path, and the discriminant is stated here.
- Law: mutation admission is the `CustodyPosture` row's own consequence — `Immutable` refuses with a typed fault naming the posture and `Mutable` admits — so no gate re-branches on a `bool Mutable` column and a third posture lands as one row.
- Boundary: `GeometryCrc` and the kernel `ContentHash` are DIFFERENT custodies and neither substitutes for the other. `GeometryCrc` wraps `GeometryBase.DataCRC`, a host-computed running remainder over the native representation: chainable, cheap, in-process, and stable only for the process that computed it, so it answers "did this handle change under me" and nothing else. The kernel `ContentHash` is the federation identity a stored or transported value carries; a `GeometryCrc` persisted or compared across a boundary is the deleted form.
- Growth: a custody policy is one `CrossingMode` behavior row over the same acquisition pipeline; a custody phase is one `HandleRelease` case the pure transitions absorb.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Threading;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<uint>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
[ValidationError]
public readonly partial struct GeometryCrc {
    public static readonly GeometryCrc Zero = Create(value: 0u);

    internal static GeometryCrc Of(GeometryBase geometry) => Create(value: geometry.DataCRC(currentRemainder: Zero));
}

[SmartEnum<int>]
public sealed partial class CustodyPosture {
    public static readonly CustodyPosture Immutable = new(key: 0,
        admit: static op => Fin.Fail<Unit>(error: new KernelFault.InvalidInput(Axis: Some(nameof(CustodyPosture)))));
    public static readonly CustodyPosture Mutable = new(key: 1, admit: static _ => Fin.Succ(value: unit));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> AdmitMutation();
}

[SmartEnum]
public sealed partial class CrossingMode {
    public static readonly CrossingMode Borrow = new(posture: CustodyPosture.Immutable, acquire: Borrowed);
    public static readonly CrossingMode Detach = new(posture: CustodyPosture.Mutable, acquire: Detached);
    public static readonly CrossingMode CopyOnWrite = new(posture: CustodyPosture.Mutable, acquire: Shared);

    public CustodyPosture Posture { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<Lease<GeometryBase>> Acquire(GeometryBase geometry);

    private static Fin<Lease<GeometryBase>> Borrowed(GeometryBase geometry) =>
        geometry.IsDocumentControlled
            ? Fin.Fail<Lease<GeometryBase>>(error: new KernelFault.InvalidInput())
            : Fin.Succ<Lease<GeometryBase>>(value: new Lease<GeometryBase>.Borrowed(Value: geometry));

    private static Fin<Lease<GeometryBase>> Detached(GeometryBase geometry) =>
        Copy(duplicate: geometry.Duplicate);

    private static Fin<Lease<GeometryBase>> Shared(GeometryBase geometry) =>
        Copy(duplicate: geometry.IsDocumentControlled ? geometry.Duplicate : geometry.DuplicateShallow);

    internal static Fin<Lease<GeometryBase>> Copy(Func<GeometryBase> duplicate) =>
        Admit.Need(duplicate)
            .Bind(factory => Try.lift(() => Optional(factory()).ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner))
            .Map(static geometry => (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: geometry));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HandleRelease {
    private HandleRelease() { }
    public sealed record Live : HandleRelease;
    public sealed record Released : HandleRelease;
    public sealed record Faulted(Seq<Error> Errors) : HandleRelease;

    internal bool Active => this is Live;
}

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record HandleState(
    Lease<GeometryBase> Lease,
    Seq<Lease<GeometryBase>> Pending,
    HandleRelease Release);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class GeometryHandle : IDisposable {
    private static long sequence;
    private readonly Lock gate = new();
    private readonly long ordinal = Interlocked.Increment(location: ref sequence);
    private readonly Atom<HandleState> state;
    private readonly CrossingMode mode;

    internal GeometryHandle(Lease<GeometryBase> lease, CrossingMode mode) {
        state = Atom(new HandleState(Lease: lease, Pending: Seq<Lease<GeometryBase>>(), Release: new HandleRelease.Live()));
        this.mode = mode;
    }

    public CrossingMode Mode => mode;
    public HandleRelease Release => state.Value.Release;

    public Fin<GeometryOutcome> Apply(GeometryOp operation) {
        return Admit.Need(operation).Bind(request => Operate(operation: request));
    }

    public Fin<GeometryOutcome> Compare(GeometryHandle other, GeometryComparison policy) {
        return from target in Admit.Need(other)
               from rule in Admit.Need(policy)
               from outcome in Matched(other: target, policy: rule)
               select outcome;
    }

    public Fin<Seq<TOut>> Measure<TOut>(Rasm.Analysis.Bounds request, Context context) where TOut : notnull {
        return from query in Admit.Need(request)
               from domain in Optional(context).ToFin(Fail: new KernelFault.MissingContext())
               from result in With(project: geometry => Analyze.In(context: domain)
                       .Run(
                           operation: Analyze.Query<GeometryBase, TOut>(query: AnalysisQuery.Bounds(query: query), key: op),
                           input: geometry)
                       .ToFin())
               select result;
    }

    public void Dispose() {
        lock (gate) {
            ignore(Cell.Step(
                state,
                held => held.Release is HandleRelease.Released && held.Pending.IsEmpty
                    ? Option<HandleState>.None
                    : Some(Settled(held: held, key: op)),
                new KernelFault.InvalidResult()));
        }
    }

    internal Fin<TResult> With<TResult>(Func<GeometryBase, Fin<TResult>> project) {
        lock (gate) {
            HandleState held = state.Value;
            return held.Release.Active
                ? Admit.Need(project)
                    .Bind(body => Try.lift(() => Acceptance.Input(value: held.Lease.Resource).Bind(body)).Run().Bind(static inner => inner))
                : Fin.Fail<TResult>(error: new KernelFault.InvalidInput());
        }
    }

    private Fin<GeometryOutcome> Operate(GeometryOp operation) =>
        operation.Trait == OpTrait.Mutates
            ? Change(operation: operation)
            : With(project: geometry =>
                from result in Evaluate(geometry: geometry, operation: operation)
                let crc = GeometryCrc.Of(geometry: geometry)
                select new GeometryOutcome(Result: result, Before: crc, After: crc, CleanupFaults: Seq<Error>()));

    private Fin<GeometryOutcome> Matched(GeometryHandle other, GeometryComparison policy) {
        Fin<GeometryOutcome> EvaluateActive() =>
            Try.lift(() => {
                HandleState left = state.Value;
                HandleState right = other.state.Value;
                return !left.Release.Active || !right.Release.Active
                    ? Fin.Fail<GeometryOutcome>(error: new KernelFault.InvalidInput())
                    : from first in Acceptance.Input(value: left.Lease.Resource)
                      from second in Acceptance.Input(value: right.Lease.Resource)
                      let before = GeometryCrc.Of(geometry: first)
                      select new GeometryOutcome(
                          Result: new GeometryResult.Compared(Policy: policy, Equal: policy.Compare(left: first, right: second)),
                          Before: before,
                          After: before,
                          CleanupFaults: Seq<Error>());
            }).Run().Bind(static inner => inner);
        if (ReferenceEquals(this, other)) {
            lock (gate) {
                return EvaluateActive();
            }
        }
        GeometryHandle first = ordinal <= other.ordinal ? this : other;
        GeometryHandle second = ReferenceEquals(first, other) ? this : other;
        lock (first.gate) {
            lock (second.gate) {
                return EvaluateActive();
            }
        }
    }

    private Fin<GeometryOutcome> Change(GeometryOp operation) {
        lock (gate) {
            HandleState held = state.Value;
            if (!held.Release.Active) {
                return Fin.Fail<GeometryOutcome>(error: new KernelFault.InvalidInput());
            }
            return from _ in mode.Posture.AdmitMutation()
                   from prepared in Try.lift(() =>
                       from active in Acceptance.Input(value: held.Lease.Resource)
                       let before = GeometryCrc.Of(geometry: active)
                       from working in CrossingMode.Copy(duplicate: active.Duplicate)
                       select (Working: working, Before: before)).Run().Bind(static inner => inner)
                   from outcome in Try.lift(() => Evaluate(geometry: prepared.Working.Resource, operation: operation)).Run().Bind(static inner => inner).Match(
                       Succ: result => Commit(working: prepared.Working, before: prepared.Before, result: result),
                       Fail: error => Fin.Fail<GeometryOutcome>(error: Retain(candidate: prepared.Working)
                           .Fold(error, static (primary, cleanup) => primary + cleanup)))
                   select outcome;
        }
    }

    private Fin<GeometryOutcome> Commit(Lease<GeometryBase> working, GeometryCrc before, GeometryResult result) =>
        Acceptance.Value(value: working.Resource).Match(
            Succ: admitted => {
                GeometryCrc after = GeometryCrc.Of(geometry: admitted);
                Lease<GeometryBase> previous = state.Value.Lease;
                ignore(Cell.Commit(state, held => held with { Lease = working, Pending = held.Pending.Add(value: previous) }));
                Seq<Error> cleanup = Sweep();
                return Fin.Succ(value: new GeometryOutcome(
                    Result: result,
                    Before: before,
                    After: after,
                    CleanupFaults: cleanup));
            },
            Fail: error => Fin.Fail<GeometryOutcome>(error: Retain(candidate: working)
                .Fold(error, static (primary, cleanup) => primary + cleanup)));

    private Seq<Error> Retain(Lease<GeometryBase> candidate) {
        ignore(Cell.Commit(state, held => held with { Pending = held.Pending.Add(value: candidate) }));
        return Sweep();
    }

    private Seq<Error> Sweep() {
        Seq<(Lease<GeometryBase> Candidate, Fin<Unit> Outcome)> attempts =
            state.Value.Pending.Map(candidate => (Candidate: candidate, Outcome: DisposeLease(lease: candidate)));
        (Seq<(Lease<GeometryBase> Candidate, Fin<Unit> Outcome)> settled, Seq<(Lease<GeometryBase> Candidate, Fin<Unit> Outcome)> refused) =
            attempts.Partition(static attempt => attempt.Outcome.IsSucc);
        ignore(settled);
        ignore(Cell.Commit(state, held => held with { Pending = refused.Map(static attempt => attempt.Candidate) }));
        return refused.Choose(static attempt => attempt.Outcome.Match(
            Succ: static _ => Option<Error>.None,
            Fail: static error => Some(error)));
    }

    private HandleState Settled(HandleState held) {
        Seq<Lease<GeometryBase>> roster = held.Release.Active ? held.Pending.Add(value: held.Lease) : held.Pending;
        Seq<(Lease<GeometryBase> Candidate, Fin<Unit> Outcome)> attempts =
            roster.Map(candidate => (Candidate: candidate, Outcome: DisposeLease(lease: candidate)));
        (Seq<(Lease<GeometryBase> Candidate, Fin<Unit> Outcome)> settled, Seq<(Lease<GeometryBase> Candidate, Fin<Unit> Outcome)> refused) =
            attempts.Partition(static attempt => attempt.Outcome.IsSucc);
        ignore(settled);
        Seq<Error> faults = refused.Choose(static attempt => attempt.Outcome.Match(
            Succ: static _ => Option<Error>.None,
            Fail: static error => Some(error)));
        return held with {
            Pending = refused.Map(static attempt => attempt.Candidate),
            Release = faults.IsEmpty ? new HandleRelease.Released() : new HandleRelease.Faulted(Errors: faults),
        };
    }

    private static Fin<GeometryResult> Evaluate(GeometryBase geometry, GeometryOp operation) =>
        operation.Switch(
            geometry,
            inspect: static (state, _) => Fin.Succ<GeometryResult>(value: new GeometryResult.Facts(Value: GeometryFacts.Of(geometry: state))),
            crc: static (state, request) => Fin.Succ<GeometryResult>(value: new GeometryResult.Hashed(
                Value: GeometryCrc.Create(value: state.DataCRC(currentRemainder: request.Chain)))),
            tag: static (state, request) => Admit.Need(request.Value)
                .Bind(tags => tags.Apply(state))
                .Map(static value => (GeometryResult)new GeometryResult.Tagged(Value: value)),
            transform: static (state, request) => Admit.Need(request.Motion)
                .Bind(motion => motion.Apply(state))
                .Map(static value => (GeometryResult)new GeometryResult.Transformed(Motion: value)),
            bounds: static (state, request) => NativeBounds.Of(state, request.Query)
                .Map(static value => (GeometryResult)new GeometryResult.Bounded(Value: value)),
            clip: static (state, request) => Admit.Need(request.Value)
                .Bind(clip => clip.Apply(state))
                .Map(static value => (GeometryResult)new GeometryResult.Clipped(Value: value)));

    private static Fin<Unit> DisposeLease(Lease<GeometryBase> lease) =>
        Try.lift(() => Fin.Succ(value: lease.Dispose())).Run().Bind(static inner => inner);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GeometryCrossing {
    public static Fin<GeometryHandle> Cross(object source, CrossingMode mode) {
        return from value in Admit.Need(source)
               from custody in Admit.Need(mode)
               from admitted in value is ClippingPlaneSeed seed
                   ? seed.Build().Map(static lease => (Lease: lease, Mode: CrossingMode.Detach))
                   : value.GeometryForm().Bind(form => form.Switch(
                       custody,
                       owned: static (_, owned) => Fin.Succ((Lease: (Lease<GeometryBase>)owned, Mode: CrossingMode.Detach)),
                       borrowed: static (state, borrowed) => state.Acquire(borrowed.Value)
                           .Map(lease => (Lease: lease, Mode: state))))
               select new GeometryHandle(lease: admitted.Lease, mode: admitted.Mode);
    }
}
```

## [03]-[PROGRAM]

- Owner: `GeometryOp` owns the single-lease host-operation family and `GeometryOutcome` preserves its typed result, content-key transition, and every losing-lease cleanup fault; `OpTrait` rows name whether a case observes or mutates so the routing reads one column; `GeometryMotion` admits each motion at its FACTORY under the ambient `Context`, so `Apply` is total on admitted values; `GeometryTrait` is the custody capability vocabulary `GeometryFacts` carries as one set.
- Entry: `GeometryHandle.Apply` discriminates by operation shape; `Compare` takes a second handle under ordinal-ordered dual gates, and `Measure<TOut>` retains the typed kernel projection — both are the operations whose shape cannot inhabit the single-lease closed-result family, so neither is a `GeometryOp` case forcing a dead dispatch arm.
- Law: each case carries only its required evidence. Host-native translation, scale, rotation, and kernel-built transformation occupy one `GeometryMotion` family instead of forcing unrelated operations to carry `Context`, and each motion factory ADMITS its payload once — a finite vector, a direction proved by the kernel claim, a scale factor above the neglect band — so no arm re-guards inside the fold.
- Law: each native motion derives its exact inverse request, while a kernel-built matrix preserves an inverse only when `TryGetInverse` proves one and captures the host decomposition classifications. The three decompositions are the host's OWN parameter vocabulary verbatim — similarity answers `(translation, dilation, rotation)` under a tolerance, rigidity answers `(translation, rotation)` under a tolerance and classifies as `TransformRigidType`, and the four-argument affine answers `(translation, rotation, orthogonal, diagonal)`; renaming a column locally makes `AppliedMotion` claim a factorization the host never computed. Both decomposition tolerances read `context.For(ToleranceLane.ScaleUniformity)` off the Matrix case's own `Context`, so no bare `RhinoMath.ZeroTolerance` survives.
- Law: bounds admit every host-returned `BoundingBox` through the shared result oracle before preserving raw and inflated world, transformed, or framed evidence, including corners, edges, center, diagonal, and inverse motion where the host proves one; the inflation vector admits at the `GeometryBounds` factory, so `BoundsEvidence` re-checks nothing.
- Law: inspection carries native validity and its diagnostic only when invalid; the custody bits ride one `CapabilitySet<GeometryTrait>` rather than two loose bools, and the record folds `IValidityEvidence` off the host's own verdict.
- Law: tag clearing snapshots once, invokes the host's atomic bag clear, and proves the resulting bag empty.
- Exemption: `BoundingBox` copy mutation inside `BoundsEvidence.Of` is the value-struct kernel required by RhinoCommon's `Inflate` surface.
- Boundary: kernel owners construct placement and analysis semantics; this owner applies or observes them inside native custody. Structural equality of the evidence records rides the carriers' own structural `Equals` — `Arr`, `Seq`, and `HashMap` compare by value — so no generated equality attribute stacks here.
- Growth: a host capability is one case and one exhaustive arm inside the existing operation or motion family; a new routing trait is one `OpTrait` row.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class GeometryComparison {
    public static readonly GeometryComparison Reference = new(compare: static (left, right) => ReferenceEquals(left, right));
    public static readonly GeometryComparison Crc = new(compare: static (left, right) =>
        GeometryCrc.Of(geometry: left) == GeometryCrc.Of(geometry: right));

    [UseDelegateFromConstructor]
    internal partial bool Compare(GeometryBase left, GeometryBase right);
}

[SmartEnum<int>]
public sealed partial class OpTrait {
    public static readonly OpTrait Observes = new(key: 0);
    public static readonly OpTrait Mutates = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class BoundsFidelity {
    public static readonly BoundsFidelity Accurate = new(key: 0, host: true);
    public static readonly BoundsFidelity Fast = new(key: 1, host: false);

    internal bool Host { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BoundsFrame {
    private BoundsFrame() { }
    public sealed record AxisAligned(BoundsFidelity Fidelity) : BoundsFrame;
    public sealed record Transformed(TransformSpec Motion, Context Domain) : BoundsFrame;
    public sealed record Oriented(Plane Value) : BoundsFrame;
}

[ComplexValueObject]
[ValidationError]
public sealed partial class GeometryBounds {
    public BoundsFrame Frame { get; }
    public Option<Vector3d> Inflation { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref BoundsFrame frame,
        ref Option<Vector3d> inflation) =>
        validationError = frame is not null
            && inflation.Map(static amount =>
                ValidityClaim.Finite(value: amount).Holds && amount.X >= 0.0 && amount.Y >= 0.0 && amount.Z >= 0.0)
                .IfNone(noneValue: true)
            ? null
            : new ValidationError(message: "Bounds query requires a frame and a finite nonnegative inflation vector.");

    public static Fin<GeometryBounds> Of(BoundsFrame frame, Option<Vector3d> inflation = default) =>
        key.OrDefault().AcceptValidated<GeometryBounds>(fault: Validate(frame, inflation, out GeometryBounds? admitted), value: admitted);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BoundsEvidence(
    BoundingBox Raw,
    BoundingBox Value,
    Point3d Center,
    Vector3d Diagonal,
    Arr<Point3d> Corners,
    Arr<Line> Edges) {
    internal static Fin<BoundsEvidence> Of(BoundingBox value, Option<Vector3d> inflation) =>
        from bounds in Acceptance.Value(value: value)
        from evidence in Try.lift(() => Fin.Succ(value: inflation.Match(
            Some: amount => {
                BoundingBox expanded = bounds;
                expanded.Inflate(xAmount: amount.X, yAmount: amount.Y, zAmount: amount.Z);
                return Capture(raw: bounds, value: expanded);
            },
            None: () => Capture(raw: bounds, value: bounds)))).Run().Bind(static inner => inner)
        select evidence;

    private static BoundsEvidence Capture(BoundingBox raw, BoundingBox value) => new(
        Raw: raw,
        Value: value,
        Center: value.Center,
        Diagonal: value.Diagonal,
        Corners: [.. value.GetCorners()],
        Edges: [.. value.GetEdges()]);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NativeBounds {
    private NativeBounds() { }
    public sealed record World(BoundsEvidence Evidence, BoundsFidelity Fidelity) : NativeBounds;
    public sealed record Moved(BoundsEvidence Evidence, global::Rhino.Geometry.Transform Motion, Option<global::Rhino.Geometry.Transform> Inverse) : NativeBounds;
    public sealed record Framed(BoundsEvidence Local, Box World, Plane Frame) : NativeBounds;

    internal static Fin<NativeBounds> Of(GeometryBase geometry, GeometryBounds query) =>
        from request in Admit.Need(query)
        from frame in Admit.Need(request.Frame)
        from result in frame.Switch(
            (Geometry: geometry, Inflation: request.Inflation),
            axisAligned: static (state, bounds) =>
                from value in Try.lift(() => Fin.Succ(value: state.Geometry.GetBoundingBox(accurate: bounds.Fidelity.Host))).Run().Bind(static inner => inner)
                from evidence in BoundsEvidence.Of(value, state.Inflation)
                select (NativeBounds)new World(Evidence: evidence, Fidelity: bounds.Fidelity),
            transformed: static (state, bounds) =>
                from domain in Optional(bounds.Domain).ToFin(Fail: new KernelFault.MissingContext())
                from spec in Admit.Need(bounds.Motion)
                from motion in Placement.Build(spec: spec, context: Some(domain))
                from value in Try.lift(() => Fin.Succ(value: state.Geometry.GetBoundingBox(xform: motion))).Run().Bind(static inner => inner)
                from evidence in BoundsEvidence.Of(value, state.Inflation)
                let inverse = motion.TryGetInverse(inverse: out global::Rhino.Geometry.Transform reversed)
                    ? Some(reversed)
                    : Option<global::Rhino.Geometry.Transform>.None
                select (NativeBounds)new Moved(Evidence: evidence, Motion: motion, Inverse: inverse),
            oriented: static (state, bounds) =>
                from plane in Admit.Demand(claim: ValidityClaim.All(bounds.Value.IsValid), value: 0, requirement: "a valid plane").Map(_ => bounds.Value)
                from raw in Try.lift(() => {
                    BoundingBox local = state.Geometry.GetBoundingBox(plane: plane, worldBox: out Box world);
                    return Fin.Succ(value: (Local: local, World: world));
                }).Run().Bind(static inner => inner)
                from evidence in BoundsEvidence.Of(raw.Local, state.Inflation)
                select (NativeBounds)new Framed(Local: evidence, World: raw.World, Frame: plane))
        select result;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryMotion {
    private GeometryMotion() { }
    public sealed record Matrix(TransformSpec Value, Context Domain) : GeometryMotion;
    public sealed record Translation : GeometryMotion { internal Translation(Vector3d vector) => Vector = vector; public Vector3d Vector { get; } }
    public sealed record UniformScale : GeometryMotion { internal UniformScale(double factor) => Factor = factor; public double Factor { get; } }
    public sealed record Rotation : GeometryMotion {
        internal Rotation(double angleRadians, Vector3d axis, Point3d center) => (AngleRadians, Axis, Center) = (angleRadians, axis, center);
        public double AngleRadians { get; }
        public Vector3d Axis { get; }
        public Point3d Center { get; }
    }

    public static Fin<GeometryMotion> Translate(Vector3d vector) {
        return guard(ValidityClaim.Finite(value: vector), new KernelFault.InvalidInput(Axis: Some(nameof(vector)))).ToFin()
            .Map(_ => (GeometryMotion)new Translation(vector: vector));
    }

    public static Fin<GeometryMotion> Scale(double factor, Context context) {
        return from admitted in Admit.Finite(value: factor)
               from domain in Optional(context).ToFin(Fail: new KernelFault.MissingContext())
               from _ in guard(Math.Abs(value: admitted) > domain.For(lane: ToleranceLane.Neglect).Value, new KernelFault.InvalidInput(Axis: Some(nameof(factor))))
               select (GeometryMotion)new UniformScale(factor: admitted);
    }

    public static Fin<GeometryMotion> Rotate(double angleRadians, Vector3d axis, Point3d center) {
        return from angle in Admit.Finite(value: angleRadians)
               from _axis in guard(ValidityClaim.Direction(value: axis), new KernelFault.InvalidInput(Axis: Some(nameof(axis))))
               from _center in guard(ValidityClaim.Finite(value: center), new KernelFault.InvalidInput(Axis: Some(nameof(center))))
               select (GeometryMotion)new Rotation(angleRadians: angle, axis: axis, center: center);
    }

    internal Fin<AppliedMotion> Apply(GeometryBase geometry) => Switch(
        geometry,
        matrix: static (state, edit) =>
            from domain in Optional(edit.Domain).ToFin(Fail: new KernelFault.MissingContext())
            from spec in Admit.Need(edit.Value)
            from value in Placement.Build(spec: spec, context: Some(domain))
            from _ in Admit.Confirm(success: state.Transform(xform: value))
            let uniformity = domain.For(lane: ToleranceLane.ScaleUniformity).Value
            let inverse = value.TryGetInverse(inverse: out global::Rhino.Transform reversed)
                ? Some(reversed)
                : Option<global::Rhino.Transform>.None
            let similarity = value.DecomposeSimilarity(
                translation: out Vector3d similarityTranslation,
                dilation: out double dilation,
                rotation: out global::Rhino.Transform similarityRotation,
                tolerance: uniformity)
            let rigidity = value.DecomposeRigid(
                translation: out Vector3d rigidTranslation,
                rotation: out global::Rhino.Transform rigidRotation,
                tolerance: uniformity)
            let affine = value.DecomposeAffine(
                translation: out Vector3d affineTranslation,
                rotation: out global::Rhino.Transform affineRotation,
                orthogonal: out global::Rhino.Transform orthogonal,
                diagonal: out Vector3d diagonal)
                ? Some((Translation: affineTranslation, Rotation: affineRotation, Orthogonal: orthogonal, Diagonal: diagonal))
                : Option<(Vector3d Translation, global::Rhino.Transform Rotation, global::Rhino.Transform Orthogonal, Vector3d Diagonal)>.None
            select (AppliedMotion)new AppliedMotion.Matrix(
                Value: value,
                Inverse: inverse,
                Similarity: similarity,
                SimilarityTranslation: similarityTranslation,
                Dilation: dilation,
                SimilarityRotation: similarityRotation,
                Rigidity: rigidity,
                RigidTranslation: rigidTranslation,
                RigidRotation: rigidRotation,
                Affine: affine),
        translation: static (state, edit) =>
            from _ in Admit.Confirm(success: state.Translate(translationVector: edit.Vector))
            select (AppliedMotion)new AppliedMotion.Native(
                Value: edit,
                Reverse: new Translation(vector: -edit.Vector)),
        uniformScale: static (state, edit) =>
            from _ in Admit.Confirm(success: state.Scale(scaleFactor: edit.Factor))
            select (AppliedMotion)new AppliedMotion.Native(
                Value: edit,
                Reverse: new UniformScale(factor: 1.0 / edit.Factor)),
        rotation: static (state, edit) =>
            from _ in Admit.Confirm(success: state.Rotate(
                angleRadians: edit.AngleRadians,
                rotationAxis: edit.Axis,
                rotationCenter: edit.Center))
            select (AppliedMotion)new AppliedMotion.Native(
                Value: edit,
                Reverse: new Rotation(
                    angleRadians: -edit.AngleRadians,
                    axis: edit.Axis,
                    center: edit.Center)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AppliedMotion {
    private AppliedMotion() { }
    public sealed record Matrix(
        global::Rhino.Geometry.Transform Value,
        Option<global::Rhino.Geometry.Transform> Inverse,
        TransformSimilarityType Similarity,
        Vector3d SimilarityTranslation,
        double Dilation,
        global::Rhino.Geometry.Transform SimilarityRotation,
        TransformRigidType Rigidity,
        Vector3d RigidTranslation,
        global::Rhino.Geometry.Transform RigidRotation,
        Option<(Vector3d Translation, global::Rhino.Geometry.Transform Rotation, global::Rhino.Geometry.Transform Orthogonal, Vector3d Diagonal)> Affine) : AppliedMotion;
    public sealed record Native(GeometryMotion Value, GeometryMotion Reverse) : AppliedMotion;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TagOp {
    private TagOp() { }
    public sealed record Read(string Key) : TagOp;
    public sealed record ReadAll : TagOp;
    public sealed record Set(string Key, string Value) : TagOp;
    public sealed record Delete(string Key) : TagOp;
    public sealed record Clear : TagOp;

    internal OpTrait Trait => Switch(
        read: static _ => OpTrait.Observes,
        readAll: static _ => OpTrait.Observes,
        set: static _ => OpTrait.Mutates,
        delete: static _ => OpTrait.Mutates,
        clear: static _ => OpTrait.Mutates);

    internal Fin<TagResult> Apply(GeometryBase geometry) => Switch(
        geometry,
        read: static (state, tag) =>
            from name in Acceptance.Text(value: tag.Key)
            select (TagResult)new TagResult.Value(Key: name, Stored: Optional(state.GetUserString(key: name))),
        readAll: static (state, _) => Fin.Succ<TagResult>(value: new TagResult.Snapshot(Stored: Snapshot(state.GetUserStrings()))),
        set: static (state, tag) =>
            from name in Acceptance.Text(value: tag.Key)
            from value in Admit.Need(tag.Value)
            let before = Optional(state.GetUserString(key: name))
            from _ in Admit.Confirm(success: state.SetUserString(key: name, value: value))
            let after = Optional(state.GetUserString(key: name))
            from __ in Admit.Confirm(success: after.Equals(Some(value)))
            select (TagResult)new TagResult.Changed(Key: name, Before: before, After: after),
        delete: static (state, tag) =>
            from name in Acceptance.Text(value: tag.Key)
            let before = Optional(state.GetUserString(key: name))
            from _ in before.IsSome
                ? Admit.Confirm(success: state.DeleteUserString(key: name))
                : Fin.Succ(value: unit)
            let after = Optional(state.GetUserString(key: name))
            from __ in Admit.Confirm(success: after.IsNone)
            select (TagResult)new TagResult.Changed(Key: name, Before: before, After: after),
        clear: static (state, _) =>
            from before in Fin.Succ(Snapshot(state.GetUserStrings()))
            from _ in Try.lift(() => {
                state.DeleteAllUserStrings();
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner)
            let after = Snapshot(state.GetUserStrings())
            from __ in Admit.Confirm(success: after.IsEmpty)
            select (TagResult)new TagResult.Cleared(Before: before, After: after));

    internal static HashMap<string, string> Snapshot(System.Collections.Specialized.NameValueCollection native) =>
        toSeq(native.AllKeys)
            .Choose(name => Optional(name).Bind(key => Optional(native[key]).Map(value => (Key: key, Value: value))))
            .Fold(HashMap<string, string>(), static (map, pair) => map.AddOrUpdate(key: pair.Key, value: pair.Value));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TagResult {
    private TagResult() { }
    public sealed record Value(string Key, Option<string> Stored) : TagResult;
    public sealed record Snapshot(HashMap<string, string> Stored) : TagResult;
    public sealed record Changed(string Key, Option<string> Before, Option<string> After) : TagResult;
    public sealed record Cleared(HashMap<string, string> Before, HashMap<string, string> After) : TagResult;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryOp {
    private GeometryOp() { }
    public sealed record Inspect : GeometryOp;
    public sealed record Crc(GeometryCrc Chain) : GeometryOp;
    public sealed record Tag(TagOp Value) : GeometryOp;
    public sealed record Transform(GeometryMotion Motion) : GeometryOp;
    public sealed record Bounds(GeometryBounds Query) : GeometryOp;
    public sealed record Clip(ClipOp Value) : GeometryOp;

    internal OpTrait Trait => Switch(
        inspect: static _ => OpTrait.Observes,
        crc: static _ => OpTrait.Observes,
        tag: static operation => operation.Value.Trait,
        transform: static _ => OpTrait.Mutates,
        bounds: static _ => OpTrait.Observes,
        clip: static operation => operation.Value.Trait);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryResult {
    private GeometryResult() { }
    public sealed record Facts(GeometryFacts Value) : GeometryResult;
    public sealed record Compared(GeometryComparison Policy, bool Equal) : GeometryResult;
    public sealed record Hashed(GeometryCrc Value) : GeometryResult;
    public sealed record Tagged(TagResult Value) : GeometryResult;
    public sealed record Transformed(AppliedMotion Motion) : GeometryResult;
    public sealed record Bounded(NativeBounds Value) : GeometryResult;
    public sealed record Clipped(ClipTransition Value) : GeometryResult;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeometryTrait : ICapability<GeometryTrait> {
    public static readonly GeometryTrait DocumentControlled = new(key: "document-controlled");
    public static readonly GeometryTrait Shallow = new(key: "shallow");
}

public sealed record GeometryFacts(
    ObjectType NativeType,
    CapabilitySet<GeometryTrait> Traits,
    Option<string> Invalidity,
    GeometryCrc Content,
    HashMap<string, string> Tags) : IValidityEvidence {
    public bool IsValid => Invalidity.IsNone;

    internal static GeometryFacts Of(GeometryBase geometry) {
        bool valid = geometry.IsValidWithLog(out string log);
        return new GeometryFacts(
            NativeType: geometry.ObjectType,
            Traits: CapabilitySet<GeometryTrait>.Of()
                .Apply(held => geometry.IsDocumentControlled ? held.With(capability: GeometryTrait.DocumentControlled) : held)
                .Apply(held => geometry.IsShallowDuplicate ? held.With(capability: GeometryTrait.Shallow) : held),
            Invalidity: valid ? Option<string>.None : HostEdge.Text(log),
            Content: GeometryCrc.Of(geometry: geometry),
            Tags: TagOp.Snapshot(geometry.GetUserStrings()));
    }
}

public sealed record GeometryOutcome(
    GeometryResult Result,
    GeometryCrc Before,
    GeometryCrc After,
    Seq<Error> CleanupFaults);
```

## [04]-[CLIPPING]

- Owner: `ClipOp` owns clipping-plane state transitions over one retained seed and one canonical membership value, and it is the folder's ONE `SetClipParticipation` writer — `Exchange/sheets` composes it for the document-attached half rather than spelling a second scope algebra. `FieldOverride<T>` declares HERE at the Document tier: the three-state Keep/Set/Clear override vocabulary the depth arm and every Exchange override site read, seated at the lowest stratum both reach.
- Law: `FieldOverride<T>` states three intentions and no more — `Keep` leaves the host state standing, `Set` writes a gate plus its value, and `Clear` forces the gate off so the host inherits. A caller carrying an override never spells a second enable flag beside its value, and a per-page override union beside this owner is the deleted form; the former Exchange-tier declaration re-points here.
- Law: the owner carries BOTH gate-plus-value arms and no page mints a third — `Apply` gates admission at write time, `Through` writes totally for a payload its own type admitted at construction; admission TIMING is the whole discriminant, and the former Exchange-tier `HostGate.Through` static twin deletes onto this member.
- Law: inclusion, exclusion, and unrestricted scope remain distinct even when their member sets are empty; admission canonicalizes identifiers once, and each scope case writes the host pair with its own literal — the case IS the exclusion discriminant, so no helper threads a bool.
- Law: each edit mutates one deep working copy, re-reads the complete state, and proves the requested transition before lease swap; the depth arm proves `Keep` by equality with the prior state, so an untouched gate is a confirmed fact rather than an unchecked hole.
- Law: viewport edits are set algebra over canonical before and after values, so add and remove are idempotent and replace derives one delta.
- Law: a clipping plane authors DETACHED — `GeometryHandle` holds one custody lease and no document — so a raw `Guid` handed to `ViewportOp` is REQUESTED membership, admitted against `Guid.Empty` alone, and `Confirm` proves only that the written set equals the requested set. `ViewportOp.Proven` is the one fence turning requested membership into existence evidence: a caller holding a document folds `ViewportTarget` addresses through `ViewportTarget.ResolveViewport` before constructing the edit, so a committed plane carries ids that name live viewports. A committing pipeline that skips that fold commits fabricated ids, and no downstream read can tell them from real ones.
- Boundary: document lookup, table mutation, and redraw remain on the document transaction spine; viewport EXISTENCE is proven by `ViewportOp.Proven` at whichever boundary holds the document, because the spine never sees the detached authoring path.
- Growth: a clipping capability extends `ClipOp`; a membership modality extends `ClipScope`; an override intention never widens — three states are the whole vocabulary.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldOverride<T> {
    private FieldOverride() { }
    public sealed record Keep : FieldOverride<T>;
    public sealed record Set(T Value) : FieldOverride<T>;
    public sealed record Clear : FieldOverride<T>;

    public Option<T> Accepts => Switch(
        keep: static _ => Option<T>.None,
        set: static held => Some(held.Value),
        clear: static _ => Option<T>.None);

    internal Fin<Unit> Apply(Func<T, Fin<T>> admit, Action<T> write, Action clear) => Switch(
        state: (Admit: admit, Write: write, Clear: clear),
        keep: static (_, _) => Fin.Succ(value: unit),
        set: static (state, held) => state.Admit(arg: held.Value).Bind(admitted => Try.lift(() => {
            state.Write(obj: admitted);
            return Fin.Succ(value: unit);
        }).Run().Bind(static inner => inner)),
        clear: static (state, _) => Try.lift(() => {
            state.Clear();
            return Fin.Succ(value: unit);
        }).Run().Bind(static inner => inner));

    internal Unit Through<THost>(THost host, Action<THost, bool> gate, Action<THost, T> value) => Switch(
        state: (Host: host, Gate: gate, Value: value),
        keep: static (_, _) => unit,
        set: static (state, seat) => {
            state.Gate(state.Host, true);
            state.Value(state.Host, seat.Value);
            return unit;
        },
        clear: static (state, _) => {
            state.Gate(state.Host, false);
            return unit;
        });
}

[ComplexValueObject]
[ValidationError]
public sealed partial class ClipSet {
    public Seq<Guid> Objects { get; }
    public Seq<int> Layers { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<Guid> objects,
        ref Seq<int> layers) {
        bool valid = !objects.Exists(static id => id == Guid.Empty) && !layers.Exists(static index => index < 0);
        objects = toSeq(objects.Distinct().Order());
        layers = toSeq(layers.Distinct().Order());
        validationError = valid ? null : new ValidationError(message: "Clip membership contains an invalid object id or layer index.");
    }

    public static Fin<ClipSet> Of(Seq<Guid> objects, Seq<int> layers) =>
        FactoryBridge.Accept<ClipSet>(fault: Validate(objects, layers, out ClipSet? admitted), value: admitted);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClippingPlaneSeed {
    private ClippingPlaneSeed() { }
    public sealed record Frame(Plane Value) : ClippingPlaneSeed;
    public sealed record Surface(PlaneSurface Value) : ClippingPlaneSeed;

    internal Fin<Lease<GeometryBase>> Build() => Switch(frame: static (op, seed) =>
            from _ in guard(seed.Value.IsValid, new KernelFault.InvalidInput(Axis: Some(nameof(Plane)))).ToFin()
            select (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: new ClippingPlaneSurface(plane: seed.Value)),
        surface: static (op, seed) =>
            from plane in Acceptance.Input(value: seed.Value)
            select (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: new ClippingPlaneSurface(planeSurface: plane)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClipScope {
    private ClipScope() { }
    public sealed record Everything : ClipScope;
    public sealed record Only(ClipSet Members) : ClipScope;
    public sealed record Except(ClipSet Members) : ClipScope;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewportOp {
    private ViewportOp() { }
    public sealed record Add(Seq<Guid> Ids) : ViewportOp;
    public sealed record Remove(Seq<Guid> Ids) : ViewportOp;
    public sealed record Replace(Seq<Guid> Ids) : ViewportOp;

    internal Fin<Seq<Guid>> Resolve(Seq<Guid> before) =>
        from current in Admit(before)
        from desired in Switch(
            current,
            add: static (state, edit) => Admit(edit.Ids).Map(ids => Canonical(state + ids)),
            remove: static (state, edit) => Admit(edit.Ids)
                .Map(ids => state.Filter(id => !ids.Exists(candidate => candidate == id))),
            replace: static (state, edit) => Admit(edit.Ids))
        select desired;

    public static Fin<Seq<Guid>> Proven(RhinoDoc document, params ReadOnlySpan<ViewportTarget> targets) {
        return toSeq(targets.ToArray())
            .Traverse(target => Admit.Need(target)
                .Bind(address => address.ResolveViewport(document: document, key: op))
                .Map(static viewport => viewport.Id)
                .ToValidation())
            .As()
            .ToFin()
            .Map(Canonical);
    }

    private static Fin<Seq<Guid>> Admit(Seq<Guid> ids) =>
        ids.Exists(static id => id == Guid.Empty)
            ? Fin.Fail<Seq<Guid>>(error: new KernelFault.InvalidInput())
            : Fin.Succ(value: Canonical(ids));

    private static Seq<Guid> Canonical(Seq<Guid> ids) => toSeq(ids.Distinct().Order());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClipOp {
    private ClipOp() { }
    public sealed record Read : ClipOp;
    public sealed record Scope(ClipScope Value) : ClipOp;
    public sealed record Depth(FieldOverride<double> Value) : ClipOp;
    public sealed record Viewports(ViewportOp Value) : ClipOp;
    public sealed record Style(Option<Guid> DimensionStyleId) : ClipOp;

    internal OpTrait Trait => this is Read ? OpTrait.Observes : OpTrait.Mutates;

    internal Fin<ClipTransition> Apply(GeometryBase geometry) =>
        geometry is ClippingPlaneSurface surface
            ? from before in State(surface)
              from _ in this.Switch(
                  (Surface: surface, Before: before),
                  read: static (_, _) => Fin.Succ(value: unit),
                  scope: static (state, edit) => Scoped(state.Surface, edit.Value),
                  depth: static (state, edit) => edit.Value.Apply(
                      admit: value => Admit.Positive(value: value),
                      write: value => {
                          state.Surface.PlaneDepth = value;
                          state.Surface.PlaneDepthEnabled = true;
                      },
                      clear: () => state.Surface.PlaneDepthEnabled = false),
                  viewports: static (state, edit) => Viewported(state.Surface, state.Before.ViewportIds, edit.Value),
                  style: static (state, edit) => Styled(state.Surface, edit.DimensionStyleId))
              from after in State(surface)
              from __ in Confirmed(before, after)
              select new ClipTransition(Before: before, After: after)
            : Fin.Fail<ClipTransition>(error: new KernelFault.InvalidInput());

    private static Fin<ClipState> State(ClippingPlaneSurface surface) =>
        Try.lift(() => {
            Seq<Guid> viewports = toSeq(surface.ViewportIds().Distinct().Order());
            Fin<ClipScope> scope = surface.ParticipationListsEnabled
                ? ScopeOf(surface)
                : Fin.Succ<ClipScope>(value: new ClipScope.Everything());
            Fin<Option<double>> depth = surface.PlaneDepthEnabled
                ? Admit.Positive(value: surface.PlaneDepth).Map(static value => Some(value))
                : Fin.Succ(Option<double>.None);
            return from admittedScope in scope
                   from admittedDepth in depth
                   from _ in viewports.Exists(static id => id == Guid.Empty)
                       ? Fin.Fail<Unit>(error: new KernelFault.InvalidResult())
                       : Fin.Succ(value: unit)
                   select new ClipState(
                       Scope: admittedScope,
                       Depth: admittedDepth,
                       ViewportIds: viewports,
                       DimensionStyleId: Optional(surface.DimensionStyleId).Filter(static id => id != Guid.Empty));
        }).Run().Bind(static inner => inner);

    private static Fin<ClipScope> ScopeOf(ClippingPlaneSurface surface) {
        surface.GetClipParticipation(
            objectIds: out IEnumerable<Guid> objects,
            layerIndices: out IEnumerable<int> layers,
            isExclusionList: out bool exclusion);
        return ClipSet.Of(toSeq(objects), toSeq(layers)).Map(set =>
            exclusion ? (ClipScope)new ClipScope.Except(Members: set) : new ClipScope.Only(Members: set));
    }

    private static Fin<Unit> Scoped(ClippingPlaneSurface surface, ClipScope scope) =>
        Admit.Need(scope).Bind(request => request.Switch(
            surface,
            everything: static (state, _) => Try.lift(() => {
                state.ClearClipParticipationLists();
                state.ParticipationListsEnabled = false;
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner),
            only: static (state, set) => Try.lift(() => {
                state.SetClipParticipation(set.Members.Objects.AsIterable(), set.Members.Layers.AsIterable(), isExclusionList: false);
                state.ParticipationListsEnabled = true;
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner),
            except: static (state, set) => Try.lift(() => {
                state.SetClipParticipation(set.Members.Objects.AsIterable(), set.Members.Layers.AsIterable(), isExclusionList: true);
                state.ParticipationListsEnabled = true;
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner)));

    private static Fin<Unit> Styled(ClippingPlaneSurface surface, Option<Guid> style) =>
        style.Match(
            Some: id => id == Guid.Empty
                ? Fin.Fail<Unit>(error: new KernelFault.InvalidInput())
                : Try.lift(() => { surface.DimensionStyleId = id; return Fin.Succ(value: unit); }).Run().Bind(static inner => inner),
            None: () => Try.lift(() => { surface.DimensionStyleId = Guid.Empty; return Fin.Succ(value: unit); }).Run().Bind(static inner => inner));

    private static Fin<Unit> Viewported(ClippingPlaneSurface surface, Seq<Guid> before, ViewportOp operation) =>
        from request in Admit.Need(operation)
        from desired in request.Resolve(before)
        from _ in before.Filter(id => !desired.Exists(candidate => candidate == id))
            .TraverseM(id => Admit.Confirm(success: surface.RemoveClipViewportId(viewportId: id))).As()
        from __ in desired.Filter(id => !before.Exists(candidate => candidate == id))
            .TraverseM(id => Admit.Confirm(success: surface.AddClipViewportId(viewportId: id))).As()
        select unit;

    private static Fin<Unit> Confirmed(ClipOp operation, ClipState before, ClipState after) =>
        operation.Switch(
            (Before: before, After: after),
            read: static (state, _) => Admit.Confirm(success: state.Before.Equals(state.After)),
            scope: static (state, edit) => Admit.Confirm(success: edit.Value.Equals(state.After.Scope)),
            depth: static (state, edit) => Admit.Confirm(success: edit.Value.Switch(
                keep: _ => state.Before.Depth.Equals(state.After.Depth),
                set: held => state.After.Depth.Equals(Some(held.Value)),
                clear: _ => state.After.Depth.IsNone)),
            viewports: static (state, edit) => edit.Value.Resolve(state.Before.ViewportIds)
                .Bind(expected => Admit.Confirm(success: expected.Equals(state.After.ViewportIds))),
            style: static (state, edit) => Admit.Confirm(success: edit.DimensionStyleId.Equals(state.After.DimensionStyleId)));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ClipState(
    ClipScope Scope,
    Option<double> Depth,
    Seq<Guid> ViewportIds,
    Option<Guid> DimensionStyleId);

public readonly record struct ClipTransition(ClipState Before, ClipState After);
```

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-geometry.md` — geometry duplication/transform members; `api-rhinocommon-objects.md` — `SetClipParticipation`, handle custody); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]`/`[ValueObject]` clip and handle vocabularies, `[ComplexValueObject]` `FieldOverride<T>`); kernel `Domain/results` (`Transition`, `Cell`) + `Analysis` measures.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
