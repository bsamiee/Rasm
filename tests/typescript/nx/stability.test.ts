import { homedir, hostname, tmpdir, userInfo } from 'node:os';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import { Array, Duration, Effect, FileSystem, Order, Path, Predicate, Record, Schema } from 'effect';
import { getOutputsForTargetAndConfiguration } from 'nx/src/tasks-runner/utils';
import { inject } from 'vitest';
import { Reading } from '#/reading';
import { declared, read, taken, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    recomputed:
        'nrwl/nx packages/nx/src/project-graph/project-graph.spec.ts: should not throw an error if sane plugins are used and called in parallel; two computations of one tree give one graph; packages/nx/src/project-graph/build-project-graph.ts: createDependencies of every plugin runs in one Promise.all, so sources and the edges under one source keep the order plugins finish in',
    replayed: 'nrwl/nx packages/nx/src/project-graph/nx-deps-cache.spec.ts shouldRecomputeWholeGraph: a cached graph is served only while it equals a recomputed one',
    portable: 'README.md PRINCIPLES: Cached targets with output that differs by platform hash one platform input, since one remote cache serves every runner; a hash or a stored command that names a path of one machine matches on no other runner',
    hashed: 'nrwl/nx packages/nx/src/hasher/native-task-hasher-impl.spec.ts: should create task hash; equal inputs give an equal hash',
    kept: 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesFunction: a plugin reads configuration files and returns project configuration, and graph creation removes no file a task wrote',
} as const;
const LISTED = 50;
const WORD = /[\s'"=]+/u;
const PATTERN = /[*{]/u;
const STAMP = /\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const graphed = ({ nodes, dependencies, externalNodes, sourceMaps, failures }: Workspace) => ({
    nodes,
    externalNodes,
    sourceMaps,
    failures,
    dependencies: Record.fromIterableWith(Array.sort(Record.keys(dependencies), Order.String), (source) => [source, Array.sortWith(dependencies[source] ?? [], ({ target, type }) => `${target} ${type}`, Order.String)]),
});

const unequal = (left: unknown, right: unknown, at: string): readonly string[] =>
    Predicate.isObjectOrArray(left) && Predicate.isObjectOrArray(right) ? Array.dedupe([...Object.keys(left), ...Object.keys(right)]).flatMap((key) => unequal(Reflect.get(left, key), Reflect.get(right, key), `${at}.${key}`)) : [at].filter(() => left !== right);

const reordered = (left: unknown, right: unknown, at: string): readonly string[] =>
    Predicate.isObjectOrArray(left) && Predicate.isObjectOrArray(right) ? [...[at].filter(() => Object.keys(left).join('\0') !== Object.keys(right).join('\0')), ...Object.keys(left).flatMap((key) => reordered(Reflect.get(left, key), Reflect.get(right, key), `${at}.${key}`))] : [];

const strings = (value: unknown, at: string): readonly (readonly [at: string, text: string])[] => (Predicate.isObjectOrArray(value) ? Object.entries(value).flatMap(([key, held]) => strings(held, `${at}.${key}`)) : Array.fromNullishOr(Predicate.isString(value) ? value : undefined).map((text) => [at, text] as const));

const local = ({ nodes, hashes }: Workspace) => {
    const [folders, names] = [
        [workspaceRoot, homedir(), tmpdir()],
        [userInfo().username, hostname()],
    ];
    const inputs = Record.collect(hashes, (task, { inputs: hashed }) => Record.keys(hashed).map((input) => [`hashes.${task}`, input] as const)).flat();
    return [...strings(nodes, 'nodes'), ...inputs].filter(([, text]) => folders.some((folder) => text.includes(folder)) || text.split('/').some((segment) => names.includes(segment)));
};

const outside = (path: Path.Path, bases: readonly string[], held: unknown, task: string) =>
    strings(held, task)
        .flatMap(([at, text]) => text.split(WORD).map((word) => ({ at, word })))
        .filter(({ word }) => (word.split('/').includes('..') || path.isAbsolute(word)) && bases.every((base) => path.relative(workspaceRoot, path.resolve(base, word)).startsWith('..')))
        .map(({ at, word }) => `${at}: ${word}`);

const escaping = (path: Path.Path, workspace: Workspace) =>
    declared(workspace)
        .filter(({ target }) => target.cache === true)
        .flatMap(({ root, task, target }) => [...outside(path, [workspaceRoot], { inputs: target.inputs, outputs: target.outputs }, task), ...outside(path, [workspaceRoot, path.join(workspaceRoot, root)], { options: target.options, configurations: target.configurations }, task)]);

const present = Effect.fnUntraced(function* (workspace: Workspace) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const outputs = declared(workspace).flatMap(({ project, name }) => Array.fromNullishOr(workspace.nodes[project]).flatMap((node) => getOutputsForTargetAndConfiguration({ project, target: name }, {}, node)));
    return yield* Effect.filter(
        Array.dedupe(outputs).filter((output) => !PATTERN.test(output)),
        (output) => fs.exists(path.join(workspaceRoot, output)),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
    suite.effect(
        'computes a byte-equal graph in two processes that share no stored state',
        Effect.fnUntraced(function* () {
            const [fresh, again] = yield* Effect.all([read('fresh'), read('again')]);
            assert.deepStrictEqual(Array.take(unequal(graphed(fresh), graphed(again), 'graph'), LISTED), [], SOURCES.recomputed);
            assert.deepStrictEqual(Array.take(reordered(graphed(fresh), graphed(again), 'graph'), LISTED), [], `${SOURCES.recomputed}: equal values in another key order`);
        }),
    );

    suite.effect(
        'reads the same graph from stored plugin results as it computes without them',
        Effect.fnUntraced(function* () {
            const [fresh, cached] = yield* Effect.all([read('fresh'), read('cached')]);
            assert.deepStrictEqual(Array.take(unequal(graphed(fresh), graphed(cached), 'graph'), LISTED), [], SOURCES.replayed);
            assert.deepStrictEqual(Array.take(reordered(graphed(fresh), graphed(cached), 'graph'), LISTED), [], `${SOURCES.replayed}: equal values in another key order`);
        }),
    );

    suite.effect(
        'gives every task the same hash in two processes that share no stored state',
        Effect.fnUntraced(function* () {
            const [fresh, again] = yield* Effect.all([read('fresh'), read('again')]);
            assert.deepStrictEqual(Array.take(unequal(fresh.hashes, again.hashes, 'hashes'), LISTED), [], SOURCES.hashed);
        }),
    );

    suite.effect(
        'holds no checkout path, home folder, or temporary folder, and no path segment that is the user name or the host name, in any node or hash input',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(Array.take(local(yield* read('fresh')), LISTED), [], SOURCES.portable);
        }),
    );

    suite.effect(
        'names no path outside the workspace in any option, input, or output of a cached target',
        Effect.fnUntraced(function* () {
            const [path, workspace] = yield* Effect.all([Path.Path, read('fresh')]);
            assert.deepStrictEqual(Array.take(escaping(path, workspace), LISTED), [], SOURCES.portable);
        }),
    );

    suite.effect(
        'holds no timestamp in any node',
        Effect.fnUntraced(function* () {
            const { nodes } = yield* read('fresh');
            assert.deepStrictEqual(
                strings(nodes, 'nodes').filter(([, text]) => STAMP.test(text)),
                [],
                SOURCES.recomputed,
            );
        }),
    );

    suite.effect(
        'answers graph reads made at once with the graph one read gives',
        Effect.fnUntraced(function* () {
            const taking = Effect.gen(function* () {
                const { checked } = yield* taken(inject('readings').data);
                const reading: Workspace = yield* Schema.decodeEffect(Reading)(yield* checked);
                return reading;
            });
            const alone = yield* taking;
            const reads = yield* Effect.all([taking, taking, taking], { concurrency: 'unbounded' });
            assert.deepStrictEqual(
                Array.take(
                    reads.flatMap((reading, at) => unequal(graphed(alone), graphed(reading), `graph of read ${at}`)),
                    LISTED,
                ),
                [],
                SOURCES.recomputed,
            );
            assert.deepStrictEqual(
                Array.take(
                    reads.flatMap((reading, at) => unequal(alone.hashes, reading.hashes, `hashes of read ${at}`)),
                    LISTED,
                ),
                [],
                SOURCES.hashed,
            );
        }),
    );

    suite.effect(
        'computes a graph without removing a file any target declares as output',
        Effect.fnUntraced(function* () {
            const [fs, workspace] = yield* Effect.all([FileSystem.FileSystem, read('cached')]);
            const before = yield* present(workspace);
            const { code } = yield* taken(yield* fs.makeTempDirectoryScoped({ prefix: 'nx-reading-' }));
            const after = yield* present(workspace);
            assert.strictEqual(Number(code), 0, SOURCES.kept);
            assert.isNotEmpty(before, 'a built workspace holds declared outputs');
            assert.deepStrictEqual([...new Set(before).difference(new Set(after))], [], SOURCES.kept);
        }, Effect.scoped),
        Duration.toMillis(Duration.minutes(10)),
    );
});
