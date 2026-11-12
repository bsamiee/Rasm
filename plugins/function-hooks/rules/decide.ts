import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { Clock, Duration, Effect, Layer, Option, Record, Schema } from 'effect';
import { KeyValueStore } from 'effect/persistence';
import { Host } from '#/host';
import manifest from '#/manifest' with { type: 'json' };
import { APPS } from '#/rules/apps';
import { decisionOf, type Request } from '#/rules/decision';
import { FILES } from '#/rules/files';
import { GHIDRA } from '#/rules/ghidra';
import { GIT } from '#/rules/git';
import { all, type Call, type Ruleset } from '#/rules/rule';
import { TASKS } from '#/rules/tasks';
import { WAITS } from '#/rules/waits';
import { analyzed, Parser } from '#/shell/invocation';
import { PROGRAMS } from '#/shell/programs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RULESETS: readonly Ruleset[] = [APPS, FILES, WAITS, GIT, GHIDRA, TASKS, { programs: PROGRAMS }];
const EVERY = RULESETS.flatMap(({ every = [] }) => every);
const CALLS = RULESETS.flatMap(({ calls = [] }) => calls);
const BY_NAME = new Map(
    Record.toEntries(RULESETS.reduce<Ruleset['programs']>((rows, { programs }) => Record.union(rows, programs, (existing, added) => ({ ...existing, ...added, rules: [...(existing.rules ?? []), ...(added.rules ?? [])] })), {})).flatMap(([name, row]) =>
        [name, ...(row.aliases ?? [])].map((alias) => [alias, row] as const),
    ),
);

// --- [MODELS] --------------------------------------------------------------------------

const Noted = Schema.Record(Schema.String, Schema.Finite);

// --- [OPERATIONS] ----------------------------------------------------------------------

const due = Effect.fnUntraced(
    function* ({ agent }: Pick<typeof Request.Type, 'agent'>, notes: ReadonlyMap<string, string>) {
        const [host, store, now] = yield* Effect.all([Host, KeyValueStore.KeyValueStore, Clock.currentTimeMillis]);
        const noted = KeyValueStore.toSchemaStore(store, Noted);
        const reader = agent ?? host.parent;
        const before = Option.getOrElse(yield* noted.get(reader), Record.empty<string, number>);
        const fresh = [...notes].filter(([subject]) => now - (before[subject] ?? Number.NEGATIVE_INFINITY) >= Duration.toMillis('10 minutes'));
        yield* noted.set(reader, { ...before, ...Record.fromIterableWith(fresh, ([subject]) => [subject, now]) });
        return fresh.map(([, note]) => note);
    },
    (effect, _request, notes) => Effect.orElseSucceed(effect, () => [...notes.values()]),
);

const decide = Effect.fnUntraced(function* (request: typeof Request.Type) {
    const host = yield* Host;
    const analysis = yield* analyzed(request.command, { cwd: host.resolve(request.cwd, Option.none()), origin: 'call' });
    const call: Call = { request, analysis, host, programs: BY_NAME };
    const verdicts = yield* Effect.forEach(analysis.invocations, (invocation) => all(...(invocation.informational ? [] : [...(BY_NAME.get(invocation.program)?.rules ?? []), ...EVERY]))(invocation, call), { concurrency: 'unbounded' });
    return yield* decisionOf(request.command, [...verdicts.flat(), ...CALLS.flatMap((rule) => rule(call))], (notes) => due(request, notes));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const Engine = Parser.layer(BY_NAME).pipe(Layer.provideMerge(Host.layer), Layer.provideMerge(KeyValueStore.layerFileSystem(join(tmpdir(), manifest.name))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { decide, due, Engine };
