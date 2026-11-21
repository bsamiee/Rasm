import type { EngineInterface, HookStream, ProcessSpawnChunk, ProcessSpawnResult, Register, Timer } from 'claude-code';
import type { Routes } from '#/claude/routes';

// --- [TYPES] ---------------------------------------------------------------------------

interface Task {
    readonly agent: string | undefined;
    readonly file: string;
    readonly told: string | undefined;
}
interface State {
    readonly served: PromiseWithResolvers<() => Promise<string>>;
    readonly servers: Map<Promise<string>, HookStream<ProcessSpawnChunk, ProcessSpawnResult>>;
    readonly tasks: Map<string, Task>;
    readonly scans: Set<Timer>;
    readonly unjudged: Map<string | undefined, string>;
    readonly turns: Set<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TRAILING_SLASHES = /\/+$/u;
const ERROR_LINE = /Error\b/u;
const STDERR_HELD = 65_536;
const TASK_OUTPUT = /\/tasks\/(?<id>[^/]+)\.output$/u;
const AGENT_TASK = /^a[0-9a-f]{16}$/u;
const TRANSCRIPT = /\/\.claude\/projects(?:\/[^/]+)?\/?$|\/\.claude\/projects\/.+\.jsonl$|\/\.codex\/(?:archived_)?sessions(?:\/|$)/u;
const SHELL_HISTORY = /\/\.(?:zsh|bash)_history$|\/\.zsh_sessions(?:\/|$)/u;
const MONITOR_MS = 300_000;
const WATCHDOG_MS = 300_000;

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

const posted = <R extends keyof Routes>($: EngineInterface, { served, servers }: State, route: R, body: Routes[R][0], signal?: AbortSignal): Promise<Routes[R][1]> => {
    const stopped = Promise.withResolvers<never>();
    const serving = () => served.promise.then((serve) => serve());
    const asked = (socketPath: string) => $.http.fetch(`http://${$.plugin.name}/${route}`, { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(body), socketPath });
    const answered = async (): Promise<Routes[R][1]> => {
        signal?.throwIfAborted();
        const [running = serving()] = servers.keys();
        const socket = await running;
        const first = await asked(socket).catch(() => undefined);
        signal?.throwIfAborted();
        const [current] = servers.keys();
        const { ok, status, text } = first ?? (await asked(await (current === undefined || (await current.catch(() => undefined)) === socket ? serving() : current)));
        // oxlint-disable-next-line typescript/no-unsafe-assignment -- hooks modules import no package to decode with, and the server encodes this body through the Routes schemas
        const answer: Routes[R][1] = ok ? JSON.parse(text) : await Promise.reject(new Error(`${route} answered ${status}, ${text}`));
        return answer;
    };
    signal?.addEventListener('abort', () => stopped.reject(signal.reason), { once: true });
    return Promise.race([answered(), stopped.promise]);
};

// --- [TASKS]

const followed = (tasks: State['tasks'], agent: string | undefined, answer: { readonly result?: unknown; readonly text?: string | undefined }): void => {
    const { result, text = '' } = answer;
    const id = typeof result === 'object' && result !== null && 'backgroundTaskId' in result && typeof result.backgroundTaskId === 'string' ? result.backgroundTaskId : undefined;
    const file = id === undefined ? undefined : text.split(/\s+/u).find((word) => word.endsWith(`/tasks/${id}.output`));
    if (id !== undefined && file !== undefined) {
        tasks.set(id, { agent, file, told: undefined });
    }
};

const scanned = async ($: EngineInterface, state: State, idle: boolean): Promise<void> => {
    const tasks = [...state.tasks];
    const [agents, watched] = tasks.length === 0 ? [[], []] : await Promise.all([$.agent.list(), posted($, state, 'tasks', { files: tasks.map(([, { file }]) => file) })]);
    const running = new Set(agents.filter(({ status }) => status === 'running').map(({ id }) => id));
    await Promise.all(
        tasks.map(async ([id, task], at) => {
            const { live = false, stuck } = watched[at] ?? {};
            const due = stuck !== undefined && stuck.key !== task.told && (task.agent === undefined ? idle : !running.has(task.agent));
            if (!live) {
                state.tasks.delete(id);
                return;
            }
            state.tasks.set(id, { ...task, told: due ? stuck.key : stuck && task.told });
            if (due) {
                const text = `Background task ${id} ${stuck.text}, so it looks stuck and its completion notice may never come (${task.file}): read its last lines, TaskStop ${id} if it hangs or loops, fix the cause, and rerun it bounded by a count or timeout`;
                await (task.agent === undefined ? $.prompt.submit({ text }) : $.session.send({ to: { agentId: task.agent }, text }));
            }
        }),
    );
};

// --- [ANSWERS]

const settled = <A extends { readonly deny?: unknown; readonly context?: readonly string[] | undefined }>(answer: A, note?: string) => (note !== undefined && answer.deny === undefined ? { ...answer, context: [...(answer.context ?? []), note] } : answer);

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on) => {
    const state: State = { served: Promise.withResolvers(), servers: new Map(), tasks: new Map(), scans: new Set(), unjudged: new Map(), turns: new Set() };
    on('session.start', async ($, e, next) => {
        const answer = await next(e);
        state.scans.forEach((timer) => timer.cancel());
        state.scans.clear();
        state.scans.add(
            $.clock.every(WATCHDOG_MS, () => {
                scanned($, state, state.turns.size === 0).catch((cause: unknown) => $.ui.log(`${$.plugin.name} watchdog failed, ${Error.isError(cause) ? cause.message : JSON.stringify(cause)}`, { to: 'debug' }));
            }),
        );
        const folder = (await $.env.get('TMPDIR')) ?? '/tmp';
        state.served.resolve(() => started($, state.servers, folder));
        if (state.servers.size === 0) {
            started($, state.servers, folder).catch(() => undefined);
        }
        return answer;
    });
    on('tool.call', { tool: ['Read', 'Grep'] }, (_$, e, next) => {
        const file = (e.tool === 'Read' ? e.file_path : e.path) ?? '';
        const deny = (
            [
                [AGENT_TASK.test(TASK_OUTPUT.exec(file)?.groups?.['id'] ?? ''), "is a running agent's output that the agent reports when it ends, SendMessage asks it for status"],
                [TRANSCRIPT.test(file), 'is a session transcript, atuin threads show <session id> --style ndjson reads it'],
                [SHELL_HISTORY.test(file), "is shell history, atuin search --filter-mode global --search-mode fulltext '<words>' reads it"],
            ] as const
        ).find(([held]) => held);
        return deny === undefined ? next(e) : { deny: `${file} ${deny[1]}` };
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
            },
            next.signal,
        );
        switch (decision._tag) {
            case 'Deny': {
                return { deny: decision.reason };
            }
            case 'Allow': {
                const answer = await next(e);
                followed(state.tasks, e.agentId, answer);
                return settled(answer, decision.note);
            }
            case 'Rewrite': {
                const { timeout: _timeout, ...untimed } = bash ?? { ...e, timeout: undefined };
                const answer = await next(bash === undefined ? { ...e, command: decision.command } : { ...(decision.background ? { ...untimed, run_in_background: true } : bash), command: decision.command });
                followed(state.tasks, e.agentId, answer);
                return settled(answer, decision.note);
            }
            case 'Monitor': {
                if (bash === undefined) {
                    return settled(await next({ ...e, command: decision.command }), decision.note);
                }
                const armed = await $.agent.call({ tool: 'Monitor', input: { command: decision.command, description: bash.description ?? 'Wait for the condition this call polled', timeout_ms: bash.timeout ?? MONITOR_MS }, ...(e.agentId !== undefined && { agentId: e.agentId }) }, { signal: next.signal });
                if (armed.deny !== undefined) {
                    return { deny: armed.deny };
                }
                const ran = {
                    result: { stdout: armed.text ?? '', stderr: '', interrupted: false },
                    context: ['function-hooks ran this wait as a Monitor: each line it prints and its exit reach you as notices and its timeout bounds it, so keep working or end the turn, and a foreground subagent, whose background work ends with its final answer, keeps working'],
                };
                return settled(armed.isError === true ? { ...ran, isError: armed.isError } : ran, decision.note);
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
    on('turn.start', (_$, e, next) => {
        if (e.agentId === undefined) {
            state.turns.add(e.turnId);
        }
        return next(e);
    });
    on('turn.complete', async (_$, e, next) => {
        const answer = await next(e);
        state.turns.delete(e.turnId);
        return answer;
    });
    on('session.end', (_$, e, next) => {
        state.tasks.clear();
        state.scans.forEach((timer) => timer.cancel());
        state.scans.clear();
        state.unjudged.clear();
        state.turns.clear();
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
