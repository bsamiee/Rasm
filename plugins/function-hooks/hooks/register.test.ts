import type { AgentInfo, HttpResponse, On, ProcessSpawnChunk } from 'claude-code';
import { expect, mock, test } from 'claude-code/testing';
import type { Routes } from '#/claude/routes';

// --- [TYPES] ---------------------------------------------------------------------------

type Decision = Routes['decide'][1];
interface Given {
    readonly agents?: readonly AgentInfo[];
    readonly printed?: Readonly<Record<string, readonly ProcessSpawnChunk[]>>;
    readonly held?: readonly string[];
}
interface Seen {
    readonly sent: (readonly [route: string, body: unknown])[];
    readonly ran: object[];
    readonly spawned: (readonly string[])[];
    readonly stopped: (readonly string[])[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SESSION = { cwd: '/work', surface: 'terminal', isInteractive: true } as const;
const SESSION_ID = 'session';
const RAN = { result: { stdout: 'ran', stderr: '', interrupted: false }, text: 'ran' } as const;
const UNJUDGED = 'function-hooks did not judge this command: Error: ';
const ALLOWED = { _tag: 'Allow' } as const satisfies Decision;
const LIST = { tool: 'Bash', command: 'ls' } as const;
const DECIDED = { session: SESSION_ID, call: expect.any(String) } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const answered = (answer: unknown): HttpResponse => ({ status: 200, ok: true, headers: {}, text: JSON.stringify(answer) });

const agentOf = (status: Exclude<AgentInfo['status'], 'waiting' | 'idle'>): AgentInfo => ({
    id: 'a1',
    description: 'launch',
    type: 'general-purpose',
    status,
    kind: 'local_agent',
    taskStatus: status,
    label: 'launch',
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
    on('session.id', () => ({ value: SESSION_ID }));
    on('session.cwd', () => ({ value: SESSION.cwd }));
    on('agent.list', () => ({ value: [...(given.agents ?? [])] }));
    on('env.get', () => ({ value: '/tmp/' }));
    on('ui.log', () => ({ value: undefined }));
    on('process.run', (_, e) => {
        const holding = e.argv[0] === 'lsof' && given.held?.includes(e.argv.at(-1) ?? '') === true;
        return { value: { exitCode: 0, stdout: holding ? '4242\n' : '', stderr: '', isStdoutTruncated: false, isStderrTruncated: false } };
    });
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
        const reply = decide();
        return Error.isError(reply) ? { deny: reply.message } : { value: reply };
    });
    on('tool.call', (_, e) => {
        const { tool_use_id: _id, ...input } = e;
        seen.ran.push(input);
        return RAN;
    });
    return seen;
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
    ] satisfies readonly { readonly decision: Decision; readonly ran: readonly object[]; readonly resolved: object }[]
).forEach(({ decision, ran, resolved }) => {
    test(`Bash call resolves by ${decision._tag} decision`, async ($, on) => {
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

test('Read of a running task output runs once per agent, a running agent output never, and an ended task output always', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), { agents: [agentOf('running')], held: ['/tmp/claude-501/x/tasks/b7k2.output'] });
    const shell = { tool: 'Read', file_path: '/tmp/claude-501/x/tasks/b7k2.output' } as const;
    const ended = { tool: 'Read', file_path: '/tmp/claude-501/x/tasks/b9z1.output' } as const;
    await $.session.start(SESSION);
    const reads = [await $.tool.call(shell), await $.tool.call(shell), await $.tool.call({ ...shell, agentId: 'a2' }), await $.tool.call({ tool: 'Read', file_path: '/tmp/claude-501/x/tasks/a1.output' }), await $.tool.call(ended), await $.tool.call(ended)];
    expect(reads).toEqual([RAN, { deny: 'Task b7k2 notifies on completion, never poll its output' }, RAN, { deny: 'Task a1 notifies on completion, never poll its output' }, RAN, RAN]);
    expect(world.ran).toEqual([shell, { ...shell, agentId: 'a2' }, ended, ended]);
});
