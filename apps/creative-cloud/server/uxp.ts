// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Cause, Clock, Config, Effect, FileSystem, Layer, Option, Path, type PlatformError, Predicate, Queue, Result, Schedule, Schema, type Scope, Stream, String } from 'effect';
import { McpProtocol, McpSchema } from 'effect/unstable/ai';
import { ChildProcess, ChildProcessSpawner } from 'effect/unstable/process';
import { RpcClient, type RpcClientError, RpcSerialization } from 'effect/unstable/rpc';
import { Socket } from 'effect/unstable/socket';
import { Node, Project } from 'ts-morph';
import { BridgeError } from './errors.ts';
import { Link } from './frames.ts';
import { read, reply } from './osascript.ts';
import { MANIPULATION } from './sdef.ts';
import type { HostId } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Manifest {
    readonly id: string;
    readonly name: string;
    readonly version: string;
    readonly host: { readonly app: string; readonly minVersion: string };
}

interface Host {
    readonly id: HostId;
    readonly bundleId: string;
}

interface Server {
    readonly health: Effect.Effect<Health, Failure>;
    readonly execute: (code: string) => Effect.Effect<Schema.Json, Failure | BridgeError>;
    readonly call: <S extends Schema.Top>(tool: string, args: Readonly<Record<string, Schema.Json>>, schema: S) => Effect.Effect<S['Type'], Failure, S['DecodingServices']>;
}

type McpError = (typeof McpSchema.McpError)['Type'];

type Failure = InstallError | RpcClientError.RpcClientError | McpError;

type Registration = (typeof Registration)['Type'];

type Health = (typeof Health)['Type'];

type HealthRow = Health['hosts'][number];

type InstallError = (typeof InstallError)['Type'];

// --- [CONSTANTS] -----------------------------------------------------------------------

const _QUIT_MS = 300_000;
const _RUNNING_MS = 5000;
const _PROTOCOL = McpProtocol.v2025_11_25.protocolVersion;
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

const Health: Schema.Struct<{
    readonly hosts: Schema.$Array<
        Schema.Struct<{
            readonly host: Schema.Struct<{ readonly id: Schema.String }>;
            readonly link: Schema.OptionFromNullOr<typeof Link>;
            readonly probe: Schema.toCodecJson<Schema.Result<Schema.Codec<Schema.Json>, typeof BridgeError>>;
        }>
    >;
}> = Schema.Struct({
    hosts: Schema.Array(Schema.Struct({ host: Schema.Struct({ id: Schema.String }), link: Schema.OptionFromNullOr(Link), probe: Schema.toCodecJson(Schema.Result(Schema.Json, BridgeError)) })),
});

const _Answer = Schema.Struct({
    result: Schema.Union([Schema.Struct({ kind: Schema.Literal('value'), value: Schema.Json }), Schema.Struct({ kind: Schema.Literal('error'), error: BridgeError })]).pipe(
        Schema.toTaggedUnion('kind'),
    ),
});

const InstallError: Schema.TaggedUnion<{
    readonly notReady: Schema.TaggedStruct<'notReady', { readonly hosts: Schema.String }>;
    readonly toolFailed: Schema.TaggedStruct<'toolFailed', { readonly tool: Schema.String; readonly text: Schema.String }>;
    readonly moduleNotDeclared: Schema.TaggedStruct<'moduleNotDeclared', { readonly typings: Schema.String; readonly module: Schema.String }>;
}> = Schema.TaggedUnion({
    notReady: { hosts: Schema.String },
    toolFailed: { tool: Schema.String, text: Schema.String },
    moduleNotDeclared: { typings: Schema.String, module: Schema.String },
});

const _Typings = Schema.fromJsonString(Schema.Struct({ types: Schema.String }));

// --- [TYPINGS] -------------------------------------------------------------------------

const declared: (
    manifest: URL,
    module: string,
    directory: string,
) => Effect.Effect<number, InstallError | PlatformError.BadArgument | PlatformError.PlatformError | Schema.SchemaError, FileSystem.FileSystem | Path.Path> = Effect.fnUntraced(function* (
    manifest: URL,
    module: string,
    directory: string,
) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const packageDir = path.dirname(yield* path.fromFileUrl(manifest));
    const { types } = yield* Schema.decodeEffect(_Typings)(yield* fs.readFileString(path.join(packageDir, 'package.json')));
    const typings = path.join(packageDir, types);
    const source = new Project({ useInMemoryFileSystem: true, manipulationSettings: MANIPULATION }).createSourceFile(path.basename(typings), yield* fs.readFileString(typings));
    const block = yield* Effect.fromOption(
        Array.findFirst(source.getModules(), (candidate) => {
            const names = candidate.getNameNodes();
            return !Array.isArray(names) && names.getLiteralValue() === module;
        }),
        () => InstallError.cases.moduleNotDeclared.make({ typings, module }),
    );
    const packaged = (specifier: string): string => path.join(path.relative(directory, path.dirname(typings)), specifier);
    const printed = Array.map(block.getStatements(), (statement) => {
        if (Node.isImportDeclaration(statement) || (Node.isExportDeclaration(statement) && statement.hasModuleSpecifier())) {
            statement.setModuleSpecifier(packaged(Option.getOrElse(Option.fromNullishOr(statement.getModuleSpecifierValue()), () => '')));
        }
        if (Node.isVariableStatement(statement)) {
            statement.setHasDeclareKeyword(true);
        }
        return statement.getText();
    });
    yield* fs.writeFileString(path.join(directory, `${module}.ts`), `${Array.join(printed, '\n')}\n`);
    return printed.length;
});

// --- [PLACEMENT] -----------------------------------------------------------------------

const install: (
    manifest: Manifest,
    project: string,
) => Effect.Effect<{ readonly folder: string; readonly registration: Registration }, PlatformError.PlatformError | Schema.SchemaError | Config.ConfigError, FileSystem.FileSystem | Path.Path> =
    Effect.fnUntraced(function* (manifest: Manifest, project: string) {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const uxp = path.join(yield* Config.String('HOME'), 'Library', 'Application Support', 'Adobe', 'UXP');
        const external = path.join(uxp, 'Plugins', 'External');
        const folder = `${manifest.id}_${manifest.version}`;
        const root = path.resolve(import.meta.dirname, '..', '..', '..');
        yield* Effect.forEach(Array.filter(yield* fs.readDirectory(external), String.startsWith(`${manifest.id}_`)), (entry) => fs.remove(path.join(external, entry), { recursive: true }));
        yield* fs.copy(path.join(root, '.artifacts', path.relative(root, project)), path.join(external, folder));
        const registry = path.join(uxp, 'PluginsInfo', 'v1', `${manifest.host.app}.json`);
        const rows = yield* Schema.decodeEffect(Registry)(yield* fs.readFileString(registry));
        const registration: Registration = {
            hostMinVersion: manifest.host.minVersion,
            name: manifest.name,
            path: `$localPlugins/External/${folder}`,
            pluginId: manifest.id,
            status: 'enabled',
            type: 'uxp',
            versionString: manifest.version,
        };
        yield* fs.writeFileString(registry, yield* Schema.encodeEffect(Registry)({ plugins: [...Array.filter(rows.plugins, (kept) => kept.pluginId !== manifest.id), registration] }));
        return { folder, registration };
    });

const relaunch: (host: Host, quit: string) => Effect.Effect<number, BridgeError, ChildProcessSpawner.ChildProcessSpawner> = Effect.fnUntraced(function* (host: Host, quit: string) {
    yield* Effect.asVoid(read(host.id, host.bundleId, _QUIT_MS, quit, Option.none())).pipe(Effect.catchTag('hostNotRunning', () => Effect.void));
    yield* Effect.asVoid(Effect.repeat(read(host.id, host.bundleId, _RUNNING_MS, '', Option.none()), POLL)).pipe(Effect.catchTag('hostNotRunning', () => Effect.void));
    yield* Effect.orDie(reply(ChildProcess.make('open', ['-b', host.bundleId])));
    return yield* Clock.currentTimeMillis;
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

const server: (host: Pick<Host, 'id'>, manifest: Pick<Manifest, 'id' | 'version'>) => Effect.Effect<Server, Failure, RpcClient.Protocol | Scope.Scope> = Effect.fnUntraced(function* (
    host: Pick<Host, 'id'>,
    manifest: Pick<Manifest, 'id' | 'version'>,
) {
    const transport = yield* RpcClient.Protocol;
    const client = yield* RpcClient.make(McpSchema.ClientRpcs);
    yield* client.initialize({ protocolVersion: _PROTOCOL, capabilities: {}, clientInfo: { name: manifest.id, version: manifest.version } });
    yield* transport.send(0, { _tag: 'Request', id: '', tag: McpSchema.InitializedNotification._tag, payload: null, headers: [], isNotification: true });
    const call = Effect.fnUntraced(function* <S extends Schema.Top>(tool: string, args: Readonly<Record<string, Schema.Json>>, schema: S) {
        const answer = yield* client['tools/call']({ name: tool, arguments: args });
        return yield* Effect.mapError(Schema.decodeUnknownEffect(schema)(answer.structuredContent), () => InstallError.cases.toolFailed.make({ tool, text: JSON.stringify(answer.content) }));
    });
    return {
        health: call('health', { host: host.id }, Health),
        execute: (code: string) =>
            Effect.retry(
                Effect.flatMap(call(`${host.id}_execute`, { code }, _Answer), ({ result }) => (result.kind === 'value' ? Effect.succeed(result.value) : Effect.fail(result.error))),
                { while: Predicate.isTagged('hostSaturated'), schedule: POLL },
            ),
        call,
    };
});

const until = <E, R>(health: Effect.Effect<Health, E, R>, ready: Predicate.Predicate<HealthRow>): Effect.Effect<HealthRow, E | InstallError, R> =>
    Effect.flatMap(health, (answer) => Effect.fromOption(Array.findFirst(answer.hosts, ready), () => InstallError.cases.notReady.make({ hosts: JSON.stringify(answer.hosts) }))).pipe(
        Effect.retry({ while: Predicate.isTagged('notReady'), schedule: POLL }),
    );

const attached = (row: HealthRow): boolean => Option.exists(row.link, Predicate.isTagged('attached'));

const probed = (row: HealthRow): boolean => Result.isSuccess(row.probe);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { HealthRow, Manifest, Server };
export { attached, declared, InstallError, install, POLL, probed, protocol, relaunch, server, until };
