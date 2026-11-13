import { homedir, hostname, tmpdir, userInfo } from 'node:os';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import { Array, Effect, FileSystem, Path, Predicate, Record, Schema } from 'effect';
import { getOutputsForTargetAndConfiguration } from 'nx/src/tasks-runner/utils';
import { inject } from 'vitest';
import { called, declared, read, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    recomputed: 'nrwl/nx packages/nx/src/project-graph/project-graph.spec.ts: should not throw an error if sane plugins are used and called in parallel; two computations of one tree give one graph',
    replayed: 'nrwl/nx packages/nx/src/project-graph/nx-deps-cache.spec.ts shouldRecomputeWholeGraph: a cached graph is served only while it equals a recomputed one',
    portable: 'README.md PRINCIPLES: Cached targets with output that differs by platform hash one platform input, since one remote cache serves every runner; a hash or a stored command that names a path of one machine matches on no other runner',
    hashed: 'nrwl/nx packages/nx/src/hasher/native-task-hasher-impl.spec.ts: should create task hash; equal inputs give an equal hash',
    kept: 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesFunction: a plugin reads configuration files and returns project configuration, and graph creation removes no file a task wrote',
} as const;
const LISTED = 50;
const WORD = /[\s'"=]+/u;
const PATTERN = /[*{]/u;
const STAMP = /\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/u;

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const graphed = ({ nodes, dependencies, externalNodes, sourceMaps }: Workspace) => ({ nodes, dependencies, externalNodes, sourceMaps });

const unequal = (left: unknown, right: unknown, at: string): readonly string[] =>
    Predicate.isObject(left) && Predicate.isObject(right) ? Array.dedupe([...Object.keys(left), ...Object.keys(right)]).flatMap((key) => unequal(Reflect.get(left, key), Reflect.get(right, key), `${at}.${key}`)) : [at].filter(() => left !== right);

const strings = (value: unknown, at: string): readonly (readonly [at: string, text: string])[] => (Predicate.isObject(value) ? Object.entries(value).flatMap(([key, held]) => strings(held, `${at}.${key}`)) : Array.fromNullishOr(Predicate.isString(value) ? value : undefined).map((text) => [at, text] as const));

const local = ({ nodes, hashes }: Workspace) => {
    const machine = [workspaceRoot, homedir(), tmpdir(), userInfo().username, hostname()];
    const inputs = Record.collect(hashes, (task, { inputs: hashed }) => Record.keys(hashed).map((input) => [`hashes.${task}`, input] as const)).flat();
    return [...strings(nodes, 'nodes'), ...inputs].filter(([, text]) => machine.some((value) => text.includes(value)));
};

const outside = (path: Path.Path, bases: readonly string[], held: unknown, task: string) =>
    strings(held, task)
        .flatMap(([at, text]) => text.split(WORD).map((word) => ({ at, word })))
        .filter(({ word }) => (word.split('/').includes('..') || path.isAbsolute(word)) && bases.every((base) => path.relative(workspaceRoot, path.resolve(base, word)).startsWith('..')))
        .map(({ at, word }) => `${at}: ${word}`);

const escaping = (path: Path.Path, workspace: Workspace) =>
    declared(workspace).flatMap(({ root, task, target }) => [...outside(path, [workspaceRoot], { inputs: target.inputs, outputs: target.outputs }, task), ...outside(path, [workspaceRoot, path.join(workspaceRoot, root)], { options: target.options, configurations: target.configurations }, task)]);

const present = Effect.fnUntraced(function* (workspace: Workspace) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const outputs = declared(workspace).flatMap(({ project, name }) => Array.fromNullishOr(workspace.nodes[project]).flatMap((node) => getOutputsForTargetAndConfiguration({ project, target: name }, {}, node)));
    return yield* Effect.filter(
        Array.dedupe(outputs).filter((output) => !PATTERN.test(output)),
        (output) => fs.exists(path.join(workspaceRoot, output)),
    );
});

const taken = Effect.fnUntraced(function* (data: string) {
    const path = yield* Path.Path;
    return yield* called(data, 'node', [path.join(import.meta.dirname, 'reading.ts')]);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('graph stability', (suite) => {
    suite.effect(
        'computes a byte-equal graph in two processes that share no stored state',
        Effect.fnUntraced(function* () {
            const [fresh, again] = yield* Effect.all([read('fresh'), read('again')]);
            assert.deepStrictEqual(Array.take(unequal(graphed(fresh), graphed(again), 'graph'), LISTED), [], SOURCES.recomputed);
            assert.isTrue((yield* Schema.encodeEffect(Json)(graphed(fresh))) === (yield* Schema.encodeEffect(Json)(graphed(again))), `${SOURCES.recomputed}: equal values in another key order`);
        }),
    );

    suite.effect(
        'reads the same graph from stored plugin results as it computes without them',
        Effect.fnUntraced(function* () {
            const [fresh, cached] = yield* Effect.all([read('fresh'), read('cached')]);
            assert.deepStrictEqual(Array.take(unequal(graphed(fresh), graphed(cached), 'graph'), LISTED), [], SOURCES.replayed);
            assert.isTrue((yield* Schema.encodeEffect(Json)(graphed(fresh))) === (yield* Schema.encodeEffect(Json)(graphed(cached))), `${SOURCES.replayed}: equal values in another key order`);
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
        'holds no checkout path, home folder, temporary folder, user name, or host name in any node or hash input',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(Array.take(local(yield* read('fresh')), LISTED), [], SOURCES.portable);
        }),
    );

    suite.effect(
        'names no path outside the workspace in any target option, input, or output',
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
            const fs = yield* FileSystem.FileSystem;
            const cached = yield* fs.readFileString(inject('readings').cached);
            const reads = yield* Effect.forEach(['first', 'second', 'third'], () => taken(inject('readings').data), { concurrency: 'unbounded' });
            assert.deepStrictEqual(
                reads.map(({ code, stdout }) => Number(code) === 0 && stdout === cached),
                [true, true, true],
                SOURCES.recomputed,
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
    );
});
