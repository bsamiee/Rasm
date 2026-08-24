# [RASM_FABRICATION_NFP]

`Nest` owns true-shape placement over heterogeneous material stock. One admitted `NestPolicy` compiles each search case into the shared candidate algebra, `NoFitPolygon` retains complete configuration-space topology, and exact arc-space collision and containment gate every emitted transform.

`Nest.Solve` preserves the process seam, `Nest.Charts` preserves the atlas projection, and `NestPlan` remains the rectangular hand-off. `Stock.FromRemnant` re-enters the same inventory union, while `FabricationResult.Placement` carries the process projection.

## [01]-[INDEX]

- [02]-[DOMAIN]: Generated owners admit stock, part rules, constraints, search policy, and NFP evidence.
- [03]-[CONFIGURATION_SPACE]: Arc-witnessed Minkowski proposals, exact feasibility, and parallel pair materialization.
- [04]-[SEARCH]: Constraint closure, candidate generation, parameterized search programs, and stock scheduling.
- [05]-[DELIVERY]: Rectangular-plan honor, remnant minting, placement evidence, and content identity.

## [02]-[DOMAIN]

- Owner: `Stock` closes physical inventory modalities while `StockBody` carries common material, topology, exclusion, piece-and-lot trace, and cost facts.
- Owner: `NestPolicy` admits search, clearance, kerf, edge allowance, objective, candidate, constraint, evaluation budget, and batch policy once.
- Cases: `PlacementMode` carries greedy, beam, evolutionary, annealed, Voronoi, and rectangular programs with case-local evidence; rectangular strategy count and depth remain explicit policy.
- Cases: `PlacementConstraint` carries precedence, grouping, separation, adjacency, containment, stock eligibility, and keep-out facts, each occurrence carrying its own `ConstraintForce`.
- Law: one `StockFacts` projection derives common body facts, and one `StockTraits` projection derives physicality, nestability, rectangular extent policy, gauge, and grain; a new modality answers remnant and stock consumers through one case arm.
- Law: `ConstraintForce.Required` rejects a candidate and fails delivery; `ConstraintForce.Preferred` admits the candidate and rides `NestObjective` as weighted penalty.
- Law: six objective weights fan onto one comparable number and every term reaches it DIMENSIONLESS. `NestBasis` carries all three nondimensionalizers — the characteristic length the cut and shared-edge terms divide through, the currency reference the cost term does, and the violation ceiling the constraint term does — on the scoring INPUT, never on `NestEvidence`, because a derived reference on the carrier is a scoring input wearing an evidence column; the basis derives once per solve from the admitted inventory and policy and threads on `SearchState`.
- Packages: `Rasm` supplies `Deterministic` (the ONE draw owner), `ChartAtlas`/`UvIsland.Boundary`, and the `Chain` loop carrier; the `Geometry2D` owner supplies morphology, Boolean, measure, and the cell diagram; `LanguageExt` supplies admission, traversal, and the `Fin` rail; `Thinktecture` supplies the generated stock, constraint, and mode families; `UnitsNet` supplies material quantities and the `Length` ratio the objective's characteristic-length nondimensionalization takes.
- Growth: a stock modality, constraint, candidate source, objective, or search algorithm lands as one case or row consumed by the existing folds.

## [03]-[CONFIGURATION_SPACE]

- Owner: `NoFitPolygon` admits one complete locus with its identity, relation, and approximation witness.
- Cases: `NfpRelation.Forbidden` carries the part-part `MorphologyKind.Sum` locus; `NfpRelation.Admitted` carries the part-stock `MorphologyKind.Difference` inner-fit locus every absolute placement seeds from.
- Cases: `NfpMethod` binds an approximation to its evidence — a chord-projected locus carries positive chord error, an arc-exact locus carries none — and `Nest.MethodOf` DERIVES the row from the operands the Minkowski walk consumes: the line-space walk is exact on bulge-free loops, so a polygonal pair mints the arc-exact locus and an arc-bearing pair the chord-projected one; a policy scalar can never assert a fidelity the operands do not carry.
- Law: `PolygonAlgebra.Apply(new PolygonOp.Morphology(...))` proposes line-space candidates; `ArcAlgebra.Apply(new ArcOp.Inspect(...))` decides containment, exclusion, and collision on the original bulged loops.
- Law: pair identity includes canonical loop geometry, tolerance, rotation, clearance, and chord error; inner-fit identity substitutes stock identity and edge allowance.
- Law: each collision profile offsets its part by half the combined clearance and kerf; stock-boundary feasibility adds edge allowance without weakening part-part or exclusion checks.
- Law: `PairMemo` content-keys the pair matrix under the same `PairTable.Key` identities through the branch `HybridCache` surface — the runtime-carried instance is the in-process tier, a durable L2 federates at the Persistence cache seam, hit and miss counts settle on `NestEvidence` and fire as the engine memo rows, and a failed build throws through the awaited factory so a fault never caches; the runtime cancellation token rides `GetOrBuild` into the awaited cache call and the awaited leg funnels through `Op.Catch`, the ONE inbound exception boundary, so token-proved cancellation lowers to the kernel cancellation rail rather than rethrowing on the async channel; inner-fit rows stay direct because an empty locus is a verdict, not a cacheable polygon.
- Law: the exact execution token owns cancellation: requested polling lowers `Errors.Cancelled`, and matching thrown cancellation lowers through `Op.Catch`; unrequested or foreign cancellation remains the exact captured failure. `PolicyInadmissible` never carries cancellation.
- Auto: `ParallelHelper.For2D` fills uncached independent pair slots; memoized rows await one `HybridCache` task per identity outside the synchronous kernel, and `TraverseM` returns the first typed geometry failure without partial cache publication.
- Packages: the `Geometry2D` owner supplies `PolygonOp.Morphology` and the arc-exact inspection rail; `Rasm` supplies the kernel Minkowski walk beneath it; `Microsoft.Extensions.Caching.Hybrid` supplies the pair memo; `CommunityToolkit.HighPerformance` supplies the parallel pair fill.
- Boundary: an empty pair morphology remains a typed fault, an empty inner-fit locus is the absent-key verdict that no position admits the part, and every returned topology component survives the projection.

## [04]-[SEARCH]

- Owner: `ConstraintGraph` proves precedence acyclic, derives closure for ordering, and precomputes the reduction rank the ordering fold reads.
- Law: exactly TWO of the seven placement constraints mint precedence edges — `Precedes` orders its pair directly and `Inside` orders an outer part before the part it contains. The other five are ORDER-FREE by construction: `Together`, `Separate`, `Adjacent`, `StockOnly`, and `KeepOut` each constrain WHERE a candidate may sit, never when, so they gate at `Accept` against the placed set and a precedence edge for any of them would forbid a placement the geometry admits.
- Law: the transitive CLOSURE and the transitive REDUCTION are both retained as receipt data on the graph — `InDegree` over the closure is the ordering's primary key and the reduction's own topological sort IS the `rank` column its tertiary key reads — so neither walk's output is computed and dropped.
- Owner: `CandidateSource` composes NFP vertices, inner-fit boundaries, arc-native contacts, stock extrema, and relaxed Voronoi centroids into one slot-keyed frontier; its `Absolute` column decides which rows can seed an empty stock.
- Auto: `PlacementMode.Compile` emits one `SearchProgram`; `SearchOp` folds order, branch, breed, mutate, cool, relax, bound, and select steps over one `SearchState`.
- Auto: `SearchState.Evidence.Evaluated` counts exact decisions across every active run, and `SearchOp.Bounded` halts the stochastic sub-program at `NestPolicy.EvaluationBudget`.
- Auto: rectangular programs delegate every packer and heuristic axis to `StockNest.Pack`; `Nest` contains no second rectangle provider switch, and the honoured plan's own `Unplaced` roster carries the graded refusal each unseated instance earned, so the rectangular lane reaches this evidence naming material, extent, or grain rather than a blanket capacity claim.
- Auto: one `PolygonScan` precomputes a branch's placed bounding envelopes once and every candidate position folds over that structure, so a disjoint position costs one bounds test; the arc-exact relation walk that names the colliding part runs only where the scan reports contact.
- Packages: `Rasm` supplies the `Deterministic` lanes every genome, mutation, and cooling draw reads; the `Geometry2D` owner supplies `PolygonOp.Cells` for the relaxed free-space frontier and `PolygonScan` for the placement scan; `QuikGraph` supplies the constraint closure; `StockNest.Pack` owns every rectangular packer axis.
- Boundary: exact containment, overlap, material, exclusion, and blocking-constraint verdicts gate a candidate before objective ranking.

## [05]-[DELIVERY]

- Law: the content preimage covers every `PartTransform` member including `Instance`, so two placements differing only by instance never collide on one key.
- Law: the evidence digest covers what the artifact IS, never what the search measured about it. Columns DERIVED from geometry the placement key already covers — shared-edge overlap, pierce census, and the placement and remnant rosters `KeyOf` frames around the digest — and columns describing the run rather than the result — memo hit and miss counters, settling stamp — stay out of the preimage, so refining a measure, changing a cache tier, or re-running the same solve can never re-key a landed plan.
- Entry: `Nest.Solve` admits profiles, inventory, policy, and the run's own `FabricationRuntime`, then dispatches resolved rectangular plans or true-shape search on one `Fin` rail. `FabricationRuntime` enters WHOLE because tap, memo, token, and settling clock are four columns of one value: the token threads the pair-memo lane into `HybridCache.GetOrCreateAsync` so an in-flight cancel surfaces on the kernel cancellation rail, and the clock stamps the settled receipt where it settles.
- Entry: `NestBench.Workload` admits the `nfp-placement` measured workload — search lane, live inventory, part and budget floors — and `NestBench.Run` is the fold the corpus gate times against `FabricationBenchClaims.NfpPlacement`, taking the same runtime the spine hands the plane so the timed entry is the one a real run reaches; measurement and receipt projection stay the bench edge's under the AppHost claim-field map.
- Entry: `Nest.Charts` admits atlas distortion and reconstructs every island boundary cycle. `Nest.Rings` is the ONE `Chain`-to-`Loop` termination in the package — `Forming/sheet` composes it rather than re-admitting the same kernel carrier, because the walk that produced the chain already owns winding and once-counted edges and a second termination forks the admitted context.
- Receipt: `Receipt<NestEvidence>` is the settled lane output — the ONE settled-receipt carrier — so plane, key, ancestry, band, and stamp arrive from the spine and this plane declares `NestEvidence` alone. `Stamped` is the column the carrier ADDS: no nest output carried a settling instant before, and it stays outside the digest for the same reason the run spine's does, because a key moving with the wall clock addresses nothing. `NestEvidence` retains solver, objective, inventory multiplicity, pair witnesses, constraint verdicts, candidate census, unplaced reasons, consumed cost, the used-to-stock area basis, and the delivered placements and remnants; the settled evidence fires the `FabricationFact.Engine.Of` candidate, evaluated, rejected, memo-hit, and memo-miss rows through `Process/telemetry#FACT_PROJECTION` as kind `engine`.
- Receipt: `FabricationResult.Placement` projects transforms, utilization, unplaced count, remnants, and the evidence-derived content key.
- Packages: `FabricationCanon` over the `Rasm.Element` `CanonicalWriter` is the ONE byte codec every preimage on this page composes — stock and pair identity, evidence digest, placement key — so a `-0.0`, a NaN payload, or a string boundary can never fork identity between two of them, and a `:R`/`:x32` text render of a scalar under a content key is the deleted form; `Rasm` supplies `ContentHash` for the memo key rendering and `UvIsland.Boundary` for the atlas projection; `Rasm.Fabrication.Process` supplies the fault taxonomy, the run runtime, the settled-receipt carrier, and the telemetry tap; `CommunityToolkit.HighPerformance` supplies the parallel pair fill.
- Boundary: remnant difference uses true profiles and the combined clearance-and-kerf offset; feasibility uses the offset collision profiles; only consumed stock enters the area and cost denominators.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using CavalierContours.Core;
using CavalierContours.Polyline;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Caching.Hybrid;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Processing;
using Rhino.Geometry;
using System.Collections.Frozen;
using System.Threading.Tasks;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlacementMode {
    private PlacementMode() { }

    public sealed record BottomLeft : PlacementMode;
    public sealed record Beam(int Width) : PlacementMode;
    public sealed record Genetic(int Population, int Generations, double Mutation, int Seed) : PlacementMode;
    public sealed record Annealed(int Iterations, int Width, double Temperature, double Cooling, int Seed) : PlacementMode;
    public sealed record FreeSpace(int Relaxations, double Strength, int Width) : PlacementMode;
    public sealed record RectFastpath(NestStrategy Strategy, int StrategyBudget, int StrategyDepth,
        int OrientationBudget, int StockLimit) : PlacementMode;

    internal SearchProgram Compile(int budget) => Switch(
        state: budget,
        bottomLeft: static (_, _) => SearchProgram.Create(Seq<SearchOp>(new SearchOp.Ordered(),
            new SearchOp.Branched(1), new SearchOp.Selected(1))),
        beam: static (cap, mode) => SearchProgram.Create(Seq<SearchOp>(new SearchOp.Ordered(),
            new SearchOp.Bounded(cap, Seq<SearchOp>(new SearchOp.Branched(mode.Width), new SearchOp.Selected(mode.Width))))),
        genetic: static (cap, mode) => SearchProgram.Create(Seq<SearchOp>(new SearchOp.Seeded(mode.Population, mode.Seed),
            new SearchOp.Bounded(cap, Seq<SearchOp>(new SearchOp.Repeated(mode.Generations,
                Seq<SearchOp>(new SearchOp.Bred(), new SearchOp.Mutated(mode.Mutation),
                    new SearchOp.Branched(mode.Population), new SearchOp.Selected(mode.Population))))))),
        annealed: static (cap, mode) => SearchProgram.Create(Seq<SearchOp>(new SearchOp.Seeded(mode.Width, mode.Seed),
            new SearchOp.Bounded(cap, Seq<SearchOp>(new SearchOp.Repeated(mode.Iterations,
                Seq<SearchOp>(new SearchOp.Mutated(1.0), new SearchOp.Branched(mode.Width),
                    new SearchOp.Cooled(mode.Temperature, mode.Cooling), new SearchOp.Selected(mode.Width))))))),
        freeSpace: static (cap, mode) => SearchProgram.Create(Seq<SearchOp>(
            new SearchOp.Relaxed(mode.Relaxations, mode.Strength),
            new SearchOp.Bounded(cap, Seq<SearchOp>(new SearchOp.Branched(mode.Width), new SearchOp.Selected(mode.Width))))),
        rectFastpath: static (_, mode) => SearchProgram.Create(Seq<SearchOp>(
            new SearchOp.Rectangular(mode.Strategy, mode.StrategyBudget, mode.StrategyDepth,
                mode.OrientationBudget, mode.StockLimit))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record SearchOp {
    private SearchOp() { }

    public sealed record Ordered : SearchOp;
    public sealed record Seeded(int Population, int Seed) : SearchOp;
    public sealed record Branched(int Width) : SearchOp;
    public sealed record Bred : SearchOp;
    public sealed record Mutated(double Rate) : SearchOp;
    public sealed record Cooled(double Temperature, double Factor) : SearchOp;
    public sealed record Relaxed(int Iterations, double Strength) : SearchOp;
    public sealed record Selected(int Width) : SearchOp;
    public sealed record Repeated(int Count, Seq<SearchOp> Body) : SearchOp;
    public sealed record Bounded(int Evaluations, Seq<SearchOp> Body) : SearchOp;
    public sealed record Rectangular(NestStrategy Strategy, int StrategyBudget, int StrategyDepth,
        int OrientationBudget, int StockLimit) : SearchOp;
}

[ComplexValueObject]
internal sealed partial class SearchProgram {
    public Seq<SearchOp> Steps { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<SearchOp> steps) =>
        validationError = steps.IsEmpty ? new ValidationError("nest:search-program") : null;
}

[SmartEnum<string>]
public sealed partial class NestObjective {
    public static readonly NestObjective Yield = new("yield", ObjectiveWeights.Create(1.0, 0.0, 0.0, 0.0, 0.0, 1.0));
    public static readonly NestObjective Cut = new("cut", ObjectiveWeights.Create(0.0, 1.0, 0.0, 0.0, 1.0, 1.0));
    public static readonly NestObjective Remnant = new("remnant", ObjectiveWeights.Create(0.0, 0.0, 1.0, 0.0, 0.0, 1.0));
    public static readonly NestObjective Cost = new("cost", ObjectiveWeights.Create(0.0, 0.0, 0.0, 1.0, 0.0, 1.0));
    public static readonly NestObjective Balanced = new("balanced", ObjectiveWeights.Create(1.0, 1.0, 1.0, 1.0, 1.0, 1.0));

    public ObjectiveWeights Weights { get; }

    // Six weights fan onto ONE comparable number, so every term reaches it dimensionless. Millimetres are this
    // page's declared carrier — the extent, gauge, and resolution columns all spell it — and the SCORING BASIS
    // supplies both nondimensionalizers: the characteristic length the cut and shared-edge terms divide through,
    // and the cost reference the currency term divides through. Both ride the scoring INPUT rather than the
    // evidence, because every `NestEvidence` column enters the content preimage and a derived reference on the
    // carrier moves every key already minted. With the cost reference threaded, the currency term is like-over-like
    // exactly as the other five are and the structural divergence that term once carried retires.
    public double Score(NestEvidence evidence, NestBasis basis) {
        double area = Math.Max(evidence.StockArea, double.Epsilon);
        return (Weights.Yield * evidence.Utilization)
            - (Weights.Cut * (UnitsNet.Length.FromMillimeters(evidence.CutLength) / basis.Reference))
            + (Weights.Remnant * evidence.RemnantValue / area)
            - (Weights.Cost * (evidence.StockCost / basis.Cost))
            // A shared cut edge is one pierce and one traverse the program never pays for, so the term rewards the
            // collinear overlap `Nesting/linking` measured over the placed set at equal yield.
            + (Weights.SharedEdge * (UnitsNet.Length.FromMillimeters(evidence.SharedEdge) / basis.Reference))
            // The penalty is a COUNT of violations weighted by force, and an unnormalized count reached this sum at
            // whatever magnitude the constraint roster happened to have — a five-constraint run swamped the yield
            // term four times over under equal weights. Its own ceiling is the reference.
            - (Weights.Constraint * evidence.ConstraintPenalty / basis.Constraint);
    }
}

// The scoring basis: the characteristic length every length term divides through, the currency reference the cost
// term does, and the violation ceiling the constraint term does. Derived ONCE per solve from the admitted
// inventory and policy and threaded, never recomputed per candidate and never seated on the digested carrier.
public readonly record struct NestBasis(UnitsNet.Length Reference, double Cost, double Constraint) {
    public static NestBasis Of(Seq<Stock> inventory, NestPolicy policy) {
        double area = Math.Max(inventory.Sum(static stock => stock.Facts.AreaMm2), double.Epsilon);
        return new NestBasis(
            UnitsNet.Length.FromMillimeters(Math.Sqrt(area)),
            Math.Max(inventory.Sum(static stock => stock.Facts.Cost), double.Epsilon),
            // The penalty's own ceiling under unit force: every admitted constraint violated once, plus every
            // requested instance left unplaced by one.
            Math.Max(policy.Constraints.Count + policy.Parts.Sum(static row => row.Quantity), 1));
    }
}

[ComplexValueObject]
public sealed partial class ObjectiveWeights {
    public double Yield { get; }
    public double Cut { get; }
    public double Remnant { get; }
    public double Cost { get; }
    public double SharedEdge { get; }
    public double Constraint { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double yield, ref double cut,
        ref double remnant, ref double cost, ref double sharedEdge, ref double constraint) {
        Seq<double> weights = Seq(yield, cut, remnant, cost, sharedEdge, constraint);
        if (weights.Exists(static weight => !double.IsFinite(weight) || weight < 0.0)
            || weights.Fold(0.0, static (sum, weight) => sum + weight) <= 0.0)
            validationError = new ValidationError("objective-weights");
    }
}

[SmartEnum<string>]
public sealed partial class CandidateSource {
    public static readonly CandidateSource Configuration = new("configuration", Absolute: false, static request =>
        Fin.Succ(request.Placed.Bind(row => request.Pairs
            .Find(PairTable.Key(row.Part, request.Variant, request.Policy)).ToSeq()
            .Bind(static polygon => polygon.Locus).Bind(static loop => toSeq(loop.Vertices))
            .Map(point => new Candidate(request.Part, row.Transform.SheetIndex, row.Stock.Identity,
                point + new Vector3d(row.Transform.Tx, row.Transform.Ty, 0.0), request.Angle, 0.0)))));
    public static readonly CandidateSource Contact = new("contact", Absolute: false, static request =>
        Fin.Succ(request.Placed.Bind(row => Nest.Contacts(row, request.Variant)
            .Map(slot => new Candidate(request.Part, row.Transform.SheetIndex, row.Stock.Identity,
                slot.Point, request.Angle, slot.Length)))));
    public static readonly CandidateSource InnerFit = new("inner-fit", Absolute: true, static request =>
        Fin.Succ(request.Inventory.Map((stock, slot) => (stock, slot)).Bind(row => request.Pairs
            .Find(PairTable.InnerKey(request.Variant, row.stock, request.Policy)).ToSeq()
            .Bind(static polygon => polygon.Locus).Bind(static loop => toSeq(loop.Vertices))
            .Map(point => new Candidate(request.Part, row.slot, row.stock.Identity, point, request.Angle, 0.0)))));
    public static readonly CandidateSource Extrema = new("extrema", Absolute: true, static request =>
        Fin.Succ(request.Inventory.Map((stock, slot) => (stock, slot)).Bind(row => row.stock.Region.Bind(loop => toSeq(loop.Vertices)
            .Map(point => new Candidate(request.Part, row.slot, row.stock.Identity,
                point - (request.Variant.Rotated.Bound().Min - Point3d.Origin), request.Angle, 0.0))))));
    public static readonly CandidateSource Voronoi = new("voronoi", Absolute: true, static request =>
        Nest.VoronoiCandidates(request.Part, request.Variant, request.Angle, request.Placed, request.Inventory,
            request.VoronoiIterations, request.VoronoiStrength));

    public bool Absolute { get; }
    internal Func<CandidateRequest, Fin<Seq<Candidate>>> Generate { get; }
}

[SmartEnum<string>]
public sealed partial class CandidateOrder {
    public static readonly CandidateOrder BottomLeft = new("bottom-left", CandidateWeights.Create(1.0, 1.0, 0.0));
    public static readonly CandidateOrder Contact = new("contact", CandidateWeights.Create(0.0, 0.0, 1.0));
    public static readonly CandidateOrder Balanced = new("balanced", CandidateWeights.Create(1.0, 1.0, 1.0));

    public CandidateWeights Weights { get; }
    internal double Rank(Candidate candidate) => (Weights.X * candidate.Point.X) + (Weights.Y * candidate.Point.Y)
        - (Weights.Contact * candidate.Contact);
}

[ComplexValueObject]
public sealed partial class CandidateWeights {
    public double X { get; }
    public double Y { get; }
    public double Contact { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double x, ref double y, ref double contact) =>
        validationError = !double.IsFinite(x) || x < 0.0 || !double.IsFinite(y) || y < 0.0
            || !double.IsFinite(contact) || contact < 0.0 || x + y + contact <= 0.0
                ? new ValidationError("nest:candidate-weights")
                : null;
}

[SmartEnum<string>]
public sealed partial class ConstraintForce {
    public static readonly ConstraintForce Required = new("required", Blocking: true, Penalty: 4.0);
    public static readonly ConstraintForce Preferred = new("preferred", Blocking: false, Penalty: 1.0);

    public bool Blocking { get; }
    public double Penalty { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlacementConstraint {
    private PlacementConstraint() { }

    public sealed record Precedes(int Before, int After, ConstraintForce Force) : PlacementConstraint;
    public sealed record Together(Seq<int> Parts, ConstraintForce Force) : PlacementConstraint;
    public sealed record Separate(int Left, int Right, double Distance, ProximityMetric Metric,
        ConstraintForce Force) : PlacementConstraint;
    public sealed record Adjacent(int Left, int Right, double MaximumDistance, ProximityMetric Metric,
        ConstraintForce Force) : PlacementConstraint;
    public sealed record Inside(int Inner, int Outer, ConstraintForce Force) : PlacementConstraint;
    public sealed record StockOnly(int Part, FrozenSet<UInt128> Stock, ConstraintForce Force) : PlacementConstraint;
    public sealed record KeepOut(UInt128 Stock, Seq<Loop> Region, ConstraintForce Force) : PlacementConstraint;

    public ConstraintForce Force => Switch(
        precedes: static row => row.Force, together: static row => row.Force, separate: static row => row.Force,
        adjacent: static row => row.Force, inside: static row => row.Force, stockOnly: static row => row.Force,
        keepOut: static row => row.Force);
}

[SmartEnum<string>]
public sealed partial class ProximityMetric {
    public static readonly ProximityMetric Centroid = new("centroid", static (left, right) =>
        left.Bound().Center.DistanceTo(right.Bound().Center));
    public static readonly ProximityMetric Envelope = new("envelope", static (left, right) => {
        BoundingBox a = left.Bound(), b = right.Bound();
        double x = Math.Max(0.0, Math.Max(a.Min.X - b.Max.X, b.Min.X - a.Max.X));
        double y = Math.Max(0.0, Math.Max(a.Min.Y - b.Max.Y, b.Min.Y - a.Max.Y));
        return Math.Sqrt((x * x) + (y * y));
    });
    public static readonly ProximityMetric Boundary = new("boundary", static (left, right) =>
        Math.Min(Nearest(left, right), Nearest(right, left)));

    internal Func<Loop, Loop, double> Measure { get; }

    // Vertex-against-edge suffices for the boundary metric: two non-crossing segments meet at an endpoint
    // projection, so the edge-pair minimum is already this census; the projection itself is the Geometry2D
    // `EdgeSeparation` point modality, never a page-local twin.
    static double Nearest(Loop host, Loop probe) => toSeq(Enumerable.Range(0, host.Count))
        .Bind(span => toSeq(probe.Vertices).Map(point => new Edge3(host.At(span), host.At(span + 1)).Gap(point)))
        .Fold(double.PositiveInfinity, Math.Min);
}

[ComplexValueObject]
public sealed partial class PartRule {
    public int PartId { get; }
    public int Quantity { get; }
    public Option<MaterialId> Material { get; }
    public Seq<double> Angles { get; }
    public Option<double> GrainAxis { get; }
    public int Priority { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int partId, ref int quantity,
        ref Option<MaterialId> material, ref Seq<double> angles, ref Option<double> grainAxis, ref int priority) =>
        validationError = partId < 0 || quantity < 1 || angles.IsEmpty || angles.Exists(static angle => !double.IsFinite(angle))
            || angles.Distinct().Count != angles.Count
            || grainAxis.Exists(static angle => !double.IsFinite(angle))
                ? new ValidationError("nest:part-rule")
                : null;
}

[ComplexValueObject]
public sealed partial class StockBody {
    public MaterialId Material { get; }
    public Context Tolerance { get; }
    public Seq<Loop> Region { get; }
    public Seq<Loop> Exclusions { get; }
    public string Piece { get; }
    public string Lot { get; }
    public Option<string> Heat { get; }
    public double Cost { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref MaterialId material, ref Context tolerance,
        ref Seq<Loop> region, ref Seq<Loop> exclusions, ref string piece, ref string lot, ref Option<string> heat, ref double cost) =>
        validationError = !tolerance.IsValid || region.IsEmpty || region.ForAll(static loop => loop.Winding() != Sign.Positive)
            || region.Concat(exclusions).Exists(loop => !loop.Closed || loop.Count < 3 || loop.Tolerance != tolerance)
            || string.IsNullOrWhiteSpace(piece) || string.IsNullOrWhiteSpace(lot) || !double.IsFinite(cost) || cost < 0.0
                ? new ValidationError("nest:stock-body")
                : null;
}

// Filament projects an empty planar region because its body region is a spool cross-section, never a nestable surface.
internal readonly record struct StockFacts(MaterialId Material, Context Tolerance, Seq<Loop> Region,
    Seq<Loop> Exclusions, double Cost) {
    public static StockFacts Of(StockBody body) => new(body.Material, body.Tolerance,
        body.Region, body.Exclusions, body.Cost);
}

public readonly record struct RectangularExtentPolicy(
    bool Admitted,
    Option<double> MaximumWidthMm,
    Option<double> MaximumLengthMm) {
    public static readonly RectangularExtentPolicy Forbidden = new(false, None, None);
    public static readonly RectangularExtentPolicy Region = new(true, None, None);

    public static RectangularExtentPolicy Bounded(double widthMm, double lengthMm) =>
        new(true, Some(widthMm), Some(lengthMm));

    public bool Fits(BoundingBox bounds) {
        double width = bounds.Max.X - bounds.Min.X;
        double length = bounds.Max.Y - bounds.Min.Y;
        return Admitted && double.IsFinite(width) && width > 0.0 && double.IsFinite(length) && length > 0.0
            && MaximumWidthMm.ForAll(maximum => width <= maximum)
            && MaximumLengthMm.ForAll(maximum => length <= maximum);
    }
}

public readonly record struct StockTraits(
    bool Physical,
    bool Nestable,
    RectangularExtentPolicy RectangularExtent,
    Option<double> GaugeMm,
    Option<double> GrainAxis);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Stock {
    private Stock() { }

    public sealed record Sheet(StockBody Body, double Thickness, Option<double> GrainAxis) : Stock;
    public sealed record Plate(StockBody Body, double Thickness, Option<double> GrainAxis) : Stock;
    public sealed record Roll(StockBody Body, double Width, double AvailableLength, Option<double> GrainAxis) : Stock;
    public sealed record Coil(StockBody Body, double Width, double AvailableLength, double Thickness, Option<double> GrainAxis) : Stock;
    public sealed record BarStock(StockBody Body, double Diameter, double Length, double EndAllowance) : Stock;
    public sealed record TubeStock(StockBody Body, double OuterDiameter, double WallThickness, double Length, double SeamAllowance, double EndAllowance) : Stock;
    public sealed record Billet(StockBody Body, double Depth) : Stock;
    public sealed record Filament(StockBody Body, double Diameter, double SpoolLength) : Stock;
    public sealed record FromRemnant(Remnant Remnant) : Stock;

    public MaterialId Material => Facts.Material;
    public Context Tolerance => Facts.Tolerance;
    public Seq<Loop> Region => Facts.Region;
    public Seq<Loop> Exclusions => Facts.Exclusions;
    public double Cost => Facts.Cost;
    public bool Physical => Traits.Physical;
    public bool Nestable => Traits.Nestable;
    public RectangularExtentPolicy RectangularExtent => Traits.RectangularExtent;
    public Option<double> GaugeMm => Traits.GaugeMm;
    public Option<double> GrainAxis => Traits.GrainAxis;

    StockFacts Facts => Switch(
        sheet: static row => StockFacts.Of(row.Body),
        plate: static row => StockFacts.Of(row.Body),
        roll: static row => StockFacts.Of(row.Body),
        coil: static row => StockFacts.Of(row.Body),
        barStock: static row => StockFacts.Of(row.Body),
        tubeStock: static row => StockFacts.Of(row.Body),
        billet: static row => StockFacts.Of(row.Body),
        filament: static row => StockFacts.Of(row.Body) with { Region = Seq<Loop>(), Exclusions = Seq<Loop>() },
        fromRemnant: static row => new StockFacts(row.Remnant.Material, row.Remnant.Boundary.Tolerance,
            row.Remnant.Region, row.Remnant.Profile.Exclusions, row.Remnant.Value.IfNone(0.0)));

    StockTraits Traits => Switch(
        state: Area,
        sheet: static (area, row) => TraitsOf(area,
            Positive(row.Thickness) && Axis(row.GrainAxis), true,
            RectangularExtentPolicy.Region, Some(row.Thickness), row.GrainAxis),
        plate: static (area, row) => TraitsOf(area,
            Positive(row.Thickness) && Axis(row.GrainAxis), true,
            RectangularExtentPolicy.Region, Some(row.Thickness), row.GrainAxis),
        roll: static (area, row) => TraitsOf(area,
            Positive(row.Width) && Positive(row.AvailableLength) && Axis(row.GrainAxis), true,
            RectangularExtentPolicy.Bounded(row.Width, row.AvailableLength), None, row.GrainAxis),
        coil: static (area, row) => TraitsOf(area,
            Positive(row.Width) && Positive(row.AvailableLength) && Positive(row.Thickness) && Axis(row.GrainAxis), true,
            RectangularExtentPolicy.Bounded(row.Width, row.AvailableLength), Some(row.Thickness), row.GrainAxis),
        barStock: static (area, row) => TraitsOf(area,
            Positive(row.Diameter) && Positive(row.Length) && Nonnegative(row.EndAllowance)
                && (2.0 * row.EndAllowance) < row.Length,
            false, RectangularExtentPolicy.Forbidden, None, None),
        tubeStock: static (area, row) => TraitsOf(area,
            Positive(row.OuterDiameter) && Positive(row.WallThickness)
                && row.WallThickness < 0.5 * row.OuterDiameter && Positive(row.Length)
                && Nonnegative(row.SeamAllowance) && Nonnegative(row.EndAllowance)
                && (2.0 * row.EndAllowance) < row.Length,
            false, RectangularExtentPolicy.Forbidden, Some(row.WallThickness), None),
        billet: static (area, row) => TraitsOf(area, Positive(row.Depth), true,
            RectangularExtentPolicy.Region, Some(row.Depth), None),
        filament: static (area, row) => TraitsOf(area,
            Positive(row.Diameter) && Positive(row.SpoolLength), false,
            RectangularExtentPolicy.Forbidden, None, None),
        fromRemnant: static (area, row) => TraitsOf(area,
            !row.Remnant.Region.IsEmpty && Axis(row.Remnant.Profile.GrainAxisRadians)
                && row.Remnant.Profile.GaugeMm.ForAll(static gauge => double.IsFinite(gauge) && gauge >= 0.0)
                && row.Remnant.Profile.CostPerSquareMillimeter.ForAll(static cost => double.IsFinite(cost) && cost >= 0.0)
                && row.Remnant.Profile.Exclusions.ForAll(exclusion => exclusion.Closed && exclusion.Count >= 3
                    && exclusion.Tolerance == row.Remnant.Boundary.Tolerance),
            true, RectangularExtentPolicy.Region, row.Remnant.Profile.GaugeMm,
            row.Remnant.Profile.GrainAxisRadians));

    public double Area => Math.Max(0.0, Math.Abs(Region.Sum(static loop => loop.Area()))
        - Exclusions.Sum(static loop => Math.Abs(loop.Area())));
    public UInt128 Identity => Nest.Identity(Region, Tolerance, writer =>
        IdentitySalt(writer).U128(Nest.Identity(Exclusions, Tolerance, static salt => salt.String("stock-exclusions"))));

    static bool Positive(double value) => double.IsFinite(value) && value > 0.0;
    static bool Nonnegative(double value) => double.IsFinite(value) && value >= 0.0;
    static bool Axis(Option<double> value) => value.ForAll(double.IsFinite);
    static StockTraits TraitsOf(double area, bool physical, bool nestable,
        RectangularExtentPolicy rectangular, Option<double> gauge, Option<double> grain) =>
        new(physical, physical && nestable && area > 0.0, rectangular, gauge, grain);

    // Each modality writes its bounded discriminator token then its own scalars through the codec; field ORDER is
    // the page's and stays, while the hand-counted string lengths go — `String` already frames by UTF-8 byte count
    // and `FabricationCanon.Maybe` already frames absence as a presence bit ahead of the payload.
    CanonicalWriter IdentitySalt(CanonicalWriter writer) => Switch(
        state: writer,
        sheet: static (sink, row) => BodyKey(sink.String("sheet"), row.Body).Double(row.Thickness).Maybe(row.GrainAxis, Scalar),
        plate: static (sink, row) => BodyKey(sink.String("plate"), row.Body).Double(row.Thickness).Maybe(row.GrainAxis, Scalar),
        roll: static (sink, row) => BodyKey(sink.String("roll"), row.Body)
            .Double(row.Width).Double(row.AvailableLength).Maybe(row.GrainAxis, Scalar),
        coil: static (sink, row) => BodyKey(sink.String("coil"), row.Body)
            .Double(row.Width).Double(row.AvailableLength).Double(row.Thickness).Maybe(row.GrainAxis, Scalar),
        barStock: static (sink, row) => BodyKey(sink.String("bar"), row.Body)
            .Double(row.Diameter).Double(row.Length).Double(row.EndAllowance),
        tubeStock: static (sink, row) => BodyKey(sink.String("tube"), row.Body)
            .Double(row.OuterDiameter).Double(row.WallThickness).Double(row.Length)
            .Double(row.SeamAllowance).Double(row.EndAllowance),
        billet: static (sink, row) => BodyKey(sink.String("billet"), row.Body).Double(row.Depth),
        filament: static (sink, row) => BodyKey(sink.String("filament"), row.Body)
            .Double(row.Diameter).Double(row.SpoolLength),
        fromRemnant: static (sink, row) => sink.String("remnant").U128(row.Remnant.Identity)
            .Maybe(row.Remnant.Profile.GaugeMm, Scalar)
            .Maybe(row.Remnant.Profile.GrainAxisRadians, Scalar)
            .Maybe(row.Remnant.Profile.CostPerSquareMillimeter, Scalar));

    static CanonicalWriter BodyKey(CanonicalWriter writer, StockBody body) => writer
        .String(body.Material.Value).String(body.Piece).String(body.Lot)
        .Maybe(body.Heat, static (held, heat) => held.String(heat))
        .Double(body.Cost);

    static CanonicalWriter Scalar(CanonicalWriter writer, double value) => writer.Double(value);
}

[ComplexValueObject]
public sealed partial class NestPolicy {
    public PlacementMode Mode { get; }
    public Seq<PartRule> Parts { get; }
    public Seq<PlacementConstraint> Constraints { get; }
    public FrozenSet<CandidateSource> Candidates { get; }
    public CandidateOrder Frontier { get; }
    public NestObjective Objective { get; }
    public double Clearance { get; }
    public double ChordError { get; }
    public double Kerf { get; }
    public double EdgeAllowance { get; }
    public double RectangleResolution { get; }
    public int PairBatchFloor { get; }
    public int PairConcurrency { get; }
    public int EvaluationBudget { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref PlacementMode mode, ref Seq<PartRule> parts,
        ref Seq<PlacementConstraint> constraints, ref FrozenSet<CandidateSource> candidates, ref CandidateOrder frontier,
        ref NestObjective objective,
        ref double clearance, ref double chordError, ref double kerf, ref double edgeAllowance,
        ref double rectangleResolution, ref int pairBatchFloor, ref int pairConcurrency, ref int evaluationBudget) =>
        validationError = parts.IsEmpty || toSeq(parts.GroupBy(static row => row.PartId)).Exists(static group => group.Count() != 1)
            || candidates.Count == 0 || !double.IsFinite(clearance) || clearance < 0.0 || !double.IsFinite(chordError)
            || chordError <= 0.0 || !double.IsFinite(kerf) || kerf < 0.0 || !double.IsFinite(edgeAllowance)
            || edgeAllowance < 0.0 || !double.IsFinite(rectangleResolution) || rectangleResolution <= 0.0
            || pairBatchFloor < 1 || pairConcurrency < 1 || evaluationBudget < 1 || !Admits(mode)
            || !candidates.Any(static source => source.Absolute)
                ? new ValidationError("nest:nest-policy")
                : null;

    static bool Admits(PlacementMode mode) => mode.Switch(
        bottomLeft: static _ => true,
        beam: static row => row.Width > 0,
        genetic: static row => row.Population > 1 && row.Generations > 0 && row.Mutation is >= 0.0 and <= 1.0,
        annealed: static row => row.Iterations > 0 && row.Width > 1 && double.IsFinite(row.Temperature)
            && row.Temperature > 0.0 && row.Cooling is > 0.0 and < 1.0,
        freeSpace: static row => row.Relaxations >= 0 && row.Strength is >= 0.0 and <= 1.0 && row.Width > 0,
        rectFastpath: static row => row.Strategy is not null && row.StrategyBudget > 0 && row.StrategyDepth >= 0
            && row.OrientationBudget > 0 && row.StockLimit > 0
            && (long)row.StrategyBudget * row.OrientationBudget <= int.MaxValue);

}

[SmartEnum<string>]
public sealed partial class NfpRelation {
    public static readonly NfpRelation Forbidden = new("forbidden", MorphologyKind.Sum, Admits: false);
    public static readonly NfpRelation Admitted = new("admitted", MorphologyKind.Difference, Admits: true);

    public bool Admits { get; }
    internal MorphologyKind Kind { get; }
}

[SmartEnum<string>]
public sealed partial class NfpMethod {
    public static readonly NfpMethod ChordProjected = new("chord-projected", Exact: false);
    public static readonly NfpMethod ArcExact = new("arc-exact", Exact: true);

    public bool Exact { get; }
}

public readonly record struct NfpWitness(UInt128 Pair, UInt128 Fixed, UInt128 Orbiting, NfpRelation Relation,
    NfpMethod Method, double ChordError, double Clearance, double Kerf, int Components, int Holes);

[ComplexValueObject]
public sealed partial class NoFitPolygon {
    public Seq<Loop> Locus { get; }
    public UInt128 Identity { get; }
    public NfpWitness Witness { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Seq<Loop> locus, ref UInt128 identity,
        ref NfpWitness witness) =>
        validationError = locus.IsEmpty || identity == UInt128.Zero || identity != witness.Pair
            || witness.Fixed == UInt128.Zero || witness.Orbiting == UInt128.Zero
            || locus.Exists(static loop => !loop.Closed || loop.Count < 3)
            || witness.Components != locus.Count
            || witness.Holes != locus.Count(static loop => loop.Winding() == Sign.Negative)
            || witness.Method.Exact == (witness.ChordError > 0.0)
            || !double.IsFinite(witness.ChordError) || witness.ChordError < 0.0 || !double.IsFinite(witness.Clearance)
            || witness.Clearance < 0.0 || !double.IsFinite(witness.Kerf) || witness.Kerf < 0.0
                ? new ValidationError(string.Join(" | ", new object?[] { Kind.Polyline, None, "nest:nfp" }))
                : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UnplacedReason {
    private UnplacedReason() { }

    public sealed record Material(int PartId, int Instance, UInt128 Stock) : UnplacedReason;
    public sealed record Grain(int PartId, int Instance, UInt128 Stock) : UnplacedReason;
    public sealed record Boundary(int PartId, int Instance, UInt128 Stock) : UnplacedReason;
    public sealed record Collision(int PartId, int Instance, int OtherPartId, int OtherInstance) : UnplacedReason;
    public sealed record Exclusion(int PartId, int Instance, UInt128 Stock) : UnplacedReason;
    public sealed record Constraint(int PartId, int Instance, PlacementConstraint Rule) : UnplacedReason;
    public sealed record Budget(int PartId, int Instance, int Evaluated) : UnplacedReason;
    public sealed record Capacity(int PartId, int Instance) : UnplacedReason;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConstraintVerdict {
    private ConstraintVerdict() { }

    public sealed record Satisfied(PlacementConstraint Rule) : ConstraintVerdict;
    public sealed record Violated(PlacementConstraint Rule) : ConstraintVerdict;

    public PlacementConstraint Constraint => Switch(
        satisfied: static row => row.Rule,
        violated: static row => row.Rule);
    public double Penalty => Switch(
        satisfied: static _ => 0.0,
        violated: static row => row.Rule.Force.Penalty);
    public bool Blocking => Switch(
        satisfied: static _ => false,
        violated: static row => row.Rule.Force.Blocking);
}
// The five DIGESTED measure columns carry the page's declared millimetre basis as bare doubles — square
// millimetres on the three area columns, millimetres on `CutLength`, the caller's price basis on `StockCost` —
// because `Digest` writes every one of them into the content preimage. A typed carrier on a digested column moves
// bytes; conversion belongs at the derivation that needs it, which is why `NestObjective.Score` lifts its
// characteristic length locally. The trailing columns are deliberately OUTSIDE that preimage. `SharedEdge` and
// `Pierces` are functions of the placed geometry the placement key already covers whole, so digesting them would
// re-key every landed plan the day the collinearity measure is refined; the memo counters are run instrumentation
// describing the search rather than the artifact; and `Placements`/`Remnants` are the delivered artifact itself,
// which `KeyOf` already frames member-by-member around this digest, so writing them here folds one fact into the
// key twice. Both ride the evidence because `Receipt<NestEvidence>` varies in `TEvidence` ALONE, and a placement
// roster beside the carrier is a second lane output on a spine that publishes one.
public sealed record NestEvidence(PlacementMode Mode, NestObjective Objective, Seq<UInt128> Stock, Seq<NfpWitness> Pairs,
    Seq<ConstraintVerdict> Constraints, Seq<UnplacedReason> Unplaced, int Candidates, int Evaluated, int Rejected,
    double UsedArea, double StockArea, double CutLength, double RemnantValue, double StockCost,
    double SharedEdge = 0.0, int Pierces = 0, int MemoHits = 0, int MemoMisses = 0,
    Seq<PartTransform> Placements = default, Seq<Remnant> Remnants = default) {
    public double Utilization => StockArea > 0.0 ? Math.Clamp(UsedArea / StockArea, 0.0, 1.0) : 0.0;
    public double ConstraintPenalty => Constraints.Sum(static row => row.Penalty)
        + Unplaced.Count(static row => row is UnplacedReason.Constraint);
}

public readonly record struct PartInstance(int PartId, int Ordinal);
internal sealed record Variant(int PartId, double Rotation, Loop True, Loop Rotated, Loop Collision, UInt128 Identity);
internal sealed record Candidate(PartInstance Part, int StockSlot, UInt128 Stock, Point3d Point, double Rotation, double Contact);
internal sealed record Placed(PartInstance Instance, Variant Part, Stock Stock, PartTransform Transform, Loop Shape, Loop Envelope);
internal sealed record CandidateRequest(PartInstance Part, Variant Variant, double Angle, Seq<Placed> Placed, Seq<Stock> Inventory,
    HashMap<UInt128, NoFitPolygon> Pairs, NestPolicy Policy, int VoronoiIterations, double VoronoiStrength);
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PlacementDecision {
    private PlacementDecision() { }
    public sealed record Accepted(Placed Value, Seq<ConstraintVerdict> Constraints) : PlacementDecision;
    public sealed record Rejected(UnplacedReason Reason) : PlacementDecision;
}
internal sealed record Genome(Seq<PartInstance> Order, HashMap<PartInstance, double> Rotation);
internal sealed record SearchRun(Seq<Placed> Placed, Seq<UnplacedReason> Unplaced, Seq<ConstraintVerdict> Constraints,
    int Candidates, int Evaluated, int Rejected);
internal sealed record SearchState(Seq<Genome> Population, Seq<SearchRun> Runs, NestEvidence Evidence, NestBasis Basis,
    ulong Random, double Temperature, int VoronoiIterations, double VoronoiStrength);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Nest {
    // Search seeds: no draw consumed, no annealing heat, one relaxation pass at full strength. Each is the zero of
    // the axis its own program step moves, so none is a tuned value and none belongs on a policy row.
    private const ulong DrawSeed = 0UL;
    private const double HeatSeed = 0.0;
    private const int RelaxationSeed = 1;
    private const double StrengthSeed = 1.0;

    // Tap, memo, token, and settling clock are FOUR columns of ONE runtime, so this entry takes that runtime
    // whole: a signature spelling them apart let a caller hand a live tap beside a foreign token, and no clock a
    // settled receipt stamps with had any way in at all.
    public static ValueTask<Fin<FabricationResult>> Solve(
        FabricationPolicy.Nest policy, FabricationInput input, FabricationRuntime runtime) {
        Fin<Arr<Loop>> admitted = input.Profiles.IsEmpty
            ? Fin.Fail<Arr<Loop>>(FabricationFault.Nested(new NestWitness.EmptyCutList()))
            : input.Profiles.ToSeq().Map((loop, index) => (loop, index))
                .TraverseM(row => Admit(row.loop, row.index)).As().Map(static rows => rows.ToArr());
        return admitted.Match(
            Succ: parts => policy.Plan.Match(
                Some: plan => ValueTask.FromResult(Honor(parts, plan)),
                None: () => policy.Inventory.IsEmpty
                    ? ValueTask.FromResult(Fin.Fail<FabricationResult>(new FabricationFault.StockOverflow(parts.Count, 0)))
                    : policy.Inventory.Filter(static stock => stock.Nestable && !stock.Region.IsEmpty) is Seq<Stock> inventory
                        && !inventory.IsEmpty
                        ? PlaceProjected(parts, inventory, policy.Nesting, runtime)
                        : ValueTask.FromResult(Fin.Fail<FabricationResult>(new FabricationFault.StockOverflow(parts.Count, 0)))),
            Fail: static error => ValueTask.FromResult(Fin.Fail<FabricationResult>(error)));
    }

    static async ValueTask<Fin<FabricationResult>> PlaceProjected(
        Arr<Loop> parts,
        Seq<Stock> inventory,
        NestPolicy policy,
        FabricationRuntime runtime) =>
        (await Place(parts, inventory, policy, runtime).ConfigureAwait(false))
            .Map(receipt => Projected(receipt, runtime.Telemetry));

    // Settled evidence fires one engine census; honored plans search nothing and stay fact-free.
    private static FabricationResult Projected(Receipt<NestEvidence> receipt, FabricationTap tap) =>
        (FabricationFact.Engine.Of(receipt.Evidence).Map(tap.Fire).Strict(), Project(receipt)).Item2;

    public static Fin<Arr<Loop>> Charts(ChartAtlas atlas, double maxAreaStretch, Context tolerance) =>
        !double.IsFinite(maxAreaStretch) || maxAreaStretch < 1.0 || !atlas.Receipt.FlipFreeBijective
        || atlas.Receipt.MaxArea > maxAreaStretch || atlas.Receipt.MinArea < 1.0 / maxAreaStretch
            ? Fin.Fail<Arr<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "atlas:distortion"))
            : atlas.Islands.TraverseM(island => island.Boundary(tolerance, Op.Of(name: nameof(Charts)))
                    .Bind(chains => Rings(chains, tolerance))).As()
                .Map(static regions => regions.Bind(static loops => loops).ToArr());

    internal static Fin<FabricationResult> Honor(Arr<Loop> parts, NestPlan plan) =>
        plan.Placements.TraverseM(row =>
            row.PartId < 0 || row.PartId >= parts.Count
                ? Fin.Fail<PartTransform>(new FabricationFault.NoFit(row.PartId, Seq<double>()))
                : Rotated(parts[row.PartId], row.RotationRadians).Bind(part =>
                    PartTransform.Admit(row.PartId, row.Instance, row.XMm - part.Bound().Min.X,
                        row.YMm - part.Bound().Min.Y, row.RotationRadians, row.SheetIndex, mirrored: false))).As()
            .Bind(placed => toSeq(parts).Head
                .Filter(_ => !placed.IsEmpty)
                .ToFin(new FabricationFault.StockOverflow(parts.Count, plan.Yield.SheetCount))
                .Bind(seed => KeyOf(placed, Seq<Remnant>(), plan.Evidence.Digest, seed.Tolerance))
                .Map(key => (FabricationResult)new FabricationResult.Placement(placed, plan.Yield.UtilizationRatio,
                    plan.Yield.UnplacedCount, Seq<Remnant>(), key)));

    // Every preimage on this page opens and closes at the S0 `FabricationCanon` facade over the one `Rasm.Element`
    // `CanonicalWriter` — the writer's constructor is private and the facade owns both mints, so a page-local
    // opening forged an identity off bytes no writer held. This one answers a UInt128 ORDER over a preimage rather
    // than the preimage itself, which is the STREAMING close: `Ordered` never materializes a buffer, so the
    // per-candidate stock and variant reads stop allocating an array per probe.
    // NAMED LOSS: the `EgressKind` frame `ContentKey.Of` prefixed onto this digest goes, so two byte-identical
    // preimages in different egress families would tie here instead of separating. WITNESS: every caller salts its
    // own bounded discriminant literal ahead of the loops — `"stock-exclusions"`, `"keep-out"`, `"plan"`, the
    // part ordinal and angle — so no two of them can present one preimage, and the one member that DOES address an
    // artifact takes `Keyed` with its kind intact.
    internal static UInt128 Identity(Seq<Loop> loops, Context tolerance, Func<CanonicalWriter, CanonicalWriter> salt) =>
        FabricationCanon.Ordered(tolerance, writer =>
            salt(writer.Double(tolerance.Absolute.Value).Double(tolerance.Angle.Value))
                .Rows(Ordered(loops), static (held, loop) => loop.CanonicalBytes(held)));

    // Set identity needs a deterministic ORDER, and each loop's own canonical preimage IS that key: `Loop.Canonical`
    // is rotation-canonical and tolerance-quantized, so the digest of one loop totally orders the set. The area,
    // count, bound, and vertex-by-vertex comparator family this replaces re-derived that same canonical rotation at
    // every comparison, quadratically, and forked the byte convention against every sibling page.
    private static Seq<Loop> Ordered(Seq<Loop> loops) => toSeq(loops.OrderBy(Preimage));

    private static UInt128 Preimage(Loop loop) =>
        FabricationCanon.Ordered(loop.Tolerance, loop.CanonicalBytes);

    // Exactness is a property of the OPERANDS, never a policy wish: the line-space Minkowski walk is exact on loops
    // that carry no bulge, and lowering introduces chord error only where an arc exists. A pair of bulge-free
    // profiles therefore mints the arc-exact locus carrying zero chord error and an arc-bearing pair the
    // chord-projected one, so both declared methods have a producer and the `NoFitPolygon` validator guards a
    // constructible case rather than an unreachable one.
    internal static NfpMethod MethodOf(params ReadOnlySpan<Loop> operands) =>
        LanguageExt.Iterable<Loop>.FromSpan(operands)
            .Exists(static loop => loop.Bulges.Exists(static bulge => bulge != 0.0))
                ? NfpMethod.ChordProjected
                : NfpMethod.ArcExact;

    static ValueTask<Fin<Receipt<NestEvidence>>> Place(
        Arr<Loop> parts, Seq<Stock> inventory, NestPolicy policy, FabricationRuntime runtime) {
        Fin<(ConstraintGraph Graph, HashMap<(int PartId, long Angle), Variant> Variants)> prepared =
            from _ in policy.Parts.Count != parts.Count
                || policy.Parts.Exists(rule => rule.PartId < 0 || rule.PartId >= parts.Count)
                ? Fin.Fail<Unit>(new KernelFault.InvalidValue("nfp", "nest:part-rule-profile"))
                : Fin.Succ(unit)
            from graph in ConstraintGraph.Admit(parts.Count, policy.Constraints)
            from variants in Variants(parts, policy)
            select (graph, variants);
        return prepared.Match(
            Succ: scope => Search(parts, inventory, policy, runtime, scope.Graph, scope.Variants),
            Fail: static error => ValueTask.FromResult(Fin.Fail<Receipt<NestEvidence>>(error)));
    }

    static async ValueTask<Fin<Receipt<NestEvidence>>> Search(
        Arr<Loop> parts,
        Seq<Stock> inventory,
        NestPolicy policy,
        FabricationRuntime runtime,
        ConstraintGraph graph,
        HashMap<(int PartId, long Angle), Variant> variants) {
        Option<PairMemo> memo = runtime.Memo.Map(static cache => new PairMemo(cache));
        Fin<HashMap<UInt128, NoFitPolygon>> built =
            await PairTable.Build(variants, inventory, policy, memo, runtime.Cancel).ConfigureAwait(false);
        return built.Bind(pairs =>
            from admitted in Initial(inventory, policy, graph)
            let initial = admitted with { Evidence = admitted.Evidence with { Pairs = pairs.Values.Map(static row => row.Witness).ToSeq() } }
            from searched in policy.Mode.Compile(policy.EvaluationBudget).Steps.FoldM<Fin, SearchState>(initial,
                (state, operation) => Apply(operation, state, parts, inventory, variants, pairs, policy, graph)).As()
            from receipt in Deliver(searched, parts, inventory, policy, graph, runtime.Clock)
            let counted = memo.Match(
                Some: cache => receipt with { Evidence = receipt.Evidence with {
                    MemoHits = (int)cache.Census.Hits, MemoMisses = (int)cache.Census.Misses } },
                None: () => receipt)
            select counted);
    }

    static Fin<SearchState> Apply(SearchOp operation, SearchState state, Arr<Loop> parts, Seq<Stock> inventory,
        HashMap<(int PartId, long Angle), Variant> variants, HashMap<UInt128, NoFitPolygon> pairs,
        NestPolicy policy, ConstraintGraph graph) => operation.Switch(
            state: (state, parts, inventory, variants, pairs, policy, graph),
            ordered: static (scope, _) => Fin.Succ(scope.state with { Population = scope.state.Population.Map(genome =>
                genome with { Order = scope.graph.Order(genome.Order, scope.policy.Parts) }) }),
            seeded: static (scope, row) => Fin.Succ(Seed(scope.state, scope.policy.Parts, row.Population, row.Seed)),
            branched: static (scope, row) => Decode(scope.state, scope.parts, scope.inventory, scope.variants, scope.pairs,
                scope.policy, scope.graph, row.Width),
            bred: static (scope, _) => Fin.Succ(Breed(scope.state)),
            mutated: static (scope, row) => Fin.Succ(Mutate(scope.state, row.Rate, scope.policy.Parts)),
            cooled: static (scope, row) => Fin.Succ(Cool(scope.state, row.Temperature, row.Factor, scope.policy.Objective)),
            relaxed: static (scope, row) => Relax(scope.state, row.Iterations, row.Strength),
            selected: static (scope, row) => Fin.Succ(Select(scope.state, scope.policy.Objective, row.Width)),
            repeated: static (scope, row) => Enumerable.Range(0, row.Count).ToSeq().FoldM<Fin, SearchState>(scope.state,
                (cycle, _) => row.Body.FoldM<Fin, SearchState>(cycle, (inner, op) => Apply(op, inner, scope.parts,
                    scope.inventory, scope.variants, scope.pairs, scope.policy, scope.graph)).As()).As(),
            bounded: static (scope, row) => row.Body.FoldM<Fin, SearchState>(scope.state, (inner, op) =>
                inner.Evidence.Evaluated >= row.Evaluations
                    ? Fin.Succ(inner)
                    : Apply(op, inner, scope.parts, scope.inventory, scope.variants, scope.pairs, scope.policy,
                        scope.graph)).As(),
            rectangular: static (scope, row) => NestRun.FromProfiles(
                    scope.parts,
                    scope.inventory,
                    scope.policy.Parts,
                    row.Strategy,
                    new RectangularBudget(row.StrategyBudget, row.StrategyDepth, row.StockLimit, row.OrientationBudget,
                        scope.policy.PairBatchFloor),
                    new RectangularGrid(scope.policy.RectangleResolution, scope.policy.Kerf, scope.policy.EdgeAllowance))
                .Bind(StockNest.Pack).Bind(plan => FromPlan(scope.state, scope.parts, plan)));

    static Fin<SearchState> Decode(SearchState state, Arr<Loop> parts, Seq<Stock> inventory,
        HashMap<(int PartId, long Angle), Variant> variants, HashMap<UInt128, NoFitPolygon> pairs,
        NestPolicy policy, ConstraintGraph graph, int width) =>
        state.Population.FoldM<Fin, (Seq<SearchRun> Runs, int Evaluated)>(
            (Seq<SearchRun>(), state.Evidence.Evaluated),
            (population, genome) => population.Evaluated >= policy.EvaluationBudget
                ? Fin.Succ(population)
                : genome.Order.FoldM<Fin, (Seq<SearchRun> Runs, int Evaluated)>(
                    (Seq(new SearchRun(Seq<Placed>(), Seq<UnplacedReason>(), state.Evidence.Constraints, 0, 0, 0)),
                        population.Evaluated),
                    (frontier, part) => Expand(frontier, part, genome, inventory, variants, pairs, policy, graph,
                        state, width)).As()
                    .Map(decoded => (population.Runs.Concat(decoded.Runs), decoded.Evaluated))).As()
        .Map(decoded => state with {
            Runs = decoded.Runs,
            Evidence = state.Evidence with { Evaluated = decoded.Evaluated },
        });

    static Fin<(Seq<SearchRun> Runs, int Evaluated)> Expand(
        (Seq<SearchRun> Runs, int Evaluated) frontier,
        PartInstance part,
        Genome genome,
        Seq<Stock> inventory,
        HashMap<(int PartId, long Angle), Variant> variants,
        HashMap<UInt128, NoFitPolygon> pairs,
        NestPolicy policy,
        ConstraintGraph graph,
        SearchState state,
        int width) => frontier.Runs.FoldM<Fin, (Seq<SearchRun> Runs, int Evaluated)>(
            (Seq<SearchRun>(), frontier.Evaluated),
            (expanded, run) => expanded.Evaluated >= policy.EvaluationBudget
                ? Fin.Succ((expanded.Runs.Add(run with {
                    Unplaced = run.Unplaced.Add(new UnplacedReason.Budget(part.PartId, part.Ordinal, expanded.Evaluated)),
                }), expanded.Evaluated))
                : Scanning(run.Placed, scan => Candidates(part, genome, run.Placed, inventory, variants, pairs, policy,
                    state.VoronoiIterations, state.VoronoiStrength)
                    .Map(rows => toSeq(rows.OrderBy((policy.Mode is PlacementMode.BottomLeft
                        ? CandidateOrder.BottomLeft
                        : policy.Frontier).Rank)))
                    .Map(rows => (Candidates: rows.Count,
                        Rows: rows.Take(policy.EvaluationBudget - expanded.Evaluated)))
                    .Bind(result => result.Rows.TraverseM(candidate => Exact(
                        candidate, run.Placed, scan, inventory, variants, policy, graph)).As()
                        .Map(decisions => (result.Candidates, result.Rows,
                            Admitted: decisions.Choose(decision => decision.Switch(
                                accepted: static row => Some((row.Value, row.Constraints)),
                                rejected: static _ => Option<(Placed Value, Seq<ConstraintVerdict> Constraints)>.None))
                                .Take(width).ToSeq(),
                            Rejected: decisions.Choose(decision => decision.Switch(
                                accepted: static _ => Option<UnplacedReason>.None,
                                rejected: static row => Some(row.Reason))))))
                    .Map(result => (Runs: result.Admitted.IsEmpty
                            ? Seq(run with {
                                Unplaced = run.Unplaced.Add(result.Rejected.IsEmpty
                                    ? new UnplacedReason.Budget(part.PartId, part.Ordinal, expanded.Evaluated + result.Rows.Count)
                                    : result.Rejected.Head.IfNone(new UnplacedReason.Budget(
                                        part.PartId, part.Ordinal, expanded.Evaluated + result.Rows.Count))),
                                Candidates = run.Candidates + result.Candidates,
                                Evaluated = run.Evaluated + result.Rows.Count,
                                Rejected = run.Rejected + result.Rows.Count,
                            })
                            : result.Admitted.Map(accepted => run with {
                                Placed = run.Placed.Add(accepted.Value),
                                Constraints = accepted.Constraints,
                                Candidates = run.Candidates + result.Candidates,
                                Evaluated = run.Evaluated + result.Rows.Count,
                                Rejected = run.Rejected + result.Rows.Count - result.Admitted.Count,
                            }),
                        Evaluated: expanded.Evaluated + result.Rows.Count))
                    .Map(result => (expanded.Runs.Concat(result.Runs), result.Evaluated)))).As()
        .Map(branches => (toSeq(branches.Runs
            .OrderByDescending(run => policy.Objective.Score(Evidence(run, state.Evidence), state.Basis))
            .Take(width)), branches.Evaluated));

    // Placed parts ARE the ONE recurring subject of a branch's whole candidate sweep, so their vertex structure
    // precomputes once per branch and every candidate position folds over it. Branches with nothing placed carry
    // no subject to scan.
    // The scan handle never outlives one branch fold, so `PolygonScan.Scan` — the owner's own bracketed entry —
    // owns its lifetime; a hand-rolled `Of` plus `using` here was that bracket re-spelled at the caller.
    static Fin<T> Scanning<T>(Seq<Placed> placed, Func<Option<PolygonScan>, Fin<T>> body) =>
        placed.IsEmpty
            ? body(None)
            : PolygonScan.Scan(
                placed.Map(static row => row.Envelope),
                PolygonFill.NonZero,
                scan => body(Some(scan)),
                Op.Of(name: nameof(Scanning)));

    static Fin<PlacementDecision> Exact(Candidate candidate, Seq<Placed> placed, Option<PolygonScan> scan,
        Seq<Stock> inventory, HashMap<(int PartId, long Angle), Variant> variants, NestPolicy policy, ConstraintGraph graph) =>
        candidate.StockSlot >= 0 && candidate.StockSlot < inventory.Count
        && inventory[candidate.StockSlot].Identity == candidate.Stock
            ? variants.Find((candidate.Part.PartId, BitConverter.DoubleToInt64Bits(candidate.Rotation)))
                .ToFin(new KernelFault.InvalidValue("nfp", "nest:variant-key"))
                .Map(found => (Stock: inventory[candidate.StockSlot], Index: candidate.StockSlot, Variant: found))
            .Bind(scope =>
                from transform in PartTransform.Admit(scope.Variant.PartId, candidate.Part.Ordinal,
                    candidate.Point.X, candidate.Point.Y, candidate.Rotation, scope.Index, mirrored: false)
                from shape in transform.Apply(scope.Variant.True)
                from envelopeTransform in PartTransform.Admit(scope.Variant.PartId, candidate.Part.Ordinal,
                    candidate.Point.X, candidate.Point.Y, rotationRadians: 0.0, sheetIndex: scope.Index, mirrored: false)
                from envelope in envelopeTransform.Apply(scope.Variant.Collision)
                from boundaryEnvelope in ArcShapeOffset(Seq(envelope), policy.EdgeAllowance).Bind(rows => rows.Count == 1
                    ? rows.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:edge-envelope-empty"))
                    : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:edge-envelope-topology")))
                from stockRelations in scope.Stock.Region.TraverseM(region => Relate(region, boundaryEnvelope)
                    .Map(relation => (region, relation))).As()
                from contact in scan.Match(
                    Some: row => row.Intersects(Seq(envelope), Op.Of(name: nameof(Exact))),
                    None: static () => Fin.Succ(false))
                from overlaps in contact
                    ? placed.TraverseM(row => Relate(row.Envelope, envelope).Map(relation => (row, relation))).As()
                    : Fin.Succ(Seq<(Placed row, ArcRelation relation)>())
                from exclusions in scope.Stock.Exclusions.TraverseM(exclusion => Relate(exclusion, envelope)
                    .Map(relation => (exclusion, relation))).As()
                from constraints in graph.Accept(candidate, shape, envelope, placed)
                let boundary = stockRelations.Exists(static row => row.region.Winding() == Sign.Positive
                        && row.relation == ArcRelation.SecondInsideFirst)
                    && stockRelations.Filter(static row => row.region.Winding() == Sign.Negative)
                        .ForAll(static row => row.relation == ArcRelation.Disjoint)
                let rejected = Seq<Option<UnplacedReason>>(
                    MaterialAccepted(candidate, scope.Stock, policy.Parts)
                        ? None : Some<UnplacedReason>(new UnplacedReason.Material(
                            candidate.Part.PartId, candidate.Part.Ordinal, scope.Stock.Identity)),
                    GrainAccepted(candidate, scope.Stock, policy.Parts)
                        ? None : Some<UnplacedReason>(new UnplacedReason.Grain(
                            candidate.Part.PartId, candidate.Part.Ordinal, scope.Stock.Identity)),
                    boundary ? None : Some<UnplacedReason>(new UnplacedReason.Boundary(
                        candidate.Part.PartId, candidate.Part.Ordinal, scope.Stock.Identity)),
                    overlaps.Find(static row => row.relation != ArcRelation.Disjoint).Map<UnplacedReason>(row =>
                        new UnplacedReason.Collision(candidate.Part.PartId, candidate.Part.Ordinal,
                            row.row.Part.PartId, row.row.Instance.Ordinal)),
                    exclusions.Find(static row => row.relation != ArcRelation.Disjoint).Map<UnplacedReason>(_ =>
                        new UnplacedReason.Exclusion(candidate.Part.PartId, candidate.Part.Ordinal, scope.Stock.Identity)),
                    constraints.Find(static verdict => verdict.Blocking).Map<UnplacedReason>(verdict =>
                        new UnplacedReason.Constraint(candidate.Part.PartId, candidate.Part.Ordinal, verdict.Constraint)))
                    .Choose(identity).Head
                let accepted = new Placed(candidate.Part, scope.Variant, scope.Stock, transform, shape, envelope)
                from decision in rejected.Match(
                    Some: reason => Fin.Succ<PlacementDecision>(new PlacementDecision.Rejected(reason)),
                    None: () => graph.Verdicts(placed.Add(accepted)).Map<PlacementDecision>(verdicts =>
                        new PlacementDecision.Accepted(accepted, verdicts)))
                select decision)
            : Fin.Fail<PlacementDecision>(new KernelFault.InvalidValue("nfp", "nest:stock-slot"));

    static Fin<Seq<Candidate>> Candidates(PartInstance part, Genome genome, Seq<Placed> placed, Seq<Stock> inventory,
        HashMap<(int PartId, long Angle), Variant> variants, HashMap<UInt128, NoFitPolygon> pairs,
        NestPolicy policy, int voronoiIterations, double voronoiStrength) =>
        genome.Rotation.Find(part).ToFin(new KernelFault.InvalidValue("nfp", "nest:rotation-key")).Bind(angle =>
            variants.Find((part.PartId, BitConverter.DoubleToInt64Bits(angle)))
                .ToFin(new KernelFault.InvalidValue("nfp", "nest:variant-key"))
                .Bind(variant => toSeq(policy.Candidates.OrderBy(static source => source.Key))
                    .TraverseM(source => source.Generate(new CandidateRequest(part, variant, angle, placed, inventory,
                        pairs, policy, voronoiIterations, voronoiStrength))).As())
                .Map(static rows => toSeq(rows.Bind(identity)
                    .DistinctBy(static row => (row.StockSlot, row.Point.X, row.Point.Y, row.Rotation)))));

    // Free-space seeds are the relaxed cell centroids of the stock outline and everything already placed on it —
    // one `PolygonOp.Cells` request per sheet, the outline its own clip ring, so this lane mints no diagram.
    internal static Fin<Seq<Candidate>> VoronoiCandidates(PartInstance part, Variant variant, double angle,
        Seq<Placed> placed, Seq<Stock> inventory, int iterations, double strength) =>
        inventory.Map(static (stock, slot) => (stock, slot)).TraverseM(row => {
            Seq<Point3d> points = toSeq(row.stock.Region.Bind(static loop => toSeq(loop.Vertices))
                .Concat(placed.Filter(placedRow => placedRow.Transform.SheetIndex == row.slot)
                    .Bind(static placedRow => toSeq(placedRow.Shape.Vertices)))
                .DistinctBy(static point => (point.X, point.Y)));
            Vector3d anchor = variant.Rotated.Bound().Center - Point3d.Origin;
            return points.Count < 3 || row.stock.Region.IsEmpty
                ? Fin.Succ(Seq<Candidate>())
                : PolygonAlgebra.Apply(
                        new PolygonOp.Cells(
                            points.ToArr(),
                            row.stock.Region[0],
                            SitePolicy.Create(relaxations: iterations, relaxationStrength: strength, merge: None)),
                        Op.Of(name: nameof(VoronoiCandidates)))
                    .Bind(trace => trace
                        .Diagram(new KernelFault.InvalidValue("nfp", "nest:cell-trace"))
                        .Map(diagram => diagram.Cells.ToSeq().Map(cell => new Candidate(
                            part, row.slot, row.stock.Identity, cell.Centroid - anchor, angle, 0.0))));
        }).As().Map(static rows => rows.Bind(identity));

    static Fin<SearchState> Relax(SearchState state, int iterations, double strength) =>
        iterations < 0 || strength is < 0.0 or > 1.0
            ? Fin.Fail<SearchState>(new KernelFault.InvalidValue("nfp", "nest:relax-policy"))
            : Fin.Succ(state with { VoronoiIterations = iterations, VoronoiStrength = strength });

    static SearchState Seed(SearchState state, Seq<PartRule> rules, int population, int seed) {
        Seq<PartInstance> canonical = toSeq(rules.OrderByDescending(static row => row.Priority)
            .SelectMany(row => Enumerable.Range(0, row.Quantity).Select(ordinal => new PartInstance(row.PartId, ordinal))));
        // Every genome draw is a LANE, not a packed key: (seed+index, partId, ordinal) enters the kernel stream
        // whole, so neighbouring part pairs cannot collide the way a shifted-XOR pack of the same fields does.
        Seq<Genome> genomes = toSeq(Enumerable.Range(0, population)).Map(index => new Genome(
            index == 0 ? canonical : toSeq(canonical.OrderBy(part => Deterministic.Stream(
                lanes: [part.PartId, part.Ordinal], seed: seed + index))),
            toHashMap(canonical.Map(part => {
                ulong stream = Deterministic.Stream(lanes: [part.PartId, part.Ordinal], seed: seed + index);
                return (part, rules.Find(row => row.PartId == part.PartId)
                    .Map(row => row.Angles[Deterministic.NextBelow(ref stream, row.Angles.Count)]).IfNone(0.0));
            }))));
        return state with { Population = genomes, Random = Deterministic.Stream(lanes: [seed]) };
    }

    static SearchState Breed(SearchState state) => state with {
        Population = state.Population.Zip(state.Population.Rev(), static (left, right) => left with {
            Order = left.Order.Map((part, index) => (index & 1) == 0 ? part : right.Order[index]).Distinct().Concat(left.Order).Distinct(),
        }),
    };

    static SearchState Mutate(SearchState state, double rate, Seq<PartRule> rules) => state with {
        // Mutation gates read a unit draw directly rather than dividing a raw word by ulong.MaxValue, which
        // loses the low bits; every index pick threads ONE state through NextBelow, so a non-power-of-two angle or
        // order count carries no modulo bias.
        Population = state.Population.Map((genome, index) => {
            ulong stream = Deterministic.Stream(lanes: [index], seed: (long)state.Random);
            if (Deterministic.Unit(lanes: [index, 0L], seed: (long)state.Random) >= rate || genome.Order.Count < 2) return genome;
            int left = Deterministic.NextBelow(ref stream, genome.Order.Count);
            int right = Deterministic.NextBelow(ref stream, genome.Order.Count);
            Seq<PartInstance> order = genome.Order.Map((part, at) => at == left ? genome.Order[right] : at == right ? genome.Order[left] : part);
            double rotation = rules.Find(row => row.PartId == order[left].PartId)
                .Map(rule => rule.Angles[Deterministic.NextBelow(ref stream, rule.Angles.Count)]).IfNone(0.0);
            return genome with { Order = order, Rotation = genome.Rotation.SetItem(order[left], rotation) };
        }), Random = Deterministic.Stream(lanes: [state.Population.Count], seed: (long)state.Random),
    };

    static SearchState Cool(SearchState state, double temperature, double factor, NestObjective objective) {
        if (state.Runs.IsEmpty) return state;
        double next = state.Temperature > 0.0 ? state.Temperature * factor : temperature;
        Seq<(SearchRun Run, double Score)> ranked = toSeq(state.Runs
            .Map(run => (run, objective.Score(Evidence(run, state.Evidence), state.Basis)))
            .OrderByDescending(static row => row.Item2));
        return ranked.Head.Match(
            Some: best => {
                (Seq<SearchRun> Rows, ulong Random) accepted = ranked.Map((row, index) => (row, index))
                    .Fold((Rows: Seq<SearchRun>(), Random: state.Random), (choice, row) => {
                        double probability = Math.Exp(Math.Clamp(
                            (row.row.Score - best.Score) / Math.Max(next, double.Epsilon), -700.0, 0.0));
                        return (row.index == 0
                            || Deterministic.Unit(lanes: [row.index + 1], seed: (long)choice.Random) <= probability
                            ? choice.Rows.Add(row.row.Run)
                            : choice.Rows, Deterministic.Stream(lanes: [row.index + 1], seed: (long)choice.Random));
                    });
                return state with { Runs = accepted.Rows, Random = accepted.Random, Temperature = next };
            },
            None: () => state);
    }

    static SearchState Select(SearchState state, NestObjective objective, int width) => state with {
        Runs = toSeq(state.Runs.OrderByDescending(run => objective.Score(Evidence(run, state.Evidence), state.Basis)).Take(width)),
    };

    static Fin<SearchState> Initial(Seq<Stock> inventory, NestPolicy policy, ConstraintGraph graph) {
        Seq<PartInstance> requested = policy.Parts.Bind(row => toSeq(Enumerable.Range(0, row.Quantity))
            .Map(ordinal => new PartInstance(row.PartId, ordinal)));
        Seq<PartInstance> order = graph.Order(requested, policy.Parts);
        Genome genome = new(order, toHashMap(order.Map(part => (part, policy.Parts.Find(row => row.PartId == part.PartId)
            .Bind(static row => row.Angles.Head).IfNone(0.0)))));
        NestEvidence evidence = new(policy.Mode, policy.Objective, inventory.Map(static stock => stock.Identity), Seq<NfpWitness>(),
            policy.Constraints.Map(static rule => (ConstraintVerdict)new ConstraintVerdict.Satisfied(rule)), Seq<UnplacedReason>(), 0, 0, 0,
            0.0, 0.0, 0.0, 0.0, 0.0);
        // Seeds are the search's own zero: no draw consumed, no annealing heat, one relaxation pass, and full
        // relaxation strength — the program's own `Cool` and `Relax` steps move every one of them from here.
        return Fin.Succ(new SearchState(Seq(genome), Seq<SearchRun>(), evidence, NestBasis.Of(inventory, policy),
            Random: DrawSeed, Temperature: HeatSeed, VoronoiIterations: RelaxationSeed, VoronoiStrength: StrengthSeed));
    }

    static Fin<SearchState> FromPlan(SearchState state, Arr<Loop> parts, NestPlan plan) =>
        plan.Placements.TraverseM(row => Rotated(parts[row.PartId], row.RotationRadians).Bind(shape =>
            PartTransform.Admit(row.PartId, row.Instance, row.XMm - shape.Bound().Min.X, row.YMm - shape.Bound().Min.Y,
                row.RotationRadians, row.SheetIndex, mirrored: false).Map(transform => FromPlanPlacement(row, shape, transform, plan.Stock[row.SheetIndex])))).As()
            // Rectangular packing GRADES its own refusals, so an honoured plan's roster arrives already naming
            // material, extent, or grain and only the residual reads `Capacity`. Re-deriving a blanket capacity
            // reason here threw that grading away at the one seam that carries it.
            .Map(rows => state with { Runs = Seq(new SearchRun(rows, plan.Unplaced,
                state.Evidence.Constraints, plan.Yield.RequestedCount, plan.Yield.RequestedCount, plan.Yield.UnplacedCount)), Evidence = state.Evidence with {
                Evaluated = plan.Yield.RequestedCount,
                UsedArea = rows.Sum(static row => Math.Abs(row.Shape.Area())),
                StockArea = plan.Yield.StockAreaMm2,
            }});

    static Placed FromPlanPlacement(NestPlacement row, Loop shape, PartTransform transform, Stock stock) {
        Variant variant = new(row.PartId, row.RotationRadians, shape, shape, shape,
            Identity(Seq(shape), shape.Tolerance, salt => salt.String("plan").Ordinal(row.PartId).Ordinal(row.Instance)));
        return new Placed(new PartInstance(row.PartId, row.Instance), variant, stock, transform, shape, shape);
    }

    // `Receipt<NestEvidence>` is the settled lane output and the ONE settled-receipt carrier, so plane, ancestry,
    // band, and stamp arrive from the spine and this fold declares its lane evidence alone. `Stamped` is the ADDED
    // column: a nest receipt never carried a settling instant, and it stays outside the digest exactly as the run
    // spine's own receipt does, because a key moving with the wall clock addresses nothing. `Produced` names this
    // run's minted placement so the provenance walk reaches it without re-deriving that roster; `Consumed` stays
    // EMPTY and says so — stock enters as a `UInt128` identity carrying no egress family, so a content key
    // manufactured for it names an artifact no producer ever addressed.
    static Fin<Receipt<NestEvidence>> Deliver(SearchState state, Arr<Loop> parts, Seq<Stock> inventory,
        NestPolicy policy, ConstraintGraph graph, IClock clock) =>
        state.Runs.Fold(Option<SearchRun>.None, (best, run) => best
                .Filter(held => policy.Objective.Score(Evidence(held, state.Evidence), state.Basis)
                    >= policy.Objective.Score(Evidence(run, state.Evidence), state.Basis)).IfNone(run))
            .ToFin(new FabricationFault.StockOverflow(parts.Count, inventory.Count))
            .Bind(best => best.Placed.IsEmpty
                ? Fin.Fail<Receipt<NestEvidence>>(new FabricationFault.StockOverflow(parts.Count, inventory.Count))
                : best.Placed.TraverseM(row => row.Transform.Apply(parts[row.Part.PartId])).As().Bind(_ => graph.Verdicts(best.Placed).Bind(verdicts =>
                verdicts.Exists(static verdict => verdict.Blocking)
                    ? Fin.Fail<Receipt<NestEvidence>>(new KernelFault.InvalidValue("nfp", "nest:constraint-verdict"))
                    : toSeq(best.Placed.GroupBy(static row => row.Transform.SheetIndex)).Map(static group => toSeq(group))
                        .TraverseM(rows => rows.Head.ToFin(new KernelFault.InvalidValue("nfp", "nest:stock-group"))
                            .Bind(head => Remnants.From(head.Stock, rows.Map(static row => row.Shape),
                                policy.Clearance + policy.Kerf))).As()
                    .Map(remnants => remnants.Bind(identity))
                    // The codec opens on the placed geometry's own admitted grid, so every preimage this run mints
                    // — geometry identity, evidence digest, placement key — shares one tolerance and one codec.
                    .Bind(remnants => best.Placed.Head
                        .ToFin(new KernelFault.InvalidValue("nfp", "nest:placed-head"))
                        .Bind(head => {
                            Seq<PartTransform> transforms = best.Placed.Map(static row => row.Transform);
                            NestEvidence evidence = Evidence(best, state.Evidence) with {
                                Constraints = verdicts,
                                RemnantValue = remnants.Sum(static row => Math.Abs(row.Region.Sum(static loop => loop.Area()))),
                                Placements = transforms,
                                Remnants = remnants,
                            };
                            return KeyOf(transforms, remnants, Digest(evidence, head.Shape.Tolerance), head.Shape.Tolerance)
                                .Map(key => new Receipt<NestEvidence> {
                                    Evidence = evidence,
                                    Concern = FabConcern.Nesting,
                                    Key = key,
                                    Produced = Seq(key),
                                    Stamped = clock.GetCurrentInstant(),
                                });
                        })))));

    static FabricationResult Project(Receipt<NestEvidence> receipt) => new FabricationResult.Placement(
        receipt.Evidence.Placements, receipt.Evidence.Utilization, receipt.Evidence.Unplaced.Count,
        receipt.Evidence.Remnants, receipt.Key);

    // The shared-edge measure and the pierce census come from the ONE `Nesting/linking` owner over the placed
    // shapes: a collinear overlap between two placed profiles is one cut the program runs once, so the objective
    // reads a measure the link plane produced rather than a second collinearity walk minted here.
    static NestEvidence Evidence(SearchRun run, NestEvidence basis) {
        CommonLineCensus shared = CommonLine.Measure(run.Placed.Map(static row => row.Shape));
        return basis with {
            Constraints = run.Constraints,
            Unplaced = run.Unplaced,
            Candidates = run.Candidates,
            Evaluated = Math.Max(basis.Evaluated, run.Evaluated),
            Rejected = run.Rejected,
            UsedArea = run.Placed.Sum(static row => Math.Abs(row.Shape.Area())),
            StockArea = Consumed(run).Sum(static stock => stock.Area),
            CutLength = run.Placed.Sum(static row => row.Shape.Length()),
            StockCost = Consumed(run).Sum(static stock => stock.Cost),
            SharedEdge = shared.OverlapMm,
            Pierces = shared.Pierces,
        };
    }

    static Seq<Stock> Consumed(SearchRun run) => toSeq(run.Placed.GroupBy(static row => row.Transform.SheetIndex))
        .Choose(static group => toSeq(group).Head.Map(static row => row.Stock));

    // Field ORDER is the page's and stays; the codec is the Element writer reached through `FabricationCanon`, so
    // every double here carries the same `-0.0`/NaN canon the geometry identity already writes and the two
    // preimages can never fork on framing. `Rows` writes the count AHEAD of its rows, so the four hand-spelled
    // `Ordinal(count)` prologues that stood beside four `foreach` bodies are the codec's own framing now — a
    // prologue and its loop drifting apart is the exact defect that framing exists to make unspellable. Unit
    // identity is FROZEN by the same law: every scalar the writer digests is a bare millimetre-basis double, the
    // basis is stated once at the evidence declaration, and lifting one of them to a typed quantity would move
    // the preimage bytes and fork every content key already minted against it.
    static UInt128 Digest(NestEvidence evidence, Context tolerance) =>
        FabricationCanon.Ordered(tolerance, writer => ModeWrite(writer, evidence.Mode)
            .Discriminant(evidence.Objective)
            .Ordinal(evidence.Candidates).Ordinal(evidence.Evaluated).Ordinal(evidence.Rejected)
            .Double(evidence.UsedArea).Double(evidence.StockArea).Double(evidence.CutLength)
            .Double(evidence.RemnantValue).Double(evidence.StockCost)
            .Rows(toSeq(evidence.Stock.Order()), static (held, stock) => held.U128(stock))
            .Rows(toSeq(evidence.Pairs.OrderBy(static row => row.Pair)), static (held, pair) => held
                .U128(pair.Pair).U128(pair.Fixed).U128(pair.Orbiting)
                .Discriminant(pair.Relation).Discriminant(pair.Method).Double(pair.ChordError)
                .Double(pair.Clearance).Double(pair.Kerf).Ordinal(pair.Components).Ordinal(pair.Holes))
            .Rows(
                toSeq(evidence.Constraints.OrderBy(row => OrderKey(w => ConstraintWrite(w, row.Constraint), tolerance))),
                static (held, verdict) => ConstraintWrite(held, verdict.Constraint)
                    .Bool(verdict is ConstraintVerdict.Satisfied).Double(verdict.Penalty))
            .Rows(
                toSeq(evidence.Unplaced.OrderBy(row => OrderKey(w => ReasonWrite(w, row), tolerance))),
                static (held, reason) => ReasonWrite(held, reason)));

    // Ordering and preimage are ONE contribution per row family: each writer below is the codec-native rendering
    // of the case — a bounded discriminant literal then typed payload primitives — so no text render, `:R` double,
    // or hex casing can reach a content-key path, and the sort key is the streaming digest of the same
    // contribution the preimage receives, total and deterministic by construction.
    static UInt128 OrderKey(Func<CanonicalWriter, CanonicalWriter> contribute, Context tolerance) =>
        FabricationCanon.Ordered(tolerance, contribute);

    static CanonicalWriter ModeWrite(CanonicalWriter writer, PlacementMode mode) => mode.Switch(
        state: writer,
        bottomLeft: static (w, _) => w.String("bottom-left"),
        beam: static (w, row) => w.String("beam").Ordinal(row.Width),
        genetic: static (w, row) => w.String("genetic").Ordinal(row.Population).Ordinal(row.Generations)
            .Double(row.Mutation).Ordinal(row.Seed),
        annealed: static (w, row) => w.String("annealed").Ordinal(row.Iterations).Ordinal(row.Width)
            .Double(row.Temperature).Double(row.Cooling).Ordinal(row.Seed),
        freeSpace: static (w, row) => w.String("free-space").Ordinal(row.Relaxations).Double(row.Strength).Ordinal(row.Width),
        rectFastpath: static (w, row) => w.String("rect").U128(row.Strategy.Identity).Ordinal(row.StrategyBudget)
            .Ordinal(row.StrategyDepth).Ordinal(row.OrientationBudget).Ordinal(row.StockLimit));

    static CanonicalWriter ConstraintWrite(CanonicalWriter writer, PlacementConstraint rule) => rule.Switch(
        state: writer.Discriminant(rule.Force),
        precedes: static (w, row) => w.String("precedes").Ordinal(row.Before).Ordinal(row.After),
        together: static (w, row) => w.String("together")
            .Rows(toSeq(row.Parts.Order()), static (held, part) => held.Ordinal(part)),
        separate: static (w, row) => w.String("separate").Ordinal(row.Left).Ordinal(row.Right)
            .Double(row.Distance).Discriminant(row.Metric),
        adjacent: static (w, row) => w.String("adjacent").Ordinal(row.Left).Ordinal(row.Right)
            .Double(row.MaximumDistance).Discriminant(row.Metric),
        inside: static (w, row) => w.String("inside").Ordinal(row.Inner).Ordinal(row.Outer),
        stockOnly: static (w, row) => w.String("stock").Ordinal(row.Part)
            .Rows(toSeq(row.Stock.Order()), static (held, stock) => held.U128(stock)),
        keepOut: static (w, row) => w.String("keep-out").U128(row.Stock)
            .U128(row.Region.Head.Map(loop => Identity(row.Region, loop.Tolerance, static salt => salt.String("keep-out")))
                .IfNone(UInt128.Zero)));

    static CanonicalWriter ReasonWrite(CanonicalWriter writer, UnplacedReason reason) => reason.Switch(
        state: writer,
        material: static (w, row) => w.String("material").Ordinal(row.PartId).Ordinal(row.Instance).U128(row.Stock),
        grain: static (w, row) => w.String("grain").Ordinal(row.PartId).Ordinal(row.Instance).U128(row.Stock),
        boundary: static (w, row) => w.String("boundary").Ordinal(row.PartId).Ordinal(row.Instance).U128(row.Stock),
        collision: static (w, row) => w.String("collision").Ordinal(row.PartId).Ordinal(row.Instance)
            .Ordinal(row.OtherPartId).Ordinal(row.OtherInstance),
        exclusion: static (w, row) => w.String("exclusion").Ordinal(row.PartId).Ordinal(row.Instance).U128(row.Stock),
        constraint: static (w, row) => ConstraintWrite(w.String("constraint").Ordinal(row.PartId).Ordinal(row.Instance), row.Rule),
        budget: static (w, row) => w.String("budget").Ordinal(row.PartId).Ordinal(row.Instance).Ordinal(row.Evaluated),
        capacity: static (w, row) => w.String("capacity").Ordinal(row.PartId).Ordinal(row.Instance));

    // Placement ADDRESS, so this one takes the RETAINING close: `Keyed` opens the buffer, frames the egress
    // family ahead of the payload, and answers on the `Fin` rail the delivery fold already rides. A key minted off
    // bytes no writer retained is the forged form the facade's private constructor exists to forbid.
    static Fin<ContentKey> KeyOf(Seq<PartTransform> placed, Seq<Remnant> remnants, UInt128 evidence, Context tolerance) =>
        FabricationCanon.Keyed(EgressKind.Placement, tolerance, writer => writer
            .U128(evidence)
            .Rows(
                toSeq(placed.OrderBy(static row => row.SheetIndex).ThenBy(static row => row.PartId)
                    .ThenBy(static row => row.Instance).ThenBy(static row => row.Tx).ThenBy(static row => row.Ty)
                    .ThenBy(static row => row.RotationRadians)),
                static (held, row) => held.Ordinal(row.PartId).Ordinal(row.Instance).Ordinal(row.SheetIndex)
                    .Double(row.Tx).Double(row.Ty).Double(row.RotationRadians))
            .Rows(toSeq(remnants.OrderBy(static row => row.Identity)), static (held, row) => held.U128(row.Identity)),
            PlacementOp);

    private static readonly Op PlacementOp = Op.Of(name: nameof(KeyOf));

    static Fin<Loop> Admit(Loop loop, int index) => !loop.Closed
        ? Fin.Fail<Loop>(new FabricationFault.OpenLoop(FabConcern.Nesting, index))
        : loop.Count < 3 || loop.Vertices.Exists(static point => !double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z))
            || loop.Bulges.Exists(static bulge => !double.IsFinite(bulge))
                ? Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Polyline, index, $"nest:profile:{index}"))
                : Fin.Succ(loop.AsCcw());

    static Fin<HashMap<(int PartId, long Angle), Variant>> Variants(Arr<Loop> parts, NestPolicy policy) =>
        policy.Parts.Bind(rule => rule.Angles.Map(angle => (rule.PartId, Angle: angle)))
            .TraverseM(row => Rotated(parts[row.PartId], row.Angle)
                .Bind(shape => ArcShapeOffset(Seq(shape), 0.5 * (policy.Clearance + policy.Kerf)))
                .Bind(collision => collision.Count == 1 ? collision.Head.Match(
                    Some: envelope => Fin.Succ(new Variant(row.PartId, row.Angle, parts[row.PartId], shape, envelope,
                        Identity(collision, parts[row.PartId].Tolerance,
                            salt => salt.Ordinal(row.PartId).Double(row.Angle)))),
                    None: () => Fin.Fail<Variant>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:clearance-topology")))
                    : Fin.Fail<Variant>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:clearance-topology")))).As()
            .Map(rows => toHashMap(rows.Map(static row =>
                ((row.PartId, BitConverter.DoubleToInt64Bits(row.Rotation)), row))));

    static Fin<Loop> Rotated(Loop part, double radians) {
        double cosine = Math.Cos(radians), sine = Math.Sin(radians);
        return Loop.Admit(part.Vertices.Map(point => new Point3d(
            (point.X * cosine) - (point.Y * sine), (point.X * sine) + (point.Y * cosine), point.Z)).ToArr(),
            part.Closed, part.Bulges, part.Tolerance);
    }

    internal static Fin<Seq<Loop>> ArcShapeOffset(Seq<Loop> loops, double distance) =>
        loops.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:arc-offset-empty"))
            .Bind(head => ArcForest.Admit(loops, head.Tolerance, head.Plane))
                .Bind(forest => ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Forest(forest), distance)))
            .Bind(static trace => trace is ArcTrace.Forest forest
                ? Fin.Succ(forest.Result.Loops)
                : Fin.Fail<Seq<Loop>>(new KernelFault.InvalidValue("nfp", "nest:arc-offset-trace")));

    internal static Fin<ArcRelation> Relate(Loop first, Loop second) =>
        ArcForest.Admit(Seq(first, second), first.Tolerance, first.Plane)
            .Bind(forest => ArcAlgebra.Apply(new ArcOp.Inspect(forest, new ArcProbe.Pair(first, second))))
            .Bind(static trace => trace is ArcTrace.Inspection { Result: ArcInspection.Pair pair }
                ? Fin.Succ(pair.Relation)
                : Fin.Fail<ArcRelation>(new KernelFault.InvalidValue("nfp", "nest:arc-relation-trace")));

    internal static Fin<Loop> Lower(Loop loop, double error) => ArcAlgebra.Densify(new ArcProjection.Lower(loop, error))
        .Bind(static trace => trace
            .Lowering(new KernelFault.InvalidValue("nfp", "nest:arc-projection-trace"))
            .Map(static evidence => evidence.Result));

    static bool MaterialAccepted(Candidate candidate, Stock stock, Seq<PartRule> rules) => rules
        .Find(rule => rule.PartId == candidate.Part.PartId).ForAll(rule => rule.Material.ForAll(material => material == stock.Material));

    static bool GrainAccepted(Candidate candidate, Stock stock, Seq<PartRule> rules) => rules
        .Find(rule => rule.PartId == candidate.Part.PartId).ForAll(rule => rule.GrainAxis.ForAll(grain => stock.GrainAxis
            .Exists(axis => Math.Abs(Math.IEEERemainder((grain + candidate.Rotation) - axis, Math.PI))
                <= stock.Tolerance.Angle.Value)));

    internal static Seq<(Point3d Point, double Length)> Contacts(Placed placed, Variant orbiting) =>
        toSeq(Enumerable.Range(0, placed.Envelope.Count)).Bind(left => {
            Point3d p0 = placed.Envelope.At(left), p1 = placed.Envelope.At(left + 1);
            Vector3d p = p1 - p0;
            if (p.Length <= placed.Envelope.Tolerance.Absolute.Value) return Seq<(Point3d, double)>();
            Vector3d normal = new(p.Y / p.Length, -p.X / p.Length, 0.0);
            double pBulge = placed.Envelope.BulgeAt(left);
            PlineVertex<double> pStart = new(p0.X, p0.Y, pBulge), pEnd = new(p1.X, p1.Y, 0.0);
            var pMiddle = PlineSeg.SegMidpoint(pStart, pEnd);
            double pArc = pBulge == 0.0 ? p.Length
                : Math.Abs(PlineSeg.SegArcRadiusAndCenter(pStart, pEnd).Radius * (4.0 * Math.Atan(pBulge)));
            Point3d pMid = new(pMiddle.X, pMiddle.Y, p0.Z);
            return toSeq(Enumerable.Range(0, orbiting.Collision.Count)).Choose(right => {
                Point3d q0 = orbiting.Collision.At(right), q1 = orbiting.Collision.At(right + 1);
                Vector3d q = q1 - q0;
                if (q.Length <= orbiting.Collision.Tolerance.Absolute.Value
                    || (p * q) / (p.Length * q.Length) > -1.0 + placed.Envelope.Tolerance.Angle.Value) return None;
                double qBulge = orbiting.Collision.BulgeAt(right);
                PlineVertex<double> qStart = new(q0.X, q0.Y, qBulge), qEnd = new(q1.X, q1.Y, 0.0);
                var qMiddle = PlineSeg.SegMidpoint(qStart, qEnd);
                double qArc = qBulge == 0.0 ? q.Length
                    : Math.Abs(PlineSeg.SegArcRadiusAndCenter(qStart, qEnd).Radius * (4.0 * Math.Atan(qBulge)));
                Point3d qMid = new(qMiddle.X, qMiddle.Y, q0.Z);
                double release = placed.Envelope.Tolerance.Absolute.Value;
                return Some((pMid + (release * normal) - (qMid - Point3d.Origin), Math.Min(pArc, qArc)));
            });
        });

    // Kernel `UvIsland.Boundary` owns the island walk — once-counted edges, face-inherited winding, outer CCW and
    // holes CW — so this owner only terminates its Chain carrier into the Loop atom at the admitted context.
    internal static Fin<Seq<Loop>> Rings(Seq<Chain> chains, Context tolerance) =>
        chains.TraverseM(chain => Loop.Admit(
            toSeq(chain.Points).ToArr(), chain.Closed, Arr<double>(), tolerance)).As();

}

// The nfp-placement measuring case for the FabricationBenchClaims.NfpPlacement no-regression claim: the
// workload aggregate arrives admitted through its own factories, and this gate proves only what makes a
// measured run non-degenerate — the true-shape search lane rather than a resolved plan, live nestable
// inventory, and literal part and budget floors — so a trivial workload cannot stamp the claim. The
// measured fold is the exact entry the claim's lane columns spell; measurement columns and the receipt
// projection stay the bench edge's under the AppHost claim-field map.
public static class NestBench {
    public const int PartFloor = 12;
    public const int BudgetFloor = 1024;

    public static Fin<(FabricationPolicy.Nest Policy, FabricationInput Input)> Workload(
        FabricationPolicy.Nest policy, FabricationInput input) =>
        policy.Plan.IsNone
        && policy.Inventory.Exists(static stock => stock.Nestable && !stock.Region.IsEmpty)
        && input.Profiles.Count >= PartFloor
        && policy.Nesting.EvaluationBudget >= BudgetFloor
            ? Fin.Succ((policy, input))
            : Fin.Fail<(FabricationPolicy.Nest, FabricationInput)>(
                new KernelFault.InvalidValue("nfp", "bench:nfp-placement"));

    // Measurement takes the same runtime the run spine hands the plane, so the bench times the entry a real run
    // reaches; a bench-local default measures a lane with no memo, no tap, and no token.
    public static ValueTask<Fin<FabricationResult>> Run(
        (FabricationPolicy.Nest Policy, FabricationInput Input) workload, FabricationRuntime runtime) =>
        Nest.Solve(workload.Policy, workload.Input, runtime);
}

// --- [CONFIGURATION_SPACE] -------------------------------------------------------------------------------------------------------------------------
// Pair memo: HybridCache owns stampede protection and the L1/L2 split — each runtime instance is L1-only in
// process, and a durable L2 federates at the Persistence cache seam. Keys are exact `PairTable.Key` identities,
// folded from canonical pair geometry, tolerance, rotation, clearance, kerf, and chord error through the S0
// `FabricationCanon.Ordered` close and rendered by `ContentHash.Hex`, so byte-identical pairs under one policy
// replay across solves and runs and no site spells its own hex format; a failed build throws through the factory
// and is never cached. Memoized rows stay asynchronous from cache lookup through solve completion.
internal sealed class PairMemo(HybridCache cache) {
    static readonly HybridCacheEntryOptions Tuned = new() {
        Expiration = TimeSpan.FromHours(8),
        LocalCacheExpiration = TimeSpan.FromHours(8),
    };

    static readonly Op MemoOp = Op.Of(name: "nest:pair-memo");

    // Two MONOTONE tallies read once, after the fan the parallel lookup awaits has completed — so the pair never
    // needs to move atomically and `Interlocked` is the exact operator for each. The `Atom<(long, long)>` this
    // replaces spelled `ignore(cell.Swap(...))`, the deleted transition spelling: it discarded the verdict the
    // whole lock-free mechanism exists to answer while paying for a boxed pair on every cache probe.
    // NAMED LOSS: a reader sampling MID-FAN could now see a hit counted before its matching miss. WITNESS: the one
    // reader is `Nest.Search`, which reads `Census` strictly after awaiting `PairTable.Build`.
    long hits;
    long misses;

    public (long Hits, long Misses) Census => (Interlocked.Read(ref hits), Interlocked.Read(ref misses));

    // `Op.Catch` is the branch's one inbound exception funnel. The caller token is the exact execution token passed
    // to FusionCache, so only token-proved cancellation lowers to the kernel cancellation rail and every foreign
    // failure remains exact; a failed build still throws through the awaited factory, so a fault is never cached.
    public async ValueTask<Fin<NoFitPolygon>> GetOrBuild(UInt128 identity, Func<Fin<NoFitPolygon>> build, CancellationToken cancel) =>
        await MemoOp.Catch(async execution => {
            bool built = false;
            NoFitPolygon polygon = await cache.GetOrCreateAsync(
                $"nfp:{ContentHash.Hex(identity)}",
                (Build: build, Mark: () => built = true),
                static (state, _) => { state.Mark(); return ValueTask.FromResult(state.Build().ThrowIfFail()); },
                Tuned,
                cancellationToken: execution).ConfigureAwait(false);
            _ = Interlocked.Increment(ref built ? ref misses : ref hits);
            return Fin.Succ(polygon);
        }, token: cancel).ConfigureAwait(false);
}

internal static class PairTable {
    public static async ValueTask<Fin<HashMap<UInt128, NoFitPolygon>>> Build(
        HashMap<(int PartId, long Angle), Variant> variants,
        Seq<Stock> inventory,
        NestPolicy policy,
        Option<PairMemo> memo = default,
        CancellationToken cancel = default) {
        Variant[] rows = variants.Values.OrderBy(static row => row.PartId).ThenBy(static row => row.Rotation).ToArray();
        return (await Op.Of(name: "nest:pair-table").Catch(
                async execution => Fin.Succ(await memo.Match(
                    Some: cache => Cached(rows, policy, cache, execution),
                    None: () => ValueTask.FromResult(Parallel(rows, policy))).ConfigureAwait(false)),
                token: cancel).ConfigureAwait(false))
            .Bind(results => results.ToSeq().TraverseM(identity).As()
                .Bind(pairs => Inner(toSeq(rows), inventory, policy).Map(inner => pairs.Concat(inner)))
                .Map(static found => toHashMap(found.DistinctBy(static row => row.Identity)
                    .Select(static row => (row.Identity, row)))));
    }

    static Fin<NoFitPolygon>[] Parallel(Variant[] variants, NestPolicy policy) {
        Fin<NoFitPolygon>[] results = new Fin<NoFitPolygon>[checked(variants.Length * variants.Length)];
        PairAction action = new(variants, results, policy);
        ParallelHelper.For2D(0..variants.Length, 0..variants.Length, in action, policy.PairBatchFloor);
        return results;
    }

    // The memoized fan is BOUNDED by policy: a variant roster of n starts n^2 cache lookups, and materializing
    // every one as a live task before awaiting any of them saturates the pool and the L2 connection at the exact
    // moment a large job most needs both. `PairConcurrency` is the admitted ceiling and the cancellation token
    // rides each awaited lookup, so an abandoned solve stops the fan rather than draining it.
    static async ValueTask<Fin<NoFitPolygon>[]> Cached(Variant[] variants, NestPolicy policy, PairMemo memo, CancellationToken cancel) {
        Fin<NoFitPolygon>[] results = new Fin<NoFitPolygon>[checked(variants.Length * variants.Length)];
        await System.Threading.Tasks.Parallel.ForEachAsync(
            Enumerable.Range(0, results.Length),
            new ParallelOptions { CancellationToken = cancel, MaxDegreeOfParallelism = policy.PairConcurrency },
            async (slot, token) => {
                Variant fixedPart = variants[slot / variants.Length], orbiting = variants[slot % variants.Length];
                UInt128 identity = Key(fixedPart, orbiting, policy);
                results[slot] = await memo
                    .GetOrBuild(identity, () => Built(fixedPart, orbiting, identity, policy), token)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);
        return results;
    }

    public static UInt128 Key(Variant fixedPart, Variant orbiting, NestPolicy policy) =>
        Nest.Identity(Seq(fixedPart.Collision, orbiting.Collision), fixedPart.Collision.Tolerance,
            salt => salt.String(NfpRelation.Forbidden.Key).U128(fixedPart.Identity).U128(orbiting.Identity)
                .Double(policy.ChordError).Double(policy.Clearance).Double(policy.Kerf));

    public static UInt128 InnerKey(Variant orbiting, Stock stock, NestPolicy policy) =>
        Nest.Identity(Seq(orbiting.Collision), orbiting.Collision.Tolerance,
            salt => salt.String(NfpRelation.Admitted.Key).U128(stock.Identity).U128(orbiting.Identity)
                .Double(policy.ChordError).Double(policy.EdgeAllowance));

    // Inner-fit erodes the stock outer boundary only; interior holes and exclusions stay on the exact arc gate in Nest.Exact.
    static Fin<Seq<NoFitPolygon>> Inner(Seq<Variant> variants, Seq<Stock> inventory, NestPolicy policy) =>
        variants.Bind(variant => inventory.Map(stock => (variant, stock)))
            .TraverseM(row => row.stock.Region.Filter(static loop => loop.Winding() == Sign.Positive).Head
                .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:inner-fit-boundary"))
                .Bind(outer =>
                    from bounded in Nest.Lower(outer, policy.ChordError)
                    from inset in policy.EdgeAllowance > 0.0
                        ? Nest.ArcShapeOffset(Seq(bounded), -policy.EdgeAllowance).Bind(static rows => rows.Head
                            .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:inner-fit-allowance")))
                        : Fin.Succ(bounded)
                    from orbitDense in Nest.Lower(row.variant.Collision, policy.ChordError)
                    from trace in PolygonAlgebra.Apply(new PolygonOp.Morphology(inset, orbitDense,
                        NfpRelation.Admitted.Kind))
                    from locus in trace.Regioned(
                            new KernelFault.InvalidValue("nfp", "nest:morphology-trace"))
                        .Map(static topology => topology.Nodes.Map(static node => node.Boundary))
                    let identity = InnerKey(row.variant, row.stock, policy)
                    // The eroded stock boundary is the operand the walk actually consumes, so the edge-allowance
                    // offset's own arcs decide fidelity here, never the pre-inset outer ring.
                    let method = Nest.MethodOf(inset, row.variant.Collision)
                    from admitted in locus.IsEmpty
                        ? Fin.Succ(Option<NoFitPolygon>.None)
                        : Admit(locus, identity, new NfpWitness(identity, row.stock.Identity, row.variant.Identity,
                            NfpRelation.Admitted, method, method.Exact ? 0.0 : policy.ChordError, policy.Clearance,
                            policy.Kerf, locus.Count, locus.Count(static loop => loop.Winding() == Sign.Negative)))
                            .Map(Some)
                    select admitted)).As()
            .Map(static rows => rows.Choose(identity).ToSeq());

    internal static Fin<NoFitPolygon> Admit(Seq<Loop> locus, UInt128 identity, NfpWitness witness) =>
        NoFitPolygon.Validate(locus, identity, witness, out NoFitPolygon polygon).Admitted(polygon);

    readonly struct PairAction(Variant[] variants, Fin<NoFitPolygon>[] results, NestPolicy policy) : IAction2D {
        public void Invoke(int i, int j) {
            Variant fixedPart = variants[i], orbiting = variants[j];
            UInt128 identity = Key(fixedPart, orbiting, policy);
            results[(i * variants.Length) + j] = Built(fixedPart, orbiting, identity, policy);
        }
    }

    static Fin<NoFitPolygon> Built(Variant fixedPart, Variant orbiting, UInt128 identity, NestPolicy policy) =>
        from fixedDense in Nest.Lower(fixedPart.Collision, policy.ChordError)
        from orbitDense in Nest.Lower(orbiting.Collision, policy.ChordError)
        from reflected in Reflect(orbitDense)
        from trace in PolygonAlgebra.Apply(new PolygonOp.Morphology(fixedDense, reflected,
            NfpRelation.Forbidden.Kind))
        from locus in trace.Regioned(new KernelFault.InvalidValue("nfp", "nest:morphology-trace"))
            .Map(static topology => topology.Nodes.Map(static node => node.Boundary))
        from admitted in locus.IsEmpty
            ? Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:nfp-empty"))
            : Fin.Succ(locus)
        let method = Nest.MethodOf(fixedPart.Collision, orbiting.Collision)
        let witness = new NfpWitness(identity, fixedPart.Identity, orbiting.Identity, NfpRelation.Forbidden,
            method, method.Exact ? 0.0 : policy.ChordError, policy.Clearance, policy.Kerf, locus.Count,
            locus.Count(static loop => loop.Winding() == Sign.Negative))
        from polygon in Admit(admitted, identity, witness)
        select polygon;

    static Fin<Loop> Reflect(Loop loop) => Loop.Admit(loop.Vertices.Map(point => Point3d.Origin - (point - Point3d.Origin)).ToArr(),
        loop.Closed, loop.Bulges, loop.Tolerance).Map(static reflected => reflected.AsCcw());
}

internal sealed class ConstraintGraph {
    readonly BidirectionalGraph<int, SEdge<int>> closure;
    readonly HashMap<int, int> rank;
    readonly Seq<PlacementConstraint> constraints;

    ConstraintGraph(BidirectionalGraph<int, SEdge<int>> closure, HashMap<int, int> rank,
        Seq<PlacementConstraint> constraints) => (this.closure, this.rank, this.constraints) = (closure, rank, constraints);

    public static Fin<ConstraintGraph> Admit(int partCount, Seq<PlacementConstraint> constraints) {
        if (partCount < 1 || constraints.Exists(rule => !Valid(rule, partCount)))
            return Fin.Fail<ConstraintGraph>(new KernelFault.InvalidValue("nfp", "nest:constraint-domain"));
        BidirectionalGraph<int, SEdge<int>> graph = new(allowParallelEdges: false, vertexCapacity: partCount);
        graph.AddVertexRange(Enumerable.Range(0, partCount));
        constraints.Iter(rule => rule.Switch(
            precedes: row => { _ = graph.AddVerticesAndEdge(new SEdge<int>(row.Before, row.After)); return unit; },
            together: static _ => unit, separate: static _ => unit, adjacent: static _ => unit,
            inside: row => { _ = graph.AddVerticesAndEdge(new SEdge<int>(row.Outer, row.Inner)); return unit; },
            stockOnly: static _ => unit, keepOut: static _ => unit));
        return graph.IsDirectedAcyclicGraph()
            ? Fin.Succ(new ConstraintGraph(
                graph.ComputeTransitiveClosure(static (source, target) => new SEdge<int>(source, target)),
                toHashMap(toSeq(graph.ComputeTransitiveReduction()
                    .TopologicalSort()).Map(static (part, index) => (part, index))),
                constraints))
            : Fin.Fail<ConstraintGraph>(new KernelFault.InvalidValue("nfp", "nest:constraint-cycle"));
    }

    public Seq<PartInstance> Order(Seq<PartInstance> requested, Seq<PartRule> rules) =>
        toSeq(requested.OrderBy(part => closure.InDegree(part.PartId))
            .ThenByDescending(part => rules.Find(rule => rule.PartId == part.PartId).Map(static rule => rule.Priority).IfNone(0))
            .ThenBy(part => rank.Find(part.PartId).IfNone(int.MaxValue))
            .ThenBy(static part => part.Ordinal));

    public Fin<Seq<ConstraintVerdict>> Accept(Candidate candidate, Loop shape, Loop envelope, Seq<Placed> placed) =>
        constraints.TraverseM(rule => rule.Switch(
        precedes: row => Fin.Succ(row.After != candidate.Part.PartId || placed.Exists(slot => slot.Part.PartId == row.Before)),
        together: row => Fin.Succ(!row.Parts.Contains(candidate.Part.PartId) || placed
            .Filter(slot => row.Parts.Contains(slot.Part.PartId))
            .ForAll(slot => slot.Transform.SheetIndex == candidate.StockSlot)),
        separate: row => Fin.Succ(candidate.Part.PartId != row.Left && candidate.Part.PartId != row.Right || placed
            .Filter(slot => slot.Part.PartId == (candidate.Part.PartId == row.Left ? row.Right : row.Left))
            .ForAll(slot => row.Metric.Measure(slot.Shape, shape) >= row.Distance)),
        adjacent: row => {
            Seq<Placed> mates = placed.Filter(slot => slot.Part.PartId == (candidate.Part.PartId == row.Left ? row.Right : row.Left));
            return Fin.Succ(candidate.Part.PartId != row.Left && candidate.Part.PartId != row.Right || mates.IsEmpty
                || mates.Exists(slot => row.Metric.Measure(slot.Shape, shape) <= row.MaximumDistance));
        },
        inside: row => candidate.Part.PartId != row.Inner
            ? Fin.Succ(true)
            : placed.Filter(slot => slot.Part.PartId == row.Outer).TraverseM(slot => Nest.Relate(slot.Shape, shape)).As()
                .Map(relations => relations.Exists(static relation => relation == ArcRelation.SecondInsideFirst)),
        stockOnly: row => Fin.Succ(row.Part != candidate.Part.PartId || row.Stock.Contains(candidate.Stock)),
        keepOut: row => row.Stock != candidate.Stock
            ? Fin.Succ(true)
            : row.Region.TraverseM(region => Nest.Relate(region, envelope)).As()
                .Map(static relations => relations.ForAll(static relation => relation == ArcRelation.Disjoint)))
            .Map(satisfied => satisfied
                ? (ConstraintVerdict)new ConstraintVerdict.Satisfied(rule)
                : new ConstraintVerdict.Violated(rule))).As();

    public Fin<Seq<ConstraintVerdict>> Verdicts(Seq<Placed> placed) => constraints.TraverseM(rule => Satisfied(rule, placed)
        .Map(satisfied => satisfied
            ? (ConstraintVerdict)new ConstraintVerdict.Satisfied(rule)
            : new ConstraintVerdict.Violated(rule))).As();

    static Fin<bool> Satisfied(PlacementConstraint rule, Seq<Placed> placed) => rule.Switch(
        precedes: row => Fin.Succ(placed.Map((slot, index) => (slot, index)).Find(slot => slot.slot.Part.PartId == row.Before)
            .Bind(before => placed.Map((slot, index) => (slot, index)).Find(slot => slot.slot.Part.PartId == row.After)
                .Map(after => before.index < after.index)).IfNone(false)),
        together: row => Fin.Succ(row.Parts.ForAll(part => placed.Exists(slot => slot.Part.PartId == part))
            && row.Parts.Bind(part => placed.Filter(slot => slot.Part.PartId == part))
                .GroupBy(static slot => slot.Transform.SheetIndex).Count() == 1),
        separate: row => Fin.Succ(placed.Filter(slot => slot.Part.PartId == row.Left).ForAll(left => placed
            .Filter(slot => slot.Part.PartId == row.Right).ForAll(right => row.Metric.Measure(left.Shape, right.Shape) >= row.Distance))),
        adjacent: row => Fin.Succ(placed.Filter(slot => slot.Part.PartId == row.Left).Exists(left => placed
            .Filter(slot => slot.Part.PartId == row.Right).Exists(right => row.Metric.Measure(left.Shape, right.Shape) <= row.MaximumDistance))),
        inside: row => placed.Filter(slot => slot.Part.PartId == row.Inner).TraverseM(inner => placed
            .Filter(slot => slot.Part.PartId == row.Outer).TraverseM(outer => Nest.Relate(outer.Shape, inner.Shape)).As()
            .Map(static relations => relations.Exists(static relation => relation == ArcRelation.SecondInsideFirst))).As()
            .Map(static verdicts => !verdicts.IsEmpty && verdicts.ForAll(identity)),
        stockOnly: row => Fin.Succ(placed.Filter(slot => slot.Part.PartId == row.Part).ForAll(slot => row.Stock.Contains(slot.Stock.Identity))),
        keepOut: row => placed.Filter(slot => slot.Stock.Identity == row.Stock).TraverseM(slot => row.Region
            .TraverseM(region => Nest.Relate(region, slot.Shape)).As()
            .Map(static relations => relations.ForAll(static relation => relation == ArcRelation.Disjoint))).As()
            .Map(static verdicts => verdicts.ForAll(identity)));

    static bool Valid(PlacementConstraint rule, int partCount) => rule.Switch(
        precedes: row => Id(row.Before, partCount) && Id(row.After, partCount) && row.Before != row.After,
        together: row => row.Parts.Distinct().Count >= 2 && row.Parts.ForAll(part => Id(part, partCount)),
        separate: row => Id(row.Left, partCount) && Id(row.Right, partCount) && row.Left != row.Right
            && double.IsFinite(row.Distance) && row.Distance >= 0.0,
        adjacent: row => Id(row.Left, partCount) && Id(row.Right, partCount) && row.Left != row.Right
            && double.IsFinite(row.MaximumDistance) && row.MaximumDistance >= 0.0,
        inside: row => Id(row.Inner, partCount) && Id(row.Outer, partCount) && row.Inner != row.Outer,
        stockOnly: row => Id(row.Part, partCount) && row.Stock.Count > 0,
        keepOut: row => !row.Region.IsEmpty && row.Region.ForAll(static loop => loop.Closed && loop.Count >= 3));

    static bool Id(int value, int partCount) => value >= 0 && value < partCount;
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
