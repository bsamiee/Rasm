import type { AgentInfo, HttpResponse, On, ProcessSpawnChunk, ToolCallResult } from 'claude-code';
import { expect, mock, test } from 'claude-code/testing';

// --- [TYPES] ---------------------------------------------------------------------------

interface Watched {
    readonly file: string;
    readonly live: boolean;
    readonly stuck?: { readonly key: string; readonly text: string };
}
interface Given {
    readonly agents?: readonly AgentInfo[];
    readonly printed?: Readonly<Record<string, readonly ProcessSpawnChunk[]>>;
    readonly watched?: Iterator<Watched>;
    readonly result?: ToolCallResult;
}
interface Seen {
    readonly sent: (readonly [route: string, body: unknown])[];
    readonly ran: object[];
    readonly spawned: (readonly string[])[];
    readonly stopped: (readonly string[])[];
    readonly envs: (Readonly<Record<string, string>> | undefined)[];
    readonly told: (readonly [to: string, text: string])[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SESSION = { cwd: '/work', surface: 'terminal', isInteractive: true } as const;
const ENDING = { reason: 'clear', sessionId: 'session', resume: { id: 'session' } } as const;
const TURN = { turnId: 't1', answer: '', durationMs: 1, isAborted: false, reason: 'answer' } as const;
const RAN = { result: { stdout: 'ran', stderr: '', interrupted: false }, text: 'ran' } as const;
const UNJUDGED = 'function-hooks did not judge this call: ';
const ALLOWED = { _tag: 'Allow' } as const;
const LIST = { tool: 'Bash', command: 'ls' } as const;
const DECIDED = { session: ENDING.sessionId, call: expect.any(String) } as const;
const OUTPUT = '/tmp/claude-501/x/tasks/b7k2.output';
const BACKGROUND = { result: { result: { stdout: '', stderr: '', interrupted: false, backgroundTaskId: 'b7k2' }, text: `Command running in background with ID: b7k2. Output is being written to: ${OUTPUT}` } } as const;
const REPEATING = { file: OUTPUT, live: true, stuck: { key: 'repeats retry #', text: 'repeats "retry #"' } } as const;
const RUNNING = { file: OUTPUT, live: true } as const;
const SCAN_MS = 300_000;
const DATA = { CLAUDE_PLUGIN_DATA: '/data/function-hooks' } as const;
const COPY = { name: 'agent-a1', from: '/work/libs', base: 'head' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const answered = (answer: unknown): HttpResponse => ({ status: 200, ok: true, headers: {}, text: JSON.stringify(answer) });

const agentOf = (status: Exclude<AgentInfo['status'], 'waiting' | 'idle'>): AgentInfo => ({
    id: 'a1',
    description: 'browse',
    type: 'general-purpose',
    status,
    kind: 'local_agent',
    taskStatus: status,
    label: 'browse',
    startTime: 0,
    cwd: SESSION.cwd,
    tokenCount: 0,
    tokenSamples: [],
    hasRow: false,
});

const worldOf = (on: On, decide: () => HttpResponse | { readonly deny: string }, given: Given = {}) => {
    const seen: Seen = { sent: [], ran: [], spawned: [], stopped: [], envs: [], told: [] };
    const clock = mock.clock(on);
    const printed = new Map(Object.entries(given.printed ?? {}));
    on('session.start', (_, e) => ({ cwd: e.cwd }));
    on('session.end', (_, e) => ({ sessionId: e.sessionId }));
    on('session.id', () => ({ value: ENDING.sessionId }));
    on('session.cwd', () => ({ value: SESSION.cwd }));
    on('turn.start', (_, e) => ({ turnId: e.turnId }));
    on('turn.complete', () => ({ text: '' }));
    on('agent.list', () => ({ value: [...(given.agents ?? [])] }));
    on('env.get', () => ({ value: '/tmp/' }));
    on('process.hookEnv', () => ({ value: DATA }));
    on('ui.log', () => ({ value: undefined }));
    on('session.send', (_, e) => {
        seen.told.push([e.to, e.text]);
        return { isDelivered: true };
    });
    on('prompt.submit', (_, e) => {
        seen.told.push(['main', e.text]);
        return { text: e.text };
    });
    on('process.spawn', async function* (_, e, next) {
        next.signal.addEventListener(
            'abort',
            () => {
                seen.stopped.push(e.argv);
            },
            { once: true },
        );
        const chunks = e.argv.slice(0, 1).flatMap((program) => printed.get(program) ?? (program === 'node' ? [{ stream: 'stdout', text: `${e.argv.at(-1)}\n` } as const] : []));
        const listening = chunks.some(({ stream }) => stream === 'stdout');
        seen.spawned.push(e.argv);
        seen.envs.push(e.env);
        yield* chunks;
        await (listening ? clock.sleep(Number.MAX_SAFE_INTEGER) : undefined);
        return { value: { code: listening ? 0 : 1, signal: null } };
    });
    on('http.fetch', (_, e) => {
        const route = new URL(e.url).pathname.slice(1);
        seen.sent.push([route, typeof e.init?.body === 'string' ? JSON.parse(e.init.body) : e.init?.body]);
        const step = route === 'tasks' ? given.watched?.next() : undefined;
        const reply = route === 'tasks' ? answered(step === undefined || step.done === true ? [] : [step.value]) : decide();
        return 'deny' in reply ? reply : { value: reply };
    });
    on('tool.call', (_, e) => {
        const { tool_use_id: _id, ...input } = e;
        seen.ran.push(input);
        return given.result ?? RAN;
    });
    on('agent.call', (_, e) => {
        seen.ran.push(e);
        return { value: given.result ?? RAN };
    });
    const scanned = async () => {
        await clock.advance(SCAN_MS);
        await clock.settle();
        return seen.told.length;
    };
    return { ...seen, scanned };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

[
    { decision: ALLOWED, ran: [LIST], resolved: RAN },
    { decision: { _tag: 'Deny', reason: 'git stash hides work, commit it' }, ran: [], resolved: { deny: 'git stash hides work, commit it' } },
    {
        decision: { _tag: 'Rewrite', command: 'rg -n x', background: true, note: 'grep became rg' },
        ran: [{ tool: 'Bash', command: 'rg -n x', run_in_background: true }],
        resolved: { ...RAN, context: ['grep became rg'] },
    },
    { decision: { _tag: 'Allow', note: 'Task b1 still runs' }, ran: [LIST], resolved: { ...RAN, context: ['Task b1 still runs'] } },
].forEach(({ decision, ran, resolved }) => {
    test(`Bash call resolves by ${decision._tag} decision${'note' in decision ? ' with its note as context' : ''}`, async ($, on) => {
        const world = worldOf(on, () => answered(decision));
        await $.session.start(SESSION);
        expect(await $.tool.call(LIST)).toEqual(resolved);
        expect(world.ran).toEqual(ran);
        expect(world.sent).toEqual([['decide', { command: 'ls', cwd: SESSION.cwd, kind: 'foreground', ...DECIDED }]]);
    });
});

test('Rewrite sets only Monitor input fields on a Monitor call', async ($, on) => {
    const world = worldOf(on, () => answered({ _tag: 'Rewrite', command: 'tail -F b.log', background: true }));
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'Monitor', description: 'log', timeout_ms: 1000, command: 'tail -f a.log' });
    expect(world.ran).toEqual([{ tool: 'Monitor', description: 'log', timeout_ms: 1000, command: 'tail -F b.log' }]);
    expect(world.sent).toEqual([['decide', { command: 'tail -f a.log', cwd: SESSION.cwd, kind: 'monitor', ...DECIDED }]]);
});

test('Monitor decision answers a Bash wait with a Monitor the calling agent runs, bounded by its timeout', async ($, on) => {
    const wait = 'until rg -q done b.log; do sleep 5; done';
    const world = worldOf(on, () => answered({ _tag: 'Monitor', command: wait }));
    await $.session.start(SESSION);
    const answer = await $.tool.call({ tool: 'Bash', command: wait, description: 'Wait for done', timeout: 600_000, agentId: 'a1' });
    expect(world.ran).toEqual([{ tool: 'Monitor', input: { command: wait, description: 'Wait for done', timeout_ms: 600_000 }, agentId: 'a1' }]);
    expect(answer).toEqual({ result: { stdout: 'ran', stderr: '', interrupted: false }, context: [expect.stringContaining('ran this wait as a Monitor')] });
});

test('Monitor decision passes the Monitor error and a timeout past half an hour through to the Bash call', async ($, on) => {
    const wait = 'until rg -q done b.log; do sleep 5; done';
    const failed = { isError: true, result: 'Monitor failed', text: 'Monitor failed' } as const;
    const world = worldOf(on, () => answered({ _tag: 'Monitor', command: wait }), { result: failed });
    await $.session.start(SESSION);
    const answers = [await $.tool.call({ tool: 'Bash', command: wait, description: 'Wait for done', timeout: 3_600_000 }), await $.tool.call({ tool: 'Bash', command: wait, description: 'Wait for done' })];
    expect(world.ran).toEqual([3_600_000, 300_000].map((timeout_ms) => ({ tool: 'Monitor', input: { command: wait, description: 'Wait for done', timeout_ms } })));
    expect(answers).toEqual(answers.map(() => ({ result: { stdout: 'Monitor failed', stderr: '', interrupted: false }, isError: true, context: [expect.stringContaining('ran this wait as a Monitor')] })));
});

test('Watchdog wakes the agent that owns a background task repeating one line, and drops the task once nothing writes it', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), { watched: [REPEATING, { file: OUTPUT, live: false }].values(), agents: [agentOf('completed')], ...BACKGROUND });
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'Bash', command: 'fetch.sh', run_in_background: true, agentId: 'a1' });
    expect([await world.scanned(), await world.scanned(), await world.scanned()]).toEqual([1, 1, 1]);
    expect(world.told).toEqual([['a1', expect.stringContaining('b7k2 repeats "retry #"')]]);
    expect(world.sent.filter(([route]) => route === 'tasks')).toEqual([
        ['tasks', { files: [OUTPUT] }],
        ['tasks', { files: [OUTPUT] }],
    ]);
});

test('Watchdog tells the owner once per stuck spell, again after the task moves and stalls anew', async ($, on) => {
    const spells = [RUNNING, REPEATING, REPEATING, REPEATING, RUNNING, REPEATING] as const;
    const world = worldOf(on, () => answered(ALLOWED), { watched: spells.values(), agents: [agentOf('completed')], ...BACKGROUND });
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'Bash', command: 'nx serve web', run_in_background: true, agentId: 'a1' });
    const counts = await spells.reduce<Promise<readonly number[]>>(async (scanned) => [...(await scanned), await world.scanned()], Promise.resolve([]));
    expect(counts).toEqual([0, 1, 1, 1, 1, 2]);
    expect(world.told[0]).toEqual(['a1', expect.stringContaining('b7k2 repeats "retry #"')]);
});

test('Watchdog prompts the main conversation about its stuck task only while the conversation is idle, once', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), { watched: [REPEATING, REPEATING, REPEATING, REPEATING].values(), ...BACKGROUND });
    await $.session.start(SESSION);
    await $.turn.start({ text: 'fetch', turnId: 't1' });
    await $.tool.call({ tool: 'Bash', command: 'fetch.sh', run_in_background: true });
    const busy = [await world.scanned(), await world.scanned()];
    await $.turn.complete(TURN);
    expect([...busy, await world.scanned(), await world.scanned()]).toEqual([0, 0, 1, 1]);
    expect(world.told).toEqual([['main', expect.stringContaining('b7k2 repeats "retry #"')]]);
});

test('Watchdog tells of a stuck task started after the conversation is cleared', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), { watched: [REPEATING].values(), ...BACKGROUND });
    await $.session.start(SESSION);
    await $.session.end(ENDING);
    await $.tool.call({ tool: 'Bash', command: 'fetch.sh', run_in_background: true });
    expect(await world.scanned()).toBe(1);
});

test('Watchdog watches a background task an earlier load of the module stored, and declines one stored under another shape', async ($, on) => {
    const stored = [
        { shape: 'tasks-1', value: { b7k2: { file: OUTPUT } } },
        { shape: 'tasks-0', value: { b7k2: OUTPUT } },
    ].values();
    const world = worldOf(on, () => answered(ALLOWED), { watched: [REPEATING, REPEATING].values() });
    on('state.get', () => ({ value: { value: stored.next().value, version: 1 } }));
    on('state.set', () => ({ value: { isSet: true, version: 2 } }));
    await $.session.start(SESSION);
    expect([await world.scanned(), await world.scanned()]).toEqual([1, 1]);
    expect(world.sent).toEqual([['tasks', { files: [OUTPUT] }]]);
});

test('Decide request states run kind and calling agent, and both calls use one server started unwatched from the plugin folder with its data folder', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED));
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'Bash', command: 'make', run_in_background: true, timeout: 5000, agentId: 'a1' });
    await $.tool.call({ tool: 'Bash', command: 'make' });
    expect(world.sent).toEqual([
        ['decide', { command: 'make', cwd: SESSION.cwd, kind: 'background', agent: 'a1', ...DECIDED }],
        ['decide', { command: 'make', cwd: SESSION.cwd, kind: 'foreground', ...DECIDED }],
    ]);
    expect(world.spawned).toEqual([['node', expect.stringMatching(/\/claude\/server\.ts$/u), expect.stringMatching(/^\/tmp\/function-hooks-[\da-f]{8}\.sock$/u)]]);
    expect(world.stopped).toEqual([]);
    expect(world.envs).toEqual([DATA]);
});

test('Call with a failed decision runs as written and notes each agent once per cause', async ($, on) => {
    const causes = ['TypeError: rule threw\n    at decide', 'TypeError: rule threw', 'TypeError: rule threw', 'RangeError: other'].values();
    const world = worldOf(on, () => ({ status: 500, ok: false, headers: {}, text: String(causes.next().value) }));
    const noted = { ...RAN, context: [`${UNJUDGED}decide answered 500, TypeError: rule threw`] };
    await $.session.start(SESSION);
    const calls = [await $.tool.call({ ...LIST, agentId: 'a1' }), await $.tool.call({ ...LIST, agentId: 'a1' }), await $.tool.call({ ...LIST, agentId: 'a2' }), await $.tool.call({ ...LIST, agentId: 'a1' })];
    expect(calls).toEqual([noted, RAN, noted, { ...RAN, context: [`${UNJUDGED}decide answered 500, RangeError: other`] }]);
    expect(world.ran).toHaveLength(calls.length);
});

test('Call runs as written with crash cause noted when server exits before listening', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), { printed: { node: [{ stream: 'stderr', text: 'file:///work/rules/decide.ts:3\nSyntaxError: Unexpected token\n    at compile\n\nNode.js v26\n' }] } });
    await $.session.start(SESSION);
    expect(await $.tool.call(LIST)).toEqual({ ...RAN, context: [`${UNJUDGED}SyntaxError: Unexpected token`] });
    expect(world.sent).toEqual([]);
});

test('Call after a lost socket stops its server, starts one new server, and gets its decision', async ($, on) => {
    const world = worldOf(on, () => (world.sent.length === 2 ? { deny: 'socket closed' } : answered({ _tag: 'Deny', reason: 'no' })));
    await $.session.start(SESSION);
    await $.tool.call(LIST);
    expect(await $.tool.call(LIST)).toEqual({ deny: 'no' });
    expect([world.spawned, world.stopped].map((streams) => streams.map(([program]) => program))).toEqual([['node', 'node'], ['node']]);
});

test('Read is refused with the reason the server gives its file, and runs where the server allows it', async ($, on) => {
    const decisions = [{ _tag: 'Deny', reason: '/Users/u/.zsh_history is shell history, atuin reads it' }, ALLOWED].values();
    const world = worldOf(on, () => answered(decisions.next().value));
    const history = { tool: 'Read', file_path: '/Users/u/.zsh_history' } as const;
    const source = { tool: 'Read', file_path: '/work/README.md' } as const;
    await $.session.start(SESSION);
    expect([await $.tool.call(history), await $.tool.call(source)]).toEqual([{ deny: '/Users/u/.zsh_history is shell history, atuin reads it' }, RAN]);
    expect(world.ran).toEqual([source]);
    expect(world.sent).toEqual([
        ['read', { file: history.file_path }],
        ['read', { file: source.file_path }],
    ]);
});

test('Working copy asked for a subagent is its own folder in place, released with nothing lost, and one asked for the session is refused', async ($, on) => {
    worldOf(on, () => answered(ALLOWED));
    const made = await $.vcs.isolate({ ...COPY, for: 'agent', agentId: 'a1' });
    expect(made).toEqual({ path: COPY.from, vcs: expect.any(String) });
    const held = { path: COPY.from, name: COPY.name, vcs: made.vcs ?? '', agentId: 'a1' } as const;
    expect([await $.vcs.status(held), await $.vcs.release({ ...held, isDiscard: false, reason: 'the subagent ended' })]).toEqual([{ changes: [], unpublished: 0 }, { isReleased: true }]);
    expect(await $.vcs.isolate({ ...COPY, for: 'session' })).toEqual({ deny: expect.stringContaining('work in place') });
});
