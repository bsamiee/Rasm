import { constants } from 'node:http2';
import { platform } from 'node:process';
import { isDeepStrictEqual } from 'node:util';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { type NxJsonConfiguration, workspaceRoot } from '@nx/devkit';
import { answered, Consumer, fromCache, Packed, remote } from '@rasm/testing/nx';
import compiler from '@typescript/native/package.json' with { type: 'json' };
import { Array, Cause, Duration, Effect, FileSystem, Function, Layer, Option, Order, Path, Predicate, Record, Schema } from 'effect';
import { minimatch } from 'minimatch';
import installed from 'nx/package.json' with { type: 'json' };
import { readNxJson } from 'nx/src/config/nx-json';
import { getOutputsForTargetAndConfiguration, validateOutputs } from 'nx/src/tasks-runner/utils';
import { inject } from 'vitest';
import { called, declared, dependents, read, refusals, tracked, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    valid: 'nrwl/nx packages/nx/src/tasks-runner/utils.spec.ts validateOutputs: should throw an error if outputs is not an array of strings prefixed with {workspaceRoot} or {projectRoot}',
    resolved: 'nrwl/nx packages/nx/src/tasks-runner/utils.spec.ts getOutputsForTargetAndConfiguration: should interpolate {workspaceRoot}, {projectRoot}, {projectName}, and {options.*}',
    placed: 'README.md PRINCIPLES: Caches sit under `.cache/<tool>` and generated output under `.artifacts/<tool>`, `dist`, `.cloudflare/output`, and editor-shared folders stay where their tool puts them; `cf workers types` writes the binding types an editor reads to `.cloudflare/types`, and SwiftPM writes `.build` beside `Package.swift`, which SourceKit-LSP shares and no configuration file moves',
    restored: 'nrwl/nx astro-docs/src/content/docs/features/cache-task-results.mdoc: On a cache hit, Nx restores the cached output files, so an output git tracks is overwritten by a replay',
    apart: 'nrwl/nx e2e/nx/src/cache.test.ts: should cache command execution; a restored output holds the files of one task alone',
    platform:
        'README.md PRINCIPLES: Cached targets with output that differs by platform hash one platform input, since one remote cache serves every runner; builds and tests of Cargo, Swift, CMake, and Unreal and test results of .NET and Python differ by platform, output of tsc, Vite, Vitest, and Storybook does not; Suites reading the project graph take tag `os:all`, hash one platform input, and run with no task beside them',
    read: "README.md PRINCIPLES: Task inputs hold the files a task's tool reads: plugin and executor source, `.config/mise.lock`, tool `--version` output, and credentials are no input",
    keyed: 'README.md OWNERS: Mac app release form — Doppler `rasm/release` `ASC_API_*` rows, set notarize a `.dmg` for Sparkle, absent release a `.zip`; a key identifier that selects the form of an output is no credential',
    versioned:
        'nrwl/nx packages/nx/src/native/tasks/hash_planner.rs external_deps: an empty externalDependencies list hashes no package, so a target stating one names the file its ecosystem states versions in (README.md OWNERS rows Package version, Swift package version, Rust package version, C++ toolchain and packages)',
    triggers: 'vitest-dev/vitest docs/config/forcereruntriggers.md: Glob pattern of file paths that will trigger the whole suite rerun, stated once per root config; a file a root config names there is an input of the test target of that config',
    replayed: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts: should cache command execution',
    globbed: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts: should support using globs as outputs',
    filesets: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts: should use consider filesets when hashing',
    sibling: 'nrwl/nx e2e/nx/src/include-ignored-cache-utils.ts: A change to a matched file misses the cache, a change to a file the inputs exclude still hits',
    upstream: 'nrwl/nx packages/vite/src/plugins/plugin.ts buildInputs: an inferred build reads `^production` where nx.json defines `production` and `^default` otherwise',
    remote: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts http remote cache: should PUT and GET cache from remote cache',
    absent: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts http remote cache: should error if server is not running names the server; the `@rasm/nx-distribution` row of nx.json turns the remote cache off for a run when the cache server does not answer',
    refused:
        'nrwl/nx packages/nx/src/native/cache/http_remote_cache.rs retrieve: the client accepts 200 and 404 alone and sets no timeout, so Nx alone fails the run on any other answer and waits on a silent server; Nx self-hosted cache OpenAPI: a read answers 200, 403, or 404; the `@rasm/nx-distribution` row of nx.json turns the remote cache off for the run after one read',
    annotated: 'GitHub Docs, Workflow commands for GitHub Actions, Setting a warning message: `::warning file={name},line={line},title={title}::{message}`',
} as const;
const OUTAGES = [
    { outage: 'answers 500 to a read', status: Option.some(constants.HTTP_STATUS_INTERNAL_SERVER_ERROR) },
    { outage: 'answers 403 to a read', status: Option.some(constants.HTTP_STATUS_FORBIDDEN) },
    { outage: 'accepts the connection and never answers', status: Option.none<number>() },
] as const;
const VERSIONED: Record.ReadonlyRecord<string, RegExp> = {
    '@rasm/nx-cargo': /Cargo\.(?:lock|toml)/u,
    '@rasm/nx-uv': /uv\.lock|pyproject\.toml/u,
    '@rasm/nx-dotnet': /Directory\.Packages\.props/u,
    '@rasm/nx-apple': /Package\.(?:resolved|swift)/u,
    '@rasm/nx-cmake': /vcpkg\.json/u,
    '@rasm/nx-typescript': /pnpm-workspace\.yaml/u,
};
const PLATFORM: Record.ReadonlyRecord<string, RegExp> = {
    '@rasm/nx-cargo': /^(?:build|test)--/u,
    '@rasm/nx-apple': /^(?:build|test)--/u,
    '@rasm/nx-cmake': /^(?:build|test)--/u,
    '@rasm/nx-distribution': /^(?:build|test)--unreal$/u,
    '@rasm/nx-dotnet': /^test--/u,
    '@rasm/nx-uv': /^test--/u,
};
const SETTINGS: NxJsonConfiguration = { plugins: ['@rasm/nx-typescript'] };
const BUILT = Duration.toMillis(Duration.minutes(10));
const TOKEN = 'test-token';
const BUILD = 'alpha:build--tsc';
const TOKENED = /\{(?:options|workspaceRoot|projectRoot|projectName)/u;
const ARTIFACT = /^\.artifacts\/[^/]+\/./u;
const KEPT = /(?:^|\/)(?:dist|\.build|\.cloudflare\/(?:output|types))(?:\/|$)/u;
const VERSION = /(?:^|\s)-{0,2}version(?:\s|$)/u;
const CREDENTIAL = /TOKEN|SECRET|PASSWORD|CREDENTIAL|_KEY$/u;
const GLOBBED = /\/[^/]*[*{].*$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.fromJsonString(Schema.Struct({ tasks: Schema.Array(Schema.Struct({ taskId: Schema.String, hash: Schema.String, cacheStatus: Schema.String, status: Schema.Int })) })).annotate({
    description: 'run.json Nx writes into its cache folder after each run: the hash, cache status, and exit code of every task',
});

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const hashing = ({ hashes }: Workspace, task: string) => Record.keys(hashes[task]?.inputs ?? {});

const stored = (workspace: Workspace) => declared(workspace).filter(({ target }) => target.cache === true);

const cached = (workspace: Workspace) => stored(workspace).flatMap(({ project, name, task }) => Array.fromNullishOr(workspace.nodes[project]).map((node) => ({ task, outputs: getOutputsForTargetAndConfiguration({ project, target: name }, {}, node) })));

const unresolved = (path: Path.Path, workspace: Workspace) => cached(workspace).flatMap(({ task, outputs }) => outputs.filter((output) => TOKENED.test(output) || path.isAbsolute(output) || path.normalize(output).startsWith('..')).map((output) => `${task}: ${output}`));

const misplaced = (workspace: Workspace) => cached(workspace).flatMap(({ task, outputs }) => outputs.filter((output) => !(ARTIFACT.test(output) || KEPT.test(output))).map((output) => `${task}: ${output}`));

const committed = (workspace: Workspace, files: readonly string[]) => {
    const held = new Set(files.flatMap((file) => Array.scan(file.split('/'), '', (folder, part) => (folder === '' ? part : `${folder}/${part}`))));
    return cached(workspace).flatMap(({ task, outputs }) =>
        outputs
            .filter((output) => held.has(output.replace(GLOBBED, '')))
            .flatMap((output) => (held.has(output) ? [output] : files.filter((file) => minimatch(file, output, { dot: true }))))
            .map((file) => `${task}: ${file}`),
    );
};

const nested = (workspace: Workspace) => {
    const owned = cached(workspace).flatMap(({ task, outputs }) => outputs.map((output) => ({ task, folder: `${output.replace(GLOBBED, '')}/` })));
    return owned.flatMap((outer) => owned.filter((inner) => inner.task !== outer.task && inner.folder.startsWith(outer.folder)).map((inner) => `${outer.task} ${outer.folder} holds ${inner.task} ${inner.folder}`));
};

const sourced = (workspace: Workspace) => {
    const rows = (readNxJson(workspaceRoot).plugins ?? []).map((row) => (Predicate.isString(row) ? row : row.plugin));
    const allowed = Record.fromIterableWith(Array.intersection(Array.dedupe([...rows, ...declared(workspace).flatMap(({ target }) => Array.fromNullishOr(target.executor?.split(':')[0]))]), Record.keys(workspace.nodes)), (plugin) => [`${plugin}:`, dependents(workspace, new Set([plugin]))]);
    return stored(workspace)
        .filter(({ project, task }) => hashing(workspace, task).some((input) => Record.some(allowed, (held, prefix) => input.startsWith(prefix) && !held.has(project))))
        .map(({ task }) => task);
};

const hashed = (workspace: Workspace, prefix: string, refused: RegExp, own: readonly string[] = []) =>
    Array.difference(
        Array.dedupe(stored(workspace).flatMap(({ task }) => hashing(workspace, task))).filter((input) => input.startsWith(prefix) && refused.test(input)),
        own,
    );

const unversioned = (workspace: Workspace) =>
    stored(workspace)
        .filter(({ target: { inputs: stated = [] } }) => stated.some(Predicate.isString) && stated.some((input) => Predicate.hasProperty(input, 'externalDependencies') && isDeepStrictEqual(input.externalDependencies, [])))
        .filter(({ task, plugin }) =>
            Option.exists(
                Option.flatMap(plugin, (owner) => Record.get(VERSIONED, owner)),
                (file) => !hashing(workspace, task).some((input) => file.test(input)),
            ),
        )
        .map(({ task }) => task);

const suites = ({ nodes }: Pick<Workspace, 'nodes'>, root: string) =>
    Object.values(nodes)
        .filter(({ data }) => data.root === root)
        .flatMap(({ name, data }) =>
            Record.keys(data.targets ?? {})
                .filter((target) => target.startsWith('test--'))
                .map((target) => `${name}:${target}`),
        );

const compiled = (workspace: Workspace) =>
    stored(workspace).filter(({ name, plugin }) =>
        Option.exists(
            Option.flatMap(plugin, (owner) => Record.get(PLATFORM, owner)),
            (tools) => tools.test(name),
        ),
    );

const every = (workspace: Workspace) => stored(workspace).filter(({ project, name }) => name.startsWith('test--') && (workspace.nodes[project]?.data.tags ?? []).includes('os:all'));

const neutral = (workspace: Workspace) =>
    Array.difference(
        stored(workspace).filter(({ plugin }) => Option.exists(plugin, (owner) => ['@rasm/nx-typescript', '@rasm/nx-vite', '@rasm/nx-vitest', '@rasm/nx-storybook'].includes(owner))),
        every(workspace),
    );

const naming = Effect.fnUntraced(function* (workspace: Workspace) {
    const systems = { darwin: ['darwin', 'macos', 'apple'], linux: ['linux'], win32: ['win32', 'windows', 'msvc'] }[String(platform)] ?? [platform];
    const commands = Array.dedupe(stored(workspace).flatMap(({ task }) => hashing(workspace, task).filter((input) => input.startsWith('runtime:'))));
    const printed = yield* Effect.forEach(commands, (command) => called(inject('readings').data, 'sh', ['-c', command.replace('runtime:', '')]));
    const named = Array.zip(commands, printed).filter(([, { stdout }]) => systems.some((system) => stdout.toLowerCase().includes(system)));
    return new Set(named.map(([command]) => command));
});

const sampled = Effect.fnUntraced(function* (workspace: Workspace) {
    const checks = stored(workspace).filter(({ project, name }) => name === 'check--tsc' && (workspace.dependencies[project] ?? []).every(({ target: needed }) => !(needed in workspace.nodes)));
    const tasks = checks.map(({ task }) => task);
    return Array.isReadonlyArrayNonEmpty(tasks) ? Array.min(tasks, Order.String) : yield* new Cause.NoSuchElementError('no cached check--tsc target without workspace dependencies');
});

const ran = Effect.fnUntraced(function* (data: string, folder: string, task: string, set: Record.ReadonlyRecord<string, string> = {}) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { code, stdout, stderr } = yield* called(data, 'nx', ['run', task, '--output-style=static'], { NX_CACHE_DIRECTORY: folder, ...set });
    const report = yield* Effect.option(Effect.flatMap(fs.readFileString(path.join(folder, 'run.json')), Schema.decodeEffect(Run)));
    const tasks = Option.getOrUndefined(report)?.tasks ?? [];
    return { code: Number(code), output: `${stdout}${stderr}`, hashes: tasks.map(({ taskId, hash }) => [taskId, hash]), statuses: tasks.filter(({ taskId }) => taskId === task).map(({ cacheStatus }) => cacheStatus) };
});

const outage = Effect.fnUntraced(function* (address: string, set: Record.ReadonlyRecord<string, string> = {}) {
    const [fs, workspace] = yield* Effect.all([FileSystem.FileSystem, read('fresh')]);
    const [folder, task] = yield* Effect.all([fs.makeTempDirectoryScoped({ prefix: 'nx-cache-' }), sampled(workspace)]);
    const { code, output, statuses } = yield* ran(inject('readings').data, folder, task, { NX_SELF_HOSTED_REMOTE_CACHE_SERVER: address, NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN: TOKEN, ...set });
    return { code, output, statuses, warnings: output.split('\n').filter((line) => line.includes(address)) };
});

const upstream = ({ nodes: { alpha } }: Pick<Workspace, 'nodes'>) => (alpha?.data.targets?.['build--tsc']?.inputs ?? []).filter(Predicate.isString).filter((input) => input.startsWith('^'));

const built = Effect.fnUntraced(function* () {
    const consumer = yield* Consumer;
    const { code, stdout, stderr } = yield* consumer.nx('run', BUILD, '--output-style=static');
    return { code: Number(code), output: `${stdout}${stderr}` };
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
        'hashes a command whose output names the operating system into every cached build and test task of Cargo, Swift, CMake, and Unreal and every cached .NET and Python test task',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const commands = yield* naming(workspace);
            const lacking = compiled(workspace).filter(({ task }) => commands.isDisjointFrom(new Set(hashing(workspace, task))));
            assert.isNotEmpty(compiled(workspace), SOURCES.platform);
            assert.deepStrictEqual(
                lacking.map(({ task }) => task),
                [],
                SOURCES.platform,
            );
        }),
    );

    suite.effect(
        'hashes a command whose output names the operating system into every cached test task of a project tagged os:all, and runs each with no task beside it',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            const commands = yield* naming(workspace);
            assert.isNotEmpty(every(workspace), SOURCES.platform);
            assert.deepStrictEqual(
                every(workspace)
                    .filter(({ task, target }) => target.parallelism !== false || commands.isDisjointFrom(new Set(hashing(workspace, task))))
                    .map(({ task }) => task),
                [],
                SOURCES.platform,
            );
        }),
    );

    suite.effect(
        'hashes no command whose output names the operating system into a cached task of tsc, Vite, Vitest, or Storybook but the test tasks of projects tagged os:all',
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

    suite.effect(
        'stores a task result in a remote cache server and replays it from there into an empty local cache',
        Effect.fnUntraced(function* () {
            const [fs, workspace, { address }] = yield* Effect.all([FileSystem.FileSystem, read('fresh'), remote(TOKEN, Function.flow(answered, Option.some))]);
            const [kept, emptied, task] = yield* Effect.all([fs.makeTempDirectoryScoped({ prefix: 'nx-cache-' }), fs.makeTempDirectoryScoped({ prefix: 'nx-cache-' }), sampled(workspace)]);
            const set = { NX_SELF_HOSTED_REMOTE_CACHE_SERVER: address, NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN: TOKEN };
            const first = yield* ran(inject('readings').data, kept, task, set);
            const second = yield* ran(emptied, emptied, task, set);
            assert.strictEqual(first.code, 0, first.output);
            assert.deepStrictEqual(second.statuses, ['remote-cache-hit'], `${SOURCES.remote}\n${second.output}`);
        }, Effect.scoped),
        BUILT,
    );

    suite.effect.each([
        { run: 'a run outside GitHub Actions', set: {}, annotated: false },
        { run: 'a run under GITHUB_ACTIONS=true', set: { GITHUB_ACTIONS: 'true' }, annotated: true },
    ])(
        'runs a cached task to exit 0 and names the server in one warning, a workflow command only under GitHub Actions, when no remote cache server listens in $run',
        Effect.fnUntraced(function* ({ set, annotated }) {
            const closed = yield* Effect.scoped(Effect.map(remote(TOKEN, Option.none), ({ address }) => address));
            const { code, output, statuses, warnings } = yield* outage(closed, set);
            assert.strictEqual(code, 0, `${SOURCES.absent}\n${output}`);
            assert.deepStrictEqual(statuses, ['cache-miss'], SOURCES.absent);
            assert.deepStrictEqual(
                warnings.map((line) => line.startsWith('::warning ')),
                [annotated],
                `${SOURCES.absent}; ${SOURCES.annotated}\n${output}`,
            );
        }, Effect.scoped),
        BUILT,
    );

    suite.effect.each(OUTAGES)(
        'runs a cached task to exit 0 with one warning and sends no request after the first read to a remote cache server that $outage',
        Effect.fnUntraced(function* ({ status }) {
            const { address, requests } = yield* remote(TOKEN, () => status);
            const { code, output, statuses, warnings } = yield* outage(address);
            assert.strictEqual(code, 0, `${SOURCES.refused}\n${output}`);
            assert.deepStrictEqual(statuses, ['cache-miss'], SOURCES.refused);
            assert.lengthOf(warnings, 1, `${SOURCES.refused}\n${output}`);
            assert.lengthOf(requests, 1, `${SOURCES.refused}: ${requests.join(', ')}`);
        }, Effect.scoped),
    );
});

it.layer(
    Layer.provideMerge(
        Layer.provide(
            Consumer.layer({
                resolved: import.meta.url,
                settings: SETTINGS,
                ignored: ['dist'],
                files: {
                    'package.json': JSON.stringify({ name: 'consumer', private: true, type: 'module', devDependencies: { nx: installed.version, '@rasm/nx-typescript': '*', typescript: compiler.version } }),
                    'docs/notes.md': 'unread\n',
                    'libs/alpha/package.json': '{ "name": "alpha", "version": "0.0.0", "type": "module", "exports": "./dist/index.js" }\n',
                    'libs/alpha/tsconfig.json': '{ "compilerOptions": { "module": "nodenext", "declaration": true, "outDir": "dist", "strict": true }, "include": ["index.ts"] }\n',
                    'libs/alpha/index.ts': 'export const alpha = 1;\n',
                },
            }),
            Packed.layer('@rasm/nx-typescript'),
        ),
        layer,
    ),
    { timeout: BUILT, excludeTestServices: true },
)((suite) => {
    suite.effect(
        'replays a build from the local cache and restores each file the build wrote',
        Effect.fnUntraced(function* () {
            const { outputs, cleaned, absent } = yield* Consumer;
            const first = yield* built();
            const { expandedOutputs = [] } = yield* outputs(BUILD);
            yield* cleaned;
            const second = yield* built();
            assert.strictEqual(first.code, 0, first.output);
            assert.notMatch(first.output, fromCache(BUILD), SOURCES.replayed);
            assert.isNotEmpty(expandedOutputs, SOURCES.globbed);
            assert.match(second.output, fromCache(BUILD), SOURCES.replayed);
            assert.deepStrictEqual(yield* absent(expandedOutputs), [], SOURCES.globbed);
        }),
        BUILT,
    );

    suite.effect(
        'writes no file outside the outputs a build declares',
        Effect.fnUntraced(function* () {
            const { outputs, cleaned, written, wrote } = yield* Consumer;
            yield* cleaned;
            yield* wrote({ 'libs/alpha/index.ts': 'export const alpha = 2;\n' });
            const [run, created] = yield* written(built());
            const { expandedOutputs = [] } = yield* outputs(BUILD);
            assert.strictEqual(run.code, 0, run.output);
            assert.notMatch(run.output, fromCache(BUILD), SOURCES.filesets);
            assert.deepStrictEqual([...new Set(created).difference(new Set(expandedOutputs))], [], SOURCES.globbed);
        }),
        BUILT,
    );

    suite.effect(
        'runs a build again after a change to a file it reads, and replays it after a change to a file it does not read',
        Effect.fnUntraced(function* () {
            const { wrote } = yield* Consumer;
            yield* built();
            yield* wrote({ 'docs/notes.md': 'still unread\n' });
            const unread = yield* built();
            yield* wrote({ 'libs/alpha/index.ts': 'export const alpha = 3;\n' });
            const changed = yield* built();
            const again = yield* built();
            assert.match(unread.output, fromCache(BUILD), SOURCES.sibling);
            assert.notMatch(changed.output, fromCache(BUILD), SOURCES.filesets);
            assert.match(again.output, fromCache(BUILD), SOURCES.replayed);
        }),
        BUILT,
    );

    suite.effect(
        'reads the default input of dependencies in a workspace without a production input and the production input once nx.json defines one',
        Effect.fnUntraced(function* () {
            const { read: graphed, wrote } = yield* Consumer;
            const before = upstream(yield* graphed);
            yield* wrote({ 'nx.json': yield* Schema.encodeEffect(Json)({ ...SETTINGS, namedInputs: { default: ['{projectRoot}/**/*'], production: ['default'] } }) });
            assert.deepStrictEqual([before, upstream(yield* graphed)], [['^default'], ['^production']], SOURCES.upstream);
        }),
        BUILT,
    );
});

it.layer(
    Layer.provideMerge(
        Layer.provide(
            Consumer.layer({
                resolved: import.meta.url,
                settings: { plugins: ['@rasm/nx-vitest'] },
                packages: ['vitest', 'vite'],
                ignored: [],
                files: {
                    'shared/schema.json': '{}\n',
                    'suites/alone/package.json': '{ "name": "alone", "version": "0.0.0", "type": "module" }\n',
                    'suites/alone/vitest.config.ts': "import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: { forceRerunTriggers: ['**/shared/schema.json'] } });\n",
                    'suites/alone/sum.test.ts': "import { expect, it } from 'vitest';\n\nit('adds', () => expect(1 + 1).toBe(2));\n",
                    'suites/other/package.json': '{ "name": "other", "version": "0.0.0", "type": "module" }\n',
                    'suites/other/vitest.config.ts': "import { defineConfig } from 'vitest/config';\n\nexport default defineConfig({ test: {} });\n",
                    'suites/other/sum.test.ts': "import { expect, it } from 'vitest';\n\nit('adds', () => expect(1 + 1).toBe(2));\n",
                },
            }),
            Packed.layer('@rasm/nx-vitest'),
        ),
        layer,
    ),
    { timeout: BUILT, excludeTestServices: true },
)((suite) => {
    suite.effect(
        'gives a file a root config names in forceRerunTriggers to the test target of that config and to no other suite',
        Effect.fnUntraced(function* () {
            const { read: graphed, inputs } = yield* Consumer;
            const workspace = yield* graphed;
            const [alone, other] = [suites(workspace, 'suites/alone'), suites(workspace, 'suites/other')];
            const [named, apart] = yield* Effect.all([Effect.forEach(alone, inputs), Effect.forEach(other, inputs)]);
            assert.deepStrictEqual([alone.length, other.length], [1, 1], SOURCES.triggers);
            assert.deepStrictEqual(
                named.map(({ files }) => files.includes('shared/schema.json')),
                [true],
                SOURCES.triggers,
            );
            assert.deepStrictEqual(
                apart.map(({ files }) => files.includes('shared/schema.json')),
                [false],
                SOURCES.triggers,
            );
        }),
        BUILT,
    );
});
