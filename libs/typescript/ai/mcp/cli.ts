import { createServer } from 'node:http';
import { join, parse } from 'node:path';
import process from 'node:process';
import { layer } from '@effect/platform-node/NodeAddressResolver';
import { layerAead, layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { layerConfig } from '@effect/platform-node/NodeDns';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layerServer } from '@effect/platform-node/NodeHttpServer';
import { run } from '@rasm/process';
import { Unmatched } from '@rasm/search';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Array, Cause, Config, ConfigProvider, Console, Context, Duration, Effect, Layer, Option, Record, Schema, SchemaGetter, String } from 'effect';
import { McpClient, McpProtocol, type McpServer } from 'effect/ai';
import { Argument, CliError, Command, Flag, GlobalFlag } from 'effect/cli';
import { HttpRouter, HttpServer } from 'effect/http';
import { AddressResolver, Host, NetAddress } from 'effect/net';
import envPaths, { type Paths } from 'env-paths';
import { parse as plist } from 'plist';
import { Server } from '#/server';

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
    budget: setting('event-characters', 'event.characters', Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThan(0)))), 8000, 'Characters of events a result leads with, the rest waiting for the next call'),
};
const Called = { tool: Argument.String('tool'), given: Argument.String('json').pipe(Argument.withSchema(Schema.fromJsonString(Schema.Record(Schema.String, Schema.Json))), Argument.withDefault({})), handle: Flag.optional(Flag.String('handle')) };

// --- [SERVICES] ------------------------------------------------------------------------

class Output extends Context.Service<Output, { readonly folder: (app: string, kind: keyof typeof FOLDERS) => string; readonly logs: string }>()('@rasm/mcp/cli/Output') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = Effect.fnUntraced(function* <E, R, E2, R2, const Declared extends Parameters<typeof Command.withGlobalFlags>[1]>(
    { domain, ...manifest }: Pick<Parameters<typeof McpServer.layerHttp>[0], 'name' | 'version' | 'description' | 'websiteUrl' | 'icons' | 'extensions' | 'cacheHints'> & { readonly domain: string },
    { listen, instructions, settings }: { readonly listen: Option.Option<Host.HostPort>; readonly instructions: readonly string[]; readonly settings: Declared },
    app: Layer.Layer<never, E, R>,
    ending: Effect.Effect<never, E2, R2>,
) {
    const named = { ...manifest, name: yield* Server.scheme(manifest.name) };
    const Listen = setting('listen', 'listen', present(Schema.HostPortFromString), listen, 'Endpoint as host:port or [IPv6]:port, a host name binding every address it resolves to');
    const defaults = yield* process.platform === 'darwin' ? run(['defaults', 'export', domain, '-']).pipe(Effect.flatMap(({ stdout: text }) => Effect.try({ try: (): unknown => plist(text), catch: (cause) => new Cause.UnknownError(cause, `defaults export of ${domain} printed no property list`) }))) : Effect.succeed({});
    const provider = ConfigProvider.fromEnv().pipe(ConfigProvider.constantCase, ConfigProvider.nested(String.constantCase(named.name)), ConfigProvider.orElse(ConfigProvider.fromUnknown(defaults).pipe(ConfigProvider.mapInput((path) => [path.join('.')]))), ConfigProvider.orElse(ConfigProvider.fromEnv()));
    const listened = Effect.fnUntraced(
        function* (transport: Parameters<typeof Server.layerHttp>[1], served: Layer.Layer<never, E, Exclude<R, Output>>) {
            const [handler, addresses] = yield* Effect.all([HttpRouter.toHttpEffect(Server.layerHttp(named, transport, served)), AddressResolver.AddressResolver.use((resolver) => resolver.resolve(transport.address))]);
            yield* Effect.forEach(addresses, (resolved) => HttpServer.serve(handler).pipe(HttpServer.withLogAddress, Layer.provide(layerServer(createServer, { host: NetAddress.formatHost(resolved), port: resolved.port, disablePreemptiveShutdown: true })), Layer.build), { discard: true });
        },
        Layer.effectDiscard,
        Layer.provide(Layer.provide(layer, layerConfig({}))),
        Layer.launch,
    );
    const located = Effect.map(Settings.output, (output) => {
        const folder: Output['Service']['folder'] = (target, kind) => Option.match(output, { onNone: () => join(envPaths(named.name, { suffix: '' })[FOLDERS[kind][0]], FOLDERS[kind][1]), onSome: (root) => join(root, target, kind) });
        return { folder, logs: folder(named.name, 'logs') };
    });
    const telemetry = Effect.map(Effect.all([located, GlobalFlag.LogLevel]), ([{ logs }, level]) =>
        layerNodeFromConfig({ resource: { serviceName: named.name, serviceVersion: manifest.version }, target: 'file', file: join(logs, `${parse(process.execPath).name}.log`), ...Option.getOrUndefined(Option.map(level, (minimum) => ({ level: minimum }))) }),
    );
    const call = Command.make(
        'call',
        Called,
        Effect.fnUntraced(
            function* ({ tool: wanted, given, handle }) {
                const bound = yield* Effect.fromOption(yield* Listen, () => new Cause.NoSuchElementError('call reaches a server over HTTP, name its address with --listen'));
                const client = Context.get(yield* Layer.build(McpClient.layerHttp({ name: named.name, version: manifest.version, url: `http://${Host.formatHostPort(bound)}/`, protocol: McpProtocol.v2026_07_28 })), McpClient.McpClient);
                const tool = yield* Unmatched.single(yield* client.listTools(), 'tool', Option.some(wanted), ({ name }) => [name]);
                const result = yield* client.callTool({ tool, arguments: { ...given, ...Record.getSomes({ handle }) } });
                const text = Array.flatMap(result.content, (block) => (block.type === 'text' ? [block.text] : [])).join('\n');
                return yield* result.isError === true ? CliError.UserError.make({ cause: result, userMessage: text }) : Console.log(text);
            },
            Effect.scoped,
            Effect.provide(layerUndici),
        ),
    );
    return yield* Command.make(
        named.name,
        {},
        Effect.fnUntraced(
            function* () {
                const [address, { output, hosts, ...transport }, folders] = yield* Effect.all([Listen, Effect.all(Settings), located]);
                yield* Effect.sync(() => Object.assign(process.env, Record.getSomes({ [String.constantCase(`${named.name}.${OUTPUT}`)]: output })));
                const served = Layer.provide(app, Layer.succeed(Output)(folders));
                return yield* Option.match(address, {
                    onNone: () => Layer.launch(Layer.effectDiscard(HttpRouter.toHttpEffect(Server.layerStdio(named, { ...transport, instructions }, served)))),
                    onSome: (bound) => listened({ ...transport, instructions, hosts, address: bound }, served),
                });
            },
            Effect.raceFirst(ending),
            Effect.provide([Layer.unwrap(telemetry), layerAead, layerSecureRandom]),
        ),
    ).pipe(Command.withSubcommands([call]), Command.withGlobalFlags([Listen, ...Object.values(Settings), ...settings]), Command.run({ version: manifest.version }), Effect.provideService(ConfigProvider.ConfigProvider, provider));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Output, serve, setting };
