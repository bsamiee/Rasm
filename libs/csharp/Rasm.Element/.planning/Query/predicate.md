# [ELEMENT_PREDICATE]

`Rasm.Element.Query` declares the ONE boolean selection closure its consumer folders currently spell in parallel — each parallel spelling recomposes onto this seam at its own recompose pass under the S-E4 consumer-breaks rows, and none has recomposed yet. It carries `Predicate<TLeaf>` the generic `All`/`Any`/`Not`/`Closure` algebra over any leaf vocabulary, `ValueMatch` the typed restriction (IDS `ValueConstraint` lowered onto the seam `PropertyValue`), `NodeMatch<TLeaf>` the exact-id-or-recursive-pattern carrier, `MatchVerdict` the fault-carrying verdict surviving negation, `Selection<TKey>` the identity-parameterized result, `WalkDepth` the walk bound, and `ElementLeaf` the closed Element-payload leaf family.

`Rasm.Bim`, `Rasm.Persistence`, and `Rasm.AppUi` are S2/S2/S4 peers none of which can reference another — this seam is the lowest folder all three reach, so each instantiates `Predicate<TLeaf>` over its own leaf family at its recompose pass (`BimLeaf` wrapping `ElementLeaf`, Persistence's store-pushdown arms, AppUi's comparison rows) and one expression mixes vocabularies through the wrapping arm instead of through a fourth algebra. `Rasm.Compute`'s rule-population selector (`Solver/satisfy#RULE_POPULATION_DERIVATION` `NodeClassSelector.Admits`) is the same closure's kind-plus-classification conjunction spelled by hand — and edition-blind, a two-string System/Code compare colliding two editions of one code, exactly what the triple-identity `ByClassification` writer forecloses — so its SELECTION half recomposes as `All(ByKind, ByClassification)` over `ElementLeaf` while its symbol-binding roster stays Compute's own rule-grounding vocabulary, which no seam leaf models.

Stored `Func<…, bool>` query filters are banned because a delegate is opaque to store push-down and unhashable for a memo key. `PredicateKey` streams any predicate into the content key used by replayable selections and caches. Evaluation stays with each consumer: this page owns algebra, verdict, and native canonical bytes, never an interpreter, generated DTO, or parallel selection family.

## [01]-[INDEX]

- [02]-[PREDICATE_ALGEBRA]: `WalkDepth`, `MatchVerdict`, `RangeBound`, `ValueMatch`, `Predicate<TLeaf>` with `Open`/`And`/`Or`/`AndNot`/`Holds`, `NodeMatch<TLeaf>`, `ElementLeaf` the eleven Element-payload arms, `Selection<TKey>` the set-algebra result, and `PredicateKey` the canonical byte projection.

## [02]-[PREDICATE_ALGEBRA]

- Owner: `Predicate<TLeaf>` the `[Union]` boolean closure — `Leaf`, n-ary `All`/`Any`, `Not`, and `Closure(Seed, WalkDepth)` the transitive-walk arm no boolean combinator derives — with `Open` the NAMED vacuous conjunction (absence of a filter is a value, never a nullable beside one); `ValueMatch` the `[Union]` typed value restriction over the seam `PropertyValue` (`Present`/`Exact`/`Prefix`/`Pattern`/`Range`/`OneOf`/`Length`/`Digits` — the IDS facet set with the prefix arm the AppUi search consumer proves; `Exact` carries a TYPED `PropertyValue`, never a rendered string) carrying its own `Matches` evaluation under the IDS any-of spread law; `RangeBound` the inclusive/exclusive bound pair over `MeasureValue` with dimension-gated `AllowsLower`/`AllowsUpper`; `NodeMatch<TLeaf>` the `[Union]` exact-`NodeId`-or-recursive-pattern carrier; `MatchVerdict` the fault-carrying verdict (`Holds` + `Faults` that SURVIVE negation fail-closed, where a plain bool silently delivers or silently drops a malformed arm); `Selection<TKey>` the identity-parameterized result (`Keys` + the store's optional `Receipt`) with `Union`/`Intersect`/`Except`; `WalkDepth` the `[ValueObject<int>]` non-negative walk bound; `ElementLeaf` the closed leaf family whose eleven arms carry ONLY Element-owned payloads; `PredicateKey` the canonical byte projection every memo key and replayable selection streams through.
- Entry: predicates BUILD as values — `new Predicate<TLeaf>.Leaf(value)`, `p.And(q)`/`Or`/`AndNot` (n-ary arms coalesce, so chained conjunction stays one `All`), `Predicate<TLeaf>.Open` the match-everything default; `p.Holds(leaf, closure)` is the ONE structural fold — the caller supplies the leaf verdict and the closure verdict (each consumer owns its walk: Bim's graph frontier, Persistence's recursive CTE, an H3 ring), `All` folds `And` over its operands (empty = `Pass`, the vacuous conjunction), `Any` folds `Or` (empty holds nothing), `Not` negates fail-closed with faults retained; `ValueMatch.Matches(value)` decides one restriction against one `PropertyValue` under the spread law; `ValueMatch.Pattern.Of(expression, key)` rails an uncompilable regex at construction and every minted `Pattern` re-resolves ONE cached anchored `NonBacktracking` compiled instance; `ValueMatch.Reaches(value)` classifies whether a restriction can meaningfully test a value's case (the picker gate — `Range` reaches measures and numerics, `Prefix`/`Pattern`/`Length` reach rendered text, `Digits` reaches measures alone (the XSD facet decides over the canonical numeric rendering), `Present`/`Exact`/`OneOf` reach every case); `Selection<TKey>.Union`/`Intersect`/`Except` compose selections over one key regime; `PredicateKey.Key(predicate, leaf)` streams the closure and the caller's leaf bytes into one seam `ContentAddress`, and `ElementLeaf.CanonicalBytes` is the Element-vocabulary leaf writer, so an `ElementLeaf` predicate keys with zero extra work.
- Auto: `MatchVerdict.And`/`Or` accumulate faults from BOTH sides and `Negate` flips only a CLEAN verdict — a malformed arm (an unreachable restriction, an unresolved leaf) keeps `Holds` false through any surrounding `Not` while its faults ride the verdict out, so a subscription filter fails closed with evidence instead of silently delivering; `Matches` spreads a multi-valued candidate (enumerated/list, RECURSIVELY — a list nested in an enumerated flattens) BEFORE the restriction decides and `Present` short-circuits on existence ahead of the spread (the IDS any-of law — a `Pattern` never false-matches across a joined-list render, an `Exact` reaches the member); numeric equality decides at the IDS relative tolerance in SI value space, never a bit compare and never a rendered-string compare; `Closure` composes `WalkDepth` so a bounded k-ring, a bounded containment descent, and an unbounded-until-fixpoint walk (`WalkDepth.Whole`) are one arm with the bound recoverable from the value.
- Law: the leaf vocabulary stays at ITS owner — `ElementLeaf` carries the arms whose payloads this folder declares (`ObjectKind`, `Classification`, `ValueMatch`, `ComposeKind`/`ConnectKind`/`VoidKind`/`AssignKind`, `WireName`, `Discipline`/`AssessmentOutcome`); Bim's IFC-schema arms, Persistence's store-pushdown arms, and AppUi's comparison rows are THEIR leaf families over this one closure, and Bim's family wraps `ElementLeaf` in one `Element(...)` arm so a mixed expression is one value.
- Law: `ByClassification` carries the RESOLVED branch closure as payload (`Seq<Classification>` — a single classification is a one-element branch): classification hierarchy is bSDD-resolved at `Rasm.Bim`, the seam carries identity and never ancestry, so the resolver hands the closed branch IN and no seam-side crosswalk exists to be structurally blind.
- Law: `WalkDepth` resolves to ONE declaration — this seam's; Persistence `Query/lane#ELEMENT_SET_ALGEBRA` composes it directly (its former same-named `[ValueObject<int>]` is deleted, and the `SelectionFault.Depth` band re-keys at its `Selections.Depth` wrapper over the seam admission); Persistence's evaluator law binds every consumer's closure verdict — a `Closure` arm is the QUESTION and the answering fold must be a genuine bounded transitive walk, never an opaque-leaf pass-through.
- Receipt: `Selection<TKey>.Receipt` is the STORE's certification of a store-answered selection (Persistence's preimage-keyed receipt survives as its own wrapper columns); a set-algebra derivation answers `Receipt: None` because the derived set was never store-certified — the consumer re-keys through `PredicateKey.Key` where it needs one.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`/`[ValueObject<int>]`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Error`), Generator.Equals (`[Equatable]`/`[OrderedEquality]` on the stored value shapes), `Rasm` (`Op.Catch` preserving the regex-construction cause, the kernel writer through the seam `ContentAddress`), `Properties/property#PROPERTY_VALUE` (`PropertyValue`/`PropertyName` the restriction operands), `Properties/quantity#MEASURE_ALGEBRA` (`MeasureValue` the range bound, `MeasureCanon.Measure` the dimensioned bound writer), `Relations/relation#EDGE_ALGEBRA` (`ComposeKind`/`ConnectKind`/`VoidKind`/`AssignKind`/`WireName`), `Classification/classification#CLASSIFICATION_AXIS` (`Classification`/`Discipline`), `Assessment/assessment#ASSESSMENT_NODE` (`AssessmentOutcome`), `Projection/address#CONTENT_ADDRESS` (the streamed key).
- Growth: a new Element query dimension is one `ElementLeaf` arm and one `CanonicalBytes` case; a new restriction is one `ValueMatch` arm; a consumer-specific dimension is one arm on its own leaf family. No generated predicate projection, parallel closure, stored delegate, or second verdict shape participates.
- Boundary: this page is host-neutral vocabulary and projection; graph walks, store lowering, and UI compilation stay with consumers. `Selection<TKey>` carries no model scope because scope belongs to the query input. `PredicateKey` is the sole durable byte projection, and `MatchVerdict` remains distinct from host verdict vocabularies.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using Generator.Equals;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Query;

// --- [TYPES] ------------------------------------------------------------------------------
// WalkDepth bounds the transitive walk the Closure arm and a store k-ring both carry: 0 is the seed set itself, Whole the
// walk-to-fixpoint sentinel row (int.MaxValue — stated, never spelled at a call site). ONE declaration corpus-wide:
// Persistence Query/lane declares a same-named twin today and recomposes THIS owner at W3, its SelectionFault.Depth
// band transferring to a wrapper over this admission (consumer-breaks row).
[ValueObject<int>]
[ValidationError]
public readonly partial struct WalkDepth {
 public static readonly WalkDepth Whole = Create(int.MaxValue);

 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
  validationError = value >= 0 ? null
   : new ValidationError("walk depth must be non-negative");
}

// MatchVerdict carries faults: a malformed arm is a REFUSAL that survives combination AND negation, where a plain
// bool silently delivers or silently drops. Named past the Verdict collisions (GH canvas, Rhino SeatVerdict).
public readonly record struct MatchVerdict(bool Holds, Seq<Error> Faults) {
 public static readonly MatchVerdict Pass = new(true, Seq<Error>());
 public static MatchVerdict Of(bool holds) => new(holds, Seq<Error>());
 public static MatchVerdict Fault(Error cause) => new(false, Seq(cause));

 public MatchVerdict And(MatchVerdict other) => new(Holds && other.Holds, Faults + other.Faults);
 public MatchVerdict Or(MatchVerdict other) => new(Holds || other.Holds, Faults + other.Faults);

 // Negate flips only a CLEAN verdict: a faulted arm stays non-matching through any surrounding Not (fail-closed),
 // because "the malformed restriction did not hold" must never read as "its negation delivers".
 public MatchVerdict Negate() => new(!Holds && Faults.IsEmpty, Faults);
}

// RangeBound admits inclusively or exclusively over a dimensioned measure: a bound only ADMITS a candidate sharing its dimension —
// a cross-dimension compare answers false rather than comparing raw magnitudes that mean nothing together.
[Union]
public abstract partial record RangeBound {
 private RangeBound() { }

 public sealed record Inclusive(MeasureValue Value) : RangeBound;
 public sealed record Exclusive(MeasureValue Value) : RangeBound;

 public MeasureValue Bound => Switch(inclusive: static b => b.Value, exclusive: static b => b.Value);

 public bool AllowsLower(MeasureValue candidate) => Switch(
  state: candidate,
  inclusive: static (value, bound) => SameDimension(value, bound.Value) && value.Si >= bound.Value.Si,
  exclusive: static (value, bound) => SameDimension(value, bound.Value) && value.Si > bound.Value.Si);

 public bool AllowsUpper(MeasureValue candidate) => Switch(
  state: candidate,
  inclusive: static (value, bound) => SameDimension(value, bound.Value) && value.Si <= bound.Value.Si,
  exclusive: static (value, bound) => SameDimension(value, bound.Value) && value.Si < bound.Value.Si);

 // Canonical identity writes through the ONE dimensioned writer (quantity#MEASURE_STAT MeasureCanon — type token +
 // SI magnitude + 7-vector + band): a bare-magnitude write minted byte-identical keys for `Length >= 5` and
 // `Mass >= 5`, and generated-total dispatch replaces the hand ternary a third case would silently misfile.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  state: w,
  inclusive: static (wr, b) => { wr.Ordinal(0).Measure(b.Value); },
  exclusive: static (wr, b) => { wr.Ordinal(1).Measure(b.Value); });

 private static bool SameDimension(MeasureValue left, MeasureValue right) => left.Dimension == right.Dimension;
}

// ValueMatch types the value restriction — the IDS ValueConstraint facet family lowered onto the seam
// PropertyValue, with the Prefix arm the AppUi search consumer proves. Matches carries the ONE evaluation law:
// a multi-valued candidate (enumerated/list) SPREADS recursively before the restriction decides (the IDS any-of
// law — a Pattern never false-matches across a joined-list render, an Exact reaches the member); Present decides
// on EXISTENCE and short-circuits ahead of the spread; numeric equality decides at the IDS relative tolerance in
// SI value space — a kernel `Tolerance` minted on the `Relative` lane, never a bare double, a bit compare, or a
// rendered-string compare.
[Union]
public abstract partial record ValueMatch {
 // The IDS relative value-space tolerance ELECTED onto the kernel owner (the last bare tolerance double at this
 // seam — Bim's twin deleted with its page): the lane is `ToleranceLane.Relative` (Band.Ratio, dimensionless) —
 // a RELATIVE compare in SI value space, never a model-scaled distance — so no `Context` threads here and the
 // 1e-6 figure is the IDS standard's OWN (provenance this facet family holds, per the kernel standards-figure
 // law) and a declaration-total value already proven inside the Relative lane's band. A query-bound predicate is
 // context-free by construction; a consumer wanting a project-tightened compare overrides the LANE in its own
 // Context and never this seat.
 private static readonly Tolerance RealTolerance = new(ToleranceLane.Relative, 1e-6);

 private ValueMatch() { }

 public sealed record Present : ValueMatch;
 // Exact carries a TYPED PropertyValue: equality is the value family's own (an Enumerated, List, Table, or Complex
 // candidate compares structurally), with the Measure pair alone deciding at tolerance in SI value space — a
 // rendered-string ordinal compare collapsed distinct typed evidence sharing one spelling.
 public sealed record Exact(PropertyValue Value) : ValueMatch;
 public sealed record Prefix(string Value) : ValueMatch;

 public sealed record Pattern : ValueMatch {
  private Pattern(string expression) { Expression = expression; }

  public string Expression { get; }

  public static Fin<ValueMatch> Of(string expression, Op key) =>
   key.Catch(() => Fin.Succ(CompiledPatterns.GetOrAdd(expression, static pattern =>
     new Regex($@"\A(?:{pattern})\z", RegexOptions.NonBacktracking | RegexOptions.CultureInvariant))))
    .Map(_ => (ValueMatch)new Pattern(expression));

  // ONE compile site behind the admission (the Bim CompiledPatterns law carried whole): ANCHORED whole-value
  // (\A(?:…)\z — an IDS/XSD pattern facet is a whole-value match, never a substring), NonBacktracking (linear-time,
  // so a hostile foreign pattern can never ReDoS-hang the fold and no exception-shaped timeout throws out of a bool
  // fold), CultureInvariant, cached per expression so Matches never recompiles per candidate. Of is the only
  // constructor and populates the cache before minting; every held Pattern therefore owns this total read.
  private static readonly ConcurrentDictionary<string, Regex> CompiledPatterns = new();

  internal static Regex Compiled(string expression) => CompiledPatterns[expression];
 }

 public sealed record Range(Option<RangeBound> Lower, Option<RangeBound> Upper) : ValueMatch;
 public sealed record OneOf(Seq<string> Allowed) : ValueMatch;
 public sealed record Length(Option<int> Min, Option<int> Max) : ValueMatch;
 // xs:totalDigits / xs:fractionDigits over the canonical numeric rendering.
 public sealed record Digits(Option<int> Total, Option<int> Fraction) : ValueMatch;

 public static readonly ValueMatch Any = new Present();

 // Reaches classifies whether this restriction can MEANINGFULLY test a value's case — the picker gate a UI reads before offering an
 // operator, so an unsatisfiable term is unbuildable rather than silently false: Range reaches dimensioned and
 // numeric cases, the text facets reach the rendered cases, and the existence/identity facets reach every case.
 public bool Reaches(PropertyValue value) => Switch(
  state: value,
  present: static (_, _) => true,
  exact: static (_, _) => true,
  prefix: static (v, _) => v is not (PropertyValue.Complex or PropertyValue.Table or PropertyValue.Reference),
  pattern: static (v, _) => v is not (PropertyValue.Complex or PropertyValue.Table or PropertyValue.Reference),
  range: static (v, _) => v is PropertyValue.Measure or PropertyValue.Integer or PropertyValue.Number or PropertyValue.Bounded,
  oneOf: static (_, _) => true,
  length: static (v, _) => v is not (PropertyValue.Complex or PropertyValue.Table or PropertyValue.Reference),
  digits: static (v, _) => v is PropertyValue.Measure);

 // Matches is the ONE evaluation entry per restriction against one seam value.
 public bool Matches(PropertyValue value) => this is Present || Spread(value).Exists(Decide);

 // Spread flattens enumerated/list candidates to members RECURSIVELY (a list nested in an enumerated selection
 // flattens through both); every other case is its own single candidate.
 private static Seq<PropertyValue> Spread(PropertyValue value) => value switch {
  PropertyValue.Enumerated e => e.Selected.Bind(Spread),
  PropertyValue.List l => l.Values.Bind(Spread),
  _ => Seq(value),
 };

 // Decide dispatches generated-total (no catch-all over the owned family — Present is its own arm even though the Matches
 // short-circuit makes it unreachable here, the generator-contract totality proof).
 private bool Decide(PropertyValue candidate) => Switch(
  state: candidate,
  present: static (_, _) => true,
  exact: static (c, m) => c is PropertyValue.Measure a && m.Value is PropertyValue.Measure b
   ? a.Value.Dimension == b.Value.Dimension && Real(a.Value.Si, b.Value.Si)
   : c.Equals(m.Value),
  prefix: static (c, m) => c.Render().StartsWith(m.Value, StringComparison.Ordinal),
  pattern: static (c, m) => Pattern.Compiled(m.Expression).IsMatch(c.Render()),
  range: static (c, m) => c is PropertyValue.Measure measure
   && m.Lower.ForAll(b => b.AllowsLower(measure.Value)) && m.Upper.ForAll(b => b.AllowsUpper(measure.Value)),
  // XSD enumeration equality is VALUE-SPACE equality: a Measure candidate parses each allowed literal invariant and
  // compares SI magnitudes at tolerance; every other candidate compares its Render ordinal, case-SENSITIVE (the
  // xbim IsSatisfiedBy(ignoreCase: false) default — an ignore-case fold admits a token the schema rejects).
  oneOf: static (c, m) => c is PropertyValue.Measure { Value: var mv }
   ? m.Allowed.Exists(a => double.TryParse(a, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) && Real(d, mv.Si))
   : m.Allowed.Exists(a => string.Equals(a, c.Render(), StringComparison.Ordinal)),
  length: static (c, m) => c.Render().Length is int chars
   && m.Min.ForAll(floor => chars >= floor) && m.Max.ForAll(ceiling => chars <= ceiling),
  // XSD totalDigits/fractionDigits decide over the CANONICAL numeric rendering — the "R" invariant of the SI
  // magnitude, sign excluded (Math.Abs), total counting SIGNIFICANT digits (0.123 carries three, not four) — with a
  // non-measure candidate and a scientific rendering (magnitude past any digits facet) never satisfying.
  digits: static (c, m) => c is PropertyValue.Measure measure
   && Math.Abs(measure.Value.Si).ToString("R", CultureInfo.InvariantCulture) is var text
   && !text.AsSpan().ContainsAny('E', 'e')
   && text.IndexOf('.') is var point
   && m.Total.ForAll(t => text.Count(char.IsAsciiDigit) - (text.StartsWith("0.", StringComparison.Ordinal) ? 1 : 0) <= t)
   && m.Fraction.ForAll(f => (point < 0 ? 0 : text.Length - point - 1) <= f));

 private static bool Real(double left, double right) =>
  Math.Abs(left - right) <= RealTolerance.Value * Math.Max(Math.Abs(left), Math.Abs(right));

 public void CanonicalBytes(CanonicalWriter w) => Switch(
  state: w,
  present: static (wr, _) => { wr.Ordinal(0); },
  exact: static (wr, m) => { wr.Ordinal(1); m.Value.CanonicalBytes(wr); },
  prefix: static (wr, m) => { wr.Ordinal(2).String(m.Value); },
  pattern: static (wr, m) => { wr.Ordinal(3).String(m.Expression); },
  range: static (wr, m) => {
   wr.Ordinal(4).Optional(m.Lower, static (b, x) => b.CanonicalBytes(x)).Optional(m.Upper, static (b, x) => b.CanonicalBytes(x));
  },
  oneOf: static (wr, m) => { wr.Ordinal(5).Rows(m.Allowed, static (v, x) => x.String(v)); },
  length: static (wr, m) => {
   wr.Ordinal(6).Optional(m.Min, static (v, x) => x.Ordinal(v)).Optional(m.Max, static (v, x) => x.Ordinal(v));
  },
  digits: static (wr, m) => {
   wr.Ordinal(7).Optional(m.Total, static (v, x) => x.Ordinal(v)).Optional(m.Fraction, static (v, x) => x.Ordinal(v));
  });
}

// Predicate<TLeaf> closes the ONE boolean algebra over ANY leaf family. Open is the NAMED vacuous conjunction; Closure the transitive
// walk no boolean combinator derives (Persistence's recursive CTE, Bim's containment descent, an H3 k-ring — the
// WALK is the consumer's, the ARM is this owner's, and the answering fold must be a GENUINE bounded transitive
// walk under the Persistence evaluator law, never an opaque-leaf pass-through). The n-ary combinators coalesce, so
// chained conjunction stays one All and a byte projection carries the flat operand run.
[Union]
public abstract partial record Predicate<TLeaf> where TLeaf : notnull {
 private Predicate() { }

 public sealed record Leaf(TLeaf Value) : Predicate<TLeaf>;
 public sealed record All(Seq<Predicate<TLeaf>> Operands) : Predicate<TLeaf>;
 public sealed record Any(Seq<Predicate<TLeaf>> Operands) : Predicate<TLeaf>;
 public sealed record Not(Predicate<TLeaf> Operand) : Predicate<TLeaf>;
 public sealed record Closure(Predicate<TLeaf> Seed, WalkDepth Depth) : Predicate<TLeaf>;

 public static readonly Predicate<TLeaf> Open = new All(Seq<Predicate<TLeaf>>());

 public Predicate<TLeaf> And(Predicate<TLeaf> other) =>
  this is All all ? new All(all.Operands.Add(other)) : new All(Seq(this, other));

 public Predicate<TLeaf> Or(Predicate<TLeaf> other) =>
  this is Any any ? new Any(any.Operands.Add(other)) : new Any(Seq(this, other));

 public Predicate<TLeaf> AndNot(Predicate<TLeaf> other) => And(new Not(other));

 // Holds runs the ONE structural fold: the caller supplies the leaf verdict and the closure verdict (each consumer owns its
 // walk), All folds And (empty = Pass, the vacuous conjunction Open exists to name), Any folds Or (empty holds
 // nothing), Not negates fail-closed with faults RETAINED.
 public MatchVerdict Holds(Func<TLeaf, MatchVerdict> leaf, Func<Closure, MatchVerdict> closure) => Switch(
  leaf: l => leaf(l.Value),
  all: all => all.Operands.Fold(MatchVerdict.Pass, (acc, p) => acc.And(p.Holds(leaf, closure))),
  any: any => any.Operands.Fold(MatchVerdict.Of(false), (acc, p) => acc.Or(p.Holds(leaf, closure))),
  not: not => not.Operand.Holds(leaf, closure).Negate(),
  closure: closure);
}

// NodeMatch carries an exact join or a nested pattern — the recursion carrier a topological arm (composed-of, connected-to, voided-by)
// takes so "connected to THIS node" and "connected to anything matching P" are one column.
[Union]
public abstract partial record NodeMatch<TLeaf> where TLeaf : notnull {
 private NodeMatch() { }

 public sealed record Exact(NodeId Id) : NodeMatch<TLeaf>;
 public sealed record Where(Predicate<TLeaf> Pattern) : NodeMatch<TLeaf>;
}

// ElementLeaf closes the Element-payload arms: every payload is a vocabulary THIS folder declares, so the family seats
// here and every peer reaches it. Bim's BimLeaf wraps it in one Element(...) arm (its IFC-schema arms ride
// beside); Persistence and AppUi instantiate their own families over the same closure. ByClassification carries
// its RESOLVED branch closure — hierarchy is bSDD-resolved at Rasm.Bim (E-E15), the seam carries identity never
// ancestry, so the resolver hands the closed branch IN.
[Union]
public abstract partial record ElementLeaf {
 private ElementLeaf() { }

 public sealed record ByKind(ObjectKind Kind) : ElementLeaf;
 public sealed record ByClassification(Seq<Classification> Branch) : ElementLeaf;
 public sealed record ByAttribute(ValueMatch Name, ValueMatch Restriction) : ElementLeaf;
 public sealed record ByProperty(ValueMatch Set, ValueMatch Name, ValueMatch Restriction) : ElementLeaf;
 public sealed record ByMaterial(ValueMatch Restriction) : ElementLeaf;
 public sealed record ByComposed(ComposeKind SubKind, NodeMatch<ElementLeaf> Whole) : ElementLeaf;
 public sealed record ByConnected(NodeMatch<ElementLeaf> Other, Option<ConnectKind> Kind) : ElementLeaf;
 public sealed record ByVoided(VoidKind SubKind, NodeMatch<ElementLeaf> Other) : ElementLeaf;
 public sealed record ByGeneric(WireName Wire, NodeMatch<ElementLeaf> Other) : ElementLeaf;
 public sealed record ByAssessment(Discipline Discipline, Option<AssessmentOutcome> Outcome) : ElementLeaf;
 // ByAssigned parameterizes the Assign-edge incidence on the AssignKind vocabulary (relation#EDGE_ALGEBRA): the
 // type-definition and group rows serve BimLeaf.OfType/InZone (the IDS partOf Grouped lowering at
 // Review/validation.md consumes the Group row), and a kind-suffixed sibling pair was the rejected arity twin.
 public sealed record ByAssigned(AssignKind Kind, NodeMatch<ElementLeaf> Other) : ElementLeaf;

 // CanonicalBytes is the Element-vocabulary leaf writer PredicateKey composes: each arm writes its frozen ordinal then its payload
 // through the owning vocabulary's own canonical spelling, so an ElementLeaf predicate keys with zero extra work
 // and a peer family owns exactly one writer of its own. Classification identity is the (System, Code, Edition)
 // TRIPLE, so the branch row writes all three — a two-column write collides two editions of one code.
 public void CanonicalBytes(CanonicalWriter w) => Switch(
  state: w,
  byKind: static (wr, m) => { wr.Ordinal(0).String(m.Kind.Key); },
  byClassification: static (wr, m) => {
   wr.Ordinal(1).Rows(m.Branch, static (c, x) => x.String(c.System).String(c.Code).String(c.Edition));
  },
  byAttribute: static (wr, m) => { wr.Ordinal(2); m.Name.CanonicalBytes(wr); m.Restriction.CanonicalBytes(wr); },
  byProperty: static (wr, m) => {
   wr.Ordinal(3); m.Set.CanonicalBytes(wr); m.Name.CanonicalBytes(wr); m.Restriction.CanonicalBytes(wr);
  },
  byMaterial: static (wr, m) => { wr.Ordinal(4); m.Restriction.CanonicalBytes(wr); },
  byComposed: static (wr, m) => { wr.Ordinal(5).String(m.SubKind.Key); PredicateKey.Node(m.Whole, wr); },
  byConnected: static (wr, m) => {
   wr.Ordinal(6).Optional(m.Kind, static (k, x) => x.String(k.Key)); PredicateKey.Node(m.Other, wr);
  },
  byVoided: static (wr, m) => { wr.Ordinal(7).String(m.SubKind.Key); PredicateKey.Node(m.Other, wr); },
  byGeneric: static (wr, m) => { wr.Ordinal(8).String(m.Wire.Value); PredicateKey.Node(m.Other, wr); },
  byAssessment: static (wr, m) => {
   wr.Ordinal(9).String(m.Discipline.Key).Optional(m.Outcome, static (o, x) => x.String(o.Key));
  },
  byAssigned: static (wr, m) => { wr.Ordinal(10).String(m.Kind.Key); PredicateKey.Node(m.Other, wr); });
}

// --- [MODELS] -----------------------------------------------------------------------------
// Selection<TKey> parameterizes the result identity: Bim instantiates NodeId (its graph-bound wrapper carries the
// ElementGraph so Bake stays railed), Persistence its content SetKey (its wrapper carries scope + preimage).
// Receipt is the STORE's certification of a store-answered selection; every set-algebra derivation answers None
// because the derived set was never store-certified — a consumer wanting a key re-streams PredicateKey.Key.
[Equatable]
public readonly partial record struct Selection<TKey>([property: OrderedEquality] Seq<TKey> Keys, Option<UInt128> Receipt) where TKey : notnull {
 public int Count => Keys.Count;

 public Selection<TKey> Union(Selection<TKey> other) =>
  new((Keys + other.Keys).Distinct().Strict(), None);

 public Selection<TKey> Intersect(Selection<TKey> other) {
  var held = toHashSet(other.Keys);
  return new(Keys.Filter(held.Contains).Strict(), None);
 }

 public Selection<TKey> Except(Selection<TKey> other) {
  var held = toHashSet(other.Keys);
  return new(Keys.Filter(key => !held.Contains(key)).Strict(), None);
 }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// PredicateKey owns the canonical byte projection: the closure frames itself (frozen arm ordinals, count-framed
// operand runs) and leaf bytes come from the instantiating family's own writer — so any predicate value streams into the
// seam content key a replayable selection and a memo share, which is the structural proof a stored delegate filter
// can never join. No generated predicate projection participates.
public static class PredicateKey {
 public static ContentAddress Key<TLeaf>(Predicate<TLeaf> predicate, Action<TLeaf, CanonicalWriter> leaf) where TLeaf : notnull =>
  ContentAddress.Of((predicate, leaf), 0.0, static (state, w) => Write(state.predicate, state.leaf, w));

 public static ContentAddress Key(Predicate<ElementLeaf> predicate) =>
  Key(predicate, static (value, w) => value.CanonicalBytes(w));

 // Write dispatches generated-total over the closed closure — no catch-all arm exists to absorb a sixth case silently.
 public static void Write<TLeaf>(Predicate<TLeaf> predicate, Action<TLeaf, CanonicalWriter> leaf, CanonicalWriter w) where TLeaf : notnull =>
  predicate.Switch(
   state: (Leaf: leaf, Writer: w),
   leaf: static (s, l) => { s.Writer.Ordinal(0); s.Leaf(l.Value, s.Writer); },
   all: static (s, all) => { s.Writer.Ordinal(1).Rows(all.Operands, (p, x) => Write(p, s.Leaf, x)); },
   any: static (s, any) => { s.Writer.Ordinal(2).Rows(any.Operands, (p, x) => Write(p, s.Leaf, x)); },
   not: static (s, not) => { s.Writer.Ordinal(3); Write(not.Operand, s.Leaf, s.Writer); },
   closure: static (s, walk) => { s.Writer.Ordinal(4).Ordinal(walk.Depth.Value); Write(walk.Seed, s.Leaf, s.Writer); });

 // Node writes the NodeMatch half the ElementLeaf topological arms compose.
 internal static void Node(NodeMatch<ElementLeaf> match, CanonicalWriter w) =>
  match.Switch(
   state: w,
   exact: static (wr, exact) => { wr.Ordinal(0).String(exact.Id.Value); },
   where: static (wr, where) => { wr.Ordinal(1); Write(where.Pattern, static (v, x) => v.CanonicalBytes(x), wr); });
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
