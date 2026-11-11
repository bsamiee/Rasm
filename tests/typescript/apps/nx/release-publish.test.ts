import { createServer, type IncomingMessage, type Server, type ServerResponse } from 'node:http';
import { findPackageJSON } from 'node:module';
import { delimiter } from 'node:path';
import { inspect } from 'node:util';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it, vi } from '@effect/vitest';
import { type ExecutorContext, getOutputsForTargetAndConfiguration, readProjectsConfigurationFromProjectGraph, workspaceRoot } from '@nx/devkit';
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
const TOOLS = ['cargo', 'uv', 'swift', 'xcrun', 'blender'];
const GLOB = /\/?[*{].*$/u;
const PUBLISHING = /\spublish(?:\s|$)/u;
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
const distribution: Subject = { plugin: { name: manifest.name, createNodes }, tools: ['blender'], files: {} };
const PUBLISHERS = [
    { plugin: '@rasm/nx-cargo', options: { cwd: '.', package: PROJECT } },
    { plugin: '@rasm/nx-distribution', options: {} },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Succeeded = Schema.Struct({ success: Schema.Literal(true) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const recorded = Effect.fnUntraced(function* () {
    yield* Effect.sync(() => vi.stubEnv('BLENDER_EXTENSIONS_TOKEN', 'token'));
    const [fs, path, folders] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('PATH')]);
    const root = yield* Effect.flatMap(fs.makeTempDirectoryScoped({ prefix: 'nx-publish-' }), fs.realPath);
    const log = path.join(root, 'started.log');
    const script = (tool: string) => [`#!/bin/sh\necho "${tool} $*" >> '${log}'`, `case "${tool} $* " in *" publish "*) exit 0 ;; esac\nPATH='${folders}' exec ${tool} "$@"\n`].join('\n');
    yield* Effect.forEach(Record.toEntries({ ...FILES, ...Record.fromIterableWith(TOOLS, (tool) => [`bin/${tool}`, script(tool)]) }), ([file, text]) => Effect.andThen(fs.makeDirectory(path.join(root, path.dirname(file)), { recursive: true }), fs.writeFileString(path.join(root, file), text)), {
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
    return { context, started, published: Effect.map(started, (lines) => lines.filter((line) => PUBLISHING.test(line))) };
});

const implemented = Effect.fnUntraced(function* (plugin: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const projects = yield* Effect.forEach(
        PUBLISHERS,
        Effect.fnUntraced(function* ({ plugin: name }) {
            const file = yield* Effect.flatMap(Effect.fromNullishOr(findPackageJSON(name, import.meta.url)), fs.realPath);
            return [name, { root: path.relative(workspaceRoot, path.dirname(file)) }] as const;
        }),
    );
    return getExecutorInformation(plugin, 'release-publish', workspaceRoot, Record.fromEntries(projects)).implementationFactory();
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
    yield* Stream.make(Zip.file(MANIFEST, FILES[`${ROOT}/${MANIFEST}`])).pipe(Stream.pipeThroughChannel(Zip.encode()), Stream.run(fs.sink(path.join(folder, `${PROJECT}-0.1.0.zip`))));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('release-publish executors', (methods) => {
    methods.effect.each(PUBLISHERS)(
        '$plugin: succeeds and starts no publish for a project nx release left without a new version',
        Effect.fnUntraced(function* ({ plugin, options }) {
            const { context, published } = yield* recorded();
            const run = yield* implemented(plugin);
            const result = yield* Effect.tryPromise(() => Promise.resolve(run({ ...options, nxReleaseVersionData: { [PROJECT]: { currentVersion: '0.1.0', newVersion: null, dependentProjects: [] } } }, context)));
            assert.deepStrictEqual(
                [Schema.is(Succeeded)(result), yield* published],
                [true, []],
                'nrwl/nx@2a424d3 packages/js/src/executors/release-publish/release-publish.impl.spec.ts "should skip publishing when nxReleaseVersionData indicates no new version", naxodev/oss@8b43d4a packages/nx-cloudflare-e2e/src/executors.spec.ts the argv the executor hands the tool binary',
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
            const run = yield* implemented(manifest.name);
            const exit = yield* Effect.exit(Effect.tryPromise(() => Promise.resolve(run({ registry }, { ...context, projectGraph, projectsConfigurations: readProjectsConfigurationFromProjectGraph(projectGraph) }))));
            assert.deepStrictEqual(
                [Exit.isSuccess(exit) && Schema.is(Succeeded)(exit.value), MutableRef.get(received).filter((line) => line.startsWith('POST '))],
                [true, posted],
                `nrwl/nx@2a424d3 packages/js/src/executors/release-publish/release-publish.impl.spec.ts "publishes when npm reports the exact version is missing", "skips publishing when the version already exists": ${inspect(exit, { depth: 12 })}`,
            );
        }),
    );
});
