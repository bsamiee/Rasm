import { bind, fault, map, ok, type Result } from './composition.ts';
import { SCAN } from './policies/command.ts';
import { basename, type Invocation } from './policies/invocation.ts';
import type { Host } from './policies/policies.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface System extends Pick<Host, 'root' | 'exists' | 'cwd' | 'home'> {
    readonly run: (argv: Invocation, init: { readonly cwd: string; readonly stdin: string }) => Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _result = <T>(pending: Promise<T>, subject: string): Promise<Result<T>> => pending.then(ok, (cause: unknown) => fault<T>({ kind: 'unstarted', subject, cause }));

const _exited = async (system: System, argv: Invocation, init: { readonly cwd: string; readonly stdin: string }): Promise<Result<string>> =>
    bind(await _result(system.run(argv, init), basename(argv[0])), ({ exitCode, stdout, stderr }) => (exitCode === 0 ? ok(stdout) : fault<string>({ kind: 'exited', subject: basename(argv[0]), code: exitCode, output: stderr.trim() })));

const host = (system: System): Host => ({
    ...system,
    scan: async (text): Promise<Result<string>> => {
        const root = await system.root();
        const cwd = root.kind === 'some' ? root.value : await system.cwd();
        const [program, ...args] = SCAN;
        return bind(
            map(await _exited(system, ['mise', 'which', program], { cwd, stdin: '' }), (path): Invocation => [path.trim(), ...args]),
            (argv) => _exited(system, argv, { cwd, stdin: text }),
        );
    },
});

const unrun = (detail: string): string => `function-hooks policy did not run, ${detail}`;

// --- [EXPORTS] -------------------------------------------------------------------------

export type { System };
export { host, unrun };
