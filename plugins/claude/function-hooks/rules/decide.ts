import { Array, Combiner, Effect, Layer, Match, Option, Predicate, Record } from 'effect';
import { RateLimiter } from 'effect/persistence';
import { Host, type HostError } from '#/host.ts';
import { Decision, decisionOf, type Request } from '#/rules/decision.ts';
import { FILES } from '#/rules/files.ts';
import { GHIDRA } from '#/rules/ghidra.ts';
import { GIT } from '#/rules/git.ts';
import { all, type Call, once, type Ruleset, settled } from '#/rules/rule.ts';
import { TASKS } from '#/rules/tasks.ts';
import { WAITS } from '#/rules/waits.ts';
import { analyzed, Parser } from '#/shell/invocation.ts';
import { PROGRAMS } from '#/shell/programs.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RULESETS: readonly Ruleset[] = [FILES, WAITS, GIT, GHIDRA, TASKS, { programs: PROGRAMS }];
const EVERY = RULESETS.flatMap(({ every = [] }) => every);
const CALLS = RULESETS.flatMap(({ calls = [] }) => calls);
const BY_NAME = new Map(
    Record.toEntries(Record.makeReducerUnion<string, Ruleset['programs'][string]>(Combiner.make((existing, added) => ({ ...existing, ...added, rules: [...(existing.rules ?? []), ...(added.rules ?? [])] }))).combineAll(RULESETS.map(({ programs }) => programs))).flatMap(([name, row]) =>
        [name, ...(row.aliases ?? [])].map((alias) => [alias, row] as const),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const due = (reader: Pick<typeof Request.Type, 'session' | 'agent'>, notes: ReadonlyMap<string, string>) =>
    Effect.map(
        Effect.filter([...notes], ([subject]) => once(reader, subject)),
        Array.map(([, note]) => note),
    );

const judged = Effect.fnUntraced(function* (request: typeof Request.Type) {
    const host = yield* Host;
    const analysis = yield* analyzed(request.command, { cwd: host.resolve(request.cwd, Option.none()), origin: 'call' });
    const call: Call = { request, analysis, host, programs: BY_NAME };
    const verdicts = yield* Effect.all([...analysis.invocations.map((invocation) => all(...(invocation.informational ? [] : [...(BY_NAME.get(invocation.program)?.rules ?? []), ...EVERY]))(invocation, call)), ...CALLS.map((rule) => settled(rule(call)))], { concurrency: 'unbounded' });
    return yield* decisionOf(request.command, verdicts.flat(), (notes) => due(request, notes));
});

const converged = Effect.fnUntraced(function* (request: typeof Request.Type, current: typeof Decision.cases.Rewrite.Type | typeof Decision.cases.Monitor.Type, seen: ReadonlySet<string>): Effect.fn.Return<typeof Decision.Type, HostError, Host | Parser | RateLimiter.RateLimiter> {
    const kind = Match.valueTags(current, { Monitor: () => 'monitor' as const, Rewrite: ({ background }) => (background ? 'background' : request.kind) });
    const again = yield* judged({ ...request, command: current.command, kind });
    if (again._tag === 'Deny') {
        return again;
    }
    const notes = Array.dedupe([current.note, again.note].filter(Predicate.isNotUndefined));
    const shared = { command: again._tag === 'Allow' ? current.command : again.command, ...(notes.length === 0 ? {} : { note: notes.join('. ') }) };
    const merged = current._tag === 'Monitor' || again._tag === 'Monitor' ? Decision.cases.Monitor.make(shared) : Decision.cases.Rewrite.make({ ...shared, background: current.background || (again._tag === 'Rewrite' && again.background) });
    const key = `${merged._tag} ${merged.command}`;
    if (merged.command === current.command && merged._tag === current._tag) {
        return merged;
    }
    if (seen.has(key) || seen.size > RULESETS.length) {
        return Decision.cases.Deny.make({ reason: `Rules undo each other's rewrite, ${current.command} then ${merged.command}, fix the rule at its cause` });
    }
    return yield* converged(request, merged, seen.union(new Set([key])));
});

const decide = Effect.fn('decide')(
    function* (request: typeof Request.Type) {
        const first = yield* judged(request);
        const decision = first._tag === 'Rewrite' || first._tag === 'Monitor' ? yield* converged(request, first, new Set([`Rewrite ${request.command}`, `${first._tag} ${first.command}`])) : first;
        const outcome = Decision.match(decision, {
            Allow: ({ note }) => Record.getSomes({ note: Option.fromNullishOr(note) }),
            Deny: ({ reason }) => ({ reason }),
            Rewrite: ({ command, background, note }) => ({ rewritten: command, background, ...Record.getSomes({ note: Option.fromNullishOr(note) }) }),
            Monitor: ({ command, note }) => ({ monitored: command, ...Record.getSomes({ note: Option.fromNullishOr(note) }) }),
        });
        yield* Effect.annotateCurrentSpan({ decision: decision._tag, kind: request.kind, command: request.command, ...outcome, tool_use_id: request.call, session: request.session, ...Record.getSomes({ agent: Option.fromNullishOr(request.agent) }) });
        yield* Effect.logInfo(decision._tag).pipe(Effect.annotateLogs({ kind: request.kind, command: request.command, ...outcome, session: request.session, agent: request.agent ?? 'main', call: request.call }));
        return decision;
    },
    (effect, request) => Effect.provideServiceEffect(effect, Host, Host.make(request.cwd)),
);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Engine = Parser.layer(BY_NAME).pipe(Layer.provideMerge(Host.layer), Layer.provideMerge(RateLimiter.layer.pipe(Layer.provide(RateLimiter.layerStoreMemory))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { decide, Engine };
