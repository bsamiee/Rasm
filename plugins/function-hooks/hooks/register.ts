import type { AgentStatus, EngineInterface, Register } from 'claude-code';
import type { Instance, Routes } from '../decision.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Sockets = Set<Promise<string>>;
type Row = Instance & { readonly agent?: string };

// --- [CONSTANTS] -----------------------------------------------------------------------

const ROW_PREFIX = 'instances:';
const LAST_SEGMENT = /\/[^/]*$/u;
const ERROR_LINE = /Error\b/u;
const SPACES = /\s+/u;
const REASON_WORDS = 9;
const KINDS = { Bash: 'foreground', Monitor: 'monitor' } as const;
const ENDED: ReadonlySet<AgentStatus> = new Set(['completed', 'failed', 'killed']);
const APP_SERVERS = [
    [/^mcp__rhino-mcp-platform__/u, 'com.mcneel.rhinoceros.*'],
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
    const [held] = sockets;
    const running = held ?? started($, sockets);
    const answered = await asked($, await running, route, body).catch(() => undefined);
    const [current] = sockets;
    const { ok, status, text } = answered ?? (await asked($, await (current === undefined || current === running ? started($, sockets) : current), route, body));
    return ok ? JSON.parse(text) : Promise.reject(new Error(`${route} answered ${status}, ${text}`));
};

// --- [INSTANCES]

const rowKeysOf = async ($: EngineInterface, session: string): Promise<readonly string[]> => (await $.store.keys()).filter((key) => key.startsWith(`${ROW_PREFIX}${session}:`));

const rowsOf = async ($: EngineInterface, session: string): Promise<readonly Row[]> => (await Promise.all((await rowKeysOf($, session)).map((key) => $.store.get(key)))).flatMap((stored) => (Array.isArray(stored) ? stored : []));

const recorded = async ($: EngineInterface, agent: string | undefined, { created, exited }: Pick<Routes['observe'][1], 'created' | 'exited'>): Promise<void> => {
    const session = await $.session.id();
    await Promise.all([...created.map((row) => $.store.set(`${ROW_PREFIX}${session}:${row.pid}`, [{ ...row, ...(agent !== undefined && { agent }) }])), ...exited.map((pid) => $.store.delete(`${ROW_PREFIX}${session}:${pid}`))]);
};

const watchFailed = ($: EngineInterface, cause: unknown): void => $.ui.log(`function-hooks watch failed, ${cause instanceof Error ? cause.message : JSON.stringify(cause)}`, { to: 'debug' });

const delivered = async ($: EngineInterface, note: string, agent: string | undefined): Promise<void> => {
    const { isDelivered } = agent === undefined ? { isDelivered: false } : await $.session.send({ to: { agentId: agent }, text: note });
    if (!isDelivered) {
        await $.prompt.submit({ text: note });
    }
};

const noted = (own: Routes['watched'][1], rows: readonly Row[], seen: Map<number, Routes['watched'][1][number]>, agent: string | undefined) =>
    own.flatMap((state) => {
        const before = seen.get(state.pid)?.notes ?? [];
        const row = rows.find(({ pid }) => pid === state.pid);
        seen.set(state.pid, state);
        return state.notes.filter((note) => !before.includes(note)).map((note) => ({ note, owner: row === undefined ? agent : row.agent }));
    });

const watching = async ($: EngineInterface, sockets: Sockets, watches: Set<string>, names: readonly string[], agent: string | undefined): Promise<void> => {
    const session = await $.session.id();
    const key = `${session} ${names.join(' ')}`;
    if (names.length === 0 || watches.has(key)) {
        return;
    }
    watches.add(key);
    const seen = new Map<number, Routes['watched'][1][number]>();
    const held = { partial: '' };
    try {
        for await (const { stream, text } of $.process.spawn({ argv: ['watch-apps', 'watch', ...names] })) {
            const lines = `${held.partial}${stream === 'stdout' ? text : ''}`;
            const cut = lines.lastIndexOf('\n') + 1;
            const states =
                cut === 0
                    ? undefined
                    : await posted($, sockets, 'watched', lines.slice(0, cut)).catch((cause: unknown) => {
                          watchFailed($, cause);
                      });
            held.partial = states === undefined ? lines : lines.slice(cut);
            if ((await $.session.id()) !== session) {
                break;
            }
            const rows = await rowsOf($, session);
            const own = (states ?? []).filter(({ pid, bundle }) => rows.every((row) => row.bundle !== bundle) || rows.some((row) => row.pid === pid));
            noted(own, rows, seen, agent).forEach(({ note, owner }) => {
                delivered($, note, owner).catch((cause: unknown) => watchFailed($, cause));
            });
            await recorded($, undefined, { created: [], exited: own.filter(({ ended }) => ended).map(({ pid }) => pid) });
            if (own.length > 0 && [...seen.values()].every(({ ended }) => ended)) {
                break;
            }
        }
    } finally {
        watches.delete(key);
    }
};

const rewatched = async ($: EngineInterface, sockets: Sockets, watches: Set<string>): Promise<void> => {
    const rows = await rowsOf($, await $.session.id());
    rows.forEach(({ bundle, agent }) => {
        watching($, sockets, watches, [bundle], agent).catch((cause: unknown) => watchFailed($, cause));
    });
};

const driven = async ($: EngineInterface, sockets: Sockets, watches: Set<string>, tool: string, agent: string | undefined): Promise<void> => {
    const identifier = APP_SERVERS.find(([server]) => server.test(tool))?.[1];
    const key = `${await $.session.id()} ${identifier}`;
    if (identifier === undefined || watches.has(key)) {
        return;
    }
    watches.add(key);
    try {
        const { exitCode, stdout } = await $.process.run(['mdfind', `kMDItemCFBundleIdentifier == "${identifier}"`]);
        await watching($, sockets, watches, exitCode === 0 ? stdout.split('\n').filter(Boolean) : [], agent);
    } finally {
        watches.delete(key);
    }
};

const observed = async ($: EngineInterface, sockets: Sockets, watches: Set<string>, agent: string | undefined, text: string | undefined): Promise<void> => {
    const change = await posted($, sockets, 'observe', text ?? '');
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
    const script = (await rowsOf($, session)).map(({ pid, bundle }) => `(lsappinfo info -only bundleID ${pid} | grep -qF '"${bundle}"' && watch-apps quit ${pid}) </dev/null >/dev/null 2>&1 &`);
    if (script.length > 0) {
        await $.process.run(['sh', '-c', script.join('\n')], { stdin: '' });
    }
    await Promise.all((await rowKeysOf($, session)).map((key) => $.store.delete(key)));
};

const failClosed = <E, A>(_$: unknown, e: E, next: { (e: E): Promise<A>; readonly called: boolean; readonly error: { readonly message?: string; readonly kind: string } }) =>
    next.called ? next(e) : { deny: `function-hooks failed, ${next.error.message ?? next.error.kind}`.split(SPACES).slice(0, REASON_WORDS).join(' ') };

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on) => {
    const sockets: Sockets = new Set();
    const watches = new Set<string>();
    on('session.start', async ($, e, next) => {
        if (sockets.size === 0) {
            started($, sockets).catch(() => undefined);
        }
        await rewatched($, sockets, watches);
        return next(e);
    });
    on('tool.call', { tool: new RegExp(APP_SERVERS.map(([server]) => server.source).join('|'), 'u') }, ($, e, next) => {
        driven($, sockets, watches, e.tool, e.agentId).catch((cause: unknown) => watchFailed($, cause));
        return next(e);
    }).catch(failClosed);
    on('classic.WorktreeCreate', () => ({ block: 'A worktree makes a second checkout, work in place' }));
    on('tool.call', { tool: 'mcp__computer-use__request_access' }, (_$, e, next) => (e.agentId === undefined ? next(e) : { deny: 'request_access waits on a person, report back and stop' })).catch(failClosed);
    on('tool.call', { tool: ['mcp__atelier__launch', 'mcp__atelier__open'] }, async ($, e, next) => {
        const answer = await next(e);
        await observed($, sockets, watches, e.agentId, answer.text);
        return answer;
    }).catch(failClosed);
    on('tool.call', { tool: ['Bash', 'Monitor'] }, async ($, e, next) => {
        if (e.command === undefined) {
            return next(e);
        }
        const decision = await posted($, sockets, 'decide', { command: e.command, cwd: await $.session.cwd(), kind: e.tool === 'Bash' && e.run_in_background === true ? 'background' : KINDS[e.tool], ...(e.tool === 'Bash' && e.timeout !== undefined && { timeout: e.timeout }) });
        if (decision._tag === 'Deny') {
            return { deny: decision.reason };
        }
        const answer = await next(decision._tag === 'Allow' ? e : { ...e, command: decision.command, ...(decision.timeout !== undefined && { timeout: decision.timeout }), ...(decision.background && { run_in_background: true }) });
        if (decision.observes && e.tool === 'Bash') {
            await observed($, sockets, watches, e.agentId, answer.text);
        }
        return decision._tag === 'Rewrite' && decision.notice !== undefined && answer.deny === undefined ? { ...answer, context: [...(answer.context ?? []), decision.notice] } : answer;
    }).catch(failClosed);
    on('turn.complete', async ($, e, next) => {
        const answer = await next(e);
        await quitEnded($, sockets);
        return answer;
    });
    on('session.end', async ($, e, next) => {
        await quitSession($, e.sessionId);
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
