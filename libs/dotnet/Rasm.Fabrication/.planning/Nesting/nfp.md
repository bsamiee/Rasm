# [RASM_FABRICATION_NFP]

`Nest` owns true-shape placement over heterogeneous material stock. One admitted `NestPolicy` compiles each search case into the shared candidate algebra, `NoFitPolygon` retains complete configuration-space topology, and exact arc-space collision and containment gate every emitted transform.

`Nest.Solve` preserves the process boundary, `Nest.Charts` the atlas projection, `NestPlan` the rectangular hand-off, `Stock.FromRemnant` the inventory union, and `FabricationResult.Placement` the process projection. Placement legality stays the material's: each stock row carries the `MaterialSymmetry` its component projects at `Rasm.Materials/Component/component#COMPONENT_OWNER`, and moves derive from that law at `Rasm/Parametric/patternmap#PATTERNING`. Part rules carry kernel chirality — parity, congruence class, book-match link — admitted through `NestParts.Of`.

## [01]-[INDEX]

- [02]-[DOMAIN]: Generated owners admit stock with its symmetry law, part rules with their parity, class, and mate, constraints, search policy, and NFP evidence, and one fold admits the kernel panel and pattern carriers into them.
- [03]-[CONFIGURATION_SPACE]: Arc-witnessed Minkowski proposals, exact feasibility, and parallel pair materialization.
- [04]-[SEARCH]: Constraint closure, law-gated move sets, candidate generation, parameterized search programs, and stock scheduling.
- [05]-[DELIVERY]: Rectangular-plan honor, remnant minting, placement evidence, and content identity.

## [02]-[DOMAIN]

- Owner: `Stock` closes physical inventory modalities while `StockBody` carries common material, topology, exclusion, piece-and-lot trace, and cost facts.
- Owner: `NestPolicy` admits search, clearance, kerf, edge allowance, objective, candidate, constraint, evaluation budget, batch, and chiral-floor policy once.
- Owner: `PartRule` carries the demand for one part — quantity, material, admitted angles, grain axis, priority — beside the three chirality columns the kernel measured: `Chirality` parity, congruence class, and book-match mate.
- Owner: `NestParts` is the ONE admission from the kernel carriers into that vocabulary; input shape selects the arm, and a lane inferring parity from geometry it did not produce is the deleted form.
- Cases: `PlacementMode` carries greedy, beam, evolutionary, annealed, Voronoi, and rectangular programs with case-local evidence; rectangular strategy count and depth remain explicit policy.
- Cases: `PlacementConstraint` carries precedence, grouping, separation, adjacency, containment, stock eligibility, and keep-out facts, each occurrence carrying its own `ConstraintForce`.
- Cases: `Chirality` carries `Straight` and `Mirrored` — `Mirrored` names the outline that IS the reflected member of its shape class, whose producer is `Rasm/Parametric/panelize#PANELIZATION` `PanelField.Flipped`.
- Law: one `StockFacts` projection derives common body facts, and one `StockTraits` projection derives physicality, nestability, rectangular extent policy, gauge, grain, and the admitted symmetry law; a new modality answers remnant and stock consumers through one case arm.
- Law: every planar modality carrying a grain axis carries the STOCK material's `MaterialSymmetry` beside it, read off `Component.Symmetry` at admission and never re-derived; an isotropic sheet passes `MaterialSymmetry.Free`, and a bar, tube, billet, or filament answers `Free` because no planar move exists to gate.
- Law: the grain VECTOR the law refines IS the existing `GrainAxis` angle in the sheet plane — one spelling, one column, and a second direction column beside it is the deleted form.
- Law: stock identity folds the law's `Rotation` and `Mirror` keys immediately after the grain axis, so one sheet declared chiral and the same sheet declared reflective are two stocks — a stricter law admits a different move set and must address differently.
- Law: a `Mate` names a part id in the roster and the naming is SYMMETRIC — the policy factory proves `a`'s mate is `b` exactly when `b`'s mate is `a`, refusing `nest:mate-asymmetric`, because a half pair obligates an adjacency no second row honours.
- Law: `ConstraintForce.Required` rejects a candidate and fails delivery; `ConstraintForce.Preferred` admits the candidate and rides `NestObjective` as weighted penalty.
- Law: six objective weights fan onto one comparable number and every term reaches it DIMENSIONLESS. `NestBasis` carries all three nondimensionalizers — the characteristic length the cut and shared-edge terms divide through, the currency reference the cost term does, and the violation ceiling the constraint term does — on the scoring input, never on `NestSearch`; the basis derives once per solve from the admitted inventory and policy and threads on `SearchState`.
- Packages: `Rasm` supplies `Deterministic` (the ONE draw owner), `ChartAtlas`/`UvIsland.Boundary`, the `Chain` loop carrier, and the `MaterialSymmetry`/`RotationOrder`/`MirrorRight` legality algebra beside the `PanelResult` and `InstanceBatch` carriers `NestParts` admits; the `Geometry2D` owner supplies morphology, Boolean, measure, and the cell diagram; `LanguageExt` supplies admission, traversal, and the `Fin` result; `Thinktecture` supplies the generated stock, constraint, chirality, and mode families; `UnitsNet` supplies material quantities and the `Length` ratio the objective's characteristic-length nondimensionalization takes.
- Growth: a stock modality, constraint, candidate source, objective, or search algorithm lands as one case or row consumed by the existing folds; a further kernel carrier of placed instances lands as one `NestParts.Of` arm over the same three chirality columns.
- Boundary: the symmetry law arrives DERIVED from the material's own construction rows — this plane admits it, folds it into identity, and derives moves from it, and a caller-set legality knob beside a material that already models direction has no parameter to reach.

## [03]-[CONFIGURATION_SPACE]

- Owner: `NoFitPolygon` admits one complete locus with its identity, relation, and approximation witness.
- Cases: `NfpRelation.Forbidden` carries the part-part `MorphologyKind.Sum` locus; `NfpRelation.Admitted` carries the part-stock `MorphologyKind.Difference` inner-fit locus every absolute placement seeds from.
- Cases: `NfpMethod` binds an approximation to its evidence — a chord-projected locus carries positive chord error, an arc-exact locus carries none — and `Nest.MethodOf` DERIVES the row from the operands the Minkowski walk consumes: the line-space walk is exact on bulge-free loops, so a polygonal pair mints the arc-exact locus and an arc-bearing pair the chord-projected one; a policy scalar can never assert a fidelity the operands do not carry.
- Law: `PolygonAlgebra.Apply(new PolygonOp.Morphology(...))` proposes line-space candidates; `ArcAlgebra.Apply(new ArcOp.Inspect(...))` decides containment, exclusion, and collision on the original bulged loops.
- Law: pair identity includes canonical loop geometry, tolerance, rotation, clearance, and chord error; inner-fit identity substitutes stock identity and edge allowance.
- Law: each collision profile offsets its part by half the combined clearance and kerf; stock-boundary feasibility adds edge allowance without weakening part-part or exclusion checks.
- Law: `PairMemo` content-keys the pair matrix under the same `PairTable.Key` identities through the branch `HybridCache` surface — the runtime-carried instance is the in-process tier, a durable L2 federates at the Persistence cache boundary, hit and miss counts settle on `NestSearch` and write as the engine memo rows, and a failed build throws through the awaited factory so a fault never caches; the runtime cancellation token rides `GetOrBuild` into the awaited cache call and the awaited leg funnels through `Try.lift`, the ONE inbound exception boundary, so token-proved cancellation lowers to the kernel cancellation fault rather than rethrowing on the async channel; inner-fit rows stay direct because an empty locus is a verdict, not a cacheable polygon.
- Law: the exact execution token owns cancellation: requested polling lowers `Errors.Cancelled`, and matching thrown cancellation lowers through `Try.lift`; unrequested or foreign cancellation remains the exact captured failure. `PolicyInadmissible` never carries cancellation.
- Auto: `ParallelHelper.For2D` fills uncached independent pair slots; memoized rows await one `HybridCache` task per identity outside the synchronous kernel, and `TraverseM` returns the first typed geometry failure without partial cache publication.
- Packages: the `Geometry2D` owner supplies `PolygonOp.Morphology` and the arc-exact inspection API; `Rasm` supplies the kernel Minkowski walk beneath it; `Microsoft.Extensions.Caching.Hybrid` supplies the pair memo; `CommunityToolkit.HighPerformance` supplies the parallel pair fill.
- Boundary: an empty pair morphology remains a typed fault, an empty inner-fit locus is the absent-key verdict that no position admits the part, and every returned topology component survives the projection.

## [04]-[SEARCH]

- Owner: `ConstraintGraph` proves precedence acyclic, derives closure for ordering, and precomputes the reduction rank the ordering fold reads.
- Law: exactly TWO of the seven placement constraints mint precedence edges — `Precedes` orders its pair directly and `Inside` orders an outer part before the part it contains. The other five are ORDER-FREE by construction: `Together`, `Separate`, `Adjacent`, `StockOnly`, and `KeepOut` each constrain WHERE a candidate may sit, never when, so they gate at `Accept` against the placed set and a precedence edge for any of them would forbid a placement the geometry admits.
- Law: the transitive CLOSURE and the transitive REDUCTION are both retained as result data on the graph — `InDegree` over the closure is the ordering's primary key and the reduction's own topological sort IS the `rank` column its tertiary key reads — so neither walk's output is computed and dropped.
- Owner: `Nest.Moves` is the ONE move set — a `(Rotation, Mirrored)` roster DERIVED per part rule against the stock laws it is handed, so one stock answers that stock's admitted turns and flips and the whole inventory answers the union a genome may draw from. There is no move family and no legality flag: a free rotation roster and a caller-set flip switch are both the deleted form.
- Law: a rotation survives when the stock's rotation order admits the part's grain RESIDUAL — the placed grain against the stock axis — under the stock's own angular tolerance, and `Nest.Fold` is the ONE place grain lowers onto the rotation order: a grain axis is a LINE, so a half-turn preserves it and a `Free`-order stock still binds a grain-bearing part to `RotationOrder.Twofold`, which is exactly the half-turn congruence the bare grain gate always ran.
- Law: `Nest.Grain` is the ONE placed-grain fold both nesting lanes read — a mirror negates local X, so a direction at `g` leaves at `π − g` before the turn applies, and a lane spelling that reflection itself forks the alignment two consumers must agree on.
- Law: a mirrored move exists only where the grant admits `MirrorRight.Place`; a `Refused` grant admits none, and the reflected loops are minted ONCE per part per parity rather than per candidate.
- Law: under `MirrorRight.Merge` a variant salts its shape CLASS instead of its part ordinal, so two congruent parts cut from one mould present one pair identity and the NFP matrix collapses from parts onto classes — sound because `NestParts.Of` lands every panel ring in its own panel frame, so congruent panels present one canonical loop preimage. Where the law refuses the merge the part ordinal stays the salt, because two moulds are two shapes.
- Law: under `MirrorRight.Pair` — the `Matched` grant — every `Mate` pair mints ONE `PlacementConstraint.Adjacent` row at `ConstraintForce.Required` on admission, so book-matched mates nest as adjacent units through the constraint graph the search already honours rather than through a second pairing mechanism. Its ceiling is the shared-cut gap — clearance and kerf under the boundary metric — because a book match that does not touch is two parts that merely landed near each other, and obligation mints where ANY admitted stock carries the right, since a match honoured on one sheet and abandoned on the next is not a match.
- Law: `Nest.Admitted` is the ONE candidate gate, answering material, then grain, then symmetry in cost order. `Grain` names the ABSENT axis — a directional part against stock declaring no direction — and `Symmetry` names the refused MOVE: this turn's residual outside the rotation order's angular tolerance, or a flip the grant refuses. Per-axis booleans let a caller read one axis and forget the other, and neither names the move the law refused.
- Owner: `CandidateSource` composes NFP vertices, inner-fit boundaries, arc-native contacts, stock extrema, and relaxed Voronoi centroids into one slot-keyed frontier; its `Absolute` column decides which rows can seed an empty stock.
- Auto: `PlacementMode.Compile` emits one `SearchProgram`; `SearchOp` folds order, branch, breed, mutate, cool, relax, bound, and select steps over one `SearchState`.
- Auto: `SearchState.Evidence.Evaluated` counts exact decisions across every active run, and `SearchOp.Bounded` halts the stochastic sub-program at `NestPolicy.EvaluationBudget`.
- Auto: the genome carries a MOVE map rather than a rotation map, and every draw — seed, mutation, initial seat — reads `Moves`; a draw reaching `PartRule.Angles` directly proposes turns the stock law already refused and spends the evaluation budget grading them.
- Auto: rectangular programs delegate every packer and heuristic axis to `StockNest.Pack`; `Nest` contains no second rectangle provider switch, and the honoured plan's own `Unplaced` roster carries the graded refusal each unseated instance earned, so the rectangular lane reaches this evidence naming material, extent, or grain rather than a blanket capacity claim.
- Auto: one `PolygonScan` precomputes a branch's placed bounding envelopes once and every candidate position folds over that structure, so a disjoint position costs one bounds test; the arc-exact relation walk that names the colliding part runs only where the scan reports contact.
- Packages: `Rasm` supplies the `Deterministic` lanes every genome, mutation, and cooling draw reads; the `Geometry2D` owner supplies `PolygonOp.Cells` for the relaxed free-space frontier and `PolygonScan` for the placement scan; `QuikGraph` supplies the constraint closure; `StockNest.Pack` owns every rectangular packer axis.
- Boundary: exact containment, overlap, material, grain, symmetry, exclusion, and blocking-constraint verdicts gate a candidate before objective ranking; pair identity folds the move's parity beside its rotation, so a shape and its reflection can never answer one NFP witness.

## [05]-[DELIVERY]

- Law: the content preimage covers every `PartTransform` member including `Instance` and `Mirrored`, so two placements differing only by instance — or only by which face of the blank went up — never collide on one key.
- Law: the evidence digest covers what the layout is, never what the search measured about it. Columns derived from geometry the placement key already covers — shared-edge overlap, pierce census, and the placement and remnant rosters `KeyOf` frames around the digest — and columns describing the run rather than the result — memo hit and miss counters and the mould and chiral-floor census — stay out of the preimage, so refining a measure, changing a cache tier, or re-running the same solve can never re-key a landed plan.
- Law: `Moulds` counts the DISTINCT cut outlines the plan pays for — two parts of one shape class share an outline where their parity agrees, or where the stock law merges the mirrored congruence and one blank turns; a classless part is its own mould. Read against the class count it prices the mould delta the layout absorbs, and `ChiralFloor` carries the split the panel law already made, so the two answer what the material choice cost and what it was always going to cost.
- Entry: `Nest.Solve` admits profiles, inventory, policy, and the run's own `FabricationRuntime`, then dispatches resolved rectangular plans or true-shape search on one `Fin` result. The token threads the pair-memo lane into `HybridCache.GetOrCreateAsync` so an in-flight cancel surfaces on the kernel cancellation fault.
- Entry: `NestBench.Workload` admits the `nfp-placement` measured workload — search lane, live inventory, part and budget floors — and `NestBench.Run` is the fold the corpus gate times against `FabricationBenchClaims.NfpPlacement`, taking the same runtime the spine hands the plane so the timed entry is the one a real run reaches; measurement and result projection stay the bench edge's under the AppHost claim-field map.
- Entry: `Nest.Charts` admits atlas distortion and reconstructs every island boundary cycle. `Nest.Rings` is the ONE `Chain`-to-`Loop` termination in the package — `Forming/sheet` composes it rather than re-admitting the same kernel carrier, because the walk that produced the chain already owns winding and once-counted edges and a second termination forks the admitted context.
- Result: `NestSearch` remains private algorithm state. Delivery returns `FabricationResult.Placement` directly with transforms, utilization, unplaced count, remnants, and the content key, while writing its engine steps through the runtime's mounted instruments.
- Packages: `FabricationCanon` over the `Rasm.Element` `CanonicalWriter` is the one byte codec every preimage on this page composes — stock and pair identity, evidence digest, placement key — so a `-0.0`, a NaN payload, or a string boundary can never fork identity between two of them; `Rasm` supplies `ContentHash` for the memo key rendering and `UvIsland.Boundary` for the atlas projection; `Rasm.Fabrication.Process` supplies faults, runtime, and mounted observation; `CommunityToolkit.HighPerformance` supplies the parallel pair fill.
- Boundary: remnant difference uses true profiles and the combined clearance-and-kerf offset; feasibility uses the offset collision profiles; only consumed stock enters the area and cost denominators.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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
using Rasm.Parametric;
using Rasm.Processing;
using Rhino.Geometry;
using System.Collections.Frozen;
using System.Threading.Tasks;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] ---------------------------------------------------------------------------
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

    internal double Score(NestSearch evidence, NestBasis basis) {
        double area = Math.Max(evidence.StockArea, double.Epsilon);
        return (Weights.Yield * evidence.Utilization)
            - (Weights.Cut * (UnitsNet.Length.FromMillimeters(evidence.CutLength) / basis.Reference))
            + (Weights.Remnant * evidence.RemnantValue / area)
            - (Weights.Cost * (evidence.StockCost / basis.Cost))
            + (Weights.SharedEdge * (UnitsNet.Length.FromMillimeters(evidence.SharedEdge) / basis.Reference))
            - (Weights.Constraint * evidence.ConstraintPenalty / basis.Constraint);
    }
}

public readonly record struct NestBasis(UnitsNet.Length Reference, double Cost, double Constraint) {
    public static NestBasis Of(Seq<Stock> inventory, NestPolicy policy, Seq<PlacementConstraint> constraints) {
        double area = Math.Max(inventory.Sum(static stock => stock.Facts.AreaMm2), double.Epsilon);
        return new NestBasis(
            UnitsNet.Length.FromMillimeters(Math.Sqrt(area)),
            Math.Max(inventory.Sum(static stock => stock.Facts.Cost), double.Epsilon),
            Math.Max(constraints.Count + policy.Parts.Sum(static row => row.Quantity), 1));
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
                point + new Vector3d(row.Transform.Tx, row.Transform.Ty, 0.0), request.Angle, request.Mirrored, 0.0)))));
    public static readonly CandidateSource Contact = new("contact", Absolute: false, static request =>
        Fin.Succ(request.Placed.Bind(row => Nest.Contacts(row, request.Variant)
            .Map(slot => new Candidate(request.Part, row.Transform.SheetIndex, row.Stock.Identity,
                slot.Point, request.Angle, request.Mirrored, slot.Length)))));
    public static readonly CandidateSource InnerFit = new("inner-fit", Absolute: true, static request =>
        Fin.Succ(request.Inventory.Map((stock, slot) => (stock, slot)).Bind(row => request.Pairs
            .Find(PairTable.InnerKey(request.Variant, row.stock, request.Policy)).ToSeq()
            .Bind(static polygon => polygon.Locus).Bind(static loop => toSeq(loop.Vertices))
            .Map(point => new Candidate(request.Part, row.slot, row.stock.Identity, point, request.Angle,
                request.Mirrored, 0.0)))));
    public static readonly CandidateSource Extrema = new("extrema", Absolute: true, static request =>
        Fin.Succ(request.Inventory.Map((stock, slot) => (stock, slot)).Bind(row => row.stock.Region.Bind(loop => toSeq(loop.Vertices)
            .Map(point => new Candidate(request.Part, row.slot, row.stock.Identity,
                point - (request.Variant.Seated.Bound().Min - Point3d.Origin), request.Angle, request.Mirrored, 0.0))))));
    public static readonly CandidateSource Voronoi = new("voronoi", Absolute: true, static request =>
        Nest.VoronoiCandidates(request.Part, request.Variant, request.Angle, request.Mirrored, request.Placed,
            request.Inventory, request.VoronoiIterations, request.VoronoiStrength));

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

    static double Nearest(Loop host, Loop probe) => toSeq(Enumerable.Range(0, host.Count))
        .Bind(span => toSeq(probe.Vertices).Map(point => new Edge3(host.At(span), host.At(span + 1)).Gap(point)))
        .Fold(double.PositiveInfinity, Math.Min);
}

[SmartEnum<string>]
public sealed partial class Chirality {
    public static readonly Chirality Straight = new("straight", Reflected: false);
    public static readonly Chirality Mirrored = new("mirrored", Reflected: true);

    public bool Reflected { get; }
}

[ComplexValueObject]
public sealed partial class PartRule {
    public int PartId { get; }
    public int Quantity { get; }
    public Option<MaterialId> Material { get; }
    public Seq<double> Angles { get; }
    public Option<double> GrainAxis { get; }
    public Chirality Parity { get; }
    public Option<int> ShapeClass { get; }
    public Option<int> Mate { get; }
    public int Priority { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int partId, ref int quantity,
        ref Option<MaterialId> material, ref Seq<double> angles, ref Option<double> grainAxis, ref Chirality parity,
        ref Option<int> shapeClass, ref Option<int> mate, ref int priority) =>
        validationError = partId < 0 || quantity < 1 || angles.IsEmpty || angles.Exists(static angle => !double.IsFinite(angle))
            || angles.Distinct().Count != angles.Count
            || grainAxis.Exists(static angle => !double.IsFinite(angle))
            || parity is null || shapeClass.Exists(static row => row < 0)
            || mate.Exists(row => row < 0 || row == partId)
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

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref MaterialId material, ref Context tolerance,
        ref Seq<Loop> region, ref Seq<Loop> exclusions, ref string piece, ref string lot, ref Option<string> heat, ref double cost) =>
        validationError = !tolerance.IsValid || region.IsEmpty || region.ForAll(static loop => loop.Winding() != Sign.Positive)
            || region.Concat(exclusions).Exists(loop => !loop.Closed || loop.Count < 3 || loop.Tolerance != tolerance)
            || string.IsNullOrWhiteSpace(piece) || string.IsNullOrWhiteSpace(lot) || !double.IsFinite(cost) || cost < 0.0
                ? new ValidationError("nest:stock-body")
                : null;
}

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
    Option<double> GrainAxis,
    MaterialSymmetry Law);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Stock {
    private Stock() { }

    public sealed record Sheet(StockBody Body, double Thickness, Option<double> GrainAxis, MaterialSymmetry Law) : Stock;
    public sealed record Plate(StockBody Body, double Thickness, Option<double> GrainAxis, MaterialSymmetry Law) : Stock;
    public sealed record Roll(StockBody Body, double Width, double AvailableLength, Option<double> GrainAxis, MaterialSymmetry Law) : Stock;
    public sealed record Coil(StockBody Body, double Width, double AvailableLength, double Thickness, Option<double> GrainAxis,
        MaterialSymmetry Law) : Stock;
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
    public MaterialSymmetry Law => Traits.Law;

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
            RectangularExtentPolicy.Region, Some(row.Thickness), row.GrainAxis, row.Law),
        plate: static (area, row) => TraitsOf(area,
            Positive(row.Thickness) && Axis(row.GrainAxis), true,
            RectangularExtentPolicy.Region, Some(row.Thickness), row.GrainAxis, row.Law),
        roll: static (area, row) => TraitsOf(area,
            Positive(row.Width) && Positive(row.AvailableLength) && Axis(row.GrainAxis), true,
            RectangularExtentPolicy.Bounded(row.Width, row.AvailableLength), None, row.GrainAxis, row.Law),
        coil: static (area, row) => TraitsOf(area,
            Positive(row.Width) && Positive(row.AvailableLength) && Positive(row.Thickness) && Axis(row.GrainAxis), true,
            RectangularExtentPolicy.Bounded(row.Width, row.AvailableLength), Some(row.Thickness), row.GrainAxis, row.Law),
        barStock: static (area, row) => TraitsOf(area,
            Positive(row.Diameter) && Positive(row.Length) && Nonnegative(row.EndAllowance)
                && (2.0 * row.EndAllowance) < row.Length,
            false, RectangularExtentPolicy.Forbidden, None, None, MaterialSymmetry.Free),
        tubeStock: static (area, row) => TraitsOf(area,
            Positive(row.OuterDiameter) && Positive(row.WallThickness)
                && row.WallThickness < 0.5 * row.OuterDiameter && Positive(row.Length)
                && Nonnegative(row.SeamAllowance) && Nonnegative(row.EndAllowance)
                && (2.0 * row.EndAllowance) < row.Length,
            false, RectangularExtentPolicy.Forbidden, Some(row.WallThickness), None, MaterialSymmetry.Free),
        billet: static (area, row) => TraitsOf(area, Positive(row.Depth), true,
            RectangularExtentPolicy.Region, Some(row.Depth), None, MaterialSymmetry.Free),
        filament: static (area, row) => TraitsOf(area,
            Positive(row.Diameter) && Positive(row.SpoolLength), false,
            RectangularExtentPolicy.Forbidden, None, None, MaterialSymmetry.Free),
        fromRemnant: static (area, row) => TraitsOf(area,
            !row.Remnant.Region.IsEmpty && Axis(row.Remnant.Profile.GrainAxisRadians)
                && row.Remnant.Profile.GaugeMm.ForAll(static gauge => double.IsFinite(gauge) && gauge >= 0.0)
                && row.Remnant.Profile.CostPerSquareMillimeter.ForAll(static cost => double.IsFinite(cost) && cost >= 0.0)
                && row.Remnant.Profile.Exclusions.ForAll(exclusion => exclusion.Closed && exclusion.Count >= 3
                    && exclusion.Tolerance == row.Remnant.Boundary.Tolerance),
            true, RectangularExtentPolicy.Region, row.Remnant.Profile.GaugeMm,
            row.Remnant.Profile.GrainAxisRadians, row.Remnant.Profile.Law));

    public double Area => Math.Max(0.0, Math.Abs(Region.Sum(static loop => loop.Area()))
        - Exclusions.Sum(static loop => Math.Abs(loop.Area())));
    public UInt128 Identity => Nest.Identity(Region, Tolerance, writer =>
        IdentitySalt(writer).U128(Nest.Identity(Exclusions, Tolerance, static salt => salt.String("stock-exclusions"))));

    static bool Positive(double value) => double.IsFinite(value) && value > 0.0;
    static bool Nonnegative(double value) => double.IsFinite(value) && value >= 0.0;
    static bool Axis(Option<double> value) => value.ForAll(double.IsFinite);
    static StockTraits TraitsOf(double area, bool physical, bool nestable,
        RectangularExtentPolicy rectangular, Option<double> gauge, Option<double> grain, MaterialSymmetry law) =>
        new(physical, physical && nestable && area > 0.0, rectangular, gauge, grain, law);

    CanonicalWriter IdentitySalt(CanonicalWriter writer) => Switch(
        state: writer,
        sheet: static (sink, row) => LawKey(BodyKey(sink.String("sheet"), row.Body)
            .Double(row.Thickness).Maybe(row.GrainAxis, Scalar), row.Law),
        plate: static (sink, row) => LawKey(BodyKey(sink.String("plate"), row.Body)
            .Double(row.Thickness).Maybe(row.GrainAxis, Scalar), row.Law),
        roll: static (sink, row) => LawKey(BodyKey(sink.String("roll"), row.Body)
            .Double(row.Width).Double(row.AvailableLength).Maybe(row.GrainAxis, Scalar), row.Law),
        coil: static (sink, row) => LawKey(BodyKey(sink.String("coil"), row.Body)
            .Double(row.Width).Double(row.AvailableLength).Double(row.Thickness).Maybe(row.GrainAxis, Scalar), row.Law),
        barStock: static (sink, row) => BodyKey(sink.String("bar"), row.Body)
            .Double(row.Diameter).Double(row.Length).Double(row.EndAllowance),
        tubeStock: static (sink, row) => BodyKey(sink.String("tube"), row.Body)
            .Double(row.OuterDiameter).Double(row.WallThickness).Double(row.Length)
            .Double(row.SeamAllowance).Double(row.EndAllowance),
        billet: static (sink, row) => BodyKey(sink.String("billet"), row.Body).Double(row.Depth),
        filament: static (sink, row) => BodyKey(sink.String("filament"), row.Body)
            .Double(row.Diameter).Double(row.SpoolLength),
        fromRemnant: static (sink, row) => LawKey(sink.String("remnant").U128(row.Remnant.Identity)
            .Maybe(row.Remnant.Profile.GaugeMm, Scalar)
            .Maybe(row.Remnant.Profile.GrainAxisRadians, Scalar), row.Remnant.Profile.Law)
            .Maybe(row.Remnant.Profile.CostPerSquareMillimeter, Scalar));

    static CanonicalWriter BodyKey(CanonicalWriter writer, StockBody body) => writer
        .String(body.Material.Value).String(body.Piece).String(body.Lot)
        .Maybe(body.Heat, static (held, heat) => held.String(heat))
        .Double(body.Cost);

    static CanonicalWriter LawKey(CanonicalWriter writer, MaterialSymmetry law) =>
        writer.Ordinal(law.Rotation.Key).String(law.Mirror.Key);

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

    public int ChiralFloor { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref PlacementMode mode, ref Seq<PartRule> parts,
        ref Seq<PlacementConstraint> constraints, ref FrozenSet<CandidateSource> candidates, ref CandidateOrder frontier,
        ref NestObjective objective,
        ref double clearance, ref double chordError, ref double kerf, ref double edgeAllowance,
        ref double rectangleResolution, ref int pairBatchFloor, ref int pairConcurrency, ref int evaluationBudget,
        ref int chiralFloor) {
        Seq<PartRule> roster = parts;
        bool paired = roster.ForAll(row => row.Mate.ForAll(mate => roster.Find(peer => peer.PartId == mate)
            .Exists(peer => peer.Mate.Exists(back => back == row.PartId))));
        validationError = parts.IsEmpty || toSeq(parts.GroupBy(static row => row.PartId)).Exists(static group => group.Count() != 1)
            || candidates.Count == 0 || !double.IsFinite(clearance) || clearance < 0.0 || !double.IsFinite(chordError)
            || chordError <= 0.0 || !double.IsFinite(kerf) || kerf < 0.0 || !double.IsFinite(edgeAllowance)
            || edgeAllowance < 0.0 || !double.IsFinite(rectangleResolution) || rectangleResolution <= 0.0
            || pairBatchFloor < 1 || pairConcurrency < 1 || evaluationBudget < 1 || chiralFloor < 0 || !Admits(mode)
            || !candidates.Any(static source => source.Absolute)
                ? new ValidationError("nest:nest-policy")
                : paired ? null : new ValidationError("nest:mate-asymmetric");
    }

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
    public sealed record Symmetry(int PartId, int Instance, UInt128 Stock) : UnplacedReason;
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
internal sealed record NestSearch(PlacementMode Mode, NestObjective Objective, Seq<UInt128> Stock, Seq<NfpWitness> Pairs,
    Seq<ConstraintVerdict> Constraints, Seq<UnplacedReason> Unplaced, int Candidates, int Evaluated, int Rejected,
    double UsedArea, double StockArea, double CutLength, double RemnantValue, double StockCost,
    double SharedEdge = 0.0, int Pierces = 0, int MemoHits = 0, int MemoMisses = 0, int Moulds = 0, int ChiralFloor = 0) {
    public double Utilization => StockArea > 0.0 ? Math.Clamp(UsedArea / StockArea, 0.0, 1.0) : 0.0;
    public double ConstraintPenalty => Constraints.Sum(static row => row.Penalty)
        + Unplaced.Count(static row => row is UnplacedReason.Constraint);
}

public readonly record struct PartInstance(int PartId, int Ordinal);
internal sealed record Variant(int PartId, double Rotation, bool Mirrored, Loop True, Loop Seated, Loop Collision, UInt128 Identity);
internal sealed record Candidate(PartInstance Part, int StockSlot, UInt128 Stock, Point3d Point, double Rotation, bool Mirrored,
    double Contact);
internal sealed record Placed(PartInstance Instance, Variant Part, Stock Stock, PartTransform Transform, Loop Shape, Loop Envelope);
internal sealed record CandidateRequest(PartInstance Part, Variant Variant, double Angle, bool Mirrored, Seq<Placed> Placed,
    Seq<Stock> Inventory, HashMap<UInt128, NoFitPolygon> Pairs, NestPolicy Policy, int VoronoiIterations, double VoronoiStrength);
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PlacementDecision {
    private PlacementDecision() { }
    public sealed record Accepted(Placed Value, Seq<ConstraintVerdict> Constraints) : PlacementDecision;
    public sealed record Rejected(UnplacedReason Reason) : PlacementDecision;
}
internal sealed record Genome(Seq<PartInstance> Order, HashMap<PartInstance, (double Rotation, bool Mirrored)> Move);
internal sealed record SearchRun(Seq<Placed> Placed, Seq<UnplacedReason> Unplaced, Seq<ConstraintVerdict> Constraints,
    int Candidates, int Evaluated, int Rejected);
internal sealed record SearchState(Seq<Genome> Population, Seq<SearchRun> Runs, NestSearch Evidence, NestBasis Basis,
    ulong Random, double Temperature, int VoronoiIterations, double VoronoiStrength);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Nest {
    private const ulong DrawSeed = 0UL;
    private const double HeatSeed = 0.0;
    private const int RelaxationSeed = 1;
    private const double StrengthSeed = 1.0;

    private static readonly (double Rotation, bool Mirrored) Still = (0.0, false);

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
                        ? Place(parts, inventory, policy.Nesting, runtime)
                        : ValueTask.FromResult(Fin.Fail<FabricationResult>(new FabricationFault.StockOverflow(parts.Count, 0)))),
            Fail: static error => ValueTask.FromResult(Fin.Fail<FabricationResult>(error)));
    }

    public static Fin<Arr<Loop>> Charts(ChartAtlas atlas, double maxAreaStretch, Context tolerance) =>
        !double.IsFinite(maxAreaStretch) || maxAreaStretch < 1.0
        || atlas.Distortion.MaxArea > maxAreaStretch || atlas.Distortion.MinArea < 1.0 / maxAreaStretch
            ? Fin.Fail<Arr<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "atlas:distortion"))
            : atlas.Islands.TraverseM(island => island.Boundary(tolerance, Op.Of(name: nameof(Charts)))
                    .Bind(chains => Rings(chains, tolerance))).As()
                .Map(static regions => regions.Bind(static loops => loops).ToArr());

    internal static Fin<FabricationResult> Honor(Arr<Loop> parts, NestPlan plan) =>
        plan.Placements.TraverseM(row =>
            row.PartId < 0 || row.PartId >= parts.Count
                ? Fin.Fail<PartTransform>(new FabricationFault.NoFit(row.PartId, Seq<double>()))
                : Seated(parts[row.PartId], row.RotationRadians, row.Mirrored).Bind(part =>
                    PartTransform.Admit(row.PartId, row.Instance, row.XMm - part.Bound().Min.X,
                        row.YMm - part.Bound().Min.Y, row.RotationRadians, row.SheetIndex, row.Mirrored))).As()
            .Bind(placed => toSeq(parts).Head
                .Filter(_ => !placed.IsEmpty)
                .ToFin(new FabricationFault.StockOverflow(parts.Count, plan.Yield.SheetCount))
                .Bind(seed => KeyOf(placed, Seq<Remnant>(), plan.Evidence.Digest, seed.Tolerance))
                .Map(key => (FabricationResult)new FabricationResult.Placement(placed, plan.Yield.UtilizationRatio,
                    plan.Yield.UnplacedCount, Seq<Remnant>())));

    internal static UInt128 Identity(Seq<Loop> loops, Context tolerance, Func<CanonicalWriter, CanonicalWriter> salt) =>
        FabricationCanon.Ordered(tolerance, writer =>
            salt(writer.Double(tolerance.Absolute.Value).Double(tolerance.Angle.Value))
                .Rows(Ordered(loops), static (held, loop) => loop.CanonicalBytes(held)));

    private static Seq<Loop> Ordered(Seq<Loop> loops) => toSeq(loops.OrderBy(Preimage));

    private static UInt128 Preimage(Loop loop) =>
        FabricationCanon.Ordered(loop.Tolerance, loop.CanonicalBytes);

    internal static NfpMethod MethodOf(params ReadOnlySpan<Loop> operands) =>
        LanguageExt.Iterable<Loop>.FromSpan(operands)
            .Exists(static loop => loop.Bulges.Exists(static bulge => bulge != 0.0))
                ? NfpMethod.ChordProjected
                : NfpMethod.ArcExact;

    static ValueTask<Fin<FabricationResult>> Place(
        Arr<Loop> parts, Seq<Stock> inventory, NestPolicy policy, FabricationRuntime runtime) {
        Fin<(ConstraintGraph Graph, HashMap<(int PartId, long Angle, bool Mirrored), Variant> Variants)> prepared =
            from _ in policy.Parts.Count != parts.Count
                || policy.Parts.Exists(rule => rule.PartId < 0 || rule.PartId >= parts.Count)
                ? Fin.Fail<Unit>(new KernelFault.InvalidValue("nfp", "nest:part-rule-profile"))
                : Fin.Succ(unit)
            from graph in ConstraintGraph.Admit(parts.Count, policy.Constraints.Concat(Mates(policy, inventory)))
            from variants in Variants(parts, inventory, policy)
            select (graph, variants);
        return prepared.Match(
            Succ: scope => Search(parts, inventory, policy, runtime, scope.Graph, scope.Variants),
            Fail: static error => ValueTask.FromResult(Fin.Fail<FabricationResult>(error)));
    }

    static Seq<PlacementConstraint> Mates(NestPolicy policy, Seq<Stock> inventory) =>
        inventory.Exists(static stock => stock.Law.Mirror.Rights.Admits(MirrorRight.Pair))
            ? policy.Parts.Choose(rule => rule.Mate.Filter(mate => rule.PartId < mate)
                .Map(mate => (PlacementConstraint)new PlacementConstraint.Adjacent(rule.PartId, mate,
                    policy.Clearance + policy.Kerf, ProximityMetric.Boundary, ConstraintForce.Required)))
            : Seq<PlacementConstraint>();

    static async ValueTask<Fin<FabricationResult>> Search(
        Arr<Loop> parts,
        Seq<Stock> inventory,
        NestPolicy policy,
        FabricationRuntime runtime,
        ConstraintGraph graph,
        HashMap<(int PartId, long Angle, bool Mirrored), Variant> variants) {
        Option<PairMemo> memo = runtime.Memo.Map(static cache => new PairMemo(cache));
        Fin<HashMap<UInt128, NoFitPolygon>> built =
            await PairTable.Build(variants, inventory, policy, memo, runtime.Cancel).ConfigureAwait(false);
        return built.Bind(pairs =>
            from admitted in Initial(inventory, policy, graph)
            let initial = admitted with { Evidence = admitted.Evidence with { Pairs = pairs.Values.Map(static row => row.Witness).ToSeq() } }
            from searched in policy.Mode.Compile(policy.EvaluationBudget).Steps.FoldM<Fin, SearchState>(initial,
                (state, operation) => Apply(operation, state, parts, inventory, variants, pairs, policy, graph)).As()
            let measured = memo.Match(
                Some: cache => searched with { Evidence = searched.Evidence with {
                    MemoHits = (int)cache.Census.Hits, MemoMisses = (int)cache.Census.Misses } },
                None: () => searched)
            from result in Deliver(measured, parts, inventory, policy, graph, runtime.Instruments)
            select result);
    }

    static Fin<SearchState> Apply(SearchOp operation, SearchState state, Arr<Loop> parts, Seq<Stock> inventory,
        HashMap<(int PartId, long Angle, bool Mirrored), Variant> variants, HashMap<UInt128, NoFitPolygon> pairs,
        NestPolicy policy, ConstraintGraph graph) => operation.Switch(
            state: (state, parts, inventory, variants, pairs, policy, graph),
            ordered: static (scope, _) => Fin.Succ(scope.state with { Population = scope.state.Population.Map(genome =>
                genome with { Order = scope.graph.Order(genome.Order, scope.policy.Parts) }) }),
            seeded: static (scope, row) => Fin.Succ(Seed(scope.state, scope.policy.Parts, scope.inventory, row.Population, row.Seed)),
            branched: static (scope, row) => Decode(scope.state, scope.parts, scope.inventory, scope.variants, scope.pairs,
                scope.policy, scope.graph, row.Width),
            bred: static (scope, _) => Fin.Succ(Breed(scope.state)),
            mutated: static (scope, row) => Fin.Succ(Mutate(scope.state, row.Rate, scope.policy.Parts, scope.inventory)),
            cooled: static (scope, row) => Fin.Succ(Cool(scope.state, row.Temperature, row.Factor, scope.policy.Objective)),
            relaxed: static (scope, row) => Relax(scope.state, row.Iterations, row.Strength),
            selected: static (scope, row) => Fin.Succ(Select(scope.state, scope.policy.Objective, row.Width)),
            repeated: static (scope, row) => Enumerable.Range(0, row.Count).ToSeq().FoldM<Fin, SearchState>(scope.state,
                (cycle, _) => row.Body.FoldM<Fin, SearchState>(cycle, (inner, op) => Apply(inner, scope.parts,
                    scope.inventory, scope.variants, scope.pairs, scope.policy, scope.graph)).As()).As(),
            bounded: static (scope, row) => row.Body.FoldM<Fin, SearchState>(scope.state, (inner, op) =>
                inner.Evidence.Evaluated >= row.Evaluations
                    ? Fin.Succ(inner)
                    : Apply(inner, scope.parts, scope.inventory, scope.variants, scope.pairs, scope.policy,
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
        HashMap<(int PartId, long Angle, bool Mirrored), Variant> variants, HashMap<UInt128, NoFitPolygon> pairs,
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
        HashMap<(int PartId, long Angle, bool Mirrored), Variant> variants,
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

    static Fin<T> Scanning<T>(Seq<Placed> placed, Func<Option<PolygonScan>, Fin<T>> body) =>
        placed.IsEmpty
            ? body(None)
            : PolygonScan.Scan(
                placed.Map(static row => row.Envelope),
                PolygonFill.NonZero,
                scan => body(Some(scan)),
                Op.Of(name: nameof(Scanning)));

    static Fin<PlacementDecision> Exact(Candidate candidate, Seq<Placed> placed, Option<PolygonScan> scan,
        Seq<Stock> inventory, HashMap<(int PartId, long Angle, bool Mirrored), Variant> variants, NestPolicy policy,
        ConstraintGraph graph) =>
        candidate.StockSlot >= 0 && candidate.StockSlot < inventory.Count
        && inventory[candidate.StockSlot].Identity == candidate.Stock
            ? variants.Find((candidate.Part.PartId, BitConverter.DoubleToInt64Bits(candidate.Rotation), candidate.Mirrored))
                .ToFin(new KernelFault.InvalidValue("nfp", "nest:variant-key"))
                .Map(found => (Stock: inventory[candidate.StockSlot], Index: candidate.StockSlot, Variant: found))
            .Bind(scope =>
                from transform in PartTransform.Admit(scope.Variant.PartId, candidate.Part.Ordinal,
                    candidate.Point.X, candidate.Point.Y, candidate.Rotation, scope.Index, candidate.Mirrored)
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
                    Admitted(candidate, scope.Stock, policy.Parts),
                    boundary ? None : Some<UnplacedReason>(new UnplacedReason.Boundary(
                        candidate.Part.PartId, candidate.Part.Ordinal, scope.Stock.Identity)),
                    overlaps.Find(static row => row.relation != ArcRelation.Disjoint).Map<UnplacedReason>(row =>
                        new UnplacedReason.Collision(candidate.Part.PartId, candidate.Part.Ordinal,
                            row.row.Part.PartId, row.row.Instance.Ordinal)),
                    exclusions.Find(static row => row.relation != ArcRelation.Disjoint).Map<UnplacedReason>(_ =>
                        new UnplacedReason.Exclusion(candidate.Part.PartId, candidate.Part.Ordinal, scope.Stock.Identity)),
                    constraints.Find(static verdict => verdict.Blocking).Map<UnplacedReason>(verdict =>
                        new UnplacedReason.Constraint(candidate.Part.PartId, candidate.Part.Ordinal, verdict.Constraint)))
                    .Somes().Head
                let accepted = new Placed(candidate.Part, scope.Variant, scope.Stock, transform, shape, envelope)
                from decision in rejected.Match(
                    Some: reason => Fin.Succ<PlacementDecision>(new PlacementDecision.Rejected(reason)),
                    None: () => graph.Verdicts(placed.Add(accepted)).Map<PlacementDecision>(verdicts =>
                        new PlacementDecision.Accepted(accepted, verdicts)))
                select decision)
            : Fin.Fail<PlacementDecision>(new KernelFault.InvalidValue("nfp", "nest:stock-slot"));

    static Fin<Seq<Candidate>> Candidates(PartInstance part, Genome genome, Seq<Placed> placed, Seq<Stock> inventory,
        HashMap<(int PartId, long Angle, bool Mirrored), Variant> variants, HashMap<UInt128, NoFitPolygon> pairs,
        NestPolicy policy, int voronoiIterations, double voronoiStrength) =>
        genome.Move.Find(part).ToFin(new KernelFault.InvalidValue("nfp", "nest:move-key")).Bind(move =>
            variants.Find((part.PartId, BitConverter.DoubleToInt64Bits(move.Rotation), move.Mirrored))
                .ToFin(new KernelFault.InvalidValue("nfp", "nest:variant-key"))
                .Bind(variant => toSeq(policy.Candidates.OrderBy(static source => source.Key))
                    .TraverseM(source => source.Generate(new CandidateRequest(part, variant, move.Rotation, move.Mirrored,
                        placed, inventory, pairs, policy, voronoiIterations, voronoiStrength))).As())
                .Map(static rows => toSeq(rows.Bind(identity)
                    .DistinctBy(static row => (row.StockSlot, row.Point.X, row.Point.Y, row.Rotation, row.Mirrored)))));

    internal static Fin<Seq<Candidate>> VoronoiCandidates(PartInstance part, Variant variant, double angle, bool mirrored,
        Seq<Placed> placed, Seq<Stock> inventory, int iterations, double strength) =>
        inventory.Map(static (stock, slot) => (stock, slot)).TraverseM(row => {
            Seq<Point3d> points = toSeq(row.stock.Region.Bind(static loop => toSeq(loop.Vertices))
                .Concat(placed.Filter(placedRow => placedRow.Transform.SheetIndex == row.slot)
                    .Bind(static placedRow => toSeq(placedRow.Shape.Vertices)))
                .DistinctBy(static point => (point.X, point.Y)));
            Vector3d anchor = variant.Seated.Bound().Center - Point3d.Origin;
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
                            part, row.slot, row.stock.Identity, cell.Centroid - anchor, angle, mirrored, 0.0))));
        }).As().Map(static rows => rows.Bind(identity));

    static Fin<SearchState> Relax(SearchState state, int iterations, double strength) =>
        iterations < 0 || strength is < 0.0 or > 1.0
            ? Fin.Fail<SearchState>(new KernelFault.InvalidValue("nfp", "nest:relax-policy"))
            : Fin.Succ(state with { VoronoiIterations = iterations, VoronoiStrength = strength });

    static SearchState Seed(SearchState state, Seq<PartRule> rules, Seq<Stock> inventory, int population, int seed) {
        Seq<PartInstance> canonical = toSeq(rules.OrderByDescending(static row => row.Priority)
            .SelectMany(row => Enumerable.Range(0, row.Quantity).Select(ordinal => new PartInstance(row.PartId, ordinal))));
        Seq<Genome> genomes = toSeq(Enumerable.Range(0, population)).Map(index => new Genome(
            index == 0 ? canonical : toSeq(canonical.OrderBy(part => Deterministic.Stream(
                lanes: [part.PartId, part.Ordinal], seed: seed + index))),
            toHashMap(canonical.Map(part => {
                ulong stream = Deterministic.Stream(lanes: [part.PartId, part.Ordinal], seed: seed + index);
                Seq<(double Rotation, bool Mirrored)> moves = Roster(rules, part.PartId, inventory);
                return (part, moves.IsEmpty ? Still : moves[Deterministic.NextBelow(ref stream, moves.Count)]);
            }))));
        return state with { Population = genomes, Random = Deterministic.Stream(lanes: [seed]) };
    }

    static SearchState Breed(SearchState state) => state with {
        Population = state.Population.Zip(state.Population.Rev(), static (left, right) => left with {
            Order = left.Order.Map((part, index) => (index & 1) == 0 ? part : right.Order[index]).Distinct().Concat(left.Order).Distinct(),
        }),
    };

    static SearchState Mutate(SearchState state, double rate, Seq<PartRule> rules, Seq<Stock> inventory) => state with {
        Population = state.Population.Map((genome, index) => {
            ulong stream = Deterministic.Stream(lanes: [index], seed: (long)state.Random);
            if (Deterministic.Unit(lanes: [index, 0L], seed: (long)state.Random) >= rate || genome.Order.Count < 2) return genome;
            int left = Deterministic.NextBelow(ref stream, genome.Order.Count);
            int right = Deterministic.NextBelow(ref stream, genome.Order.Count);
            Seq<PartInstance> order = genome.Order.Map((part, at) => at == left ? genome.Order[right] : at == right ? genome.Order[left] : part);
            Seq<(double Rotation, bool Mirrored)> moves = Roster(rules, order[left].PartId, inventory);
            (double Rotation, bool Mirrored) move = moves.IsEmpty
                ? Still
                : moves[Deterministic.NextBelow(ref stream, moves.Count)];
            return genome with { Order = order, Move = genome.Move.SetItem(order[left], move) };
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
        Genome genome = new(order, toHashMap(order.Map(part =>
            (part, Roster(policy.Parts, part.PartId, inventory).Head.IfNone(Still)))));
        NestSearch evidence = new(policy.Mode, policy.Objective, inventory.Map(static stock => stock.Identity), Seq<NfpWitness>(),
            graph.Constraints.Map(static rule => (ConstraintVerdict)new ConstraintVerdict.Satisfied(rule)), Seq<UnplacedReason>(), 0, 0, 0,
            0.0, 0.0, 0.0, 0.0, 0.0) with { ChiralFloor = policy.ChiralFloor };
        return Fin.Succ(new SearchState(Seq(genome), Seq<SearchRun>(), evidence,
            NestBasis.Of(inventory, policy, graph.Constraints),
            Random: DrawSeed, Temperature: HeatSeed, VoronoiIterations: RelaxationSeed, VoronoiStrength: StrengthSeed));
    }

    static Fin<SearchState> FromPlan(SearchState state, Arr<Loop> parts, NestPlan plan) =>
        plan.Placements.TraverseM(row => Seated(parts[row.PartId], row.RotationRadians, row.Mirrored).Bind(shape =>
            PartTransform.Admit(row.PartId, row.Instance, row.XMm - shape.Bound().Min.X, row.YMm - shape.Bound().Min.Y,
                row.RotationRadians, row.SheetIndex, row.Mirrored).Map(transform => FromPlanPlacement(row, shape, transform, plan.Stock[row.SheetIndex])))).As()
            .Map(rows => state with { Runs = Seq(new SearchRun(rows, plan.Unplaced,
                state.Evidence.Constraints, plan.Yield.RequestedCount, plan.Yield.RequestedCount, plan.Yield.UnplacedCount)), Evidence = state.Evidence with {
                Evaluated = plan.Yield.RequestedCount,
                UsedArea = rows.Sum(static row => Math.Abs(row.Shape.Area())),
                StockArea = plan.Yield.StockAreaMm2,
            }});

    static Placed FromPlanPlacement(NestPlacement row, Loop shape, PartTransform transform, Stock stock) {
        Variant variant = new(row.PartId, row.RotationRadians, row.Mirrored, shape, shape, shape,
            Identity(Seq(shape), shape.Tolerance, salt => salt.String("plan").Ordinal(row.PartId).Ordinal(row.Instance)));
        return new Placed(new PartInstance(row.PartId, row.Instance), variant, stock, transform, shape, shape);
    }

    static Fin<FabricationResult> Deliver(SearchState state, Arr<Loop> parts, Seq<Stock> inventory,
        NestPolicy policy, ConstraintGraph graph, Option<InstrumentSet> set) =>
        state.Runs.Fold(Option<SearchRun>.None, (best, run) => best
                .Filter(held => policy.Objective.Score(Evidence(held, state.Evidence), state.Basis)
                    >= policy.Objective.Score(Evidence(run, state.Evidence), state.Basis)).IfNone(run))
            .ToFin(new FabricationFault.StockOverflow(parts.Count, inventory.Count))
            .Bind(best => best.Placed.IsEmpty
                ? Fin.Fail<FabricationResult>(new FabricationFault.StockOverflow(parts.Count, inventory.Count))
                : best.Placed.TraverseM(row => row.Transform.Apply(parts[row.Part.PartId])).As().Bind(_ => graph.Verdicts(best.Placed).Bind(verdicts =>
                verdicts.Exists(static verdict => verdict.Blocking)
                    ? Fin.Fail<FabricationResult>(new KernelFault.InvalidValue("nfp", "nest:constraint-verdict"))
                    : toSeq(best.Placed.GroupBy(static row => row.Transform.SheetIndex)).Map(static group => toSeq(group))
                        .TraverseM(rows => rows.Head.ToFin(new KernelFault.InvalidValue("nfp", "nest:stock-group"))
                            .Bind(head => Remnants.From(head.Stock, rows.Map(static row => row.Shape),
                                policy.Clearance + policy.Kerf))).As()
                    .Map(remnants => remnants.Bind(identity))
                    .Bind(remnants => best.Placed.Head
                        .ToFin(new KernelFault.InvalidValue("nfp", "nest:placed-head"))
                        .Bind(head => {
                            Seq<PartTransform> transforms = best.Placed.Map(static row => row.Transform);
                            NestSearch evidence = Evidence(best, state.Evidence) with {
                                Constraints = verdicts,
                                RemnantValue = remnants.Sum(static row => Math.Abs(row.Region.Sum(static loop => loop.Area()))),
                                Moulds = MouldsOf(best, policy.Parts),
                            };
                            return KeyOf(transforms, remnants, Digest(evidence, head.Shape.Tolerance), head.Shape.Tolerance)
                                .Bind<FabricationResult>(key =>
                                    from _steps in set.Steps(
                                        (EnginePhase.Candidates, evidence.Candidates),
                                        (EnginePhase.Evaluated, evidence.Evaluated),
                                        (EnginePhase.CandidatesRejected, evidence.Rejected),
                                        (EnginePhase.MemoHits, evidence.MemoHits),
                                        (EnginePhase.MemoMisses, evidence.MemoMisses),
                                        (EnginePhase.Moulds, evidence.Moulds),
                                        (EnginePhase.ChiralFloor, evidence.ChiralFloor))
                                    select (FabricationResult)new FabricationResult.Placement(
                                        transforms, evidence.Utilization, evidence.Unplaced.Count, remnants));
                        })))));

    static NestSearch Evidence(SearchRun run, NestSearch basis) {
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

    static UInt128 Digest(NestSearch evidence, Context tolerance) =>
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
        symmetry: static (w, row) => w.String("symmetry").Ordinal(row.PartId).Ordinal(row.Instance).U128(row.Stock),
        boundary: static (w, row) => w.String("boundary").Ordinal(row.PartId).Ordinal(row.Instance).U128(row.Stock),
        collision: static (w, row) => w.String("collision").Ordinal(row.PartId).Ordinal(row.Instance)
            .Ordinal(row.OtherPartId).Ordinal(row.OtherInstance),
        exclusion: static (w, row) => w.String("exclusion").Ordinal(row.PartId).Ordinal(row.Instance).U128(row.Stock),
        constraint: static (w, row) => ConstraintWrite(w.String("constraint").Ordinal(row.PartId).Ordinal(row.Instance), row.Rule),
        budget: static (w, row) => w.String("budget").Ordinal(row.PartId).Ordinal(row.Instance).Ordinal(row.Evaluated),
        capacity: static (w, row) => w.String("capacity").Ordinal(row.PartId).Ordinal(row.Instance));

    static Fin<ContentKey> KeyOf(Seq<PartTransform> placed, Seq<Remnant> remnants, UInt128 evidence, Context tolerance) =>
        FabricationCanon.Keyed(EgressKind.Placement, tolerance, writer => writer
            .U128(evidence)
            .Rows(
                toSeq(placed.OrderBy(static row => row.SheetIndex).ThenBy(static row => row.PartId)
                    .ThenBy(static row => row.Instance).ThenBy(static row => row.Tx).ThenBy(static row => row.Ty)
                    .ThenBy(static row => row.RotationRadians).ThenBy(static row => row.Mirrored)),
                static (held, row) => held.Ordinal(row.PartId).Ordinal(row.Instance).Ordinal(row.SheetIndex)
                    .Double(row.Tx).Double(row.Ty).Double(row.RotationRadians).Bool(row.Mirrored))
            .Rows(toSeq(remnants.OrderBy(static row => row.Identity)), static (held, row) => held.U128(row.Identity)),
            PlacementOp);


    static Fin<Loop> Admit(Loop loop, int index) => !loop.Closed
        ? Fin.Fail<Loop>(new FabricationFault.OpenLoop(FabConcern.Nesting, index))
        : loop.Count < 3 || loop.Vertices.Exists(static point => !double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z))
            || loop.Bulges.Exists(static bulge => !double.IsFinite(bulge))
                ? Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Polyline, index, $"nest:profile:{index}"))
                : Fin.Succ(loop.AsCcw());

    static Fin<HashMap<(int PartId, long Angle, bool Mirrored), Variant>> Variants(
        Arr<Loop> parts, Seq<Stock> inventory, NestPolicy policy) =>
        policy.Parts.Bind(rule => rule.Angles.Bind(angle => Parities(inventory)
                .Map(mirrored => (Rule: rule, Angle: angle, Mirrored: mirrored))))
            .TraverseM(row => Seated(parts[row.Rule.PartId], row.Angle, row.Mirrored)
                .Bind(shape => ArcShapeOffset(Seq(shape), 0.5 * (policy.Clearance + policy.Kerf))
                .Bind(collision => collision.Count == 1
                    ? collision.Head
                        .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:clearance-topology"))
                        .Map(envelope => new Variant(row.Rule.PartId, row.Angle, row.Mirrored,
                        parts[row.Rule.PartId], shape, envelope,
                        Identity(collision, parts[row.Rule.PartId].Tolerance,
                            salt => Mould(salt, row.Rule, inventory).Double(row.Angle).Bool(row.Mirrored))))
                    : Fin.Fail<Variant>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:clearance-topology"))))).As()
            .Map(rows => toHashMap(rows.Map(static row =>
                ((row.PartId, BitConverter.DoubleToInt64Bits(row.Rotation), row.Mirrored), row))));

    static Seq<bool> Parities(Seq<Stock> inventory) =>
        inventory.Exists(static stock => stock.Law.Mirror.Rights.Admits(MirrorRight.Place))
            ? Seq(false, true)
            : Seq(false);

    static CanonicalWriter Mould(CanonicalWriter writer, PartRule rule, Seq<Stock> inventory) =>
        rule.ShapeClass
            .Filter(_ => inventory.ForAll(static stock => stock.Law.Mirror.Rights.Admits(MirrorRight.Merge)))
            .Match(
                Some: shapeClass => writer.String("class").Ordinal(shapeClass),
                None: () => writer.Ordinal(rule.PartId));

    internal static Fin<Loop> Seated(Loop part, double radians, bool mirrored) {
        double cosine = Math.Cos(radians), sine = Math.Sin(radians);
        return Loop.Admit(part.Vertices.Map(point => {
                double x = mirrored ? -point.X : point.X;
                return new Point3d((x * cosine) - (point.Y * sine), (x * sine) + (point.Y * cosine), point.Z);
            }).ToArr(),
            part.Closed, mirrored ? part.Bulges.Map(static bulge => -bulge) : part.Bulges, part.Tolerance);
    }

    internal static Fin<Seq<Loop>> ArcShapeOffset(Seq<Loop> loops, double distance) =>
        loops.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, None, "nest:arc-offset-empty"))
            .Bind(head => ArcForest.Admit(loops, head.Tolerance, head.Plane))
                .Bind(forest => ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Forest(forest), distance)))
            .Bind(static trace => trace is ArcTrace.Forest forest
                ? Fin.Succ(forest.Geometry.Loops)
                : Fin.Fail<Seq<Loop>>(new KernelFault.InvalidValue("nfp", "nest:arc-offset-trace")));

    internal static Fin<ArcRelation> Relate(Loop first, Loop second) =>
        ArcForest.Admit(Seq(first, second), first.Tolerance, first.Plane)
            .Bind(forest => ArcAlgebra.Apply(new ArcOp.Inspect(forest, new ArcProbe.Pair(first, second))))
            .Bind(static trace => trace is ArcTrace.Inspection { Evidence: ArcInspection.Pair pair }
                ? Fin.Succ(pair.Relation)
                : Fin.Fail<ArcRelation>(new KernelFault.InvalidValue("nfp", "nest:arc-relation-trace")));

    internal static Fin<Loop> Lower(Loop loop, double error) => ArcAlgebra.Densify(new ArcProjection.Lower(loop, error))
        .Bind(static trace => trace
            .Lowering(new KernelFault.InvalidValue("nfp", "nest:arc-projection-trace"))
            .Map(static evidence => evidence.Output));

    internal static RotationOrder Fold(Option<double> grain, MaterialSymmetry law) =>
        law.Rotation == RotationOrder.Free && grain.IsSome ? RotationOrder.Twofold : law.Rotation;

    internal static double Grain(double axis, double rotation, bool mirrored) =>
        (mirrored ? Math.PI - axis : axis) + rotation;

    internal static Seq<(double Rotation, bool Mirrored)> Moves(PartRule rule, Seq<Stock> inventory) =>
        inventory.Bind(stock => {
            RotationOrder order = Fold(rule.GrainAxis, stock.Law);
            double cone = stock.Tolerance.Angle.Value;
            Seq<(double Rotation, bool Mirrored)> straight = rule.Angles.Map(static angle => (Rotation: angle, Mirrored: false));
            return (stock.Law.Mirror.Rights.Admits(MirrorRight.Place)
                    ? straight.Concat(rule.Angles.Map(static angle => (Rotation: angle, Mirrored: true)))
                    : straight)
                .Filter(move => rule.GrainAxis.ForAll(axis => stock.GrainAxis
                    .Exists(grain => order.Admits(Grain(axis, move.Rotation, move.Mirrored) - grain, cone))));
        }).Distinct();

    static Seq<(double Rotation, bool Mirrored)> Roster(Seq<PartRule> rules, int partId, Seq<Stock> inventory) =>
        rules.Find(row => row.PartId == partId).Map(rule => Moves(rule, inventory))
            .IfNone(Seq<(double Rotation, bool Mirrored)>());

    static Option<UnplacedReason> Admitted(Candidate candidate, Stock stock, Seq<PartRule> rules) =>
        rules.Find(rule => rule.PartId == candidate.Part.PartId).Bind(rule =>
            !rule.Material.ForAll(material => material == stock.Material)
                ? Some<UnplacedReason>(new UnplacedReason.Material(
                    candidate.Part.PartId, candidate.Part.Ordinal, stock.Identity))
                : rule.GrainAxis.IsSome && stock.GrainAxis.IsNone
                    ? Some<UnplacedReason>(new UnplacedReason.Grain(
                        candidate.Part.PartId, candidate.Part.Ordinal, stock.Identity))
                    : Moves(rule, Seq(stock)).Contains((candidate.Rotation, candidate.Mirrored))
                        ? Option<UnplacedReason>.None
                        : Some<UnplacedReason>(new UnplacedReason.Symmetry(
                            candidate.Part.PartId, candidate.Part.Ordinal, stock.Identity)));

    static int MouldsOf(SearchRun run, Seq<PartRule> rules) => run.Placed
        .Choose(row => rules.Find(rule => rule.PartId == row.Part.PartId).Map(rule => (
            Class: rule.ShapeClass,
            Part: rule.ShapeClass.IsSome ? Option<int>.None : Some(rule.PartId),
            Parity: row.Stock.Law.Mirror.Rights.Admits(MirrorRight.Merge) ? Chirality.Straight : rule.Parity)))
        .Distinct().Count;

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

    internal static Fin<Seq<Loop>> Rings(Seq<Chain> chains, Context tolerance) =>
        chains.TraverseM(chain => Loop.Admit(
            toSeq(chain.Points).ToArr(), chain.Points.IsClosed, Arr<double>(), tolerance)).As();

}

public static class NestParts {
    private const int MotifClass = 0;

    public static Fin<(Arr<Loop> Profiles, Seq<PartRule> Rules, int ChiralFloor)> Of(
        PanelResult panels, MaterialSymmetry stockLaw, Seq<double> angles, Context tolerance) =>
        Placeable(panels.Field.Flipped, stockLaw)
            .Bind(_ => toSeq(Enumerable.Range(0, panels.Field.Flipped.Count))
                .TraverseM(panel =>
                    from loop in Outline(panels.Field, panel, tolerance)
                    from rule in Rule(panel, angles, panels.Field.Flipped[panel],
                        Some(panels.Field.ShapeClass[panel]), Option<int>.None)
                    select (loop, rule)).As())
            .Map(rows => (rows.Map(static row => row.loop).ToArr(), rows.Map(static row => row.rule),
                panels.ChiralSplit));

    public static Fin<(Arr<Loop> Profiles, Seq<PartRule> Rules, int ChiralFloor)> Of(
        InstanceBatch instances, Loop motif, MaterialSymmetry stockLaw) {
        bool pairs = stockLaw.Mirror.Rights.Admits(MirrorRight.Pair);
        return Placeable(instances.Mirrored, stockLaw)
            .Bind(_ => toSeq(Enumerable.Range(0, instances.Spin.Count))
                .TraverseM(site =>
                    from loop in Nest.Seated(motif, radians: 0.0, instances.Mirrored[site])
                    from rule in Rule(site, Seq(instances.Spin[site]), instances.Mirrored[site], Some(MotifClass),
                        pairs ? instances.PairOf[site] : Option<int>.None)
                    select (loop, rule)).As())
            .Map(rows => (rows.Map(static row => row.loop).ToArr(), rows.Map(static row => row.rule),
                Split(instances.Mirrored, stockLaw)));
    }

    static Fin<Unit> Placeable(Arr<bool> mirrored, MaterialSymmetry law) =>
        law.Mirror.Rights.Admits(MirrorRight.Place) || !mirrored.Exists(identity)
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("nfp", "nest:mirror-refused"));

    static int Split(Arr<bool> mirrored, MaterialSymmetry law) =>
        !law.Mirror.Rights.Admits(MirrorRight.Merge) && mirrored.Exists(identity) ? 1 : 0;

    static Fin<Loop> Outline(PanelField field, int panel, Context tolerance) {
        Vector3d x = field.XAxis[panel], y = Vector3d.CrossProduct(field.ZAxis[panel], field.XAxis[panel]);
        Point3d origin = field.Origin[panel];
        return Loop.Admit(
            toSeq(Enumerable.Range(field.CornerOffsets[panel], field.CornerOffsets[panel + 1] - field.CornerOffsets[panel]))
                .Map(slot => field.Vertices[field.Corners[slot]] - origin)
                .Map(offset => new Point3d(offset * x, offset * y, 0.0)).ToArr(),
            closed: true, Arr<double>(), tolerance);
    }

    static Fin<PartRule> Rule(int partId, Seq<double> angles, bool flipped, Option<int> shapeClass, Option<int> mate) =>
        PartRule.Validate(partId, quantity: 1, Option<MaterialId>.None, angles, Option<double>.None,
                flipped ? Chirality.Mirrored : Chirality.Straight, shapeClass, mate, priority: 0, out PartRule rule)
            .Admitted(rule);
}

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

    public static ValueTask<Fin<FabricationResult>> Run(
        (FabricationPolicy.Nest Policy, FabricationInput Input) workload, FabricationRuntime runtime) =>
        Nest.Solve(workload.Policy, workload.Input, runtime);
}

// --- [CONFIGURATION_SPACE] -------------------------------------------------------------
internal sealed class PairMemo(HybridCache cache) {
    static readonly HybridCacheEntryOptions Tuned = new() {
        Expiration = TimeSpan.FromHours(8),
        LocalCacheExpiration = TimeSpan.FromHours(8),
    };

    long hits;
    long misses;

    public (long Hits, long Misses) Census => (Interlocked.Read(ref hits), Interlocked.Read(ref misses));

    public async ValueTask<Fin<NoFitPolygon>> GetOrBuild(UInt128 identity, Func<Fin<NoFitPolygon>> build, CancellationToken cancel) =>
        await Try.lift(async execution => {
            bool built = false;
            NoFitPolygon polygon = await cache.GetOrCreateAsync(
                $"nfp:{ContentHash.Hex(identity)}",
                (Build: build, Mark: () => built = true),
                static (state, _) => { state.Mark(); return ValueTask.FromResult(state.Build().ThrowIfFail()); },
                Tuned,
                cancellationToken: execution).ConfigureAwait(false);
            _ = Interlocked.Increment(ref built ? ref misses : ref hits);
            return Fin.Succ(polygon);
        }).Run().Bind(static inner => inner).ConfigureAwait(false);
}

internal static class PairTable {
    public static async ValueTask<Fin<HashMap<UInt128, NoFitPolygon>>> Build(
        HashMap<(int PartId, long Angle, bool Mirrored), Variant> variants,
        Seq<Stock> inventory,
        NestPolicy policy,
        Option<PairMemo> memo = default,
        CancellationToken cancel = default) {
        Variant[] rows = variants.Values.OrderBy(static row => row.PartId).ThenBy(static row => row.Rotation)
            .ThenBy(static row => row.Mirrored).ToArray();
        return (await Try.lift(async execution => Fin.Succ(await memo.Match(
                    Some: cache => Cached(rows, policy, cache, execution),
                    None: () => ValueTask.FromResult(Parallel(rows, policy))).ConfigureAwait(false))).Run().Bind(static inner => inner).ConfigureAwait(false))
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
                    let method = Nest.MethodOf(inset, row.variant.Collision)
                    from admitted in locus.IsEmpty
                        ? Fin.Succ(Option<NoFitPolygon>.None)
                        : Admit(locus, identity, new NfpWitness(identity, row.stock.Identity, row.variant.Identity,
                            NfpRelation.Admitted, method, method.Exact ? 0.0 : policy.ChordError, policy.Clearance,
                            policy.Kerf, locus.Count, locus.Count(static loop => loop.Winding() == Sign.Negative)))
                            .Map(Some)
                    select admitted)).As()
            .Map(static rows => rows.Somes());

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

    public Seq<PlacementConstraint> Constraints => constraints;

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
-->

(none)
