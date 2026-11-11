import type { AgentStatus, EngineInterface, Register } from 'claude-code';
import type { Instance } from '#/instances';
import type { Routes } from '#/server';

// --- [TYPES] ---------------------------------------------------------------------------

type Sockets = Set<Promise<string>>;
type Row = Instance & { readonly agent?: string };
type Watched = Routes['watched'][1][number];
type Noted = Watched & { readonly owner: string | undefined; readonly untold: readonly string[] };

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROW_PREFIX = 'instances:';
const LAST_SEGMENT = /\/[^/]*$/u;
const ERROR_LINE = /Error\b/u;
const NOTE_INTERVAL = 600_000;
const KINDS = { Bash: 'foreground', Monitor: 'monitor' } as const;
const ENDED: ReadonlySet<AgentStatus> = new Set(['completed', 'failed', 'killed']);
const APP_SERVERS = [
    [/^mcp__rhino-mcp-platform__/u, 'com.mcneel.rhinoceros'],
    [/^mcp__(?:mcp-for-)?blender__/u, 'org.blenderfoundation.blender'],
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVER]

const started = ($: EngineInterface, sockets: Sockets): Promise<string> => {
    const ready = Promise.withResolvers<string>();
    const supervised = async (): Promise<void> => {
        const child = $.process.spawn({ argv: ['mise', 'exec', '--', 'node', `${$.plugin.root}/server.ts`], cwd: $.plugin.root });
        const seen = { stdout: '', stderr: '' };
        for await (const { stream, text } of child) {
            seen[stream] = `${seen[stream]}${text}`;
            if (seen.stdout.endsWith('\n')) {
                ready.resolve(seen.stdout.split('\n', 1).join(''));
            }
        }
        const { signal } = await child.result;
        if (signal !== null && seen.stdout !== '') {
            await $.process.run(['rm', '-r', seen.stdout.split('\n', 1).join('').replace(LAST_SEGMENT, '')]);
        }
        ready.reject(new Error(seen.stderr.split('\n').find((line) => ERROR_LINE.test(line)) ?? `server ended ${signal ?? seen.stderr.trim()}`));
    };
    sockets.clear();
    sockets.add(ready.promise);
    supervised()
        .finally(() => sockets.delete(ready.promise))
        .catch(ready.reject);
    return ready.promise;
};

const asked = <R extends keyof Routes>($: EngineInterface, socketPath: string, route: R, body: Routes[R][0]) => $.http.fetch(`http://function-hooks/${route}`, { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(body), socketPath });

const posted = async <R extends keyof Routes>($: EngineInterface, sockets: Sockets, route: R, body: Routes[R][0]): Promise<Routes[R][1]> => {
    const [socket] = sockets;
    const running = socket ?? started($, sockets);
    const answered = await asked($, await running, route, body).catch(() => undefined);
    const [current] = sockets;
    const { ok, status, text } = answered ?? (await asked($, await (current === undefined || current === running ? started($, sockets) : current), route, body));
    const answer: Routes[R][1] = ok ? JSON.parse(text) : await Promise.reject(new Error(`${route} answered ${status}, ${text}`));
    return answer;
};

// --- [NOTES]

const NOTED = new Map<number, Noted>();

const held = (state: Noted): void => {
    if (state.ended && state.untold.length === 0) {
        NOTED.delete(state.pid);
    } else {
        NOTED.set(state.pid, state);
    }
};

const shown = ($: EngineInterface): void => {
    const standing = [...NOTED.values()].flatMap(({ conditions }) => conditions);
    $.ui.status(standing.length === 0 ? undefined : standing.join('; '));
};

const noted = ($: EngineInterface, own: readonly Watched[], rows: readonly Row[], agent: string | undefined): void => {
    own.forEach((state) => {
        const before = NOTED.get(state.pid);
        const fresh = state.notes.filter((note) => before?.notes.includes(note) !== true);
        const row = rows.find(({ pid }) => pid === state.pid);
        (state.ended ? fresh : []).forEach((note) => $.ui.log(note));
        held({ ...state, owner: row === undefined ? agent : row.agent, untold: [...(before?.untold ?? []).filter((note) => state.notes.includes(note)), ...fresh] });
    });
    shown($);
};

const taken = async ($: EngineInterface, agent: string | undefined): Promise<readonly Noted[]> => {
    const orphaned = agent === undefined && [...NOTED.values()].some(({ owner, untold }) => owner !== undefined && untold.length > 0);
    const running = new Set(orphaned ? (await $.agent.list()).filter(({ status }) => !ENDED.has(status)).map(({ id }) => id) : []);
    const owned = [...NOTED.values()].filter(({ owner, untold }) => untold.length > 0 && (owner === agent || (agent === undefined && owner !== undefined && !running.has(owner))));
    owned.forEach((state) => held({ ...state, untold: [] }));
    return owned;
};

// --- [INSTANCES]

const rowKeysOf = async ($: EngineInterface, session: string): Promise<readonly string[]> => (await $.store.keys()).filter((key) => key.startsWith(`${ROW_PREFIX}${session}:`));

const rowsOf = async ($: EngineInterface, session: string): Promise<readonly Row[]> => (await Promise.all((await rowKeysOf($, session)).map((key) => $.store.get(key)))).flatMap((stored) => (Array.isArray(stored) ? stored : []));

const recorded = async ($: EngineInterface, agent: string | undefined, { created, exited }: Pick<Routes['observe'][1], 'created' | 'exited'>): Promise<void> => {
    const session = await $.session.id();
    await Promise.all([...created.map((row) => $.store.set(`${ROW_PREFIX}${session}:${row.pid}`, [{ ...row, ...(agent !== undefined && { agent }) }])), ...exited.map((pid) => $.store.delete(`${ROW_PREFIX}${session}:${pid}`))]);
};

const watchFailed = ($: EngineInterface, cause: unknown): void => $.ui.log(`function-hooks watch failed, ${cause instanceof Error ? cause.message : JSON.stringify(cause)}`, { to: 'debug' });

const watching = async ($: EngineInterface, sockets: Sockets, watches: Set<string>, names: readonly string[], agent: string | undefined): Promise<void> => {
    const session = await $.session.id();
    const key = `${session} ${names.join(' ')}`;
    if (names.length === 0 || watches.has(key)) {
        return;
    }
    watches.add(key);
    const seen = new Set<number>();
    const buffer = { partial: '' };
    const child = $.process.spawn({ argv: ['watch-apps', 'watch', ...names] });
    try {
        for await (const { stream, text } of child) {
            const lines = stream === 'stdout' ? `${buffer.partial}${text}` : buffer.partial;
            const cut = lines.lastIndexOf('\n') + 1;
            const states =
                cut === 0
                    ? undefined
                    : await posted($, sockets, 'watched', lines.slice(0, cut)).catch((cause: unknown) => {
                          watchFailed($, cause);
                      });
            buffer.partial = states === undefined ? lines : lines.slice(cut);
            if ((await $.session.id()) !== session) {
                return;
            }
            const rows = await rowsOf($, session);
            const own = (states ?? []).filter(({ pid, bundle }) => rows.every((row) => row.bundle !== bundle) || rows.some((row) => row.pid === pid));
            own.forEach(({ pid }) => seen.add(pid));
            noted($, own, rows, agent);
            await recorded($, undefined, { created: [], exited: own.filter(({ ended }) => ended).map(({ pid }) => pid) });
            if (own.length > 0 && [...seen].every((pid) => NOTED.get(pid)?.ended !== false)) {
                return;
            }
        }
        const { code } = await child.result;
        if (code !== null && code !== 0) {
            watchFailed($, new Error(`watch-apps watch ${names.join(' ')} exited ${code}`));
        }
    } finally {
        watches.delete(key);
        [...seen].filter((pid) => NOTED.get(pid)?.ended === false).forEach((pid) => NOTED.delete(pid));
        shown($);
    }
};

const rewatched = async ($: EngineInterface, sockets: Sockets, watches: Set<string>): Promise<void> => {
    const rows = await rowsOf($, await $.session.id());
    rows.forEach(({ bundle, agent }) => {
        watching($, sockets, watches, [bundle], agent).catch((cause: unknown) => watchFailed($, cause));
    });
};

const driven = ($: EngineInterface, sockets: Sockets, watches: Set<string>, tool: string, agent: string | undefined): Promise<void> =>
    watching(
        $,
        sockets,
        watches,
        APP_SERVERS.filter(([server]) => server.test(tool)).map(([, identifier]) => identifier),
        agent,
    );

const observed = async ($: EngineInterface, sockets: Sockets, watches: Set<string>, agent: string | undefined, text: string | undefined): Promise<void> => {
    if (text === undefined) {
        return;
    }
    const change = await posted($, sockets, 'observe', text);
    await recorded($, agent, change);
    [...change.created, ...change.reused].forEach(({ bundle }) => {
        watching($, sockets, watches, [bundle], agent).catch((cause: unknown) => watchFailed($, cause));
    });
};

const quitEnded = async ($: EngineInterface, sockets: Sockets): Promise<void> => {
    const ended = new Set((await $.agent.list()).filter(({ status }) => ENDED.has(status)).map(({ id }) => id));
    const instances = (await rowsOf($, await $.session.id())).filter(({ agent }) => agent !== undefined && ended.has(agent));
    await Promise.all([...Map.groupBy(instances, ({ agent }) => agent)].map(async ([agent, owned]) => recorded($, agent, await posted($, sockets, 'quit', owned))));
};

const quitSession = async ($: EngineInterface, session: string): Promise<void> => {
    const script = (await rowsOf($, session)).map(({ pid, bundle }) => `(watch-apps find ${pid} | grep -qF '"identifier":"${bundle}"' && watch-apps quit ${pid}) </dev/null >/dev/null 2>&1 &`);
    if (script.length > 0) {
        await $.process.run(['sh', '-c', script.join('\n')], { stdin: '' });
    }
    await Promise.all((await rowKeysOf($, session)).map((key) => $.store.delete(key)));
};

// --- [FAILURES]

const TOLD = new Map<string | undefined, number>();

const unjudged = (cause: string) => `function-hooks did not judge this command: ${cause.split('\n', 1).join('')}`;

const failOpen = async <E extends { readonly agentId?: string | undefined }, A extends { readonly deny?: string | undefined; readonly context?: readonly string[] | undefined }>(
    $: EngineInterface,
    e: E,
    next: { (e: E): Promise<A>; readonly error: { readonly message?: string; readonly kind: string }; readonly called: boolean },
): Promise<A> => {
    const line = unjudged(next.error.message ?? next.error.kind);
    const now = await $.clock.now();
    const due = now - (TOLD.get(e.agentId) ?? Number.NEGATIVE_INFINITY) >= NOTE_INTERVAL;
    $.ui.log(line, { to: 'debug' });
    const answer = await next(e);
    if (next.called || !due || answer.deny !== undefined) {
        return answer;
    }
    TOLD.set(e.agentId, now);
    return { ...answer, context: [...(answer.context ?? []), line] };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on) => {
    const sockets: Sockets = new Set();
    const watches = new Set<string>();
    on('session.start', async ($, e, next) => {
        if (sockets.size === 0) {
            started($, sockets).catch(() => undefined);
        }
        await rewatched($, sockets, watches);
        shown($);
        return next(e);
    });
    on('prompt.submit', async ($, e, next) => {
        const owned = e.command === undefined ? await taken($, undefined) : [];
        const answer = await next(owned.length === 0 ? e : { ...e, context: [...(e.context ?? []), ...owned.flatMap(({ untold }) => untold)] });
        if (answer.drop !== undefined) {
            owned.forEach((state) => held({ ...(NOTED.get(state.pid) ?? state), untold: [...state.untold, ...(NOTED.get(state.pid)?.untold ?? [])] }));
        }
        return answer;
    });
    on('tool.call', async ($, e, next) => {
        const answer = await next(e);
        if (answer.deny !== undefined) {
            return answer;
        }
        const notes = (await taken($, e.agentId)).flatMap(({ untold }) => untold);
        return notes.length === 0 ? answer : { ...answer, context: [...(answer.context ?? []), ...notes] };
    });
    on('tool.call', { tool: new RegExp(APP_SERVERS.map(([server]) => server.source).join('|'), 'u') }, ($, e, next) => {
        driven($, sockets, watches, e.tool, e.agentId).catch((cause: unknown) => watchFailed($, cause));
        return next(e);
    }).catch(failOpen);
    on('classic.WorktreeCreate', () => ({ block: 'A worktree makes a second checkout, work in place' }));
    on('tool.call', { tool: 'mcp__computer-use__request_access' }, (_$, e, next) => (e.agentId === undefined ? next(e) : { deny: 'request_access waits on a person, run list_granted_applications' })).catch(failOpen);
    on('tool.call', { tool: ['mcp__atelier__launch', 'mcp__atelier__open'] }, async ($, e, next) => {
        const answer = await next(e);
        await observed($, sockets, watches, e.agentId, answer.text);
        return answer;
    }).catch(failOpen);
    on('tool.call', { tool: ['Bash', 'Monitor'] }, async ($, e, next) => {
        if (e.command === undefined) {
            return next(e);
        }
        const decision = await posted($, sockets, 'decide', {
            command: e.command,
            cwd: await $.session.cwd(),
            kind: e.tool === 'Bash' && e.run_in_background === true ? 'background' : KINDS[e.tool],
            ...(e.tool === 'Bash' && e.timeout !== undefined && { timeout: e.timeout }),
            ...(e.agentId !== undefined && { agent: e.agentId }),
        });
        const observing = async <A extends { readonly text?: string | undefined }>(answer: A, observes: boolean) => {
            if (observes && e.tool === 'Bash') {
                await observed($, sockets, watches, e.agentId, answer.text);
            }
            return answer;
        };
        switch (decision._tag) {
            case 'Deny':
                return { deny: decision.reason };
            case 'Allow':
                return observing(await next(e), decision.observes);
            case 'Rewrite': {
                const answer = await observing(await next({ ...e, command: decision.command, ...(decision.timeout !== undefined && { timeout: decision.timeout }), ...(decision.background && { run_in_background: true }) }), decision.observes);
                return decision.notice !== undefined && answer.deny === undefined ? { ...answer, context: [...(answer.context ?? []), decision.notice] } : answer;
            }
            default:
                return decision satisfies never;
        }
    }).catch(failOpen);
    on('turn.complete', async ($, e, next) => {
        const answer = await next(e);
        await quitEnded($, sockets);
        return answer;
    });
    on('session.end', async ($, e, next) => {
        NOTED.clear();
        shown($);
        await quitSession($, e.sessionId);
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
