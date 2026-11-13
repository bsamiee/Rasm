import { assert, it } from '@effect/vitest';
import { Consumer, fromCache } from '@rasm/testing/nx/consumer';
import { NX, README } from '@rasm/testing/nx/exemplars';
import { named, placed, ran } from '@rasm/testing/nx/projects';
import { Array, Duration, Effect, Path } from 'effect';
import { artifacts, CARGO, files, member, workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const bounds = { built: '5 minutes', test: Duration.toMillis('5 minutes') } as const;
const [APP, CORE, GENERATOR] = ['crates/app', 'crates/core', 'tools/generator'] as const;
const SOURCES = { core: 'crates/core/src/lib.rs', generator: 'tools/generator/src/main.rs' } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const warmed = Effect.fnUntraced(function* (verb: string, root: string) {
    const project = yield* named(root);
    yield* ran(verb, project);
    const warm = yield* ran(verb, project);
    return { project, warm, hit: fromCache(`${project}:${verb}--`) };
});

const under = (created: readonly string[], output: string) => created.some((file) => file === output || file.startsWith(`${output}/`));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(workspace(), { timeout: bounds.built, excludeTestServices: true })('@rasm/nx-cargo task cache over a Cargo workspace', (methods) => {
    methods.effect.each([APP, CORE])(
        'restores the build of %s from the cache with every file cargo reports for the package',
        Effect.fnUntraced(function* (root) {
            const [{ cleaned, absent }, { id, name }, project] = yield* Effect.all([Consumer, member(root), named(root)]);
            yield* cleaned;
            const first = yield* ran('build', project, '--skip-nx-cache');
            assert.strictEqual(first.code, 0, first.output);
            const written = yield* artifacts(id, name);
            assert.isNotEmpty(written, `[${CARGO.messages}]`);
            yield* ran('build', project);
            yield* cleaned;
            const second = yield* ran('build', project);
            assert.match(second.output, fromCache(`${project}:build--`), `[${NX.restored}]`);
            assert.deepStrictEqual(yield* absent(written), [], `[${NX.restored}; ${CARGO.messages}]`);
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
            assert.deepStrictEqual(
                declared.flatMap(({ outputPaths }) => outputPaths).filter((output) => !under(created, output)),
                [],
                `[${NX.restored}]`,
            );
        }),
        bounds.test,
    );

    methods.effect(
        'hashes every source file cargo lists in the dep-info of a binary',
        Effect.fnUntraced(function* () {
            const [{ root, text, inputs }, path, { name }, project] = yield* Effect.all([Consumer, Path.Path, member(APP), named(APP)]);
            const built = yield* ran('build', project);
            assert.strictEqual(built.code, 0, built.output);
            const listed = yield* text(`target/debug/${name}.d`);
            const read = listed
                .trim()
                .slice(listed.indexOf(': ') + 2)
                .split(' ')
                .map((file) => path.relative(root, file));
            const { files: hashed } = yield* inputs(`${project}:build`);
            assert.isAbove(read.length, 1, `[${CARGO.depInfo}]`);
            assert.deepStrictEqual(Array.difference(read, hashed), [], `[${NX.read}; ${CARGO.depInfo}]`);
        }),
        bounds.test,
    );

    methods.effect.each(['build', 'test'])(
        'misses the %s cache of a dependent crate after a source edit in a crate it depends on through another',
        Effect.fnUntraced(function* (verb) {
            const { wrote } = yield* Consumer;
            const { project, warm, hit } = yield* warmed(verb, APP);
            assert.match(warm.output, hit, `[${NX.dependent}]`);
            yield* wrote({ [SOURCES.core]: `${files[SOURCES.core]}\npub fn probe_${verb}() -> u32 {\n    7\n}\n` });
            const edited = yield* ran(verb, project);
            assert.strictEqual(edited.code, 0, edited.output);
            assert.notMatch(edited.output, hit, `[${NX.dependent}]`);
        }),
        bounds.test,
    );

    methods.effect.each(['build', 'test'])(
        'hits the %s cache of a crate after a source edit in a crate outside its dependencies',
        Effect.fnUntraced(function* (verb) {
            const { wrote } = yield* Consumer;
            const { project, hit } = yield* warmed(verb, APP);
            yield* wrote({ [SOURCES.generator]: `${files[SOURCES.generator]}\npub fn probe_${verb}() {}\n` });
            const edited = yield* ran(verb, project);
            assert.match(edited.output, hit, `[${NX.unrelated}]`);
        }),
        bounds.test,
    );

    methods.effect.each(['build', 'test'])(
        'hits the %s cache of a crate after a file cargo never reads appears inside it',
        Effect.fnUntraced(function* (verb) {
            const { wrote } = yield* Consumer;
            const { project, hit } = yield* warmed(verb, APP);
            yield* wrote({ [`crates/app/notes-${verb}.md`]: 'not an input\n' });
            const edited = yield* ran(verb, project);
            assert.match(edited.output, hit, `[${NX.unread}; ${CARGO.depInfo}]`);
        }),
        bounds.test,
    );

    methods.effect.each(['--batch', '--no-batch'])(
        'builds every crate with %s',
        Effect.fnUntraced(function* (flag) {
            const [{ cleaned, absent }, { id, name }] = yield* Effect.all([Consumer, member(APP)]);
            yield* cleaned;
            const built = yield* ran('run-many', '--targets=build', '--skip-nx-cache', flag);
            assert.strictEqual(built.code, 0, `[${NX.batch}]: ${built.output}`);
            assert.deepStrictEqual(yield* absent(yield* artifacts(id, name)), [], `[${NX.batch}; ${CARGO.messages}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'fails in one batch only the crate that does not compile, and prints the compiler error',
        Effect.fnUntraced(function* () {
            const [{ wrote }, broken, sound] = yield* Effect.all([Consumer, named(GENERATOR), named(APP)]);
            yield* wrote({ [SOURCES.generator]: 'fn main() {\n    absent_function();\n}\n' });
            const built = yield* ran('run-many', '--targets=build', '--skip-nx-cache', '--batch', '--nx-bail=false');
            yield* wrote({ [SOURCES.generator]: files[SOURCES.generator] });
            const failed = built.output.slice(built.output.indexOf('Failed tasks:'));
            assert.notStrictEqual(built.code, 0, `[${NX.results}]`);
            assert.include(built.output, 'absent_function', `[${NX.results}]`);
            assert.include(failed, `${broken}:build`, `[${NX.results}]`);
            assert.notInclude(failed, `${sound}:build`, `[${NX.results}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'passes the tests of a crate, replays a rerun from the cache, and fails once a test fails',
        Effect.fnUntraced(function* () {
            const [{ wrote }, project] = yield* Effect.all([Consumer, named(CORE)]);
            const first = yield* ran('test', project);
            assert.strictEqual(first.code, 0, `[${NX.tested}]: ${first.output}`);
            const second = yield* ran('test', project);
            assert.match(second.output, fromCache(`${project}:test--`), `[${NX.tested}]`);
            yield* wrote({ [SOURCES.core]: files[SOURCES.core].replace('super::one(), 1', 'super::one(), 2') });
            const failing = yield* ran('test', project);
            yield* wrote({ [SOURCES.core]: files[SOURCES.core] });
            assert.notStrictEqual(failing.code, 0, `[${NX.tested}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'runs the build, test, and check of every crate in one parallel run',
        Effect.fnUntraced(function* () {
            const run = yield* ran('run-many', '--targets=build,test,check', '--skip-nx-cache', '--parallel=8');
            assert.strictEqual(run.code, 0, `[${NX.many}]: ${run.output}`);
        }),
        bounds.test,
    );

    methods.effect(
        'restores a build from the cache after clean removed it',
        Effect.fnUntraced(function* () {
            const [{ absent }, { id, name }, project] = yield* Effect.all([Consumer, member(APP), named(APP)]);
            yield* ran('build', project);
            const written = yield* artifacts(id, name);
            const cleaned = yield* ran('clean', project);
            assert.strictEqual(cleaned.code, 0, `[${NX.cleaned}]: ${cleaned.output}`);
            assert.sameMembers([...(yield* absent(written))], [...written], `[${README.tasks} clean]`);
            const rebuilt = yield* ran('build', project);
            assert.match(rebuilt.output, fromCache(`${project}:build--`), `[${NX.cleaned}]`);
            assert.deepStrictEqual(yield* absent(written), [], `[${NX.cleaned}; ${CARGO.messages}]`);
        }),
        bounds.test,
    );

    methods.effect(
        'installs a binary crate where cargo lists it as installed',
        Effect.fnUntraced(function* () {
            const [{ root, called }, path, { name, version }, project] = yield* Effect.all([Consumer, Path.Path, member(APP), named(APP)]);
            const folder = path.join(root, '.installed');
            const installed = yield* called('env', [`CARGO_INSTALL_ROOT=${folder}`, 'nx', 'install', project, '--output-style=static']);
            const listed = yield* called('cargo', ['install', '--list', '--root', folder]);
            assert.strictEqual(Number(installed.code), 0, `${installed.stdout}${installed.stderr}`);
            assert.include(listed.stdout, `${name} v${version}`, `[${README.tasks} install; ${CARGO.installed}]`);
        }),
        bounds.test,
    );
});
