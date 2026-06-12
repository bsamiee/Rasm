# time-scheduling — bedrock

## one clock seam

- Two contracts, one seam, disjoint jurisdictions: the BCL time abstraction owns elapsed measurement, timers, and timestamps; the calendar clock contract owns "what instant is it" as a semantic fact.
- Both authorities are registered once at the composition root as the process's only time sources.
  - Every other temporal capability is a derivation, never a peer.
- The surface chooser between BCL temporal types arrives settled and is composed, not re-taught.
  - This lane owns the seam's composition and threading, never the type-choice law.
- The calendar side is a projection tower derived from one clock value: clock → zoned clock (`InZone`, `InUtc`, `InTzdbSystemDefaultZone` extensions) → per-call current values.
- The zoned-clock capsule is clock + zone + calendar system in one value, with per-meaning current-value projections: `GetCurrentZonedDateTime`, `GetCurrentLocalDateTime`, `GetCurrentOffsetDateTime`, `GetCurrentDate`, `GetCurrentTimeOfDay`.
- User-facing components take the capsule; receipt writers take the bare clock and store instants.
  - The projection a component receives encodes its temporal jurisdiction.
- Nothing downstream constructs its own clock: construction is a root-only verb, which is what makes "what time source did this use" a composition fact.
- Ambient time is the named universal defect this lane resolves: wall-clock statics, ambient stopwatch starts, and machine-local now-reads bypass the seam and make temporal behavior unprovable.
- The seam's value is total substitution — with both authorities injected, every temporal behavior (expiry, cadence, hysteresis windows, receipt stamps) is driveable from a test clock.
- Culture rides the same seam law: temporal text is pattern values (`CreateWithInvariantCulture`, `WithCulture` transforms), so culture is data on the pattern, never an ambient read.
- The culture half of the ambient defect resolves by the same move as the time half — both become values threaded from the root.
- Persisted and exported text uses the invariant/roundtrip pattern singletons.
  - `InstantPattern.ExtendedIso`, `OffsetDateTimePattern.Rfc3339`, `PeriodPattern.Roundtrip`.
- Culture-bearing patterns exist only at user-display boundaries with the culture supplied as a value.
  - Persistence never sees a culture-variant spelling.
- Pattern parsing is a typed rail: `ParseResult<T>` carries success or typed failure as a value — temporal text admission never rides exceptions.
- Pattern transforms (`WithTemplateValue`, `WithResolver`, `WithZoneProvider`, `WithCalendar`) make a pattern a self-contained admission policy: text shape, default fields, mapping policy, and zone lookup in one value.
  - A temporal wire format is one pattern declaration.
- Instant projection verbs are the fact-to-meaning bridge: `InUtc` for canonical display, `InZone` for user-facing projection, `WithOffset` for export.
  - A stored instant projects outward through exactly one of the three, named at the projection site.
- Epoch interop is explicit and directional: unix-time factory/projection pairs on the instant type are the only numeric-timestamp seam.
- BCL conversion extensions (`ToInstant`, `ToDateTimeOffset`, `ToDuration`, `ToTimeSpan`) are the only BCL-type seam.
  - Both interop families live at boundaries, never inside domain flow.
- Type-converter and XML serialization settings are explicit interop admissions.
  - Projection into legacy serializers is a declared policy, never a default behavior.
- The fake time authority's three verbs are distinct test instruments: advance moves time and fires due timers; set-now jumps and fires whatever became due; adjust shifts time without timer side effects.
  - The rebase verb for clock-skew handling.
- Per-read auto-advance and a controllable local zone close the fake authority's surface.
  - Polling loops and local-projection code are testable without real waits.
- The fake calendar clock constructs from a UTC instant factory, offers unit-suffixed advancement from ticks through days, and rebases through reset.
- The fake authority's controllable local zone is its own instrument: local-projection code is testable against arbitrary zones without touching machine state.
  - Zone-dependence becomes a per-spec parameter.
- Scripted zone sources plus single- and multi-transition fake zones make DST-edge behavior a constructed fixture rather than a tzdb dependency.
  - Transition tests never depend on real zone data shipping a convenient edge.
- High-rate stamping has compact fixed-width instant and duration value rows when receipt volume makes the full types too heavy.
  - The seam decides the representation once; consumers never convert.

## calendar vocabulary

- Type-per-meaning is the vocabulary law: instant for recorded facts, zoned date-time for user-facing display, offset date-time for export, local date/time for policy values, year-month and annual-date for period boundaries and recurring dates, duration for elapsed quantity, period for calendar quantity, interval/date-interval for windows.
- Choosing the type IS the semantic decision: a maintenance window's start is a local time plus a zone id; a receipt stamp is an instant; an export timestamp is an offset date-time.
  - Each meaning has exactly one spelling.
- Conversion between meanings always names its policy — there is no implicit temporal coercion anywhere in the vocabulary.
- Local-to-instant conversion is a three-outcome total fold, not a function: a local time in a zone maps to zero (gap), one, or two (overlap) instants, and the local-mapping query returns that mapping as a value.
- Boundary admission consumes the mapping or an explicit resolver — return-earlier/return-later/throw for overlaps, forward-shift for gaps, strict and lenient as the composed presets.
- The throwing strict conversion is admissible only after admission has already proven uniqueness — it is a post-admission assertion, never an intake.
- Treating local-to-instant as infallible is the canonical DST bug, and it is unrepresentable when the mapping value is the API.
  - The vocabulary makes the bug a type error.
- Zone identity law: the tzdb provider is canonical; the BCL-zone provider exists only to translate at OS boundaries; zone ids persist as strings and rehydrate through the provider.
- Zone lookup has the strict/lenient mirror: the nullable lookup is the admission verb, the throwing indexer the post-admission verb.
  - The provider's id inventory makes zone-id validation a boot-time set-membership check.
- Persisted zone ids are verified against the provider inventory at boot with absence folding to degradation.
  - A stale or renamed zone id is a boot-time capability fact, never a first-firing surprise.
- The serialization zone provider exists for round-trip stability of persisted zone references — persisted zone ids survive provider data updates.
- Zone rules change under a running process: the zone-interval queries expose the offset-transition structure, so cadence and window logic reason about upcoming transitions instead of discovering them by firing.
- Calendar arithmetic splits by quantity type: duration arithmetic is exact and zone-free; period arithmetic is calendar-relative and must run in local space then re-map through the zone.
- Adding a duration to a zoned value and adding a period to its local value answer different questions.
  - The split forces the asker to pick, which is the point.
- `Period.Between` is the calendar-difference verb; the period builder is the calendar-construction verb.
  - Calendar quantities are computed, never assembled from component arithmetic at call sites.
- Date-start-of-day-in-zone is the canonical date→instant projection, and it is itself gap-aware.
  - Midnight does not exist on some transition days, and the projection absorbs that.
- Interval membership over instants is the receipt-window primitive: support windows, validity spans, and evaluation windows are interval values tested by containment.
  - Never paired comparisons re-derived at call sites.
- Weekday and calendar identities (`IsoDayOfWeek`, calendar systems) are vocabulary values consumed by schedule rows and policy records.
  - Schedule semantics never re-encode weekday numbering.
- Calendar recurrence splits from clock cadence: annually-recurring dates persist as annual-date values and period-stepped recurrences as period values, both resolved through calendar arithmetic.
  - Cron expressions own clock-shaped cadence only — encoding a calendar recurrence as a cron row loses the calendar semantics (leap handling, month-length variance) the vocabulary already owns.

## schedule rows

- A schedule entry is data: expression text, field format, optional jitter seed, zone id.
- Expressions persist as text and rebuild at composition through the non-throwing parse.
  - The parse-failure exception never crosses the configuration boundary.
- The expression value has structural equality and normalized text projection.
  - Schedule identity is value identity, and the normalized text is the canonical persisted form.
- The grammar is two formats: five-field (minute resolution) and six-field with a mandatory leading seconds field — second resolution is a format admission, not a separate scheduler.
- Specials cover last-day, nearest-weekday, and nth-weekday; macros (`@yearly` through `@every_second`) are named intakes for the common rows.
- Static template values exist for every canonical cadence, including the six-field every-second form — templates beat literal expressions for the standard rows: no parse, no typo class, value identity for free.
- Parse failures are a two-class taxonomy: malformed expression versus seedless jitter — and the non-throwing parse family mirrors every throwing overload (format, seed), so boundary intake always has a guarded spelling.
- Occurrence output has three shapes — UTC instant, zone-projected instant, offset value — and the offset form preserves the occurrence's local offset.
  - Choose the offset output when firing receipts must carry local-time evidence; the UTC form when receipts are pure facts.
- Hash jitter (`H`) is deterministic per seed: parsing an `H` expression without a seed throws — jitter is reproducible spread, not randomness.
- Jitter templates exist for every macro cadence — seeded canonical rows without hand-written expressions.
- The seed is schedule-identity policy and the entire fleet-skew design space: derive it from the stable schedule key and every process in a fleet computes identical fire times; derive it from a per-node key and fleet load spreads deliberately — one seed-derivation row decides which.
- Occurrence math is UTC-pure: UTC instants in, UTC instants out, zone projection internal to the call.
- UTC-kind enforcement throws on local-kind input — local timestamps cannot leak into occurrence math even by accident.
- The UTC zone is a fast path that skips mapping entirely — UTC-native schedules pay no zone cost.
- Absent next/previous occurrence is null — a schedule that never fires again, or a window with no hits, is an ordinary value outcome, never an exception.
- DST behavior is defined, asymmetric, and shape-dependent: fall-back overlap fires interval expressions in both repeated hours but point schedules once; spring-forward gaps shift skipped occurrences to the first valid post-transition moment.
- Cadence semantics around transitions are decided by expression shape — choosing interval form versus point form is choosing the DST contract, and zoned-schedule review checks exactly that.
- Range queries are lazy ascending or descending occurrence streams over a window, from-inclusive/to-exclusive by default, with inverted bounds rejected.
- The descending stream is the audit direction — most-recent-first inspection of what should have fired without materializing the window.
- The window query between last-known-fire and now IS the missed-occurrence inventory — no scheduler bookkeeping exists beyond the last-fire stamp.
- The previous-occurrence query bounds catch-up after restart: planned fires between the persisted stamp and boot are enumerable without any scheduler state having survived.
  - Schedule state is one instant per row, by construction.

## cadence law

- The cadence loop is a derivation, not a scheduler: next = the expression's next occurrence after now (clock seam) in the row's zone; sleep until next on a seam timer; fire; recompute.
- Nothing is stored but the last-fire stamp — cadence has no durable state machine to corrupt or migrate.
- Cadence owns WHEN exclusively: a row yields instants and never carries parallelism, queueing, or execution budget.
- Throughput is the concurrency layer's jurisdiction — a schedule row growing a max-concurrent knob has crossed the boundary, and the knob is the trespass signature.
- Misfire is a closed policy axis per row, resolved with the range query: skip-to-next (drop the inventory), fire-once-catchup (one consolidated firing for the gap), fire-all (replay the inventory).
- Process suspension, clock jumps, and long prior runs all reduce to inventory-length-greater-than-one — one policy row handles every wake-up-late cause identically.
- Firing receipts carry planned-versus-actual: (schedule key, planned instant, actual instant, skew).
- Skew is evidence, not error — sustained skew diagnoses timer pressure or suspension, and the receipt stream is where misfire policy decisions become auditable.
- Zone-aware rows re-evaluate the zone mapping per occurrence — there is no cached-offset staleness class; a zone-rule change lands at the next computation with no invalidation machinery.
- Cadence and calendar vocabulary compose: a row's zone id resolves through the canonical provider, its fire instants are instants, its receipts stamp through the clock seam.
  - The schedule subsystem introduces zero temporal types of its own.

## divergent — one-clock-seam

- The seam as one frozen record: { time authority, calendar clock, home zone, calendar system } registered at the root.
- Every temporal capability — zoned clocks, timers, cadence evaluation, expiry, hysteresis, receipt stamps.
  - Derives from the record's members and nothing else; posture swap replaces the record once, and no temporal test seam exists anywhere else in the process.
- The two-fakes trap: the fake time authority and the fake calendar clock are independent clocks, and advancing one without the other silently skews any spec crossing jurisdictions.
  - Timer-driven code stamping calendar receipts is the common case.
- Law: tests advance time through one verb that moves both fakes in lockstep — the test-side mirror of the frozen seam record; a spec touching two time substitutes directly is the defect signature.
- The three-verb advancement distinction is itself test-design law: advance proves cadence behavior, set-now proves jump tolerance, adjust proves skew handling without cadence side effects.
- A temporal component's spec matrix is the three verbs × its policy rows, with scripted-transition fake zones adding the DST axis without tzdb coupling — temporal coverage is enumerable, not exploratory.
- Zone-sensitive components take their zone from the seam record or an explicit per-row zone id — never the system default; the system-default projections exist for edge tooling only.
- "What zone did this run in" is then a recoverable fact from composition plus rows alone — zone provenance needs no logging.
- The seam record is also the culture record: invariant patterns for persistence, per-boundary display cultures as values — a process's complete temporal-and-textual posture is one record plus pattern rows.
- The ambient-read audit is mechanical: a grep for the static now/clock accessors must return only the seam's own registration line — the defect is detectable as a presence, the discipline as an absence.

## divergent — schedule-rows-cadence

- Schedule catalog as the unifying owner: frozen rows (key, expression, format, seed derivation, zone id, misfire policy, drain band) folded by ONE cadence driver per process.
- The driver fold: compute each row's next occurrence, take the minimum, sleep one seam timer until it, fire that row, recompute — N schedules cost one timer and one loop.
- The bespoke timer-per-job pattern is the lane's headline collapse: per-job timers, per-job zone handling, and per-job catch-up logic all dissolve into the fold.
- The single-driver fold has a correctness edge the per-job pattern lacks: recomputation after every firing means zone-rule transitions, catalog swaps, and jitter seeds all take effect at the next occurrence boundary with no restart and no stale-timer cancellation choreography.
- The catalog is a frozen-publish policy surface: a reload republishes rows and the driver folds the new set on next wake — schedule changes are policy transitions with receipts, never scheduler surgery.
- A removed row simply stops appearing in the fold; a new row enters at its first future occurrence — catalog mutation has no edge cases because the driver holds no per-row state.
- Cadence composes the lifecycle spine rather than owning shutdown: the driver is one hosted participation whose loop awaits on the phase token.
- The row's drain-band value decides whether an in-flight firing completes or cancels at stop, and the misfire inventory naturally absorbs anything skipped during drain.
  - Restart-and-catch-up is the same code path as suspend-and-catch-up.
- Per-row firing receipts plus the misfire policy give the catalog an audit identity: planned, fired, skipped-with-policy, and consolidated firings are all receipt classes derivable from the row plus the inventory.
- "Did the nightly run happen, and if not, what was decided about it" is answerable from receipts without scheduler logs — the receipt grammar is the operational interface.
- Boundary with effect-retry scheduling: effect-rail schedule combinators own retry/repeat cadence inside an operation — algorithmic backoff over an effect; calendar rows own wall-calendar cadence across operations.
- The discriminant is the question asked — "when does the system act next" is a row; "how does this action persist until it succeeds" is rail policy. A cron expression inside a retry loop or a backoff curve inside the schedule catalog are the two trespass signatures.
- Second-resolution rows ride the same driver: the six-field format admits them and the min-fold is resolution-blind — sub-minute cadence is a format admission plus a row, never a second driver.
- Cadence below the timer's practical resolution is the signal the work is a stream concern, not a schedule — the catalog's floor is where the schedule lane hands off to the throughput lane.
