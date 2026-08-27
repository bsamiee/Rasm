# `Numerics/atoms.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Numerics/atoms.md`

Apply the moves in order. Counts are authored, nonblank C# fence lines; generated Thinktecture members and required prose ripples are excluded. Under the exact formatting below, the target loses 117 fenced LOC, 20 declared members, 13 publicly reachable members, and two module-level types. No move adds a type, helper, enum, carrier, policy rail, or second ownership surface.

Authority: `CLAUDE.md`; the owning `libs/`, `libs/dotnet/`, and `libs/dotnet/Rasm/` planning and ruling surfaces; all of `docs/stacks/csharp/`; `libs/dotnet/.api/api-languageext.md`; `libs/dotnet/.api/api-thinktecture-runtime-extensions.md`; `libs/dotnet/.api/api-rhinocommon.md`; `libs/dotnet/Rasm/.api/api-rhino.md`; `libs/dotnet/.api/api-unicolour.md`; and direct consumers across `libs/dotnet/`.

## 1. Put the cubic reconstruction body on its Thinktecture row and retain only consumed row metadata

**Location:** `libs/dotnet/Rasm/.planning/Numerics/atoms.md`, anchors `LatticeInterpolation.Nearest` through private `CatmullRom`.

**From:**

```csharp
public static readonly LatticeInterpolation Nearest = new(key: 0, support: 0, continuity: 0,
    axis: static (tap, _) => tap(arg: 0));
public static readonly LatticeInterpolation Linear  = new(key: 1, support: 1, continuity: 0,
    axis: static (tap, t) => double.Lerp(tap(arg: 0), tap(arg: 1), t));
public static readonly LatticeInterpolation Cubic   = new(key: 2, support: 2, continuity: 1,
    axis: static (tap, t) => CatmullRom(p0: tap(arg: -1), p1: tap(arg: 0), p2: tap(arg: 1), p3: tap(arg: 2), t: t));
internal int Support { get; }
internal int Continuity { get; }
internal double CenterOffset => Support == 0 ? 0.0 : 0.5;

private static double CatmullRom(double p0, double p1, double p2, double p3, double t) =>
    p1 + (0.5 * t * (p2 - p0 + (t * ((2.0 * p0) - (5.0 * p1) + (4.0 * p2) - p3 + (t * ((3.0 * (p1 - p2)) + p3 - p0))))));
```

**To:**

```csharp
public static readonly LatticeInterpolation Nearest = new(key: 0, centerOffset: 0.0,
    axis: static (tap, _) => tap(arg: 0));
public static readonly LatticeInterpolation Linear  = new(key: 1, centerOffset: 0.5,
    axis: static (tap, t) => double.Lerp(tap(arg: 0), tap(arg: 1), t));
public static readonly LatticeInterpolation Cubic   = new(key: 2, centerOffset: 0.5,
    axis: static (tap, t) => {
        double p0 = tap(-1), p1 = tap(0), p2 = tap(1), p3 = tap(2);
        return p1 + (0.5 * t * (p2 - p0 + (t * ((2.0 * p0) - (5.0 * p1) + (4.0 * p2) - p3 + (t * ((3.0 * (p1 - p2)) + p3 - p0))))));
    });
internal double CenterOffset { get; }
```

**Effect:** fenced LOC `11 -> 10` (`-1`); declared members `-3`; each tap is still sampled exactly once. The row retains the only metadata any consumer reads, while two unused integer columns and the single-use polynomial member disappear.

**API/consumer proof:** `[UseDelegateFromConstructor]` makes `Axis` the behavior column of each `LatticeInterpolation` row. `CatmullRom` has exactly one caller, the `Cubic` row, so the private method is a one-hop name over that row rather than an independently reusable numeric owner. The four explicit locals preserve the current one-read-per-tap behavior without the one-arm tuple switch and `var` that the C# language law rejects. Across `libs/dotnet/`, `Spatial/fields.md` consumes only `Axis` and `CenterOffset`; neither `Support` nor `Continuity` has a read. Encoding the offset directly therefore preserves every live behavior without keeping unobserved reconstruction taxonomy.

**Ripples:** none.

## 2. Collapse `OrientationSense` into the boolean column it actually is

`OrientationSense` is a two-case, payloadless public type whose only two methods translate host enum results. It has no direct consumer outside this fence and no behavior a boolean column fails to carry. Preserve the admitted host-status gate; remove the type.

### 2a. Delete the degenerate smart enum

**Location:** same file, anchors `[SmartEnum<int>] public sealed partial class OrientationSense` through its closing brace.

**From:**

```csharp
[SmartEnum<int>]
public sealed partial class OrientationSense {
    public static readonly OrientationSense Reversing = new(key: -1);
    public static readonly OrientationSense Preserving = new(key: 1);

    internal static Fin<OrientationSense> Of(TransformSimilarityType value, Op key) =>
        value switch {
            TransformSimilarityType.OrientationReversing => Fin.Succ(Reversing),
            TransformSimilarityType.OrientationPreserving => Fin.Succ(Preserving),
            TransformSimilarityType.NotSimilarity => Fin.Fail<OrientationSense>(error: key.InvalidResult()),
            _ => Fin.Fail<OrientationSense>(error: key.InvalidResult()),
        };

    internal static Fin<OrientationSense> Of(TransformRigidType value, Op key) =>
        value switch {
            TransformRigidType.RigidReversing => Fin.Succ(Reversing),
            TransformRigidType.Rigid => Fin.Succ(Preserving),
            TransformRigidType.NotRigid => Fin.Fail<OrientationSense>(error: key.InvalidResult()),
            _ => Fin.Fail<OrientationSense>(error: key.InvalidResult()),
        };
}
```

**To:**

```csharp
```

### 2b. Put the boolean fact on the two result cases

**Location:** same file, `Decomposition.Similarity` and `Decomposition.Rigid`.

**From:**

```csharp
public sealed record Similarity(Vector3d Translation, double Dilation, Transform Rotation, OrientationSense Orientation) : Decomposition;
public sealed record Rigid(Vector3d Translation, Transform Rotation, OrientationSense Orientation) : Decomposition;
```

**To:**

```csharp
public sealed record Similarity(Vector3d Translation, double Dilation, Transform Rotation, bool ReversesOrientation) : Decomposition;
public sealed record Rigid(Vector3d Translation, Transform Rotation, bool ReversesOrientation) : Decomposition;
```

### 2c. Gate the host result exhaustively and project the boolean in place

**Location:** same file, the return expressions in `SimilarityOf` and `RigidOf`.

**From:**

```csharp
return from orientation in OrientationSense.Of(value: kind, key: key)
       from result in (key.AcceptValue(value: translation), key.AcceptValue(value: dilation), key.AcceptValue(value: rotation))
           .Apply((move, scale, spin) => (Decomposition)new Decomposition.Similarity(
               Translation: move,
               Dilation: scale,
               Rotation: spin,
               Orientation: orientation))
           .As()
       select result;
```

**To:**

```csharp
return kind is TransformSimilarityType.OrientationReversing or TransformSimilarityType.OrientationPreserving
    ? (key.AcceptValue(translation), key.AcceptValue(dilation), key.AcceptValue(rotation))
        .Apply((move, scale, spin) => (Decomposition)new Decomposition.Similarity(move, scale, spin,
            ReversesOrientation: kind is TransformSimilarityType.OrientationReversing)).As()
    : Fin.Fail<Decomposition>(key.InvalidResult());
```

**From:**

```csharp
return from orientation in OrientationSense.Of(value: kind, key: key)
       from result in (key.AcceptValue(value: translation), key.AcceptValue(value: rotation))
           .Apply((move, spin) => (Decomposition)new Decomposition.Rigid(
               Translation: move,
               Rotation: spin,
               Orientation: orientation))
           .As()
       select result;
```

**To:**

```csharp
return kind is TransformRigidType.RigidReversing or TransformRigidType.Rigid
    ? (key.AcceptValue(translation), key.AcceptValue(rotation))
        .Apply((move, spin) => (Decomposition)new Decomposition.Rigid(move, spin,
            ReversesOrientation: kind is TransformRigidType.RigidReversing)).As()
    : Fin.Fail<Decomposition>(key.InvalidResult());
```

**Effect:** fenced LOC `19 -> 0` for the deleted type and the two consumers `17 -> 10` (`-26` total); module-level types `-1`; declared members `-4` (two rows and two conversion methods); the two decomposition case arities remain unchanged.

**API/consumer proof:** RhinoCommon returns exactly `OrientationReversing`/`OrientationPreserving` or `NotSimilarity`, and `RigidReversing`/`Rigid` or `NotRigid`. The outer pattern admits only the two successful host outcomes and refuses the named failure plus unknown future enum values before touching their undefined out-values; this preserves the current dependent query semantics instead of incorrectly adding the host status as an independent `Apply` arm. The tuple `Apply` remains only over the independent outputs of an admitted decomposition. A repository-wide exact-name scan finds no `OrientationSense` or `Decomposition.Similarity`/`.Rigid` consumer outside this fence, so no caller depends on row identity or `.Key`.

**Ripples:** update this section's Owner/Auto/Output/Growth prose to describe `ReversesOrientation`; no code fence outside the target changes.

## 3. Collapse `ChainClosure` into `bool isClosed` and delete the second spelling

**Location:** same file, `[SmartEnum<int>] public sealed partial class ChainClosure` and `VectorFrame.Chain`.

**From:**

```csharp
[SmartEnum<int>]
public sealed partial class ChainClosure {
    public static readonly ChainClosure Open = new(key: 0);
    public static readonly ChainClosure Closed = new(key: 1);
}
```

**To:**

```csharp
```

**From:**

```csharp
public static Fin<Seq<VectorFrame>> Chain(Seq<Point3d> points, Direction initialNormal, ChainClosure closure, Context context, Op? key = null) =>
    NeighborKernel.BishopChain(points: points, initialNormal: initialNormal, closure: closure, context: context, key: key.OrDefault())
```

**To:**

```csharp
public static Fin<Seq<VectorFrame>> Chain(Seq<Point3d> points, Direction initialNormal, bool isClosed, Context context, Op? key = null) =>
    NeighborKernel.BishopChain(points: points, initialNormal: initialNormal, isClosed: isClosed, context: context, key: key.OrDefault())
```

**Effect:** fenced LOC `5 -> 0` (`-5`); module-level types `-1`; declared members `-2`; `VectorFrame.Chain` keeps one clearly named binary column and the same arity.

**API/consumer proof:** the smart enum carries neither payload nor delegate behavior. Its sole consumer immediately evaluates `closure.Equals(ChainClosure.Closed)` into a local `bool closed`; this is exactly the forbidden second spelling of a binary fact. Thinktecture adds generated surface here but no domain capability.

**Ripples:** in `libs/dotnet/Rasm/.planning/Spatial/neighbors.md`, pass `isClosed: true` for `VectorCloud.RingCase`, `isClosed: false` for `PolylineCase`, change the internal `BishopChain` parameter to `bool isClosed`, delete `bool closed = ...`, and read `isClosed` in the final-tangent branch. Rewrite that section's law; its present “never a boolean” ruling preserves the redundant type rather than a survival discriminant. No other file names `ChainClosure`.

## 4. Inline the three single-use `TransformRewrite` implementations into their Thinktecture rows

Each delegate is already the generated behavior column. The three private methods are a second dispatch layer.

### 4a. Inline `AffineOf`

**Location:** same file, row `TransformRewrite.Affine` and helper `AffineOf`.

**From:**

```csharp
public static readonly TransformRewrite Affine = new(apply: AffineOf);
private static Fin<Transform> AffineOf(Transform source, Context context, Op key) {
    Transform rewritten = source;
    rewritten.Affineize();
    return key.AcceptValue(value: rewritten);
}
```

**To:**

```csharp
public static readonly TransformRewrite Affine = new(apply: static (source, _, key) => {
    source.Affineize();
    return key.AcceptValue(source);
});
```

### 4b. Inline `LinearOf`

**Location:** same file, row `TransformRewrite.Linear` and helper `LinearOf`.

**From:**

```csharp
public static readonly TransformRewrite Linear = new(apply: LinearOf);
private static Fin<Transform> LinearOf(Transform source, Context context, Op key) {
    Transform rewritten = source;
    rewritten.Linearize();
    return key.AcceptValue(value: rewritten);
}
```

**To:**

```csharp
public static readonly TransformRewrite Linear = new(apply: static (source, _, key) => {
    source.Linearize();
    return key.AcceptValue(source);
});
```

### 4c. Inline `OrthogonalOf`

**Location:** same file, row `TransformRewrite.Orthogonal` and helper `OrthogonalOf`.

**From:**

```csharp
public static readonly TransformRewrite Orthogonal = new(apply: OrthogonalOf);
private static Fin<Transform> OrthogonalOf(Transform source, Context context, Op key) {
    Transform rewritten = source;
    double tolerance = Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: context.Fractional);
    return rewritten.Orthogonalize(tolerance: tolerance)
        ? key.AcceptValue(value: rewritten)
        : Fin.Fail<Transform>(error: key.InvalidResult());
}
```

**To:**

```csharp
public static readonly TransformRewrite Orthogonal = new(apply: static (source, context, key) =>
    source.Orthogonalize(Math.Max(EpsilonPolicy.SqrtEpsilon, context.Fractional))
        ? key.AcceptValue(source)
        : Fin.Fail<Transform>(error: key.InvalidResult()));
```

**Effect:** fenced LOC `20 -> 12` (`-8`); declared members `-3`; method-local bindings `-4`; behavior rows and generated exhaustive surface remain unchanged.

**API/consumer proof:** `api-thinktecture-runtime-extensions.md` makes `[UseDelegateFromConstructor]` the behavior-bearing row mechanism. `Transform` is passed by value, so mutating the lambda parameter preserves the current copy-before-mutation contract without a second local. RhinoCommon's `Affineize`, `Linearize`, and `Orthogonalize` calls and the existing tolerance floor are unchanged. No file outside the target names the deleted helpers.

**Ripples:** update this section's Auto prose from “copies before” to “mutates the by-value delegate parameter.”

## 5. Delete the three policy-free one-hop host wrappers

`TransformBoundingBox`, `TransformList`, and `Transpose` are public forwarding surface over RhinoCommon plus generic admission. An exact scan across `libs/dotnet/` finds no caller. None selects domain policy, carries a behavior row, or returns a Rasm-owned semantic result.

### 5a. Delete `TransformBoundingBox`

**Location:** same file, `extension(Transform source)`, anchor `public Fin<BoundingBox> TransformBoundingBox`.

**From:**

```csharp
public Fin<BoundingBox> TransformBoundingBox(BoundingBox bounds, Op? key = null) {
    Op op = key.OrDefault();
    return from active in op.AcceptInput(value: source)
           from admitted in op.AcceptInput(value: bounds)
           from result in op.AcceptValue(value: active.TransformBoundingBox(bbox: admitted))
           select result;
}
```

**To:**

```csharp
```

### 5b. Delete `TransformList`

**Location:** same extension block, anchor `public Fin<Seq<Point3d>> TransformList`.

**From:**

```csharp
public Fin<Seq<Point3d>> TransformList(IEnumerable<Point3d> points, Op? key = null) {
    Op op = key.OrDefault();
    return from active in op.AcceptInput(value: source)
           from values in Optional(points).ToFin(Fail: op.InvalidInput())
           from admitted in values.AsIterable().ToSeq()
               .TraverseM(value => op.AcceptInput(value: value))
               .As()
           from result in op.Catch(body: () => op.Accept(values: active.TransformList(points: admitted)))
           select result;
}
```

**To:**

```csharp
```

### 5c. Delete `Transpose`

**Location:** same extension block, anchor `public Fin<Transform> Transpose`.

**From:**

```csharp
public Fin<Transform> Transpose(Op? key = null) {
    Op op = key.OrDefault();
    return from active in op.AcceptInput(value: source)
           from result in op.AcceptValue(value: active.Transpose())
           select result;
}
```

**To:**

```csharp
```

**Effect:** fenced LOC `23 -> 0` (`-23`); declared/public members `-3`; no replacement abstraction.

**API/consumer proof:** `api-rhinocommon.md` already owns all three host operations. Unlike `Inverse`, `Decompose`, and `Rewrite`, these wrappers introduce no Rasm policy or result algebra. Exact-name scans find only their declarations and the host API catalog entries. A future real consumer can admit its concrete input at its owning boundary instead of restoring a speculative generic wrapper.

**Ripples:** remove the three names from this section's Entry/Output prose. No consumer fence changes.

## 6. Inline the one-use tonal walk and its one-use default into `ToneFor`

**Location:** same file, `ToneSweep.Walk`, `PerceptualColor.ToneGrid`, and `PerceptualColor.ToneFor`.

**From:**

```csharp
internal Seq<UnitInterval> Walk(double ground, Dimension grid) =>
    (Direction: Step(ground: ground), Steps: grid.Value) switch {
        var (direction, steps) => toSeq(Enumerable.Range(start: 0, count: steps + 1))
            .Map(step => UnitInterval.Create(value: direction > 0 ? 1.0 - ((double)step / steps) : (double)step / steps)),
    };
```

**To:**

```csharp
```

**From:**

```csharp
public static Dimension ToneGrid { get; } = Dimension.Create(value: 100);
public Fin<PerceptualColor> ToneFor(PerceptualColor against, PositiveMagnitude ratio, ToneSweep sweep, Option<Dimension> grid = default, Op? key = null) {
    PerceptualColor seed = this;
    return sweep.Walk(ground: against.ReferenceLightness, grid: grid.IfNone(ToneGrid))
        .Map(tone => seed.Tone(tone: tone))
        .Choose(static candidate => candidate.ToOption())
        .TakeWhile(candidate => candidate.Contrast(other: against) >= ratio.Value)
        .Last
        .ToFin(key.OrDefault().InvalidResult());
}
```

**To:**

```csharp
public Fin<PerceptualColor> ToneFor(PerceptualColor against, PositiveMagnitude ratio, ToneSweep sweep, Option<Dimension> grid = default, Op? key = null) {
    int direction = sweep.Step(against.ReferenceLightness), steps = grid.Map(static value => value.Value).IfNone(100);
    return toSeq(Enumerable.Range(0, steps).Append(steps))
        .Map(step => Tone(UnitInterval.Create(direction > 0 ? 1.0 - ((double)step / steps) : (double)step / steps)))
        .Choose(static candidate => candidate.ToOption())
        .TakeWhile(candidate => candidate.Contrast(against) >= ratio.Value)
        .Last
        .ToFin(key.OrDefault().InvalidResult());
}
```

**Effect:** fenced LOC `15 -> 9` (`-6`); declared members `-2`; publicly reachable members `-1`; method-local bindings unchanged.

**API/consumer proof:** `ToneSweep.Step` is the generated Thinktecture behavior column; `Walk` has exactly one caller. LanguageExt `Seq.Map`, `Choose`, `Last`, and `Option.ToFin` retain the same total pipeline, while mapping the ordinal directly to `Tone` removes an intermediate `Seq<UnitInterval>`. `Enumerable.Range(0, steps).Append(steps)` preserves the inclusive `0..steps` walk without the checked `steps + 1` overflow a lawful `Dimension(int.MaxValue)` currently triggers. `ToneGrid` has no consumer outside `ToneFor`; the private default is therefore the already-proven integer `100`, while every caller-supplied override still arrives as an admitted `Dimension`. `PerceptualColor.Tone` remains public because `Rasm.AppUi/.planning/Theme/tokens.md` consumes it directly.

**Ripples:** remove `Walk` from the section's Auto prose. No caller changes.

## 7. Inline the three one-use `Placement` helpers into their generated union arms

### 7a. Inline transform composition

**Location:** same file, `Placement.Build`'s `compose:` arm and private helper `Compose`.

**From:**

```csharp
compose: static (state, value) => Compose(
    values: value.Values,
    key: state.Key)));

private static Fin<Transform> Compose(Seq<Transform> values, Op key) =>
    values
        .TraverseM(value => key.AcceptInput(value: value))
        .As()
        .Map(static admitted => admitted.Fold(
            initialState: Transform.Identity,
            f: static (combined, next) => next * combined))
        .Bind(result => key.AcceptValue(value: result));
```

**To:**

```csharp
compose: static (state, value) => value.Values
    .TraverseM(transform => state.Key.AcceptInput(transform))
    .As()
    .Map(static admitted => admitted.Fold(
        initialState: Transform.Identity,
        f: static (combined, next) => next * combined))
    .Bind(result => state.Key.AcceptValue(result))));
```

### 7b. Inline vector-basis admission

**Location:** same generated switch, `vectorBasisMap:` arm and private `VectorBasis`.

**From:**

```csharp
vectorBasisMap: static (state, value) => VectorBasis(
    x0: value.X0,
    y0: value.Y0,
    z0: value.Z0,
    x1: value.X1,
    y1: value.Y1,
    z1: value.Z1,
    key: state.Key),

private static Fin<Transform> VectorBasis(
    Vector3d x0,
    Vector3d y0,
    Vector3d z0,
    Vector3d x1,
    Vector3d y1,
    Vector3d z1,
    Op key) =>
    (key.AcceptInput(value: x0),
     key.AcceptInput(value: y0),
     key.AcceptInput(value: z0),
     key.AcceptInput(value: x1),
     key.AcceptInput(value: y1),
     key.AcceptInput(value: z1))
        .Apply(static (ax, ay, az, bx, by, bz) => Transform.ChangeBasis(
            X0: ax,
            Y0: ay,
            Z0: az,
            X1: bx,
            Y1: by,
            Z1: bz))
        .As()
        .Bind(result => key.AcceptValue(value: result));
```

**To:**

```csharp
vectorBasisMap: static (state, value) =>
    (state.Key.AcceptInput(value.X0), state.Key.AcceptInput(value.Y0), state.Key.AcceptInput(value.Z0),
     state.Key.AcceptInput(value.X1), state.Key.AcceptInput(value.Y1), state.Key.AcceptInput(value.Z1))
        .Apply(static (x0, y0, z0, x1, y1, z1) => Transform.ChangeBasis(
            X0: x0, Y0: y0, Z0: z0, X1: x1, Y1: y1, Z1: z1))
        .As()
        .Bind(result => state.Key.AcceptValue(result)),
```

### 7c. Inline point-basis admission

**Location:** same generated switch, `pointBasisMap:` arm and private `PointBasis`.

**From:**

```csharp
pointBasisMap: static (state, value) => PointBasis(
    p0: value.P0,
    x0: value.X0,
    y0: value.Y0,
    z0: value.Z0,
    p1: value.P1,
    x1: value.X1,
    y1: value.Y1,
    z1: value.Z1,
    key: state.Key),

private static Fin<Transform> PointBasis(
    Point3d p0,
    Vector3d x0,
    Vector3d y0,
    Vector3d z0,
    Point3d p1,
    Vector3d x1,
    Vector3d y1,
    Vector3d z1,
    Op key) =>
    (key.AcceptInput(value: p0),
     key.AcceptInput(value: x0),
     key.AcceptInput(value: y0),
     key.AcceptInput(value: z0),
     key.AcceptInput(value: p1),
     key.AcceptInput(value: x1),
     key.AcceptInput(value: y1),
     key.AcceptInput(value: z1))
        .Apply(static (a0, ax, ay, az, b0, bx, by, bz) => Transform.ChangeBasis(
            P0: a0,
            X0: ax,
            Y0: ay,
            Z0: az,
            P1: b0,
            X1: bx,
            Y1: by,
            Z1: bz))
        .As()
        .Bind(result => key.AcceptValue(value: result));
```

**To:**

```csharp
pointBasisMap: static (state, value) =>
    (state.Key.AcceptInput(value.P0), state.Key.AcceptInput(value.X0), state.Key.AcceptInput(value.Y0),
     state.Key.AcceptInput(value.Z0), state.Key.AcceptInput(value.P1), state.Key.AcceptInput(value.X1),
     state.Key.AcceptInput(value.Y1), state.Key.AcceptInput(value.Z1))
        .Apply(static (p0, x0, y0, z0, p1, x1, y1, z1) => Transform.ChangeBasis(
            P0: p0, X0: x0, Y0: y0, Z0: z0,
            P1: p1, X1: x1, Y1: y1, Z1: z1))
        .As()
        .Bind(result => state.Key.AcceptValue(result)),
```

**Effect:** fenced LOC `81 -> 23` (`-58`); declared members `-3`; transform order, empty-sequence identity, validation order, and host overload selection are unchanged.

**API/consumer proof:** Thinktecture's generated `TransformSpec.Switch` is already the exhaustive case owner. `api-languageext.md` confirms `TraverseM` as mapping plus monadic sequencing, tuple `Apply` as independent admission fan-in, and `Seq.Fold` as the ordered reduction. The rewrite preserves the target's existing named `Transform.ChangeBasis` coordinates exactly and claims no new RhinoCommon surface; each deleted helper has exactly one generated-switch caller and contributes no semantic beyond that arm.

**Ripples:** none.

## 8. Make lattice admission overflow-safe before comparing with the ceiling

**Location:** same file, `CellLattice.Of(Transform indexToWorld, ...)`, anchors `long cells =` and the `KernelFault.OutOfRange` construction.

**From:**

```csharp
long cells = (long)columns.Value * rows.Value * layers.Value;
```

**To:**

```csharp
Int128 cells = (Int128)columns.Value * rows.Value * layers.Value;
```

**From:**

```csharp
Scalar: cells,
```

**To:**

```csharp
Scalar: double.CreateSaturating(cells),
```

**Effect:** fenced LOC `2 -> 2`; declared members `0`; unchecked intermediate overflow paths `1 -> 0`.

**API/consumer proof:** three admitted `Dimension.Value` operands are positive `int`s, but their product can exceed `long` before `cells <= ceiling` runs. `Int128` is the BCL arithmetic carrier for the exact intermediate; `double.CreateSaturating` preserves the fault's `double Scalar` field without an exception path. Because successful admission requires the exact product to be at most the admitted `long ceiling`, the existing public `long CellCount` remains safe for every constructed lattice and all current consumers remain source-identical.

**Ripples:** none.

## 9. Enforce the lattice's affine law and derive cell measure from its basis

**Location:** same file, `CellLattice.Of(Transform indexToWorld, ...)` and `CellMeasure`.

**From:**

```csharp
return indexToWorld.TryGetInverse(inverseTransform: out Transform inverse) && inverse.IsValid
```

**To:**

```csharp
return indexToWorld.IsAffine && indexToWorld.TryGetInverse(inverseTransform: out Transform inverse) && inverse.IsValid
```

**From:**

```csharp
public double CellMeasure => Rank is 2 ? CellSize.X * CellSize.Y : CellSize.X * CellSize.Y * CellSize.Z;
```

**To:**

```csharp
public double CellMeasure => Rank is 2
    ? Vector3d.CrossProduct(new(IndexToWorld.M00, IndexToWorld.M10, IndexToWorld.M20), new(IndexToWorld.M01, IndexToWorld.M11, IndexToWorld.M21)).Length
    : Math.Abs(IndexToWorld.Determinant);
```

**Effect:** fenced LOC `2 -> 4` (`+2`); declared members `0`; projective transforms admitted as affine lattices `1 -> 0`; sheared-cell measure error `1 -> 0`.

**API/consumer proof:** the page declares `IndexToWorld` to be affine, while the direct overload currently admits any invertible `Transform`; RhinoCommon publishes `IsAffine` as the classification gate. A product of basis-vector lengths is area/volume only for an orthogonal basis. The cross-product norm is the exact parallelogram area for the rank-two lattice and `abs(Determinant)` the exact parallelepiped volume for rank three, including shear. `Rasm.Element/Geospatial/coverage.md` derives resolution from `CellMeasure`, and `Processing/sample.md` multiplies it by `CellCount`, so this is a live correctness repair rather than unused completeness work.

**Ripples:** update the CellLattice Auto prose from “reports its own extents” to state basis-derived area/volume under rotation, anisotropy, and shear. No consumer signature changes.

## 10. Refuse an unrepresentable bounds-derived census before narrowing to `int`

**Location:** same file, `CellLattice.Of(BoundingBox bounds, PositiveMagnitude cell, ...)`, the three `Dimension` admissions.

**From:**

```csharp
return bounds.IsValid
    ? from columns in op.AcceptValidated<Dimension>(candidate: (int)Math.Ceiling(a: bounds.Diagonal.X / cell.Value))
      from rows in op.AcceptValidated<Dimension>(candidate: (int)Math.Ceiling(a: bounds.Diagonal.Y / cell.Value))
      from layers in op.AcceptValidated<Dimension>(candidate: Math.Max(val1: 1, val2: (int)Math.Ceiling(a: bounds.Diagonal.Z / cell.Value)))
      from scale in Placement.Build(spec: new TransformSpec.UniformScale(Anchor: Point3d.Origin, Factor: cell.Value), key: op)
      from shift in Placement.Build(spec: new TransformSpec.Translation(Motion: (Vector3d)bounds.Min), key: op)
      from map in Placement.Build(spec: new TransformSpec.Compose(Values: Seq(scale, shift)), key: op)
      from lattice in Of(indexToWorld: map, columns: columns, rows: rows, layers: layers, ceiling: ceiling, key: op)
      select lattice
    : Fin.Fail<CellLattice>(error: op.InvalidInput());
```

**To:**

```csharp
if (!bounds.IsValid) { return Fin.Fail<CellLattice>(error: op.InvalidInput()); }
Vector3d extent = bounds.Diagonal;
(double Columns, double Rows, double Layers) counts =
    (Math.Ceiling(extent.X / cell.Value), Math.Ceiling(extent.Y / cell.Value), Math.Max(1.0, Math.Ceiling(extent.Z / cell.Value)));
return counts is { Columns: >= 1.0 and <= int.MaxValue, Rows: >= 1.0 and <= int.MaxValue, Layers: >= 1.0 and <= int.MaxValue }
    ? from columns in op.AcceptValidated<Dimension>((int)counts.Columns)
      from rows in op.AcceptValidated<Dimension>((int)counts.Rows)
      from layers in op.AcceptValidated<Dimension>((int)counts.Layers)
      from scale in Placement.Build(spec: new TransformSpec.UniformScale(Anchor: Point3d.Origin, Factor: cell.Value), key: op)
      from shift in Placement.Build(spec: new TransformSpec.Translation(Motion: (Vector3d)bounds.Min), key: op)
      from map in Placement.Build(spec: new TransformSpec.Compose(Values: Seq(scale, shift)), key: op)
      from lattice in Of(indexToWorld: map, columns: columns, rows: rows, layers: layers, ceiling: ceiling, key: op)
      select lattice
    : Fin.Fail<CellLattice>(error: op.InvalidInput());
```

**Effect:** fenced LOC `4 -> 8` (`+4`); declared members `0`; unchecked out-of-range floating-to-integer conversions `3 -> 0`; the extent and each quotient are evaluated once. The early refusal also keeps the replacement a valid query-expression prefix: C# query syntax cannot begin with `let`.

**API/consumer proof:** the BCL conversion from an out-of-range `double` to `int` outside a checked context does not certify the intended cell census; clamping would be worse because it would silently shorten the covered bounds. The property pattern rejects `NaN`, infinities, nonpositive extents, and values above `int.MaxValue` before all three casts, then the generated `Dimension` admission remains the construction authority. The later exact `Int128` product enforces the caller's budget. No exception-shaped conversion enters the domain path.

**Ripples:** none.

## 11. Preserve odd-axis coverage and the active dimensionality when coarsening the lattice

**Location:** same file, the complete query in `CellLattice.Coarsen`.

**From:**

```csharp
return from scale in Placement.Build(spec: new TransformSpec.UniformScale(Anchor: Point3d.Origin, Factor: 2.0), key: op)
       from map in Placement.Build(spec: new TransformSpec.Compose(Values: Seq(scale, IndexToWorld)), key: op)
       from columns in op.AcceptValidated<Dimension>(candidate: Math.Max(val1: 1, val2: Columns.Value / 2))
       from rows in op.AcceptValidated<Dimension>(candidate: Math.Max(val1: 1, val2: Rows.Value / 2))
       from layers in op.AcceptValidated<Dimension>(candidate: Rank is 3 ? Math.Max(val1: 1, val2: Layers.Value / 2) : 1)
       from level in Of(indexToWorld: map, columns: columns, rows: rows, layers: layers, ceiling: Ceiling, key: op)
       select level;
```

**To:**

```csharp
return from columns in op.AcceptValidated<Dimension>((Columns.Value / 2) + (Columns.Value % 2))
       from rows in op.AcceptValidated<Dimension>((Rows.Value / 2) + (Rows.Value % 2))
       from layers in op.AcceptValidated<Dimension>(Rank is 3 ? Math.Max(2, (Layers.Value / 2) + (Layers.Value % 2)) : 1)
       from scale in Placement.Build(spec: new TransformSpec.Diagonal(Values: new Vector3d(
           x: Columns.Value > 1 ? 2.0 : 1.0,
           y: Rows.Value > 1 ? 2.0 : 1.0,
           z: Rank is 3 && Layers.Value > 2 ? 2.0 : 1.0)), key: op)
       from map in Placement.Build(spec: new TransformSpec.Compose(Values: Seq(scale, IndexToWorld)), key: op)
       from level in Of(indexToWorld: map, columns: columns, rows: rows, layers: layers, ceiling: Ceiling, key: op)
       select level;
```

**Effect:** fenced LOC `7 -> 10` (`+3`); declared members unchanged; odd-axis coarse levels that drop the final source cell `1 -> 0`; 3-D levels that silently collapse to rank two at a two-layer depth `1 -> 0`; terminal axes that keep one cell while doubling their world extent `1 -> 0`.

**API/consumer proof:** every `Dimension` is already at least one, so ceiling division by two is exactly `(value / 2) + (value % 2)` with no overflow even at `int.MaxValue`. Each axis that still has reducible cells doubles its own basis column; a terminal axis stays at one cell and keeps its existing basis instead of expanding on every later level. The rank law says one layer is a plane, so an admitted 3-D lattice must retain a two-layer floor; otherwise coarsening a two-layer volume changes its dimensionality and zeroes its `Bounds` depth. Odd reducible axes still ceiling-divide, producing the minimum coarse census whose doubled cells cover the final source cell. `TransformSpec.Diagonal` is the existing affine construction case for this per-axis scale, so no new policy owner appears.

**Ripples:** update the CellLattice Entry/Auto prose from “halves the census” to the ceiling-half, active-axis scaling, and 3-D depth-floor law. In `libs/dotnet/Rasm.Materials/.planning/Raster/plane.md`, update the two `Coarsen` law sentences that currently say “halves every lattice axis” and “halved census with a floor of one, doubled cell” so terminal axes stay fixed and only reducible axes double. Apply the same wording correction to the “successive halvings” boundary statements in `libs/dotnet/Rasm.Element/.planning/Geospatial/coverage.md` and `libs/dotnet/Rasm.Bim/.planning/Semantics/raster.md`: odd axes ceiling-half, terminal axes remain unchanged, and a three-dimensional chain retains at least two layers. No consumer signature changes.

## 12. Make the derived node census exact instead of overflowing before the cast

**Location:** same file, `CellLattice.NodeCount`.

**From:**

```csharp
public long NodeCount => (long)(Columns.Value + 1) * (Rows.Value + 1) * (Layers.Value + 1);
```

**To:**

```csharp
public Int128 NodeCount => ((Int128)Columns.Value + 1) * ((Int128)Rows.Value + 1)
    * (Rank is 3 ? (Int128)Layers.Value + 1 : 1);
```

**Effect:** fenced LOC `1 -> 2` (`+1`); declared members unchanged; three pre-widening `int + 1` overflows and the later `long` product overflow are removed; planar node overcount by a phantom second z-plane `1 -> 0`.

**API/consumer proof:** `Dimension` can lawfully carry `int.MaxValue`; the current expression performs `Columns.Value + 1` as `int` before its cast and can therefore become negative even when the admitted cell product fits `long`. The exact result can reach roughly `2^93`, so `Int128` is the smallest BCL integer carrier that represents every admitted lattice without adding a second node-budget policy. `Rank` is already the owner's dimensionality authority: a one-layer lattice is a plane and `Center`, `Corner`, and `Bounds` all hold z at zero, so multiplying its planar node census by `(Layers + 1) == 2` contradicts the same owner. The three live reads in `Meshing/reconstruct.md` are accounted below; leaving its `long` evidence field unchanged would not compile, and its present unchecked `int` cast is itself lossy.

**Ripples:** update the CellLattice Entry/Auto prose to identify `NodeCount` as an exact derived census. In `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md`, change `IsoSurfaceRun.CornerSampleCount` from `long` to `Int128`; the constructor assignment and exact equality then remain source-identical. Replace `ValidityClaim.CountAtLeast(count: (int)Grid.NodeCount, ...)` with the short-circuit gate `Grid.NodeCount <= int.MaxValue && InsideNodeCount >= 0 && OutsideNodeCount >= 0 && ((Int128)InsideNodeCount + OutsideNodeCount) <= Grid.NodeCount` so the int-backed realized volume cannot certify a truncated census and the two realized counts cannot overflow before comparison.

## 13. Restrict host transforms and ordinal helpers to their actual assembly boundary

**Location:** same file, `CellLattice` properties `IndexToWorld`, `WorldToIndex` and methods `Extent`, `Stride`, `Spacing`.

**From:**

```csharp
public Transform IndexToWorld { get; }
public Transform WorldToIndex { get; }
public Dimension Extent(int ordinal) => ordinal switch { 0 => Columns, 1 => Rows, _ => Layers };
public int Stride(int ordinal) => ordinal switch { 0 => 1, 1 => Columns.Value, _ => Columns.Value * Rows.Value };
public double Spacing(int ordinal) => ordinal switch { 0 => CellSize.X, 1 => CellSize.Y, _ => CellSize.Z };
```

**To:**

```csharp
internal Transform IndexToWorld { get; }
internal Transform WorldToIndex { get; }
internal Dimension Extent(int ordinal) => ordinal switch { 0 => Columns, 1 => Rows, _ => Layers };
internal int Stride(int ordinal) => ordinal switch { 0 => 1, 1 => Columns.Value, _ => Columns.Value * Rows.Value };
internal double Spacing(int ordinal) => ordinal switch { 0 => CellSize.X, 1 => CellSize.Y, _ => CellSize.Z };
```

**Effect:** fenced LOC unchanged; publicly reachable members `-5`; internal members `+5`; no behavioral or call-site change.

**API/consumer proof:** exact consumers of `IndexToWorld`/`WorldToIndex` are only `Rasm`'s `Spatial/index.md` and `Meshing/reconstruct.md`; exact consumers of `Extent`/`Stride`/`Spacing` are only `Rasm`'s `Numerics/transform.md`. Cross-assembly consumers use the host-neutral `Affine`/`Inverse`, `Columns`/`Rows`/`Layers`, `CellCount`, `CellSize`, addressing, and bounds surface. The visibility therefore matches the already-real boundary instead of publishing RhinoCommon implementation state.

**Ripples:** no fence edit; update the CellLattice Entry/Output prose so only `Affine`/`Inverse` are public transform projections.

## 14. Rename `DecomposeAs` to the conventional behavior name `DecompositionMethod`

### 14a. Rename the behavior-bearing type and every row declaration

**Location:** same file, the smart-enum declaration and its ten row fields.

**From:**

```csharp
public sealed partial class DecomposeAs {
    public static readonly DecomposeAs Similarity = new(apply: SimilarityOf);
    public static readonly DecomposeAs Rigid = new(apply: RigidOf);
    public static readonly DecomposeAs TranslationLinear = new(apply: TranslationLinearOf);
    public static readonly DecomposeAs LinearTranslation = new(apply: LinearTranslationOf);
    public static readonly DecomposeAs AffineFactors = new(apply: AffineFactorsOf);
    public static readonly DecomposeAs Symmetric = new(apply: SymmetricOf);
    public static readonly DecomposeAs Quaternion = new(apply: QuaternionOf);
    public static readonly DecomposeAs YawPitchRoll = new(apply: YawPitchRollOf);
    public static readonly DecomposeAs EulerZYZ = new(apply: EulerZYZOf);
    public static readonly DecomposeAs Texture = new(apply: TextureOf);
```

**To:**

```csharp
public sealed partial class DecompositionMethod {
    public static readonly DecompositionMethod Similarity = new(apply: SimilarityOf);
    public static readonly DecompositionMethod Rigid = new(apply: RigidOf);
    public static readonly DecompositionMethod TranslationLinear = new(apply: TranslationLinearOf);
    public static readonly DecompositionMethod LinearTranslation = new(apply: LinearTranslationOf);
    public static readonly DecompositionMethod AffineFactors = new(apply: AffineFactorsOf);
    public static readonly DecompositionMethod Symmetric = new(apply: SymmetricOf);
    public static readonly DecompositionMethod Quaternion = new(apply: QuaternionOf);
    public static readonly DecompositionMethod YawPitchRoll = new(apply: YawPitchRollOf);
    public static readonly DecompositionMethod EulerZYZ = new(apply: EulerZYZOf);
    public static readonly DecompositionMethod Texture = new(apply: TextureOf);
```

### 14b. Rename the public parameter and the admitted local

**Location:** same file, `extension(Transform source)`, member `Decompose`.

**From:**

```csharp
public Fin<Decomposition> Decompose(DecomposeAs mode, Context context, Op? key = null) {
    Op op = key.OrDefault();
    return from active in op.AcceptInput(value: source)
           from selector in Optional(mode).ToFin(Fail: op.InvalidInput())
           from model in Optional(context).ToFin(Fail: op.MissingContext())
           from result in selector.Apply(source: active, context: model, key: op)
           select result;
}
```

**To:**

```csharp
public Fin<Decomposition> Decompose(DecompositionMethod method, Context context, Op? key = null) {
    Op op = key.OrDefault();
    return from active in op.AcceptInput(value: source)
           from activeMethod in Optional(method).ToFin(Fail: op.InvalidInput())
           from model in Optional(context).ToFin(Fail: op.MissingContext())
           from result in activeMethod.Apply(source: active, context: model, key: op)
           select result;
}
```

**Effect:** fenced LOC unchanged; symbol count unchanged; one coined verb-phrase type name and one vague local are removed without shadowing the renamed parameter.

**API/consumer proof:** the type is a behavior-bearing Thinktecture smart enum whose rows select decomposition implementations. “Method” is standard software terminology for that role; `DecomposeAs` is neither a noun nor a C# naming convention. Exact-name scanning finds no code consumer outside the target, so the rename is confined to this fence and its prose.

**Ripples:** replace `DecomposeAs` in this section's Owner/Auto/Entry/Output/Growth prose. No external fence changes.

## 15. Rename `AtomProjection` to `ResultProjection`

**Location:** same file, `internal static class AtomProjection` and every same-assembly reference.

**From:**

```csharp
internal static class AtomProjection {
```

**To:**

```csharp
internal static class ResultProjection {
```

Replace each `AtomProjection.` qualifier with `ResultProjection.`.

**Effect:** fenced LOC and symbol count unchanged; one misleading module name removed. The class projects heterogeneous operation results; it is unrelated to `LanguageExt.Atom<T>` and does not operate on a scalar “atom.”

**API/consumer proof:** the class is internal, and `ResultProjection` has no existing declaration across `libs/dotnet/`. Its public conceptual operation is a result-type projection: `Self`, `Value`, `Rows`, `Raw`, and `Unsupported`. “Result projection” is conventional and states that role without colliding with the repository's actual LanguageExt atomic-cell vocabulary.

**Ripples:** mechanical qualifier/prose rename in these exact same-assembly files: `Domain/evaluation.md`; `Meshing/mesh.md`; `Meshing/reconstruct.md`; `Numerics/spectral.md`; `Parametric/projections.md`; `Processing/decimate.md`; `Processing/extract.md`; `Processing/flatten.md`; `Processing/flow.md`; `Processing/intent.md`; `Processing/register.md`; `Processing/remesh.md`; `Processing/sample.md`; `Processing/segment.md`; `Spatial/cloud.md`; `Spatial/support.md`; `Spatial/transport.md`; `Rasm/ARCHITECTURE.md`; and `Rasm/RULINGS.md`. No package above `Rasm` names the internal class.

## Protected non-moves

- Keep `BoundarySense` and `RgbTransfer`. Each two-case family owns behavior (`Sign` and encode/decode delegates), so each is a survival discriminant rather than a payloadless synonym for `bool`.
- Keep `TransformRewrite` and `DecompositionMethod` after the refinements above. Their generated behavior columns own real algorithm selection; zero current external call sites alone does not refute that modeled operation.
- Keep the ten named `DecompositionMethod` implementations. Unlike the deleted forwarding helpers, each owns a distinct, nontrivial host factorization and its dependent success/failure gate; embedding those bodies into ten row initializers removes names at the cost of burying the algorithms inside the roster.
- Keep `PerceptualColor.Tone`. `Rasm.AppUi/.planning/Theme/tokens.md` calls it directly; only its one-use `ToneSweep.Walk` helper is redundant.
- Keep `ProjectionRow` and both `Rows` overloads. The parameterless overload exists because C# cannot combine an optional context before a `params` tail; it is not a forwarding helper invented around LanguageExt or Thinktecture.
- Keep the explicit `Fin.Succ`/`Fin.Fail` construction in `PerceptualColor.Of`. The checked-in LanguageExt catalogue publishes those constructors and no implicit `PerceptualColor -> Fin<PerceptualColor>` or `Error -> Fin<PerceptualColor>` lift; replacing the ternary arms with bare values is an uncompilable cosmetic shortening, not refinement.
- Do not replace `RgbProfile.Viewed` with a guessed `AtomHashMap` call. The current `Cell.Claim` path has explicit winner/loser semantics, and the checked-in LanguageExt catalog does not prove a total `FindOrMaybeAdd` return path that can mint this non-optional `Configuration` without a fallback or exception.
