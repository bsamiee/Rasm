import { createServer } from 'node:http';
import { layerServer } from '@effect/platform-node/NodeHttpServer';
import { layerTelemetry } from '@rasm/process';
import { Config, type Duration, Effect, Layer, Option, Schema } from 'effect';
import { Command, Flag, GlobalFlag } from 'effect/cli';
import { HttpRouter } from 'effect/http';
import { Host } from 'effect/net';
import { Server } from '#/server';

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = Effect.fnUntraced(function* <E, R>(
    manifest: { readonly name: string; readonly version: string; readonly description: string },
    { retention, instructions, listen }: { readonly retention: Duration.Duration; readonly instructions: readonly string[]; readonly listen?: Host.HostPort },
    app: Layer.Layer<never, E, R>,
) {
    const name = yield* Schema.decodeEffect(Server.Scheme)(manifest.name);
    const listened = Flag.String('listen').pipe(Flag.withSchema(Schema.HostPortFromString), Flag.withDescription('Endpoint as host:port or [IPv6]:port, a host name listening on the first address it resolves to'));
    const settings = {
        listen: listen === undefined ? listened : Flag.withDefault(listened, listen),
        hosts: Flag.String('allowed-host').pipe(Flag.atLeast(1), Flag.withFallbackConfig(Config.Array(Schema.String, 'MCP_ALLOWED_HOSTS')), Flag.withDefault([]), Flag.withDescription('Accepted Host header beyond loopback and bound address')),
    };
    const telemetry = Effect.map(GlobalFlag.LogLevel, (flag) => layerTelemetry({ resource: { serviceName: name, serviceVersion: manifest.version }, target: 'file', ...Option.getOrUndefined(Option.map(flag, (level) => ({ level }))) }));
    return yield* Command.make(name, settings, ({ listen: address, hosts }) =>
        HttpRouter.serve(Server.layerHttp({ ...manifest, name }, { address, hosts, retention, instructions }, app), { disableLogger: true }).pipe(
            Layer.provide(layerServer(createServer, { host: Host.formatHost(address.host), port: address.port, disablePreemptiveShutdown: true })),
            Layer.provide(Layer.unwrap(telemetry)),
            Layer.launch,
        ),
    ).pipe(Command.run({ version: manifest.version }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { serve };
