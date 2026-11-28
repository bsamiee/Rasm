import { homedir } from 'node:os';
import { Target } from '@rasm/application-tools/handle/target';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Instance } from '@rasm/applications/server/instance';
import type { Application } from '@rasm/creative-cloud-desktop/application';
import { Preferences } from '@rasm/creative-cloud-desktop/tools';
import { Link } from '@rasm/mcp/link';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Upstream } from '@rasm/mcp/upstream';
import { run } from '@rasm/process';
import { Format } from '@rasm/report';
import { Unmatched } from '@rasm/search/unmatched';
import { HOSTS } from '@rasm/uxp/manifest';
import { Array, Data, Effect, Equivalence, FileSystem, Function, Glob, Option, Order, Path, Predicate, Record, Result, Schema, Stream, SubscriptionRef, UndefinedOr } from 'effect';
import { Xml } from 'effect/encoding';
import { Host, NetAddress } from 'effect/net';
import { Applications } from '#/applications';

// --- [TYPES] ---------------------------------------------------------------------------

type Store = Data.TaggedEnum<{
    IllustratorPrefs: object;
    PremiereData: { readonly root: readonly string[]; readonly folder: string; readonly file: string };
}>;
interface NativeServer {
    readonly prefix: string;
    readonly server: Host.Host;
    readonly ports: Array.NonEmptyReadonlyArray<number>;
    readonly path: string;
    readonly authenticated: boolean;
    readonly store: Store;
}

// --- [MODELS] --------------------------------------------------------------------------

const Store = Data.taggedEnum<Store>();
const McpPreferences = Schema.Struct({
    server: Schema.optionalKey(Schema.HostFromString),
    port: Schema.optionalKey(
        Schema.FiniteFromString.pipe(
            Schema.decodeTo(Schema.Port),
            Schema.catchDecoding(() => Effect.succeedNone),
        ),
    ),
    path: Schema.optionalKey(Schema.String),
    authenticated: Schema.optionalKey(Schema.Union([Schema.Literal('1').transform(true), Schema.Literal('0').transform(false)])),
}).pipe(Schema.encodeKeys({ server: 'HSL.Prefs.McpServer', port: 'HSL.Prefs.McpPort', path: 'HSL.Prefs.McpPath', authenticated: 'HSL.Prefs.McpAuthenticationEnabled' }));

// --- [TABLES] --------------------------------------------------------------------------

const NATIVE: Record.ReadonlyRecord<string, NativeServer> = {
    [HOSTS.AI.product]: { prefix: 'illustrator_', server: NetAddress.ipv4Loopback, ports: Array.range(18_412, 18_422), path: 'v1/mcp', authenticated: true, store: Store.IllustratorPrefs() },
    [HOSTS.aftereffects.product]: {
        prefix: 'aftereffects_',
        server: NetAddress.ipv4Loopback,
        ports: [3100, 3000],
        path: 'aftereffects/mcp',
        authenticated: false,
        store: Store.PremiereData({ root: ['Library', 'Preferences', 'Adobe'], folder: 'After Effects', file: 'Adobe After Effects * MC Prefs' }),
    },
    [HOSTS.premierepro.product]: {
        prefix: 'premiere_',
        server: NetAddress.ipv4Loopback,
        ports: [3000],
        path: 'premiere/mcp',
        authenticated: true,
        store: Store.PremiereData({ root: ['Documents', 'Adobe'], folder: 'Premiere Pro', file: 'Profile-*/Adobe Premiere Pro* Prefs' }),
    },
    [HOSTS.ame.product]: {
        prefix: 'mediaencoder_',
        server: NetAddress.ipv6Loopback,
        ports: [3001],
        path: 'ame/mcp',
        authenticated: false,
        store: Store.PremiereData({ root: ['Documents', 'Adobe'], folder: 'Adobe Media Encoder', file: 'Adobe Media Encoder Prefs' }),
    },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const readStore = (row: Application, entry: NativeServer) =>
    Store.$match(entry.store, {
        IllustratorPrefs: () => ({
            settings: Effect.succeed(entry),
            token: Effect.flatMap(Preferences.illustrator(row), (held) =>
                Effect.fromOption(
                    Array.findFirst(held, ({ plugin }) => Option.fromUndefinedOr(plugin.AIMCPServer?.AuthToken)),
                    () => Unmatched.of({ subject: `AIMCPServer AuthToken in the preferences of ${row.name}` }),
                ),
            ),
        }),
        PremiereData: ({ root, folder, file }) => ({
            settings: Effect.gen(function* () {
                const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
                const at = path.join(homedir(), ...root);
                const found = yield* files.glob(`${Glob.escape(row.channel === 'beta' ? `${folder} (Beta)` : folder)}/*/${file}`, { root: at });
                const written = yield* Effect.forEach(found, (each) => Effect.map(files.stat(path.join(at, each)), ({ mtime }) => ({ file: path.join(at, each), mtime })), { concurrency: 'unbounded' });
                const elements = yield* Array.match(written, {
                    onEmpty: () => Effect.succeed(Array.empty<Xml.Element>()),
                    onNonEmpty: (listed) => files.stream(Array.max(listed, Order.Struct({ mtime: Option.makeOrder(Order.Date) })).file).pipe(Stream.pipeThroughChannel(Xml.decode()), Stream.pipeThroughChannel(Xml.elements(['PremiereData', 'Preferences', 'Properties', '*'])), Stream.runCollect),
                });
                const { port, ...stated } = yield* Schema.decodeEffect(McpPreferences)(Record.fromEntries(Array.filterMap(elements, ({ name, children: [text] }) => (Predicate.isString(text) ? Result.succeed([name, text] as const) : Result.failVoid))));
                return { ...entry, ...stated, ports: UndefinedOr.match(port, { onUndefined: () => entry.ports, onDefined: (held) => Array.dedupe(Array.prepend(entry.ports, held)) }) };
            }),
            token: Effect.map(run(['security', 'find-generic-password', '-s', 'PremierePro.MCP', '-w']), ({ stdout }) => stdout.trim()),
        }),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const Native = {
    instruction: `A running application's own MCP server adds its tools under its prefix, one of ${Format.list(Record.values(NATIVE).map(({ prefix }) => prefix))}.`,
    upstreams: Stream.unwrap(
        Effect.gen(function* () {
            const [{ installed }, server] = yield* Effect.all([Applications, ApplicationServer]);
            const proxy = ([row, entry]: readonly [Application, NativeServer]) => {
                const { settings, token } = readStore(row, entry);
                const ports = (instances: readonly Instance[]) => Result.transposeMapOption(Array.findFirst(instances, Instance.runs(row)), ({ listeningPorts }) => listeningPorts);
                const port = (open: Option.Option<readonly number[]>, stated: Pick<NativeServer, 'ports'>) => Option.flatMap(open, (held) => Array.head(Array.intersection(stated.ports, held)));
                const early = ToolCall.use(({ returnEarly }) => Target.blocking(row).pipe(Effect.provideService(ApplicationServer, server), Effect.flatMap(returnEarly)));
                return Upstream.fromHttp({
                    peer: Link.make({ application: row.url, plugin: 'mcp' }),
                    prefix: entry.prefix,
                    endpoint: Effect.gen(function* () {
                        const [instances, stated] = yield* Effect.all([SubscriptionRef.get(server.running), settings]);
                        const open = yield* Effect.fromResult(ports(instances));
                        const at = yield* Effect.fromOption(port(open, stated), () => Unmatched.of({ subject: `MCP server port ${row.name} listens on`, candidates: stated.ports.map(String) }));
                        const authorization = stated.authenticated ? Option.some(`Bearer ${yield* token}`) : Option.none<string>();
                        return { url: new URL(stated.path, `http://${Host.formatHostPort(Host.hostPortUnsafe(stated.server, at))}`), headers: Record.getSomes({ authorization }) };
                    }),
                    presence: SubscriptionRef.changes(server.running).pipe(
                        Stream.map(ports),
                        Stream.changesWith(Result.makeEquivalence(Option.makeEquivalence(Array.makeEquivalence(Equivalence.Number)), Function.constTrue)),
                        Stream.tap((read) => Result.match(read, { onSuccess: () => Effect.void, onFailure: (failure) => Effect.logWarning(`${row.name} listening ports are unread, its MCP server stands as last seen`, failure) })),
                        Stream.filterMap(Function.identity),
                        Stream.mapEffect((open) => Effect.map(settings, (stated) => Option.isSome(port(open, stated))).pipe(Effect.catch((error) => Effect.as(Effect.logWarning(`${row.name} MCP server preferences are unreadable`, error), false)))),
                    ),
                }).pipe(Stream.map(Array.map((each) => new Upstream({ ...each, call: (name, given) => Effect.andThen(early, each.call(name, given)) }))));
            };
            return SubscriptionRef.changes(installed).pipe(
                Stream.map(Array.filterMap((row: Application) => Option.match(Record.get(NATIVE, row.product), { onNone: () => Result.failVoid, onSome: (entry) => Result.succeed([row, entry] as const) }))),
                Stream.changesWith(Array.makeEquivalence(Equivalence.mapInput(Equivalence.Struct({ url: Equivalence.mapInput(Equivalence.String, (url: URL) => url.href), version: Equivalence.String }), ([row]: readonly [Application, NativeServer]) => row))),
                Stream.switchMap((served) => Array.match(Array.map(served, proxy), { onEmpty: () => Stream.succeed([]), onNonEmpty: (upstreams) => Stream.map(Stream.zipLatestAll(...upstreams), Array.flatten) })),
            );
        }),
    ),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Native };
