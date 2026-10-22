import { createServer } from 'node:http';
import { NodeHttpServer, NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Config, Effect, Exit, Layer, Logger, LogLevel, References, Runtime, Schema } from 'effect';
import type { McpServer } from 'effect/ai';
import { Command, Flag } from 'effect/cli';
import { FetchHttpClient, HttpRouter } from 'effect/http';
import { NetAddress } from 'effect/net';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { layerHttp } from './server.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const main = <E>(manifest: { readonly name: string; readonly version: string; readonly description: string }, app: Layer.Layer<never, E, McpServer.McpServer | HttpRouter.HttpRouter | NodeServices.NodeServices>): void => {
    const name = manifest.name.slice(manifest.name.lastIndexOf('/') + 1);
    const settings = {
        listen: Flag.String('listen').pipe(Flag.withFallbackConfig(Config.String('MCP_LISTEN')), Flag.mapTryCatch(NetAddress.inetAddressFromStringUnsafe, String), Flag.withDescription('Socket address the Streamable HTTP endpoint listens on, as host:port or [IPv6]:port')),
        hosts: Flag.String('allowed-host').pipe(Flag.atLeast(1), Flag.withFallbackConfig(Config.Array(Schema.String, 'MCP_ALLOWED_HOSTS')), Flag.withDefault([]), Flag.withDescription('Host authority answered beside loopback names and the bound address')),
        log: Flag.Literals('log', LogLevel.values).pipe(Flag.withFallbackConfig(Config.LogLevel('MCP_LOG')), Flag.withDefault('Info' as const), Flag.withDescription('Minimum log level')),
    };
    Command.make(
        name,
        settings,
        Effect.fnUntraced(function* ({ listen: address, hosts, log }) {
            return yield* HttpRouter.serve(Layer.provide(app, layerHttp({ ...manifest, name }, { address, hosts })), { disableLogger: true }).pipe(
                Layer.provide(NodeHttpServer.layerServer(createServer, { host: NetAddress.formatHost(address), port: address.port, disablePreemptiveShutdown: true })),
                Layer.launch,
                Effect.provide(Otlp.layerFromConfig({ resource: { serviceName: name, serviceVersion: manifest.version } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson])))),
                Effect.provideService(References.MinimumLogLevel, log),
            );
        }),
    ).pipe(Command.run({ version: manifest.version }), Effect.provide(NodeServices.layer), NodeRuntime.runMain({ teardown: (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit)) }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { main };
