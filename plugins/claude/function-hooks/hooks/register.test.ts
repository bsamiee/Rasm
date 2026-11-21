import type { AgentInfo, HttpResponse, On, ProcessSpawnChunk, ToolCallResult } from 'claude-code';
import { expect, mock, test } from 'claude-code/testing';
import type { Routes } from '#/claude/routes';

// --- [TYPES] ---------------------------------------------------------------------------

type Decision = Routes['decide'][1];
type Watched = Routes['tasks'][1][number];
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
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SESSION = { cwd: '/work', surface: 'terminal', isInteractive: true } as const;
const ENDING = { reason: 'clear', sessionId: 'session', resume: { id: 'session' } } as const;
const TURN = { turnId: 't1', answer: '', durationMs: 1, isAborted: false, reason: 'answer' } as const;
const RAN = { result: { stdout: 'ran', stderr: '', interrupted: false }, text: 'ran' } as const;
const UNJUDGED = 'function-hooks did not judge this command: Error: ';
const ALLOWED = { _tag: 'Allow' } as const satisfies Decision;
const LIST = { tool: 'Bash', command: 'ls' } as const;
const DECIDED = { session: ENDING.sessionId, call: expect.any(String) } as const;
const OUTPUT = '/tmp/claude-501/x/tasks/b7k2.output';
const BACKGROUND = { result: { result: { stdout: '', stderr: '', interrupted: false, backgroundTaskId: 'b7k2' }, text: `Command running in background with ID: b7k2. Output is being written to: ${OUTPUT}` } } as const;
const REPEATING = { file: OUTPUT, live: true, stuck: { key: 'repeats retry #', text: 'repeats "retry #"' } } as const satisfies Watched;
const QUIET = { file: OUTPUT, live: true, stuck: { key: 'quiet', text: 'wrote nothing for 12m' } } as const satisfies Watched;
const RUNNING = { file: OUTPUT, live: true } as const satisfies Watched;

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

const worldOf = (on: On, decide: () => HttpResponse | Error, given: Given = {}) => {
    const seen: Seen = { sent: [], ran: [], spawned: [], stopped: [] };
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
    on('ui.log', () => ({ value: undefined }));
    on('process.spawn', async function* (_, e, next) {
        next.signal.addEventListener(
            'abort',
            () => {
                seen.stopped.push(e.argv);
            },
            { once: true },
        );
        const chunks = e.argv.slice(0, 1).flatMap((program) => printed.get(program) ?? (program === 'mise' ? [{ stream: 'stdout', text: `${e.argv.at(-1)}\n` } as const] : []));
        const listening = chunks.some(({ stream }) => stream === 'stdout');
        seen.spawned.push(e.argv);
        yield* chunks;
        await (listening ? clock.sleep(Number.MAX_SAFE_INTEGER) : undefined);
        return { value: { code: listening ? 0 : 1, signal: null } };
    });
    on('http.fetch', (_, e) => {
        const route = new URL(e.url).pathname.slice(1);
        seen.sent.push([route, typeof e.init?.body === 'string' ? JSON.parse(e.init.body) : e.init?.body]);
        const step = route === 'tasks' ? given.watched?.next() : undefined;
        const reply = route === 'tasks' ? answered(step === undefined || step.done === true ? [] : [step.value]) : decide();
        return Error.isError(reply) ? { deny: reply.message } : { value: reply };
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
    return { ...seen, clock };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

(
    [
        { decision: ALLOWED, ran: [LIST], resolved: RAN },
        { decision: { _tag: 'Deny', reason: 'git stash hides work, commit it' }, ran: [], resolved: { deny: 'git stash hides work, commit it' } },
        {
            decision: { _tag: 'Rewrite', command: 'rg -n x', background: true, note: 'grep became rg' },
            ran: [{ tool: 'Bash', command: 'rg -n x', run_in_background: true }],
            resolved: { ...RAN, context: ['grep became rg'] },
        },
        { decision: { _tag: 'Allow', note: 'Task b1 still runs' }, ran: [LIST], resolved: { ...RAN, context: ['Task b1 still runs'] } },
    ] satisfies readonly { readonly decision: Decision; readonly ran: readonly object[]; readonly resolved: object }[]
).forEach(({ decision, ran, resolved }) => {
    test(`Bash call resolves by ${decision._tag} decision${'note' in decision ? ' with its note as context' : ''}`, async ($, on) => {
        const world = worldOf(on, () => answered(decision));
        await $.session.start(SESSION);
        expect(await $.tool.call(LIST)).toEqual(resolved);
        expect(world.ran).toEqual(ran);
        expect(world.sent).toEqual([['decide', { command: 'ls', cwd: SESSION.cwd, kind: 'foreground', ...DECIDED }]]);
    });
});

test('Rewrite sets only Monitor input fields on a Monitor call', async ($, on) => {
    const world = worldOf(on, () => answered({ _tag: 'Rewrite', command: 'tail -F b.log', background: true } satisfies Decision));
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'Monitor', description: 'log', timeout_ms: 1000, command: 'tail -f a.log' });
    expect(world.ran).toEqual([{ tool: 'Monitor', description: 'log', timeout_ms: 1000, command: 'tail -F b.log' }]);
    expect(world.sent).toEqual([['decide', { command: 'tail -f a.log', cwd: SESSION.cwd, kind: 'monitor', ...DECIDED }]]);
});

test('Monitor decision answers a Bash wait with a Monitor the calling agent runs, bounded by its timeout', async ($, on) => {
    const wait = 'until rg -q done b.log; do sleep 5; done';
    const world = worldOf(on, () => answered({ _tag: 'Monitor', command: wait } satisfies Decision));
    await $.session.start(SESSION);
    const answer = await $.tool.call({ tool: 'Bash', command: wait, description: 'Wait for done', timeout: 600_000, agentId: 'a1' });
    expect(world.ran).toEqual([{ tool: 'Monitor', input: { command: wait, description: 'Wait for done', timeout_ms: 600_000 }, agentId: 'a1' }]);
    expect(answer).toEqual({ result: { stdout: 'ran', stderr: '', interrupted: false }, context: [expect.stringContaining('ran this wait as a Monitor')] });
});

test('Monitor decision passes the Monitor error and a timeout past half an hour through to the Bash call', async ($, on) => {
    const wait = 'until rg -q done b.log; do sleep 5; done';
    const failed = { isError: true, result: 'Monitor failed', text: 'Monitor failed' } as const;
    const world = worldOf(on, () => answered({ _tag: 'Monitor', command: wait } satisfies Decision), { result: failed });
    await $.session.start(SESSION);
    const answers = [await $.tool.call({ tool: 'Bash', command: wait, description: 'Wait for done', timeout: 3_600_000 }), await $.tool.call({ tool: 'Bash', command: wait, description: 'Wait for done' })];
    expect(world.ran).toEqual([3_600_000, 300_000].map((timeout_ms) => ({ tool: 'Monitor', input: { command: wait, description: 'Wait for done', timeout_ms } })));
    expect(answers).toEqual(answers.map(() => ({ result: { stdout: 'Monitor failed', stderr: '', interrupted: false }, isError: true, context: [expect.stringContaining('ran this wait as a Monitor')] })));
});

test('Watchdog wakes the agent that owns a background task repeating one line, and drops the task once nothing writes it', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), { watched: [REPEATING, { file: OUTPUT, live: false }].values(), agents: [agentOf('completed')], ...BACKGROUND });
    const sent: unknown[] = [];
    on('session.send', (_, e) => {
        sent.push(e);
        return { isDelivered: true };
    });
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'Bash', command: 'fetch.sh', run_in_background: true, agentId: 'a1' });
    await Array.from({ length: 3 }).reduce<Promise<void>>(async (scanned) => {
        await scanned;
        await world.clock.advance(300_000);
        await world.clock.settle();
    }, Promise.resolve());
    expect(sent).toEqual([expect.objectContaining({ to: 'a1', text: expect.stringContaining('b7k2 repeats "retry #"') })]);
    expect(world.sent.filter(([route]) => route === 'tasks')).toEqual([
        ['tasks', { files: [OUTPUT] }],
        ['tasks', { files: [OUTPUT] }],
    ]);
});

test('Watchdog tells the owner once per stuck spell, again after the task moves and stalls anew', async ($, on) => {
    const spells = [RUNNING, QUIET, QUIET, QUIET, RUNNING, QUIET] as const;
    const world = worldOf(on, () => answered(ALLOWED), { watched: spells.values(), agents: [agentOf('completed')], ...BACKGROUND });
    const sent: unknown[] = [];
    on('session.send', (_, e) => {
        sent.push(e);
        return { isDelivered: true };
    });
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'Bash', command: 'nx serve web', run_in_background: true, agentId: 'a1' });
    const counts = await spells.reduce<Promise<readonly number[]>>(async (scanned) => {
        const before = await scanned;
        await world.clock.advance(300_000);
        await world.clock.settle();
        return [...before, sent.length];
    }, Promise.resolve([]));
    expect(counts).toEqual([0, 1, 1, 1, 1, 2]);
    expect(sent).toEqual([expect.objectContaining({ to: 'a1', text: expect.stringContaining('b7k2 wrote nothing for 12m') }), expect.objectContaining({ to: 'a1' })]);
});

test('Watchdog prompts the main conversation about its stuck task only while the conversation is idle, once', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), { watched: [REPEATING, REPEATING, REPEATING, REPEATING].values(), ...BACKGROUND });
    const submitted: string[] = [];
    on('prompt.submit', (_, e) => {
        submitted.push(e.text);
        return { text: e.text };
    });
    const scanned = async () => {
        await world.clock.advance(300_000);
        await world.clock.settle();
        return submitted.length;
    };
    await $.session.start(SESSION);
    await $.turn.start({ text: 'fetch', turnId: 't1' });
    await $.tool.call({ tool: 'Bash', command: 'fetch.sh', run_in_background: true });
    const busy = [await scanned(), await scanned()];
    await $.turn.complete(TURN);
    expect([...busy, await scanned(), await scanned()]).toEqual([0, 0, 1, 1]);
    expect(submitted).toEqual([expect.stringContaining('b7k2 repeats "retry #"')]);
});

test('Decide request states run kind and calling agent, and both calls use one server process', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED));
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'Bash', command: 'make', run_in_background: true, timeout: 5000, agentId: 'a1' });
    await $.tool.call({ tool: 'Bash', command: 'make' });
    expect(world.sent).toEqual([
        ['decide', { command: 'make', cwd: SESSION.cwd, kind: 'background', agent: 'a1', ...DECIDED }],
        ['decide', { command: 'make', cwd: SESSION.cwd, kind: 'foreground', ...DECIDED }],
    ]);
    expect([world.spawned, world.stopped].map((streams) => streams.map(([program]) => program))).toEqual([['mise'], []]);
});

test('Command with a failed decision runs as written and notes each agent once per cause', async ($, on) => {
    const causes = ['TypeError: rule threw\n    at decide', 'TypeError: rule threw', 'TypeError: rule threw', 'RangeError: other'].values();
    const world = worldOf(on, () => ({ status: 500, ok: false, headers: {}, text: String(causes.next().value) }));
    const noted = { ...RAN, context: [`${UNJUDGED}decide answered 500, TypeError: rule threw`] };
    await $.session.start(SESSION);
    const calls = [await $.tool.call({ ...LIST, agentId: 'a1' }), await $.tool.call({ ...LIST, agentId: 'a1' }), await $.tool.call({ ...LIST, agentId: 'a2' }), await $.tool.call({ ...LIST, agentId: 'a1' })];
    expect(calls).toEqual([noted, RAN, noted, { ...RAN, context: [`${UNJUDGED}decide answered 500, RangeError: other`] }]);
    expect(world.ran).toHaveLength(calls.length);
});

test('Command runs as written with crash cause noted when server exits before listening', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), { printed: { mise: [{ stream: 'stderr', text: 'file:///work/rules/decide.ts:3\nSyntaxError: Unexpected token\n    at compile\n\nNode.js v26\n' }] } });
    await $.session.start(SESSION);
    expect(await $.tool.call(LIST)).toEqual({ ...RAN, context: [`${UNJUDGED}SyntaxError: Unexpected token`] });
    expect(world.sent).toEqual([]);
});

test('Call after a lost socket stops its server, starts one new server, and gets its decision', async ($, on) => {
    const world = worldOf(on, () => (world.sent.length === 2 ? new Error('socket closed') : answered({ _tag: 'Deny', reason: 'no' } satisfies Decision)));
    await $.session.start(SESSION);
    await $.tool.call(LIST);
    expect(await $.tool.call(LIST)).toEqual({ deny: 'no' });
    expect([world.spawned, world.stopped].map((streams) => streams.map(([program]) => program))).toEqual([['mise', 'mise'], ['mise']]);
});

test('Command runs as written with the run error noted when the watched server fails to start', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), {
        printed: {
            mise: [
                { stream: 'stderr', text: 'file:///work/host.ts:3\nTypeError: host broke\n    at compile\n' },
                { stream: 'stdout', text: "Failed running 'claude/server.ts'. Waiting for file changes before restarting...\n" },
            ],
        },
    });
    await $.session.start(SESSION);
    expect(await $.tool.call(LIST)).toEqual({ ...RAN, context: [`${UNJUDGED}TypeError: host broke`] });
    expect(world.sent).toEqual([]);
});

test('Read of a task output always runs, and Read or Grep of an agent output, transcript, or shell history is refused with its route', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), { agents: [agentOf('running')] });
    const shell = { tool: 'Read', file_path: OUTPUT } as const;
    const records = [
        ['/tmp/claude-501/x/tasks/a46c9759cea20d51a.output', "is a running agent's output that the agent reports when it ends, SendMessage asks it for status"],
        ['/Users/u/.claude/projects/-x/5f1e.jsonl', 'is a session transcript, atuin threads show <session id> --style ndjson reads it'],
        ['/Users/u/.zsh_history', "is shell history, atuin search --filter-mode global --search-mode fulltext '<words>' reads it"],
    ] as const;
    await $.session.start(SESSION);
    const reads = [await $.tool.call(shell), await $.tool.call(shell), ...(await Promise.all(records.map(([file_path]) => $.tool.call({ tool: 'Read', file_path }))))];
    const greps = await $.tool.call({ tool: 'Grep', pattern: 'error', path: '/Users/u/.claude/projects/-x' });
    expect(reads).toEqual([RAN, RAN, ...records.map(([file, route]) => ({ deny: `${file} ${route}` }))]);
    expect(greps).toEqual({ deny: '/Users/u/.claude/projects/-x is a session transcript, atuin threads show <session id> --style ndjson reads it' });
    expect(world.ran).toEqual([shell, shell]);
});
