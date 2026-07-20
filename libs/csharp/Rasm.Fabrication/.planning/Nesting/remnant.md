# [RASM_FABRICATION_REMNANT]

`Remnant` owns one connected, arc-preserving offcut, canonical content identity, material and stock lineage, reusable-region evidence, and the generation carried into the next nesting inventory. Equivalent loop rotations and hole orderings mint one identity, while winding remains part of that identity.

`RemnantInventory` owns one material lane. `RemnantOp` admits stock, reservation, disposition, and physical-census events through one `Reconcile` fold; revision conflicts remain typed receipt evidence, geometry faults remain on `Fin`, and every successful transition retains its before-and-after row.

## [01]-[INDEX]

- [01]-[REMNANT_LIFECYCLE]: owns offcut admission, topology, lineage, policy, optimistic reservation, physical reconciliation, retirement evidence, and stock projection.

## [02]-[REMNANT_LIFECYCLE]

- Owner: `Remnant` is the generated connected-offcut owner; `ReusePolicy` owns admission; `RemnantState`, `RemnantCondition`, `RemnantOp`, `ReservationDisposition`, `ReuseTrait`, `RetireCause`, and `RemnantConflict` close lifecycle behavior and evidence.
- Cases: `RemnantOp` carries `Stocking`, `Claim`, `Close`, and `Sweep`; `ReservationDisposition.Consume` subtracts its used region and stocks each surviving connected child in the same receipt.
- Entry: `Admit(Seq<Loop>, MaterialId, RemnantOrigin, RemnantProfile)` mints each connected component, `Reconcile(RemnantOp, RemnantInventory)` folds lifecycle operations, `From(Stock, Seq<Loop>, double)` inverts consumed stock, `Holds(Seq<Loop>, Option<double>, ReusePolicy)` answers policy-inset fit with grain, and `Stockable(RemnantInventory)` projects the next inventory smallest-adequate first.
- Auto: arc-exact offsets and containment route through `ArcAlgebra.Apply`; chord projection routes through `ArcAlgebra.Densify`; exact measures route through `Loop.Area` and `Loop.Length`; independent row gates partition through `ParallelHelper`; lineage acyclicity and order route through `QuikGraph`; lease membership routes through `Interval.Contains`; content identity routes through `ContentKey.Of` over framed `BinaryPrimitives` writes.
- Receipt: `RemnantPlan` carries the next inventory, admissions, accumulated retirement causes, conflicts, validated transitions, per-source-stock `RemnantYield` rows, and the standing potential, consumed, and scrapped `RemnantMeasure` pairs of area and value.
- Packages: `CommunityToolkit.HighPerformance`, `LanguageExt.Core`, `NodaTime`, `QuikGraph`, `Rasm`, `Rasm.Element`, `RhinoCommon`, and `Thinktecture.Runtime.Extensions` compose the owner.
- Growth: each reuse gate adds one `ReusePolicy` member and one payload-bearing `RetireCause` case; each traceability demand adds one `ReuseTrait` row inside `ReusePolicy.Required`; each lifecycle operation adds one `RemnantOp` case and one generated dispatch arm; each physical observation axis adds one `RemnantObservation` member.
- Boundary: `Remnant.Key` is the lifecycle key, `Stock.FromRemnant` is the next-nest carrier, and `FabricationResult.Placement.Remnants` is the placement receipt seam; `ParallelHelper` slots, mutable `QuikGraph` construction, and pooled canonical-byte writes are statement-bearing package boundaries.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers.Binary;
using System.Linq;
using System.Text;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class RemnantState {
    public static readonly RemnantState Minted = new("minted", terminal: false, static () => Arr(Quarantined, Stocked, Scrapped));
    public static readonly RemnantState Quarantined = new("quarantined", terminal: false, static () => Arr(Stocked, Scrapped));
    public static readonly RemnantState Stocked = new("stocked", terminal: false, static () => Arr(Quarantined, Reserved, Scrapped));
    public static readonly RemnantState Reserved = new("reserved", terminal: false, static () => Arr(Stocked, Quarantined, Consumed, Scrapped));
    public static readonly RemnantState Consumed = new("consumed", terminal: true, static () => Arr<RemnantState>());
    public static readonly RemnantState Scrapped = new("scrapped", terminal: true, static () => Arr<RemnantState>());

    public bool Terminal { get; }

    [UseDelegateFromConstructor]
    public partial Arr<RemnantState> Successors();

    public bool Admits(RemnantState next) => !Terminal && Successors().Contains(next);
}

[SmartEnum<string>]
public sealed partial class RemnantCondition {
    public static readonly RemnantCondition Serviceable = new("serviceable", RemnantState.Stocked,
        static (current, activeLease) => current == RemnantState.Reserved && activeLease
            ? RemnantState.Reserved
            : RemnantState.Stocked);
    public static readonly RemnantCondition Quarantine = new("quarantine", RemnantState.Quarantined,
        static (_, _) => RemnantState.Quarantined);
    public static readonly RemnantCondition Retire = new("retire", RemnantState.Scrapped,
        static (_, _) => RemnantState.Scrapped);

    public RemnantState State { get; }

    [UseDelegateFromConstructor]
    public partial RemnantState Resolve(RemnantState current, bool activeLease);
}

[SmartEnum<string>]
public sealed partial class ReuseTrait {
    public static readonly ReuseTrait Grain = new("grain", static profile => profile.GrainAxisRadians.IsSome);
    public static readonly ReuseTrait Location = new("location", static profile => profile.Location.IsSome);
    public static readonly ReuseTrait Lot = new("lot", static profile => profile.Lot.IsSome);
    public static readonly ReuseTrait Heat = new("heat", static profile => profile.Heat.IsSome);
    public static readonly ReuseTrait Valuation = new("valuation", static profile => profile.CostPerSquareMillimeter.IsSome);

    [UseDelegateFromConstructor]
    public partial bool Carried(RemnantProfile profile);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetireCause {
    private RetireCause() { }

    public sealed record AreaFloor(double ActualMm2, double RequiredMm2) : RetireCause;
    public sealed record FeatureWidth(double RequiredMm) : RetireCause;
    public sealed record SliverAspect(double Actual, double Required) : RetireCause;
    public sealed record Gauge(Option<double> ActualMm, double RequiredMm) : RetireCause;
    public sealed record Generation(int Actual, int Maximum) : RetireCause;
    public sealed record Material(MaterialId Actual, MaterialId Required) : RetireCause;
    public sealed record Duplicate(UInt128 Identity) : RetireCause;
    public sealed record Compactness(double Actual, double Required) : RetireCause;
    public sealed record Observation(Instant LastSeen, Instant RetiredAt) : RetireCause;
    public sealed record Inspection(ContentKey Key) : RetireCause;
    public sealed record Traceability(ReuseTrait Trait) : RetireCause;
    public sealed record Salvage(double Actual, double Required) : RetireCause;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RemnantConflict {
    private RemnantConflict() { }

    public sealed record Kind(ContentKey Key) : RemnantConflict;
    public sealed record Missing(ContentKey Key) : RemnantConflict;
    public sealed record Revision(ContentKey Key, int Expected, int Actual) : RemnantConflict;
    public sealed record State(ContentKey Key, RemnantState Actual) : RemnantConflict;
    public sealed record Lease(ContentKey Key, int Job, Instant At, Option<RemnantLease> Actual) : RemnantConflict;
    public sealed record Claims(ContentKey Key, int Actual, int Maximum) : RemnantConflict;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReservationDisposition {
    private ReservationDisposition() { }

    public sealed record Release : ReservationDisposition;
    public sealed record Consume(Seq<Loop> Used) : ReservationDisposition;
    public sealed record Scrap(RetireCause Cause) : ReservationDisposition;

    public RemnantState Next => Switch(
        release: static _ => RemnantState.Stocked,
        consume: static _ => RemnantState.Consumed,
        scrap: static _ => RemnantState.Scrapped);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RemnantOp {
    private RemnantOp() { }

    public sealed record Stocking(Seq<Remnant> Minted, Instant Now) : RemnantOp;
    public sealed record Claim(ContentKey Key, int Job, int ExpectedRevision, Instant Now) : RemnantOp;
    public sealed record Close(ContentKey Key, int Job, int ExpectedRevision, Instant Now, ReservationDisposition Disposition) : RemnantOp;
    public sealed record Sweep(Seq<RemnantObservation> Observed, Instant Now) : RemnantOp;
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class ReusePolicy {
    public double KerfTrimMm { get; }
    public double RegripMarginMm { get; }
    public double MinUsableAreaMm2 { get; }
    public double MinReusableSpanMm { get; }
    public double MinAspect { get; }
    public double MinCompactness { get; }
    public double MinGaugeMm { get; }
    public double ArcToleranceMm { get; }
    public double MinSalvageValue { get; }
    public double GrainToleranceRadians { get; }
    public int MaxGeneration { get; }
    public int MaxClaims { get; }
    public Duration LeaseDuration { get; }
    public Duration ObservationHorizon { get; }
    public Set<ReuseTrait> Required { get; }

    public double InsetMm => KerfTrimMm + RegripMarginMm;

    public Seq<ReuseTrait> Missing(RemnantProfile profile) =>
        Required.ToSeq().Filter(trait => !trait.Carried(profile));

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double kerfTrimMm,
        ref double regripMarginMm,
        ref double minUsableAreaMm2,
        ref double minReusableSpanMm,
        ref double minAspect,
        ref double minCompactness,
        ref double minGaugeMm,
        ref double arcToleranceMm,
        ref double minSalvageValue,
        ref double grainToleranceRadians,
        ref int maxGeneration,
        ref int maxClaims,
        ref Duration leaseDuration,
        ref Duration observationHorizon,
        ref Set<ReuseTrait> required) {
        double[] scalars = [kerfTrimMm, regripMarginMm, minUsableAreaMm2, minReusableSpanMm, minAspect, minCompactness, minGaugeMm,
            arcToleranceMm, minSalvageValue, grainToleranceRadians];
        if (scalars.Any(static value => !double.IsFinite(value) || value < 0.0) || minAspect > 1.0 || minCompactness > 1.0 || arcToleranceMm <= 0.0
            || grainToleranceRadians >= Math.PI || maxGeneration < 0 || maxClaims < 1
            || leaseDuration <= Duration.Zero || observationHorizon <= Duration.Zero
            || (minSalvageValue > 0.0 && !required.Contains(ReuseTrait.Valuation)))
            validationError = new ValidationError("remnant reuse policy is outside its admitted domain");
    }
}

[ValueObject<string>]
public readonly partial struct RemnantLocation {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) validationError = new ValidationError("remnant location cannot be empty");
    }
}

public sealed record RemnantProfile(
    Option<double> GaugeMm,
    Option<double> GrainAxisRadians,
    Seq<Loop> Exclusions,
    Option<RemnantLocation> Location,
    Option<string> Lot,
    Option<string> Heat,
    Option<double> CostPerSquareMillimeter) {
    public static readonly RemnantProfile Empty = new(None, None, Seq<Loop>(), None, None, None, None);
}

public readonly record struct RemnantOrigin(UInt128 Stock, Option<UInt128> Parent, int Generation);
[ComplexValueObject]
public sealed partial class Remnant {
    public Loop Boundary { get; }
    public Seq<Loop> Holes { get; }
    public MaterialId Material { get; }
    public RemnantOrigin Origin { get; }
    public RemnantProfile Profile { get; }

    public Seq<Loop> Region => Seq(Boundary).Concat(Holes);
    public Option<UInt128> Parent => Origin.Parent;
    public int Generation => Origin.Generation;
    public ContentKey Key => KeyOf(Boundary, Holes, Material, Origin);
    public UInt128 Identity => Key.Digest;
    public double AreaMm2 => Math.Abs(Region.Sum(static loop => loop.Area()));
    public Option<double> Value => Profile.CostPerSquareMillimeter.Map(rate => rate * AreaMm2);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Loop boundary,
        ref Seq<Loop> holes,
        ref MaterialId material,
        ref RemnantOrigin origin,
        ref RemnantProfile profile) {
        Seq<Loop> region = Seq(boundary).Concat(holes);
        bool connected = ComponentsOf(region).Match(
            Succ: static components => components.Count == 1,
            Fail: static _ => false);
        validationError = boundary.Winding() != Sign.Positive || holes.Exists(static hole => hole.Winding() != Sign.Negative)
            || origin.Generation < 0 || !connected
                ? new ValidationError("remnant requires one connected topology and admitted lineage")
                : null;
    }
}

public readonly record struct RemnantLease(int Job, Interval Active);
public sealed record RemnantObservation(ContentKey Key, RemnantCondition Condition, RemnantProfile Profile);
public sealed record RemnantRow(
    Remnant Remnant,
    RemnantState State,
    ContentKey Key,
    Seq<Loop> Usable,
    double UsableAreaMm2,
    int Revision,
    int Claims,
    Instant ObservedAt,
    RemnantCondition Condition,
    RemnantProfile Profile,
    Option<RemnantLease> Lease);

public sealed record RemnantInventory(MaterialId Material, Map<UInt128, RemnantRow> Rows, ReusePolicy Policy) {
    public static RemnantInventory Empty(MaterialId material, ReusePolicy policy) => new(material, Map<UInt128, RemnantRow>(), policy);
}

public sealed record RemnantTransition(RemnantRow Before, RemnantRow After);
public sealed record RemnantRetirement(RemnantRow Row, Seq<RetireCause> Causes);

public readonly record struct RemnantMeasure(double AreaMm2, double Value) {
    public static readonly RemnantMeasure Zero = new(0.0, 0.0);

    public static RemnantMeasure Of(RemnantRow row) => new(row.UsableAreaMm2, row.Remnant.Value.IfNone(0.0));

    public static RemnantMeasure operator +(RemnantMeasure left, RemnantMeasure right) =>
        new(left.AreaMm2 + right.AreaMm2, left.Value + right.Value);
}

public sealed record RemnantYield(UInt128 Stock, int Descendants, int Depth, RemnantMeasure Live, RemnantMeasure Lost);

public sealed record RemnantPlan(
    RemnantInventory Next,
    Seq<RemnantRow> Admitted,
    Seq<RemnantRetirement> Retired,
    Seq<RemnantTransition> Transitions,
    Seq<RemnantConflict> Conflicts,
    Seq<RemnantYield> Yields,
    RemnantMeasure Potential,
    RemnantMeasure Consumed,
    RemnantMeasure Scrapped);

file readonly struct InventoryGate(RemnantInventory inventory, RemnantRow[] rows, Error?[] faults) : IAction {
    public void Invoke(int index) => faults[index] = Remnant.RowFault(rows[index], inventory);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public sealed partial class Remnant {
    public static Fin<Seq<Remnant>> Admit(
        Seq<Loop> region,
        MaterialId material,
        RemnantOrigin origin,
        RemnantProfile profile) =>
        ComponentsOf(region).Map(components => components.Map(component =>
            Mint(component.Outer, component.Holes, material, origin, profile)));

    public static Fin<Seq<Remnant>> From(Stock stock, Seq<Loop> placed, double clearance) =>
        !double.IsFinite(clearance) || clearance < 0.0
            ? Fin.Fail<Seq<Remnant>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:clearance").ToError())
            : from available in Available(stock)
              from remaining in placed.IsEmpty
                  ? Fin.Succ(available)
                  : placed.Traverse(loop => Offset(Seq(loop), 0.5 * clearance).ToValidation())
                      .As().ToFin()
                      .Bind(inflated => Boolean(available, inflated.Bind(static loops => loops), BoolKind.Not))
              from components in ComponentsOf(remaining)
              select components.Map(component =>
                  Mint(component.Outer, component.Holes, stock.Material, Lineage(stock), Profile(stock)));

    private static Fin<Seq<Loop>> Available(Stock stock) => stock.Exclusions.IsEmpty
        ? Fin.Succ(stock.Region)
        : Boolean(stock.Region, stock.Exclusions, BoolKind.Not);

    // Fit is measured against the policy-inset, exclusion-subtracted usable region — never the raw boundary — and
    // a directional lane refuses a remnant whose grain is unknown or misaligned modulo pi.
    public Fin<Option<double>> Holds(Seq<Loop> part, Option<double> grainAxisRadians, ReusePolicy policy) =>
        grainAxisRadians.Exists(demand => !Profile.GrainAxisRadians
            .Exists(carried => Aligned(demand, carried, policy.GrainToleranceRadians)))
            ? Fin.Succ(Option<double>.None)
            : from usable in Usable(this, Profile, policy)
              from outside in Boolean(part, usable, BoolKind.Not)
              from measure in outside.IsEmpty
                  ? Measure(usable, policy.ArcToleranceMm).Map(static value => Some(value))
                  : Fin.Succ(Option<(double Area, double Aspect, double Compactness)>.None)
              select measure.Map(usableMeasure =>
                  usableMeasure.Area - Math.Abs(part.Sum(static loop => loop.Area())));

    private static bool Aligned(double demand, double carried, double tolerance) =>
        Math.Abs(Math.IEEERemainder(demand - carried, Math.PI)) <= tolerance;

    public static Fin<RemnantPlan> Reconcile(RemnantOp op, RemnantInventory inventory) =>
        AdmitInventory(inventory).Bind(_ => op.Switch(
            state: inventory,
            stocking: static (held, request) => Stock(request.Minted, request.Now, held),
            claim: static (held, request) => Claim(request, held),
            close: static (held, request) => Close(request, held),
            sweep: static (held, request) => Sweep(request, held)));

    // Smallest-adequate-first: a nest consuming the least reusable area preserves the large offcuts, and value
    // density breaks ties so an equal-area lane spends the cheapest stock.
    public static Fin<Seq<Stock>> Stockable(RemnantInventory inventory) =>
        AdmitInventory(inventory).Bind(_ => inventory.Rows.Values.ToSeq()
            .Filter(static row => row.State == RemnantState.Stocked && row.Condition == RemnantCondition.Serviceable)
            .OrderBy(static row => row.UsableAreaMm2)
            .ThenBy(static row => row.Remnant.Value.Map(value => value / Math.Max(row.UsableAreaMm2, double.Epsilon)).IfNone(0.0))
            .ThenBy(static row => row.Remnant.Identity)
            .ToSeq()
            .Traverse(row => Usable(row, inventory.Policy)
                .Bind(usable => Admit(
                    usable,
                    row.Remnant.Material,
                    new RemnantOrigin(row.Remnant.Origin.Stock, Some(row.Remnant.Identity), row.Remnant.Generation),
                    row.Profile))
                .Map(remnants => remnants.Map(static remnant => (Stock)new Stock.FromRemnant(remnant))).ToValidation())
            .As().ToFin().Map(static rows => rows.Bind(identity)));

    private static Remnant Mint(Loop boundary, Seq<Loop> holes, MaterialId material, RemnantOrigin origin, RemnantProfile profile) =>
        Create(boundary, holes, material, origin, profile);

    private static RemnantOrigin Lineage(Stock stock) => stock switch {
        Stock.FromRemnant source => new RemnantOrigin(
            source.Remnant.Origin.Stock,
            Some(source.Remnant.Identity),
            source.Remnant.Generation + 1),
        _ => new RemnantOrigin(stock.Identity, None, 0),
    };

    private static RemnantProfile Profile(Stock stock) => stock.Switch(
        sheet: static source => Profile(source.Body, Some(source.Thickness), source.GrainAxis),
        plate: static source => Profile(source.Body, Some(source.Thickness), source.GrainAxis),
        roll: static source => Profile(source.Body, None, source.GrainAxis),
        coil: static source => Profile(source.Body, Some(source.Thickness), source.GrainAxis),
        barStock: static source => Profile(source.Body, None, None),
        tubeStock: static source => Profile(source.Body, Some(source.WallThickness), None),
        billet: static source => Profile(source.Body, Some(source.Depth), None),
        filament: static source => Profile(source.Body, None, None),
        fromRemnant: static source => source.Remnant.Profile);

    private static RemnantProfile Profile(StockBody body, Option<double> gauge, Option<double> grainAxis) =>
        new(gauge, grainAxis, body.Exclusions, None, Some(body.Lot), body.Heat,
            CostRate(body));

    private static Option<double> CostRate(StockBody body) {
        double area = Math.Abs(body.Region.Sum(static loop => loop.Area()));
        return area > 0.0 ? Some(body.Cost / area) : None;
    }

    private static Fin<Seq<(Loop Outer, Seq<Loop> Holes)>> ComponentsOf(Seq<Loop> region) =>
        region.IsEmpty || region.Exists(static loop => !Valid(loop))
            ? Fin.Fail<Seq<(Loop Outer, Seq<Loop> Holes)>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:region").ToError())
            : Relations(region).Bind(relations =>
                InvalidTopology(region, relations)
                    ? Fin.Fail<Seq<(Loop Outer, Seq<Loop> Holes)>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:topology").ToError())
                    : Assemble(region, relations));

    private static bool InvalidTopology(
        Seq<Loop> region,
        Seq<(int First, int Second, ArcRelation Relation)> relations) =>
        relations.Exists(static row => row.Relation == ArcRelation.Intersected || row.Relation == ArcRelation.Overlapping
            || row.Relation == ArcRelation.InvalidInput)
        || toSeq(Enumerable.Range(0, region.Count)).Exists(index => Parent(index, region, relations)
            .Exists(parent => region[parent].Winding() == region[index].Winding()));

    private static Option<int> Parent(
        int child,
        Seq<Loop> region,
        Seq<(int First, int Second, ArcRelation Relation)> relations) =>
        region.Map((loop, index) => (loop, index))
            .Filter(row => row.index != child && Inside(row.index, child, relations))
            .OrderBy(static row => Math.Abs(row.loop.Area()))
            .Map(static row => row.index)
            .HeadOrNone();

    private static Fin<Seq<(int First, int Second, ArcRelation Relation)>> Relations(Seq<Loop> region) =>
        toSeq(Enumerable.Range(0, region.Count)).Bind(first =>
                toSeq(Enumerable.Range(first + 1, region.Count - first - 1)).Map(second => (First: first, Second: second)))
            .Traverse(pair => Relation(region[pair.First], region[pair.Second])
                .Map(relation => (pair.First, pair.Second, relation)).ToValidation())
            .As().ToFin();

    private static Fin<Seq<(Loop Outer, Seq<Loop> Holes)>> Assemble(
        Seq<Loop> region,
        Seq<(int First, int Second, ArcRelation Relation)> relations) {
        Seq<(Loop Loop, int Index)> outers = region.Map((loop, index) => (loop, index)).Filter(static row => row.Loop.Winding() == Sign.Positive);
        Seq<(Loop Loop, int Index)> holes = region.Map((loop, index) => (loop, index)).Filter(static row => row.Loop.Winding() == Sign.Negative);
        return outers.IsEmpty
            ? Fin.Fail<Seq<(Loop Outer, Seq<Loop> Holes)>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:outer").ToError())
            : holes.Traverse(hole => outers
                    .Filter(outer => Inside(outer.Index, hole.Index, relations))
                    .OrderBy(static outer => Math.Abs(outer.Loop.Area())).Head
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:orphan-hole").ToError())
                    .Map(outer => (Hole: hole.Loop, Owner: outer.Index)).ToValidation())
                .As().ToFin().Map(assignments => outers
                    .Map(outer => (Outer: outer.Loop, Holes: assignments.Filter(row => row.Owner == outer.Index)
                        .Map(static row => row.Hole).OrderBy(CanonicalBytes, StringComparer.Ordinal).ToSeq()))
                    .OrderBy(static component => CanonicalBytes(component.Outer), StringComparer.Ordinal).ToSeq());
    }

    private static bool Inside(int outer, int hole, Seq<(int First, int Second, ArcRelation Relation)> relations) =>
        relations.Exists(row => row.First == Math.Min(outer, hole) && row.Second == Math.Max(outer, hole) &&
            (outer < hole ? row.Relation == ArcRelation.SecondInsideFirst : row.Relation == ArcRelation.FirstInsideSecond));

    private static bool Valid(Loop loop) => loop.Closed && loop.Count >= 3
        && loop.Vertices.ForAll(static point => double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z))
        && loop.Bulges.ForAll(double.IsFinite);

    // Identity is a framed content digest, so the batch dedup threads one seen-set instead of re-digesting each
    // prior remnant per candidate.
    private static Fin<RemnantPlan> Stock(Seq<Remnant> minted, Instant now, RemnantInventory inventory) =>
        minted.Map(static remnant => (Remnant: remnant, Identity: remnant.Identity))
            .Fold(
                (Seen: Set<UInt128>(), Rows: Seq<(Remnant Remnant, bool Duplicate)>()),
                static (state, row) => (
                    state.Seen.Add(row.Identity),
                    state.Rows.Add((row.Remnant, state.Seen.Contains(row.Identity)))))
            .Rows
            .Traverse(row => Gate(row.Remnant, row.Duplicate, now, inventory).Map(verdict => (row.Remnant, Verdict: verdict)).ToValidation())
            .As().ToFin().Bind(gated => StockPlan(gated, now, inventory));

    private static Fin<Either<Seq<RetireCause>, RemnantRow>> Gate(
        Remnant remnant,
        bool duplicate,
        Instant now,
        RemnantInventory inventory) =>
        Assess(remnant, remnant.Profile, inventory.Policy).Map(assessment => Verdict(
            remnant,
            assessment.Usable,
            assessment.Area,
            assessment.Aspect,
            assessment.Compactness,
            assessment.Spans,
            duplicate || inventory.Rows.Find(remnant.Identity).IsSome,
            now,
            inventory));

    private static Either<Seq<RetireCause>, RemnantRow> Verdict(
        Remnant remnant,
        Seq<Loop> usable,
        double area,
        double aspect,
        double compactness,
        bool spans,
        bool duplicate,
        Instant now,
        RemnantInventory inventory) {
        Seq<RetireCause> causes = Causes(
            remnant, remnant.Profile, area, aspect, compactness, spans, duplicate, inventory.Material, inventory.Policy);
        return causes.IsEmpty
            ? Right<Seq<RetireCause>, RemnantRow>(new RemnantRow(
                remnant,
                RemnantState.Stocked,
                remnant.Key,
                usable,
                area,
                Revision: 1,
                Claims: 0,
                now,
                RemnantCondition.Serviceable,
                remnant.Profile,
                None))
            : Left<Seq<RetireCause>, RemnantRow>(causes);
    }

    private static Fin<RemnantPlan> StockPlan(
        Seq<(Remnant Remnant, Either<Seq<RetireCause>, RemnantRow> Verdict)> gated,
        Instant now,
        RemnantInventory inventory) {
        Seq<RemnantRow> admitted = gated.Bind(static row => row.Verdict.Match(static _ => Seq<RemnantRow>(), Seq1));
        Seq<RemnantRetirement> retired = gated.Bind(row => row.Verdict.Match(
            causes => Seq(new RemnantRetirement(
                new RemnantRow(row.Remnant, RemnantState.Scrapped, row.Remnant.Key, row.Remnant.Region,
                    row.Remnant.AreaMm2, 1, 0, now,
                    RemnantCondition.Retire, row.Remnant.Profile, None), causes)),
            static _ => Seq<RemnantRetirement>()));
        Map<UInt128, RemnantRow> next = admitted.Concat(retired
                .Filter(static row => !row.Causes.Exists(static cause => cause is RetireCause.Duplicate or RetireCause.Material))
                .Map(static row => row.Row))
            .Fold(inventory.Rows, static (rows, row) => rows.AddOrUpdate(row.Remnant.Identity, row));
        Seq<RemnantTransition> transitions = admitted.Map(row => new RemnantTransition(row with {
                State = RemnantState.Minted, Revision = 0,
            }, row))
            .Concat(retired.Map(item => new RemnantTransition(item.Row with {
                State = RemnantState.Minted, Revision = 0,
            }, item.Row)));
        return transitions.Traverse(transition => Transition(transition.Before, transition.After).ToValidation())
            .As().ToFin().Map(_ => Plan(
                inventory with { Rows = next }, admitted, retired, transitions,
                scrapped: Total(retired
                    .Filter(static row => !row.Causes.Exists(static cause => cause is RetireCause.Duplicate or RetireCause.Material))
                    .Map(static row => row.Row))));
    }

    private static Fin<RemnantPlan> Claim(RemnantOp.Claim request, RemnantInventory inventory) =>
        request.Job < 0 || request.ExpectedRevision < 0
            ? Fin.Fail<RemnantPlan>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:claim").ToError())
            : Resolve(request.Key, request.ExpectedRevision, inventory).Match(
                conflict => Fin.Succ(ConflictPlan(inventory, conflict)),
                row => (row.State == RemnantState.Stocked, row.Lease.IsSome, row.Claims >= inventory.Policy.MaxClaims) switch {
                    (false, _, _) => Fin.Succ(ConflictPlan(inventory, new RemnantConflict.State(request.Key, row.State))),
                    (true, true, _) => Fin.Succ(ConflictPlan(
                        inventory,
                        new RemnantConflict.Lease(request.Key, request.Job, request.Now, row.Lease))),
                    (true, false, true) => Fin.Succ(ConflictPlan(
                        inventory,
                        new RemnantConflict.Claims(request.Key, row.Claims, inventory.Policy.MaxClaims))),
                    _ => Shift(inventory, row, row with {
                                State = RemnantState.Reserved,
                                Revision = row.Revision + 1,
                                Claims = row.Claims + 1,
                                Lease = Some(new RemnantLease(
                                    request.Job,
                                    new Interval(request.Now, request.Now + inventory.Policy.LeaseDuration))),
                            }),
                });

    private static Fin<RemnantPlan> Close(RemnantOp.Close request, RemnantInventory inventory) =>
        request.Job < 0 || request.ExpectedRevision < 0
            ? Fin.Fail<RemnantPlan>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:close").ToError())
            : Resolve(request.Key, request.ExpectedRevision, inventory).Match(
                conflict => Fin.Succ(ConflictPlan(inventory, conflict)),
                row => (row.State == RemnantState.Reserved,
                    row.Lease.Exists(lease => lease.Job == request.Job && lease.Active.Contains(request.Now))) switch {
                    (false, _) => Fin.Succ(ConflictPlan(inventory, new RemnantConflict.State(request.Key, row.State))),
                    (true, false) => Fin.Succ(ConflictPlan(
                        inventory,
                        new RemnantConflict.Lease(request.Key, request.Job, request.Now, row.Lease))),
                    _ => Dispose(request.Disposition, row, inventory, request.Now),
                });

    private static Fin<RemnantPlan> Dispose(
        ReservationDisposition disposition,
        RemnantRow row,
        RemnantInventory inventory,
        Instant now) =>
        disposition.Switch(
            state: (Row: row, Inventory: inventory, Now: now, Next: disposition.Next),
            release: static state => Shift(state.Inventory, state.Row, state.Row with {
                State = state.Next, Revision = state.Row.Revision + 1, Lease = None,
            }),
            consume: static (state, use) =>
                from _ in ComponentsOf(use.Used)
                from usable in Usable(state.Row, state.Inventory.Policy)
                from outside in Boolean(use.Used, usable, BoolKind.Not)
                from measure in outside.IsEmpty
                    ? Measure(use.Used, state.Inventory.Policy.ArcToleranceMm)
                    : Fin.Fail<(double Area, double Aspect, double Compactness)>(
                        new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:consumption").ToError())
                from remaining in Boolean(usable, use.Used, BoolKind.Not)
                from components in remaining.IsEmpty
                    ? Fin.Succ(Seq<(Loop Outer, Seq<Loop> Holes)>())
                    : ComponentsOf(remaining)
                let origin = new RemnantOrigin(
                    state.Row.Remnant.Origin.Stock,
                    Some(state.Row.Remnant.Identity),
                    state.Row.Remnant.Generation + 1)
                let recovered = components.Map(component => Mint(
                    component.Outer,
                    component.Holes,
                    state.Row.Remnant.Material,
                    origin,
                    state.Row.Profile with { Exclusions = Seq<Loop>() }))
                from consumed in Shift(state.Inventory, state.Row, state.Row with {
                    State = state.Next, Revision = state.Row.Revision + 1, Lease = None,
                }, consumed: Prorated(state.Row, measure.Area))
                from stocked in Stock(recovered, state.Now, consumed.Next)
                select Merge(consumed, stocked),
            scrap: static (state, scrap) => Shift(state.Inventory, state.Row, state.Row with {
                State = state.Next, Revision = state.Row.Revision + 1, Lease = None,
            }, retirement: Some(Seq(scrap.Cause)), scrapped: RemnantMeasure.Of(state.Row)));

    private static Fin<RemnantPlan> Sweep(RemnantOp.Sweep request, RemnantInventory inventory) =>
        from _ in guard(
            request.Observed.ForAll(static observation => observation.Key.Kind == EgressKind.Remnant)
                && request.Observed.Map(static observation => observation.Key.Digest).Distinct().Count == request.Observed.Count,
            new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:observation").ToError()).ToFin()
        let observed = request.Observed.Fold(
            Map<UInt128, RemnantObservation>(),
            static (rows, observation) => rows.AddOrUpdate(observation.Key.Digest, observation))
        let unmatched = request.Observed
            .Filter(observation => inventory.Rows.Find(observation.Key.Digest).IsNone)
            .Map(static observation => (RemnantConflict)new RemnantConflict.Missing(observation.Key))
        from options in inventory.Rows.Values.ToSeq()
            .Traverse(row => SweepRow(row, observed.Find(row.Remnant.Identity), request.Now, inventory.Policy).ToValidation())
            .As().ToFin()
        let changed = options.Somes().ToSeq()
        let transitions = changed.Bind(outcome => inventory.Rows.Find(outcome.Row.Remnant.Identity)
            .Map(before => new RemnantTransition(before, outcome.Row)).ToSeq())
        from __ in transitions.Traverse(transition => Transition(transition.Before, transition.After).ToValidation()).As().ToFin()
        let next = changed.Fold(
            inventory.Rows,
            static (rows, outcome) => rows.AddOrUpdate(outcome.Row.Remnant.Identity, outcome.Row))
        let retired = changed
            .Filter(static outcome => outcome.Row.State == RemnantState.Scrapped)
            .Map(static outcome => new RemnantRetirement(outcome.Row, outcome.Causes))
        select Plan(
            inventory with { Rows = next }, retired: retired, transitions: transitions, conflicts: unmatched,
            scrapped: Total(retired.Map(static item => item.Row)));

    private static Fin<Option<(RemnantRow Row, Seq<RetireCause> Causes)>> SweepRow(
        RemnantRow row,
        Option<RemnantObservation> observation,
        Instant now,
        ReusePolicy policy) => row.State.Terminal
            ? Fin.Succ(None)
            : observation.Map(seen =>
                from assessment in Assess(row.Remnant, seen.Profile, policy)
                let policyCauses = Causes(
                    row.Remnant,
                    seen.Profile,
                    assessment.Area,
                    assessment.Aspect,
                    assessment.Compactness,
                    assessment.Spans,
                    duplicate: false,
                    row.Remnant.Material,
                    policy)
                let causes = seen.Condition == RemnantCondition.Retire
                    ? policyCauses.Concat(Seq1<RetireCause>(new RetireCause.Inspection(row.Key)))
                    : policyCauses
                let condition = seen.Condition == RemnantCondition.Serviceable && !policyCauses.IsEmpty
                    ? RemnantCondition.Retire
                    : seen.Condition
                let activeLease = condition == RemnantCondition.Serviceable
                    ? row.Lease.Filter(lease => lease.Active.Contains(now))
                    : None
                let state = condition.Resolve(row.State, activeLease.IsSome)
                select Some((
                    Row: row with {
                        State = state,
                        Usable = assessment.Usable,
                        UsableAreaMm2 = assessment.Area,
                        Revision = row.Revision + 1,
                        ObservedAt = now,
                        Condition = condition,
                        Profile = seen.Profile,
                        Lease = state == RemnantState.Reserved ? activeLease : None,
                    },
                    Causes: causes)))
                .IfNone(Fin.Succ((
                    row.State == RemnantState.Reserved && row.Lease.Exists(lease => !lease.Active.Contains(now)),
                    now - row.ObservedAt >= policy.ObservationHorizon) switch {
                    (true, _) => Some((
                        Row: row with { State = RemnantState.Stocked, Revision = row.Revision + 1, Lease = None },
                        Causes: Seq<RetireCause>())),
                    (false, true) => Some((
                        Row: row with { State = RemnantState.Scrapped, Revision = row.Revision + 1, Lease = None },
                        Causes: Seq1<RetireCause>(new RetireCause.Observation(row.ObservedAt, now)))),
                    _ => None,
                }));

    private static Either<RemnantConflict, RemnantRow> Resolve(ContentKey key, int expectedRevision, RemnantInventory inventory) =>
        key.Kind != EgressKind.Remnant
            ? Left<RemnantConflict, RemnantRow>(new RemnantConflict.Kind(key))
            : inventory.Rows.Find(key.Digest).Match(
                Some: row => (row.Revision == expectedRevision, row.State.Terminal) switch {
                    (false, _) => Left<RemnantConflict, RemnantRow>(
                        new RemnantConflict.Revision(key, expectedRevision, row.Revision)),
                    (true, true) => Left<RemnantConflict, RemnantRow>(new RemnantConflict.State(key, row.State)),
                    _ => Right<RemnantConflict, RemnantRow>(row),
                },
                None: () => Left<RemnantConflict, RemnantRow>(new RemnantConflict.Missing(key)));

    private static Fin<Unit> AdmitInventory(RemnantInventory inventory) {
        RemnantRow[] rows = inventory.Rows.Values.ToArray();
        Error?[] faults = new Error?[rows.Length];
        ParallelHelper.For(0, rows.Length, new InventoryGate(inventory, rows, faults));
        Fin<Unit> rowGate = toSeq(faults).Traverse(error => error is null
                ? Fin.Succ(unit).ToValidation()
                : Fin.Fail<Unit>(error).ToValidation())
            .As().ToFin().Map(static _ => unit);
        return rowGate.Bind(_ => AdmitLineage(inventory, rows));
    }

    private static Fin<Unit> AdmitLineage(RemnantInventory inventory, RemnantRow[] rows) {
        // Acyclicity plus single-parent edges already force a forest, so transitive closure and reduction prove
        // nothing here; the load-bearing lineage law is generation succession and root-stock agreement along
        // every retained parent edge, checked against the resolved parent row.
        Map<UInt128, RemnantRow> byIdentity = toSeq(rows).Fold(
            Map<UInt128, RemnantRow>(),
            static (map, row) => map.AddOrUpdate(row.Remnant.Identity, row));
        bool keyed = toSeq(inventory.Rows.Keys).ForAll(key => inventory.Rows.Find(key)
            .Exists(row => key == row.Remnant.Identity));
        BidirectionalGraph<UInt128, SEdge<UInt128>> lineage = new(allowParallelEdges: false);
        lineage.AddVertexRange(byIdentity.Keys);
        if (!keyed || byIdentity.Count != rows.Length
            || toSeq(rows).Exists(row => row.Remnant.Parent.Exists(parent => !byIdentity.ContainsKey(parent))))
            return Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:lineage-parent").ToError());
        toSeq(rows).Iter(row => row.Remnant.Parent
            .Iter(parent => lineage.AddEdge(new SEdge<UInt128>(parent, row.Remnant.Identity))));
        if (!lineage.IsDirectedAcyclicGraph())
            return Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:lineage-cycle").ToError());
        bool succession = toSeq(lineage.Edges).ForAll(edge =>
            (from parent in byIdentity.Find(edge.Source)
             from child in byIdentity.Find(edge.Target)
             select child.Remnant.Generation == parent.Remnant.Generation + 1
                 && child.Remnant.Origin.Stock == parent.Remnant.Origin.Stock).IfNone(false));
        bool roots = toSeq(rows).ForAll(static row => row.Remnant.Parent.IsSome == row.Remnant.Generation > 0);
        return toSeq(lineage.TopologicalSort()).Count == rows.Length && succession && roots
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:lineage").ToError());
    }

    internal static Error? RowFault(RemnantRow row, RemnantInventory inventory) {
        bool lifecycle = row.Condition == RemnantCondition.Serviceable
                && (row.State == RemnantState.Stocked || row.State == RemnantState.Reserved || row.State == RemnantState.Consumed)
            || row.Condition == RemnantCondition.Quarantine && row.State == RemnantState.Quarantined
            || row.Condition == RemnantCondition.Retire && row.State == RemnantState.Scrapped;
        bool lease = (row.State == RemnantState.Reserved) == row.Lease.IsSome
            && row.Lease.ForAll(static held => held.Job >= 0 && held.Active.HasStart && held.Active.HasEnd
                && held.Active.Start < held.Active.End);
        bool profile = row.Profile.GaugeMm.ForAll(static value => double.IsFinite(value) && value > 0.0)
            && row.Profile.GrainAxisRadians.ForAll(double.IsFinite)
            && row.Profile.CostPerSquareMillimeter.ForAll(static value => double.IsFinite(value) && value >= 0.0)
            && row.Profile.Lot.ForAll(static value => !string.IsNullOrWhiteSpace(value))
            && row.Profile.Heat.ForAll(static value => !string.IsNullOrWhiteSpace(value))
            && row.Profile.Exclusions.ForAll(Valid);
        double measuredArea = Math.Abs(row.Usable.Sum(static loop => loop.Area()));
        double areaTolerance = inventory.Policy.ArcToleranceMm
            * Math.Max(1.0, row.Usable.Sum(static loop => loop.Length()));
        bool usable = !row.Usable.IsEmpty
            && ComponentsOf(row.Usable).Match(Succ: static components => !components.IsEmpty, Fail: static _ => false)
            && double.IsFinite(row.UsableAreaMm2) && row.UsableAreaMm2 > 0.0
            && Math.Abs(measuredArea - row.UsableAreaMm2) <= areaTolerance;
        return row.Key.Kind == EgressKind.Remnant && row.Key.Digest == row.Remnant.Identity
            && row.Remnant.Material == inventory.Material && row.Revision >= 0
            && row.Claims >= 0 && row.Claims <= inventory.Policy.MaxClaims
            && lifecycle && lease && profile && usable
            ? null
            : new GeometryFault.DegenerateInput(Kind.Polyline, -1, $"remnant:inventory:{row.Remnant.Identity}").ToError();
    }

    private static Fin<Seq<Loop>> Offset(Seq<Loop> loops, double distance) => loops.IsEmpty
        ? Fin.Succ(Seq<Loop>())
        : ArcForest.Admit(loops, loops.Head().Tolerance, loops.Head().Plane).ToFin()
            .Bind(forest => ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Forest(forest), distance)))
            .Bind(ArcPaths);

    private static Fin<Seq<Loop>> Boolean(Seq<Loop> subject, Seq<Loop> clip, BoolKind kind) => subject.IsEmpty
        ? Fin.Succ(Seq<Loop>())
        : ArcForest.Admit(subject, subject.Head().Tolerance, subject.Head().Plane).ToFin()
            .Bind(first => ArcForest.Admit(clip, first.Tolerance, first.Plane).ToFin()
                .Bind(second => ArcAlgebra.Apply(new ArcOp.Boolean(first, second, kind))))
            .Bind(ArcPaths);

    private static Fin<ArcRelation> Relation(Loop first, Loop second) =>
        ArcForest.Admit(Seq(first, second), first.Tolerance, first.Plane).ToFin()
            .Bind(forest => ArcAlgebra.Apply(new ArcOp.Inspect(forest, new ArcProbe.Pair(first, second))))
            .Bind(static trace => trace is ArcTrace.Inspection { Result: ArcInspection.Pair pair }
                ? Fin.Succ(pair.Relation)
                : Fin.Fail<ArcRelation>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:relation-trace").ToError()));

    private static Fin<Loop> Lower(Loop loop, double error) =>
        ArcAlgebra.Densify(new ArcProjection.Lower(loop, error))
            .Bind(static trace => trace is ArcTrace.Densified densified
                ? Fin.Succ(densified.Receipt.Result)
                : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:projection-trace").ToError()));

    private static Fin<Seq<Loop>> ArcPaths(ArcTrace trace) => trace switch {
        ArcTrace.Forest forest => Fin.Succ(forest.Result.Loops),
        ArcTrace.Paths paths => Fin.Succ(paths.Result),
        _ => Fin.Fail<Seq<Loop>>(new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:arc-trace").ToError()),
    };

    private static Fin<(Seq<Loop> Usable, double Area, double Aspect, double Compactness, bool Spans)> Assess(
        Remnant remnant,
        RemnantProfile profile,
        ReusePolicy policy) =>
        from usable in Usable(remnant, profile, policy)
        from measure in Measure(usable, policy.ArcToleranceMm)
        from spanCore in Offset(usable, -0.5 * policy.MinReusableSpanMm)
        select (usable, measure.Area, measure.Aspect, measure.Compactness, !spanCore.IsEmpty);

    private static Seq<RetireCause> Causes(
        Remnant remnant,
        RemnantProfile profile,
        double area,
        double aspect,
        double compactness,
        bool spans,
        bool duplicate,
        MaterialId material,
        ReusePolicy policy) =>
        Seq(
            remnant.Material != material
                ? Some<RetireCause>(new RetireCause.Material(remnant.Material, material))
                : None,
            duplicate
                ? Some<RetireCause>(new RetireCause.Duplicate(remnant.Identity))
                : None,
            remnant.Generation > policy.MaxGeneration
                ? Some<RetireCause>(new RetireCause.Generation(remnant.Generation, policy.MaxGeneration))
                : None,
            profile.GaugeMm.Filter(gauge => gauge >= policy.MinGaugeMm).IsNone && policy.MinGaugeMm > 0.0
                ? Some<RetireCause>(new RetireCause.Gauge(profile.GaugeMm, policy.MinGaugeMm))
                : None,
            area < policy.MinUsableAreaMm2
                ? Some<RetireCause>(new RetireCause.AreaFloor(area, policy.MinUsableAreaMm2))
                : None,
            !spans
                ? Some<RetireCause>(new RetireCause.FeatureWidth(policy.MinReusableSpanMm))
                : None,
            aspect < policy.MinAspect
                ? Some<RetireCause>(new RetireCause.SliverAspect(aspect, policy.MinAspect))
                : None,
            compactness < policy.MinCompactness
                ? Some<RetireCause>(new RetireCause.Compactness(compactness, policy.MinCompactness))
                : None,
            profile.CostPerSquareMillimeter.Map(rate => rate * area).Filter(value => value < policy.MinSalvageValue)
                .Map(value => (RetireCause)new RetireCause.Salvage(value, policy.MinSalvageValue)))
            .Somes()
            .Concat(policy.Missing(profile).Map(static trait => (RetireCause)new RetireCause.Traceability(trait)))
            .ToSeq();

    private static Fin<Seq<Loop>> Usable(RemnantRow row, ReusePolicy policy) =>
        Usable(row.Remnant, row.Profile, policy);

    private static Fin<Seq<Loop>> Usable(Remnant remnant, RemnantProfile profile, ReusePolicy policy) =>
        Offset(remnant.Region, -policy.InsetMm).Bind(inset => profile.Exclusions.IsEmpty
            ? Fin.Succ(inset)
            : Boolean(inset, profile.Exclusions, BoolKind.Not));

    private static Fin<(double Area, double Aspect, double Compactness)> Measure(Seq<Loop> region, double tolerance) =>
        region.Traverse(loop => MeasureLoop(loop, tolerance))
            .As().ToFin().Map(rows => {
                double area = Math.Abs(rows.Sum(static row => row.Area));
                double perimeter = rows.Sum(static row => row.Length);
                double compactness = perimeter == 0.0 ? 0.0 : Math.Min(1.0, (4.0 * Math.PI * area) / (perimeter * perimeter));
                Seq<Point3d> points = rows.Bind(static row => toSeq(row.Polygon.Vertices));
                Seq<(double Width, double Height, double Area)> envelopes = rows
                    .Bind(static row => toSeq(row.Polygon.Vertices).Map((point, index) => (A: point, B: row.Polygon.At(index + 1))))
                    .Map(edge => edge.B - edge.A)
                    .Filter(static direction => direction.Length > 0.0)
                    .Map(direction => {
                        double ux = direction.X / direction.Length;
                        double uy = direction.Y / direction.Length;
                        Seq<double> along = points.Map(point => (point.X * ux) + (point.Y * uy));
                        Seq<double> across = points.Map(point => (-point.X * uy) + (point.Y * ux));
                        double width = along.Max() - along.Min();
                        double height = across.Max() - across.Min();
                        return (width, height, width * height);
                    });
                double aspect = envelopes.OrderBy(static envelope => envelope.Area).HeadOrNone()
                    .Map(static envelope => Math.Min(envelope.Width, envelope.Height) / Math.Max(envelope.Width, envelope.Height))
                    .IfNone(0.0);
                return (area, aspect, compactness);
            });

    private static K<Validation<Error>, (double Area, double Length, Loop Polygon)> MeasureLoop(Loop loop, double tolerance) =>
        Lower(loop, tolerance).ToValidation()
            .Map(polygon => (loop.Area(), loop.Length(), polygon));

    private static Fin<RemnantPlan> Shift(
        RemnantInventory inventory,
        RemnantRow before,
        RemnantRow after,
        RemnantMeasure consumed = default,
        Option<Seq<RetireCause>> retirement = default,
        RemnantMeasure scrapped = default) =>
        Transition(before, after).Map(_ => Plan(
            inventory with { Rows = inventory.Rows.AddOrUpdate(after.Remnant.Identity, after) },
            retired: retirement.Map(causes => new RemnantRetirement(after, causes)).ToSeq(),
            transitions: Seq(new RemnantTransition(before, after)),
            consumed: consumed,
            scrapped: scrapped));

    private static Fin<Unit> Transition(RemnantRow before, RemnantRow after) =>
        guard(
            (before.State == after.State || before.State.Admits(after.State))
                && after.Revision == before.Revision + 1
                && before.Key.Kind == after.Key.Kind
                && before.Key.Digest == after.Key.Digest,
            new GeometryFault.DegenerateInput(Kind.Polyline, -1, "remnant:transition").ToError()).ToFin();

    private static RemnantPlan Merge(RemnantPlan first, RemnantPlan second) => Plan(
        second.Next,
        first.Admitted.Concat(second.Admitted),
        first.Retired.Concat(second.Retired),
        first.Transitions.Concat(second.Transitions),
        first.Conflicts.Concat(second.Conflicts),
        first.Consumed + second.Consumed,
        first.Scrapped + second.Scrapped);

    private static RemnantPlan ConflictPlan(RemnantInventory inventory, RemnantConflict conflict) =>
        Plan(inventory, conflicts: Seq(conflict));

    // Potential is the standing serviceable measure of the resulting inventory, so every arm reports one
    // comparable stock-on-hand figure instead of an operation-local admitted total.
    private static RemnantPlan Plan(
        RemnantInventory next,
        Seq<RemnantRow> admitted = default,
        Seq<RemnantRetirement> retired = default,
        Seq<RemnantTransition> transitions = default,
        Seq<RemnantConflict> conflicts = default,
        RemnantMeasure consumed = default,
        RemnantMeasure scrapped = default) =>
        new(next, admitted, retired, transitions, conflicts, Yields(next),
            Total(next.Rows.Values.ToSeq()
                .Filter(static row => !row.State.Terminal && row.Condition == RemnantCondition.Serviceable)),
            consumed,
            scrapped);

    private static Seq<RemnantYield> Yields(RemnantInventory inventory) =>
        toSeq(inventory.Rows.Values.GroupBy(static row => row.Remnant.Origin.Stock))
            .Map(static group => Yield(group.Key, group.ToSeq()))
            .OrderBy(static row => row.Stock).ToSeq();

    private static RemnantYield Yield(UInt128 stock, Seq<RemnantRow> rows) => new(
        stock,
        rows.Count,
        rows.Map(static row => row.Remnant.Generation).Max(),
        Total(rows.Filter(static row => !row.State.Terminal)),
        Total(rows.Filter(static row => row.State == RemnantState.Scrapped)));

    private static RemnantMeasure Total(Seq<RemnantRow> rows) =>
        rows.Fold(RemnantMeasure.Zero, static (measure, row) => measure + RemnantMeasure.Of(row));

    private static RemnantMeasure Prorated(RemnantRow row, double areaMm2) =>
        new(areaMm2, row.Remnant.Value.Map(value => row.UsableAreaMm2 <= 0.0 ? 0.0 : value * areaMm2 / row.UsableAreaMm2).IfNone(0.0));

    private static ContentKey KeyOf(
        Loop boundary,
        Seq<Loop> holes,
        MaterialId material,
        RemnantOrigin origin) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Write(writer, material.Value);
        Write(writer, origin.Stock);
        Write(writer, origin.Generation);
        Write(writer, origin.Parent);
        Write(writer, boundary);
        Write(writer, holes.Count);
        holes.OrderBy(CanonicalBytes, StringComparer.Ordinal).Iter(hole => Write(writer, hole));
        return ContentKey.Of(EgressKind.Remnant, writer.WrittenSpan);
    }

    private static string CanonicalBytes(Loop loop) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Write(writer, loop);
        return Convert.ToHexString(writer.WrittenSpan);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(sizeof(int)), value);
        writer.Advance(sizeof(int));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, ulong value) {
        BinaryPrimitives.WriteUInt64LittleEndian(writer.GetSpan(sizeof(ulong)), value);
        writer.Advance(sizeof(ulong));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, double value) {
        BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(sizeof(double)), value);
        writer.Advance(sizeof(double));
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, UInt128 value) {
        Write(writer, (ulong)(value >> 64));
        Write(writer, (ulong)value);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, string value) {
        int length = Encoding.UTF8.GetByteCount(value);
        Write(writer, length);
        Encoding.UTF8.GetBytes(value, writer.GetSpan(length));
        writer.Advance(length);
    }

    private static void Write(ArrayPoolBufferWriter<byte> writer, Option<UInt128> value) {
        Write(writer, value.IsSome ? 1 : 0);
        value.Iter(inner => Write(writer, inner));
    }

    // Rotation-invariant preimage: winding stays identity while the vertex walk starts at the rotation whose
    // quantized station sequence is lexicographically least, so congruent loops cannot fork one identity.
    private static void Write(ArrayPoolBufferWriter<byte> writer, Loop loop) {
        Loop canonical = loop.AsCcw();
        double quantum = canonical.Tolerance.Absolute.Value;
        int start = toSeq(Enumerable.Range(0, canonical.Count))
            .Fold(0, (best, candidate) => Rotation(canonical, candidate, best, quantum) < 0 ? candidate : best);
        Write(writer, loop.Winding() == Sign.Positive ? 1 : -1);
        Write(writer, canonical.Closed ? 1 : 0);
        Write(writer, canonical.Count);
        toSeq(Enumerable.Range(0, canonical.Count)).Iter(offset => {
            (double X, double Y, double Z, double Bulge) station = Station(canonical, start + offset, quantum);
            Write(writer, station.X);
            Write(writer, station.Y);
            Write(writer, station.Z);
            Write(writer, station.Bulge);
        });
    }

    private static int Rotation(Loop loop, int left, int right, double quantum) =>
        toSeq(Enumerable.Range(0, loop.Count))
            .Map(offset => Station(loop, left + offset, quantum).CompareTo(Station(loop, right + offset, quantum)))
            .Find(static order => order != 0)
            .IfNone(0);

    private static (double X, double Y, double Z, double Bulge) Station(Loop loop, int at, double quantum) =>
        (Quantized(loop.At(at).X, quantum), Quantized(loop.At(at).Y, quantum),
         Quantized(loop.At(at).Z, quantum), Quantized(loop.BulgeAt(at), quantum));

    private static double Quantized(double value, double quantum) =>
        Math.Round(value / quantum, MidpointRounding.ToEven) * quantum;
}
```
