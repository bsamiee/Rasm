# ownership-topology — bedrock

## one-owner law

- Exactly one retry owner exists per outbound hop, and it is held at the composition root: the hop's pipeline is one registry row keyed by hop identity, declared where the process composes — never inside the code that calls the hop.
- The law is outbound-only: inbound work admission is not a hop and never acquires a claim row — claim vocabulary that leaks into inbound seams misdescribes admission as ownership.
- Every hop pipeline resolves from the one root registry; a pipeline built directly inside a seam escapes the claim cell, the conflict detection, and the epoch fence at once — direct builder use inside seams is the rejected form.
- The hop callable that domain code receives is already resilient — call sites carry zero resilience vocabulary, which is what makes a second owner a visible anomaly rather than a habit.
- Claim identity is the hop, not the policy and not the instance: registration-key equality must mean hop equality.
- Hop identity is normalized before claiming — scheme casing, host casing, default ports: un-normalized identities splinter one hop into several claims, and splintered claims under-detect conflicts because each fragment looks singly owned.
- Per-target instances (authority-keyed pipeline instances under one registration) are one owner with N isolated states — instance fan-out is never a second owner, and conflating the two produces false conflicts.
- The hop row is one closed record and every law on this page is a column: key (two formatted axes), owner declaration symbol, allotment class, idempotency class (two sub-columns: semantic idempotency, body replayability), policy reference, override binding.
- The policy column is a reference, not an inline value: N hops subscribe to one named policy class, so editing the class re-tunes every subscribing hop through per-row reload — policy classes deduplicate across rows.
- The override-binding column names the hop's capability group: a capability group is a set of hop rows, and the group is the unit the override rail and the shared manual control bind to.
- The row being the only growth surface is the page's absorption law: a new hop is one row; a new resilience law is one column; neither is a new code surface.
- The second-owner taxonomy has three detection layers, each with its own receipt shape.
- Layer (a) — same root, same hop key: the deferred-registration verb returns its verdict synchronously and atomically, false on a duplicate key.
- On a false verdict the losing registration degrades to no pipeline — single pass through the incumbent — and the verdict projects into conflict evidence carrying both declaration sites.
- The conflict receipt also records the loser's policy parameters: repairing a conflict requires comparing the two policies, and the receipt is the only place the discarded policy survives.
- Layer (b) — two handlers stacked on one client seam: structural, found by scanning the seam's handler chain for more than one resilience handler; the receipt's severity datum is the multiplicative attempt count (m × n sends per logical call).
- Layer (c) — a library or driver retrying beneath the seam: classified by the layer table below and resolved by configuration (disable one loop), never by wrapping the inner loop in an outer one.
- Degradation to single pass never removes protection: the incumbent pipeline still guards the hop; only the duplicate's policy is discarded — conflict handling reduces redundancy, never coverage.
- Conflict receipts are facts, not faults: detection at composition emits evidence into the one fact stream and continues with the single incumbent owner.
- Conflict evidence has two emission moments under one vocabulary: the composition-time verdict (layer a) and the audit-time structural scan (layer b) — receipts from both moments fold into one conflict view.
- A second claim must never fail boot — the system is safe with one owner — and must never be silent: the duplicate declaration is latent attempt multiplication waiting for a configuration change.
- Claim atomicity rides the registration verdict alone; no lock spans receipt emission — receipts order by stamp, never by holding the claim cell open.
- The claim row carries the owner's declaration symbol (`nameof`-derived, never a string literal) so a conflict receipt names both claimants without reflection or log archaeology.
- Claim rows are configuration-enumerable: the full hop set of a process is readable from its rows at boot — the resilience topology is a queryable artifact, not emergent behavior.
- The receipt vocabulary of this page is four kinds with fixed evidence fields: claim-conflict (both symbols, multiplication factor, the discarded policy), degradation-transition (from-level, to-level, trigger, derived-or-forced), handoff (drain completed or expired, epoch), succession (reload versus epoch change, generation).
- All four receipt kinds are rows of the one fact stream, and every operational view of hop ownership is a fold over them — there is no second ownership ledger.

## retry-layer classification

- Transport pipeline at the seam: the failure vocabulary is the wire's — status classes, rejection exceptions, connect timeouts; the remedy needs per-attempt deadlines, admission, and endpoint health memory; owner is the seam pipeline at the root.
- Transport marker: the fault is meaningful only outside the process.
- Domain schedule on effect rails: the failure vocabulary is the domain's — typed errors on rails; retry is a schedule policy value composed onto the effect, with the schedule algebra arriving settled and consumed, never re-derived.
- Schedule marker: the fault is a typed value and the operation never leaves the process through a seam that already owns a pipeline.
- Store execution strategy: connection and transaction retry belongs to the store's own execution strategy; outbound resilience never wraps a store call.
- Store marker: the callee owns transactional semantics.
- A pipeline around store work replays from the wrong boundary — re-running work the store already committed or already retried internally — and double-counts the store's loop; this is the one misclassification that corrupts data rather than wasting attempts.
- The decision procedure is two ordered questions: does the callee own transactions? — store row; does the call cross a process seam? — pipeline row; otherwise — schedule row.
- Ambiguity after both questions means the seam is mis-factored, not that the table is incomplete — the repair is re-factoring the seam, never adding a fourth row.
- Boundary case — capsule-internal wire: an in-process call into a capsule that internally crosses a wire is owned at the capsule's seam; the caller stays bare. Ownership follows the seam, not the call stack.
- Boundary case — durable handoff: a queue or outbox producer has no retry owner at all; the persistence of intent IS the resilience — single pass plus persisted intent, and adding retry above it duplicates enqueued work.
- Boundary case — readiness polling: awaiting a resource into readiness is convergence, not retry — schedule-driven iteration on a success predicate (settled converge law); classifying it as retry mis-types success-awaiting as failure-recovery and attaches failure telemetry to a healthy path.
- Boundary case — fan-out: each hop in a fan-out keeps its own owner and the aggregate gets no umbrella retry — re-running the fan-out re-runs completed legs.
- The split is exclusive per seam: domain-internal retry stays schedule policy on rails, transport resilience stays a pipeline at the seam, never both on one seam.
- Stacking both multiplies attempts invisibly — schedule m × pipeline n — and silently inflates the idempotency window by m.
- The multiplication is invisible precisely because each layer is locally correct — neither layer's review catches it; only the seam-exclusivity rule does.
- The structural guard is type-level: hop callables are typed so they cannot appear under a rail retry combinator, and rail-level retry policies are declared in vocabularies that have no seam-callable constructor.
- Making the conflict unrepresentable beats detecting it — detection fires once per audit; the type wall fires at every compile.
- The inverse rule binds equally: a pipeline retry must never wrap domain logic — pipeline predicates speak the wire's vocabulary (exceptions, outcomes) and cannot observe typed domain failures on rails.
- Routing domain values through exceptions to make a pipeline retry them inverts the fault architecture — the wire's predicate language must never become the domain's error transport.
- A fourth pseudo-layer is rejected by construction: ambient retry — interceptors, base-class hooks, middleware applied to all calls — has no hop identity, therefore no claim row, therefore no horizon.
- Anything that retries without a key the claim cell can hold is unownable and is dismantled into one of the three real layers before other resilience work proceeds.

## deadline allotment

- A hop owns one allotment — the total budget for the logical call — and every per-attempt deadline is a linked child scope of it: the attempt deadline swaps in a linked child token for the attempt and restores the parent afterward.
- Parent allotment, caller cancellation, and attempt deadline form one cancellation tree with a single root — there is never a second, parallel cancellation lineage on a hop.
- Allotment classes are policy rows: total-and-attempt span pairs validated coherent at composition (attempt ≤ total, health window sized to the attempt span), each class naming both spans plus the derived horizon.
- New hops select an allotment class; inventing span pairs inline is the rejected form — span pairs that never co-validated are how budget incoherence enters a fleet.
- Single-pass rows collapse the allotment class to one span: total equals attempt, because there is no loop for the two spans to bound separately.
- A break consumes allotment rather than pausing it: an open circuit inside the loop spends budget while rejecting fast, so the horizon never extends because a breaker was open — the total allotment caps every delay source including breaks.
- Allotments inherit through nested seams: a hop calling another hop hands down its remaining budget, and the child's effective allotment is the minimum of its own class span and the inherited remainder.
- A child allotment class larger than any plausible inherited remainder is a composition defect surfaced at the claim row — the budget hierarchy is checkable from rows alone, before any request flows.
- Cancelled-versus-rejected discrimination is structural, not heuristic: a cancellation surfacing while the parent fired is a cancelled outcome — the caller's intent, passing through untyped.
- A cancellation whose child deadline fired while the parent did not converts to a typed rejection carrying the deadline value.
- Every layer above the seam can therefore distinguish "the caller left" from "the attempt was too slow" by outcome type alone — no message inspection, no timing heuristics.
- The total-outcome rail makes hop execution exhaustive before any fault surfaces: every execution folds to exactly one of completed, cancelled, rejected, or faulted.
- The catch rail performs the fold at the seam using the settled capture grammar; above the seam the outcome is a closed union and exception handling no longer exists there.
- The fold happens exactly once, at the seam capsule: a second fold at the caller re-opens the union and re-introduces the exception vocabulary the seam already retired.
- Completed carries the value; cancelled carries the fact that the parent token fired; faulted carries everything unclassified.
- The rejected arm is itself a closed taxonomy keyed by exception type, each row carrying its evidence.
- Deadline rejection carries the deadline span that fired.
- Health rejection (open circuit) carries the remaining break hint when known.
- Operator rejection (isolated circuit) is distinguishable from organic health rejection by type alone — forced darkness never masquerades as dependency failure.
- Admission rejection (rate limit) carries the retry-after hint when the limiter knows one.
- Every rejection names its rejecting layer through the telemetry source stamped at throw — the rejecting strategy is recoverable from the rejection value alone.
- Escalation logic reads the typed value, never message text — message parsing is the rejected form of outcome handling.
- A rejection without its evidence is a malformed seam: the evidence channels exist on every rejection type, so an empty-evidence rejection means the seam discarded information it had.

## idempotency window

- The idempotency window equals the retry horizon: a repeated send must be deduplicable for exactly as long as the owner can re-send.
- With a total allotment the horizon IS the allotment; without one it is the sum of computed delays plus attempts × attempt deadline — which is why ownerless ad-hoc retry makes the window incomputable, and why the one-owner law is also the idempotency law.
- Hedging adds a concurrency dimension to the window: the deduplication store must tolerate overlapping duplicates in flight simultaneously, not merely sequential repeats — a window sized for sequential retry under-specifies a hedged seam.
- Server-directed delays extend the horizon past any computed bound: delay-generator output bypasses the configured delay cap, so idempotency keys sized to the backoff curve alone expire early under server pushback.
- Key lifetime derives from the allotment, never from backoff parameters — the allotment is the only bound that survives every delay source.
- Idempotency is a typed column on the hop row, not a comment: each seam row declares its idempotency class, and the policy shape derives from it.
- Idempotent rows admit retry and hedging; non-idempotent rows admit single pass plus typed fault, or method-filtered retry where the seam's verb vocabulary distinguishes safe calls.
- Hedging is admitted only on idempotent rows because overlapping attempts can both commit — concurrent duplication is strictly stronger than sequential duplication.
- A non-idempotent operation that must retry is repaired by upgrading the operation — minting a dedup key and flipping the row's idempotency column — never by weakening the single-pass posture; the row column is the forcing function for making operations idempotent.
- The key is minted above the owner: an idempotency key generated inside the retried callback changes per attempt and defeats itself.
- The key is fixed in the context before the pipeline runs, attempts inherit it, and the window clock starts at first send — all of which falls out of the owner being held at the root where the context is constructed.
- The per-execution operation-key channel is the key's natural transport: carried by the context, it reaches every attempt and lands on every telemetry event of the execution — key, attempts, and evidence correlate by construction rather than by joins.

## degradation and override

- Degradation is a fold over one retained capability set, not a scatter of booleans: capability levels form a closed, ranked vocabulary, and the current level derives by folding hop-health evidence into the highest level whose requirements all hold.
- The fold's inputs are typed facts: breaker states read from the state-provider surface, conflict receipts, rejection-rate facts — every input already exists as evidence; the fold introduces no new signal source.
- Level requirements are predicates over capability groups — the same hop-row sets the override binding names — so the fold's domain and the override rail's domain are one vocabulary, never two group systems.
- The fold is pure and deterministic: the same fact set always derives the same level, so any historical level is replayable from receipts — degradation decisions are auditable computations, not operator lore.
- Every consumer reads the one derived level; a consumer folding raw signals privately is the defect, and its symptom is a level transition with no receipt chain.
- Escalation is immediate: a single qualifying breach drops the level now — the breaker's open transition is already the debounced edge, and re-debouncing it adds dead time.
- Recovery is hysteretic: re-promotion requires consecutive-healthy evidence AND a minimum dwell at the lower level.
- Both hysteresis thresholds are policy values on the level row; symmetric escalate/recover thresholds (the rejected form) oscillate under flapping dependencies.
- Forced override beats derived state, and release re-derives: the override rail is a two-verb union — force(level) and release.
- Force pins the fold's output; release returns to derivation from live evidence, never to the pre-force level, because state may have moved while pinned.
- Both verbs are idempotent — re-forcing the current pin and releasing an unpinned state are no-ops — so receipts deduplicate on (verb, level, epoch) and repeated operator commands cannot stack.
- One configuration row projects the total state into this union, and operator configuration, the wire verb, and programmatic release all speak the same two verbs — three entry points, one rail, no third state.
- The mechanical anchor for forced-dark is the multi-breaker manual control: one control instance registered across every breaker in a capability group isolates and closes them as a set; construction-time isolation supports boot-into-degraded.
- The isolate verb is sticky until the close verb, and close resets health statistics — which is exactly force/release semantics, with re-derivation falling out of the statistics reset.
- A pinned control re-applies isolation to every breaker that registers while it is pinned — the override survives pipeline reloads and lazy instance materialization, because new generations isolate themselves at registration.
- Override durability across reload is therefore structural, not bookkeeping: the control instance lives outside pipeline generations, and no reload can silently lift a forced-dark state.
- Forced-dark composes with lazy materialization: isolation declared at construction applies before the first request, so a degraded boot never serves a single undegraded call.
- The isolated rejection type stays distinguishable from the organic open-circuit rejection, so receipts attribute degradation to the operator, not the dependency.
- Degradation receipts carry the transition evidence: from-level, to-level, the triggering fact, and whether the transition was derived or forced — the degradation history is a fold over these receipts.

## epoch and handoff

- The composition epoch is fenced by registry disposal: tearing down the root force-disposes every hop pipeline including those still referenced, and any later use of a stale reference throws — a holder from the previous epoch cannot silently operate inside the new one.
- Per-pipeline disposal callbacks are the reclaim hooks: release external claims, emit handoff receipts — reclaim is declared beside the claim, at the row.
- Graceful handoff drains: the disposal path waits for in-flight executions before disposing — bounded wait, then dispose regardless.
- The drain bound converts a wedged execution from a hang into a race; handoff receipts therefore record whether the drain completed or expired, and an expired drain is a distinct evidence class, not a silent edge.
- Crash reclaim fences: no drain ran, so a successor treats every persisted in-flight claim as suspect and adjudicates by epoch comparison.
- In-flight claim records written at send time carry the operation key and the epoch — successor adjudication needs only the receipt stream, never the predecessor's memory.
- Staleness is decided by epoch ordering, never by wall-clock age alone; the cross-process epoch-fence mechanics arrive settled and are composed, not restated here.
- Rebuild re-derives, never restores: a restarted root re-declares every hop row from configuration and re-derives the degradation level from live evidence.
- Strategy internals — breaker statistics, limiter queues — are process-local by construction and intentionally unrecoverable; the only state worth persisting across epochs is claims and receipts, never strategy state.
- Mid-life policy change is the third lifecycle path beside handoff and crash: per-row reload rebuilds one hop's pipeline in place while the old generation finishes in-flight work and is disposed in the background — ownership continuity without an epoch bump.
- Reload (same owner, new policy) and epoch change (new owner set) must emit distinguishable succession receipts, with a monotonic generation counter per row — a successor reasoning over receipts needs to know whether the claim survived and how many policy generations it crossed.
- Handoff receipts are written by the predecessor and adjudicated by the successor: writer and reader are different epochs by definition, which is why the receipt schema evolves append-only.

## divergent

- hop-registry-claims — the registry IS the claim cell, no parallel structure: deferred builder registration gives atomic first-wins claim semantics with a synchronous verdict, materialization gives lazy per-instance state, the dispose callback gives reclaim, and the reload token gives policy succession.
- hop-registry-claims — a bespoke claim table beside the registry would re-implement all four capabilities with weaker guarantees; the claim design therefore reduces to a key-shape decision — one process-wide key record (hop identity, two formatted axes) whose builder-axis equality defines what "second owner" means.
- hop-registry-claims — the one capability the registry verdict lacks, naming the incumbent in the receipt, is supplied by the claim row keeping the declaration symbol beside the key — the row completes the cell, not a second cell.
- hop-registry-claims — claims compose hierarchically without nesting registries: a suite-level claim (this process owns outbound hop X within the suite) and a process-level claim (this root owns pipeline X within the process) are the same row shape at two altitudes.
- hop-registry-claims — the suite-level cell rides the settled token-gated lifecycle law, so claim escalation across process boundaries is rows plus epoch stamps — zero new machinery at the wider altitude.
- hop-registry-claims — the conflict receipt doubles as a migration instrument: because the verdict carries both declaration symbols and the attempt-multiplication factor, a fleet-wide sweep that promotes scattered retry into root-held rows can rank repairs by m × n severity from receipts alone, making consolidation measurable rather than archaeological.
- retry-layer-classification — the table's force comes from its misclassification costs, stated as receipts: seam-work classified as domain retry produces wire exceptions smuggled into rails (capture without typing); domain work classified as seam retry produces predicates that cannot see the failure (silent single-pass with a dead pipeline).
- retry-layer-classification — store work classified as either other row produces replayed transactions: the only data-corrupting misclassification, which is why it anchors the audit order.
- retry-layer-classification — the cost ranking orders the audit: verify store seams first, then wire seams, then rails, descending by blast radius.
- retry-layer-classification — the deadline dimension classifies with the same table: total allotments live only at the layer that owns the retry loop, so a deadline declared at a non-owning layer is the temporal form of a second owner — two budgets racing — and the repair is identical: move the deadline to the owner's allotment class and let inner layers inherit linked child scopes.
- degradation-override-rail — the override rail generalizes to every forced state in the process: force/release with release-re-derives is the only sound shape for any operator override over derived state (sampling rates, feature gates, chaos enablement), so the rail is one generic union over a level-vocabulary type parameter; the degradation instance is merely its first consumer.
- degradation-override-rail — a second override surface with restore-previous semantics is the rejected form: restore-previous resurrects decisions whose evidence has expired, and any surface offering it forks the override rail into two incompatible state machines.
- degradation-override-rail — dwell and consecutive-healthy thresholds are derivable, not guessed: minimum dwell ≥ the breaker's break duration (re-promoting inside a break window guarantees an immediate re-breach), and the consecutive-healthy count ≥ the fraction of the health window's minimum throughput that restores statistical significance.
- degradation-override-rail — tying the hysteresis row to the hop rows it folds keeps the two policy surfaces from drifting independently — the quantitative reason the degradation fold and the hop registry live on one page.
