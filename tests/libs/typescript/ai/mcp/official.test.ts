import { createServer } from 'node:http';
import { layerServer } from '@effect/platform-node/NodeHttpServer';
import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import { Cause, Context, Duration, Effect, FileSystem, Layer, Match, Path, Schema, Stream } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { NetAddress } from 'effect/net';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { hosted } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SCENARIOS = [
    'server-initialize',
    'server-session-lifecycle',
    'ping',
    'tools-list',
    'tools-call-simple-text',
    'tools-call-image',
    'tools-call-embedded-resource',
    'tools-call-mixed-content',
    'tools-call-error',
    'tools-call-with-progress',
    'resources-list',
    'resources-read-text',
    'resources-read-binary',
    'resources-templates-read',
    'resources-subscribe',
    'resources-unsubscribe',
    'sep-2164-resource-not-found',
    'dns-rebinding-protection',
    'http-header-validation',
] as const;
const RUN = Duration.minutes(2);

// --- [MODELS] --------------------------------------------------------------------------

const Checks = Schema.Struct({ id: Schema.String }).pipe(Schema.Array, Schema.fromJsonString);

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = (address: NetAddress.InetAddress) => Effect.map(Effect.fromResult(NetAddress.toUrl(address)), ({ href }) => href);

const endpoint = Effect.gen(function* () {
    const listening = Context.get(yield* Layer.build(layerServer(createServer, { host: '127.0.0.1', port: 0 })), HttpServer.HttpServer);
    const url = yield* Match.valueTags(listening.address, { UnixPathAddress: Effect.die, InetAddressV4: served, InetAddressV6: served });
    const handler = yield* HttpRouter.toHttpEffect((yield* hosted(new URL(url).host)).layer);
    yield* handler.pipe(HttpServer.serve(), Layer.build, Effect.provideService(HttpServer.HttpServer, listening));
    return url;
});

const checked = Effect.fnUntraced(function* (scenario: (typeof SCENARIOS)[number]) {
    const [fs, path, spawner, url] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, endpoint]);
    const results = yield* fs.makeTempDirectoryScoped({ prefix: 'mcp-conformance-' });
    const run = yield* spawner.spawn(ChildProcess.make('conformance', ['server', '--url', url, '--scenario', scenario, '--expected-failures', path.join(import.meta.dirname, 'expected-failures.yaml'), '--output-dir', results], { cwd: results }));
    const [output, code] = yield* Effect.all([run.all.pipe(Stream.decodeText, Stream.mkString), run.exitCode], { concurrency: 'unbounded' }).pipe(
        Effect.timeoutOrElse({ duration: RUN, orElse: () => Effect.fail(new Cause.UnknownError(scenario, `conformance ${scenario} exited with no code after ${Duration.format(RUN)}`)) }),
    );
    const files = (yield* fs.readDirectory(results, { recursive: true })).filter((file) => path.basename(file) === 'checks.json');
    const checks = yield* Effect.forEach(files, (file) => Effect.flatMap(fs.readFileString(path.join(results, file)), Schema.decodeEffect(Checks)));
    return { output, code, checks: checks.flat() };
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(layer, { excludeTestServices: true, concurrent: true })('official conformance scenarios against the served endpoint', (methods) => {
    methods.effect.each(SCENARIOS)(
        '%s: every check of the scenario runs, and fails only where the expected-failures baseline lists the scenario',
        Effect.fnUntraced(function* (scenario) {
            const { output, code, checks } = yield* checked(scenario);
            assert.deepStrictEqual({ code, ran: checks.length > 0 }, { code: 0, ran: true }, output);
        }),
    );
});
