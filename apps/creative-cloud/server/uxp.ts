// --- [IMPORTS] -------------------------------------------------------------------------

import { NodeRuntime, NodeServices } from '@effect/platform-node';
import {
    Array,
    Cause,
    Clock,
    Config,
    Console,
    type Crypto,
    Duration,
    Effect,
    FileSystem,
    flow,
    Layer,
    Option,
    Path,
    type PlatformError,
    Predicate,
    Queue,
    Result,
    Schedule,
    Schema,
    type Scope,
    Stream,
    String,
    Struct,
} from 'effect';
import { McpProtocol, McpSchema } from 'effect/unstable/ai';
import { Command } from 'effect/unstable/cli';
import { ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';
import { RpcClient, type RpcClientError, RpcSerialization } from 'effect/unstable/rpc';
import { Socket } from 'effect/unstable/socket';
import { Failure } from './contract.ts';
import { type BridgeError, exited, faulted } from './errors.ts';
import { Link } from './frames.ts';
import { Health } from './health.ts';
import { discover } from './hosts.ts';
import { Jobs, root } from './jobs.ts';
import { type Bridge, type Manifest, Placed } from './manifest.ts';
import { read, reply } from './osascript.ts';
import { answered, layer as bridged, execution, attached as joined, Links, linkState, probing, type Session } from './socket.ts';
import { ARTIFACTS, HostId, PROBE_MS, type Row, type SOCKETS, type SocketHost, TIMEOUT_CEILING_MS, TIMEOUT_MS } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Server {
    readonly health: Effect.Effect<(typeof Health)['Type'], ServerError>;
    readonly call: <S extends Schema.Top>(tool: string, args: Readonly<Record<string, Schema.Json>>, schema: S) => Effect.Effect<S['Type'], ServerError | BridgeError, S['DecodingServices']>;
}

type Executor = (code: string) => Effect.Effect<Schema.Json, BridgeError, Crypto.Crypto>;

type ServerError = InstallError | RpcClientError.RpcClientError | (typeof McpSchema.McpError)['Type'];

type HealthRow = (typeof Health)['Type']['hosts'][number];

type InstallError = (typeof InstallError)['Type'];

type Placement = InstallError | BridgeError | PlatformError.PlatformError | Schema.SchemaError | Config.ConfigError;

// --- [CONSTANTS] -----------------------------------------------------------------------

const POLL: Schedule.Schedule<number> = Schedule.spaced('250 millis').pipe(Schedule.upTo({ duration: '120 seconds' }));

// --- [MODELS] --------------------------------------------------------------------------

const Registration: Schema.Struct<{
    readonly hostMinVersion: Schema.String;
    readonly name: Schema.String;
    readonly path: Schema.String;
    readonly pluginId: Schema.String;
    readonly status: Schema.Literal<'enabled'>;
    readonly type: Schema.Literal<'uxp'>;
    readonly versionString: Schema.String;
}> = Schema.Struct({
    hostMinVersion: Schema.String,
    name: Schema.String,
    path: Schema.String,
    pluginId: Schema.String,
    status: Schema.Literal('enabled'),
    type: Schema.Literal('uxp'),
    versionString: Schema.String,
});

const Registry = Schema.fromJsonString(Schema.Struct({ plugins: Schema.Array(Registration) }));

const _Reply = Schema.Struct({ result: Schema.Union([Failure, Schema.Json]) });

const InstallError: Schema.TaggedUnion<{
    readonly notReady: Schema.TaggedStruct<'notReady', { readonly hosts: (typeof Health)['fields']['hosts'] }>;
    readonly hostRunning: Schema.TaggedStruct<'hostRunning', { readonly host: typeof HostId; readonly pid: Schema.Int }>;
    readonly hostStillRunning: Schema.TaggedStruct<'hostStillRunning', { readonly host: typeof HostId }>;
    readonly toolFailed: Schema.TaggedStruct<'toolFailed', { readonly tool: Schema.String; readonly content: Schema.$Array<typeof McpSchema.ContentBlock>; readonly cause: Schema.Defect }>;
}> = Schema.TaggedUnion({
    notReady: { hosts: Health.fields.hosts },
    hostRunning: { host: HostId, pid: Schema.Int },
    hostStillRunning: { host: HostId },
    toolFailed: { tool: Schema.String, content: Schema.Array(McpSchema.ContentBlock), cause: Schema.Defect() },
});

// --- [SERVER] --------------------------------------------------------------------------

const _bytes = (chunk: Uint8Array | string): Uint8Array => (Predicate.isString(chunk) ? new TextEncoder().encode(chunk) : chunk);

const protocol: Layer.Layer<RpcClient.Protocol, PlatformError.PlatformError, ChildProcessSpawner.ChildProcessSpawner> = Layer.unwrap(
    Effect.gen(function* () {
        const spawner = yield* ChildProcessSpawner.ChildProcessSpawner;
        const stdin = yield* Queue.make<Uint8Array, Cause.Done>();
        const handle = yield* spawner.spawn(ChildProcess.make('node', ['main.ts'], { cwd: import.meta.dirname, stdin: Stream.fromQueue(stdin), stderr: 'inherit' }));
        const socket = Socket.make({
            reader: Effect.map(Stream.toPull(handle.stdout), (pull) => ({
                pull: Effect.mapError(
                    pull,
                    (error) => new Socket.SocketError({ reason: Cause.isDone(error) ? new Socket.SocketCloseError({ code: 1000 }) : new Socket.SocketReadError({ cause: error }) }),
                ),
                upgrade: Socket.SocketUpgradeError.unsupported,
            })),
            writer: Effect.succeed({
                write: (chunk: Uint8Array | string | Socket.CloseEvent) => Effect.asVoid(Socket.isCloseEvent(chunk) ? Queue.end(stdin) : Queue.offer(stdin, _bytes(chunk))),
                writeAll: (chunks: Array.NonEmptyReadonlyArray<Uint8Array | string>) => Effect.asVoid(Queue.offerAll(stdin, Array.map(chunks, _bytes))),
            }),
        });
        return RpcClient.layerProtocolSocket().pipe(Layer.provide(Layer.succeed(Socket.Socket, socket)), Layer.provide(RpcSerialization.layerNdJsonRpc()));
    }),
);

const server: (host: Pick<Row, 'id'>, manifest: Pick<Manifest, 'id' | 'version'>) => Effect.Effect<Server, ServerError, RpcClient.Protocol | Scope.Scope> = Effect.fnUntraced(function* (
    host: Pick<Row, 'id'>,
    manifest: Pick<Manifest, 'id' | 'version'>,
) {
    const client = yield* RpcClient.make(McpSchema.ClientRpcs);
    yield* client.initialize({ protocolVersion: McpProtocol.v2025_11_25.protocolVersion, capabilities: {}, clientInfo: { name: manifest.id, version: manifest.version } });
    yield* client['notifications/initialized'](undefined);
    const called = (tool: string, args: Readonly<Record<string, Schema.Json>>): Effect.Effect<McpSchema.CallToolResult, ServerError> =>
        Effect.filterOrFail(
            client['tools/call']({ name: tool, arguments: args }),
            (answer) => answer.isError !== true,
            (answer) => InstallError.cases.toolFailed.make({ tool, content: answer.content, cause: answer }),
        );
    const decoded = <S extends Schema.Top>(tool: string, answer: McpSchema.CallToolResult, schema: S, value: unknown): Effect.Effect<S['Type'], InstallError, S['DecodingServices']> =>
        Effect.mapError(Schema.decodeUnknownEffect(schema)(value), (cause) => InstallError.cases.toolFailed.make({ tool, content: answer.content, cause }));
    const call = Effect.fnUntraced(function* <S extends Schema.Top>(tool: string, args: Readonly<Record<string, Schema.Json>>, schema: S) {
        const answer = yield* called(tool, args);
        const { result } = yield* decoded(tool, answer, _Reply, answer.structuredContent);
        return yield* Option.match(Option.liftPredicate(result, Schema.is(Failure)), { onSome: ({ error }) => Effect.fail(error), onNone: () => decoded(tool, answer, schema, result) });
    });
    return { health: Effect.flatMap(called('health', { host: host.id }), (answer) => decoded('health', answer, Health, answer.structuredContent)), call };
});

const until = <E, R>(health: Effect.Effect<(typeof Health)['Type'], E, R>, ready: Predicate.Predicate<HealthRow>): Effect.Effect<HealthRow, E | InstallError, R> =>
    Effect.flatMap(health, (answer) => Effect.fromOption(Array.findFirst(answer.hosts, ready), () => InstallError.cases.notReady.make({ hosts: answer.hosts }))).pipe(
        Effect.retry({ while: Predicate.isTagged('notReady'), schedule: POLL }),
    );

const probed = (row: HealthRow): boolean => Result.isSuccess(row.probe);

// --- [DEPLOYMENT] ----------------------------------------------------------------------

const deploy = <Launched, Ready, LaunchError, ReadyError, LaunchServices, ReadyServices>(
    host: (typeof SOCKETS)[SocketHost],
    bridge: Bridge,
    directory: string,
    launched: Effect.Effect<Launched, LaunchError, LaunchServices>,
    ready: (execute: Executor) => Effect.Effect<Ready, ReadyError, ReadyServices>,
): Effect.Effect<
    void,
    Placement | BridgeError | LaunchError | ReadyError,
    LaunchServices | ReadyServices | ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path | Crypto.Crypto
> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const base = yield* root;
        const found = yield* discover(host);
        const uxp = path.join(yield* Config.String('HOME'), 'Library', 'Application Support', 'Adobe', 'UXP');
        const external = path.join(uxp, 'Plugins', 'External');
        const folder = `${bridge.manifest.id}_${bridge.manifest.version}`;
        yield* Effect.forEach(Array.filter(yield* fs.readDirectory(external), String.startsWith(`${bridge.manifest.id}_`)), (entry) => fs.remove(path.join(external, entry), { recursive: true }));
        yield* fs.copy(path.join(base, ARTIFACTS, path.relative(base, directory)), path.join(external, folder));
        yield* fs.writeFileString(
            path.join(external, folder, 'manifest.json'),
            yield* Schema.encodeEffect(Schema.fromJsonString(Placed))({ ...bridge.manifest, host: { ...bridge.manifest.host, minVersion: found.version } }),
        );
        const registry = path.join(uxp, 'PluginsInfo', 'v1', `${host.uxp.app}.json`);
        const rows = yield* Schema.decodeEffect(Registry)(yield* fs.readFileString(registry));
        const registration = Registration.make({
            hostMinVersion: found.version,
            name: bridge.manifest.name,
            path: `$localPlugins/External/${folder}`,
            pluginId: bridge.manifest.id,
            status: 'enabled',
            type: 'uxp',
            versionString: bridge.manifest.version,
        });
        yield* fs.writeFileString(registry, yield* Schema.encodeEffect(Registry)({ plugins: [...Array.filter(rows.plugins, (kept) => kept.pluginId !== bridge.manifest.id), registration] }));
        const [endpoints, queues] = yield* Effect.all([Links, Jobs]);
        const session: Session = { link: endpoints[host.id], host: queues[host.id] };
        yield* Effect.asVoid(read(host.id, host.bundleId, TIMEOUT_CEILING_MS, host.quit, Option.none())).pipe(Effect.catchTag('hostNotRunning', () => Effect.void));
        yield* Effect.repeat(read(host.id, host.bundleId, PROBE_MS, '', Option.none()), POLL).pipe(
            Effect.andThen(Effect.fail(InstallError.cases.hostStillRunning.make({ host: host.id }))),
            Effect.catchTag('hostNotRunning', () => Effect.void),
        );
        yield* Effect.mapError(reply(ChildProcess.make('open', ['-b', host.bundleId])), exited(host.id));
        const launchedAt = yield* Clock.currentTimeMillis;
        const [launching, shown] = yield* Effect.timed(launched);
        yield* joined(session.link);
        const attachedAt = yield* Clock.currentTimeMillis;
        const probe = yield* Effect.flatMap(probing(session), Effect.fromResult).pipe(
            Effect.retry({ while: Predicate.some([Predicate.isTagged('hostBusy'), Predicate.isTagged('hostNotAttached'), Predicate.isTagged('deadlineExceeded')]), schedule: POLL }),
        );
        const probedAt = yield* Clock.currentTimeMillis;
        const facts = yield* ready((code) => Effect.map(answered(session, TIMEOUT_MS, Schema.Json, flow(execution(code), Effect.succeed)), Struct.get('value')));
        const link = yield* Effect.map(linkState(session.link), Schema.encodeSync(Schema.toCodecJson(Link)));
        yield* Console.log(
            JSON.stringify(
                {
                    folder,
                    registration,
                    launched: shown,
                    launchedMs: Duration.toMillis(launching),
                    attachedAfterLaunchMs: attachedAt - launchedAt,
                    probedAfterAttachMs: probedAt - attachedAt,
                    link,
                    probe,
                    ready: facts,
                },
                null,
                4,
            ),
        );
    }).pipe(Effect.scoped, Effect.provide(bridged([host.id])));

const main = <Name extends string>(command: Command.Command<Name, never, unknown, unknown, NodeServices.NodeServices>, version: string): void =>
    Command.run(command, { version }).pipe(Effect.tapCause(faulted), Effect.provide(NodeServices.layer), NodeRuntime.runMain({ disableErrorReporting: true }));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Executor, HealthRow, Server };
export { deploy, InstallError, main, POLL, probed, protocol, server, until };
