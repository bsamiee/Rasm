import { delimiter, join } from 'node:path';
import { type ProjectGraph, type TargetConfiguration, workspaceRoot } from '@nx/devkit';
import { Completed } from '@rasm/process';
import { inherited } from '@rasm/testing/nx';
import { Array, Effect, FileSystem, Formatter, Option, Path, Record, Schema } from 'effect';
import { ChildProcess } from 'effect/process';
import { createTaskId } from 'nx/src/tasks-runner/utils';
import { inject } from 'vitest';
import { Reading } from '#/reading';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'vitest' {
    interface ProvidedContext {
        readonly readings: { readonly data: string; readonly fresh: string; readonly again: string; readonly cached: string };
    }
}

interface Workspace extends Required<Pick<ProjectGraph, 'nodes' | 'dependencies' | 'externalNodes'>> {
    readonly sourceMaps: (typeof Reading.Type)['sourceMaps'];
    readonly hashes: (typeof Reading.Type)['hashes'];
    readonly failures: (typeof Reading.Type)['failures'];
}

interface Declared {
    readonly project: string;
    readonly root: string;
    readonly name: string;
    readonly task: string;
    readonly plugin: Option.Option<string>;
    readonly target: TargetConfiguration;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const VERB = /(?<=^- `)[a-z]+(?=` )/gmu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const called = Effect.fn('called')(function* (data: string, command: string, args: readonly string[], set: Record.ReadonlyRecord<string, string> = {}) {
    const path = yield* Path.Path;
    const searched = [path.join(workspaceRoot, 'node_modules/.bin'), ...Option.toArray(Record.get(inherited, 'PATH'))].join(delimiter);
    return yield* Completed.run(ChildProcess.make(command, args, { cwd: workspaceRoot, stdin: 'ignore', extendEnv: false, env: { ...inherited, NX_DAEMON: 'false', NX_TUI: 'false', FORCE_COLOR: '0', NX_WORKSPACE_DATA_DIRECTORY: data, PATH: searched, ...set } }));
});

const nx = (args: readonly string[], set: Record.ReadonlyRecord<string, string> = {}) => called(inject('readings').data, 'nx', args, set);

const taken = (data: string, set: Record.ReadonlyRecord<string, string> = {}) => called(data, 'node', [join(import.meta.dirname, 'reading.ts')], set);

const tracked = Effect.fnUntraced(function* (...patterns: readonly string[]) {
    const { checked } = yield* called(inject('readings').data, 'git', ['ls-files', '-z', '--', ...patterns]);
    return (yield* checked).split('\0').filter((file) => file !== '');
});

const read = Effect.fnUntraced(function* (named: 'fresh' | 'again' | 'cached') {
    const fs = yield* FileSystem.FileSystem;
    const reading: Workspace = yield* Effect.flatMap(fs.readFileString(inject('readings')[named]), Schema.decodeEffect(Reading));
    return reading;
});

const declared = ({ nodes, sourceMaps }: Pick<Workspace, 'nodes' | 'sourceMaps'>): readonly Declared[] =>
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

const dependents = ({ dependencies }: Pick<Workspace, 'dependencies'>, projects: ReadonlySet<string>): ReadonlySet<string> => {
    const next = projects.union(new Set(Record.keys(Record.filter(dependencies, (edges) => edges.some(({ target }) => projects.has(target))))));
    return next.size === projects.size ? projects : dependents({ dependencies }, next);
};

const verbs = Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const text = yield* fs.readFileString(path.join(workspaceRoot, 'README.md'));
    return Array.fromIterable(text.slice(text.indexOf('## [04]-[TASKS]'), text.indexOf('Other tasks run through their own commands')).matchAll(VERB)).map(([verb]) => verb);
});

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

export { called, declared, dependents, nx, read, refusals, taken, tracked, verbs, type Workspace };
