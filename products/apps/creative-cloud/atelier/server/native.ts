import { glob } from 'node:fs/promises';
import { homedir } from 'node:os';
import { dirname, join } from 'node:path';
import { Installation, type InstanceSnapshot } from '@rasm/applications';
import { Workspace } from '@rasm/applications/workspace';
import type { Application } from '@rasm/creative-cloud-desktop';
import { Link, Upstream } from '@rasm/mcp';
import { HOSTS } from '@rasm/uxp';
import { Array, Cause, Duration, Effect, FileSystem, Option, Record, Schema, Stream, SubscriptionRef } from 'effect';
import { RegistryApplications } from '#/applications';

// --- [TYPES] ---------------------------------------------------------------------------

interface NativeServer {
    readonly host: string;
    readonly ports: Array.NonEmptyReadonlyArray<number>;
    readonly path: string;
    readonly prefix: string;
    readonly settings: Option.Option<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const NATIVE: Record.ReadonlyRecord<string, NativeServer> = {
    [HOSTS.AI.product]: { host: '127.0.0.1', ports: Array.range(18_412, 18_422), path: '/v1/mcp', prefix: 'illustrator_', settings: Option.some('Adobe Illustrator*Settings') },
    [HOSTS.aftereffects.product]: { host: '127.0.0.1', ports: [3100], path: '/aftereffects/mcp', prefix: 'aftereffects_', settings: Option.none() },
    [HOSTS.premierepro.product]: { host: '127.0.0.1', ports: [3000], path: '/premiere/mcp', prefix: 'premiere_', settings: Option.none() },
    [HOSTS.ame.product]: { host: '[::1]', ports: [3001], path: '/ame/mcp', prefix: 'mediaencoder_', settings: Option.none() },
};
const TOKEN = /\/AIMCPServer \{[^}]*\/AuthToken \[ \d+\s+(?<hex>[\da-f\s]+)\]/u;

// --- [MODELS] --------------------------------------------------------------------------

const RunConfig = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Unknown)).annotate({ description: 'Illustrator .runConfig, keyed by the application version that wrote it' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const token = (preferences: string) => Option.flatMap(Option.fromNullishOr(TOKEN.exec(preferences)?.groups?.['hex']), (hex) => Schema.decodeOption(Schema.StringFromHex)(hex.replaceAll(/\s/gu, '')));
const bearer = Effect.fnUntraced(function* (settings: string, { name, version }: Application) {
    const files = yield* FileSystem.FileSystem;
    const configs = yield* Stream.runCollect(Stream.fromAsyncIterable(glob(join(homedir(), 'Library', 'Preferences', settings, '*', '.runConfig')), (cause) => new Cause.UnknownError(cause, `${settings} preferences do not list`)));
    const held = yield* Effect.filter(configs, (file) => Effect.map(Effect.flatMap(files.readFileString(file), Schema.decodeEffect(RunConfig)), Record.has(version)));
    const config = yield* Effect.fromOption(Array.head(held), () => new Cause.NoSuchElementError(`No ${settings} folder holds preferences of ${name} ${version}`));
    const preferences = yield* files.readFileString(join(dirname(config), 'Adobe Illustrator Prefs'));
    return { authorization: `Bearer ${yield* Effect.fromOption(token(preferences), () => new Cause.NoSuchElementError(`${name} preferences hold no AIMCPServer AuthToken, which its MCP server writes once enabled`))}` };
});
const listeningPort =
    (row: Pick<Application, 'url'>, { ports }: Pick<NativeServer, 'ports'>) =>
    (instances: readonly InstanceSnapshot[]) =>
        Option.flatMap(Array.findFirst(instances, Installation.runs(row)), ({ listening }) => Array.findFirst(ports, (port) => listening.includes(port)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const native = Stream.unwrap(
    Effect.gen(function* () {
        const [rows, { running, blocking }] = yield* Effect.all([RegistryApplications, Workspace]);
        const proxy = (row: Application, entry: NativeServer) =>
            Upstream.fromHttp({
                peer: Link.make({ application: row.url, plugin: 'mcp', root: Option.none() }),
                enabled: true,
                prefix: entry.prefix,
                endpoint: Effect.gen(function* () {
                    const port = yield* Effect.fromOption(listeningPort(row, entry)(yield* SubscriptionRef.get(running)), () => new Cause.NoSuchElementError(`${row.name} listens on none of ${entry.ports.join(', ')}`));
                    return { url: new URL(`http://${entry.host}:${port}${entry.path}`), headers: yield* Option.match(entry.settings, { onNone: () => Effect.succeed({}), onSome: (settings) => bearer(settings, row) }) };
                }),
                presence: Stream.map(SubscriptionRef.changes(running), (instances) => Option.isSome(listeningPort(row, entry)(instances))),
                annotations: blocking(row),
                poll: Duration.minutes(1),
            });
        return SubscriptionRef.changes(rows).pipe(
            Stream.map(Array.flatMap((row) => Option.toArray(Option.map(Record.get(NATIVE, row.product), (entry) => proxy(row, entry))))),
            Stream.switchMap(Array.match({ onEmpty: () => Stream.succeed([]), onNonEmpty: (upstreams) => Stream.map(Stream.zipLatestAll(...upstreams), Array.flatten) })),
        );
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { native };
