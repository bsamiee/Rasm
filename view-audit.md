# 1. Fold the optional facing into direction admission

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:144`
```csharp
               from bearing in facing.Match(
                   Some: hint => Fin.Succ(new Vector3d(hint.Value.X, hint.Value.Y, 0.0)),
                   None: () => Fin.Succ(-Vector3d.YAxis))
               from horizontal in Direction.Of(value: bearing.IsTiny() ? -Vector3d.YAxis : bearing, context: context, key: key)
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:144`
```csharp
               from horizontal in Direction.Of(
                   value: facing.Map(static hint => new Vector3d(hint.Value.X, hint.Value.Y, 0.0)).Filter(static bearing => !bearing.IsTiny()).IfNone(-Vector3d.YAxis),
                   context: context, key: key)
```

## Why
Both presence arms only lift a vector into `Fin`; `Direction.Of` is the single fallible admission.

## Change
Choose the planar bearing through `Option.Map`/`Filter`/`IfNone` before admitting it.

## Delta
`LOC: -1; symbols: -1`

# 2. Delete the redundant face-range predicate

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:178`
```csharp
public readonly record struct PartSpan(int VertexStart, int VertexCount, int FaceStart, int FaceCount) {
    public bool HoldsFace(int face) => face >= FaceStart && face < FaceStart + FaceCount;
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:178`
```csharp
public readonly record struct PartSpan(int VertexStart, int VertexCount, int FaceStart, int FaceCount);
// PartSpan.HoldsFace DELETED
```

## Why
`FaceOwner` is already the module's O(1) face-to-part authority; this unused predicate duplicates that capability.

## Change
Keep the offset/count row and remove its second face lookup path.

## Delta
`LOC: -2; symbols: -1`

# 3. Carry contact classification as a result fact

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:182`
```csharp
public sealed record PartContact(int A, int B, ContactKind Kind, int Chains);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:182`
```csharp
public sealed record PartContact(int A, int B, bool Penetrating, int Chains);
// PartContact.Kind DELETED
```

## Why
The contact pass already derives the binary penetrating fact; a payload-free two-row owner adds no evidence or behavior.

## Change
Publish the derived fact directly, with `false` retaining the existing tangent/coplanar meaning.

## Delta
`LOC: +0; symbols: +0`


# 4. Expose visibility as the derived boolean fact

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:237`
```csharp
public sealed record ProjectedSegment(
    Point3d ScreenA, Point3d ScreenB, EdgeKind Edge, int Invisibility, Option<int> Next,
    Option<int> SourceA, Option<int> SourceB, (double A, double B) Depth, Option<int> Part, Option<int> SourceFace) {
    public Visibility State => Invisibility == 0 ? Visibility.Visible : Visibility.Hidden;
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:237`
```csharp
public sealed record ProjectedSegment(
    Point3d ScreenA, Point3d ScreenB, EdgeKind Edge, int Invisibility, Option<int> Next,
    Option<int> SourceA, Option<int> SourceB, (double A, double B) Depth, Option<int> Part, Option<int> SourceFace) {
    public bool Visible => Invisibility == 0;
    // ProjectedSegment.State DELETED
}
```

## Why
Visibility is exactly the zero-versus-positive projection of the retained invisibility count.

## Change
Publish that derivation as the named `Visible` fact.

## Ripples
In `libs/dotnet/Rasm.AppUi/.planning/Render/drafting.md:210-213`, replace `segment.State == Visibility.Hidden` with `!segment.Visible`. At lines 155-156 and 1008, replace the `State`/`Visibility` owner narration with the `Visible` result fact.

## Delta
`LOC: +0; symbols: +0`

# 5. Key edge tallies on the existing edge vocabulary

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:243`
```csharp
public sealed record EdgeHistogram(int Silhouette, int Crease, int Boundary, int Intersection, int VisibleCount, int HiddenCount) {
    public static readonly EdgeHistogram Empty = new(0, 0, 0, 0, 0, 0);

    public EdgeHistogram Add(ProjectedSegment s) {
        EdgeHistogram tally = s.Edge.Switch(
            silhouette:   () => this with { Silhouette = Silhouette + 1 },
            crease:       () => this with { Crease = Crease + 1 },
            boundary:     () => this with { Boundary = Boundary + 1 },
            intersection: () => this with { Intersection = Intersection + 1 });
        return s.Invisibility > 0
            ? tally with { HiddenCount = tally.HiddenCount + 1 }
            : tally with { VisibleCount = tally.VisibleCount + 1 };
    }
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:243`
```csharp
public sealed record EdgeHistogram(HashMap<EdgeKind, int> Counts, int VisibleCount, int HiddenCount) {
    public static readonly EdgeHistogram Empty = new(new HashMap<EdgeKind, int>(), 0, 0);

    public EdgeHistogram Add(ProjectedSegment segment) => new(
        Counts.AddOrUpdate(segment.Edge, static count => count + 1, 1),
        VisibleCount + (segment.Visible ? 1 : 0),
        HiddenCount + (segment.Visible ? 0 : 1));
    // EdgeHistogram.Silhouette DELETED
    // EdgeHistogram.Crease DELETED
    // EdgeHistogram.Boundary DELETED
    // EdgeHistogram.Intersection DELETED
}
```

## Why
Four counters mirror `EdgeKind` and require a field plus switch arm for every new row. The catalogued `HashMap.AddOrUpdate` makes the existing vocabulary authoritative.

## Change
Store per-kind counts in one immutable map while retaining the visible and hidden totals consumed by Fabrication.

## Ripples
In `libs/dotnet/Rasm.Fabrication/.planning/Documentation/projection.md:25`, describe `EdgeHistogram.Counts` as the per-kind tally; `VisibleCount` and the code at line 297 remain unchanged.

## Delta
`LOC: -5; symbols: -3`

# 6. Key emission state by boolean visibility

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:614`
```csharp
        Dictionary<Visibility, List<ProjectedSegment>> sets = Visibility.Items.ToDictionary(static row => row, static _ => new List<ProjectedSegment>());
        Dictionary<(Visibility Set, int Vertex), int> heads = [];
        List<(Visibility Set, int Run, int EndVertex)> terminals = [];
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:626`
```csharp
            (double prevT, int count, Option<int> prevRun, Visibility prevSet) = (0.0, edgeSeed[e], Option<int>.None, Visibility.Visible);
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:630`
```csharp
                    Visibility state = count > 0 ? Visibility.Hidden : Visibility.Visible;
                    if (state == Visibility.Hidden && !retains) { prevRun = Option<int>.None; }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:614`
```csharp
        Dictionary<bool, List<ProjectedSegment>> sets = new() { [true] = [], [false] = [] };
        Dictionary<(bool Visible, int Vertex), int> heads = [];
        List<(bool Visible, int Run, int EndVertex)> terminals = [];
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:626`
```csharp
            (double prevT, int count, Option<int> prevRun, bool prevVisible) = (0.0, edgeSeed[e], Option<int>.None, true);
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:630`
```csharp
                    bool visible = count == 0;
                    if (!visible && !retains) { prevRun = Option<int>.None; }
```

## Why
The dictionaries partition one binary fact and consume none of a generated owner's identity, ordering, or dispatch.

## Change
Use `true` and `false` as the run keys.

## Delta
`LOC: +0; symbols: +0`

# 7. Link and emit runs on boolean keys

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:633`
```csharp
                        List<ProjectedSegment> set = sets[state];
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:645`
```csharp
                        prevRun.Filter(_ => prevSet == state).Iter(prior => set[prior] = set[prior] with { Next = Some(run) });
                        segment.SourceA.Iter(source => heads.TryAdd((state, source), run));
                        segment.SourceB.Iter(_ => terminals.Add((state, run, b)));
                        (prevRun, prevSet) = (Some(run), state);
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:655`
```csharp
        foreach ((Visibility state, int run, int endVertex) in terminals) {
            List<ProjectedSegment> set = sets[state];
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:661`
```csharp
        return new DrawingProjection(toSeq(sets[Visibility.Visible]), toSeq(sets[Visibility.Hidden]), histogram, new Arr<EdgeHistogram>(parts), contacts);
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:633`
```csharp
                        List<ProjectedSegment> set = sets[visible];
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:645`
```csharp
                        prevRun.Filter(_ => prevVisible == visible).Iter(prior => set[prior] = set[prior] with { Next = Some(run) });
                        segment.SourceA.Iter(source => heads.TryAdd((visible, source), run));
                        segment.SourceB.Iter(_ => terminals.Add((visible, run, b)));
                        (prevRun, prevVisible) = (Some(run), visible);
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:655`
```csharp
        foreach ((bool visible, int run, int endVertex) in terminals) {
            List<ProjectedSegment> set = sets[visible];
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:661`
```csharp
        return new DrawingProjection(toSeq(sets[true]), toSeq(sets[false]), histogram, new Arr<EdgeHistogram>(parts), contacts);
```

## Why
Successor linking needs only equality of the derived partition key, and `DrawingProjection` already names the two output sets.

## Change
Carry the boolean through link keys and read it into the existing visible and hidden slots.

## Delta
`LOC: +0; symbols: +0`

# 8. Delete the replaced visibility owner

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:84`
```csharp
[SmartEnum<int>]
public sealed partial class Visibility {
    public static readonly Visibility Visible = new(0);
    public static readonly Visibility Hidden  = new(1);
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:84`
```csharp
// Visibility DELETED
```

## Why
Every producer and consumer now reads the boolean derived from `Invisibility`, leaving the type and two rows with no independent capability.

## Change
Remove the payload-free smart-enum.

## Ripples
Remove `Visibility` from the Rasm package roster in `libs/dotnet/Rasm.AppUi/.planning/Render/drafting.md:156`; move 10 specifies its code and narration changes.

## Delta
`LOC: -5; symbols: -3`

# 9. Replace packed edge keys with canonical tuples

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:42`
```csharp
using EdgeKeySet = System.Collections.Generic.HashSet<long>;
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:496`
```csharp
                else if (creases.Contains(Key(edge.u, edge.v))) {
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:519`
```csharp
    static Fin<EdgeKeySet> CreaseEdges(Assembly assembly, Camera camera, ViewPolicy policy, Op key) =>
        toSeq(Enumerable.Range(0, assembly.Posed.Length))
            .TraverseM(p => Creases(assembly, p, camera, policy, key))
            .As()
            .Map(static sets => sets.Fold(new EdgeKeySet(), static (union, set) => { union.UnionWith(set); return union; }));

    static Fin<EdgeKeySet> Creases(Assembly assembly, int part, Camera camera, ViewPolicy policy, Op key) {
        int offset = assembly.Spans[part].VertexStart;
        return MeshFeaturePolicy.Of(dihedralRadians: policy.CreaseDihedral.Value, space: assembly.Posed[part], faceRegions: Option<Arr<int>>.None, key: key)
            .Bind(features => SegmentKernel.DetectFeatureEdgesDetailed(space: assembly.Posed[part], policy: features, key: key))
            .Map(features => new EdgeKeySet(features.Edges
                .Filter(static e => e.Kind == MeshFeatureKind.Crease)
                .Map(e => Key(e.A + offset, e.B + offset))));
    }
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:539`
```csharp
    static long Key(int a, int b) { (int lo, int hi) = a < b ? (a, b) : (b, a); return ((long)lo << 32) | (uint)hi; }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:42`
```csharp
// EdgeKeySet DELETED
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:496`
```csharp
                else if (creases.Contains(edge)) {
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:519`
```csharp
    static Fin<System.Collections.Generic.HashSet<(int A, int B)>> CreaseEdges(Assembly assembly, Camera camera, ViewPolicy policy, Op key) =>
        toSeq(Enumerable.Range(0, assembly.Posed.Length))
            .TraverseM(p => Creases(assembly, p, camera, policy, key))
            .As()
            .Map(static sets => sets.Fold(new System.Collections.Generic.HashSet<(int A, int B)>(), static (union, set) => { union.UnionWith(set); return union; }));

    static Fin<System.Collections.Generic.HashSet<(int A, int B)>> Creases(Assembly assembly, int part, Camera camera, ViewPolicy policy, Op key) {
        int offset = assembly.Spans[part].VertexStart;
        return MeshFeaturePolicy.Of(dihedralRadians: policy.CreaseDihedral.Value, space: assembly.Posed[part], faceRegions: Option<Arr<int>>.None, key: key)
            .Bind(features => SegmentKernel.DetectFeatureEdgesDetailed(space: assembly.Posed[part], policy: features, key: key))
            .Map(features => new System.Collections.Generic.HashSet<(int A, int B)>(features.Edges
                .Filter(static e => e.Kind == MeshFeatureKind.Crease)
                .Map(e => (int.Min(e.A, e.B) + offset, int.Max(e.A, e.B) + offset))));
    }
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:539`
```csharp
// Key DELETED
```

## Why
The packed `long` is a page-local encoding of a pair already represented canonically by the incidence dictionary's `(int, int)` key.

## Change
Keep canonical endpoint tuples in the BCL `HashSet`, compare the incidence key directly, and delete the alias and bit-packing helper.

## Delta
`LOC: -2; symbols: -2`

# 10. Replace two-case policy rows with named boolean decisions

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:291`
```csharp
public sealed record ViewPolicy(
    VectorAngle CreaseDihedral, PositiveMagnitude BetaSquared, IntersectPolicy Narrow, BuildPolicy Broad,
    ContactPosture Contact, HashMap<int, PartRole> Masks, Context Tolerance) {

    public static Fin<ViewPolicy> Of(
        Context context, Option<VectorAngle> creaseDihedral = default, Option<PositiveMagnitude> betaSquared = default,
        Option<IntersectPolicy> narrow = default, Option<BuildPolicy> broad = default,
        Option<ContactPosture> contact = default, HashMap<int, PartRole> masks = default, Op? key = null) {
        Op op = key.OrDefault();
        return from dihedral in creaseDihedral.Match(
                   Some: static row => Fin.Succ(row),
                   None: () => op.AcceptValidated<VectorAngle>(candidate: context.For(ToleranceLane.Torsal).Value))
               from admitted in betaSquared.Match(
                   Some: static row => Fin.Succ(row),
                   None: () => op.AcceptValidated<PositiveMagnitude>(candidate: 4.0))
               select new ViewPolicy(dihedral, admitted, narrow.IfNone(IntersectPolicy.Canonical), broad.IfNone(BuildPolicy.Canonical),
                   contact.IfNone(ContactPosture.Weld), masks, context);
    }
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:291`
```csharp
public sealed record ViewPolicy(
    VectorAngle CreaseDihedral, PositiveMagnitude BetaSquared, IntersectPolicy Narrow, BuildPolicy Broad,
    bool WeldCoplanar, HashMap<int, bool> Occludes, Context Tolerance) {
    // ViewPolicy.Contact DELETED
    // ViewPolicy.Masks DELETED

    public static Fin<ViewPolicy> Of(
        Context context, Option<VectorAngle> creaseDihedral = default, Option<PositiveMagnitude> betaSquared = default,
        Option<IntersectPolicy> narrow = default, Option<BuildPolicy> broad = default,
        bool weldCoplanar = true, HashMap<int, bool> occludes = default, Op? key = null) {
        Op op = key.OrDefault();
        return from dihedral in creaseDihedral.Match(
                   Some: static row => Fin.Succ(row),
                   None: () => op.AcceptValidated<VectorAngle>(candidate: context.For(ToleranceLane.Torsal).Value))
               from admitted in betaSquared.Match(
                   Some: static row => Fin.Succ(row),
                   None: () => op.AcceptValidated<PositiveMagnitude>(candidate: 4.0))
               select new ViewPolicy(dihedral, admitted, narrow.IfNone(IntersectPolicy.Canonical), broad.IfNone(BuildPolicy.Canonical), weldCoplanar, occludes, context);
    }
}
```

## Why
Weld versus refuse is one boolean decision. A present part-mask row also chooses between complementary draw and occlude exceptions, while absence retains the drawn-and-occluding default.

## Change
Name the decisions `WeldCoplanar` and `Occludes`, removing generated identity and the optional wrapper around a defaulted boolean.

## Delta
`LOC: -1; symbols: +0`

# 11. Thread the sparse occlusion decisions through admission

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:349`
```csharp
        if (op.Parts.IsEmpty || op.Policy.Masks.Keys.Exists(part => part < 0 || part >= op.Parts.Count)) {
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:353`
```csharp
            Admit(op.Parts, op.Policy.Masks, k).Bind(assembly =>
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:378`
```csharp
    static Fin<Assembly> Admit(Seq<ViewSubject> parts, HashMap<int, PartRole> masks, Op key) =>
        parts.Map(static (part, ordinal) => (Part: part, Ordinal: ordinal))
            .TraverseM(entry => Seat(entry.Part, entry.Ordinal, key))
            .As()
            .Map(seated => Freeze(seated, masks));
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:349`
```csharp
        if (op.Parts.IsEmpty || op.Policy.Occludes.Keys.Exists(part => part < 0 || part >= op.Parts.Count)) {
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:353`
```csharp
            Admit(op.Parts, op.Policy.Occludes, k).Bind(assembly =>
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:378`
```csharp
    static Fin<Assembly> Admit(Seq<ViewSubject> parts, HashMap<int, bool> occludes, Op key) =>
        parts.Map(static (part, ordinal) => (Part: part, Ordinal: ordinal))
            .TraverseM(entry => Seat(entry.Part, entry.Ordinal, key))
            .As()
            .Map(seated => Freeze(seated, occludes));
```

## Why
Key admission and roster traversal do not dispatch on a role; they only transport sparse per-part decisions to `Freeze`.

## Change
Read and pass the renamed boolean map unchanged.

## Delta
`LOC: +0; symbols: +0`

# 12. Derive the complementary assembly columns directly

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:408`
```csharp
    static Assembly Freeze(Seq<PartMesh> parts, HashMap<int, PartRole> masks) {
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:427`
```csharp
        foreach ((int part, PartRole role) in masks) {
            (occludes[part], draws[part]) = role.Switch(
                drawnNotOccluding: () => (false, true),
                occludingNotDrawn: () => (true, false));
        }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:408`
```csharp
    static Assembly Freeze(Seq<PartMesh> parts, HashMap<int, bool> mask) {
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:427`
```csharp
        foreach ((int part, bool occludesPart) in mask)
            (occludes[part], draws[part]) = (occludesPart, !occludesPart);
```

## Why
The two exceptional roles are exact complements, so the switch re-derives one boolean from the other for every present key.

## Change
Expand each sparse decision directly into the dense `Occludes` and `Draws` columns.

## Delta
`LOC: -3; symbols: +0`

# 13. Use the admitted face-owner array without a sentinel wrapper

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:370`
```csharp
    internal readonly record struct Assembly(
        MeshSpace[] Posed, Point3d[] V, (int A, int B, int C)[] F, PartSpan[] Spans, int[] FaceOwner,
        bool[] Occludes, bool[] Draws) {
        public int PartOfFace(int face) => face >= 0 && face < FaceOwner.Length ? FaceOwner[face] : -1;
    }
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:488`
```csharp
                    edges.Add((edge.u, edge.v, EdgeKind.Boundary, side[face] == Sign.Positive ? ThirdVertex(soup.F[face], edge.u, edge.v) : -1, assembly.PartOfFace(face), face));
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:494`
```csharp
                    edges.Add((edge.u, edge.v, EdgeKind.Silhouette, ThirdVertex(soup.F[front], edge.u, edge.v), assembly.PartOfFace(front), front));
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:498`
```csharp
                    edges.Add((edge.u, edge.v, EdgeKind.Crease, -1, assembly.PartOfFace(lower), lower));
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:707`
```csharp
            if (!assembly.Occludes[assembly.PartOfFace(f)]) continue;
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:370`
```csharp
    internal readonly record struct Assembly(
        MeshSpace[] Posed, Point3d[] V, (int A, int B, int C)[] F, PartSpan[] Spans, int[] FaceOwner,
        bool[] Occludes, bool[] Draws);
    // Assembly.PartOfFace DELETED
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:488`
```csharp
                    edges.Add((edge.u, edge.v, EdgeKind.Boundary, side[face] == Sign.Positive ? ThirdVertex(soup.F[face], edge.u, edge.v) : -1, assembly.FaceOwner[face], face));
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:494`
```csharp
                    edges.Add((edge.u, edge.v, EdgeKind.Silhouette, ThirdVertex(soup.F[front], edge.u, edge.v), assembly.FaceOwner[front], front));
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:498`
```csharp
                    edges.Add((edge.u, edge.v, EdgeKind.Crease, -1, assembly.FaceOwner[lower], lower));
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:707`
```csharp
            if (!assembly.Occludes[assembly.FaceOwner[f]]) continue;
```

## Why
Every caller supplies an index from the admitted `F` array, and `Freeze` builds a same-length owner array. Returning `-1` for a state those callers cannot construct weakens that invariant and adds an indirection.

## Change
Index the canonical owner column directly and remove `PartOfFace`.

## Delta
`LOC: -1; symbols: -1`

# 14. Consume the contact decisions and facts directly

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:447`
```csharp
                            ? Contact(pair.Left, pair.Right, chains, policy.Contact, assembly.Draws, traits, key)
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:454`
```csharp
    static Fin<Option<(PartContact, Seq<(Point3d A, Point3d B, int Part)>)>> Contact(int a, int b, IntersectResult.Chains chains, ContactPosture posture, bool[] draws, CapabilitySet<ViewTrait> traits, Op key) {
        bool penetrating = chains.Table.Segments.Count > 0;
        bool coplanar = chains.Table.Coplanar.Count > 0;
        if (!penetrating && !coplanar) return Fin.Succ(Option<(PartContact, Seq<(Point3d, Point3d, int)>)>.None);
        if (coplanar && posture == ContactPosture.Refuse)
            return Fin.Fail<Option<(PartContact, Seq<(Point3d, Point3d, int)>)>>(
                new GeometryFault.DegenerateInput(Kind.Mesh, b, $"coplanar contact with part {a}"));
        int carried = draws[int.Min(a, b)] ? int.Min(a, b) : int.Max(a, b);
        Seq<(Point3d, Point3d, int)> contacts = penetrating && traits.Admits(ViewTrait.Contacts)
            ? chains.Walked.Bind(chain => toSeq(Enumerable.Range(0, chain.Points.Count - 1)
                .Select(i => (chain.Points[i], chain.Points[i + 1], carried))))
            : Seq<(Point3d, Point3d, int)>();
        return Fin.Succ(Some((
            new PartContact(a, b, penetrating ? ContactKind.Penetrating : ContactKind.Tangent, chains.Walked.Count),
            contacts)));
    }
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:447`
```csharp
                            ? Contact(pair.Left, pair.Right, chains, policy.WeldCoplanar, assembly.Draws, traits, key)
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:454`
```csharp
    static Fin<Option<(PartContact, Seq<(Point3d A, Point3d B, int Part)>)>> Contact(int a, int b, IntersectResult.Chains chains, bool weldCoplanar, bool[] draws, CapabilitySet<ViewTrait> traits, Op key) {
        bool penetrating = chains.Table.Segments.Count > 0;
        bool coplanar = chains.Table.Coplanar.Count > 0;
        if (!penetrating && !coplanar) return Fin.Succ(Option<(PartContact, Seq<(Point3d, Point3d, int)>)>.None);
        if (coplanar && !weldCoplanar)
            return Fin.Fail<Option<(PartContact, Seq<(Point3d, Point3d, int)>)>>(
                new GeometryFault.DegenerateInput(Kind.Mesh, b, $"coplanar contact with part {a}"));
        int carried = draws[a] ? a : b;
        Seq<(Point3d, Point3d, int)> contacts = penetrating && (draws[a] || draws[b]) && traits.Admits(ViewTrait.Contacts)
            ? chains.Walked.Bind(chain => toSeq(Enumerable.Range(0, chain.Points.Count - 1)
                .Select(i => (chain.Points[i], chain.Points[i + 1], carried))))
            : Seq<(Point3d, Point3d, int)>();
        return Fin.Succ(Some((new PartContact(a, b, penetrating, chains.Walked.Count), contacts)));
    }
```

## Why
The pair query already guarantees `a < b`, and the method already computes both contact facts. The current min/max round-trip is redundant and can attach contact linework to a part when neither part is drawn.

## Change
Test `WeldCoplanar`, retain `penetrating`, choose the first drawn pair member, and emit contact edges only when at least one member is drawn.

## Delta
`LOC: -2; symbols: +0`

# 15. Delete the three replaced two-case owners

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:90`
```csharp
[SmartEnum<int>]
public sealed partial class PartRole {
    public static readonly PartRole DrawnNotOccluding = new(0);
    public static readonly PartRole OccludingNotDrawn = new(1);
}

[SmartEnum<int>]
public sealed partial class ContactPosture {
    public static readonly ContactPosture Weld   = new(0);
    public static readonly ContactPosture Refuse = new(1);
}

[SmartEnum<int>]
public sealed partial class ContactKind {
    public static readonly ContactKind Penetrating = new(0);
    public static readonly ContactKind Tangent     = new(1);
}
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:90`
```csharp
// PartRole DELETED
// ContactPosture DELETED
// ContactKind DELETED
```

## Why
The policy and result columns now carry every decision and fact these payload-free generated families duplicated.

## Change
Remove the three types and their six static rows.

## Delta
`LOC: -15; symbols: -9`

# 16. Remove the unused camera parameter from crease classification

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:475`
```csharp
        CreaseEdges(assembly, camera, policy, key).Bind(creases => {
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:519`
```csharp
    static Fin<EdgeKeySet> CreaseEdges(Assembly assembly, Camera camera, ViewPolicy policy, Op key) =>
        toSeq(Enumerable.Range(0, assembly.Posed.Length))
            .TraverseM(p => Creases(assembly, p, camera, policy, key))
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:525`
```csharp
    static Fin<EdgeKeySet> Creases(Assembly assembly, int part, Camera camera, ViewPolicy policy, Op key) {
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:475`
```csharp
        CreaseEdges(assembly, policy, key).Bind(creases => {
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:519`
```csharp
    static Fin<System.Collections.Generic.HashSet<(int A, int B)>> CreaseEdges(Assembly assembly, ViewPolicy policy, Op key) =>
        toSeq(Enumerable.Range(0, assembly.Posed.Length))
            .TraverseM(p => Creases(assembly, p, policy, key))
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:525`
```csharp
    static Fin<System.Collections.Generic.HashSet<(int A, int B)>> Creases(Assembly assembly, int part, ViewPolicy policy, Op key) {
```

## Why
Neither method reads the camera; crease classification is entirely the composed per-part feature-edge dihedral.

## Change
Remove the unused parameter from both methods and their calls.

## Delta
`LOC: +0; symbols: -2`

# 17. Inline the one-call face-sign predicate

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:492`
```csharp
                if (FacesOppose(side, faces[0], faces[1])) {
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:516`
```csharp
    static bool FacesOppose(Sign[] side, int f0, int f1) =>
        side[f0] != side[f1] && side[f0] != Sign.Zero && side[f1] != Sign.Zero;
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:492`
```csharp
                if (side[faces[0]] != side[faces[1]] && side[faces[0]] != Sign.Zero && side[faces[1]] != Sign.Zero) {
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:516`
```csharp
// FacesOppose DELETED
```

## Why
The helper is called once and only renames a three-clause predicate whose operands are already local.

## Change
Keep the sign test at its sole decision site and delete the forwarding member.

## Delta
`LOC: -2; symbols: -1`

# 18. Preserve the spatial winding triangle carrier

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:547`
```csharp
        (BoundingBox[] boxes, Point3d[] triangles, int[] worldFace) = Occluders(assembly);
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:585`
```csharp
    static Fin<int[]> Seeds(Assembly assembly, Locus locus, int[] component, Camera camera, SpatialIndex world, int[] worldFace, Point3d[] triangles, ViewPolicy policy, Op key) {
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:547`
```csharp
        (BoundingBox[] boxes, Arr<(Point3d A, Point3d B, Point3d C)> triangles, int[] worldFace) = Occluders(assembly);
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:585`
```csharp
    static Fin<int[]> Seeds(Assembly assembly, Locus locus, int[] component, Camera camera, SpatialIndex world, int[] worldFace, Arr<(Point3d A, Point3d B, Point3d C)> triangles, ViewPolicy policy, Op key) {
```

## Why
`Occluders` returns the catalogued `SpatialIndex.Query` triangle shape. Flattening it to `Point3d[]` loses triangle boundaries and does not match the returned value.

## Change
Thread `Arr<(Point3d A, Point3d B, Point3d C)>` unchanged through `Seeds`.

## Delta
`LOC: +0; symbols: +0`

# 19. Delete three one-call forwarding helpers

## From
`libs/dotnet/Rasm/.planning/Drawing/view.md:562`
```csharp
                                ? points.Hits.Map(hit => (Edge: pair.Left, Row: (ParameterAt(candidate2d[pair.Left], hit),
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:769`
```csharp
    static double ParameterAt(Line segment, Point3d crossing) => segment.ClosestParameter(crossing);
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:549`
```csharp
                    Emit(assembly, locus.Edges, table, PropagateSeeds(component, locus.Edges, seeds), contacts, camera, traits, locus.V))));
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:748`
```csharp
    static int[] PropagateSeeds(int[] component, Seq<(int A, int B, EdgeKind Kind, int Apex, int Part, int Face)> edges, int[] seeds) {
        int[] perEdge = new int[edges.Count];
        for (int e = 0; e < edges.Count; e++) perEdge[e] = seeds[component[e]];
        return perEdge;
    }
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:364`
```csharp
                : Fin.Succ(Emit(assembly, locus.Edges, EmptyTable(locus.Edges.Count), new int[locus.Edges.Count],
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:777`
```csharp
    static Seq<(double T, int Delta)>[] EmptyTable(int edgeCount) =>
        [.. Enumerable.Repeat(Seq<(double T, int Delta)>(), edgeCount)];
```

## To
`libs/dotnet/Rasm/.planning/Drawing/view.md:562`
```csharp
                                ? points.Hits.Map(hit => (Edge: pair.Left, Row: (candidate2d[pair.Left].ClosestParameter(hit),
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:769`
```csharp
// ParameterAt DELETED
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:549`
```csharp
                    Emit(assembly, locus.Edges, table, System.Array.ConvertAll(component, label => seeds[label]), contacts, camera, traits, locus.V))));
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:748`
```csharp
// PropagateSeeds DELETED
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:364`
```csharp
                : Fin.Succ(Emit(assembly, locus.Edges, [.. Enumerable.Repeat(Seq<(double T, int Delta)>(), locus.Edges.Count)], new int[locus.Edges.Count],
```

`libs/dotnet/Rasm/.planning/Drawing/view.md:777`
```csharp
// EmptyTable DELETED
```

## Why
`ParameterAt` only renames the catalogued `Line.ClosestParameter`; `PropagateSeeds` only indexes one array by another and carries a redundant `edges` parameter; `EmptyTable` only repeats a typed empty row to a count.

## Change
Call the Rhino member directly, use `Array.ConvertAll` at `Emit`, construct the empty table at its only use, and remove all three helpers.

## Delta
`LOC: -8; symbols: -4`
