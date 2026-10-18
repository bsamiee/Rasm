import { DatabaseSync } from 'node:sqlite';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Array, Cause, Config, Effect, Exit, FileSystem, Filter, flow, Option, Order, Path, pipe, Schema, String, Struct } from 'effect';
import { Argument, Command } from 'effect/cli';
import { XMLParser } from 'fast-xml-parser';
import { stdout } from '../process.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ADOBE = '/Library/Application Support/Adobe';
const INSTALLER = `${ADOBE}/Adobe Desktop Common/RemoteComponents/UPI`;

// --- [OPERATIONS] ----------------------------------------------------------------------

const install = Effect.fn('install')(function* (built: string, agent: string) {
    const [fs, path, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME')]);
    const Manifest = Schema.fromJsonString(Schema.StructWithRest(Schema.Struct({ id: Schema.String, name: Schema.String, version: Schema.String, host: Schema.ArrayEnsure(Schema.Struct({ app: Schema.String, minVersion: Schema.String })) }), [Schema.JsonObject]));
    const [manifest, { Configuration }, installed] = yield* Effect.all(
        [
            Effect.flatMap(fs.readFileString(path.join(built, 'manifest.json')), Schema.decodeEffect(Manifest)),
            Effect.flatMap(fs.readFileString(`${INSTALLER}/ExManCoreLib/Contents/Resources/XManConfig.xml`), (xml) =>
                Schema.decodeUnknownEffect(
                    Schema.Struct({
                        Configuration: Schema.Struct({
                            Product: Schema.Array(
                                Schema.Struct({
                                    sapCode: Schema.OptionFromOptionalKey(Schema.String),
                                    minVersion: Schema.OptionFromOptionalKey(Schema.String),
                                    Data: Schema.Array(Schema.Struct({ key: Schema.String, '#text': Schema.OptionFromOptionalKey(Schema.String) })),
                                }),
                            ),
                        }),
                    }),
                )(new XMLParser({ ignoreAttributes: false, attributeNamePrefix: '', parseTagValue: false, isArray: (name) => name === 'Product' || name === 'Data' }).parse(xml)),
            ),
            Effect.acquireUseRelease(
                Effect.sync(() => new DatabaseSync(`${ADOBE}/caps/hdpim.db`, { readOnly: true })),
                (products) => Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ SAPCode: Schema.String, ProductVersion: Schema.String })))(products.prepare('SELECT SAPCode, ProductVersion FROM product_installation_info').all()),
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
    const owned = Array.cartesian(Configuration.Product, installed).flatMap(([{ sapCode, minVersion, Data }, { SAPCode, ProductVersion }]) =>
        Option.exists(Option.all([sapCode, minVersion]), ([code, minimum]) => code === SAPCode && atLeast(ProductVersion, minimum)) ? Data.flatMap(({ key, '#text': text }) => (key === 'UxpHostCode' ? Option.toArray(text) : [])) : [],
    );
    const stage = Effect.fn('stage')(function* (folder: string, host: (typeof manifest.host)[number]) {
        yield* fs.remove(folder, { recursive: true, force: true });
        yield* fs.copy(built, folder);
        yield* fs.writeFileString(path.join(folder, 'manifest.json'), yield* Schema.encodeEffect(Manifest)({ ...manifest, host: [host] }));
    });
    const register = Effect.fn('register')(function* (host: (typeof manifest.host)[number]) {
        const Registry = Schema.fromJsonString(Schema.Struct({ plugins: Schema.Array(Schema.StructWithRest(Schema.Struct({ pluginId: Schema.String, path: Schema.String }), [Schema.JsonObject])) }));
        const [uxp, token] = [path.join(home, ADOBE, 'UXP'), '$localPlugins'];
        const [local, registry, folder] = [path.join(uxp, 'Plugins'), path.join(uxp, 'PluginsInfo', 'v1', `${host.app}.json`), path.join('External', `${manifest.id}_${host.app}`)];
        const [kept, replaced] = Array.partition(
            yield* fs.readFileString(registry).pipe(
                Effect.flatMap(Schema.decodeEffect(Registry)),
                Effect.map(Struct.get('plugins')),
                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeed([])),
            ),
            Filter.fromPredicate(({ pluginId }) => pluginId !== manifest.id),
        );
        yield* Effect.forEach(replaced, (row) => fs.remove(String.replace(token, local)(row.path), { recursive: true, force: true }), { discard: true });
        yield* stage(path.join(local, folder), host);
        yield* fs.makeDirectory(path.dirname(registry), { recursive: true });
        yield* fs.writeFileString(
            registry,
            yield* Schema.encodeEffect(Registry)({
                plugins: [...kept, { hostMinVersion: host.minVersion, name: manifest.name, path: path.join(token, folder), pluginId: manifest.id, status: 'enabled', type: 'uxp', versionString: manifest.version }],
            }),
        );
    });
    const submit = Effect.fn('submit')(function* (host: (typeof manifest.host)[number]) {
        const staged = path.join(yield* fs.makeTempDirectoryScoped(), `${manifest.id}_${host.app}`);
        yield* stage(staged, host);
        yield* stdout('ditto', ['-c', '-k', '--norsrc', staged, `${staged}.ccx`], { stderr: 'inherit' });
        yield* stdout(agent, ['--install', `${staged}.ccx`], { stderr: 'inherit' }).pipe(Effect.filterOrFail(String.includes('Installation Successful'), (answer) => new Cause.UnknownError(answer, `Adobe plug-in installer refused the ${host.app} archive: ${String.trim(answer)}`)));
    }, Effect.scoped);
    yield* Effect.forEach(manifest.host, (host) => Effect.exit(owned.includes(host.app) ? submit(host) : register(host))).pipe(Effect.flatMap(Exit.asVoidAll));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const bundle = `${INSTALLER}/UnifiedPluginInstallerAgent/UnifiedPluginInstallerAgent.app/Contents`;
    const { CFBundleShortVersionString, CFBundleExecutable } = yield* stdout('plutil', ['-convert', 'json', '-o', '-', `${bundle}/Info.plist`], { stderr: 'inherit' }).pipe(
        Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Struct({ CFBundleShortVersionString: Schema.String, CFBundleExecutable: Schema.String })))),
    );
    return yield* Command.make('install', { built: Argument.Directory('built', { mustExist: true }) }, ({ built }) => install(built, `${bundle}/MacOS/${CFBundleExecutable}`)).pipe(Command.run({ version: CFBundleShortVersionString }));
}).pipe(Effect.provide(layer), runMain);
