import { layer } from '@effect/sql-sqlite-node/SqliteClient';
import { parse } from '@plist/parse';
import { run } from '@rasm/process';
import { Array, Context, Effect, FileSystem, Option, Order, Path, Predicate, Result, Schema, SchemaGetter, SchemaIssue, String, Struct } from 'effect';
import { SqlClient, SqlSchema } from 'effect/sql';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CODE = /^(?<product>.+?)(?<beta>BETA)?$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Channel = Schema.Literals(['release', 'beta']).annotate({ description: 'Release channel, Beta newest, then year releases' });
class Registered extends Schema.Opaque<Registered, { readonly brand: unique symbol }>()(Schema.Struct({ code: Schema.String, directory: Schema.String, bundle: Schema.String, build: Schema.OptionFromNullOr(Schema.String) })) {}
const Plist = Schema.Union([Schema.String, Schema.Uint8Array]).pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((input: string | Uint8Array) => Effect.try({ try: () => parse(Predicate.isString(input) ? input : new Uint8Array(input).buffer), catch: () => new SchemaIssue.InvalidValue({ expected: 'a property list' }) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
const Receipt = Plist.pipe(Schema.decodeTo(Schema.Struct({ volume: Schema.String, 'install-location': Schema.String })));
const Info = Plist.pipe(Schema.decodeTo(Schema.Struct({ CFBundleIdentifier: Schema.String, CFBundleShortVersionString: Schema.String, CFBundleDocumentTypes: Schema.optionalKey(Schema.Array(Schema.Struct({ CFBundleTypeExtensions: Schema.optionalKey(Schema.Array(Schema.String)) }))) })));
const Numeric = Order.mapInput(Array.makeOrder(Order.Number), (version: string) => version.split('.').map(Number));

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
    static readonly Order: Order.Order<Application> = Order.flip(Order.Struct({ channel: Order.mapInput(Order.Number, (channel: Application['channel']) => Channel.literals.indexOf(channel)), version: Numeric, build: Option.makeOrder(Numeric) }));
    static readonly installed = Effect.gen(function* () {
        const [files, path, registry] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Registry]);
        const candidates = yield* Effect.all(
            [
                products(undefined).pipe(
                    Effect.provide(layer({ filename: registry, readonly: true })),
                    Effect.when(files.exists(registry)),
                    Effect.map(Option.getOrElse(Array.empty)),
                    Effect.map(
                        Array.map(({ code, directory, bundle, build }) => {
                            const { product = code, beta } = CODE.exec(code)?.groups ?? {};
                            return { bundle, name: path.basename(directory), product: Option.some(product), channel: beta === undefined ? Channel.literals[0] : Channel.literals[1], build };
                        }),
                    ),
                ),
                run(['pkgutil', '--pkgs']).pipe(
                    Effect.flatMap(({ stdout }) => Effect.forEach(Array.filter(lines(stdout), String.startsWith('com.adobe.')), receipted(path), { concurrency: 'unbounded' })),
                    Effect.map((found) => Array.map(Array.flatten(found), (bundle) => ({ bundle, name: path.basename(bundle, '.app'), product: Option.none<string>(), channel: Channel.literals[0], build: Option.none<string>() }))),
                ),
            ],
            { concurrency: 'unbounded' },
        );
        const present = yield* Effect.filter(Array.flatten(candidates), ({ bundle }) => files.exists(bundle), { concurrency: 'unbounded' });
        const read = yield* Effect.forEach(
            present,
            Effect.fnUntraced(function* ({ bundle, name, product, channel, build }) {
                const info = yield* Effect.flatMap(files.readFile(path.join(bundle, 'Contents', 'Info.plist')), Schema.decodeEffect(Info));
                return Application.make({
                    name,
                    product: Option.getOrElse(product, () => info.CFBundleIdentifier),
                    channel,
                    version: info.CFBundleShortVersionString,
                    build,
                    identifier: info.CFBundleIdentifier,
                    documents: Array.dedupe((info.CFBundleDocumentTypes ?? []).flatMap(({ CFBundleTypeExtensions = [] }) => CFBundleTypeExtensions.map(String.toLowerCase))),
                    url: yield* path.toFileUrl(`${bundle}/`),
                });
            }),
            { concurrency: 'unbounded' },
        );
        const unique = Array.dedupeWith(read, (left, right) => left.url.href === right.url.href);
        const grouped = Object.values(Array.groupBy(Array.sort(unique, Application.Order), Struct.get('product')));
        return Array.flatten(Array.sortWith(grouped, (rows) => Array.headNonEmpty(rows).name, Order.String));
    });
}

// --- [SERVICES] ------------------------------------------------------------------------

class Registry extends Context.Service<Registry, string>()('@rasm/creative-cloud-desktop/application/Registry') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const products = SqlSchema.findAll({
    Request: Schema.Void,
    Result: Registered,
    execute: () =>
        SqlClient.SqlClient.use(
            (sql) => sql`SELECT SAPCode AS code, directory, build, directory || substr(launch, instr(launch, '/'), instr(launch, '.app/') + length('.app') - instr(launch, '/')) AS bundle
            FROM (SELECT SAPCode,
            MAX(Value) FILTER (WHERE Key = 'InstallDir') AS directory,
            MAX(Value) FILTER (WHERE Key = 'AppLaunch') AS launch,
            MAX(Value) FILTER (WHERE Key = 'BuildVersion') AS build,
            MAX(Value) FILTER (WHERE Key = 'isVisibleProduct') AS visible
            FROM product_installation_info JOIN product_installation_meta_info USING (SAPCode, ProductVersion, ProcessorFamily)
            GROUP BY SAPCode, ProductVersion, ProcessorFamily)
            WHERE visible = 'true' AND instr(launch, '.app/') > 0`,
        ),
});
const lines = (text: string) => text.split('\n').filter(String.isNonEmpty);
const receipted = (path: Path.Path) =>
    Effect.fnUntraced(function* (id: string) {
        const { volume, 'install-location': location } = yield* Effect.flatMap(run(['pkgutil', '--pkg-info-plist', id]), ({ stdout }) => Schema.decodeEffect(Receipt)(stdout));
        const listed = yield* Effect.when(run(['pkgutil', '--only-dirs', '--files', id]), Effect.succeed(location.startsWith('Applications/')));
        return Array.flatMap(Option.toArray(listed), ({ stdout }) => Array.filterMap(lines(stdout), (entry) => (!entry.includes('/') && entry.endsWith('.app') ? Result.succeed(path.join(volume, location, entry)) : Result.failVoid)));
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Application, Registry };
