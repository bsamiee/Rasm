# 1. Remove dead capability ranks

`select.md:61-81 — EdgeFeature and SilhouetteTrait row declarations`

From

```csharp
public static readonly EdgeFeature Boundary = new(key: "boundary", rank: 0, provenance: CurveFeature.Boundary);
public static readonly EdgeFeature NakedOuter = new(key: "naked-outer", rank: 1, provenance: CurveFeature.NakedOuter);
public static readonly EdgeFeature NakedInner = new(key: "naked-inner", rank: 2, provenance: CurveFeature.NakedInner);
public static readonly EdgeFeature Interior = new(key: "interior", rank: 3, provenance: CurveFeature.Interior);
public static readonly EdgeFeature NonManifold = new(key: "non-manifold", rank: 4, provenance: CurveFeature.NonManifold);
public static readonly EdgeFeature OuterLoop = new(key: "outer-loop", rank: 5, provenance: CurveFeature.OuterLoop);
public static readonly EdgeFeature InnerLoop = new(key: "inner-loop", rank: 6, provenance: CurveFeature.InnerLoop);
public int Rank { get; }
public static readonly SilhouetteTrait Projecting = new(key: "projecting", rank: 0, bit: (int)SilhouetteType.Projecting);
public static readonly SilhouetteTrait TangentProjects = new(key: "tangent-projects", rank: 1, bit: (int)SilhouetteType.TangentProjects);
public static readonly SilhouetteTrait Tangent = new(key: "tangent", rank: 2, bit: (int)SilhouetteType.Tangent);
public static readonly SilhouetteTrait Crease = new(key: "crease", rank: 3, bit: (int)SilhouetteType.Crease);
public static readonly SilhouetteTrait Boundary = new(key: "boundary", rank: 4, bit: (int)SilhouetteType.Boundary);
public int Rank { get; }
```

To

```csharp
public static readonly EdgeFeature Boundary = new(key: "boundary", provenance: CurveFeature.Boundary);
public static readonly EdgeFeature NakedOuter = new(key: "naked-outer", provenance: CurveFeature.NakedOuter);
public static readonly EdgeFeature NakedInner = new(key: "naked-inner", provenance: CurveFeature.NakedInner);
public static readonly EdgeFeature Interior = new(key: "interior", provenance: CurveFeature.Interior);
public static readonly EdgeFeature NonManifold = new(key: "non-manifold", provenance: CurveFeature.NonManifold);
public static readonly EdgeFeature OuterLoop = new(key: "outer-loop", provenance: CurveFeature.OuterLoop);
public static readonly EdgeFeature InnerLoop = new(key: "inner-loop", provenance: CurveFeature.InnerLoop);
// EdgeFeature.Rank DELETED
public static readonly SilhouetteTrait Projecting = new(key: "projecting", bit: (int)SilhouetteType.Projecting);
public static readonly SilhouetteTrait TangentProjects = new(key: "tangent-projects", bit: (int)SilhouetteType.TangentProjects);
public static readonly SilhouetteTrait Tangent = new(key: "tangent", bit: (int)SilhouetteType.Tangent);
public static readonly SilhouetteTrait Crease = new(key: "crease", bit: (int)SilhouetteType.Crease);
public static readonly SilhouetteTrait Boundary = new(key: "boundary", bit: (int)SilhouetteType.Boundary);
// SilhouetteTrait.Rank DELETED
```

Why

`ICapability<TSelf>` requires only the generated item roster and key. Capability membership, wire order, and silhouette mask aggregation read neither rank, so both columns are zero-value state rather than capability.

Change

Remove the two constructor columns and generated properties while retaining edge provenance and host silhouette bits.

Delta

-2 implementation LOC; -2 module-level members; no type change.

Ripples

Repository-wide Markdown consumer search found no read of either rank outside this specification; generated smart-enum constructor arity changes only at these row declarations.

# 2. Inline the one-call curve admission wrapper

`select.md:151-166 — Curves.Operation admission and CanProject(Type)`

From

```csharp
CanProject(type: typeof(TGeometry)) switch {
internal bool CanProject(Type type) =>
    Capability.Universal(type: type)
    || Kind.Of(type: type).Exists(kind => CanProject(topology: kind.Topology, type: type));
```

To

```csharp
(Capability.Universal(type: typeof(TGeometry))
    || Kind.Of(type: typeof(TGeometry)).Exists(kind => CanProject(topology: kind.Topology, type: typeof(TGeometry)))) switch {
// CanProject(Type) DELETED
```

Why

The overload has one caller and only forwards the universal-or-topology admission expression to the real generated case fold.

Change

Keep the admission expression at the operation build site and retain only `CanProject(Topology, Type)`, which owns the union policy.

Delta

-2 implementation LOC; -1 module-level member; no type change.

# 3. Inline curve-form projection

`select.md:159 — Curves.Operation CurveForm output arm`

From

```csharp
Type t when t == typeof(CurveForm) && this is FormCase => Project<TGeometry, TOut, CurveForm>(key: Key, aspect: this, project: static (p, _, context, op) => Classify(projection: p, context: context, op: op)),
```

To

```csharp
Type t when t == typeof(CurveForm) && this is FormCase => Project<TGeometry, TOut, CurveForm>(key: Key, aspect: this, project: static (p, _, context, op) => p.As<Curve>(key: op).Map(curve => Normalization.CurveFormOf(curve: curve, context: context))),
```

`select.md:279-280 — Curves.Classify`

From

```csharp
internal static Fin<CurveForm> Classify(TopologyProjection projection, Context context, Op op) =>
    projection.As<Curve>(key: op).Map(curve => Normalization.CurveFormOf(curve: curve, context: context));
```

To

```csharp
// Classify DELETED
```

Why

`Classify` has one caller and only forwards one `Fin.Map`; it owns neither classification policy nor admission.

Change

Compose the typed projection and the existing normalization owner directly in the output arm, then delete the forwarder.

Delta

-2 implementation LOC; -1 module-level member; no type change.

# 4. Remove the variadic feature guard

`select.md:172 — Curves.CanProject Brep-only feature arm`

From

```csharp
EdgeFeature feature when FeatureIsAny(feature, EdgeFeature.NakedOuter, EdgeFeature.NakedInner, EdgeFeature.OuterLoop, EdgeFeature.InnerLoop) => Capability.Native(state.Type, state.Topology, (Topology.Brep, typeof(Brep))),
```

To

```csharp
EdgeFeature feature when feature == EdgeFeature.NakedOuter || feature == EdgeFeature.NakedInner
    || feature == EdgeFeature.OuterLoop || feature == EdgeFeature.InnerLoop => Capability.Native(state.Type, state.Topology, (Topology.Brep, typeof(Brep))),
```

`select.md:284-285 — Curves.FeatureIsAny`

From

```csharp
private static bool FeatureIsAny(EdgeFeature feature, params ReadOnlySpan<EdgeFeature> features) =>
    features.Contains(feature);
```

To

```csharp
// FeatureIsAny DELETED
```

Why

The helper declares a general variadic surface for one fixed four-row policy and obscures the exact admitted set.

Change

State the four generated-owner comparisons at the decision site and delete the one-call helper.

Delta

-1 implementation LOC; -1 module-level member; no type change.

# 5. Use capability mask aggregation directly

`select.md:83 — SilhouetteTrait.MaskOf`

From

```csharp
public static int MaskOf(CapabilitySet<SilhouetteTrait> traits) => traits.Mask(bit: static trait => trait.Bit);
```

To

```csharp
// SilhouetteTrait.MaskOf DELETED
```

`select.md:328 — Curves.Silhouettes host call`

From

```csharp
_ => Rhino.Geometry.Silhouette.Compute(shape, (SilhouetteType)SilhouetteTrait.MaskOf(traits: silhouette.Traits), direction, context.For(lane: ToleranceLane.Deviation).Value, context.For(lane: ToleranceLane.Orientation).Value, [], cancel),
```

To

```csharp
_ => Rhino.Geometry.Silhouette.Compute(shape, (SilhouetteType)silhouette.Traits.Mask(bit: static trait => trait.Bit), direction, context.For(lane: ToleranceLane.Deviation).Value, context.For(lane: ToleranceLane.Orientation).Value, [], cancel),
```

Why

`CapabilitySet.Mask` already owns bit aggregation; the public wrapper adds no silhouette-domain rule.

Change

Call the capability-set operation at the only use and remove the forwarding member.

Delta

-1 implementation LOC; -1 module-level member; no type change.

Ripples

Repository-wide Markdown consumer search found no external call to `SilhouetteTrait.MaskOf`; `SilhouetteTrait.Bit` remains the typed host correspondence.

# 6. Inline curve index selection

`select.md:235 — Curves.Project evaluator`

From

```csharp
from chosen in state.Aspect.Select(curves: curves).ToEff()
```

To

```csharp
from chosen in state.Aspect.Switch(
    state: curves,
    edgesCase: static (items, _) => Fin.Succ(items),
    segmentsCase: static (items, _) => Fin.Succ(items),
    isoCase: static (items, _) => Fin.Succ(items),
    silhouetteCase: static (items, _) => Fin.Succ(items),
    atCase: static (items, at) => items.At(index: at.Value, key: Key),
    formCase: static (items, form) => form.Index.IsSome ? items.At(index: form.Index, key: Key) : Fin.Succ(items)).ToEff()
```

`select.md:185-192 — Curves.Select`

From

```csharp
internal Fin<Seq<TopologyProjection>> Select(Seq<TopologyProjection> curves) => Switch(
    state: curves,
    edgesCase: static (items, _) => Fin.Succ(items),
    segmentsCase: static (items, _) => Fin.Succ(items),
    isoCase: static (items, _) => Fin.Succ(items),
    silhouetteCase: static (items, _) => Fin.Succ(items),
    atCase: static (items, at) => items.At(index: at.Value, key: Key),
    formCase: static (items, form) => form.Index.IsSome ? items.At(index: form.Index, key: Key) : Fin.Succ(items));
```

To

```csharp
// Select DELETED
```

Why

`Select` has one caller and only forwards generated exhaustive dispatch; the evaluator already owns both the selector and extracted sequence.

Change

Run the generated case fold at the evaluator, retaining the shared `IndexSelection.At` policy for the two positional cases, and delete the forwarding member.

Delta

Neutral implementation LOC; -1 module-level member; no type change.

# 7. Preserve selected edge projection failures

`select.md:245-247 — Brep-loop Matching projection`

From

```csharp
project: loop => Optional(loop.To3dCurve()).Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.BrepLoop, loop.LoopIndex)))),
```

To

```csharp
project: loop => TopologyProjection.Of(curve: loop.To3dCurve(), source: new ComponentIndex(ComponentIndexType.BrepLoop, loop.LoopIndex))),
```

`select.md:254-256 — mesh-edge Matching projection`

From

```csharp
project: i => Some(TopologyProjection.Of(curve: mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: i).ToNurbsCurve(), source: new ComponentIndex(ComponentIndexType.MeshTopologyEdge, i)))),
```

To

```csharp
project: i => TopologyProjection.Of(curve: mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: i).ToNurbsCurve(), source: new ComponentIndex(ComponentIndexType.MeshTopologyEdge, i))),
```

`select.md:286-289 — BrepEdges projection`

From

```csharp
project: edge => Optional(edge.DuplicateCurve()).Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex))));
```

To

```csharp
project: edge => TopologyProjection.Of(curve: edge.DuplicateCurve(), source: new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex)));
```

`select.md:304-305 — Curves.Matching`

From

```csharp
private static Fin<Seq<TopologyProjection>> Matching<TPrimitive>(IEnumerable<TPrimitive> source, Curves selector, Func<TPrimitive, EdgeDescriptor> describe, Func<TPrimitive, Option<Fin<TopologyProjection>>> project) =>
    toSeq(source).Choose(item => selector.Matches(descriptor: describe(arg: item)) ? project(arg: item) : Option<Fin<TopologyProjection>>.None).TraverseM(identity).As();
```

To

```csharp
private static Fin<Seq<TopologyProjection>> Matching<TPrimitive>(IEnumerable<TPrimitive> source, Curves selector, Func<TPrimitive, EdgeDescriptor> describe, Func<TPrimitive, Fin<TopologyProjection>> project) =>
    toSeq(source).Filter(item => selector.Matches(descriptor: describe(arg: item))).TraverseM(project).As();
```

Why

The outer `Option` conflates an unselected primitive with a selected primitive whose host projection returned null. `Choose` silently drops both, contradicting the page's typed-refusal law and shortening successful output.

Change

Filter only on selection, then monadically traverse every selected projection so `TopologyProjection.Of` preserves null or admission failure.

Delta

Neutral implementation LOC, module-level members, and types; one erased failure channel is removed.

Ripples

Curve selection now returns the existing typed failure when any selected edge or loop cannot materialize instead of reporting a shortened success; operation signatures are unchanged.

# 8. Fold face-edge extraction into its dispatch arm

`select.md:252 — Curves.Extract BrepFace arm`

From

```csharp
(BrepFace face, Curves candidate) when ServesRun(candidate, BoundaryRun) => FaceEdges(face: face, selector: aspect),
```

To

```csharp
(BrepFace face, Curves candidate) when ServesRun(candidate, BoundaryRun) =>
    toSeq(face.Loops).Bind(loop => toSeq(loop.Trims).Choose(static trim => Optional(trim.Edge)))
        .TraverseM(edge => TopologyProjection.Of(curve: edge.DuplicateCurve(), source: new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex))).As(),
```

`select.md:306-311 — Curves.FaceEdges`

From

```csharp
private static Fin<Seq<TopologyProjection>> FaceEdges(BrepFace face, Curves selector) =>
    toSeq(face.Loops).Bind(loop => toSeq(loop.Trims).Choose(trim => (selector, trim.Edge) switch {
        (Curves candidate, BrepEdge edge) when ServesRun(candidate, BoundaryRun) =>
            Optional(edge.DuplicateCurve()).Map(curve => TopologyProjection.Of(curve: curve, source: new ComponentIndex(ComponentIndexType.BrepEdge, edge.EdgeIndex))),
        _ => Option<Fin<TopologyProjection>>.None,
    })).TraverseM(identity).As();
```

To

```csharp
// FaceEdges DELETED
```

Why

The extraction arm already proves the boundary-run policy. The helper repeats that proof for every trim and uses `Option` to erase a selected edge's failed duplication.

Change

Keep nullable trim-edge filtering in the guarded arm, traverse each present edge through `TopologyProjection.Of`, and delete the selector parameter and helper.

Delta

-4 implementation LOC; -1 module-level member; no type change.

# 9. Preserve face Brep conversion failure

`select.md:385-386 — Faces.Operation Brep output arm`

From

```csharp
Type t when t == typeof(Brep) => Build<TGeometry, TOut, Brep>(key: Key, selector: this, requirement: Requirement.None,
    project: static (chosen, _) => Key.Accept(values: chosen.Choose(static face => face.As<Brep>()))),
```

To

```csharp
Type t when t == typeof(Brep) => Build<TGeometry, TOut, Brep>(key: Key, selector: this, requirement: Requirement.None,
    project: static (chosen, _) => chosen.TraverseM(face => face.As<Brep>(key: Key)).As()
        .Bind(breps => Key.Accept(values: breps))),
```

Why

`Choose` converts a failed `TopologyProjection.As<Brep>()` into absence, so a partial face projection can masquerade as complete success.

Change

Use the typed `Fin` overload and `TraverseM` every selected face before output admission.

Delta

+1 implementation LOC; neutral module-level members and types; required typed-failure correction within the larger net reduction.

Ripples

Face-to-Brep callers now receive the existing typed unsupported failure if any selected face cannot materialize; signatures are unchanged.

# 10. Inline generated face selection dispatch

`select.md:421 — Faces.Build evaluator`

From

```csharp
from chosen in Choose(key: state.Key, faces: faces, selector: state.Selector, runtime: context).ToEff()
```

To

```csharp
from chosen in state.Selector.Switch(
    state: (Key: state.Key, Faces: faces, Runtime: context),
    allCase: static (s, _) => Fin.Succ(s.Faces),
    rankedCase: static (s, ranked) => Ranked(state: s, axis: ranked.Axis, direction: ranked.Direction),
    atCase: static (s, at) => s.Faces.At(index: at.Value, key: s.Key)).ToEff()
```

`select.md:433-437 — Faces.Choose`

From

```csharp
private static Fin<Seq<TopologyProjection>> Choose(Op key, Seq<TopologyProjection> faces, Faces selector, Context runtime) => selector.Switch(
    state: (Key: key, Faces: faces, Runtime: runtime),
    allCase: static (s, _) => Fin.Succ(s.Faces),
    rankedCase: static (s, ranked) => Ranked(state: s, axis: ranked.Axis, direction: ranked.Direction),
    atCase: static (s, at) => s.Faces.At(index: at.Value, key: s.Key));
```

To

```csharp
// Choose DELETED
```

Why

`Choose` is a one-call shell over the Thinktecture-generated exhaustive `Switch`; the evaluator already owns every state value the shell forwards.

Change

Run generated case dispatch in `Build` and delete the forwarding member while retaining total case handling.

Delta

-1 implementation LOC; -1 module-level member; no type change.

# 11. Replace reflected case-name operation keys

`select.md:463 — Points imports`

From

```csharp
using System.Collections.Frozen;
```

To

```csharp
// System.Collections.Frozen import DELETED
```

`select.md:537-540 — Points.Keys and Key`

From

```csharp
private static readonly Lazy<FrozenDictionary<Type, Op>> Keys = new(static () =>
    typeof(Points).GetNestedTypes().Where(static shape => shape.IsSubclassOf(c: typeof(Points)))
        .ToFrozenDictionary(static shape => shape, static shape => Op.Of(name: shape.Name)));
internal Op Key => Keys.Value[GetType()];
```

To

```csharp
// Points.Keys DELETED
internal Op Key => Switch(
    extremaCase: static _ => Op.Of(name: nameof(ExtremaCase)),
    edgeMidpointsCase: static _ => Op.Of(name: nameof(EdgeMidpointsCase)),
    verticesCase: static _ => Op.Of(name: nameof(VerticesCase)),
    controlPointsCase: static _ => Op.Of(name: nameof(ControlPointsCase)),
    spreadCase: static _ => Op.Of(name: nameof(SpreadCase)));
```

Why

The Thinktecture catalogue explicitly publishes exhaustive union dispatch but no stable case-name token. Reflection couples operation identity to runtime metadata and trimming; each typed arm can derive the same existing operation token from its own case symbol.

Change

Derive each operation key through generated `Switch` and `nameof` on its case, then remove the reflection dictionary and its sole import.

Delta

+1 implementation LOC; -1 module-level member; no type change.

Ripples

Operation telemetry and fault keys remain `ExtremaCase`, `EdgeMidpointsCase`, `VerticesCase`, `ControlPointsCase`, and `SpreadCase`; only their derivation changes.

# 12. Fit collinearity on a line

`select.md:487-492 — SpreadAspect.Collinear row`

From

```csharp
public static readonly SpreadAspect Collinear = new(key: 3, output: OutputBinding.Of<bool>(),
    fit: static (points, _, context, op) => points.Count <= 2
        ? Fin.Succ(Seq<object>(true))
        : Fitted(points: points, op: op)
            .Bind(fit => Minor(fit: fit.Plane, points: points, context: context, op: op))
            .Map(spread => Seq<object>(spread <= context.For(lane: ToleranceLane.Collinear).Value)));
```

To

```csharp
public static readonly SpreadAspect Collinear = new(key: 3, output: OutputBinding.Of<bool>(),
    fit: static (points, _, context, op) => Collinear(points: points, tolerance: context.For(lane: ToleranceLane.Collinear), op: op)
        .Map(static collinear => Seq<object>(collinear)));
```

`select.md:520-526 — SpreadAspect.Minor`

From

```csharp
private static Fin<double> Minor(Plane fit, Seq<Point3d> points, Context context, Op op) =>
    from angle in Principal(points: points, fit: fit, context: context, op: op)
    from offsets in points.TraverseM(point => VectorSpan.Of(anchor: fit.Origin, vector: point - fit.Origin, context: context, key: op)
        .Bind(span => span.Components(frame: fit, key: op))
        .Map(components => Math.Abs(value: (components.X * -Math.Sin(a: angle)) + (components.Y * Math.Cos(d: angle))))).As()
    from spread in Stat.Extrema(items: offsets, projection: static offset => offset, band: context.For(lane: ToleranceLane.Project), direction: ExtremumDirection.Maximum).Head.ToFin(op.InvalidResult())
    select spread;
```

To

```csharp
// Minor DELETED
private static Fin<bool> Collinear(Seq<Point3d> points, Tolerance tolerance, Op op) =>
    points.Count <= 2 || points.Head.Exists(anchor => points.ForAll(point => point.DistanceTo(anchor) <= tolerance.Value))
        ? Fin.Succ(true)
        : Line.TryFitLineToPoints(points.AsIterable(), out Line axis) && axis.IsValid
            ? Fin.Succ(points.ForAll(point => axis.DistanceTo(point, false) <= tolerance.Value))
            : Fin.Fail<bool>(op.InvalidResult());
```

Why

Collinearity is distance from a fitted line, not a plane fit followed by a second PCA. The current path can also turn a plane-fit refusal for a coincident or line-degenerate set into failure instead of a boolean verdict.

Change

Replace the plane/PCA/minor-axis detour with RhinoCommon's least-squares line fit and compare every point to the infinite fitted line under the typed collinearity tolerance.

Delta

-5 implementation LOC; neutral module-level members and types.

Ripples

`Points.Spread(SpreadAspect.Collinear)` keeps its boolean contract, returns true for coincident and other line-degenerate sets, and preserves typed failure when a non-coincident line fit cannot be produced.

# 13. Use the RhinoCommon coplanarity predicate

`select.md:493-496 — SpreadAspect.Coplanar row`

From

```csharp
public static readonly SpreadAspect Coplanar = new(key: 4, output: OutputBinding.Of<bool>(),
    fit: static (points, _, context, op) => points.Count <= 2
        ? Fin.Succ(Seq<object>(true))
        : Fitted(points: points, op: op).Map(fit => Seq<object>(fit.Deviation <= context.For(lane: ToleranceLane.PlaneDistance).Value)));
```

To

```csharp
public static readonly SpreadAspect Coplanar = new(key: 4, output: OutputBinding.Of<bool>(),
    fit: static (points, _, context, _) => Fin.Succ(Seq<object>(points.Count <= 3
        || Point3d.ArePointsCoplanar(points.AsIterable(), context.For(lane: ToleranceLane.PlaneDistance).Value))));
```

Why

Every set of at most three points is coplanar, and RhinoCommon already owns the tolerance-banded point-set predicate. Requiring a successful plane fit first rejects valid degenerate sets and reimplements the package capability through a deviation side channel.

Change

Short-circuit the dimensionally guaranteed case, then call the catalogued `Point3d.ArePointsCoplanar` predicate directly for larger sets.

Delta

-1 implementation LOC; neutral module-level members and types.

Ripples

`Points.Spread(SpreadAspect.Coplanar)` keeps its boolean output and now classifies line-degenerate and three-point sets as coplanar instead of exposing plane-fit refusal.
