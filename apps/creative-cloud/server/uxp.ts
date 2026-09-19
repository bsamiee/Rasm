// --- [IMPORTS] -------------------------------------------------------------------------

import { env, execPath } from 'node:process';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Client } from '@modelcontextprotocol/client';
import { StdioClientTransport } from '@modelcontextprotocol/client/stdio';
import {
    Array,
    Clock,
    Config,
    Console,
    type Crypto,
    Duration,
    Effect,
    FileSystem,
    Option,
    Path,
    type PlatformError,
    Predicate,
    Record,
    Result,
    Schedule,
    Schema,
    type Scope,
    Stream,
    String,
} from 'effect';
import { Command } from 'effect/unstable/cli';
import { ChildProcess, type ChildProcessSpawner } from 'effect/unstable/process';
import { Failure } from './contract.ts';
import { BridgeError, classify, exited } from './errors.ts';
import { Link } from './frames.ts';
import { Health } from './health.ts';
import { type Bundle, discover, type HostKeyError } from './hosts.ts';
import { Jobs, liveness, request, root } from './jobs.ts';
import { type Bridge, type Manifest, Placed } from './manifest.ts';
import { reply } from './osascript.ts';
import { attached, Links, layer, linkState, probing, type Session } from './socket.ts';
import { ARTIFACTS, HostId, PROBE_MS, type Row, type SOCKETS, type SocketHost, TIMEOUT_CEILING_MS } from './values.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Server {
    readonly health: Effect.Effect<(typeof Health)['Type'], InstallError | BridgeError>;
    readonly call: <S extends Schema.Top>(tool: string, args: Readonly<Record<string, Schema.Json>>, schema: S) => Effect.Effect<S['Type'], InstallError | BridgeError, S['DecodingServices']>;
}

type HealthRow = (typeof Health)['Type']['hosts'][number];

type InstallError = (typeof InstallError)['Type'];

type Placement = InstallError | BridgeError | PlatformError.PlatformError | Schema.SchemaError | Config.ConfigError | Array.NonEmptyArray<HostKeyError>;

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

const InstallError: Schema.TaggedUnion<{
    readonly notReady: Schema.TaggedStruct<'notReady', { readonly hosts: (typeof Health)['fields']['hosts'] }>;
    readonly hostRunning: Schema.TaggedStruct<'hostRunning', { readonly host: typeof HostId; readonly pid: Schema.Int }>;
    readonly hostStillRunning: Schema.TaggedStruct<'hostStillRunning', { readonly host: typeof HostId }>;
    readonly serverFailed: Schema.TaggedStruct<'serverFailed', { readonly operation: Schema.Literals<readonly ['connect', 'close']>; readonly cause: Schema.Defect }>;
    readonly toolFailed: Schema.TaggedStruct<'toolFailed', { readonly tool: Schema.String; readonly cause: Schema.Defect }>;
}> = Schema.TaggedUnion({
    notReady: { hosts: Health.fields.hosts },
    hostRunning: { host: HostId, pid: Schema.Int },
    hostStillRunning: { host: HostId },
    serverFailed: { operation: Schema.Literals(['connect', 'close']), cause: Schema.Defect() },
    toolFailed: { tool: Schema.String, cause: Schema.Defect() },
});

// --- [SERVER] --------------------------------------------------------------------------

const server: (host: Pick<Row, 'id'>, manifest: Pick<Manifest, 'id' | 'version'>) => Effect.Effect<Server, InstallError, Scope.Scope> = Effect.fnUntraced(function* (
    host: Pick<Row, 'id'>,
    manifest: Pick<Manifest, 'id' | 'version'>,
) {
    const client = yield* Effect.acquireRelease(
        Effect.sync(() => new Client({ name: manifest.id, version: manifest.version })),
        (connection) => Effect.tryPromise({ try: () => connection.close(), catch: (cause) => InstallError.cases.serverFailed.make({ operation: 'close', cause }) }).pipe(Effect.orDie),
    );
    yield* Effect.tryPromise({
        try: (signal) => client.connect(new StdioClientTransport({ command: execPath, args: ['main.ts'], cwd: import.meta.dirname, env: Record.filter(env, Predicate.isString) }), { signal }),
        catch: (cause) => InstallError.cases.serverFailed.make({ operation: 'connect', cause }),
    });
    const call = Effect.fnUntraced(function* <S extends Schema.Top>(tool: string, args: Readonly<Record<string, Schema.Json>>, schema: S) {
        const answer = yield* Effect.tryPromise({
            try: (signal) => client.callTool({ name: tool, arguments: args }, { signal, timeout: TIMEOUT_CEILING_MS }),
            catch: (cause) => InstallError.cases.toolFailed.make({ tool, cause }),
        });
        if (answer.isError === true) {
            const { result } = yield* Schema.decodeUnknownEffect(Schema.toCodecJson(Schema.Struct({ result: Failure })))(answer.structuredContent).pipe(
                Effect.mapError((cause) => InstallError.cases.toolFailed.make({ tool, cause: { answer, cause } })),
            );
            return yield* Effect.fail(result.error);
        }
        return yield* Schema.decodeUnknownEffect(Schema.toCodecJson(schema))(answer.structuredContent).pipe(
            Effect.mapError((cause) => InstallError.cases.toolFailed.make({ tool, cause: { answer, cause } })),
        );
    });
    return {
        health: call('health', { host: host.id }, Health),
        call,
    };
});

const until = <E, R>(health: Effect.Effect<(typeof Health)['Type'], E, R>, ready: Predicate.Predicate<HealthRow>): Effect.Effect<HealthRow, E | InstallError, R> =>
    Effect.flatMap(health, (answer) => Effect.fromOption(Array.findFirst(answer.hosts, ready), () => InstallError.cases.notReady.make({ hosts: answer.hosts }))).pipe(
        Effect.retry({ while: Predicate.isTagged('notReady'), schedule: POLL }),
    );

const probed = (row: HealthRow): boolean => Result.isSuccess(row.probe);

// --- [DEPLOYMENT] ----------------------------------------------------------------------

const deploy = <E = never, R = never>(
    host: (typeof SOCKETS)[SocketHost],
    bridge: Bridge,
    directory: string,
    activate: Option.Option<(bundle: (typeof Bundle)['Type']) => Effect.Effect<Schema.Json, E, R>>,
): Effect.Effect<void, Placement | BridgeError | E, R | ChildProcessSpawner.ChildProcessSpawner | FileSystem.FileSystem | Path.Path | Crypto.Crypto> =>
    Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem;
        const path = yield* Path.Path;
        const base = yield* root;
        const found = yield* discover(host);
        const minVersion = yield* Schema.decodeEffect(Placed.fields.host.fields.minVersion)(found.version);
        const uxp = path.join(yield* Config.String('HOME'), 'Library', 'Application Support', 'Adobe', 'UXP');
        const external = path.join(uxp, 'Plugins', 'External');
        const folder = `${bridge.manifest.id}_${bridge.manifest.version}`;
        const [endpoints, queues] = yield* Effect.all([Links, Jobs]);
        const session: Session = { link: endpoints[host.id], host: queues[host.id] };
        const shutdown = [
            'on run argv',
            `with timeout of ${Math.ceil(Duration.toSeconds(Duration.millis(TIMEOUT_CEILING_MS)))} seconds`,
            'if running of application (item 1 of argv) then',
            'tell application (item 1 of argv)',
            host.quit,
            'end tell',
            'end if',
            'end timeout',
            'end run',
        ];
        yield* reply(ChildProcess.make('osascript', ['-', found.bundlePath], { stdin: Stream.encodeText(Stream.make(shutdown.join('\n'))) })).pipe(
            Effect.mapError((exit) => classify(host.id, exit)),
            Effect.timeoutOrElse({
                duration: TIMEOUT_CEILING_MS,
                orElse: () => Effect.fail(BridgeError.cases.scriptTimedOut.make({ host: host.id, timeoutMs: TIMEOUT_CEILING_MS })),
            }),
        );
        yield* Effect.repeat(liveness(session.host), POLL).pipe(
            Effect.andThen(Effect.fail(InstallError.cases.hostStillRunning.make({ host: host.id }))),
            Effect.catchTag('hostNotRunning', () => Effect.void),
        );
        yield* Effect.forEach(Array.filter(yield* fs.readDirectory(external), String.startsWith(`${bridge.manifest.id}_`)), (entry) => fs.remove(path.join(external, entry), { recursive: true }));
        yield* fs.copy(path.join(base, ARTIFACTS, path.relative(base, directory)), path.join(external, folder));
        yield* fs.writeFileString(
            path.join(external, folder, 'manifest.json'),
            yield* Schema.encodeEffect(Schema.fromJsonString(Placed))({ ...bridge.manifest, host: { ...bridge.manifest.host, minVersion } }),
        );
        const registry = path.join(uxp, 'PluginsInfo', 'v1', `${host.uxp.app}.json`);
        const rows = yield* Schema.decodeEffect(Registry)(yield* fs.readFileString(registry));
        const registration = Registration.make({
            hostMinVersion: minVersion,
            name: bridge.manifest.name,
            path: `$localPlugins/External/${folder}`,
            pluginId: bridge.manifest.id,
            status: 'enabled',
            type: 'uxp',
            versionString: bridge.manifest.version,
        });
        yield* fs.writeFileString(registry, yield* Schema.encodeEffect(Registry)({ plugins: [...Array.filter(rows.plugins, (kept) => kept.pluginId !== bridge.manifest.id), registration] }));
        yield* Effect.mapError(reply(ChildProcess.make('open', ['-a', found.bundlePath])), exited(host.id));
        const launchedAt = yield* Clock.currentTimeMillis;
        const [activating, activation] = yield* Effect.timed(Effect.transposeOption(Option.map(activate, (open) => open(found))));
        const attachment = yield* request(TIMEOUT_CEILING_MS);
        yield* attached(session.link).pipe(
            Effect.timeoutOrElse({
                duration: attachment.deadlineAt - (yield* Clock.currentTimeMillis),
                orElse: () => Effect.fail(BridgeError.cases.deadlineExceeded.make({ host: host.id, jobId: attachment.jobId })),
            }),
        );
        const attachedAt = yield* Clock.currentTimeMillis;
        const probe = yield* Effect.flatMap(request(PROBE_MS), (entry) => probing(session, entry, Option.none())).pipe(
            Effect.flatMap(Effect.fromResult),
            Effect.retry({ while: Predicate.some([Predicate.isTagged('hostBusy'), Predicate.isTagged('hostNotAttached'), Predicate.isTagged('deadlineExceeded')]), schedule: POLL }),
        );
        const probedAt = yield* Clock.currentTimeMillis;
        const link = yield* Effect.map(linkState(session.link), Schema.encodeSync(Schema.toCodecJson(Link)));
        yield* Console.log(
            JSON.stringify(
                {
                    folder,
                    registration,
                    activation: Option.getOrNull(activation),
                    activationMs: Duration.toMillis(activating),
                    attachedAfterLaunchMs: attachedAt - launchedAt,
                    probedAfterAttachMs: probedAt - attachedAt,
                    link,
                    probe,
                },
                null,
                4,
            ),
        );
    }).pipe(Effect.scoped, Effect.provide(layer([host.id])));

const main = <Name extends string>(command: Command.Command<Name, never, unknown, unknown, NodeServices.NodeServices>, version: string): void =>
    Command.run(command, { version }).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { HealthRow, Server };
export { deploy, InstallError, main, POLL, probed, server, until };
