# [RASM_ARCHITECTURE]

The domain map of `Rasm` — the KERNEL RhinoCommon-aware geometry/numeric kernel, an ordinary planning-scoped package whose whole design corpus lives under one `.planning/` root in nine sub-domain folders. Namespace mirrors folder path (`[03]`). The kernel remains RhinoCommon-aware end to end per the Tier-0 universal-vs-capture law; the pure-numeric floor is host-neutral-shaped without minting a host-free assembly.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm/
├── Domain/                  # The kernel substrate floor every sibling composes
│   ├── Rails.cs             # Op operation key, Expected/Fault union, Catch/Side rail, Lease<T>, IValidityEvidence fold, generator contracts
│   ├── Context.cs           # Tolerance/units value objects + the immutable Context bundle; Of(RhinoDoc) boundary adapter
│   ├── Identity.cs          # ContentHash.Of seed-zero XxHash128 federation key + the one splitmix64 derivation owner
│   ├── Validation.cs        # Requirement/Check readiness algebra + the one OpAcceptance validity oracle + admission vocabulary
│   ├── Normalization.cs     # Topology/Kind taxonomy + capability web, CurveForm, the coercion lattice, one projection carrier
│   ├── Evaluation.cs        # ClosestHit receipt + ClosestOf polymorphic evaluation, frames, sampling, signed distance
│   └── Stats.cs             # ScalarMetric vocabulary, Welford Stat, banded Extrema, Distribution, SampleMoment covariance
├── Numerics/                # Exact-predicate floor + host-neutral-shaped numerics
│   ├── Predicates.cs        # PrecisionTier ladder (Double→DoubleDouble→Interval→Expansion→Rational) exact + implicit-point predicates
│   ├── Faults.cs            # Consolidated band-2400 GeometryFault family
│   ├── Atoms.cs             # Vector-algebra primitive floor + the AtomProjection/ProjectionRow projection dispatch
│   ├── Matrix.cs            # Dense/sparse/complex algebra over MathNet+CSparse; CholeskySparse, GaugePolicy, LOBPCG MatrixKernel
│   ├── Integrate.cs         # IntegratorKind 9-tableau RK floor, ButcherTableau moment validation, dense output, FieldIntegrator
│   ├── Spectral.cs          # DiscreteCalculus DEC bundle, SpectralBasis/SpectralFilter algebra, SpectralDescriptor
│   └── Calculus.cs          # Six-axis central-difference stencil, FieldNoise lattices, weight-kernel/falloff math
├── Spatial/                 # Proximity, clouds, neighborhoods, transport, fields, acceleration, naming
│   ├── Index.cs             # SAH-BVH/Morton-octree SpatialIndex over NodeStore with query/refit fold
│   ├── Naming.cs            # TopoName lineage/NameTable/Track re-anchor
│   ├── Reconciliation.cs    # Reconciliation.Apply(ReconcileOp) fence: EncodeForm {Mesh·Cloud·Parametric} → GeometryHash + NamingHash reconcile
│   ├── Support.cs           # SupportProjection 14-case gated projection over the SupportSpace boundary adapter
│   ├── Cloud.cs             # VectorCloud union, 30-case VectorCloudMetric, CloudKernel PCA, realized hull rail
│   ├── Neighbors.cs         # The one kNN/radius/graph substrate: RTree + KDTree, MST normals, curvature, the one RMF owner
│   ├── Transport.cs         # Log-domain Sinkhorn OT (balanced/unbalanced/debiased) + SinkhornPlan projections
│   └── Fields.cs            # ScalarField/VectorField/TensorField unions, typed SdfKind primitives, BlendKind, SampleDetailed seam
├── Parametric/              # The vendored NURBS engine + host-neutral op tier, evaluation + location
│   ├── Nurbs.cs             # THE vendored NURBS engine (one-engine law): Nurbs.Of four-shape admission, De Boor, Piegl-Tiller curve/surface fits, ToEncodeForm
│   ├── Curve.cs             # ParametricOp eight-case rail (Evaluate·Measure·Divide·Stations·Split·Reconstruct·Offset·Intersect2D); StationField SoA producer
│   ├── Surface.cs           # SurfaceOp six-case rail (Tessellate·Isolines·Geodesics·NormalOffset·CurvatureSample·Pullback); UvTessellation UV carrier
│   ├── Subdivide.cs         # Stencil-row subdivision: Catmull-Clark/Loop SubdivisionScheme rows over ONE refinement fold, semi-sharp creases, Stam limit
│   ├── Develop.cs           # Guaranteed-isometric developable strips: MMP-exact rails, torsal ruling solve, ddouble isometry witness, DevelopmentReceipt
│   ├── Panelize.cs          # Cross-field-guided panelization: Lattice + Seeded families, per-panel frames, planarity acceptance, PanelField SoA wire
│   ├── Patternmap.cs        # Wallpaper-group pattern-to-surface instancing: 17 Seitz rows over ONE orbit fold, PL log-map inversion, vector-heat frames
│   ├── Projections.cs       # CurveProjection/SurfaceProjection selectors, the one shape-operator + pose-slerp owners, SurfaceSpace
│   └── Locate.cs            # Locator/LocationValue/Division location algebra with curvature extrema
├── Meshing/                 # Mesh substrate + construction lattice
│   ├── Delaunay.cs          # Constrained Bowyer-Watson Delaunay/tetrahedralization on InCircle/InSphere
│   ├── Arrangement.cs       # Managed exact boolean/overlay cell-complex retiring the native CSG gate
│   ├── Intersect.cs         # Predicate-exact IntersectOp crossing lattice
│   ├── Slice.cs             # Slicing.Apply slice-stack fold: LayerPlan rows over ONE March integrator, exact-parity nesting forest, SliceStack SoA wire
│   ├── Offset.cs            # Aichholzer-Aurenhammer wavefront OffsetOp (Skeleton/Weighted/Offset/Medial/Minkowski/Clearance)
│   ├── Skeleton.cs          # Au-2008 MCF 3D curve-skeleton: implicit contraction over the MeshEdit arena, cost-ordered collapse to 1D, CurveSkeleton SoA wire
│   ├── Mesh.cs              # MeshSpace snapshot handle, LaplacianCache, IntrinsicMesh + MeshAdjointSnapshot, one cotangent owner, power diagram
│   ├── Edit.cs              # MeshEdit single-writer SoA build arena: one polymorphic Of (space|soup), weld kernel + knob
│   ├── Dec.cs               # AssembleDecOperators, CR connection heat, CDS holonomy, harmonic basis + Hodge decomposition
│   └── Reconstruct.cs       # RBF/MLS/Levin/APSS/Poisson kernels, the unified signed-heat spine, mesh-SDF, iso-extraction
├── Processing/              # Algorithm pipelines over the floors
│   ├── Repair.cs            # HealOp repair algebra + Heal.Repair session fold
│   ├── Receipts.cs          # Typed RebuildReceipt chain + ManifoldStatus + HealSession/RebuildLog
│   ├── Decimate.cs          # SimplifyOp (QuadricCollapse/ProgressiveMesh/VoxelRemesh/FeaturePreserve) Garland-Heckbert QEM decimation
│   ├── Remesh.cs            # Remeshing.Apply two-row rewrite: Botsch-Kobbelt isotropic + cross-field quad extraction; QuadProvenance panelize substrate
│   ├── Flatten.cs           # Harmonic/LSCM/ARAP/BFF ParamOp UV-flattening over the DEC substrate
│   ├── Intent.cs            # VectorIntent consumer rail: Project<TOut>(Context, Op?) dispatch composing every owner
│   ├── Sample.cs            # SampleKind union (Bridson…BNOT power-CCVT) + SampleKernel domain dispatch
│   ├── Extract.cs           # ExtractionDomain ingress, ContourPolicy native-first sectioning, Extraction union
│   ├── Flow.cs              # Termination 6-stop union + FlowKernel.Trace<TOut> dense-output tracing
│   ├── Register.cs          # AlignKind 6-variant ICP dispatcher behind one AlignmentPolicy
│   ├── Geodesics.cs         # Heat-method + MMP geodesics, log/exp maps, vector-heat transport, MCF
│   └── Segment.cs           # HKS/WKS descriptors, feature edges, MeshSegmentation union, cross-fields/stripes, host remesh capture
├── Solving/                 # Nonlinear least-squares owners over the matrix floor
│   ├── Solver.cs            # Lm.Minimize ILmModel λ-ladder (the ONE damped Gauss-Newton) + Constraint solver, island decomposition, DofReport/DofAnalysis
│   └── Fit.cs               # MLESAC FitOp primitive-fit (PROSAC/NAPSAC draws over the kd-tree lane) + FitModel : ILmModel orthogonal-distance refine
├── Drawing/                 # Kernel-quality 2D drawing-geometry producers
│   ├── View.cs              # Predicate-exact hidden-line/silhouette ViewOp returning DrawingProjection
│   └── Pack.cs              # Canonical PackOp geometry-encoding lattice returning EncodedGeometry
└── Analysis/                # The measured-query public entry
    ├── Query.cs             # AnalysisQuery request algebra, Operation<TGeometry,TOut>, Env reader, Analyze facade
    ├── Measure.cs           # Measure mass-property union, Bounds enclosing fits, ConformanceMetric residuals
    ├── Inspect.cs           # Topologies genus/Euler folds + Meshes defect/quality capture
    ├── Select.cs            # Curves/Faces/Points selection unions over the edge taxonomy + PCA spread
    └── Relations.cs         # 25-row Rhino intersection lattice, IntersectionHit/RayQuery, deviation, classification
```

## [02]-[SEAMS]

```text seams
Domain/Identity.cs        ⇄  csharp:Rasm.Element/Projection/address     # [CONTENT_KEY]: seed-zero XxHash128 ContentHash.Of — ONE NodeId/ContentAddress hasher
Domain/Identity.cs        →  csharp:Rasm.Persistence/Element/codec      # [CONTENT_KEY]: ContentAddress composes seed-zero XxHash128 entry, no codec hasher
Domain/Identity.cs        →  csharp:Rasm.Compute/Model/identity         # [CONTENT_KEY]: ModelIdentity.Checksum → ContentHash.Of, the ONE federation hasher
Spatial/Reconciliation.cs →  csharp:Rasm.Persistence/Query/topology     # [CONTENT_KEY]: GeometryHash through the Domain/Identity seed, content-hash ONLY
Spatial/Reconciliation.cs ⇄  python:runtime/evidence/identity           # [CONTENT_KEY]: canonical bytes reproducing the Domain/Identity XxHash128 seed-zero
Spatial/Reconciliation.cs ⇄  typescript:core/value/contentKey           # [CONTENT_KEY]: content-hashing wasm reproducing the Domain/Identity seed-zero
Numerics/Spectral.cs      ⇄  csharp:Rasm.Compute                        # [SHAPE]: DiscreteCalculus DEC operator bundle — the frozen adjoint-carrier shape
Meshing/Mesh.cs           →  csharp:Rasm.Compute                        # [SHAPE]: MeshAdjointSnapshot adjoint handle over the cached DiscreteCalculus
Spatial/Index.cs          →  csharp:Rasm.Fabrication/Toolpath/guard     # [SHAPE]: SpatialIndex BVH broad-phase keep-out prune
Spatial/Index.cs          →  csharp:Rasm.Fabrication/Posting/projection # [SHAPE]: SpatialIndex BVH occluder broad-phase prune
Spatial/Index.cs          →  csharp:Rasm.Compute                        # [WIRE]: Spatial.Apply Wire case emits; Compute decodes
Meshing/Intersect.cs      →  csharp:Rasm.Fabrication/Posting            # [WIRE]: IntersectResult / PlaneMesh section curve
Meshing/Slice.cs          →  csharp:Rasm.Fabrication/Toolpath           # [WIRE]: SliceStack five-channel forest: layers·contours·nesting·chains·elevations
Meshing/Slice.cs          →  csharp:Rasm.Compute                        # [WIRE]: AtElevations story-elevation contours through the SliceStack wire
Meshing/Offset.cs         ⇄  csharp:Rasm.Fabrication/Toolpath           # [SHAPE]: the ONE 2D/3D clearance family — Medial + Clearance(probe) radius payload
Meshing/Skeleton.cs       →  csharp:Rasm.Fabrication/Toolpath           # [WIRE]: CurveSkeleton node/arc/radius SoA + Clearance(probe) — the 3D clearance half
Processing/Remesh.cs      →  csharp:Rasm.Compute                        # [SHAPE]: Isotropic volumetric boundary-conditioning, no Compute-side remesher
Meshing/Arrangement.cs    →  csharp:Rasm.Fabrication/Posting/projection # [WIRE]: Arrangement Apply/ToMesh kept-cell boundary watertight outline
Numerics/Predicates.cs    →  csharp:Rasm.Fabrication/Posting            # [WIRE]: Predicate.Orient2D/Orient3D exact silhouette/winding verdict
Numerics/Predicates.cs    →  csharp:Rasm.Compute/Solver/discretization  # [SHAPE]: CDTet gates — Predicate.Orient3D/InSphere verdicts, no Compute-side mint
Drawing/View.cs           →  csharp:Rasm.Fabrication/Posting            # [PROJECTION]: DrawingProjection / HLR visible/hidden segments
Drawing/View.cs           →  csharp:Rasm.AppUi/Render                   # [PROJECTION]: DrawingProjection / drafting-sheet layout
Drawing/Pack.cs           →  csharp:Rasm.AppHost/Runtime                # [WIRE]: EncodedGeometry / Encode.Apply(PackOp, Op?) channel discriminant
Drawing/Pack.cs           →  csharp:Rasm.Compute/Tensor/residency       # [WIRE]: EncodedGeometry wrapped as EncodedTensor — residency view, never a re-pack
Processing/Flatten.cs     →  csharp:Rasm.Fabrication/Nesting/nfp        # [PROJECTION]: ChartAtlas / UV island layout + DistortionReceipt
Processing/Flatten.cs     →  csharp:Rasm.AppUi/Render                   # [PROJECTION]: ChartAtlas / texture UV channel
Parametric/Develop.cs     →  csharp:Rasm.Fabrication/Nesting/nfp        # [PROJECTION]: ChartAtlas isometric strips + DevelopmentReceipt isometry witness
Parametric/Nurbs.cs       →  csharp:Rasm.Generation                     # [WIRE]: Nurbs.Of(NurbsWire) arbitrary-knot ingress — SpineRef surface resolution G1
Parametric/Curve.cs       →  csharp:Rasm.Generation                     # [WIRE]: Stations StationField SoA over SpineRef window — PathRow/Placement producer
Parametric/Surface.cs     →  csharp:Rasm.Generation                     # [WIRE]: UvTessellation/Isolines/Geodesics/NormalOffset — the UV-provenance carrier
Parametric/Subdivide.cs   →  csharp:Rasm.Generation                     # [WIRE]: region subdivision via SubdividePolicy.Region with sealed T-junction closure
Parametric/Develop.cs     →  csharp:Rasm.Generation                     # [WIRE]: developable gate item — isometry-witnessed strips off DevelopmentReceipt
Parametric/Panelize.cs    →  csharp:Rasm.Generation                     # [WIRE]: PanelField graph + per-panel placement frames — panelization gate item
Parametric/Patternmap.cs  →  csharp:Rasm.Generation                     # [WIRE]: InstanceStream vector-heat-transported frames — PATTERN/TILING input
Processing/Intent.cs      →  csharp:Rasm.Rhino/Camera                   # [BOUNDARY]: VectorIntent + VectorFrame + MotionInterpolation frozen-name contract
Analysis/Query.cs         →  csharp:Rasm.Rhino/Commands                 # [BOUNDARY]: Analyze/AnalysisQuery/Env frozen-name contract; Commands+Overlay bind
*                         ←  csharp:Rasm.Fabrication                    # [SHAPE]: Matrix / Point3d / Vector3d
```

## [03]-[NAMESPACES]

Namespace mirrors folder path — `.editorconfig` `dotnet_style_namespace_match_folder = true:error`: every fence under `Rasm/<Folder>/` declares `namespace Rasm.<Folder>;`, giving the nine roots `Rasm.Analysis`, `Rasm.Domain`, `Rasm.Drawing`, `Rasm.Meshing`, `Rasm.Numerics`, `Rasm.Parametric`, `Rasm.Processing`, `Rasm.Solving`, `Rasm.Spatial`.

The kernel compiles as ONE assembly — the single `Rasm.csproj` — so internal members cross the nine namespaces with no build edge; the root-homed `GeometryFault` union composing upper-tier discriminants (`Numerics/Faults.cs`) is the recorded exception to strata direction under that one-assembly law.

`Rasm.Domain.Fault` and the band-2400 `GeometryFault` family (`Numerics/Faults.cs`) are two families by explicit decision — kernel-substrate faults and robust-core geometry faults; `Numerics/Faults.cs` and `Domain/Rails.cs` each state the seam, and neither absorbs the other.
