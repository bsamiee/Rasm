import type { Register } from 'claude-code';
import { given, some } from '../composition.ts';
import { commandDecision, type Request, type System, unrun, WORKTREE } from '../policies/policies.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ISOLATED = /isolation\s*:\s*['"`]worktree['"`]/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const requested = (e: { readonly tool: 'Monitor' } | { readonly tool: 'Bash'; readonly run_in_background?: boolean; readonly timeout?: number }): Request => {
    if (e.tool === 'Monitor') {
        return { kind: 'monitor' };
    }
    const timeout = given(e.timeout);
    return e.run_in_background === true ? { kind: 'background' } : { kind: 'foreground', timeout };
};

const located = async (run: System['run'], root: string): Promise<string> => {
    const { exitCode, stdout, stderr } = await run(['mise', 'which', 'ast-grep'], { cwd: root, stdin: '' });
    if (exitCode !== 0) {
        return Promise.reject(new Error(`mise which ast-grep exited ${exitCode}. ${stderr.trim()}`));
    }
    return stdout.trim();
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const register: Register = (on, options) => {
    const project = String(options.project);
    const scanners = new Map<string, string>();
    on('tool.call', { tool: 'EnterWorktree' }, () => ({ deny: `EnterWorktree ${WORKTREE}` }));
    on('tool.call', { tool: 'Agent', isolation: 'worktree' }, () => ({ deny: `Agent with isolation worktree ${WORKTREE}` }));
    on('tool.call', { tool: 'Workflow' }, async ($, e, next) => (ISOLATED.test(e.scriptPath === undefined ? (e.script ?? '') : await $.fs.read(e.scriptPath)) ? { deny: `Workflow agent with isolation worktree ${WORKTREE}` } : next(e)));
    on('tool.call', { tool: ['Bash', 'Monitor'] }, async ($, e, next) => {
        if (e.command === undefined) {
            return next(e);
        }
        const run: System['run'] = (argv, init) => $.process.run(argv, init);
        const scanner = scanners.get($.plugin.root) ?? (await located(run, $.plugin.root));
        scanners.set($.plugin.root, scanner);
        const [repo, cwd, home] = await Promise.all([$.session.repo(), $.session.cwd(), $.env.get('HOME')]);
        const system: System = { run, exists: (path) => $.fs.exists(path), read: (path) => $.fs.read(path), scanner, plugin: $.plugin.root, root: given(repo?.root), cwd, home: given(home) };
        const decision = await commandDecision(system, e.command, project, some(requested(e)));
        if (decision.kind === 'deny') {
            return { deny: decision.reason };
        }
        if (decision.kind === 'allow') {
            return next(e);
        }
        const { command, notice, timeout, background } = decision;
        const answer = await next({ ...e, command, ...(timeout.kind === 'some' && { timeout: timeout.value }), ...(background && { run_in_background: true }) });
        return notice.kind === 'none' || answer.deny !== undefined ? answer : { ...answer, context: [...(answer.context ?? []), notice.value] };
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: unrun(next.error.message ?? next.error.kind) }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
