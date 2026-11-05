import type { EngineInterface, NextBudget, Register } from 'claude-code';
import { Maybe } from '../monads.ts';
import { type CommandRequest, commandDecision, policyFailure, type System } from '../policies/command-decision.ts';
import { bundleId, WATCHER } from '../policies/internal/apps.ts';
import { WORKTREE } from '../policies/internal/git.ts';
import { type AppInstance, applyChange, type Change, createdPids, detachedQuit, outputChange, storedInstances } from './app-instance.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKTREE_ISOLATION = /isolation\s*:\s*['"`]worktree['"`]/u;
const NEWLINE = /\n$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [SCANNER]

const scannerPath = async ($: EngineInterface): Promise<string> => {
    const argv = ['mise', 'which', 'ast-grep'];
    const { exitCode, stdout, stderr } = await $.process.run(argv, { cwd: $.plugin.root, stdin: '' });
    if (exitCode !== 0) {
        return Promise.reject(new Error(`${argv.join(' ')} exited ${exitCode}. ${stderr.trim()}`));
    }
    return stdout.trim();
};

// --- [INSTANCES]

const storeKey = (session: string): string => `instances:${session}`;

const runningInstances = async ($: EngineInterface, pids: readonly number[]): Promise<readonly AppInstance[]> =>
    (
        await Promise.all(
            pids.map(async (pid) => {
                const { exitCode, stdout } = await $.process.run(['lsappinfo', 'info', '-only', 'bundleID', String(pid)]);
                return exitCode === 0 ? bundleId(stdout).values.map((bundle) => ({ pid, bundle })) : [];
            }),
        )
    ).flat();

const recordChange = async ($: EngineInterface, agent: string | undefined, { created, exited }: Change): Promise<void> => {
    if (created.length + exited.size === 0) {
        return;
    }
    const key = storeKey(await $.session.id());
    await $.store.set(key, applyChange(storedInstances(await $.store.get(key)), { created: created.map((instance) => ({ ...instance, ...(agent !== undefined && { agent }) })), exited }));
};

// --- [QUIT]

const liveInstances = async ($: EngineInterface, instances: readonly AppInstance[]): Promise<readonly AppInstance[]> => {
    const running = await runningInstances(
        $,
        instances.map(({ pid }) => pid),
    );
    return instances.filter((instance) => running.some(({ pid, bundle }) => pid === instance.pid && bundle === instance.bundle));
};

const quitAgentInstances = async ($: EngineInterface, agent: string, budget: NextBudget): Promise<void> => {
    const key = storeKey(await $.session.id());
    const agentInstances = storedInstances(await $.store.get(key)).filter((instance) => instance.agent === agent);
    if (agentInstances.length === 0) {
        return;
    }
    const live = await liveInstances($, agentInstances);
    const timeoutMs = Math.floor(budget.remainingMs);
    const quit = await Promise.all(
        live.map(({ pid }) =>
            $.process.run([WATCHER, 'quit', '--pid', String(pid)], { timeoutMs }).then(
                ({ stdout }) => [...outputChange(stdout).exited],
                () => [],
            ),
        ),
    );
    const gone = agentInstances.filter((instance) => !live.includes(instance)).map(({ pid }) => pid);
    await $.store.set(key, applyChange(storedInstances(await $.store.get(key)), { created: [], exited: new Set([...gone, ...quit.flat()]) }));
};

const quitSessionInstances = async ($: EngineInterface, session: string): Promise<void> => {
    const key = storeKey(session);
    const live = await liveInstances($, storedInstances(await $.store.get(key)));
    if (live.length > 0) {
        await $.process.run(['sh', '-c', detachedQuit(live.map(({ pid }) => pid))], { stdin: '' });
    }
    await $.store.delete(key);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on) => {
    const scanners = new Map<string, Promise<string>>();
    on('tool.call', { tool: 'EnterWorktree' }, (_$, e) => ({ deny: `${e.tool} ${WORKTREE}` }));
    on('tool.call', { tool: 'Agent', isolation: 'worktree' }, (_$, e) => ({ deny: `${e.tool} with isolation ${e.isolation} ${WORKTREE}` }));
    on('tool.call', { tool: 'Workflow' }, async ($, e, next) => (WORKTREE_ISOLATION.test(e.scriptPath === undefined ? (e.script ?? '') : await $.fs.read(e.scriptPath)) ? { deny: `${e.tool} agent with isolation worktree ${WORKTREE}` } : next(e)));
    on('tool.call', { tool: 'mcp__computer-use__request_access' }, (_$, e, next) => (e.agentId === undefined ? next(e) : { deny: 'request_access waits on a person, report pid, window number and capture path to the lead and stop' }));
    on('tool.call', { tool: ['mcp__atelier__launch', 'mcp__atelier__open'] }, async ($, e, next) => {
        const answer = await next(e);
        await recordChange($, e.agentId, { created: await runningInstances($, createdPids(answer.result)), exited: new Set() });
        return answer;
    });
    on('tool.call', { tool: ['Bash', 'Monitor'] }, async ($, e, next) => {
        if (e.command === undefined) {
            return next(e);
        }
        const [scanner, repo, cwd, home] = await Promise.all([scanners.getOrInsertComputed($.plugin.root, () => scannerPath($)), $.session.repo(), $.session.cwd(), $.env.get('HOME')]);
        const system: System = {
            run: (argv, init) => $.process.run(argv, init),
            exists: (path) => $.fs.exists(path),
            read: (path) => $.fs.read(path),
            environment: async (name) => {
                const { exitCode, stdout } = await $.process.run(['printenv', name]);
                return exitCode === 0 ? Maybe.some(stdout.replace(NEWLINE, '')) : Maybe.none;
            },
            scanner,
            plugin: $.plugin.root,
            root: Maybe.fromNullable(repo?.root),
            cwd,
            home: Maybe.fromNullable(home),
        };
        const request: CommandRequest = e.tool === 'Bash' && e.run_in_background !== true ? { kind: 'foreground', timeout: Maybe.fromNullable(e.timeout) } : { kind: e.tool === 'Bash' ? 'background' : 'monitor' };
        const decision = await commandDecision(system, e.command, request);
        if (decision.kind === 'deny') {
            return { deny: decision.reason };
        }
        const answer = await next(
            decision.kind === 'allow'
                ? e
                : {
                      ...e,
                      command: decision.command,
                      ...decision.timeout.match(
                          (timeout) => ({ timeout }),
                          () => ({}),
                      ),
                      ...(decision.background && { run_in_background: true }),
                  },
        );
        await recordChange($, e.agentId, outputChange(answer.result));
        return decision.kind === 'allow' || answer.deny !== undefined
            ? answer
            : decision.notice.match(
                  (notice) => ({ ...answer, context: [...(answer.context ?? []), notice] }),
                  () => answer,
              );
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: policyFailure(next.error.message ?? next.error.kind) }));
    on('turn.complete', async ($, e, next) => {
        if (e.agentId !== undefined) {
            await quitAgentInstances($, e.agentId, next.budget);
        }
        return next(e);
    });
    on('session.end', async ($, e, next) => {
        await quitSessionInstances($, e.sessionId);
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
