import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import { Array, Effect, FileSystem, Layer, Match, Option, Path, Predicate } from 'effect';
import { checkFilesAreInputs, getTaskOutputs } from 'nx/src/hasher/check-task-files';
import { Composed, SLOW, type Stated } from '#/workspace/graph';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SPACE = /\s+/u;
const FLAG = /^--?[\w.-]+=|^['"]|['"]$/gu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const tokens = (value: unknown): readonly string[] =>
    Match.value(value).pipe(
        Match.when(Predicate.isString, (text) => text.split(SPACE)),
        Match.when(Predicate.isObject, (held) => Object.values(held).flatMap(tokens)),
        Match.orElse(() => []),
    );

const unread = Effect.fnUntraced(function* ({ node, task, configuration }: Stated, seed: Parameters<typeof getTaskOutputs>[1]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder: unknown = configuration.options?.cwd;
    const stated = tokens([configuration.options, configuration.configurations]).map((token) => token.replaceAll(FLAG, '').replace('{workspaceRoot}/', '').replace('{projectRoot}', node.data.root));
    const named = Array.dedupe(stated.flatMap((file) => [path.normalize(file), path.normalize(path.join(Predicate.isString(folder) ? folder : '.', file))])).filter((file) => !(file.startsWith('..') || path.isAbsolute(file) || file.includes('node_modules')));
    const files = yield* Effect.filter(named, (file) =>
        Effect.map(
            Effect.option(fs.stat(path.join(workspaceRoot, file))),
            Option.exists(({ type }) => type === 'File'),
        ),
    );
    const { resolved } = yield* Effect.promise(() => getTaskOutputs(task, seed));
    const read = files.filter((file) => !resolved.some((output) => file === output || file.startsWith(`${output}/`)));
    const { unmatched } = Array.isReadonlyArrayNonEmpty(read) ? yield* Effect.promise(() => checkFilesAreInputs(task, read)) : { unmatched: [] };
    return unmatched.map((file) => `${task} ${file}`);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.merge(layer, Composed.layer), { excludeTestServices: true, timeout: SLOW.timeout })('inputs', (methods) => {
    methods.effect(
        'lists as an input of each cacheable target every workspace file its options, command, or configurations name outside its outputs',
        Effect.fnUntraced(function* () {
            const { targets, seed } = yield* Composed;
            const missing = yield* Effect.forEach(
                targets.filter(({ configuration }) => configuration.cache === true),
                (target) => unread(target, seed),
            );
            assert.deepStrictEqual(missing.flat(), [], 'nrwl/nx@2a424d3 astro-docs kb/sandboxing.mdoc, packages/nx/src/hasher/check-task-files.spec.ts, kody-hq/kody@a08d383 tools/nx-cache-contract.node.test.ts');
        }),
        SLOW,
    );
});
