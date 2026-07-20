# [RASM_FABRICATION_NFP]

`Nest` owns true-shape placement over heterogeneous material stock. One admitted `NestPolicy` compiles each search case into the shared candidate algebra, `NoFitPolygon` retains complete configuration-space topology, and exact arc-space collision and containment gate every emitted transform.

`Nest.Solve` preserves the process seam, `Nest.Charts` preserves the atlas projection, and `NestPlan` remains the rectangular hand-off. `Stock.FromRemnant` re-enters the same inventory union, while `FabricationResult.Placement` carries the process projection.

## [01]-[INDEX]

- [01]-[DOMAIN]: Generated owners admit stock, part rules, constraints, search policy, and NFP evidence.
- [02]-[CONFIGURATION_SPACE]: Arc-witnessed Minkowski proposals, exact feasibility, and parallel pair materialization.
- [03]-[SEARCH]: Constraint closure, candidate generation, parameterized search programs, and stock scheduling.
- [04]-[DELIVERY]: Rectangular-plan honor, remnant minting, placement evidence, and content identity.

## [02]-[DOMAIN]

- Owner: `Stock` closes physical inventory modalities while `StockBody` carries common material, topology, exclusion, piece-and-lot trace, and cost facts.
- Law: one `StockFacts` projection derives common body facts, and one `StockTraits` projection derives physicality, nestability, rectangular extent policy, gauge, and grain; a new modality answers remnant and stock consumers through one case arm.
- Owner: `NestPolicy` admits search, clearance, kerf, edge allowance, objective, candidate, constraint, evaluation budget, and batch policy once.
- Cases: `PlacementMode` carries greedy, beam, evolutionary, annealed, Voronoi, and rectangular programs with case-local evidence; rectangular strategy count and depth remain explicit policy.
- Cases: `PlacementConstraint` carries precedence, grouping, separation, adjacency, containment, stock eligibility, and keep-out facts, each occurrence carrying its own `ConstraintForce`.
- Law: `ConstraintForce.Required` rejects a candidate and fails delivery; `ConstraintForce.Preferred` admits the candidate and rides `NestObjective` as weighted penalty.
- Growth: a stock modality, constraint, candidate source, objective, or search algorithm lands as one case or row consumed by the existing folds.

## [03]-[CONFIGURATION_SPACE]

- Owner: `NoFitPolygon` admits one complete locus with its identity, relation, and approximation witness.
- Cases: `NfpRelation.Forbidden` carries the part-part `MorphologyKind.Sum` locus; `NfpRelation.Admitted` carries the part-stock `MorphologyKind.Difference` inner-fit locus every absolute placement seeds from.
- Cases: `NfpMethod` binds an approximation to its evidence — a chord-projected locus carries positive chord error, an arc-exact locus carries none.
- Law: `PolygonAlgebra.Apply(new PolygonOp.Morphology(...))` proposes line-space candidates; `ArcAlgebra.Apply(new ArcOp.Inspect(...))` decides containment, exclusion, and collision on the original bulged loops.
- Law: pair identity includes canonical loop geometry, tolerance, rotation, clearance, and chord error; inner-fit identity substitutes stock identity and edge allowance.
- Law: collision envelopes carry half the combined clearance and kerf; stock-boundary feasibility adds edge allowance without weakening part-part or exclusion checks.
- Auto: `ParallelHelper.For2D` fills independent pair slots, then `TraverseM` returns the first typed geometry failure without partial cache publication.
- Boundary: an empty pair morphology remains a typed fault, an empty inner-fit locus is the absent-key verdict that no position admits the part, and every returned topology component survives the projection.

## [04]-[SEARCH]

- Owner: `ConstraintGraph` proves precedence acyclic, derives closure for ordering, and precomputes the reduction rank the ordering fold reads.
- Owner: `CandidateSource` composes NFP vertices, inner-fit boundaries, arc-native contacts, stock extrema, and relaxed Voronoi centroids into one slot-keyed frontier; its `Absolute` column decides which rows can seed an empty stock.
- Auto: `PlacementMode.Compile` emits one `SearchProgram`; `SearchOp` folds order, branch, breed, mutate, cool, relax, bound, and select steps over one `SearchState`.
- Auto: `SearchState.Evidence.Evaluated` counts exact decisions across every active run, and `SearchOp.Bounded` halts the stochastic sub-program at `NestPolicy.EvaluationBudget`.
- Auto: rectangular programs delegate every packer and heuristic axis to `StockNest.Pack`; `Nest` contains no second rectangle provider switch.
- Boundary: exact containment, overlap, material, exclusion, and blocking-constraint verdicts gate a candidate before objective ranking.

## [05]-[DELIVERY]

- Entry: `Nest.Solve` admits profiles, inventory, and policy, then dispatches resolved rectangular plans or true-shape search inside the `EngineSpan.NestSolve` span; the run spine passes the `FabricationTap`, defaulting silent for headless callers.
- Entry: `Nest.Charts` admits atlas distortion and reconstructs every island boundary cycle.
- Receipt: `NestEvidence` retains solver, objective, inventory multiplicity, pair witnesses, constraint verdicts, candidate census, unplaced reasons, consumed cost, and the used-to-stock area basis; the settled evidence fires the `FabricationFact.Engine.Of` candidate, evaluated, and rejected rows through `Process/telemetry#FACT_PROJECTION` as kind `engine`.
- Receipt: `FabricationResult.Placement` projects transforms, utilization, unplaced count, remnants, and the evidence-derived content key.
- Law: the content preimage covers every `PartTransform` member including `Instance`, so two placements differing only by instance never collide on one key.
- Boundary: remnant difference uses true profiles and the combined clearance-and-kerf offset; feasibility uses collision envelopes; only consumed stock enters the area and cost denominators.

```csharp signature
extern alias Voronoi;

// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Text;
using CavalierContours.Polyline;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using VPlane = Voronoi::SharpVoronoiLib.VoronoiPlane;
using VSite = Voronoi::SharpVoronoiLib.VoronoiSite;
using BorderEdgeGeneration = Voronoi::SharpVoronoiLib.BorderEdgeGeneration;
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
    public sealed record FreeSpace(int Relaxations, float Strength, int Width) : PlacementMode;
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
    public sealed record Relaxed(int Iterations, float Strength) : SearchOp;
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
        validationError = steps.IsEmpty ? new ValidationError(message: "search program is empty") : null;
}

[SmartEnum<string>]
public sealed partial class NestObjective {
    public static readonly NestObjective Yield = new("yield", ObjectiveWeights.Create(1.0, 0.0, 0.0, 0.0, 1.0));
    public static readonly NestObjective Cut = new("cut", ObjectiveWeights.Create(0.0, 1.0, 0.0, 0.0, 1.0));
    public static readonly NestObjective Remnant = new("remnant", ObjectiveWeights.Create(0.0, 0.0, 1.0, 0.0, 1.0));
    public static readonly NestObjective Cost = new("cost", ObjectiveWeights.Create(0.0, 0.0, 0.0, 1.0, 1.0));
    public static readonly NestObjective Balanced = new("balanced", ObjectiveWeights.Create(1.0, 1.0, 1.0, 1.0, 1.0));

    public ObjectiveWeights Weights { get; }

    public double Score(NestEvidence evidence) {
        double area = Math.Max(evidence.StockArea, double.Epsilon), scale = Math.Sqrt(area);
        return (Weights.Yield * evidence.Utilization) - (Weights.Cut * evidence.CutLength / scale)
            + (Weights.Remnant * evidence.RemnantValue / area) - (Weights.Cost * evidence.StockCost / area)
            - (Weights.Constraint * evidence.ConstraintPenalty);
    }
}

[ComplexValueObject]
public sealed partial class ObjectiveWeights {
    public double Yield { get; }
    public double Cut { get; }
    public double Remnant { get; }
    public double Cost { get; }
    public double Constraint { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double yield, ref double cut,
        ref double remnant, ref double cost, ref double constraint) {
        Seq<double> weights = Seq(yield, cut, remnant, cost, constraint);
        validationError = weights.Exists(static weight => !double.IsFinite(weight) || weight < 0.0)
            || weights.Sum() <= 0.0
                ? new ValidationError(message: "objective weights are outside the admitted domain")
                : null;
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
                ? new ValidationError(message: "candidate weights are outside the admitted domain")
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
        .Bind(span => toSeq(probe.Vertices).Map(point => Projected(point, host.At(span), host.At(span + 1))))
        .Fold(double.PositiveInfinity, Math.Min);

    static double Projected(Point3d point, Point3d from, Point3d to) {
        Vector3d span = to - from, offset = point - from;
        double length = span * span;
        return point.DistanceTo(from + (length <= 0.0 ? Vector3d.Zero
            : Math.Clamp((offset * span) / length, 0.0, 1.0) * span));
    }
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
                ? new ValidationError(message: "part rule is outside the admitted domain")
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
                ? new ValidationError(message: "stock body is outside the admitted domain")
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
    public UInt128 Identity => Nest.Identity(Region, Tolerance, FormattableString.Invariant(
        $"{IdentitySalt()}:{Nest.Identity(Exclusions, Tolerance, "stock-exclusions"):X32}"));

    static bool Positive(double value) => double.IsFinite(value) && value > 0.0;
    static bool Nonnegative(double value) => double.IsFinite(value) && value >= 0.0;
    static bool Axis(Option<double> value) => value.ForAll(double.IsFinite);
    static StockTraits TraitsOf(double area, bool physical, bool nestable,
        RectangularExtentPolicy rectangular, Option<double> gauge, Option<double> grain) =>
        new(physical, physical && nestable && area > 0.0, rectangular, gauge, grain);

    string IdentitySalt() => Switch(
        sheet: static row => FormattableString.Invariant(
            $"sheet:{BodyKey(row.Body)}:{row.Thickness:R}:{row.GrainAxis.IfNone(double.NaN):R}"),
        plate: static row => FormattableString.Invariant(
            $"plate:{BodyKey(row.Body)}:{row.Thickness:R}:{row.GrainAxis.IfNone(double.NaN):R}"),
        roll: static row => FormattableString.Invariant(
            $"roll:{BodyKey(row.Body)}:{row.Width:R}:{row.AvailableLength:R}:{row.GrainAxis.IfNone(double.NaN):R}"),
        coil: static row => FormattableString.Invariant(
            $"coil:{BodyKey(row.Body)}:{row.Width:R}:{row.AvailableLength:R}:{row.Thickness:R}:{row.GrainAxis.IfNone(double.NaN):R}"),
        barStock: static row => FormattableString.Invariant(
            $"bar:{BodyKey(row.Body)}:{row.Diameter:R}:{row.Length:R}:{row.EndAllowance:R}"),
        tubeStock: static row => FormattableString.Invariant(
            $"tube:{BodyKey(row.Body)}:{row.OuterDiameter:R}:{row.WallThickness:R}:{row.Length:R}:{row.SeamAllowance:R}:{row.EndAllowance:R}"),
        billet: static row => FormattableString.Invariant($"billet:{BodyKey(row.Body)}:{row.Depth:R}"),
        filament: static row => FormattableString.Invariant(
            $"filament:{BodyKey(row.Body)}:{row.Diameter:R}:{row.SpoolLength:R}"),
        fromRemnant: static row => FormattableString.Invariant(
            $"remnant:{row.Remnant.Identity:X32}:{row.Remnant.Profile.GaugeMm.IfNone(double.NaN):R}:"
            + $"{row.Remnant.Profile.GrainAxisRadians.IfNone(double.NaN):R}:{row.Remnant.Profile.CostPerSquareMillimeter.IfNone(double.NaN):R}"));

    static string BodyKey(StockBody body) {
        string material = body.Material.Value, heat = body.Heat.IfNone(string.Empty);
        return FormattableString.Invariant(
            $"{material.Length}:{material}:{body.Piece.Length}:{body.Piece}:{body.Lot.Length}:{body.Lot}:{heat.Length}:{heat}:{body.Cost:R}");
    }
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
    public int EvaluationBudget { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref PlacementMode mode, ref Seq<PartRule> parts,
        ref Seq<PlacementConstraint> constraints, ref FrozenSet<CandidateSource> candidates, ref CandidateOrder frontier,
        ref NestObjective objective,
        ref double clearance, ref double chordError, ref double kerf, ref double edgeAllowance,
        ref double rectangleResolution, ref int pairBatchFloor, ref int evaluationBudget) =>
        validationError = parts.IsEmpty || parts.GroupBy(static row => row.PartId).Exists(static group => group.Count() != 1)
            || candidates.Count == 0 || !double.IsFinite(clearance) || clearance < 0.0 || !double.IsFinite(chordError)
            || chordError <= 0.0 || !double.IsFinite(kerf) || kerf < 0.0 || !double.IsFinite(edgeAllowance)
            || edgeAllowance < 0.0 || !double.IsFinite(rectangleResolution) || rectangleResolution <= 0.0
            || pairBatchFloor < 1 || evaluationBudget < 1 || !Admits(mode)
            || !candidates.Any(static source => source.Absolute)
                ? new ValidationError(message: "nest policy is outside the admitted domain")
                : null;

    static bool Admits(PlacementMode mode) => mode.Switch(
        bottomLeft: static _ => true,
        beam: static row => row.Width > 0,
        genetic: static row => row.Population > 1 && row.Generations > 0 && row.Mutation is >= 0.0 and <= 1.0,
        annealed: static row => row.Iterations > 0 && row.Width > 1 && double.IsFinite(row.Temperature)
            && row.Temperature > 0.0 && row.Cooling is > 0.0 and < 1.0,
        freeSpace: static row => row.Relaxations >= 0 && row.Strength is >= 0.0f and <= 1.0f && row.Width > 0,
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
                ? new ValidationError(message: "no-fit polygon is outside the admitted domain")
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
public sealed record NestEvidence(PlacementMode Mode, NestObjective Objective, Seq<UInt128> Stock, Seq<NfpWitness> Pairs,
    Seq<ConstraintVerdict> Constraints, Seq<UnplacedReason> Unplaced, int Candidates, int Evaluated, int Rejected,
    double UsedArea, double StockArea, double CutLength, double RemnantValue, double StockCost) {
    public double Utilization => StockArea > 0.0 ? Math.Clamp(UsedArea / StockArea, 0.0, 1.0) : 0.0;
    public double ConstraintPenalty => Constraints.Sum(static row => row.Penalty)
        + Unplaced.Count(static row => row is UnplacedReason.Constraint);
}

public readonly record struct PartInstance(int PartId, int Ordinal);
internal sealed record Variant(int PartId, double Rotation, Loop True, Loop Rotated, Loop Collision, UInt128 Identity);
internal sealed record Candidate(PartInstance Part, int StockSlot, UInt128 Stock, Point3d Point, double Rotation, double Contact);
internal sealed record Placed(PartInstance Instance, Variant Part, Stock Stock, PartTransform Transform, Loop Shape, Loop Envelope);
internal sealed record CandidateRequest(PartInstance Part, Variant Variant, double Angle, Seq<Placed> Placed, Seq<Stock> Inventory,
    HashMap<UInt128, NoFitPolygon> Pairs, NestPolicy Policy, int VoronoiIterations, float VoronoiStrength);
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record PlacementDecision {
    private PlacementDecision() { }
    public sealed record Accepted(Placed Value, Seq<ConstraintVerdict> Constraints) : PlacementDecision;
    public sealed record Rejected(UnplacedReason Reason) : PlacementDecision;
}
internal sealed record Genome(Seq<PartInstance> Order, HashMap<PartInstance, double> Rotation);
internal sealed record SearchRun(Seq<Placed> Placed, Seq<UnplacedReason> Unplaced, Seq<ConstraintVerdict> Constraints,
    int Candidates, int Evaluated, int Rejected);
internal sealed record SearchState(Seq<Genome> Population, Seq<SearchRun> Runs, NestEvidence Evidence,
    ulong Random, double Temperature, int VoronoiIterations, float VoronoiStrength);
internal sealed record NestReceipt(Seq<PartTransform> Placements, Seq<Remnant> Remnants, NestEvidence Evidence, ContentKey Key);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Nest {
    public static Fin<FabricationResult> Solve(FabricationPolicy.Nest policy, FabricationInput input, FabricationTap? tap = null) =>
        EngineSpan.NestSolve.Traced(_ =>
            from parts in input.Profiles.IsEmpty
                ? Fin.Fail<Arr<Loop>>(FabricationFault.Nest(NestFault.EmptyCutList, 0).ToError())
                : input.Profiles.ToSeq().Map((loop, index) => (loop, index))
                    .TraverseM(row => Admit(row.loop, row.index)).As().Map(static rows => rows.ToArr())
            let port = tap ?? FabricationTap.Silent
            from result in input.Plan.Match(
                Some: plan => Honor(parts, plan),
                None: () => input.Inventory.IsEmpty
                    ? Fin.Fail<FabricationResult>(FabricationFault.StockOverflow(parts.Count, 0).ToError())
                    : input.Inventory.Filter(static stock => stock.Nestable && !stock.Region.IsEmpty) is Seq<Stock> inventory
                        && !inventory.IsEmpty
                        ? Place(parts, inventory, policy.Nesting).Map(receipt => Projected(receipt, port))
                        : Fin.Fail<FabricationResult>(FabricationFault.StockOverflow(parts.Count, 0).ToError()))
            select result);

    // Engine rows fire where the evidence settles, so the true-shape search reports its census once and
    // the honored-plan path, which searches nothing, stays fact-free.
    private static FabricationResult Projected(NestReceipt receipt, FabricationTap tap) =>
        (FabricationFact.Engine.Of(receipt.Evidence).Map(tap.Fire).Strict(), Project(receipt)).Item2;

    public static Fin<Arr<Loop>> Charts(ChartAtlas atlas, double maxAreaStretch, Context tolerance) =>
        !double.IsFinite(maxAreaStretch) || maxAreaStretch < 1.0 || !atlas.Receipt.FlipFreeBijective
        || atlas.Receipt.MaxArea > maxAreaStretch || atlas.Receipt.MinArea < 1.0 / maxAreaStretch
            ? Fin.Fail<Arr<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "atlas:distortion").ToError())
            : atlas.Islands.TraverseM(island => Boundaries(island, tolerance)).As()
                .Map(static regions => regions.Bind(static loops => loops).ToArr());

    internal static Fin<FabricationResult> Honor(Arr<Loop> parts, NestPlan plan) =>
        plan.Placements.TraverseM(row =>
            row.PartId < 0 || row.PartId >= parts.Count
                ? Fin.Fail<PartTransform>(FabricationFault.NoFit(row.PartId, Seq<double>()).ToError())
                : Rotated(parts[row.PartId], row.RotationRadians).Bind(part =>
                    PartTransform.Admit(row.PartId, row.Instance, row.XMm - part.Bound().Min.X,
                        row.YMm - part.Bound().Min.Y, row.RotationRadians, row.SheetIndex, mirrored: false))).As()
            .Bind(placed => placed.IsEmpty
                ? Fin.Fail<FabricationResult>(FabricationFault.StockOverflow(parts.Count, plan.Yield.SheetCount).ToError())
                : Fin.Succ((FabricationResult)new FabricationResult.Placement(placed, plan.Yield.UtilizationRatio,
                    plan.Yield.UnplacedCount, Seq<Remnant>(), KeyOf(placed, Seq<Remnant>(), plan.Evidence.Digest))));

    internal static UInt128 Identity(Seq<Loop> loops, Context tolerance, string salt) {
        using ArrayPoolBufferWriter<byte> buffer = new();
        Write(buffer, tolerance.Absolute.Value);
        Write(buffer, tolerance.Angular.Value);
        Write(buffer, salt);
        Write(buffer, loops.Count);
        foreach (Loop loop in loops.OrderByDescending(static row => Math.Abs(row.Area())).ThenBy(static row => row.Count)
            .ThenBy(static row => row.Bound().Min.X).ThenBy(static row => row.Bound().Min.Y)
            .ThenBy(static row => row, Comparer<Loop>.Create(CanonicalCompare))) {
            Write(buffer, loop.Count);
            int start = CanonicalStart(loop);
            foreach (int index in toSeq(Enumerable.Range(0, loop.Count)).Map(offset => (start + offset) % loop.Count)) {
                Point3d point = loop.At(index);
                Write(buffer, point.X); Write(buffer, point.Y); Write(buffer, point.Z); Write(buffer, loop.BulgeAt(index));
            }
        }
        return ContentKey.Of(EgressKind.StockSnapshot, buffer.WrittenSpan).Digest;
    }

    private static int CanonicalCompare(Loop left, Loop right) {
        int leftStart = CanonicalStart(left), rightStart = CanonicalStart(right);
        for (int offset = 0; offset < left.Count; offset++) {
            int order = VertexCompare(left, (leftStart + offset) % left.Count, right, (rightStart + offset) % right.Count);
            if (order != 0) return order;
        }
        return 0;
    }

    private static int CanonicalStart(Loop loop) {
        int start = 0;
        for (int index = 1; index < loop.Count; index++)
            if (VertexCompare(loop, index, loop, start) < 0) start = index;
        return start;
    }

    private static int VertexCompare(Loop left, int leftIndex, Loop right, int rightIndex) {
        Point3d a = left.At(leftIndex), b = right.At(rightIndex);
        foreach (int order in new[] { ScalarCompare(a.X, b.X), ScalarCompare(a.Y, b.Y), ScalarCompare(a.Z, b.Z),
                     ScalarCompare(left.BulgeAt(leftIndex), right.BulgeAt(rightIndex)) })
            if (order != 0) return order;
        return 0;
    }

    private static int ScalarCompare(double left, double right) {
        int order = left.CompareTo(right);
        return order != 0 ? order : BitConverter.DoubleToInt64Bits(left).CompareTo(BitConverter.DoubleToInt64Bits(right));
    }

    static Fin<NestReceipt> Place(Arr<Loop> parts, Seq<Stock> inventory, NestPolicy policy) =>
        from _ in policy.Parts.Count != parts.Count
            || policy.Parts.Exists(rule => rule.PartId < 0 || rule.PartId >= parts.Count)
            ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:part-rule-profile").ToError())
            : Fin.Succ(unit)
        from graph in ConstraintGraph.Admit(parts.Count, policy.Constraints)
        from variants in Variants(parts, policy)
        from pairs in PairTable.Build(variants, inventory, policy)
        from admitted in Initial(inventory, policy, graph)
        let initial = admitted with { Evidence = admitted.Evidence with { Pairs = pairs.Values.Map(static row => row.Witness).ToSeq() } }
        from searched in policy.Mode.Compile(policy.EvaluationBudget).Steps.FoldM<Fin, SearchState>(initial,
            (state, operation) => Apply(operation, state, parts, inventory, variants, pairs, policy, graph)).As()
        from receipt in Deliver(searched, parts, inventory, policy, graph)
        select receipt;

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
            rectangular: static (scope, row) => NestRun.FromProfiles(scope.parts, scope.inventory, scope.policy.Parts, row.Strategy,
                row.StrategyBudget, row.StrategyDepth, row.StockLimit, row.OrientationBudget,
                scope.policy.PairBatchFloor, scope.policy.RectangleResolution, scope.policy.Kerf, scope.policy.EdgeAllowance)
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
                : Candidates(part, genome, run.Placed, inventory, variants, pairs, policy,
                    state.VoronoiIterations, state.VoronoiStrength)
                    .Map(rows => rows.OrderBy((policy.Mode is PlacementMode.BottomLeft
                        ? CandidateOrder.BottomLeft
                        : policy.Frontier).Rank).ToSeq())
                    .Map(rows => (Candidates: rows.Count,
                        Rows: rows.Take(policy.EvaluationBudget - expanded.Evaluated).ToSeq()))
                    .Bind(result => result.Rows.TraverseM(candidate => Exact(
                        candidate, run.Placed, inventory, variants, policy, graph)).As()
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
                    .Map(result => (expanded.Runs.Concat(result.Runs), result.Evaluated))).As()
        .Map(branches => (branches.Runs
            .OrderByDescending(run => policy.Objective.Score(Evidence(run, state.Evidence)))
            .Take(width).ToSeq(), branches.Evaluated));

    static Fin<PlacementDecision> Exact(Candidate candidate, Seq<Placed> placed, Seq<Stock> inventory,
        HashMap<(int PartId, long Angle), Variant> variants, NestPolicy policy, ConstraintGraph graph) =>
        candidate.StockSlot >= 0 && candidate.StockSlot < inventory.Count
        && inventory[candidate.StockSlot].Identity == candidate.Stock
            ? variants.Find((candidate.Part.PartId, BitConverter.DoubleToInt64Bits(candidate.Rotation)))
                .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:variant-key").ToError())
                .Map(found => (Stock: inventory[candidate.StockSlot], Index: candidate.StockSlot, Variant: found))
            .Bind(scope =>
                from transform in PartTransform.Admit(scope.Variant.PartId, candidate.Part.Ordinal,
                    candidate.Point.X, candidate.Point.Y, candidate.Rotation, scope.Index, mirrored: false)
                from shape in transform.Apply(scope.Variant.True)
                from envelopeTransform in PartTransform.Admit(scope.Variant.PartId, candidate.Part.Ordinal,
                    candidate.Point.X, candidate.Point.Y, rotationRadians: 0.0, sheetIndex: scope.Index, mirrored: false)
                from envelope in envelopeTransform.Apply(scope.Variant.Collision)
                from boundaryEnvelope in ArcShapeOffset(Seq(envelope), policy.EdgeAllowance).Bind(rows => rows.Count == 1
                    ? rows.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:edge-envelope-empty").ToError())
                    : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:edge-envelope-topology").ToError()))
                from stockRelations in scope.Stock.Region.TraverseM(region => Relate(region, boundaryEnvelope)
                    .Map(relation => (region, relation))).As()
                from overlaps in placed.TraverseM(row => Relate(row.Envelope, envelope).Map(relation => (row, relation))).As()
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
            : Fin.Fail<PlacementDecision>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:stock-slot").ToError());

    static Fin<Seq<Candidate>> Candidates(PartInstance part, Genome genome, Seq<Placed> placed, Seq<Stock> inventory,
        HashMap<(int PartId, long Angle), Variant> variants, HashMap<UInt128, NoFitPolygon> pairs,
        NestPolicy policy, int voronoiIterations, float voronoiStrength) =>
        genome.Rotation.Find(part).ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:rotation-key").ToError()).Bind(angle =>
            variants.Find((part.PartId, BitConverter.DoubleToInt64Bits(angle)))
                .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:variant-key").ToError())
                .Bind(variant => policy.Candidates.OrderBy(static source => source.Key).ToSeq()
                    .TraverseM(source => source.Generate(new CandidateRequest(part, variant, angle, placed, inventory,
                        pairs, policy, voronoiIterations, voronoiStrength))).As())
                .Map(static rows => rows.Bind(identity)
                    .DistinctBy(static row => (row.StockSlot, row.Point.X, row.Point.Y, row.Rotation)).ToSeq()));

    internal static Fin<Seq<Candidate>> VoronoiCandidates(PartInstance part, Variant variant, double angle,
        Seq<Placed> placed, Seq<Stock> inventory, int iterations, float strength) =>
        Try.lift<Fin<Seq<Candidate>>>(f: () => inventory.Map((stock, slot) => (stock, slot)).TraverseM(row => {
            BoundingBox box = new(row.stock.Region.Bind(static loop => loop.Vertices));
            Seq<Point3d> points = row.stock.Region.Bind(static loop => toSeq(loop.Vertices))
                .Concat(placed.Filter(placedRow => placedRow.Transform.SheetIndex == row.slot)
                    .Bind(static placedRow => toSeq(placedRow.Shape.Vertices)))
                .DistinctBy(static point => (point.X, point.Y)).ToSeq();
            if (points.Count < 2) return Fin.Succ(Seq<Candidate>());
            VPlane plane = new(box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);
            plane.SetSites(points.Map(static point => new VSite(point.X, point.Y)).ToList());
            plane.Tessellate(BorderEdgeGeneration.MakeBorderEdges);
            plane.Relax(iterations, strength, reTessellate: true);
            Vector3d anchor = variant.Rotated.Bound().Center - Point3d.Origin;
            return points.Count - plane.DuplicateCount >= 2
                ? Fin.Succ(toSeq(plane.Sites).Filter(static site => site.Closed)
                    .Map(site => new Candidate(part, row.slot, row.stock.Identity,
                        new Point3d(site.Centroid.X, site.Centroid.Y, 0.0) - anchor, angle, 0.0)))
                : Fin.Succ(Seq<Candidate>());
        }).As().Map(static rows => rows.Bind(identity))).Run()
            .MapFail(error => new GeometryFault.DegenerateInput(Kind.Polyline, -1, $"nest:voronoi:{error.Message}").ToError())
            .Bind(static result => result);

    static Fin<SearchState> Relax(SearchState state, int iterations, float strength) =>
        iterations < 0 || strength is < 0.0f or > 1.0f
            ? Fin.Fail<SearchState>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:relax-policy").ToError())
            : Fin.Succ(state with { VoronoiIterations = iterations, VoronoiStrength = strength });

    static SearchState Seed(SearchState state, Seq<PartRule> rules, int population, int seed) {
        Seq<PartInstance> canonical = rules.OrderByDescending(static row => row.Priority)
            .Bind(row => toSeq(Enumerable.Range(0, row.Quantity)).Map(ordinal => new PartInstance(row.PartId, ordinal))).ToSeq();
        Seq<Genome> genomes = toSeq(Enumerable.Range(0, population)).Map(index => new Genome(
            index == 0 ? canonical : canonical.OrderBy(part => Mix((ulong)(uint)(seed + index),
                ((ulong)(uint)part.PartId << 32) | (uint)part.Ordinal)).ToSeq(),
            toHashMap(canonical.Map(part => (part, rules.Find(row => row.PartId == part.PartId).Map(row =>
                row.Angles[(int)(Mix((ulong)(uint)(seed + index),
                    ((ulong)(uint)part.PartId << 32) | (uint)part.Ordinal) % (ulong)row.Angles.Count)]).IfNone(0.0))))));
        return state with { Population = genomes, Random = (ulong)(uint)seed };
    }

    static SearchState Breed(SearchState state) => state with {
        Population = state.Population.Zip(state.Population.Rev(), static (left, right) => left with {
            Order = left.Order.Map((part, index) => (index & 1) == 0 ? part : right.Order[index]).Distinct().Concat(left.Order).Distinct().ToSeq(),
        }),
    };

    static SearchState Mutate(SearchState state, double rate, Seq<PartRule> rules) => state with {
        Population = state.Population.Map((genome, index) => {
            ulong draw = Mix(state.Random + (ulong)index, (ulong)genome.Order.Count);
            if ((draw / (double)ulong.MaxValue) >= rate || genome.Order.Count < 2) return genome;
            int left = (int)(draw % (ulong)genome.Order.Count), right = (int)(Mix(draw, 1) % (ulong)genome.Order.Count);
            Seq<PartInstance> order = genome.Order.Map((part, at) => at == left ? genome.Order[right] : at == right ? genome.Order[left] : part);
            double rotation = rules.Find(row => row.PartId == order[left].PartId)
                .Map(rule => rule.Angles[(int)(Mix(draw, 2) % (ulong)rule.Angles.Count)]).IfNone(0.0);
            return genome with { Order = order, Rotation = genome.Rotation.SetItem(order[left], rotation) };
        }), Random = Mix(state.Random, (ulong)state.Population.Count),
    };

    static SearchState Cool(SearchState state, double temperature, double factor, NestObjective objective) {
        if (state.Runs.IsEmpty) return state;
        double next = state.Temperature > 0.0 ? state.Temperature * factor : temperature;
        Seq<(SearchRun Run, double Score)> ranked = state.Runs
            .Map(run => (run, objective.Score(Evidence(run, state.Evidence))))
            .OrderByDescending(static row => row.Item2).ToSeq();
        return ranked.Head.Match(
            Some: best => {
                (Seq<SearchRun> Rows, ulong Random) accepted = ranked.Map((row, index) => (row, index))
                    .Fold((Rows: Seq<SearchRun>(), Random: state.Random), (choice, row) => {
                        ulong draw = Mix(choice.Random, (ulong)(row.index + 1));
                        double probability = Math.Exp(Math.Clamp(
                            (row.row.Score - best.Score) / Math.Max(next, double.Epsilon), -700.0, 0.0));
                        return (row.index == 0 || draw / (double)ulong.MaxValue <= probability
                            ? choice.Rows.Add(row.row.Run)
                            : choice.Rows, draw);
                    });
                return state with { Runs = accepted.Rows, Random = accepted.Random, Temperature = next };
            },
            None: () => state);
    }

    static SearchState Select(SearchState state, NestObjective objective, int width) => state with {
        Runs = state.Runs.OrderByDescending(run => objective.Score(Evidence(run, state.Evidence))).Take(width).ToSeq(),
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
        return Fin.Succ(new SearchState(Seq(genome), Seq<SearchRun>(), evidence, 0, 0.0, 1, 1.0f));
    }

    static Fin<SearchState> FromPlan(SearchState state, Arr<Loop> parts, NestPlan plan) =>
        plan.Placements.TraverseM(row => Rotated(parts[row.PartId], row.RotationRadians).Bind(shape =>
            PartTransform.Admit(row.PartId, row.Instance, row.XMm - shape.Bound().Min.X, row.YMm - shape.Bound().Min.Y,
                row.RotationRadians, row.SheetIndex, mirrored: false).Map(transform => FromPlanPlacement(row, shape, transform, plan.Stock[row.SheetIndex])))).As()
            .Map(rows => state with { Runs = Seq(new SearchRun(rows, plan.Run.Parts
                .Filter(part => !rows.Exists(placed => placed.Instance == new PartInstance(part.PartId, part.Instance)))
                .Map<UnplacedReason>(static part => new UnplacedReason.Capacity(part.PartId, part.Instance)),
                state.Evidence.Constraints, plan.Yield.RequestedCount, plan.Yield.RequestedCount, plan.Yield.UnplacedCount)), Evidence = state.Evidence with {
                Evaluated = plan.Yield.RequestedCount,
                UsedArea = rows.Sum(static row => Math.Abs(row.Shape.Area())),
                StockArea = plan.Yield.StockAreaMm2,
            }});

    static Placed FromPlanPlacement(NestPlacement row, Loop shape, PartTransform transform, Stock stock) {
        Variant variant = new(row.PartId, row.RotationRadians, shape, shape, shape,
            Identity(Seq(shape), shape.Tolerance, $"plan:{row.PartId}:{row.Instance}"));
        return new Placed(new PartInstance(row.PartId, row.Instance), variant, stock, transform, shape, shape);
    }

    static Fin<NestReceipt> Deliver(SearchState state, Arr<Loop> parts, Seq<Stock> inventory, NestPolicy policy, ConstraintGraph graph) =>
        state.Runs.OrderByDescending(run => policy.Objective.Score(Evidence(run, state.Evidence))).Head
            .ToFin(FabricationFault.StockOverflow(parts.Count, inventory.Count).ToError())
            .Bind(best => best.Placed.IsEmpty
                ? Fin.Fail<NestReceipt>(FabricationFault.StockOverflow(parts.Count, inventory.Count).ToError())
                : best.Placed.TraverseM(row => row.Transform.Apply(parts[row.Part.PartId])).As().Bind(_ => graph.Verdicts(best.Placed).Bind(verdicts =>
                verdicts.Exists(static verdict => verdict.Blocking)
                    ? Fin.Fail<NestReceipt>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:constraint-verdict").ToError())
                    : best.Placed.GroupBy(static row => row.Transform.SheetIndex).ToSeq().Map(static group => toSeq(group))
                        .TraverseM(rows => rows.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:stock-group").ToError())
                            .Bind(head => Remnant.From(head.Stock, rows.Map(static row => row.Shape),
                                policy.Clearance + policy.Kerf))).As()
                    .Map(remnants => remnants.Bind(identity))
                    .Map(remnants => {
                        NestEvidence evidence = Evidence(best, state.Evidence) with { Constraints = verdicts,
                            RemnantValue = remnants.Sum(static row => Math.Abs(row.Region.Sum(static loop => loop.Area()))) };
                        Seq<PartTransform> transforms = best.Placed.Map(static row => row.Transform);
                        return new NestReceipt(transforms, remnants, evidence, KeyOf(transforms, remnants, Digest(evidence)));
                    }))));

    static FabricationResult Project(NestReceipt receipt) => new FabricationResult.Placement(receipt.Placements,
        receipt.Evidence.Utilization, receipt.Evidence.Unplaced.Count, receipt.Remnants, receipt.Key);

    static NestEvidence Evidence(SearchRun run, NestEvidence basis) => basis with {
        Constraints = run.Constraints,
        Unplaced = run.Unplaced,
        Candidates = run.Candidates,
        Evaluated = Math.Max(basis.Evaluated, run.Evaluated),
        Rejected = run.Rejected,
        UsedArea = run.Placed.Sum(static row => Math.Abs(row.Shape.Area())),
        StockArea = Consumed(run).Sum(static stock => stock.Area),
        CutLength = run.Placed.Sum(static row => row.Shape.Length()),
        StockCost = Consumed(run).Sum(static stock => stock.Cost),
    };

    static Seq<Stock> Consumed(SearchRun run) => run.Placed.GroupBy(static row => row.Transform.SheetIndex)
        .Choose(static group => toSeq(group).Head.Map(static row => row.Stock)).ToSeq();

    static UInt128 Digest(NestEvidence evidence) {
        using ArrayPoolBufferWriter<byte> buffer = new();
        Write(buffer, ModeKey(evidence.Mode)); Write(buffer, evidence.Objective.Key);
        Write(buffer, evidence.Candidates); Write(buffer, evidence.Evaluated); Write(buffer, evidence.Rejected);
        Write(buffer, evidence.UsedArea); Write(buffer, evidence.StockArea); Write(buffer, evidence.CutLength);
        Write(buffer, evidence.RemnantValue); Write(buffer, evidence.StockCost);
        Write(buffer, evidence.Stock.Count);
        foreach (UInt128 stock in evidence.Stock.Order()) Write(buffer, stock);
        Write(buffer, evidence.Pairs.Count);
        foreach (NfpWitness pair in evidence.Pairs.OrderBy(static row => row.Pair)) {
            Write(buffer, pair.Pair); Write(buffer, pair.Fixed); Write(buffer, pair.Orbiting);
            Write(buffer, pair.Relation.Key); Write(buffer, pair.Method.Key); Write(buffer, pair.ChordError);
            Write(buffer, pair.Clearance); Write(buffer, pair.Kerf); Write(buffer, pair.Components); Write(buffer, pair.Holes);
        }
        Write(buffer, evidence.Constraints.Count);
        foreach (ConstraintVerdict verdict in evidence.Constraints.OrderBy(static row => ConstraintKey(row.Constraint))) {
            Write(buffer, ConstraintKey(verdict.Constraint)); Write(buffer, verdict is ConstraintVerdict.Satisfied ? 1 : 0);
            Write(buffer, verdict.Penalty);
        }
        Write(buffer, evidence.Unplaced.Count);
        foreach (UnplacedReason reason in evidence.Unplaced.OrderBy(ReasonKey)) Write(buffer, ReasonKey(reason));
        return ContentKey.Of(EgressKind.Placement, buffer.WrittenSpan).Digest;
    }

    static string ModeKey(PlacementMode mode) => mode.Switch(
        bottomLeft: static _ => "bottom-left",
        beam: static row => FormattableString.Invariant($"beam:{row.Width}"),
        genetic: static row => FormattableString.Invariant($"genetic:{row.Population}:{row.Generations}:{row.Mutation:R}:{row.Seed}"),
        annealed: static row => FormattableString.Invariant(
            $"annealed:{row.Iterations}:{row.Width}:{row.Temperature:R}:{row.Cooling:R}:{row.Seed}"),
        freeSpace: static row => FormattableString.Invariant($"free-space:{row.Relaxations}:{row.Strength:R}:{row.Width}"),
        rectFastpath: static row => FormattableString.Invariant(
            $"rect:{row.Strategy.Identity}:{row.StrategyBudget}:{row.StrategyDepth}:{row.OrientationBudget}:{row.StockLimit}"));

    static string ConstraintKey(PlacementConstraint rule) => $"{rule.Force.Key}:" + rule.Switch(
        precedes: static row => FormattableString.Invariant($"precedes:{row.Before}:{row.After}"),
        together: static row => $"together:{string.Join(',', row.Parts.Order())}",
        separate: static row => FormattableString.Invariant($"separate:{row.Left}:{row.Right}:{row.Distance:R}:{row.Metric.Key}"),
        adjacent: static row => FormattableString.Invariant(
            $"adjacent:{row.Left}:{row.Right}:{row.MaximumDistance:R}:{row.Metric.Key}"),
        inside: static row => FormattableString.Invariant($"inside:{row.Inner}:{row.Outer}"),
        stockOnly: static row => $"stock:{row.Part}:{string.Join(',', row.Stock.Order().Map(static value => $"{value:X32}"))}",
        keepOut: static row => FormattableString.Invariant(
            $"keep-out:{row.Stock:X32}:{row.Region.Head.Map(loop => Identity(row.Region, loop.Tolerance, "keep-out")).IfNone(UInt128.Zero):X32}"));

    static string ReasonKey(UnplacedReason reason) => reason.Switch(
        material: static row => FormattableString.Invariant($"material:{row.PartId}:{row.Instance}:{row.Stock:X32}"),
        grain: static row => FormattableString.Invariant($"grain:{row.PartId}:{row.Instance}:{row.Stock:X32}"),
        boundary: static row => FormattableString.Invariant($"boundary:{row.PartId}:{row.Instance}:{row.Stock:X32}"),
        collision: static row => FormattableString.Invariant(
            $"collision:{row.PartId}:{row.Instance}:{row.OtherPartId}:{row.OtherInstance}"),
        exclusion: static row => FormattableString.Invariant($"exclusion:{row.PartId}:{row.Instance}:{row.Stock:X32}"),
        constraint: static row => $"constraint:{row.PartId}:{row.Instance}:{ConstraintKey(row.Rule)}",
        budget: static row => FormattableString.Invariant($"budget:{row.PartId}:{row.Instance}:{row.Evaluated}"),
        capacity: static row => FormattableString.Invariant($"capacity:{row.PartId}:{row.Instance}"));

    static ContentKey KeyOf(Seq<PartTransform> placed, Seq<Remnant> remnants, UInt128 evidence) {
        using ArrayPoolBufferWriter<byte> buffer = new();
        Write(buffer, evidence);
        Write(buffer, placed.Count);
        foreach (PartTransform row in placed.OrderBy(static row => row.SheetIndex).ThenBy(static row => row.PartId)
            .ThenBy(static row => row.Instance).ThenBy(static row => row.Tx).ThenBy(static row => row.Ty)
            .ThenBy(static row => row.RotationRadians)) {
            Write(buffer, row.PartId); Write(buffer, row.Instance); Write(buffer, row.SheetIndex);
            Write(buffer, row.Tx); Write(buffer, row.Ty); Write(buffer, row.RotationRadians);
        }
        Write(buffer, remnants.Count);
        foreach (Remnant row in remnants.OrderBy(static row => row.Identity)) Write(buffer, row.Identity);
        return ContentKey.Of(EgressKind.Placement, buffer.WrittenSpan);
    }

    static Fin<Loop> Admit(Loop loop, int index) => !loop.Closed
        ? Fin.Fail<Loop>(FabricationFault.OpenLoop(FabConcern.Nest, index).ToError())
        : loop.Count < 3 || loop.Vertices.Exists(static point => !double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z))
            || loop.Bulges.Exists(static bulge => !double.IsFinite(bulge))
                ? Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, $"nest:profile:{index}").ToError())
                : Fin.Succ(loop.AsCcw());

    static Fin<HashMap<(int PartId, long Angle), Variant>> Variants(Arr<Loop> parts, NestPolicy policy) =>
        policy.Parts.Bind(rule => rule.Angles.Map(angle => (rule.PartId, Angle: angle)))
            .TraverseM(row => Rotated(parts[row.PartId], row.Angle)
                .Bind(shape => ArcShapeOffset(Seq(shape), 0.5 * (policy.Clearance + policy.Kerf)))
                .Bind(collision => collision.Count == 1 ? collision.Head.Match(
                    Some: envelope => Fin.Succ(new Variant(row.PartId, row.Angle, parts[row.PartId], shape, envelope,
                        Identity(collision, parts[row.PartId].Tolerance, FormattableString.Invariant($"{row.PartId}:{row.Angle:R}")))),
                    None: () => Fin.Fail<Variant>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:clearance-topology").ToError()))
                    : Fin.Fail<Variant>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:clearance-topology").ToError()))).As()
            .Map(rows => toHashMap(rows.Map(static row =>
                ((row.PartId, BitConverter.DoubleToInt64Bits(row.Rotation)), row))));

    static Fin<Loop> Rotated(Loop part, double radians) {
        double cosine = Math.Cos(radians), sine = Math.Sin(radians);
        return Loop.Admit(part.Vertices.Map(point => new Point3d(
            (point.X * cosine) - (point.Y * sine), (point.X * sine) + (point.Y * cosine), point.Z)).ToArr(),
            part.Closed, part.Bulges, part.Tolerance);
    }

    internal static Fin<Seq<Loop>> ArcShapeOffset(Seq<Loop> loops, double distance) =>
        loops.Head.ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:arc-offset-empty").ToError())
            .Bind(head => ArcForest.Admit(loops, head.Tolerance, head.Plane).ToFin())
                .Bind(forest => ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Forest(forest), distance)))
            .Bind(static trace => trace is ArcTrace.Forest forest
                ? Fin.Succ(forest.Result.Loops)
                : Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:arc-offset-trace").ToError()));

    internal static Fin<ArcRelation> Relate(Loop first, Loop second) =>
        ArcForest.Admit(Seq(first, second), first.Tolerance, first.Plane).ToFin()
            .Bind(forest => ArcAlgebra.Apply(new ArcOp.Inspect(forest, new ArcProbe.Pair(first, second))))
            .Bind(static trace => trace is ArcTrace.Inspection { Result: ArcInspection.Pair pair }
                ? Fin.Succ(pair.Relation)
                : Fin.Fail<ArcRelation>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:arc-relation-trace").ToError()));

    internal static Fin<Loop> Lower(Loop loop, double error) => ArcAlgebra.Densify(new ArcProjection.Lower(loop, error))
        .Bind(static trace => trace.DensifiedReceipt.Map(static receipt => receipt.Result)
            .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:arc-projection-trace").ToError()));

    static bool MaterialAccepted(Candidate candidate, Stock stock, Seq<PartRule> rules) => rules
        .Find(rule => rule.PartId == candidate.Part.PartId).ForAll(rule => rule.Material.ForAll(material => material == stock.Material));

    static bool GrainAccepted(Candidate candidate, Stock stock, Seq<PartRule> rules) => rules
        .Find(rule => rule.PartId == candidate.Part.PartId).ForAll(rule => rule.GrainAxis.ForAll(grain => stock.GrainAxis
            .Exists(axis => Math.Abs(Math.IEEERemainder((grain + candidate.Rotation) - axis, Math.PI))
                <= stock.Tolerance.Angular.Value)));

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
                    || (p * q) / (p.Length * q.Length) > -1.0 + placed.Envelope.Tolerance.Angular.Value) return None;
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

    static Fin<Seq<Loop>> Boundaries(UvIsland island, Context tolerance) {
        FrozenDictionary<int, int> local = island.Vertices.Map((vertex, index) => (vertex, index))
            .ToFrozenDictionary(static row => row.vertex, static row => row.index);
        Seq<(int A, int B)> edges = island.Faces.Bind(static face => Seq((face.A, face.B), (face.B, face.C), (face.C, face.A)));
        Seq<(int A, int B)> boundary = edges.GroupBy(edge => edge.A < edge.B ? (edge.A, edge.B) : (edge.B, edge.A))
            .Choose(static group => group.Count() == 1 ? group.Head : None);
        return boundary.IsEmpty || boundary.Exists(edge => !local.ContainsKey(edge.A) || !local.ContainsKey(edge.B))
            ? Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "atlas:island-boundary").ToError())
            : Cycles(boundary).Bind(cycles => cycles.TraverseM(cycle => Loop.Admit(cycle.Map(vertex => new Point3d(island.Uv[local[vertex]].X,
                island.Uv[local[vertex]].Y, 0.0)).ToArr(), closed: true, Arr<double>(), tolerance)).As());
    }

    static Fin<Seq<Seq<int>>> Cycles(Seq<(int A, int B)> edges) {
        HashMap<int, int> successor = toHashMap(edges.Map(static edge => (edge.A, edge.B)));
        return successor.Count != edges.Count
            ? Fin.Fail<Seq<Seq<int>>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "atlas:branching-boundary").ToError())
            : edges.Map(static edge => edge.A).FoldM<Fin, (Set<int> Seen, Seq<Seq<int>> Cycles)>(
                (Seen: Set<int>(), Cycles: Seq<Seq<int>>()), (state, seed) => state.Seen.Contains(seed)
                    ? Fin.Succ(state)
                    : Walk(successor, seed).Map(cycle => (cycle.Fold(state.Seen, static (seen, vertex) => seen.Add(vertex)),
                        state.Cycles.Add(cycle)))).As().Map(static state => state.Cycles);
    }

    static Fin<Seq<int>> Walk(HashMap<int, int> successor, int seed) =>
        toSeq(Enumerable.Range(0, successor.Count)).FoldM<Fin, (Seq<int> Path, int At, bool Closed)>(
            (Path: Seq(seed), At: seed, Closed: false), (state, _) => state.Closed
                ? Fin.Succ(state)
                : successor.Find(state.At).ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "atlas:open-boundary").ToError())
                    .Map(step => step == seed ? state with { Closed = true } : (state.Path.Add(step), step, false))).As()
            .Bind(static state => state.Closed
                ? Fin.Succ(state.Path)
                : Fin.Fail<Seq<int>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "atlas:open-boundary").ToError()));

    static ulong Mix(ulong state, ulong value) {
        ulong mixed = state ^ (value + 0x9E3779B97F4A7C15UL + (state << 6) + (state >> 2));
        mixed ^= mixed >> 30; mixed *= 0xBF58476D1CE4E5B9UL; mixed ^= mixed >> 27; mixed *= 0x94D049BB133111EBUL;
        return mixed ^ (mixed >> 31);
    }

    static void Write(ArrayPoolBufferWriter<byte> buffer, int value) { Span<byte> slot = buffer.GetSpan(sizeof(int)); BinaryPrimitives.WriteInt32LittleEndian(slot, value); buffer.Advance(sizeof(int)); }
    static void Write(ArrayPoolBufferWriter<byte> buffer, double value) { Span<byte> slot = buffer.GetSpan(sizeof(double)); BinaryPrimitives.WriteDoubleLittleEndian(slot, value); buffer.Advance(sizeof(double)); }
    static void Write(ArrayPoolBufferWriter<byte> buffer, UInt128 value) { Span<byte> slot = buffer.GetSpan(16); BinaryPrimitives.WriteUInt128LittleEndian(slot, value); buffer.Advance(16); }
    static void Write(ArrayPoolBufferWriter<byte> buffer, string value) { int length = Encoding.UTF8.GetByteCount(value); Write(buffer, length);
        Span<byte> slot = buffer.GetSpan(length); _ = Encoding.UTF8.GetBytes(value, slot); buffer.Advance(length); }
}

// --- [CONFIGURATION_SPACE] -------------------------------------------------------------------------------------------------------------------------
internal static class PairTable {
    public static Fin<HashMap<UInt128, NoFitPolygon>> Build(HashMap<(int PartId, long Angle), Variant> variants,
        Seq<Stock> inventory, NestPolicy policy) {
        Variant[] rows = variants.Values.OrderBy(static row => row.PartId).ThenBy(static row => row.Rotation).ToArray();
        Fin<NoFitPolygon>[] results = new Fin<NoFitPolygon>[checked(rows.Length * rows.Length)];
        PairAction action = new(rows, results, policy);
        ParallelHelper.For2D(0..rows.Length, 0..rows.Length, in action, policy.PairBatchFloor);
        return results.ToSeq().TraverseM(identity).As()
            .Bind(pairs => Inner(toSeq(rows), inventory, policy).Map(inner => pairs.Concat(inner)))
            .Map(static found => toHashMap(found.DistinctBy(static row => row.Identity)
                .Map(static row => (row.Identity, row))));
    }

    public static UInt128 Key(Variant fixedPart, Variant orbiting, NestPolicy policy) =>
        Nest.Identity(Seq(fixedPart.Collision, orbiting.Collision), fixedPart.Collision.Tolerance,
            FormattableString.Invariant($"{NfpRelation.Forbidden.Key}:{fixedPart.Identity:X32}:{orbiting.Identity:X32}"
                + $":{policy.ChordError:R}:{policy.Clearance:R}:{policy.Kerf:R}"));

    public static UInt128 InnerKey(Variant orbiting, Stock stock, NestPolicy policy) =>
        Nest.Identity(Seq(orbiting.Collision), orbiting.Collision.Tolerance,
            FormattableString.Invariant($"{NfpRelation.Admitted.Key}:{stock.Identity:X32}:{orbiting.Identity:X32}"
                + $":{policy.ChordError:R}:{policy.EdgeAllowance:R}"));

    // Inner-fit erodes the stock outer boundary only; interior holes and exclusions stay on the exact arc gate in Nest.Exact.
    static Fin<Seq<NoFitPolygon>> Inner(Seq<Variant> variants, Seq<Stock> inventory, NestPolicy policy) =>
        variants.Bind(variant => inventory.Map(stock => (variant, stock)))
            .TraverseM(row => row.stock.Region.Filter(static loop => loop.Winding() == Sign.Positive).Head
                .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:inner-fit-boundary").ToError())
                .Bind(outer =>
                    from bounded in Nest.Lower(outer, policy.ChordError)
                    from inset in policy.EdgeAllowance > 0.0
                        ? Nest.ArcShapeOffset(Seq(bounded), -policy.EdgeAllowance).Bind(static rows => rows.Head
                            .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:inner-fit-allowance").ToError()))
                        : Fin.Succ(bounded)
                    from orbitDense in Nest.Lower(row.variant.Collision, policy.ChordError)
                    from trace in PolygonAlgebra.Apply(new PolygonOp.Morphology(inset, orbitDense,
                        NfpRelation.Admitted.Kind))
                    from locus in trace is PolygonTrace.Regions regions
                        ? Fin.Succ(regions.Result.Nodes.Map(static node => node.Boundary))
                        : Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:morphology-trace").ToError())
                    let identity = InnerKey(row.variant, row.stock, policy)
                    from admitted in locus.IsEmpty
                        ? Fin.Succ(Option<NoFitPolygon>.None)
                        : Admit(locus, identity, new NfpWitness(identity, row.stock.Identity, row.variant.Identity,
                            NfpRelation.Admitted, NfpMethod.ChordProjected, policy.ChordError, policy.Clearance,
                            policy.Kerf, locus.Count, locus.Count(static loop => loop.Winding() == Sign.Negative)))
                            .Map(Some)
                    select admitted)).As()
            .Map(static rows => rows.Choose(identity).ToSeq());

    internal static Fin<NoFitPolygon> Admit(Seq<Loop> locus, UInt128 identity, NfpWitness witness) {
        ValidationError? error = NoFitPolygon.Validate(locus, identity, witness, out NoFitPolygon? polygon);
        return error is null && polygon is not null
            ? Fin.Succ(polygon)
            : Fin.Fail<NoFitPolygon>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, $"nest:nfp:{error?.Message}").ToError());
    }

    readonly struct PairAction(Variant[] variants, Fin<NoFitPolygon>[] results, NestPolicy policy) : IAction2D {
        public void Invoke(int i, int j) {
            Variant fixedPart = variants[i], orbiting = variants[j];
            results[(i * variants.Length) + j] =
                from fixedDense in Nest.Lower(fixedPart.Collision, policy.ChordError)
                from orbitDense in Nest.Lower(orbiting.Collision, policy.ChordError)
                from reflected in Reflect(orbitDense)
                from trace in PolygonAlgebra.Apply(new PolygonOp.Morphology(fixedDense, reflected,
                    NfpRelation.Forbidden.Kind))
                from locus in trace is PolygonTrace.Regions regions
                    ? Fin.Succ(regions.Result.Nodes.Map(static node => node.Boundary))
                    : Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:morphology-trace").ToError())
                from admitted in locus.IsEmpty
                    ? Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:nfp-empty").ToError())
                    : Fin.Succ(locus)
                let identity = Key(fixedPart, orbiting, policy)
                let witness = new NfpWitness(identity, fixedPart.Identity, orbiting.Identity, NfpRelation.Forbidden,
                    NfpMethod.ChordProjected, policy.ChordError, policy.Clearance, policy.Kerf, locus.Count,
                    locus.Count(static loop => loop.Winding() == Sign.Negative))
                from polygon in Admit(admitted, identity, witness)
                select polygon;
        }

        static Fin<Loop> Reflect(Loop loop) => Loop.Admit(loop.Vertices.Map(point => Point3d.Origin - (point - Point3d.Origin)).ToArr(),
            loop.Closed, loop.Bulges, loop.Tolerance).Map(static reflected => reflected.AsCcw());
    }
}

internal sealed class ConstraintGraph {
    readonly BidirectionalGraph<int, SEdge<int>> closure;
    readonly HashMap<int, int> rank;
    readonly Seq<PlacementConstraint> constraints;

    ConstraintGraph(BidirectionalGraph<int, SEdge<int>> closure, HashMap<int, int> rank,
        Seq<PlacementConstraint> constraints) => (this.closure, this.rank, this.constraints) = (closure, rank, constraints);

    public static Fin<ConstraintGraph> Admit(int partCount, Seq<PlacementConstraint> constraints) {
        if (partCount < 1 || constraints.Exists(rule => !Valid(rule, partCount)))
            return Fin.Fail<ConstraintGraph>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:constraint-domain").ToError());
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
                toHashMap(toSeq(graph.ComputeTransitiveReduction(static (source, target) => new SEdge<int>(source, target))
                    .TopologicalSort()).Map(static (part, index) => (part, index))),
                constraints))
            : Fin.Fail<ConstraintGraph>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "nest:constraint-cycle").ToError());
    }

    public Seq<PartInstance> Order(Seq<PartInstance> requested, Seq<PartRule> rules) =>
        requested.OrderBy(part => closure.InDegree(part.PartId))
            .ThenByDescending(part => rules.Find(rule => rule.PartId == part.PartId).Map(static rule => rule.Priority).IfNone(0))
            .ThenBy(part => rank.Find(part.PartId).IfNone(int.MaxValue))
            .ThenBy(static part => part.Ordinal).ToSeq();

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
