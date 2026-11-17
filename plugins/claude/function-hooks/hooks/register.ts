import { type AgentStatus, atom, type EngineInterface, type HookStream, type ProcessSpawnChunk, type ProcessSpawnResult, type Register, read, update } from 'claude-code';
import type { Routes } from '#/claude/routes';
import type { Row } from '#/hooks/state';
import type { WATCH_APP } from '#/shell/programs';

// --- [TYPES] ---------------------------------------------------------------------------

type Watched = Routes['watched'][1]['states'][number];
type Tracked = Watched & { readonly owner: string | undefined; readonly pending: readonly string[]; readonly launching: boolean };
interface State {
    readonly session: PromiseWithResolvers<{ readonly served: () => Promise<string>; readonly watched: (bundle: string, agent: string | undefined) => Promise<void> }>;
    readonly servers: Map<Promise<string>, HookStream<ProcessSpawnChunk, ProcessSpawnResult>>;
    readonly watches: Map<string, Map<string, Watched>>;
    readonly tracked: Map<string, Tracked>;
    readonly read: Set<string>;
    readonly unjudged: Map<string | undefined, string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WATCH_APP_CLI = 'watch-app' satisfies typeof WATCH_APP;
const ROWS = atom({ plugin: 'function-hooks', key: 'rows' } as const, [], { shape: 'rows-1' });
const TRAILING_SLASHES = /\/+$/u;
const ERROR_LINE = /Error\b/u;
const STDERR_HELD = 65_536;
const WATCHED_TOOLS: readonly (readonly [tool: RegExp, bundle: string])[] = [
    [/^mcp__plugin_rhino_rhino-mcp-platform__/u, 'com.mcneel.rhinoceros'],
    [/^mcp__plugin_blender_(?:mcp-for-)?blender__/u, 'org.blenderfoundation.blender'],
];
const TASK_OUTPUT = /\/tasks\/(?<id>[^/]+)\.output$/u;
const ENDED: ReadonlySet<AgentStatus> = new Set(['completed', 'failed', 'killed']);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVER]

const started = ($: EngineInterface, servers: State['servers'], folder: string): Promise<string> => {
    const socket = `${folder.replace(TRAILING_SLASHES, '')}/${$.plugin.name}-${crypto.randomUUID().slice(0, 8)}.sock`;
    const child = $.process.spawn({ argv: ['mise', 'exec', '--', 'node', '--watch', `${$.plugin.root}/claude/server.ts`, socket], cwd: $.plugin.root });
    const run = { ready: Promise.withResolvers<string>(), settled: false, stderr: '' };
    const failed = (fallback: string) => {
        const lines = run.stderr.split('\n').filter((line) => line.trim() !== '');
        return new Error(lines.find((line) => ERROR_LINE.test(line)) ?? lines.at(-1) ?? fallback);
    };
    const settle = (outcome: string | Error | undefined) => {
        const ready = run.settled ? Promise.withResolvers<string>() : run.ready;
        ready.promise.catch(() => undefined);
        servers.delete(run.ready.promise);
        servers.set(ready.promise, child);
        if (typeof outcome === 'string') {
            ready.resolve(outcome);
        } else if (outcome !== undefined) {
            ready.reject(outcome);
        }
        Object.assign(run, { ready, settled: outcome !== undefined, stderr: '' });
    };
    const supervised = async (): Promise<void> => {
        for await (const { stream, text } of child) {
            run.stderr = stream === 'stderr' ? `${run.stderr}${text}`.slice(-STDERR_HELD) : run.stderr;
            (stream === 'stdout' ? text.split('\n').filter((line) => line !== '') : []).forEach((line) => {
                const cause = run.stderr === '' ? undefined : failed(line);
                settle(line === socket ? socket : cause);
            });
        }
        const ended = await child.result.catch(() => undefined);
        run.ready.reject(failed(`server ended ${ended?.signal ?? ended?.code ?? 'replaced'}`));
        servers.delete(run.ready.promise);
    };
    const first = run.ready.promise;
    first.catch(() => undefined);
    servers.forEach((replaced) => {
        replaced.return({ code: null, signal: null }).catch(() => undefined);
    });
    servers.clear();
    servers.set(first, child);
    supervised().catch((error: unknown) => {
        run.ready.reject(error);
        servers.delete(run.ready.promise);
    });
    return first;
};

const posted = <R extends keyof Routes>($: EngineInterface, { session, servers }: State, route: R, body: Routes[R][0], signal?: AbortSignal): Promise<Routes[R][1]> => {
    const stopped = Promise.withResolvers<never>();
    const served = () => session.promise.then((opened) => opened.served());
    const asked = (socketPath: string) => $.http.fetch(`http://${$.plugin.name}/${route}`, { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(body), socketPath });
    const answered = async (): Promise<Routes[R][1]> => {
        signal?.throwIfAborted();
        const [running = served()] = servers.keys();
        const socket = await running;
        const first = await asked(socket).catch(() => undefined);
        signal?.throwIfAborted();
        const [current] = servers.keys();
        const { ok, status, text } = first ?? (await asked(await (current === undefined || (await current.catch(() => undefined)) === socket ? served() : current)));
        // oxlint-disable-next-line typescript/no-unsafe-assignment -- hooks modules import no package to decode with, and the server encodes this body through the Routes schemas
        const answer: Routes[R][1] = ok ? JSON.parse(text) : await Promise.reject(new Error(`${route} answered ${status}, ${text}`));
        return answer;
    };
    signal?.addEventListener('abort', () => stopped.reject(signal.reason), { once: true });
    return Promise.race([answered(), stopped.promise]);
};

// --- [NOTES]

const held = (tracked: State['tracked'], state: Tracked): void => {
    if (state.ended && state.pending.length === 0) {
        tracked.delete(state.instance);
    } else {
        tracked.set(state.instance, state);
    }
};

const cleared = (tracked: State['tracked'], seen: Map<string, Watched>, absent?: readonly string[]): void =>
    seen.forEach(({ ended, bundle }, instance) => {
        if (!ended && absent?.includes(bundle) !== false) {
            tracked.delete(instance);
            seen.delete(instance);
        }
    });

const shown = ($: EngineInterface, tracked: State['tracked']): void => {
    const standing = [...tracked.values()].flatMap(({ conditions }) => conditions);
    $.ui.status(standing.length === 0 ? undefined : standing.join('; '));
};

const noted = ($: EngineInterface, tracked: State['tracked'], own: readonly Watched[], rows: readonly Row[], agent: string | undefined): void => {
    own.forEach((watched) => {
        const before = tracked.get(watched.instance);
        const launching = watched.status === 'launching' || (before?.launching === true && watched.status === 'unresponsive');
        const state = launching && !watched.asks ? { ...watched, conditions: [], notes: [] } : watched;
        const fresh = state.notes.filter((note) => before?.notes.includes(note) !== true);
        const row = rows.find(({ instance }) => instance === state.instance);
        (state.ended ? fresh : []).forEach((note) => $.ui.log(note));
        held(tracked, { ...state, launching, owner: row === undefined ? agent : row.agent, pending: [...(before?.pending ?? []).filter((note) => state.notes.includes(note)), ...fresh] });
    });
    shown($, tracked);
};

const taken = async ($: EngineInterface, tracked: State['tracked'], agent: string | undefined): Promise<readonly Tracked[]> => {
    const orphaned = agent === undefined && [...tracked.values()].some(({ owner, pending }) => owner !== undefined && pending.length > 0);
    const agents = orphaned ? await $.agent.list() : [];
    const running = new Set(agents.filter(({ status }) => !ENDED.has(status)).map(({ id }) => id));
    const owned = [...tracked.values()].filter(({ owner, pending }) => pending.length > 0 && (owner === agent || (agent === undefined && owner !== undefined && !running.has(owner))));
    owned.forEach((state) => held(tracked, { ...state, pending: [] }));
    return owned;
};

// --- [INSTANCES]

const recorded = async ($: EngineInterface, agent: string | undefined, { created, exited }: Pick<Routes['observe'][1], 'created' | 'exited'>): Promise<void> => {
    const replaced = new Set([...exited, ...created.map(({ instance }) => instance)]);
    if (replaced.size > 0) {
        await $.store.set(await $.session.id(), await update($, ROWS, (rows) => [...rows.filter(({ instance }) => !replaced.has(instance)), ...created.map((row) => ({ ...row, ...(agent !== undefined && { agent }) }))]));
    }
};

const watchFailed =
    ($: EngineInterface) =>
    (cause: unknown): void =>
        $.ui.log(`${$.plugin.name} watch failed, ${Error.isError(cause) ? cause.message : JSON.stringify(cause)}`, { to: 'debug' });

const watching = async ($: EngineInterface, state: State, bundle: string, agent: string | undefined): Promise<void> => {
    const session = await $.session.id();
    const key = `${session} ${bundle}`;
    if (state.watches.has(key)) {
        return;
    }
    const seen = new Map<string, Watched>();
    state.watches.set(key, seen);
    const buffer = { partial: '' };
    const child = $.process.spawn({ argv: [WATCH_APP_CLI, 'watch', bundle] });
    try {
        for await (const { stream, text } of child) {
            const lines = stream === 'stdout' ? `${buffer.partial}${text}` : buffer.partial;
            const cut = lines.lastIndexOf('\n') + 1;
            const { absent, states } = cut === 0 ? { absent: [], states: [] } : await posted($, state, 'watched', lines.slice(0, cut));
            buffer.partial = lines.slice(cut);
            if ((await $.session.id()) !== session) {
                return;
            }
            const rows = await read($, ROWS);
            const own = states.filter((watched) => seen.has(watched.instance) || rows.every((row) => row.bundle !== watched.bundle) || rows.some((row) => row.instance === watched.instance));
            cleared(state.tracked, seen, absent);
            own.forEach((watched) => {
                seen.set(watched.instance, watched);
            });
            noted($, state.tracked, own, rows, agent);
            await recorded($, undefined, { created: [], exited: own.filter(({ ended }) => ended).map(({ instance }) => instance) });
            if (seen.size > 0 && [...seen.values()].every(({ ended }) => ended)) {
                return;
            }
        }
        const { code } = await child.result;
        if (code !== null && code !== 0) {
            watchFailed($)(new Error(`${WATCH_APP_CLI} watch ${bundle} exited ${code}`));
        }
    } finally {
        state.watches.delete(key);
        cleared(state.tracked, seen);
        shown($, state.tracked);
    }
};

const observed = async ($: EngineInterface, state: State, agent: string | undefined, text: string | undefined, signal: AbortSignal): Promise<void> => {
    if (text === undefined) {
        return;
    }
    const change = await posted($, state, 'observe', text, signal);
    await recorded($, agent, change);
    const { watched } = await state.session.promise;
    [...change.created, ...change.reused].forEach(({ bundle }) => {
        watched(bundle, agent).catch(watchFailed($));
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on) => {
    const state: State = { session: Promise.withResolvers(), servers: new Map(), watches: new Map(), tracked: new Map(), read: new Set(), unjudged: new Map() };
    on('session.start', async ($, e, next) => {
        const answer = await next(e);
        const stored = await $.store.get(await $.session.id());
        // oxlint-disable-next-line typescript/no-unsafe-return -- $.store answers unknown JSON, here the rows recorded() wrote, and hooks modules import no package to decode with
        const rows = await update($, ROWS, (current): Row[] => (current.length === 0 && Array.isArray(stored) ? stored : current));
        const folder = (await $.env.get('TMPDIR')) ?? '/tmp';
        state.session.resolve({ served: () => started($, state.servers, folder), watched: (bundle, agent) => watching($, state, bundle, agent) });
        if (state.servers.size === 0) {
            started($, state.servers, folder).catch(() => undefined);
        }
        rows.forEach(({ bundle, agent }) => {
            watching($, state, bundle, agent).catch(watchFailed($));
        });
        shown($, state.tracked);
        return answer;
    });
    on('prompt.submit', async ($, e, next) => {
        const owned = e.command === undefined && e.turnId === undefined ? await taken($, state.tracked, undefined) : [];
        const answer = await next(owned.length === 0 ? e : { ...e, context: [...(e.context ?? []), ...owned.flatMap(({ pending }) => pending)] });
        if (answer.drop !== undefined) {
            owned.filter((dropped) => dropped.ended || state.tracked.has(dropped.instance)).forEach((dropped) => held(state.tracked, { ...(state.tracked.get(dropped.instance) ?? dropped), pending: [...dropped.pending, ...(state.tracked.get(dropped.instance)?.pending ?? [])] }));
        }
        return answer;
    });
    on('tool.call', async ($, e, next) => {
        const answer = await next(e);
        if (answer.deny !== undefined) {
            return answer;
        }
        const owned = await taken($, state.tracked, e.agentId);
        const notes = owned.flatMap(({ pending }) => pending);
        return notes.length === 0 ? answer : { ...answer, context: [...(answer.context ?? []), ...notes] };
    });
    on('tool.call', { tool: WATCHED_TOOLS.map(([tool]) => tool) }, ($, e, next) => {
        WATCHED_TOOLS.filter(([tool]) => tool.test(e.tool)).forEach(([, bundle]) => {
            state.session.promise.then(({ watched }) => watched(bundle, e.agentId)).catch(watchFailed($));
        });
        return next(e);
    });
    on('tool.call', { tool: 'Read' }, async ($, e, next) => {
        const id = TASK_OUTPUT.exec(e.file_path)?.groups?.['id'];
        if (id === undefined) {
            return next(e);
        }
        const [agents, { stdout }] = await Promise.all([$.agent.list(), $.process.run(['lsof', '-t', e.file_path])]);
        const agent = agents.find((listed) => listed.id === id);
        const reader = `${e.agentId ?? 'main'} ${id}`;
        const again = state.read.has(reader);
        state.read.add(reader);
        const polled = agent === undefined ? again && stdout.trim() !== '' : !ENDED.has(agent.status);
        return polled ? { deny: `Task ${id} notifies on completion, never poll its output` } : next(e);
    });
    on('tool.call', { tool: 'AskUserQuestion' }, async ($, e, next) => {
        const since = await $.clock.now();
        const waited = async (first: readonly string[]): Promise<readonly string[]> => {
            const slice = await posted($, state, 'paused', { since, first }, next.signal);
            return slice.holds.length === 0 ? slice.notes : waited(first.length === 0 ? slice.holds : first);
        };
        const notes = await waited([]);
        const answer = await next(e);
        return notes.length === 0 || answer.deny !== undefined ? answer : { ...answer, context: [...(answer.context ?? []), ...notes] };
    });
    on('classic.WorktreeCreate', () => ({ block: 'A worktree makes a second checkout, work in place' }));
    on('tool.call', { tool: 'mcp__atelier__applications' }, async ($, e, next) => {
        const answer = await next(e);
        await observed($, state, e.agentId, answer.text, next.signal);
        return answer;
    });
    on('tool.call', { tool: ['Bash', 'Monitor'] }, async ($, e, next) => {
        if (e.command === undefined) {
            return next(e);
        }
        const bash = e.tool === 'Bash' ? e : undefined;
        const decision = await posted(
            $,
            state,
            'decide',
            {
                command: e.command,
                cwd: await $.session.cwd(),
                kind: bash?.run_in_background === true ? 'background' : ({ Bash: 'foreground', Monitor: 'monitor' } as const)[e.tool],
                ...(e.agentId !== undefined && { agent: e.agentId }),
                session: await $.session.id(),
                call: e.tool_use_id,
            },
            next.signal,
        );
        if (decision._tag !== 'Allow') {
            $.ui.log(`${$.plugin.name} ${decision._tag} ${e.tool_use_id}: ${decision._tag === 'Deny' ? decision.reason : decision.command}`, { to: 'debug' });
        }
        const settled = async <A extends { readonly text?: string | undefined; readonly deny?: unknown; readonly context?: readonly string[] | undefined }>(answer: A, observes: boolean, note?: string) => {
            if (observes && bash !== undefined) {
                await observed($, state, e.agentId, answer.text, next.signal);
            }
            return note !== undefined && answer.deny === undefined ? { ...answer, context: [...(answer.context ?? []), note] } : answer;
        };
        switch (decision._tag) {
            case 'Deny': {
                return { deny: decision.reason };
            }
            case 'Allow': {
                return settled(await next(e), decision.observes);
            }
            case 'Rewrite': {
                const { timeout: _timeout, ...untimed } = bash ?? { ...e, timeout: undefined };
                return settled(await next(bash === undefined ? { ...e, command: decision.command } : { ...(decision.background ? { ...untimed, run_in_background: true } : bash), command: decision.command }), decision.observes, decision.note);
            }
            default: {
                return decision satisfies never;
            }
        }
    }).catch(async ($, e, next) => {
        if (next.called) {
            return next(e);
        }
        const line = `${$.plugin.name} did not judge this command: ${(next.error.message ?? next.error.kind).split('\n', 1).join('')}`;
        const answer = await next(e);
        $.ui.log(line, { to: 'debug' });
        if (answer.deny !== undefined || state.unjudged.get(e.agentId) === line) {
            return answer;
        }
        state.unjudged.set(e.agentId, line);
        return { ...answer, context: [...(answer.context ?? []), line] };
    });
    on('turn.complete', async ($, e, next) => {
        const answer = await next(e);
        const [agents, rows] = await Promise.all([$.agent.list(), read($, ROWS)]);
        const ended = new Set(agents.filter(({ status }) => ENDED.has(status)).map(({ id }) => id));
        const owned = rows.filter(({ agent }) => agent !== undefined && ended.has(agent));
        if (owned.length > 0) {
            await recorded($, undefined, { created: [], exited: await posted($, state, 'quit', owned, next.signal) });
        }
        return answer;
    });
    on('session.end', async ($, e, next) => {
        const rows = await read($, ROWS);
        state.tracked.clear();
        state.read.clear();
        state.watches.forEach((seen) => seen.clear());
        state.unjudged.clear();
        shown($, state.tracked);
        if (rows.length > 0) {
            await $.process.run(['sh', '-c', `${WATCH_APP_CLI} quit "$@" </dev/null >/dev/null 2>&1 &`, 'sh', ...rows.map(({ instance }) => instance)], { stdin: '' });
        }
        await update($, ROWS, () => []);
        await $.store.delete(e.sessionId);
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
