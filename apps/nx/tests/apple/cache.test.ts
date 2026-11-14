import { assert, it } from '@effect/vitest';
import { Consumer, fromCache } from '@rasm/testing/nx';
import { NX, README } from '@rasm/testing/nx/exemplars';
import { named, placed, ran, swept } from '@rasm/testing/nx/projects';
import { Array, Duration, Effect, Path } from 'effect';
import { APPLE, files, members, settings, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('10 minutes') } as const;
const [TOOL, CORE, VIEWER] = ['apps/Tool', 'libs/Core', 'apps/Viewer'] as const;
const SOURCES = { core: 'libs/Core/Core.swift', tool: 'apps/Tool/main.swift', tests: 'libs/tests/CoreTests/CoreTests.swift' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const executable = Effect.gen(function* () {
    const [{ root, called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const shown = yield* called('swift', ['build', '--show-bin-path']);
    const folder = yield* Effect.orDie(Effect.fromOption(Array.last(shown.stdout.trim().split('\n'))));
    return path.join(path.relative(root, folder), 'Tool');
});

const tested = Effect.gen(function* () {
    const { held } = yield* placed;
    const root = yield* Effect.orDie(Effect.fromOption(Array.head(held('test'))));
    return yield* named(root);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-apple task cache over a Swift package and an Xcode project', (methods) => {
    methods.effect(
        'restores the build of an executable module from the cache with the binary SwiftPM places in its bin path',
        Effect.fnUntraced(function* () {
            const [{ cleaned, absent }, project] = yield* Effect.all([Consumer, named(TOOL)]);
            yield* cleaned;
            const first = yield* ran('build', project, '--skip-nx-cache');
            assert.strictEqual(first.code, 0, first.output);
            const binary = yield* executable;
            assert.deepStrictEqual(yield* absent([binary]), [], `[${APPLE.bin}]`);
            yield* ran('build', project);
            yield* cleaned;
            const second = yield* ran('build', project);
            assert.match(second.output, fromCache(`${project}:build--`), `[${NX.restored}]`);
            assert.deepStrictEqual(yield* absent([binary]), [], `[${NX.restored}; ${APPLE.bin}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'restores the build of an Xcode project from the cache with the product xcodebuild names',
        Effect.fnUntraced(function* () {
            const [{ cleaned, absent, written }, { product }, project] = yield* Effect.all([Consumer, settings('apps/Viewer/Viewer.xcodeproj', 'Viewer', 'Release'), named(VIEWER)]);
            yield* cleaned;
            const [first, created] = yield* written(ran('build', project, '--skip-nx-cache'));
            assert.strictEqual(first.code, 0, first.output);
            const products = created.filter((file) => file.endsWith(`/${product}`));
            assert.isNotEmpty(products, `[${APPLE.settings} FULL_PRODUCT_NAME]`);
            yield* ran('build', project);
            yield* cleaned;
            const second = yield* ran('build', project);
            assert.match(second.output, fromCache(`${project}:build--`), `[${NX.restored}]`);
            assert.deepStrictEqual(yield* absent(products), [], `[${NX.restored}; ${APPLE.settings} FULL_PRODUCT_NAME]`);
        }),
        bounds.test,
    );

    methods.effect(
        'misses the build cache of an executable module after a source edit in a module it depends on through another',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(TOOL)]);
            yield* ran('build', project);
            const warm = yield* ran('build', project);
            assert.match(warm.output, fromCache(`${project}:build--`), `[${NX.dependent}]`);
            yield* wrote({ [SOURCES.core]: `${files[SOURCES.core]}public func probe() -> Int { 7 }\n` });
            const edited = yield* ran('build', project);
            assert.strictEqual(edited.code, 0, edited.output);
            assert.notMatch(edited.output, fromCache(`${project}:build--`), `[${NX.dependent}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hits the build cache of a module after a source edit in a module outside its dependencies',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(CORE)]);
            yield* ran('build', project);
            yield* wrote({ [SOURCES.tool]: `${files[SOURCES.tool]}print("probe")\n` });
            const edited = yield* ran('build', project);
            assert.match(edited.output, fromCache(`${project}:build--`), `[${NX.unrelated}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hits the build cache of the Xcode project after a file outside its sources appears beside it',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project, stated] = yield* Effect.all([Consumer, named(VIEWER), members]);
            yield* ran('build', project);
            yield* wrote({ 'apps/Viewer/notes.md': 'not an input\n' });
            const edited = yield* ran('build', project);
            assert.notInclude(
                stated.flatMap(({ sources }) => sources),
                'apps/Viewer/notes.md',
                `[${APPLE.described} targets[].sources]`,
            );
            assert.match(edited.output, fromCache(`${project}:build--`), `[${NX.unread}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'passes the tests SwiftPM runs for a test target, replays a rerun from the cache, and fails once a test fails',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, tested]);
            const first = yield* ran('test', project);
            assert.strictEqual(first.code, 0, `[${NX.tested}]: ${first.output}`);
            const second = yield* ran('test', project);
            assert.match(second.output, fromCache(`${project}:test--`), `[${NX.tested}]`);
            yield* wrote({ [SOURCES.tests]: files[SOURCES.tests].replace('one() == 1', 'one() == 2') });
            const failing = yield* ran('test', project);
            yield* wrote({ [SOURCES.tests]: files[SOURCES.tests] });
            assert.notStrictEqual(failing.code, 0, `[${NX.tested}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'runs the build, test, and check of every project in one parallel run',
        Effect.fnUntraced(function* () {
            const run = yield* ran('run-many', '--targets=build,test,check', '--skip-nx-cache', '--parallel=8');
            assert.strictEqual(run.code, 0, `[${NX.many}]: ${run.output}`);
        }),
        bounds.test,
    );

    methods.effect(
        'restores a build from the cache after clean removed it',
        Effect.fnUntraced(function* () {
            const [{ absent }, project] = yield* Effect.all([Consumer, named(TOOL)]);
            yield* ran('build', project);
            const binary = yield* executable;
            const cleaned = yield* ran('clean', project);
            assert.strictEqual(cleaned.code, 0, `[${NX.cleaned}]: ${cleaned.output}`);
            assert.deepStrictEqual(yield* absent([binary]), [binary], `[${README.cleaned}; ${APPLE.bin}]`);
            const rebuilt = yield* ran('build', project);
            assert.match(rebuilt.output, fromCache(`${project}:build--`), `[${NX.cleaned}]`);
            assert.deepStrictEqual(yield* absent([binary]), [], `[${NX.cleaned}; ${APPLE.bin}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'removes with clean every file a build and test run wrote, the state files SwiftPM keeps directly inside .build excepted',
        Effect.fnUntraced(function* () {
            const { run, clean, created, left } = yield* swept('build', 'test');
            assert.strictEqual(run.code, 0, run.output);
            assert.isNotEmpty(created, `[${APPLE.bin}]`);
            assert.strictEqual(clean.code, 0, `[${README.cleaned}]: ${clean.output}`);
            assert.deepStrictEqual(
                left.filter((file) => !/^\.build\/[^/]+$/u.test(file)),
                [],
                `[${README.cleaned}; ${APPLE.cleaned}]`,
            );
        }),
        bounds.test,
    );
});
