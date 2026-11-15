import { fileURLToPath } from 'node:url';
import { type TargetConfiguration, workspaceRoot } from '@nx/devkit';
import type { ExitError } from '@rasm/process';
import { environment, exited, type Graph, rows } from '@rasm/testing/nx';
import { Reading } from '@rasm/testing/reading';
import { Array, Effect, FileSystem, Formatter, Option, type PlatformError, Record, Schema } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { readNxJson } from 'nx/src/config/nx-json';
import { createTaskId } from 'nx/src/tasks-runner/utils';
import { inject } from 'vitest';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'vitest' {
    interface ProvidedContext {
        readonly readings: { readonly data: string; readonly fresh: string; readonly again: string; readonly cached: string };
    }
}

interface Declared {
    readonly project: string;
    readonly root: string;
    readonly name: string;
    readonly task: string;
    readonly plugin: Option.Option<string>;
    readonly target: TargetConfiguration;
}

interface Ecosystem {
    readonly sources: readonly string[];
    readonly shared: readonly string[];
    readonly locks: readonly string[];
    readonly versions: Option.Option<RegExp>;
    readonly platform: Option.Option<RegExp>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const NODE = { sources: [], shared: [], locks: ['pnpm-lock.yaml'], versions: Option.none(), platform: Option.none() } as const satisfies Ecosystem;

const ECOSYSTEMS: Record.ReadonlyRecord<string, Ecosystem> = {
    '@rasm/nx-cargo': { sources: ['*.rs'], shared: ['Cargo.lock', 'Cargo.toml', 'rust-toolchain.toml'], locks: ['Cargo.lock', 'Cargo.toml', 'rust-toolchain.toml'], versions: Option.some(/Cargo\.(?:lock|toml)/u), platform: Option.some(/^(?:build|test)--/u) },
    '@rasm/nx-uv': { sources: ['*.py'], shared: [], locks: ['uv.lock'], versions: Option.some(/uv\.lock|pyproject\.toml/u), platform: Option.some(/^test--/u) },
    '@rasm/nx-dotnet': { sources: ['*.cs'], shared: ['Directory.Packages.props'], locks: ['Directory.Packages.props'], versions: Option.some(/Directory\.Packages\.props/u), platform: Option.some(/^test--/u) },
    '@rasm/nx-apple': { sources: ['*.swift', ':(exclude)*Package*.swift'], shared: [], locks: ['Package.resolved'], versions: Option.some(/Package\.(?:resolved|swift)/u), platform: Option.some(/^(?:build|test)--/u) },
    '@rasm/nx-cmake': { sources: ['*.cpp'], shared: [], locks: ['vcpkg.json'], versions: Option.some(/vcpkg\.json/u), platform: Option.some(/^(?:build|test)--/u) },
    '@rasm/nx-distribution': { sources: [], shared: [], locks: [], versions: Option.none(), platform: Option.some(/^(?:build|test)--unreal$/u) },
    '@rasm/nx-typescript': { ...NODE, sources: ['*/index.ts'], shared: ['tsconfig.base.json'], versions: Option.some(/pnpm-workspace\.yaml/u) },
    '@rasm/nx-vite': NODE,
    '@rasm/nx-vitest': NODE,
    '@rasm/nx-storybook': NODE,
};

const plugins = rows(readNxJson(workspaceRoot));

// --- [OPERATIONS] ----------------------------------------------------------------------

const called = (
    data: string,
    argv: Array.NonEmptyReadonlyArray<string>,
    set: Record.ReadonlyRecord<string, string> = {},
): Effect.Effect<{ readonly code: number; readonly stdout: string; readonly stderr: string; readonly checked: Effect.Effect<string, ExitError> }, PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner> =>
    exited(argv, { cwd: workspaceRoot, extendEnv: false, env: environment(workspaceRoot, { NX_WORKSPACE_DATA_DIRECTORY: data, ...set }) });

const nx = (args: readonly string[], set: Record.ReadonlyRecord<string, string> = {}) => called(inject('readings').data, ['nx', ...args], set);

const taken = (data: string, set: Record.ReadonlyRecord<string, string> = {}) => called(data, ['node', fileURLToPath(import.meta.resolve('@rasm/testing/reading'))], set);

const tracked = Effect.fnUntraced(function* (...patterns: readonly string[]) {
    const { checked } = yield* called(inject('readings').data, ['git', 'ls-files', '-z', '--', ...patterns]);
    return (yield* checked).split('\0').filter((file) => file !== '');
});

const read = Effect.fnUntraced(function* (named: 'fresh' | 'again' | 'cached') {
    const fs = yield* FileSystem.FileSystem;
    const reading: Graph = yield* Effect.flatMap(fs.readFileString(inject('readings')[named]), Schema.decodeEffect(Reading));
    return reading;
});

const declared = ({ nodes, sourceMaps }: Pick<Graph, 'nodes' | 'sourceMaps'>): readonly Declared[] =>
    Object.values(nodes).flatMap((node) =>
        Record.collect(node.data.targets ?? {}, (name, target) => ({
            project: node.name,
            root: node.data.root,
            name,
            task: createTaskId(node.name, name, target.defaultConfiguration),
            plugin: Option.map(Option.fromNullishOr(sourceMaps[node.data.root]?.[`targets.${name}`]), ([, plugin]) => plugin),
            target,
        })),
    );

const verbs = (graph: Pick<Graph, 'nodes' | 'sourceMaps'>) => Array.dedupe(declared(graph).flatMap(({ name }) => Array.fromNullishOr(name.includes('--') ? name.split('--')[0] : undefined)));

const dependents = ({ dependencies }: Pick<Graph, 'dependencies'>, projects: ReadonlySet<string>): ReadonlySet<string> => {
    const next = projects.union(new Set(Record.keys(Record.filter(dependencies, (edges) => edges.some(({ target }) => projects.has(target))))));
    return next.size === projects.size ? projects : dependents({ dependencies }, next);
};

const refusals = <A>(items: readonly A[], named: (item: A) => string, attempted: (item: A) => unknown) =>
    Effect.map(
        Effect.forEach(items, (item) =>
            Effect.match(
                Effect.tryPromise(() => Promise.resolve(item).then(attempted)),
                { onSuccess: () => [], onFailure: ({ cause }) => [`${named(item)}: ${Formatter.format(cause)}`] },
            ),
        ),
        Array.flatten,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { called, declared, dependents, ECOSYSTEMS, nx, plugins, read, refusals, taken, tracked, verbs };
