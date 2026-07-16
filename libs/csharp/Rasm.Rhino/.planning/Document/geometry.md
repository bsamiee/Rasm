# [RASM_RHINO_GEOMETRY]

`Rasm.Rhino.Document` owns the native `GeometryBase` crossing: custody becomes an explicit resource mode, host identity stays process-local, kernel-built transforms apply through one deformability policy, kernel bounds run inside the live handle extent, and clipping state mutates through one receipted operation family.

## [01]-[INDEX]

- [02]-[CUSTODY_AND_IDENTITY]: explicit borrow, detach, and copy-on-write custody; transient identity; user-string mutation; handle-scoped kernel bounds.
- [03]-[TRANSFORM_APPLICATION]: transactional native transform application under one deformability policy.
- [04]-[CLIP_PARTICIPATION]: scope, depth, viewport, and dimension-style state through one receipted operation.
- [05]-[IMPLEMENTATION]: the canonical eventual source-file declaration.
- [06]-[SURFACE_LEDGER]: concern ownership and public entries.

## [02]-[CUSTODY_AND_IDENTITY]

- Owner: `CrossingMode` is the custody policy; `GeometryHandle` is the resource capsule; `HandleRelease` is the closed lifecycle receipt; `GeometryCrc` is the chainable host change probe; `GeometryComparison` is the transient equality policy; `TagOp` is the user-string operation family.
- Packages: RhinoCommon supplies native geometry state; LanguageExt.Core carries the rails and immutable collections; Thinktecture.Runtime.Extensions generates the closed owners; `Rasm` supplies identity, taxonomy, context, and bounds analysis.
- Entry: `GeometryCrossing.Cross(object, CrossingMode, Op?)` absorbs kernel geometry forms and `ClippingPlaneSeed` into a `GeometryHandle`; `Inspect`, `Matches`, `Crc`, `Tag`, `Move`, `Clip`, and `Bounds<TOut>` keep every native call inside the handle's live extent.
- Law: kernel-owned forms transfer directly; a borrowed native form stays borrowed under `Borrow`, deep-copies under `Detach`, and shallow-copies under `CopyOnWrite`. Every mutation runs on a deep working copy and atomically replaces the live lease only after success.
- Law: `GeometryComparison` and `GeometryCrc` remain process-local evidence. Federation identity exists only after projection to a kernel `EncodeForm` and `Reconciliation.Apply`; this crossing mints no content hash.
- Law: a stored empty user string is present. `TagResult` preserves the nullable host read without filtering its payload, and every mutation reports its before and after values.
- Law: `Bounds<TOut>` runs the kernel's `Rasm.Analysis.Bounds` family inside the handle lock and projects its `Validation` through `Fin`; no native geometry result can escape the custody extent.
- Receipt: `GeometryFacts` carries host observations; one `Committed<TValue>` receipt carries every operation's value, CRC transition, and cleanup evidence — `Tag` lands `Committed<TagResult>` — and `GeometryHandle.Release` carries disposal state.
- Growth: a custody behavior extends `CrossingMode`, a kernel geometry kind extends `GeometryForm`, and a host fact extends `GeometryFacts` inside the existing crossing.
- Boundary: `ObjectType`, `ComponentIndex`, document control, shallow status, deformability, memory estimate, and reference equality remain host facts; kernel taxonomy begins at `Kind.Of`.

## [03]-[TRANSFORM_APPLICATION]

- Owner: `Motion` binds one kernel `TransformSpec` to one `DeformationPolicy`; the policy owns non-similarity admission before host mutation.
- Packages: `Rasm.Numerics` resolves the transform algebra; RhinoCommon applies the native matrix; LanguageExt.Core and Thinktecture.Runtime.Extensions carry the rail and policy row.
- Entry: `GeometryHandle.Move(Motion, Context, Op?)` resolves the kernel transform and applies it through the handle's transactional copy-on-write boundary.
- Law: `Require` admits an already-deformable target and `Promote` invokes `MakeDeformable`; both paths call `GeometryBase.Transform` exactly once after admission.
- Receipt: `Move` lands the shared `Committed<Unit>` receipt — the before-and-after `GeometryCrc` pair plus post-commit cleanup evidence.
- Growth: a new native preparation rule is one `DeformationPolicy` row; transform construction grows only on the kernel `TransformSpec` owner.
- Boundary: `Rasm.Numerics.Placement` owns transform construction, composition, decomposition, inversion, and transformed-box projection; this page owns only native application and deformability.

## [04]-[CLIP_PARTICIPATION]

- Owner: `ClippingPlaneSeed` admits native construction, `ClipSet` admits canonical object and layer membership, `ClipScope` closes participation state, `ViewportOp` owns viewport-set algebra, and `ClipOp` owns the state transition.
- Packages: RhinoCommon supplies `ClippingPlaneSurface`; LanguageExt.Core carries set and result rails; Thinktecture.Runtime.Extensions generates source, scope, and operation dispatch.
- Entry: `GeometryCrossing.Cross(object, CrossingMode, Op?)` originates an owned handle through its `ClippingPlaneSeed` arm, and `GeometryHandle.Clip(ClipOp, Op?)` reads or changes scope, positive depth, clipped viewport IDs, and `DimensionStyleId`.
- Law: `ClippingPlaneSeed` requires a valid `Plane` or `PlaneSurface`, so construction cannot originate an uninitialized clipping plane.
- Law: participation lists, depth enablement, and viewport idempotence become cases and receipts. An already-present add and an already-absent remove are successful unchanged transitions.
- Law: object IDs, layer indices, and viewport IDs reject invalid members and canonicalize ordering and duplicates before native mutation.
- Law: the handle's working-copy swap makes every multi-ID change atomic; a refused native step disposes the work copy and preserves the live clipping state.
- Receipt: `Clip` lands `Committed<ClipTransition>` — before-and-after state, the CRC transition, cleanup evidence, and the derived `Changed` projection; every mutation succeeds only after the re-read state confirms the requested transition.
- Growth: a new clip origin extends `ClippingPlaneSeed`, a new state axis extends `ClipOp`, and a new viewport algebra extends `ViewportOp` without another entrypoint.
- Boundary: clipping-plane object lookup, viewport existence, display participation, and redraw remain document and visual operations over the IDs this surface persists.

## [05]-[IMPLEMENTATION]

Declaration order preserves generated owners and lifetime boundaries before the operation kernels that consume them.

```csharp signature
// --- [RUNTIME_PRELUDE] -------------------------------------------------------------------
using System.Threading;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Document;

// --- [TYPES] -----------------------------------------------------------------------------

[ValueObject<uint>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public readonly partial struct GeometryCrc {
    public static readonly GeometryCrc Seed = Create(value: 0u);
}

[SmartEnum]
public sealed partial class CrossingMode {
    public static readonly CrossingMode Borrow = new(mutable: false, acquire: Borrowed);
    public static readonly CrossingMode Detach = new(mutable: true, acquire: Detached);
    public static readonly CrossingMode CopyOnWrite = new(mutable: true, acquire: Shallow);

    public bool Mutable { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<Lease<GeometryBase>> Acquire(GeometryBase geometry, Op key);

    private static Fin<Lease<GeometryBase>> Borrowed(GeometryBase geometry, Op _) =>
        Fin.Succ<Lease<GeometryBase>>(value: new Lease<GeometryBase>.Borrowed(Value: geometry));

    private static Fin<Lease<GeometryBase>> Detached(GeometryBase geometry, Op key) =>
        Copy(duplicate: geometry.Duplicate, key: key);

    private static Fin<Lease<GeometryBase>> Shallow(GeometryBase geometry, Op key) =>
        Copy(duplicate: geometry.DuplicateShallow, key: key);

    internal static Fin<Lease<GeometryBase>> Copy(Func<GeometryBase> duplicate, Op key) =>
        Minted<GeometryBase>(create: duplicate, key: key)
            .Map(static admitted => (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: admitted));

    internal static Fin<TNative> Minted<TNative>(Func<TNative?> create, Op key) where TNative : GeometryBase =>
        Optional(create).ToFin(Fail: key.InvalidInput())
            .Bind(factory => key.Catch(() => Optional(factory()).ToFin(Fail: key.InvalidResult())))
            .Bind(minted => key.AcceptValue(value: minted).Match(
                Succ: static admitted => Fin.Succ(value: admitted),
                Fail: primary => key.Catch(() => { minted.Dispose(); return Fin.Succ(value: unit); }).Match(
                    Succ: _ => Fin.Fail<TNative>(error: primary),
                    Fail: cleanup => Fin.Fail<TNative>(error: primary + cleanup))));
}

[SmartEnum]
public sealed partial class GeometryComparison {
    public static readonly GeometryComparison Shape = new(matches: GeometryBase.GeometryEquals);
    public static readonly GeometryComparison Reference = new(matches: GeometryBase.GeometryReferenceEquals);

    [UseDelegateFromConstructor]
    internal partial bool Matches(GeometryBase left, GeometryBase right);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TagOp {
    private TagOp() { }
    public sealed record Set(string Key, string Value) : TagOp;
    public sealed record Read(string Key) : TagOp;
    public sealed record ReadAll : TagOp;
    public sealed record Delete(string Key) : TagOp;
    public sealed record Clear : TagOp;

    internal bool Mutates => Switch(
        set: static _ => true,
        read: static _ => false,
        readAll: static _ => false,
        delete: static _ => true,
        clear: static _ => true);
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
public abstract partial record HandleRelease {
    private HandleRelease() { }
    public sealed record Live : HandleRelease;
    public sealed record Released : HandleRelease;
    public sealed record Faulted(Error Error) : HandleRelease;

    internal bool Active => Switch(
        live: static _ => true,
        released: static _ => false,
        faulted: static _ => false);
}

[SmartEnum]
public sealed partial class DeformationPolicy {
    public static readonly DeformationPolicy Require = new(prepare: RequireDeformable);
    public static readonly DeformationPolicy Promote = new(prepare: PromoteDeformable);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Prepare(GeometryBase geometry, Transform transform, Op key);

    private static Fin<Unit> RequireDeformable(GeometryBase geometry, Transform transform, Op key) =>
        NeedsDeformable(transform: transform) && !geometry.IsDeformable
            ? Fin.Fail<Unit>(error: key.InvalidInput())
            : Fin.Succ(value: unit);

    private static Fin<Unit> PromoteDeformable(GeometryBase geometry, Transform transform, Op key) =>
        NeedsDeformable(transform: transform) && !geometry.IsDeformable
            ? key.Confirm(success: geometry.MakeDeformable())
            : Fin.Succ(value: unit);

    private static bool NeedsDeformable(Transform transform) =>
        transform.SimilarityType is TransformSimilarityType.NotSimilarity;
}

[ComplexValueObject]
public sealed partial class ClipSet {
    public Seq<Guid> Objects { get; }
    public Seq<int> Layers { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<Guid> objects,
        ref Seq<int> layers) {
        bool valid = !objects.Exists(static id => id == Guid.Empty) && !layers.Exists(static index => index < 0);
        objects = objects.AsIterable().Distinct().Order().ToSeq();
        layers = layers.AsIterable().Distinct().Order().ToSeq();
        validationError = valid ? null : new ValidationError(message: "ClipSet requires non-empty object ids and non-negative layer indices.");
    }

    public static Fin<ClipSet> Of(Seq<Guid> objects, Seq<int> layers, Op? key = null) {
        Op op = key.OrDefault();
        return Validate(objects, layers, out ClipSet? admitted) is null && admitted is not null
            ? Fin.Succ(value: admitted)
            : Fin.Fail<ClipSet>(error: op.InvalidInput());
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClippingPlaneSeed {
    private ClippingPlaneSeed() { }
    public sealed record Frame(Plane Value) : ClippingPlaneSeed;
    public sealed record Surface(PlaneSurface Value) : ClippingPlaneSeed;

    internal Fin<Lease<GeometryBase>> Build(Op key) => Switch(
        state: key,
        frame: static (op, source) =>
            from frame in Rasm.Domain.Admit.Plane(basis: source.Value, key: op)
            from created in CrossingMode.Minted(create: () => new ClippingPlaneSurface(plane: frame), key: op)
            select (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: created),
        surface: static (op, source) =>
            from surface in op.AcceptInput(value: source.Value)
            from created in CrossingMode.Minted(create: () => new ClippingPlaneSurface(planeSurface: surface), key: op)
            select (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: created));
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

    internal Fin<Seq<Guid>> Resolve(Seq<Guid> before, Op key) =>
        from current in Admit(ids: before, key: key)
        from desired in Switch(
            state: (Current: current, Op: key),
            add: static (ctx, request) => Admit(ids: request.Ids, key: ctx.Op)
                .Map(ids => Canonical(ids: ctx.Current.Concat(ids))),
            remove: static (ctx, request) => Admit(ids: request.Ids, key: ctx.Op)
                .Map(ids => ctx.Current.Filter(id => !ids.Exists(candidate => candidate == id))),
            replace: static (ctx, request) => Admit(ids: request.Ids, key: ctx.Op))
        select desired;

    private static Fin<Seq<Guid>> Admit(Seq<Guid> ids, Op key) =>
        ids.Exists(static id => id == Guid.Empty)
            ? Fin.Fail<Seq<Guid>>(error: key.InvalidInput())
            : Fin.Succ(value: Canonical(ids: ids));

    private static Seq<Guid> Canonical(Seq<Guid> ids) => ids.AsIterable().Distinct().Order().ToSeq();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClipOp {
    private ClipOp() { }
    public sealed record Read : ClipOp;
    public sealed record Scope(ClipScope Value) : ClipOp;
    public sealed record Depth(Option<double> Value) : ClipOp;
    public sealed record Viewports(ViewportOp Value) : ClipOp;
    public sealed record Style(Option<Guid> DimensionStyleId) : ClipOp;

    internal bool Mutates => Switch(
        read: static _ => false,
        scope: static _ => true,
        depth: static _ => true,
        viewports: static _ => true,
        style: static _ => true);

    internal Fin<Unit> Confirm(ClipState before, ClipState after, Op key) => Switch(
        state: (Before: before, After: after, Op: key),
        read: static (ctx, _) => ctx.Op.Confirm(success: ctx.Before.Equals(ctx.After)),
        scope: static (ctx, change) => Optional(change.Value).ToFin(Fail: ctx.Op.InvalidInput())
            .Bind(expected => ctx.Op.Confirm(success: expected.Equals(ctx.After.Scope))),
        depth: static (ctx, change) => ctx.Op.Confirm(success: change.Value.Equals(ctx.After.Depth)),
        viewports: static (ctx, change) => Optional(change.Value).ToFin(Fail: ctx.Op.InvalidInput())
            .Bind(request => request.Resolve(before: ctx.Before.ViewportIds, key: ctx.Op))
            .Bind(expected => ctx.Op.Confirm(success: expected.Equals(ctx.After.ViewportIds))),
        style: static (ctx, change) => ctx.Op.Confirm(success: change.DimensionStyleId.Equals(ctx.After.DimensionStyleId)));
}

// --- [MODELS] ----------------------------------------------------------------------------

public sealed record GeometryFacts(
    ObjectType NativeType,
    Option<Kind> Kind,
    ComponentIndex Component,
    bool DocumentControlled,
    bool Shallow,
    bool Deformable,
    bool HasBrepForm,
    int UserStringCount,
    uint MemoryBytes);

public readonly record struct Committed<T>(T Value, GeometryCrc Before, GeometryCrc After, Option<Error> Cleanup);

public sealed record Motion(TransformSpec Spec, DeformationPolicy Deformation) {
    internal Fin<Unit> Apply(GeometryBase geometry, Context context, Op key) =>
        from specification in Optional(Spec).ToFin(Fail: key.InvalidInput())
        from transform in Placement.Build(spec: specification, context: Some(context), key: key)
        from policy in Optional(Deformation).ToFin(Fail: key.InvalidInput())
        from _ in policy.Prepare(geometry: geometry, transform: transform, key: key)
        from __ in key.Confirm(success: geometry.Transform(xform: transform))
        select unit;
}

public sealed record ClipState(
    ClipScope Scope,
    Option<double> Depth,
    Seq<Guid> ViewportIds,
    Option<Guid> DimensionStyleId);

public readonly record struct ClipTransition(ClipState Before, ClipState After);

public sealed class GeometryHandle : IDisposable {
    private static long nextOrdinal;
    private Lease<GeometryBase> lease;
    private readonly Lock gate = new();
    private readonly long ordinal = Interlocked.Increment(location: ref nextOrdinal);
    private readonly CrossingMode mode;
    private HandleRelease release = new HandleRelease.Live();

    internal GeometryHandle(Lease<GeometryBase> lease, CrossingMode mode) {
        this.lease = lease;
        this.mode = mode;
    }

    public CrossingMode Mode => mode;
    public HandleRelease Release { get { lock (gate) { return release; } } }

    public Fin<GeometryFacts> Inspect(Op? key = null) =>
        With(key: key.OrDefault(), project: static geometry => Fin.Succ(value: new GeometryFacts(
            NativeType: geometry.ObjectType,
            Kind: Kind.Of(type: geometry.GetType()),
            Component: geometry.ComponentIndex(),
            DocumentControlled: geometry.IsDocumentControlled,
            Shallow: geometry.IsShallowDuplicate,
            Deformable: geometry.IsDeformable,
            HasBrepForm: geometry.HasBrepForm,
            UserStringCount: geometry.UserStringCount,
            MemoryBytes: geometry.MemoryEstimate())));

    public Fin<bool> Matches(GeometryHandle other, GeometryComparison comparison, Op? key = null) {
        Op op = key.OrDefault();
        return from counterpart in Optional(other).ToFin(Fail: op.InvalidInput())
               from policy in Optional(comparison).ToFin(Fail: op.InvalidInput())
               from equal in Match(other: counterpart, comparison: policy, key: op)
               select equal;
    }

    public Fin<GeometryCrc> Crc(GeometryCrc chain, Op? key = null) =>
        With(key: key.OrDefault(), project: geometry => Fin.Succ(value: GeometryCrc.Create(value: geometry.DataCRC(currentRemainder: chain))));

    public Fin<Committed<TagResult>> Tag(TagOp operation, Op? key = null) {
        Op op = key.OrDefault();
        return from verb in Optional(operation).ToFin(Fail: op.InvalidInput())
               from receipt in Operate(
                   mutates: verb.Mutates,
                   apply: geometry => Tagging.Apply(geometry: geometry, operation: verb, key: op),
                   key: op)
               select receipt;
    }

    public Fin<Committed<Unit>> Move(Motion motion, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from verb in Optional(motion).ToFin(Fail: op.InvalidInput())
               from model in Optional(context).ToFin(Fail: op.MissingContext())
               from committed in Change(key: op, mutation: geometry => verb.Apply(geometry: geometry, context: model, key: op))
               select committed;
    }

    public Fin<Committed<ClipTransition>> Clip(ClipOp operation, Op? key = null) {
        Op op = key.OrDefault();
        return from request in Optional(operation).ToFin(Fail: op.InvalidInput())
               from receipt in Operate(
                   mutates: request.Mutates,
                   apply: geometry => ClipParticipation.Apply(geometry: geometry, operation: request, key: op),
                   key: op)
               select receipt;
    }

    public Fin<Seq<TOut>> Bounds<TOut>(Rasm.Analysis.Bounds request, Context context, Op? key = null) where TOut : notnull {
        Op op = key.OrDefault();
        return from bounds in Optional(request).ToFin(Fail: op.InvalidInput())
               from model in Optional(context).ToFin(Fail: op.MissingContext())
               from result in With(
                   key: op,
                   project: geometry => Analyze.In(context: model)
                       .Run(
                           operation: Analyze.Query<GeometryBase, TOut>(query: AnalysisQuery.Bounds(query: bounds), key: op),
                           input: geometry)
                       .ToFin())
               select result;
    }

    public void Dispose() {
        lock (gate) {
            if (release.Active) {
                release = DisposeLease(lease: lease, key: Op.Of(name: nameof(Dispose))).Match(
                    Succ: static _ => (HandleRelease)new HandleRelease.Released(),
                    Fail: static error => new HandleRelease.Faulted(Error: error));
            }
        }
    }

    private Fin<Committed<TValue>> Operate<TValue>(bool mutates, Func<GeometryBase, Fin<TValue>> apply, Op key) =>
        mutates
            ? Change(key: key, mutation: apply)
            : With(key: key, project: geometry =>
                from value in apply(arg: geometry)
                let crc = GeometryCrc.Create(value: geometry.DataCRC(currentRemainder: GeometryCrc.Seed))
                select new Committed<TValue>(Value: value, Before: crc, After: crc, Cleanup: Option<Error>.None));

    internal Fin<TResult> With<TResult>(Op key, Func<GeometryBase, Fin<TResult>> project) {
        lock (gate) {
            return release.Active
                ? Optional(project).ToFin(Fail: key.InvalidInput()).Bind(body => key.Catch(() =>
                    key.AcceptInput(value: lease.Resource).Bind(active => body(arg: active))))
                : Fin.Fail<TResult>(error: key.InvalidInput());
        }
    }

    private Fin<bool> Match(GeometryHandle other, GeometryComparison comparison, Op key) {
        GeometryHandle first = ordinal <= other.ordinal ? this : other;
        GeometryHandle second = ReferenceEquals(objA: first, objB: other) ? this : other;
        lock (first.gate) {
            if (ReferenceEquals(objA: first, objB: second)) {
                return ActiveMatch(other: other, comparison: comparison, key: key);
            }
            lock (second.gate) {
                return ActiveMatch(other: other, comparison: comparison, key: key);
            }
        }
    }

    private Fin<bool> ActiveMatch(GeometryHandle other, GeometryComparison comparison, Op key) =>
        !release.Active || !other.release.Active
            ? Fin.Fail<bool>(error: key.InvalidInput())
            : key.Catch(() =>
                from left in key.AcceptInput(value: lease.Resource)
                from right in key.AcceptInput(value: other.lease.Resource)
                select comparison.Matches(left: left, right: right));

    private Fin<Committed<TResult>> Change<TResult>(Op key, Func<GeometryBase, Fin<TResult>> mutation) {
        lock (gate) {
            if (!release.Active || !Mode.Mutable) {
                return Fin.Fail<Committed<TResult>>(error: key.InvalidInput());
            }
            return Optional(mutation).ToFin(Fail: key.InvalidInput()).Bind(body =>
                key.Catch(() =>
                    from active in key.AcceptInput(value: lease.Resource)
                    let before = GeometryCrc.Create(value: active.DataCRC(currentRemainder: GeometryCrc.Seed))
                    from working in CrossingMode.Copy(duplicate: active.Duplicate, key: key)
                    select (Working: working, Before: before)).Bind(prepared => {
                    Lease<GeometryBase> working = prepared.Working;
                    return key.Catch(() =>
                        from result in body(arg: working.Resource)
                        from admitted in key.AcceptValue(value: working.Resource)
                        let after = GeometryCrc.Create(value: admitted.DataCRC(currentRemainder: GeometryCrc.Seed))
                        select (Result: result, After: after)).Match(
                        Succ: committed => {
                            Lease<GeometryBase> previous = lease;
                            lease = working;
                            Option<Error> cleanup = DisposeLease(lease: previous, key: key).Match(
                                Succ: static _ => Option<Error>.None,
                                Fail: static error => Some(error));
                            return Fin.Succ(value: new Committed<TResult>(
                                Value: committed.Result,
                                Before: prepared.Before,
                                After: committed.After,
                                Cleanup: cleanup));
                        },
                        Fail: primary => DisposeLease(lease: working, key: key).Match(
                            Succ: _ => Fin.Fail<Committed<TResult>>(error: primary),
                            Fail: cleanup => Fin.Fail<Committed<TResult>>(error: primary + cleanup)));
                }));
        }
    }

    private static Fin<Unit> DisposeLease(Lease<GeometryBase> lease, Op key) =>
        key.Catch(() => Fin.Succ(value: lease.Dispose()));
}

// --- [OPERATIONS] ------------------------------------------------------------------------

public static class GeometryCrossing {
    public static Fin<GeometryHandle> Cross(object source, CrossingMode mode, Op? key = null) {
        Op op = key.OrDefault();
        return from custody in Optional(mode).ToFin(Fail: op.InvalidInput())
               from value in Optional(source).ToFin(Fail: op.InvalidInput())
               from form in value is ClippingPlaneSeed seed ? seed.Build(key: op) : value.GeometryForm(key: op)
               from lease in form.Switch(
                   state: (Mode: custody, Op: op),
                   owned: static (_, owned) => Fin.Succ<Lease<GeometryBase>>(value: owned),
                   borrowed: static (ctx, borrowed) => ctx.Mode.Acquire(geometry: borrowed.Value, key: ctx.Op))
               select new GeometryHandle(lease: lease, mode: custody);
    }

    extension(Committed<ClipTransition> receipt) {
        public bool Changed =>
            !receipt.Value.Before.Equals(receipt.Value.After) || !receipt.Before.Equals(receipt.After);
    }
}

internal static class Tagging {
    internal static Fin<TagResult> Apply(GeometryBase geometry, TagOp operation, Op key) =>
        operation.Switch(
            state: (Geometry: geometry, Op: key),
            set: static (ctx, tag) => Set(geometry: ctx.Geometry, tag: tag, key: ctx.Op),
            read: static (ctx, tag) =>
                from admitted in ctx.Op.AcceptText(value: tag.Key)
                select (TagResult)new TagResult.Value(Key: admitted, Stored: Optional(ctx.Geometry.GetUserString(key: admitted))),
            readAll: static (ctx, _) => ctx.Op.Catch(() => Fin.Succ<TagResult>(value: new TagResult.Snapshot(Stored: Snapshot(geometry: ctx.Geometry)))),
            delete: static (ctx, tag) => Delete(geometry: ctx.Geometry, tag: tag, key: ctx.Op),
            clear: static (ctx, _) => Clear(geometry: ctx.Geometry, key: ctx.Op));

    private static Fin<TagResult> Set(GeometryBase geometry, TagOp.Set tag, Op key) =>
        from admitted in key.AcceptText(value: tag.Key)
        from value in Optional(tag.Value).ToFin(Fail: key.InvalidInput())
        let before = Optional(geometry.GetUserString(key: admitted))
        from _ in key.Confirm(success: geometry.SetUserString(key: admitted, value: value))
        let after = Optional(geometry.GetUserString(key: admitted))
        from __ in key.Confirm(success: after.Equals(Some(value)))
        select (TagResult)new TagResult.Changed(Key: admitted, Before: before, After: after);

    private static Fin<TagResult> Delete(GeometryBase geometry, TagOp.Delete tag, Op key) =>
        from admitted in key.AcceptText(value: tag.Key)
        let before = Optional(geometry.GetUserString(key: admitted))
        from _ in before.IsSome ? key.Confirm(success: geometry.DeleteUserString(key: admitted)) : Fin.Succ(value: unit)
        let after = Optional(geometry.GetUserString(key: admitted))
        from __ in key.Confirm(success: after.IsNone)
        select (TagResult)new TagResult.Changed(Key: admitted, Before: before, After: after);

    private static Fin<TagResult> Clear(GeometryBase geometry, Op key) =>
        key.Catch(() => {
            HashMap<string, string> before = Snapshot(geometry: geometry);
            geometry.DeleteAllUserStrings();
            HashMap<string, string> after = Snapshot(geometry: geometry);
            return after.IsEmpty
                ? Fin.Succ<TagResult>(value: new TagResult.Cleared(Before: before, After: after))
                : Fin.Fail<TagResult>(error: key.InvalidResult());
        });

    private static HashMap<string, string> Snapshot(GeometryBase geometry) {
        System.Collections.Specialized.NameValueCollection native = geometry.GetUserStrings();
        return toSeq(native.AllKeys)
            .Choose(key => Optional(key).Bind(valid => Optional(native[valid]).Map(value => (Key: valid, Value: value))))
            .Fold(HashMap<string, string>(), static (map, pair) => map.AddOrUpdate(key: pair.Key, value: pair.Value));
    }
}

internal static class ClipParticipation {
    internal static Fin<ClipTransition> Apply(GeometryBase geometry, ClipOp operation, Op key) =>
        geometry is ClippingPlaneSurface surface
            ? from before in State(surface: surface, key: key)
              from _ in operation.Switch(
                  state: (Surface: surface, Before: before, Op: key),
                  read: static (_, _) => Fin.Succ(value: unit),
                  scope: static (ctx, change) => ApplyScope(
                      surface: ctx.Surface,
                      scope: change.Value,
                      key: ctx.Op),
                  depth: static (ctx, change) => ApplyDepth(
                      surface: ctx.Surface,
                      depth: change.Value,
                      key: ctx.Op),
                  viewports: static (ctx, change) => ApplyViewports(
                      surface: ctx.Surface,
                      before: ctx.Before.ViewportIds,
                      operation: change.Value,
                      key: ctx.Op),
                  style: static (ctx, change) => ApplyStyle(
                      surface: ctx.Surface,
                      style: change.DimensionStyleId,
                      key: ctx.Op))
              from after in State(surface: surface, key: key)
              from __ in operation.Confirm(before: before, after: after, key: key)
              select new ClipTransition(Before: before, After: after)
            : Fin.Fail<ClipTransition>(error: key.InvalidInput());

    private static Fin<ClipState> State(ClippingPlaneSurface surface, Op key) =>
        key.Catch(() => {
            Seq<Guid> viewports = surface.ViewportIds().AsIterable().Distinct().Order().ToSeq();
            if (viewports.Exists(static id => id == Guid.Empty)) {
                return Fin.Fail<ClipState>(error: key.InvalidResult());
            }

            Fin<ClipScope> scope = surface.ParticipationListsEnabled
                ? ScopeOf(surface: surface, key: key)
                : Fin.Succ<ClipScope>(value: new ClipScope.Everything());

            Fin<Option<double>> depth = surface.PlaneDepthEnabled
                ? key.Positive(value: surface.PlaneDepth).Map(static value => Some(value))
                : Fin.Succ(Option<double>.None);

            return from admittedScope in scope
                   from admittedDepth in depth
                   select new ClipState(
                       Scope: admittedScope,
                       Depth: admittedDepth,
                       ViewportIds: viewports,
                       DimensionStyleId: Optional(surface.DimensionStyleId).Filter(static id => id != Guid.Empty));
        });

    private static Fin<ClipScope> ScopeOf(ClippingPlaneSurface surface, Op key) {
        surface.GetClipParticipation(
            objectIds: out IEnumerable<Guid> objects,
            layerIndices: out IEnumerable<int> layers,
            isExclusionList: out bool exclusion);
        return ClipSet.Of(objects: toSeq(objects), layers: toSeq(layers), key: key).Map(members =>
            exclusion ? (ClipScope)new ClipScope.Except(Members: members) : new ClipScope.Only(Members: members));
    }

    private static Fin<Unit> ApplyScope(ClippingPlaneSurface surface, ClipScope scope, Op key) =>
        Optional(scope).ToFin(Fail: key.InvalidInput()).Bind(request => request.Switch(
            state: (Surface: surface, Op: key),
            everything: static (ctx, _) => ctx.Op.Catch(() => {
                ctx.Surface.ClearClipParticipationLists();
                ctx.Surface.ParticipationListsEnabled = false;
                return Fin.Succ(value: unit);
            }),
            only: static (ctx, membership) => SetScope(
                surface: ctx.Surface,
                members: membership.Members,
                exclusion: false,
                key: ctx.Op),
            except: static (ctx, membership) => SetScope(
                surface: ctx.Surface,
                members: membership.Members,
                exclusion: true,
                key: ctx.Op)));

    private static Fin<Unit> SetScope(
        ClippingPlaneSurface surface,
        ClipSet members,
        bool exclusion,
        Op key) =>
        Optional(members).ToFin(Fail: key.InvalidInput()).Bind(admitted => key.Catch(() => {
            surface.SetClipParticipation(
                objectIds: admitted.Objects.AsIterable(),
                layerIndices: admitted.Layers.AsIterable(),
                isExclusionList: exclusion);
            surface.ParticipationListsEnabled = true;
            return Fin.Succ(value: unit);
        }));

    private static Fin<Unit> ApplyDepth(ClippingPlaneSurface surface, Option<double> depth, Op key) =>
        depth.Match(
            Some: value =>
                from admitted in key.Positive(value: value)
                from _ in key.Catch(() => {
                    surface.PlaneDepth = admitted;
                    surface.PlaneDepthEnabled = true;
                    return Fin.Succ(value: unit);
                })
                select unit,
            None: () => key.Catch(() => {
                surface.PlaneDepthEnabled = false;
                return Fin.Succ(value: unit);
            }));

    private static Fin<Unit> ApplyStyle(ClippingPlaneSurface surface, Option<Guid> style, Op key) =>
        style.Match(
            Some: id => id == Guid.Empty
                ? Fin.Fail<Unit>(error: key.InvalidInput())
                : key.Catch(() => {
                    surface.DimensionStyleId = id;
                    return Fin.Succ(value: unit);
                }),
            None: () => key.Catch(() => {
                surface.DimensionStyleId = Guid.Empty;
                return Fin.Succ(value: unit);
            }));

    private static Fin<Unit> ApplyViewports(
        ClippingPlaneSurface surface,
        Seq<Guid> before,
        ViewportOp operation,
        Op key) =>
        from change in Optional(operation).ToFin(Fail: key.InvalidInput())
        from desired in change.Resolve(before: before, key: key)
        from _ in before.Filter(id => !desired.Exists(candidate => candidate == id))
            .TraverseM(id => key.Confirm(success: surface.RemoveClipViewportId(viewportId: id)))
            .As()
        from __ in desired.Filter(id => !before.Exists(candidate => candidate == id))
            .TraverseM(id => key.Confirm(success: surface.AddClipViewportId(viewportId: id)))
            .As()
        select unit;
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]             | [OWNER]              | [ENTRY]        |
| :-----: | :-------------------- | :------------------- | :------------- |
|  [01]   | custody               | `GeometryCrossing`   | `Cross`        |
|  [02]   | host facts            | `GeometryHandle`     | `Inspect`      |
|  [03]   | geometry comparison   | `GeometryComparison` | `Matches`      |
|  [04]   | change probe          | `GeometryCrc`        | `Crc`          |
|  [05]   | release state         | `HandleRelease`      | `Release`      |
|  [06]   | tag state             | `TagOp`              | `Tag`          |
|  [07]   | kernel bounds         | `GeometryHandle`     | `Bounds<TOut>` |
|  [08]   | transform application | `Motion`             | `Move`         |
|  [09]   | clip construction     | `ClippingPlaneSeed`  | `Cross`        |
|  [10]   | clip state            | `ClipOp`             | `Clip`         |
