import { createServer } from 'node:http';
import { layerServer } from '@effect/platform-node/NodeHttpServer';
import { Cause, Config, type Duration, Effect, Exit, Layer, Logger, Runtime, Schema } from 'effect';
import { Command, Flag } from 'effect/cli';
import { FetchHttpClient, HttpRouter } from 'effect/http';
import { Host } from 'effect/net';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { Server } from '#/server';

// --- [COMPOSITION] ---------------------------------------------------------------------

const teardown: Runtime.Teardown = (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit));
const serve = Object.assign(
    Effect.fnUntraced(function* <E, R>(manifest: { readonly name: string; readonly version: string; readonly description: string }, { retention, instructions, listen }: { readonly retention: Duration.Duration; readonly instructions: readonly string[]; readonly listen?: Host.HostPort }, app: Layer.Layer<never, E, R>) {
        const name = yield* Schema.decodeEffect(Server.Scheme)(manifest.name);
        const listened = Flag.String('listen').pipe(Flag.withSchema(Schema.HostPortFromString), Flag.withDescription('Endpoint as host:port or [IPv6]:port, a host name listening on the first address it resolves to'));
        const settings = {
            listen: listen === undefined ? listened : Flag.withDefault(listened, listen),
            hosts: Flag.String('allowed-host').pipe(Flag.atLeast(1), Flag.withFallbackConfig(Config.Array(Schema.String, 'MCP_ALLOWED_HOSTS')), Flag.withDefault([]), Flag.withDescription('Accepted Host header beyond loopback and bound address')),
        };
        return yield* Command.make(name, settings, ({ listen: address, hosts }) =>
            HttpRouter.serve(Server.layerHttp({ ...manifest, name }, { address, hosts, retention, instructions }, app), { disableLogger: true }).pipe(
                Layer.provide(layerServer(createServer, { host: Host.formatHost(address.host), port: address.port, disablePreemptiveShutdown: true })),
                Layer.launch,
                Effect.provide(Otlp.layerFromConfig({ resource: { serviceName: name, serviceVersion: manifest.version } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson, Logger.tracerLogger])))),
            ),
        ).pipe(Command.run({ version: manifest.version }));
    }),
    { teardown },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { serve };
