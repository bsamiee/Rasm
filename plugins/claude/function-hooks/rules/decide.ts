import { Array, Combiner, Effect, Layer, Match, Option, Predicate, Record } from 'effect';
import { RateLimiter } from 'effect/persistence';
import { Host, type HostError } from '#/host';
import { Decision, decisionOf, type Request } from '#/rules/decision';
import { FILES } from '#/rules/files';
import { GHIDRA } from '#/rules/ghidra';
import { GIT } from '#/rules/git';
import { all, type Call, once, type Ruleset, settled } from '#/rules/rule';
import { TASKS } from '#/rules/tasks';
import { WAITS } from '#/rules/waits';
import { analyzed, Parser } from '#/shell/invocation';
import { PROGRAMS } from '#/shell/programs';
import { parsed } from '#/shell/script';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BACKGROUND = 'Call runs in the background and notifies on completion, never poll it';
const RULESETS: readonly Ruleset[] = [FILES, WAITS, GIT, GHIDRA, TASKS, { programs: PROGRAMS }];
const EVERY = RULESETS.flatMap(({ every = [] }) => every);
const CALLS = RULESETS.flatMap(({ calls = [] }) => calls);
const BY_NAME = new Map(
    Record.toEntries(Record.makeReducerUnion<string, Ruleset['programs'][string]>(Combiner.make((existing, added) => ({ ...existing, ...added, rules: [...(existing.rules ?? []), ...(added.rules ?? [])] }))).combineAll(RULESETS.map(({ programs }) => programs))).flatMap(([name, row]) =>
        [name, ...(row.aliases ?? [])].map((alias) => [alias, row] as const),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const due = (reader: Pick<Request, 'session' | 'agent'>, notes: ReadonlyMap<string, string>) =>
    Effect.map(
        Effect.filter([...notes], ([subject]) => once(reader, subject)),
        Array.map(([, note]) => note),
    );

const judged = Effect.fnUntraced(function* (request: Request) {
    const host = yield* Host;
    const analysis = yield* analyzed(request.command, { cwd: host.resolve(request.cwd, Option.none()), origin: 'call' });
    const call: Call = { request, analysis, host, programs: BY_NAME };
    const verdicts = yield* Effect.all([...analysis.invocations.map((invocation) => all(...(invocation.informational ? [] : [...(BY_NAME.get(invocation.program)?.rules ?? []), ...EVERY]))(invocation, call)), ...CALLS.map((rule) => settled(rule(call)))], { concurrency: 'unbounded' });
    return decisionOf(request.command, verdicts.flat());
});

const converged = Effect.fnUntraced(function* (
    request: Request,
    current: Extract<ReturnType<typeof decisionOf>['decision'], { readonly _tag: 'Rewrite' | 'Monitor' }>,
    subjects: ReturnType<typeof decisionOf>['subjects'],
    seen: ReadonlySet<string>,
): Effect.fn.Return<ReturnType<typeof decisionOf>, HostError, Host | Parser | RateLimiter.RateLimiter> {
    const kind = Match.valueTags(current, { Monitor: () => 'monitor' as const, Rewrite: ({ background }) => (background ? 'background' : request.kind) });
    const again = yield* judged({ ...request, command: current.command, kind });
    if (again.decision._tag === 'Deny') {
        return again;
    }
    const notes = Array.dedupe([current.note, again.decision.note].filter(Predicate.isNotUndefined));
    const shared = { command: again.decision._tag === 'Allow' ? current.command : again.decision.command, ...(notes.length === 0 ? {} : { note: notes.join('. ') }) };
    const merged = current._tag === 'Monitor' || again.decision._tag === 'Monitor' ? Decision.cases.Monitor.make(shared) : Decision.cases.Rewrite.make({ ...shared, background: current.background || (again.decision._tag === 'Rewrite' && again.decision.background) });
    const joined = new Map([...subjects, ...again.subjects]);
    const key = `${merged._tag} ${merged.command}`;
    if (merged.command === current.command && merged._tag === current._tag) {
        return { decision: merged, subjects: joined };
    }
    if (seen.has(key) || seen.size > RULESETS.length) {
        return { decision: Decision.cases.Deny.make({ reason: `Rules undo each other's rewrite, ${current.command} then ${merged.command}, fix the rule at its cause` }), subjects: joined };
    }
    return yield* converged(request, merged, joined, seen.union(new Set([key])));
});

const parses = (text: string) => Effect.isSuccess(Effect.catch(parsed(text, 'bash'), () => parsed(text, 'zsh')));

const decide = Effect.fn('decide')(
    function* (request: Request) {
        const first = yield* judged(request);
        const { decision: settledOn, subjects } = first.decision._tag === 'Rewrite' || first.decision._tag === 'Monitor' ? yield* converged(request, first.decision, first.subjects, new Set([`Rewrite ${request.command}`, `${first.decision._tag} ${first.decision.command}`])) : first;
        const broken = (settledOn._tag === 'Rewrite' || settledOn._tag === 'Monitor') && (yield* parses(request.command)) && !(yield* parses(settledOn.command));
        const reached = broken && 'command' in settledOn ? Decision.cases.Deny.make({ reason: `Rules rewrote the call to ${settledOn.command}, which does not parse, fix the rule at its cause` }) : settledOn;
        const notes = reached._tag === 'Deny' ? [] : [...Option.toArray(Option.fromNullishOr(reached.note)), ...(yield* due(request, subjects)), ...(reached._tag === 'Rewrite' && reached.background ? [BACKGROUND] : [])];
        const noted = notes.length === 0 ? {} : { note: notes.join('. ') };
        const decision = Decision.match(reached, {
            Allow: () => Decision.cases.Allow.make(noted),
            Deny: ({ reason }) => Decision.cases.Deny.make({ reason: `${reason}. The call was refused whole, so none of it ran` }),
            Rewrite: ({ command, background }) => Decision.cases.Rewrite.make({ command, background, ...noted }),
            Monitor: ({ command }) => Decision.cases.Monitor.make({ command, ...noted }),
        });
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
