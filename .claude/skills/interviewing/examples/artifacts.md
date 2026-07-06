# [ARTIFACT_CRAFT]

A durable artifact seals a ruling for a future session to consult as law; an instance that hedges its ruling, drops a reopen condition, fakes a multi-tier fan, leaks a plan into a horizon, or asserts a gap by assumption fails the cold read a year later. Each entry names one instance defect across the five durable kinds under the fixed Detection / Rejected / Accepted / Reason / Reframe card; the Rejected and Accepted bodies are schema-conformant fragments using each template's leader and field grammar.

## [01]-[HEDGED_RULING]

A decision record's Ruling field carries a preference weighing options instead of the single owning sentence the record exists to seal.

- Detection: A `Ruling` that hedges — a comparative, a conditional, an option still weighed — leaving the downstream author to re-litigate the decision the record exists to close.
- Rejected:
  ```markdown
  - [07]-[ACCEPTED]: Graph carrier ownership
    - Context: Three backends sit behind a string union no type-checker resolves.
    - Options: single owner | per-backend owner
    - Ruling: A single owner is probably cleaner, though a per-backend split could also work depending on the algorithm set.
  ```
- Accepted:
  ```markdown
  - [07]-[ACCEPTED]: Graph carrier ownership
    - Context: Three backends sit behind a string union no type-checker resolves; the third arm is GPL.
    - Drivers: Strict-typing law; GPL isolation; algorithm coverage.
    - Options: single tagged owner | per-backend owner
    - Ruling: `GraphCarrier` is one tagged-union owner; each backend is a resolvable tag and the GPL backend is confined to an optional build lane.
    - Consequence: Every operation dispatches on the tag; the GPL lane stays out of the default wheel and the `igraph`-only algorithms move behind it.
    - Confirmation: The carrier type-checks under strict mode and the default build excludes the GPL tag.
    - Premises: fact: `<unit>/planning/graph.md:55` carries the string union today.
  ```
- Reason: A record binds downstream by asserting one resolution as law; a hedged ruling binds nothing and forces the authoring pass to re-decide what the interview already paid for.
- Reframe: Compress the ruling to one owning sentence in the indicative, move the weighing into `Drivers` and `Options`, and record the losing branch under `Rejected` with its argument.

## [02]-[REJECTION_WITHOUT_REOPEN]

A rejected option records the argument that lost it but not the condition that revives it, so a future session cannot tell a closed door from a permanent wall.

- Detection: A `Rejected` field naming the loser and its defeat with no `reopens when` clause, stranding the reader who hits the exact condition that revives it.
- Rejected:
  ```markdown
  - [07]-[ACCEPTED]: Graph carrier ownership
    - Ruling: `GraphCarrier` is one tagged-union owner over three backends.
    - Rejected: per-backend owner — proliferates one concept across three surfaces against the collapse law.
  ```
- Accepted:
  ```markdown
  - [07]-[ACCEPTED]: Graph carrier ownership
    - Ruling: `GraphCarrier` is one tagged-union owner over three backends.
    - Rejected: per-backend owner — proliferates one concept across three surfaces against the collapse law; reopens when a backend's algorithm set diverges so far that a shared traversal contract costs more than three owners.
  ```
- Reason: The reopen condition is the record's forward value — it tells a future maintainer whether the ground has shifted; without it the rejection reads as dogma and gets re-argued from zero.
- Reframe: Append `reopens when <condition>` to every rejection that stays useful, naming the observable that revives the losing option.

## [03]-[SINGLE_TIER_COSTUMES]

A direction set dresses one idea in three costumes — three variations at the same tier — instead of spanning the scope-time spectrum the fan exists to open.

- Detection: Every direction shares a tier and a thesis, differing only in a knob; the set offers no real fork between a patch, a structural fix, and a rebuild.
- Rejected:
  ```markdown
  - [01]-[STRUCTURAL]: Tagged union carrier
  - [02]-[STRUCTURAL]: Tagged union with a resolver cache
  - [03]-[STRUCTURAL]: Tagged union with lazy backend load
  ```
- Accepted:
  ```markdown
  - [01]-[PATCH]: Type the union in place
    - Thesis: Replace the string union with a tagged union at its one call site; smallest change that restores type-checking.
    - Cost: One page; leaves the GPL lane in the default build.
    - Kills: Nothing structural — a later split stays open.
    - Reversibility: Full.
    - Evidence: `<unit>/planning/graph.md:55` is the sole carrier site.
    - Confidence: high
  - [02]-[STRUCTURAL]: Split the GPL backend out
    - Thesis: Confine the GPL backend to an optional lane; the core carries only permissive backends.
    - Cost: A build-lane split and a re-home of the `igraph`-only algorithms.
    - Kills: A single default wheel carrying every algorithm.
    - Reversibility: Partial — the lane split is sticky.
    - Evidence: The GPL license on the carrier's third arm.
    - Confidence: medium
  - [03]-[REBUILD]: Backend-agnostic graph service
    - Thesis: Own graph compute as a service admitting backends by capability, not by type.
    - Cost: A new owner and a capability registry.
    - Kills: Direct backend access from consumers.
    - Reversibility: Low.
    - Evidence: No current consumer needs runtime backend selection.
    - Confidence: low
  ```
- Reason: A set of variations on one thesis hides the real decision — how far to reach — behind a knob nobody needs; a true set spans tiers so the user rules on scope, not on tuning.
- Reframe: Generate directions across at least two tiers with materially different theses, attack each with the evidence inconsistent with it, and record thesis, cost, kills, reversibility, and confidence per direction.

## [04]-[DATED_HORIZON]

A roadmap horizon carries a dated task list — a plan leaking upward — where the schema demands an outcome and the condition that promotes it.

- Detection: A horizon entry whose `Bet` or body sequences dated work — week one, then week two — instead of naming a change in behavior and how attainment is observed.
- Rejected:
  ```markdown
  - [01]: Ship the graph carrier
    - Bet: Week 1 write the tagged union; week 2 wire the resolver; week 3 add the tests.
  ```
- Accepted:
  ```markdown
  - [01]: Graph carrier type-checks under strict mode
    - Why: The string union blocks every downstream graph consumer from strict typing.
    - Bet: A tagged-union owner with a per-backend resolver.
    - Measure: The carrier passes strict type-check and consumers drop their `Any` casts.
    - Confidence: The carrier has one call site today.
  - [02]: The GPL backend leaves the default wheel
    - Why: The default build ships a GPL surface it does not need.
    - Bet: An optional build lane for the GPL backend.
    - Measure: The default wheel excludes the GPL tag.
    - Promote: A consumer names a GPL-only algorithm it depends on, or the license audit flags the default wheel.
  ```
- Reason: Horizons are confidence bands, not schedules; a dated task list belongs to the plan that its execution consumes, and lodging it in the roadmap freezes a sequence the roadmap has no authority to hold.
- Reframe: State each entry as an outcome with its measure, move dated mechanics down to the plan, and give every `[NEXT]` entry a `Promote` condition that carries it into `[NOW]`.

## [05]-[ANCHORLESS_FINDING]

A blindspot finding records a feeling with no anchor, so a reader cannot verify it, size its blast radius, or fold it back into the owning task.

- Detection: A finding whose `Anchor` is a vibe — "feels under-typed", "seems fragile" — with no path, no observed fact, and a consequence stated as a guess.
- Rejected:
  ```markdown
  - [01]-[APPROACH]-[OPEN]: The graph carrier feels under-typed
    - Anchor: general impression from reading the page.
    - Consequence: might cause problems later.
  ```
- Accepted:
  ```markdown
  - [01]-[CAPABILITY_WEAKNESS]-[OPEN]: Graph carrier types as a string union
    - Anchor: `<unit>/planning/graph.md:55` — `AnyGraph = "RxGraph | NxGraph | igraph.Graph"`, a string no type-checker resolves.
    - Consequence: Every consumer casts through `Any`, strict mode misses a wrong-backend call, and the GPL backend rides the default surface unmarked; the blast reaches every graph consumer.
    - Fold-back: Retype `GraphCarrier` as a tagged union, confine the GPL tag to an optional lane, and drop the consumer `Any` casts.
    - Route: The graph carrier owner.
  ```
- Reason: A finding earns entry by surviving on-disk verification — the anchor is the evidence and the blast radius is the rank; a vibe carries no order, no proof, and no landing surface.
- Reframe: Re-open the candidate on disk, record the path and the observed fact as `Anchor`, size the blast in `Consequence`, and write the `Fold-back` as the copyable prompt that lands the fix.

## [06]-[UNMARKED_ASSUMPTION]

An assumption enters a record dressed as a fact, stripping the invalidating condition a future session needs to know when the ruling expired.

- Detection: A `Premises` mark reading `fact` on a claim no anchor supports — a future consumer's behavior, an unbuilt seam — with no condition that falsifies it.
- Rejected:
  ```markdown
  - [03]-[ACCEPTED]: Impact search stays deferred
    - Premises: fact: a downstream consumer drives the impact search sweep.
  ```
- Accepted:
  ```markdown
  - [03]-[ACCEPTED]: Impact search stays deferred
    - Premises: assumption: a downstream consumer drives the impact search sweep — invalidated when the impact owner must serve search standalone, which pulls the deferred sweep into current scope.
  ```
- Reason: Marks are load-bearing — a fact carries its anchor and an assumption carries its invalidating condition; mislabeling an unverified premise as fact hides the tripwire that tells the next session the ground moved.
- Reframe: Mark every premise `fact`, `assumption`, or `inference`; give each assumption the observable that falsifies it, and convert any premise that cannot be anchored to an open question.

## [07]-[UNCHECKED_GAPS]

A capability entry asserts an empty `Gaps` field by assumption, claiming the surface is fully exploited without checking the admitted capability against the owner.

- Detection: A `Gaps` of `none` on a capability whose owner demonstrably leaves admitted library surface unused, and a maturity marker inflated past the built reality.
- Rejected:
  ```markdown
  - [01]-[BUILT]: Impact assessment over element sets
    - Owner: The impact domain owner.
    - Edges: depends-on the element graph; consumed-by the assessment report.
    - Importance: Serves the environmental-scoring direction.
    - Gaps: none.
  ```
- Accepted:
  ```markdown
  - [01]-[GENESIS]: Impact assessment over element sets
    - Owner: The impact domain owner.
    - Edges: depends-on the element graph; consumed-by the assessment report.
    - Importance: Serves the environmental-scoring direction.
    - Gaps: The admitted impact library owns an EC3 search stream and wide `MultiLCA` sweeps the owner gates behind an unnamed consumer; both are unexploited capability, so the surface is a thin slice, not a full owner.
  ```
- Reason: `Gaps` is the load-bearing field for elicitation — an empty gaps field asserts full exploitation, and that assertion gets checked against the admitted surface, never assumed; the same check corrects the maturity marker the empty field inflated.
- Reframe: Sample the owner against the full attribute and capability space of its admitted dependencies, record every unexploited member as a gap, and set the maturity marker to the built reality the gap check reveals.
