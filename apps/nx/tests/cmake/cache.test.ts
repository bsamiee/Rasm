import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Consumer, fromCache, Packed } from '@rasm/testing/nx';
import { Duration, Effect, Layer } from 'effect';
import { files, members, named, plugin, ran, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ARTIFACTS = 'CMake file API target reply artifacts[].path';
const TASKS = 'README.md TASKS';
const RESTORED = 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts:62-76';
const CLEANED = 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts:423-435';
const MANY = 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts:330-347';
const TESTED = 'nrwl/nx e2e/dotnet/src/dotnet.test.ts:124-128, e2e/dotnet/src/dotnet-cache.test.ts:229-239';
const DEPENDENT = 'nrwl/nx e2e/gradle/src/gradle-task-dependencies.test.ts:160-184';
const UNRELATED = 'nrwl/nx e2e/gradle/src/gradle-task-dependencies.test.ts:186-195';
const UNREAD = 'nrwl/nx e2e/gradle/src/gradle-cache.test.ts:73-76';
const bounds = { built: Duration.minutes(5), test: Duration.toMillis(Duration.minutes(8)) };
const [TOOL, CORE] = ['apps/tool', 'libs/core'] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const member = (root: string) => Effect.flatMap(members, (stated) => Effect.orDie(Effect.fromNullishOr(stated.find((candidate) => candidate.root === root))));

const built = (artifacts: readonly string[], created: readonly string[]) => created.filter((file) => artifacts.some((artifact) => file.endsWith(`/${artifact}`)));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.provideMerge(Packed.layer(plugin.name), layer), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-cmake task cache', (suite) => {
    suite.layer(workspace(), { timeout: bounds.built })('over a CMake tree of nested projects', (methods) => {
        methods.effect.each([TOOL, CORE])(
            'restores the build of %s from the cache with every artifact the file API reports for its targets',
            Effect.fnUntraced(function* (root) {
                const [{ cleaned, absent, written }, { artifacts }, project] = yield* Effect.all([Consumer, member(root), named(root)]);
                yield* cleaned;
                const [first, created] = yield* written(ran('build', project));
                assert.strictEqual(first.code, 0, first.output);
                assert.notMatch(first.output, fromCache(`${project}:build--`), `[${RESTORED}]`);
                const products = built(artifacts, created);
                assert.lengthOf(products, artifacts.length, `[${ARTIFACTS}]: ${created.slice(0, 40).join(', ')}`);
                yield* cleaned;
                const second = yield* ran('build', project);
                assert.match(second.output, fromCache(`${project}:build--`), `[${RESTORED}]`);
                assert.deepStrictEqual(yield* absent(products), [], `[${RESTORED}; ${ARTIFACTS}]`);
            }),
            bounds.test,
        );

        methods.effect.each([
            ['a source', 'libs/core/core.cpp', `${files['libs/core/core.cpp']}\nint probe() { return 7; }\n`],
            ['an included header', 'libs/core/core.hpp', `${files['libs/core/core.hpp']}int probe();\n`],
        ] as const)(
            'misses the build cache of a project after an edit to %s of a project it links through another',
            Effect.fnUntraced(function* ([, file, text]) {
                const [{ wrote }, project] = yield* Effect.all([Consumer, named(TOOL)]);
                yield* ran('build', project);
                const warm = yield* ran('build', project);
                assert.match(warm.output, fromCache(`${project}:build--`), `[${DEPENDENT}]`);
                yield* wrote({ [file]: text });
                const edited = yield* ran('build', project);
                assert.strictEqual(edited.code, 0, edited.output);
                assert.notMatch(edited.output, fromCache(`${project}:build--`), `[${DEPENDENT}]`);
            }),
            bounds.test,
        );

        methods.effect(
            'hits the build cache of a project after a source edit in a project outside its target dependencies',
            Effect.fnUntraced(function* () {
                const [{ wrote }, project] = yield* Effect.all([Consumer, named(CORE)]);
                yield* ran('build', project);
                yield* wrote({ 'apps/tool/main.cpp': `${files['apps/tool/main.cpp']}\nint probe() { return 7; }\n` });
                const edited = yield* ran('build', project);
                assert.match(edited.output, fromCache(`${project}:build--`), `[${UNRELATED}]`);
            }),
            bounds.test,
        );

        methods.effect(
            'hits the build cache of a project after a file no target lists appears inside it',
            Effect.fnUntraced(function* () {
                const [{ wrote }, { sources }, project] = yield* Effect.all([Consumer, member(CORE), named(CORE)]);
                yield* ran('build', project);
                yield* wrote({ 'libs/core/notes.md': 'not an input\n' });
                const edited = yield* ran('build', project);
                assert.notInclude([...sources], 'libs/core/notes.md', '[CMake file API target reply sources[]]');
                assert.match(edited.output, fromCache(`${project}:build--`), `[${UNREAD}]`);
            }),
            bounds.test,
        );

        methods.effect(
            'passes the tests CTest runs for a project, replays a rerun from the cache, and fails once a test fails',
            Effect.fnUntraced(function* () {
                const [{ wrote }, project] = yield* Effect.all([Consumer, named(CORE)]);
                const first = yield* ran('test', project);
                assert.strictEqual(first.code, 0, `[${TESTED}]: ${first.output}`);
                const second = yield* ran('test', project);
                assert.match(second.output, fromCache(`${project}:test--`), `[${TESTED}]`);
                yield* wrote({ 'libs/core/core_test.cpp': files['libs/core/core_test.cpp'].replace('one() == 1', 'one() == 2') });
                const failing = yield* ran('test', project);
                yield* wrote({ 'libs/core/core_test.cpp': files['libs/core/core_test.cpp'] });
                assert.notStrictEqual(failing.code, 0, `[${TESTED}]`);
            }),
            bounds.test,
        );

        methods.effect(
            'runs the build, test, and check of every project in one parallel run',
            Effect.fnUntraced(function* () {
                const run = yield* ran('run-many', '--targets=build,test,check', '--skip-nx-cache', '--parallel=8');
                assert.strictEqual(run.code, 0, `[${MANY}]: ${run.output}`);
            }),
            bounds.test,
        );

        methods.effect(
            'restores a build from the cache after clean removed it',
            Effect.fnUntraced(function* () {
                const [{ absent, written, cleaned: wiped }, { artifacts }, project] = yield* Effect.all([Consumer, member(TOOL), named(TOOL)]);
                yield* wiped;
                const [, created] = yield* written(ran('build', project));
                const products = built(artifacts, created);
                const cleaned = yield* ran('clean', project);
                assert.strictEqual(cleaned.code, 0, `[${CLEANED}]: ${cleaned.output}`);
                assert.sameMembers([...(yield* absent(products))], products, `[${TASKS} clean; ${ARTIFACTS}]`);
                const rebuilt = yield* ran('build', project);
                assert.match(rebuilt.output, fromCache(`${project}:build--`), `[${CLEANED}]`);
                assert.deepStrictEqual(yield* absent(products), [], `[${CLEANED}; ${ARTIFACTS}]`);
            }),
            bounds.test,
        );
    });
});
