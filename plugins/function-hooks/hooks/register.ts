import type { Hook, NextBudget, Register } from 'claude-code';
import { Option } from '../composition.ts';
import { held, type Instance, identified, launched, logged, recorded, reported } from '../policies/instances.ts';
import { commandDecision, type Request, type System, unrun, WATCHER, WORKTREE } from '../policies/policies.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Host = Parameters<Hook<'session.end'>>[0];

// --- [CONSTANTS] -----------------------------------------------------------------------

const ISOLATED = /isolation\s*:\s*['"`]worktree['"`]/u;
const NEWLINE = /\n$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const requested = (e: { readonly tool: 'Monitor' } | { readonly tool: 'Bash'; readonly run_in_background?: boolean; readonly timeout?: number }): Request => {
    if (e.tool === 'Monitor') {
        return { kind: 'monitor' };
    }
    const timeout = Option.given(e.timeout);
    return e.run_in_background === true ? { kind: 'background' } : { kind: 'foreground', timeout };
};

const located = async (run: System['run'], root: string): Promise<string> => {
    const { exitCode, stdout, stderr } = await run(['mise', 'which', 'ast-grep'], { cwd: root, stdin: '' });
    if (exitCode !== 0) {
        return Promise.reject(new Error(`mise which ast-grep exited ${exitCode}. ${stderr.trim()}`));
    }
    return stdout.trim();
};

const kept = (session: string): string => `instances:${session}`;

const alive = async ($: Host, pids: readonly number[]): Promise<readonly Instance[]> =>
    (
        await Promise.all(
            pids.map(async (pid) => {
                const { exitCode, stdout } = await $.process.run(['lsappinfo', 'info', '-only', 'bundleID', String(pid)]);
                return exitCode === 0
                    ? identified(stdout)
                          .toArray()
                          .map((bundle) => ({ pid, bundle }))
                    : [];
            }),
        )
    ).flat();

const tracked = async ($: Host, agent: string | undefined, created: readonly Instance[], exited: ReadonlySet<number>): Promise<void> => {
    if (created.length + exited.size === 0) {
        return;
    }
    const key = kept(await $.session.id());
    await $.store.set(
        key,
        recorded(
            held(await $.store.get(key)),
            created.map((instance) => ({ ...instance, ...(agent !== undefined && { agent }) })),
            exited,
        ),
    );
};

const released = async ($: Host, instances: readonly Instance[], budget: NextBudget): Promise<void> => {
    const live = await alive(
        $,
        instances.map(({ pid }) => pid),
    );
    const matched = instances.filter((instance) => live.some(({ pid, bundle }) => pid === instance.pid && bundle === instance.bundle));
    const quit = async (targets: readonly Instance[], force: readonly string[]): Promise<ReadonlySet<number>> => {
        const timeoutMs = Math.floor(budget.remainingMs / (force.length === 0 ? 2 : 1));
        const results = await Promise.all(
            targets.map(({ pid }) =>
                $.process.run([WATCHER, 'quit', '--pid', String(pid), ...force], { timeoutMs }).then(
                    ({ exitCode, stdout }) => (exitCode === 0 ? [...reported(stdout).exited] : []),
                    () => [],
                ),
            ),
        );
        return new Set(results.flat());
    };
    const gone = await quit(matched, []);
    await quit(
        matched.filter(({ pid }) => !gone.has(pid)),
        ['--force'],
    );
};

const finished = async ($: Host, agent: string, budget: NextBudget): Promise<void> => {
    const key = kept(await $.session.id());
    const owned = held(await $.store.get(key));
    const spawned = owned.filter((instance) => instance.agent === agent);
    if (spawned.length === 0) {
        return;
    }
    await released($, spawned, budget);
    await $.store.set(
        key,
        held(await $.store.get(key)).filter((instance) => instance.agent !== agent),
    );
};

const ended = async ($: Host, session: string, budget: NextBudget): Promise<void> => {
    const key = kept(session);
    const log = await $.env.get('WATCH_APPS_LOG');
    const text = log !== undefined && (await $.fs.exists(log)) ? await $.fs.read(log) : '';
    await released($, recorded(logged(text, session), held(await $.store.get(key)), new Set()), budget);
    await $.store.delete(key);
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on, options) => {
    const project = String(options.project);
    const scanners = new Map<string, string>();
    on('tool.call', { tool: 'EnterWorktree' }, () => ({ deny: `EnterWorktree ${WORKTREE}` }));
    on('tool.call', { tool: 'Agent', isolation: 'worktree' }, () => ({ deny: `Agent with isolation worktree ${WORKTREE}` }));
    on('tool.call', { tool: 'Workflow' }, async ($, e, next) => (ISOLATED.test(e.scriptPath === undefined ? (e.script ?? '') : await $.fs.read(e.scriptPath)) ? { deny: `Workflow agent with isolation worktree ${WORKTREE}` } : next(e)));
    on('tool.call', { tool: 'mcp__computer-use__open_application' }, (_$, e) => ({ deny: `open_application brings ${String(e.app ?? '<app>')} to front. Use ${WATCHER} launch` }));
    on('tool.call', { tool: ['mcp__atelier__launch', 'mcp__atelier__open'] }, async ($, e, next) => {
        const answer = await next(e);
        await tracked($, e.agentId, await alive($, launched(answer.result)), new Set());
        return answer;
    });
    on('tool.call', { tool: ['Bash', 'Monitor'] }, async ($, e, next) => {
        if (e.command === undefined) {
            return next(e);
        }
        const run: System['run'] = (argv, init) => $.process.run(argv, init);
        const scanner = scanners.get($.plugin.root) ?? (await located(run, $.plugin.root));
        scanners.set($.plugin.root, scanner);
        const [repo, cwd, home] = await Promise.all([$.session.repo(), $.session.cwd(), $.env.get('HOME')]);
        const system: System = {
            run,
            exists: (path) => $.fs.exists(path),
            read: (path) => $.fs.read(path),
            environment: async (name) => {
                const { exitCode, stdout } = await $.process.run(['printenv', name]);
                return exitCode === 0 ? Option.some(stdout.replace(NEWLINE, '')) : Option.none;
            },
            scanner,
            plugin: $.plugin.root,
            root: Option.given(repo?.root),
            cwd,
            home: Option.given(home),
        };
        const decision = await commandDecision(system, e.command, project, requested(e));
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
        const { created, exited } = reported(answer.result);
        await tracked($, e.agentId, created, exited);
        return decision.kind === 'allow' || answer.deny !== undefined
            ? answer
            : decision.notice.match(
                  (notice) => ({ ...answer, context: [...(answer.context ?? []), notice] }),
                  () => answer,
              );
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: unrun(next.error.message ?? next.error.kind) }));
    on('turn.complete', async ($, e, next) => {
        if (e.agentId !== undefined) {
            await finished($, e.agentId, next.budget);
        }
        return next(e);
    });
    on('session.end', async ($, e, next) => {
        await ended($, e.sessionId, next.budget);
        return next(e);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
