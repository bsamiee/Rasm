import { Effect, Layer, Option, Record } from 'effect';
import { decisionOf, type Request, unjudged, Verdict } from '#/decision';
import { Host } from '#/host';
import { WATCHER } from '#/instances';
import { APPS } from '#/policies/apps';
import { FILES } from '#/policies/files';
import { all, type Call, type Policy } from '#/policies/fix';
import { GHIDRA } from '#/policies/ghidra';
import { GIT } from '#/policies/git';
import { TASKS } from '#/policies/tasks';
import { WAITS } from '#/policies/waits';
import { PROGRAMS, type Programs } from '#/shell/programs';
import { analyzed, Shell } from '#/shell/site';

// --- [CONSTANTS] -----------------------------------------------------------------------

const POLICIES: readonly Policy[] = [APPS, FILES, WAITS, GIT, GHIDRA, TASKS, { programs: PROGRAMS }];
const EVERY = POLICIES.flatMap(({ every = [] }) => every);
const CALLS = POLICIES.flatMap(({ calls = [] }) => calls);
const MERGED = POLICIES.reduce<Programs>((rows, { programs }) => Record.union(rows, programs, (held, added) => ({ ...held, ...added, fixes: [...(held.fixes ?? []), ...(added.fixes ?? [])] })), {});
const PROGRAMS_BY_NAME = new Map(Record.toEntries(MERGED).flatMap(([name, row]) => [name, ...(row.aliases ?? [])].map((alias) => [alias, row] as const)));

// --- [OPERATIONS] ----------------------------------------------------------------------

const decide = Effect.fnUntraced(function* (request: Request) {
    const host = yield* Host;
    const analysis = yield* analyzed(request.command, { cwd: host.place(request.cwd, Option.none()), origin: 'call' });
    const call: Call = { request, analysis, host };
    const verdicts = yield* Effect.forEach(analysis.sites, (site) => all(...(site.informational ? [] : [...(site.row.fixes ?? []), ...EVERY]))(site, call), { concurrency: 'unbounded' });
    const held = [...verdicts.flat(), ...CALLS.flatMap((fix) => fix(call))];
    return decisionOf(request.command, held, analysis.sites.some(({ program }) => program === WATCHER) || held.some((verdict) => Verdict.$is('Edit')(verdict) && verdict.text.startsWith(WATCHER)));
}, Effect.catchCause(unjudged));

// --- [COMPOSITION] ---------------------------------------------------------------------

const live = Shell.layer(PROGRAMS_BY_NAME).pipe(Layer.provideMerge(Host.layer));

// --- [EXPORTS] -------------------------------------------------------------------------

export { decide, live };
