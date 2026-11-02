import type { Register } from 'claude-code';
import { given, some } from '../composition.ts';
import { commandDecision, unrun, WORKTREE } from '../policies/policies.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ISOLATED = /isolation\s*:\s*['"`]worktree['"`]/u;

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on, options) => {
    const project = String(options.project);
    on('tool.call', { tool: 'EnterWorktree' }, () => ({ deny: `EnterWorktree ${WORKTREE}` }));
    on('tool.call', { tool: 'Agent', isolation: 'worktree' }, () => ({ deny: `Agent with isolation worktree ${WORKTREE}` }));
    on('tool.call', { tool: 'Workflow' }, async ($, e, next) => (ISOLATED.test(e.scriptPath === undefined ? (e.script ?? '') : await $.fs.read(e.scriptPath)) ? { deny: `Workflow agent with isolation worktree ${WORKTREE}` } : next(e)));
    on('tool.call', { tool: ['Bash', 'Monitor'] }, async ($, e, next) => {
        if (e.command === undefined) {
            return next(e);
        }
        const [repo, cwd, home] = await Promise.all([$.session.repo(), $.session.cwd(), $.env.get('HOME')]);
        const decision = await commandDecision(
            { run: (argv, init) => $.process.run(argv, init), exists: (path) => $.fs.exists(path), plugin: $.plugin.root, root: given(repo?.root), cwd, home: given(home) },
            e.command,
            project,
            some(e.tool === 'Bash' ? { background: e.run_in_background === true, timeout: given(e.timeout) } : { background: true, timeout: some(e.timeout_ms) }),
        );
        if (decision.kind === 'deny') {
            return { deny: decision.reason };
        }
        if (decision.kind === 'allow') {
            return next(e);
        }
        const { command, notice, timeout, background } = decision;
        const limit = 600_000;
        const answer = await next({ ...e, command, ...(timeout.kind === 'some' && { timeout: Math.min(timeout.value, limit) }), ...(background && { run_in_background: true }) });
        return notice.kind === 'none' || answer.deny !== undefined ? answer : { ...answer, context: [...(answer.context ?? []), notice.value] };
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: unrun(next.error.message ?? next.error.kind) }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
