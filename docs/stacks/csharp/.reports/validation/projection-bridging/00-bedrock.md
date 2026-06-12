# [BEDROCK] validation / projection-bridging

## outcome-shape

- `ValidationResult.IsValid` is `Errors.Count == 0` and nothing else — severity-blind: `Warning` and `Info` failures sit in the same `Errors` list and gate `IsValid` exactly like `Error` failures.
- Any projection reading `IsValid` directly treats advisories as rejections; the bridge must partition before it decides — `IsValid` in bridge code is a pre-partition smell, not a usable verdict.
- `ValidationFailure` is a seven-field evidence record, each field with a fixed projection role:
  - `PropertyName` — the machine-facing path key, dotted and element-indexed; the bridge's field-evidence source.
  - `ErrorMessage` — rendered text; display tier only, never identity.
  - `AttemptedValue` — the raw offending value; evidence with a classification obligation.
  - `CustomState` — arbitrary typed payload; the typed-fault side channel.
  - `Severity` — the band selector the partition reads.
  - `ErrorCode` — the stable symbol everything else derives from.
  - `FormattedMessagePlaceholderValues` — `Dictionary<string, object>` of every placeholder the formatter resolved; the re-render capital.
- The projection consumes structured fields exclusively; `ErrorMessage` is never parsed, compared, or persisted as identity.
- Every `ValidationFailure` field is a mutable property and the record is hand-constructible — the three-argument constructor `(propertyName, errorMessage, attemptedValue)` plus property sets is how provider-exception arms mint failures; after the run, the bridge treats every record as a frozen snapshot, and post-run mutation is rejected as evidence tampering.
- The outcome is null-safe by construction: the failures collection filters null entries at every ingress — constructor copies, the merge constructor, and the setter — so the bridge's fold carries no null guards.
- Failure order is run order: rules execute positionally and failures append to one list, so accumulation order in the outcome is deterministic for a given rule graph — orderings in projected faults are reproducible, and order-sensitive consumers get stability without sorting.
- `AttemptedValue` carries raw boundary input verbatim — the one bridge field that can smuggle unclassified sensitive data past the seam; the bridge applies the classification policy to it before any export-facing projection, while the classification taxonomy itself stays telemetry-page law.
- `ToDictionary()` projects `IDictionary<string, string[]>` grouped by `PropertyName` — the flat per-field shape for form-style consumers; it discards code, severity, and placeholders, which makes it a terminal display projection, never a transport shape.
- `ToDictionary` keys are the raw indexed paths — collection failures group per element path (`items[0].code`, `items[1].code`), never per collection; consumers wanting per-collection grouping normalize indices themselves, downstream of the projection.
- `ToString(separator)` joins messages for log lines — same terminal-only status.
- The executed-variant receipt rides the outcome but is seam evidence, not bridge input — the bridge ignores it entirely; the admission record is its only consumer.

## bridge-law

- One generic bridge per system — not per boundary — projects a validation outcome onto the typed rail; it is generic over the fault family's factory contract, so every seam crosses through the same fold and a new boundary adds zero bridge code.
- The fold, step 1: partition `Errors` by `Severity` — the `Error` band gates, the `Warning`/`Info` bands ride.
- Step 2: mint one typed fault per gating failure through a three-tier cascade — `CustomState` holding an already-typed fault case is taken as-is (zero mapping); otherwise `ErrorCode` keys a frozen code-to-case map; otherwise the failure routes through the string-bearing `Create` tier carrying code, path, and message.
- Step 3: combine the minted faults through the family's accumulation algebra into the accumulating carrier's failure.
- Step 4: attach the non-gating bands to the success branch as evidence values.
- Fault-family shape, the factory-contract rail lift, and the applicative accumulation algebra arrive settled — the bridge's own law is the partition, the three-tier mint, and the band routing.
- The bridge runs once per seam outcome — upstream merging of multi-validator results happens before the fold, so N validators still cost exactly one projection.
- The bridge's input type is `IEnumerable<ValidationFailure>`, not `ValidationResult` — outcomes arriving as results, as the throw-style exception's `Errors` payload from a foreign adapter, or as hand-built failures from a provider-exception arm all enter the identical fold; transport never forks the projection.
- Tier-1 minting is the density lever: `WithState` accepts providers of arity `(T)`, `(T, TProperty)`, and `(T, TProperty, ValidationContext<T>)`, so the rule that detects the fault constructs the typed fault case with full access to instance, value, and run context — the bridge merely unwraps.
- Tier-1 unwrap is type-tested, never trusted: `CustomState` is pattern-checked against the fault family's base before unwrapping, and a non-fault state falls through to tier 2 — a foreign or careless state value cannot poison the rail with an untyped payload.
- Rules minting their own typed faults collapse the bridge's mapping table to third-party residue; a mature boundary converges tier-1-dominant with tier-3 as the foreign-validator fallback.
- Negative outcomes are values on the rail in both directions: gating faults are the failure value, non-gating findings are success-side evidence — no part of a validation run exists only as a log line or an exception.
- Every consumer decision — retry, display, gate, audit — folds over rail values; a consumer that needs a fact the rail does not carry names a missing field on the fault family, not a new side channel.
- Rejected bridge form — per-boundary bespoke mapping switches: the generic fold already discriminates on the failure's own fields.
- Rejected bridge form — throw-and-catch into the rail: the outcome was already a value; round-tripping it through an exception erases band structure.
- Rejected bridge form — message-text dispatch: locale-fragile, breaks on the first translation.
- Rejected bridge form — severity-blind gating via `IsValid`: advisories become rejections.
- Rejected bridge form — dropping `Warning`/`Info` bands at the bridge: evidence loss; the bands exist precisely to survive admission.
- Rejected bridge form — `CustomState` as a grab-bag (strings, anonymous types, tuples): tier 1 works only when state IS a fault-family case; untyped state degrades the failure to tier 2/3 while looking structured.
- The bridge is a pure fold — no validator, no I/O, no context — from failure records to rail values; its laws (partition totality, tier precedence, band routing) hold over arbitrary failure records independent of any validation run.

## foreign-adapter-grammar

- A foreign validation source enters by mapping its record to the seven-field shape under three fail-closed defaults, each a one-line rule:
  - unknown or missing severity maps to `Error` — classification uncertainty gates rather than rides.
  - missing code maps into the reserved bridge band — foreign faults are identifiable as foreign forever.
  - missing path maps to the root path — never to an invented field name.
- Foreign rendered text, when it is all the source offers, rides as the tier-3 message — it is display residue, not identity; the adapter never reverse-engineers codes or paths out of message text.
- One adapter per foreign source, owned beside that source's seam — adapters are boundary translation and never leak the foreign record type past the seam.

## evidence-lifecycle

- Riding-band evidence attaches to the success value at the bridge and travels with it — consumers downstream read it, fold it into gates, and export it as facts; none of them re-decide admission.
- Evidence is never promoted back into a gating fault downstream: a consumer that wants to block on a `Warning` is requesting a different seam policy, and the change lands in the seam row or the severity selection — re-classification after admission is a second admission and is rejected.
- Evidence lifetime ends with the value it rides: persisted aggregates persist the evidence only when the storage schema declares a seat for it; silently dropping evidence at a persistence boundary is legal, silently dropping gating faults never is.

## wire-fault-shape

- What crosses a process boundary when failures travel: `ErrorCode`, `PropertyName`, `Severity`, and the placeholder values — the four culture-invariant, process-independent fields.
- What never crosses: `AttemptedValue` (classification risk, and the receiver re-admits anyway), `CustomState` (process-local typed payload with no serialization guarantee), rendered `ErrorMessage` (culture freeze).
- Placeholder values that cross must be wire-safe: the dictionary holds `object`, so validators populating it for transportable faults append primitives and pre-formatted invariant strings, never live object graphs — a placeholder that cannot serialize is a process-local rendering aid only.
- The wire fault is therefore a four-field projection of the seven-field record, and the receiving process reconstructs display text from code plus placeholders under its own culture — fault transport and fault rendering are fully decoupled.
- Severity crosses as the closed three-member enum — no foreign tier survives transport unmapped, because adapter ingress defaults already normalized classification before the fault could travel.
- A fault round-trip (rail fault back out to a wire error contract) is the same projection run outward — code, path, severity, placeholders — never an inverse parse of rendered text; contract composition mechanics belong to the wire pages.

## code-bands

- `ErrorCode` defaults to the failing validator's `Name` through `ValidatorOptions.Global.ErrorCodeResolver` (`Func<IPropertyValidator, string>`) — one process-wide mint point.
- Swapping the resolver derives codes uniformly from validator identity — band prefix, context tag — with zero per-rule ceremony; `WithErrorCode` pins stable domain codes per rule and always wins over the resolver.
- Code-band allocation per concern: each bounded context owns a numeric band, subdivided by concern class — structural shape, cross-field consistency, capability pairing, I/O-backed existence, override policy — so a code's band locates its owner and concern without a lookup.
- The code value embeds the band — band x concern x ordinal in one number — letting the bridge's tier-2 map and the fault family's numeric-code law agree by construction: band recovery is arithmetic, never string splitting.
- One band is reserved for bridge-internal minting — tier-3 fall-throughs and provider-exception conversions carry codes from this band, so "the bridge minted this" is recoverable from the code alone and unknown-origin faults cannot masquerade as domain rejections.
- The tier-2 code-to-case map is frozen and proven total over the declared code vocabulary; an unrecognized code is not a mapping error but a routed fall-through to tier 3 with the code preserved — unknown codes degrade losslessly instead of failing the bridge.
- The map constructs once at the composition root and is read-only thereafter — code-to-case correspondence is immutable per process, so two faults with one code can never project to two cases within a run.
- The closure sweep over declared codes versus map keys catches genuine drift at composition time — the totality proof is the seam lane's fold pattern applied to this map.
- Default validator-name codes (`NotNullValidator`, `NotEmptyValidator`, ...) are stable shipped identifiers — usable as tier-2 keys for built-in rules with no `WithErrorCode` ceremony; custom domain codes are reserved for facts the built-in vocabulary cannot name.
- Stability boundary: resolver-derived codes follow the validator's `Name`, so renaming a custom validator class silently renames every code it mints — any code that crosses a process boundary or persists must be pinned (`WithErrorCode` or an explicit `Name` constant); resolver-derived codes are process-local conveniences only.
- Pinning a code without registering its template is half a registration: code-first lookup misses and falls back to the validator-name template, so the message stops tracking the code — code and template register together as one vocabulary row, never separately.
- Override eligibility is declared per band, never per incident: the seam's override policy names which code bands may be reclassified, so "can this rejection be forced through" is a static property of the code — the override-eligible set is itself sweep-checkable data.

## severity-mapping

- The three severity tiers map to exactly three projection destinations: `Error` to a fault-family case on the failure branch; `Warning` to recorded evidence on the success branch; `Info` to advisory facts on the success branch, telemetry-grade and never gate-relevant.
- The `Warning` destination is where forced-override output lands — reclassified failures arrive with full failure detail intact, so the override's evidence is structurally identical to ordinary advisories and needs no special channel.
- The mapping is one frozen table owned by the bridge; seams and validators select severities, only the bridge interprets them.
- Severity is per-failure data, not per-run data: one run routinely produces mixed bands, and the bridge's partition is the only place the mixture resolves — upstream code branching on "the run's severity" has invented a fact the model does not carry.
- The mapping table is provably total without a sweep: the severity enum is closed at three members, so the table is one of the few projection maps whose totality is structural rather than proven.
- Evidence values from the riding bands feed the operational fact stream as ordinary facts — the telemetry pages govern their export; the bridge's responsibility ends at producing them as values.

## field-path-evidence

- `PropertyName` is the projection key: the full chain path with element indices (`items[2].code`), composed mechanically through child and collection runs — the bridge converts it to the fault family's field-evidence representation with no transformation beyond separator policy.
- `OverridePropertyName` and `WithName` split machine from human: `OverridePropertyName` rewrites the failure's `PropertyName` key — changing what the bridge and wire consumers see — while `WithName` rewrites only the display name inside rendered messages.
- The `{PropertyName}` placeholder renders the DISPLAY name, not the key: localized or `WithName`-overridden labels appear in messages while `ValidationFailure.PropertyName` keeps the invariant path — the asymmetry is the design, and confusing the two surfaces is the canonical projection bug in both directions.
- Using `WithName` to fix a path drifts the projection key silently; using `OverridePropertyName` to prettify a label breaks every keyed consumer.
- Keyed collection paths — element identity instead of ordinal index — flow into the same key: the indexer-override hook rewrites the bracket text at failure time, so re-ordered collections keep stable failure keys; the hook is rule-graph law, the stable-key consequence is the projection's.
- The separator inside paths is global, pre-rule policy — the bridge's path decomposition assumes exactly one separator for the process lifetime, and persisted paths make the separator effectively unchangeable once any outcome has been stored.

## localization

- Message resolution is a fixed cascade: explicit per-rule message first — `WithMessage` literal or factory of arity `(T)` / `(T, TProperty)`; otherwise the validator's default template resolved code-first — the language manager is queried with `ErrorCode` as the key, a non-empty hit wins, and only then does the validator's `Name` key the fallback.
- The code-first hop means registering a translation under a custom error code re-messages every rule carrying that code across the whole system in one registration — the code is simultaneously rail dispatch key, wire contract identifier, and message lookup key: one symbol, three derived surfaces.
- `LanguageManager.AddTranslation(language, key, message)` registers per-culture templates at runtime — composition-root seeding of the domain code vocabulary; `Clear()` resets to built-ins.
- The `language` argument is the culture name string, and specific-culture registrations shadow neutral ones through the fallback walk — seed the neutral culture for coverage, specific cultures only for genuine regional divergence.
- `LanguageManager.Culture` pins resolution deterministically; left null it follows the ambient UI culture, which renders identical failures differently across threads and processes; `Enabled = false` collapses to the English built-ins.
- `GetString(key, culture)` takes an explicit per-call culture — render-at-the-edge resolves per-user culture through the argument, never by mutating the pinned global or the ambient thread culture.
- Culture precedence is a fixed three-level chain — per-call argument, then the pinned manager culture, then the ambient UI culture — mirroring the message cascade's shape: the most explicit declaration wins, ambience is the fallback of last resort.
- Resolution walks specific culture, then the parent chain, then English — partial translation sets degrade per-key, not per-culture.
- Placeholder economy: templates carry `{PropertyName}`/`{PropertyValue}` plus validator-specific arguments (`{ComparisonValue}`, `{MinLength}`, `{MaxLength}`, `{TotalLength}`, `{RegularExpression}`, `{CollectionIndex}`), appended by validators at failure time.
- Placeholders accept format specifiers — `{PropertyValue:F2}` routes through standard format strings — but the formatting call carries no explicit culture: numeric and date placeholders render under the ambient culture even when the template was resolved under a pinned one. Pinning the language manager's culture does NOT pin placeholder formatting; deterministic rendering requires culture control at the rendering call site.
- Unresolved placeholders pass through verbatim — a typo'd `{MaxLenght}` renders as literal braces instead of throwing, so template defects are invisible at run time and only snapshot-style proofs over rendered output catch them.
- Custom validators appending placeholder arguments own a name discipline: argument names share one flat namespace per failure with the built-ins, and a collision silently overwrites — prefix custom argument names with the validator's concern.
- `MessageFormatterFactory` swaps the formatting algebra system-wide — custom delimiter handling, format-string policy, culture-pinned placeholder rendering — in one assignment; it is the repair point for the ambient-culture formatting gap.
- The no-drift persistence law: `FormattedMessagePlaceholderValues` preserves the resolved placeholder dictionary on every failure, so the durable record of a failure is `ErrorCode` + `PropertyName` + placeholder values — rendered text is reconstructible in any culture at any later time at the display edge.
- Persisting `ErrorMessage` freezes one culture into storage — the drift defect the placeholder dictionary exists to prevent.
- Drift catalogue, one foreclosure per row:
  - call-site `WithMessage` literals — fork the template per rule; instance-derived factory templates are the only literal-bearing exception.
  - persisting rendered messages — culture freeze in storage.
  - dispatching on message text — logic breaks under the first translation.
  - per-language code forks — contradict code culture-invariance; languages are render-time data.
  - ambient-culture rendering in multi-process suites — the same fault produces different bytes, defeating log correlation and snapshot proofs.

## availability-projection

- The validation-to-save gate is a fold over current rail state, severity-partitioned: gating-band faults block the action, recorded-band evidence permits it while traveling with it.
- The gate is derived data, recomputed from the outcome value — never a stored boolean that can desynchronize from the failures that justified it.
- The gate value carries the blocking faults, not only the verdict — consumers render reasons from the same value that blocks, so explanation and enforcement cannot disagree and no second query exists.
- Multi-seam gates fold monoidally over per-seam gate values with the combine policy a declared value (all-must-pass versus quorum) — aggregation is one fold, not bespoke gate wiring per composite.
- Gate recomputation triggers are seam events — admission ran, override applied — not consumer polling; the command-availability algebra that consumes the gate signal is interaction-layer law, and this page ends at the typed gate value.

## divergent

- outcome-projection — the bridge as the page's absorbing owner: every rule kind, validator origin, and seam feeds the same four-step fold because the discrimination rides the failure's own fields — `CustomState` presence, `ErrorCode` band, `Severity` — so growth lands as tier-1 state providers on new rules, new rows in the frozen code map, or new evidence consumers, never as a new bridge.
- outcome-projection — contravariant reuse is the deepest consequence: foreign validation sources (a third-party library's failures, a remote service's fault payload) enter the same fold by adapting their records to the seven-field shape — the bridge is the system's single validation-outcome ingress, not merely the local package's.
- outcome-projection — tier economics quantified, per tier:
  - tier 1 costs one provider lambda per rule and zero bridge maintenance.
  - tier 2 costs one frozen-map row per code plus one closure-sweep entry.
  - tier 3 costs nothing locally but defers all structure to the fault family's string tier.
  The optimization gradient is monotone — every failure moved up a tier deletes mapping code downstream — so bridge-side mapping growth is the metric flagging rules that should mint their own faults.
- outcome-projection — there is no performance argument for bespoke per-seam bridges: the partition is one O(n) pass, tier discrimination is per-failure field reads, and the fold allocates only the faults it mints — the bridge's cost is invisible beside the validator run it follows.
- code-bands-localization — the one-symbol law instantiated: `ErrorCode` is this page's symbolic-reference spine — rail dispatch (tier-2 map), wire contract (stable identifier across processes), localization (code-first template lookup), and audit (band arithmetic locating owner and concern) all derive from the single code value; any second naming axis — message text, display name, per-locale identifiers — is derived presentation, reconstructible from code plus placeholders alone.
- code-bands-localization — one declaration, three projections: a bounded context's code vocabulary is declarable as one generated enumeration, making the tier-2 map's totality sweep, the wire contract's stability check, and the translation seed list three projections of the same declaration — vocabulary growth is one new enumeration row, and every derived surface follows or fails loudly.
- code-bands-localization — the placeholder-name vocabulary is part of the contract: cross-process re-rendering requires both ends to agree on placeholder names per code, so the code enumeration row carries its placeholder schema — which makes template evolution diff-checkable (a template referencing a placeholder the code's schema does not declare fails the vocabulary sweep, the localization instance of fail-closed).
- code-bands-localization — render-at-the-edge as an architectural position: because code plus placeholders fully determine text, rendering belongs to the outermost display surface (per-user culture, per-channel formatting), storage and transport stay culture-invariant, and proofs assert against code plus placeholder values rather than text — every interior layer is translation-proof and the translation set is hot-swappable without touching stored or in-flight outcomes.
