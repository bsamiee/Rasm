import { assert } from '@effect/vitest';
import type { ProjectGraph } from '@nx/devkit';
import { graph, hashed } from '@rasm/testing/nx/cli';
import { Array, Effect, Order, Predicate, Record } from 'effect';
import type { Subject } from '#/inference/cases';
import { amended, located, type Methods, placed, present, row, SLOW } from '#/inference/workspaces';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PRODUCTION = { namedInputs: { default: ['{projectRoot}/**/*'], production: ['default', '!{projectRoot}/**/*.test.ts'] } };

// --- [OPERATIONS] ----------------------------------------------------------------------

const reaching = (listed: ProjectGraph, project: string) => Object.keys(listed.nodes).reduce((reached) => Array.dedupe([...reached, ...Record.keys(Record.filter(listed.dependencies, (edges) => edges.some(({ target }) => reached.includes(target))))]), [project]);

const naming = (listed: ProjectGraph, names: readonly string[]) =>
    Object.values(listed.nodes)
        .flatMap((node) =>
            Record.collect(
                Record.filter(node.data.targets ?? {}, ({ inputs = [] }) => Array.intersection(inputs.filter(Predicate.isString), names).length > 0),
                (target) => `${node.name}:${target}`,
            ),
        )
        .toSorted();

const tasks = (subject: Subject, it: Methods) => {
    const live = it.effect.skipIf(!present(subject));

    it.effect.skipIf(!present(subject) || subject.compiled === undefined)(
        'gives each compiled task of a project that reaches a changed project a new hash and leaves the hash of every task that does not list the changed file',
        Effect.fnUntraced(function* () {
            const [file, ...targets] = yield* Effect.fromNullishOr(subject.compiled);
            const text = yield* Effect.fromNullishOr(subject.files[file]);
            const root = yield* placed(subject);
            const [listed, before] = yield* Effect.all([graph(root), hashed(root)]);
            yield* amended(root, file, `${text}\n`);
            const after = yield* hashed(root);
            const [changed] = Object.values(listed.nodes)
                .filter((node) => file.startsWith(`${node.data.root}/`))
                .toSorted(Order.mapInput(Order.flip(Order.Number), (node) => node.data.root.length));
            const dependents = reaching(listed, (yield* Effect.fromNullishOr(changed)).name);
            const compiled = Record.filter(before, ({ project, target }) => dependents.includes(project) && targets.includes(target));
            assert.deepStrictEqual(
                [Record.keys(Record.filter(compiled, ({ hash }, task) => hash === after[task]?.hash)), Record.keys(Record.filter(before, ({ hash, files }, task) => !files.includes(file) && hash !== after[task]?.hash)), dependents.length > 1 && Record.size(compiled) > 1],
                [[], [], true],
                'naxodev/oss@8b43d4a packages/gonx-e2e/src/caching.spec.ts "build cache invalidates when a direct dependency changes", "unrelated project stays cached"',
            );
        }),
        SLOW,
    );

    live(
        'hashes into no task a file of a project nested below the folder of its project',
        Effect.fnUntraced(function* () {
            const owners = located(yield* Effect.flatMap(placed(subject), graph));
            const below = (folder: string) => (folder === '.' ? 'nested' : `${folder}/nested`);
            const nested = Array.dedupe(Object.values(owners)).flatMap((folder, index) => [[`${below(folder)}/project.json`, `{ "name": "nested-${index}" }\n`] as const, [`${below(folder)}/note.txt`, 'note\n'] as const]);
            const hashes = yield* Effect.flatMap(placed(subject, Record.fromEntries(nested)), hashed);
            const read = Record.filter(
                Record.map(hashes, ({ files, project }) => files.filter((file) => file.startsWith(`${below(owners[project] ?? project)}/`))),
                Array.isReadonlyArrayNonEmpty,
            );
            assert.deepStrictEqual(read, {}, 'nrwl/nx@2a424d3 packages/nx/src/hasher/hash-plan-inspector.spec.ts "leaves out the files of a project nested below the project root"');
        }),
        SLOW,
    );

    live(
        'names no production input where nx.json defines none, and where nx.json defines production names no ^default input and reads production, never default, for its build targets',
        Effect.fnUntraced(function* () {
            const plain = yield* Effect.flatMap(placed(subject), graph);
            const defined = yield* Effect.flatMap(placed(subject, {}, { plugins: [row(subject)], ...PRODUCTION }), graph);
            assert.deepStrictEqual(
                [naming(plain, ['production', '^production']), naming(defined, ['^default']), naming(defined, ['default']).filter((task) => task.includes(':build--'))],
                [[], [], []],
                'nrwl/nx@2a424d3 packages/docker/src/plugins/plugin.spec.ts "should create nodes based on Dockerfile" inputs production and ^production, "should use default namedInputs when production is not available", packages/storybook/src/plugins/plugin.spec.ts "should stop using production once nx.json removes it"',
            );
        }),
    );

    it.effect.skipIf(!present(subject) || subject.reads === undefined)(
        'lists as an input of a task each file outside its project folder that its config extends',
        Effect.fnUntraced(function* () {
            const reads = yield* Effect.fromNullishOr(subject.reads);
            const hashes = yield* Effect.flatMap(placed(subject), hashed);
            assert.deepStrictEqual(
                Record.map(reads, (files, task) => Array.difference(files, hashes[task]?.files ?? [])),
                Record.map(reads, () => []),
                'nrwl/nx@2a424d3 packages/vitest/src/plugins/setup-file-inputs.spec.ts, packages/js/src/plugins/typescript/plugin.spec.ts "inputs" extends chain, packages/playwright/src/plugins/plugin.spec.ts "tsconfig inputs"',
            );
        }),
        SLOW,
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { tasks };
