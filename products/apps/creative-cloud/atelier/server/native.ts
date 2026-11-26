import { homedir } from 'node:os';
import { Installation, type InstanceSnapshot } from '@rasm/applications/contract';
import { Workspace } from '@rasm/applications/workspace';
import type { Application } from '@rasm/creative-cloud-desktop/application';
import { Preferences } from '@rasm/creative-cloud-desktop/tools';
import { Link } from '@rasm/mcp/link';
import { Upstream } from '@rasm/mcp/upstream';
import { run } from '@rasm/process';
import { HOSTS } from '@rasm/uxp/manifest';
import { Array, Cause, Data, Duration, Effect, Equivalence, FileSystem, Glob, Option, Order, Path, Predicate, Record, Result, Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { Xml } from 'effect/encoding';
import { Host } from 'effect/net';
import { RegistryApplications } from '#/applications';

// --- [TYPES] ---------------------------------------------------------------------------

type Store = Data.TaggedEnum<{
    IllustratorPrefs: object;
    PremiereData: { readonly root: readonly string[]; readonly folder: string; readonly file: string };
}>;
interface NativeServer {
    readonly prefix: string;
    readonly server: Host.Host;
    readonly port: number;
    readonly fallback: readonly number[];
    readonly path: string;
    readonly authenticated: boolean;
    readonly store: Store;
}

// --- [MODELS] --------------------------------------------------------------------------

const Store = Data.taggedEnum<Store>();
const McpPort = Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Port));
const McpAuthenticationEnabled = Schema.Union([Schema.Literal('1').transform(true), Schema.Literal('0').transform(false)]);

// --- [TABLES] --------------------------------------------------------------------------

const NATIVE: Record.ReadonlyRecord<string, NativeServer> = {
    [HOSTS.AI.product]: { prefix: 'illustrator_', server: Host.hostFromStringUnsafe('127.0.0.1'), port: 18_412, fallback: Array.range(18_413, 18_422), path: 'v1/mcp', authenticated: true, store: Store.IllustratorPrefs() },
    [HOSTS.aftereffects.product]: {
        prefix: 'aftereffects_',
        server: Host.hostFromStringUnsafe('127.0.0.1'),
        port: 3100,
        fallback: [3000],
        path: 'aftereffects/mcp',
        authenticated: false,
        store: Store.PremiereData({ root: ['Library', 'Preferences', 'Adobe'], folder: 'After Effects', file: 'Adobe After Effects * MC Prefs' }),
    },
    [HOSTS.premierepro.product]: {
        prefix: 'premiere_',
        server: Host.hostFromStringUnsafe('127.0.0.1'),
        port: 3000,
        fallback: [],
        path: 'premiere/mcp',
        authenticated: true,
        store: Store.PremiereData({ root: ['Documents', 'Adobe'], folder: 'Premiere Pro', file: 'Profile-*/Adobe Premiere Pro* Prefs' }),
    },
    [HOSTS.ame.product]: {
        prefix: 'mediaencoder_',
        server: Host.hostFromStringUnsafe('localhost'),
        port: 3001,
        fallback: [],
        path: 'ame/mcp',
        authenticated: false,
        store: Store.PremiereData({ root: ['Documents', 'Adobe'], folder: 'Adobe Media Encoder', file: 'Adobe Media Encoder Prefs' }),
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const listeningPort = (listening: Option.Option<readonly number[]>, { port, fallback }: Pick<NativeServer, 'port' | 'fallback'>) => Option.flatMap(listening, (held) => Array.findFirst([port, ...fallback], (each) => held.includes(each)));
const readPremiereData = Effect.fnUntraced(function* (row: Application, entry: NativeServer, { root, folder, file }: Data.TaggedEnum.Value<Store, 'PremiereData'>) {
    const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const at = path.join(homedir(), ...root);
    const found = yield* files.glob(`${Glob.escape(row.channel === 'beta' ? `${folder} (Beta)` : folder)}/*/${file}`, { root: at });
    const written = yield* Effect.forEach(found, (each) => Effect.map(files.stat(path.join(at, each)), ({ mtime }) => ({ file: path.join(at, each), mtime })));
    const newest = Option.map(Option.liftPredicate(written, Array.isArrayNonEmpty<(typeof written)[number]>), Array.max(Order.mapInput(Option.makeOrder(Order.Date), (each: (typeof written)[number]) => each.mtime)));
    const elements = yield* Option.match(newest, {
        onNone: () => Effect.succeed(Array.empty<Xml.Element>()),
        onSome: (chosen) => Stream.unwrap(FileSystem.FileSystem.useSync((held) => held.stream(chosen.file))).pipe(Stream.pipeThroughChannel(Xml.decode()), Stream.pipeThroughChannel(Xml.elements(['PremiereData', 'Preferences', 'Properties', '*'])), Stream.runCollect),
    });
    const stated = Record.fromEntries(Array.filterMap(elements, ({ name, children: [text] }) => (Predicate.isString(text) ? Result.succeed([name, text] as const) : Result.failVoid)));
    return {
        ...entry,
        server: Option.getOrElse(Option.flatMap(Record.get(stated, 'HSL.Prefs.McpServer'), Schema.decodeUnknownOption(Schema.HostFromString)), () => entry.server),
        port: Option.getOrElse(Option.flatMap(Record.get(stated, 'HSL.Prefs.McpPort'), Schema.decodeUnknownOption(McpPort)), () => entry.port),
        path: Option.getOrElse(Record.get(stated, 'HSL.Prefs.McpPath'), () => entry.path),
        authenticated: Option.getOrElse(Option.flatMap(Record.get(stated, 'HSL.Prefs.McpAuthenticationEnabled'), Schema.decodeUnknownOption(McpAuthenticationEnabled)), () => entry.authenticated),
    };
});
const readStore = (row: Application, entry: NativeServer) =>
    Store.$match(entry.store, {
        IllustratorPrefs: () => ({
            settings: Effect.succeed(entry),
            token: Effect.flatMap(Preferences.illustrator(row), (held) =>
                Effect.fromOption(
                    Option.map(
                        Array.findFirst(held, ({ plugin }) => Option.fromUndefinedOr(plugin.AIMCPServer)),
                        ({ AuthToken }) => AuthToken,
                    ),
                    () => new Cause.NoSuchElementError(`${row.name} preferences hold no AIMCPServer AuthToken, which its MCP server writes once enabled`),
                ),
            ),
        }),
        PremiereData: (place) => ({
            settings: readPremiereData(row, entry, place),
            token: Effect.map(run(['security', 'find-generic-password', '-s', 'PremierePro.MCP', '-w']), ({ stdout }) => stdout.trim()),
        }),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const native = Stream.unwrap(
    Effect.gen(function* () {
        const [rows, { running, blocking }] = yield* Effect.all([RegistryApplications, Workspace]);
        const proxy = (row: Application, entry: NativeServer) => {
            const { settings, token } = readStore(row, entry);
            const listening = (instances: readonly InstanceSnapshot[]) => Option.map(Array.findFirst(instances, Installation.runs(row)), Struct.get('listening'));
            return Upstream.fromHttp({
                peer: Link.make({ application: row.url, plugin: 'mcp' }),
                prefix: entry.prefix,
                endpoint: Effect.gen(function* () {
                    const [instances, stated] = yield* Effect.all([SubscriptionRef.get(running), settings]);
                    const port = yield* Effect.fromOption(listeningPort(listening(instances), stated), () => new Cause.NoSuchElementError(`${row.name} listens on none of ${[stated.port, ...stated.fallback].join(', ')}`));
                    const authorization = yield* Effect.when(
                        Effect.gen(function* () {
                            return `Bearer ${yield* token}`;
                        }),
                        Effect.succeed(stated.authenticated),
                    );
                    return { url: new URL(stated.path, `http://${Host.formatHostPort(Host.hostPortUnsafe(stated.server, port))}`), headers: Record.getSomes({ authorization }) };
                }),
                presence: SubscriptionRef.changes(running).pipe(
                    Stream.map((instances) => listening(instances)),
                    Stream.changesWith(Option.makeEquivalence(Array.makeEquivalence(Equivalence.strictEqual<number>()))),
                    Stream.mapEffect((held) => Effect.map(settings, (stated) => Option.isSome(listeningPort(held, stated))).pipe(Effect.catch((error) => Effect.as(Effect.logWarning(`${row.name} MCP server preferences are unreadable`, error), false)))),
                ),
                annotations: blocking(row),
                poll: Duration.minutes(1),
            });
        };
        return SubscriptionRef.changes(rows).pipe(
            Stream.map((held) => held.filter((row) => Record.has(NATIVE, row.product))),
            Stream.changesWith(Array.makeEquivalence(Equivalence.make((left: Application, right: Application) => left.url.href === right.url.href && left.version === right.version))),
            Stream.map(Array.flatMap((row) => Option.toArray(Option.map(Record.get(NATIVE, row.product), (entry) => proxy(row, entry))))),
            Stream.switchMap(Array.match({ onEmpty: () => Stream.succeed([]), onNonEmpty: (upstreams) => Stream.map(Stream.zipLatestAll(...upstreams), Array.flatten) })),
        );
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { native };
