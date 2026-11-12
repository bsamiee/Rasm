import type { AgentInfo, HttpResponse, On, ProcessSpawnChunk } from 'claude-code';
import { expect, mock, test } from 'claude-code/testing';
import type { Routes } from '#/server';

// --- [TYPES] ---------------------------------------------------------------------------

type Decision = Routes['decide'][1];
interface Given {
    readonly observe?: Routes['observe'][1];
    readonly watched?: Routes['watched'][1];
    readonly quit?: Routes['quit'][1];
    readonly agents?: readonly AgentInfo[];
    readonly printed?: Readonly<Record<string, readonly ProcessSpawnChunk[]>>;
}
interface Seen {
    readonly sent: (readonly [route: string, body: unknown])[];
    readonly ran: object[];
    readonly spawned: (readonly string[])[];
    readonly runs: (readonly string[])[];
    readonly status: (string | undefined)[];
    readonly stopped: (readonly string[])[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SESSION = { cwd: '/work', surface: 'terminal', isInteractive: true } as const;
const ENDING = { reason: 'clear', sessionId: 'session', resume: { id: 'session' } } as const;
const TURN = { turnId: 't1', answer: '', durationMs: 1, isAborted: false, reason: 'answer' } as const;
const RAN = { result: { stdout: 'ran', stderr: '', interrupted: false }, text: 'ran' } as const;
const NOTE_INTERVAL = 600_000;
const UNJUDGED = 'function-hooks did not judge this command: ';
const ALLOWED = { _tag: 'Allow', observes: false } as const satisfies Decision;
const OBSERVED = { _tag: 'Allow', observes: true } as const satisfies Decision;
const LIST = { tool: 'Bash', command: 'ls' } as const;
const LAUNCH = { tool: 'Bash', command: 'watch-apps launch com.example.app', agentId: 'a1' } as const;
const APP = { pid: 41, instance: '41@7', bundle: 'com.example.app' } as const;
const NONE = { created: [], reused: [], exited: [] } as const satisfies Given['observe'];
const CREATED = { ...NONE, created: [APP] } as const satisfies Given['observe'];
const CONDITION = 'com.example.app 41 shows sheet window';
const NOTE = `${CONDITION}, run watch-apps read 41@7 --window 3`;
const ASKING = [
    { ...APP, ended: false, status: 'responding', asks: true, conditions: [CONDITION], notes: [NOTE] },
    { ...APP, pid: 77, instance: '77@3', ended: false, status: 'responding', asks: true, conditions: ['com.example.app 77 shows alert window'], notes: ['com.example.app 77 shows alert window'] },
] as const satisfies Given['watched'];
const PRINTED: NonNullable<Given['printed']> = { mise: [{ stream: 'stdout', text: '/run/function-hooks/decide.sock\n' }], 'watch-apps': [{ stream: 'stdout', text: '{}\n' }] };

// --- [OPERATIONS] ----------------------------------------------------------------------

const answered = (answer: unknown): HttpResponse => ({ status: 200, ok: true, headers: {}, text: JSON.stringify(answer) });

const agentOf = (status: AgentInfo['status']): AgentInfo => ({ id: 'a1', description: 'launch', type: 'general-purpose', status });

const worldOf = (on: On, decide: () => HttpResponse | Error, given: Given = {}) => {
    const seen: Seen = { sent: [], ran: [], spawned: [], runs: [], status: [], stopped: [] };
    const clock = mock.clock(on);
    const replies = new Map<string, unknown>(Object.entries({ observe: NONE, quit: NONE, watched: [], ...given }));
    const printed = new Map(Object.entries({ ...PRINTED, ...given.printed }));
    on('session.start', (_, e) => ({ cwd: e.cwd }));
    on('session.end', (_, e) => ({ sessionId: e.sessionId }));
    on('session.id', () => ({ value: ENDING.sessionId }));
    on('session.cwd', () => ({ value: SESSION.cwd }));
    on('prompt.submit', (_, e) => ({ text: e.text, ...(e.context !== undefined && { context: e.context }) }));
    on('turn.complete', () => ({ text: '' }));
    on('agent.list', () => ({ value: [...(given.agents ?? [])] }));
    on('ui.log', () => ({ value: undefined }));
    on('ui.status', (_, e) => {
        seen.status.push(e.text);
        return { value: undefined };
    });
    on('process.run', (_, e) => {
        seen.runs.push(e.argv);
        return { value: { exitCode: 0, stdout: '', stderr: '', isStdoutTruncated: false, isStderrTruncated: false } };
    });
    on('process.spawn', async function* (_, e, next) {
        next.signal.addEventListener('abort', () => seen.stopped.push(e.argv), { once: true });
        const chunks = e.argv.slice(0, 1).flatMap((program) => printed.get(program) ?? []);
        const listening = chunks.some(({ stream }) => stream === 'stdout');
        seen.spawned.push(e.argv);
        yield* chunks;
        await (listening ? clock.sleep(Number.MAX_SAFE_INTEGER) : undefined);
        return { value: { code: listening ? 0 : 1, signal: null } };
    });
    on('http.fetch', (_, e) => {
        const route = new URL(e.url).pathname.slice(1);
        seen.sent.push([route, JSON.parse(String(e.init?.body))]);
        const reply = route === 'decide' ? decide() : answered(replies.get(route));
        return reply instanceof Error ? { deny: reply.message } : { value: reply };
    });
    on('tool.call', (_, e) => {
        const { tool_use_id: _id, ...input } = e;
        seen.ran.push(input);
        return RAN;
    });
    return { ...seen, clock };
};

// --- [COMPOSITION] ---------------------------------------------------------------------

(
    [
        { decision: ALLOWED, ran: [LIST], resolved: RAN },
        { decision: { _tag: 'Deny', reason: 'git stash hides work, commit it' }, ran: [], resolved: { deny: 'git stash hides work, commit it' } },
        {
            decision: { _tag: 'Rewrite', command: 'rg -n x', observes: false, background: true, notice: 'grep became rg', timeout: 60_000 },
            ran: [{ tool: 'Bash', command: 'rg -n x', timeout: 60_000, run_in_background: true }],
            resolved: { ...RAN, context: ['grep became rg'] },
        },
    ] satisfies readonly { readonly decision: Decision; readonly ran: readonly object[]; readonly resolved: object }[]
).forEach(({ decision, ran, resolved }) => {
    test(`Bash call resolves by ${decision._tag} decision`, async ($, on) => {
        const world = worldOf(on, () => answered(decision));
        await $.session.start(SESSION);
        expect(await $.tool.call(LIST)).toEqual(resolved);
        expect(world.ran).toEqual(ran);
        expect(world.sent).toEqual([['decide', { command: 'ls', cwd: SESSION.cwd, kind: 'foreground' }]]);
    });
});

test('Rewrite sets only Monitor input fields on a Monitor call', async ($, on) => {
    const world = worldOf(on, () => answered({ _tag: 'Rewrite', command: 'tail -F b.log', observes: false, background: true, timeout: 60_000 } satisfies Decision));
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'Monitor', description: 'log', timeout_ms: 1000, command: 'tail -f a.log' });
    expect(world.ran).toEqual([{ tool: 'Monitor', description: 'log', timeout_ms: 1000, command: 'tail -F b.log' }]);
    expect(world.sent).toEqual([['decide', { command: 'tail -f a.log', cwd: SESSION.cwd, kind: 'monitor' }]]);
});

test('Decide request states run kind, timeout, and calling agent, and both calls use one server process', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED));
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'Bash', command: 'make', run_in_background: true, timeout: 5000, agentId: 'a1' });
    await $.tool.call({ tool: 'Bash', command: 'make' });
    expect(world.sent).toEqual([
        ['decide', { command: 'make', cwd: SESSION.cwd, kind: 'background', timeout: 5000, agent: 'a1' }],
        ['decide', { command: 'make', cwd: SESSION.cwd, kind: 'foreground' }],
    ]);
    expect([world.spawned, world.stopped].map((streams) => streams.map(([program]) => program))).toEqual([['mise'], []]);
});

test('Command with a failed decision runs as written and notes each agent once per interval', async ($, on) => {
    const world = worldOf(on, () => ({ status: 500, ok: false, headers: {}, text: 'TypeError: rule threw\n    at decide' }));
    const noted = { ...RAN, context: [`${UNJUDGED}decide answered 500, TypeError: rule threw`] };
    await $.session.start(SESSION);
    const early = [await $.tool.call({ ...LIST, agentId: 'a1' }), await $.tool.call({ ...LIST, agentId: 'a1' }), await $.tool.call({ ...LIST, agentId: 'a2' })];
    await world.clock.advance(NOTE_INTERVAL);
    expect([...early, await $.tool.call({ ...LIST, agentId: 'a1' })]).toEqual([noted, RAN, noted, noted]);
    expect(world.ran).toHaveLength(early.length + 1);
});

test('Command runs as written with crash cause noted when server exits before listening', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED), { printed: { mise: [{ stream: 'stderr', text: 'file:///work/policies/rules.ts:3\nSyntaxError: Unexpected token\n    at compile\n\nNode.js v26\n' }] } });
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

test('Subagent request_access call is denied and main-loop call runs', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED));
    const access = { tool: 'mcp__computer-use__request_access', apps: ['Notes'] } as const;
    await $.session.start(SESSION);
    expect([await $.tool.call(access), await $.tool.call({ ...access, agentId: 'a1' })]).toEqual([RAN, { deny: expect.stringContaining('list_granted_applications') }]);
    expect(world.ran).toEqual([access]);
});

test('Rhino tool call starts one watch of Rhino', async ($, on) => {
    const world = worldOf(on, () => answered(ALLOWED));
    await $.session.start(SESSION);
    await $.tool.call({ tool: 'mcp__rhino-mcp-platform__get_context' });
    await $.tool.call({ tool: 'mcp__rhino-mcp-platform__get_context' });
    await world.clock.settle();
    expect(world.spawned.slice(1)).toEqual([['watch-apps', 'watch', 'com.mcneel.rhinoceros']]);
});

test('Note of a launched app reaches its launching agent once and status line shows its condition', async ($, on) => {
    const world = worldOf(on, () => answered(world.sent.length === 1 ? OBSERVED : ALLOWED), { observe: CREATED, watched: ASKING });
    await $.session.start(SESSION);
    await $.tool.call(LAUNCH);
    await world.clock.settle();
    const [other, owner, again] = [await $.tool.call({ ...LIST, agentId: 'a2' }), await $.tool.call({ ...LIST, agentId: 'a1' }), await $.tool.call({ ...LIST, agentId: 'a1' })];
    expect([other, owner, again]).toEqual([RAN, { ...RAN, context: [NOTE] }, RAN]);
    expect([world.status.at(-1), world.spawned.at(-1)]).toEqual([CONDITION, ['watch-apps', 'watch', APP.bundle]]);
});

test('Note of an ended agent app reaches the next main-loop prompt', async ($, on) => {
    const world = worldOf(on, () => answered(OBSERVED), { observe: CREATED, watched: ASKING, agents: [agentOf('completed')] });
    await $.session.start(SESSION);
    await $.tool.call(LAUNCH);
    await world.clock.settle();
    expect(await $.prompt.submit({ text: 'next', wait: false, origin: { kind: 'composer' } })).toEqual({ text: 'next', context: [NOTE] });
});

(
    [
        ['running', []],
        ['completed', [['quit', [expect.objectContaining(APP)]]]],
    ] as const
).forEach(([status, quits]) => {
    test(`Turn end posts ${quits.length} quit for apps of a ${status} agent over two turns`, async ($, on) => {
        const world = worldOf(on, () => answered(OBSERVED), { observe: CREATED, quit: { ...NONE, exited: [APP.instance] }, agents: [agentOf(status)] });
        await $.session.start(SESSION);
        await $.tool.call(LAUNCH);
        await $.turn.complete(TURN);
        await $.turn.complete(TURN);
        expect(world.sent.filter(([route]) => route === 'quit')).toEqual(quits);
    });
});

test('Session end quits apps of the ending session in one detached process, once', async ($, on) => {
    const world = worldOf(on, () => answered(OBSERVED), { observe: CREATED });
    await $.session.start(SESSION);
    await $.tool.call(LAUNCH);
    await $.session.end(ENDING);
    await $.session.end(ENDING);
    expect(world.runs).toEqual([expect.arrayContaining([expect.stringContaining('watch-apps quit'), APP.instance])]);
});
