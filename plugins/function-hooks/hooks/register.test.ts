import { expect, mock, test } from 'claude-code/testing';

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNJUDGED = /did not judge this command/u;
const RUN = { stdout: 'listed', stderr: '', interrupted: false } as const;
const LISTENING = { stream: 'stdout', text: '/run/decide.sock\n' } as const;
const ALLOWED = { _tag: 'Allow', observes: false } as const;
const DECISIONS = [
    { decision: ALLOWED, settled: { result: RUN, deny: undefined } },
    { decision: { _tag: 'Deny', reason: 'refused by policy' }, settled: { result: undefined, deny: 'refused by policy' } },
] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

test('a Bash call runs unjudged once its hook is out of budget while no decision server can start, and the next call is judged once one listens', { timeoutMs: 30_000 }, async ($, on) => {
    const asked = { requests: 0 };
    mock.store(on);
    on('session.start', (_, e) => ({ cwd: e.cwd }));
    on('session.id', () => ({ value: 'session' }));
    on('session.cwd', () => ({ value: '/' }));
    on('clock.now', () => ({ value: 0 }));
    on('ui.status', () => ({ value: undefined }));
    on('ui.log', () => ({ value: undefined }));
    on('process.spawn', async function* () {
        yield LISTENING;
        return { value: { code: 0, signal: null } };
    });
    on('http.fetch', () => {
        asked.requests += 1;
        return { value: { status: 200, ok: true, headers: {}, text: JSON.stringify(ALLOWED) } };
    });
    on('tool.call', () => ({ result: RUN }));
    const unjudged = await $.tool.call({ tool: 'Bash', command: 'ls' });
    await $.session.start({ cwd: '/', surface: 'terminal', isInteractive: true });
    const judged = await $.tool.call({ tool: 'Bash', command: 'ls' });
    expect([unjudged.result, judged.result]).toEqual([RUN, RUN]);
    expect((unjudged.context ?? []).join('\n')).toMatch(UNJUDGED);
    expect(asked.requests).toBe(1);
});

DECISIONS.forEach(({ decision, settled }) => {
    test(`a Bash call the decision server answers ${decision._tag} settles as decided`, async ($, on) => {
        mock.store(on);
        on('session.start', (_, e) => ({ cwd: e.cwd }));
        on('session.id', () => ({ value: 'session' }));
        on('session.cwd', () => ({ value: '/' }));
        on('ui.status', () => ({ value: undefined }));
        on('process.spawn', async function* () {
            yield LISTENING;
            return { value: { code: 0, signal: null } };
        });
        on('http.fetch', () => ({ value: { status: 200, ok: true, headers: {}, text: JSON.stringify(decision) } }));
        on('tool.call', () => ({ result: RUN }));
        await $.session.start({ cwd: '/', surface: 'terminal', isInteractive: true });
        expect(await $.tool.call({ tool: 'Bash', command: 'ls' })).toEqual(settled);
    });
});
