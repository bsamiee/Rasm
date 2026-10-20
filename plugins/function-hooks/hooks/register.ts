import type { Register } from 'claude-code';
import { none, some } from '../composition.ts';
import { commandDecision, unrun, WORKTREE } from '../policies/policies.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on, options) => {
    const project = String(options.project);
    on('tool.call', { tool: 'EnterWorktree' }, () => ({ deny: `EnterWorktree ${WORKTREE}` }));
    on('tool.call', { tool: 'Agent', isolation: 'worktree' }, () => ({ deny: `Agent with isolation worktree ${WORKTREE}` }));
    on('tool.call', { tool: ['Bash', 'Monitor'] }, async ($, e, next) => {
        if (e.command === undefined) {
            return next(e);
        }
        const [repo, cwd, home] = await Promise.all([$.session.repo(), $.session.cwd(), $.env.get('HOME')]);
        const decision = await commandDecision({ run: (argv, init) => $.process.run(argv, init), exists: (path) => $.fs.exists(path), plugin: $.plugin.root, root: repo === null ? none : some(repo.root), cwd, home: home === undefined ? none : some(home) }, e.command, project);
        switch (decision.kind) {
            case 'deny':
                return { deny: decision.reason };
            case 'allow':
                return next(e);
            default: {
                const { command, notice } = decision;
                const answer = await next({ ...e, command });
                if (notice.kind === 'none' || answer.deny !== undefined) {
                    return answer;
                }
                $.ui.toast(notice.value);
                return { ...answer, context: [...(answer.context ?? []), notice.value] };
            }
        }
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: unrun(next.error.message ?? next.error.kind) }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
