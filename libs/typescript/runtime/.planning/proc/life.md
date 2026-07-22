# [RUNTIME_LIFE]

Lifecycle and health are one owner because they are one skeleton: register ranked rows at Layer construction, run each row under its own budget on a severed fiber, convert every outcome to evidence, fold the evidence into a graded receipt. `Life` holds the closed phase spine — `booting → running → draining → halted` — in one `SubscriptionRef` every lifecycle question projects from, the ranked drain registry whose fold runs on interrupt before the graph's finalizers release resources, and the probe registry whose kind rows — `started`, `ready`, `live`, each carrying its canonical k8s route — feed a memoized concurrent report fold. The budgeted row executor is spelled exactly once: drain rows and probe rows are two registries over one `_bounded` fold, their verdicts two graders of one `Exit`-of-`Option` evidence shape. Readiness composes the phase — outside `running` the ready report fails by fold, so the drain flip stops traffic instantly — while liveness ignores it so an orderly drain is never mistaken for a hang. The drain total budget is the number `iac` mirrors into `terminationGracePeriod`; a `process.on("SIGTERM")` listener, an exit-hook library, and teardown-as-ordinary-step are unspellable because the runtime row's `runMain` already owns the signal edge. The module is `runtime/src/proc/life.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                    | [PUBLIC] |
| :-----: | :------------- | :------------------------------------------------------------------------ | :------- |
|  [01]   | `PHASE_SPINE`  | the phase vocabulary, the cell, the parked boot entry                     | `Life`   |
|  [02]   | `RANKED_FOLD`  | the one budgeted row executor both registries share                       | `Life`   |
|  [03]   | `DRAIN_BANDS`  | ranked drain rows, the two-tier budget, the drain receipt                 | `Life`   |
|  [04]   | `PROBE_ROUTES` | the kind/route anchor, the grade lattice, the memoized phase-gated report | `Life`   |

## [02]-[PHASE_SPINE]

[PHASE_SPINE]:
- Owner: the phase tuple and the cell — the tuple is a semantic order (lifecycle rank is load-bearing; the anchor derives `Phase` and no consumer re-lists states); the cell starts at `booting`, `life.online` stamps `running` (the boot module's last act before serving), and only the drain fold advances further; the published `phase` is the read-only `Subscribable` projection of the interior cell, so a consumer reads and subscribes and a write is unspellable.
- Law: the park is the drain trigger — `life.parked` is `online` then `Effect.never` with the drain fold attached through `Effect.onInterrupt`, so the boot module runs `row.main(Effect.scoped(Effect.provide(life.parked, root)))` and a delivered `SIGINT`/`SIGTERM` (which `runMain` converts to root-fiber interruption) executes the ordered drain BEFORE scope close releases resources — choreography first, finalizers second, exactly once, on every exit class.
- Law: `Layer.launch` remains the boot for graphs with no ordered drain; the moment one drain step exists, `parked` is the entry — two boot shapes selected by whether choreography is registered, never mixed.
- Boundary: phase consumption is one-directional — the ready fold gates on the cell, the serving edge stops intake by it, and `iac` aligns `terminationGracePeriod` to the drain budget row rather than a second constant; browser lifecycle is the ui wave's own boot fact.
- Entry: `Life.register(step)` and `Life.probe(row)` from any Layer build (`accessors: true`); `life.parked` in the boot module; `life.phase` for observers.
- Packages: `effect` (`SubscriptionRef`, `Subscribable`, `Deferred`, `Ref`, `Chunk`), `./config.ts` (`Setting`).

## [03]-[RANKED_FOLD]

[RANKED_FOLD]:
- Owner: `_bounded` — the one budgeted row executor: measure the open instant, run the row's effect under `Effect.exit` with the budget applied as `Effect.timeoutOption` over `Effect.disconnect`, measure the close, and return the `Exit`-of-`Option` evidence beside the elapsed span; both registries fold through it, so the lapse-is-a-verdict law, the crash-is-evidence law, and the severed-deadline law are stated once.
- Law: every deadline rides a severed fiber — the drain fold runs inside the interrupt's masked finalizer, where a bare timeout waits instead of interrupting, so `Effect.disconnect` severs the row's work onto its own fiber and the deadline settles on time while the shielded work finishes in background; a lapse is a verdict, never an abort.
- Law: a row never fails its fold — a crash converts through `Effect.exit`, a lapse folds from `Option.none`, so the surrounding report is total and the serving edge carries zero recovery arms; the graders are the only per-surface difference — the drain grader folds to `drained | lapsed | crashed`, the probe grader to the grade lattice with lapse and crash landing at `fail` with their detail.
- Growth: a third ranked surface (a warm-up band, a maintenance sweep) is one registry plus one grader over the same executor.
- Packages: `effect` (`Clock`, `Duration`, `Effect`, `Exit`, `Option`).

```typescript signature
import {
    Array,
    Chunk,
    Clock,
    DateTime,
    Deferred,
    Duration,
    Effect,
    Exit,
    Option,
    Order,
    Record,
    Ref,
    Schema,
    Struct,
    type Subscribable,
    SubscriptionRef,
    pipe,
} from 'effect';
import { Setting } from './config.ts';

const _PHASES = ['booting', 'running', 'draining', 'halted'] as const;
const _BANDS = { intake: 0, domain: 10, report: 90 } as const;
const _GRADES = { pass: { rank: 0 }, warn: { rank: 1 }, fail: { rank: 2 } } as const;
const _KINDS = { started: { route: '/startupz' }, ready: { route: '/readyz' }, live: { route: '/livez' } } as const;

class _Graded extends Schema.Class<_Graded>('Life/Graded')({
    label: Schema.NonEmptyString,
    grade: Schema.Literal(...Struct.keys(_GRADES)),
    elapsed: Schema.DurationFromMillis,
    detail: Schema.optionalWith(Schema.String, { as: 'Option' }),
}) {}

class _Report extends Schema.Class<_Report>('Life/Report')({
    kind: Schema.Literal(...Struct.keys(_KINDS)),
    overall: Schema.Literal(...Struct.keys(_GRADES)),
    rows: Schema.Array(_Graded),
    at: Schema.DateTimeUtc,
}) {}

class _Row extends Schema.Class<_Row>('Life/Row')({
    label: Schema.NonEmptyString,
    rank: Schema.Int,
    verdict: Schema.Literal('drained', 'lapsed', 'crashed'),
    elapsed: Schema.DurationFromMillis,
}) {}

class _Receipt extends Schema.Class<_Receipt>('Life/Receipt')({
    at: Schema.DateTimeUtc,
    rows: Schema.Array(_Row),
    pending: Schema.Array(Schema.NonEmptyString),
    disposition: Schema.Literal('drained', 'expired'),
    landed: Schema.Literal(..._PHASES),
}) {}

class LifeFault extends Schema.TaggedError<LifeFault>()('LifeFault', {
    operation: Schema.Literal('register', 'probe'),
    phase: Schema.Literal(..._PHASES),
}) {}

declare namespace Life {
    type Phase = (typeof _PHASES)[number];
    type Verdict = 'drained' | 'lapsed' | 'crashed';
    type Grade = keyof typeof _GRADES;
    type Kind = keyof typeof _KINDS;
    type Step = {
        readonly label: string;
        readonly rank: number;
        readonly budget: Option.Option<Duration.Duration>;
        readonly run: Effect.Effect<void>;
    };
    type Probe = { readonly label: string; readonly kind: Kind; readonly run: Effect.Effect<Grade> };
    type Row = _Row;
    type Graded = _Graded;
    type Receipt = _Receipt;
    type Report = _Report;
    type _Kinds<T extends Record<Kind, { readonly route: string }> = typeof _KINDS> = T;
    type _Grades<T extends Record<Grade, { readonly rank: number }> = typeof _GRADES> = T;
}

const _TRANSITIONS = {
    online: { booting: 'running', running: 'running', draining: 'draining', halted: 'halted' },
    drain: { booting: 'draining', running: 'draining', draining: 'draining', halted: 'halted' },
    halt: { booting: 'halted', running: 'halted', draining: 'halted', halted: 'halted' },
} as const satisfies Record<'online' | 'drain' | 'halt', Record<Life.Phase, Life.Phase>>;

const _byRank: Order.Order<Life.Step> = Order.mapInput(Order.number, (step: Life.Step) => step.rank);
const _byGrade: Order.Order<Life.Grade> = Order.mapInput(Order.number, (grade: Life.Grade) => _GRADES[grade].rank);

const _bounded = <A>(
    run: Effect.Effect<A>,
    budget: Option.Option<Duration.Duration>,
): Effect.Effect<{ readonly outcome: Exit.Exit<Option.Option<A>>; readonly elapsed: Duration.Duration }> =>
    Effect.gen(function* () {
        const opened = yield* Clock.currentTimeMillis;
        const outcome = yield* Effect.exit(
            Option.match(budget, {
                onNone: () => Effect.map(run, Option.some),
                onSome: (limit) => Effect.timeoutOption(Effect.disconnect(run), limit),
            }),
        );
        const closed = yield* Clock.currentTimeMillis;
        return { outcome, elapsed: Duration.millis(closed - opened) };
    });

const _drainGrade: (outcome: Exit.Exit<Option.Option<void>>) => Life.Verdict = Exit.match({
    onFailure: () => 'crashed' as const,
    onSuccess: Option.match({ onNone: () => 'lapsed' as const, onSome: () => 'drained' as const }),
});

const _probeGrade: (outcome: Exit.Exit<Option.Option<Life.Grade>>) => readonly [Life.Grade, Option.Option<string>] = Exit.match({
    onFailure: () => ['fail', Option.some('crashed')] as const,
    onSuccess: Option.match({
        onNone: () => ['fail', Option.some('lapsed')] as const,
        onSome: (grade: Life.Grade) => [grade, Option.none<string>()] as const,
    }),
});
```

## [04]-[DRAIN_BANDS]

[DRAIN_BANDS]:
- Owner: the drain fold — a step is a row (`label`, `rank`, `budget: Option<Duration>`, `run`) admitted while the phase is `booting | running`; one semaphore linearizes registration with the transition-and-snapshot seam, and late registration fails as `LifeFault` rather than disappearing behind the snapshot. The total `_TRANSITIONS` matrix prevents `draining | halted → running`, the fold flips to `draining` before reading the ranked queue, and `halt` remains terminal under every disposition.
- Law: budgets are two-tier by construction — the per-row `Option<Duration>` bounds its row and `Setting.life.drain` bounds the whole fold, both over the severed-fiber executor; a step that ignores its budget cannot stall the process, and the total is the number `iac` mirrors into the pod's grace period.
- Law: the receipt is total and truthful — `Life.Row` and `Life.Receipt` are `Schema.Class` authorities, each graded drain row appends to the interior ledger and leaves the pending queue as it settles; `disposition: drained | expired` distinguishes normal completion from total-budget expiry, and `pending` names every snapshotted domain row the total budget omitted. The terminal phase stamps BEFORE the `Deferred` settles so the receipt's `landed` equals the cell the moment it is observable; every drain disposition settles `life.settled`, and no shutdown observer can suspend indefinitely. The report band runs after the settle, so a terminal reporter — the `otel/emit` flush is the standing rank-90 registrant — reads the settled receipt without awaiting evidence its own completion gates, and report-band rows are post-receipt forensics, never receipt members.
- Law: growth is a row — a new graceful concern (stop intake, pause queues, flush spans, checkpoint state) is one `register` call at its owner's Layer build with a rank inside the `_BANDS` anchor (0–9 intake, 10–89 domain, 90+ reporters); no new surface, hook API, or event bus.
- Receipt: `Life.Receipt` via `life.settled`.

## [05]-[PROBE_ROUTES]

[PROBE_ROUTES]:
- Owner: the probe vocabulary and the report fold — `_KINDS` closes the taxonomy with its serving routes (`/startupz`, `/readyz`, `/livez` — the k8s trio), `_GRADES` closes the verdict lattice with rank columns so worst-of merge is an `Order` projection; `Life.report(kind)` filters the registry to the kind, sweeps every probe with unbounded concurrency (probes are independent — accumulation, never abort), bounds each by `Setting.life.probe` through the shared executor, and merges grades worst-of; zero probes fold to `pass`, vacuously healthy.
- Law: kind semantics are the row's contract — `started` answers once per boot (slow warm-up allowed), `ready` answers "route traffic to me now" (dependency reachability, queue depth), `live` answers "is this process worth keeping" (event-loop responsiveness, deadlock sentinels); a dependency check inside a liveness probe is the classic self-inflicted restart loop and is the named defect.
- Law: the ready fold gates on the phase first — outside `running` the report is `fail` with the phase as detail before any probe runs; liveness never reads the phase.
- Law: memo posture follows kind semantics — `started` uses `Effect.cached` and settles once per boot, while `ready | live` use `Effect.cachedWithTTL`; the readiness phase gate remains outside the cached probe sweep, so a transition to `draining` returns failure immediately even when the prior probe result remains warm.
- Law: routes are data — `Life.route(kind)` projects the row; the serving edge mounts the three routes from this anchor and encodes the report (`pass/warn → 200`, `fail → 503`), `iac` writes the same three paths into workload manifests, so the path never exists twice.
- Receipt: `Life.Report` — one `Schema.Class` (kind, overall grade, `Life.Graded` rows with millis-encoded elapsed and `Option` detail, instant) riding the owner as a static, so the serving edge encodes the derived wire twin (`pass/warn → 200`, `fail → 503`), telemetry consumes the same rows, and no hand-serialized health body or second health shape exists.

```typescript signature
class Life extends Effect.Service<Life>()('runtime/Life', {
    scoped: Effect.gen(function* () {
        const setting = yield* Setting;
        const cell = yield* SubscriptionRef.make<Life.Phase>('booting');
        const phase: Subscribable.Subscribable<Life.Phase> = cell;
        const steps = yield* Ref.make(Chunk.empty<Life.Step>());
        const probes = yield* Ref.make(Chunk.empty<Life.Probe>());
        const ledger = yield* Ref.make(Chunk.empty<Life.Row>());
        const pending = yield* Ref.make(Chunk.empty<Life.Step>());
        const settled = yield* Deferred.make<Life.Receipt>();
        const gate = yield* Effect.makeSemaphore(1);
        const advance = (signal: keyof typeof _TRANSITIONS): Effect.Effect<void> =>
            gate.withPermits(1)(SubscriptionRef.update(cell, (held) => _TRANSITIONS[signal][held]));

        const ran = (step: Life.Step): Effect.Effect<Life.Row> =>
            Effect.map(
                _bounded(step.run, step.budget),
                ({ elapsed, outcome }) =>
                    new _Row({
                        label: step.label,
                        rank: step.rank,
                        verdict: _drainGrade(outcome),
                        elapsed,
                    }),
            );

        const receipt = (disposition: Life.Receipt['disposition']): Effect.Effect<Life.Receipt> =>
            Effect.map(
                Effect.all({ at: DateTime.now, rows: Ref.get(ledger), remaining: Ref.get(pending) }),
                ({ at, rows, remaining }) =>
                    new _Receipt({
                        at,
                        rows: Chunk.toReadonlyArray(rows),
                        pending: Array.map(Chunk.toReadonlyArray(remaining), (step) => step.label),
                        disposition,
                        landed: 'halted',
                    }),
            );

        const drained = Effect.gen(function* () {
            const queue = yield* gate.withPermits(1)(
                Effect.zipRight(
                    SubscriptionRef.update(cell, (held) => _TRANSITIONS.drain[held]),
                    Effect.map(Ref.get(steps), (registered) => Array.sort(Chunk.toReadonlyArray(registered), _byRank)),
                ),
            );
            const [drain, report] = Array.splitWhere(queue, (step) => step.rank >= _BANDS.report);
            yield* Ref.set(pending, Chunk.fromIterable(drain));
            yield* Effect.forEach(
                drain,
                (step) =>
                    ran(step).pipe(
                        Effect.tap((row) => Ref.update(ledger, Chunk.append(row))),
                        Effect.tap(() => Ref.update(pending, Chunk.drop(1))),
                    ),
                { concurrency: 1, discard: true },
            );
            yield* advance('halt'); // the phase stamps before the receipt settles: landed equals the cell the moment it is observable
            yield* Effect.flatMap(receipt('drained'), (evidence) => Deferred.succeed(settled, evidence));
            yield* Effect.forEach(report, ran, { concurrency: 1, discard: true }); // post-receipt forensics: reporters read life.settled, their rows never join it
        }).pipe(
            Effect.disconnect,
            Effect.timeoutOption(setting.life.drain),
            Effect.asVoid,
            Effect.ensuring(
                // the total-budget expiry path: phase and receipt settle unconditionally with the rows graded so far — Deferred.succeed is a no-op on the normal path
                Effect.zipRight(
                    advance('halt'),
                    Effect.flatMap(receipt('expired'), (evidence) => Deferred.succeed(settled, evidence)),
                ),
            ),
        );

        const proven = (probe: Life.Probe): Effect.Effect<Life.Graded> =>
            Effect.map(_bounded(probe.run, Option.some(setting.life.probe)), ({ elapsed, outcome }) =>
                pipe(_probeGrade(outcome), ([grade, detail]) => new _Graded({ label: probe.label, grade, elapsed, detail })),
            );

        const swept = (kind: Life.Kind): Effect.Effect<Life.Report> =>
            Effect.gen(function* () {
                const at = yield* DateTime.now;
                const held = yield* cell.get;
                const gated = kind === 'ready' && held !== 'running';
                const registered = Array.filter(Chunk.toReadonlyArray(yield* Ref.get(probes)), (probe) => probe.kind === kind);
                const rows = gated
                    ? [new _Graded({ label: 'life', grade: 'fail', elapsed: Duration.zero, detail: Option.some(held) })]
                    : yield* Effect.forEach(registered, proven, { concurrency: 'inherit' });
                const overall = pipe(
                    Array.map(rows, (row) => row.grade),
                    (graded) => (Array.isNonEmptyReadonlyArray(graded) ? Array.max(graded, _byGrade) : 'pass'),
                );
                return new _Report({ kind, overall, rows, at });
            });

        const memo = yield* Effect.all(
            Record.map(_KINDS, (_, kind) =>
                kind === 'started' ? Effect.cached(swept(kind)) : Effect.cachedWithTTL(swept(kind), setting.life.report),
            ),
            { concurrency: 'inherit' },
        );

        const report = (kind: Life.Kind): Effect.Effect<Life.Report> =>
            kind === 'ready'
                ? Effect.flatMap(cell.get, (held) =>
                      held === 'running'
                          ? memo.ready
                          : Effect.map(
                                DateTime.now,
                                (at) =>
                                    new _Report({
                                        kind,
                                        overall: 'fail',
                                        rows: [new _Graded({ label: 'life', grade: 'fail', elapsed: Duration.zero, detail: Option.some(held) })],
                                        at,
                                    }),
                            ),
                  )
                : memo[kind];

        return {
            phase,
            online: advance('online'),
            register: (step: Life.Step): Effect.Effect<void, LifeFault> =>
                gate.withPermits(1)(
                    Effect.flatMap(cell.get, (held) =>
                        held === 'draining' || held === 'halted'
                            ? Effect.fail(new LifeFault({ operation: 'register', phase: held }))
                            : Ref.update(steps, Chunk.append(step)),
                    ),
                ),
            probe: (row: Life.Probe): Effect.Effect<void, LifeFault> =>
                gate.withPermits(1)(
                    Effect.flatMap(cell.get, (held) =>
                        held === 'draining' || held === 'halted'
                            ? Effect.fail(new LifeFault({ operation: 'probe', phase: held }))
                            : Ref.update(probes, Chunk.append(row)),
                    ),
                ),
            report,
            route: (kind: Life.Kind): string => _KINDS[kind].route,
            parked: Effect.zipRight(advance('online'), Effect.never).pipe(Effect.onInterrupt(() => drained)),
            settled: Deferred.await(settled),
        };
    }),
    dependencies: [Setting.Default],
    accessors: true,
}) {
    static readonly Graded = _Graded;
    static readonly Report = _Report;
    static readonly Row = _Row;
    static readonly Receipt = _Receipt;
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Life, LifeFault };
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
