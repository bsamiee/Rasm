# [RASM_FABRICATION_REMNANT]

`Remnant` owns one connected, arc-preserving offcut, canonical content identity, material and stock lineage, reusable-region evidence, and the generation carried into the next nesting inventory. Equivalent loop rotations and hole orderings mint one identity, while winding remains part of that identity.

`RemnantInventory` owns one material lane. `RemnantOp` admits stock, reservation, disposition, and physical-census events through one `Reconcile` fold; revision conflicts remain typed result evidence, geometry faults remain on `Fin`, and every successful transition retains its before-and-after row.

## [01]-[INDEX]

- [02]-[LIFECYCLE]: the state, condition, disposition, cause, conflict, and operation vocabularies every offcut transition names.
- [03]-[INVENTORY]: profile, row, inventory, plan, yield, measure, and slot owners for one material lane.
- [04]-[RECONCILIATION]: admission, containment, minting, reconciliation, sweep, lineage admission, projection, and the canonical preimage.

## [02]-[LIFECYCLE]

- Owner: `RemnantState`, `RemnantCondition`, `ReservationDisposition`, `ReuseTrait`, `RetireCause`, `RemnantConflict`, and `RemnantOp` close lifecycle behaviour and evidence; `ReusePolicy` owns reuse admission.
- Cases: `RemnantOp` carries `Stocking`, `Claim`, `Close`, and `Sweep`; `ReservationDisposition.Consume` subtracts its used region and stocks each surviving connected child in the same result.
- Entry: `Admit(Seq<Loop>, MaterialId, RemnantOrigin, RemnantProfile)` mints each connected component, `Reconcile(RemnantOp, RemnantInventory)` folds lifecycle operations, `From(Stock, Seq<Loop>, double)` inverts consumed stock, `Holds(Seq<Loop>, Option<double>, ReusePolicy)` answers policy-inset fit with grain, and `Stockable(RemnantInventory)` projects the next inventory smallest-adequate first.
- Packages: `CommunityToolkit.HighPerformance`, `LanguageExt.Core`, `NodaTime`, `QuikGraph`, `Rasm` (`ICapability`/`CapabilitySet`, `Context`/`ToleranceLane`), `Rasm.Element`, `RhinoCommon`, `Thinktecture.Runtime.Extensions`, and `UnitsNet` (`Length`, `Area`, `Ratio` on the reuse policy's own floors) compose the owner.
- Law: `ReuseTrait` carries the kernel `ICapability` floor, so a traceability demand is a `CapabilitySet<ReuseTrait>` value and never a hand-walked predicate roster — `Missing` answers the accumulating retire causes and `Require` guards the one admission that genuinely refuses, the salvage floor that cannot be stated without the valuation trait carrying it. Canonical text orders by the stable key; this unordered vocabulary carries no semantic ordinal.
- Growth: each reuse gate adds one `ReusePolicy` member and one `ReuseGates` row minting its payload-bearing `RetireCause` case; each traceability demand adds one `ReuseTrait` row the `Required` capability column admits by name; each lifecycle operation adds one `RemnantOp` case and one generated dispatch arm; each physical observation axis adds one `RemnantObservation` member.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
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
using Rhino.Geometry;
using System.Linq;
using Thinktecture;
using static LanguageExt.Prelude;
using Interval = NodaTime.Interval;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] ---------------------------------------------------------------------------
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
public sealed partial class ReuseTrait : ICapability<ReuseTrait> {
    public static readonly ReuseTrait Grain = new("grain", static profile => profile.GrainAxisRadians.IsSome);
    public static readonly ReuseTrait Location = new("location", static profile => profile.Location.IsSome);
    public static readonly ReuseTrait Lot = new("lot", static profile => profile.Lot.IsSome);
    public static readonly ReuseTrait Heat = new("heat", static profile => profile.Heat.IsSome);
    public static readonly ReuseTrait Valuation = new("valuation", static profile => profile.CostPerSquareMillimeter.IsSome);

    [UseDelegateFromConstructor]
    public partial bool Carried(RemnantProfile profile);

    public static CapabilitySet<ReuseTrait> Of(RemnantProfile profile) =>
        CapabilitySet<ReuseTrait>.Of(Items.Where(trait => trait.Carried(profile)).ToArray());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RetireCause {
    private RetireCause() { }

    public sealed record AreaFloor(double ActualMm2, double RequiredMm2) : RetireCause;
    public sealed record FeatureWidth(double RequiredMm) : RetireCause;
    public sealed record SliverAspect(Option<double> Actual, double Required) : RetireCause;
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

```

## [03]-[INVENTORY]

- Owner: `RemnantProfile` carries traceability, gauge, grain, the inherited symmetry law, cost, and exclusion facts; `RemnantRow` is one inventory line with its state, condition, revision, claim census, and optional lease; `RemnantInventory` owns one material lane; `RemnantPlan` is the settled reconciliation result.
- Law: an offcut is the SAME substance as the stock it came off, so its placement legality travels with the geometry and `Stock.FromRemnant` projects the profile's `Law` straight back into inventory — a re-read from appearance at re-entry would let one sheet's offcut nest under a law its parent refused.
- Law: a row's identity IS its content key — `Row.Key.Digest == Remnant.Identity` is an admitted invariant, so an inventory keyed by anything else cannot exist and the batch dedup threads ONE seen-set rather than re-digesting each prior remnant per candidate. `Remnant.Key` is an ADMITTED COLUMN minted once on the `Keyed` path and re-derived by the validator as its proof, never a property re-folding the canonical preimage on every `Identity` read.
- Law: `ReusePolicy` carries typed measures — `Length`, `Area`, `Ratio`, `Duration` — and reads its arc and grain budgets off the admitted `Context` lanes, because `ToleranceLane` owns every band it derives; a tolerance column beside a lane is a copy that drifts from it. Only the salvage floor stays a bare double, and it says so: a shop currency has no admitted dimension.
- Law: lineage is a forest by construction — single-parent edges plus acyclicity — so transitive closure and reduction prove nothing here and are refused by name; the load-bearing law is generation succession and root-stock agreement along every retained parent edge, checked against the resolved parent row.
- Result: `RemnantPlan` carries the next inventory, admissions, accumulated retirement causes, conflicts, validated transitions, per-source-stock `RemnantYield` rows, and the standing potential, consumed, and scrapped `RemnantMeasure` pairs of area and value.
- Boundary: `RemnantSlots` names the `store.fabrication.remnant.<verb>` streams the validated transitions and the re-admitted inventory census ride on the Persistence slot registry, so shop offcuts survive restart and share across apps without collision.

```csharp
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;
using Interval = NodaTime.Interval;

namespace Rasm.Fabrication.Nesting;
// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class ReusePolicy {
    public Length KerfTrim { get; }
    public Length RegripMargin { get; }
    public Area MinUsable { get; }
    public Length MinReusableSpan { get; }
    public Ratio MinAspect { get; }
    public Ratio MinCompactness { get; }
    public Length MinGauge { get; }
    public double MinSalvageValue { get; }
    public int MaxGeneration { get; }
    public int MaxClaims { get; }
    public Duration LeaseDuration { get; }
    public Duration ObservationHorizon { get; }
    public Context Tolerance { get; }
    public CapabilitySet<ReuseTrait> Required { get; }

    public double ArcToleranceMm => Tolerance.For(ToleranceLane.Arc).Value;
    public double GrainToleranceRadians => Tolerance.For(ToleranceLane.Grain).Value;
    public double InsetMm => (KerfTrim + RegripMargin).Millimeters;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length kerfTrim,
        ref Length regripMargin,
        ref Area minUsable,
        ref Length minReusableSpan,
        ref Ratio minAspect,
        ref Ratio minCompactness,
        ref Length minGauge,
        ref double minSalvageValue,
        ref int maxGeneration,
        ref int maxClaims,
        ref Duration leaseDuration,
        ref Duration observationHorizon,
        ref Context tolerance,
        ref CapabilitySet<ReuseTrait> required) {
        double[] floors = [kerfTrim.Millimeters, regripMargin.Millimeters, minUsable.SquareMillimeters,
            minReusableSpan.Millimeters, minGauge.Millimeters, minSalvageValue,
            minAspect.DecimalFractions, minCompactness.DecimalFractions];
        validationError = floors.Any(static value => !double.IsFinite(value) || value < 0.0)
            || minAspect.DecimalFractions > 1.0 || minCompactness.DecimalFractions > 1.0
            || !tolerance.IsValid || tolerance.For(ToleranceLane.Arc).Value <= 0.0
            || tolerance.For(ToleranceLane.Grain).Value >= Math.PI
            || maxGeneration < 0 || maxClaims < 1
            || leaseDuration <= Duration.Zero || observationHorizon <= Duration.Zero
            || (minSalvageValue > 0.0 && !required.Admits(ReuseTrait.Valuation))
                ? new ValidationError("remnant:reuse-policy")
                : null;
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct RemnantLocation {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        if (!Witness.Keyed(value))
            validationError = new ValidationError("remnant:location");
    }
}

public sealed record RemnantProfile(
    Option<double> GaugeMm,
    Option<double> GrainAxisRadians,
    MaterialSymmetry Law,
    Seq<Loop> Exclusions,
    Option<RemnantLocation> Location,
    Option<string> Lot,
    Option<string> Heat,
    Option<double> CostPerSquareMillimeter) {
    public static readonly RemnantProfile Empty = new(None, None, MaterialSymmetry.Free, Seq<Loop>(), None, None, None, None);
}

public readonly record struct RemnantOrigin(UInt128 Stock, Option<UInt128> Parent, int Generation);
[ComplexValueObject]
public sealed partial class Remnant {
    public Loop Boundary { get; }
    public Seq<Loop> Holes { get; }
    public MaterialId Material { get; }
    public RemnantOrigin Origin { get; }
    public RemnantProfile Profile { get; }

    public ContentKey Key { get; }

    public Seq<Loop> Region => Seq(Boundary).Concat(Holes);
    public Option<UInt128> Parent => Origin.Parent;
    public int Generation => Origin.Generation;
    public UInt128 Identity => Key.Digest;
    public double AreaMm2 => Math.Abs(Region.Sum(static loop => loop.Area()));
    public Option<double> Value => Profile.CostPerSquareMillimeter.Map(rate => rate * AreaMm2);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Loop boundary,
        ref Seq<Loop> holes,
        ref MaterialId material,
        ref RemnantOrigin origin,
        ref RemnantProfile profile,
        ref ContentKey key) {
        Seq<Loop> region = Seq(boundary).Concat(holes);
        bool connected = Remnants.ComponentsOf(region).Match(
            Succ: static components => components.Count == 1,
            Fail: static _ => false);
        Loop admittedBoundary = boundary;
        Seq<Loop> admittedHoles = holes;
        MaterialId admittedMaterial = material;
        RemnantOrigin admittedOrigin = origin;
        ContentKey admittedKey = key;
        if (boundary.Winding() != Sign.Positive || holes.Exists(static hole => hole.Winding() != Sign.Negative)
            || origin.Generation < 0 || !connected
            || !Remnants.KeyOf(admittedBoundary, admittedHoles, admittedMaterial, admittedOrigin)
                .ToOption().Exists(minted => admittedKey == minted))
            validationError = new ValidationError(string.Join(" | ", new object?[] { Kind.Polyline, None, "remnant:topology" }));
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

public readonly record struct RemnantMeasure(double AreaMm2, Option<double> Value) {
    public static readonly RemnantMeasure Zero = new(0.0, Some(0.0));

    public static RemnantMeasure Of(RemnantRow row) => new(row.UsableAreaMm2, Priced(row));

    public static Option<double> Priced(RemnantRow row) => row.Remnant.Value.IsSome
        ? row.Remnant.Value
        : row.Profile.CostPerSquareMillimeter.Map(rate => rate * row.UsableAreaMm2);

    public Option<double> Density => Value.Map(value => value / Math.Max(AreaMm2, double.Epsilon));

    public static RemnantMeasure operator +(RemnantMeasure left, RemnantMeasure right) =>
        new(left.AreaMm2 + right.AreaMm2, (left.Value, right.Value).Apply(static (a, b) => a + b).As());
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

public static class RemnantSlots {
    public const string Transition = "store.fabrication.remnant.transition";
    public const string Census = "store.fabrication.remnant.census";
}

file readonly struct InventoryGate(RemnantInventory inventory, RemnantRow[] rows, Error?[] faults) : IAction {
    public void Invoke(int index) => faults[index] = Remnants.RowFault(rows[index], inventory);
}

```

## [04]-[RECONCILIATION]

- Owner: `Remnants` owns admission, minting, containment, reconciliation, sweep, lineage admission, projection, and the canonical preimage; `Remnant` stays a value with no fold of its own, so the type is not a partial split across two sections.
- Law: containment reads the `Geometry2D` owner's own topology walk — the `PolygonTrace.Regioned` projection publishes `Depth`, `Parent`, and `IsHole`, so a hole's owner is a COLUMN read and the pairwise arc-relation matrix over the region deletes whole. `ForestDisjointSet` is refused by name: union-find collapses a set to an arbitrary representative and cannot answer WHICH member encloses.
- Law: `Loop.CanonicalBytes` is the ONE loop preimage — rotation-canonical, CCW-oriented, quantized on the loop's own admitted grid — so the hand rotation search and its station comparator delete onto it, and set ordering keys on each loop's own digest. Both preimage CLOSES seat at the S0 `FabricationCanon` — `Keyed` mints the retaining remnant address, `Ordered` answers the streaming digest that totally orders a region's loops without materializing a buffer per probe. Hex TEXT renders of a preimage decide no byte order here; the folder preimage law forbids it.
- Law: the nine reuse gates are ROWS over one `RemnantAssessment` measurement carrier, so a new gate is one row and both call sites read the same fold.
- Law: an absent measure is carried, never forged. Value resolves from the remnant's own figure or the profile rate over its usable area, and a remnant with neither stays UNPRICED through every total and sorts behind every priced row; aspect is absent where the calipers walk returns no `OrientedEnvelope` and the sliver gate retires on that absence with the absence in its cause. A zero standing in for either fact makes an unmeasured offcut indistinguishable from a worthless one and scraps stock under a verdict nobody reached, and a provider failure rides the typed result rather than becoming a measure.
- Auto: arc-exact offsets and Booleans route through `ArcAlgebra.Apply`; chord projection routes through `ArcAlgebra.Densify`; exact measures route through `Loop.Area` and `Loop.Length`; independent row gates partition through `ParallelHelper`; lineage acyclicity and order route through `QuikGraph`; lease membership routes through `Interval.Contains`.
- Exemption: `InventoryGate` is the measured per-row partition boundary and `AdmitLineage` the bounded graph-population kernel; mutation stays inside their own admitted containers.
- Boundary: `Remnant.Key` is the lifecycle key, `Stock.FromRemnant` is the next-nest carrier, and `FabricationResult.Placement.Remnants` is the placement result contract.

```csharp
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using NodaTime;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;
using Interval = NodaTime.Interval;

namespace Rasm.Fabrication.Nesting;
// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Remnants {
    public static Fin<Seq<Remnant>> Admit(
        Seq<Loop> region,
        MaterialId material,
        RemnantOrigin origin,
        RemnantProfile profile) =>
        ComponentsOf(region).Bind(components => components
            .Traverse(component => Mint(component.Outer, component.Holes, material, origin, profile).ToValidation())
            .As().ToFin());

    public static Fin<Seq<Remnant>> From(Stock stock, Seq<Loop> placed, double clearance) =>
        !double.IsFinite(clearance) || clearance < 0.0
            ? Fin.Fail<Seq<Remnant>>(new KernelFault.InvalidValue("remnant", "remnant:clearance"))
            : from available in Available(stock)
              from remaining in placed.IsEmpty
                  ? Fin.Succ(available)
                  : placed.Traverse(loop => Offset(Seq(loop), 0.5 * clearance).ToValidation())
                      .As().ToFin()
                      .Bind(inflated => Boolean(available, inflated.Bind(static loops => loops), BoolKind.Not))
              from components in ComponentsOf(remaining)
              from minted in components
                  .Traverse(component => Mint(
                      component.Outer, component.Holes, stock.Material, Lineage(stock), Profile(stock)).ToValidation())
                  .As().ToFin()
              select minted;

    private static Fin<Seq<Loop>> Available(Stock stock) => stock.Exclusions.IsEmpty
        ? Fin.Succ(stock.Region)
        : Boolean(stock.Region, stock.Exclusions, BoolKind.Not);

    public Fin<Option<double>> Holds(Seq<Loop> part, Option<double> grainAxisRadians, ReusePolicy policy) =>
        grainAxisRadians.Exists(demand => !Profile.GrainAxisRadians
            .Exists(carried => Aligned(demand, carried, policy.GrainToleranceRadians)))
            ? Fin.Succ(Option<double>.None)
            : from usable in Usable(this, Profile, policy)
              from outside in Boolean(part, usable, BoolKind.Not)
              from measure in outside.IsEmpty
                  ? Measure(usable, policy.ArcToleranceMm).Map(static value => Some(value))
                  : Fin.Succ(Option<(double Area, Option<double> Aspect, double Compactness)>.None)
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

    public static Fin<Seq<Stock>> Stockable(RemnantInventory inventory) =>
        AdmitInventory(inventory).Bind(_ => toSeq(inventory.Rows.Values.ToSeq()
            .Filter(static row => row.State == RemnantState.Stocked && row.Condition == RemnantCondition.Serviceable)
            .OrderBy(static row => row.UsableAreaMm2)
            .ThenBy(static row => RemnantMeasure.Of(row).Density.IfNone(double.PositiveInfinity))
            .ThenBy(static row => row.Remnant.Identity))
            .Traverse(row => Usable(row, inventory.Policy)
                .Bind(usable => Admit(
                    usable,
                    row.Remnant.Material,
                    new RemnantOrigin(row.Remnant.Origin.Stock, Some(row.Remnant.Identity), row.Remnant.Generation + 1),
                    row.Profile))
                .Map(remnants => remnants.Map(static remnant => (Stock)new Stock.FromRemnant(remnant))).ToValidation())
            .As().ToFin().Map(static rows => rows.Bind(identity)));

    private static Fin<Remnant> Mint(Loop boundary, Seq<Loop> holes, MaterialId material, RemnantOrigin origin, RemnantProfile profile) =>
        KeyOf(boundary, holes, material, origin).Bind(key =>
            Remnant.Validate(boundary, holes, material, origin, profile, key, out Remnant remnant).Admitted(remnant));

    private static RemnantOrigin Lineage(Stock stock) => stock switch {
        Stock.FromRemnant source => new RemnantOrigin(
            source.Remnant.Origin.Stock,
            Some(source.Remnant.Identity),
            source.Remnant.Generation + 1),
        _ => new RemnantOrigin(stock.Identity, None, 0),
    };

    private static RemnantProfile Profile(Stock stock) => stock.Switch(
        sheet: source => Profile(source.Body, Some(source.Thickness), source.GrainAxis, stock.Law),
        plate: source => Profile(source.Body, Some(source.Thickness), source.GrainAxis, stock.Law),
        roll: source => Profile(source.Body, None, source.GrainAxis, stock.Law),
        coil: source => Profile(source.Body, Some(source.Thickness), source.GrainAxis, stock.Law),
        barStock: source => Profile(source.Body, None, None, stock.Law),
        tubeStock: source => Profile(source.Body, Some(source.WallThickness), None, stock.Law),
        billet: source => Profile(source.Body, Some(source.Depth), None, stock.Law),
        filament: source => Profile(source.Body, None, None, stock.Law),
        fromRemnant: static source => source.Remnant.Profile);

    private static RemnantProfile Profile(StockBody body, Option<double> gauge, Option<double> grainAxis,
        MaterialSymmetry law) =>
        new(gauge, grainAxis, law, body.Exclusions, None, Some(body.Lot), body.Heat,
            CostRate(body));

    private static Option<double> CostRate(StockBody body) {
        double area = Math.Abs(body.Region.Sum(static loop => loop.Area()));
        return area > 0.0 ? Some(body.Cost / area) : None;
    }

    internal static Fin<Seq<(Loop Outer, Seq<Loop> Holes)>> ComponentsOf(Seq<Loop> region) =>
        region.IsEmpty || region.Exists(static loop => !Valid(loop))
            ? Fin.Fail<Seq<(Loop Outer, Seq<Loop> Holes)>>(
                new GeometryFault.DegenerateInput(Kind.Polyline, None, "remnant:region"))
            : PolygonAlgebra
                .Apply(new PolygonOp.Topology(region, PolygonFill.NonZero))
                .Bind(static trace => trace
                    .Regioned(new KernelFault.InvalidValue("remnant", "remnant:topology-trace"))
                    .Bind(Assemble))
                .Bind(static components => components.IsEmpty
                    ? Fin.Fail<Seq<(Loop Outer, Seq<Loop> Holes)>>(
                        new GeometryFault.DegenerateInput(Kind.Polyline, None, "remnant:outer"))
                    : Fin.Succ(components));

    private static Fin<Seq<(Loop Outer, Seq<Loop> Holes)>> Assemble(RegionTopology topology) {
        Map<int, RegionNode> byIndex = toMap(topology.Nodes.Map(static node => (node.Index, node)));
        return topology.Nodes
            .Filter(static node => node.IsHole)
            .Traverse(hole => hole.Parent
                .Bind(byIndex.Find)
                .Filter(static owner => !owner.IsHole)
                .Map(owner => (Hole: hole, Owner: owner.Index))
                .ToValidation(new GeometryFault.DegenerateInput(Kind.Polyline, hole.Index, "remnant:orphan-hole")))
            .As().ToFin()
            .Map(assignments => Ordered(topology.Nodes
                .Filter(static node => !node.IsHole)
                .Map(outer => (
                    Outer: outer.Boundary,
                    Holes: Ordered(assignments.Filter(row => row.Owner == outer.Index).Map(static row => row.Hole))))));
    }

    private static Seq<Loop> Ordered(Seq<RegionNode> nodes) =>
        toSeq(nodes.OrderBy(static node => Preimage(node.Boundary))).Map(static node => node.Boundary);

    private static Seq<(Loop Outer, Seq<Loop> Holes)> Ordered(Seq<(Loop Outer, Seq<Loop> Holes)> components) =>
        toSeq(components.OrderBy(static component => Preimage(component.Outer)));

    private static UInt128 Preimage(Loop loop) =>
        FabricationCanon.Ordered(loop.Tolerance, loop.CanonicalBytes);

    private static bool Valid(Loop loop) => loop.Closed && loop.Count >= 3
        && loop.Vertices.ForAll(static point => double.IsFinite(point.X) && double.IsFinite(point.Y) && double.IsFinite(point.Z))
        && loop.Bulges.ForAll(double.IsFinite);

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
        Option<double> aspect,
        double compactness,
        bool spans,
        bool duplicate,
        Instant now,
        RemnantInventory inventory) {
        Seq<RetireCause> causes = Causes(new RemnantAssessment(
            remnant, remnant.Profile, area, aspect, compactness, spans, duplicate, inventory.Material, inventory.Policy));
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
        Seq<RemnantRow> admitted = gated.Bind(static row => row.Verdict.Match(static _ => Seq<RemnantRow>(), Seq));
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
            ? Fin.Fail<RemnantPlan>(new KernelFault.InvalidValue("remnant", "remnant:claim"))
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
            ? Fin.Fail<RemnantPlan>(new KernelFault.InvalidValue("remnant", "remnant:close"))
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
                    : Fin.Fail<(double Area, Option<double> Aspect, double Compactness)>(
                        new KernelFault.InvalidValue("remnant", "remnant:consumption"))
                from remaining in Boolean(usable, use.Used, BoolKind.Not)
                from components in remaining.IsEmpty
                    ? Fin.Succ(Seq<(Loop Outer, Seq<Loop> Holes)>())
                    : ComponentsOf(remaining)
                let origin = new RemnantOrigin(
                    state.Row.Remnant.Origin.Stock,
                    Some(state.Row.Remnant.Identity),
                    state.Row.Remnant.Generation + 1)
                from recovered in components
                    .Traverse(component => Mint(
                        component.Outer,
                        component.Holes,
                        state.Row.Remnant.Material,
                        origin,
                        state.Row.Profile with { Exclusions = Seq<Loop>() }).ToValidation())
                    .As().ToFin()
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
            (Error)new KernelFault.InvalidValue("remnant", "remnant:observation")).ToFin()
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
                let policyCauses = Causes(new RemnantAssessment(
                    row.Remnant,
                    seen.Profile,
                    assessment.Area,
                    assessment.Aspect,
                    assessment.Compactness,
                    assessment.Spans,
                    Duplicate: false,
                    row.Remnant.Material,
                    policy))
                let causes = seen.Condition == RemnantCondition.Retire
                    ? policyCauses.Concat(Seq<RetireCause>(new RetireCause.Inspection(row.Key)))
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
                        Causes: Seq<RetireCause>(new RetireCause.Observation(row.ObservedAt, now)))),
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
                ? Validation<Error, Unit>.Success(unit)
                : Validation<Error, Unit>.Fail(error))
            .As().ToFin().Map(static _ => unit);
        return rowGate.Bind(_ => AdmitLineage(inventory, rows));
    }

    private static Fin<Unit> AdmitLineage(RemnantInventory inventory, RemnantRow[] rows) {
        Map<UInt128, RemnantRow> byIdentity = toSeq(rows).Fold(
            Map<UInt128, RemnantRow>(),
            static (map, row) => map.AddOrUpdate(row.Remnant.Identity, row));
        bool keyed = toSeq(inventory.Rows.Keys).ForAll(key => inventory.Rows.Find(key)
            .Exists(row => key == row.Remnant.Identity));
        BidirectionalGraph<UInt128, SEdge<UInt128>> lineage = new(allowParallelEdges: false);
        lineage.AddVertexRange(byIdentity.Keys);
        if (!keyed || byIdentity.Count != rows.Length
            || toSeq(rows).Exists(row => row.Remnant.Parent.Exists(parent => !byIdentity.ContainsKey(parent))))
            return Fin.Fail<Unit>(new KernelFault.InvalidValue("remnant", "remnant:lineage-parent"));
        toSeq(rows).Iter(row => row.Remnant.Parent
            .Iter(parent => lineage.AddEdge(new SEdge<UInt128>(parent, row.Remnant.Identity))));
        if (!lineage.IsDirectedAcyclicGraph())
            return Fin.Fail<Unit>(new KernelFault.InvalidValue("remnant", "remnant:lineage-cycle"));
        bool succession = toSeq(lineage.Edges).ForAll(edge =>
            (from parent in byIdentity.Find(edge.Source)
             from child in byIdentity.Find(edge.Target)
             select child.Remnant.Generation == parent.Remnant.Generation + 1
                 && child.Remnant.Origin.Stock == parent.Remnant.Origin.Stock).IfNone(false));
        bool roots = toSeq(rows).ForAll(static row => row.Remnant.Parent.IsSome == row.Remnant.Generation > 0);
        return toSeq(lineage.TopologicalSort()).Count == rows.Length && succession && roots
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new KernelFault.InvalidValue("remnant", "remnant:lineage"));
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
            : new KernelFault.InvalidValue("remnant", $"remnant:inventory:{row.Remnant.Identity}");
    }

    private static Fin<Seq<Loop>> Offset(Seq<Loop> loops, double distance) => loops.Head.Match(
        Some: anchor => ArcForest.Admit(loops, anchor.Tolerance, anchor.Plane)
            .Bind(forest => ArcAlgebra.Apply(new ArcOp.Offset(new ArcOffsetSource.Forest(forest), distance)))
            .Bind(ArcPaths),
        None: static () => Fin.Succ(Seq<Loop>()));

    private static Fin<Seq<Loop>> Boolean(Seq<Loop> subject, Seq<Loop> clip, BoolKind kind) => subject.Head.Match(
        Some: anchor => ArcForest.Admit(subject, anchor.Tolerance, anchor.Plane)
            .Bind(first => ArcForest.Admit(clip, first.Tolerance, first.Plane)
                .Bind(second => ArcAlgebra.Apply(new ArcOp.Boolean(first, second, kind))))
            .Bind(ArcPaths),
        None: static () => Fin.Succ(Seq<Loop>()));

    private static Fin<ArcRelation> Relation(Loop first, Loop second) =>
        ArcForest.Admit(Seq(first, second), first.Tolerance, first.Plane)
            .Bind(forest => ArcAlgebra.Apply(new ArcOp.Inspect(forest, new ArcProbe.Pair(first, second))))
            .Bind(static trace => trace is ArcTrace.Inspection { Evidence: ArcInspection.Pair pair }
                ? Fin.Succ(pair.Relation)
                : Fin.Fail<ArcRelation>(new KernelFault.InvalidValue("remnant", "remnant:relation-trace")));

    private static Fin<Loop> Lower(Loop loop, double error) =>
        ArcAlgebra.Densify(new ArcProjection.Lower(loop, error))
            .Bind(static trace => trace
                .Lowering(new KernelFault.InvalidValue("remnant", "remnant:projection-trace"))
                .Map(static evidence => evidence.Output));

    private static Fin<Seq<Loop>> ArcPaths(ArcTrace trace) => trace switch {
        ArcTrace.Forest forest => Fin.Succ(forest.Geometry.Loops),
        ArcTrace.Paths paths => Fin.Succ(paths.Geometry),
        _ => Fin.Fail<Seq<Loop>>(new KernelFault.InvalidValue("remnant", "remnant:arc-trace")),
    };

    private static Fin<(Seq<Loop> Usable, double Area, Option<double> Aspect, double Compactness, bool Spans)> Assess(
        Remnant remnant,
        RemnantProfile profile,
        ReusePolicy policy) =>
        from usable in Usable(remnant, profile, policy)
        from measure in Measure(usable, policy.ArcToleranceMm)
        from spanCore in Offset(usable, -0.5 * policy.MinReusableSpan.Millimeters)
        select (usable, measure.Area, measure.Aspect, measure.Compactness, !spanCore.IsEmpty);

    public readonly record struct RemnantAssessment(
        Remnant Remnant,
        RemnantProfile Profile,
        double Area,
        Option<double> Aspect,
        double Compactness,
        bool Spans,
        bool Duplicate,
        MaterialId Material,
        ReusePolicy Policy);

    private static readonly Seq<Func<RemnantAssessment, Option<RetireCause>>> ReuseGates = Seq<Func<RemnantAssessment, Option<RetireCause>>>(
        static assessment => assessment.Remnant.Material != assessment.Material
            ? Some<RetireCause>(new RetireCause.Material(assessment.Remnant.Material, assessment.Material))
            : None,
        static assessment => assessment.Duplicate
            ? Some<RetireCause>(new RetireCause.Duplicate(assessment.Remnant.Identity))
            : None,
        static assessment => assessment.Remnant.Generation > assessment.Policy.MaxGeneration
            ? Some<RetireCause>(new RetireCause.Generation(assessment.Remnant.Generation, assessment.Policy.MaxGeneration))
            : None,
        static assessment => assessment.Policy.MinGauge.Millimeters > 0.0
            && assessment.Profile.GaugeMm.Filter(gauge => gauge >= assessment.Policy.MinGauge.Millimeters).IsNone
                ? Some<RetireCause>(new RetireCause.Gauge(assessment.Profile.GaugeMm, assessment.Policy.MinGauge.Millimeters))
                : None,
        static assessment => assessment.Area < assessment.Policy.MinUsable.SquareMillimeters
            ? Some<RetireCause>(new RetireCause.AreaFloor(assessment.Area, assessment.Policy.MinUsable.SquareMillimeters))
            : None,
        static assessment => !assessment.Spans
            ? Some<RetireCause>(new RetireCause.FeatureWidth(assessment.Policy.MinReusableSpan.Millimeters))
            : None,
        static assessment => assessment.Aspect.Filter(aspect => aspect >= assessment.Policy.MinAspect.DecimalFractions).IsNone
            ? Some<RetireCause>(new RetireCause.SliverAspect(assessment.Aspect, assessment.Policy.MinAspect.DecimalFractions))
            : None,
        static assessment => assessment.Compactness < assessment.Policy.MinCompactness.DecimalFractions
            ? Some<RetireCause>(new RetireCause.Compactness(assessment.Compactness, assessment.Policy.MinCompactness.DecimalFractions))
            : None,
        static assessment => assessment.Profile.CostPerSquareMillimeter
            .Map(rate => rate * assessment.Area)
            .Filter(value => value < assessment.Policy.MinSalvageValue)
            .Map(value => (RetireCause)new RetireCause.Salvage(value, assessment.Policy.MinSalvageValue)));

    private static Seq<RetireCause> Causes(RemnantAssessment assessment) =>
        ReuseGates.Map(gate => gate(assessment)).Somes()
            .Concat(toSeq(ReuseTrait.Of(assessment.Profile).Missing(assessment.Policy.Required).Held
                    .OrderBy(static trait => trait.Rank))
                .Map(static trait => (RetireCause)new RetireCause.Traceability(trait)));

    private static Fin<Seq<Loop>> Usable(RemnantRow row, ReusePolicy policy) =>
        Usable(row.Remnant, row.Profile, policy);

    private static Fin<Seq<Loop>> Usable(Remnant remnant, RemnantProfile profile, ReusePolicy policy) =>
        Offset(remnant.Region, -policy.InsetMm).Bind(inset => profile.Exclusions.IsEmpty
            ? Fin.Succ(inset)
            : Boolean(inset, profile.Exclusions, BoolKind.Not));

    private static Fin<(double Area, Option<double> Aspect, double Compactness)> Measure(Seq<Loop> region, double tolerance) =>
        region.Traverse(loop => MeasureLoop(loop, tolerance))
            .As().ToFin().Bind(rows => {
                double area = Math.Abs(rows.Sum(static row => row.Area));
                double perimeter = rows.Sum(static row => row.Length);
                double compactness = perimeter == 0.0 ? 0.0 : Math.Min(1.0, (4.0 * Math.PI * area) / (perimeter * perimeter));
                return PolygonAlgebra.Apply(new PolygonOp.Calipers(rows.Map(static row => row.Polygon)))
                    .Bind(static trace => trace
                        .Envelope(new KernelFault.InvalidValue("remnant", "remnant:envelope-trace"))
                        .Map(static envelope => Some(envelope.Aspect)))
                    .BindFail(static error => error.IsType<GeometryFault.DegenerateInput>()
                        ? Fin.Succ(Option<double>.None)
                        : Fin.Fail<Option<double>>(error))
                    .Map(aspect => (area, aspect, compactness));
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
            (Error)new KernelFault.InvalidValue("remnant", "remnant:transition")).ToFin();

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
        toSeq(toSeq(inventory.Rows.Values.GroupBy(static row => row.Remnant.Origin.Stock))
            .Map(static group => Yield(group.Key, toSeq(group)))
            .OrderBy(static row => row.Stock));

    private static RemnantYield Yield(UInt128 stock, Seq<RemnantRow> rows) => new(
        stock,
        rows.Count,
        rows.Max(static row => row.Remnant.Generation),
        Total(rows.Filter(static row => !row.State.Terminal)),
        Total(rows.Filter(static row => row.State == RemnantState.Scrapped)));

    private static RemnantMeasure Total(Seq<RemnantRow> rows) =>
        rows.Fold(RemnantMeasure.Zero, static (measure, row) => measure + RemnantMeasure.Of(row));

    private static RemnantMeasure Prorated(RemnantRow row, double areaMm2) =>
        new(areaMm2, RemnantMeasure.Priced(row)
            .Map(value => row.UsableAreaMm2 <= 0.0 ? 0.0 : value * areaMm2 / row.UsableAreaMm2));

    internal static Fin<ContentKey> KeyOf(
        Loop boundary,
        Seq<Loop> holes,
        MaterialId material,
        RemnantOrigin origin) =>
        FabricationCanon.Keyed(EgressKind.Remnant, boundary.Tolerance, writer => writer
            .String(material.Value).U128(origin.Stock).Ordinal(origin.Generation)
            .Maybe(origin.Parent, static (held, parent) => held.U128(parent))
            .Rows(Seq(boundary), static (held, loop) => loop.CanonicalBytes(held))
            .Rows(Ordered(holes), static (held, hole) => hole.CanonicalBytes(held)),
            RemnantOp);


    private static Seq<Loop> Ordered(Seq<Loop> loops) => toSeq(loops.OrderBy(Preimage));

}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
