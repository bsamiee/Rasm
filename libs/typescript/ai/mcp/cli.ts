import { createServer } from 'node:http';
import { join, parse } from 'node:path';
import process from 'node:process';
import { layer } from '@effect/platform-node/NodeAddressResolver';
import { layerAead, layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { layerConfig } from '@effect/platform-node/NodeDns';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layerHttpServices, layerServer } from '@effect/platform-node/NodeHttpServer';
import { run } from '@rasm/process';
import { Unmatched } from '@rasm/search';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Array, Cause, Config, ConfigProvider, Console, Context, Duration, Effect, flow, Layer, Option, Predicate, Record, Schema, SchemaGetter, String } from 'effect';
import { McpClient, type McpProtocol, type McpServer } from 'effect/ai';
import { Argument, CliError, Command, Flag, GlobalFlag } from 'effect/cli';
import { HttpRouter, HttpServer } from 'effect/http';
import { AddressResolver, Host, NetAddress } from 'effect/net';
import envPaths, { type Paths } from 'env-paths';
import { parse as plist } from 'plist';
import { Server } from '#/server';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FOLDERS = { logs: ['log'], state: ['data'], declarations: ['cache', 'declarations'] } as const satisfies Readonly<Record<string, readonly [keyof Paths, ...string[]]>>;

// --- [MODELS] --------------------------------------------------------------------------

const Count = Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThan(0))));
const optional = <A>(schema: Schema.Codec<A, string>) => schema.pipe(Schema.decodeTo(Schema.Option(Schema.toType(schema)), { decode: SchemaGetter.transform(Option.some<A>), encode: SchemaGetter.forbiddenEncoding }));
const setting = <const Name extends string, A>(name: Name, schema: Schema.Codec<A, string>, fallback: A, description: string) =>
    GlobalFlag.Setting(name)({ flag: Flag.String(name).pipe(Flag.withSchema(schema), Flag.withFallbackConfig(Config.schema(schema, name.split('-'))), Flag.withDefault(fallback), Flag.withDescription(description)) });
const Settings = {
    output: setting('output-dir', optional(Schema.String), Option.none(), 'Folder holding logs, state, and fetched declarations per application, the platform log, data, and cache folders when absent'),
    hosts: setting('allowed-hosts', Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), { decode: SchemaGetter.split(), encode: SchemaGetter.forbiddenEncoding })), [], 'Comma-separated Host headers accepted beyond loopback and the bound address'),
    retention: setting('retention', Schema.DurationFromString, Duration.hours(1), 'How long an idle handle and a finished execution stay'),
    limit: setting('events', Count, 256, 'Events a handle can fall behind by before the oldest drop'),
    budget: setting('event-characters', Count, 8000, 'Characters of events a result leads with, the rest waiting for the next call'),
};
const CallArguments = { tool: Argument.String('tool'), given: Argument.String('json').pipe(Argument.withSchema(Schema.fromJsonString(Schema.Record(Schema.String, Schema.Json))), Argument.withDefault({})), handle: Flag.optional(Flag.String('handle')) };

// --- [SERVICES] ------------------------------------------------------------------------

class Output extends Context.Service<Output, { readonly folder: (app: string, kind: keyof typeof FOLDERS) => string; readonly logs: string; readonly state: string }>()('@rasm/mcp/cli/Output') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = Effect.fnUntraced(function* <E, R, E2, R2, const Declared extends Parameters<typeof Command.withGlobalFlags>[1]>(
    { identifier, ...manifest }: Pick<Parameters<typeof McpServer.layerHttp>[0], 'name' | 'version' | 'description' | 'websiteUrl' | 'icons' | 'extensions' | 'cacheHints'> & { readonly identifier: string },
    { listen, instructions, settings }: { readonly listen: Option.Option<Host.HostPort>; readonly instructions: readonly string[]; readonly settings: Declared },
    app: Layer.Layer<never, E, R>,
    ending: Effect.Effect<never, E2, R2>,
) {
    const named = { ...manifest, name: yield* Server.scheme(manifest.name) };
    const name = identifier.slice(identifier.lastIndexOf('.') + 1);
    const Listen = setting('listen', optional(Schema.HostPortFromString), listen, 'Endpoint as host:port or [IPv6]:port, a host name binding every address it resolves to');
    const defaults = yield* process.platform === 'darwin'
        ? run(['defaults', 'export', identifier, '-']).pipe(Effect.flatMap(({ stdout: text }) => Effect.try({ try: (): unknown => plist(text), catch: (cause) => new Cause.UnknownError(cause, `defaults export of ${identifier} printed no property list`) })))
        : Effect.succeed({});
    const provider = ConfigProvider.fromEnv().pipe(
        ConfigProvider.nested(name),
        ConfigProvider.constantCase,
        ConfigProvider.orElse(ConfigProvider.fromUnknown(defaults).pipe(ConfigProvider.mapInput(flow(Array.span(Predicate.isString), ([keys, indices]) => [keys.join('.'), ...indices])))),
        ConfigProvider.orElse(ConfigProvider.fromEnv()),
    );
    const folders = Effect.map(Settings.output, (output) => {
        const paths = envPaths(identifier, { suffix: '' });
        const folder: Output['Service']['folder'] = (target, kind) => Option.match(output, { onNone: () => join(paths[FOLDERS[kind][0]], ...FOLDERS[kind].slice(1)), onSome: (root) => join(root, target, kind) });
        return { folder, logs: folder(name, 'logs'), state: folder(name, 'state') };
    });
    const telemetry = Effect.map(Effect.all([folders, GlobalFlag.LogLevel]), ([{ logs }, level]) => layerNodeFromConfig({ resource: { serviceName: named.name, serviceVersion: manifest.version }, target: 'file', file: join(logs, `${parse(process.execPath).name}.log`), ...Record.getSomes({ level }) }));
    const call = Command.make(
        'call',
        CallArguments,
        Effect.fnUntraced(
            function* ({ tool: wanted, given, handle }) {
                const bound = yield* Effect.fromOption(yield* Listen, () => new Cause.NoSuchElementError(`call reaches a server over HTTP, name its address with --${Listen.id}`));
                const protocol = yield* Effect.fromOption(Array.findFirst(Server.protocols, (adapter): adapter is Extract<typeof adapter, { readonly runtime: McpProtocol.StatelessRuntimeDescriptor }> => adapter.runtime._tag === 'Stateless'));
                const client = yield* McpClient.make({ name: named.name, version: manifest.version }).pipe(Effect.provideServiceEffect(McpClient.Transport, McpClient.http({ url: `http://${Host.formatHostPort(bound)}/`, protocol })));
                const tool = yield* Unmatched.single(yield* client.listTools(), 'tool', Option.some(wanted), (each) => [each.name]);
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
                const [address, { output, hosts, ...transport }, outputFolders, resolver] = yield* Effect.all([Listen, Effect.all(Settings), folders, AddressResolver.AddressResolver]);
                yield* Effect.sync(() => Object.assign(process.env, Record.getSomes({ [String.constantCase(`${name}-${Settings.output.id}`)]: output })));
                const appLayer = Layer.provide(app, Layer.succeed(Output)(outputFolders));
                const [handler, addresses] = yield* Option.match(address, {
                    onNone: () => Effect.all([HttpRouter.toHttpEffect(Server.layerStdio(named, { ...transport, instructions }, appLayer)), Effect.succeed([])]),
                    onSome: (bound) => Effect.all([HttpRouter.toHttpEffect(Server.layerHttp(named, { ...transport, instructions, hosts, address: bound }, appLayer)), resolver.resolve(bound)]),
                });
                yield* Effect.forEach(addresses, (resolved) => HttpServer.serve(handler).pipe(HttpServer.withLogAddress, Layer.provide(layerServer(createServer, { host: NetAddress.formatHost(resolved), port: resolved.port, disablePreemptiveShutdown: true })), Layer.build), { discard: true });
                return yield* Effect.never;
            },
            Effect.scoped,
            Effect.raceFirst(ending),
            Effect.provide([Layer.unwrap(telemetry), layerAead, layerSecureRandom, layerHttpServices, Layer.provide(layer, layerConfig({}))]),
        ),
    ).pipe(Command.withSubcommands([call]), Command.withGlobalFlags([Listen, ...Object.values(Settings), ...settings]), Command.run({ version: manifest.version }), Effect.provideService(ConfigProvider.ConfigProvider, provider));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Output, serve, setting };
