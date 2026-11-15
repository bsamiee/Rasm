import { platform } from 'node:process';
import { isDeepStrictEqual } from 'node:util';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { type Graph, recorded } from '@rasm/testing/nx';
import { Array, Cause, Duration, Effect, FileSystem, Option, Order, Path, Predicate, Record } from 'effect';
import { minimatch } from 'minimatch';
import { getOutputsForTargetAndConfiguration, validateOutputs } from 'nx/src/tasks-runner/utils';
import { inject } from 'vitest';
import { called, declared, dependents, ECOSYSTEMS, plugins, read, refusals, tracked } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    valid: 'nrwl/nx packages/nx/src/tasks-runner/utils.spec.ts validateOutputs: should throw an error if outputs is not an array of strings prefixed with {workspaceRoot} or {projectRoot}',
    resolved: 'nrwl/nx packages/nx/src/tasks-runner/utils.spec.ts getOutputsForTargetAndConfiguration: should interpolate {workspaceRoot}, {projectRoot}, {projectName}, and {options.*}',
    placed: 'README.md PRINCIPLES: Caches sit under `.cache/<tool>` and generated output under `.artifacts/<tool>`, `dist`, `.cloudflare/output`, and editor-shared folders stay where their tool puts them; `cf workers types` writes the binding types an editor reads to `.cloudflare/types`, and SwiftPM writes `.build` beside `Package.swift`, which SourceKit-LSP shares and no configuration file moves',
    restored: 'nrwl/nx astro-docs/src/content/docs/features/cache-task-results.mdoc: On a cache hit, Nx restores the cached output files, so an output git tracks is overwritten by a replay',
    apart: 'nrwl/nx e2e/nx/src/cache.test.ts: should cache command execution; a restored output holds the files of one task alone',
    platform:
        'README.md PRINCIPLES: Cached targets with output that differs by platform hash one platform input, since one remote cache serves every runner; builds and tests of Cargo, Swift, CMake, and Unreal and test results of .NET and Python differ by platform, output of tsc, Vite, Vitest, and Storybook does not; Suites reading the project graph take tag `os:all` and hash one platform input',
    read: "README.md PRINCIPLES: Task inputs hold the files a task's tool reads: plugin and executor source, `.config/mise.lock`, tool `--version` output, and credentials are no input",
    keyed: 'README.md OWNERS: Mac app release form — Doppler `rasm/release` `ASC_API_*` rows, set notarize a `.dmg` for Sparkle, absent release a `.zip`; a key identifier that selects the form of an output is no credential',
    versioned:
        'nrwl/nx packages/nx/src/native/tasks/hash_planner.rs external_deps: an empty externalDependencies list hashes no package, so a target stating one names the file its ecosystem states versions in (README.md OWNERS rows Package version, Swift package version, Rust package version, C++ toolchain and packages)',
    replayed: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts: should cache command execution',
} as const;
const BUILT = Duration.toMillis(Duration.minutes(10));
const TOKENED = /\{(?:options|workspaceRoot|projectRoot|projectName)/u;
const ARTIFACT = /^\.artifacts\/[^/]+\/./u;
const KEPT = /(?:^|\/)(?:dist|\.build|\.cloudflare\/(?:output|types))(?:\/|$)/u;
const VERSION = /(?:^|\s)-{0,2}version(?:\s|$)/u;
const CREDENTIAL = /TOKEN|SECRET|PASSWORD|CREDENTIAL|_KEY$/u;
const GLOBBED = /\/[^/]*[*{].*$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashing = ({ hashes }: Graph, task: string) => Record.keys(hashes[task]?.inputs ?? {});

const stored = (workspace: Graph) => declared(workspace).filter(({ target }) => target.cache === true);

const owned = (workspace: Graph, fact: (ecosystem: (typeof ECOSYSTEMS)[string]) => Option.Option<RegExp>) =>
    Array.getSomes(
        stored(workspace).map((declaration) =>
            declaration.plugin.pipe(
                Option.flatMap((owner) => Record.get(ECOSYSTEMS, owner)),
                Option.flatMap(fact),
                Option.map((pattern) => ({ ...declaration, pattern })),
            ),
        ),
    );

const cached = (workspace: Graph) => stored(workspace).flatMap(({ project, name, task }) => Array.fromNullishOr(workspace.nodes[project]).map((node) => ({ task, outputs: getOutputsForTargetAndConfiguration({ project, target: name }, {}, node) })));

const unresolved = (path: Path.Path, workspace: Graph) => cached(workspace).flatMap(({ task, outputs }) => outputs.filter((output) => TOKENED.test(output) || path.isAbsolute(output) || path.normalize(output).startsWith('..')).map((output) => `${task}: ${output}`));

const misplaced = (workspace: Graph) => cached(workspace).flatMap(({ task, outputs }) => outputs.filter((output) => !(ARTIFACT.test(output) || KEPT.test(output))).map((output) => `${task}: ${output}`));

const committed = (workspace: Graph, files: readonly string[]) => {
    const held = new Set(files.flatMap((file) => Array.scan(file.split('/'), '', (folder, part) => (folder === '' ? part : `${folder}/${part}`))));
    return cached(workspace).flatMap(({ task, outputs }) =>
        outputs
            .filter((output) => held.has(output.replace(GLOBBED, '')))
            .flatMap((output) => (held.has(output) ? [output] : files.filter((file) => minimatch(file, output, { dot: true }))))
            .map((file) => `${task}: ${file}`),
    );
};

const nested = (workspace: Graph) => {
    const folders = cached(workspace).flatMap(({ task, outputs }) => outputs.map((output) => ({ task, folder: `${output.replace(GLOBBED, '')}/` })));
    return folders.flatMap((outer) => folders.filter((inner) => inner.task !== outer.task && inner.folder.startsWith(outer.folder)).map((inner) => `${outer.task} ${outer.folder} holds ${inner.task} ${inner.folder}`));
};

const sourced = (workspace: Graph) => {
    const allowed = Record.fromIterableWith(Array.intersection(Array.dedupe([...plugins.map(({ plugin }) => plugin), ...declared(workspace).flatMap(({ target }) => Array.fromNullishOr(target.executor?.split(':')[0]))]), Record.keys(workspace.nodes)), (plugin) => [
        `${plugin}:`,
        dependents(workspace, new Set([plugin])),
    ]);
    return stored(workspace)
        .filter(({ project, task }) => hashing(workspace, task).some((input) => Record.some(allowed, (held, prefix) => input.startsWith(prefix) && !held.has(project))))
        .map(({ task }) => task);
};

const hashed = (workspace: Graph, prefix: string, refused: RegExp, own: readonly string[] = []) =>
    Array.difference(
        Array.dedupe(stored(workspace).flatMap(({ task }) => hashing(workspace, task))).filter((input) => input.startsWith(prefix) && refused.test(input)),
        own,
    );

const unversioned = (workspace: Graph) =>
    owned(workspace, ({ versions }) => versions)
        .filter(({ target: { inputs: stated = [] } }) => stated.some(Predicate.isString) && stated.some((input) => Predicate.hasProperty(input, 'externalDependencies') && isDeepStrictEqual(input.externalDependencies, [])))
        .filter(({ task, pattern }) => !hashing(workspace, task).some((input) => pattern.test(input)))
        .map(({ task }) => task);

const compiled = (workspace: Graph) => owned(workspace, ({ platform: differs }) => differs).filter(({ name, pattern }) => pattern.test(name));

const every = (workspace: Graph) => stored(workspace).filter(({ project, name }) => name.startsWith('test--') && (workspace.nodes[project]?.data.tags ?? []).includes('os:all'));

const neutral = (workspace: Graph) =>
    Array.difference(
        stored(workspace).filter(({ plugin }) =>
            Option.exists(
                Option.flatMap(plugin, (owner) => Record.get(ECOSYSTEMS, owner)),
                ({ platform: differs }) => Option.isNone(differs),
            ),
        ),
        every(workspace),
    );

const naming = Effect.fnUntraced(function* (workspace: Graph) {
    const systems = { darwin: ['darwin', 'macos', 'apple'], linux: ['linux'], win32: ['win32', 'windows', 'msvc'] }[String(platform)] ?? [platform];
    const commands = Array.dedupe(stored(workspace).flatMap(({ task }) => hashing(workspace, task).filter((input) => input.startsWith('runtime:'))));
    const printed = yield* Effect.forEach(commands, (command) => called(inject('readings').data, ['sh', '-c', command.replace('runtime:', '')]));
    const named = Array.zip(commands, printed).filter(([, { stdout }]) => systems.some((system) => stdout.toLowerCase().includes(system)));
    return new Set(named.map(([command]) => command));
});

const sampled = Effect.fnUntraced(function* (workspace: Graph) {
    const checks = stored(workspace).filter(({ project, name }) => name === 'check--tsc' && (workspace.dependencies[project] ?? []).every(({ target: needed }) => !(needed in workspace.nodes)));
    const tasks = checks.map(({ task }) => task);
    return Array.isReadonlyArrayNonEmpty(tasks) ? Array.min(tasks, Order.String) : yield* new Cause.NoSuchElementError('no cached check--tsc target without workspace dependencies');
});

const ran = Effect.fnUntraced(function* (data: string, folder: string, task: string, set: Record.ReadonlyRecord<string, string> = {}) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { code, stdout, stderr } = yield* called(data, ['nx', 'run', task, '--output-style=static'], { NX_CACHE_DIRECTORY: folder, ...set });
    const tasks = (yield* fs.exists(path.join(folder, 'run.json'))) ? yield* recorded(folder) : [];
    return { code, output: `${stdout}${stderr}`, hashes: tasks.map(({ taskId, hash }) => [taskId, hash]), statuses: tasks.filter(({ taskId }) => taskId === task).map(({ cacheStatus }) => cacheStatus) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })((suite) => {
    suite.effect(
        'states every output with a workspace or project root token Nx accepts',
        Effect.fnUntraced(function* () {
            const invalid = yield* refusals(
                declared(yield* read('fresh')),
                ({ task }) => task,
                ({ target }) => validateOutputs(target.outputs ?? []),
            );
            assert.deepStrictEqual(invalid, [], SOURCES.valid);
        }),
    );

    suite.effect(
        'resolves every output of a cached target to a path inside the workspace with no token left',
        Effect.fnUntraced(function* () {
            const [path, workspace] = yield* Effect.all([Path.Path, read('fresh')]);
            assert.deepStrictEqual(unresolved(path, workspace), [], SOURCES.resolved);
        }),
    );

    suite.effect(
        'places every output of a cached target under .artifacts/<tool>, a dist folder, .cloudflare/output, .cloudflare/types, or the .build folder of SwiftPM',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(misplaced(yield* read('fresh')), [], SOURCES.placed);
        }),
    );

    suite.effect(
        'declares no file git tracks as an output of a cached target',
        Effect.fnUntraced(function* () {
            const [workspace, files] = yield* Effect.all([read('fresh'), tracked()]);
            assert.isNotEmpty(files, SOURCES.restored);
            assert.deepStrictEqual(committed(workspace, files), [], SOURCES.restored);
        }),
    );

    suite.effect(
        'gives no two cached tasks an output path one holds inside the other',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(nested(yield* read('fresh')), [], SOURCES.apart);
        }),
    );

    suite.effect(
        'hashes a command whose output names the operating system into every cached task of a plugin whose output differs by platform for that target',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const commands = yield* naming(workspace);
            assert.isNotEmpty(compiled(workspace), SOURCES.platform);
            assert.deepStrictEqual(
                compiled(workspace)
                    .filter(({ task }) => commands.isDisjointFrom(new Set(hashing(workspace, task))))
                    .map(({ task }) => task),
                [],
                SOURCES.platform,
            );
        }),
    );

    suite.effect(
        'hashes a command whose output names the operating system into every cached test task of a project tagged os:all',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const commands = yield* naming(workspace);
            assert.isNotEmpty(every(workspace), SOURCES.platform);
            assert.deepStrictEqual(
                every(workspace)
                    .filter(({ task }) => commands.isDisjointFrom(new Set(hashing(workspace, task))))
                    .map(({ task }) => task),
                [],
                SOURCES.platform,
            );
        }),
    );

    suite.effect(
        'hashes no command whose output names the operating system into a cached task of a plugin whose output never differs by platform but the test tasks of projects tagged os:all',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const commands = yield* naming(workspace);
            assert.isNotEmpty(neutral(workspace), SOURCES.platform);
            assert.deepStrictEqual(
                neutral(workspace)
                    .filter(({ task }) => !commands.isDisjointFrom(new Set(hashing(workspace, task))))
                    .map(({ task }) => task),
                [],
                SOURCES.platform,
            );
        }),
    );

    suite.effect(
        'hashes plugin and executor source into a cached task only where its project depends on that plugin',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(sourced(yield* read('fresh')), [], SOURCES.read);
        }),
    );

    suite.effect(
        'hashes the output of no tool version command (--version, -version, <tool> version) into a cached task',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(hashed(yield* read('fresh'), 'runtime:', VERSION), [], SOURCES.read);
        }),
    );

    suite.effect(
        'hashes no credential variable into a cached task beyond the encryption key Nx itself adds',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(hashed(yield* read('fresh'), 'env:', CREDENTIAL, ['env:NX_CLOUD_ENCRYPTION_KEY']), [], `${SOURCES.read}; ${SOURCES.keyed}`);
        }),
    );

    suite.effect(
        'hashes the file its ecosystem states versions in into every cached task that reads its own files and states no external dependency',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(unversioned(yield* read('fresh')), [], SOURCES.versioned);
        }),
    );
});

it.layer(layer, { excludeTestServices: true })((suite) => {
    suite.effect(
        'replays from the local cache the type check of the first library without workspace dependencies',
        Effect.fnUntraced(function* () {
            const [fs, workspace] = yield* Effect.all([FileSystem.FileSystem, read('fresh')]);
            const [folder, task] = yield* Effect.all([fs.makeTempDirectoryScoped({ prefix: 'nx-cache-' }), sampled(workspace)]);
            const first = yield* ran(inject('readings').data, folder, task);
            const second = yield* ran(inject('readings').data, folder, task);
            assert.strictEqual(first.code, 0, first.output);
            assert.deepStrictEqual(second.hashes, first.hashes, SOURCES.replayed);
            assert.deepStrictEqual(second.statuses, ['local-cache-hit'], SOURCES.replayed);
        }, Effect.scoped),
        BUILT,
    );
});
