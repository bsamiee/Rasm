# [BIM_PROJECTION_FIDELITY]

`Rasm.Bim` owns the round-trip fidelity ledger every IFC exchange half accumulates: the named bounded-drop vocabulary, the fact stream, the `FidelityLog` monoid that stream folds on, and the `WriterT<FidelityLog, Fin, A>` carrier the `Projection/*` lowerings return on. A drop is a RETURNED fact beside its value, never a side effect, so "which drops, how many, on which entities" is per-exchange evidence a version stores rather than a prose promise.

The carrier is LanguageExt `WriterT<FidelityLog, Fin, A>` — the evidence channel stacked over the failure rail, so an accumulating ledger and a refusing rail stay separate currencies and no log folds into a fault payload. `FidelityDrop` rows are legislated here and told at their lowering sites; the ingress lowerings are `Projection/semantic#SEMANTIC_PROJECTOR` and `Projection/value#PROPERTY_LOWERING`, the relation fold `Projection/relations#RELATION_ALGEBRA`, the material narrowing `Semantics/composition#MATERIAL_COMPOSITION`, and the egress re-author `Projection/egress#IFC_EGRESS`. `Review/versioning#VERSION_GRAPH` carries the receipt as commit metadata, excluded from the commit's own content key.

## [01]-[INDEX]

- [02]-[FIDELITY_LEDGER]: `FidelityDrop`/`FidelityFact`/`FidelityLog`/`Fidelity` — the drop vocabulary, its fact stream, the monoid the stream folds on and derives its census from, and the writer carrier every drop-capable lowering returns.

## [02]-[FIDELITY_LEDGER]

- Owner: `FidelityDrop` the `[SmartEnum<string>]` naming every bounded drop the exchange halves legislate — one row per drop LAW, so a drop is a counted, anchor-bearing observable and an unnamed loss is unrepresentable; `FidelityFact` one occurrence, the row naming the law and the anchor naming the entity (`GlobalId`, set name, or wire name) a federation manager acts on; `FidelityLog` the `Monoid<FidelityLog>` over the ordered fact stream — `Combine` is fact concatenation, `Empty` the identity, and the order is the exchange chronology, so the monoid is associative and NON-commutative by design, its `Counts` census DERIVED from the same stream; `Fidelity` the carrier vocabulary — `Clean`/`Drop`/`Lift`/`Run` over `WriterT<FidelityLog, Fin, A>`.
- Entry: `Fidelity.Clean(value)` lifts a lossless step; `Fidelity.Drop(row, anchor, value)` tells one fact beside the value it kept; `Fidelity.Told(log, value)` tells a whole ledger a collaborator returned beside its value; `Fidelity.Lift(rail)` admits a `Fin`-returning step into the carrier; `Fidelity.Run(writer)` is the ONE egress, yielding `Fin<(A Value, FidelityLog Log)>` at the fold edge.
- Law: accumulation is the fold's OWN state — a lowering returns its log beside its value and the parent's `Bind`/`Traverse` `Combine`s the children's, so one fact has one owner. A mutable field beside a returned value gives one fact two owners: the field's write order and the value's, which diverge the moment a fold is re-run, partially run, or run twice on one instance.
- Receipt: the run's own `FidelityLog` is the per-exchange evidence `Review/versioning#VERSION_GRAPH` seals beside the version its ingest produced — carried metadata, never identity, so a re-ingest with different drops still content-keys the same model.
- Packages: LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new bounded drop is one `FidelityDrop` row with one `Fidelity.Drop` at its lowering site; a new drop-capable lowering returns the carrier and needs no ledger of its own; a new census axis derives off `Facts`, never as a stored column.
- Boundary: the ledger accumulates what merely HAPPENED and refuses nothing — a refusal is the `Fin` rail carrying a `Model/faults#FAULT_BAND` `BimFault`, and folding a drop into a fault payload (or a fault into the log) is the deleted form that made a recoverable narrowing indistinguishable from a malformed model; a drop row names a LAW, so an anchor-only fact with no row, or a row minted at a call site, is the deleted form; the log is read only through `Run` at the fold edge or `WriterT.listen` mid-fold, never through a struct field, because the carrier is delegate-backed; the `Fidelity` members PIN the three carrier type arguments a hundred call sites otherwise repeat, which is what separates them from a bare rename wrapper, and a second stored artifact beside `Facts` (the retired per-drop count record) is the mirror this derivation deletes.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using LanguageExt;
using LanguageExt.Traits;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FidelityDrop {
    public static readonly FidelityDrop StringIdentity       = new("string-identity");
    public static readonly FidelityDrop MeasureUnmapped      = new("measure-unmapped");
    public static readonly FidelityDrop MeasureFlattened     = new("measure-flattened");
    public static readonly FidelityDrop ReferenceResource    = new("reference-resource");
    public static readonly FidelityDrop GroupFactor          = new("group-factor");
    public static readonly FidelityDrop EccentricityDegraded = new("eccentricity-degraded");
    public static readonly FidelityDrop LinearPlacement      = new("linear-placement");
    public static readonly FidelityDrop AssessmentSkipped    = new("assessment-skipped");
    public static readonly FidelityDrop PredefinedPsetOpaque = new("predefined-pset-opaque");
    public static readonly FidelityDrop StructuralResidue    = new("structural-residue");
    public static readonly FidelityDrop SafResidue           = new("saf-residue");
    public static readonly FidelityDrop GeoLevelLowered      = new("geo-level-lowered");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct FidelityFact(FidelityDrop Drop, string Anchor);

public readonly record struct FidelityLog(Seq<FidelityFact> Facts) : Monoid<FidelityLog> {
    public static FidelityLog Empty { get; } = new(Seq<FidelityFact>());

    public static FidelityLog Of(FidelityDrop drop, string anchor) => new(Seq(new FidelityFact(drop, anchor)));

    public FidelityLog Combine(FidelityLog rhs) => new(Facts.Concat(rhs.Facts));

    public static FidelityLog operator +(FidelityLog left, FidelityLog right) => left.Combine(right);

    public Map<FidelityDrop, int> Counts =>
        Facts.Fold(Map<FidelityDrop, int>(), static (map, fact) => map.AddOrUpdate(fact.Drop, static n => n + 1, static () => 1));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Fidelity {
    public static WriterT<FidelityLog, Fin, A> Clean<A>(A value) => WriterT.pure<FidelityLog, Fin, A>(value);

    public static WriterT<FidelityLog, Fin, A> Drop<A>(FidelityDrop drop, string anchor, A value) =>
        Told(FidelityLog.Of(drop, anchor), value);

    public static WriterT<FidelityLog, Fin, A> Told<A>(FidelityLog log, A value) =>
        WriterT.write<FidelityLog, Fin, A>(value, log);

    public static WriterT<FidelityLog, Fin, A> Lift<A>(Fin<A> rail) => WriterT.lift<FidelityLog, Fin, A>(rail);

    public static Fin<(A Value, FidelityLog Log)> Run<A>(WriterT<FidelityLog, Fin, A> writer) => writer.Run().As();
}
```

## [03]-[RESEARCH]

(none)
