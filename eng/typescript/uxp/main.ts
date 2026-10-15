import { DatabaseSync } from 'node:sqlite';
import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Cause, Config, Effect, FileSystem, flow, identity, Option, Order, Path, pipe, Result, Schema, String, Struct } from 'effect';
import { Argument, Command } from 'effect/cli';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { XMLParser } from 'fast-xml-parser';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ADOBE = '/Library/Application Support/Adobe';
const INSTALLER = `${ADOBE}/Adobe Desktop Common/RemoteComponents/UPI`;

// --- [MODELS] --------------------------------------------------------------------------

const Host = Schema.Struct({ app: Schema.String, minVersion: Schema.String });
const Manifest = Schema.fromJsonString(Schema.StructWithRest(Schema.Struct({ id: Schema.String, name: Schema.String, version: Schema.String, host: Schema.Union([Host, Schema.Array(Host)]) }), [Schema.JsonObject]));
const Registry = Schema.fromJsonString(Schema.Struct({ plugins: Schema.Array(Schema.StructWithRest(Schema.Struct({ pluginId: Schema.String, path: Schema.String }), [Schema.JsonObject])) }));
const Installed = Schema.Array(Schema.Struct({ SAPCode: Schema.String, ProductVersion: Schema.String }));
const Products = Schema.Struct({
    Configuration: Schema.Struct({
        Product: Schema.Array(
            Schema.Struct({
                sapCode: Schema.OptionFromOptionalKey(Schema.String),
                minVersion: Schema.OptionFromOptionalKey(Schema.String),
                Data: Schema.Array(Schema.Struct({ key: Schema.String, '#text': Schema.OptionFromOptionalKey(Schema.String) })),
            }),
        ),
    }),
});

// --- [ERRORS] --------------------------------------------------------------------------

const InstallReason = Schema.TaggedUnion({ Refused: { host: Schema.String, answer: Schema.String } });
class InstallError extends Schema.TaggedError<InstallError, { readonly brand: unique symbol }>()('InstallError', { reason: InstallReason }) {
    override get message(): string {
        return InstallReason.match(this.reason, { Refused: ({ host, answer }) => `Adobe's plug-in installer did not install the ${host} archive: ${answer}` });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const stage = Effect.fn('stage')(function* (built: string, folder: string, manifest: typeof Manifest.Type, host: typeof Host.Type) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    yield* fs.remove(folder, { recursive: true, force: true });
    yield* fs.copy(built, folder);
    yield* fs.writeFileString(path.join(folder, 'manifest.json'), yield* Schema.encodeEffect(Manifest)({ ...manifest, host }));
});
const register = Effect.fn('register')(function* (built: string, manifest: typeof Manifest.Type, host: typeof Host.Type) {
    const [fs, path, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME')]);
    const [uxp, token] = [path.join(home, ADOBE, 'UXP'), '$localPlugins'];
    const [local, registry, folder] = [path.join(uxp, 'Plugins'), path.join(uxp, 'PluginsInfo', 'v1', `${host.app}.json`), path.join('External', `${manifest.id}_${host.app}`)];
    const [kept, replaced] = Array.partition(
        yield* fs.readFileString(registry).pipe(
            Effect.flatMap(Schema.decodeEffect(Registry)),
            Effect.map(Struct.get('plugins')),
            Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed([])),
        ),
        Result.liftPredicate(({ pluginId }) => pluginId !== manifest.id, identity),
    );
    yield* Effect.forEach(replaced, (row) => fs.remove(String.replace(token, local)(row.path), { recursive: true, force: true }), { discard: true });
    yield* stage(built, path.join(local, folder), manifest, host);
    yield* fs.makeDirectory(path.dirname(registry), { recursive: true });
    yield* fs.writeFileString(
        registry,
        yield* Schema.encodeEffect(Registry)({
            plugins: [...kept, { hostMinVersion: host.minVersion, name: manifest.name, path: path.join(token, folder), pluginId: manifest.id, status: 'enabled', type: 'uxp', versionString: manifest.version }],
        }),
    );
});
const submit = Effect.fn('submit')(function* (built: string, manifest: typeof Manifest.Type, host: typeof Host.Type) {
    const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
    const staged = path.join(yield* fs.makeTempDirectoryScoped(), `${manifest.id}_${host.app}`);
    yield* stage(built, staged, manifest, host);
    yield* spawner.string(ChildProcess.make('ditto', ['-c', '-k', '--norsrc', staged, `${staged}.ccx`]));
    yield* spawner
        .string(ChildProcess.make(`${INSTALLER}/UnifiedPluginInstallerAgent/UnifiedPluginInstallerAgent.app/Contents/MacOS/UnifiedPluginInstallerAgent`, ['--install', `${staged}.ccx`]))
        .pipe(Effect.filterOrFail(String.includes('Installation Successful'), (answer) => InstallError.make({ reason: { _tag: 'Refused', host: host.app, answer: String.trim(answer) } })));
}, Effect.scoped);
const install = Effect.fn('install')(function* ({ built }: { readonly built: string }) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const [manifest, { Configuration }, installed] = yield* Effect.all(
        [
            Effect.flatMap(fs.readFileString(path.join(built, 'manifest.json')), Schema.decodeEffect(Manifest)),
            Effect.flatMap(fs.readFileString(`${INSTALLER}/ExManCoreLib/Contents/Resources/XManConfig.xml`), (xml) => Schema.decodeUnknownEffect(Products)(new XMLParser({ ignoreAttributes: false, attributeNamePrefix: '', parseTagValue: false, isArray: (name) => name === 'Product' || name === 'Data' }).parse(xml))),
            Effect.acquireUseRelease(
                Effect.sync(() => new DatabaseSync(`${ADOBE}/caps/hdpim.db`, { readOnly: true })),
                (products) => Schema.decodeUnknownEffect(Installed)(products.prepare('SELECT SAPCode, ProductVersion FROM product_installation_info').all()),
                (products) => Effect.sync(() => products.close()),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const atLeast = pipe(
        Array.makeOrder(Order.Number),
        Order.mapInput(
            flow(
                String.split('.'),
                Array.map(Number),
                Array.reverse,
                Array.dropWhile((part) => part === 0),
                Array.reverse,
            ),
        ),
        Order.isGreaterThanOrEqualTo,
    );
    const owned = Array.flatMap(Configuration.Product, ({ sapCode, minVersion, Data }) =>
        Option.exists(Option.all([sapCode, minVersion]), ([code, minimum]) => Array.some(installed, ({ SAPCode, ProductVersion }) => SAPCode === code && atLeast(ProductVersion, minimum))) ? Array.flatMap(Data, ({ key, '#text': text }) => (key === 'UxpHostCode' ? Option.toArray(text) : [])) : [],
    );
    yield* Effect.validate(
        Array.ensure(manifest.host),
        Effect.fnUntraced(function* (host) {
            return yield* Array.contains(owned, host.app) ? submit(built, manifest, host) : register(built, manifest, host);
        }),
        { discard: true },
    ).pipe(Effect.catch((errors) => Effect.failCause(Cause.fromReasons(Array.map(errors, Cause.makeFailReason)))));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Command.make('install', { built: Argument.Directory('built', { mustExist: true }) }, install).pipe(Command.run({ version: '0' }), Effect.provide(NodeServices.layer), NodeRuntime.runMain);
