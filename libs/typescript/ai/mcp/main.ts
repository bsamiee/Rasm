import { createServer } from 'node:http';
import { NodeHttpServer, NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Cause, Config, Context, Effect, Exit, Layer, Logger, References, Runtime, Schema } from 'effect';
import { McpProtocol, McpServer } from 'effect/ai';
import { Command, Flag } from 'effect/cli';
import { FetchHttpClient, HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { NetAddress } from 'effect/net';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { Instructions, layer } from './server.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const main = <E>(manifest: { readonly name: string; readonly version: string; readonly description: string }, app: Layer.Layer<never, E, McpServer.McpServer | HttpRouter.HttpRouter | NodeServices.NodeServices>): void => {
    const name = manifest.name.slice(manifest.name.lastIndexOf('/') + 1);
    const settings = {
        listen: Flag.String('listen').pipe(Flag.withFallbackConfig(Config.String('MCP_LISTEN')), Flag.withDescription('Socket address the Streamable HTTP endpoint listens on, as host:port or [IPv6]:port')),
        hosts: Flag.String('allowed-host').pipe(Flag.atLeast(1), Flag.withFallbackConfig(Config.Array(Schema.String, 'MCP_ALLOWED_HOSTS')), Flag.withDefault([]), Flag.withDescription('Host authority answered beside loopback names and the bound address')),
        log: Flag.Literals('log', ['All', 'Fatal', 'Error', 'Warn', 'Info', 'Debug', 'Trace', 'None']).pipe(Flag.withFallbackConfig(Config.LogLevel('MCP_LOG')), Flag.withDefault('Info' as const), Flag.withDescription('Minimum log level')),
        otlp: Flag.Boolean('otlp').pipe(Flag.withFallbackConfig(Config.Boolean('MCP_OTLP')), Flag.withDefault(false), Flag.withDescription('Exports traces, metrics, and logs over OTLP/HTTP to the endpoints OTEL_EXPORTER_OTLP_* variables name')),
    };
    Command.make(
        name,
        settings,
        Effect.fnUntraced(function* ({ listen, hosts, log, otlp }) {
            const address = yield* Effect.fromResult(NetAddress.inetAddressFromString(listen));
            const allowed = new Set([...hosts, 'localhost', '127.0.0.1', '[::1]', ...(NetAddress.isUnspecified(address.address) ? [] : [NetAddress.formatUrlHost(address.address)])]);
            const admitted = HttpRouter.middleware((next) => Effect.flatMap(HttpServerRequest.HttpServerRequest, (request) => (allowed.has(request.headers.host ?? '') || allowed.has(URL.parse(`http://${request.headers.host}`)?.hostname ?? '') ? next : Effect.succeed(HttpServerResponse.empty({ status: 403 })))), {
                global: true,
            });
            const runtime = Layer.unwrap(
                Effect.map(McpServer.McpServer, (server) =>
                    McpServer.layerHttp({
                        ...manifest,
                        name,
                        get instructions() {
                            return Array.dedupe([manifest.description, ...[...server.tools, ...server.resources, ...server.resourceTemplates, ...server.prompts].flatMap(({ annotations }) => Context.get(annotations, Instructions))]).join('\n');
                        },
                        path: '/',
                        allowSessionTermination: true,
                        protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18, McpProtocol.v2025_03_26, McpProtocol.v2024_11_05],
                    }),
                ),
            ).pipe(Layer.provideMerge(McpServer.McpServer.layer));
            const logged = Logger.layer([Logger.consoleJson]);
            return yield* HttpRouter.serve(Layer.mergeAll(Layer.provide(app, layer(name)).pipe(Layer.provide(runtime)), admitted), { disableLogger: true }).pipe(
                Layer.provide(NodeHttpServer.layerServer(createServer, { host: NetAddress.formatHost(address), port: address.port, disablePreemptiveShutdown: true })),
                Layer.launch,
                Effect.provide(otlp ? Otlp.layerFromConfig({ resource: { serviceName: name, serviceVersion: manifest.version } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(logged)) : logged),
                Effect.provideService(References.MinimumLogLevel, log),
            );
        }),
    ).pipe(Command.run({ version: manifest.version }), Effect.provide(NodeServices.layer), NodeRuntime.runMain({ teardown: (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit)) }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { main };
