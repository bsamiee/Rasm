import type { EngineInterface, ProcessRunInit, Register } from 'claude-code';
import { bind, decoded, fault, map, none, type Option, ok, type Result, rendered, some } from '../composition.ts';
import { SCAN } from '../policies/command.ts';
import { basename, type Invocation } from '../policies/invocation.ts';
import { commandDecision, type Host, WORKTREE } from '../policies/policies.ts';
import { type Catalog, catalog, type Graph } from '../policies/targets.ts';

// --- [MEMOS] ---------------------------------------------------------------------------

const _scanners = new Map<string, Promise<Result<Invocation>>>();
const _environments = new Map<string, Promise<Result<{ readonly NX_WORKSPACE_DATA_DIRECTORY: string }>>>();
const _catalogs = new Map<string, Promise<Result<Catalog>>>();
const _logged = new Set<string>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const _result = <T>(pending: Promise<T>, kind: 'unstarted' | 'unread', subject: string): Promise<Result<T>> => pending.then(ok, (cause: unknown) => fault<T>({ kind, subject, cause }));

const _loaded = <T>(held: Map<string, Promise<Result<T>>>, key: string, load: () => Promise<Result<T>>): Promise<Result<T>> => {
    const loading: Promise<Result<T>> = held.getOrInsertComputed(key, () =>
        load().then((result) => {
            if (result.kind === 'fault' && held.get(key) === loading) {
                held.delete(key);
            }
            return result;
        }),
    );
    return loading;
};

const _exited = async ($: EngineInterface, argv: Invocation, init: ProcessRunInit): Promise<Result<string>> =>
    bind(await _result($.process.run(argv, init), 'unstarted', basename(argv[0])), ({ exitCode, stdout, stderr }) => (exitCode === 0 ? ok(stdout) : fault<string>({ kind: 'exited', subject: basename(argv[0]), code: exitCode, output: stderr.trim() })));

const _scanner = async ($: EngineInterface): Promise<Result<Invocation>> => {
    const [program, ...args] = SCAN;
    const cwd = await $.session.root();
    return _loaded(_scanners, cwd, async () => map(await _exited($, ['mise', 'which', program], { cwd }), (path): Invocation => [path.trim(), ...args]));
};

const _catalog = async ($: EngineInterface): Promise<Option<Catalog>> => {
    const repo = await $.session.repo();
    if (repo === null) {
        return none;
    }
    const environment = await _loaded(_environments, repo.root, async () => decoded('mise env', await _exited($, ['mise', 'env', '--json'], { cwd: repo.root })));
    const loaded = await bind(environment, async ({ NX_WORKSPACE_DATA_DIRECTORY }) => {
        const file = `${NX_WORKSPACE_DATA_DIRECTORY}/project-graph.json`;
        return bind(await _result($.fs.stat(file), 'unread', file), ({ mtimeMs }) => {
            const key = `${file}:${mtimeMs}`;
            _catalogs
                .keys()
                .filter((other) => other !== key)
                .forEach((other) => {
                    _catalogs.delete(other);
                });
            return _loaded(_catalogs, key, async () => map(decoded<Graph>(file, await _result($.fs.read(file), 'unread', file)), (graph) => catalog(graph, repo.root)));
        });
    });
    if (loaded.kind === 'ok') {
        return some(loaded.value);
    }
    const line = `Nx routing off, ${rendered(loaded.faults)}`;
    if (!_logged.has(line)) {
        _logged.add(line);
        $.ui.log(line);
    }
    return none;
};

const _host = ($: EngineInterface): Host => ({
    scan: async (text) => bind(await _scanner($), (argv) => _exited($, argv, { stdin: text })),
    root: () => $.session.repo().then((found) => (found === null ? none : some(found.root))),
    exists: (path) => $.fs.exists(path),
    cwd: () => $.session.cwd(),
    home: () => $.env.get('HOME').then((home) => (home === undefined ? none : some(home))),
    catalog: () => _catalog($),
    read: (path) => $.fs.read(path).then(some, () => none),
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
    }).catch((_$, e, next) => (next.called ? next(e) : { deny: `function-hooks failed, so the command did not run: ${next.error.message ?? next.error.kind}` }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
