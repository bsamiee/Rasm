import type { EngineInterface, NextBudget, Register } from 'claude-code';
import { Option } from '../monads.ts';
import { type AppInstance, applyChange, bundleId, type Change, createdPids, logInstances, outputChange, storedInstances } from '../policies/apps.ts';
import { commandDecision, policyFailure, type Request, type System, WATCHER, WORKTREE } from '../policies/policies.ts';

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
                return exitCode === 0
                    ? bundleId(stdout)
                          .toArray()
                          .map((bundle) => ({ pid, bundle }))
                    : [];
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

const quitInstances = async ($: EngineInterface, instances: readonly AppInstance[], budget: NextBudget): Promise<void> => {
    const running = await runningInstances(
        $,
        instances.map(({ pid }) => pid),
    );
    const stillRunning = instances.filter((instance) => running.some(({ pid, bundle }) => pid === instance.pid && bundle === instance.bundle));
    const quit = async (targets: readonly AppInstance[], force: readonly string[]): Promise<ReadonlySet<number>> => {
        const timeoutMs = Math.floor(budget.remainingMs / (force.length === 0 ? 2 : 1));
        const results = await Promise.all(
            targets.map(({ pid }) =>
                $.process.run([WATCHER, 'quit', '--pid', String(pid), ...force], { timeoutMs }).then(
                    ({ exitCode, stdout }) => (exitCode === 0 ? [...outputChange(stdout).exited] : []),
                    () => [],
                ),
            ),
        );
        return new Set(results.flat());
    };
    const exited = await quit(stillRunning, []);
    await quit(
        stillRunning.filter(({ pid }) => !exited.has(pid)),
        ['--force'],
    );
};

const quitAgentInstances = async ($: EngineInterface, agent: string, budget: NextBudget): Promise<void> => {
    const key = storeKey(await $.session.id());
    const agentInstances = storedInstances(await $.store.get(key)).filter((instance) => instance.agent === agent);
    if (agentInstances.length === 0) {
        return;
    }
    await quitInstances($, agentInstances, budget);
    await $.store.set(
        key,
        storedInstances(await $.store.get(key)).filter((instance) => instance.agent !== agent),
    );
};

const quitSessionInstances = async ($: EngineInterface, session: string, budget: NextBudget): Promise<void> => {
    const key = storeKey(session);
    const log = await $.env.get('WATCH_APPS_LOG');
    const text = log !== undefined && (await $.fs.exists(log)) ? await $.fs.read(log) : '';
    await quitInstances($, applyChange(logInstances(text, session), { created: storedInstances(await $.store.get(key)), exited: new Set() }), budget);
    await $.store.delete(key);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on, options) => {
    const project = String(options.project);
    const scanners = new Map<string, string>();
    on('tool.call', { tool: 'EnterWorktree' }, (_$, e) => ({ deny: `${e.tool} ${WORKTREE}` }));
    on('tool.call', { tool: 'Agent', isolation: 'worktree' }, (_$, e) => ({ deny: `${e.tool} with isolation ${e.isolation} ${WORKTREE}` }));
    on('tool.call', { tool: 'Workflow' }, async ($, e, next) => (WORKTREE_ISOLATION.test(e.scriptPath === undefined ? (e.script ?? '') : await $.fs.read(e.scriptPath)) ? { deny: `${e.tool} agent with isolation worktree ${WORKTREE}` } : next(e)));
    on('tool.call', { tool: 'mcp__computer-use__open_application' }, (_$, e) => ({ deny: `open_application brings ${String(e.app ?? '<app>')} to front, use ${WATCHER} launch` }));
    on('tool.call', { tool: ['mcp__atelier__launch', 'mcp__atelier__open'] }, async ($, e, next) => {
        const answer = await next(e);
        await recordChange($, e.agentId, { created: await runningInstances($, createdPids(answer.result)), exited: new Set() });
        return answer;
    });
    on('tool.call', { tool: ['Bash', 'Monitor'] }, async ($, e, next) => {
        if (e.command === undefined) {
            return next(e);
        }
        const scanner = scanners.get($.plugin.root) ?? (await scannerPath($));
        scanners.set($.plugin.root, scanner);
        const [repo, cwd, home] = await Promise.all([$.session.repo(), $.session.cwd(), $.env.get('HOME')]);
        const system: System = {
            run: (argv, init) => $.process.run(argv, init),
            exists: (path) => $.fs.exists(path),
            read: (path) => $.fs.read(path),
            environment: async (name) => {
                const { exitCode, stdout } = await $.process.run(['printenv', name]);
                return exitCode === 0 ? Option.some(stdout.replace(NEWLINE, '')) : Option.none;
            },
            scanner,
            plugin: $.plugin.root,
            root: Option.fromNullable(repo?.root),
            cwd,
            home: Option.fromNullable(home),
        };
        const request: Request = e.tool === 'Bash' && e.run_in_background !== true ? { kind: 'foreground', timeout: Option.fromNullable(e.timeout) } : { kind: e.tool === 'Bash' ? 'background' : 'monitor' };
        const decision = await commandDecision(system, e.command, project, request);
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
        await quitSessionInstances($, e.sessionId, next.budget);
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
