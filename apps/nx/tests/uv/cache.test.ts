import { assert, it } from '@effect/vitest';
import { Consumer, fromCache } from '@rasm/testing/nx/consumer';
import { NX, README } from '@rasm/testing/nx/exemplars';
import { named, placed, ran } from '@rasm/testing/nx/projects';
import { Array, Duration, Effect, Path } from 'effect';
import { files, member, UV, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('5 minutes') } as const;
const [CORE, MIDDLE, APP] = ['packages/core', 'packages/middle', 'packages/app'] as const;
const SOURCES = { core: 'packages/core/src/rasm_nx_fixture_core/__init__.py', report: 'tools/report/report.py', tests: 'packages/middle/tests/test_middle.py' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const lacking = Effect.fnUntraced(function* (names: readonly string[], created: readonly string[]) {
    const path = yield* Path.Path;
    return Array.difference(
        names,
        created.map((file) => path.basename(file)),
    );
});

const under = (created: readonly string[], output: string) => created.some((file) => file === output || file.startsWith(`${output}/`));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-uv task cache over a uv workspace', (methods) => {
    methods.effect.each([CORE, APP])(
        'restores the build of %s from the cache with every distribution uv builds for the member',
        Effect.fnUntraced(function* (root) {
            const [{ cleaned, written }, { names }, project] = yield* Effect.all([Consumer, member(root), named(root)]);
            yield* cleaned;
            const first = yield* ran('build', project);
            assert.strictEqual(first.code, 0, first.output);
            assert.notMatch(first.output, fromCache(`${project}:build--`), `[${NX.restored}]`);
            yield* cleaned;
            const [second, created] = yield* written(ran('build', project));
            assert.match(second.output, fromCache(`${project}:build--`), `[${NX.restored}]`);
            assert.deepStrictEqual(yield* lacking(names, created), [], `[${NX.restored}; ${UV.build}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'declares as build outputs only paths the build wrote',
        Effect.fnUntraced(function* () {
            const [{ cleaned, written, outputs }, { targets }] = yield* Effect.all([Consumer, placed]);
            yield* cleaned;
            const [built, created] = yield* written(ran('run-many', '--targets=build', '--skip-nx-cache'));
            assert.strictEqual(built.code, 0, built.output);
            const builds = targets.filter(({ target, verb }) => verb === 'build' && target !== verb);
            const declared = yield* Effect.forEach(builds, ({ project, target }) => outputs(`${project}:${target}`));
            const paths = declared.flatMap(({ outputPaths }) => outputPaths);
            assert.isNotEmpty(paths, `[${NX.restored}]`);
            assert.deepStrictEqual(
                paths.filter((output) => !under(created, output)),
                [],
                `[${NX.restored}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'misses the test cache of a member after a source edit in a member it depends on',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(MIDDLE)]);
            yield* ran('test', project);
            const warm = yield* ran('test', project);
            assert.match(warm.output, fromCache(`${project}:test--`), `[${NX.dependent}]`);
            yield* wrote({ [SOURCES.core]: `${files[SOURCES.core]}\n\ndef probe() -> int:\n    return 7\n` });
            const edited = yield* ran('test', project);
            assert.strictEqual(edited.code, 0, edited.output);
            assert.notMatch(edited.output, fromCache(`${project}:test--`), `[${NX.dependent}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hits the test cache of a member after a source edit in a member outside its dependencies',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(MIDDLE)]);
            yield* ran('test', project);
            yield* wrote({ [SOURCES.report]: `${files[SOURCES.report]}print("probe")\n` });
            const edited = yield* ran('test', project);
            assert.match(edited.output, fromCache(`${project}:test--`), `[${NX.unrelated}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hits the build cache of a member after a file uv packs into no distribution appears inside it',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(CORE)]);
            yield* ran('build', project);
            yield* wrote({ 'packages/core/notes.md': 'not an input\n' });
            const { sources } = yield* member(CORE);
            const edited = yield* ran('build', project);
            assert.notInclude([...sources], 'packages/core/notes.md', `[${UV.sdist}]`);
            assert.match(edited.output, fromCache(`${project}:build--`), `[${NX.unread}; ${UV.sdist}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'passes the tests of a member, replays a rerun from the cache, and fails once a test fails',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(MIDDLE)]);
            const first = yield* ran('test', project);
            assert.strictEqual(first.code, 0, `[${NX.tested}]: ${first.output}`);
            const second = yield* ran('test', project);
            assert.match(second.output, fromCache(`${project}:test--`), `[${NX.tested}]`);
            yield* wrote({ [SOURCES.tests]: files[SOURCES.tests].replace('two() == 2', 'two() == 3') });
            const failing = yield* ran('test', project);
            yield* wrote({ [SOURCES.tests]: files[SOURCES.tests] });
            assert.notStrictEqual(failing.code, 0, `[${NX.tested}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'runs the build, test, and check of every member in one parallel run',
        Effect.fnUntraced(function* () {
            const run = yield* ran('run-many', '--targets=build,test,check', '--skip-nx-cache', '--parallel=8');
            assert.strictEqual(run.code, 0, `[${NX.many}]: ${run.output}`);
        }),
        bounds.test,
    );

    methods.effect(
        'restores a build from the cache after clean removed it',
        Effect.fnUntraced(function* () {
            const [{ written }, { names }, project] = yield* Effect.all([Consumer, member(CORE), named(CORE)]);
            yield* ran('build', project);
            const cleaned = yield* ran('clean', project);
            assert.strictEqual(cleaned.code, 0, `[${NX.cleaned}]: ${cleaned.output}`);
            const [rebuilt, created] = yield* written(ran('build', project));
            assert.match(rebuilt.output, fromCache(`${project}:build--`), `[${NX.cleaned}]`);
            assert.deepStrictEqual(yield* lacking(names, created), [], `[${NX.cleaned}; ${README.tasks} clean; ${UV.build}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'installs a member with a script where uv lists it as an installed tool',
        Effect.fnUntraced(function* () {
            const [{ root, called }, path, { name }, project] = yield* Effect.all([Consumer, Path.Path, member(APP), named(APP)]);
            const folders = [`UV_TOOL_DIR=${path.join(root, '.tools')}`, `UV_TOOL_BIN_DIR=${path.join(root, '.tools/bin')}`];
            const installed = yield* called('env', [...folders, 'nx', 'install', project, '--output-style=static']);
            const listed = yield* called('env', [...folders, 'uv', 'tool', 'list']);
            assert.strictEqual(Number(installed.code), 0, `${installed.stdout}${installed.stderr}`);
            assert.include(listed.stdout, `${name} v0.1.0`, `[${README.tasks} install; ${UV.listed}]`);
        }),
        bounds.test,
    );
});
