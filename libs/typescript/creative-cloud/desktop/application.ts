import { layer } from '@effect/sql-sqlite-node/SqliteClient';
import { parse } from '@plist/parse';
import { ExitError, run } from '@rasm/process';
import { Array, Effect, FileSystem, Option, Order, Path, Schema, String, Struct } from 'effect';
import { SqlClient, SqlSchema } from 'effect/sql';

// --- [CONSTANTS] -----------------------------------------------------------------------

const INSTALL_DIRECTORY = /\[installdir\]/iu;
const CHANNELS = ['release', 'beta', 'prerelease'] as const;
const CODE = new RegExp(`^(?<product>.+?)(?<suffix>${CHANNELS.slice(1).map(String.toUpperCase).join('|')})?$`, 'u');

// --- [MODELS] --------------------------------------------------------------------------

const Channel = Schema.Literals(CHANNELS).annotate({ description: 'Release channel, prerelease newest, then Beta, then year releases' });
class Registered extends Schema.Opaque<Registered, { readonly brand: unique symbol }>()(Schema.Struct({ code: Schema.String, directory: Schema.String, launch: Schema.String, build: Schema.NullOr(Schema.String) })) {}
const Receipt = Schema.Struct({ volume: Schema.String, 'install-location': Schema.String });
const Info = Schema.Struct({
    CFBundleIdentifier: Schema.String,
    CFBundleShortVersionString: Schema.String,
    CFBundleDocumentTypes: Schema.Array(Schema.Struct({ CFBundleTypeExtensions: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
});
const Numeric = Order.mapInput(Array.makeOrder(Order.Number), (version: string) => version.split('.').map(Number));
const Released = Order.combine(
    Order.mapInput(Order.Number, ({ channel }: Application) => CHANNELS.indexOf(channel)),
    Order.combine(
        Order.mapInput(Numeric, ({ version }: Application) => version),
        Order.mapInput(Option.makeOrder(Numeric), ({ build }: Application) => build),
    ),
);

class Application extends Schema.Class<Application, { readonly brand: unique symbol }>('Application')({
    name: Schema.String.annotate({ description: 'Folder or app its installer placed' }),
    product: Schema.String.annotate({ description: 'Creative Cloud product code, or bundle identifier of an app outside the Creative Cloud registry' }),
    channel: Channel,
    version: Schema.String.annotate({ description: 'CFBundleShortVersionString' }),
    build: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Creative Cloud registry BuildVersion' })),
    identifier: Schema.String.annotate({ description: 'Bundle identifier' }),
    documents: Schema.Array(Schema.String).annotate({ description: 'Lowercase file extensions of the document types the bundle declares' }),
    url: Schema.URL.annotate({ description: 'Bundle' }),
}) {
    static readonly Order: Order.Order<Application> = (left, right) => Released(right, left);
    static readonly installed = Effect.gen(function* () {
        const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const registry = '/Library/Application Support/Adobe/caps/hdpim.db';
        const candidates = yield* Effect.all(
            [
                products(undefined).pipe(Effect.provide(layer({ filename: registry, readonly: true })), Effect.when(files.exists(registry)), Effect.map(Option.match({ onNone: Array.empty, onSome: Array.flatMap(registered(path)) }))),
                run(['pkgutil', '--pkgs=com[.]adobe[.]acrobat[.].*']).pipe(
                    Effect.map(({ stdout }) => lines(stdout)),
                    Effect.catchIf(absent, () => Effect.succeed(Array.empty<string>())),
                    Effect.flatMap(Effect.forEach(receipted(path), { concurrency: 'unbounded' })),
                    Effect.map((found) => Array.map(Array.flatten(found), placed(path))),
                ),
                run(['osascript', '-e', 'POSIX path of (path to application id "com.adobe.acc.AdobeCreativeCloud")']).pipe(
                    Effect.map(({ stdout }) => [path.resolve(stdout.trim())]),
                    Effect.catchIf(absent, () => Effect.succeed(Array.empty<string>())),
                    Effect.map(Array.map(placed(path))),
                ),
            ],
            { concurrency: 'unbounded' },
        );
        const present = yield* Effect.filter(Array.flatten(candidates), ({ bundle }) => files.exists(bundle), { concurrency: 'unbounded' });
        const read = yield* Effect.forEach(
            present,
            Effect.fnUntraced(function* ({ bundle, name, product, channel, build }) {
                const info = yield* files.readFile(path.join(bundle, 'Contents', 'Info.plist')).pipe(Effect.flatMap((bytes) => Schema.decodeUnknownEffect(Info)(parse(new Uint8Array(bytes).buffer))));
                return Application.make({
                    name,
                    product: Option.getOrElse(product, () => info.CFBundleIdentifier),
                    channel,
                    version: info.CFBundleShortVersionString,
                    build,
                    identifier: info.CFBundleIdentifier,
                    documents: info.CFBundleDocumentTypes.flatMap(({ CFBundleTypeExtensions }) => CFBundleTypeExtensions.map(String.toLowerCase)),
                    url: yield* path.toFileUrl(`${bundle}/`),
                });
            }),
            { concurrency: 'unbounded' },
        );
        const grouped = Array.groupBy(
            Array.sort(
                Array.dedupeWith(read, (left, right) => left.url.href === right.url.href),
                Application.Order,
            ),
            Struct.get('product'),
        );
        return Array.flatten(
            Array.sort(
                Object.values(grouped),
                Order.mapInput(Order.String, (rows: Array.NonEmptyReadonlyArray<Application>) => Array.headNonEmpty(rows).name),
            ),
        );
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const products = SqlSchema.findAll({
    Request: Schema.Void,
    Result: Registered,
    execute: () =>
        SqlClient.SqlClient.use(
            (sql) => sql`SELECT SAPCode AS code,
            MAX(Value) FILTER (WHERE Key = 'InstallDir') AS directory,
            MAX(Value) FILTER (WHERE Key = 'AppLaunch') AS launch,
            MAX(Value) FILTER (WHERE Key = 'BuildVersion') AS build
            FROM product_installation_info JOIN product_installation_meta_info USING (SAPCode, ProductVersion, ProcessorFamily)
            GROUP BY SAPCode, ProductVersion, ProcessorFamily
            HAVING MAX(Value) FILTER (WHERE Key = 'isVisibleProduct') = 'true' AND launch <> ''`,
        ),
});
const lines = (text: string) => text.split('\n').filter(String.isNonEmpty);
const absent = (error: unknown): error is ExitError => Schema.is(ExitError)(error) && error.code === 1 && String.isEmpty(error.stdout);
const placed =
    (path: Path.Path) =>
    (bundle: string): { readonly bundle: string; readonly name: string; readonly product: Option.Option<string>; readonly channel: Application['channel']; readonly build: Option.Option<string> } => ({ bundle, name: path.basename(bundle, '.app'), product: Option.none(), channel: CHANNELS[0], build: Option.none() });
const registered =
    (path: Path.Path) =>
    ({ code, directory, launch, build }: Registered) => {
        const parts = launch.replace(INSTALL_DIRECTORY, directory).split('/');
        const { product = code, suffix } = CODE.exec(code)?.groups ?? {};
        const channel = Option.getOrElse(
            Array.findFirst(CHANNELS, (each) => String.toUpperCase(each) === suffix),
            () => CHANNELS[0],
        );
        return Option.toArray(Option.map(Array.findFirstIndex(parts, String.endsWith('.app')), (at) => ({ bundle: parts.slice(0, at + 1).join('/'), name: path.basename(directory), product: Option.some(product), channel, build: Option.fromNullOr(build) })));
    };
const receipted = (path: Path.Path) =>
    Effect.fnUntraced(function* (id: string) {
        const [receipt, { stdout }] = yield* Effect.all([run(['pkgutil', '--pkg-info-plist', id]).pipe(Effect.flatMap(({ stdout: plist }) => Schema.decodeUnknownEffect(Receipt)(parse(plist)))), run(['pkgutil', '--only-dirs', '--files', id])]);
        return Array.map(
            Array.filter(lines(stdout), (entry) => receipt['install-location'].startsWith('Applications/') && !entry.includes('/') && entry.endsWith('.app')),
            (app) => path.join(receipt.volume, receipt['install-location'], app),
        );
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Application };
