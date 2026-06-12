# lifecycle-drain-health — bedrock

## ordered start

- The exact start sequence: link the caller token with the application-stopping token into one CTS (the startup timeout arms a cancel-after on it) → `IHostLifetime.WaitForStartAsync` → resolve the hosted-service enumerable (registration order is start order) → startup-validation sweep (the startup validator resolved optionally) → `StartingAsync` across all lifecycle services → per-service `StartAsync` → `StartedAsync` across all lifecycle services → the started token fires.
- The validation sweep sits after the lifetime gate and before any service code — invalid policy means zero service side effects.
- The startup timeout spans the whole boot path — lifetime wait, validation sweep, and all three start phases share one deadline; per-service start budgets are fractions of it, not additions to it.
- Lifecycle-hook detection happens once at start: the lifecycle-service set is computed from the resolved hosted services, and with the sealed collection that set is immutable.
  - Hooks exist exactly for start-time members, never late arrivals.
- Because the lifetime adapter is already attached when the sweep runs, a validation failure is reported through the modality's own channel.
  - The service manager sees a failed start, not a silent exit.
- The pre/post hooks (`StartingAsync`/`StartedAsync`, `StoppingAsync`/`StoppedAsync`) run as a phase across every lifecycle service before the next phase begins — they are phase barriers, not per-service wrappers.
- A lifecycle service can therefore stage cross-service work — open a shared resource in the pre-phase, verify it in the post-phase — with guaranteed ordering relative to all siblings.
- Failure routing differs by concurrency mode: sequential start aborts at the first failing service, and later services never start; concurrent start runs all and aggregates every failure.
- One start failure rethrows on its original stack; several throw one aggregate — the boot failure inventory mirrors the build-validation pattern: complete inventory, single throw.
- Sequential is the safe default when later services assume earlier ones started; concurrent start is admissible only when the start set is provably order-free.
  - The participation model below turns order-freedom into a declared property rather than a guess.
- `HostOptions` is host-configuration-bound under invariant parsing through four symbolic keys — `shutdownTimeoutSeconds`, `startupTimeoutSeconds`, `servicesStartConcurrently`, `servicesStopConcurrently`.
  - Lifetime policy is rankable configuration, not code.
- Lifetime policy defaults: thirty-second shutdown timeout, infinite startup timeout, sequential start and stop.
- `BackgroundService.StartAsync` queues `ExecuteAsync` to the thread pool via `Task.Run` and returns already-completed: the boot path never waits on, and is never failed by, a background loop.
- Even an instantly-throwing `ExecuteAsync` does not abort start — the fault routes through the post-start fault fold asynchronously.
- Consequence: fail-fast boot checks cannot live in `ExecuteAsync`; they belong in the startup-validation sweep, the `StartingAsync` phase, or an explicit `StartAsync` override.
- The historical block-the-boot-with-a-synchronous-prefix trap is inverted — the live trap is assuming `ExecuteAsync` failures surface at boot.
- Background fault routing after start: a faulted execute task is logged and folded through `BackgroundServiceExceptionBehavior` — stop-host (the default) signals coordinated shutdown; ignore leaves a logged zombie loop.
- The background fault is never rethrown from the run path: a crashed background loop produces a clean-looking process exit unless the fault is independently folded into a receipt or an exit-code seam.
- Cancellation-shaped faults during shutdown are suppressed entirely — cooperative-stop noise never masquerades as failure.
- `BackgroundService.StopAsync` cancels the loop's linked token and awaits the execute task with throw-suppression under the stop token.
  - A loop that ignores its token holds its slot until the shutdown deadline poisons the wait; token discipline inside the loop is what makes band budgets meaningful.
- The shutdown wait verbs and run verbs are sugar over start + wait-for-stopping + stop; embedded modalities drive start and stop directly and skip the wait loop entirely.

## stop and signals

- The stop sequence: shutdown-timeout CTS → `StoppingAsync` phase (reverse registration order) → the stopping token fires → per-service `StopAsync` (reverse order) → `StoppedAsync` phase → the stopped token fires → `IHostLifetime.StopAsync` last.
- Stop never aborts early: every service's stop runs regardless of earlier failures, and all exceptions — including the lifetime's own stop failure — aggregate at the end.
- Drain code can therefore rely on being invoked even when a sibling failed — the property the band model builds on.
- The stopped notification fires even when stop phases threw — observers of the stopped edge see it on failed shutdowns too, so stopped-edge logic must not infer success.
- Stop-initiation topology is two-path and the token order inverts between them: signal-initiated shutdown fires the stopping token first, then runs the stop sequence; direct programmatic stop runs the `StoppingAsync` phase before the stopping token fires.
- Law from the inversion: code must not assume the stopping token precedes the stopping phase or vice versa.
  - Keying drain admission off either alone is a latent ordering bug; key it off the phase cell, which observes both edges and normalizes them.
- Stop on a never-started host degenerates to signaling the stopping and stopped tokens — phase logic must tolerate stop-without-start as a legal path.
- Lifetime token callbacks execute inline on the signaling thread at cancellation: work registered on the started/stopping/stopped tokens delays the transition itself — tokens carry edges; work belongs in hosted phases.
- One cancellation spine, two segments: the start segment (caller ∪ stopping ∪ startup timeout) and the stop segment (caller ∪ shutdown timeout).
- Every component receives its segment token from its phase; no component creates a root CTS, and a component-private timeout is a linked child so the spine always wins.
- Console lifetime signal set: SIGINT, SIGQUIT, SIGTERM through POSIX signal registrations, with Windows shutdown routed through a distinct handler.
- The console handler cancels default process termination and signals stop-application — every interactive termination becomes a full drain.
- Signal registration is platform-gated — on platforms without POSIX signal support the console lifetime degrades to token-only operation, and drain initiation must arrive programmatically.
- The systemd lifetime registers SIGTERM only and is otherwise notify-driven: ready-notify fires on the started token, stopping-notify on the stopping token.
- The systemd lifetime's own stop hook is a no-op — drain rides the host's path; the notify channel is reporting, never control.
- The service-control lifetime derives from the service base class and bridges the manager callbacks — start, stop, shutdown — into the same two signals.
  - Manager-initiated stop and console-initiated stop are indistinguishable to drain code.
- Net signal law: all modality lifetimes converge on the stop-application edge; drain logic is written once against the phase machine and is modality-blind.
- Service-manager clock interplay: the stopping-notify starts the manager's own kill countdown, so the host's shutdown timeout must sit strictly inside the manager's stop window — two nested deadlines, the host's inner.
- The outer deadline is environmental policy verified at boot — a degradation fact when absent or misordered, never an assumption.

## banded drain

- The platform natively provides exactly one cooperative/forced pair: the stopping edge (cooperative — finish in-flight, refuse new) and the shutdown-timeout CTS (forced — poisons every remaining stop await at the deadline).
- The banded drain law generalizes the pair: drain is an ordered band sequence — stop admission → drain in-flight work → flush durable effects → emit final receipts.
- Each band is a cooperative await under a band budget with a forced per-band token at the band edge, the host deadline as the outermost guard.
- Because stop never aborts early, a band overrun cancels that band's stragglers and the next band still runs — receipts and flushes survive a hung worker by construction, not by luck.
- Band assignment is registration-order-derived: stop order is reverse start order, so declaring hosted participations in dependency order (sources before sinks before flushers) yields the correct drain order with zero extra mechanism.
- The band table is a property of the module-contribution order, and band budgets are policy rows — fractions of the shutdown timeout — never literals inside drain code; retuning drain is a policy edit.
- Admission fencing composes the settled serializable-permit law: the admission band flips the gate and the drain band awaits the permit population reaching zero within its budget; permit mechanics arrive from the boundary layer and are never re-derived here.
- The drain receipt carries the band ledger — per band: budget, elapsed, completed-cooperatively versus forced, residual count — making "did we drain clean" a fold over one receipt instead of log archaeology.
- Escalation grammar is the same two-phase pair at every scale: per-operation (operation token linked to band token), per-band (band budget), per-process (shutdown timeout), per-supervisor (manager kill window).
- Each level is cooperative-then-forced with the next level as backstop; a level designed without a forced edge stalls the level above it.

## health evaluation

- The evaluator runs every registration concurrently — one pooled task per check, joined at the end — so report latency is the slowest check, never the sum; a slow probe cannot serialize its siblings.
- Each check executes inside its own async service scope: contributors may take scoped dependencies and are isolated per evaluation.
- A contributor holding state across evaluations is mis-shaped — evaluation state belongs to the rank cell, not the probe.
- Health evaluation is total: a contributor exception becomes an entry carrying the registration's failure status with the exception attached; no contributor can crash the evaluator.
- A per-registration timeout (linked CTS) becomes the same failure-status entry with a timeout description, distinguished from outer cancellation — slow and broken stay separable in the report.
- Severity is registration policy, not contributor code: the registration's failure status decides whether a failing probe grades unhealthy or degraded.
  - The same probe registered twice serves liveness (unhealthy-grade) and capability reporting (degraded-grade) without code duplication.
- Registration is the policy row: name, tags, timeout, failure status, plus per-registration delay/period cadence overrides — the complete probe posture is data.
- The registration store is itself composition-visible (the service-options registration set) — programmatic registry folds at the root can derive probe rows from module contributions instead of hand-listing them.
- Tags partition the contributor set into named projections: filtered evaluation takes a registration predicate, which is how liveness, readiness, startup, and capability families share one registry without parallel registries.
- The registration intake family — typed contributor, fixed instance, inline delegate, async delegate, type-activated with constructor arguments.
  - Converges on the same registration row; the form is intake convenience, the row is the truth.
- Entry payloads carry evidence, not just status: description, duration, exception, tags, and a data dictionary.
  - Retained capacity, queue depth, schema versions ride the data channel, which is what the rank fold consumes; a probe reporting only a status wastes its evidence channel.
- Aggregate status is the minimum across entries with the status order unhealthy < degraded < healthy, short-circuiting at unhealthy — one degraded entry degrades the whole report.
- Aggregation is worst-of, never quorum; partial-capability semantics must live in the rank fold below, not in the report status.
- Publisher cadence: the publisher host groups registrations by effective (delay, period) and creates one timer per distinct pair.
  - Heterogeneous cadences cost one timer per cadence class, not per check, and each timer evaluates only its group.
- Publisher options defaults: five-second delay, thirty-second period, thirty-second timeout, optional registration predicate.
- Publishers are projections of evaluation results; they own no state and never become a second machine.
- The publisher host is itself a hosted service: cadence starts and stops with the host's own phases, and each publisher run is canceled at the publisher timeout — a slow publisher loses its run, never accumulates backlog.
- On-demand evaluation (full or predicate-filtered) is always available beside the cadence channel — the rank fold can evaluate at transition-relevant moments; publisher cadence is only the push projection.
- The stop-application signal is idempotent — repeated stop requests from signals, faults, and programmatic paths collapse into one stopping edge, so multiple escalation sources need no mutual coordination.

## degradation rail

- Health contributors project capability state; the runtime owns the state machine consuming it — the split is absolute: probes never mutate runtime state, runtime state never lives in a probe.
- The degradation rail is a rank algebra: a closed ordered rank family (full → reduced → essential → drain-only), each rank carrying its retained capability set as data.
- Consumers ask "is capability X retained at the current rank", never "what rank are we" — rank names stay private to the fold, so re-ranking never ripples.
- Transition law is asymmetric by design: escalation is immediate — one qualifying evaluation lifts the rank now, because capability loss must propagate at once.
- Recovery is hysteretic — a rank lowers only after N consecutive qualifying windows, N a policy value per rank edge; flapping dependencies cannot oscillate the system because the asymmetry absorbs them structurally.
- Rank transitions emit typed receipts (from-rank, to-rank, triggering entry, evidence window); the current rank lives in one cell every projection folds from — health publisher, admission gates, capability queries.
- No component holds a private copy of "are we degraded" — the cell is the single truth, and the receipts are its history.
- Provisioning-is-verification-only: boot-time environment checks — peer reachable, store schema acceptable, socket present, manager deadline configured.
  - Are health contributors evaluated at the start phase whose failures fold to an initial rank.
- A missing optional capability boots the process into a reduced rank with a receipt; the process never performs the alteration itself.
- The same contributor keeps verifying at cadence, so a later environmental repair recovers the rank through ordinary hysteresis.
  - No special-case re-provisioning path exists, because verification and recovery are one mechanism.
- Mandatory-versus-optional capability is a rank-table fact, not a probe fact: a probe failure mapping to drain-only rank is the typed spelling of "cannot run"; the same probe in another process's table may map to reduced.
  - The table owns criticality, probes stay reusable.

## divergent — phase-machine-lifecycle

- One closed phase family — created → validating → starting(pre, services, post) → running → degraded(rank) → draining(bands) → stopped | faulted — held in one atomic phase cell.
- The cell advances only from five edge sources: lifecycle hook phases, lifetime tokens, background-fault routing, the health rank fold, and the drain band sequence.
- Every "are we up / degraded / draining" question anywhere in the process is a projection of the cell; the host's own tokens and hooks are the cell's inputs, never parallel truths consumers read directly.
- The cell is precisely what dissolves the two-path stop-ordering hazard — consumers key off the cell's single normalized edge order, not raw token-versus-hook order.
- Phase-keyed behavior is a total fold over the phase enum: admission policy, health-report annotation, receipt tagging, and signal handling each declare one row per phase; an unhandled phase is a compile-time hole, not a runtime surprise.
- A new phase — a warming phase between started and running — is one enum case plus the rows the compiler then demands.
- The lifecycle-service surface collapses to one runtime-owned hosted lifecycle adapter: domain modules contribute phase participations (which phases they act in, with what budget) as rows, and the single adapter folds them into the hook phases.
- N modules share one hosted service instead of N hosted services with implicit ordering contracts — start order degenerates to row order in one table.
- The adapter is also where the concurrent-start decision becomes safe: participations declare their dependency edges as data; the adapter starts independent rows concurrently and dependent rows in order.
  - The concurrency flag stops being a global gamble and becomes a per-edge derivation.
- Faulted is a real phase, not an exception path: background-fault routing, band-overrun escalation, and start-failure aggregation all land in the same faulted arm with their evidence attached.
  - Exit-code mapping, final receipts, and supervisor signaling are one fold over the faulted payload.

## divergent — banded-drain-escalation

- The two-phase pair as a reusable value: (cooperative: signal + await-with-budget; forced: linked-token cancel) parameterized by budget and scope is one generic shape instantiated at operation, band, process, and supervisor levels.
- Drain, timeout, and kill-escalation cease to be separate mechanisms and become one policy-row family with four scopes.
- The forced edge of level k is the cooperative signal of level k+1 — the escalation ladder is compositional, and verifying it reduces to verifying each adjacent budget pair is strictly nested.
- Drain-aware admission inverts cleanly: the admission gate is a phase projection — running, and not past the admission band — so refusing new work during drain needs no flag plumbing; the phase cell is the flag.
- The refusal carries a typed drain rejection so upstream peers distinguish "draining, go elsewhere" from "broken" — drain rejections are routing signals, not faults.
- Final-receipt flush is itself the last band with the largest forced budget: receipts about the drain must survive the drain, so the receipt sink's stop participation pins to the last band by construction (first-registered participation).
- The band ledger's own emission is synchronous within that band — the one place where awaiting durability inside stop is law rather than smell.
- Suspend/resume and crash-restart reuse the same ledger vocabulary: a boot that finds a previous drain ledger without a final receipt knows the prior drain was forced at some band.
  - Restart-time recovery work is selected by the residual band, not by guessing.

## divergent — health-degradation-algebra

- Contributors, ranks, and capabilities form a fold pipeline with three closed tables: contributor rows (probe, tags, severity policy, cadence), rank rows (rank, retained capability set, recovery window), and a mapping fold (tag-grouped worst-of entry status → candidate rank).
- The entire degradation behavior of a process is recoverable from the three tables; no health logic lives outside them.
- Capability sets compose across processes: a suite-level rank is a fold over member processes' published ranks using the same worst-of-with-hysteresis algebra.
- The algebra is scale-free — suite degradation needs no second formalism, only a contributor that reads peer ranks.
- The timeout-as-degraded pattern: register the same dependency twice — short timeout mapping to degraded (latency evidence) and long timeout mapping to unhealthy (availability evidence).
- Response-time bands become rank pressure using registration policy alone; the rank fold distinguishes slow from down without any contributor measuring time itself.
- Rank-driven cadence feedback: the publisher predicate and per-registration cadence overrides let a degraded rank tighten the failing family's evaluation period (faster recovery detection) while relaxing healthy families.
  - Cadence becomes a function of rank, declared as rows, closing the evaluation-state loop without a controller component.
- The degradation receipts and the drain band ledger share one evidence grammar — typed transition, cause, budget, residual.
  - Operational state changes of every kind read as one receipt family — the runtime's behavior is reconstructable from receipts alone.
