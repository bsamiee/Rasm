# 1. Remove the unused frame-axis dispatch from SignedAxis

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:152-164`

```csharp
[SmartEnum<int>]
public sealed partial class SignedAxis {
    public static readonly SignedAxis NegativeX = new(key: -1, world: -Vector3d.XAxis, axis: static frame => -frame.XAxis);
    public static readonly SignedAxis PositiveX = new(key: 1, world: Vector3d.XAxis, axis: static frame => frame.XAxis);
    public static readonly SignedAxis NegativeY = new(key: -2, world: -Vector3d.YAxis, axis: static frame => -frame.YAxis);
    public static readonly SignedAxis PositiveY = new(key: 2, world: Vector3d.YAxis, axis: static frame => frame.YAxis);
    public static readonly SignedAxis NegativeZ = new(key: -3, world: -Vector3d.ZAxis, axis: static frame => -frame.ZAxis);
    public static readonly SignedAxis PositiveZ = new(key: 3, world: Vector3d.ZAxis, axis: static frame => frame.ZAxis);
    public Vector3d World { get; }
    internal Vector3d Of(Option<Plane> frame) => frame.Map(Axis).IfNone(World);
    internal static Seq<SignedAxis> Cardinal(Dimension rank) => toSeq(Items).Filter(axis => Math.Abs(value: axis.Key) <= rank.Value);
    [UseDelegateFromConstructor] private partial Vector3d Axis(Plane frame);
}
```

To:

```csharp
[SmartEnum<int>]
public sealed partial class SignedAxis {
    public static readonly SignedAxis NegativeX = new(key: -1, world: -Vector3d.XAxis);
    public static readonly SignedAxis PositiveX = new(key: 1, world: Vector3d.XAxis);
    public static readonly SignedAxis NegativeY = new(key: -2, world: -Vector3d.YAxis);
    public static readonly SignedAxis PositiveY = new(key: 2, world: Vector3d.YAxis);
    public static readonly SignedAxis NegativeZ = new(key: -3, world: -Vector3d.ZAxis);
    public static readonly SignedAxis PositiveZ = new(key: 3, world: Vector3d.ZAxis);
    public Vector3d World { get; }
    internal static Seq<SignedAxis> Cardinal(Dimension rank) => toSeq(Items).Filter(axis => Math.Abs(value: axis.Key) <= rank.Value);
}
```

Why: No consumer requests a frame-relative axis. The delegate column and forwarding member add generated surface to a smart enum whose used capability is only its world vector and cardinal filtering.

Change: Remove the constructor delegate column, `Axis`, and `Of`; keep the six canonical signed Cartesian axes and `Cardinal` unchanged.

Delta: −2 C# LOC; −2 explicit members; no type reduction.

# 2. Delete the unused angle-pivot case family

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:166-184`

```csharp
[Union]
public abstract partial record AnglePivot {
    private AnglePivot() { }
    public sealed record WorldCase : AnglePivot;
    public sealed record FrameCase(Plane Value) : AnglePivot;
    public sealed record NormalCase(Direction Value) : AnglePivot;
    public static AnglePivot World { get; } = new WorldCase();
    public static AnglePivot Frame(Plane frame) => new FrameCase(Value: frame);
    public static AnglePivot Normal(Direction normal) => new NormalCase(Value: normal);
    internal Fin<AnglePivot> Admit() => Switch(
        worldCase: static pivot => Fin.Succ<AnglePivot>(pivot),
        frameCase: static (pivot) => Admit.Plane(basis: pivot.Value).Map(_ => (AnglePivot)pivot),
        normalCase: static (pivot) => guard(pivot.Value.IsValid, new KernelFault.InvalidInput()).ToFin().Map(_ => (AnglePivot)pivot));
    internal double Compute(Vector3d a, Vector3d b) => Switch(
        state: (A: a, B: b),
        worldCase: static (state, _) => Vector3d.VectorAngle(a: state.A, b: state.B),
        frameCase: static (state, frame) => Vector3d.VectorAngle(a: state.A, b: state.B, plane: frame.Value),
        normalCase: static (state, normal) => Vector3d.VectorAngle(v1: state.A, v2: state.B, vNormal: normal.Value.Value));
}
```

To:

```csharp
// AnglePivot DELETED
```

Why: Every implemented call selects the world-angle branch; the sole external caller passes `None`, which defaults to that same branch. The union therefore models no exercised case distinction and duplicates RhinoCommon overload selection.

Change: Delete `AnglePivot`; remove its owner, generated-dispatch, and boundary claims from this sheet; update the three in-sheet `VectorAngle.Of` calls to the world-angle overload introduced next.

Delta: −19 C# LOC; −1 top-level type, −3 nested case types, and −6 explicit members.

Ripples: In `libs/dotnet/Rasm/.planning/Analysis/inspect.md:449`, remove `pivot: Option<AnglePivot>.None` from the `VectorAngle.Of` call and remove any `AnglePivot` import or prose reference made redundant by that edit.

# 3. Make world-angle measurement the direct VectorAngle admission

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:190-198`

```csharp
internal static Fin<VectorAngle> Of(Direction a, Direction b, AnglePivot pivot) =>
    from activePivot in pivot.Admit()
    from angle in FactoryBridge.Accept<VectorAngle>(candidate: activePivot.Compute(a: a.Value, b: b.Value))
    select angle;
internal static Fin<VectorAngle> Of(Vector3d a, Vector3d b, Context context, Option<AnglePivot> pivot) =>
    from left in Direction.Of(value: a, context: context)
    from right in Direction.Of(value: b, context: context)
    from angle in Of(a: left, b: right, pivot: pivot.IfNone(AnglePivot.World))
    select angle;
```

To:

```csharp
internal static Fin<VectorAngle> Of(Direction a, Direction b) =>
    FactoryBridge.Accept<VectorAngle>(candidate: Vector3d.VectorAngle(a: a.Value, b: b.Value));
internal static Fin<VectorAngle> Of(Vector3d a, Vector3d b, Context context) =>
    from left in Direction.Of(value: a, context: context)
    from right in Direction.Of(value: b, context: context)
    from angle in Of(a: left, b: right)
    select angle;
```

Why: The admitted-direction overload prevents duplicate vector admission, while the raw-vector overload preserves the context-dependent boundary. With no genuine pivot cases, direct RhinoCommon world-angle measurement is the complete operation.

Change: Remove both pivot parameters and the pivot bind; call `Vector3d.VectorAngle` directly after direction admission; update `VectorCone.Contains` and `VectorCone.Enclose` to call `Of(a, b)`.

Delta: −2 C# LOC; no member or type reduction; −2 parameters across the two overloads.

# 4. Remove the unused VectorRelation projection wrapper

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:218`

```csharp
internal Fin<TOut> Project<TOut>() => ResultProjection.Self<VectorRelation, TOut>(value: this);
```

To:

```csharp
// VectorRelation.Project DELETED
```

Why: Consumers use `VectorRelation.Of` and match the smart-enum rows directly. No raw boundary or typed consumer calls this identity-only forwarding member.

Change: Delete `VectorRelation.Project<TOut>` without changing `VectorRelation` or its four used relation cases.

Delta: −1 C# LOC; −1 member; no type reduction.

# 5. Lift generated PerceptualColor admission through FactoryBridge

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:415-418`

```csharp
public static Fin<PerceptualColor> Of(double lightness, double opponentA, double opponentB, double alpha = 1.0) =>
    Validate(lightness, opponentA, opponentB, alpha, out PerceptualColor? admitted) is null && admitted is not null
        ? Fin.Succ(value: admitted)
        : Fin.Fail<PerceptualColor>(error: new KernelFault.InvalidInput());
```

To:

```csharp
public static Fin<PerceptualColor> Of(double lightness, double opponentA, double opponentB, double alpha = 1.0) =>
    FactoryBridge.Lift<PerceptualColor>(
        fault: Validate(lightness, opponentA, opponentB, alpha, out PerceptualColor? admitted), admitted: admitted);
```

Why: `FactoryBridge.Lift` is the repository's existing boundary from Thinktecture's generated nullable validation result to `Fin<T>`. The ternary hand-rolls that package bridge and repeats its null-state handling.

Change: Replace the local validation ternary with the existing generated-admission lift.

Delta: −1 C# LOC; no member or type reduction.

# 6. Delete the literal-filling achromatic factory

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:460-461`

```csharp
public static Fin<PerceptualColor> Achromatic(double lightness, double alpha = 1.0) =>
    Of(lightness: lightness, opponentA: 0.0, opponentB: 0.0, alpha: alpha);
```

To:

```csharp
// PerceptualColor.Achromatic DELETED
```

Why: The member only supplies two zero literals to `Of`; it owns no invariant, algorithm, carrier transition, or domain case.

Change: Delete `Achromatic` and spell the two neutral opponent components at each of its three callers.

Delta: −2 C# LOC; −1 member; no type reduction.

Ripples: Replace the calls in `libs/dotnet/Rasm/.planning/Drawing/sheet.md:1180`, `libs/dotnet/Rasm.AppUi/.planning/Render/reality.md:298`, and `libs/dotnet/Rasm.AppUi/.planning/Theme/assets.md:206` with `PerceptualColor.Of(..., opponentA: 0.0, opponentB: 0.0, ...)`, preserving each caller's existing lightness and alpha.

# 7. Reflect a direction with the canonical vector formula

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:994-1001`

```csharp
public Fin<Direction> Reflect(Direction normal) {
    Direction self = this;
    return Placement.Build(
            spec: new TransformSpec.Mirror(
                Point: Point3d.Origin,
                Normal: normal.Value))
        .Bind(transform => Transported(value: transform * self.Value));
}
```

To:

```csharp
public Fin<Direction> Reflect(Direction normal) =>
    Transported(value: Value - (2.0 * (Value * normal.Value) * normal.Value));
```

Why: Reflection of an admitted unit direction about an admitted unit normal is the direct formula `v − 2(v·n)n`. Building and interpreting a transform union for that expression is avoidable allocation and dispatch.

Change: Replace the mirror-spec construction with the direct vector expression and retain `Transported` as the result-admission boundary.

Delta: −6 C# LOC; no member or type reduction.

# 8. Use RhinoCommon plane mapping in parallel transport

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:1018-1025`

```csharp
toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: admittedFrames.Count - 1))).Fold(
    initialState: Transported(value: value),
    f: (acc, i) => acc.Bind(prev =>
        Placement.Build(
                spec: new TransformSpec.PlaneMap(
                    From: admittedFrames[index: i - 1],
                    To: admittedFrames[index: i]))
            .Bind(transform => Transported(value: transform * prev.Value)))));
```

To:

```csharp
toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: admittedFrames.Count - 1))).Fold(
    initialState: Transported(value: value),
    f: (acc, i) => acc.Bind(prev => Transported(
        value: Transform.PlaneToPlane(admittedFrames[index: i - 1], admittedFrames[index: i]) * prev.Value))));
```

Why: `Admit.All` has already validated every plane. `Placement.Build(PlaneMap)` only calls RhinoCommon's `Transform.PlaneToPlane` and re-admits its result, so it adds a redundant union round trip inside every fold step.

Change: Construct the plane-to-plane transform directly at the use site and keep dependent result admission in the existing `Fin` fold.

Delta: −4 C# LOC; no member or type reduction.

# 9. Rotate the enclosing-cone axis directly

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:1137-1151`

```csharp
_ => guard(envelope.Half <= Math.PI + envelope.Tolerance, new KernelFault.InvalidInput())
    .Bind(_ => Placement.Build(
        spec: new TransformSpec.AxisRotation(
            Angle: envelope.Half - envelope.A,
            Axis: rotationAxis,
            Center: Point3d.Origin),
        context: Some(model)))
    .Bind(transform => Direction.Of(
        value: transform * left.Axis.Value,
        context: model))
    .Bind(axis => Of(
        apex: left.Apex,
        axis: axis.Value,
        halfAngleRadians: Math.Min(val1: Math.PI, val2: envelope.Half),
        context: model)),
```

To:

```csharp
_ => guard(envelope.Half <= Math.PI + envelope.Tolerance, new KernelFault.InvalidInput())
    .Bind(_ => Direction.Of(
        value: Transform.Rotation(
            angleRadians: envelope.Half - envelope.A,
            rotationAxis: rotationAxis,
            rotationCenter: Point3d.Origin) * left.Axis.Value,
        context: model))
    .Bind(axis => Of(
        apex: left.Apex,
        axis: axis.Value,
        halfAngleRadians: Math.Min(val1: Math.PI, val2: envelope.Half),
        context: model)),
```

Why: The branch has already admitted the model, axis operands, and angle bound. The transform-spec branch contributes no further domain decision; `Direction.Of` remains the correct admission for the rotated result.

Change: Replace `Placement.Build(AxisRotation)` with direct `Transform.Rotation` multiplication.

Delta: −3 C# LOC; no member or type reduction.

# 10. Rotate partition rays directly

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:1162-1171`

```csharp
from rays in toSeq(Enumerable.Range(start: 0, count: sectorCount.Value)).TraverseM(i =>
    Placement.Build(
            spec: new TransformSpec.AxisRotation(
                Angle: stepAngle * i,
                Axis: cone.Axis.Value,
                Center: Point3d.Origin),
            context: Some(context))
        .Bind(transform => Direction.Of(
            value: coaxial + (lateral * (transform * rim.Value)),
            context: context))).As()
```

To:

```csharp
from rays in toSeq(Enumerable.Range(start: 0, count: sectorCount.Value)).Traverse(i =>
    Direction.Of(
        value: coaxial + (lateral * (Transform.Rotation(
            angleRadians: stepAngle * i,
            rotationAxis: cone.Axis.Value,
            rotationCenter: Point3d.Origin) * rim.Value)),
        context: context)).As()
```

Why: Sector count and both directions are already admitted. The spec union only forwards one RhinoCommon rotation call before `Direction.Of` validates the produced ray.

Change: Inline `Transform.Rotation` into the independent `Traverse`, so every per-sector `Fin` participates in applicative failure accumulation.

Delta: −3 C# LOC; no member or type reduction.

# 11. Delete the unconsumed transform inspection extensions

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:920-945`

```csharp
extension(Transform source) {
    public Fin<Transform> Inverse() {
        return from active in Admit.Value(value: source)
               from inverse in active.TryGetInverse(inverseTransform: out Transform result)
                   ? Acceptance.Value(value: result)
                   : Fin.Fail<Transform>(error: new KernelFault.InvalidResult())
               select inverse;
    }

    public Fin<Decomposition> Decompose(DecompositionMethod method, Context context) {
        return from active in Admit.Value(value: source)
               from activeMethod in Optional(method).ToFin(Fail: new KernelFault.InvalidInput())
               from model in Optional(context).ToFin(Fail: new KernelFault.MissingContext())
               from result in activeMethod.Apply(source: active, context: model)
               select result;
    }

    public Fin<Transform> Rewrite(TransformRewrite rewrite, Context context) {
        return from active in Admit.Value(value: source)
               from selector in Optional(rewrite).ToFin(Fail: new KernelFault.InvalidInput())
               from model in Optional(context).ToFin(Fail: new KernelFault.MissingContext())
               from result in selector.Apply(source: active, context: model)
               select result;
    }
}
```

To:

```csharp
// Placement.Transform extensions DELETED
```

Why: Repository-wide consumers call `Placement.Build` but none calls these three extensions. They expose a second transform API family and keep two otherwise dead case owners alive.

Change: Delete the complete `extension(Transform source)` block while retaining `Placement.Build`, `TransformSpec`, and `RotationBasis`.

Delta: −23 nonblank C# LOC; −3 members; no type reduction.

# 12. Delete the unconsumed decomposition-method dispatch

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:599-709`

```csharp
[SmartEnum]
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

    [UseDelegateFromConstructor]
    internal partial Fin<Decomposition> Apply(Transform source, Context context);
```

To:

```csharp
// DecompositionMethod DELETED
```

Why: The only route to this smart enum is the unconsumed `Transform.Decompose` extension. Ten delegate rows wrap RhinoCommon decomposition calls without serving any consumer.

Change: Delete `DecompositionMethod`, including the ten row fields, generated `Apply` delegate, and the ten private `*Of` implementations through line 709.

Delta: −100 nonblank C# LOC; −1 top-level type and −21 explicit members.

# 13. Delete the now-unreachable decomposition result union

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:584-597`

```csharp
[Union]
public abstract partial record Decomposition {
    private Decomposition() { }
    public sealed record Similarity(Vector3d Translation, double Dilation, Transform Rotation, bool ReversesOrientation) : Decomposition;
    public sealed record Rigid(Vector3d Translation, Transform Rotation, bool ReversesOrientation) : Decomposition;
    public sealed record TranslationLinear(Vector3d Translation, Transform Linear) : Decomposition;
    public sealed record LinearTranslation(Transform Linear, Vector3d Translation) : Decomposition;
    public sealed record AffineFactors(Vector3d Translation, Transform Rotation, Transform Orthogonal, Vector3d Diagonal) : Decomposition;
    public sealed record Symmetric(Transform Basis, Vector3d Diagonal) : Decomposition;
    public sealed record Quaternion(Rhino.Geometry.Quaternion Value) : Decomposition;
    public sealed record YawPitchRoll(double Yaw, double Pitch, double Roll) : Decomposition;
    public sealed record EulerZYZ(double Alpha, double Beta, double Gamma) : Decomposition;
    public sealed record Texture(Vector3d Offset, Vector3d Repeat, Vector3d Rotation) : Decomposition;
}
```

To:

```csharp
// Decomposition DELETED
```

Why: Once the unused decomposition dispatch is removed, no producer or consumer remains for these ten generated cases. Keeping the union would preserve types without capability.

Change: Delete the complete `Decomposition` union and remove its prose from the placement owner description.

Delta: −14 C# LOC; −1 top-level type and −10 nested case types.

# 14. Delete the unconsumed transform-rewrite smart enum

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:711-728`

```csharp
[SmartEnum]
public sealed partial class TransformRewrite {
    public static readonly TransformRewrite Affine = new(apply: static (source, _, key) => {
        source.Affineize();
        return Acceptance.Value(source);
    });
    public static readonly TransformRewrite Linear = new(apply: static (source, _, key) => {
        source.Linearize();
        return Acceptance.Value(source);
    });
    public static readonly TransformRewrite Orthogonal = new(apply: static (source, context, key) =>
        source.Orthogonalize(Math.Max(EpsilonPolicy.SqrtEpsilon, context.Fractional))
            ? Acceptance.Value(source)
            : Fin.Fail<Transform>(error: new KernelFault.InvalidResult()));

    [UseDelegateFromConstructor]
    internal partial Fin<Transform> Apply(Transform source, Context context);
}
```

To:

```csharp
// TransformRewrite DELETED
```

Why: The sole caller is the unconsumed `Transform.Rewrite` extension. The three rows expose mutable RhinoCommon rewrites behind a generated abstraction with no boundary demand.

Change: Delete `TransformRewrite` and remove its cases from the placement prose and package roster.

Delta: −17 nonblank C# LOC; −1 top-level type and −4 explicit members.

# 15. Remove the reverse dependency from VectorFrame to Spatial

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:1078-1080`

```csharp
public static Fin<Seq<VectorFrame>> Chain(Seq<Point3d> points, Direction initialNormal, bool isClosed, Context context) =>
    NeighborKernel.BishopChain(points: points, initialNormal: initialNormal, isClosed: isClosed, context: context)
        .Bind(planes => planes.TraverseM(p => Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: context)).As());
```

To:

```csharp
// VectorFrame.Chain DELETED
```

Why: No consumer calls the forwarding member, and it makes the numeric atom layer depend upward on `Spatial` solely to rewrap frames already produced by `NeighborKernel.BishopChain`.

Change: Delete `VectorFrame.Chain` and revise this sheet's vector-algebra boundary prose so `NeighborKernel.BishopChain` remains the sole rotation-minimizing-frame entry.

Delta: −3 C# LOC; −1 member; no type reduction.

Ripples: In `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:5,526-527`, remove claims that `VectorFrame.Chain` delegates to or carries posture into `BishopChain`; describe direct consumers of `NeighborKernel.BishopChain` instead.

# 16. Move the Newell kernel to its only owner

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:1085-1092`

```csharp
public static Vector3d NewellNormal(ReadOnlySpan<Point3d> ring) {
    Vector3d normal = Vector3d.Zero;
    for (int i = 0; i < ring.Length; i++) {
        (Point3d a, Point3d b) = (ring[i], ring[(i + 1) % ring.Length]);
        normal += new Vector3d(x: (a.Y - b.Y) * (a.Z + b.Z), y: (a.Z - b.Z) * (a.X + b.X), z: (a.X - b.X) * (a.Y + b.Y));
    }
    return normal;
}
```

To:

```csharp
// VectorFrame.NewellNormal DELETED
```

Why: The only caller is the spatial ring-normal path. A public numeric-frame member for a single spatial implementation detail expands the atom surface and forces the lower package to own an algorithm it does not otherwise use.

Change: Delete `VectorFrame.NewellNormal` and its vector-algebra prose; move the unchanged imperative kernel into the private operation scope beside its sole caller.

Delta: −8 C# LOC and −1 public atom member; net project LOC is unchanged because the kernel moves intact.

Ripples: In `libs/dotnet/Rasm/.planning/Spatial/neighbors.md:537`, replace `VectorFrame.NewellNormal` with a private local `NewellNormal` implementation in that operation's code fence and update the nearby ownership prose.

# 17. Construct a bounds lattice with RhinoCommon transforms directly

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:1237-1252`

```csharp
public static Fin<CellLattice> Of(BoundingBox bounds, PositiveMagnitude cell, long ceiling) {
    if (!bounds.IsValid) { return Fin.Fail<CellLattice>(error: new KernelFault.InvalidInput()); }
    Vector3d extent = bounds.Diagonal;
    (double Columns, double Rows, double Layers) counts =
        (Math.Ceiling(extent.X / cell.Value), Math.Ceiling(extent.Y / cell.Value), Math.Max(1.0, Math.Ceiling(extent.Z / cell.Value)));
    return counts is { Columns: >= 1.0 and <= int.MaxValue, Rows: >= 1.0 and <= int.MaxValue, Layers: >= 1.0 and <= int.MaxValue }
        ? from columns in FactoryBridge.Accept<Dimension>((int)counts.Columns)
          from rows in FactoryBridge.Accept<Dimension>((int)counts.Rows)
          from layers in FactoryBridge.Accept<Dimension>((int)counts.Layers)
          from scale in Placement.Build(spec: new TransformSpec.UniformScale(Anchor: Point3d.Origin, Factor: cell.Value))
          from shift in Placement.Build(spec: new TransformSpec.Translation(Motion: (Vector3d)bounds.Min))
          from map in Placement.Build(spec: new TransformSpec.Compose(Values: Seq(scale, shift)))
          from lattice in Of(indexToWorld: map, columns: columns, rows: rows, layers: layers, ceiling: ceiling)
          select lattice
        : Fin.Fail<CellLattice>(error: new KernelFault.InvalidInput());
}
```

To:

```csharp
public static Fin<CellLattice> Of(BoundingBox bounds, PositiveMagnitude cell, long ceiling) {
    if (!bounds.IsValid) { return Fin.Fail<CellLattice>(error: new KernelFault.InvalidInput()); }
    Vector3d extent = bounds.Diagonal;
    (double Columns, double Rows, double Layers) counts =
        (Math.Ceiling(extent.X / cell.Value), Math.Ceiling(extent.Y / cell.Value), Math.Max(1.0, Math.Ceiling(extent.Z / cell.Value)));
    return counts is { Columns: >= 1.0 and <= int.MaxValue, Rows: >= 1.0 and <= int.MaxValue, Layers: >= 1.0 and <= int.MaxValue }
        ? from columns in FactoryBridge.Accept<Dimension>((int)counts.Columns)
          from rows in FactoryBridge.Accept<Dimension>((int)counts.Rows)
          from layers in FactoryBridge.Accept<Dimension>((int)counts.Layers)
          from lattice in Of(indexToWorld: Transform.Translation((Vector3d)bounds.Min) * Transform.Scale(Point3d.Origin, cell.Value), columns: columns, rows: rows, layers: layers, ceiling: ceiling)
          select lattice
        : Fin.Fail<CellLattice>(error: new KernelFault.InvalidInput());
}
```

Why: The two spec factories and compose row only forward an origin scale and translation that RhinoCommon already constructs. `CellLattice.Of` remains the invariant owner for invertibility, dimensions, cell budget, and ceiling.

Change: Replace the three `Placement.Build` binds with `Transform.Translation * Transform.Scale`, preserving the transform order and the caller-supplied `ceiling`.

Delta: −2 C# LOC; no member or type reduction.

# 18. Use RhinoCommon's transformed-bounds operation

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:1298-1305`

```csharp
public BoundingBox Bounds {
    get {
        BoundingBox box = new(min: Point3d.Origin,
            max: new Point3d(x: Columns.Value, y: Rows.Value, z: Rank is 3 ? Layers.Value : 0.0));
        _ = box.Transform(xform: IndexToWorld);
        return box;
    }
}
```

To:

```csharp
public BoundingBox Bounds => IndexToWorld.TransformBoundingBox(new BoundingBox(
    min: Point3d.Origin,
    max: new Point3d(x: Columns.Value, y: Rows.Value, z: Rank is 3 ? Layers.Value : 0.0)));
```

Why: RhinoCommon already provides the pure transformed-copy operation. The current getter mutates a local box, discards a status value, and then returns the mutation.

Change: Replace the imperative mutation with `Transform.TransformBoundingBox`.

Delta: −5 C# LOC; no member or type reduction.

# 19. Compose the coarse lattice transform directly

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:1307-1318`

```csharp
public Fin<CellLattice> Coarsen() {
    return from columns in FactoryBridge.Accept<Dimension>((Columns.Value / 2) + (Columns.Value % 2))
           from rows in FactoryBridge.Accept<Dimension>((Rows.Value / 2) + (Rows.Value % 2))
           from layers in FactoryBridge.Accept<Dimension>(Rank is 3 ? Math.Max(2, (Layers.Value / 2) + (Layers.Value % 2)) : 1)
           from scale in Placement.Build(spec: new TransformSpec.Diagonal(Values: new Vector3d(
               x: Columns.Value > 1 ? 2.0 : 1.0,
               y: Rows.Value > 1 ? 2.0 : 1.0,
               z: Rank is 3 && Layers.Value > 2 ? 2.0 : 1.0)))
           from map in Placement.Build(spec: new TransformSpec.Compose(Values: Seq(scale, IndexToWorld)))
           from level in Of(indexToWorld: map, columns: columns, rows: rows, layers: layers, ceiling: Ceiling)
           select level;
}
```

To:

```csharp
public Fin<CellLattice> Coarsen() =>
    from columns in FactoryBridge.Accept<Dimension>((Columns.Value / 2) + (Columns.Value % 2))
    from rows in FactoryBridge.Accept<Dimension>((Rows.Value / 2) + (Rows.Value % 2))
    from layers in FactoryBridge.Accept<Dimension>(Rank is 3 ? Math.Max(2, (Layers.Value / 2) + (Layers.Value % 2)) : 1)
    from level in Of(indexToWorld: IndexToWorld * Transform.Diagonal(new Vector3d(
        x: Columns.Value > 1 ? 2.0 : 1.0,
        y: Rows.Value > 1 ? 2.0 : 1.0,
        z: Rank is 3 && Layers.Value > 2 ? 2.0 : 1.0)), columns: columns, rows: rows, layers: layers, ceiling: Ceiling)
    select level;
```

Why: The diagonal transform and composition are total package operations over already admitted values. The only fallible construction that must remain is `CellLattice.Of`, which re-establishes the lattice invariant and preserves the budget ceiling.

Change: Replace the `Diagonal` and `Compose` spec binds with `IndexToWorld * Transform.Diagonal(...)`; retain the axis-rounding rules and `Ceiling` unchanged.

Delta: −3 C# LOC; no member or type reduction.

# 20. Replace the one-row raw-admission capability set with a boolean

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:1335-1341`

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RawAdmission : ICapability<RawAdmission> {
    public static readonly RawAdmission VectorMagnitude = new(key: "vector-magnitude", rank: 0);
    public int Rank { get; }
}
```

To:

```csharp
// RawAdmission DELETED
```

Why: A generated, string-keyed capability set with one row encodes one binary fact. It adds a type, key, rank, comparer, set construction, and membership lookup where `admitsMagnitude` states the same condition directly.

Change: Delete `RawAdmission`; change the `ResultProjection.Accepts` and `Raw` parameters to `bool admitsMagnitude`; replace both `admits.Admits(RawAdmission.VectorMagnitude)` branches with that boolean; rewrite the projection-row prose around the conditional vector-magnitude arm.

Delta: −6 C# LOC; −1 top-level type and −2 explicit members.

Ripples: In `libs/dotnet/Rasm/.planning/Parametric/projections.md:5,21-22,39-80,115,122`, replace `CapabilitySet<RawAdmission> Admits` with `bool AdmitsMagnitude`, pass `true` only for `CurveProjection.Curvature`, pass `false` for every other curve and surface row, update `ResultProjection.Accepts`/`Raw` calls, and remove the obsolete capability-set ownership prose for this concern.

# 21. Remove VectorFrame's unused generic projection table

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:1093-1101`

```csharp
internal Fin<TOut> Project<TOut>() {
    VectorFrame self = this;
    return ResultProjection.Rows<VectorFrame, TOut>(self: self,
        ProjectionRow.Of<Plane>(() => Admit.Plane(basis: self.Value)),
        ProjectionRow.Of<Transform>(() => Placement.Build(
            spec: new TransformSpec.PlaneMap(
                From: Plane.WorldXY,
                To: self.Value))));
}
```

To:

```csharp
// VectorFrame.Project DELETED
```

Why: The only caller is the raw `Plane` to `VectorFrame` arm, which already possesses the admitted frame it needs to return. The transform projection row has no consumer and routes a one-call package transform through `Placement.Build`.

Change: Delete `VectorFrame.Project<TOut>`; in `ResultProjection.Raw`, replace the `Plane` to `VectorFrame` arm's final `frame.Project<TOut>()` with `Value<VectorFrame, TOut>(value: frame)`; remove the transform-projection claim from vector-algebra prose.

Delta: −9 C# LOC; −1 member; no type reduction.

# 22. Inline VectorAngle raw projection at its only caller

From: `libs/dotnet/Rasm/.planning/Numerics/atoms.md:199`

```csharp
internal Fin<TOut> Project<TOut>() => ResultProjection.SelfOrValue<VectorAngle, double, TOut>(self: this, value: Value);
```

To:

```csharp
// VectorAngle.Project DELETED
```

Why: Only `ResultProjection.Raw` calls this one-line generic forwarding member. The raw boundary already dispatches on the requested output type, so keeping another type-directed hop on the value object adds a symbol without capability.

Change: Delete `VectorAngle.Project<TOut>`; split the raw switch arm into an exact `VectorAngle` arm using `Value<VectorAngle, TOut>(angle)` and an exact `double` arm using `Value<double, TOut>(angle.Value)`.

Delta: Net 0 C# LOC after replacing one raw arm with two; −1 member; no type reduction.
