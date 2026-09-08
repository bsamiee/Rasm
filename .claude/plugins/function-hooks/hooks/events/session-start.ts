// Session facts written once, the prune, and under the dispatch option one spawn of the orchestrator over the open findings or a due part,
// settled by its report after the start returned

// --- [IMPORTS] -------------------------------------------------------------------------

import type { On } from 'claude-code';
import type { Options } from '../host/options.ts';
import {
    type Cleaned,
    decode,
    decodeJson,
    type Entry,
    findings,
    isCleaned,
    isDispatch,
    isStringRecord,
    key,
    keys,
    type Namespace,
} from '../host/store.ts';
import { batch, batchPrompt, DUE_MS, expiredFindings, FINDING_VIEWS, landed, ORCHESTRATOR, summarize } from '../policies/findings.ts';
import { expiredScans } from '../policies/scan.ts';
import { first } from '../text/lines.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

// Session-scoped keys of another session, deleted at start, the store then holds one session's facts beside the durable rows
const _SESSION_SCOPED: readonly Namespace[] = ['session', 'injected', 'loaded', 'snapshot', 'dns', 'prompt', 'roslyn'];

// --- [OPERATIONS] ----------------------------------------------------------------------

// The keys the start deletes: other sessions' facts, the scan hits past the window, and the closed findings past it
const _expired = (all: readonly string[], session: string, entries: readonly Entry[], now: number): readonly string[] => [
    ..._SESSION_SCOPED.flatMap((namespace) =>
        keys(namespace)(all).filter((name) => name !== key(namespace, session) && !name.startsWith(`${key(namespace, session)}/`)),
    ),
    ...expiredScans(all, now),
    ...expiredFindings(entries, now),
];

// --- [REGISTRATION] --------------------------------------------------------------------

const _facts = (on: On): void => {
    on('session.start', async ($, e, next) => {
        // mise.toml owns the environment, mise env --json in the session's working directory answers it, and every child runs over it
        const answered = await $.process.run(['mise', 'env', '--json']);
        const env = decode(isStringRecord)(decodeJson(answered.stdout));
        if (env === undefined) {
            throw new Error(`mise env --json answered exit ${answered.exitCode} in ${e.cwd}: ${answered.stderr.trim()}`);
        }
        const session = await $.session.id();
        await $.store.set(key('session', session), env);
        const all = await $.store.keys();
        const entries = await Promise.all(keys('findings')(all).map(async (name): Promise<Entry> => ({ key: name, value: await $.store.get(name) })));
        const expired = _expired(all, session, entries, $.clock.now());
        await Promise.all(expired.map((name) => $.store.delete(name)));
        // The summary row is rebuilt from the kept rows, the one scan behind the band and the open-question block
        await $.store.set(key('summary'), summarize(findings(entries.filter((entry) => !expired.includes(entry.key))).map((entry) => entry.row)));
        return next(e);
    });
};

// A dispatch row younger than the due window holds every start until its settle deletes it, an older one is a crashed batch
const _holds = (value: unknown, now: number): boolean => isDispatch(value) && now - value.spawnedAt < DUE_MS;

// One spawn per start over the open rows or a due part, the start returns before the run and the report settles the rows in its own arm
const _dispatch = (on: On): void => {
    on('session.start', { interactive: true }, async ($, e, next) => {
        const all = await $.store.keys();
        const now = $.clock.now();
        const dispatches = keys('dispatch')(all);
        const [cleanedRow, ...dispatchRows] = await Promise.all([$.store.get(key('cleaned')), ...dispatches.map((name) => $.store.get(name))]);
        const stale = dispatches.filter((_name, index) => !_holds(dispatchRows[index], now));
        await Promise.all(stale.map((name) => $.store.delete(name)));
        const cleaned: Cleaned = decode(isCleaned)(cleanedRow) ?? {};
        const entries = await Promise.all(keys('findings')(all).map(async (name): Promise<Entry> => ({ key: name, value: await $.store.get(name) })));
        const named = stale.length < dispatches.length ? undefined : batch(entries, cleaned, now, crypto.randomUUID());
        if (named === undefined) {
            return next(e);
        }
        await $.store.set(key('dispatch', named.batchId), named.dispatch);
        // The spawn resolves once the orchestrator ran, and the first start is awaited before turn one
        $.agent
            .spawn({ subagentType: ORCHESTRATOR, name: named.batchId, prompt: batchPrompt(named, entries), background: true })
            .then(async (spawned) => {
                const failed = spawned.deny !== undefined || spawned.isError === true;
                const current = await Promise.all(
                    keys('findings')(await $.store.keys()).map(async (name): Promise<Entry> => ({ key: name, value: await $.store.get(name) })),
                );
                const writes = failed
                    ? [
                          {
                              key: key('notice'),
                              value: {
                                  text: `The ${ORCHESTRATOR} spawn over batch ${named.batchId} failed: ${first(spawned.deny ?? spawned.text ?? '')}`,
                              },
                          },
                      ]
                    : [
                          ...landed(named, current, first(spawned.text ?? ''), $.clock.now()),
                          ...('part' in named.dispatch
                              ? [{ key: key('cleaned'), value: { ...cleaned, [named.dispatch.part]: new Date($.clock.now()).toISOString() } }]
                              : []),
                      ];
                await Promise.all([...writes.map((write) => $.store.set(write.key, write.value)), $.store.delete(key('dispatch', named.batchId))]);
                FINDING_VIEWS.map((view) => $.ui.invalidate(view));
            })
            .catch(() => undefined);
        return next(e);
    });
};

// The plain hook writes the session row, the matched one dispatches while the option is on
const sessionStart = (on: On, options: Options): void => {
    _facts(on);
    if (options.dispatch) {
        _dispatch(on);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { sessionStart };
