import type { EngineInterface, Register } from 'claude-code';
import { none, some } from '../composition.ts';
import { host, unrun } from '../host.ts';
import { commandDecision, type Host, WORKTREE } from '../policies/policies.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _host = ($: EngineInterface): Host =>
    host({
        run: (argv, init) => $.process.run(argv, init),
        read: (path) => $.fs.read(path),
        stat: (path) => $.fs.stat(path),
        exists: (path) => $.fs.exists(path),
        root: () => $.session.repo().then((found) => (found === null ? none : some(found.root))),
        cwd: () => $.session.cwd(),
        home: () => $.env.get('HOME').then((home) => (home === undefined ? none : some(home))),
        log: (line) => $.ui.log(line),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on, options) => {
    const project = String(options.project);
    on('tool.call', { tool: 'EnterWorktree' }, () => ({ deny: `EnterWorktree ${WORKTREE}` }));
    on('tool.call', { tool: 'Agent', isolation: 'worktree' }, () => ({ deny: `Agent with isolation worktree ${WORKTREE}` }));
    on('tool.call', { tool: ['Bash', 'Monitor'] }, async ($, e, next) => {
        if (e.command === undefined) {
            return next(e);
        }
        const decision = await commandDecision(_host($), e.command, project);
        if (decision.kind === 'deny') {
            return { deny: decision.reason };
        }
        if (decision.rewrite.kind === 'none') {
            return next(e);
        }
        const { command, notice } = decision.rewrite.value;
        if (notice.kind === 'none') {
            return next({ ...e, command });
        }
        $.ui.toast(notice.value);
        const answer = await next({ ...e, command });
        return answer.deny === undefined ? { ...answer, context: [...(answer.context ?? []), notice.value] } : answer;
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: unrun(next.error.message ?? next.error.kind) }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
