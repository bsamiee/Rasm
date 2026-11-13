import { assert, it } from '@effect/vitest';
import { Consumer, fromCache } from '@rasm/testing/nx/consumer';
import { NX, README } from '@rasm/testing/nx/exemplars';
import { named, placed, ran } from '@rasm/testing/nx/projects';
import { Duration, Effect } from 'effect';
import { files, MSBUILD, member, members, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('8 minutes') } as const;
const [APP, CORE, MIDDLE, TESTS] = ['src/App', 'src/Core', 'src/Middle', 'tests/Core.Tests'] as const;
const SOURCES = { core: 'src/Core/Numbers.cs', app: 'src/App/Program.cs', tests: 'tests/Core.Tests/NumbersTests.cs' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const under = (created: readonly string[], output: string) => created.some((file) => file === output || file.startsWith(`${output}/`));

const outside = (created: readonly string[], declared: readonly string[]) => created.filter((file) => !(file.includes('/obj/') || declared.some((output) => file === output || file.startsWith(`${output}/`))));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-dotnet task cache over MSBuild projects', (methods) => {
    methods.effect.each([APP, CORE])(
        'restores the build of %s from the cache with the assembly MSBuild targets',
        Effect.fnUntraced(function* (root) {
            const [{ cleaned, absent }, { target }, project] = yield* Effect.all([Consumer, member(root), named(root)]);
            yield* cleaned;
            const first = yield* ran('build', project);
            assert.strictEqual(first.code, 0, first.output);
            assert.notMatch(first.output, fromCache(`${project}:build--`), `[${NX.restored}]`);
            assert.deepStrictEqual(yield* absent([target]), [], `[${MSBUILD.target}]`);
            yield* cleaned;
            const second = yield* ran('build', project);
            assert.match(second.output, fromCache(`${project}:build--`), `[${NX.restored}]`);
            assert.deepStrictEqual(yield* absent([target]), [], `[${NX.restored}; ${MSBUILD.target}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'declares every file a build writes outside obj as an output, and no output the build did not write',
        Effect.fnUntraced(function* () {
            const [{ cleaned, written, outputs }, { targets }] = yield* Effect.all([Consumer, placed]);
            yield* cleaned;
            const [built, created] = yield* written(ran('run-many', '--targets=build', '--skip-nx-cache'));
            assert.strictEqual(built.code, 0, built.output);
            const builds = targets.filter(({ target, verb }) => verb === 'build' && target !== verb);
            const declared = yield* Effect.forEach(builds, ({ project, target }) => outputs(`${project}:${target}`));
            const paths = declared.flatMap(({ outputPaths }) => outputPaths);
            assert.deepStrictEqual(outside(created, paths), [], `[${NX.restored}]`);
            assert.deepStrictEqual(
                paths.filter((output) => !under(created, output)),
                [],
                `[${NX.restored}]`,
            );
        }),
        bounds.test,
    );

    methods.effect.each([
        ['build', APP],
        ['test', TESTS],
    ] as const)(
        'misses the %s cache of %s after a source edit in a project it references',
        Effect.fnUntraced(function* ([verb, root]) {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(root)]);
            yield* ran(verb, project);
            const warm = yield* ran(verb, project);
            assert.match(warm.output, fromCache(`${project}:${verb}--`), `[${NX.mixed}]`);
            yield* wrote({ [SOURCES.core]: files[SOURCES.core].replace('One() => 1;', `One() => 1;\n\n    public static int Probe${verb}() => 7;`) });
            const edited = yield* ran(verb, project);
            assert.strictEqual(edited.code, 0, edited.output);
            assert.notMatch(edited.output, fromCache(`${project}:${verb}--`), `[${NX.mixed}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hits the build cache of a project after a source edit in a project outside its references',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(MIDDLE)]);
            yield* ran('build', project);
            yield* wrote({ [SOURCES.app]: `${files[SOURCES.app]}System.Console.WriteLine("probe");\n` });
            const edited = yield* ran('build', project);
            assert.match(edited.output, fromCache(`${project}:build--`), `[${NX.unrelated}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'hits the build cache of a project after a file outside its Compile items appears inside it',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(CORE)]);
            yield* ran('build', project);
            yield* wrote({ 'src/Core/notes.md': 'not an input\n' });
            const { sources } = yield* member(CORE);
            const edited = yield* ran('build', project);
            assert.notInclude([...sources], 'src/Core/notes.md', `[${MSBUILD.compiled}]`);
            assert.match(edited.output, fromCache(`${project}:build--`), `[${NX.unread}; ${MSBUILD.compiled}]`);
        }),
        bounds.test,
    );

    methods.effect.each(['--batch', '--no-batch'])(
        'builds every project with %s',
        Effect.fnUntraced(function* (flag) {
            const [{ cleaned, absent }, stated] = yield* Effect.all([Consumer, members]);
            yield* cleaned;
            const built = yield* ran('run-many', '--targets=build', '--skip-nx-cache', flag);
            assert.strictEqual(built.code, 0, `[${NX.batch}]: ${built.output}`);
            assert.deepStrictEqual(yield* absent(stated.map(({ target }) => target)), [], `[${NX.batch}; ${MSBUILD.target}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'fails in one batch only the project that does not compile, and prints the compiler error',
        Effect.fnUntraced(function* () {
            const [{ wrote }, broken, sound] = yield* Effect.all([Consumer, named(TESTS), named(APP)]);
            yield* wrote({ [SOURCES.tests]: files[SOURCES.tests].replace('Fixture.Core.Numbers.One()', 'AbsentSymbol.One()') });
            const built = yield* ran('run-many', '--targets=build', '--skip-nx-cache', '--batch', '--nx-bail=false');
            yield* wrote({ [SOURCES.tests]: files[SOURCES.tests] });
            const failed = built.output.slice(built.output.indexOf('Failed tasks:'));
            assert.notStrictEqual(built.code, 0, `[${NX.results}]`);
            assert.include(built.output, 'AbsentSymbol', `[${NX.results}]`);
            assert.include(failed, `${broken}:build`, `[${NX.results}]`);
            assert.notInclude(failed, `${sound}:build`, `[${NX.results}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'passes the tests of a test project, replays a rerun from the cache, and fails once a test fails',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(TESTS)]);
            const first = yield* ran('test', project);
            assert.strictEqual(first.code, 0, `[${NX.tested}]: ${first.output}`);
            const second = yield* ran('test', project);
            assert.match(second.output, fromCache(`${project}:test--`), `[${NX.tested}]`);
            yield* wrote({ [SOURCES.tests]: files[SOURCES.tests].replace('Equal(1,', 'Equal(2,') });
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
            const [{ absent }, { target }, project] = yield* Effect.all([Consumer, member(CORE), named(CORE)]);
            yield* ran('build', project);
            const cleaned = yield* ran('clean', project);
            assert.strictEqual(cleaned.code, 0, `[${NX.cleaned}]: ${cleaned.output}`);
            assert.deepStrictEqual(yield* absent([target]), [target], `[${README.tasks} clean; ${MSBUILD.target}]`);
            const rebuilt = yield* ran('build', project);
            assert.match(rebuilt.output, fromCache(`${project}:build--`), `[${NX.cleaned}]`);
            assert.deepStrictEqual(yield* absent([target]), [], `[${NX.cleaned}; ${MSBUILD.target}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'packs a packable project into the package file NuGet names from its id and version',
        Effect.fnUntraced(function* () {
            const [{ written }, { packageId, version }, project] = yield* Effect.all([Consumer, member(CORE), named(CORE)]);
            const [packed, created] = yield* written(ran('pack', project, '--skip-nx-cache'));
            assert.strictEqual(packed.code, 0, `[${NX.packed}]: ${packed.output}`);
            assert.isNotEmpty(
                created.filter((file) => file.endsWith(`/${packageId}.${version}.nupkg`)),
                `[${NX.packed}; ${MSBUILD.packageFile}]: ${created.join(', ')}`,
            );
        }),
        bounds.test,
    );
});
