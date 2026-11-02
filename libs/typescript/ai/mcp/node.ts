import { createServer } from 'node:http';
import { NodeHttpServer, NodeRuntime, NodeServices } from '@effect/platform-node';
import { Cause, Config, Effect, Exit, Layer, Logger, Runtime, Schema } from 'effect';
import type { McpServer } from 'effect/ai';
import { Command, Flag } from 'effect/cli';
import { FetchHttpClient, HttpRouter } from 'effect/http';
import { NetAddress } from 'effect/net';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { layerHttp } from '#/server';

// --- [COMPOSITION] ---------------------------------------------------------------------

const main = <E>(manifest: Parameters<typeof layerHttp>[0], app: Layer.Layer<never, E, McpServer.McpServer | HttpRouter.HttpRouter | NodeServices.NodeServices>): void => {
    const name = manifest.name.slice(manifest.name.lastIndexOf('/') + 1);
    const settings = {
        listen: Flag.String('listen').pipe(Flag.withFallbackConfig(Config.String('MCP_LISTEN')), Flag.withSchema(Schema.InetAddressFromString), Flag.withDescription('Endpoint address as host:port or [IPv6]:port')),
        hosts: Flag.String('allowed-host').pipe(Flag.atLeast(1), Flag.withFallbackConfig(Config.Array(Schema.String, 'MCP_ALLOWED_HOSTS')), Flag.withDefault([]), Flag.withDescription('Accepted Host header beyond loopback and bound address')),
    };
    Command.make(
        name,
        settings,
        Effect.fnUntraced(function* ({ listen: address, hosts }) {
            return yield* HttpRouter.serve(layerHttp({ ...manifest, name }, { address, hosts }, app), { disableLogger: true }).pipe(
                Layer.provide(NodeHttpServer.layerServer(createServer, { host: NetAddress.formatHost(address), port: address.port, disablePreemptiveShutdown: true })),
                Layer.launch,
                Effect.provide(Otlp.layerFromConfig({ resource: { serviceName: name, serviceVersion: manifest.version } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson])))),
            );
        }),
    ).pipe(Command.run({ version: manifest.version }), Effect.provide(NodeServices.layer), NodeRuntime.runMain({ teardown: (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit)) }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { main };
