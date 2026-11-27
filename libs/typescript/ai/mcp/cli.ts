import { subscribe } from 'node:diagnostics_channel';
import { once } from 'node:events';
import { createServer } from 'node:http';
import { flushCompileCache } from 'node:module';
import type { ListenOptions } from 'node:net';
import { constants } from 'node:os';
import { extname, join, parse, resolve } from 'node:path';
import process from 'node:process';
import { isSea } from 'node:sea';
import { getSystemErrorMessage } from 'node:util';
import { layer as addressResolver } from '@effect/platform-node/NodeAddressResolver';
import { layerAead, layerSecureRandom } from '@effect/platform-node/NodeCrypto';
import { layer as dns } from '@effect/platform-node/NodeDns';
import { layerUndici } from '@effect/platform-node/NodeHttpClient';
import { layerHttpServices, layerServer } from '@effect/platform-node/NodeHttpServer';
import { Unmatched } from '@rasm/search/unmatched';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Array, Cause, Config, ConfigProvider, Console, Context, DateTime, Deferred, Duration, Effect, FileSystem, Function, flow, Layer, Match, Option, Predicate, Record, Schema, SchemaGetter, Stream, String, Struct } from 'effect';
import { McpClient, type McpProtocol, McpSchema, type McpServer } from 'effect/ai';
import { Argument, CliError, CliOutput, Command, Flag, GlobalFlag, type HelpDoc } from 'effect/cli';
import { HttpBody, HttpRouter, HttpServer, HttpServerRequest, HttpServerResponse, Mime } from 'effect/http';
import { AddressResolver, Host, NetAddress } from 'effect/net';
import envPaths, { type Paths } from 'env-paths';
import { Server } from '#/server';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FOLDERS = { logs: ['log'], declarations: ['cache', 'declarations'], results: ['cache', 'results'] } as const satisfies Readonly<Record<string, readonly [keyof Paths, ...string[]]>>;
const HANDOFF = 'LISTEN_DESCRIPTORS';
const CF = { kCFStringEncodingUTF8: 0x08_00_01_00, kCFNumberDoubleType: 13 } as const;
const IMAGE = {
    launch_activate_socket: { arguments: ['string', 'pointer', 'pointer'], return: 'i32' },
    free: { arguments: ['pointer'] },
    dup: { arguments: ['i32'], return: 'i32' },
    __error: { return: 'pointer' },
    CFRelease: { arguments: ['pointer'] },
    CFGetTypeID: { arguments: ['pointer'], return: 'u64' },
    CFStringGetTypeID: { return: 'u64' },
    CFNumberGetTypeID: { return: 'u64' },
    CFBooleanGetTypeID: { return: 'u64' },
    CFDateGetTypeID: { return: 'u64' },
    CFDataGetTypeID: { return: 'u64' },
    CFArrayGetTypeID: { return: 'u64' },
    CFDictionaryGetTypeID: { return: 'u64' },
    CFStringCreateWithCString: { arguments: ['pointer', 'string', 'u32'], return: 'pointer' },
    CFStringCreateExternalRepresentation: { arguments: ['pointer', 'pointer', 'u32', 'u8'], return: 'pointer' },
    CFNumberGetValue: { arguments: ['pointer', 'i64', 'pointer'], return: 'bool' },
    CFBooleanGetValue: { arguments: ['pointer'], return: 'bool' },
    CFDateGetAbsoluteTime: { arguments: ['pointer'], return: 'f64' },
    CFDataCreate: { arguments: ['pointer', 'buffer', 'i64'], return: 'pointer' },
    CFDataGetBytePtr: { arguments: ['pointer'], return: 'pointer' },
    CFDataGetLength: { arguments: ['pointer'], return: 'i64' },
    CFArrayGetCount: { arguments: ['pointer'], return: 'i64' },
    CFArrayGetValueAtIndex: { arguments: ['pointer', 'i64'], return: 'pointer' },
    CFDictionaryGetCount: { arguments: ['pointer'], return: 'i64' },
    CFDictionaryGetKeysAndValues: { arguments: ['pointer', 'pointer', 'pointer'] },
    CFPropertyListCreateWithData: { arguments: ['pointer', 'pointer', 'u64', 'pointer', 'pointer'], return: 'pointer' },
    CFPreferencesCopyAppValue: { arguments: ['pointer', 'pointer'], return: 'pointer' },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Count = Schema.FiniteFromString.check(Schema.isInt(), Schema.isGreaterThan(0));
const Descriptors = Schema.String.pipe(Schema.decodeTo(Schema.Array(Count), { decode: SchemaGetter.split(), encode: SchemaGetter.transform((listed: readonly string[]) => listed.join(',')) })).annotate({
    description: 'Comma-separated listening descriptors a server takes over from the image it re-executed',
});
const Subscription = Schema.fromJsonString(Schema.Struct({ method: Schema.Literal('subscriptions/listen') })).annotate({ description: 'MCP request body opening a notification stream that stays open until the server ends it' });
const Agent = Schema.Struct({
    BundleProgram: Schema.optionalKey(Schema.String),
    ExitTimeOut: Schema.optionalKey(Schema.Int),
    Sockets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ SockNodeName: Schema.String, SockServiceName: Schema.Int }))),
}).annotate({ description: 'launchd.plist of an agent job in the bundle holding this executable' });
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

const darwin = Effect.gen(function* () {
    const [ffi, files] = yield* Effect.all([Effect.promise(() => import('node:ffi')), FileSystem.FileSystem]);
    const { functions: cf, lib } = yield* Effect.sync(() => ffi.dlopen(null, IMAGE));
    const [epoch, decoder] = [ffi.getFloat64(lib.getSymbol('kCFAbsoluteTimeIntervalSince1970')), new TextDecoder()];
    const released = <A>(owned: bigint, read: (ref: bigint) => A) => {
        const result = read(owned);
        cf.CFRelease(owned);
        return result;
    };
    const created = (text: string) => cf.CFStringCreateWithCString(null, text, CF.kCFStringEncodingUTF8);
    const bytes = (data: bigint) => {
        const length = Number(cf.CFDataGetLength(data));
        return length === 0 ? new Uint8Array() : ffi.toBuffer(cf.CFDataGetBytePtr(data), length);
    };
    const text = (string: bigint) => decoder.decode(released(cf.CFStringCreateExternalRepresentation(null, string, CF.kCFStringEncodingUTF8, 0), bytes));
    const value = (ref: bigint): unknown => readers.get(cf.CFGetTypeID(ref))?.(ref);
    const readers: ReadonlyMap<bigint, (ref: bigint) => unknown> = new Map<bigint, (ref: bigint) => unknown>([
        [cf.CFStringGetTypeID(), text],
        [
            cf.CFNumberGetTypeID(),
            (number) => {
                const held = new Float64Array(1);
                cf.CFNumberGetValue(number, CF.kCFNumberDoubleType, held);
                return ffi.getFloat64(ffi.getRawPointer(held));
            },
        ],
        [cf.CFBooleanGetTypeID(), (boolean) => cf.CFBooleanGetValue(boolean) === 1],
        [cf.CFDateGetTypeID(), (date) => Duration.seconds(cf.CFDateGetAbsoluteTime(date) + epoch).pipe(Duration.toMillis, DateTime.makeUnsafe)],
        [cf.CFDataGetTypeID(), bytes],
        [cf.CFArrayGetTypeID(), (array) => globalThis.Array.from({ length: Number(cf.CFArrayGetCount(array)) }, (_, index) => value(cf.CFArrayGetValueAtIndex(array, index)))],
        [
            cf.CFDictionaryGetTypeID(),
            (dictionary) => {
                const count = Number(cf.CFDictionaryGetCount(dictionary));
                const [keys, values] = [new BigUint64Array(count), new BigUint64Array(count)];
                cf.CFDictionaryGetKeysAndValues(dictionary, keys, values);
                return Object.fromEntries(Array.zipWith(keys, values, (key, entry) => [text(key), value(entry)] as const));
            },
        ],
    ]);
    return {
        propertyList: (path: string) =>
            files.readFile(path).pipe(
                Effect.flatMap((data) => {
                    const wrapped = cf.CFDataCreate(null, data, data.byteLength);
                    const parsed = cf.CFPropertyListCreateWithData(null, wrapped, 0, null, null);
                    cf.CFRelease(wrapped);
                    return parsed === 0n ? new Cause.UnknownError(path, `${path} holds no property list`) : Effect.succeed(released(parsed, value));
                }),
            ),
        preferences: (domain: string) =>
            ConfigProvider.make((path) =>
                Effect.suspend(() => {
                    const [keys, indices] = Array.span(path, Predicate.isString);
                    const [key, application] = [created(keys.join('.')), created(domain)];
                    const found = cf.CFPreferencesCopyAppValue(key, application);
                    cf.CFRelease(key);
                    cf.CFRelease(application);
                    return ConfigProvider.fromUnknown(found === 0n ? undefined : released(found, value)).load(indices);
                }),
            ),
        activate: (name: string) =>
            Effect.suspend(() => {
                const slots = new BigUint64Array(2);
                const answer = cf.launch_activate_socket(name, slots, slots.subarray(1));
                return Match.value(answer).pipe(
                    Match.when(0, () => {
                        const [listed, count] = [ffi.getUint64(ffi.getRawPointer(slots)), Number(ffi.getUint64(ffi.getRawPointer(slots), BigUint64Array.BYTES_PER_ELEMENT))];
                        const fds = Array.fromIterable(new Int32Array(ffi.toArrayBuffer(listed, count * Int32Array.BYTES_PER_ELEMENT)));
                        cf.free(listed);
                        return Effect.succeedSome(fds);
                    }),
                    Match.when(
                        (code) => code === constants.errno.ESRCH,
                        () => Effect.succeedNone,
                    ),
                    Match.orElse((code) => new Cause.UnknownError(code, `Check-in of launchd socket ${name} failed: ${getSystemErrorMessage(-code)}`)),
                );
            }),
        duplicate: (fd: number) =>
            Effect.suspend(() => {
                const copy = cf.dup(fd);
                return copy < 0 ? new Cause.UnknownError(fd, `Duplicating listening descriptor ${fd} failed: ${getSystemErrorMessage(-ffi.getInt32(cf.__error()))}`) : Effect.succeed(copy);
            }),
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = Effect.fnUntraced(function* <E, R, E2, R2, const Declared extends Parameters<typeof Command.withGlobalFlags>[1]>(
    { identifier, ...manifest }: Pick<Parameters<typeof McpServer.layerHttp>[0], 'name' | 'version' | 'description' | 'websiteUrl' | 'icons' | 'extensions' | 'cacheHints'> & { readonly identifier: string },
    { instructions, settings }: { readonly instructions: readonly string[]; readonly settings: Declared },
    app: (stop: Duration.Duration) => Layer.Layer<never, E, R>,
    ending: Effect.Effect<never, E2, R2>,
) {
    const [named, files, native] = yield* Effect.all([
        Effect.map(Server.scheme(manifest.name), (scheme) => ({ ...manifest, name: scheme })),
        FileSystem.FileSystem,
        Effect.when(
            darwin,
            Effect.sync(() => process.platform === 'darwin'),
        ),
    ]);
    const name = identifier.slice(identifier.lastIndexOf('.') + 1);
    const bundle = resolve(process.execPath, '..', '..', '..');
    const agents = join(bundle, 'Contents', 'Library', 'LaunchAgents');
    const agent = yield* native.pipe(
        Option.map(({ propertyList }) =>
            files.readDirectory(agents).pipe(
                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed(Array.empty<string>())),
                Effect.map(Array.filter((file) => extname(file) === '.plist')),
                Effect.flatMap(Effect.forEach((file) => Effect.flatMap(propertyList(join(agents, file)), Schema.decodeUnknownEffect(Agent)))),
                Effect.map(Array.findFirst(({ BundleProgram }) => BundleProgram !== undefined && join(bundle, BundleProgram) === process.execPath)),
            ),
        ),
        Effect.transposeOption,
        Effect.map(Option.flatten),
    );
    const sockets = Option.flatMap(agent, ({ Sockets }) => Option.fromUndefinedOr(Sockets));
    const share = agent.pipe(
        Option.flatMap(({ ExitTimeOut }) => Option.fromUndefinedOr(ExitTimeOut)),
        Option.filter((seconds) => seconds > 0),
        Option.map((seconds) => Duration.seconds(seconds / 2)),
    );
    const declared = yield* sockets.pipe(
        Option.flatMap(flow(Record.values, Array.head)),
        Option.map(({ SockNodeName, SockServiceName }) => Effect.fromResult(Host.hostPortFromInput({ host: SockNodeName, port: SockServiceName }))),
        Effect.transposeOption,
    );
    const Listen = setting('listen', Schema.HostPortFromString, (flag) => Flag.optional(flag).pipe(Flag.map(Option.orElse(() => declared))), 'Endpoint as host:port or [IPv6]:port, a host name binding every address it resolves to, the Sockets address of the agent plist when absent');
    const scoped = ConfigProvider.fromEnv().pipe(ConfigProvider.nested(name), ConfigProvider.constantCase);
    const provider = Option.match(native, { onNone: () => scoped, onSome: ({ preferences }) => ConfigProvider.orElse(scoped, preferences(identifier)) }).pipe(ConfigProvider.orElse(ConfigProvider.fromEnv()));
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
            Effect.provide(layerUndici),
        ),
    ).pipe(Command.withDescription('Calls one tool of the running server and prints its result blocks in order, an image or other binary block as the path of a file holding it'));
    const described = Effect.fnUntraced(function* (doc: HelpDoc.HelpDoc) {
        const [, address] = yield* Listen.flag.parse({ flags: {}, arguments: [] });
        const offered = yield* address.pipe(
            Effect.fromOption,
            Effect.flatMap(connect),
            Effect.flatMap((client) => client.listTools()),
            Effect.scoped,
            Effect.provide(layerUndici),
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
                const [{ output, state, hosts, ...transport }, located, resolver, inherited, draining] = yield* Effect.all([Effect.all(Settings), folders, AddressResolver.AddressResolver, Config.option(Config.schema(Descriptors, HANDOFF)).parse(ConfigProvider.fromEnv()), Deferred.make<void>()]);
                const exported = Record.getSomes(Record.mapKeys({ [Settings.output.id]: output, [Settings.state.id]: state }, (id) => String.constantCase(`${name}-${id}`)));
                yield* Effect.sync(() => {
                    Object.assign(process.env, exported);
                    Reflect.deleteProperty(process.env, HANDOFF);
                });
                const appLayer = Layer.provide(app(Option.getOrElse(share, () => Duration.infinity)), Layer.succeed(Output)(located));
                const handler = yield* Option.match(address, {
                    onNone: () => HttpRouter.toHttpEffect(Server.layerStdio(named, { ...transport, instructions, state: located.state }, appLayer)),
                    onSome: (bound) => HttpRouter.toHttpEffect(Server.layerHttp(named, { ...transport, instructions, state: located.state, draining, hosts, address: bound }, appLayer)),
                });
                const descriptors = yield* Option.match(inherited, {
                    onSome: Effect.succeedSome,
                    onNone: () =>
                        Option.match(Option.all([native, sockets]), {
                            onNone: () => Effect.succeedNone,
                            onSome: ([{ activate }, declaring]) => Effect.map(Effect.forEach(Record.keys(declaring), activate), (answers) => Option.map(Option.all(answers), Array.flatten)),
                        }),
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
                subscribe('http.server.response.finish', () => {
                    for (const { server } of servers) {
                        if (!server.listening) {
                            server.closeIdleConnections();
                        }
                    }
                });
                const served = Effect.zipWith(handler, Effect.isSuccess(HttpServerRequest.HttpServerRequest.pipe(Effect.flatMap(Struct.get('text')), Effect.flatMap(Schema.decodeEffect(Subscription)))), (response, subscribed) =>
                    subscribed && response.body._tag === 'Stream' ? HttpServerResponse.setBody(response, HttpBody.stream(Stream.interruptWhen(response.body.stream, Deferred.await(draining)), response.body.contentType, response.body.contentLength)) : response,
                );
                yield* Effect.forEach(servers, ({ listen, server }) => HttpServer.serve(served).pipe(HttpServer.withLogAddress, Layer.provide(layerServer(Function.constant(server), { ...listen, ...Record.getSomes({ gracefulShutdownTimeout: share }) })), Layer.build), {
                    discard: true,
                });
                yield* Effect.addFinalizer(() => Effect.asVoid(Deferred.succeed(draining, undefined)));
                yield* Effect.sync(flushCompileCache);
                const hangup = Option.match(Option.all([native, descriptors]), {
                    onNone: () => Effect.never,
                    onSome: ([{ duplicate }, fds]) => Effect.promise((signal) => once(process, 'SIGHUP', { signal })).pipe(Effect.andThen(Effect.forEach(fds, duplicate)), Effect.flatMap(Schema.encodeEffect(Descriptors)), Effect.orDie),
                });
                return yield* Effect.raceFirst(
                    Effect.map(hangup, (listed) => ({ ...process.env, [HANDOFF]: listed })),
                    ending,
                );
            },
            Effect.scoped,
            Effect.provide([Layer.unwrap(telemetry), layerAead, layerSecureRandom, layerHttpServices, Layer.provide(addressResolver, dns)]),
            Effect.flatMap((replaced) =>
                Effect.try({
                    try: () => {
                        flushCompileCache();
                        process.execve?.(process.execPath, [process.execPath, ...(isSea() ? process.argv.slice(2) : [...process.execArgv, ...process.argv.slice(1)])], replaced);
                    },
                    catch: (cause) => new Cause.UnknownError(cause, `Re-executing ${process.execPath} failed`),
                }),
            ),
        ),
    ).pipe(
        Command.withSubcommands([call]),
        Command.withGlobalFlags([...Object.values(Settings), ...settings]),
        Command.run({ version: manifest.version }),
        Effect.provideService(CliOutput.Formatter, CliOutput.defaultFormatter({ globalFlags: 'summary' })),
        Effect.provideService(CliOutput.Presenter, presenter),
        Effect.provideService(ConfigProvider.ConfigProvider, provider),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Output, serve, setting };
