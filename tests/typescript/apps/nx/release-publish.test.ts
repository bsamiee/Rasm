import { createServer, type IncomingMessage, type Server, type ServerResponse } from 'node:http';
import { findPackageJSON } from 'node:module';
import { delimiter } from 'node:path';
import { inspect } from 'node:util';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it, vi } from '@effect/vitest';
import { type ExecutorContext, getOutputsForTargetAndConfiguration, readProjectsConfigurationFromProjectGraph, workspaceRoot } from '@nx/devkit';
import devkit from '@rasm/nx-devkit/package.json' with { type: 'json' };
import { createNodes } from '@rasm/nx-distribution';
import manifest from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { entered, graphed } from '@rasm/testing/nx/inference';
import { Array, Config, Effect, Exit, FileSystem, MutableRef, Path, Predicate, Record, Schema, Stream } from 'effect';
import { Zip } from 'effect/encoding';
import type { NxJsonConfiguration } from 'nx/src/config/nx-json';
import { getExecutorInformation, LoadedNxPlugin } from 'nx/src/devkit-internals';
import type { Subject } from '#/inference/cases';
import { present } from '#/inference/workspaces';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = 'sheet';
const ROOT = 'packages/sheet';
const MANIFEST = 'blender_manifest.toml';
const EXECUTABLE = 0o755;
const TOOLS = ['cargo', 'uv', 'swift', 'xcrun', 'blender', 'gh'];
const GLOB = /\/?[*{].*$/u;
const PUBLISHING = /\spublish(?:\s|$)/u;
const ATTACHING = 'gh release upload ';
const HELD = 'held.txt';
const SDIST = `${ROOT}/dist/${PROJECT}-0.1.0.tar.gz`;
const ARCHIVE = `${PROJECT}-0.1.0.zip`;
const TAG = `${PROJECT}@0.1.0`;
const NX: NxJsonConfiguration = { release: { projects: [PROJECT], projectsRelationship: 'independent' } };
const FILES = {
    'package.json': '{ "name": "root" }\n',
    'nx.json': JSON.stringify(NX),
    'Cargo.toml': `[workspace]\nresolver = "3"\nmembers = ["${ROOT}"]\n`,
    [`${ROOT}/Cargo.toml`]: `[package]\nname = "${PROJECT}"\nversion = "0.1.0"\nedition = "2024"\n`,
    [`${ROOT}/src/lib.rs`]: 'pub fn sheets() -> usize {\n    1\n}\n',
    [`${ROOT}/${MANIFEST}`]: `schema_version = "1.0.0"\nid = "${PROJECT}"\nversion = "0.1.0"\nname = "Sheets"\ntagline = "Sheet layout"\nmaintainer = "Studio <studio@example.com>"\ntype = "add-on"\nblender_version_min = "5.0.0"\nlicense = ["SPDX:MIT"]\n`,
    [`${ROOT}/__init__.py`]: 'def register():\n    pass\n\n\ndef unregister():\n    pass\n',
};
const STORED = [
    { held: 'lacks the version', data: [], posted: [`POST /api/v1/extensions/${PROJECT}/versions/upload/`] },
    { held: 'holds the version', data: [{ id: PROJECT, version: '0.1.0' }], posted: [] },
] as const;
const PACKAGED = { build: { outputs: ['{projectRoot}/dist'] }, 'nx-release-publish': { dependsOn: ['build'] } };
const RELEASED = [
    { held: 'lacks the file', names: '', uploaded: true },
    { held: 'holds the file', names: `${ARCHIVE}\n`, uploaded: false },
] as const;
const distribution: Subject = { plugin: { name: manifest.name, createNodes }, tools: ['blender'], files: {} };
const PUBLISHERS = [
    { plugin: '@rasm/nx-cargo', executor: 'release-publish', options: { cwd: '.', package: PROJECT } },
    { plugin: '@rasm/nx-distribution', executor: 'release-publish', options: {} },
    { plugin: devkit.name, executor: 'publish', options: { publish: ['uv', 'publish'] } },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Succeeded = Schema.Struct({ success: Schema.Literal(true) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const recorded = Effect.fnUntraced(function* () {
    yield* Effect.sync(() => vi.stubEnv('BLENDER_EXTENSIONS_TOKEN', 'token'));
    const [fs, path, folders] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('PATH')]);
    const root = yield* Effect.flatMap(fs.makeTempDirectoryScoped({ prefix: 'nx-publish-' }), fs.realPath);
    const log = path.join(root, 'started.log');
    const script = (tool: string) => [`#!/bin/sh\necho "${tool} $*" >> '${log}'`, `case "${tool} $* " in "gh release view "*) cat '${path.join(root, HELD)}'; exit 0 ;; *" publish "*|*" upload "*) exit 0 ;; esac\nPATH='${folders}' exec ${tool} "$@"\n`].join('\n');
    yield* Effect.forEach(Record.toEntries({ ...FILES, [HELD]: '', ...Record.fromIterableWith(TOOLS, (tool) => [`bin/${tool}`, script(tool)]) }), ([file, text]) => Effect.andThen(fs.makeDirectory(path.join(root, path.dirname(file)), { recursive: true }), fs.writeFileString(path.join(root, file), text)), {
        discard: true,
    });
    yield* Effect.forEach(TOOLS, (tool) => fs.chmod(path.join(root, 'bin', tool), EXECUTABLE), { discard: true });
    vi.stubEnv('PATH', [path.join(root, 'bin'), folders].join(delimiter));
    const context: ExecutorContext = {
        root,
        cwd: root,
        isVerbose: false,
        projectName: PROJECT,
        targetName: 'nx-release-publish',
        nxJsonConfiguration: NX,
        projectsConfigurations: { version: 2, projects: { [PROJECT]: { root: ROOT } } },
        projectGraph: { nodes: { [PROJECT]: { name: PROJECT, type: 'lib', data: { root: ROOT } } }, dependencies: {} },
    };
    const started = fs.readFileString(log).pipe(
        Effect.map((text) => text.split('\n')),
        Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed([])),
    );
    return { context, started, published: Effect.map(started, (lines) => lines.filter((line) => PUBLISHING.test(line))), attached: Effect.map(started, (lines) => lines.filter((line) => line.startsWith(ATTACHING))) };
});

const implemented = Effect.fnUntraced(function* (plugin: string, executor: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const projects = yield* Effect.forEach(
        PUBLISHERS,
        Effect.fnUntraced(function* ({ plugin: name }) {
            const file = yield* Effect.flatMap(Effect.fromNullishOr(findPackageJSON(name, import.meta.url)), fs.realPath);
            return [name, { root: path.relative(workspaceRoot, path.dirname(file)) }] as const;
        }),
    );
    return getExecutorInformation(plugin, executor, workspaceRoot, Record.fromEntries(projects)).implementationFactory();
});

const stocked = Effect.fnUntraced(function* (data: ReadonlyArray<{ readonly id: string; readonly version: string }>) {
    const received = MutableRef.make<readonly string[]>([]);
    const answered = (request: IncomingMessage, response: ServerResponse) => {
        MutableRef.update(received, Array.append(`${request.method} ${request.url}`));
        request.resume().on('end', () => response.setHeader('content-type', 'application/json').end(JSON.stringify({ data })));
    };
    const server = yield* Effect.acquireRelease(
        Effect.callback<Server>((resume) => {
            const held = createServer(answered).listen(0, '127.0.0.1', () => resume(Effect.succeed(held)));
        }),
        (held) => Effect.sync(() => held.close().closeAllConnections()),
    );
    const address = server.address();
    const port = yield* Effect.fromNullishOr(Predicate.isString(address) ? undefined : address?.port);
    return { registry: `http://127.0.0.1:${port}`, received };
});

const packed = Effect.fnUntraced(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    yield* fs.makeDirectory(folder, { recursive: true });
    yield* Stream.make(Zip.file(MANIFEST, FILES[`${ROOT}/${MANIFEST}`])).pipe(Stream.pipeThroughChannel(Zip.encode()), Stream.run(fs.sink(path.join(folder, ARCHIVE))));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('release-publish executors', (methods) => {
    methods.effect.each(PUBLISHERS)(
        '$plugin: succeeds and starts no publish for a project nx release left without a new version',
        Effect.fnUntraced(function* ({ plugin, executor, options }) {
            const { context, published } = yield* recorded();
            const run = yield* implemented(plugin, executor);
            const result = yield* Effect.tryPromise(() => Promise.resolve(run({ ...options, nxReleaseVersionData: { [PROJECT]: { currentVersion: '0.1.0', newVersion: null, dependentProjects: [] } } }, context)));
            assert.deepStrictEqual(
                [Schema.is(Succeeded)(result), yield* published],
                [true, []],
                'nrwl/nx@2a424d3 packages/js/src/executors/release-publish/release-publish.impl.spec.ts "should skip publishing when nxReleaseVersionData indicates no new version", naxodev/oss@8b43d4a packages/nx-cloudflare-e2e/src/executors.spec.ts "runs the executors and invokes wrangler with the built argv"',
            );
        }),
    );

    methods.effect.each(STORED.filter(() => present(distribution)))(
        '@rasm/nx-distribution: posts the built package to a store that $held',
        Effect.fnUntraced(function* ({ data, posted }) {
            const path = yield* Path.Path;
            const { context } = yield* recorded();
            const { registry, received } = yield* stocked(data);
            yield* entered(context.root);
            const projectGraph = yield* graphed(context.root, [new LoadedNxPlugin(distribution.plugin, { plugin: manifest.name })]);
            const node = yield* Effect.fromNullishOr(projectGraph.nodes[PROJECT]);
            const folders = Record.keys(node.data.targets ?? {})
                .flatMap((target) => getOutputsForTargetAndConfiguration({ project: PROJECT, target }, {}, node))
                .map((output) => output.replace(GLOB, ''));
            yield* Effect.forEach(folders, (folder) => packed(path.join(context.root, folder)), { discard: true });
            const run = yield* implemented(manifest.name, 'release-publish');
            const exit = yield* Effect.exit(Effect.tryPromise(() => Promise.resolve(run({ registry }, { ...context, projectGraph, projectsConfigurations: readProjectsConfigurationFromProjectGraph(projectGraph) }))));
            assert.deepStrictEqual(
                [Exit.isSuccess(exit) && Schema.is(Succeeded)(exit.value), MutableRef.get(received).filter((line) => line.startsWith('POST '))],
                [true, posted],
                `nrwl/nx@2a424d3 packages/js/src/executors/release-publish/release-publish.impl.spec.ts "publishes when npm reports the exact version is missing", "skips publishing when the requested tag already points to the current version": ${inspect(exit, { depth: 12 })}`,
            );
        }),
    );

    methods.effect.each(RELEASED.filter(() => present(distribution)))(
        '@rasm/nx-distribution: attaches the built package to a GitHub release that $held',
        Effect.fnUntraced(function* ({ names, uploaded }) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const { context, attached } = yield* recorded();
            const { registry } = yield* stocked([{ id: PROJECT, version: '0.1.0' }]);
            yield* fs.writeFileString(path.join(context.root, HELD), names);
            yield* entered(context.root);
            const projectGraph = yield* graphed(context.root, [new LoadedNxPlugin(distribution.plugin, { plugin: manifest.name })]);
            const node = yield* Effect.fromNullishOr(projectGraph.nodes[PROJECT]);
            const folders = Array.dedupe(
                Record.keys(node.data.targets ?? {})
                    .flatMap((target) => getOutputsForTargetAndConfiguration({ project: PROJECT, target }, {}, node))
                    .map((output) => output.replace(GLOB, '')),
            );
            yield* Effect.forEach(folders, (folder) => packed(path.join(context.root, folder)), { discard: true });
            const run = yield* implemented(manifest.name, 'release-publish');
            const exit = yield* Effect.exit(Effect.tryPromise(() => Promise.resolve(run({ registry }, { ...context, projectGraph, projectsConfigurations: readProjectsConfigurationFromProjectGraph(projectGraph) }))));
            assert.deepStrictEqual(
                [Exit.isSuccess(exit) && Schema.is(Succeeded)(exit.value), yield* attached],
                [true, uploaded ? [[ATTACHING.trim(), TAG, ...folders.map((folder) => path.join(folder, ARCHIVE))].join(' ')] : []],
                `release-assets/nrwl-nx/astro-docs/src/content/docs/reference/nx-json.mdoc:645-651 an independent project tags as {projectName}@{version}, automate-github-releases.mdoc:67-80 createRelease makes that tag's GitHub release, release-assets/_channels/github-releases/gh-cli/gh_release_upload.txt:6-11 \`gh release upload <tag> <files>...\` fails on an asset of the same name without --clobber: ${inspect(exit, { depth: 12 })}`,
            );
        }),
    );

    methods.effect(
        '@rasm/nx-devkit: should proceed with publishing when nxReleaseVersionData is not provided',
        Effect.fnUntraced(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const { context, published } = yield* recorded();
            yield* fs.makeDirectory(path.join(context.root, path.dirname(SDIST)), { recursive: true });
            yield* fs.writeFileString(path.join(context.root, SDIST), '');
            const run = yield* implemented(devkit.name, 'publish');
            const result = yield* Effect.tryPromise(() => Promise.resolve(run({ publish: ['uv', 'publish'] }, { ...context, projectGraph: { nodes: { [PROJECT]: { name: PROJECT, type: 'lib', data: { root: ROOT, targets: PACKAGED } } }, dependencies: {} } })));
            assert.deepStrictEqual(
                [Schema.is(Succeeded)(result), yield* published],
                [true, [`uv publish ${SDIST}`]],
                'nrwl/nx@2a424d3 packages/js/src/executors/release-publish/release-publish.impl.spec.ts "should proceed with publishing when nxReleaseVersionData is not provided", delivery/uv/docs/guides/package.md `uv publish` takes the distribution files',
            );
        }),
    );

    methods.effect(
        '@rasm/nx-devkit: fails without publishing on a project whose build wrote no file',
        Effect.fnUntraced(function* () {
            const { context, published } = yield* recorded();
            const run = yield* implemented(devkit.name, 'publish');
            const exit = yield* Effect.exit(Effect.tryPromise(() => Promise.resolve(run({ publish: ['uv', 'publish'] }, { ...context, projectGraph: { nodes: { [PROJECT]: { name: PROJECT, type: 'lib', data: { root: ROOT, targets: PACKAGED } } }, dependencies: {} } }))));
            assert.deepStrictEqual([Exit.isFailure(exit), yield* published], [true, []], `nrwl/nx@2a424d3 packages/js/src/executors/release-publish/release-publish.impl.spec.ts "fails without publishing on %s": ${inspect(exit, { depth: 12 })}`);
        }),
    );
});
