import { bind, decoded, fault, map, none, type Option, ok, type Result, rendered, some } from './composition.ts';
import { SCAN } from './policies/command.ts';
import { basename, type Invocation } from './policies/invocation.ts';
import type { Host } from './policies/policies.ts';
import { type Catalog, catalog, type Graph } from './policies/targets.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface System extends Pick<Host, 'root' | 'exists' | 'cwd' | 'home'> {
    readonly run: (argv: Invocation, init: { readonly cwd: string; readonly stdin: string }) => Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }>;
    readonly read: (path: string) => Promise<string>;
    readonly stat: (path: string) => Promise<{ readonly mtimeMs: number }>;
    readonly log: (line: string) => void;
}

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

const _exited = async (system: System, argv: Invocation, init: { readonly cwd: string; readonly stdin: string }): Promise<Result<string>> =>
    bind(await _result(system.run(argv, init), 'unstarted', basename(argv[0])), ({ exitCode, stdout, stderr }) => (exitCode === 0 ? ok(stdout) : fault<string>({ kind: 'exited', subject: basename(argv[0]), code: exitCode, output: stderr.trim() })));

const _catalog = async (system: System, root: string): Promise<Result<Catalog>> =>
    bind(await _loaded(_environments, root, async () => decoded('mise env', await _exited(system, ['mise', 'env', '--json'], { cwd: root, stdin: '' }))), async ({ NX_WORKSPACE_DATA_DIRECTORY }) => {
        const file = `${NX_WORKSPACE_DATA_DIRECTORY}/project-graph.json`;
        return bind(await _result(system.stat(file), 'unread', file), ({ mtimeMs }) => {
            const key = `${file}:${mtimeMs}`;
            _catalogs
                .keys()
                .filter((other) => other !== key)
                .forEach((other) => {
                    _catalogs.delete(other);
                });
            return _loaded(_catalogs, key, async () => map(decoded<Graph>(file, await _result(system.read(file), 'unread', file)), (graph) => catalog(graph, root)));
        });
    });

const host = (system: System): Host => ({
    ...system,
    scan: async (text): Promise<Result<string>> => {
        const root = await system.root();
        const cwd = root.kind === 'some' ? root.value : await system.cwd();
        const [program, ...args] = SCAN;
        return bind(await _loaded(_scanners, cwd, async () => map(await _exited(system, ['mise', 'which', program], { cwd, stdin: '' }), (path): Invocation => [path.trim(), ...args])), (argv) => _exited(system, argv, { cwd, stdin: text }));
    },
    catalog: async (): Promise<Option<Catalog>> => {
        const root = await system.root();
        if (root.kind === 'none') {
            return none;
        }
        const loaded = await _catalog(system, root.value);
        if (loaded.kind === 'ok') {
            return some(loaded.value);
        }
        const line = `Nx routing off, ${rendered(loaded.faults)}`;
        if (!_logged.has(line)) {
            _logged.add(line);
            system.log(line);
        }
        return none;
    },
    read: (path) => system.read(path).then(some, () => none),
});

const unrun = (detail: string): string => `function-hooks policy did not run, ${detail}`;

// --- [EXPORTS] -------------------------------------------------------------------------

export type { System };
export { host, unrun };
