import process from 'node:process';
import { layerDigest } from '@effect/platform-node/NodeCrypto';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { ApplicationServer } from '@rasm/applications/node';
import { Peer } from '@rasm/atelier-protocol/peer';
import { serve } from '@rasm/mcp/node';
import { spawn } from '@rasm/process';
import { teardown } from '@rasm/telemetry/node';
import { Array, Cause, Config, Duration, Effect, FileSystem, identity, Layer, Option, Path, Result, Schema, ScopedRef, Sink, Stream, SubscriptionRef } from 'effect';
import { McpProtocol } from 'effect/ai';
import { Flag } from 'effect/cli';
import { Host } from 'effect/net';
import { parse } from 'plist';
import { appleEvents } from '#/features/apple-events';
import { applications } from '#/features/applications';
import { Fonts, fonts } from '#/features/fonts';
import { plugins } from '#/features/plugins';
import { typesetting } from '#/features/typesetting';
import { listening } from '#/libraries';
import manifest from '#/package.json' with { type: 'json' };
import { routes } from '#/routes';
import { status } from '#/status';

// --- [MODELS] --------------------------------------------------------------------------

const Bundle = Schema.Struct({ CFBundleExecutable: Schema.String, CFBundleShortVersionString: Schema.String }).annotate({ description: 'Info.plist of the app bundle holding this server' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const tools = Effect.gen(function* () {
    const [server, links, dependencies] = yield* Effect.all([ApplicationServer, listening, status]);
    const round = Effect.flatMap(routes(dependencies, links), (routed) => Effect.all([applications(routed, dependencies), appleEvents(routed), fonts(routed), typesetting(routed), plugins(routed, links)], { discard: true }));
    const discovered = SubscriptionRef.changes(server.installations).pipe(
        Stream.tap(Result.match({ onSuccess: () => Effect.void, onFailure: (error) => Effect.logError('Adobe application discovery failed, tools keep the last installations', error) })),
        Stream.filterMap(identity),
        Stream.map(Array.map((row) => [row.name, row.url.href] as const)),
        Stream.changes,
    );
    const [first, rest] = yield* Stream.peel(discovered, Sink.head());
    const generation = yield* ScopedRef.fromAcquire(Effect.forEach(first, () => round, { discard: true }));
    yield* Effect.forkScoped(Stream.runForEach(Stream.merge(rest, Stream.drop(links.changes, 1)), () => ScopedRef.set(generation, round)));
});
Effect.gen(function* () {
    const [path, files] = yield* Effect.all([Path.Path, FileSystem.FileSystem]);
    const contents = path.join(path.dirname(process.execPath), '..');
    const { CFBundleExecutable, CFBundleShortVersionString } = yield* files.readFileString(path.join(contents, 'Info.plist')).pipe(
        Effect.flatMap((text) => Effect.try(() => parse(text))),
        Effect.flatMap(Schema.decodeUnknownEffect(Bundle)),
    );
    return yield* serve(
        { name: manifest.name, description: manifest.description, version: CFBundleShortVersionString },
        {
            listen: Flag.String('listen').pipe(Flag.withSchema(Schema.HostPortFromString), Flag.withDefault(Host.hostPortFromStringUnsafe(Peer.link.url.host)), Flag.map(Option.some), Flag.withDescription('Endpoint as host:port or [IPv6]:port, a host name binds every address it resolves to')),
            hosts: Flag.String('allowed-host').pipe(Flag.atLeast(1), Flag.withFallbackConfig(Config.Array(Schema.String, 'MCP_ALLOWED_HOSTS')), Flag.withDefault([]), Flag.withDescription('Accepted Host header beyond loopback and bound address')),
            target: 'file',
            executions: { name: 'executions', description: 'Refused state-changing calls that keep running', retention: Duration.hours(1) },
            protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18, McpProtocol.v2025_03_26, McpProtocol.v2024_11_05],
            instructions: [
                'Specifiers come from results',
                'State-changing calls return early when a dialog requires an answer',
                'capture reads every window of a background application with no activation',
                'execute_script runs a body as written, activate in a body takes focus from the user and reports active on the call',
                'A blocking window is a new window on a blocked main thread, perform answers it with a key action, its leaving alone proves the answer, else a person answers it',
            ],
        },
        Layer.effectDiscard(tools).pipe(Layer.provideMerge(Fonts.layer), Layer.provide(layerDigest)),
        ApplicationServer.pipe(
            Effect.flatMap(({ exited }) => exited),
            Effect.flatMap((code) => Effect.fail(new Cause.UnknownError(code, `Atelier helper exited ${code}`))),
        ),
    ).pipe(Effect.provide(ApplicationServer.layer(spawn([path.join(contents, 'MacOS', CFBundleExecutable)], { stderr: 'inherit' }))));
}).pipe(Effect.provide(layer), runMain({ teardown }));
