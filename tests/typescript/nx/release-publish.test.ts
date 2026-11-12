import { delimiter } from 'node:path';
import { inspect } from 'node:util';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it, vi } from '@effect/vitest';
import { type ExecutorContext, workspaceRoot } from '@nx/devkit';
import devkit from '@rasm/nx-devkit/package.json' with { type: 'json' };
import { Config, Effect, Exit, FileSystem, Path, Record, Schema } from 'effect';
import type { NxJsonConfiguration } from 'nx/src/config/nx-json';
import { getExecutorInformation } from 'nx/src/devkit-internals';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROJECT = 'sheet';
const ROOT = 'packages/sheet';
const EXECUTABLE = 0o755;
const TOOLS = ['uv', 'gh'];
const SDIST = `${ROOT}/dist/${PROJECT}-0.1.0.tar.gz`;
const NX: NxJsonConfiguration = { release: { projects: [PROJECT] } };
const FILES = {
    'package.json': '{ "name": "root" }\n',
    'nx.json': JSON.stringify(NX),
    [`${ROOT}/package.json`]: `{ "name": "${PROJECT}", "version": "0.1.0" }\n`,
};
const DELIVERIES = [
    { executor: 'publish', target: 'nx-release-publish', options: { publish: ['uv', 'publish'] }, started: `uv publish ${SDIST}`, cited: 'delivery/uv/docs/guides/package.md `uv publish` takes the distribution files' },
    {
        executor: 'attach',
        target: 'attach--gh',
        options: {},
        started: `gh release upload v0.1.0 ${SDIST} --clobber`,
        cited: 'release-assets/nrwl-nx/astro-docs/src/content/docs/reference/nx-json.mdoc a fixed group tags as v{version}, release-assets/_channels/github-releases/gh-cli/gh_release_upload.txt:6-11 `gh release upload <tag> <files>... --clobber`',
    },
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Succeeded = Schema.Struct({ success: Schema.Literal(true) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const recorded = Effect.fnUntraced(function* (target: string) {
    const [fs, path, folders] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('PATH')]);
    const root = yield* Effect.flatMap(fs.makeTempDirectoryScoped({ prefix: 'nx-publish-' }), fs.realPath);
    const log = path.join(root, 'started.log');
    yield* Effect.forEach(
        Record.toEntries({ ...FILES, ...Record.fromIterableWith(TOOLS, (tool) => [`bin/${tool}`, `#!/bin/sh\necho "${tool} $*" >> '${log}'\n`]) }),
        ([file, text]) => Effect.andThen(fs.makeDirectory(path.join(root, path.dirname(file)), { recursive: true }), fs.writeFileString(path.join(root, file), text)),
        {
            discard: true,
        },
    );
    yield* Effect.forEach(TOOLS, (tool) => fs.chmod(path.join(root, 'bin', tool), EXECUTABLE), { discard: true });
    vi.stubEnv('PATH', [path.join(root, 'bin'), folders].join(delimiter));
    const context: ExecutorContext = {
        root,
        cwd: root,
        isVerbose: false,
        projectName: PROJECT,
        targetName: target,
        nxJsonConfiguration: NX,
        projectsConfigurations: { version: 2, projects: { [PROJECT]: { root: ROOT } } },
        projectGraph: { nodes: { [PROJECT]: { name: PROJECT, type: 'lib', data: { root: ROOT, targets: { build: { outputs: ['{projectRoot}/dist'] }, [target]: { dependsOn: ['build'] } } } } }, dependencies: {} },
    };
    const started = fs.readFileString(log).pipe(
        Effect.map((text) => text.split('\n').filter((line) => line !== '')),
        Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed([])),
    );
    return { context, started };
});

const implemented = (executor: string) => getExecutorInformation(devkit.name, executor, workspaceRoot, {}).implementationFactory();

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true })('release executors', (methods) => {
    methods.effect.each(DELIVERIES)(
        '$executor: runs the tool over the files the packaging target wrote',
        Effect.fnUntraced(function* ({ executor, target, options, started: expected, cited }) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const { context, started } = yield* recorded(target);
            yield* fs.makeDirectory(path.join(context.root, path.dirname(SDIST)), { recursive: true });
            yield* fs.writeFileString(path.join(context.root, SDIST), '');
            const exit = yield* Effect.exit(Effect.tryPromise(() => Promise.resolve(implemented(executor)(options, context))));
            assert.deepStrictEqual(
                [Exit.isSuccess(exit) && Schema.is(Succeeded)(exit.value), yield* started],
                [true, [expected]],
                `nrwl/nx@2a424d3 packages/js/src/executors/release-publish/release-publish.impl.spec.ts "should proceed with publishing when nxReleaseVersionData is not provided", ${cited}: ${inspect(exit, { depth: 12 })}`,
            );
        }),
    );

    methods.effect.each(DELIVERIES)(
        '$executor: fails without running the tool on a project whose packaging target wrote no file',
        Effect.fnUntraced(function* ({ executor, target, options }) {
            const { context, started } = yield* recorded(target);
            const exit = yield* Effect.exit(Effect.tryPromise(() => Promise.resolve(implemented(executor)(options, context))));
            assert.deepStrictEqual([Exit.isFailure(exit), yield* started], [true, []], `nrwl/nx@2a424d3 packages/js/src/executors/release-publish/release-publish.impl.spec.ts "fails without publishing on %s": ${inspect(exit, { depth: 12 })}`);
        }),
    );
});
