import process from 'node:process';
import { fileURLToPath } from 'node:url';
import { layerUndiciOptions } from '@effect/platform-node/NodeHttpClient';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Cause, Effect, FileSystem, Path, Stream } from 'effect';
import { HttpClient, HttpClientRequest } from 'effect/http';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVER = fileURLToPath(import.meta.resolve('@rasm/function-hooks/server'));
const ALLOWED = { _tag: 'Allow', observes: false } as const;
const NOTICE = { _tag: 'Deny', reason: 'Task b7k2 notifies on completion, never poll its output' } as const;
const HOLDERS = {
    writer: (output: string) => ChildProcess.make('sh', ['-c', 'exec sleep 60 >> "$0"', output]),
    reader: (output: string) => ChildProcess.make('tail', ['-f', output], { stdout: 'ignore' }),
} as const;
const BROWSERS = [
    ['playwright cli close-all', { _tag: 'Deny', reason: "playwright cli close-all closes every agent's sessions, use playwright cli -s=<session> close" }],
    ['playwright cli kill-all', { _tag: 'Deny', reason: "playwright cli kill-all kills every agent's browsers, use playwright cli -s=<session> close" }],
    ['playwright cli install', { _tag: 'Deny', reason: 'playwright cli install writes a .playwright workspace in the working folder, use playwright cli install-browser <browser>' }],
    ['playwright cli install-browser webkit', ALLOWED],
    ['playwright cli list', ALLOWED],
    ['playwright install chromium', ALLOWED],
    ['playwright cli snapshot', { _tag: 'Rewrite', command: 'playwright cli -s=a1 snapshot', observes: false, background: false }],
    ['playwright cli open https://example.com', { _tag: 'Rewrite', command: 'playwright cli -s=a1 open https://example.com', observes: false, background: false }],
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = Effect.fnUntraced(function* () {
    const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const folder = yield* fs.makeTempDirectoryScoped();
    const socket = path.join(folder, 'decide.sock');
    const server = yield* spawner.spawn(ChildProcess.make(process.execPath, [SERVER, socket]));
    yield* Effect.raceFirst(
        Stream.runHead(Stream.filter(Stream.splitLines(Stream.decodeText(server.stdout)), (line) => line === socket)),
        Effect.flatMap(server.exitCode, (code) => Effect.fail(new Cause.UnknownError(code, `${SERVER} exits ${code} before it listens on ${socket}`))),
    );
    const decided = (command: string, kind: 'foreground' | 'monitor', stated: { readonly cwd?: string; readonly agent?: string } = {}) =>
        HttpClient.execute(HttpClientRequest.bodyJsonUnsafe(HttpClientRequest.post('http://function-hooks/decide'), { command, cwd: folder, kind, session: 'session', call: crypto.randomUUID(), ...stated })).pipe(
            Effect.flatMap(({ json }) => json),
            Effect.provide(layerUndiciOptions({ connect: { socketPath: socket } })),
        );
    return { folder, decided };
});

const held = Effect.fnUntraced(function* (holder: keyof typeof HOLDERS) {
    const [fs, path, spawner, { folder, decided }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, served()]);
    const output = path.join(folder, 'tasks', 'b7k2.output');
    yield* fs.makeDirectory(path.dirname(output));
    yield* fs.writeFileString(output, '');
    const { pid } = yield* spawner.spawn(HOLDERS[holder](output));
    return { output, pid, decided };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

it.live(
    'Monitor tail of a written task output waits on its writer, and the rewrite decides to itself',
    Effect.fnUntraced(
        function* () {
            const { output, pid, decided } = yield* held('writer');
            const bounded = `{ tail -f ${output} & caffeinate -w ${pid}; kill $!; }`;
            assert.deepStrictEqual([yield* decided(`tail -f ${output}`, 'monitor'), yield* decided(bounded, 'monitor')], [{ _tag: 'Rewrite', command: bounded, observes: false, background: false }, ALLOWED]);
        },
        Effect.scoped,
        Effect.provide(layer),
    ),
    30_000,
);

it.live.each([
    ['writer', [ALLOWED, NOTICE]],
    ['reader', [ALLOWED, ALLOWED]],
] as const)(
    'Repeated read of a task output its %s holds decides %j',
    Effect.fnUntraced(
        function* ([holder, decisions]) {
            const { output, decided } = yield* held(holder);
            assert.deepStrictEqual([yield* decided(`cat ${output}`, 'foreground'), yield* decided(`cat ${output}`, 'foreground')], decisions);
        },
        Effect.scoped,
        Effect.provide(layer),
    ),
    30_000,
);

it.live(
    'Playwright CLI calls of an agent outside the root stay in its session',
    Effect.fnUntraced(
        function* () {
            const { decided } = yield* served();
            const decisions = yield* Effect.forEach(BROWSERS, ([command]) => decided(command, 'foreground', { cwd: '/private/tmp', agent: 'a1' }));
            assert.deepStrictEqual(
                decisions,
                BROWSERS.map(([, decision]) => decision),
            );
        },
        Effect.scoped,
        Effect.provide(layer),
    ),
    30_000,
);

it.live(
    'Nx show piped into jq keeps its stderr out of the JSON',
    Effect.fnUntraced(
        function* () {
            const { decided } = yield* served();
            assert.deepStrictEqual(yield* decided('nx show project rasm --json 2>&1 | jq .name', 'foreground'), { _tag: 'Rewrite', command: 'set -o pipefail; nx show project rasm --json | jq .name', observes: false, background: false });
        },
        Effect.scoped,
        Effect.provide(layer),
    ),
    30_000,
);
