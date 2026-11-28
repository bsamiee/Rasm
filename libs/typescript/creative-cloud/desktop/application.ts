import type { Installation } from '@rasm/applications/server/installation';
import { sqliteNodeEngine } from '@rasm/database/sqlite-node';
import { Array, Context, Effect, FileSystem, Option, Order, Path, Record, Result, Schema, Struct } from 'effect';
import { SqlClient, SqlSchema } from 'effect/sql';

// --- [MODELS] --------------------------------------------------------------------------

const Channel = Schema.Literals(['release', 'beta']);

class Application extends Schema.Class<Application, { readonly brand: unique symbol }>('Application')({
    name: Schema.String,
    product: Schema.String,
    channel: Channel,
    version: Schema.String,
    build: Schema.OptionFromOptionalKey(Schema.String),
    identifier: Schema.String,
    url: Schema.URL,
    urlSchemes: Schema.Array(Schema.String).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
}) {
    static readonly VersionOrder: Order.Order<string> = Order.mapInput(Array.makeOrder(Order.Number), (version: string) => version.split('.').map(Number));
    static readonly Order: Order.Order<Application> = Order.flip(Order.Struct({ channel: Order.mapInput(Order.Number, (channel: Application['channel']) => Channel.literals.indexOf(channel)), version: Application.VersionOrder, build: Option.makeOrder(Application.VersionOrder) }));
    static readonly fromInstallations = Effect.fnUntraced(function* (offered: readonly Installation[]) {
        const [files, path, registry] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Registry]);
        const rows = yield* findProducts(undefined).pipe(Effect.provide(sqliteNodeEngine({ filename: registry, readonly: true })), Effect.when(files.exists(registry)), Effect.map(Option.getOrElse(Array.empty)));
        const registered = Record.fromIterableBy(rows, Struct.get('executable'));
        const listed = yield* Effect.filterMapEffect(
            offered,
            Effect.fnUntraced(function* ({ url, identifier, name, version, executable, urlSchemes }) {
                const at = yield* Effect.transposeOption(Option.map(executable, path.fromFileUrl));
                const row = Option.flatMap(at, (key) => Record.get(registered, key));
                const [product, channel] = Option.match(row, { onNone: () => [identifier, 'release'] as const, onSome: Struct.get('code') });
                return Option.match(version, { onNone: () => Result.failVoid, onSome: (stated) => Result.succeed(Application.make({ name, product, channel, version: stated, build: Option.flatMap(row, Struct.get('build')), identifier, url, urlSchemes: Option.getOrElse(urlSchemes, Array.empty) })) });
            }),
        );
        const grouped = Record.values(Array.groupBy(Array.sort(listed, Application.Order), Struct.get('product')));
        return Array.flatten(Array.sortWith(grouped, (forms) => Array.headNonEmpty(forms).name, Order.String));
    });
}

// --- [SERVICES] ------------------------------------------------------------------------

class Registry extends Context.Service<Registry, string>()('@rasm/creative-cloud-desktop/application/Registry') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const findProducts = SqlSchema.findAll({
    Request: Schema.Void,
    Result: Schema.Struct({
        code: Schema.Union([Schema.TemplateLiteralParser([Schema.String, Schema.Literal('BETA').transform('beta')]), Schema.TemplateLiteralParser([Schema.String, Schema.Literal('').transform('release')])]),
        executable: Schema.String,
        build: Schema.OptionFromNullOr(Schema.String),
    }),
    execute: () =>
        SqlClient.SqlClient.use(
            (sql) => sql`SELECT SAPCode AS code, build, directory || substr(launch, instr(launch, '/')) AS executable
            FROM (SELECT SAPCode,
            MAX(Value) FILTER (WHERE Key = 'InstallDir') AS directory,
            MAX(Value) FILTER (WHERE Key = 'AppLaunch') AS launch,
            MAX(Value) FILTER (WHERE Key = 'BuildVersion') AS build
            FROM product_installation_info JOIN product_installation_meta_info USING (SAPCode, ProductVersion, ProcessorFamily)
            GROUP BY SAPCode, ProductVersion, ProcessorFamily)
            WHERE instr(launch, '/') > 0`,
        ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Application, Registry };
