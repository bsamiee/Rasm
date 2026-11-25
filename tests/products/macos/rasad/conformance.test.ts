import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Array, Effect, FileSystem, Layer, Path, Record, Schema, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { served } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCENARIOS = ['server-initialize', 'server-session-lifecycle', 'ping', 'tools-list', 'dns-rebinding-protection', 'http-header-validation'] as const;
const TOOLS = ['applications', 'inspect', 'act', 'view', 'diagnostics', 'execute_script'] as const;
const RUN = 180_000;

// --- [MODELS] --------------------------------------------------------------------------

const Checks = Schema.fromJsonString(Schema.Array(Schema.Struct({ id: Schema.String })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const checked = Effect.fnUntraced(function* (url: string, scenario: (typeof SCENARIOS)[number]) {
    const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const results = yield* fs.makeTempDirectoryScoped({ prefix: 'rasad-conformance-' });
    const run = yield* spawner.spawn(ChildProcess.make('conformance', ['server', '--url', url, '--scenario', scenario, '--output-dir', results], { cwd: results }));
    const [output, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(run.all)), run.exitCode], { concurrency: 'unbounded' });
    const files = (yield* fs.readDirectory(results, { recursive: true })).filter((file) => path.basename(file) === 'checks.json');
    const checks = yield* Effect.forEach(files, (file) => Effect.flatMap(fs.readFileString(path.join(results, file)), Schema.decodeEffect(Checks)));
    return { scenario, code, ran: checks.flat().length > 0, output };
}, Effect.scoped);
const schemas = (client: Effect.Success<ReturnType<typeof served>>['connect']) =>
    Effect.map(
        Effect.flatMap(client, (connected) => connected.listTools()),
        (tools) =>
            Record.fromEntries(
                Array.map(
                    Array.filter(tools, ({ name }) => Array.contains(TOOLS, name)),
                    ({ name, inputSchema }) => [name, JSON.stringify(inputSchema)] as const,
                ),
            ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Layer.mergeAll(layer, layerUndici), { excludeTestServices: true })('built Rasad helper', (methods) => {
    methods.effect(
        'passes every protocol scenario of the official MCP conformance suite, and lists the six tools with the input schema Atelier lists for each',
        Effect.fnUntraced(function* () {
            const [rasad, atelier] = yield* Effect.all([served('@rasm/rasad', 'Rasad', [], []), served('@rasm/atelier', 'Atelier', [], ['--registry', '/var/empty/hdpim.db'])]);
            const [own, defined] = yield* Effect.all([schemas(rasad.connect), schemas(atelier.connect)], { concurrency: 'unbounded' });
            const runs = yield* Effect.forEach(SCENARIOS, (scenario) => checked(rasad.url, scenario), { concurrency: 'unbounded' });
            assert.deepStrictEqual(
                { scenarios: Array.map(runs, ({ scenario, code, ran }) => ({ scenario, code, ran })), tools: Record.keys(defined).toSorted(), schemas: own },
                { scenarios: Array.map(SCENARIOS, (scenario) => ({ scenario, code: 0, ran: true })), tools: [...TOOLS].toSorted(), schemas: defined },
                Array.map(
                    Array.filter(runs, ({ code }) => code !== 0),
                    ({ output }) => output,
                ).join('\n'),
            );
        }, Effect.scoped),
        RUN,
    );
});
