import { DatabaseSync } from 'node:sqlite';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Manifest } from '@rasm/uxp/manifest.ts';
import { ge } from '@renovatebot/pep440';
import { Array, Cause, Config, Effect, FileSystem, Filter, Path, Schema, String, Struct } from 'effect';
import { Argument, Command } from 'effect/cli';
import { XMLParser } from 'fast-xml-parser';
import { stdout } from '../process.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ADOBE = '/Library/Application Support/Adobe';

// --- [OPERATIONS] ----------------------------------------------------------------------

const install = Effect.fn('install')(function* (built: string, installer: string, agent: string) {
    const [fs, path, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME')]);
    const product = Schema.decodeUnknownOption(Schema.Struct({ sapCode: Schema.String, minVersion: Schema.String, platform: Schema.optionalKey(Schema.Literal('mac')), Data: Schema.Array(Schema.Struct({ key: Schema.String, '#text': Schema.String })) }));
    const [manifest, products, installed] = yield* Effect.all(
        [
            fs.readFileString(path.join(built, 'manifest.json')).pipe(Effect.flatMap(Schema.decodeEffect(Manifest.json))),
            fs.readFileString(path.join(installer, 'ExManCoreLib', 'Contents', 'Resources', 'XManConfig.xml')).pipe(
                Effect.flatMap((xml) => Effect.try(() => new XMLParser({ ignoreAttributes: false, attributeNamePrefix: '', parseTagValue: false, isArray: (name) => name === 'Product' || name === 'Data' }).parse(xml))),
                Effect.flatMap(Schema.decodeUnknownEffect(Schema.Struct({ Configuration: Schema.Struct({ Product: Schema.Array(Schema.Unknown) }) }))),
                Effect.map(({ Configuration }) => Array.getSomes(Configuration.Product.map((row) => product(row)))),
            ),
            Effect.try(() => {
                using database = new DatabaseSync(path.join(ADOBE, 'caps', 'hdpim.db'), { readOnly: true });
                return database.prepare('SELECT SAPCode, ProductVersion FROM product_installation_info').all();
            }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Schema.Array(Schema.Struct({ SAPCode: Schema.String, ProductVersion: Schema.String }))))),
        ],
        { concurrency: 'unbounded' },
    );
    const owned = products.flatMap(({ sapCode, minVersion, Data }) => (installed.some(({ SAPCode, ProductVersion }) => SAPCode === sapCode && ge(ProductVersion, minVersion)) ? Data.filter(({ key }) => key === 'UxpHostCode').map(Struct.get('#text')) : []));
    const { host } = manifest;
    const stage = (folder: string) => Effect.andThen(fs.remove(folder, { recursive: true, force: true }), fs.copy(built, folder));
    const register = Effect.fn('register')(function* () {
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
        yield* stage(path.join(local, folder));
        yield* fs.makeDirectory(path.dirname(registry), { recursive: true });
        yield* fs.writeFileString(
            registry,
            yield* Schema.encodeEffect(Registry)({
                plugins: [...kept, { hostMinVersion: host.minVersion, name: manifest.name, path: path.join(token, folder), pluginId: manifest.id, status: 'enabled', type: 'uxp', versionString: manifest.version }],
            }),
        );
    });
    const submit = Effect.fn('submit')(function* () {
        const staged = path.join(yield* fs.makeTempDirectoryScoped(), `${manifest.id}_${host.app}`);
        yield* stage(staged);
        yield* stdout('ditto', ['-c', '-k', '--norsrc', staged, `${staged}.ccx`]);
        yield* stdout(agent, ['--install', `${staged}.ccx`]).pipe(Effect.filterOrFail(String.includes('Installation Successful'), (answer) => new Cause.UnknownError(undefined, `Adobe plug-in installer refused the ${host.app} archive: ${answer.trim()}`)));
    }, Effect.scoped);
    yield* owned.includes(host.app) ? submit() : register();
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const path = yield* Path.Path;
    const installer = path.join(ADOBE, 'Adobe Desktop Common', 'RemoteComponents', 'UPI');
    const bundle = path.join(installer, 'UnifiedPluginInstallerAgent', 'UnifiedPluginInstallerAgent.app', 'Contents');
    const { CFBundleShortVersionString, CFBundleExecutable } = yield* stdout('plutil', ['-convert', 'json', '-o', '-', path.join(bundle, 'Info.plist')]).pipe(Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Struct({ CFBundleShortVersionString: Schema.String, CFBundleExecutable: Schema.String })))));
    return yield* Command.make('install', { built: Argument.Directory('built', { mustExist: true }) }, ({ built }) => install(built, installer, path.join(bundle, 'MacOS', CFBundleExecutable))).pipe(Command.run({ version: CFBundleShortVersionString }));
}).pipe(Effect.provide(layer), runMain);
