// @effect-diagnostics asyncFunction:off
import type { AgentStatus, EngineInterface, HookStream, ProcessSpawnChunk, ProcessSpawnResult, Register } from 'claude-code';
import type { Routes } from '#/claude/routes';

// --- [TYPES] ---------------------------------------------------------------------------

interface State {
    readonly session: PromiseWithResolvers<{ readonly served: () => Promise<string> }>;
    readonly servers: Map<Promise<string>, HookStream<ProcessSpawnChunk, ProcessSpawnResult>>;
    readonly read: Set<string>;
    readonly unjudged: Map<string | undefined, string>;
    readonly tasks: Map<string, { readonly task: string; readonly agent: string | undefined }>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TRAILING_SLASHES = /\/+$/u;
const ERROR_LINE = /Error\b/u;
const STDERR_HELD = 65_536;
const TASK_OUTPUT = /\/tasks\/(?<id>[^/]+)\.output$/u;
const ENDED: ReadonlySet<AgentStatus> = new Set(['completed', 'failed', 'killed']);

// --- [OPERATIONS] ----------------------------------------------------------------------

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

const readied = async ($: EngineInterface, state: State, ids: readonly string[], result: unknown, agent: string | undefined): Promise<void> => {
    const task = typeof result === 'object' && result !== null && 'backgroundTaskId' in result && typeof result.backgroundTaskId === 'string' ? result.backgroundTaskId : undefined;
    const waited = async (pending: readonly string[]): Promise<void> => {
        const rows = await posted($, state, 'ready', pending);
        const open = rows.filter(({ readiness }) => readiness !== 1 && readiness !== 2).map(({ id }) => id);
        pending
            .filter((id) => !rows.some((row) => row.id === id))
            .forEach((id) => {
                state.tasks.delete(id);
            });
        await Promise.all(
            rows
                .filter(({ readiness }) => readiness === 1 || readiness === 2)
                .map(({ id, readiness, url }) => {
                    const message = { type: 'user' as const, content: [{ type: 'text' as const, text: readiness === 1 ? [id, 'is ready', ...(url === undefined ? [] : ['at', url])].join(' ') : `${id} failed its readiness check, read its task output for the cause` }] };
                    return $.session.append({ message, ...(agent !== undefined && { agentId: agent }) }).catch(() => $.session.append({ message }));
                }),
        );
        return open.length === 0 ? undefined : waited(open);
    };
    if (task === undefined || ids.length === 0) {
        return;
    }
    ids.forEach((id) => {
        state.tasks.set(id, { task, agent });
    });
    await waited(ids);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on) => {
    const state: State = { session: Promise.withResolvers(), servers: new Map(), read: new Set(), unjudged: new Map(), tasks: new Map() };
    on('session.start', async ($, e, next) => {
        const answer = await next(e);
        const folder = (await $.env.get('TMPDIR')) ?? '/tmp';
        state.session.resolve({ served: () => started($, state.servers, folder) });
        if (state.servers.size === 0) {
            started($, state.servers, folder).catch(() => undefined);
        }
        return answer;
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
                ...(state.tasks.size > 0 && { owned: Object.fromEntries([...state.tasks].map(([id, { task }]) => [id, task])) }),
            },
            next.signal,
        );
        if (decision._tag !== 'Allow') {
            $.ui.log(`${$.plugin.name} ${decision._tag} ${e.tool_use_id}: ${decision._tag === 'Deny' ? decision.reason : decision.command}`, { to: 'debug' });
        }
        switch (decision._tag) {
            case 'Deny': {
                return { deny: decision.reason };
            }
            case 'Allow': {
                return next(e);
            }
            case 'Rewrite': {
                const { timeout: _timeout, ...untimed } = bash ?? { ...e, timeout: undefined };
                const answer = await next(bash === undefined ? { ...e, command: decision.command } : { ...(decision.background ? { ...untimed, run_in_background: true } : bash), command: decision.command });
                readied($, state, decision.tasks ?? [], answer.result, e.agentId).catch((cause: unknown) => {
                    $.ui.log(`${$.plugin.name} readiness failed: ${Error.isError(cause) ? cause.message : JSON.stringify(cause)}`, { to: 'debug' });
                });
                return decision.note !== undefined && answer.deny === undefined ? { ...answer, context: [...(answer.context ?? []), decision.note] } : answer;
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
    on('session.end', (_, e, next) => {
        state.read.clear();
        state.unjudged.clear();
        state.tasks.clear();
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
