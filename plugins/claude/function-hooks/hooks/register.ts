import { atom, type EngineInterface, type HookStream, type ProcessSpawnChunk, type ProcessSpawnResult, type Register, read, update } from 'claude-code';

// --- [TYPES] ---------------------------------------------------------------------------

type Decision = { readonly _tag: 'Allow'; readonly note?: string } | { readonly _tag: 'Deny'; readonly reason: string } | { readonly _tag: 'Rewrite'; readonly command: string; readonly background: boolean; readonly note?: string } | { readonly _tag: 'Monitor'; readonly command: string; readonly note?: string };
interface Routes {
    readonly decide: readonly [{ readonly command: string; readonly cwd: string; readonly kind: 'foreground' | 'background' | 'monitor'; readonly agent?: string; readonly session: string; readonly call: string }, Decision];
    readonly read: readonly [{ readonly file: string }, Decision];
    readonly tasks: readonly [{ readonly files: readonly string[] }, readonly { readonly file: string; readonly live: boolean; readonly stuck?: { readonly key: string; readonly text: string } }[]];
    readonly opened: readonly [Caller & { readonly file?: string; readonly until?: number }, Record<never, never>];
    readonly closed: readonly [Caller, Record<never, never>];
    readonly wrote: readonly [Omit<Caller, 'cwd'> & { readonly file: string; readonly result: unknown }, Record<never, never>];
}
interface Caller {
    readonly session: string;
    readonly agent?: string;
    readonly call: string;
    readonly cwd: string;
}
type Phase = { readonly kind: 'starting'; readonly waiting: ((cause?: Error) => void)[] } | { readonly kind: 'listening' } | { readonly kind: 'failed'; readonly cause: Error };
interface Server {
    readonly child: HookStream<ProcessSpawnChunk, ProcessSpawnResult>;
    readonly socket: string;
    readonly phase: Phase;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TRAILING_SLASHES = /\/+$/u;
const ERROR_LINE = /Error\b/u;
const STDERR_HELD = 65_536;
const MONITOR_MS = 300_000;
const WATCHDOG_MS = 300_000;
const IN_PLACE = 'inplace';
const WORKTREE = 'A worktree makes a second checkout, work in place';
const TASKS = atom({ plugin: 'function-hooks', key: 'tasks' } as const, {}, { shape: 'tasks-1' });

// --- [SERVICES] ------------------------------------------------------------------------

const live: { server: Promise<Server> | undefined } = { server: undefined };
const turns = new Set<string>();
const unjudged = new Map<string | undefined, string>();

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SERVER]

const moved = (server: Server, phase: Phase): void => {
    const { phase: before } = server;
    Object.assign(server, { phase });
    (before.kind === 'starting' ? before.waiting : []).forEach((settle) => {
        settle(phase.kind === 'failed' ? phase.cause : undefined);
    });
};

const listening = ({ socket, phase }: Server): Promise<string> => {
    if (phase.kind !== 'starting') {
        return phase.kind === 'listening' ? Promise.resolve(socket) : Promise.reject(phase.cause);
    }
    return new Promise((resolve, reject) => {
        phase.waiting.push((cause) => {
            if (cause === undefined) {
                resolve(socket);
            } else {
                reject(cause);
            }
        });
    });
};

const supervised = async (server: Server): Promise<void> => {
    const heard = { stderr: '' };
    const failed = (fallback: string): Phase => {
        const lines = heard.stderr.split('\n').filter((line) => line.trim() !== '');
        return { kind: 'failed', cause: new Error(lines.find((line) => ERROR_LINE.test(line)) ?? lines.at(-1) ?? fallback) };
    };
    for await (const { stream, text } of server.child) {
        heard.stderr = stream === 'stderr' ? `${heard.stderr}${text}`.slice(-STDERR_HELD) : heard.stderr;
        if (stream === 'stdout' && text.split('\n').includes(server.socket)) {
            moved(server, { kind: 'listening' });
        }
    }
    const ended = await server.child.result.catch(() => undefined);
    moved(server, failed(`server ended ${ended?.signal ?? ended?.code ?? 'replaced'}`));
};

const started = async ($: EngineInterface): Promise<Server> => {
    const [folder = '/tmp', env] = await Promise.all([$.env.get('TMPDIR'), $.process.hookEnv()]);
    const socket = `${folder.replace(TRAILING_SLASHES, '')}/${$.plugin.name}-${crypto.randomUUID().slice(0, 8)}.sock`;
    return { child: $.process.spawn({ argv: ['node', `${$.plugin.root}/claude/server.ts`, socket], cwd: $.plugin.root, env }), socket, phase: { kind: 'starting', waiting: [] } };
};

const serving = ($: EngineInterface, lost?: Promise<Server>): Promise<Server> => {
    const { server: held } = live;
    if (held !== undefined && held !== lost) {
        return held;
    }
    const fresh = started($);
    live.server = fresh;
    held?.then(({ child }) => child.return({ code: null, signal: null })).catch(() => undefined);
    fresh
        .then(supervised)
        .finally(() => {
            live.server = live.server === fresh ? undefined : live.server;
        })
        .catch(() => undefined);
    return fresh;
};

const asked = <R extends keyof Routes>($: EngineInterface, route: R, body: Routes[R][0], signal?: AbortSignal): Promise<Routes[R][1]> => {
    const posted = (socketPath: string) => $.http.fetch(`http://${$.plugin.name}/${route}`, { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(body), socketPath });
    const answered = async (): Promise<Routes[R][1]> => {
        const server = serving($);
        const first = await posted(await listening(await server)).catch(() => undefined);
        signal?.throwIfAborted();
        const { ok, status, text } = first ?? (await posted(await listening(await serving($, server))));
        if (!ok) {
            throw new Error(`${route} answered ${status}, ${text}`);
        }
        const answer: Routes[R][1] = JSON.parse(text);
        return answer;
    };
    const stopped = new Promise<never>((_resolve, reject) => {
        signal?.addEventListener(
            'abort',
            () => {
                reject(new Error(`${route} call abandoned`));
            },
            { once: true },
        );
    });
    return Promise.race([answered(), stopped]);
};

// --- [TASKS]

const followed = async ($: EngineInterface, caller: Caller, { result, text = '' }: { readonly result?: unknown; readonly text?: string | undefined }, signal: AbortSignal): Promise<void> => {
    const id = typeof result === 'object' && result !== null && 'backgroundTaskId' in result && typeof result.backgroundTaskId === 'string' ? result.backgroundTaskId : undefined;
    const file = id === undefined ? undefined : text.split(/\s+/u).find((word) => word.endsWith(`/tasks/${id}.output`));
    if (id !== undefined && file !== undefined) {
        await update($, TASKS, (tasks) => ({ ...tasks, [id]: { file, ...caller } }));
    }
    await (id === undefined ? asked($, 'closed', caller, signal) : undefined);
};

const scanned = async ($: EngineInterface): Promise<void> => {
    const tasks = Object.entries(await read($, TASKS));
    if (tasks.length === 0) {
        return;
    }
    const [agents, watched] = await Promise.all([$.agent.list(), asked($, 'tasks', { files: tasks.map(([, { file }]) => file) })]);
    const running = new Set(agents.filter(({ status }) => status === 'running').map(({ id }) => id));
    const judged = tasks.map(([id, task], at) => {
        const { live: written = false, stuck } = watched[at] ?? {};
        const resting = task.agent === undefined ? turns.size === 0 : !running.has(task.agent);
        const due = stuck !== undefined && stuck.key !== task.told && resting ? [stuck] : [];
        const { told: _told, ...untold } = task;
        const told = due.map(({ key }) => ({ ...untold, told: key })).at(0) ?? task;
        return { id, task, due, held: written ? [stuck === undefined ? untold : told] : [] };
    });
    const scans = new Map(judged.map(({ id, held }) => [id, held] as const));
    await update($, TASKS, (stored) => Object.fromEntries(Object.entries(stored).flatMap(([id, task]) => (scans.get(id) ?? [task]).map((kept) => [id, kept] as const))));
    await Promise.all(judged.flatMap(({ task: { file: _file, told: _told, ...caller }, held }) => (held.length === 0 && caller.call !== undefined && caller.session !== undefined && caller.cwd !== undefined ? [asked($, 'closed', { ...caller, call: caller.call, session: caller.session, cwd: caller.cwd })] : [])));
    await Promise.all(
        judged.flatMap(({ id, task: { agent, file }, due }) =>
            due.map((evidence) => {
                const text = `Background task ${id} ${evidence.text}, so it looks stuck and its completion notice may never come (${file}): read its last lines, TaskStop ${id} if it hangs or loops, fix the cause, and rerun it bounded by a count or timeout`;
                return agent === undefined ? $.prompt.submit({ text }) : $.session.send({ to: { agentId: agent }, text });
            }),
        ),
    );
};

// --- [ANSWERS]

const bounded = ({ tool, timeout, timeout_ms }: { readonly tool: string } & Partial<Record<'timeout' | 'timeout_ms', number>>, monitored: boolean) => (tool === 'Monitor' || monitored ? { until: Date.now() + (timeout_ms ?? timeout ?? MONITOR_MS) } : {});

const noted = <A extends { readonly deny?: unknown; readonly context?: readonly string[] | undefined }>(answer: A, note?: string) => (note !== undefined && answer.deny === undefined ? { ...answer, context: [...(answer.context ?? []), note] } : answer);

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on) => {
    on('session.start', async ($, e, next) => {
        const answer = await next(e);
        $.clock.every(WATCHDOG_MS, () => {
            scanned($).catch((error: unknown) => $.ui.log(`${$.plugin.name} watchdog failed, ${typeof error === 'object' && error !== null && 'message' in error && typeof error.message === 'string' ? error.message : JSON.stringify(error)}`, { to: 'debug' }));
        });
        serving($).catch(() => undefined);
        return answer;
    });
    on('vcs.isolate', (_$, e) => (e.for === 'agent' ? { path: e.from, vcs: IN_PLACE } : { deny: WORKTREE })).catch(() => ({ deny: WORKTREE }));
    on('vcs.status', { vcs: IN_PLACE }, () => ({ changes: [], unpublished: 0 }));
    on('vcs.release', { vcs: IN_PLACE }, () => ({ isReleased: true }));
    on('tool.call', { tool: ['Bash', 'Monitor', 'Read'] }, async ($, e, next) => {
        if (e.tool === 'Read') {
            const reading = await asked($, 'read', { file: e.file_path }, next.signal);
            return reading._tag === 'Deny' ? { deny: reading.reason } : next(e);
        }
        if (e.command === undefined) {
            return next(e);
        }
        const [cwd, session] = await Promise.all([$.session.cwd(), $.session.id()]);
        const kind = e.tool === 'Bash' && e.run_in_background === true ? 'background' : ({ Bash: 'foreground', Monitor: 'monitor' } as const)[e.tool];
        const caller = { session, call: e.tool_use_id, cwd, ...(e.agentId !== undefined && { agent: e.agentId }) };
        const decision = await asked($, 'decide', { command: e.command, kind, ...caller }, next.signal);
        if (decision._tag === 'Deny') {
            return { deny: decision.reason };
        }
        await asked($, 'opened', { ...caller, ...bounded(e, decision._tag === 'Monitor') }, next.signal);
        if (decision._tag === 'Allow' || e.tool === 'Monitor') {
            const answer = await next(decision._tag === 'Allow' ? e : { ...e, command: decision.command });
            await (e.tool === 'Bash' ? followed($, caller, answer, next.signal) : undefined);
            return noted(answer, decision.note);
        }
        if (decision._tag === 'Monitor') {
            const armed = await $.agent.call({ tool: 'Monitor', input: { command: decision.command, description: e.description ?? 'Wait for the condition this call polled', timeout_ms: e.timeout ?? MONITOR_MS }, ...(e.agentId !== undefined && { agentId: e.agentId }) }, { signal: next.signal });
            const ran = {
                result: { stdout: armed.text ?? '', stderr: '', interrupted: false },
                context: ['function-hooks ran this wait as a Monitor: each line it prints and its exit reach you as notices and its timeout bounds it, so keep working or end the turn, and a foreground subagent, whose background work ends with its final answer, keeps working'],
            };
            return armed.deny === undefined ? noted(armed.isError === true ? { ...ran, isError: armed.isError } : ran, decision.note) : { deny: armed.deny };
        }
        const { timeout: _timeout, ...untimed } = e;
        const answer = await next(decision.background ? { ...untimed, command: decision.command, run_in_background: true } : { ...e, command: decision.command });
        await followed($, caller, answer, next.signal);
        return noted(answer, decision.note);
    }).catch(async ($, e, next) => {
        if (next.called) {
            return next(e);
        }
        const line = `${$.plugin.name} did not judge this call: ${(next.error.message ?? next.error.kind).split('\n', 1).join('')}`;
        const answer = await next(e);
        $.ui.log(line, { to: 'debug' });
        if (answer.deny !== undefined || unjudged.get(e.agentId) === line) {
            return answer;
        }
        unjudged.set(e.agentId, line);
        return { ...answer, context: [...(answer.context ?? []), line] };
    });
    on('tool.call', { tool: ['Edit', 'Write'] }, async ($, e, next) => {
        const [cwd, session] = await Promise.all([$.session.cwd(), $.session.id()]);
        const caller = { session, call: e.tool_use_id, ...(e.agentId !== undefined && { agent: e.agentId }) };
        await asked($, 'opened', { ...caller, cwd, file: e.file_path }, next.signal);
        const answer = await next(e);
        await asked($, 'wrote', { ...caller, file: e.file_path, result: answer.deny === undefined && answer.isError !== true ? answer.result : undefined }, next.signal);
        return answer;
    });
    on('turn.start', (_$, e, next) => {
        if (e.agentId === undefined) {
            turns.add(e.turnId);
        }
        return next(e);
    });
    on('turn.complete', async (_$, e, next) => {
        const answer = await next(e);
        turns.delete(e.turnId);
        return answer;
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
