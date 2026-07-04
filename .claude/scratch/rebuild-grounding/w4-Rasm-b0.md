# [W4_RASM_B0_GROUNDING] — verified primary extracts

Batch: `Meshing/slice.md` [new] · `Meshing/skeleton.md` [new] · `Processing/remesh.md` [new] · `Parametric/nurbs.md` [new].
All extracts carry `file:line` anchors. Member surfaces verified against the freshly decompile-authored `.api` catalogs (both tiers); GShark spot-verified live via `uv run python -m tools.assay api query NurbsBase --key gshark` (2.3.1, `lib/netstandard2.0/GShark.dll`, artifact `.artifacts/assay/scope/api/GShark/decompile.cs`).

## [00]-[REAL_LS_INVENTORIES]

### Doctrine root — `docs/stacks/csharp/`
```
README.md language.md shapes.md surfaces-and-dispatch.md rails-and-effects.md boundaries.md algorithms.md system-apis.md
domain/ -> README.md compute.md concurrency.md data-interchange.md diagnostics.md durability.md interaction.md persistence.md postgres.md resilience.md runtime.md transport.md validation.md visuals.md
```
Atlas order (README.md:11-33): language·shapes·surfaces-and-dispatch·rails-and-effects·boundaries·algorithms·system-apis -> domain router (13 shards).

### Shared substrate `.api` — `libs/csharp/.api/` (31 catalogs)
```
api-csparse api-extensions-ai api-generator-equals api-grpc-aspnetcore api-grpc-client-web api-grpc-client api-grpc-common api-grpc-core-api api-grpc-tools api-hashing api-highperformance api-hybrid-cache api-jsonpatch api-languageext api-mapperly api-mathnet-numerics api-mathnet-providers api-messagepack api-nodatime-protobuf api-nodatime-stj api-nodatime api-protobuf api-quikgraph api-redaction api-system-configuration api-tensors api-thinktecture-json api-thinktecture-messagepack api-thinktecture-runtime-extensions api-unicolour api-unitsnet
```

### Folder domain `.api` — `libs/csharp/Rasm/.api/` (12 catalogs)
```
api-bigrational api-doubledouble api-gshark api-hashing api-kdtree api-manifold api-mathnet-numerics api-miconvexhull api-peteronumbers api-rhino api-tensors
```
Stacking law (CLAUDE.md): layer shared rails (LanguageExt/Thinktecture/MathNet/QuikGraph/HighPerformance) ON TOP OF folder domain packages (GShark/Manifold/KDTree/MIConvexHull), never the folder set alone.

## [01]-[DECISION_PAGE_ROWS] — `RASM-CS-GEOMETRY-DECISION.md`

### Row 17 slice.md (`:95`)
> `| 17 | Meshing/slice.md | NEW → new | Meshing · Rasm.Geometry.Intersection · SectionFault 2425. The slice-stack owner: ONE section fold composing IntersectOp.PlaneMesh over a parallel-plane family; layer-height POLICY ROWS (uniform · adaptive · variable-by-slope · support-interface — ALL FOUR as seed rows, the family OPEN per [GENERATOR_LAW] ...); per-layer contours ORIENTED (outer CCW / holes CW — riding intersect's re-founded oriented chains) with typed OPEN-chain rows for non-watertight sections (never silent closure); contour-NESTING TREES emitted as a kernel-owned SoA forest wire pinned to FIVE channels — layers · contours · nesting parent/child index arrays · open chains · elevations — the complete schema the FAB:48 and Compute-circulation decoders bind (QuikGraph serves in-computation only, per the landed lane law); V10b exactness rides the cheapest implicit sub-family (LPI-3D + projected Orient2D + LessThan ordering; contour orientation/nesting are exact signs; coordinates round at Polyline emission) |` entry `Fin<SliceStack> Slicing.Apply(SliceOp, Op? key = null)` | out: intersect (`PlaneMesh` fold + oriented chains), edit (soup), predicates (nesting signs), faults; in: Fabrication (`FAB:23` `Slicing.cs` DIES; `FAB:48` re-route REALIZED), Compute circulation (`RASM-CS-COMPUTE [V12]`a).

### Row 18 skeleton.md (`:96`)
> `| 18 | Meshing/skeleton.md | NEW → new | Meshing · Rasm.Geometry.Offsetting · CollapseStalled 2418 (+ SkeletonStalled 2417 shared stride). 3D MCF curve-skeleton: author-kernel mean-curvature-flow skeletonization COMPOSING the landed machinery (geodesics.md implicit MCF + fields.md MeanCurvatureFlow case — no permissive engine exists; CGAL GPL rejection stands); COMPOSES offset.md's clearance result family — per-point clearance RADIUS + Clearance(Point3d probe) arbitrary-probe query as the SAME first-class result family across 2D/3D (one clearance vocabulary, never a sibling shape); skeleton topology emitted as a kernel-owned SoA wire (node/edge/radius arrays) |` entry `Fin<CurveSkeleton> Skeletonize.Apply(SkeletonOp, Op? key = null)` | out: `geodesics.md`/`fields.md` (landed MCF), offset (clearance vocabulary — W2 backward), edit (arena), `mesh.md` (substrate), faults; in: Fabrication (`FAB:22` payload).

### Row 19 remesh.md (`:97`)
> `| 19 | Processing/remesh.md | NEW → new | Processing · Rasm.Geometry.Simplification · RemeshStalled 2441 (the widened mesh-rewrite-under-budget charter ...). The remesh substrate, BOTH tiers landed (quad remesh is NOT deferred — panelize composes it ...): Botsch-Kobbelt isotropic remesh (split/collapse/flip/tangential-relax, genus-preserving) + cross-field-guided QUAD remesh composing landed segment.md Knöppel cross-fields/stripes — the AUTHOR-KERNEL robust tier BESIDE segment.md's landed HOST-capture tier (QuadRemesh/QuadTarget/RemeshKind), coexisting per the capture law with one anchor each, sited in the SAME folder; decimate.md's VoxelRemesh stays a distinct row (recorded); Compute volumetric discretization names isotropic as its boundary-conditioning pre-step |` entry `Fin<RemeshResult> Remeshing.Apply(RemeshOp, Op? key = null)` (Isotropic · QuadField rows) | out: edit (arena), `segment.md` (landed cross-fields, one anchor), predicates (flip gates), `mesh.md`, faults; in: panelize (W4 backward), Fabrication mesh-prep, `RASM-CS-COMPUTE [V7]`.

### Row 20 nurbs.md (`:98`)
> `| 20 | Parametric/nurbs.md | NEW → new | Parametric · Rasm.Geometry.Parametric · 2448 (shared). THE VENDORED NURBS ENGINE (ruling R1): the whole MIT algorithm set owned in-kernel — NURBS-Book evaluation (curve+surface, arbitrary order), the KnotVector algebra (public Refine both carriers, Normalize, periodic/clamped vocabulary), Piegl-Tiller Bezier-decomposed Gauss-Legendre arc-length (1e-6) with ParameterAtLength/length inversion, Wang-2008 double-reflection RMF with the closure-correction policy row (#373) + coincident-sample guard, Fitting interpolation/approximation for curves AND the Piegl-Tiller SURFACE interpolation/approximation lane (G5 — normal-offset refit substrate), PUBLIC arbitrary-knot construction for both carriers (G1 — vendored-source edit anchor NurbsSurface.cs:29), PUBLIC raw RationalDerivatives with the #234 order convention FIXED (G2 — anchor Evaluate/Surface.cs:25), fundamental forms I/II + principal/Gaussian/mean curvature as kernel projections over those derivatives (G3), ClosestParameter with PARAMETERIZED iteration/tolerance knobs (G7); ingress law: knots NORMALIZED + clamped at construction, canonical bytes hash the normalized form (G6/G10 ...); every construction/fitting throw site wrapped to Fin (G9); SoA span/caching wins recorded as the perf law (#391 class) ... |` entry `Fin<NurbsForm> Nurbs.Of(NurbsWire)` — ONE polymorphic admission discriminating curve/surface wire; evaluation members live on the carriers; op rails live in curve/surface | out: predicates (exact gates where signs matter), `Rasm.Vectors` (wire types), faults; in: curve, surface, develop (strip evaluation); host wire ingress; manifest: GShark pin + `Rasm.csproj:21` + kernel `api-gshark.md` DELETE with this landing.

### Fault mints — `[01]` signature-lock (`:48`)
> `SectionFault(int layer, double elevation, int openChains)` 2425 · `CollapseStalled(int iteration, double residual)` 2418 · `RemeshStalled(double targetLength, double achieved, int iterations)` 2441 · `ParametricFault(ParametricStage stage, string carrier, string witness)` 2448 (stage ∈ {Construction, Evaluation, Station, Offset, Encode}).
Rich kept (`:49`): `SkeletonStalled` 2417.

### Namespace/cluster homes — `[01]` partition (`:34,:36,:40,:42`)
- `Rasm.Geometry.Offsetting` `:34` — offset, skeleton — 2416-2419 (`DegenerateOffset`,`SkeletonStalled` 2417,`CollapseStalled` 2418).
- `Rasm.Geometry.Intersection` `:36` — intersect, slice — 2424-2427 (`IntersectionFault`,`SectionFault` 2425).
- `Rasm.Geometry.Simplification` `:40` — decimate, remesh — 2440-2443 (`DecimationFault`,`RemeshStalled` 2441; charter widens: mesh rewrite under budget).
- `Rasm.Geometry.Parametric` `:42` — nurbs, curve, surface, subdivide, develop, panelize, patternmap — 2448-2449 (`ParametricFault` 2448, `DevelopmentFault` 2449). ONE namespace over whole tier.

### R1 GShark vendor ruling (`:7-11`)
> `:9` verified floor: `Divide(maxSegmentLength, equalSegmentLengths=false)` genuine Piegl-Tiller Bezier-decomposed Gauss-Legendre arc-length at 1e-6, `ParameterAtLength`, `PointAtLength`, `PerpendicularFrames(List<double>, Vector3?, Vector3?)` genuine Wang-2008 double-reflection RMF, `IsoCurve`, `ClosestParameter` (curve+surface), `DivideByChordLength` (returns `List<double>`), `DecomposeIntoBeziers`, `Offset`, full `From*` factory family, `SplitAt`/`SubCurve` `[T0,T1]` window, complete canonical-bytes READ surface. Gaps: G1 `NurbsSurface` sole ctor `internal` (`NurbsSurface.cs:29`); G2 surface derivatives `internal` order-flip #234 (`Evaluate/Surface.cs:25`); G3 fundamental forms absent; G5 no surface interp/approx; G6 closed/periodic soft spot + RMF #373; G7 `ClosestParameter` hardcodes maxIterations=5/tol=1e-3; G8 curve `Offset` no deviation bound; G9 exception boundary; G10 raw-knot-domain eval.
> `:11` RULING: vendor WHOLE composed algorithm set one motion (MIT-clean); no-re-mint clause `api-gshark.md:171` forbids owned-kernel NURBS-eval re-implementation.

### Seam ledger + consumer gates (`:117,:123-124,:214,:221-224`)
- `:117` in-kernel: `intersect ← ...·slice·view·curve`; `offset ← skeleton (the ONE clearance vocabulary)`; `nurbs ← curve·surface·develop`; `edit ← ...·remesh·slice·skeleton...`.
- `:123` `FAB:22` `Toolpath/Skeleton.cs` dies → consumes `Offsetting.Apply` + `Skeletonize.Apply` clearance-radius/arbitrary-probe payload (ONE clearance family 2D+3D). `:124` `FAB:23`+`FAB:48` `Slicing.cs` dies → `Slicing.Apply` slice-stack seam.
- `:214` G1 ingress hinge → `nurbs.md` `Nurbs.Of(NurbsWire)` public arbitrary-knot construction. `:221` slice stacks gate. `:223` 3D MCF curve-skeleton composes SAME clearance family.
- `:244` GShark VENDOR manifest motion: wave-4 pin `PROPS:71` + `Rasm.csproj:21` + `api-gshark.md` DELETE with `nurbs.md` landing (one-engine law).
- `[09]:249` QuikGraph bounded-lane law generalizes: kernel graph RESULTS emit SoA wires (skeleton topology, slice forests, naming migration).

## [02]-[GSHARK_CATALOG] — `libs/csharp/Rasm/.api/api-gshark.md` (nurbs vendored source)

- `:54` `[01] NurbsBase (abstract)` — the shared curve algebra: `Degree`/`Knots`/`Weights`/`ControlPoints`/`ControlPointLocations`; whole `PointAt`/`TangentAt`/`ClosestPoint`/`Length`/`Divide`/`SplitAt`/`ElevateDegree`/`Offset` instance surface; base of every concrete curve.
- `:56` `[03] NurbsSurface` — `DegreeU`/`DegreeV`/`KnotsU`/`KnotsV`/`ControlPoints` + `Weights`/`ControlPointLocations` (dehomogenized view — full canonical-bytes READ surface); `From*`/`Ruled`/`Revolved` factory family; `PointAt(u,v)`/`EvaluateAt`/`IsoCurve`/`SplitAt`/`ClosestPoint`.
- `:84` `PerpendicularFrameAt(double t)` / `PerpendicularFrames(List<double> u, …)` — `Plane` rotation-minimizing frame / frame sequence (sweep rails).
- `:85` `Length` (prop) / `LengthAt(double t)` / `ParameterAtLength(double)` / `ParameterAtChordLength(double t, double chordLength)` — total/partial arc length (Gauss-Legendre); param at target length/chord.
- `:86` `ClosestPoint(Point3)` / `ClosestParameter(Point3)` — foot-of-perpendicular / parameter (Newton projection).
- `:87` `Divide(int numberOfSegments)` / `Divide(double maxSegmentLength, bool equalSegmentLengths=false)` / `DivideByChordLength(double)` → `(Points, Parameters)`; `DivideByChordLength` returns `List<double>` (params only).
- `:88` `SplitAt(double t)` / `SplitAt(double[] parameters)` / `SubCurve(Interval domain)`.
- `:128` `Curve.Interpolated(List<Point3> pts, int degree, Vector3? startTangent=null, Vector3? endTangent=null, bool centripetal=false)`. `:129` `Curve.Approximate(List<Point3> pts, int degree, bool centripetal=false)`. `:130` `Curve.InterpolateBezier(List<Point3> pts)` → `List<NurbsBase>`.
- `:156` `Evaluate.RationalDerivatives(NurbsBase curve, double parameter, int numberOfDerivatives=1)` → `List<Vector3>` — the primitive `DerivativeAt` wraps (G2 surface analog is the internal `Evaluate/Surface.cs:25` the vendor publicizes).
- `:157` `Evaluate.OneBasisFunction(int degree, KnotVector knots, int span, double knot)`.
- `:166` internal-vs-public boundary: `Sampling`/`Analyze`/`Modify` dominantly `internal static`; public = `NurbsBase`/`NurbsSurface` INSTANCE + `Fitting.Curve` + `Intersection.Intersect` + `Evaluate.Evaluate` + named statics `Sampling.Curve.AdaptiveSample`/`AdaptiveSampleRange` + `KnotVector.Refine` (arbitrary knot refinement, BOTH carriers).
- `:175` STACKING vs Rhino: wire is READ-only for surfaces — `NurbsSurface`'s sole ctor is `internal`, no arbitrary-knot public construction (**G1 wall the vendor edits**).
- `:179` GShark is `double`-only, carries NO exact/robust arithmetic — any result feeding a degeneracy-sensitive predicate escalates to `ddouble`→`Expansion`→`Fraction` ladder; evaluation is the geometry, never the adjudication.
- `:171` no-re-mint clause: GShark replaces the in-house NURBS-Book hand-roll — admit the engine, do NOT re-mint basis functions/knot insertion/De Boor/Gauss-Legendre length/Newton closest-point from the textbook.

**assay live spot-verify** (GShark 2.3.1 `NurbsBase` decompile, artifact `.artifacts/assay/scope/api/GShark/decompile.cs`):
```
public abstract class NurbsBase : IGeometry<NurbsBase>, IEquatable<NurbsBase>, ITransformable<NurbsBase>
  public List<double> Weights; public int Degree; public List<Point3> ControlPointLocations;
  public List<Point4> ControlPoints; public KnotVector Knots;
  public virtual double Length => GShark.Analyze.Curve.Length(this);   // internal GL kernel reached ONLY via public Length
  public virtual Point3 StartPoint => PointAt(0.0);  MidPoint => PointAt(0.5);  EndPoint => PointAt(1.0);
  public virtual bool IsClosed; public bool IsPeriodic;
```
Catalog `:54,:73-74` confirmed accurate: `PointAt` on normalized `[0,1]`, `Length` wraps `GShark.Analyze.Curve.Length` (internal Gauss-Legendre).

## [03]-[MATHNET_FOLDER_CATALOG] — `libs/csharp/Rasm/.api/api-mathnet-numerics.md` (nurbs quadrature/root-find; remesh indirect)

- `:101` `Integrate.GaussLegendre(f, a, b, order = 128)` — fixed-order Gauss-Legendre. (**nurbs arc-length quadrature owner; algorithms.md LIBRARY_DEPTH: MathNet owns quadrature**.)
- `:102` `Integrate.GaussKronrod(f, a, b, [out error, out L1Norm,] targetRelativeError=1e-8, maximumDepth=15, order=15)` — adaptive with error estimate.
- `:114` `Brent.TryFindRoot(f, lower, upper, accuracy, maxIter, out root)` → `bool` — canonical no-throw entry (**nurbs `ParameterAtLength` length-inversion root-find**).
- `:117` `NewtonRaphson.FindRoot(f, df, lower, upper, accuracy, maxIter)`. `:118` `RobustNewtonRaphson.FindRoot(f, df, lower, upper, ..., subdivision=20)` — Newton guarded by bisection (**nurbs `ClosestParameter` Newton projection with G7 parameterized iteration/tolerance/subdivision knobs**).
- `:183/:194` `TryFindRoot(..., out double root)` is the no-throw twin the `Fin`/`Option` rail composes; `FindRoot` throws `NonConvergenceException` (G9 wrap-to-`Fin`).
- `:19,:23` MathNet `6.0.0-beta2`; net10 binds `lib/net8.0`. (Version HELD — DECISION E14: `Integrate.OnCuboid` 6.0-only.)
Note: dense/sparse linear algebra + provider is `api-mathnet-providers`/`api-csparse` (`:13`), routed through landed `matrix.md` — the G5 Piegl-Tiller SURFACE interpolation/approximation banded solve rides matrix.md sparse owners, not this catalog.

## [04]-[QUIKGRAPH_CATALOG] — `libs/csharp/.api/api-quikgraph.md` (slice nesting-forest; skeleton graph)

- `:57` `SEdge<TVertex>` — default struct directed edge, `Source`/`Target`, no allocation (dense `int`-keyed network).
- `:70` `BidirectionalGraph<TVertex, TEdge>` — in AND out edges; `InDegree`/`InEdge`/`Degree`, `Roots`/`Sinks`, condensation.
- `:71` `UndirectedGraph<TVertex, TEdge>` — symmetric adjacency; MST + connected-components read.
- `:108` `Roots` / `Sinks` `(this IBidirectionalGraph)` → `IEnumerable<TVertex>` — no-in-edge sources / no-out-edge sinks (**slice nesting-forest roots**).
- `:120` `ComputeTransitiveClosure` / `ComputeTransitiveReduction` `(this IEdgeListGraph[, edgeFactory])` → `BidirectionalGraph` — reachability closure / minimal equivalent edge set (**slice: immediate-parent forest from the full even-odd containment DAG in one call — the redundant-ancestor prune**).
- `:118` `StronglyConnectedComponents` / `WeaklyConnectedComponents` `(graph, IDictionary<TVertex,int> components)` → `int` — labels each vertex with component index (**skeleton branch/component segmentation**).
- `:132` `MinimumSpanningTreePrim` `(this IUndirectedGraph, Func<TEdge,double> edgeWeights)` → `IEnumerable<TEdge>` (**skeleton tree extraction from the collapsed junction graph**).
- `:107` `BreadthFirstSearchAlgorithm<TVertex,TEdge>` + `DiscoverVertex`/`TreeEdge`/`FinishVertex` events (**skeleton connectivity closure**).
- `:145,:150,:159` construction+content-key law: fold typed record into transient graph, run algorithm, project back to typed receipt; graph is `[IgnoreEquality]` transient, NEVER a stored field; key on content address.
- `:170` REJECT: durable graph store — the built view is a transient per-snapshot cache. (**Matches DECISION `[09]:249` "QuikGraph serves in-computation only; kernel graph RESULTS emit SoA wires".**)
- `:38` version `2.5.0`, MS-PL, netstandard2.0.

## [05]-[HIGHPERFORMANCE_CATALOG] — `libs/csharp/.api/api-highperformance.md` (arena SoA wires — all 3 mesh pages)

- `:35` `MemoryOwner<T>` — `IMemoryOwner<T>` over a rented array; heap-allocatable, async-safe; `.Memory`/`.Span`/`Slice` (**SoA wire channel owner: slice 5-channel forest, skeleton node/edge/radius arrays**).
- `:26` `Span2D<T>` / `:29` `Memory2D<T>` — dense plane addressing.
- `:74` `ArrayPoolExtensions.Resize<T>(this ArrayPool<T>, ref T[], int newSize, bool clearArray=false)` — amortized-doubling grow-in-place (arena capacity law).
- `:114` `ParallelHelper.For<TAction>` (`(int start,int end)`/`Range`) + `:118-120` `ForEach<TItem,TAction>` over `Memory<T>`/plane — struct `IAction` folds (**remesh parallel tangential-relax sweep; skeleton parallel contraction**).
- `:176` action seeding: no-seed (`default`-constructed) vs `in TAction` (captured state); `minimumActionsPerThread` floor.
- `:15` version `8.4.2`, MIT; net10 binds `lib/net8.0`.
- `:193` REJECT `HashCode<T>.Combine` — XxHash128/Crc32 monopoly.
Admission (DECISION `[09]:243`): admitted on edit.md arena shapes; slice/skeleton/remesh reach it THROUGH `MeshEdit`.

## [06]-[THINKTECTURE_CATALOG] — `libs/csharp/.api/api-thinktecture-runtime-extensions.md` (all pages: owners/dispatch)

- `:86` `[Union]` / `[Union<T...>]` — regular/ad-hoc discriminated-union + exhaustive dispatch (**`SliceOp`/`SkeletonOp`/`RemeshOp`/`ParametricOp` request unions; `NurbsForm`/`NurbsWire` carrier unions; `RemeshKind` Isotropic|QuadField**).
- `:82` `[SmartEnum]` / `[SmartEnum<TKey>]` — cases, lookup, parse, `Switch`, `Map` (**layer-height policy rows for slice; stage discriminants**).
- `:83` generated lookups are ONLY `Get`/`TryGet(out)`/`Items`; an `Option<T>`-returning `TryGet(key)` is the hand-written one-expression lift ON the owner, never assumed.
- `:89` `Validate(value, provider, out item)` generated factory seam; `:91` `Switch(...)`/`Map(...)` exhaustive dispatch.
- `:8` version `10.4.0`; net10 binds `lib/net9.0`.

## [07]-[SIBLING_SEAM_ANCHORS]

### offset.md (skeleton composes the clearance family) — `libs/csharp/Rasm/.planning/Meshing/offset.md`
- `:5` MINTS the kernel's ONE clearance vocabulary — the SAME result family `Meshing/skeleton` (W4, 3D MCF) composes, so 2D medial and 3D curve-skeleton speak one clearance language.
- `:128` `public sealed record ClearanceNode(Point3d At, double Radius, int NearestEdge);` — position, distance-to-boundary radius, nearest-feature witness.
- `:134-135` `public sealed record SkeletonArc(int From, int To, int OriginEdge);` `public sealed record SkeletonGraph(Seq<ClearanceNode> Nodes, Seq<SkeletonArc> Arcs);` — ONE graph shape for skeleton AND medial.
- `:226` `public sealed record Clearance(Polyline Ring, Point3d Probe, OffsetPolicy Policy) : OffsetOp;` → `:213` `Probe(ClearanceNode Node) : OffsetResult`.
- `:19` "the 3D clearance consumer (`Meshing/skeleton`, W4) COMPOSES `ClearanceNode` — the family widens by zero types".
- `:20` "a sibling 3D clearance shape, or a per-consumer clearance record is the named capability defect".

### intersect.md (slice composes PlaneMesh + oriented Chain) — `libs/csharp/Rasm/.planning/Meshing/intersect.md`
- `:65` `public static readonly IntersectKind PlaneMesh = new("plane-mesh", PrimitiveKind.Plane, PrimitiveKind.Mesh);`
- `:97` `public sealed record Chain(Polyline Points, bool Closed);`
- `:14` `IntersectResult` cases `Points`·`Segments`·`Chains` — `Chains` carries BOTH walked `Chain` rows AND the frozen `CrossLattice`.
- `:16` `PlaneMesh` = sign-driven per-face sweep (infinite plane, no box prune); Chain assembly forward-following material-oriented segments — closed loop closes ORIENTED (outer CCW / holes CW in section frame by construction), source endpoint opens a typed OPEN chain.
- `:19` "the slice-stack consumer (`Meshing/slice`, W4) composes `PlaneMesh` over a plane family — a consumer fold, never a seventh case here".
- `:16` `Intersection.Apply` broad-phase composes `Spatial.Apply`; soup via `MeshEdit.Of`.

### segment.md (remesh composes cross-fields; host-capture tier boundary) — `libs/csharp/Rasm/.planning/Processing/segment.md`
- `:12` `[05]-[DIRECTION_FIELDS]: Knöppel GODF cross fields (smoothest eigenvector / constrained solve / cone holonomy) + stripe patterns.`
- `:744` `SegmentKernel.CrossFieldAt(space, symmetry, constraints, cones, sample, key)` → `Fin<Vector3d>` (frozen `VectorField.CrossField` delegate).
- `:878` `internal static Fin<double> StripeAt(MeshSpace space, VectorField crossField, double frequency, Point3d sample, Op key)` — cross-field-aligned level-set scalar.
- `:13/:894/:927` `[06]-[RESTRUCTURE]`: host-native `QuadRemesh`/`Reduce` behind `RemeshKind` `[Union]` (QuadCase/SimplifyCase) + `QuadTarget` `[Union]` (EdgeLengthCase/QuadCountCase) — the HOST-CAPTURE tier.
- `:897` "this tier is HOST CAPTURE by the standing capture law — the robust first-principles decimation and flattening owners are the settled `decimate`/`flatten`" (remesh.md is the AUTHOR-KERNEL robust tier BESIDE this, one anchor each).

### geodesics.md (skeleton composes implicit MCF) — `libs/csharp/Rasm/.planning/Processing/geodesics.md`
- `:134` `internal static Fin<double> MeanCurvatureMagnitudeAt(MeshSpace space, double timeStep, int iterations, Point3d sample, Op key)`.
- `:138-146` `EnsureMcfDisplacements` — memoized backward-Euler MCF: `space.Laplacian(MeshLaplacian.IntrinsicDelaunay)` → `MeshKernel.AssembleMassStiffnessSystem(laplacian, stiffnessScale: timeStep)` → `CholeskySparse.Of(symmetric)` → `IterateMcf` → `ComputeDisplacements`.
- `:148-159` `IterateMcf` — one SPD factor, per-round mass-weighted RHS solves over x/y/z via `TraverseM`; produces smoothed (CONTRACTED) positions internally.
- `:160-168` `ComputeDisplacements` returns per-vertex displacement MAGNITUDE `Arr<double>` (NOT the contracted geometry — the contracted positions stay private to `IterateMcf`).

### fields.md — `libs/csharp/Rasm/.planning/Spatial/fields.md` (skeleton MCF case; SDF lane)
- `:281` `public sealed record MeanCurvatureFlowCase(MeshSpace Space, PositiveMagnitude TimeStep, Dimension Iterations) : ScalarField;`
- `:284` `SignedDistanceFromMeshCase(MeshSpace Space, SdfMeshPolicy Policy)`; `:298` `Geodesic(MeshSpace space, Seq<int> sources)`.
- `:365` `meanCurvatureFlow -> GeodesicKernel.MeanCurvatureMagnitudeAt`.

### edit.md (all 3 mesh pages compose the arena) — `libs/csharp/Rasm/.planning/Meshing/edit.md`
- `:3` two-carrier seam: `MeshSpace` (immutable snapshot) ⟂ `MeshEdit` (build arena) — the ONLY two mesh carriers; publish by freeze `ToSpace` → `MeshSpace.Of`.
- `:16` `public static MeshEdit Of(MeshSpace space, ArenaPolicy? policy = null)` / `Of(ReadOnlySpan<Point3d>, ReadOnlySpan<(int,int,int)>, ArenaPolicy?)`; `public Fin<MeshSpace> ToSpace(Context, Op?)`; `Kernels.WeldDuplicates(MeshEdit)`.
- `:15` mutation verbs `AddVertex`·`AddFace`·`SetPosition`·`SetFace`·`KillFace`·`Touch` — "`SetFace` is the corner rewrite the decimate edge-collapse re-point and the remesh edge-flip land on".
- `:3/:5` arena consumers explicitly include "remesh, slice, skeleton"; HighPerformance ADMITTED on arena shapes (`Span2D`/`Memory2D`/`ArrayPoolExtensions`/`ParallelHelper`/`BitHelper`).

### curve.md (nurbs consumer; current pre-vendor GShark-package model) — `libs/csharp/Rasm/.planning/Parametric/curve.md`
- `:3` current owner wraps GShark PACKAGE `NurbsBase`/`NurbsSurface` directly; `:33` `using Rhino.Geometry` + borrowed `ParameterizationFault` — the pre-rebuild state. Post-rebuild (row 21) curve.md composes the VENDORED `nurbs.md` `Nurbs.Of(NurbsWire)` engine; nurbs.md supplies the evaluation members, curve.md owns the op rails (Stations/Offset/Divide).
- `:16` marshal pattern: `Rasm.Vectors Point3d/Vector3d/Plane` ↔ GShark `Point3/Vector3/Plane` AT THE BOUNDARY (`ToGShark`/`FromGShark`, `:142-145`).

## [08]-[ARCHITECTURE_ANCHORS] — `libs/csharp/Rasm/ARCHITECTURE.md`
- `:43` `Meshing/Offset.cs # Aichholzer-Aurenhammer skeleton/medial/minkowski OffsetOp`; `:42` `Meshing/Intersect.cs # Predicate-exact IntersectOp crossing lattice`; `:60` `Processing/Segment.cs # ... cross-fields/stripes, host remesh capture`; `:59` `Processing/Geodesics.cs # Heat-method + MMP geodesics, log/exp maps, vector-heat transport, MCF`; `:37` `Parametric/Curve.cs # Host-neutral GShark NURBS contract`.
- `:113` `Rasm.Geometry.*` namespace row frozen by "settled robust-core law; the geometry campaign owns its namespace reconciliation".
- `:101` `* ← csharp:Rasm.Fabrication # [SHAPE]: Matrix / Point3d / Vector3d` (Fabrication is the downstream consumer of slice/skeleton clearance + slice-stack wires).

## [09]-[MANIFEST_FACTS]
- GShark `2.3.1` pin `PROPS:71` + `Rasm.csproj:21` reference + kernel `api-gshark.md` DELETE with `nurbs.md` wave-4 landing (DECISION `[09]:244`; one-engine law — no package beside the vendor). Wave-1 applied 6 catalog corrections so the catalog stays truthful while settled pages cite it.
- MathNet `6.0.0-beta2` HELD (DECISION E14; `Integrate.OnCuboid` 6.0-only). QuikGraph `2.5.0`. HighPerformance `8.4.2`. Thinktecture `10.4.0`. Manifold = in-house `manifoldc` P/Invoke (NO NuGet, arrangement only).
