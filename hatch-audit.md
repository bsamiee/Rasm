# 1. Use the existing rhythm band admission

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:53`
```csharp
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double length, ref double gap, ref double stagger) =>
        validationError = HatchClaim.Banded(label: nameof(Length), band: Band.Nonnegative, value: length)
            ?? HatchClaim.Banded(label: nameof(Gap), band: Band.Nonnegative, value: gap)
            ?? HatchClaim.Banded(label: nameof(Stagger), band: Band.Fractional, value: stagger)
            ?? (length + gap > 0.0
                ? null
                : HatchClaim.Refused(label: nameof(HatchRhythm), requirement: "a dash period with Length + Gap strictly above zero"));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:53`
```csharp
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double length, ref double gap, ref double stagger) =>
        validationError = Band.Nonnegative.Guard(label: nameof(Length), value: ref length)
            ?? Band.Nonnegative.Guard(label: nameof(Gap), value: ref gap)
            ?? Band.Fractional.Guard(label: nameof(Stagger), value: ref stagger)
            ?? (length + gap > 0.0
                ? null
                : ValidationError.Create("Length + Gap must be strictly positive."));
```

## Why
`Band.Guard` already returns the `ValidationError?` required by the generated hook, checks finiteness and bounds, and canonicalizes signed zero through the `ref` argument.

## Change
Call the admitted bands directly and retain only the rhythm-specific period claim.

## Delta
`LOC: +0; symbols: +0`

# 2. Use the existing family band admission

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:69`
```csharp
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double angleOffset, ref PositiveMagnitude spacingScale, ref double phase, ref Option<HatchRhythm> dash) =>
        validationError = ValidityClaim.Finite(value: angleOffset).Holds
            ? HatchClaim.Banded(label: nameof(Phase), band: Band.Fractional, value: phase)
            : HatchClaim.Refused(label: nameof(AngleOffset), requirement: "a finite radian offset from the plan angle");
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:69`
```csharp
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double angleOffset, ref PositiveMagnitude spacingScale, ref double phase, ref Option<HatchRhythm> dash) =>
        validationError = ValidityClaim.Finite(value: angleOffset).Holds
            ? Band.Fractional.Guard(label: nameof(Phase), value: ref phase)
            : ValidationError.Create("AngleOffset must be a finite radian offset from the plan angle.");
```

## Why
The Thinktecture hook speaks `ValidationError`, and `Band.Fractional.Guard` already owns phase admission in that currency.

## Change
Route phase through `Band.Guard` and mint the angle refusal directly.

## Delta
`LOC: +0; symbols: +0`

# 3. Delete the redundant validation adapter

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:109`
```csharp
internal static class HatchClaim {
    internal static Fault? Banded(string label, Band band, double value) =>
        band.Admits(value: value)
            ? null
            : Refused(label: label, requirement: string.Create(CultureInfo.InvariantCulture, $"a value in {band.Interval} (got {value:R})"));

    internal static Fault Refused(string label, string requirement) =>
        new KernelFault.InvalidValue(Label: label, Requirement: requirement);
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:109`
```csharp
// HatchClaim DELETED
```

## Why
The class only renames existing admission and converts factory evidence into an unrelated domain-fault currency.

## Change
Delete `HatchClaim` and its two members.

## Delta
`LOC: -9; symbols: -3`

# 4. Remove the orphaned globalization import

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:29`
```csharp
using System.Globalization;
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:29`
```csharp
```

## Why
No hatch fence uses culture-sensitive formatting after `HatchClaim` is deleted.

## Change
Remove `System.Globalization`.

## Delta
`LOC: -1; symbols: +0`

# 5. Absorb the region pair into its operation case

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:251`
```csharp
        public Regions(Seq<HatchRegion> set, HatchPolicy policy) : base(policy) => Set = set;
        public Seq<HatchRegion> Set { get; }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:251`
```csharp
        public Regions(Seq<(Seq<Polyline> Rings, HatchPlan Plan)> set, HatchPolicy policy) : base(policy) => Set = set;
        public Seq<(Seq<Polyline> Rings, HatchPlan Plan)> Set { get; }
```

## Why
`HatchRegion` adds no admission, identity, or behavior to the pair consumed exclusively by `HatchOp.Regions`.

## Change
Carry the named pair directly on the operation case.

## Delta
`LOC: +0; symbols: +0`

# 6. Delete the absorbed region wrapper

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:152`
```csharp
[Equatable]
public readonly partial record struct HatchRegion([property: OrderedEquality] Seq<Polyline> Rings, HatchPlan Plan);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:152`
```csharp
// HatchRegion DELETED
```

## Why
The request case now owns the exact pair, and no outside-target fence constructs or reads `HatchRegion`.

## Change
Delete `HatchRegion`.

## Delta
`LOC: -2; symbols: -1`

# 7. Put native discriminant and census columns on the result

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:173`
```csharp
    [property: OrderedEquality] Arr<HatchArm> Arm,
    HatchCensus Census) : IValidityEvidence {
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:173`
```csharp
    [property: OrderedEquality] Arr<bool> IsMotif,
    [property: UnorderedEquality] HashMap<HatchCount, int> Census) : IValidityEvidence {
```

## Why
The emission distinction has two payload-free states, so a named boolean carries it without a generated type. The census wrapper adds no invariant beyond its map.

## Change
Replace `Arm` with `IsMotif` and carry the census map directly.

## Delta
`LOC: +0; symbols: +0`

# 8. Validate the renamed discriminant column

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:182`
```csharp
        ValidityClaim.CountExactly(count: Arm.Count, expected: Start.Count),
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:182`
```csharp
        ValidityClaim.CountExactly(count: IsMotif.Count, expected: Start.Count),
```

## Why
Column integrity must follow the surviving boolean discriminant.

## Change
Point the count claim at `IsMotif`.

## Delta
`LOC: +0; symbols: +0`

# 9. Validate every census slot through the map

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:184`
```csharp
        Census.Crossings.Map(static census => census >= 0).IfNone(true),
        Census.Grazed.Map(static census => census >= 0).IfNone(true),
        Census.Instances.Match(
            Some: total => Census.Culled.Exists(culled => culled <= total),
            None: () => Census.Culled.IsNone));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:184`
```csharp
        Census.Values.ForAll(static count => count >= 0),
        Census.Find(HatchCount.Instances).Match(
            Some: total => Census.Find(HatchCount.Culled).Exists(culled => culled <= total),
            None: () => !Census.ContainsKey(HatchCount.Culled)));
```

## Why
The original checks admit negative region, course, instance, and culled facts. One value fold covers every slot while retaining the instance/culled relation.

## Change
Use the map's `Values`, `Find`, and `ContainsKey` operations.

## Delta
`LOC: -1; symbols: +0`

# 10. Carry the boolean discriminant in the emission row

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:201`
```csharp
internal readonly record struct HatchRow(Point3d A, Point3d B, int Region, int Family, int Course, int Next, HatchArm Arm);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:201`
```csharp
internal readonly record struct HatchRow(Point3d A, Point3d B, int Region, int Family, int Course, int Next, bool IsMotif);
```

## Why
The arena row should carry the same minimal discriminant its frozen result exposes.

## Change
Replace `HatchRow.Arm` with `HatchRow.IsMotif`.

## Delta
`LOC: +0; symbols: +0`

# 11. Allocate the native discriminant column

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:226`
```csharp
        HatchArm[] arm = new HatchArm[written.Length];
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:226`
```csharp
        bool[] isMotif = new bool[written.Length];
```

## Why
Freezing now targets `Arr<bool>` and needs no generated-row array.

## Change
Allocate the boolean output column.

## Delta
`LOC: +0; symbols: +0`

# 12. Project the native discriminant column

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:228`
```csharp
            (start[i], end[i], region[i], family[i], course[i], next[i], arm[i]) =
                (written[i].A, written[i].B, written[i].Region, written[i].Family, written[i].Course, written[i].Next, written[i].Arm);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:228`
```csharp
            (start[i], end[i], region[i], family[i], course[i], next[i], isMotif[i]) =
                (written[i].A, written[i].B, written[i].Region, written[i].Family, written[i].Course, written[i].Next, written[i].IsMotif);
```

## Why
The freeze pass should copy the surviving row field without translation.

## Change
Project `IsMotif` into the boolean array.

## Delta
`LOC: +0; symbols: +0`

# 13. Freeze the native result columns directly

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:231`
```csharp
        return new(new(start), new(end), new(region), new(family), new(course), new(next), new(arm), new HatchCensus(counts));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:231`
```csharp
        return new(new(start), new(end), new(region), new(family), new(course), new(next), new(isMotif), counts);
```

## Why
Both result columns now accept their arena representations directly.

## Change
Pass the boolean array and census map into `HatchResult`.

## Delta
`LOC: +0; symbols: +0`

# 14. Mark undashed course rows directly

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:426`
```csharp
            _ = store.Add(new HatchRow(At(origin, frame, c, sA), At(origin, frame, c, sB), region, frame.Family, course, -1, HatchArm.Course));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:426`
```csharp
            _ = store.Add(new HatchRow(At(origin, frame, c, sA), At(origin, frame, c, sB), region, frame.Family, course, -1, IsMotif: false));
```

## Why
Course emission is the false arm of the named column.

## Change
Write `IsMotif: false` for an undashed run.

## Delta
`LOC: +0; symbols: +0`

# 15. Mark dashed course rows directly

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:441`
```csharp
                _ = store.Add(new HatchRow(At(origin, frame, c, a), At(origin, frame, c, b), region, frame.Family, course, -1, HatchArm.Course));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:441`
```csharp
                _ = store.Add(new HatchRow(At(origin, frame, c, a), At(origin, frame, c, b), region, frame.Family, course, -1, IsMotif: false));
```

## Why
Dash carving changes course extent, not the emission arm.

## Change
Write `IsMotif: false` for each dash run.

## Delta
`LOC: +0; symbols: +0`

# 16. Mark motif rows directly

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:487`
```csharp
            int slot = store.Add(new HatchRow(loop.Points[v], loop.Points[v + 1], region, familyOrdinal, courseOrdinal, -1, HatchArm.Motif));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:487`
```csharp
            int slot = store.Add(new HatchRow(loop.Points[v], loop.Points[v + 1], region, familyOrdinal, courseOrdinal, -1, IsMotif: true));
```

## Why
Motif rings are the true arm of the named column.

## Change
Write `IsMotif: true` for each motif edge.

## Delta
`LOC: +0; symbols: +0`

# 17. Delete the payload-free two-case owner

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:75`
```csharp
[SmartEnum<int>]
public sealed partial class HatchArm {
    public static readonly HatchArm Course = new(0);
    public static readonly HatchArm Motif  = new(1);
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:75`
```csharp
// HatchArm DELETED
```

## Why
`IsMotif` carries the complete distinction. The generated owner adds a type, two rows, and lookup/dispatch surface without capability.

## Change
Delete `HatchArm` and its rows.

## Delta
`LOC: -5; symbols: -3`

# 18. Delete the one-field census carrier

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:155`
```csharp
[Equatable]
public sealed partial record HatchCensus([property: UnorderedEquality] HashMap<HatchCount, int> Counts) {
    public static readonly HatchCensus Empty = new(HashMap<HatchCount, int>());

    public Option<int> Of(HatchCount slot) => Counts.Find(slot);
    public Option<int> Regions => Of(HatchCount.Regions);
    public Option<int> Courses => Of(HatchCount.Courses);
    public Option<int> Crossings => Of(HatchCount.Crossings);
    public Option<int> Grazed => Of(HatchCount.Grazed);
    public Option<int> Instances => Of(HatchCount.Instances);
    public Option<int> Culled => Of(HatchCount.Culled);
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:155`
```csharp
// HatchCensus DELETED
```

## Why
Every surviving operation is already a `HashMap` member; the wrapper provides no invariant and adds eight forwarding members.

## Change
Delete `HatchCensus` and its members.

## Delta
`LOC: -12; symbols: -9`

# 19. Remove the result self alias

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:192`
```csharp
    public Seq<Polyline> ToPolylines() {
        HatchResult wire = this;
        return SuccessorChain.Walk(
                toSeq(Enumerable.Range(0, Start.Count)),
                i => wire.Next[i] >= 0 ? Some(wire.Next[i]) : Option<int>.None)
            .Map(chain => new Polyline(wire.Start[chain[0]].Cons(chain.Map(i => wire.End[i]))));
    }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:192`
```csharp
    public Seq<Polyline> ToPolylines() =>
        SuccessorChain.Walk(
                toSeq(Enumerable.Range(0, Start.Count)),
                i => Next[i] >= 0 ? Some(Next[i]) : Option<int>.None)
            .Map(chain => new Polyline(Start[chain[0]].Cons(chain.Map(i => End[i]))));
```

## Why
The local aliases immutable `this` without changing capture, lifetime, or meaning.

## Change
Make `ToPolylines` expression-bodied and read its columns directly.

## Delta
`LOC: -2; symbols: -1`

# 20. Inline the canonical course ceiling

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:238`
```csharp
public sealed record HatchPolicy(ArrangementPolicy Arrange, BuildPolicy Broad, Dimension CourseBudget) {
    public static readonly Dimension WireCourseCeiling = Dimension.Create(value: 100_000);

    public static readonly HatchPolicy Canonical = new(
        Arrange: ArrangementPolicy.Canonical, Broad: BuildPolicy.Canonical, CourseBudget: WireCourseCeiling);
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:238`
```csharp
public sealed record HatchPolicy(ArrangementPolicy Arrange, BuildPolicy Broad, Dimension CourseBudget) {
    public static readonly HatchPolicy Canonical = new(
        Arrange: ArrangementPolicy.Canonical, Broad: BuildPolicy.Canonical, CourseBudget: Dimension.Create(value: 100_000));
}
// HatchPolicy.WireCourseCeiling DELETED
```

## Why
The field has one reader and no independent policy role; only `Canonical` chooses this literal.

## Change
Construct the default budget on `Canonical` and delete `WireCourseCeiling`.

## Delta
`LOC: -2; symbols: -1`

# 21. Normalize region input at its only call site

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:276`
```csharp
                    .TraverseM(entry => Normalize(entry.Region.Rings, r.Policy, s.Key)
                        .Bind(loops => Weave(s.Store, entry.Ordinal, loops, entry.Region.Plan, r.Policy, s.Key)))
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:276`
```csharp
                    .TraverseM(entry => Arrangement.Apply(new ArrangementOp.PlanarOverlay(
                            A: entry.Region.Rings, B: Seq<Polyline>(), Op: BooleanOp.Union, Plane: Axis.Z, Policy: r.Policy.Arrange), s.Key)
                        .Bind(result => result is ArrangementResult.Overlay overlay
                            ? Weave(s.Store, entry.Ordinal, overlay.Loops, entry.Region.Plan, r.Policy, s.Key)
                            : Fin.Fail<Unit>(s.Key.InvalidResult())))
```

## Why
`Normalize` has one caller and only forwards to `Arrangement.Apply` before narrowing the result case.

## Change
Compose `PlanarOverlay` directly into `Weave` in the `Regions` arm.

## Delta
`LOC: +3; symbols: +0`

# 22. Delete the absorbed normalization helper

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:289`
```csharp
    static Fin<Seq<Chain>> Normalize(Seq<Polyline> rings, HatchPolicy policy, Op key) =>
        Arrangement.Apply(new ArrangementOp.PlanarOverlay(A: rings, B: Seq<Polyline>(), Op: BooleanOp.Union, Plane: Axis.Z, Policy: policy.Arrange), key)
            .Bind(result => result is ArrangementResult.Overlay overlay
                ? Fin.Succ(overlay.Loops)
                : Fin.Fail<Seq<Chain>>(key.InvalidResult()));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:289`
```csharp
// Normalize DELETED
```

## Why
The region arm now resolves normalization in one hop.

## Change
Delete `Normalize`.

## Delta
`LOC: -5; symbols: -1`

# 23. Trust the canonical overlay boundary once

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:298`
```csharp
        if (loops.Exists(static loop => !loop.Points.IsClosed)) {
            return Fin.Fail<Unit>(new GeometryFault.HatchFailed(plan.Pattern, region, "open boundary chain"));
        }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:298`
```csharp
```

## Why
Both arms reach `Weave` through `ArrangementResult.Overlay.Loops`. `PlanarOverlay` rejects open input, derives the kept-triangle rim, and returns it through `Chain.Of`; repeating closure admission adds no evidence.

## Change
Remove the interior open-chain check.

## Delta
`LOC: -3; symbols: +0`

# 24. Use the polyline segment surface at the course call site

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:307`
```csharp
            None: () => Courses(store, region, Edges(loops), plan, policy, key));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:307`
```csharp
            None: () => Courses(store, region,
                [.. loops.Bind(static loop => toSeq(loop.Points.GetSegments()).Map(static segment => (A: segment.From, B: segment.To)))],
                plan, policy, key));
```

## Why
`Polyline.GetSegments` already owns adjacent-edge projection. Flattening it here removes a pooled writer that immediately copied into an array.

## Change
Flatten the canonical loops' segments directly into the course edge array.

## Delta
`LOC: +2; symbols: +0`

# 25. Delete the hand-rolled edge projection

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:310`
```csharp
    static (Point3d A, Point3d B)[] Edges(Seq<Chain> loops) {
        using ArrayPoolBufferWriter<(Point3d A, Point3d B)> edges = new();
        foreach (Chain loop in loops) {
            for (int v = 0; v + 1 < loop.Points.Count; v++) {
                Span<(Point3d A, Point3d B)> slot = edges.GetSpan(sizeHint: 1);
                slot[0] = (loop.Points[v], loop.Points[v + 1]);
                edges.Advance(count: 1);
            }
        }
        return edges.WrittenSpan.ToArray();
    }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:310`
```csharp
// Edges DELETED
```

## Why
The call site now composes the existing polyline segment surface, so this adjacency loop and transient arena duplicate package capability.

## Change
Delete `Edges`.

## Delta
`LOC: -11; symbols: -1`

# 26. Use the ordered monadic fold for winding state

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:385`
```csharp
            .Bind(scan => scan.Rows
                .Fold(Fin.Succ((Winding: 0, Open: 0.0)), (state, row) => state.Bind(held => {
                    int stepped = held.Winding + row.Delta;
                    return held.Winding == 0 && stepped != 0
                        ? Fin.Succ((stepped, row.S))
                        : held.Winding != 0 && stepped == 0 && row.S > held.Open
                            ? Dashes(store, origin, frame, region, ordinal, odd, c, held.Open, row.S, pattern, policy, key).Map(_ => (stepped, held.Open))
                            : Fin.Succ((stepped, held.Open));
                }))
                .Map(_ => {
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:385`
```csharp
            .Bind(scan => scan.Rows
                .FoldBackM((Winding: 0, Open: 0.0), (held, row) => {
                    int stepped = held.Winding + row.Delta;
                    return held.Winding == 0 && stepped != 0
                        ? Fin.Succ((stepped, row.S))
                        : held.Winding != 0 && stepped == 0 && row.S > held.Open
                            ? Dashes(store, origin, frame, region, ordinal, odd, c, held.Open, row.S, pattern, policy, key).Map(_ => (stepped, held.Open))
                            : Fin.Succ((stepped, held.Open));
                }).As()
                .Map(_ => {
```

## Why
The current fold nests `Fin` in its state. LanguageExt's head-to-tail `FoldBackM` preserves sorted crossing order while making failure the carrier.

## Change
Fold the plain winding tuple monadically and re-anchor with `As()`.

## Delta
`LOC: +0; symbols: -1`

# 27. Use the ordered monadic fold for crossing construction

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:402`
```csharp
        ids.Fold(Fin.Succ((Rows: Seq<(double S, int Delta)>(), Grazed: 0)), (state, id) => state.Bind(acc => {
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:402`
```csharp
        ids.FoldBackM((Rows: Seq<(double S, int Delta)>(), Grazed: 0), (acc, id) => {
```

## Why
The candidate scan repeats the nested result accumulator; `FoldBackM` preserves order and short-circuits exact-intersection failure.

## Change
Fold the plain crossing accumulator monadically.

## Delta
`LOC: +0; symbols: -1`

# 28. Re-anchor the crossing fold result

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:420`
```csharp
            }))
            .Map(static acc => (toSeq(acc.Rows.OrderBy(static row => row.S).ThenByDescending(static row => row.Delta)), acc.Grazed));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:420`
```csharp
            }).As()
            .Map(static acc => (toSeq(acc.Rows.OrderBy(static row => row.S).ThenByDescending(static row => row.Delta)), acc.Grazed));
```

## Why
`FoldBackM` returns the trait-typed carrier; `As()` lands it on concrete `Fin` before sorting.

## Change
Close and re-anchor the monadic fold.

## Delta
`LOC: +0; symbols: +0`

# 29. Build the spatial index at its only consumer

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:329`
```csharp
                : Broad(edges, policy.Broad, key).Bind(index =>
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:329`
```csharp
                : SpatialIndex.Build(SpatialKind.Bvh, System.Array.ConvertAll(edges, static edge => new BoundingBox([edge.A, edge.B])), policy.Broad, key).Bind(index =>
```

## Why
`Broad` calls one package operation and adds no policy, invariant, or result mapping.

## Change
Call `SpatialIndex.Build` directly in `Courses`.

## Delta
`LOC: +0; symbols: +0`

# 30. Delete the forwarding broad-phase helper

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:496`
```csharp
    static Fin<SpatialIndex> Broad((Point3d A, Point3d B)[] edges, BuildPolicy policy, Op key) =>
        SpatialIndex.Build(SpatialKind.Bvh, System.Array.ConvertAll(edges, static e => new BoundingBox([e.A, e.B])), policy, key);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:496`
```csharp
// Broad DELETED
```

## Why
The build now resolves in one hop.

## Change
Delete `Broad`.

## Delta
`LOC: -2; symbols: -1`

# 31. Query candidates at the course owner

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:383`
```csharp
        return Candidates(index, hatch, key)
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:383`
```csharp
        BoundingBox box = new([hatch.From, hatch.To]);
        box.Inflate(hatch.Length * EpsilonPolicy.SqrtEpsilon);
        return index.Query(box, key: key)
```

## Why
`Candidates` only prepares the course box for `SpatialIndex.Query`; the tolerance belongs beside the line it bounds.

## Change
Construct, inflate, and query the box inside `CourseOf`.

## Delta
`LOC: +2; symbols: +0`

# 32. Delete the absorbed candidate-query helper

## From
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:499`
```csharp
    static Fin<Seq<int>> Candidates(SpatialIndex index, Line hatch, Op key) {
        BoundingBox box = new([hatch.From, hatch.To]);
        box.Inflate(hatch.Length * EpsilonPolicy.SqrtEpsilon);
        return index.Query(box, key: key);
    }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/hatch.md:499`
```csharp
// Candidates DELETED
```

## Why
The query preparation now lives at its only semantic owner.

## Change
Delete `Candidates`.

## Delta
`LOC: -5; symbols: -1`
