import { Buffer } from 'node:buffer';
import { createServer, type Server } from 'node:http';
import { constants } from 'node:http2';
import { platform } from 'node:process';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { workspaceRoot } from '@nx/devkit';
import { Consumer, fromCache, Packed } from '@rasm/testing/nx/consumer';
import { Array, Cause, Duration, Effect, FileSystem, Layer, Match, Option, Order, Path, Predicate, Record, Schema } from 'effect';
import { readNxJson } from 'nx/src/config/nx-json';
import { getOutputsForTargetAndConfiguration, validateOutputs } from 'nx/src/tasks-runner/utils';
import { inject } from 'vitest';
import { called, declared, dependents, read, refusals, type Workspace } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SOURCES = {
    valid: 'nrwl/nx packages/nx/src/tasks-runner/utils.spec.ts validateOutputs: should throw an error if outputs is not an array of strings prefixed with {workspaceRoot} or {projectRoot}',
    resolved: 'nrwl/nx packages/nx/src/tasks-runner/utils.spec.ts getOutputsForTargetAndConfiguration: should interpolate {workspaceRoot}, {projectRoot}, {projectName}, and {options.*}',
    placed: 'README.md PRINCIPLES: Caches sit under `.cache/<tool>` and generated output under `.artifacts/<tool>`, `dist`, `.cloudflare/output`, and editor-shared folders stay where their tool puts them',
    apart: 'nrwl/nx e2e/nx/src/cache.test.ts: should cache command execution; a restored output holds the files of one task alone',
    platform: 'README.md PRINCIPLES: Cached targets with output that differs by platform hash one platform input, since one remote cache serves every runner',
    read: "README.md PRINCIPLES: Task inputs hold the files a task's tool reads: plugin and executor source, `.config/mise.lock`, tool `--version` output, and credentials are no input",
    triggers: 'vitest-dev/vitest docs/config/forcereruntriggers.md: Glob pattern of file paths that will trigger the whole suite rerun, stated once per root config; a file a root config names there is an input of the test target of that config',
    replayed: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts: should cache command execution',
    globbed: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts: should support using globs as outputs',
    filesets: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts: should use consider filesets when hashing',
    sibling: 'nrwl/nx e2e/nx/src/include-ignored-cache-utils.ts: A change to a matched file misses the cache, a change to a file the inputs exclude still hits',
    remote: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts http remote cache: should PUT and GET cache from remote cache',
    absent: 'nrwl/nx e2e/nx/src/cache-no-daemon.test.ts http remote cache: should error if server is not running names the server; the `@rasm/nx-devkit` row of nx.json turns the remote cache off for a run when the cache server does not answer',
} as const;
const BUILT = Duration.toMillis(Duration.minutes(10));
const TOKEN = 'test-token';
const BUILD = 'alpha:build--tsc';
const TOKENED = /\{(?:options|workspaceRoot|projectRoot|projectName)/u;
const ARTIFACT = /^\.artifacts\/[^/]+\/./u;
const KEPT = /(?:^|\/)(?:dist|\.cloudflare\/output)(?:\/|$)/u;
const VERSION = /(?:^|\s)--version(?:\s|$)/u;
const CREDENTIAL = /TOKEN|SECRET|PASSWORD|CREDENTIAL|_KEY/u;
const GLOBBED = /\/[^/]*[*{].*$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.fromJsonString(Schema.Struct({ tasks: Schema.Array(Schema.Struct({ taskId: Schema.String, hash: Schema.String, cacheStatus: Schema.String, status: Schema.Int })) })).annotate({
    description: 'run.json Nx writes into its cache folder after each run: the hash, cache status, and exit code of every task',
});

// --- [SERVICES] ------------------------------------------------------------------------

const answered = Match.type<{ readonly authorized: boolean; readonly held: Buffer | undefined }>().pipe(
    Match.when({ authorized: false }, () => constants.HTTP_STATUS_UNAUTHORIZED),
    Match.when({ held: Match.undefined }, () => constants.HTTP_STATUS_NOT_FOUND),
    Match.orElse(() => constants.HTTP_STATUS_OK),
);

const remote = Effect.acquireRelease(
    Effect.callback<Server>((resume) => {
        const stored = new Map<string, Buffer>();
        const server = createServer((request, response) => {
            const chunks: Buffer[] = [];
            request.on('data', (chunk: Buffer) => chunks.push(chunk));
            request.on('end', () => {
                const held = request.method === 'PUT' ? stored.set(String(request.url), Buffer.concat(chunks)).get(String(request.url)) : stored.get(String(request.url));
                response.writeHead(answered({ authorized: request.headers.authorization === `Bearer ${TOKEN}`, held })).end(request.method === 'GET' ? held : undefined);
            });
        });
        server.listen(0, '127.0.0.1', () => resume(Effect.succeed(server)));
    }),
    (server) =>
        Effect.callback<void>((resume) => {
            server.close(() => resume(Effect.void));
        }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const cached = (workspace: Workspace) =>
    declared(workspace)
        .filter(({ target }) => target.cache === true)
        .flatMap(({ project, name, task }) => Array.fromNullishOr(workspace.nodes[project]).map((node) => ({ task, outputs: getOutputsForTargetAndConfiguration({ project, target: name }, {}, node) })));

const unresolved = (path: Path.Path, workspace: Workspace) => cached(workspace).flatMap(({ task, outputs }) => outputs.filter((output) => TOKENED.test(output) || path.isAbsolute(output) || path.normalize(output).startsWith('..')).map((output) => `${task}: ${output}`));

const misplaced = (workspace: Workspace) => cached(workspace).flatMap(({ task, outputs }) => outputs.filter((output) => !(ARTIFACT.test(output) || KEPT.test(output))).map((output) => `${task}: ${output}`));

const nested = (workspace: Workspace) => {
    const owned = cached(workspace).flatMap(({ task, outputs }) => outputs.map((output) => ({ task, folder: `${output.replace(GLOBBED, '')}/` })));
    return owned.flatMap((outer) => owned.filter((inner) => inner.task !== outer.task && inner.folder.startsWith(outer.folder)).map((inner) => `${outer.task} ${outer.folder} holds ${inner.task} ${inner.folder}`));
};

const sourced = (workspace: Workspace) => {
    const rows = (readNxJson(workspaceRoot).plugins ?? []).map((row) => (Predicate.isString(row) ? row : row.plugin));
    const allowed = Record.fromIterableWith(Array.intersection(Array.dedupe([...rows, ...declared(workspace).flatMap(({ target }) => Array.fromNullishOr(target.executor?.split(':')[0]))]), Record.keys(workspace.nodes)), (plugin) => [`${plugin}:`, dependents(workspace, new Set([plugin]))]);
    return declared(workspace)
        .filter(({ project, task, target }) => target.cache === true && Record.keys(workspace.hashes[task]?.inputs ?? {}).some((input) => Record.some(allowed, (held, prefix) => input.startsWith(prefix) && !held.has(project))))
        .map(({ task }) => task);
};

const hashed = (workspace: Workspace, prefix: string, refused: RegExp, own: readonly string[] = []) => {
    const inputs = declared(workspace)
        .filter(({ target }) => target.cache === true)
        .flatMap(({ task }) => Record.keys(workspace.hashes[task]?.inputs ?? {}));
    return Array.difference(
        Array.dedupe(inputs).filter((input) => input.startsWith(prefix) && refused.test(input)),
        own,
    );
};

const suites = ({ nodes }: Pick<Workspace, 'nodes'>, root: string) =>
    Object.values(nodes)
        .filter(({ data }) => data.root === root)
        .flatMap(({ name, data }) =>
            Record.keys(data.targets ?? {})
                .filter((target) => target.startsWith('test--'))
                .map((target) => `${name}:${target}`),
        );

const compiled = (workspace: Workspace) => declared(workspace).filter(({ name, plugin, target }) => target.cache === true && Array.isReadonlyArrayNonEmpty(target.outputs ?? []) && (name.endsWith('--unreal') || Option.exists(plugin, (owner) => ['@rasm/nx-cargo', '@rasm/nx-apple', '@rasm/nx-cmake'].includes(owner))));

const unnamed = Effect.fnUntraced(function* (workspace: Workspace) {
    const systems = { darwin: ['darwin', 'macos', 'apple'], linux: ['linux'], win32: ['win32', 'windows', 'msvc'] }[String(platform)] ?? [platform];
    const commands = Array.dedupe(compiled(workspace).flatMap(({ task }) => Record.keys(workspace.hashes[task]?.inputs ?? {}).filter((input) => input.startsWith('runtime:'))));
    const printed = yield* Effect.forEach(commands, (command) => called(inject('readings').data, 'sh', ['-c', command.replace('runtime:', '')]));
    const naming = new Set(
        Array.zip(commands, printed)
            .filter(([, { stdout }]) => systems.some((system) => stdout.toLowerCase().includes(system)))
            .map(([command]) => command),
    );
    return compiled(workspace)
        .filter(({ task }) => naming.isDisjointFrom(new Set(Record.keys(workspace.hashes[task]?.inputs ?? {}))))
        .map(({ task }) => task);
});

const sampled = Effect.fnUntraced(function* (workspace: Workspace) {
    const checks = declared(workspace).filter(({ project, name, target }) => name === 'check--tsc' && target.cache === true && (workspace.dependencies[project] ?? []).every(({ target: needed }) => !(needed in workspace.nodes)));
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

const addressed = (server: Server) => {
    const address = server.address();
    return `http://127.0.0.1:${Predicate.isObject(address) ? address.port : address}`;
};

const built = Effect.fnUntraced(function* () {
    const { nx } = yield* Consumer;
    const { code, stdout, stderr } = yield* nx('run', BUILD, '--output-style=static');
    return { code: Number(code), output: `${stdout}${stderr}` };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('cache declarations', (suite) => {
    suite.effect(
        'states every output with a workspace or project root token Nx accepts',
        Effect.fnUntraced(function* () {
            const refused = yield* refusals(
                declared(yield* read('fresh')),
                ({ task }) => task,
                ({ target }) => validateOutputs(target.outputs ?? []),
            );
            assert.deepStrictEqual(refused, [], SOURCES.valid);
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
        'places every output of a cached target under .artifacts/<tool>, a dist folder, or .cloudflare/output',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(misplaced(yield* read('fresh')), [], SOURCES.placed);
        }),
    );

    suite.effect(
        'gives no two cached tasks an output path one holds inside the other',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(nested(yield* read('fresh')), [], SOURCES.apart);
        }),
    );

    suite.effect(
        'hashes into every cached task of a compiled language a command whose output names the operating system',
        Effect.fnUntraced(function* () {
            const workspace = yield* read('fresh');
            assert.isNotEmpty(compiled(workspace), SOURCES.platform);
            assert.deepStrictEqual(yield* unnamed(workspace), [], SOURCES.platform);
        }),
    );

    suite.effect(
        'hashes plugin and executor source into a cached task only where its project depends on that plugin',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(sourced(yield* read('fresh')), [], SOURCES.read);
        }),
    );

    suite.effect(
        'hashes the output of no tool --version command into a cached task',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(hashed(yield* read('fresh'), 'runtime:', VERSION), [], SOURCES.read);
        }),
    );

    suite.effect(
        'hashes no credential variable into a cached task beyond the encryption key Nx itself adds',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual(hashed(yield* read('fresh'), 'env:', CREDENTIAL, ['env:NX_CLOUD_ENCRYPTION_KEY']), [], SOURCES.read);
        }),
    );
});

it.layer(layer, { excludeTestServices: true })('cache replay in this workspace', (suite) => {
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
            const [fs, workspace, server] = yield* Effect.all([FileSystem.FileSystem, read('fresh'), remote]);
            const [stored, emptied, task] = yield* Effect.all([fs.makeTempDirectoryScoped({ prefix: 'nx-cache-' }), fs.makeTempDirectoryScoped({ prefix: 'nx-cache-' }), sampled(workspace)]);
            const served = { NX_SELF_HOSTED_REMOTE_CACHE_SERVER: addressed(server), NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN: TOKEN };
            const first = yield* ran(inject('readings').data, stored, task, served);
            const second = yield* ran(emptied, emptied, task, served);
            assert.strictEqual(first.code, 0, first.output);
            assert.deepStrictEqual(second.statuses, ['remote-cache-hit'], `${SOURCES.remote}\n${second.output}`);
        }, Effect.scoped),
        BUILT,
    );

    suite.effect(
        'runs a cached task to exit 0 and names the server in a warning when no remote cache server listens',
        Effect.fnUntraced(function* () {
            const [fs, workspace] = yield* Effect.all([FileSystem.FileSystem, read('fresh')]);
            const refusing = yield* Effect.scoped(Effect.map(remote, addressed));
            const [folder, task] = yield* Effect.all([fs.makeTempDirectoryScoped({ prefix: 'nx-cache-' }), sampled(workspace)]);
            const { code, output, statuses } = yield* ran(inject('readings').data, folder, task, { NX_SELF_HOSTED_REMOTE_CACHE_SERVER: refusing, NX_SELF_HOSTED_REMOTE_CACHE_ACCESS_TOKEN: TOKEN });
            assert.strictEqual(code, 0, `${SOURCES.absent}\n${output}`);
            assert.deepStrictEqual(statuses, ['cache-miss'], SOURCES.absent);
            assert.include(output, refusing, SOURCES.absent);
        }, Effect.scoped),
        BUILT,
    );
});

it.layer(
    Layer.provideMerge(
        Layer.provide(
            Consumer.layer({
                resolved: import.meta.url,
                settings: { plugins: ['@rasm/nx-typescript'] },
                packages: ['@typescript/native'],
                ignored: ['dist'],
                files: {
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
)('cache replay in a workspace that installs @rasm/nx-typescript', (suite) => {
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
)('test inputs in a workspace that installs @rasm/nx-vitest', (suite) => {
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
