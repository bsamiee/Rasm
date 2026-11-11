import { createServer } from 'node:http';
import { layer } from '@effect/platform-node/NodeAddressResolver';
import { layerConfig } from '@effect/platform-node/NodeDns';
import { layerServer } from '@effect/platform-node/NodeHttpServer';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Config, type Duration, Effect, Layer, Option, Schema } from 'effect';
import { Command, Flag, GlobalFlag } from 'effect/cli';
import { HttpRouter, HttpServer } from 'effect/http';
import { AddressResolver, type Host, NetAddress } from 'effect/net';
import { Server } from '#/server';

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = Effect.fnUntraced(function* <E, R, E2, R2>(
    manifest: { readonly name: string; readonly version: string; readonly description: string },
    { retention, instructions, listen }: { readonly retention: Duration.Duration; readonly instructions: readonly string[]; readonly listen?: Host.HostPort },
    app: Layer.Layer<never, E, R>,
    ending: Effect.Effect<never, E2, R2>,
) {
    const name = yield* Schema.decodeEffect(Server.Scheme)(manifest.name);
    const listened = Flag.String('listen').pipe(Flag.withSchema(Schema.HostPortFromString), Flag.withDescription('Endpoint as host:port or [IPv6]:port, a host name binds every address it resolves to'));
    const settings = {
        listen: listen === undefined ? listened : Flag.withDefault(listened, listen),
        hosts: Flag.String('allowed-host').pipe(Flag.atLeast(1), Flag.withFallbackConfig(Config.Array(Schema.String, 'MCP_ALLOWED_HOSTS')), Flag.withDefault([]), Flag.withDescription('Accepted Host header beyond loopback and bound address')),
    };
    const telemetry = Effect.map(GlobalFlag.LogLevel, (flag) => layerNodeFromConfig({ resource: { serviceName: name, serviceVersion: manifest.version }, target: 'file', ...Option.getOrUndefined(Option.map(flag, (level) => ({ level }))) }));
    return yield* Command.make(
        name,
        settings,
        Effect.fnUntraced(
            function* ({ listen: address, hosts }) {
                const [handler, addresses] = yield* Effect.all([HttpRouter.toHttpEffect(Server.layerHttp({ ...manifest, name }, { address, hosts, retention, instructions }, app)), AddressResolver.AddressResolver.use((resolver) => resolver.resolve(address))]);
                yield* Effect.forEach(addresses, (bound) => HttpServer.serve(handler).pipe(HttpServer.withLogAddress, Layer.provide(layerServer(createServer, { host: NetAddress.formatHost(bound), port: bound.port, disablePreemptiveShutdown: true })), Layer.build), { discard: true });
            },
            Layer.effectDiscard,
            Layer.provide(Layer.provide(layer, layerConfig({}))),
            Layer.launch,
            Effect.raceFirst(ending),
            Effect.provide(Layer.unwrap(telemetry)),
        ),
    ).pipe(Command.run({ version: manifest.version }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { serve };
