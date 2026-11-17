import { Array, Combiner, Effect, Layer, Option, Predicate, Record } from 'effect';
import { RateLimiter } from 'effect/persistence';
import { Host, type HostError } from '#/host';
import { APPS } from '#/rules/apps';
import { Decision, decisionOf, type Request } from '#/rules/decision';
import { FILES } from '#/rules/files';
import { GHIDRA } from '#/rules/ghidra';
import { GIT } from '#/rules/git';
import { all, type Call, type Ruleset, settled } from '#/rules/rule';
import { TASKS } from '#/rules/tasks';
import { WAITS } from '#/rules/waits';
import { analyzed, Parser } from '#/shell/invocation';
import { PROGRAMS } from '#/shell/programs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RULESETS: readonly Ruleset[] = [APPS, FILES, WAITS, GIT, GHIDRA, TASKS, { programs: PROGRAMS }];
const EVERY = RULESETS.flatMap(({ every = [] }) => every);
const CALLS = RULESETS.flatMap(({ calls = [] }) => calls);
const BY_NAME = new Map(
    Record.toEntries(Record.makeReducerUnion<string, Ruleset['programs'][string]>(Combiner.make((existing, added) => ({ ...existing, ...added, rules: [...(existing.rules ?? []), ...(added.rules ?? [])] }))).combineAll(RULESETS.map(({ programs }) => programs))).flatMap(([name, row]) =>
        [name, ...(row.aliases ?? [])].map((alias) => [alias, row] as const),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const due = Effect.fnUntraced(function* ({ session, agent }: Pick<typeof Request.Type, 'session' | 'agent'>, notes: ReadonlyMap<string, string>) {
    const [host, limiter] = yield* Effect.all([Host, RateLimiter.RateLimiter]);
    const reader = `${session ?? host.parent} ${agent ?? 'main'}`;
    const fresh = yield* Effect.filter([...notes], ([subject]) => Effect.isSuccess(limiter.consume({ key: `${reader} ${subject}`, limit: 1, window: '10 minutes', algorithm: 'fixed-window', onExceeded: 'fail' })));
    return fresh.map(([, note]) => note);
});

const judged = Effect.fnUntraced(function* (request: typeof Request.Type, noted: (notes: ReadonlyMap<string, string>) => Effect.Effect<readonly string[], never, Host | RateLimiter.RateLimiter>) {
    const host = yield* Host;
    const analysis = yield* analyzed(request.command, { cwd: host.resolve(request.cwd, Option.none()), origin: 'call' }, request.shell);
    const call: Call = { request, analysis, host, programs: BY_NAME };
    const verdicts = yield* Effect.all([...analysis.invocations.map((invocation) => all(...(invocation.informational ? [] : [...(BY_NAME.get(invocation.program)?.rules ?? []), ...EVERY]))(invocation, call)), ...CALLS.map((rule) => settled(rule(call)))], { concurrency: 'unbounded' });
    return yield* decisionOf(request.command, verdicts.flat(), noted);
});

const converged = Effect.fnUntraced(function* (request: typeof Request.Type, rewrite: typeof Decision.cases.Rewrite.Type, seen: ReadonlySet<string>): Effect.fn.Return<typeof Decision.Type, HostError, Host | Parser | RateLimiter.RateLimiter> {
    const again = yield* judged({ ...request, command: rewrite.command, kind: rewrite.background ? 'background' : request.kind }, (notes) => due(request, notes));
    if (again._tag !== 'Rewrite') {
        return Decision.match(again, { Allow: () => rewrite, Deny: (denied) => denied, Rewrite: () => rewrite });
    }
    const notes = Array.dedupe([rewrite.note, again.note].filter(Predicate.isNotUndefined));
    const tasks = Array.dedupe([...(rewrite.tasks ?? []), ...(again.tasks ?? [])]);
    const merged = Decision.cases.Rewrite.make({ command: again.command, background: rewrite.background || again.background, ...(notes.length === 0 ? {} : { note: notes.join('. ') }), ...(tasks.length === 0 ? {} : { tasks }) });
    if (again.command === rewrite.command) {
        return merged;
    }
    if (seen.has(again.command) || seen.size > RULESETS.length) {
        return Decision.cases.Deny.make({ reason: `Rules undo each other's rewrite, ${rewrite.command} then ${again.command}, fix the rule at its cause` });
    }
    return yield* converged(request, merged, seen.union(new Set([again.command])));
});

const decide = Effect.fn('decide')(
    function* (request: typeof Request.Type) {
        const first = yield* judged(request, (notes) => due(request, notes));
        const decision = first._tag === 'Rewrite' ? yield* converged(request, first, new Set([request.command, first.command])) : first;
        const outcome = Decision.match(decision, { Allow: () => ({}), Deny: ({ reason }) => ({ reason }), Rewrite: ({ command }) => ({ rewritten: command }) });
        yield* Effect.annotateCurrentSpan({ decision: decision._tag, kind: request.kind, command: request.command, ...outcome, ...Record.getSomes({ tool_use_id: Option.fromNullishOr(request.call), session: Option.fromNullishOr(request.session), agent: Option.fromNullishOr(request.agent) }) });
        yield* Effect.logInfo(decision._tag).pipe(Effect.annotateLogs({ kind: request.kind, command: request.command, ...outcome, session: request.session ?? 'unknown', agent: request.agent ?? 'main', call: request.call ?? 'unknown' }));
        return decision;
    },
    (effect, request) => Effect.provideServiceEffect(effect, Host, Host.make(request.cwd)),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Engine = Parser.layer(BY_NAME).pipe(Layer.provideMerge(Host.layer), Layer.provideMerge(RateLimiter.layer.pipe(Layer.provide(RateLimiter.layerStoreMemory))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { decide, due, Engine };
