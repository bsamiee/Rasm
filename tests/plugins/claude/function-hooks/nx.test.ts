import { randomUUID } from 'node:crypto';
import { join } from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { layerUndiciOptions } from '@effect/platform-node/NodeHttpClient';
import { layer as NodeServices } from '@effect/platform-node/NodeServices';
import { Decision } from '@rasm/function-hooks/rules/decision';
import { it as workspaceTest } from '@rasm/testing/nx';
import { Cause, Context, Effect, FileSystem, Layer, ManagedRuntime, Path, Schema, Stream } from 'effect';
import { HttpClient, HttpClientRequest, HttpClientResponse } from 'effect/http';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { expect } from 'vitest';
import { refusing } from '#/engine';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVER = fileURLToPath(import.meta.resolve('@rasm/function-hooks/claude/server'));
const DATA = '.nx/workspace-data';
const HELP = (command: string) => ({ help: { command, example: {} } });
const FILES = {
    'nx.json': '{}',
    'notes/plan.md': '',
    'suite/project.json': JSON.stringify({
        name: 'suite',
        targets: {
            test: { executor: 'nx:noop', dependsOn: [{ target: 'test--*', params: 'forward' }] },
            'test--vitest': { command: 'vitest run', cache: true, metadata: HELP('pnpm exec vitest run --help') },
            clean: { executor: 'nx:noop', dependsOn: [{ target: 'clean--*', params: 'forward' }] },
            'clean--tsc': { command: 'tsc --build --clean', metadata: HELP('pnpm exec tsc --build --help') },
            'clean--vitest': { command: 'vitest --clearCache', metadata: HELP('pnpm exec vitest --help') },
            serve: { executor: 'nx:noop', dependsOn: [{ target: 'serve--*', params: 'forward' }] },
            'serve--vite': { command: 'vite', continuous: true },
        },
    }),
    'e2e/playwright.config.ts': '',
    'e2e/project.json': JSON.stringify({
        name: 'e2e',
        targets: {
            test: { executor: 'nx:noop', dependsOn: [{ target: 'test--*', params: 'forward' }] },
            'test--playwright': { command: 'playwright test', options: { cwd: 'e2e' }, cache: true, metadata: HELP('pnpm exec playwright test --help') },
        },
    }),
} as const;
const REWRITTEN = [
    ['nx test suite -- -t decide', 'suite', 'nx run suite:test--vitest -- -t decide'],
    ['vitest run', 'suite', 'nx test suite'],
    ['timeout 600 nx test suite', 'suite', 'nx test suite'],
    ['timeout 600 vitest run', 'suite', 'nx test suite'],
    ['playwright test -c e2e --grep toast', '.', 'nx run e2e:test--playwright -- --grep toast'],
] as const;
const ALLOWED = [
    ['vitest run', 'notes'],
    ['timeout 30 nx serve suite', 'suite'],
    ['playwright show-report', 'e2e'],
] as const;
const REFUSED_READS = [
    ['/tmp/claude-501/x/tasks/a46c9759cea20d51a.output', "is a running agent's output"],
    ['/Users/u/.zsh_history', 'is shell history'],
] as const;

// --- [SERVICES] ------------------------------------------------------------------------

class Served extends Context.Service<Served>()('@rasm/test-function-hooks/nx.test/Served', {
    make: Effect.fnUntraced(function* (root: string) {
        const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
        yield* Effect.filterOrFail(
            spawner.exitCode(ChildProcess.make('git', ['init', '--quiet'], { cwd: root })),
            (code) => code === 0,
            (code) => new Cause.UnknownError(code, `git init exits ${code} in ${root}`),
        );
        const socket = path.join(yield* fs.makeTempDirectoryScoped(), 'decide.sock');
        const server = yield* spawner.spawn(ChildProcess.make(process.execPath, [SERVER, socket], { cwd: root, env: { NX_WORKSPACE_DATA_DIRECTORY: DATA, FUNCTION_HOOKS_LOG_TARGET: 'stderr', FUNCTION_HOOKS_LOG_LEVEL: 'warn' }, extendEnv: true, stderr: 'inherit' }));
        yield* Effect.raceFirst(
            Stream.runHead(Stream.filter(Stream.splitLines(Stream.decodeText(server.stdout)), (line) => line === socket)),
            Effect.flatMap(server.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `${SERVER} exits ${code} before it listens on ${socket}`))),
        );
        return {
            asked: (route: 'decide' | 'read', body: object) =>
                HttpClient.execute(HttpClientRequest.bodyJsonUnsafe(HttpClientRequest.post(`http://function-hooks/${route}`), body)).pipe(
                    Effect.flatMap(HttpClientResponse.matchStatus({ '2xx': ({ json }) => json, orElse: ({ status, text }) => Effect.flatMap(text, (answer) => Effect.fail(new Cause.UnknownError(answer, `${route} answers ${status}, ${answer}`))) })),
                    Effect.flatMap(Schema.decodeUnknownEffect(Decision)),
                    Effect.provide(layerUndiciOptions({ connect: { socketPath: socket } })),
                ),
        };
    }),
}) {
    static readonly layer = (root: string) => Layer.effect(this, this.make(root));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const it = workspaceTest.extend('engine', { scope: 'file' }, async ({ fileWorkspace }, { onCleanup }) => {
    const graphed = await fileWorkspace.write(FILES).graph({});
    fileWorkspace.write({ [`${DATA}/project-graph.json`]: JSON.stringify(graphed) });
    const runtime = ManagedRuntime.make(Layer.provideMerge(Served.layer(fileWorkspace.root), NodeServices));
    onCleanup(() => runtime.dispose());
    const { asked } = await runtime.runPromise(Served.use(Effect.succeed));
    return {
        decided: (command: string, folder: string) => runtime.runPromise(asked('decide', { command, cwd: join(fileWorkspace.root, folder), kind: 'foreground', session: randomUUID(), call: randomUUID() })),
        read: (file: string) => runtime.runPromise(asked('read', { file })),
    };
});

it.for(REWRITTEN)('Served engine rewrites %s in folder %s of an Nx workspace to %s, which runs unchanged when decided again', { timeout: 30_000 }, async ([command, folder, rewritten], { engine }) => {
    await expect(engine.decided(command, folder)).resolves.toMatchObject({ _tag: 'Rewrite', command: rewritten, background: false });
    await expect(engine.decided(rewritten, folder)).resolves.toStrictEqual({ _tag: 'Allow' });
});

it.for(ALLOWED)('Served engine runs %s in folder %s of an Nx workspace unchanged', { timeout: 30_000 }, async ([command, folder], { engine }) => {
    await expect(engine.decided(command, folder)).resolves.toStrictEqual({ _tag: 'Allow' });
});

it('Served engine refuses one-run arguments to an aggregate target with several tools, naming each tool target', { timeout: 30_000 }, async ({ engine }) => {
    await expect(engine.decided('nx clean suite -- --verbose', 'suite')).resolves.toSatisfy(refusing('clean--tsc,clean--vitest'), 'refused naming clean--tsc,clean--vitest');
});

it('Served engine allows a read of a shell task output', { timeout: 30_000 }, async ({ engine }) => {
    await expect(engine.read('/tmp/claude-501/x/tasks/b7k2.output')).resolves.toStrictEqual({ _tag: 'Allow' });
});

it.for(REFUSED_READS)('Served engine refuses a read of %s because it %s', { timeout: 30_000 }, async ([file, reason], { engine }) => {
    await expect(engine.read(file)).resolves.toSatisfy(refusing(reason), `refused because ${reason}`);
});
