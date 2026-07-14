# [RASM_FABRICATION_REMNANT]

The offcut LIFECYCLE owner: `Nesting/nfp#NESTING` MINTS each `Remnant` (the kerf-inflated Boolean-difference producer stamping content identity, `Parent` lineage, and the `Generation` re-nest depth), and THIS page owns everything that happens to a remnant AFTER its mint — cross-job stocking, reuse admission, reservation, consumption, retirement, and the inventory audit — through the ONE polymorphic `Remnant.Reconcile` fold over the `RemnantOp` `[Union]` (`Stocking` absorbs single and batch on one `Seq` case; `Claim` is the reservation edge and the `RemnantStale` 2736 producer; `Release` closes a reservation to consumed or back to stocked; `Sweep` is the cross-job physical-inventory audit). The type is ONE record in two concern halves: the nfp page declares the mint half (`Of`/`From`/`Holds` plus the `Generation` mint column), this page the lifecycle partial (`Reconcile`/`Stockable` and the reuse gates) — one `Remnant`, one namespace, never a parallel `Offcut`/`RemnantManager` sibling. The inventory is INPUT-CARRIED exactly like the owner's `ResidualStock` truth-carry: `Reconcile` takes the current `RemnantInventory` (the typed ledger + admission-policy owner — raw map carriage never crosses the seam) and returns the NEXT inventory inside the `RemnantPlan` receipt — run N's reconcile feeds run N+1's inventory, and no mutable registry exists anywhere on this plane. The ledger is MATERIAL-SCOPED: `RemnantInventory` carries the one `MaterialId` its rows share, so a steel offcut can never be offered to an acrylic job — the caller keeps one inventory lane per material, and the scoping is a structural column, never a runtime filter a consumer must remember. The ruled batch seam is `Remnant.Reconcile(Seq<Remnant>, RemnantInventory) → RemnantPlan`; the claim/release/sweep edges ride the SAME canonical name through the op-discriminated overload, modality living in the request shape.

Reuse admission is a policy TABLE, never code-path constants: `ReusePolicy` rows carry the kerf re-trim margin (the burned edge a reused offcut re-trims), the re-grip margin (the clamp/gripper band a re-fixtured plate reserves), the minimum-usable-area floor, the sliver aspect floor, and the generation cap (a remnant-of-a-remnant chain retires at the traceability bound); the usable interior is the boundary INSET through the one `Geometry2D/algebra#POLYGON_ALGEBRA` `Offset` owner, and an admitted row's stockable child re-mints through `Remnant.Of` with the stocked remnant as `Parent` — the content-address law holds (a trimmed boundary is a NEW identity, a re-labeled one is the named defect). The reuse-depth gate reads the MINT-STAMPED `Generation` column: nfp's `From` counts `FromRemnant` ancestry structurally, so a virgin-sheet offcut is generation 0, a re-nested offcut generation N+1, and NO ledger walk exists — the old parent-chain walk could not resolve a virgin stock digest or a re-minted stockable child (neither is a ledger row) and silently retired every real offcut; the `Parent` column survives as pure audit lineage. Every retirement is a TYPED verdict: `RetireCause` names why a candidate refused (inset failure, area floor, sliver aspect, generation cap, sweep-missing), so the `RemnantPlan.Retired` rows are an audit trail a shop can act on, never an unexplained drop.

Three seams stay exactly where the sibling pages put them. The `remnant` `EgressKind`/Persistence `ArtifactKind` enrollment RIDES nfp (the mint page); this page's ledger rows key the SAME `ContentKey(EgressKind.Remnant, identity)` — lifecycle here, mint there, one durable key, never a second enrollment. The rectangular per-sheet offcuts on `Nesting/stock#STOCK_NEST`'s `NestYield` stay YIELD EVIDENCE (procurement ledger rows) and never enter this lifecycle — only true-shape minted `Remnant` polygons do; the two owners stay disjoint. Reuse REDUCES the Compute waste rollup as a RECORDED seam: reclaimed usable area subtracts from the same `NestWasteArea` frozen-key wire value the landed `Rasm.Compute` counterpart decodes (`ElementQuantity.WasteAreaM2`/`NestWasteM2`, SI m²), the quantity-bag lowering riding the deferred `FabricationProjector` exactly as the stock rollup does — recorded-only, no Compute edit, never a direct sustainability read. Staleness versus depth is the fault law's split: `Generation` gates reuse admission (a too-deep offcut retires as a verdict row), while a `Claim` against a key the ledger cannot resolve — or a row already terminal — routes the typed `FabricationFault.RemnantStale` 2736, never a silent virgin-sheet substitution.

Wire posture: HOST-LOCAL. The ledger crosses only the in-process seam — `Stockable` projects stocked rows as `Stock.FromRemnant` inventory for the next `FabricationInput.Inventory`, and the `ContentKey` rows are the persisted-decode contract the Persistence artifact index already carries via nfp's enrollment; no type on this page sits between wire and rail.

## [01]-[INDEX]

- [01]-[REMNANT_LIFECYCLE]: owns the `RemnantState` transition vocabulary, the `RetireCause` verdict vocabulary, the `ReusePolicy` admission table, the material-scoped `RemnantRow`/`RemnantInventory`/`RemnantPlan` ledger model, the `RemnantOp` `[Union]`, and the lifecycle partial of `Remnant` — the one `Reconcile` fold (the ruled batch seam plus the op-discriminated claim/release/sweep overload) and the `Stockable` inventory projection the next nest consumes.

## [02]-[REMNANT_LIFECYCLE]

- Owner: `RemnantState` `[SmartEnum<string>]` the lifecycle axis (`minted` → `stocked` → `reserved` → `consumed` | `scrapped`) with the `Terminal` flag and the expression-shaped `Admits` transition relation — a terminal row admits nothing; `RetireCause` `[SmartEnum<string>]` the typed retirement verdict (`inset-failed`/`area-floor`/`sliver-aspect`/`generation-cap`/`sweep-missing`); `ReusePolicy` the admission table (kerf re-trim, re-grip margin, usable-area floor, sliver aspect floor, generation cap) with `Flatbed`/`Plate` seed rows and the derived `InsetMm`; `RemnantRow` the ledger row binding the minted `Remnant`, its `State`, its `ContentKey` (the SAME key nfp's enrollment mints), its mint-stamped `Generation`, the inset `Usable` interior with its area, and the reserving job; `RemnantOp` `[Union]` the reconcile request (`Stocking`/`Claim`/`Release`/`Sweep`); `RemnantInventory` the typed MATERIAL-SCOPED inventory owner (`MaterialId` lane + ledger rows + admission policy) crossing the public seam in place of raw map carriage; `RemnantPlan` the receipt carrying the NEXT inventory plus the shifted/retired/stale evidence and the reclaimed-area scalars; `Remnant` (lifecycle partial) the static surface owning `Reconcile` and `Stockable`.
- Cases: `RemnantState` rows 5, transitions {minted→stocked, minted→scrapped, stocked→reserved, stocked→scrapped, reserved→consumed, reserved→stocked} — the relation is total over the row pairs and everything else is refused; `RetireCause` rows 5 — every refused candidate names its gate; `RemnantOp` cases 4 — `Stocking(Seq<Remnant>)` (single and batch on ONE case, arity absorbed by the Seq), `Claim(ContentKey, int job)`, `Release(ContentKey, bool consumed)`, `Sweep(Seq<ContentKey> observed)`; `ReusePolicy` seed rows 2 (`flatbed` thin-sheet thermal reuse; `plate` heavy-plate re-grip reuse) — a new machine class is one policy row, never a branch.
- Entry: `public static RemnantPlan Remnant.Reconcile(Seq<Remnant> minted, RemnantInventory inventory)` — THE ruled batch seam (stocking never faults: a rejected candidate is a RETIRED row with its typed cause); `public static Fin<RemnantPlan> Remnant.Reconcile(RemnantOp op, RemnantInventory inventory)` — the SAME canonical name carrying the full lifecycle through the generated total `Switch`, `Fin<T>` routing `FabricationFault.RemnantStale` 2736 on an unresolvable or terminal claim/release key; `public static Seq<Stock> Remnant.Stockable(RemnantInventory inventory)` — the inventory projection re-minting each stocked row's usable interior as a lineage-stamped child (`Remnant.Of(usable, Some(identity), row.Generation)` — a trim of the same physical piece keeps its generation; the NEXT nest's difference-mint increments it) wrapped `Stock.FromRemnant`, feeding the next run's `FabricationInput.Inventory` so the landed nfp scheduler packs real offcuts before virgin sheets — one material lane per inventory, so the projection is material-correct by construction.
- Auto: `Stocking` gates each minted remnant — inset the boundary by `InsetMm` through `PolygonAlgebra.Offset` (negative delta, `OffsetEnds.Polygon`), measure the usable interior through `PolygonAlgebra.Area`, read the mint-stamped `Generation`, and admit as a `stocked` row or retire with its `RetireCause` under the area/aspect/generation rows; `Claim` shifts a resolvable non-terminal row to `reserved` stamping the job, `Release` closes it to `consumed` or returns it to `stocked`; `Sweep` retires every live row whose identity is absent from the observed physical inventory (`RetireCause.SweepMissing`) and reports the stale keys; every arm returns the next inventory INSIDE the plan — the caller carries it forward, the run-N→run-N+1 loop closing exactly as the owner's truth-carry fields do. The `Shifted` receipt seq carries the claim/release row that moved (the stocking arms leave it empty); `Admitted`/`Retired` are the stocking verdicts.
- Receipt: `RemnantPlan` — the next `RemnantInventory` plus `Admitted` rows, `Retired` `(row, RetireCause)` verdicts, `Shifted` claim/release rows, `Stale` keys, `ReclaimedAreaMm2` (the usable area entering stock), and `WasteReductionMm2` (the recorded `NestWasteArea` wire reduction the Compute rollup decodes downstream); the rows ARE the audit trail, no parallel event log.
- Packages: `Rasm.Fabrication.Geometry2D` (`PolygonAlgebra.Offset`/`Area` — the one inset/metric owner), `Process/owner#FABRICATION_OWNER` (`Loop`/`EgressKind`/`ContentKey` — the egress spine this ledger keys through), `Process/faults#FAULT_BAND` (`RemnantStale` 2736), `Rasm.Element` (`MaterialId` — the inventory lane scope), the sibling `Nesting/nfp#NESTING` `Remnant` mint half + `Stock.FromRemnant`, Thinktecture.Runtime.Extensions, LanguageExt.Core (`Fin`/`Map`/`Seq`/`Option`), `Rhino.Geometry` (`BoundingBox`), BCL inbox.
- Growth: a new admission gate is one `ReusePolicy` column read inside `Gate` plus one `RetireCause` row; a new lifecycle station is one `RemnantState` row plus its `Admits` pairs; a new reconcile mode is one `RemnantOp` case plus one `Switch` arm (the generated dispatch breaking the build until the arm lands); a defect-zone mask on a stocked remnant is one `RemnantRow` column the gate subtracts; zero new entrypoints.
- Boundary: this page is the ONE lifecycle owner and a `RemnantManager`/`OffcutService`/inventory-singleton sibling is the deleted form — the ledger travels input-carried and no mutable registry exists; the mint stays nfp's (`Remnant.Of`/`From` — this page never re-derives a difference or re-hashes a boundary) and the yield stays stock's (rectangular `NestYield` offcuts never enter this lifecycle); reuse depth reads the mint-stamped `Generation` and a ledger-walked parent chain is the named unresolvable-parent defect (a virgin stock digest and a re-minted stockable child are never ledger rows, so the walk retired every real offcut); the stockable child RE-MINTS through `Remnant.Of` with `Parent` lineage and its carried generation, and a re-labeled boundary keeping a stale identity is the named content-address defect; the inventory is material-scoped by column and a cross-material `Stockable` offer is structurally impossible — a runtime material filter a consumer must remember is the deleted form; the durable key is the SAME `ContentKey(EgressKind.Remnant, identity)` nfp enrolls — a second enrollment row or a parallel remnant digest is the second-hasher defect; staleness is the typed `RemnantStale` fault on the CLAIM edge and a silent fallback to virgin stock is the deleted form; every retirement carries its `RetireCause` and an unexplained drop is the deleted form; the waste-rollup reduction is recorded-only on the frozen `NestWasteArea` wire and a Fabrication-side `Rasm.Compute` reference is the forbidden strata edge; the type is one record in two declared halves and a third partial site is the split-brain defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Element;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Nesting;

// --- [TYPES] ------------------------------------------------------------------------------
// Lifecycle axis: Terminal rows admit nothing; the transition relation is expression-shaped and
// total over the row pairs — everything unlisted is refused, never an imperative state machine.
[SmartEnum<string>]
public sealed partial class RemnantState {
    public static readonly RemnantState Minted = new("minted", terminal: false);
    public static readonly RemnantState Stocked = new("stocked", terminal: false);
    public static readonly RemnantState Reserved = new("reserved", terminal: false);
    public static readonly RemnantState Consumed = new("consumed", terminal: true);
    public static readonly RemnantState Scrapped = new("scrapped", terminal: true);

    public bool Terminal { get; }

    public bool Admits(RemnantState next) =>
        !Terminal && ((this == Minted && (next == Stocked || next == Scrapped))
                   || (this == Stocked && (next == Reserved || next == Scrapped))
                   || (this == Reserved && (next == Consumed || next == Stocked)));
}

// Every retirement names its gate: the Retired rows are an actionable audit trail, never an unexplained drop.
[SmartEnum<string>]
public sealed partial class RetireCause {
    public static readonly RetireCause InsetFailed = new("inset-failed");
    public static readonly RetireCause AreaFloor = new("area-floor");
    public static readonly RetireCause SliverAspect = new("sliver-aspect");
    public static readonly RetireCause GenerationCap = new("generation-cap");
    public static readonly RetireCause SweepMissing = new("sweep-missing");
}

// --- [MODELS] -----------------------------------------------------------------------------
// Reuse admission table: every gate a row datum. InsetMm is the burned-edge re-trim PLUS the
// re-grip clamp band — the usable interior a reused offcut actually offers.
public sealed record ReusePolicy(double KerfTrimMm, double RegripMarginMm, double MinUsableAreaMm2, double AspectFloor, int MaxGeneration) {
    public static readonly ReusePolicy Flatbed = new(KerfTrimMm: 1.0, RegripMarginMm: 15.0, MinUsableAreaMm2: 10_000.0, AspectFloor: 0.05, MaxGeneration: 3);
    public static readonly ReusePolicy Plate = new(KerfTrimMm: 2.0, RegripMarginMm: 25.0, MinUsableAreaMm2: 40_000.0, AspectFloor: 0.10, MaxGeneration: 2);

    public double InsetMm => KerfTrimMm + RegripMarginMm;
}

// One ledger row per remnant: Key is the SAME ContentKey nfp's Persistence enrollment mints —
// lifecycle here, mint there, one durable key. Generation is the MINT-stamped re-nest depth;
// Usable is the InsetMm interior the reuse offers.
public sealed record RemnantRow(Remnant Remnant, RemnantState State, ContentKey Key, int Generation, Loop Usable, double UsableAreaMm2,
    Option<int> ReservedJob);

// Single and batch stocking are ONE Seq case; Claim/Release are the reservation edges; Sweep the
// cross-job physical-inventory audit.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RemnantOp {
    private RemnantOp() { }

    public sealed record Stocking(Seq<Remnant> Minted) : RemnantOp;
    public sealed record Claim(ContentKey Key, int JobId) : RemnantOp;
    public sealed record Release(ContentKey Key, bool Consumed) : RemnantOp;
    public sealed record Sweep(Seq<ContentKey> Observed) : RemnantOp;
}

// The typed MATERIAL-SCOPED inventory owner at the public seam — one ledger lane per MaterialId, so a steel
// offcut is structurally unofferable to an acrylic job; raw Map carriage never crosses the seam. Input-carried
// like the owner's truth fields.
public sealed record RemnantInventory(MaterialId Material, Map<UInt128, RemnantRow> Rows, ReusePolicy Policy) {
    public static RemnantInventory Empty(MaterialId material, ReusePolicy policy) => new(material, Map<UInt128, RemnantRow>(), policy);
}

// The plan carries the NEXT inventory (input-carried, run N feeds run N+1 — the owner's truth-carry
// discipline) beside the typed audit evidence: Admitted stocking rows, Retired (row, cause) verdicts,
// Shifted claim/release rows, Stale swept keys. WasteReductionMm2 is the recorded NestWasteArea wire
// reduction the landed Compute rollup decodes downstream.
public sealed record RemnantPlan(RemnantInventory Next, Seq<RemnantRow> Admitted, Seq<(RemnantRow Row, RetireCause Cause)> Retired,
    Seq<RemnantRow> Shifted, Seq<ContentKey> Stale, double ReclaimedAreaMm2, double WasteReductionMm2);

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The lifecycle partial of the nfp-minted record: Of/From/Holds and the Generation mint column declare on
// Nesting/nfp; Reconcile/Stockable and the reuse gates declare HERE — one type, two concern halves.
public sealed partial record Remnant {
    // THE ruled batch seam: minted offcuts + current inventory in, the reconciled plan out. Stocking never
    // faults — a refused candidate retires with its typed cause — so the ruled shape is a bare RemnantPlan.
    public static RemnantPlan Reconcile(Seq<Remnant> minted, RemnantInventory inventory) =>
        Stock(minted, inventory);

    // The full lifecycle surface — the SAME canonical name, modality in the request shape: Claim/Release/
    // Sweep ride the op union and can fault (RemnantStale 2736); the Stocking case shares the batch interior.
    public static Fin<RemnantPlan> Reconcile(RemnantOp op, RemnantInventory inventory) =>
        op.Switch(
            state:    inventory,
            stocking: static (inv, o) => Fin.Succ(Stock(o.Minted, inv)),
            claim:    static (inv, o) => inv.Rows.Find(o.Key.Digest)
                .Filter(row => row.State.Admits(RemnantState.Reserved))
                .Match(
                    Some: row => Fin.Succ(Shift(inv, row with { State = RemnantState.Reserved, ReservedJob = Some(o.JobId) })),
                    None: () => Fin.Fail<RemnantPlan>(FabricationFault.RemnantStale(o.Key).ToError())),
            release:  static (inv, o) => inv.Rows.Find(o.Key.Digest)
                .Filter(static row => row.State == RemnantState.Reserved)
                .Match(
                    Some: row => Fin.Succ(Shift(inv, row with { State = o.Consumed ? RemnantState.Consumed : RemnantState.Stocked, ReservedJob = None })),
                    None: () => Fin.Fail<RemnantPlan>(FabricationFault.RemnantStale(o.Key).ToError())),
            sweep:    static (inv, o) => Fin.Succ(Audit(inv, toSet(o.Observed.Map(static k => k.Digest)))));

    // The next-inventory projection: each stocked row's usable interior RE-MINTS as a lineage-stamped child
    // (content-address law — a trimmed boundary is a new identity) carrying the row's generation (a trim of the
    // same physical piece is not a re-nest; the NEXT difference-mint increments), wrapped Stock.FromRemnant.
    public static Seq<Stock> Stockable(RemnantInventory inventory) =>
        inventory.Rows.Values.ToSeq()
            .Filter(static row => row.State == RemnantState.Stocked)
            .Map(static row => (Stock)new Stock.FromRemnant(Of(row.Usable, Some(row.Remnant.Identity), row.Remnant.Generation)));

    static RemnantPlan Stock(Seq<Remnant> minted, RemnantInventory inventory) {
        Seq<(Remnant R, Either<RetireCause, RemnantRow> Verdict)> gated = minted.Map(r => (r, Gate(r, inventory)));
        Seq<RemnantRow> admitted = gated.Bind(static g => g.Verdict.Match(
            Right: static row => Seq1(row),
            Left: static _ => Seq<RemnantRow>()));
        Seq<(RemnantRow Row, RetireCause Cause)> retired = gated.Bind(g => g.Verdict.Match(
            Right: static _ => Seq<(RemnantRow, RetireCause)>(),
            Left: cause => Seq1((new RemnantRow(g.R, RemnantState.Scrapped, KeyOf(g.R), g.R.Generation, g.R.Boundary, 0.0, None), cause))));
        Map<UInt128, RemnantRow> next = admitted.Concat(retired.Map(static r => r.Row))
            .Fold(inventory.Rows, static (m, row) => m.AddOrUpdate(row.Remnant.Identity, row));
        double reclaimed = admitted.Sum(static a => a.UsableAreaMm2);
        return new RemnantPlan(inventory with { Rows = next }, admitted, retired, Seq<RemnantRow>(), Seq<ContentKey>(), reclaimed, reclaimed);
    }

    // The reuse gate: inset through the ONE Geometry2D offset owner, then the generation / area / sliver-aspect
    // admission rows — a refused candidate retires with its TYPED cause, never a fault and never an unexplained
    // drop. Generation is the mint-stamped column; no ledger walk exists.
    static Either<RetireCause, RemnantRow> Gate(Remnant r, RemnantInventory inventory) =>
        r.Generation > inventory.Policy.MaxGeneration
            ? Left<RetireCause, RemnantRow>(RetireCause.GenerationCap)
            : PolygonAlgebra.Offset(Seq(r.Boundary), -inventory.Policy.InsetMm, OffsetEnds.Polygon).ToOption()
                .Bind(static loops => loops.HeadOrNone())
                .Match(
                    None: () => Left<RetireCause, RemnantRow>(RetireCause.InsetFailed),
                    Some: usable => Math.Abs(PolygonAlgebra.Area(usable)) is var area && area < inventory.Policy.MinUsableAreaMm2
                        ? Left<RetireCause, RemnantRow>(RetireCause.AreaFloor)
                        : Aspect(usable) < inventory.Policy.AspectFloor
                            ? Left<RetireCause, RemnantRow>(RetireCause.SliverAspect)
                            : Right<RetireCause, RemnantRow>(
                                new RemnantRow(r, RemnantState.Stocked, KeyOf(r), r.Generation, usable, area, None)));

    static RemnantPlan Audit(RemnantInventory inventory, Set<UInt128> observed) {
        Seq<RemnantRow> stale = inventory.Rows.Values.ToSeq().Filter(row => !row.State.Terminal && !observed.Contains(row.Remnant.Identity));
        Map<UInt128, RemnantRow> next = stale.Fold(inventory.Rows, static (m, row) => m.AddOrUpdate(row.Remnant.Identity, row with { State = RemnantState.Scrapped }));
        return new RemnantPlan(inventory with { Rows = next }, Seq<RemnantRow>(),
            stale.Map(static row => (row with { State = RemnantState.Scrapped }, RetireCause.SweepMissing)),
            Seq<RemnantRow>(), stale.Map(static row => row.Key), 0.0, 0.0);
    }

    static RemnantPlan Shift(RemnantInventory inventory, RemnantRow row) =>
        new(inventory with { Rows = inventory.Rows.AddOrUpdate(row.Remnant.Identity, row) },
            Seq<RemnantRow>(), Seq<(RemnantRow, RetireCause)>(), Seq1(row), Seq<ContentKey>(), 0.0, 0.0);

    static double Aspect(Loop usable) {
        BoundingBox b = usable.Bound();
        double w = b.Diagonal.X, h = b.Diagonal.Y;
        return Math.Min(w, h) / Math.Max(1e-9, Math.Max(w, h));
    }

    static ContentKey KeyOf(Remnant r) => new(EgressKind.Remnant, r.Identity);
}
```

```mermaid
---
config:
  layout: elk
  theme: base
---
flowchart LR
    Mint["nfp Remnant.From kerf-difference mint (Generation stamped)"] -->|"Stocking(Seq)"| Gate["Gate: generation · inset · area · aspect"]
    Gate -->|admitted| Stocked["stocked row (ContentKey = nfp enrollment key)"]
    Gate -->|"refused → (row, RetireCause)"| Scrapped["scrapped (typed verdict, no fault)"]
    Stocked -->|"Claim(key, job)"| Reserved["reserved"]
    Reserved -->|"Release(consumed: true)"| Consumed["consumed"]
    Reserved -->|"Release(consumed: false)"| Stocked
    Stocked -->|"Sweep unobserved → sweep-missing"| Scrapped
    Claim["Claim on missing/terminal key"] -.->|2736| Stale["FabricationFault.RemnantStale"]
    Stocked -->|"Stockable → Stock.FromRemnant (re-minted child, generation carried)"| Inventory["next FabricationInput.Inventory (one MaterialId lane)"]
    Gate -->|ReclaimedAreaMm2| Waste["NestWasteArea wire reduction (recorded Compute seam)"]
```
