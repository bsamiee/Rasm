# 1. Admit the development policy once
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L43-L55**
```csharp
public sealed record DevelopPolicy(
    double StripWidth, Dimension RulingStations, Tolerance Torsal, Tolerance Isometry,
    Arr<Point2d> Seed) : IValidityEvidence {
    public static DevelopPolicy Of(Context context, double stripWidth) => new(
        StripWidth: stripWidth, RulingStations: Dimension.Create(value: 32),
        Torsal: context.For(lane: ToleranceLane.Torsal),
        Isometry: context.For(lane: ToleranceLane.Deviation),
        Seed: Arr<Point2d>.Empty);

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Positive(value: StripWidth),
        Torsal.IsValid, Isometry.IsValid,
        ValidityClaim.CountAtLeast(count: RulingStations.Value, floor: 2));
}
```
**To**
```csharp
public sealed record DevelopPolicy(
    PositiveMagnitude Width, Dimension Stations, Tolerance Torsal, Tolerance Isometry,
    Option<Arr<Point2d>> Seed) {
    public static Fin<DevelopPolicy> Of(
        Context context, double width, Option<Arr<Point2d>> seed = default, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: width)
            .Map(admitted => new(
                Width: admitted, Stations: Dimension.Create(value: 32),
                Torsal: context.For(lane: ToleranceLane.Torsal),
                Isometry: context.For(lane: ToleranceLane.Deviation),
                Seed: seed.Filter(static points => !points.IsEmpty)));
}
```
**Why**
The raw width is currently checked again on every execution, while an empty array overloads a real seed column with absence. `PositiveMagnitude` is the existing generated positive-finite owner, and `Option` gives the default isoline one representation.
**Change**
Admit the raw width through `AcceptValidated` at policy construction, retain the admitted value thereafter, canonicalize an empty supplied seed to `None`, and rename the two policy columns to concise domain terms. `SeedOf` folds `Seed` once, and arithmetic reads `Width.Value` only where a primitive is required.
**Ripples**
No `DevelopPolicy.Of` call is search-resolved. In `libs/dotnet/Rasm.Fabrication/.planning/Forming/sheet.md:FormPolicy.ValidateFactoryArguments`, delete the `development is { IsValid: true }` clause; the generated non-null admission already owns presence, and the policy has admitted its width and context tolerances. Any future raw-width caller binds the returned `Fin<DevelopPolicy>` instead of constructing the record directly.
**Delta**
LOC 0; types 0; members -1; raw primitive slots -1; sentinel states -1.

# 2. Remove interior revalidation and exhaust the surface result correctly
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L92-L100**
```csharp
    static Fin<StripField> DecomposeOf(SurfaceResult.UvTessellation source, DevelopPolicy policy, Op key) =>
        !policy.IsValid
            ? Fin.Fail<StripField>(key.InvalidInput())
            : Surfaces.Apply(
                    new SurfaceOp.Geodesics(source, new GeodesicPlan(
                        SeedOf(source, policy), LevelLadder(source, policy.StripWidth), GeodesicGrade.Exact)), key)
                .Bind(edges => edges is SurfaceResult.GeodesicField field
                    ? Rulings(source, policy, field, key)
                    : Fin.Fail<StripField>(key.InvalidResult()));
```
**To**
```csharp
    static Fin<StripField> DecomposeOf(SurfaceResult.UvTessellation source, DevelopPolicy policy, Op key) =>
        Surfaces.Apply(
                new SurfaceOp.Geodesics(source, new GeodesicPlan(
                    SeedOf(source, policy), LevelLadder(source, policy.Width), GeodesicGrade.Exact)), key)
            .Bind(edges => edges.SwitchPartially(
                state: (Source: source, Policy: policy, Key: key),
                @default: static (state, _) => Fin.Fail<StripField>(state.Key.InvalidResult()),
                geodesicField: static (state, field) =>
                    Rulings(state.Source, state.Policy, field, state.Key)));
```
**Why**
An admitted policy must not be revalidated in the interior, and a structural `is` probe over the closed `SurfaceResult` family forfeits generated dispatch. `SwitchPartially` states the one accepted case and the typed refusal for every other case without weakening the generated exhaustive pair.
**Change**
Delete the `IsValid` branch, pass the admitted width owner to the ladder, and replace the type test with the verified generated partial dispatch while threading state closure-free.
**Delta**
LOC 0; types 0; branches -1; repeated validation passes -1.

# 3. Keep strip width typed through ladder construction
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L103-L103**
```csharp
    static Arr<double> LevelLadder(SurfaceResult.UvTessellation source, double stripWidth);
```
**To**
```csharp
    static Arr<double> LevelLadder(SurfaceResult.UvTessellation source, PositiveMagnitude width);
```
**Why**
The ladder consumes the already-admitted design width; accepting a raw `double` reopens the invalid scalar domain beneath the policy boundary.
**Change**
Accept `PositiveMagnitude`, read `width.Value` inside the arithmetic body, and remove any local positivity guard from the implementation.
**Delta**
LOC 0; types 0; members 0; raw primitive parameters -1.

# 4. Remove layout mirrors from the decomposition carrier
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L59-L62**
```csharp
public sealed record StripField(
    Arr<int> RailOffsets, Arr<Point2d> RailUv,
    Arr<int> RulingOffsets, Arr<Point2d> RulingA, Arr<Point2d> RulingB, Arr<double> TorsalResidual,
    Arr<int> Component, Arr<int> LayoutParent);
```
**To**
```csharp
public sealed record StripField(
    Arr<int> RailOffsets, Arr<Point2d> RailUv,
    Arr<int> RulingOffsets, Arr<Point2d> RulingA, Arr<Point2d> RulingB, Arr<double> TorsalResidual);
```
**Why**
`Component` and `LayoutParent` duplicate values computed only after unrolling by `ConnectedComponents` and the Kruskal forest. They cannot truthfully be decomposition output and no repository consumer reads them.
**Change**
Keep only the rail and ruling SoA columns produced by `Rulings`; pass layout labels and forest edges directly from `Emit` to atlas construction.
**Delta**
LOC -1; types 0; members -2; stored columns -2.

# 5. Derive evidence counts from their owning columns
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L64-L64**
```csharp
public sealed record Isometry(int Strips, int Rulings, Arr<double> Witness, Stat<Scalar> Band, Stat<Scalar> Torsal, int Components);
```
**To**
```csharp
public sealed record Isometry(Arr<double> Witness, Stat<Scalar> Band, Stat<Scalar> Torsal, int Components);
```
**Why**
`Strips` mirrors `Witness.Count`, and `Rulings` mirrors `Torsal.Count`. Persisting both copies lets the census contradict the evidence columns and summary that own it.
**Change**
Stop passing copied counts from `Atlas`; readers derive the strip count from `Witness.Count` and ruling count from `Torsal.Count`.
**Ripples**
In `libs/dotnet/Rasm.Fabrication/.planning/Forming/sheet.md:Canonical`, replace `result.Strips` with `result.Witness.Count`, `result.Rulings` with `result.Torsal.Count`, and the unresolved `result.IsometryOf` with `result.Witness`.
**Delta**
LOC 0; types 0; members -2; stored slots -2.

# 6. Return completed output without decomposition state
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L81-L81**
```csharp
    public sealed record Unrolled(ChartAtlas Atlas, StripField Field, Isometry Isometry) : DevelopmentResult;
```
**To**
```csharp
    public sealed record Unrolled(ChartAtlas Atlas, Isometry Isometry) : DevelopmentResult;
```
**Why**
The `Strips` case already owns decomposition inspection. The completed case needs the atlas and acceptance evidence; retaining `Field` returns the same intermediate state a second time solely so one consumer can count it.
**Change**
Remove `Field` from the completed case and keep it local while `Atlas` builds the output.
**Ripples**
In `libs/dotnet/Rasm.Fabrication/.planning/Forming/sheet.md:DevelopSurface`, delete the second isometry/torsal gate because `Development` already refuses every over-budget strip and the torsal statistic is evidence by the target law, not a fabrication gate. Replace the unresolved `unrolled.Result` reads with `unrolled.Isometry`, store `Some(unrolled.Isometry)`, and derive any strip census from `unrolled.Isometry.Witness.Count`. In `libs/dotnet/Rasm.Fabrication/.planning/Forming/tube.md:SectionedCope`, retain the atlas-only read and replace the `is DevelopmentResult.Unrolled` probe with generated `SwitchPartially` dispatch over the target union.
**Delta**
LOC 0; types 0; members -1; duplicated result payloads -1.

# 7. Accumulate independent strip failures
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L108-L118**
```csharp
    static Fin<DevelopmentResult> UnrollOf(SurfaceResult.UvTessellation source, DevelopPolicy policy, StripField field, Op key) =>
        StripCount(field) switch {
            0 => Fin.Fail<DevelopmentResult>(new GeometryFault.NoDevelopableStrips()),
            int strips => Range(0, strips).ToSeq()
                .TraverseM(strip => Develop(source, field, strip).Bind(unrolled =>
                    unrolled.Witness <= (ddouble)policy.Isometry.Value
                        ? Fin.Succ(unrolled)
                        : Fin.Fail<UnrolledStrip>(new GeometryFault.StripIsometryExceeded(strip, (double)unrolled.Witness, policy.Isometry))))
                .As()
                .Bind(unrolled => Emit(source, field, unrolled, key)),
        };
```
**To**
```csharp
    static Fin<DevelopmentResult> UnrollOf(SurfaceResult.UvTessellation source, DevelopPolicy policy, StripField field, Op key) =>
        (field.RailOffsets.Count - 1) switch {
            0 => Fin.Fail<DevelopmentResult>(new GeometryFault.NoDevelopableStrips()),
            int strips => Range(0, strips).ToSeq()
                .Traverse(strip => Develop(source, field, strip).Bind(unrolled =>
                    unrolled.Witness <= (ddouble)policy.Isometry.Value
                        ? Fin.Succ(unrolled)
                        : Fin.Fail<UnrolledStrip>(new GeometryFault.StripIsometryExceeded(strip, (double)unrolled.Witness, policy.Isometry))).ToValidation())
                .As().ToFin()
                .Bind(unrolled => Emit(source, field, unrolled, key)),
        };
```
**Why**
Each strip develops independently, so monadic `TraverseM` hides every failure after the first. Applicative `Traverse` over `Validation<Error, T>` evaluates the whole strip set, accumulates all typed failures through `Error`, and crosses back to `Fin` once before layout.
**Change**
Derive the strip count directly from the rail CSR offsets, lift each strip result to `Validation`, traverse applicatively, and convert the accumulated result to `Fin` at the layout boundary.
**Delta**
LOC 0; types 0; members 0; first-failure traversals -1; accumulation boundaries +1.

# 8. Delete the strip-count forwarding helper
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L122-L122**
```csharp
    static int StripCount(StripField field);
```
**To**
```csharp
// Development.StripCount DELETED
```
**Why**
The rail offset count already states the invariant at its sole call site; a helper around that expression adds a second name without owning an algorithm or admission.
**Change**
Delete the declaration and implementation after task 7 reads the CSR count directly.
**Delta**
LOC -1; types 0; members -1; call hops -1.

# 9. Remove copied strip identity from the unroll row
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L120-L120**
```csharp
    internal readonly record struct UnrolledStrip(int Strip, Arr<int> Vertices, Arr<(int A, int B, int C)> Faces, Arr<Point2d> Planar, ddouble Witness, double MaxJacobianRatio);
```
**To**
```csharp
    internal readonly record struct UnrolledStrip(Arr<int> Vertices, Arr<(int A, int B, int C)> Faces, Arr<Point2d> Planar, ddouble Witness, double MaxJacobianRatio);
```
**Why**
The traversed strip range and its returned sequence have the same stable order, which is also the index used by component labels and atlas emission. Storing that index again permits a row identity to disagree with its position.
**Change**
Remove the constructor slot and stop copying the traversal index in `Develop`; keep the local index only where `StripIsometryExceeded` is minted.
**Delta**
LOC 0; types 0; members -1; stored slots -1.

# 10. Derive one weighted shared-rail edge
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L137-L138**
```csharp
    static Seq<(int A, int B)> SharedRails(StripField field);
    static double SharedRailLength(StripField field, int a, int b);
```
**To**
```csharp
    static Seq<STaggedEdge<int, double>> SharedRails(StripField field);
// Development.SharedRailLength DELETED
```
**Why**
The shared-rail fold already discovers both endpoints and their common length. Returning only endpoints and rescanning the field for every Kruskal weight splits one derived graph fact across two helpers; `STaggedEdge` is the catalogued value edge carrying that payload without per-edge reference allocation.
**Change**
Emit one `STaggedEdge<int, double>` per shared rail with length in `Tag`, then delete `SharedRailLength` and its implementation.
**Delta**
LOC -1; types 0; members -1; repeated field scans -1 per edge; edge allocations -1 per edge.

# 11. Batch graph admission and inline the one-use forest local
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L126-L135**
```csharp
    static Fin<DevelopmentResult> Emit(SurfaceResult.UvTessellation source, StripField field, Seq<UnrolledStrip> strips, Op key) {
        UndirectedGraph<int, SEdge<int>> adjacency = new(allowParallelEdges: false);
        adjacency.AddVertexRange(Enumerable.Range(0, strips.Count));
        foreach ((int a, int b) in SharedRails(field)) { adjacency.AddEdge(new SEdge<int>(a, b)); }
        Dictionary<int, int> components = new();
        int componentCount = adjacency.ConnectedComponents(components);
        Arr<int> componentOf = new([.. Enumerable.Range(0, strips.Count).Select(strip => components[strip])]);
        IEnumerable<SEdge<int>> order = adjacency.MinimumSpanningTreeKruskal(edge => 1.0 / (1.0 + SharedRailLength(field, edge.Source, edge.Target)));
        return Atlas(source, field, strips, componentOf, toSeq(order), componentCount, key);
    }
```
**To**
```csharp
    static Fin<DevelopmentResult> Emit(SurfaceResult.UvTessellation source, StripField field, Seq<UnrolledStrip> strips, Op key) {
        UndirectedGraph<int, STaggedEdge<int, double>> adjacency = new(allowParallelEdges: false);
        adjacency.AddVertexRange(Enumerable.Range(0, strips.Count));
        adjacency.AddEdgeRange(SharedRails(field));
        Dictionary<int, int> components = new();
        int componentCount = adjacency.ConnectedComponents(components);
        Arr<int> componentOf = new([.. Enumerable.Range(0, strips.Count).Select(strip => components[strip])]);
        return Atlas(source, field, strips, componentOf, toSeq(adjacency.MinimumSpanningTreeKruskal(
            static edge => 1.0 / (1.0 + edge.Tag))), componentCount, key);
    }
```
**Why**
QuikGraph owns verified batch edge admission once the vertices exist, and the edge tag is the Kruskal weight. The statement loop, repeated rail lookup, and one-use `order` local add no domain capability.
**Change**
Use the value-tagged edge graph, admit the edge sequence through `AddEdgeRange`, read `Tag` in the weight selector, and pass the resulting forest directly to `Atlas`.
**Delta**
LOC -1; types 0; locals -1; statement loops -1.

# 12. Preserve the weighted forest through placement
**From — libs/dotnet/Rasm/.planning/Parametric/develop.md:L139-L142**
```csharp
    internal static Arr<int> PlacementOrder(Seq<UnrolledStrip> strips, Arr<int> componentOf, Seq<SEdge<int>> forest);
    static Fin<DevelopmentResult> Atlas(
        SurfaceResult.UvTessellation source, StripField field, Seq<UnrolledStrip> strips,
        Arr<int> componentOf, Seq<SEdge<int>> forest, int componentCount, Op key);
```
**To**
```csharp
    internal static Arr<int> PlacementOrder(Seq<UnrolledStrip> strips, Arr<int> componentOf, Seq<STaggedEdge<int, double>> forest);
    static Fin<DevelopmentResult> Atlas(
        SurfaceResult.UvTessellation source, StripField field, Seq<UnrolledStrip> strips,
        Arr<int> componentOf, Seq<STaggedEdge<int, double>> forest, int componentCount, Op key);
```
**Why**
Erasing the shared-length payload at the layout boundary would either discard the graph's ordering evidence or force the deleted field rescan back into placement. `PlacementOrder` remains a genuine ordering kernel rather than being inlined into atlas construction.
**Change**
Thread the tagged Kruskal forest through `PlacementOrder` and `Atlas`; both read endpoints directly and placement may use `Tag` without another `StripField` scan.
**Delta**
LOC 0; types 0; members 0; payload-erasing conversions -1.
