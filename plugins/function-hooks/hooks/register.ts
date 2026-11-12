import { type AgentStatus, atom, type EngineInterface, type HookStream, type ProcessSpawnChunk, type ProcessSpawnResult, type Register, read, update } from 'claude-code';
import type { Routes } from '#/claude/routes';
import type { Row } from '#/hooks/state';

// --- [TYPES] ---------------------------------------------------------------------------

type Watched = Routes['watched'][1][number];
type Tracked = Watched & { readonly owner: string | undefined; readonly pending: readonly string[]; readonly launching: boolean };
interface State {
    readonly session: PromiseWithResolvers<{ readonly served: () => Promise<string>; readonly watched: (bundle: string, agent: string | undefined) => Promise<void> }>;
    readonly servers: Map<Promise<string>, HookStream<ProcessSpawnChunk, ProcessSpawnResult>>;
    readonly watches: Set<string>;
    readonly tracked: Map<string, Tracked>;
    readonly unjudged: Map<string | undefined, string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WATCH_APPS = 'watch-apps' satisfies typeof import('#/shell/programs').WATCH_APPS;
const ROWS = atom({ plugin: 'function-hooks', key: 'rows' } as const, [], { shape: 'rows-1' });
const LAST_SEGMENT = /\/[^/]*$/u;
const ERROR_LINE = /Error\b/u;
const RHINO_TOOLS = /^mcp__rhino-mcp-platform__/u;
const BLENDER_TOOLS = /^mcp__(?:mcp-for-)?blender__/u;
const ENDED: ReadonlySet<AgentStatus> = new Set(['completed', 'failed', 'killed']);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVER]

const started = ($: EngineInterface, servers: State['servers']): Promise<string> => {
    const ready = Promise.withResolvers<string>();
    const child = $.process.spawn({ argv: ['mise', 'exec', '--', 'node', `${$.plugin.root}/claude/server.ts`], cwd: $.plugin.root });
    const supervised = async (): Promise<void> => {
        const printed = { stdout: '', stderr: '', socket: '' };
        for await (const { stream, text } of child) {
            printed[stream] += text;
            printed.socket = printed.stdout.slice(0, Math.max(0, printed.stdout.indexOf('\n')));
            if (printed.socket !== '') {
                ready.resolve(printed.socket);
            }
        }
        const ended = await child.result.catch(() => undefined);
        const lines = printed.stderr.split('\n').filter((line) => line.trim() !== '');
        if (ended?.signal !== null && printed.socket !== '') {
            await $.process.run(['rm', '-r', printed.socket.replace(LAST_SEGMENT, '')]);
        }
        ready.reject(new Error(lines.find((line) => ERROR_LINE.test(line)) ?? lines.at(-1) ?? `server ended ${ended?.signal ?? ended?.code ?? 'replaced'}`));
    };
    servers.forEach((replaced) => {
        replaced.return({ code: null, signal: null }).catch(() => undefined);
    });
    servers.clear();
    servers.set(ready.promise, child);
    supervised()
        .finally(() => servers.delete(ready.promise))
        .catch(ready.reject);
    return ready.promise;
};

const posted = <R extends keyof Routes>($: EngineInterface, { session, servers }: State, route: R, body: Routes[R][0], signal?: AbortSignal): Promise<Routes[R][1]> => {
    const stopped = Promise.withResolvers<never>();
    const served = () => session.promise.then((opened) => opened.served());
    const asked = (socketPath: string) => $.http.fetch(`http://${$.plugin.name}/${route}`, { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(body), socketPath });
    const answered = async (): Promise<Routes[R][1]> => {
        const [running = served()] = servers.keys();
        const socket = await running;
        const first = await asked(socket).catch(() => undefined);
        signal?.throwIfAborted();
        const [current] = servers.keys();
        const { ok, status, text } = first ?? (await asked(await (current === undefined || (await current.catch(() => undefined)) === socket ? served() : current)));
        const answer: Routes[R][1] = ok ? JSON.parse(text) : await Promise.reject(new Error(`${route} answered ${status}, ${text}`));
        return answer;
    };
    signal?.addEventListener('abort', () => stopped.reject(signal.reason), { once: true });
    return Promise.race([signal?.aborted === true ? Promise.reject(signal.reason) : answered(), stopped.promise]);
};

// --- [NOTES]

const held = (tracked: State['tracked'], state: Tracked): void => {
    if (state.ended && state.pending.length === 0) {
        tracked.delete(state.instance);
    } else {
        tracked.set(state.instance, state);
    }
};

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
    const running = new Set(orphaned ? (await $.agent.list()).filter(({ status }) => !ENDED.has(status)).map(({ id }) => id) : []);
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
        $.ui.log(`${$.plugin.name} watch failed, ${cause instanceof Error ? cause.message : JSON.stringify(cause)}`, { to: 'debug' });

const watching = async ($: EngineInterface, state: State, bundle: string, agent: string | undefined): Promise<void> => {
    const session = await $.session.id();
    const key = `${session} ${bundle}`;
    if (state.watches.has(key)) {
        return;
    }
    state.watches.add(key);
    const seen = new Set<string>();
    const buffer = { partial: '' };
    const child = $.process.spawn({ argv: [WATCH_APPS, 'watch', bundle] });
    try {
        for await (const { stream, text } of child) {
            const lines = stream === 'stdout' ? `${buffer.partial}${text}` : buffer.partial;
            const cut = lines.lastIndexOf('\n') + 1;
            const states = cut === 0 ? [] : await posted($, state, 'watched', lines.slice(0, cut));
            buffer.partial = lines.slice(cut);
            if ((await $.session.id()) !== session) {
                return;
            }
            const rows = await read($, ROWS);
            const own = states.filter((watched) => rows.every((row) => row.bundle !== watched.bundle) || rows.some((row) => row.instance === watched.instance));
            own.forEach(({ instance }) => seen.add(instance));
            noted($, state.tracked, own, rows, agent);
            await recorded($, undefined, { created: [], exited: own.filter(({ ended }) => ended).map(({ instance }) => instance) });
            if (own.length > 0 && [...seen].every((instance) => state.tracked.get(instance)?.ended !== false)) {
                return;
            }
        }
        const { code } = await child.result;
        if (code !== null && code !== 0) {
            watchFailed($)(new Error(`${WATCH_APPS} watch ${bundle} exited ${code}`));
        }
    } finally {
        state.watches.delete(key);
        [...seen].filter((instance) => state.tracked.get(instance)?.ended === false).forEach((instance) => state.tracked.delete(instance));
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
    const state: State = { session: Promise.withResolvers(), servers: new Map(), watches: new Set(), tracked: new Map(), unjudged: new Map() };
    on('session.start', async ($, e, next) => {
        const answer = await next(e);
        const stored = await $.store.get(await $.session.id());
        const rows = await update($, ROWS, (current) => (current.length === 0 && Array.isArray(stored) ? stored : current));
        state.session.resolve({ served: () => started($, state.servers), watched: (bundle, agent) => watching($, state, bundle, agent) });
        if (state.servers.size === 0) {
            started($, state.servers).catch(() => undefined);
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
            owned.forEach((dropped) => held(state.tracked, { ...(state.tracked.get(dropped.instance) ?? dropped), pending: [...dropped.pending, ...(state.tracked.get(dropped.instance)?.pending ?? [])] }));
        }
        return answer;
    });
    on('tool.call', async ($, e, next) => {
        const answer = await next(e);
        if (answer.deny !== undefined) {
            return answer;
        }
        const notes = (await taken($, state.tracked, e.agentId)).flatMap(({ pending }) => pending);
        return notes.length === 0 ? answer : { ...answer, context: [...(answer.context ?? []), ...notes] };
    });
    on('tool.call', { tool: RHINO_TOOLS }, ($, e, next) => {
        state.session.promise.then(({ watched }) => watched('com.mcneel.rhinoceros', e.agentId)).catch(watchFailed($));
        return next(e);
    });
    on('tool.call', { tool: BLENDER_TOOLS }, ($, e, next) => {
        state.session.promise.then(({ watched }) => watched('org.blenderfoundation.blender', e.agentId)).catch(watchFailed($));
        return next(e);
    });
    on('classic.WorktreeCreate', () => ({ block: 'A worktree makes a second checkout, work in place' }));
    on('tool.call', { tool: 'mcp__computer-use__request_access' }, (_$, e, next) => (e.agentId === undefined ? next(e) : { deny: 'request_access waits on a person, run list_granted_applications' }));
    on('tool.call', { tool: ['mcp__atelier__launch', 'mcp__atelier__open'] }, async ($, e, next) => {
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
                ...(bash?.timeout !== undefined && { timeout: bash.timeout }),
                ...(e.agentId !== undefined && { agent: e.agentId }),
            },
            next.signal,
        );
        const settled = async <A extends { readonly text?: string | undefined; readonly deny?: unknown; readonly context?: readonly string[] | undefined }>(answer: A, observes: boolean, note?: string) => {
            if (observes && bash !== undefined) {
                await observed($, state, e.agentId, answer.text, next.signal);
            }
            return note !== undefined && answer.deny === undefined ? { ...answer, context: [...(answer.context ?? []), note] } : answer;
        };
        switch (decision._tag) {
            case 'Deny':
                return { deny: decision.reason };
            case 'Allow':
                return settled(await next(e), decision.observes);
            case 'Rewrite':
                return settled(await next(bash === undefined ? { ...e, command: decision.command } : { ...bash, command: decision.command, ...(decision.timeout !== undefined && { timeout: decision.timeout }), ...(decision.background && { run_in_background: true }) }), decision.observes, decision.note);
            default:
                return decision satisfies never;
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
        const ended = new Set((await $.agent.list()).filter(({ status }) => ENDED.has(status)).map(({ id }) => id));
        const owned = (await read($, ROWS)).filter(({ agent }) => agent !== undefined && ended.has(agent));
        if (owned.length > 0) {
            await recorded($, undefined, { created: [], exited: await posted($, state, 'quit', owned, next.signal) });
        }
        return answer;
    });
    on('session.end', async ($, e, next) => {
        const rows = await read($, ROWS);
        state.tracked.clear();
        state.unjudged.clear();
        shown($, state.tracked);
        if (rows.length > 0) {
            await $.process.run(['sh', '-c', `${WATCH_APPS} quit "$@" </dev/null >/dev/null 2>&1 &`, 'sh', ...rows.map(({ instance }) => instance)], { stdin: '' });
        }
        await update($, ROWS, () => []);
        await $.store.delete(e.sessionId);
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
