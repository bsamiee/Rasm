# [RASM_RHINO_GEOMETRY]

`GeometryHandle` owns retained `GeometryBase` custody from raw crossing through typed observation, motion, bounds, clipping, kernel projection, and release. Every mutation commits a deep working copy, and failed disposal remains in the handle's retry roster until release settles it.

## [01]-[INDEX]

- [02]-[CUSTODY]: `GeometryHandle` owns the active lease, retryable losing-lease roster, and observable release state.
- [03]-[PROGRAM]: `GeometryOp` folds host observation, motion, native bounds, tags, and clipping through `Apply`; `Compare` and `Bounds<TOut>` are the binary-lease and typed-projection siblings whose shape cannot inhabit the single-lease closed-result family.
- [04]-[CLIPPING]: `ClipOp` owns clipping-plane scope, depth, viewport, and style transitions.
- [05]-[IMPLEMENTATION]: `Geometry.cs` carries the complete owner declaration.

## [02]-[CUSTODY]

- Owner: `GeometryHandle` owns retained native custody and serializes every borrow, comparison, mutation, retry, and release against one lease state.
- Entry: `GeometryCrossing.Cross` admits each foreign geometry form through one custody policy and returns the frozen handle wire.
- Law: document-controlled ingress leaves document custody only through a deep copy; copy-on-write shares only non-document material and deepens before mutation.
- Law: `With` lends the native value only for the synchronous extent of the supplied rail. Commit and rollback retain each failed losing lease, and `Dispose` retries the complete roster.
- Exemption: `Lock` scopes are the platform-forced custody kernel; ordinal lock order makes two-handle comparison deadlock-free.
- Growth: a custody policy is one `CrossingMode` behavior row over the same acquisition rail.

## [03]-[PROGRAM]

- Owner: `GeometryOp` owns the single-lease host-operation family, and `GeometryReceipt` preserves its typed result, content-key transition, and every losing-lease cleanup fault.
- Entry: `GeometryHandle.Apply` discriminates by operation shape; `Compare` takes a second handle under ordinal-ordered dual locks, and `Bounds<TOut>` retains the typed kernel projection — both are the operations whose shape cannot inhabit the single-lease closed-result family, so neither is a `GeometryOp` case forcing a dead dispatch arm.
- Law: each case carries only its required evidence. Host-native translation, scale, rotation, and kernel-built transformation occupy one `GeometryMotion` family instead of forcing unrelated operations to carry `Context`.
- Law: each native motion derives its exact inverse request, while a kernel-built matrix preserves an inverse only when `TryGetInverse` proves one and captures the host decomposition classifications.
- Law: bounds admit every host-returned `BoundingBox` through the shared result oracle before preserving raw and inflated world, transformed, or framed evidence, including corners, edges, center, diagonal, and inverse motion where the host proves one.
- Law: inspection carries native validity and its diagnostic only when invalid; inflation admits a finite nonnegative component vector.
- Law: tag clearing snapshots once, invokes the host's atomic bag clear, and proves the resulting bag empty.
- Exemption: `BoundingBox` copy mutation inside `BoundsEvidence.Of` is the value-struct kernel required by RhinoCommon's `Inflate` surface.
- Boundary: kernel owners construct placement and analysis semantics; this owner applies or observes them inside native custody.
- Growth: a host capability is one case and one exhaustive arm inside the existing operation or motion family.

## [04]-[CLIPPING]

- Owner: `ClipOp` owns clipping-plane state transitions over one retained seed and one canonical membership value.
- Law: inclusion, exclusion, and unrestricted scope remain distinct even when their member sets are empty; admission canonicalizes identifiers once.
- Law: each edit mutates one deep working copy, re-reads the complete state, and proves the requested transition before lease swap.
- Law: viewport edits are set algebra over canonical before and after values, so add and remove are idempotent and replace derives one delta.
- Boundary: document lookup, viewport existence, table mutation, and redraw remain on the document transaction spine.
- Growth: a clipping capability extends `ClipOp`; a membership modality extends `ClipScope`.

## [05]-[IMPLEMENTATION]

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
    public static readonly GeometryCrc Zero = Create(value: 0u);

    internal static GeometryCrc Of(GeometryBase geometry) => Create(value: geometry.DataCRC(currentRemainder: Zero));
}

[SmartEnum]
public sealed partial class CrossingMode {
    public static readonly CrossingMode Borrow = new(mutable: false, acquire: Borrowed);
    public static readonly CrossingMode Detach = new(mutable: true, acquire: Detached);
    public static readonly CrossingMode CopyOnWrite = new(mutable: true, acquire: Shared);

    public bool Mutable { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<Lease<GeometryBase>> Acquire(GeometryBase geometry, Op key);

    private static Fin<Lease<GeometryBase>> Borrowed(GeometryBase geometry, Op key) =>
        geometry.IsDocumentControlled
            ? Fin.Fail<Lease<GeometryBase>>(error: key.InvalidInput())
            : Fin.Succ<Lease<GeometryBase>>(value: new Lease<GeometryBase>.Borrowed(Value: geometry));

    private static Fin<Lease<GeometryBase>> Detached(GeometryBase geometry, Op key) =>
        Copy(duplicate: geometry.Duplicate, key: key);

    private static Fin<Lease<GeometryBase>> Shared(GeometryBase geometry, Op key) =>
        Copy(duplicate: geometry.IsDocumentControlled ? geometry.Duplicate : geometry.DuplicateShallow, key: key);

    internal static Fin<Lease<GeometryBase>> Copy(Func<GeometryBase> duplicate, Op key) =>
        Optional(duplicate).ToFin(Fail: key.InvalidInput())
            .Bind(factory => key.Catch(() => Optional(factory()).ToFin(Fail: key.InvalidResult())))
            .Map(static geometry => (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: geometry));
}

[SmartEnum]
public sealed partial class GeometryComparison {
    public static readonly GeometryComparison Reference = new(compare: static (left, right) => ReferenceEquals(left, right));
    public static readonly GeometryComparison Crc = new(compare: static (left, right) =>
        GeometryCrc.Of(geometry: left) == GeometryCrc.Of(geometry: right));

    [UseDelegateFromConstructor]
    internal partial bool Compare(GeometryBase left, GeometryBase right);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BoundsFrame {
    private BoundsFrame() { }
    public sealed record AxisAligned(bool Accurate) : BoundsFrame;
    public sealed record Transformed(TransformSpec Motion, Context Domain) : BoundsFrame;
    public sealed record Oriented(Plane Value) : BoundsFrame;
}

[ComplexValueObject]
public sealed partial class GeometryBounds {
    public BoundsFrame Frame { get; }
    public Option<Vector3d> Inflation { get; }
}

public sealed record BoundsEvidence(
    BoundingBox Raw,
    BoundingBox Value,
    Point3d Center,
    Vector3d Diagonal,
    Arr<Point3d> Corners,
    Arr<Line> Edges) {
    internal static Fin<BoundsEvidence> Of(BoundingBox value, Option<Vector3d> inflation, Op key) =>
        from bounds in key.AcceptValue(value: value)
        from evidence in inflation.Match(
            Some: amount =>
                from admitted in Admit.Finite(vector: amount, key: key)
                from _ in guard(admitted.X >= 0.0 && admitted.Y >= 0.0 && admitted.Z >= 0.0, key.InvalidInput()).ToFin()
                from captured in key.Catch(() => {
                    BoundingBox expanded = bounds;
                    expanded.Inflate(xAmount: admitted.X, yAmount: admitted.Y, zAmount: admitted.Z);
                    return Fin.Succ(value: Capture(raw: bounds, value: expanded));
                })
                select captured,
            None: () => key.Catch(() => Fin.Succ(value: Capture(raw: bounds, value: bounds))))
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
    public sealed record World(BoundsEvidence Evidence, bool Accurate) : NativeBounds;
    public sealed record Moved(BoundsEvidence Evidence, Transform Motion, Option<Transform> Inverse) : NativeBounds;
    public sealed record Framed(BoundsEvidence Local, Box World, Plane Frame) : NativeBounds;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryMotion {
    private GeometryMotion() { }
    public sealed record Matrix(TransformSpec Value, Context Domain) : GeometryMotion;
    public sealed record Translation(Vector3d Vector) : GeometryMotion;
    public sealed record UniformScale(double Factor) : GeometryMotion;
    public sealed record Rotation(double AngleRadians, Vector3d Axis, Point3d Center) : GeometryMotion;

    internal Fin<MotionReceipt> Apply(GeometryBase geometry, Op key) => Switch(
        (Geometry: geometry, Op: key),
        matrix: static (state, edit) =>
            from domain in Optional(edit.Domain).ToFin(Fail: state.Op.MissingContext())
            from spec in Optional(edit.Value).ToFin(Fail: state.Op.InvalidInput())
            from value in Placement.Build(spec: spec, context: Some(domain), key: state.Op)
            from _ in state.Op.Confirm(success: state.Geometry.Transform(xform: value))
            let inverse = value.TryGetInverse(inverse: out Transform reversed) ? Some(reversed) : Option<Transform>.None
            let similarity = value.DecomposeSimilarity(
                translation: out Vector3d similarityTranslation,
                dilation: out double dilation,
                rotation: out Transform similarityRotation,
                tolerance: RhinoMath.ZeroTolerance)
            let rigidity = value.DecomposeRigid(
                translation: out Vector3d rigidTranslation,
                rotation: out Transform rigidRotation,
                tolerance: RhinoMath.ZeroTolerance)
            let affine = value.DecomposeAffine(
                translation: out Vector3d affineTranslation,
                linear: out Transform linear,
                rotation: out Transform affineRotation,
                diagonal: out Vector3d diagonal)
                ? Some((Translation: affineTranslation, Linear: linear, Rotation: affineRotation, Diagonal: diagonal))
                : Option<(Vector3d Translation, Transform Linear, Transform Rotation, Vector3d Diagonal)>.None
            select (MotionReceipt)new MotionReceipt.Matrix(
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
            from _ in Admit.Finite(vector: edit.Vector, key: state.Op)
            from __ in state.Op.Confirm(success: state.Geometry.Translate(translationVector: edit.Vector))
            select (MotionReceipt)new MotionReceipt.Native(
                Value: edit,
                Reverse: new GeometryMotion.Translation(Vector: -edit.Vector)),
        uniformScale: static (state, edit) =>
            from factor in state.Op.Finite(value: edit.Factor)
            from _nonzero in guard(Math.Abs(value: factor) > RhinoMath.ZeroTolerance, state.Op.InvalidInput()).ToFin()
            from _ in state.Op.Confirm(success: state.Geometry.Scale(scaleFactor: factor))
            select (MotionReceipt)new MotionReceipt.Native(
                Value: edit,
                Reverse: new GeometryMotion.UniformScale(Factor: 1.0 / factor)),
        rotation: static (state, edit) =>
            from angle in state.Op.Finite(value: edit.AngleRadians)
            from axis in Admit.Directional(value: edit.Axis, tolerance: RhinoMath.ZeroTolerance, key: state.Op)
            from _center in Admit.Finite(point: edit.Center, key: state.Op)
            from _ in state.Op.Confirm(success: state.Geometry.Rotate(
                angleRadians: angle,
                rotationAxis: axis,
                rotationCenter: edit.Center))
            select (MotionReceipt)new MotionReceipt.Native(
                Value: edit,
                Reverse: new GeometryMotion.Rotation(
                    AngleRadians: -angle,
                    Axis: axis,
                    Center: edit.Center)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MotionReceipt {
    private MotionReceipt() { }
    public sealed record Matrix(
        Transform Value,
        Option<Transform> Inverse,
        TransformSimilarityType Similarity,
        Vector3d SimilarityTranslation,
        double Dilation,
        Transform SimilarityRotation,
        int Rigidity,
        Vector3d RigidTranslation,
        Transform RigidRotation,
        Option<(Vector3d Translation, Transform Linear, Transform Rotation, Vector3d Diagonal)> Affine) : MotionReceipt;
    public sealed record Native(GeometryMotion Value, GeometryMotion Reverse) : MotionReceipt;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TagOp {
    private TagOp() { }
    public sealed record Read(string Key) : TagOp;
    public sealed record ReadAll : TagOp;
    public sealed record Set(string Key, string Value) : TagOp;
    public sealed record Delete(string Key) : TagOp;
    public sealed record Clear : TagOp;

    internal bool Mutates => this is Set or Delete or Clear;

    internal Fin<TagResult> Apply(GeometryBase geometry, Op key) => Switch(
        (Geometry: geometry, Op: key),
        read: static (state, tag) =>
            from name in state.Op.AcceptText(value: tag.Key)
            select (TagResult)new TagResult.Value(Key: name, Stored: Optional(state.Geometry.GetUserString(key: name))),
        readAll: static (state, _) => Fin.Succ<TagResult>(value: new TagResult.Snapshot(Stored: Snapshot(state.Geometry))),
        set: static (state, tag) =>
            from name in state.Op.AcceptText(value: tag.Key)
            from value in Optional(tag.Value).ToFin(Fail: state.Op.InvalidInput())
            let before = Optional(state.Geometry.GetUserString(key: name))
            from _ in state.Op.Confirm(success: state.Geometry.SetUserString(key: name, value: value))
            let after = Optional(state.Geometry.GetUserString(key: name))
            from __ in state.Op.Confirm(success: after.Equals(Some(value)))
            select (TagResult)new TagResult.Changed(Key: name, Before: before, After: after),
        delete: static (state, tag) =>
            from name in state.Op.AcceptText(value: tag.Key)
            let before = Optional(state.Geometry.GetUserString(key: name))
            from _ in before.IsSome
                ? state.Op.Confirm(success: state.Geometry.DeleteUserString(key: name))
                : Fin.Succ(value: unit)
            let after = Optional(state.Geometry.GetUserString(key: name))
            from __ in state.Op.Confirm(success: after.IsNone)
            select (TagResult)new TagResult.Changed(Key: name, Before: before, After: after),
        clear: static (state, _) =>
            from before in Fin.Succ(Snapshot(state.Geometry))
            from _ in state.Op.Catch(() => {
                state.Geometry.DeleteAllUserStrings();
                return Fin.Succ(value: unit);
            })
            let after = Snapshot(state.Geometry)
            from __ in state.Op.Confirm(success: after.IsEmpty)
            select (TagResult)new TagResult.Cleared(Before: before, After: after));

    internal static HashMap<string, string> Snapshot(GeometryBase geometry) {
        System.Collections.Specialized.NameValueCollection native = geometry.GetUserStrings();
        return toSeq(native.AllKeys)
            .Choose(name => Optional(name).Bind(key => Optional(native[key]).Map(value => (Key: key, Value: value))))
            .Fold(HashMap<string, string>(), static (map, pair) => map.AddOrUpdate(key: pair.Key, value: pair.Value));
    }
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
    public sealed record NativeBounds(GeometryBounds Query) : GeometryOp;
    public sealed record Clip(ClipOp Value) : GeometryOp;

    internal bool Mutates => Switch(
        inspect: static _ => false,
        crc: static _ => false,
        tag: static operation => operation.Value.Mutates,
        transform: static _ => true,
        nativeBounds: static _ => false,
        clip: static operation => operation.Value.Mutates);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeometryResult {
    private GeometryResult() { }
    public sealed record Facts(GeometryFacts Value) : GeometryResult;
    public sealed record Compared(bool Equal) : GeometryResult;
    public sealed record Hashed(GeometryCrc Value) : GeometryResult;
    public sealed record Tagged(TagResult Value) : GeometryResult;
    public sealed record Transformed(MotionReceipt Motion) : GeometryResult;
    public sealed record Bounded(NativeBounds Value) : GeometryResult;
    public sealed record Clipped(ClipTransition Value) : GeometryResult;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HandleRelease {
    private HandleRelease() { }
    public sealed record Live : HandleRelease;
    public sealed record Released : HandleRelease;
    public sealed record Faulted(Seq<Error> Errors) : HandleRelease;

    internal bool Active => this is Live;
}

[ComplexValueObject]
public sealed partial class ClipSet {
    public Seq<Guid> Objects { get; }
    public Seq<int> Layers { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<Guid> objects,
        ref Seq<int> layers) {
        bool valid = !objects.Exists(static id => id == Guid.Empty) && !layers.Exists(static index => index < 0);
        objects = objects.AsIterable().Distinct().Order().ToSeq();
        layers = layers.AsIterable().Distinct().Order().ToSeq();
        validationError = valid ? null : new ValidationError(message: "Clip membership contains an invalid object id or layer index.");
    }

    public static Fin<ClipSet> Of(Seq<Guid> objects, Seq<int> layers, Op key) =>
        Admission.Admitted(fault: Validate(objects, layers, out ClipSet? admitted), value: admitted, refusal: key.InvalidInput());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ClippingPlaneSeed {
    private ClippingPlaneSeed() { }
    public sealed record Frame(Plane Value) : ClippingPlaneSeed;
    public sealed record Surface(PlaneSurface Value) : ClippingPlaneSeed;

    internal Fin<Lease<GeometryBase>> Build(Op key) => Switch(
        key,
        frame: static (op, seed) =>
            from plane in Admit.Plane(basis: seed.Value, key: op)
            select (Lease<GeometryBase>)new Lease<GeometryBase>.Owned(Value: new ClippingPlaneSurface(plane: plane)),
        surface: static (op, seed) =>
            from plane in op.AcceptInput(value: seed.Value)
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

    internal Fin<Seq<Guid>> Resolve(Seq<Guid> before, Op key) =>
        from current in Admit(before, key)
        from desired in Switch(
            (Current: current, Op: key),
            add: static (state, edit) => Admit(edit.Ids, state.Op).Map(ids => Canonical(state.Current + ids)),
            remove: static (state, edit) => Admit(edit.Ids, state.Op)
                .Map(ids => state.Current.Filter(id => !ids.Exists(candidate => candidate == id))),
            replace: static (state, edit) => Admit(edit.Ids, state.Op))
        select desired;

    private static Fin<Seq<Guid>> Admit(Seq<Guid> ids, Op key) =>
        ids.Exists(static id => id == Guid.Empty)
            ? Fin.Fail<Seq<Guid>>(error: key.InvalidInput())
            : Fin.Succ(value: Canonical(ids));

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

    internal bool Mutates => this is not Read;
}

// --- [MODELS] ----------------------------------------------------------------------------
public sealed record GeometryFacts(
    ObjectType NativeType,
    bool DocumentControlled,
    bool Shallow,
    bool Valid,
    Option<string> Invalidity,
    GeometryCrc Content,
    HashMap<string, string> Tags);

public sealed record GeometryReceipt(
    GeometryResult Result,
    GeometryCrc Before,
    GeometryCrc After,
    Seq<Error> CleanupFaults);

public sealed record ClipState(
    ClipScope Scope,
    Option<double> Depth,
    Seq<Guid> ViewportIds,
    Option<Guid> DimensionStyleId);

public readonly record struct ClipTransition(ClipState Before, ClipState After);

public sealed class GeometryHandle : IDisposable {
    private static long sequence;
    private readonly Lock gate = new();
    private readonly long ordinal = Interlocked.Increment(location: ref sequence);
    private Lease<GeometryBase> lease;
    private Seq<Lease<GeometryBase>> pending = Seq<Lease<GeometryBase>>();
    private readonly CrossingMode mode;
    private HandleRelease release = new HandleRelease.Live();

    internal GeometryHandle(Lease<GeometryBase> lease, CrossingMode mode) {
        this.lease = lease;
        this.mode = mode;
    }

    public CrossingMode Mode => mode;
    public HandleRelease Release { get { lock (gate) { return release; } } }

    public Fin<GeometryReceipt> Apply(GeometryOp operation, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(operation).ToFin(Fail: op.InvalidInput())
            .Bind(request => Operate(operation: request, key: op));
    }

    public Fin<GeometryReceipt> Compare(GeometryHandle other, GeometryComparison policy, Op? key = null) {
        Op op = key.OrDefault();
        return from target in Optional(other).ToFin(Fail: op.InvalidInput())
               from rule in Optional(policy).ToFin(Fail: op.InvalidInput())
               from receipt in Matched(other: target, policy: rule, key: op)
               select receipt;
    }

    public Fin<Seq<TOut>> Bounds<TOut>(Rasm.Analysis.Bounds request, Context context, Op? key = null) where TOut : notnull {
        Op op = key.OrDefault();
        return from query in Optional(request).ToFin(Fail: op.InvalidInput())
               from domain in Optional(context).ToFin(Fail: op.MissingContext())
               from result in With(
                   key: op,
                   project: geometry => Analyze.In(context: domain)
                       .Run(
                           operation: Analyze.Query<GeometryBase, TOut>(query: AnalysisQuery.Bounds(query: query), key: op),
                           input: geometry)
                       .ToFin())
               select result;
    }

    public void Dispose() {
        lock (gate) {
            if (release is HandleRelease.Released && pending.IsEmpty) {
                return;
            }
            Op op = Op.Of(name: nameof(Dispose));
            Seq<Error> faults = release.Active ? Retain(candidate: lease, key: op) : Retry(key: op);
            release = faults.IsEmpty
                ? new HandleRelease.Released()
                : new HandleRelease.Faulted(Errors: faults);
        }
    }

    internal Fin<TResult> With<TResult>(Op key, Func<GeometryBase, Fin<TResult>> project) {
        lock (gate) {
            return release.Active
                ? Optional(project).ToFin(Fail: key.InvalidInput())
                    .Bind(body => key.Catch(() => key.AcceptInput(value: lease.Resource).Bind(body)))
                : Fin.Fail<TResult>(error: key.InvalidInput());
        }
    }

    private Fin<GeometryReceipt> Operate(GeometryOp operation, Op key) =>
        operation.Mutates
            ? Change(operation: operation, key: key)
            : With(key: key, project: geometry =>
                from result in Evaluate(geometry: geometry, operation: operation, key: key)
                let crc = GeometryCrc.Of(geometry: geometry)
                select new GeometryReceipt(Result: result, Before: crc, After: crc, CleanupFaults: Seq<Error>()));

    private Fin<GeometryReceipt> Matched(GeometryHandle other, GeometryComparison policy, Op key) {
        Fin<GeometryReceipt> EvaluateActive() =>
            key.Catch(() =>
                !release.Active || !other.release.Active
                    ? Fin.Fail<GeometryReceipt>(error: key.InvalidInput())
                    : from left in key.AcceptInput(value: lease.Resource)
                      from right in key.AcceptInput(value: other.lease.Resource)
                      let before = GeometryCrc.Of(geometry: left)
                      select new GeometryReceipt(
                          Result: new GeometryResult.Compared(Equal: policy.Compare(left: left, right: right)),
                          Before: before,
                          After: before,
                          CleanupFaults: Seq<Error>()));
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

    private Fin<GeometryReceipt> Change(GeometryOp operation, Op key) {
        lock (gate) {
            if (!release.Active || !mode.Mutable) {
                return Fin.Fail<GeometryReceipt>(error: key.InvalidInput());
            }
            return key.Catch(() =>
                from active in key.AcceptInput(value: lease.Resource)
                let before = GeometryCrc.Of(geometry: active)
                from working in CrossingMode.Copy(duplicate: active.Duplicate, key: key)
                select (Working: working, Before: before)).Bind(prepared =>
                    key.Catch(() => Evaluate(geometry: prepared.Working.Resource, operation: operation, key: key)).Match(
                        Succ: result => Commit(working: prepared.Working, before: prepared.Before, result: result, key: key),
                        Fail: error => Fin.Fail<GeometryReceipt>(error: Retain(candidate: prepared.Working, key: key)
                            .Fold(error, static (primary, cleanup) => primary + cleanup))));
        }
    }

    private Fin<GeometryReceipt> Commit(Lease<GeometryBase> working, GeometryCrc before, GeometryResult result, Op key) =>
        key.AcceptValue(value: working.Resource).Match(
            Succ: admitted => {
                GeometryCrc after = GeometryCrc.Of(geometry: admitted);
                Lease<GeometryBase> previous = lease;
                lease = working;                                              // Exemption: the capsule lease swap is the boundary mutation the commit owns
                Seq<Error> cleanup = Retain(candidate: previous, key: key);
                return Fin.Succ(value: new GeometryReceipt(
                    Result: result,
                    Before: before,
                    After: after,
                    CleanupFaults: cleanup));
            },
            Fail: error => Fin.Fail<GeometryReceipt>(error: Retain(candidate: working, key: key)
                .Fold(error, static (primary, cleanup) => primary + cleanup)));

    private Seq<Error> Retain(Lease<GeometryBase> candidate, Op key) {
        pending = pending.Add(value: candidate);
        return Retry(key: key);
    }

    private Seq<Error> Retry(Op key) {
        (Seq<Lease<GeometryBase>> Retry, Seq<Error> Errors) outcome = pending.Fold(
            (Retry: Seq<Lease<GeometryBase>>(), Errors: Seq<Error>()),
            (state, candidate) => DisposeLease(candidate, key).Match(
                Succ: _ => state,
                Fail: error => (
                    Retry: state.Retry.Add(value: candidate),
                    Errors: state.Errors.Add(value: error))));
        pending = outcome.Retry;
        return outcome.Errors;
    }

    private static Fin<GeometryResult> Evaluate(GeometryBase geometry, GeometryOp operation, Op key) =>
        operation.Switch(
            (Geometry: geometry, Op: key),
            inspect: static (state, _) => Fin.Succ<GeometryResult>(value: new GeometryResult.Facts(Value: Facts(state.Geometry))),
            crc: static (state, request) => Fin.Succ<GeometryResult>(value: new GeometryResult.Hashed(
                Value: GeometryCrc.Create(value: state.Geometry.DataCRC(currentRemainder: request.Chain)))),
            tag: static (state, request) => Optional(request.Value).ToFin(Fail: state.Op.InvalidInput())
                .Bind(tags => tags.Apply(state.Geometry, state.Op))
                .Map(static value => (GeometryResult)new GeometryResult.Tagged(Value: value)),
            transform: static (state, request) => Optional(request.Motion).ToFin(Fail: state.Op.InvalidInput())
                .Bind(motion => motion.Apply(state.Geometry, state.Op))
                .Map(static value => (GeometryResult)new GeometryResult.Transformed(Motion: value)),
            nativeBounds: static (state, request) => Bound(state.Geometry, request.Query, state.Op)
                .Map(static value => (GeometryResult)new GeometryResult.Bounded(Value: value)),
            clip: static (state, request) => Optional(request.Value).ToFin(Fail: state.Op.InvalidInput())
                .Bind(clip => clip.Apply(state.Geometry, state.Op))
                .Map(static value => (GeometryResult)new GeometryResult.Clipped(Value: value)));

    private static GeometryFacts Facts(GeometryBase geometry) {
        bool valid = geometry.IsValidWithLog(out string log);
        return new GeometryFacts(
            NativeType: geometry.ObjectType,
            DocumentControlled: geometry.IsDocumentControlled,
            Shallow: geometry.IsShallowDuplicate,
            Valid: valid,
            Invalidity: valid ? Option<string>.None : Optional(log).Filter(static detail => detail.Length > 0),
            Content: GeometryCrc.Of(geometry: geometry),
            Tags: TagOp.Snapshot(geometry));
    }

    private static Fin<NativeBounds> Bound(GeometryBase geometry, GeometryBounds query, Op key) =>
        from request in Optional(query).ToFin(Fail: key.InvalidInput())
        from frame in Optional(request.Frame).ToFin(Fail: key.InvalidInput())
        from result in frame.Switch(
            (Geometry: geometry, Inflation: request.Inflation, Op: key),
            axisAligned: static (state, bounds) =>
                from value in state.Op.Catch(() => Fin.Succ(value: state.Geometry.GetBoundingBox(accurate: bounds.Accurate)))
                from evidence in BoundsEvidence.Of(value, state.Inflation, state.Op)
                select (NativeBounds)new NativeBounds.World(Evidence: evidence, Accurate: bounds.Accurate),
            transformed: static (state, bounds) =>
                from domain in Optional(bounds.Domain).ToFin(Fail: state.Op.MissingContext())
                from spec in Optional(bounds.Motion).ToFin(Fail: state.Op.InvalidInput())
                from motion in Placement.Build(spec: spec, context: Some(domain), key: state.Op)
                from value in state.Op.Catch(() => Fin.Succ(value: state.Geometry.GetBoundingBox(xform: motion)))
                from evidence in BoundsEvidence.Of(value, state.Inflation, state.Op)
                let inverse = motion.TryGetInverse(inverse: out Transform reversed) ? Some(reversed) : Option<Transform>.None
                select (NativeBounds)new NativeBounds.Moved(Evidence: evidence, Motion: motion, Inverse: inverse),
            oriented: static (state, bounds) =>
                from frame in Admit.Plane(basis: bounds.Value, key: state.Op)
                from raw in state.Op.Catch(() => {
                    BoundingBox local = state.Geometry.GetBoundingBox(plane: frame, worldBox: out Box world);
                    return Fin.Succ(value: (Local: local, World: world));
                })
                from evidence in BoundsEvidence.Of(raw.Local, state.Inflation, state.Op)
                select (NativeBounds)new NativeBounds.Framed(Local: evidence, World: raw.World, Frame: frame))
        select result;

    private static Fin<Unit> DisposeLease(Lease<GeometryBase> lease, Op key) =>
        key.Catch(() => Fin.Succ(value: lease.Dispose()));
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class GeometryCrossing {
    public static Fin<GeometryHandle> Cross(object source, CrossingMode mode, Op? key = null) {
        Op op = key.OrDefault();
        return from value in Optional(source).ToFin(Fail: op.InvalidInput())
               from custody in Optional(mode).ToFin(Fail: op.InvalidInput())
               from admitted in value is ClippingPlaneSeed seed
                   ? seed.Build(key: op).Map(static lease => (Lease: lease, Mode: CrossingMode.Detach))
                   : value.GeometryForm(key: op).Bind(form => form.Switch(
                       (Mode: custody, Op: op),
                       owned: static (_, owned) => Fin.Succ((Lease: (Lease<GeometryBase>)owned, Mode: CrossingMode.Detach)),
                       borrowed: static (state, borrowed) => state.Mode.Acquire(borrowed.Value, state.Op)
                           .Map(lease => (Lease: lease, Mode: state.Mode))))
               select new GeometryHandle(lease: admitted.Lease, mode: admitted.Mode);
    }
}

public abstract partial record ClipOp {
    internal Fin<ClipTransition> Apply(GeometryBase geometry, Op key) =>
        geometry is ClippingPlaneSurface surface
            ? from before in State(surface, key)
              from _ in this.Switch(
                  (Surface: surface, Before: before, Op: key),
                  read: static (_, _) => Fin.Succ(value: unit),
                  scope: static (state, edit) => Scope(state.Surface, edit.Value, state.Op),
                  depth: static (state, edit) => Depth(state.Surface, edit.Value, state.Op),
                  viewports: static (state, edit) => Viewports(state.Surface, state.Before.ViewportIds, edit.Value, state.Op),
                  style: static (state, edit) => Style(state.Surface, edit.DimensionStyleId, state.Op))
              from after in State(surface, key)
              from __ in Confirm(this, before, after, key)
              select new ClipTransition(Before: before, After: after)
            : Fin.Fail<ClipTransition>(error: key.InvalidInput());

    private static Fin<ClipState> State(ClippingPlaneSurface surface, Op key) =>
        key.Catch(() => {
            Seq<Guid> viewports = surface.ViewportIds().AsIterable().Distinct().Order().ToSeq();
            Fin<ClipScope> scope = surface.ParticipationListsEnabled
                ? ScopeOf(surface, key)
                : Fin.Succ<ClipScope>(value: new ClipScope.Everything());
            Fin<Option<double>> depth = surface.PlaneDepthEnabled
                ? key.Positive(value: surface.PlaneDepth).Map(static value => Some(value))
                : Fin.Succ(Option<double>.None);
            return from admittedScope in scope
                   from admittedDepth in depth
                   from _ in viewports.Exists(static id => id == Guid.Empty)
                       ? Fin.Fail<Unit>(error: key.InvalidResult())
                       : Fin.Succ(value: unit)
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
        return ClipSet.Of(toSeq(objects), toSeq(layers), key).Map(set =>
            exclusion ? (ClipScope)new ClipScope.Except(Members: set) : new ClipScope.Only(Members: set));
    }

    private static Fin<Unit> Scope(ClippingPlaneSurface surface, ClipScope scope, Op key) =>
        Optional(scope).ToFin(Fail: key.InvalidInput()).Bind(request => request.Switch(
            (Surface: surface, Op: key),
            everything: static (state, _) => state.Op.Catch(() => {
                state.Surface.ClearClipParticipationLists();
                state.Surface.ParticipationListsEnabled = false;
                return Fin.Succ(value: unit);
            }),
            only: static (state, set) => SetScope(state.Surface, set.Members, exclusion: false, state.Op),
            except: static (state, set) => SetScope(state.Surface, set.Members, exclusion: true, state.Op)));

    private static Fin<Unit> SetScope(ClippingPlaneSurface surface, ClipSet set, bool exclusion, Op key) =>
        Optional(set).ToFin(Fail: key.InvalidInput()).Bind(members => key.Catch(() => {
            surface.SetClipParticipation(members.Objects.AsIterable(), members.Layers.AsIterable(), isExclusionList: exclusion);
            surface.ParticipationListsEnabled = true;
            return Fin.Succ(value: unit);
        }));

    private static Fin<Unit> Depth(ClippingPlaneSurface surface, Option<double> depth, Op key) =>
        depth.Match(
            Some: value => key.Positive(value).Bind(admitted => key.Catch(() => {
                surface.PlaneDepth = admitted;
                surface.PlaneDepthEnabled = true;
                return Fin.Succ(value: unit);
            })),
            None: () => key.Catch(() => {
                surface.PlaneDepthEnabled = false;
                return Fin.Succ(value: unit);
            }));

    private static Fin<Unit> Style(ClippingPlaneSurface surface, Option<Guid> style, Op key) =>
        style.Match(
            Some: id => id == Guid.Empty
                ? Fin.Fail<Unit>(error: key.InvalidInput())
                : key.Catch(() => { surface.DimensionStyleId = id; return Fin.Succ(value: unit); }),
            None: () => key.Catch(() => { surface.DimensionStyleId = Guid.Empty; return Fin.Succ(value: unit); }));

    private static Fin<Unit> Viewports(ClippingPlaneSurface surface, Seq<Guid> before, ViewportOp operation, Op key) =>
        from request in Optional(operation).ToFin(Fail: key.InvalidInput())
        from desired in request.Resolve(before, key)
        from _ in before.Filter(id => !desired.Exists(candidate => candidate == id))
            .TraverseM(id => key.Confirm(success: surface.RemoveClipViewportId(viewportId: id))).As()
        from __ in desired.Filter(id => !before.Exists(candidate => candidate == id))
            .TraverseM(id => key.Confirm(success: surface.AddClipViewportId(viewportId: id))).As()
        select unit;

    private static Fin<Unit> Confirm(ClipOp operation, ClipState before, ClipState after, Op key) =>
        operation.Switch(
            (Before: before, After: after, Op: key),
            read: static (state, _) => state.Op.Confirm(success: state.Before.Equals(state.After)),
            scope: static (state, edit) => state.Op.Confirm(success: edit.Value.Equals(state.After.Scope)),
            depth: static (state, edit) => state.Op.Confirm(success: edit.Value.Equals(state.After.Depth)),
            viewports: static (state, edit) => edit.Value.Resolve(state.Before.ViewportIds, state.Op)
                .Bind(expected => state.Op.Confirm(success: expected.Equals(state.After.ViewportIds))),
            style: static (state, edit) => state.Op.Confirm(success: edit.DimensionStyleId.Equals(state.After.DimensionStyleId)));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
