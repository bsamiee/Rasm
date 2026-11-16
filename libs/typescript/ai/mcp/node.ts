import { createServer } from 'node:http';
import { layer } from '@effect/platform-node/NodeAddressResolver';
import { layerConfig } from '@effect/platform-node/NodeDns';
import { layerServer } from '@effect/platform-node/NodeHttpServer';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Effect, Layer, Option, Schema } from 'effect';
import { Command, type Flag, GlobalFlag } from 'effect/cli';
import { HttpRouter, HttpServer } from 'effect/http';
import { AddressResolver, type Host, NetAddress } from 'effect/net';
import { Server } from '#/server';

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = Effect.fnUntraced(function* <E, R, E2, R2>(
    manifest: { readonly name: string; readonly version: string; readonly description: string },
    {
        listen,
        hosts,
        target,
        ...settings
    }: Parameters<typeof Server.layerStdio>[1] & {
        readonly listen: Flag.Flag<Option.Option<Host.HostPort>>;
        readonly hosts: Flag.Flag<readonly string[]>;
        readonly target: Parameters<typeof layerNodeFromConfig>[0]['target'];
    },
    app: Layer.Layer<never, E, R>,
    ending: Effect.Effect<never, E2, R2>,
) {
    const named = { ...manifest, name: yield* Schema.decodeEffect(Server.Scheme)(manifest.name) };
    const telemetry = Effect.map(GlobalFlag.LogLevel, (flag) => layerNodeFromConfig({ resource: { serviceName: named.name, serviceVersion: manifest.version }, target, ...Option.getOrUndefined(Option.map(flag, (level) => ({ level }))) }));
    const listened = Effect.fnUntraced(
        function* (bound: Host.HostPort, allowed: readonly string[]) {
            const [handler, addresses] = yield* Effect.all([HttpRouter.toHttpEffect(Server.layerHttp(named, { ...settings, address: bound, hosts: allowed }, app)), AddressResolver.AddressResolver.use((resolver) => resolver.resolve(bound))]);
            yield* Effect.forEach(addresses, (resolved) => HttpServer.serve(handler).pipe(HttpServer.withLogAddress, Layer.provide(layerServer(createServer, { host: NetAddress.formatHost(resolved), port: resolved.port, disablePreemptiveShutdown: true })), Layer.build), { discard: true });
        },
        Layer.effectDiscard,
        Layer.provide(Layer.provide(layer, layerConfig({}))),
        Layer.launch,
    );
    return yield* Command.make(
        named.name,
        { listen, hosts },
        Effect.fnUntraced(
            function* ({ listen: address, hosts: allowed }) {
                return yield* Option.match(address, { onNone: () => Layer.launch(Server.layerStdio(named, settings, app)), onSome: (bound) => listened(bound, allowed) });
            },
            Effect.raceFirst(ending),
            Effect.provide(Layer.unwrap(telemetry)),
        ),
    ).pipe(Command.run({ version: manifest.version }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { serve };
