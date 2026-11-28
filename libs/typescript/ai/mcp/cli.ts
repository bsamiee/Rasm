import { channel } from 'node:diagnostics_channel';
import { createServer } from 'node:http';
import type { ListenOptions } from 'node:net';
import { join, parse } from 'node:path';
import process from 'node:process';
import { layer as addressResolver } from '@effect/platform-node/NodeAddressResolver';
import { layerAead, layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { layer as dns } from '@effect/platform-node/NodeDns';
import { layerNodeHttpOptions } from '@effect/platform-node/NodeHttpClient';
import { layerHttpServices, layerServer } from '@effect/platform-node/NodeHttpServer';
import { Unmatched } from '@rasm/search/unmatched';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Array, Cause, Config, ConfigProvider, Console, Context, Deferred, Duration, Effect, FileSystem, Function, flow, Layer, Match, Option, Record, Schema, SchemaGetter, Stream, String, Struct } from 'effect';
import { McpClient, type McpProtocol, McpSchema, type McpServer } from 'effect/ai';
import { Argument, CliError, CliOutput, Command, Flag, GlobalFlag, type HelpDoc } from 'effect/cli';
import { HttpBody, HttpRouter, HttpServer, HttpServerRequest, HttpServerResponse, Mime } from 'effect/http';
import { AddressResolver, Host, NetAddress } from 'effect/net';
import envPaths, { type Paths } from 'env-paths';
import { Server } from '#/server';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FOLDERS = { logs: ['log'], declarations: ['cache', 'declarations'], results: ['cache', 'results'] } as const satisfies Readonly<Record<string, readonly [keyof Paths, ...string[]]>>;

// --- [MODELS] --------------------------------------------------------------------------

const Count = Schema.FiniteFromString.check(Schema.isInt(), Schema.isGreaterThan(0));
const Subscription = Schema.fromJsonString(Schema.Struct({ method: Schema.Literal('subscriptions/listen') }));
const setting = <const Name extends string, A, B>(name: Name, schema: Schema.Codec<A, string>, close: (flag: Flag.Flag<A>) => Flag.Flag<B>, description: string) =>
    GlobalFlag.Setting(name)({ flag: close(Flag.String(name).pipe(Flag.withSchema(schema), Flag.withFallbackConfig(Config.schema(schema, name.split('-'))), Flag.withDescription(description))) });
const Settings = {
    output: setting('output-dir', Schema.String, Flag.optional, 'Folder holding logs, fetched declarations, and call results per application, the platform log and cache folders when absent'),
    state: setting('state-dir', Schema.String, Flag.optional, 'Folder holding state that outlives a run, the platform data folder when absent'),
    hosts: setting('allowed-hosts', Schema.String.pipe(Schema.decodeTo(Schema.Array(Schema.String), { decode: SchemaGetter.split(), encode: SchemaGetter.forbiddenEncoding })), Flag.withDefault([]), 'Comma-separated Host headers accepted beyond loopback and the bound address'),
    retention: setting('retention', Schema.DurationFromString, Flag.withDefault(Duration.hours(1)), 'How long an idle handle and a finished execution stay'),
    limit: setting('events', Count, Flag.withDefault(256), 'Events a handle can fall behind by before the oldest drop'),
    budget: setting('event-characters', Count, Flag.withDefault(8000), 'Characters of events a result leads with, the rest waiting for the next call'),
};
const CallArguments = {
    tool: Argument.String('tool').pipe(Argument.withDescription('Tool to call, one listed under TOOLS')),
    given: Argument.String('json').pipe(Argument.withSchema(Schema.fromJsonString(Schema.Record(Schema.String, Schema.Json))), Argument.withDefault({}), Argument.withDescription('Tool parameters as one JSON object')),
    handle: Flag.optional(Flag.String('handle').pipe(Flag.withDescription('Handle from an earlier result, carrying its state into this call'))),
};

// --- [SERVICES] ------------------------------------------------------------------------

class Output extends Context.Service<Output, { readonly folder: (app: string, kind: keyof typeof FOLDERS) => string; readonly logs: string; readonly state: string }>()('@rasm/mcp/cli/Output') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = Effect.fnUntraced(function* <E, R, E2, R2, const Declared extends Parameters<typeof Command.withGlobalFlags>[1]>(
    { identifier, ...manifest }: Pick<Parameters<typeof McpServer.layerHttp>[0], 'name' | 'version' | 'description' | 'websiteUrl' | 'icons' | 'extensions' | 'cacheHints'> & { readonly identifier: string },
    { instructions, settings }: { readonly instructions: readonly string[]; readonly settings: Declared },
    { address: declared, descriptors, share, provider }: { readonly address: Option.Option<Host.HostPort>; readonly descriptors: Option.Option<readonly number[]>; readonly share: Option.Option<Duration.Duration>; readonly provider: ConfigProvider.ConfigProvider },
    app: Layer.Layer<never, E, R>,
    ending: Effect.Effect<never, E2, R2>,
) {
    const [named, files] = yield* Effect.all([Effect.map(Server.scheme(manifest.name), (scheme) => ({ ...manifest, name: scheme })), FileSystem.FileSystem]);
    const name = identifier.slice(identifier.lastIndexOf('.') + 1);
    const Listen = setting('listen', Schema.HostPortFromString, (flag) => Flag.optional(flag).pipe(Flag.map(Option.orElse(() => declared))), 'Endpoint as host:port or [IPv6]:port, a host name binding every address it resolves to, the address its service manager listens on when absent');
    const folders = Effect.map(Effect.all([Settings.output, Settings.state]), ([output, state]) => {
        const paths = envPaths(identifier, { suffix: '' });
        const folder: Output['Service']['folder'] = (target, kind) => Option.match(output, { onNone: () => join(paths[FOLDERS[kind][0]], ...FOLDERS[kind].slice(1)), onSome: (root) => join(root, target, kind) });
        return { folder, logs: folder(name, 'logs'), state: Option.getOrElse(state, () => paths.data) };
    });
    const telemetry = Effect.map(Effect.all([folders, GlobalFlag.LogLevel]), ([{ logs }, level]) => layerNodeFromConfig({ resource: { serviceName: identifier, serviceVersion: manifest.version }, target: 'file', file: join(logs, `${parse(process.execPath).name}.log`), ...Record.getSomes({ level }) }));
    const [environment, protocol] = [yield* Effect.context<Command.Environment>(), Array.findFirst(Server.protocols, (adapter): adapter is Extract<typeof adapter, { readonly runtime: McpProtocol.StatelessRuntimeDescriptor }> => adapter.runtime._tag === 'Stateless')];
    const connect = (bound: Host.HostPort) => Effect.flatMap(Effect.fromOption(protocol), (stateless) => McpClient.make({ name: named.name, version: manifest.version }).pipe(Effect.provideServiceEffect(McpClient.Transport, McpClient.http({ url: `http://${Host.formatHostPort(bound)}/`, protocol: stateless }))));
    const call = Command.make(
        'call',
        { ...CallArguments, address: Listen.flag },
        Effect.fnUntraced(
            function* ({ tool: wanted, given, handle, address }) {
                const bound = yield* Effect.fromOption(address, () => new Cause.NoSuchElementError(`call reaches a server over HTTP, name its address with --${Listen.id}`));
                const [client, { folder }] = yield* Effect.all([connect(bound), folders]);
                const tool = yield* Unmatched.single(yield* client.listTools(), 'tool', Option.some(wanted), (each) => [each.name]);
                const result = yield* client.callTool({ tool, arguments: { ...given, ...Record.getSomes({ handle }) } });
                const results = folder(name, 'results');
                const save = (data: Uint8Array, mimeType: string | undefined) =>
                    files.makeDirectory(results, { recursive: true }).pipe(
                        Effect.andThen(files.makeTempFile({ directory: results, prefix: `${tool.name}-`, suffix: Option.getOrUndefined(Option.map(Option.flatMap(Option.fromUndefinedOr(mimeType), Mime.getExtension), (extension) => `.${extension}`)) })),
                        Effect.tap((path) => files.writeFile(path, data)),
                    );
                const lines = yield* Effect.forEach(
                    result.content,
                    Match.type<McpSchema.ContentBlock>().pipe(
                        Match.discriminatorsExhaustive('type')({
                            text: ({ text }) => Effect.succeed(text),
                            image: ({ data, mimeType }) => save(data, mimeType),
                            audio: ({ data, mimeType }) => save(data, mimeType),
                            resource: ({ resource }) => (Schema.is(McpSchema.TextResourceContents)(resource) ? Effect.succeed(resource.text) : save(resource.blob, resource.mimeType)),
                            resource_link: ({ uri }) => Effect.succeed(uri),
                        }),
                    ),
                );
                const text = lines.join('\n');
                return yield* result.isError === true ? CliError.UserError.make({ cause: result, userMessage: text }) : Console.log(text);
            },
            Effect.scoped,
            Effect.provide(layerNodeHttpOptions({ keepAlive: true })),
        ),
    ).pipe(Command.withDescription('Calls one tool of the running server and prints its result blocks in order, an image or other binary block as the path of a file holding it'));
    const described = Effect.fnUntraced(function* (doc: HelpDoc.HelpDoc) {
        const [, address] = yield* Listen.flag.parse({ flags: {}, arguments: [] });
        const offered = yield* address.pipe(
            Effect.fromOption,
            Effect.flatMap(connect),
            Effect.flatMap((client) => client.listTools()),
            Effect.scoped,
            Effect.provide(layerNodeHttpOptions({ keepAlive: true })),
            Effect.option,
        );
        const rows = Option.map(
            offered,
            Array.map((tool) => ({ name: tool.name, alias: undefined, shortDescription: undefined, description: Array.headNonEmpty(String.split(tool.description ?? '', '\n')) })),
        );
        return Option.match(rows, {
            onNone: () => ({ ...doc, description: [doc.description, Option.match(address, { onNone: () => `No server address is set, name it with --${Listen.id}`, onSome: (bound) => `No server answers at ${Host.formatHostPort(bound)}, run ${named.name} with no subcommand to start one` })].join('\n  ') }),
            onSome: (listed) => Array.match(listed, { onEmpty: () => doc, onNonEmpty: (commands) => ({ ...doc, subcommands: [{ group: 'TOOLS', commands }] }) }),
        });
    });
    const presenter: CliOutput.Presenter = {
        present: (event) =>
            event._tag !== 'Version' && Option.contains(Array.last(event.commandPath), call.name)
                ? described(event.helpDoc).pipe(
                      Effect.orElseSucceed(() => event.helpDoc),
                      Effect.provideContext(environment),
                      Effect.flatMap((helpDoc) => CliOutput.defaultPresenter.present({ ...event, helpDoc })),
                  )
                : CliOutput.defaultPresenter.present(event),
    };
    return yield* Command.make(
        named.name,
        { address: Listen.flag },
        Effect.fnUntraced(
            function* ({ address }) {
                const [{ output, state, hosts, ...transport }, located, resolver, draining] = yield* Effect.all([Effect.all(Settings), folders, AddressResolver.AddressResolver, Deferred.make<void>()]);
                const exported = Record.getSomes(Record.mapKeys({ [Settings.output.id]: output, [Settings.state.id]: state }, (id) => String.constantCase(`${name}-${id}`)));
                yield* Effect.sync(() => Object.assign(process.env, exported));
                const appLayer = Layer.provide(app, Layer.succeed(Output)(located));
                const handler = yield* Option.match(address, {
                    onNone: () => HttpRouter.toHttpEffect(Server.layerStdio(named, { ...transport, instructions, state: located.state }, appLayer)),
                    onSome: (bound) => HttpRouter.toHttpEffect(Server.layerHttp(named, { ...transport, instructions, state: located.state, draining, hosts, address: bound }, appLayer)),
                });
                const listening: readonly ListenOptions[] = yield* Option.match(descriptors, {
                    onSome: (fds) => Effect.succeed(Array.map(fds, (fd) => ({ fd }))),
                    onNone: () =>
                        Effect.map(
                            Effect.forEach(Option.toArray(address), (bound) => resolver.resolve(bound)),
                            flow(
                                Array.flatten,
                                Array.map((resolved) => ({ host: NetAddress.formatHost(resolved), port: resolved.port })),
                            ),
                        ),
                });
                const servers = Array.map(listening, (listen) => ({ listen, server: createServer() }));
                const finish = channel('http.server.response.finish');
                const closeIdle = () => {
                    for (const { server } of servers) {
                        if (!server.listening) {
                            server.closeIdleConnections();
                        }
                    }
                };
                yield* Effect.acquireRelease(
                    Effect.sync(() => finish.subscribe(closeIdle)),
                    Function.constant(Effect.sync(() => finish.unsubscribe(closeIdle))),
                );
                const served = Effect.zipWith(handler, Effect.isSuccess(HttpServerRequest.HttpServerRequest.pipe(Effect.flatMap(Struct.get('text')), Effect.flatMap(Schema.decodeEffect(Subscription)))), (response, subscribed) =>
                    subscribed && response.body._tag === 'Stream' ? HttpServerResponse.setBody(response, HttpBody.stream(Stream.interruptWhen(response.body.stream, Deferred.await(draining)), response.body.contentType, response.body.contentLength)) : response,
                );
                yield* Effect.forEach(servers, ({ listen, server }) => HttpServer.serve(served).pipe(HttpServer.withLogAddress, Layer.provide(layerServer(Function.constant(server), { ...listen, ...Record.getSomes({ gracefulShutdownTimeout: share }) })), Layer.build), {
                    discard: true,
                });
                yield* Effect.addFinalizer(() => Effect.asVoid(Deferred.succeed(draining, undefined)));
                return yield* ending;
            },
            Effect.scoped,
            Effect.provide([Layer.unwrap(telemetry), layerAead, layerSecureRandom, layerHttpServices, Layer.provide(addressResolver, dns)]),
        ),
    ).pipe(
        Command.withSubcommands([call]),
        Command.withGlobalFlags([...Object.values(Settings), ...settings]),
        Command.run({ version: manifest.version }),
        Effect.provideService(CliOutput.Formatter, CliOutput.defaultFormatter({ globalFlags: 'summary' })),
        Effect.provideService(CliOutput.Presenter, presenter),
        Effect.provideService(ConfigProvider.ConfigProvider, ConfigProvider.fromEnv().pipe(ConfigProvider.nested(name), ConfigProvider.constantCase, ConfigProvider.orElse(provider), ConfigProvider.orElse(ConfigProvider.fromEnv()))),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Output, serve, setting };
