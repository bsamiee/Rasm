import { Clock, Context, Duration, Effect, Layer, Option, Record, Schema, String } from 'effect';
import { KeyValueStore } from 'effect/persistence';
import { decisionOf, editsOf, notesOf, type Request } from '#/decision';
import { Host } from '#/host';
import { APPS, WATCHER } from '#/policies/apps';
import { FILES } from '#/policies/files';
import { all, type Call, type Policy } from '#/policies/fix';
import { GHIDRA } from '#/policies/ghidra';
import { GIT } from '#/policies/git';
import { TASKS } from '#/policies/tasks';
import { WAITS } from '#/policies/waits';
import { PROGRAMS } from '#/shell/programs';
import { analyzed, Shell } from '#/shell/site';

// --- [CONSTANTS] -----------------------------------------------------------------------

const NOTE_INTERVAL = Duration.toMillis('10 minutes');
const POLICIES: readonly Policy[] = [APPS, FILES, WAITS, GIT, GHIDRA, TASKS, { programs: PROGRAMS }];
const EVERY = POLICIES.flatMap(({ every = [] }) => every);
const CALLS = POLICIES.flatMap(({ calls = [] }) => calls);
const MERGED = POLICIES.reduce<Policy['programs']>((rows, { programs }) => Record.union(rows, programs, (existing, added) => ({ ...existing, ...added, fixes: [...(existing.fixes ?? []), ...(added.fixes ?? [])] })), {});
const PROGRAMS_BY_NAME = new Map(Record.toEntries(MERGED).flatMap(([name, row]) => [name, ...(row.aliases ?? [])].map((alias) => [alias, row] as const)));

// --- [MODELS] --------------------------------------------------------------------------

const Told = Schema.Record(Schema.String, Schema.Finite);

// --- [SERVICES] ------------------------------------------------------------------------

class Notes extends Context.Service<Notes>()('@rasm/function-hooks/policies/rules/Notes', {
    make: Effect.map(KeyValueStore.KeyValueStore, (store) => {
        const told = KeyValueStore.toSchemaStore(store, Told);
        return {
            due: Effect.fn('Notes.due')(
                function* (agent: string, notes: ReadonlyMap<string, string>) {
                    const now = yield* Clock.currentTimeMillis;
                    const before = Option.getOrElse(yield* told.get(agent), Record.empty<string, number>);
                    const fresh = [...notes].filter(([subject]) => now - (before[subject] ?? Number.NEGATIVE_INFINITY) >= NOTE_INTERVAL);
                    yield* told.set(agent, { ...before, ...Record.fromIterableWith(fresh, ([subject]) => [subject, now]) });
                    return fresh.map(([, note]) => note);
                },
                (effect, _agent, notes) => Effect.orElseSucceed(effect, () => [...notes.values()]),
            ),
        };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const decide = Effect.fnUntraced(function* (request: Request) {
    const [host, told] = yield* Effect.all([Host, Notes]);
    const analysis = yield* analyzed(request.command, { cwd: host.place(request.cwd, Option.none()), origin: 'call' });
    const call: Call = { request, analysis, host };
    const verdicts = yield* Effect.forEach(analysis.sites, (site) => all(...(site.informational ? [] : [...(site.row.fixes ?? []), ...EVERY]))(site, call), { concurrency: 'unbounded' });
    const judged = [...verdicts.flat(), ...CALLS.flatMap((fix) => fix(call))];
    const edits = editsOf(judged);
    const notes = notesOf(edits);
    const due = notes.size === 0 ? [] : yield* told.due(request.agent ?? host.parent, notes);
    return decisionOf(request.command, judged, edits, due, analysis.sites.some(({ program }) => program === WATCHER) || edits.some(({ text }) => Option.exists(text, String.startsWith(WATCHER))));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const live = (root: string) => Layer.merge(Shell.layer(PROGRAMS_BY_NAME), Notes.layer).pipe(Layer.provideMerge(Host.layer(root)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { decide, live, Notes };
