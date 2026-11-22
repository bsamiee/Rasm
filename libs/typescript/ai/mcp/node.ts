import { createServer } from 'node:http';
import { join, parse } from 'node:path';
import process from 'node:process';
import { layer } from '@effect/platform-node/NodeAddressResolver';
import { layerAead, layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { layerConfig } from '@effect/platform-node/NodeDns';
import { layerServer } from '@effect/platform-node/NodeHttpServer';
import { run } from '@rasm/process';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Cause, Config, ConfigProvider, Context, Duration, Effect, Layer, Option, Record, Schema, SchemaGetter, String } from 'effect';
import type { McpServer } from 'effect/ai';
import { Command, Flag, GlobalFlag } from 'effect/cli';
import { HttpRouter, HttpServer } from 'effect/http';
import { AddressResolver, type Host, NetAddress } from 'effect/net';
import envPaths, { type Paths } from 'env-paths';
import { parse as plist } from 'plist';
import { Server } from '#/server.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FOLDERS = { logs: ['log', ''], captures: ['cache', 'captures'], media: ['cache', 'media'], declarations: ['cache', 'declarations'] } as const satisfies Readonly<Record<string, readonly [keyof Paths, string]>>;
const OUTPUT = 'output.dir';

// --- [MODELS] --------------------------------------------------------------------------

const present = <A>(schema: Schema.Codec<A, string>) => schema.pipe(Schema.decodeTo(Schema.Option(Schema.toType(schema)), { decode: SchemaGetter.transform(Option.some<A>), encode: SchemaGetter.forbiddenEncoding }));
const setting = <const Name extends string, A>(name: Name, key: string, schema: Schema.Codec<A, string>, fallback: A, description: string) =>
    GlobalFlag.Setting(name)({ flag: Flag.String(name).pipe(Flag.withSchema(schema), Flag.withFallbackConfig(Config.schema(schema, key.split('.'))), Flag.withDefault(fallback), Flag.withDescription(description)) });
const Settings = {
    output: setting('output', OUTPUT, present(Schema.String), Option.none(), 'Folder holding logs, captures, media, and fetched declarations per application, the platform log and cache folders when absent'),
    hosts: setting('allowed-hosts', 'allowed.hosts', Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), { decode: SchemaGetter.split(), encode: SchemaGetter.forbiddenEncoding })), [], 'Comma-separated Host headers accepted beyond loopback and the bound address'),
    retention: setting('retention', 'retention', Schema.DurationFromString, Duration.hours(1), 'How long an idle handle and a finished execution stay'),
    limit: setting('events', 'events', Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThan(0)))), 256, 'Events a handle can fall behind by before the oldest drop'),
};

// --- [SERVICES] ------------------------------------------------------------------------

class Output extends Context.Service<Output, { readonly folder: (app: string, kind: keyof typeof FOLDERS) => string; readonly logs: string }>()('@rasm/mcp/node/Output') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = Effect.fnUntraced(function* <E, R, E2, R2, const Declared extends Parameters<typeof Command.withGlobalFlags>[1]>(
    { domain, ...manifest }: Pick<Parameters<typeof McpServer.layerHttp>[0], 'name' | 'version' | 'description' | 'websiteUrl' | 'icons' | 'extensions' | 'cacheHints'> & { readonly domain: string },
    { listen, instructions, settings }: { readonly listen: Option.Option<Host.HostPort>; readonly instructions: readonly string[]; readonly settings: Declared },
    app: Layer.Layer<never, E, R>,
    ending: Effect.Effect<never, E2, R2>,
) {
    const named = { ...manifest, name: yield* Schema.decodeEffect(Server.Scheme)(manifest.name) };
    const variable = [named.name, ...OUTPUT.split('.')].map(String.constantCase).join('_');
    const Listen = setting('listen', 'listen', present(Schema.HostPortFromString), listen, 'Endpoint as host:port or [IPv6]:port, a host name binding every address it resolves to');
    const defaults = yield* process.platform === 'darwin' ? run(['defaults', 'export', domain, '-']).pipe(Effect.flatMap(({ stdout: text }) => Effect.try({ try: (): unknown => plist(text), catch: (cause) => new Cause.UnknownError(cause, `defaults export of ${domain} printed no property list`) }))) : Effect.succeed({});
    const provider = ConfigProvider.fromEnv().pipe(ConfigProvider.constantCase, ConfigProvider.nested(String.constantCase(named.name)), ConfigProvider.orElse(ConfigProvider.fromUnknown(defaults).pipe(ConfigProvider.mapInput((path) => [path.join('.')]))), ConfigProvider.orElse(ConfigProvider.fromEnv()));
    const listened = Effect.fnUntraced(
        function* (bound: Host.HostPort, transport: Omit<Parameters<typeof Server.layerHttp>[1], 'address'>, served: Layer.Layer<never, E, R | Output>, output: Output['Service']) {
            const [handler, addresses] = yield* Effect.all([HttpRouter.toHttpEffect(Server.layerHttp(named, { ...transport, address: bound }, Layer.provide(served, Layer.succeed(Output)(output)))), AddressResolver.AddressResolver.use((resolver) => resolver.resolve(bound))]);
            yield* Effect.forEach(addresses, (resolved) => HttpServer.serve(handler).pipe(HttpServer.withLogAddress, Layer.provide(layerServer(createServer, { host: NetAddress.formatHost(resolved), port: resolved.port, disablePreemptiveShutdown: true })), Layer.build), { discard: true });
        },
        Layer.effectDiscard,
        Layer.provide(Layer.provide(layer, layerConfig({}))),
        Layer.launch,
    );
    const located = Effect.map(Settings.output, (output) => {
        const folder = (target: string, kind: keyof typeof FOLDERS) => Option.match(output, { onNone: () => join(envPaths(named.name, { suffix: '' })[FOLDERS[kind][0]], FOLDERS[kind][1]), onSome: (root) => join(root, target, kind) });
        return { folder, logs: folder(named.name, 'logs') };
    });
    const telemetry = Effect.map(Effect.all([located, GlobalFlag.LogLevel]), ([{ logs }, level]) =>
        layerNodeFromConfig({ resource: { serviceName: named.name, serviceVersion: manifest.version }, target: 'file', file: join(logs, `${parse(process.execPath).name}.log`), ...Option.getOrUndefined(Option.map(level, (minimum) => ({ level: minimum }))) }),
    );
    return yield* Command.make(
        named.name,
        {},
        Effect.fnUntraced(
            function* () {
                const [address, { output, hosts, retention, limit }, folders] = yield* Effect.all([Listen, Effect.all(Settings), located]);
                yield* Effect.sync(() => Object.assign(process.env, Record.getSomes({ [variable]: output })));
                const transport = { instructions, retention, limit };
                return yield* Option.match(address, {
                    onNone: () => Layer.launch(Layer.effectDiscard(HttpRouter.toHttpEffect(Server.layerStdio(named, transport, Layer.provide(app, Layer.succeed(Output)(folders)))))),
                    onSome: (bound) => listened(bound, { ...transport, hosts }, app, folders),
                });
            },
            Effect.raceFirst(ending),
            Effect.provide([Layer.unwrap(telemetry), layerAead, layerSecureRandom]),
        ),
    ).pipe(Command.withGlobalFlags([Listen, ...Object.values(Settings), ...settings]), Command.run({ version: manifest.version }), Effect.provideService(ConfigProvider.ConfigProvider, provider));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Output, serve, setting };
